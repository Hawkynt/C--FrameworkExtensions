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
/// FNES DES (Diagonal Edge Scaling) - 1x1 edge-aware filter.
/// </summary>
/// <remarks>
/// <para>Filters each pixel using diagonal edge detection.</para>
/// <para>
/// Uses cardinal neighbors to compute 4 potential corner values,
/// then averages them to produce a single output pixel.
/// This is a pre-processing filter rather than a scaler.
/// </para>
/// <para>From FNES emulator.</para>
/// </remarks>
[ScalerInfo("DES", Author = "FNES Team", Year = 2000,
  Description = "Diagonal Edge Scaling filter (1x1)", Category = ScalerCategory.PixelArt)]
public readonly struct Des : IPixelScaler {

  /// <inheritdoc />
  public ScaleFactor Scale => new(1, 1);

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
    => callback.Invoke(new DesKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp));

  /// <summary>
  /// Gets the list of scale factors supported.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(1, 1)];

  /// <summary>
  /// Determines whether the specified scale factor is supported.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 1, Y: 1 };

  /// <summary>
  /// Enumerates all possible target dimensions.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth, sourceHeight);
  }

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Des Default => new();
}

file readonly struct DesKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 1;
  public int ScaleY => 1;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    // Cardinal neighbors + center
    // NeighborWindow uses [Row][Column] naming: M1P0 = row -1, col 0 = TOP
    var n = window.M1P0; // north/top (row -1, col 0)
    var w = window.P0M1; // west/left (row 0, col -1)
    var c = window.P0P0; // center (row 0, col 0)
    var e = window.P0P1; // east/right (row 0, col +1)
    var s = window.P1P0; // south/bottom (row +1, col 0)

    var center = c.Work;

    // Pre-compute keys
    var kn = n.Key;
    var kw = w.Key;
    var ke = e.Key;
    var ks = s.Key;

    // Compute 4 corner potentials based on edge detection
    // p0 = top-left corner: use west if west==north AND north!=east AND west!=south
    var p0 = equality.Equals(kw, kn) && !equality.Equals(kn, ke) && !equality.Equals(kw, ks) ? w.Work : center;
    // p1 = top-right corner: use east if north==east AND north!=west AND east!=south
    var p1 = equality.Equals(kn, ke) && !equality.Equals(kn, kw) && !equality.Equals(ke, ks) ? e.Work : center;
    // p2 = bottom-left corner: use west if west==south AND west!=north AND south!=east
    var p2 = equality.Equals(kw, ks) && !equality.Equals(kw, kn) && !equality.Equals(ks, ke) ? w.Work : center;
    // p3 = bottom-right corner: use east if south==east AND west!=south AND north!=east
    var p3 = equality.Equals(ks, ke) && !equality.Equals(kw, ks) && !equality.Equals(kn, ke) ? e.Work : center;

    // Average all 4 corners
    var result = lerp.Lerp(lerp.Lerp(p0, p1), lerp.Lerp(p2, p3));

    destTopLeft[0] = encoder.Encode(result);
  }
}

/// <summary>
/// FNES DES2 (Diagonal Edge Scaling 2) - 2x scaler with weighted averaging.
/// </summary>
/// <remarks>
/// <para>Scales images by 2x using diagonal edge detection with weighted blending.</para>
/// <para>
/// Each output corner is computed based on edge detection, then blended
/// with the center and a directional neighbor using 3:1 weights.
/// </para>
/// <para>From FNES emulator.</para>
/// </remarks>
[ScalerInfo("DES 2x", Author = "FNES Team", Year = 2000,
  Description = "Diagonal Edge Scaling with weighted blending", Category = ScalerCategory.PixelArt)]
public readonly struct Des2 : IPixelScaler {

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
    => callback.Invoke(new Des2Kernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp));

  /// <summary>
  /// Gets the list of scale factors supported.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2)];

  /// <summary>
  /// Determines whether the specified scale factor is supported.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 };

  /// <summary>
  /// Enumerates all possible target dimensions.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
  }

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Des2 Default => new();
}

file readonly struct Des2Kernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 2;
  public int ScaleY => 2;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    // Neighbors
    // NeighborWindow uses [Row][Column] naming: M1P0 = row -1, col 0 = TOP
    var n = window.M1P0; // north/top (row -1, col 0)
    var w = window.P0M1; // west/left (row 0, col -1)
    var c = window.P0P0; // center (row 0, col 0)
    var e = window.P0P1; // east/right (row 0, col +1)
    var s = window.P1P0; // south/bottom (row +1, col 0)
    var se = window.P1P1; // southeast/bottom-right (row +1, col +1)

    var center = c.Work;

    // Pre-compute keys
    var kn = n.Key;
    var kw = w.Key;
    var ke = e.Key;
    var ks = s.Key;

    // Compute 4 corner potentials based on edge detection
    var p0 = equality.Equals(kw, kn) && !equality.Equals(kn, ke) && !equality.Equals(kw, ks) ? w.Work : center;
    var p1 = equality.Equals(kn, ke) && !equality.Equals(kn, kw) && !equality.Equals(ke, ks) ? e.Work : center;
    var p2 = equality.Equals(kw, ks) && !equality.Equals(kw, kn) && !equality.Equals(ks, ke) ? w.Work : center;
    var p3 = equality.Equals(ks, ke) && !equality.Equals(kw, ks) && !equality.Equals(kn, ke) ? e.Work : center;

    // Directional blends with center (3:1 ratio)
    var cx = center;
    var ce = lerp.Lerp(center, e.Work, 3, 1);  // 3:1 = 75% center, 25% east
    var cs = lerp.Lerp(center, s.Work, 3, 1);  // 3:1 = 75% center, 25% south
    var cse = lerp.Lerp(center, se.Work, 3, 1); // 3:1 = 75% center, 25% southeast

    // Final output: blend corner potential with directional blend (3:1 ratio)
    // Reference: sPixel.Interpolate(p0, cx, 3, 1) = 75% p0, 25% cx
    var d0 = lerp.Lerp(cx, p0, 1, 3);   // 3:1 = 25% cx, 75% p0
    var d1 = lerp.Lerp(ce, p1, 1, 3);   // 3:1 = 25% ce, 75% p1
    var d2 = lerp.Lerp(cs, p2, 1, 3);   // 3:1 = 25% cs, 75% p2
    var d3 = lerp.Lerp(cse, p3, 1, 3);  // 3:1 = 25% cse, 75% p3

    // Write 2x2 output
    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;

    row0[0] = encoder.Encode(d0);
    row0[1] = encoder.Encode(d1);
    row1[0] = encoder.Encode(d2);
    row1[1] = encoder.Encode(d3);
  }
}
