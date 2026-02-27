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
/// SABR variant type.
/// </summary>
public enum SabrVariant {
  /// <summary>Standard SABR (threshold=0.32, coef=2.0).</summary>
  Standard,
  /// <summary>Sharp SABR - more edge detection (threshold=0.20, coef=2.5).</summary>
  Sharp,
  /// <summary>Smooth SABR - less edge detection (threshold=0.45, coef=1.5).</summary>
  Smooth
}

/// <summary>
/// Joshua Street's SABR (Scalable Bicubic Renderer) v3.0 pixel-art scaling filter.
/// </summary>
/// <remarks>
/// <para>Reference: Joshua Street, with portions from Hyllian's 5xBR v3.7c</para>
/// <para>See: https://github.com/libretro/common-shaders/tree/master/sabr</para>
/// <para>Algorithm: Uses 21-point (5x5 minus corners) neighborhood analysis with
/// multi-directional edge detection (45/30/60 degrees) and smoothstep blending
/// for smooth edge interpolation at integer scale factors.</para>
/// <para>Supports 2x, 3x, and 4x scaling with Standard, Sharp, and Smooth variants.</para>
/// </remarks>
[ScalerInfo("SABR", Author = "Joshua Street", Year = 2012,
  Url = "https://github.com/libretro/common-shaders/tree/master/sabr",
  Description = "Scalable bicubic renderer with multi-angle edge detection", Category = ScalerCategory.PixelArt)]
public readonly struct Sabr : IPixelScaler {
  private readonly int _scale;
  private readonly SabrVariant _variant;

  /// <summary>
  /// Creates a SABR scaler with specified scale factor and variant.
  /// </summary>
  /// <param name="scale">Scale factor (2, 3, or 4).</param>
  /// <param name="variant">The SABR variant to use.</param>
  public Sabr(int scale = 2, SabrVariant variant = SabrVariant.Standard) {
    ArgumentOutOfRangeException.ThrowIfLessThan(scale, 2);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(scale, 4);
    this._scale = scale;
    this._variant = variant;
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
    where TEncode : struct, IEncode<TWork, TPixel> {
    var (threshold, coef) = this._variant switch {
      SabrVariant.Sharp => (SabrHelpers.SharpThreshold, SabrHelpers.SharpCoef),
      SabrVariant.Smooth => (SabrHelpers.SmoothThreshold, SabrHelpers.SmoothCoef),
      _ => (SabrHelpers.StandardThreshold, SabrHelpers.StandardCoef)
    };
    return this._scale switch {
      0 or 2 => callback.Invoke(new Sabr2xKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode>(equality, default, lerp, threshold, coef)),
      3 => callback.Invoke(new Sabr3xKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode>(equality, default, lerp, threshold, coef)),
      4 => callback.Invoke(new Sabr4xKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode>(equality, default, lerp, threshold, coef)),
      _ => throw new InvalidOperationException($"Invalid scale factor: {this._scale}")
    };
  }

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

  /// <summary>Gets a standard 2x SABR scaler.</summary>
  public static Sabr X2 => new(2);
  /// <summary>Gets a standard 3x SABR scaler.</summary>
  public static Sabr X3 => new(3);
  /// <summary>Gets a standard 4x SABR scaler.</summary>
  public static Sabr X4 => new(4);
  /// <summary>Gets the default SABR scaler (standard 2x).</summary>
  public static Sabr Default => X2;

  #endregion
}

/// <summary>
/// SABR Sharp variant - more aggressive edge detection.
/// </summary>
[ScalerInfo("SABR Sharp", Author = "Joshua Street", Year = 2012,
  Url = "https://github.com/libretro/common-shaders/tree/master/sabr",
  Description = "SABR with sharper edge detection", Category = ScalerCategory.PixelArt)]
public readonly struct SabrSharp : IPixelScaler {
  private readonly int _scale;

  /// <summary>
  /// Creates a Sharp SABR scaler with specified scale factor.
  /// </summary>
  /// <param name="scale">Scale factor (2, 3, or 4).</param>
  public SabrSharp(int scale = 2) {
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
      0 or 2 => callback.Invoke(new Sabr2xKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode>(equality, default, lerp, SabrHelpers.SharpThreshold, SabrHelpers.SharpCoef)),
      3 => callback.Invoke(new Sabr3xKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode>(equality, default, lerp, SabrHelpers.SharpThreshold, SabrHelpers.SharpCoef)),
      4 => callback.Invoke(new Sabr4xKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode>(equality, default, lerp, SabrHelpers.SharpThreshold, SabrHelpers.SharpCoef)),
      _ => throw new InvalidOperationException($"Invalid scale factor: {this._scale}")
    };

  /// <summary>Gets the list of scale factors supported.</summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2), new(3, 3), new(4, 4)];
  /// <summary>Determines whether the specified scale factor is supported.</summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 } or { X: 3, Y: 3 } or { X: 4, Y: 4 };
  /// <summary>Enumerates all possible target dimensions.</summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
    yield return (sourceWidth * 3, sourceHeight * 3);
    yield return (sourceWidth * 4, sourceHeight * 4);
  }

  /// <summary>Gets a sharp 2x SABR scaler.</summary>
  public static SabrSharp X2 => new(2);
  /// <summary>Gets a sharp 3x SABR scaler.</summary>
  public static SabrSharp X3 => new(3);
  /// <summary>Gets a sharp 4x SABR scaler.</summary>
  public static SabrSharp X4 => new(4);
  /// <summary>Gets the default Sharp SABR scaler (2x).</summary>
  public static SabrSharp Default => X2;
}

/// <summary>
/// SABR Smooth variant - less edge detection for smoother output.
/// </summary>
[ScalerInfo("SABR Smooth", Author = "Joshua Street", Year = 2012,
  Url = "https://github.com/libretro/common-shaders/tree/master/sabr",
  Description = "SABR with smoother edge blending", Category = ScalerCategory.PixelArt)]
public readonly struct SabrSmooth : IPixelScaler {
  private readonly int _scale;

  /// <summary>
  /// Creates a Smooth SABR scaler with specified scale factor.
  /// </summary>
  /// <param name="scale">Scale factor (2, 3, or 4).</param>
  public SabrSmooth(int scale = 2) {
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
      0 or 2 => callback.Invoke(new Sabr2xKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode>(equality, default, lerp, SabrHelpers.SmoothThreshold, SabrHelpers.SmoothCoef)),
      3 => callback.Invoke(new Sabr3xKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode>(equality, default, lerp, SabrHelpers.SmoothThreshold, SabrHelpers.SmoothCoef)),
      4 => callback.Invoke(new Sabr4xKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode>(equality, default, lerp, SabrHelpers.SmoothThreshold, SabrHelpers.SmoothCoef)),
      _ => throw new InvalidOperationException($"Invalid scale factor: {this._scale}")
    };

  /// <summary>Gets the list of scale factors supported.</summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2), new(3, 3), new(4, 4)];
  /// <summary>Determines whether the specified scale factor is supported.</summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 } or { X: 3, Y: 3 } or { X: 4, Y: 4 };
  /// <summary>Enumerates all possible target dimensions.</summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
    yield return (sourceWidth * 3, sourceHeight * 3);
    yield return (sourceWidth * 4, sourceHeight * 4);
  }

  /// <summary>Gets a smooth 2x SABR scaler.</summary>
  public static SabrSmooth X2 => new(2);
  /// <summary>Gets a smooth 3x SABR scaler.</summary>
  public static SabrSmooth X3 => new(3);
  /// <summary>Gets a smooth 4x SABR scaler.</summary>
  public static SabrSmooth X4 => new(4);
  /// <summary>Gets the default Smooth SABR scaler (2x).</summary>
  public static SabrSmooth Default => X2;
}

#region SABR Helpers

file static class SabrHelpers {
  // Edge detection thresholds (UNorm32 raw value scale: threshold * UNorm32.One / 100)
  // These map to perceptual distances: 0.32 -> 32, 0.20 -> 20, 0.45 -> 45
  public const uint StandardThreshold = (uint)(0.032 * uint.MaxValue);
  public const uint SharpThreshold = (uint)(0.020 * uint.MaxValue);
  public const uint SmoothThreshold = (uint)(0.045 * uint.MaxValue);

  // Angle coefficients (scaled by 100)
  public const int StandardCoef = 200;
  public const int SharpCoef = 250;
  public const int SmoothCoef = 150;

  // Weight scale for integer lerp operations
  public const int WeightScale = 1000;

  /// <summary>
  /// Computes distance between two keys using the color metric.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint Dist<TKey, TMetric>(in TKey a, in TKey b, TMetric metric)
    where TKey : unmanaged, IColorSpace
    where TMetric : struct, IColorMetric<TKey>
    => metric.Distance(a, b).RawValue;

  /// <summary>
  /// Weighted distance measure from the SABR shader using pairwise color metric distances.
  /// d(a,b) + d(a,c) + d(d,e) + d(d,f) + 4*d(g,h)
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ulong WeightedDistance<TKey, TMetric>(
    in TKey a, in TKey b, in TKey c,
    in TKey d, in TKey e, in TKey f,
    in TKey g, in TKey h,
    TMetric metric)
    where TKey : unmanaged, IColorSpace
    where TMetric : struct, IColorMetric<TKey>
    => (ulong)Dist(a, b, metric) + Dist(a, c, metric) + Dist(d, e, metric) + Dist(d, f, metric) + 4ul * Dist(g, h, metric);

  /// <summary>
  /// Integer smoothstep: returns a value in [0, WeightScale] representing the smoothstep blend.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Smoothstep(int edge0, int edge1, int x) {
    if (x <= edge0)
      return 0;
    if (x >= edge1)
      return WeightScale;

    var t = (x - edge0) * WeightScale / (edge1 - edge0);
    return (int)((long)t * t * (3 * WeightScale - 2 * t) / ((long)WeightScale * WeightScale));
  }

  /// <summary>
  /// Core SABR edge detection and blend weight computation using color metric distances.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int ComputeBlendWeight<TKey, TMetric, TEquality>(
    in TKey k7, in TKey k8, in TKey k11, in TKey k12, in TKey k13,
    in TKey k14, in TKey k16, in TKey k17, in TKey k18, in TKey k19,
    in TKey k22, in TKey k23,
    int fpx, int fpy,
    uint threshold, int coef,
    TMetric metric, TEquality equality,
    out bool edgePixelIsP17
  )
    where TKey : unmanaged, IColorSpace
    where TMetric : struct, IColorMetric<TKey>
    where TEquality : struct, IColorEquality<TKey> {
    // 45 degree edge detection using weighted distance metric
    var e45 = WeightedDistance(k12, k8, k16, k18, k22, k14, k17, k13, metric);
    var econt = WeightedDistance(k17, k11, k23, k13, k7, k19, k12, k18, metric);

    // 30 and 60 degree edge detection
    var e30 = Dist(k13, k16, metric);
    var e60 = Dist(k8, k17, metric);

    // Edge direction rules using equality and distance
    var d12_13 = Dist(k12, k13, metric);
    var d12_17 = Dist(k12, k17, metric);

    var r45 = d12_13 > 0 && d12_17 > 0 && (
      (!equality.Equals(k13, k7) && !equality.Equals(k13, k8)) ||
      (!equality.Equals(k17, k11) && !equality.Equals(k17, k16)) ||
      (equality.Equals(k12, k18) && (
        (!equality.Equals(k13, k14) && !equality.Equals(k13, k19)) ||
        (!equality.Equals(k17, k22) && !equality.Equals(k17, k23)))) ||
      equality.Equals(k12, k16) ||
      equality.Equals(k12, k8));

    var r30 = d12_17 > threshold && Dist(k11, k16, metric) > threshold;
    var r60 = d12_13 > threshold && Dist(k7, k8, metric) > threshold;

    // Edge detection results
    var edr45 = e45 < econt && r45;
    var edrrn = e45 <= econt;
    var edr30 = (ulong)e30 * (uint)coef <= (ulong)e60 * 100 && r30;
    var edr60 = (ulong)e60 * (uint)coef <= (ulong)e30 * 100 && r60;

    // Determine final edge types
    var final45 = !edr30 && !edr60 && edr45;
    var final30 = edr45 && edr30 && !edr60;
    var final60 = edr45 && edr60 && !edr30;
    var final36 = edr45 && edr30 && edr60;
    var finalrn = !edr45 && edrrn;

    // Angle mask computations (subpixel position in [0, 100])
    var val45 = fpy + fpx;
    var ma45 = Smoothstep(110, 190, val45);
    var ma30 = Smoothstep(80, 120, val45);
    var ma60 = Smoothstep(160, 240, fpy + 2 * fpx);
    var marn = Smoothstep(130, 210, val45);

    // Pixel selection: blend toward closer neighbor
    edgePixelIsP17 = d12_17 < d12_13;

    // Calculate blending mask
    var mac = 0;
    if (final36)
      mac = Math.Max(ma30, ma60);
    else if (final30)
      mac = ma30;
    else if (final60)
      mac = ma60;
    else if (final45)
      mac = ma45;
    else if (finalrn)
      mac = marn;

    return mac;
  }
}

#endregion

#region SABR 2x Kernel

file readonly struct Sabr2xKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode>(
  TEquality equality, TDistance metric, TLerp lerp, uint threshold, int coef)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TDistance : struct, IColorMetric<TKey>
  where TEquality : struct, IColorEquality<TKey>
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
    // Sample 21-point neighborhood keys for edge detection
    var k7 = window.M1P0.Key;
    var k8 = window.M1P1.Key;
    var k11 = window.P0M1.Key;
    var k12 = window.P0P0.Key;
    var k13 = window.P0P1.Key;
    var k14 = window.P0P2.Key;
    var k16 = window.P1M1.Key;
    var k17 = window.P1P0.Key;
    var k18 = window.P1P1.Key;
    var k19 = window.P1P2.Key;
    var k22 = window.P2P0.Key;
    var k23 = window.P2P1.Key;

    var centerWork = window.P0P0.Work;
    var p13Work = window.P0P1.Work;
    var p17Work = window.P1P0.Work;

    for (var oy = 0; oy < 2; ++oy)
    for (var ox = 0; ox < 2; ++ox) {
      var fpx = (ox * 2 + 1) * 100 / (2 * 2);
      var fpy = (oy * 2 + 1) * 100 / (2 * 2);

      var mac = SabrHelpers.ComputeBlendWeight(
        k7, k8, k11, k12, k13, k14, k16, k17, k18, k19, k22, k23,
        fpx, fpy, threshold, coef, metric, equality, out var edgePixelIsP17);

      TWork result;
      if (mac <= 0)
        result = centerWork;
      else {
        var edgeWork = edgePixelIsP17 ? p17Work : p13Work;
        result = lerp.Lerp(centerWork, edgeWork, SabrHelpers.WeightScale - mac, mac);
      }

      dest[oy * destStride + ox] = encoder.Encode(result);
    }
  }
}

#endregion

#region SABR 3x Kernel

file readonly struct Sabr3xKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode>(
  TEquality equality, TDistance metric, TLerp lerp, uint threshold, int coef)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TDistance : struct, IColorMetric<TKey>
  where TEquality : struct, IColorEquality<TKey>
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
    var k7 = window.M1P0.Key;
    var k8 = window.M1P1.Key;
    var k11 = window.P0M1.Key;
    var k12 = window.P0P0.Key;
    var k13 = window.P0P1.Key;
    var k14 = window.P0P2.Key;
    var k16 = window.P1M1.Key;
    var k17 = window.P1P0.Key;
    var k18 = window.P1P1.Key;
    var k19 = window.P1P2.Key;
    var k22 = window.P2P0.Key;
    var k23 = window.P2P1.Key;

    var centerWork = window.P0P0.Work;
    var p13Work = window.P0P1.Work;
    var p17Work = window.P1P0.Work;

    for (var oy = 0; oy < 3; ++oy)
    for (var ox = 0; ox < 3; ++ox) {
      var fpx = (ox * 2 + 1) * 100 / (2 * 3);
      var fpy = (oy * 2 + 1) * 100 / (2 * 3);

      var mac = SabrHelpers.ComputeBlendWeight(
        k7, k8, k11, k12, k13, k14, k16, k17, k18, k19, k22, k23,
        fpx, fpy, threshold, coef, metric, equality, out var edgePixelIsP17);

      TWork result;
      if (mac <= 0)
        result = centerWork;
      else {
        var edgeWork = edgePixelIsP17 ? p17Work : p13Work;
        result = lerp.Lerp(centerWork, edgeWork, SabrHelpers.WeightScale - mac, mac);
      }

      dest[oy * destStride + ox] = encoder.Encode(result);
    }
  }
}

#endregion

#region SABR 4x Kernel

file readonly struct Sabr4xKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode>(
  TEquality equality, TDistance metric, TLerp lerp, uint threshold, int coef)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TDistance : struct, IColorMetric<TKey>
  where TEquality : struct, IColorEquality<TKey>
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
    var k7 = window.M1P0.Key;
    var k8 = window.M1P1.Key;
    var k11 = window.P0M1.Key;
    var k12 = window.P0P0.Key;
    var k13 = window.P0P1.Key;
    var k14 = window.P0P2.Key;
    var k16 = window.P1M1.Key;
    var k17 = window.P1P0.Key;
    var k18 = window.P1P1.Key;
    var k19 = window.P1P2.Key;
    var k22 = window.P2P0.Key;
    var k23 = window.P2P1.Key;

    var centerWork = window.P0P0.Work;
    var p13Work = window.P0P1.Work;
    var p17Work = window.P1P0.Work;

    for (var oy = 0; oy < 4; ++oy)
    for (var ox = 0; ox < 4; ++ox) {
      var fpx = (ox * 2 + 1) * 100 / (2 * 4);
      var fpy = (oy * 2 + 1) * 100 / (2 * 4);

      var mac = SabrHelpers.ComputeBlendWeight(
        k7, k8, k11, k12, k13, k14, k16, k17, k18, k19, k22, k23,
        fpx, fpy, threshold, coef, metric, equality, out var edgePixelIsP17);

      TWork result;
      if (mac <= 0)
        result = centerWork;
      else {
        var edgeWork = edgePixelIsP17 ? p17Work : p13Work;
        result = lerp.Lerp(centerWork, edgeWork, SabrHelpers.WeightScale - mac, mac);
      }

      dest[oy * destStride + ox] = encoder.Encode(result);
    }
  }
}

#endregion
