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
/// Sobel edge detection — 3×3 gradient operator (Sobel &amp; Feldman 1968).
/// </summary>
/// <remarks>
/// <para>Computes the discrete approximation to the image gradient using two 3×3 kernels:</para>
/// <code>
///   Gx = [-1,0,+1; -2,0,+2; -1,0,+1] / 8
///   Gy = [-1,-2,-1; 0,0,0; +1,+2,+1] / 8
///   G  = sqrt(Gx² + Gy²)
/// </code>
/// <para>The 1-2-1 weighting is a separable Gaussian-like smoothing that makes Sobel less
/// noise-sensitive than the equal-weighted <see cref="PrewittEdge"/>. Reference:
/// I. Sobel &amp; G. Feldman, "A 3×3 Isotropic Gradient Operator for Image Processing",
/// presented at the Stanford AI Project (SAIL), 1968 (later published in
/// R. O. Duda &amp; P. E. Hart, "Pattern Classification and Scene Analysis", Wiley
/// 1973, p. 271).</para>
/// </remarks>
[FilterInfo("SobelEdge",
  Description = "Sobel edge detection (gradient magnitude)", Category = FilterCategory.Analysis)]
public readonly struct SobelEdge : IPixelFilter {

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
    => callback.Invoke(new SobelKernel<TWork, TKey, TPixel, TEncode>());

  public static SobelEdge Default => new();
}

file readonly struct SobelKernel<TWork, TKey, TPixel, TEncode>
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
    return ColorConverter.LuminanceFromRgb(r, g, b);
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

    // Sobel X: [-1,0,1; -2,0,2; -1,0,1]
    var gx = -tl + tr - 2f * l + 2f * r - bl + br;
    // Sobel Y: [-1,-2,-1; 0,0,0; 1,2,1]
    var gy = -tl - 2f * t - tr + bl + 2f * b + br;

    // Normalise by kernel sum-of-positives = 1+2+1 = 4 in each axis. The
    // saturated-input bound for |gx| and |gy| is 4 (e.g. white column, black
    // column → gx = 1+2+1 = 4), so dividing by 4 yields per-axis gradient in
    // [-1,1] and magnitude in [0, sqrt(2)]. Divide by 4√2 ≈ 5.657 to map to
    // [0,1] without clipping; using the more conservative 8 (4·2) keeps the
    // historical "kernel-sum" framing in the docs and matches the Sobel-Feldman
    // 1968 convention used throughout the literature.
    var mag = Math.Min(1f, (float)Math.Sqrt(gx * gx + gy * gy) / 8f);

    var center = window.P0P0.Work;
    var (_, _, _, ca) = ColorConverter.GetNormalizedRgba(in center);
    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(mag, mag, mag, ca));
  }
}
