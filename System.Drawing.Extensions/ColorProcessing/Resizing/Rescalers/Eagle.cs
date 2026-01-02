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
using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Rescalers;

/// <summary>
/// Eagle pixel-art scaling algorithm.
/// </summary>
/// <remarks>
/// <para>Scales images by 2x using simple corner detection based on diagonal neighbors.</para>
/// <para>
/// For each pixel P with 8-connected neighbors S, T, U, V, X, W, Y, Z:
/// <list type="bullet">
/// <item>E1 = if T==S and T==V then interpolate(T, S, V) else P (top-left corner)</item>
/// <item>E2 = if U==T and U==X then interpolate(U, T, X) else P (top-right corner)</item>
/// <item>E3 = if W==V and W==Y then interpolate(W, V, Y) else P (bottom-left corner)</item>
/// <item>E4 = if Y==X and Y==Z then interpolate(Y, X, Z) else P (bottom-right corner)</item>
/// </list>
/// </para>
/// <para>One of the earliest pixel-art scalers, developed in 1997.</para>
/// </remarks>
[ScalerInfo("Eagle", Year = 1997,
  Description = "Early pixel-art scaler using simple corner detection", Category = ScalerCategory.PixelArt)]
public readonly struct Eagle : IPixelScaler {

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
    where TDistance : struct, IColorMetric<TKey>, INormalizedMetric
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new EagleKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp));

  /// <summary>
  /// Gets the list of scale factors supported by Eagle.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2)];

  /// <summary>
  /// Determines whether Eagle supports the specified scale factor.
  /// </summary>
  /// <param name="scale">The scale factor to check.</param>
  /// <returns><c>true</c> if the scale is 2x2; otherwise, <c>false</c>.</returns>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 };

  /// <summary>
  /// Enumerates all possible target dimensions for Eagle.
  /// </summary>
  /// <param name="sourceWidth">The source image width.</param>
  /// <param name="sourceHeight">The source image height.</param>
  /// <returns>The target dimensions (2x in both dimensions).</returns>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
  }

  /// <summary>
  /// Gets the default Eagle configuration.
  /// </summary>
  public static Eagle Default => new();
}

/// <summary>
/// Internal kernel for Eagle algorithm.
/// </summary>
/// <remarks>
/// Eagle pattern (uses 3x3 neighborhood):
///
/// S T U      (top-left, top, top-right)
/// V P X      (left, center, right)
/// W Y Z      (bottom-left, bottom, bottom-right)
///
/// Output 2x2 block:
/// E1 E2
/// E3 E4
///
/// Rules:
/// - E1 = (T==S and T==V) ? interpolate(T, S, V) : P
/// - E2 = (U==T and U==X) ? interpolate(U, T, X) : P
/// - E3 = (W==V and W==Y) ? interpolate(W, V, Y) : P
/// - E4 = (Y==X and Y==Z) ? interpolate(Y, X, Z) : P
///
/// Eagle was one of the earliest pixel-art scalers, using simple corner detection.
/// </remarks>
file readonly struct EagleKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
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
    // Get the 3x3 source neighborhood
    // NeighborWindow uses [Row][Column] naming: M1P0 = row -1, col 0 = TOP
    var s = window.M1M1; // top-left (row -1, col -1)
    var t = window.M1P0; // top (row -1, col 0)
    var u = window.M1P1; // top-right (row -1, col +1)
    var v = window.P0M1; // left (row 0, col -1)
    var p = window.P0P0; // center (row 0, col 0)
    var x = window.P0P1; // right (row 0, col +1)
    var w = window.P1M1; // bottom-left (row +1, col -1)
    var y = window.P1P0; // bottom (row +1, col 0)
    var z = window.P1P1; // bottom-right (row +1, col +1)

    var pWork = p.Work;

    // Default all outputs to center pixel
    var e1 = pWork;
    var e2 = pWork;
    var e3 = pWork;
    var e4 = pWork;

    // Pre-compute keys for equality tests
    var sKey = s.Key;
    var tKey = t.Key;
    var uKey = u.Key;
    var vKey = v.Key;
    var xKey = x.Key;
    var wKey = w.Key;
    var yKey = y.Key;
    var zKey = z.Key;

    // Eagle corner rules - interpolate matched neighbors
    // E1: top-left corner - if T==S and T==V, blend all three
    if (equality.Equals(tKey, sKey) && equality.Equals(tKey, vKey))
      e1 = lerp.Lerp(t.Work, s.Work, v.Work);

    // E2: top-right corner - if U==T and U==X, blend all three
    if (equality.Equals(uKey, tKey) && equality.Equals(uKey, xKey))
      e2 = lerp.Lerp(u.Work, t.Work, x.Work);

    // E3: bottom-left corner - if W==V and W==Y, blend all three
    if (equality.Equals(wKey, vKey) && equality.Equals(wKey, yKey))
      e3 = lerp.Lerp(w.Work, v.Work, y.Work);

    // E4: bottom-right corner - if Y==X and Y==Z, blend all three
    if (equality.Equals(yKey, xKey) && equality.Equals(yKey, zKey))
      e4 = lerp.Lerp(y.Work, x.Work, z.Work);

    // Write directly to destination with encoding
    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;
    row0[0] = encoder.Encode(e1);
    row0[1] = encoder.Encode(e2);
    row1[0] = encoder.Encode(e3);
    row1[1] = encoder.Encode(e4);
  }
}
