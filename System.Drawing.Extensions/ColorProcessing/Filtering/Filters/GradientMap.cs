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
using Hawkynt.ColorProcessing.Constants;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Resizing;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Filtering.Filters;

/// <summary>
/// Photoshop-style Gradient Map filter: replaces each pixel's BT.601 luminance
/// with the colour sampled from a user-supplied gradient (sorted list of stops).
/// Linear interpolation between adjacent stops; endpoints are clamped.
/// Alpha is preserved.
/// </summary>
[FilterInfo("GradientMap",
  Description = "Map luminance to gradient colours", Category = FilterCategory.Artistic)]
public readonly struct GradientMap : IPixelFilter {
  private readonly float[] _positions;
  private readonly float[] _r;
  private readonly float[] _g;
  private readonly float[] _b;

  /// <summary>
  /// Creates a GradientMap filter from an array of stops. Each stop is a tuple
  /// <c>(position, r, g, b)</c> with all components in <c>[0,1]</c>.
  /// Stops must be sorted by position ascending; the first stop's position must
  /// be 0 and the last stop's position must be 1; at least two stops are required.
  /// Passing <c>null</c> yields the black-and-white identity gradient
  /// (used by reflection-based defaults).
  /// </summary>
  public GradientMap((float position, float r, float g, float b)[] stops = null) {
    if (stops == null)
      stops = [(0f, 0f, 0f, 0f), (1f, 1f, 1f, 1f)];
    if (stops.Length < 2)
      throw new ArgumentException("At least two stops are required.", nameof(stops));
    if (stops[0].position != 0f)
      throw new ArgumentException("First stop position must be 0.", nameof(stops));
    if (stops[stops.Length - 1].position != 1f)
      throw new ArgumentException("Last stop position must be 1.", nameof(stops));
    for (var i = 1; i < stops.Length; ++i)
      if (stops[i].position < stops[i - 1].position)
        throw new ArgumentException("Stops must be sorted by position ascending.", nameof(stops));

    var n = stops.Length;
    this._positions = new float[n];
    this._r = new float[n];
    this._g = new float[n];
    this._b = new float[n];
    for (var i = 0; i < n; ++i) {
      this._positions[i] = stops[i].position;
      this._r[i] = stops[i].r;
      this._g[i] = stops[i].g;
      this._b[i] = stops[i].b;
    }
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
    => callback.Invoke(new GradientMapKernel<TWork, TKey, TPixel, TEncode>(
      this._positions, this._r, this._g, this._b));

  /// <summary>Identity-ish: maps luminance directly to grey (equivalent to grayscale).</summary>
  public static GradientMap BlackWhite => new(new[] {
    (0f, 0f, 0f, 0f),
    (1f, 1f, 1f, 1f)
  });

  /// <summary>Heatmap: blue → red → yellow.</summary>
  public static GradientMap Heatmap => new(new[] {
    (0f, 0f, 0f, 0.5f),
    (0.5f, 1f, 0f, 0f),
    (1f, 1f, 1f, 0f)
  });

  /// <summary>Warm sepia tone: black → mid-sepia → warm cream.</summary>
  public static GradientMap Sepia => new(new[] {
    (0f, 0f, 0f, 0f),
    (0.5f, 0.5f, 0.35f, 0.2f),
    (1f, 1f, 0.95f, 0.85f)
  });

  public static GradientMap Default => BlackWhite;
}

file readonly struct GradientMapKernel<TWork, TKey, TPixel, TEncode>(
  float[] positions, float[] r, float[] g, float[] b)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 1;
  public int ScaleY => 1;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var pixel = window.P0P0.Work;
    var (pr, pg, pb, pa) = ColorConverter.GetNormalizedRgba(in pixel);
    var lum = ColorConverter.LuminanceFromRgb(pr, pg, pb);

    var n = positions.Length;
    float or, og, ob;
    if (lum <= positions[0]) {
      or = r[0]; og = g[0]; ob = b[0];
    } else if (lum >= positions[n - 1]) {
      or = r[n - 1]; og = g[n - 1]; ob = b[n - 1];
    } else {
      // Find segment [i, i+1] with positions[i] <= lum < positions[i+1].
      var i = 0;
      while (i < n - 2 && lum >= positions[i + 1])
        ++i;
      var p0 = positions[i];
      var p1 = positions[i + 1];
      var span = p1 - p0;
      var t = span > 0f ? (lum - p0) / span : 0f;
      or = r[i] + (r[i + 1] - r[i]) * t;
      og = g[i] + (g[i + 1] - g[i]) * t;
      ob = b[i] + (b[i + 1] - b[i]) * t;
    }

    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(or, og, ob, pa));
  }
}
