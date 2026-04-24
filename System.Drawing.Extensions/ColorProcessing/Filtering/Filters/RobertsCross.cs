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
/// Roberts Cross edge detection using 2×2 diagonal difference kernels.
/// </summary>
/// <remarks>
/// <para>
/// One of the earliest edge detectors (1963), using a pair of 2×2 diagonal
/// gradient kernels to approximate <c>|∂I/∂x| + |∂I/∂y|</c> in the rotated
/// 45° basis. Cheaper than Sobel/Prewitt/Scharr and produces thinner edges,
/// at the cost of being far more sensitive to noise.
/// </para>
/// <para>
/// Kx: <c>[[1,0],[0,-1]]</c> &nbsp;&nbsp; Ky: <c>[[0,1],[-1,0]]</c><br/>
/// The output pixel combines the top-left sample with its P1P1 diagonal
/// neighbour and the top-right sample with its P1M… neighbour.
/// </para>
/// <para>
/// Typical use case: quick preview edge maps, sub-pixel boundary tracing on
/// clean synthetic images, or pedagogical demonstrations of gradient
/// operators.
/// </para>
/// <para>
/// Reference: Roberts, L.G. (1963) <em>Machine Perception of Three-Dimensional
/// Solids</em>, MIT PhD thesis.
/// </para>
/// </remarks>
[FilterInfo("RobertsCross",
  Description = "Roberts Cross 2x2 diagonal edge detection", Category = FilterCategory.Analysis,
  Author = "Lawrence Roberts", Year = 1963)]
public readonly struct RobertsCross : IPixelFilter {

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
    => callback.Invoke(new RobertsCrossKernel<TWork, TKey, TPixel, TEncode>());

  /// <summary>Gets the default Roberts Cross filter.</summary>
  public static RobertsCross Default => new();
}

file readonly struct RobertsCrossKernel<TWork, TKey, TPixel, TEncode>
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
    // Anchor the 2x2 footprint at the current pixel (P0P0) with its P1P1 diagonal.
    var p00 = _Lum(window.P0P0);
    var p01 = _Lum(window.P0P1);
    var p10 = _Lum(window.P1P0);
    var p11 = _Lum(window.P1P1);

    // Gx = p00 - p11   (main diagonal)
    // Gy = p01 - p10   (anti-diagonal)
    var gx = p00 - p11;
    var gy = p01 - p10;

    var mag = Math.Min(1f, (float)Math.Sqrt(gx * gx + gy * gy));

    var center = window.P0P0.Work;
    var (_, _, _, ca) = ColorConverter.GetNormalizedRgba(in center);
    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(mag, mag, mag, ca));
  }
}
