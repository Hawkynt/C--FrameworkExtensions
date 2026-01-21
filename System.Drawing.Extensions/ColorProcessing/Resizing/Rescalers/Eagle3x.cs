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
/// Eagle 3x scaling algorithm - Hawkynt's extension of Eagle to 3x.
/// </summary>
/// <remarks>
/// <para>Scales images by 3x using corner and edge detection.</para>
/// <para>
/// For each pixel with its 8 neighbors:
/// - Corners are interpolated if diagonal neighbor matches both adjacent neighbors
/// - Edges are interpolated if both adjacent corners meet their conditions
/// - Center remains as the original pixel
/// </para>
/// </remarks>
[ScalerInfo("Eagle 3x", Author = "Hawkynt", Year = 2008,
  Description = "3x extension of the classic Eagle algorithm with edge interpolation", Category = ScalerCategory.PixelArt)]
public readonly struct Eagle3x : IPixelScaler {

  /// <inheritdoc />
  public ScaleFactor Scale => new(3, 3);

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
    => callback.Invoke(new Eagle3xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp));

  /// <summary>
  /// Gets the list of scale factors supported.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(3, 3)];

  /// <summary>
  /// Determines whether the specified scale factor is supported.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 3, Y: 3 };

  /// <summary>
  /// Enumerates all possible target dimensions.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 3, sourceHeight * 3);
  }

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Eagle3x Default => new();
}

file readonly struct Eagle3xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 3;
  public int ScaleY => 3;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    // 3x3 source neighborhood
    // NeighborWindow uses [Row][Column] naming: M1P0 = row -1, col 0 = TOP
    var c0 = window.M1M1; // top-left (row -1, col -1)
    var c1 = window.M1P0; // top (row -1, col 0)
    var c2 = window.M1P1; // top-right (row -1, col +1)
    var c3 = window.P0M1; // left (row 0, col -1)
    var c4 = window.P0P0; // center (row 0, col 0)
    var c5 = window.P0P1; // right (row 0, col +1)
    var c6 = window.P1M1; // bottom-left (row +1, col -1)
    var c7 = window.P1P0; // bottom (row +1, col 0)
    var c8 = window.P1P1; // bottom-right (row +1, col +1)

    var center = c4.Work;

    // Default all to center
    var e00 = center;
    var e01 = center;
    var e02 = center;
    var e10 = center;
    var e12 = center;
    var e20 = center;
    var e21 = center;
    var e22 = center;

    // Pre-compute keys
    var k0 = c0.Key;
    var k1 = c1.Key;
    var k2 = c2.Key;
    var k3 = c3.Key;
    var k5 = c5.Key;
    var k6 = c6.Key;
    var k7 = c7.Key;
    var k8 = c8.Key;

    // Corner conditions
    var corner00 = equality.Equals(k0, k1) && equality.Equals(k0, k3);
    var corner02 = equality.Equals(k2, k1) && equality.Equals(k2, k5);
    var corner20 = equality.Equals(k6, k3) && equality.Equals(k6, k7);
    var corner22 = equality.Equals(k8, k5) && equality.Equals(k8, k7);

    // Corners: interpolate if diagonal matches adjacent
    if (corner00)
      e00 = lerp.Lerp(lerp.Lerp(c0.Work, c1.Work), c3.Work, 2, 1);

    if (corner02)
      e02 = lerp.Lerp(lerp.Lerp(c2.Work, c1.Work), c5.Work, 2, 1);

    if (corner20)
      e20 = lerp.Lerp(lerp.Lerp(c6.Work, c3.Work), c7.Work, 2, 1);

    if (corner22)
      e22 = lerp.Lerp(lerp.Lerp(c8.Work, c5.Work), c7.Work, 2, 1);

    // Edges: interpolate if BOTH adjacent corners meet their conditions
    if (corner00 && corner02)
      e01 = lerp.Lerp(e00, e02);

    if (corner02 && corner22)
      e12 = lerp.Lerp(e02, e22);

    if (corner20 && corner22)
      e21 = lerp.Lerp(e20, e22);

    if (corner00 && corner20)
      e10 = lerp.Lerp(e00, e20);

    // Write 3x3 output
    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;
    var row2 = row1 + destStride;

    row0[0] = encoder.Encode(e00);
    row0[1] = encoder.Encode(e01);
    row0[2] = encoder.Encode(e02);
    row1[0] = encoder.Encode(e10);
    row1[1] = encoder.Encode(center); // center always unchanged
    row1[2] = encoder.Encode(e12);
    row2[0] = encoder.Encode(e20);
    row2[1] = encoder.Encode(e21);
    row2[2] = encoder.Encode(e22);
  }
}

/// <summary>
/// Eagle 3x B scaling algorithm - simplified corner-only variant.
/// </summary>
/// <remarks>
/// <para>Simpler version of Eagle 3x that only interpolates corners, not edges.</para>
/// <para>Can look blockier but is faster and sometimes preferable for certain art styles.</para>
/// </remarks>
[ScalerInfo("Eagle 3x B", Author = "Hawkynt", Year = 2008,
  Description = "Simplified Eagle 3x with corner-only interpolation", Category = ScalerCategory.PixelArt)]
public readonly struct Eagle3xB : IPixelScaler {

  /// <inheritdoc />
  public ScaleFactor Scale => new(3, 3);

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
    => callback.Invoke(new Eagle3xBKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp));

  /// <summary>
  /// Gets the list of scale factors supported.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(3, 3)];

  /// <summary>
  /// Determines whether the specified scale factor is supported.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 3, Y: 3 };

  /// <summary>
  /// Enumerates all possible target dimensions.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 3, sourceHeight * 3);
  }

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Eagle3xB Default => new();
}

file readonly struct Eagle3xBKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 3;
  public int ScaleY => 3;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    // 3x3 source neighborhood
    // NeighborWindow uses [Row][Column] naming: M1P0 = row -1, col 0 = TOP
    var c0 = window.M1M1; // top-left (row -1, col -1)
    var c1 = window.M1P0; // top (row -1, col 0)
    var c2 = window.M1P1; // top-right (row -1, col +1)
    var c3 = window.P0M1; // left (row 0, col -1)
    var c4 = window.P0P0; // center (row 0, col 0)
    var c5 = window.P0P1; // right (row 0, col +1)
    var c6 = window.P1M1; // bottom-left (row +1, col -1)
    var c7 = window.P1P0; // bottom (row +1, col 0)
    var c8 = window.P1P1; // bottom-right (row +1, col +1)

    var center = c4.Work;
    var encodedCenter = encoder.Encode(center);

    // Default corners to center
    var e00 = center;
    var e02 = center;
    var e20 = center;
    var e22 = center;

    // Pre-compute keys
    var k0 = c0.Key;
    var k1 = c1.Key;
    var k2 = c2.Key;
    var k3 = c3.Key;
    var k5 = c5.Key;
    var k6 = c6.Key;
    var k7 = c7.Key;
    var k8 = c8.Key;

    // Corners only - no edge interpolation
    if (equality.Equals(k0, k1) && equality.Equals(k0, k3))
      e00 = lerp.Lerp(lerp.Lerp(c0.Work, c1.Work), c3.Work, 2, 1);

    if (equality.Equals(k2, k1) && equality.Equals(k2, k5))
      e02 = lerp.Lerp(lerp.Lerp(c2.Work, c1.Work), c5.Work, 2, 1);

    if (equality.Equals(k6, k3) && equality.Equals(k6, k7))
      e20 = lerp.Lerp(lerp.Lerp(c6.Work, c3.Work), c7.Work, 2, 1);

    if (equality.Equals(k8, k5) && equality.Equals(k8, k7))
      e22 = lerp.Lerp(lerp.Lerp(c8.Work, c5.Work), c7.Work, 2, 1);

    // Write 3x3 output - edges and center are always center pixel
    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;
    var row2 = row1 + destStride;

    row0[0] = encoder.Encode(e00);
    row0[1] = encodedCenter;
    row0[2] = encoder.Encode(e02);
    row1[0] = encodedCenter;
    row1[1] = encodedCenter;
    row1[2] = encodedCenter;
    row2[0] = encoder.Encode(e20);
    row2[1] = encodedCenter;
    row2[2] = encoder.Encode(e22);
  }
}
