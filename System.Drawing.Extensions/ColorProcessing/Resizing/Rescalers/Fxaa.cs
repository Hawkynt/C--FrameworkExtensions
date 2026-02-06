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
/// FXAA (Fast Approximate Anti-Aliasing) - Luma-based edge detection scaler (2x, 3x, 4x).
/// </summary>
/// <remarks>
/// <para>Fast post-processing anti-aliasing that detects edges using luma gradients.</para>
/// <para>
/// Applies directional blending based on edge detection thresholds.
/// Uses Rec.709 luma coefficients for edge detection.
/// </para>
/// <para>Algorithm by Timothy Lottes/NVIDIA, 2009.</para>
/// </remarks>
[ScalerInfo("FXAA", Author = "Timothy Lottes/NVIDIA", Year = 2009,
  Description = "Fast Approximate Anti-Aliasing", Category = ScalerCategory.PixelArt)]
public readonly struct Fxaa : IPixelScaler {

  private readonly int _scale;

  // FXAA quality presets
  private const float EdgeThreshold = 0.166f;
  private const float EdgeThresholdMin = 0.0833f;
  private const float SubpixelQuality = 0.75f;

  /// <summary>
  /// Creates a new FXAA instance.
  /// </summary>
  /// <param name="scale">Scale factor (2, 3, or 4).</param>
  public Fxaa(int scale = 2) {
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
      0 or 2 => callback.Invoke(new Fxaa2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      3 => callback.Invoke(new Fxaa3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      4 => callback.Invoke(new Fxaa4xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
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

  /// <summary>
  /// Gets a 2x scale instance.
  /// </summary>
  public static Fxaa Scale2x => new(2);

  /// <summary>
  /// Gets a 3x scale instance.
  /// </summary>
  public static Fxaa Scale3x => new(3);

  /// <summary>
  /// Gets a 4x scale instance.
  /// </summary>
  public static Fxaa Scale4x => new(4);

  /// <summary>
  /// Gets the default configuration (2x).
  /// </summary>
  public static Fxaa Default => Scale2x;

  /// <summary>
  /// Computes RGB to luma.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float RgbToLuma<TWork>(in TWork color) where TWork : unmanaged, IColorSpace
    => ColorConverter.GetLuminance(color);

  /// <summary>
  /// Applies FXAA edge detection and blending.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static TWork ApplyFxaa<TWork, TLerp>(
    in TWork center, in TWork n, in TWork s, in TWork e, in TWork w,
    in TWork nw, in TWork ne, in TWork sw, in TWork se,
    TLerp lerp)
    where TWork : unmanaged, IColorSpace
    where TLerp : struct, ILerp<TWork> {

    // Calculate luma for FXAA
    var lumaCenter = RgbToLuma(center);
    var lumaN = RgbToLuma(n);
    var lumaS = RgbToLuma(s);
    var lumaE = RgbToLuma(e);
    var lumaW = RgbToLuma(w);

    // Find min/max luma
    var lumaMin = MathF.Min(lumaCenter, MathF.Min(MathF.Min(lumaN, lumaS), MathF.Min(lumaE, lumaW)));
    var lumaMax = MathF.Max(lumaCenter, MathF.Max(MathF.Max(lumaN, lumaS), MathF.Max(lumaE, lumaW)));
    var lumaRange = lumaMax - lumaMin;

    // Early exit if below threshold (no edge)
    if (lumaRange < MathF.Max(EdgeThresholdMin, lumaMax * EdgeThreshold))
      return center;

    // Calculate luma for diagonals
    var lumaNW = RgbToLuma(nw);
    var lumaNE = RgbToLuma(ne);
    var lumaSW = RgbToLuma(sw);
    var lumaSE = RgbToLuma(se);

    // Calculate directional gradients
    var lumaNS = lumaN + lumaS;
    var lumaWE = lumaW + lumaE;
    var lumaNWSW = lumaNW + lumaSW;
    var lumaNESE = lumaNE + lumaSE;
    var lumaNWNE = lumaNW + lumaNE;
    var lumaSWSE = lumaSW + lumaSE;

    // Calculate edge direction
    var edgeHorizontal = MathF.Abs(-2.0f * lumaW + lumaNWSW) + MathF.Abs(-2.0f * lumaCenter + lumaNS) * 2.0f + MathF.Abs(-2.0f * lumaE + lumaNESE);
    var edgeVertical = MathF.Abs(-2.0f * lumaN + lumaNWNE) + MathF.Abs(-2.0f * lumaCenter + lumaWE) * 2.0f + MathF.Abs(-2.0f * lumaS + lumaSWSE);

    var isHorizontal = edgeHorizontal >= edgeVertical;

    // Select edge pixels
    var luma1 = isHorizontal ? lumaS : lumaE;
    var luma2 = isHorizontal ? lumaN : lumaW;

    // Calculate gradient
    var gradient1 = luma1 - lumaCenter;
    var gradient2 = luma2 - lumaCenter;

    var is1Steepest = MathF.Abs(gradient1) >= MathF.Abs(gradient2);

    // Sub-pixel anti-aliasing
    float lumaLocalAverage;
    if (is1Steepest)
      lumaLocalAverage = 0.5f * (luma1 + lumaCenter);
    else
      lumaLocalAverage = 0.5f * (luma2 + lumaCenter);

    var subPixelOffset1 = MathF.Abs(lumaLocalAverage - lumaCenter) / lumaRange;
    var subPixelOffset2 = (-2.0f * subPixelOffset1 + 3.0f) * subPixelOffset1 * subPixelOffset1;
    var subPixelOffsetFinal = subPixelOffset2 * subPixelOffset2 * SubpixelQuality;

    // Calculate blend amount
    var blendL = MathF.Max(subPixelOffsetFinal, 0.0f);
    blendL = MathF.Min(blendL, 1.0f);

    // Select pixel to blend
    TWork pixelToBlend;
    if (isHorizontal)
      pixelToBlend = is1Steepest ? s : n;
    else
      pixelToBlend = is1Steepest ? e : w;

    var blendW2 = (int)(blendL * 256f);
    return lerp.Lerp(center, pixelToBlend, 256 - blendW2, blendW2);
  }
}

file readonly struct Fxaa2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp)
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
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    // Get 3x3 neighborhood
    var center = window.P0P0.Work;
    var n = window.M1P0.Work;  // row -1, col 0
    var s = window.P1P0.Work;  // row +1, col 0
    var e = window.P0P1.Work;  // row 0, col +1
    var w = window.P0M1.Work;  // row 0, col -1
    var nw = window.M1M1.Work; // row -1, col -1
    var ne = window.M1P1.Work; // row -1, col +1
    var sw = window.P1M1.Work; // row +1, col -1
    var se = window.P1P1.Work; // row +1, col +1

    var result = Fxaa.ApplyFxaa(center, n, s, e, w, nw, ne, sw, se, lerp);
    var encoded = encoder.Encode(result);

    // Write 2x2 output (same pixel)
    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;

    row0[0] = encoded;
    row0[1] = encoded;
    row1[0] = encoded;
    row1[1] = encoded;
  }
}

file readonly struct Fxaa3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp)
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
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    // Get 3x3 neighborhood
    var center = window.P0P0.Work;
    var n = window.M1P0.Work;
    var s = window.P1P0.Work;
    var e = window.P0P1.Work;
    var w = window.P0M1.Work;
    var nw = window.M1M1.Work;
    var ne = window.M1P1.Work;
    var sw = window.P1M1.Work;
    var se = window.P1P1.Work;

    var result = Fxaa.ApplyFxaa(center, n, s, e, w, nw, ne, sw, se, lerp);
    var encoded = encoder.Encode(result);

    // Write 3x3 output (same pixel)
    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;
    var row2 = row1 + destStride;

    row0[0] = encoded;
    row0[1] = encoded;
    row0[2] = encoded;
    row1[0] = encoded;
    row1[1] = encoded;
    row1[2] = encoded;
    row2[0] = encoded;
    row2[1] = encoded;
    row2[2] = encoded;
  }
}

file readonly struct Fxaa4xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp)
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
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    // Get 3x3 neighborhood
    var center = window.P0P0.Work;
    var n = window.M1P0.Work;
    var s = window.P1P0.Work;
    var e = window.P0P1.Work;
    var w = window.P0M1.Work;
    var nw = window.M1M1.Work;
    var ne = window.M1P1.Work;
    var sw = window.P1M1.Work;
    var se = window.P1P1.Work;

    var result = Fxaa.ApplyFxaa(center, n, s, e, w, nw, ne, sw, se, lerp);
    var encoded = encoder.Encode(result);

    // Write 4x4 output (same pixel)
    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;
    var row2 = row1 + destStride;
    var row3 = row2 + destStride;

    row0[0] = encoded;
    row0[1] = encoded;
    row0[2] = encoded;
    row0[3] = encoded;
    row1[0] = encoded;
    row1[1] = encoded;
    row1[2] = encoded;
    row1[3] = encoded;
    row2[0] = encoded;
    row2[1] = encoded;
    row2[2] = encoded;
    row2[3] = encoded;
    row3[0] = encoded;
    row3[1] = encoded;
    row3[2] = encoded;
    row3[3] = encoded;
  }
}
