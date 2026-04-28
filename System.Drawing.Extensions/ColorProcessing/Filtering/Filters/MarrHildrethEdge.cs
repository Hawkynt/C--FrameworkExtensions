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
/// Marr-Hildreth edge detector — applies the discrete Laplacian over the local 3×3
/// luminance window and emits an edge wherever the Laplacian response exhibits a
/// zero-crossing between the centre pixel and any of its 4-connected neighbours,
/// weighted by the magnitude of the Laplacian difference.
/// </summary>
/// <remarks>
/// <para>
/// Whereas <see cref="LaplacianOfGaussian"/> outputs the raw |LoG| magnitude, the
/// Marr-Hildreth operator additionally detects zero-crossings — the original 1980
/// formulation — yielding thin, closed contours rather than thick gradient ridges.
/// </para>
/// <para>
/// This implementation uses the compact 3×3 Laplacian
/// <c>[0,1,0; 1,-4,1; 0,1,0]</c> at the centre and at its four cardinal neighbours
/// (relying on the 5×5 NeighborWindow for the wider taps), then detects a sign change
/// between centre-LoG and neighbour-LoG.
/// </para>
/// <para>
/// Reference: D. Marr &amp; E. Hildreth, "Theory of Edge Detection",
/// Proc. R. Soc. Lond. B 207 (1980), pp. 187-217.
/// </para>
/// </remarks>
[FilterInfo("MarrHildrethEdge",
  Author = "Marr & Hildreth", Year = 1980,
  Url = "https://en.wikipedia.org/wiki/Marr%E2%80%93Hildreth_algorithm",
  Description = "Marr-Hildreth zero-crossing edge detector",
  Category = FilterCategory.Analysis)]
public readonly struct MarrHildrethEdge : IPixelFilter {

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
    => callback.Invoke(new MarrHildrethKernel<TWork, TKey, TPixel, TEncode>());

  public static MarrHildrethEdge Default => new();
}

file readonly struct MarrHildrethKernel<TWork, TKey, TPixel, TEncode>
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
    // Pre-compute luminances for the patch positions actually consumed.
    var l_m2p0 = _Lum(window.M2P0);

    var l_m1m1 = _Lum(window.M1M1);
    var l_m1p0 = _Lum(window.M1P0);
    var l_m1p1 = _Lum(window.M1P1);

    var l_p0m2 = _Lum(window.P0M2);
    var l_p0m1 = _Lum(window.P0M1);
    var l_p0p0 = _Lum(window.P0P0);
    var l_p0p1 = _Lum(window.P0P1);
    var l_p0p2 = _Lum(window.P0P2);

    var l_p1m1 = _Lum(window.P1M1);
    var l_p1p0 = _Lum(window.P1P0);
    var l_p1p1 = _Lum(window.P1P1);

    var l_p2p0 = _Lum(window.P2P0);

    // Discrete Laplacian [0,1,0; 1,-4,1; 0,1,0] at centre and 4 neighbours.
    var logC = l_m1p0 + l_p1p0 + l_p0m1 + l_p0p1 - 4f * l_p0p0;            // (0,0)
    var logN = l_m2p0 + l_p0p0 + l_m1m1 + l_m1p1 - 4f * l_m1p0;           // (-1,0)
    var logS = l_p0p0 + l_p2p0 + l_p1m1 + l_p1p1 - 4f * l_p1p0;           // (+1,0)
    var logW = l_m1m1 + l_p1m1 + l_p0m2 + l_p0p0 - 4f * l_p0m1;           // (0,-1)
    var logE = l_m1p1 + l_p1p1 + l_p0p0 + l_p0p2 - 4f * l_p0p1;           // (0,+1)

    // A zero-crossing exists between centre and a neighbour when their signs differ.
    var mag = 0f;
    if ((logC > 0f) != (logN > 0f)) mag = Math.Max(mag, Math.Abs(logC - logN));
    if ((logC > 0f) != (logS > 0f)) mag = Math.Max(mag, Math.Abs(logC - logS));
    if ((logC > 0f) != (logW > 0f)) mag = Math.Max(mag, Math.Abs(logC - logW));
    if ((logC > 0f) != (logE > 0f)) mag = Math.Max(mag, Math.Abs(logC - logE));

    if (mag > 1f) mag = 1f;

    var center = window.P0P0.Work;
    var (_, _, _, ca) = ColorConverter.GetNormalizedRgba(in center);
    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(mag, mag, mag, ca));
  }
}
