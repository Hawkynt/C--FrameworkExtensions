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
/// Sharp-Bilinear scaler - integer prescaling followed by bilinear for crisp pixels.
/// </summary>
/// <remarks>
/// <para>First performs integer nearest-neighbor scaling, then applies bilinear interpolation.</para>
/// <para>This preserves crisp pixel boundaries while allowing smooth scaling to integer factors.</para>
/// <para>Combines the sharpness of nearest-neighbor with controlled smoothness of bilinear.</para>
/// <para>Reference: LibRetro (https://github.com/libretro/common-shaders/blob/master/interpolation/shaders/sharp-bilinear.cg)</para>
/// </remarks>
[ScalerInfo("Sharp Bilinear", Author = "LibRetro", Year = 2014,
  Url = "https://github.com/libretro/common-shaders/blob/master/interpolation/shaders/sharp-bilinear.cg",
  Description = "Crisp integer scaling with controlled bilinear smoothing", Category = ScalerCategory.PixelArt)]
public readonly struct SharpBilinear : IPixelScaler {
  private readonly int _scale;

  /// <summary>
  /// Creates a Sharp-Bilinear scaler with the specified factor.
  /// </summary>
  /// <param name="scale">Scale factor (2, 3, or 4).</param>
  public SharpBilinear(int scale = 2) {
    ArgumentOutOfRangeException.ThrowIfLessThan(scale, 2);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(scale, 4);
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
      0 or 2 => callback.Invoke(new SharpBilinear2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      3 => callback.Invoke(new SharpBilinear3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      4 => callback.Invoke(new SharpBilinear4xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      _ => throw new InvalidOperationException($"Invalid scale factor: {this._scale}")
    };

  /// <summary>
  /// Gets the list of scale factors supported.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2), new(3, 3), new(4, 4)];

  /// <summary>
  /// Determines whether the specified scale factor is supported.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 } or { X: 3, Y: 3 } or { X: 4, Y: 4 };

  /// <summary>
  /// Enumerates all possible target dimensions.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
    yield return (sourceWidth * 3, sourceHeight * 3);
    yield return (sourceWidth * 4, sourceHeight * 4);
  }

  #region Static Presets

  /// <summary>Gets a 2x Sharp-Bilinear scaler.</summary>
  public static SharpBilinear X2 => new(2);

  /// <summary>Gets a 3x Sharp-Bilinear scaler.</summary>
  public static SharpBilinear X3 => new(3);

  /// <summary>Gets a 4x Sharp-Bilinear scaler.</summary>
  public static SharpBilinear X4 => new(4);

  /// <summary>Gets the default Sharp-Bilinear scaler (2x).</summary>
  public static SharpBilinear Default => X2;

  #endregion
}

#region Sharp-Bilinear Helpers

/// <summary>
/// Helper methods for Sharp-Bilinear interpolation.
/// </summary>
file static class SharpBilinearHelpers {
  /// <summary>
  /// Sharpening function - pushes values toward 0 or ±0.5 based on position.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Sharpen(float x) {
    const float sharpness = 2.0f;
    var sign = x < 0 ? -1.0f : 1.0f;
    var absX = MathF.Abs(x);
    var sharpened = MathF.Pow(absX * 2, sharpness) / 2;
    return sign * MathF.Min(0.5f, sharpened);
  }

  /// <summary>
  /// Computes bilinear interpolation weights for sharp-bilinear sampling.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (int x0, int y0, int x1, int y1, int w00, int w01, int w10, int w11) ComputeWeights(
    float srcX, float srcY, int maxX, int maxY) {
    // Sharp-bilinear: snap to texel boundaries for the integer part
    var texCenterX = MathF.Floor(srcX) + 0.5f;
    var texCenterY = MathF.Floor(srcY) + 0.5f;

    // Calculate distance from texel center and apply sharpening
    var distX = srcX - texCenterX;
    var distY = srcY - texCenterY;
    var sharpX = Sharpen(distX);
    var sharpY = Sharpen(distY);

    // Final sampling position
    var sampleX = texCenterX + sharpX;
    var sampleY = texCenterY + sharpY;

    // Get integer coordinates and fractional parts
    var x0 = (int)MathF.Floor(sampleX);
    var y0 = (int)MathF.Floor(sampleY);
    var x1 = x0 + 1;
    var y1 = y0 + 1;

    // Clamp to bounds
    x0 = Math.Clamp(x0, 0, maxX);
    y0 = Math.Clamp(y0, 0, maxY);
    x1 = Math.Clamp(x1, 0, maxX);
    y1 = Math.Clamp(y1, 0, maxY);

    // Fractional parts for bilinear interpolation (scaled to integers for lerp)
    var fx = sampleX - MathF.Floor(sampleX);
    var fy = sampleY - MathF.Floor(sampleY);
    fx = Math.Clamp(fx, 0, 1);
    fy = Math.Clamp(fy, 0, 1);

    // Convert to integer weights (0-256 range for precision)
    const int scale = 256;
    var ifx = (int)(fx * scale);
    var ify = (int)(fy * scale);
    var w00 = (scale - ifx) * (scale - ify);
    var w01 = ifx * (scale - ify);
    var w10 = (scale - ifx) * ify;
    var w11 = ifx * ify;

    return (x0, y0, x1, y1, w00, w01, w10, w11);
  }
}

#endregion

#region Sharp-Bilinear 2x Kernel

file readonly struct SharpBilinear2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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
    // For 2x, each sub-pixel is at offset from center
    // (0,0): -0.25, -0.25  (1,0): +0.25, -0.25
    // (0,1): -0.25, +0.25  (1,1): +0.25, +0.25

    // Get source pixels from neighborhood
    var c = window.P0P0.Work;   // center
    var r = window.P0P1.Work;   // right
    var d = window.P1P0.Work;   // down
    var dr = window.P1P1.Work;  // down-right

    // For sharp bilinear at 2x, the sharpening makes most sub-pixels pick the center
    // Only at the very edges do we blend slightly

    // Top-left: mostly center
    var e00 = c;

    // Top-right: slight blend with right
    var e01 = lerp.Lerp(c, r, 7, 1);

    // Bottom-left: slight blend with down
    var e10 = lerp.Lerp(c, d, 7, 1);

    // Bottom-right: slight blend with all four
    var mid1 = lerp.Lerp(c, r);
    var mid2 = lerp.Lerp(d, dr);
    var e11 = lerp.Lerp(mid1, mid2, 3, 1);

    // Write output
    dest[0] = encoder.Encode(e00);
    dest[1] = encoder.Encode(e01);
    dest[destStride] = encoder.Encode(e10);
    dest[destStride + 1] = encoder.Encode(e11);
  }
}

#endregion

#region Sharp-Bilinear 3x Kernel

file readonly struct SharpBilinear3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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
    // Get source pixels
    var c = window.P0P0.Work;
    var r = window.P0P1.Work;
    var d = window.P1P0.Work;
    var dr = window.P1P1.Work;

    // Center pixel encoded once
    var pc = encoder.Encode(c);

    // For 3x, center column and row stay sharp (center pixel)
    // Corners get slight blending

    // Row 0: top row
    dest[0] = pc;  // top-left corner - center
    dest[1] = pc;  // top-center - center
    dest[2] = encoder.Encode(lerp.Lerp(c, r, 5, 1));  // top-right - slight blend

    // Row 1: middle row
    var row1 = dest + destStride;
    row1[0] = pc;  // middle-left - center
    row1[1] = pc;  // middle-center - center
    row1[2] = encoder.Encode(lerp.Lerp(c, r, 3, 1));  // middle-right - blend

    // Row 2: bottom row
    var row2 = row1 + destStride;
    row2[0] = encoder.Encode(lerp.Lerp(c, d, 5, 1));  // bottom-left - slight blend
    row2[1] = encoder.Encode(lerp.Lerp(c, d, 3, 1));  // bottom-center - blend
    var mid = lerp.Lerp(lerp.Lerp(c, r), lerp.Lerp(d, dr));
    row2[2] = encoder.Encode(lerp.Lerp(c, mid, 2, 1));  // bottom-right - corner blend
  }
}

#endregion

#region Sharp-Bilinear 4x Kernel

file readonly struct SharpBilinear4xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 4;
  public int ScaleY => 4;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    // Get source pixels
    var c = window.P0P0.Work;
    var r = window.P0P1.Work;
    var d = window.P1P0.Work;
    var dr = window.P1P1.Work;

    // Pre-compute encoded center
    var pc = encoder.Encode(c);

    // Pre-compute blends
    var cr = lerp.Lerp(c, r);
    var cd = lerp.Lerp(c, d);
    var mid = lerp.Lerp(cr, lerp.Lerp(d, dr));

    // Row 0
    dest[0] = pc;
    dest[1] = pc;
    dest[2] = encoder.Encode(lerp.Lerp(c, r, 7, 1));
    dest[3] = encoder.Encode(lerp.Lerp(c, r, 5, 1));

    // Row 1
    var row1 = dest + destStride;
    row1[0] = pc;
    row1[1] = pc;
    row1[2] = encoder.Encode(lerp.Lerp(c, r, 5, 1));
    row1[3] = encoder.Encode(lerp.Lerp(c, r, 3, 1));

    // Row 2
    var row2 = row1 + destStride;
    row2[0] = encoder.Encode(lerp.Lerp(c, d, 7, 1));
    row2[1] = encoder.Encode(lerp.Lerp(c, d, 5, 1));
    row2[2] = encoder.Encode(lerp.Lerp(c, mid, 3, 1));
    row2[3] = encoder.Encode(lerp.Lerp(c, mid, 2, 1));

    // Row 3
    var row3 = row2 + destStride;
    row3[0] = encoder.Encode(lerp.Lerp(c, d, 5, 1));
    row3[1] = encoder.Encode(lerp.Lerp(c, d, 3, 1));
    row3[2] = encoder.Encode(lerp.Lerp(c, mid, 2, 1));
    row3[3] = encoder.Encode(lerp.Lerp(c, mid, 1, 1));
  }
}

#endregion
