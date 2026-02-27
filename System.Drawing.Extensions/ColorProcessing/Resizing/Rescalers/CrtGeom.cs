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
/// CRT-Geom scaler - Classic CRT geometry with shadow mask and scanlines.
/// </summary>
/// <remarks>
/// <para>Reference: https://github.com/libretro/glsl-shaders/tree/master/crt/shaders/crt-geom</para>
/// <para>Algorithm: CRT simulation with phosphor shadow mask patterns and scanline darkening.</para>
/// <para>Inspired by the CRT-Geom shader from the libretro community.</para>
/// </remarks>
[ScalerInfo("CRT-Geom",
  Url = "https://github.com/libretro/glsl-shaders/tree/master/crt/shaders/crt-geom",
  Description = "CRT simulation with shadow mask and scanline darkening", Category = ScalerCategory.Resampler)]
public readonly struct CrtGeom : IPixelScaler {
  private readonly int _scale;

  /// <summary>
  /// Creates a CRT-Geom scaler with the specified factor.
  /// </summary>
  /// <param name="scale">Scale factor (2 or 3).</param>
  public CrtGeom(int scale = 2) {
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
      0 or 2 => callback.Invoke(new CrtGeom2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      3 => callback.Invoke(new CrtGeom3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
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

  /// <summary>Gets a 2x CRT-Geom scaler.</summary>
  public static CrtGeom X2 => new(2);

  /// <summary>Gets a 3x CRT-Geom scaler.</summary>
  public static CrtGeom X3 => new(3);

  /// <summary>Gets the default CRT-Geom scaler (2x).</summary>
  public static CrtGeom Default => X2;

  #endregion
}

#region CrtGeom Helpers

file static class CrtGeomHelpers {
  /// <summary>Weight scale for integer lerp operations.</summary>
  public const int WeightScale = 1000;

  /// <summary>Scanline darkening factor (75% brightness).</summary>
  public const int ScanlineWeight = 750;

  /// <summary>Shadow mask R-dominant emphasis (full on primary, 85% on others).</summary>
  public const int MaskDim = 850;

  /// <summary>Corner darkening reduction for 2x (85% brightness at extremes).</summary>
  public const int CornerWeight = 850;
}

#endregion

#region CrtGeom 2x Kernel

file readonly struct CrtGeom2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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

    // Bottom row - scanline darkening (75% brightness)
    var scanline = lerp.Lerp(default, pixel, CrtGeomHelpers.WeightScale - CrtGeomHelpers.ScanlineWeight, CrtGeomHelpers.ScanlineWeight);

    // Shadow mask: slightly dim non-primary channels per column
    var dimmed = lerp.Lerp(default, pixel, CrtGeomHelpers.WeightScale - CrtGeomHelpers.MaskDim, CrtGeomHelpers.MaskDim);
    var scanlineDimmed = lerp.Lerp(default, scanline, CrtGeomHelpers.WeightScale - CrtGeomHelpers.MaskDim, CrtGeomHelpers.MaskDim);

    // 2x2 pattern:
    // [full]   [dimmed]
    // [scan]   [scan-dim]
    dest[0] = pixelEncoded;
    dest[1] = encoder.Encode(dimmed);
    dest[destStride] = encoder.Encode(scanline);
    dest[destStride + 1] = encoder.Encode(scanlineDimmed);
  }
}

#endregion

#region CrtGeom 3x Kernel

file readonly struct CrtGeom3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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

    // Shadow mask dimmed variant (85%)
    var dimmed = lerp.Lerp(default, pixel, CrtGeomHelpers.WeightScale - CrtGeomHelpers.MaskDim, CrtGeomHelpers.MaskDim);
    var dimPixel = encoder.Encode(dimmed);

    // Slightly dimmed row (85%)
    var midBright = lerp.Lerp(default, pixel, CrtGeomHelpers.WeightScale - CrtGeomHelpers.CornerWeight, CrtGeomHelpers.CornerWeight);
    var midDimmed = lerp.Lerp(default, dimmed, CrtGeomHelpers.WeightScale - CrtGeomHelpers.CornerWeight, CrtGeomHelpers.CornerWeight);
    var midPixel = encoder.Encode(midBright);
    var midDimPixel = encoder.Encode(midDimmed);

    // Scanline row (35% brightness - dark gap)
    var scanline = lerp.Lerp(default, pixel, CrtGeomHelpers.WeightScale - 350, 350);
    var scanlinePixel = encoder.Encode(scanline);

    // 3x3 CRT-Geom pattern with shadow mask:
    // Row 0: Full brightness (alternating mask)
    dest[0] = pixelEncoded;
    dest[1] = dimPixel;
    dest[2] = pixelEncoded;

    // Row 1: Medium brightness
    dest[destStride] = midPixel;
    dest[destStride + 1] = midDimPixel;
    dest[destStride + 2] = midPixel;

    // Row 2: Dark scanline gap
    dest[2 * destStride] = scanlinePixel;
    dest[2 * destStride + 1] = scanlinePixel;
    dest[2 * destStride + 2] = scanlinePixel;
  }
}

#endregion
