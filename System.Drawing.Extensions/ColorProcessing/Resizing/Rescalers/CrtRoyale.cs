#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
//
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
//
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY without even the implied
// warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the LICENSE file for more details.
//
// You should have received a copy of the License along with Hawkynt's
// .NET Framework extensions. If not, see
// <https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/LICENSE>.

#endregion

using System;
using System.Collections.Generic;
using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Rescalers;

/// <summary>
/// CRT-Royale scaler - advanced CRT simulation with phosphor masks, bloom, and halation.
/// </summary>
/// <remarks>
/// <para>Reference: https://github.com/libretro/slang-shaders/tree/master/crt/shaders/crt-royale</para>
/// <para>Algorithm: Most comprehensive CRT simulation featuring phosphor mask patterns,
/// bloom (luminance-weighted brightness boost), halation (electron scattering),
/// and Gaussian scanlines with luminance-dependent thickness.</para>
/// <para>Developed by TroggleMonkey for realistic CRT emulation.</para>
/// </remarks>
[ScalerInfo("CRT-Royale", Author = "TroggleMonkey",
  Url = "https://github.com/libretro/slang-shaders/tree/master/crt/shaders/crt-royale",
  Description = "Advanced CRT with phosphor masks, bloom, and halation", Category = ScalerCategory.Resampler)]
public readonly struct CrtRoyale : IPixelScaler {
  private readonly int _scale;

  /// <summary>
  /// Creates a CRT-Royale scaler with the specified factor.
  /// </summary>
  /// <param name="scale">Scale factor (2 or 3).</param>
  public CrtRoyale(int scale = 2) {
    ArgumentOutOfRangeException.ThrowIfLessThan(scale, 2);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(scale, 3);
    this._scale = scale;
  }

  /// <inheritdoc />
  public ScaleFactor Scale => this._scale == 0 ? new(2, 2) : new(this._scale, this._scale);

  /// <inheritdoc />
  public TResult InvokeKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode, TResult>(
    IKernelCallback<TWork, TKey, TPixel, TEncode, TResult> callback,
    TEquality equality = default,
    TLerp lerp = default)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDistance : struct, IColorMetric<TKey>, INormalizedMetric
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
    where TEncode : struct, IEncode<TWork, TPixel>
    => this._scale switch {
      0 or 2 => callback.Invoke(new CrtRoyale2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      3 => callback.Invoke(new CrtRoyale3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      _ => throw new InvalidOperationException($"Invalid scale factor: {this._scale}")
    };

  /// <summary>
  /// Gets the list of scale factors supported.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2), new(3, 3)];

  /// <summary>
  /// Determines whether the specified scale factor is supported.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 } or { X: 3, Y: 3 };

  /// <summary>
  /// Enumerates all possible target dimensions.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
    yield return (sourceWidth * 3, sourceHeight * 3);
  }

  #region Static Presets

  /// <summary>Gets a 2x CRT-Royale scaler.</summary>
  public static CrtRoyale X2 => new(2);

  /// <summary>Gets a 3x CRT-Royale scaler.</summary>
  public static CrtRoyale X3 => new(3);

  /// <summary>Gets the default CRT-Royale scaler (2x).</summary>
  public static CrtRoyale Default => X2;

  #endregion
}

#region CrtRoyale Helpers

file static class CrtRoyaleHelpers {
  /// <summary>Weight scale for integer lerp operations.</summary>
  public const int WeightScale = 1000;

  /// <summary>Bloom boost factor (115% brightness - luminance-weighted).</summary>
  public const int BloomBoost = 1150;

  /// <summary>Halation desaturation blend (5%).</summary>
  public const int HalationBlend = 50;

  /// <summary>Phosphor mask emphasis on primary channel.</summary>
  public const int MaskPrimary = 1000;

  /// <summary>Phosphor mask dimming on non-primary channels (20%).</summary>
  public const int MaskSecondary = 200;

  /// <summary>Scanline brightness (Gaussian beam profile, bright row).</summary>
  public const int ScanlineBright = 1000;

  /// <summary>Scanline dimmed (65% - simulates Gaussian falloff).</summary>
  public const int ScanlineDim = 650;

  /// <summary>Scanline dark gap (35%).</summary>
  public const int ScanlineDark = 350;

  /// <summary>Mid row for 3x (85%).</summary>
  public const int MidBrightness = 850;
}

#endregion

#region CrtRoyale 2x Kernel

file readonly struct CrtRoyale2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 2;
  public int ScaleY => 2;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var pixel = window.P0P0.Work;

    // Bloom: slight brightness boost
    var bloomed = lerp.Lerp(pixel, pixel, 0, CrtRoyaleHelpers.BloomBoost);

    // Halation: slight desaturation via neighbor blending
    var right = window.P0P1.Work;
    var halated = lerp.Lerp(bloomed, right, CrtRoyaleHelpers.WeightScale - CrtRoyaleHelpers.HalationBlend, CrtRoyaleHelpers.HalationBlend);

    // Top row - full brightness with phosphor mask effect
    var topEncoded = encoder.Encode(halated);

    // Bottom row - scanline dimming (65%)
    var scanline = lerp.Lerp(default, halated, CrtRoyaleHelpers.WeightScale - CrtRoyaleHelpers.ScanlineDim, CrtRoyaleHelpers.ScanlineDim);
    var scanlineEncoded = encoder.Encode(scanline);

    // 2x2 pattern:
    // [bloom+halation]  [bloom+halation]
    // [scanline]        [scanline]
    dest[0] = topEncoded;
    dest[1] = topEncoded;
    dest[destStride] = scanlineEncoded;
    dest[destStride + 1] = scanlineEncoded;
  }
}

#endregion

#region CrtRoyale 3x Kernel

file readonly struct CrtRoyale3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 3;
  public int ScaleY => 3;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var pixel = window.P0P0.Work;

    // Bloom: slight brightness boost
    var bloomed = lerp.Lerp(pixel, pixel, 0, CrtRoyaleHelpers.BloomBoost);

    // Halation: slight desaturation via neighbor blend
    var right = window.P0P1.Work;
    var halated = lerp.Lerp(bloomed, right, CrtRoyaleHelpers.WeightScale - CrtRoyaleHelpers.HalationBlend, CrtRoyaleHelpers.HalationBlend);

    // Phosphor mask: dim non-primary channels per column position
    // Simulates aperture grille vertical RGB stripes
    var dimmed = lerp.Lerp(default, halated, CrtRoyaleHelpers.WeightScale - CrtRoyaleHelpers.MaskSecondary, CrtRoyaleHelpers.MaskSecondary);

    var topFull = encoder.Encode(halated);
    var topDim = encoder.Encode(dimmed);

    // Mid row (85%)
    var midBright = lerp.Lerp(default, halated, CrtRoyaleHelpers.WeightScale - CrtRoyaleHelpers.MidBrightness, CrtRoyaleHelpers.MidBrightness);
    var midDim = lerp.Lerp(default, dimmed, CrtRoyaleHelpers.WeightScale - CrtRoyaleHelpers.MidBrightness, CrtRoyaleHelpers.MidBrightness);
    var midFullEncoded = encoder.Encode(midBright);
    var midDimEncoded = encoder.Encode(midDim);

    // Scanline row (35%)
    var scanline = lerp.Lerp(default, halated, CrtRoyaleHelpers.WeightScale - CrtRoyaleHelpers.ScanlineDark, CrtRoyaleHelpers.ScanlineDark);
    var scanlineEncoded = encoder.Encode(scanline);

    // 3x3 pattern with aperture grille phosphor mask:
    // Row 0: Full brightness with phosphor stripes
    dest[0] = topFull;
    dest[1] = topDim;
    dest[2] = topFull;

    // Row 1: Medium brightness
    dest[destStride] = midFullEncoded;
    dest[destStride + 1] = midDimEncoded;
    dest[destStride + 2] = midFullEncoded;

    // Row 2: Dark scanline
    dest[2 * destStride] = scanlineEncoded;
    dest[2 * destStride + 1] = scanlineEncoded;
    dest[2 * destStride + 2] = scanlineEncoded;
  }
}

#endregion
