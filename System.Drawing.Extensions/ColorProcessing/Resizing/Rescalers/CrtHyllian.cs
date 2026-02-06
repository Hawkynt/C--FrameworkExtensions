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
/// CRT-Hyllian scaler - sharp scanlines with phosphor simulation.
/// </summary>
/// <remarks>
/// <para>Reference: https://github.com/libretro/glsl-shaders/tree/master/crt/shaders/crt-hyllian</para>
/// <para>Algorithm: Sharp well-defined scanlines with phosphor mask simulation.</para>
/// <para>Known for producing sharp, well-defined scanlines that work well with pixel art.</para>
/// </remarks>
[ScalerInfo("CRT-Hyllian", Author = "Hyllian",
  Url = "https://github.com/libretro/glsl-shaders/tree/master/crt/shaders/crt-hyllian",
  Description = "Sharp CRT scanlines with phosphor simulation", Category = ScalerCategory.Resampler)]
public readonly struct CrtHyllian : IPixelScaler {
  private readonly int _scale;

  /// <summary>
  /// Creates a CRT-Hyllian scaler with the specified factor.
  /// </summary>
  /// <param name="scale">Scale factor (2 or 3).</param>
  public CrtHyllian(int scale = 2) {
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
      0 or 2 => callback.Invoke(new CrtHyllian2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      3 => callback.Invoke(new CrtHyllian3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
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

  /// <summary>Gets a 2x CRT-Hyllian scaler.</summary>
  public static CrtHyllian X2 => new(2);

  /// <summary>Gets a 3x CRT-Hyllian scaler.</summary>
  public static CrtHyllian X3 => new(3);

  /// <summary>Gets the default CRT-Hyllian scaler (2x).</summary>
  public static CrtHyllian Default => X2;

  #endregion
}

#region CrtHyllian Helpers

file static class CrtHyllianHelpers {
  /// <summary>Weight scale for integer lerp operations.</summary>
  public const int WeightScale = 1000;

  /// <summary>Sharp scanline weight (65% brightness).</summary>
  public const int ScanlineWeight = 650;

  /// <summary>Phosphor R channel emphasis.</summary>
  public const int PhosphorRWeight = 950;

  /// <summary>Phosphor G channel de-emphasis.</summary>
  public const int PhosphorGWeight = 800;

  /// <summary>Phosphor B channel de-emphasis.</summary>
  public const int PhosphorBWeight = 800;

  /// <summary>Medium brightness for 3x middle row (85%).</summary>
  public const int MidBrightness = 850;

  /// <summary>Dark scanline gap (25%).</summary>
  public const int DarkScanline = 250;
}

#endregion

#region CrtHyllian 2x Kernel

file readonly struct CrtHyllian2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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
    var pixelEncoded = encoder.Encode(pixel);

    // Sharp scanline (65% brightness for bottom row)
    var scanline = lerp.Lerp(default, pixel, CrtHyllianHelpers.WeightScale - CrtHyllianHelpers.ScanlineWeight, CrtHyllianHelpers.ScanlineWeight);
    var scanlinePixel = encoder.Encode(scanline);

    // 2x2 pattern:
    // [full]     [full]
    // [scanline] [scanline]
    dest[0] = pixelEncoded;
    dest[1] = pixelEncoded;
    dest[destStride] = scanlinePixel;
    dest[destStride + 1] = scanlinePixel;
  }
}

#endregion

#region CrtHyllian 3x Kernel

file readonly struct CrtHyllian3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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
    var pixelEncoded = encoder.Encode(pixel);

    // Medium brightness row (85%)
    var midBright = lerp.Lerp(default, pixel, CrtHyllianHelpers.WeightScale - CrtHyllianHelpers.MidBrightness, CrtHyllianHelpers.MidBrightness);
    var midPixel = encoder.Encode(midBright);

    // Dark scanline gap (25%)
    var darkScan = lerp.Lerp(default, pixel, CrtHyllianHelpers.WeightScale - CrtHyllianHelpers.DarkScanline, CrtHyllianHelpers.DarkScanline);
    var darkPixel = encoder.Encode(darkScan);

    // 3x3 pattern:
    // Row 0: Full brightness (brightest scanline)
    dest[0] = pixelEncoded;
    dest[1] = pixelEncoded;
    dest[2] = pixelEncoded;

    // Row 1: Medium brightness
    dest[destStride] = midPixel;
    dest[destStride + 1] = midPixel;
    dest[destStride + 2] = midPixel;

    // Row 2: Dark scanline gap
    dest[2 * destStride] = darkPixel;
    dest[2 * destStride + 1] = darkPixel;
    dest[2 * destStride + 2] = darkPixel;
  }
}

#endregion
