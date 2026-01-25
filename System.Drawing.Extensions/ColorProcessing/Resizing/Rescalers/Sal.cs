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

using System.Collections.Generic;
using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Rescalers;

/// <summary>
/// Guest's 2xSaL (Scaline Algorithm) pixel art scaler.
/// </summary>
/// <remarks>
/// <para>Algorithm: 2xSaL by guest(r) (2007-2016)</para>
/// <para>Reference: https://github.com/libretro/common-shaders/tree/master/xsal</para>
/// <para>Analyzes diagonal color differences and uses inverse-weighted interpolation</para>
/// <para>to blend pixels based on edge continuity.</para>
/// </remarks>
[ScalerInfo("SaL", Author = "guest(r)", Year = 2007,
  Url = "https://github.com/libretro/common-shaders/tree/master/xsal",
  Description = "Scaline algorithm using diagonal color distance weighting", Category = ScalerCategory.PixelArt)]
public readonly struct Sal : IPixelScaler {
  private readonly bool _level2;

  /// <summary>
  /// Creates a SaL scaler.
  /// </summary>
  /// <param name="level2">If true, uses Level 2 with extended 12-point sampling.</param>
  public Sal(bool level2 = false) => this._level2 = level2;

  /// <inheritdoc />
  public ScaleFactor Scale => new(2, 2);

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
    => this._level2
      ? callback.Invoke(new Sal2xLevel2Kernel<TWork, TKey, TPixel, TDistance, TLerp, TEncode>(lerp))
      : callback.Invoke(new Sal2xKernel<TWork, TKey, TPixel, TDistance, TLerp, TEncode>(lerp));

  /// <summary>
  /// Gets the list of scale factors supported.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2)];

  /// <summary>
  /// Determines whether the specified scale factor is supported.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 };

  /// <summary>
  /// Enumerates all possible target dimensions.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
  }

  #region Static Presets

  /// <summary>Gets the default SaL scaler.</summary>
  public static Sal Default => new(false);

  /// <summary>Gets the Level 2 SaL scaler with extended 12-point sampling.</summary>
  public static Sal Level2 => new(true);

  #endregion
}

#region SaL Helpers

file static class SalHelpers {
  /// <summary>Scale factor for converting float weights to integer weights.</summary>
  public const int WeightScale = 1000;

  /// <summary>Small epsilon to avoid division by zero.</summary>
  public const float Epsilon = 0.001f;

  /// <summary>Smaller epsilon for Level 2 inverse distance calculations.</summary>
  public const float SmallEpsilon = 0.00001f;

  /// <summary>
  /// Gets the color distance using the provided metric.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float GetDistance<TKey, TDistance>(in TDistance metric, in TKey a, in TKey b)
    where TKey : unmanaged, IColorSpace
    where TDistance : struct, IColorMetric<TKey>, INormalizedMetric
    => metric.Distance(a, b).ToFloat();

  /// <summary>
  /// Performs weighted 4-color blend using diagonal distances.
  /// </summary>
  /// <remarks>
  /// Formula: (m1*(c02+c20) + m2*(c00+c22)) / (2*(m1+m2))
  /// Equivalent to: lerp(avg(c02,c20), avg(c00,c22), m1, m2)
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TWork WeightedDiagonalBlend<TWork, TLerp>(
    in TLerp lerp,
    in TWork c00, in TWork c20, in TWork c02, in TWork c22,
    float m1, float m2)
    where TWork : unmanaged, IColorSpace
    where TLerp : struct, ILerp<TWork> {
    // avg1 = average of c02 and c20 (the anti-diagonal pair)
    var avg1 = lerp.Lerp(c02, c20);
    // avg2 = average of c00 and c22 (the main diagonal pair)
    var avg2 = lerp.Lerp(c00, c22);
    // Blend the two averages weighted by m1 and m2
    // m1 weights the anti-diagonal, m2 weights the main diagonal
    var w1 = (int)(m1 * WeightScale);
    var w2 = (int)(m2 * WeightScale);
    return lerp.Lerp(avg1, avg2, w1, w2);
  }

  /// <summary>
  /// Performs weighted 3-color blend for Level 2.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TWork WeightedBlend3<TWork, TLerp>(
    in TLerp lerp,
    in TWork c1, in TWork c2, in TWork c3,
    float w1, float w2, float w3)
    where TWork : unmanaged, IColorSpace
    where TLerp : struct, ILerp<TWork> {
    // Blend c1 and c2 first
    var iw1 = (int)(w1 * WeightScale);
    var iw2 = (int)(w2 * WeightScale);
    var iw3 = (int)(w3 * WeightScale);

    // First combine c1 and c2
    var blend12 = lerp.Lerp(c1, c2, iw1, iw2);
    // Then combine with c3
    return lerp.Lerp(blend12, c3, iw1 + iw2, iw3);
  }

  /// <summary>
  /// Performs weighted 4-pair blend for Level 2.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TWork WeightedBlend4Pairs<TWork, TLerp>(
    in TLerp lerp,
    in TWork c1, in TWork c2, in TWork c3, in TWork c4,
    in TWork c5, in TWork c6, in TWork c7, in TWork c8,
    float w1, float w2, float w3, float w4)
    where TWork : unmanaged, IColorSpace
    where TLerp : struct, ILerp<TWork> {
    // Each pair forms an average, then weighted blend
    var avg1 = lerp.Lerp(c1, c2);
    var avg2 = lerp.Lerp(c3, c4);
    var avg3 = lerp.Lerp(c5, c6);
    var avg4 = lerp.Lerp(c7, c8);

    var iw1 = (int)(w1 * WeightScale);
    var iw2 = (int)(w2 * WeightScale);
    var iw3 = (int)(w3 * WeightScale);
    var iw4 = (int)(w4 * WeightScale);

    // Combine in pairs
    var blend12 = lerp.Lerp(avg1, avg2, iw1, iw2);
    var blend34 = lerp.Lerp(avg3, avg4, iw3, iw4);
    return lerp.Lerp(blend12, blend34, iw1 + iw2, iw3 + iw4);
  }
}

#endregion

#region SaL 2x Kernel

file readonly struct Sal2xKernel<TWork, TKey, TPixel, TDistance, TLerp, TEncode>(TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TDistance : struct, IColorMetric<TKey>, INormalizedMetric
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
    TDistance metric = default;

    // Process each of the 4 output pixels
    // Top-left (dx=-0.25, dy=-0.25)
    dest[0] = encoder.Encode(_Interpolate(window, metric, -0.25f, -0.25f));
    // Top-right (dx=+0.25, dy=-0.25)
    dest[1] = encoder.Encode(_Interpolate(window, metric, 0.25f, -0.25f));
    // Bottom-left (dx=-0.25, dy=+0.25)
    dest[destStride] = encoder.Encode(_Interpolate(window, metric, -0.25f, 0.25f));
    // Bottom-right (dx=+0.25, dy=+0.25)
    dest[destStride + 1] = encoder.Encode(_Interpolate(window, metric, 0.25f, 0.25f));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private TWork _Interpolate(
    in NeighborWindow<TWork, TKey> n,
    in TDistance metric,
    float dx, float dy) {
    // Select 4 diagonal corners based on quadrant
    TWork c00, c20, c02, c22;
    TKey k00, k20, k02, k22;

    if (dx < 0 && dy < 0) {
      // Top-left quadrant
      c00 = n.M1M1.Work; k00 = n.M1M1.Key;
      c20 = n.P0M1.Work; k20 = n.P0M1.Key;
      c02 = n.M1P0.Work; k02 = n.M1P0.Key;
      c22 = n.P0P0.Work; k22 = n.P0P0.Key;
    } else if (dx >= 0 && dy < 0) {
      // Top-right quadrant
      c00 = n.P0M1.Work; k00 = n.P0M1.Key;
      c20 = n.P1M1.Work; k20 = n.P1M1.Key;
      c02 = n.P0P0.Work; k02 = n.P0P0.Key;
      c22 = n.P1P0.Work; k22 = n.P1P0.Key;
    } else if (dx < 0) {
      // Bottom-left quadrant
      c00 = n.M1P0.Work; k00 = n.M1P0.Key;
      c20 = n.P0P0.Work; k20 = n.P0P0.Key;
      c02 = n.M1P1.Work; k02 = n.M1P1.Key;
      c22 = n.P0P1.Work; k22 = n.P0P1.Key;
    } else {
      // Bottom-right quadrant
      c00 = n.P0P0.Work; k00 = n.P0P0.Key;
      c20 = n.P1P0.Work; k20 = n.P1P0.Key;
      c02 = n.P0P1.Work; k02 = n.P0P1.Key;
      c22 = n.P1P1.Work; k22 = n.P1P1.Key;
    }

    // Calculate diagonal distances
    var m1 = SalHelpers.GetDistance(metric, k00, k22) + SalHelpers.Epsilon;
    var m2 = SalHelpers.GetDistance(metric, k02, k20) + SalHelpers.Epsilon;

    // Weighted blend: inverse of distance = higher weight
    // m1 weights c02+c20 (anti-diagonal), m2 weights c00+c22 (main diagonal)
    return SalHelpers.WeightedDiagonalBlend(lerp, c00, c20, c02, c22, m1, m2);
  }
}

#endregion

#region SaL 2x Level 2 Kernel

file readonly struct Sal2xLevel2Kernel<TWork, TKey, TPixel, TDistance, TLerp, TEncode>(TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TDistance : struct, IColorMetric<TKey>, INormalizedMetric
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
    TDistance metric = default;

    // Process each of the 4 output pixels
    dest[0] = encoder.Encode(_Interpolate(window, metric, -0.25f, -0.25f));
    dest[1] = encoder.Encode(_Interpolate(window, metric, 0.25f, -0.25f));
    dest[destStride] = encoder.Encode(_Interpolate(window, metric, -0.25f, 0.25f));
    dest[destStride + 1] = encoder.Encode(_Interpolate(window, metric, 0.25f, 0.25f));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private TWork _Interpolate(
    in NeighborWindow<TWork, TKey> n,
    in TDistance metric,
    float dx, float dy) {
    // Center pixel for final comparison
    var c11 = n.P0P0;

    // Select samples based on quadrant - standard, horizontal-extended, vertical-extended
    TWork s00, s20, s02, s22;
    TKey ks00, ks20, ks02, ks22;
    TWork h00, h20, h02, h22;
    TKey kh00, kh20, kh02, kh22;
    TWork v00, v20, v02, v22;
    TKey kv00, kv20, kv02, kv22;

    if (dx < 0 && dy < 0) {
      // Top-left quadrant
      s00 = n.M1M1.Work; ks00 = n.M1M1.Key;
      s20 = n.P0M1.Work; ks20 = n.P0M1.Key;
      s02 = n.M1P0.Work; ks02 = n.M1P0.Key;
      s22 = n.P0P0.Work; ks22 = n.P0P0.Key;
      h00 = n.M2M1.Work; kh00 = n.M2M1.Key;
      h20 = n.P1M1.Work; kh20 = n.P1M1.Key;
      h02 = n.M2P0.Work; kh02 = n.M2P0.Key;
      h22 = n.P1P0.Work; kh22 = n.P1P0.Key;
      v00 = n.M1M2.Work; kv00 = n.M1M2.Key;
      v20 = n.P0M2.Work; kv20 = n.P0M2.Key;
      v02 = n.M1P1.Work; kv02 = n.M1P1.Key;
      v22 = n.P0P1.Work; kv22 = n.P0P1.Key;
    } else if (dx >= 0 && dy < 0) {
      // Top-right quadrant
      s00 = n.P0M1.Work; ks00 = n.P0M1.Key;
      s20 = n.P1M1.Work; ks20 = n.P1M1.Key;
      s02 = n.P0P0.Work; ks02 = n.P0P0.Key;
      s22 = n.P1P0.Work; ks22 = n.P1P0.Key;
      h00 = n.M1M1.Work; kh00 = n.M1M1.Key;
      h20 = n.P2M1.Work; kh20 = n.P2M1.Key;
      h02 = n.M1P0.Work; kh02 = n.M1P0.Key;
      h22 = n.P2P0.Work; kh22 = n.P2P0.Key;
      v00 = n.P0M2.Work; kv00 = n.P0M2.Key;
      v20 = n.P1M2.Work; kv20 = n.P1M2.Key;
      v02 = n.P0P1.Work; kv02 = n.P0P1.Key;
      v22 = n.P1P1.Work; kv22 = n.P1P1.Key;
    } else if (dx < 0) {
      // Bottom-left quadrant
      s00 = n.M1P0.Work; ks00 = n.M1P0.Key;
      s20 = n.P0P0.Work; ks20 = n.P0P0.Key;
      s02 = n.M1P1.Work; ks02 = n.M1P1.Key;
      s22 = n.P0P1.Work; ks22 = n.P0P1.Key;
      h00 = n.M2P0.Work; kh00 = n.M2P0.Key;
      h20 = n.P1P0.Work; kh20 = n.P1P0.Key;
      h02 = n.M2P1.Work; kh02 = n.M2P1.Key;
      h22 = n.P1P1.Work; kh22 = n.P1P1.Key;
      v00 = n.M1M1.Work; kv00 = n.M1M1.Key;
      v20 = n.P0M1.Work; kv20 = n.P0M1.Key;
      v02 = n.M1P2.Work; kv02 = n.M1P2.Key;
      v22 = n.P0P2.Work; kv22 = n.P0P2.Key;
    } else {
      // Bottom-right quadrant
      s00 = n.P0P0.Work; ks00 = n.P0P0.Key;
      s20 = n.P1P0.Work; ks20 = n.P1P0.Key;
      s02 = n.P0P1.Work; ks02 = n.P0P1.Key;
      s22 = n.P1P1.Work; ks22 = n.P1P1.Key;
      h00 = n.M1P0.Work; kh00 = n.M1P0.Key;
      h20 = n.P2P0.Work; kh20 = n.P2P0.Key;
      h02 = n.M1P1.Work; kh02 = n.M1P1.Key;
      h22 = n.P2P1.Work; kh22 = n.P2P1.Key;
      v00 = n.P0M1.Work; kv00 = n.P0M1.Key;
      v20 = n.P1M1.Work; kv20 = n.P1M1.Key;
      v02 = n.P0P2.Work; kv02 = n.P0P2.Key;
      v22 = n.P1P2.Work; kv22 = n.P1P2.Key;
    }

    // Calculate inverse distances for weighting
    var m1 = 1.0f / (SalHelpers.GetDistance(metric, ks00, ks22) + SalHelpers.SmallEpsilon);
    var m2 = 1.0f / (SalHelpers.GetDistance(metric, ks02, ks20) + SalHelpers.SmallEpsilon);

    var h1 = 1.0f / (SalHelpers.GetDistance(metric, ks00, kh22) + SalHelpers.SmallEpsilon);
    var h2 = 1.0f / (SalHelpers.GetDistance(metric, ks02, kh20) + SalHelpers.SmallEpsilon);
    var h3 = 1.0f / (SalHelpers.GetDistance(metric, kh00, ks22) + SalHelpers.SmallEpsilon);
    var h4 = 1.0f / (SalHelpers.GetDistance(metric, kh02, ks20) + SalHelpers.SmallEpsilon);

    var v1 = 1.0f / (SalHelpers.GetDistance(metric, ks00, kv22) + SalHelpers.SmallEpsilon);
    var v2 = 1.0f / (SalHelpers.GetDistance(metric, ks02, kv20) + SalHelpers.SmallEpsilon);
    var v3 = 1.0f / (SalHelpers.GetDistance(metric, kv00, ks22) + SalHelpers.SmallEpsilon);
    var v4 = 1.0f / (SalHelpers.GetDistance(metric, kv02, ks20) + SalHelpers.SmallEpsilon);

    // Calculate three interpolation candidates
    var t1 = SalHelpers.WeightedDiagonalBlend(lerp, s00, s22, s02, s20, m1, m2);
    var t2 = SalHelpers.WeightedBlend4Pairs(lerp, s00, h22, s02, h20, h00, s22, h02, s20, h1, h2, h3, h4);
    var t3 = SalHelpers.WeightedBlend4Pairs(lerp, s00, v22, s02, v20, v00, s22, v02, s20, v1, v2, v3, v4);

    // Blend candidates based on similarity to center
    // Need to convert interpolated colors to key space for distance calculation
    // Since TWork may not be the same as TKey, we approximate by using the center's key
    var k1 = 1.0f / (SalHelpers.GetDistance(metric, c11.Key, c11.Key) + SalHelpers.SmallEpsilon);
    var k2 = k1; // Approximation - ideally we'd convert t1, t2, t3 to key space
    var k3 = k1;

    // In practice, for Level 2 we weight equally when we can't compute proper distances
    return SalHelpers.WeightedBlend3(lerp, t1, t2, t3, 1.0f, 1.0f, 1.0f);
  }
}

#endregion
