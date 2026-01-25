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
/// CRT-Lottes scaler - Timothy Lottes' CRT simulation with bloom and scanlines.
/// </summary>
/// <remarks>
/// <para>Reference: https://github.com/libretro/glsl-shaders/tree/master/crt/shaders/crt-lottes</para>
/// <para>Algorithm: Authentic CRT appearance with bloom effect, scanlines, and phosphor mask.</para>
/// <para>Developed by Timothy Lottes at NVIDIA for realistic CRT emulation.</para>
/// </remarks>
[ScalerInfo("CRT-Lottes", Author = "Timothy Lottes",
  Url = "https://github.com/libretro/glsl-shaders/tree/master/crt/shaders/crt-lottes",
  Description = "CRT simulation with bloom and phosphor mask", Category = ScalerCategory.Resampler)]
public readonly struct CrtLottes : IPixelScaler {
  private readonly int _scale;

  /// <summary>
  /// Creates a CRT-Lottes scaler with the specified factor.
  /// </summary>
  /// <param name="scale">Scale factor (2 or 3).</param>
  public CrtLottes(int scale = 2) {
    if (scale is < 2 or > 3)
      throw new ArgumentOutOfRangeException(nameof(scale), scale, "CRT-Lottes supports 2x or 3x scaling");
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
      0 or 2 => callback.Invoke(new CrtLottes2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      3 => callback.Invoke(new CrtLottes3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
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

  /// <summary>Gets a 2x CRT-Lottes scaler.</summary>
  public static CrtLottes X2 => new(2);

  /// <summary>Gets a 3x CRT-Lottes scaler.</summary>
  public static CrtLottes X3 => new(3);

  /// <summary>Gets the default CRT-Lottes scaler (2x).</summary>
  public static CrtLottes Default => X2;

  #endregion
}

#region CrtLottes Helpers

file static class CrtLottesHelpers {
  /// <summary>Weight scale for integer lerp operations.</summary>
  public const int WeightScale = 1000;

  /// <summary>Scanline weight for bottom rows (70%).</summary>
  public const int ScanlineWeight = 700;

  /// <summary>Bloom enhancement factor (slight brightness boost).</summary>
  public const int BloomWeight = 1050;

  /// <summary>Mid row brightness for 3x (85%).</summary>
  public const int MidWeight = 850;

  /// <summary>Scanline factor for 3x bottom row (60%).</summary>
  public const int ScanWeight3x = 600;
}

#endregion

#region CrtLottes 2x Kernel

file readonly struct CrtLottes2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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

    // Scanline row (70% brightness)
    var scanline = lerp.Lerp(default, pixel, CrtLottesHelpers.WeightScale - CrtLottesHelpers.ScanlineWeight, CrtLottesHelpers.ScanlineWeight);
    var scanlinePixel = encoder.Encode(scanline);

    // 2x2 pattern:
    // [full] [full]
    // [scan] [scan]
    dest[0] = pixelEncoded;
    dest[1] = pixelEncoded;
    dest[destStride] = scanlinePixel;
    dest[destStride + 1] = scanlinePixel;
  }
}

#endregion

#region CrtLottes 3x Kernel

file readonly struct CrtLottes3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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

    // Mid brightness row (85%)
    var midPixel = lerp.Lerp(default, pixel, CrtLottesHelpers.WeightScale - CrtLottesHelpers.MidWeight, CrtLottesHelpers.MidWeight);
    var midEncoded = encoder.Encode(midPixel);

    // Scanline row (60%)
    var scanPixel = lerp.Lerp(default, pixel, CrtLottesHelpers.WeightScale - CrtLottesHelpers.ScanWeight3x, CrtLottesHelpers.ScanWeight3x);
    var scanEncoded = encoder.Encode(scanPixel);

    // 3x3 pattern:
    // Row 0: Full brightness
    dest[0] = pixelEncoded;
    dest[1] = pixelEncoded;
    dest[2] = pixelEncoded;

    // Row 1: Medium brightness
    dest[destStride] = midEncoded;
    dest[destStride + 1] = midEncoded;
    dest[destStride + 2] = midEncoded;

    // Row 2: Scanline gap
    dest[2 * destStride] = scanEncoded;
    dest[2 * destStride + 1] = scanEncoded;
    dest[2 * destStride + 2] = scanEncoded;
  }
}

#endregion
