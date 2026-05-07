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
using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Resizing;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Filtering.Filters;

/// <summary>
/// Photoshop-style Curves filter using monotone cubic Hermite (Fritsch-Carlson)
/// spline interpolation through user-supplied control points. The same curve is
/// applied to all RGB channels.
/// </summary>
[FilterInfo("Curves",
  Description = "Remap pixel values via monotone cubic Hermite spline through control points", Category = FilterCategory.ColorCorrection)]
public readonly struct Curves : IPixelFilter {
  private static readonly float[] _IdentityLut = _BuildIdentityLut();
  private readonly float[] _lut;

  private static float[] _BuildIdentityLut() {
    var lut = new float[256];
    for (var i = 0; i < 256; ++i)
      lut[i] = i / 255f;
    return lut;
  }

  /// <summary>
  /// Creates a Curves filter from an array of (input, output) control points.
  /// Inputs must be strictly ascending, the first input must be 0 and the last must be 1.
  /// Passing <c>null</c> yields the identity curve (used by reflection-based defaults).
  /// </summary>
  public Curves((float input, float output)[] controlPoints = null) {
    if (controlPoints == null)
      controlPoints = [(0f, 0f), (1f, 1f)];
    if (controlPoints.Length < 2)
      throw new ArgumentException("At least two control points are required.", nameof(controlPoints));
    if (controlPoints[0].input != 0f)
      throw new ArgumentException("First control point input must be 0.", nameof(controlPoints));
    if (controlPoints[controlPoints.Length - 1].input != 1f)
      throw new ArgumentException("Last control point input must be 1.", nameof(controlPoints));
    for (var i = 1; i < controlPoints.Length; ++i)
      if (controlPoints[i].input <= controlPoints[i - 1].input)
        throw new ArgumentException("Control points must be sorted by input ascending and strictly increasing.", nameof(controlPoints));

    this._lut = _BuildLut(controlPoints);
  }

  private static float[] _BuildLut((float input, float output)[] cp) {
    var n = cp.Length;
    var xs = new float[n];
    var ys = new float[n];
    for (var i = 0; i < n; ++i) {
      xs[i] = cp[i].input;
      ys[i] = cp[i].output;
    }

    // Fritsch-Carlson monotone cubic Hermite interpolation tangents.
    var d = new float[n - 1]; // secant slopes
    var dx = new float[n - 1];
    for (var i = 0; i < n - 1; ++i) {
      dx[i] = xs[i + 1] - xs[i];
      d[i] = (ys[i + 1] - ys[i]) / dx[i];
    }

    var m = new float[n]; // tangents at each knot
    m[0] = d[0];
    m[n - 1] = d[n - 2];
    for (var i = 1; i < n - 1; ++i)
      m[i] = (d[i - 1] + d[i]) * 0.5f;

    // Enforce monotonicity (Fritsch-Carlson step 3-5).
    for (var i = 0; i < n - 1; ++i) {
      if (d[i] == 0f) {
        m[i] = 0f;
        m[i + 1] = 0f;
        continue;
      }

      var a = m[i] / d[i];
      var b = m[i + 1] / d[i];
      // If a or b is negative, the slope sign disagrees with secant — clamp to 0.
      if (a < 0f) m[i] = 0f;
      if (b < 0f) m[i + 1] = 0f;

      var s = a * a + b * b;
      if (s > 9f) {
        var t = 3f / (float)Math.Sqrt(s);
        m[i] = t * a * d[i];
        m[i + 1] = t * b * d[i];
      }
    }

    var lut = new float[256];
    var seg = 0;
    for (var i = 0; i < 256; ++i) {
      var x = i / 255f;

      // Advance segment until xs[seg+1] >= x.
      while (seg < n - 2 && x > xs[seg + 1])
        ++seg;

      var h = dx[seg];
      var t = (x - xs[seg]) / h;
      var t2 = t * t;
      var t3 = t2 * t;
      // Hermite basis.
      var h00 = 2f * t3 - 3f * t2 + 1f;
      var h10 = t3 - 2f * t2 + t;
      var h01 = -2f * t3 + 3f * t2;
      var h11 = t3 - t2;

      var y = h00 * ys[seg] + h10 * h * m[seg] + h01 * ys[seg + 1] + h11 * h * m[seg + 1];
      if (y < 0f) y = 0f;
      else if (y > 1f) y = 1f;
      lut[i] = y;
    }

    return lut;
  }

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
    => callback.Invoke(new CurvesKernel<TWork, TKey, TPixel, TEncode>(this._lut ?? _IdentityLut));

  /// <summary>Identity curve: no change.</summary>
  public static Curves Identity => new(new[] { (0f, 0f), (1f, 1f) });

  /// <summary>Classic S-curve: increases contrast by lifting highlights and dropping shadows.</summary>
  public static Curves SCurve => new(new[] { (0f, 0f), (0.25f, 0.18f), (0.75f, 0.82f), (1f, 1f) });

  /// <summary>Brightens shadows while keeping highlights mostly intact.</summary>
  public static Curves LiftShadows => new(new[] { (0f, 0f), (0.25f, 0.4f), (0.75f, 0.85f), (1f, 1f) });

  public static Curves Default => Identity;
}

file readonly struct CurvesKernel<TWork, TKey, TPixel, TEncode>(float[] lut)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 1;
  public int ScaleY => 1;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float _Map(float v) {
    if (v <= 0f)
      return lut[0];
    if (v >= 1f)
      return lut[255];
    var idx = (int)(v * 255f + 0.5f);
    if (idx < 0) idx = 0;
    else if (idx > 255) idx = 255;
    return lut[idx];
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var pixel = window.P0P0.Work;
    var (r, g, b, a) = ColorConverter.GetNormalizedRgba(in pixel);
    r = this._Map(r);
    g = this._Map(g);
    b = this._Map(b);
    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(r, g, b, a));
  }
}
