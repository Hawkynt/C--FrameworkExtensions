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
using Guard;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Filtering;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Resizing;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.LookupTable;

/// <summary>
/// <see cref="IPixelFilter"/> wrapper that applies a <see cref="Lut3D"/> to every pixel.
/// </summary>
/// <remarks>
/// <para>
/// Use case: apply a color-grading "look" loaded from a <c>.cube</c> file via
/// <see cref="Lut3DReader"/>, or apply procedural LUT transforms.
/// </para>
/// <para>
/// The LUT operates in the working color space's normalized RGB. Alpha is preserved.
/// </para>
/// </remarks>
[Hawkynt.ColorProcessing.Filtering.FilterInfo("Lut3D",
  Description = "Applies a 3D color look-up table (.cube / .3dl) per pixel",
  Category = Hawkynt.ColorProcessing.Filtering.FilterCategory.ColorCorrection)]
public readonly struct Lut3DFilter : IPixelFilter {
  private readonly Lut3D? _lut;
  private readonly Lut3DInterpolation _interp;

  /// <summary>Default constructor (identity LUT — no-op).</summary>
  public Lut3DFilter() : this(Lut3D.Identity(), Lut3DInterpolation.Tetrahedral) { }

  /// <summary>Creates a filter that applies the given LUT.</summary>
  /// <param name="lut">The look-up table.</param>
  /// <param name="interpolation">The interpolation kernel (tetrahedral by default).</param>
  public Lut3DFilter(Lut3D lut, Lut3DInterpolation interpolation = Lut3DInterpolation.Tetrahedral) {
    Against.ArgumentIsNull(lut);
    this._lut = lut;
    this._interp = interpolation;
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
    => callback.Invoke(new Lut3DKernel<TWork, TKey, TPixel, TEncode>(this._lut ?? Lut3D.Identity(), this._interp));

  /// <summary>Identity LUT default — useful as a no-op fallback.</summary>
  public static Lut3DFilter Default => new();
}

file readonly struct Lut3DKernel<TWork, TKey, TPixel, TEncode>(Lut3D lut, Lut3DInterpolation interp)
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
    var (r, g, b, a) = ColorConverter.GetNormalizedRgba(in pixel);

    var (or, og, ob) = lut.Sample(r, g, b, interp);

    or = or < 0f ? 0f : (or > 1f ? 1f : or);
    og = og < 0f ? 0f : (og > 1f ? 1f : og);
    ob = ob < 0f ? 0f : (ob > 1f ? 1f : ob);

    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(or, og, ob, a));
  }
}
