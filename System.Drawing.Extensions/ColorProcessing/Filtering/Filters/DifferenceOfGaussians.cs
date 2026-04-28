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
/// Difference of Gaussians (DoG) edge / blob detector — subtracts a wider Gaussian blur
/// from a narrower one, leaving a band-pass response that approximates the Laplacian of
/// Gaussian. Operates on luminance and outputs the (rectified, normalised) magnitude of
/// the difference replicated to all RGB channels.
/// </summary>
/// <remarks>
/// <para>
/// Default σ pair is (1.0, 1.6) — the classic ratio used by Marr &amp; Hildreth (1980)
/// and SIFT (Lowe 2004) — implemented as an explicit 5×5 convolution per σ over the
/// <see cref="NeighborWindow{TWork,TKey}"/>, so no frame access is required.
/// </para>
/// <para>
/// Use case: feature detection, edge enhancement, biological-vision-inspired filtering.
/// For a scale-space style multi-octave version use the SIFT-based pipelines elsewhere.
/// </para>
/// </remarks>
[FilterInfo("DifferenceOfGaussians",
  Author = "Marr & Hildreth", Year = 1980,
  Url = "https://en.wikipedia.org/wiki/Difference_of_Gaussians",
  Description = "Difference of Gaussians band-pass edge response (σ=1.0/1.6)",
  Category = FilterCategory.Analysis)]
public readonly struct DifferenceOfGaussians : IPixelFilter {

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
    => callback.Invoke(new DoGKernel<TWork, TKey, TPixel, TEncode>());

  public static DifferenceOfGaussians Default => new();
}

file readonly struct DoGKernel<TWork, TKey, TPixel, TEncode>
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 1;
  public int ScaleY => 1;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Lum(in NeighborPixel<TWork, TKey> p) {
    var px = p.Work;
    var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in px);
    return ColorMatrices.BT601_R * r + ColorMatrices.BT601_G * g + ColorMatrices.BT601_B * b;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _GaussSum(in NeighborWindow<TWork, TKey> w, float sigma) {
    // Pre-computed normalized 1D weights for radius 2 at given sigma.
    var inv2s2 = 1.0 / (2.0 * sigma * sigma);
    var w0 = 1.0;
    var w1 = Math.Exp(-1.0 * inv2s2);
    var w2 = Math.Exp(-4.0 * inv2s2);
    var sum1d = w0 + 2.0 * w1 + 2.0 * w2;
    var n0 = (float)(w0 / sum1d);
    var n1 = (float)(w1 / sum1d);
    var n2 = (float)(w2 / sum1d);

    // Separable 2D convolution: rows weighted then summed by row weight.
    float Row(NeighborPixel<TWork, TKey> a, NeighborPixel<TWork, TKey> b, NeighborPixel<TWork, TKey> c, NeighborPixel<TWork, TKey> d, NeighborPixel<TWork, TKey> e)
      => _Lum(a) * n2 + _Lum(b) * n1 + _Lum(c) * n0 + _Lum(d) * n1 + _Lum(e) * n2;

    var r0 = Row(w.M2M2, w.M2M1, w.M2P0, w.M2P1, w.M2P2);
    var r1 = Row(w.M1M2, w.M1M1, w.M1P0, w.M1P1, w.M1P2);
    var r2 = Row(w.P0M2, w.P0M1, w.P0P0, w.P0P1, w.P0P2);
    var r3 = Row(w.P1M2, w.P1M1, w.P1P0, w.P1P1, w.P1P2);
    var r4 = Row(w.P2M2, w.P2M1, w.P2P0, w.P2P1, w.P2P2);

    return r0 * n2 + r1 * n1 + r2 * n0 + r3 * n1 + r4 * n2;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var gNarrow = _GaussSum(in window, 1.0f);
    var gWide = _GaussSum(in window, 1.6f);
    var diff = gNarrow - gWide;
    // Centre and scale to 0..1 — typical DoG amplitudes are tiny, so amplify.
    var v = 0.5f + diff * 4f;
    if (v < 0f) v = 0f;
    else if (v > 1f) v = 1f;

    var center = window.P0P0.Work;
    var (_, _, _, ca) = ColorConverter.GetNormalizedRgba(in center);
    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(v, v, v, ca));
  }
}
