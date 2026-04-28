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
/// Laplacian sharpening — adds a fraction of the discrete Laplacian back to the source
/// image, accentuating high-frequency detail. Uses the 8-connected Laplacian
/// <c>[1,1,1; 1,-8,1; 1,1,1]</c> applied per RGB channel and adds <c>amount × Laplacian</c>
/// to the original pixel.
/// </summary>
/// <remarks>
/// <para>
/// This is distinct from <see cref="LaplacianEdge"/> (which outputs the raw Laplacian
/// magnitude) and from <see cref="UnsharpMask"/> (which uses Gaussian-blur subtraction):
/// it is the simplest single-pass discrete-Laplacian sharpening operator described in
/// Gonzalez &amp; Woods §3.6.2.
/// </para>
/// <para>
/// Default <paramref name="amount"/> = 0.5. Negative values produce a blur-like effect
/// (Laplacian smoothing); values &gt; 1 over-sharpen and may clip.
/// </para>
/// </remarks>
[FilterInfo("LaplacianSharpen",
  Author = "Gonzalez & Woods", Year = 1992,
  Url = "https://en.wikipedia.org/wiki/Discrete_Laplace_operator",
  Description = "Laplacian sharpening — image + amount × Laplacian per channel",
  Category = FilterCategory.Enhancement)]
public readonly struct LaplacianSharpen : IPixelFilter {
  private readonly float _amount;

  public LaplacianSharpen() : this(0.5f) { }

  public LaplacianSharpen(float amount) {
    this._amount = Math.Max(-2f, Math.Min(4f, amount));
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
    => callback.Invoke(new LaplacianSharpenKernel<TWork, TKey, TPixel, TEncode>(this._amount));

  public static LaplacianSharpen Default => new();
}

file readonly struct LaplacianSharpenKernel<TWork, TKey, TPixel, TEncode>(float amount)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 1;
  public int ScaleY => 1;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static (float r, float g, float b) _Rgb(in NeighborPixel<TWork, TKey> p) {
    var px = p.Work;
    var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in px);
    return (r, g, b);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var (cr, cg, cb) = _Rgb(window.P0P0);
    var (tlr, tlg, tlb) = _Rgb(window.M1M1);
    var (tr, tg, tb) = _Rgb(window.M1P0);
    var (trr, trg, trb) = _Rgb(window.M1P1);
    var (lr, lg, lb) = _Rgb(window.P0M1);
    var (rr, rg, rb) = _Rgb(window.P0P1);
    var (blr, blg, blb) = _Rgb(window.P1M1);
    var (br, bg, bb) = _Rgb(window.P1P0);
    var (brr, brg, brb) = _Rgb(window.P1P1);

    // 8-connected Laplacian per channel.
    var lapR = tlr + tr + trr + lr + rr + blr + br + brr - 8f * cr;
    var lapG = tlg + tg + trg + lg + rg + blg + bg + brg - 8f * cg;
    var lapB = tlb + tb + trb + lb + rb + blb + bb + brb - 8f * cb;

    // Sharpening: subtract the Laplacian (since centre coefficient is negative, this
    // adds the magnitude back to the centre pixel — the standard form).
    var or = cr - amount * lapR;
    var og = cg - amount * lapG;
    var ob = cb - amount * lapB;

    if (or < 0f) or = 0f; else if (or > 1f) or = 1f;
    if (og < 0f) og = 0f; else if (og > 1f) og = 1f;
    if (ob < 0f) ob = 0f; else if (ob > 1f) ob = 1f;

    var center = window.P0P0.Work;
    var (_, _, _, ca) = ColorConverter.GetNormalizedRgba(in center);
    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(or, og, ob, ca));
  }
}
