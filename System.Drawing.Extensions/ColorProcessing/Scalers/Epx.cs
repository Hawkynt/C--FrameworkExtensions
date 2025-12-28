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

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Pipeline;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Scalers;

/// <summary>
/// Eric Johnston's EPX (Eric's Pixel Expansion) pixel-art scaling algorithm.
/// </summary>
/// <remarks>
/// <para>Scales images by 2x using corner detection based on cardinal neighbors.</para>
/// <para>
/// For each pixel P with neighbors A (top), B (right), C (left), D (bottom):
/// If C==A and C!=D and A!=B, top-left corner becomes A.
/// Similar logic for other corners.
/// </para>
/// <para>Developed by Eric Johnston at LucasArts in 1992 for porting SCUMM engine games to Macintosh.</para>
/// </remarks>
[ScalerInfo("EPX", Author = "Eric Johnston", Year = 1992, Url = "https://en.wikipedia.org/wiki/Pixel-art_scaling_algorithms#EPX",
  Description = "Eric's Pixel Expansion, developed at LucasArts for SCUMM engine games on Macintosh", Category = ScalerCategory.PixelArt)]
public readonly struct Epx : IPixelScaler {

  /// <inheritdoc />
  public ScaleFactor Scale => new(2, 2);

  /// <inheritdoc />
  public TResult InvokeKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode, TResult>(
    IKernelCallback<TWork, TKey, TPixel, TEncode, TResult> callback,
    TEquality equality = default,
    TLerp lerp = default)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDistance : struct, IColorMetric<TKey>
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new EpxKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp));

  /// <summary>
  /// Gets the list of scale factors supported by EPX.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2)];

  /// <summary>
  /// Determines whether EPX supports the specified scale factor.
  /// </summary>
  /// <param name="scale">The scale factor to check.</param>
  /// <returns><c>true</c> if the scale is 2x2; otherwise, <c>false</c>.</returns>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 };

  /// <summary>
  /// Enumerates all possible target dimensions for EPX.
  /// </summary>
  /// <param name="sourceWidth">The source image width.</param>
  /// <param name="sourceHeight">The source image height.</param>
  /// <returns>The target dimensions (2x in both dimensions).</returns>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
  }

  /// <summary>
  /// Gets the default EPX configuration.
  /// </summary>
  public static Epx Default => new();
}

/// <summary>
/// Internal kernel for EPX algorithm.
/// </summary>
/// <remarks>
/// EPX pattern:
///   A        (top)
/// C P B    (left, center, right)
///   D        (bottom)
///
/// Output 2x2 block:
/// E1 E2
/// E3 E4
///
/// Rules:
/// - If C==A and C!=D and A!=B, E1 = lerp(C, A)
/// - If A==B and A!=C and B!=D, E2 = lerp(A, B)
/// - If D==C and D!=B and C!=A, E3 = lerp(D, C)
/// - If B==D and B!=A and D!=C, E4 = lerp(B, D)
/// Otherwise, all output pixels equal P.
/// </remarks>
file readonly struct EpxKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  /// <inheritdoc />
  public int ScaleX => 2;

  /// <inheritdoc />
  public int ScaleY => 2;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    // EPX pattern uses cardinal neighbors only
    var a = window.P0M1; // top
    var b = window.P1P0; // right
    var c = window.M1P0; // left
    var d = window.P0P1; // bottom
    var p = window.P0P0; // center

    var pWork = p.Work;

    // Default all outputs to center pixel
    var e1 = pWork;
    var e2 = pWork;
    var e3 = pWork;
    var e4 = pWork;

    // Pre-compute equality tests
    var aKey = a.Key;
    var bKey = b.Key;
    var cKey = c.Key;
    var dKey = d.Key;

    var ca = equality.Equals(cKey, aKey);
    var ab = equality.Equals(aKey, bKey);
    var bd = equality.Equals(bKey, dKey);
    var dc = equality.Equals(dKey, cKey);

    // EPX corner rules - interpolate matched values (not just copy)
    // Even when equality is detected, we interpolate because equality
    // doesn't mean identity (tolerance-based matching)
    if (ca && !dc && !ab)
      e1 = lerp.Lerp(c.Work, a.Work);

    if (ab && !ca && !bd)
      e2 = lerp.Lerp(a.Work, b.Work);

    if (dc && !bd && !ca)
      e3 = lerp.Lerp(d.Work, c.Work);

    if (bd && !ab && !dc)
      e4 = lerp.Lerp(b.Work, d.Work);

    // Write directly to destination with encoding
    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;
    row0[0] = encoder.Encode(e1);
    row0[1] = encoder.Encode(e2);
    row1[0] = encoder.Encode(e3);
    row1[1] = encoder.Encode(e4);
  }
}
