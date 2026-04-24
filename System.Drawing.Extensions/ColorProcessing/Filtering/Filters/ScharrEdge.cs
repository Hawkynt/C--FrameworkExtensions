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
/// Scharr edge detection using rotation-optimized 3×3 gradient kernels.
/// </summary>
/// <remarks>
/// <para>
/// The Scharr operator is a refinement of the Sobel operator that minimises
/// the weighted mean-squared angular error in Fourier domain, giving much
/// better rotational symmetry than Sobel or Prewitt. It is therefore the
/// preferred choice when gradient <em>direction</em> matters (e.g. optical
/// flow, structure tensors) rather than pure magnitude.
/// </para>
/// <para>
/// X kernel: <c>[-3,0,3; -10,0,10; -3,0,3]</c><br/>
/// Y kernel: <c>[-3,-10,-3; 0,0,0; 3,10,3]</c>
/// </para>
/// <para>
/// Typical use case: high-quality edge detection for computer-vision
/// pipelines where gradient orientation needs to be preserved across
/// arbitrary image rotations.
/// </para>
/// <para>
/// Reference: Scharr, H. (2000) <em>Optimal Operators in Digital Image
/// Processing</em>, PhD thesis, Heidelberg University.
/// </para>
/// </remarks>
[FilterInfo("ScharrEdge",
  Description = "Scharr edge detection (rotation-optimal gradient)", Category = FilterCategory.Analysis,
  Author = "Hannes Scharr", Year = 2000)]
public readonly struct ScharrEdge : IPixelFilter {

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
    => callback.Invoke(new ScharrKernel<TWork, TKey, TPixel, TEncode>());

  /// <summary>Gets the default Scharr edge filter.</summary>
  public static ScharrEdge Default => new();
}

file readonly struct ScharrKernel<TWork, TKey, TPixel, TEncode>
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
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var tl = _Lum(window.M1M1);
    var t = _Lum(window.M1P0);
    var tr = _Lum(window.M1P1);
    var l = _Lum(window.P0M1);
    var r = _Lum(window.P0P1);
    var bl = _Lum(window.P1M1);
    var b = _Lum(window.P1P0);
    var br = _Lum(window.P1P1);

    // Scharr X: [-3,0,3; -10,0,10; -3,0,3]
    var gx = -3f * tl + 3f * tr - 10f * l + 10f * r - 3f * bl + 3f * br;
    // Scharr Y: [-3,-10,-3; 0,0,0; 3,10,3]
    var gy = -3f * tl - 10f * t - 3f * tr + 3f * bl + 10f * b + 3f * br;

    // Normalise: kernel sum-of-positives = 16, so mag ≤ 16√2 for saturated input.
    var mag = Math.Min(1f, (float)Math.Sqrt(gx * gx + gy * gy) / 16f);

    var center = window.P0P0.Work;
    var (_, _, _, ca) = ColorConverter.GetNormalizedRgba(in center);
    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(mag, mag, mag, ca));
  }
}
