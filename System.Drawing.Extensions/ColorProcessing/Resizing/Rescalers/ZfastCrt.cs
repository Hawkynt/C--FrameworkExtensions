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
/// zfast-CRT scaler - Performance-optimized CRT simulation.
/// </summary>
/// <remarks>
/// <para>Reference: https://github.com/libretro/glsl-shaders/tree/master/crt/shaders/zfast-crt</para>
/// <para>Algorithm: Minimal overhead CRT effect with scanlines and simple color mask.</para>
/// <para>Designed for real-time applications with minimal performance impact.</para>
/// </remarks>
[ScalerInfo("zfast-CRT",
  Url = "https://github.com/libretro/glsl-shaders/tree/master/crt/shaders/zfast-crt",
  Description = "Performance-optimized CRT simulation", Category = ScalerCategory.Resampler)]
public readonly struct ZfastCrt : IPixelScaler {
  private readonly int _scale;

  /// <summary>
  /// Creates a zfast-CRT scaler with the specified factor.
  /// </summary>
  /// <param name="scale">Scale factor (2 or 3).</param>
  public ZfastCrt(int scale = 2) {
    if (scale is < 2 or > 3)
      throw new ArgumentOutOfRangeException(nameof(scale), scale, "zfast-CRT supports 2x or 3x scaling");
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
      0 or 2 => callback.Invoke(new ZfastCrt2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      3 => callback.Invoke(new ZfastCrt3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
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

  /// <summary>Gets a 2x zfast-CRT scaler.</summary>
  public static ZfastCrt X2 => new(2);

  /// <summary>Gets a 3x zfast-CRT scaler.</summary>
  public static ZfastCrt X3 => new(3);

  /// <summary>Gets the default zfast-CRT scaler (2x).</summary>
  public static ZfastCrt Default => X2;

  #endregion
}

#region ZfastCrt Helpers

file static class ZfastCrtHelpers {
  /// <summary>Weight scale for integer lerp operations.</summary>
  public const int WeightScale = 1000;

  /// <summary>Scanline darkening factor (70% brightness).</summary>
  public const int ScanlineWeight = 700;

  /// <summary>Dim channel factor (85%).</summary>
  public const int DimWeight = 850;

  /// <summary>Very dim factor (20%).</summary>
  public const int VeryDimWeight = 200;

  /// <summary>Mid brightness (80%).</summary>
  public const int MidWeight = 800;

  /// <summary>Deep scanline (35%).</summary>
  public const int DeepScanWeight = 350;
}

#endregion

#region ZfastCrt 2x Kernel

file readonly struct ZfastCrt2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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
    var scanline = lerp.Lerp(default, pixel, ZfastCrtHelpers.WeightScale - ZfastCrtHelpers.ScanlineWeight, ZfastCrtHelpers.ScanlineWeight);
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

#region ZfastCrt 3x Kernel

file readonly struct ZfastCrt3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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

    // Slightly dimmed version (80%)
    var dimmed = lerp.Lerp(default, pixel, ZfastCrtHelpers.WeightScale - ZfastCrtHelpers.MidWeight, ZfastCrtHelpers.MidWeight);
    var dimPixel = encoder.Encode(dimmed);

    // Deep scanline (35%)
    var scanline = lerp.Lerp(default, pixel, ZfastCrtHelpers.WeightScale - ZfastCrtHelpers.DeepScanWeight, ZfastCrtHelpers.DeepScanWeight);
    var scanlinePixel = encoder.Encode(scanline);

    // 3x3 pattern:
    // Row 0: Full brightness
    dest[0] = pixelEncoded;
    dest[1] = pixelEncoded;
    dest[2] = pixelEncoded;

    // Row 1: Slightly dimmed
    dest[destStride] = dimPixel;
    dest[destStride + 1] = dimPixel;
    dest[destStride + 2] = dimPixel;

    // Row 2: Scanline gap
    dest[2 * destStride] = scanlinePixel;
    dest[2 * destStride + 1] = scanlinePixel;
    dest[2 * destStride + 2] = scanlinePixel;
  }
}

#endregion
