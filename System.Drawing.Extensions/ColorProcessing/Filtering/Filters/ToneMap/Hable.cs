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

namespace Hawkynt.ColorProcessing.Filtering.Filters.ToneMap;

/// <summary>
/// Hable / Uncharted 2 filmic tone-mapping operator (John Hable, 2010).
/// Per-channel filmic curve with toe + linear + shoulder regions, normalised so
/// the configured <c>linearWhite</c> value maps to 1.0 in display.
/// </summary>
/// <remarks>
/// <para>
/// Curve: <c>U(x) = ((x(Ax+CB)+DE)/(x(Ax+B)+DF)) − E/F</c> with constants
/// <c>A=0.15, B=0.50, C=0.10, D=0.20, E=0.02, F=0.30</c>. Output normalised by
/// <c>U(linearWhite)</c>. Default <c>linearWhite=11.2</c> matches the
/// canonical Uncharted 2 settings.
/// </para>
/// <para>
/// Use case: classic in-engine HDR → LDR before sRGB encoding. Higher
/// contrast and slightly more "punch" than Reinhard; widely used 2010-2018
/// before ACES became dominant.
/// </para>
/// <para>Parameter ranges: <paramref name="exposure"/> 0.1–10 (default 2),
/// <paramref name="linearWhite"/> 1–20 (default 11.2).</para>
/// </remarks>
[FilterInfo("Hable",
  Author = "John Hable", Year = 2010,
  Url = "http://filmicworlds.com/blog/filmic-tonemapping-operators/",
  Description = "Uncharted 2 filmic tone-mapping operator",
  Category = FilterCategory.ColorCorrection)]
public readonly struct Hable : IPixelFilter {
  private readonly float _exposure;
  private readonly float _linearWhite;

  public Hable() : this(2f, 11.2f) { }

  public Hable(float exposure = 2f, float linearWhite = 11.2f) {
    this._exposure = Math.Max(0.1f, Math.Min(10f, exposure));
    this._linearWhite = Math.Max(1f, Math.Min(20f, linearWhite));
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
    => callback.Invoke(new HableKernel<TWork, TKey, TPixel, TEncode>(this._exposure, this._linearWhite));

  public static Hable Default => new();
}

file readonly struct HableKernel<TWork, TKey, TPixel, TEncode>(float exposure, float linearWhite)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 1;
  public int ScaleY => 1;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _U(float x) {
    const float A = 0.15f, B = 0.50f, C = 0.10f, D = 0.20f, E = 0.02f, F = 0.30f;
    return ((x * (A * x + C * B) + D * E) / (x * (A * x + B) + D * F)) - E / F;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var pixel = window.P0P0.Work;
    var (r, g, b, a) = ColorConverter.GetNormalizedRgba(in pixel);

    var whiteScale = 1f / _U(linearWhite);
    var or = _U(r * exposure) * whiteScale;
    var og = _U(g * exposure) * whiteScale;
    var ob = _U(b * exposure) * whiteScale;

    or = or < 0f ? 0f : (or > 1f ? 1f : or);
    og = og < 0f ? 0f : (og > 1f ? 1f : og);
    ob = ob < 0f ? 0f : (ob > 1f ? 1f : ob);

    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(or, og, ob, a));
  }
}
