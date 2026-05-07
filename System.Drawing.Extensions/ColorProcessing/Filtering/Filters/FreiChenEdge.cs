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
/// Frei-Chen edge detector — projects the local 3×3 luminance patch onto an orthonormal
/// basis of nine 3×3 templates (Frei &amp; Chen 1977) and returns the ratio of energy in
/// the four "edge" templates to the total energy. Output is in [0,1] and is more
/// rotationally isotropic than Sobel/Prewitt.
/// </summary>
/// <remarks>
/// <para>
/// The four edge templates are the Sobel-X / Sobel-Y / two diagonal Sobels (scaled by
/// 1/(2√2) so the basis is orthonormal). The square root of (sum of squared edge
/// projections / sum of all squared projections) gives a normalised edge cosine.
/// </para>
/// <para>
/// Reference: W. Frei &amp; C.-C. Chen, "Fast Boundary Detection: A Generalization
/// and a New Algorithm", IEEE TC, 1977.
/// </para>
/// </remarks>
[FilterInfo("FreiChenEdge",
  Author = "Frei & Chen", Year = 1977,
  Url = "https://en.wikipedia.org/wiki/Edge_detection",
  Description = "Frei-Chen edge detector (orthonormal 3×3 basis projection)",
  Category = FilterCategory.Analysis)]
public readonly struct FreiChenEdge : IPixelFilter {

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
    => callback.Invoke(new FreiChenKernel<TWork, TKey, TPixel, TEncode>());

  public static FreiChenEdge Default => new();
}

file readonly struct FreiChenKernel<TWork, TKey, TPixel, TEncode>
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
    // Layout: a b c
    //         d e f
    //         g h i
    var a = _Lum(window.M1M1);
    var b = _Lum(window.M1P0);
    var c = _Lum(window.M1P1);
    var d = _Lum(window.P0M1);
    var e = _Lum(window.P0P0);
    var f = _Lum(window.P0P1);
    var g = _Lum(window.P1M1);
    var h = _Lum(window.P1P0);
    var i = _Lum(window.P1P1);

    var sqrt2 = (float)Math.Sqrt(2.0);
    var inv = 1f / (2f * sqrt2);

    // Frei-Chen edge basis templates 1..4 — all zero-mean over the 3×3 patch.
    // f1 (horizontal edge):   [ 1  √2  1 ;  0  0  0 ; -1 -√2 -1 ]
    // f2 (vertical edge):     [ 1   0 -1 ; √2  0 -√2;  1   0 -1 ]
    // f3 (diag 1, NE-SW):     [ √2 -1  0 ;-1   0  1 ;  0   1 -√2]
    // f4 (diag 2, NW-SE):     [ 0  -1  √2;  1   0 -1 ; -√2  1   0]
    var m1 = inv * (a + sqrt2 * b + c - g - sqrt2 * h - i);
    var m2 = inv * (a - c + sqrt2 * d - sqrt2 * f + g - i);
    var m3 = inv * (sqrt2 * a - b - d + f + h - sqrt2 * i);
    var m4 = inv * (-b + sqrt2 * c + d - f - sqrt2 * g + h);
    var edgeEnergy = m1 * m1 + m2 * m2 + m3 * m3 + m4 * m4;

    // Total energy = sum of squared luminances over the patch (basis is orthonormal).
    var totalEnergy = a * a + b * b + c * c + d * d + e * e + f * f + g * g + h * h + i * i;
    if (totalEnergy < 1e-12f)
      totalEnergy = 1e-12f;

    var ratio = edgeEnergy / totalEnergy;
    if (ratio < 0f) ratio = 0f;
    else if (ratio > 1f) ratio = 1f;
    var mag = (float)Math.Sqrt(ratio);

    var center = window.P0P0.Work;
    var (_, _, _, ca) = ColorConverter.GetNormalizedRgba(in center);
    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(mag, mag, mag, ca));
  }
}
