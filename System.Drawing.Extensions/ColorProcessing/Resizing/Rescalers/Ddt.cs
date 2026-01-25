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
/// DDT variant for the scaler.
/// </summary>
public enum DdtVariant {
  /// <summary>Basic DDT - simple 2x2 diagonal analysis.</summary>
  Basic,
  /// <summary>DDT Sharp - uses 4x4 neighborhood with weighted analysis.</summary>
  Sharp,
  /// <summary>DDT Extended - uses 16-pixel voting system.</summary>
  Extended
}

/// <summary>
/// Hyllian's DDT (Data Dependent Triangulation) scaling filter.
/// </summary>
/// <remarks>
/// <para>Reference: Hyllian 2011-2016 (sergiogdb@gmail.com)</para>
/// <para>See: https://github.com/libretro/common-shaders/tree/master/ddt</para>
/// <para>Algorithm: Divides 2x2 pixel square into triangles based on diagonal luma differences,
/// then performs bilinear interpolation within the optimal triangle orientation.</para>
/// <para>Three variants available: Basic (simple), Sharp (weighted 4x4), Extended (16-pixel voting).</para>
/// </remarks>
[ScalerInfo("DDT", Author = "Hyllian", Year = 2011,
  Url = "https://github.com/libretro/common-shaders/tree/master/ddt",
  Description = "Data Dependent Triangulation pixel scaling", Category = ScalerCategory.PixelArt)]
public readonly struct Ddt : IPixelScaler {
  private readonly DdtVariant _variant;

  /// <summary>
  /// Creates a DDT scaler with the specified variant.
  /// </summary>
  /// <param name="variant">The DDT variant to use.</param>
  public Ddt(DdtVariant variant = DdtVariant.Basic) {
    this._variant = variant;
  }

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
    => this._variant switch {
      DdtVariant.Basic => callback.Invoke(new DdtBasicKernel<TWork, TKey, TPixel, TDistance, TLerp, TEncode>(lerp)),
      DdtVariant.Sharp => callback.Invoke(new DdtSharpKernel<TWork, TKey, TPixel, TDistance, TLerp, TEncode>(lerp)),
      DdtVariant.Extended => callback.Invoke(new DdtExtendedKernel<TWork, TKey, TPixel, TDistance, TLerp, TEncode>(lerp)),
      _ => throw new InvalidOperationException($"Invalid DDT variant: {this._variant}")
    };

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

  /// <summary>Gets the basic DDT scaler.</summary>
  public static Ddt Basic => new(DdtVariant.Basic);

  /// <summary>Gets the sharp DDT scaler with weighted 4x4 analysis.</summary>
  public static Ddt Sharp => new(DdtVariant.Sharp);

  /// <summary>Gets the extended DDT scaler with 16-pixel voting.</summary>
  public static Ddt Extended => new(DdtVariant.Extended);

  /// <summary>Gets the default DDT scaler (Basic).</summary>
  public static Ddt Default => Basic;

  #endregion
}

#region DDT Helpers

/// <summary>
/// Helper methods for DDT algorithm.
/// </summary>
file static class DdtHelpers {
  // DDT Sharp weights
  public const float Wp1 = 1.0f;
  public const float Wp2 = 1.0f;
  public const float Wp3 = -1.0f;

  /// <summary>
  /// Gets the color distance using the provided metric (used as luma proxy).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float GetDistance<TKey, TDistance>(in TDistance metric, in TKey a, in TKey b)
    where TKey : unmanaged, IColorSpace
    where TDistance : struct, IColorMetric<TKey>, INormalizedMetric
    => metric.Distance(a, b).ToFloat();

  /// <summary>
  /// DDT interpolation based on triangulation.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TWork DdtInterpolate<TWork, TLerp>(
    in TWork a, in TWork b, in TWork c, in TWork d,
    float p, float q, float wd1, float wd2, in TLerp lerp)
    where TWork : unmanaged, IColorSpace
    where TLerp : struct, ILerp<TWork> {
    // pos relative to center of 2x2 grid
    var posX = p - 0.5f;
    var posY = q - 0.5f;

    // Direction signs
    var dirX = Math.Sign(posX);
    var dirY = Math.Sign(posY);

    // Distance to diagonal corners
    var k = Math.Abs(posX - dirX);
    var l = Math.Abs(posY - dirY);

    // Working copies
    var wA = a;
    var wB = b;
    var wC = c;
    var wD = d;

    // Select triangle orientation based on diagonal difference
    if (wd1 < wd2) {
      // Favor A-D diagonal
      if (k < l)
        // Replace C with extrapolation from A, D, B
        wC = lerp.Lerp(lerp.Lerp(a, d), b, 2, -1);
      else
        // Replace B with extrapolation from A, D, C
        wB = lerp.Lerp(lerp.Lerp(a, d), c, 2, -1);
    } else if (wd1 > wd2)
      // Favor B-C diagonal, replace D
      wD = lerp.Lerp(lerp.Lerp(b, c), a, 2, -1);

    // Bilinear interpolation
    var pAbs = Math.Abs(posX);
    var qAbs = Math.Abs(posY);

    var w00 = (int)((1 - pAbs) * (1 - qAbs) * 256);
    var w10 = (int)(pAbs * (1 - qAbs) * 256);
    var w01 = (int)((1 - pAbs) * qAbs * 256);
    var w11 = 256 - w00 - w10 - w01;

    var top = lerp.Lerp(wA, wB, w00, w10);
    var bottom = lerp.Lerp(wC, wD, w01, w11);
    return lerp.Lerp(top, bottom);
  }
}

#endregion

#region DDT Basic Kernel

file readonly struct DdtBasicKernel<TWork, TKey, TPixel, TDistance, TLerp, TEncode>(TLerp lerp = default)
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
    // Get 2x2 neighborhood: A=center, B=right, C=below, D=below-right
    var a = window.P0P0;
    var b = window.P1P0;
    var c = window.P0P1;
    var d = window.P1P1;

    TDistance metric = default;

    // Diagonal differences using distance metric
    var wd1 = DdtHelpers.GetDistance(metric, a.Key, d.Key);
    var wd2 = DdtHelpers.GetDistance(metric, b.Key, c.Key);

    // Output pixels at 4 subpixel positions
    // Position (0.25, 0.25) - top-left quadrant
    dest[0] = encoder.Encode(DdtHelpers.DdtInterpolate(a.Work, b.Work, c.Work, d.Work, 0.25f, 0.25f, wd1, wd2, lerp));
    // Position (0.75, 0.25) - top-right quadrant
    dest[1] = encoder.Encode(DdtHelpers.DdtInterpolate(a.Work, b.Work, c.Work, d.Work, 0.75f, 0.25f, wd1, wd2, lerp));
    // Position (0.25, 0.75) - bottom-left quadrant
    dest[destStride] = encoder.Encode(DdtHelpers.DdtInterpolate(a.Work, b.Work, c.Work, d.Work, 0.25f, 0.75f, wd1, wd2, lerp));
    // Position (0.75, 0.75) - bottom-right quadrant
    dest[destStride + 1] = encoder.Encode(DdtHelpers.DdtInterpolate(a.Work, b.Work, c.Work, d.Work, 0.75f, 0.75f, wd1, wd2, lerp));
  }
}

#endregion

#region DDT Sharp Kernel

file readonly struct DdtSharpKernel<TWork, TKey, TPixel, TDistance, TLerp, TEncode>(TLerp lerp = default)
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
    // Get 4x4 neighborhood for sharp analysis
    //    A1 B1
    // A0 A  B  B2
    // C0 C  D  D2
    //    C3 D3
    var a = window.P0P0;
    var b = window.P1P0;
    var c = window.P0P1;
    var d = window.P1P1;

    var a1 = window.P0M1;
    var b1 = window.P1M1;
    var a0 = window.M1P0;
    var c0 = window.M1P1;

    var b2 = window.P2P0;
    var d2 = window.P2P1;
    var c3 = window.P0P2;
    var d3 = window.P1P2;

    TDistance metric = default;

    // Sharp weighted diagonal differences using color distances
    var wd1 = DdtHelpers.Wp1 * DdtHelpers.GetDistance(metric, a.Key, d.Key)
              + DdtHelpers.Wp2 * (DdtHelpers.GetDistance(metric, b.Key, a1.Key) + DdtHelpers.GetDistance(metric, b.Key, d2.Key) + DdtHelpers.GetDistance(metric, c.Key, a0.Key) + DdtHelpers.GetDistance(metric, c.Key, d3.Key))
              + DdtHelpers.Wp3 * (DdtHelpers.GetDistance(metric, a1.Key, d2.Key) + DdtHelpers.GetDistance(metric, a0.Key, d3.Key));

    var wd2 = DdtHelpers.Wp1 * DdtHelpers.GetDistance(metric, b.Key, c.Key)
              + DdtHelpers.Wp2 * (DdtHelpers.GetDistance(metric, a.Key, b1.Key) + DdtHelpers.GetDistance(metric, a.Key, c0.Key) + DdtHelpers.GetDistance(metric, d.Key, b2.Key) + DdtHelpers.GetDistance(metric, d.Key, c3.Key))
              + DdtHelpers.Wp3 * (DdtHelpers.GetDistance(metric, b1.Key, c0.Key) + DdtHelpers.GetDistance(metric, b2.Key, c3.Key));

    // Output pixels
    dest[0] = encoder.Encode(DdtHelpers.DdtInterpolate(a.Work, b.Work, c.Work, d.Work, 0.25f, 0.25f, wd1, wd2, lerp));
    dest[1] = encoder.Encode(DdtHelpers.DdtInterpolate(a.Work, b.Work, c.Work, d.Work, 0.75f, 0.25f, wd1, wd2, lerp));
    dest[destStride] = encoder.Encode(DdtHelpers.DdtInterpolate(a.Work, b.Work, c.Work, d.Work, 0.25f, 0.75f, wd1, wd2, lerp));
    dest[destStride + 1] = encoder.Encode(DdtHelpers.DdtInterpolate(a.Work, b.Work, c.Work, d.Work, 0.75f, 0.75f, wd1, wd2, lerp));
  }
}

#endregion

#region DDT Extended Kernel

file readonly struct DdtExtendedKernel<TWork, TKey, TPixel, TDistance, TLerp, TEncode>(TLerp lerp = default)
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
    // Get 4x4 neighborhood
    var a = window.P0P0;
    var b = window.P1P0;
    var c = window.P0P1;
    var d = window.P1P1;

    // Extended neighborhood for voting
    var c00 = window.M1M1;
    var c01 = window.P0M1;
    var c02 = window.P1M1;
    var c03 = window.P2M1;

    var c10 = window.M1P0;
    var c20 = window.M1P1;
    var c13 = window.P2P0;
    var c23 = window.P2P1;

    var c30 = window.M1P2;
    var c31 = window.P0P2;
    var c32 = window.P1P2;
    var c33 = window.P2P2;

    TDistance metric = default;

    // Vote counting for diagonal preference using color distances
    var count1 = 0;
    var count2 = 0;

    // Compare diagonal distances for each voting pair
    var d1 = DdtHelpers.GetDistance(metric, c00.Key, a.Key);
    var d2 = DdtHelpers.GetDistance(metric, c01.Key, c10.Key);
    if (d1 < d2) ++count1;
    if (d1 > d2) ++count2;

    d1 = DdtHelpers.GetDistance(metric, c01.Key, b.Key);
    d2 = DdtHelpers.GetDistance(metric, c02.Key, a.Key);
    if (d1 < d2) ++count1;
    if (d1 > d2) ++count2;

    d1 = DdtHelpers.GetDistance(metric, c02.Key, c13.Key);
    d2 = DdtHelpers.GetDistance(metric, c03.Key, b.Key);
    if (d1 < d2) ++count1;
    if (d1 > d2) ++count2;

    d1 = DdtHelpers.GetDistance(metric, c10.Key, c.Key);
    d2 = DdtHelpers.GetDistance(metric, c20.Key, a.Key);
    if (d1 < d2) ++count1;
    if (d1 > d2) ++count2;

    d1 = DdtHelpers.GetDistance(metric, b.Key, c23.Key);
    d2 = DdtHelpers.GetDistance(metric, c13.Key, d.Key);
    if (d1 < d2) ++count1;
    if (d1 > d2) ++count2;

    d1 = DdtHelpers.GetDistance(metric, c20.Key, c31.Key);
    d2 = DdtHelpers.GetDistance(metric, c.Key, c30.Key);
    if (d1 < d2) ++count1;
    if (d1 > d2) ++count2;

    d1 = DdtHelpers.GetDistance(metric, c32.Key, c.Key);
    d2 = DdtHelpers.GetDistance(metric, c31.Key, d.Key);
    if (d1 < d2) ++count1;
    if (d1 > d2) ++count2;

    d1 = DdtHelpers.GetDistance(metric, c33.Key, d.Key);
    d2 = DdtHelpers.GetDistance(metric, c32.Key, c23.Key);
    if (d1 < d2) ++count1;
    if (d1 > d2) ++count2;

    // Main diagonal distances
    var distAD = DdtHelpers.GetDistance(metric, a.Key, d.Key);
    var distBC = DdtHelpers.GetDistance(metric, b.Key, c.Key);

    // Determine diagonal preference using votes
    float wd1, wd2;
    if (count1 >= 5 || (distAD < distBC && count2 <= 4)) {
      wd1 = 0;
      wd2 = 1;
    } else if (count2 >= 5 || distAD > distBC) {
      wd1 = 1;
      wd2 = 0;
    } else {
      wd1 = distAD;
      wd2 = distBC;
    }

    // Output pixels
    dest[0] = encoder.Encode(DdtHelpers.DdtInterpolate(a.Work, b.Work, c.Work, d.Work, 0.25f, 0.25f, wd1, wd2, lerp));
    dest[1] = encoder.Encode(DdtHelpers.DdtInterpolate(a.Work, b.Work, c.Work, d.Work, 0.75f, 0.25f, wd1, wd2, lerp));
    dest[destStride] = encoder.Encode(DdtHelpers.DdtInterpolate(a.Work, b.Work, c.Work, d.Work, 0.25f, 0.75f, wd1, wd2, lerp));
    dest[destStride + 1] = encoder.Encode(DdtHelpers.DdtInterpolate(a.Work, b.Work, c.Work, d.Work, 0.75f, 0.75f, wd1, wd2, lerp));
  }
}

#endregion
