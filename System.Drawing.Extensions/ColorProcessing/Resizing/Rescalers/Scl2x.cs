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
/// FNES 2xSCL - Simple Corner-based Line scaler.
/// </summary>
/// <remarks>
/// <para>Scales images by 2x using corner detection based on cardinal neighbors.</para>
/// <para>
/// For each pixel, corners are filled with adjacent neighbor colors when edge conditions are met.
/// Similar to DES but outputs different corners based on edge relationships.
/// </para>
/// <para>From FNES emulator.</para>
/// </remarks>
[ScalerInfo("2xSCL", Author = "FNES Team", Year = 2000,
  Description = "Simple corner-based line scaling", Category = ScalerCategory.PixelArt)]
public readonly struct Scl2x : IPixelScaler {

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
    => callback.Invoke(new Scl2xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp));

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
  public static Scl2x Default => new();
}

file readonly struct Scl2xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
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

    // Compute corners based on edge detection
    var p0 = equality.Equals(kw, kn) && !equality.Equals(kn, ke) && !equality.Equals(kw, ks) ? w.Work : center;
    var p1 = equality.Equals(kn, ke) && !equality.Equals(kn, kw) && !equality.Equals(ke, ks) ? e.Work : center;
    var p2 = equality.Equals(kw, ks) && !equality.Equals(kw, kn) && !equality.Equals(ks, ke) ? w.Work : center;
    var p3 = equality.Equals(ks, ke) && !equality.Equals(kw, ks) && !equality.Equals(kn, ke) ? e.Work : center;

    // Write 2x2 output
    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;

    row0[0] = encoder.Encode(p0);
    row0[1] = encoder.Encode(p1);
    row1[0] = encoder.Encode(p2);
    row1[1] = encoder.Encode(p3);
  }
}

/// <summary>
/// FNES Super 2xSCL - Enhanced corner scaler with color mixing.
/// </summary>
/// <remarks>
/// <para>Enhanced version of 2xSCL that blends colors at edges.</para>
/// <para>
/// Non-edge corners are blended with center (3:1 ratio) rather than using center directly.
/// Edge corners use the detected edge neighbor directly.
/// </para>
/// <para>From FNES emulator.</para>
/// </remarks>
[ScalerInfo("Super 2xSCL", Author = "FNES Team", Year = 2000,
  Description = "Enhanced 2xSCL with color mixing", Category = ScalerCategory.PixelArt)]
public readonly struct Scl2xSuper : IPixelScaler {

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
    => callback.Invoke(new Scl2xSuperKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp));

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
  public static Scl2xSuper Default => new();
}

file readonly struct Scl2xSuperKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
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
    // Cardinal neighbors + center
    // NeighborWindow uses [Row][Column] naming: M1P0 = row -1, col 0 = TOP
    var n = window.M1P0; // north/top (row -1, col 0)
    var w = window.P0M1; // west/left (row 0, col -1)
    var c = window.P0P0; // center (row 0, col 0)
    var e = window.P0P1; // east/right (row 0, col +1)
    var s = window.P1P0; // south/bottom (row +1, col 0)

    var center = c.Work;
    var wWork = w.Work;
    var eWork = e.Work;

    // Pre-compute keys
    var kn = n.Key;
    var kw = w.Key;
    var ke = e.Key;
    var ks = s.Key;

    // Blended defaults (3:1 center:neighbor)
    var cw = lerp.Lerp(center, wWork, 0.25f);
    var ce = lerp.Lerp(center, eWork, 0.25f);

    // Compute corners: edge detection uses neighbor directly, non-edge uses blend
    var p0 = equality.Equals(kw, kn) && !equality.Equals(kn, ke) && !equality.Equals(kw, ks) ? wWork : cw;
    var p1 = equality.Equals(kn, ke) && !equality.Equals(kn, kw) && !equality.Equals(ke, ks) ? eWork : ce;
    var p2 = equality.Equals(kw, ks) && !equality.Equals(kw, kn) && !equality.Equals(ks, ke) ? wWork : cw;
    var p3 = equality.Equals(ks, ke) && !equality.Equals(kw, ks) && !equality.Equals(kn, ke) ? eWork : ce;

    // Write 2x2 output
    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;

    row0[0] = encoder.Encode(p0);
    row0[1] = encoder.Encode(p1);
    row1[0] = encoder.Encode(p2);
    row1[1] = encoder.Encode(p3);
  }
}

/// <summary>
/// FNES Ultra 2xSCL - Enhanced corner scaler with unsharp mask.
/// </summary>
/// <remarks>
/// <para>Most advanced version of 2xSCL with color unmixing for sharpness.</para>
/// <para>
/// Applies an unsharp mask variant after Super 2xSCL processing to enhance edges.
/// </para>
/// <para>From FNES emulator.</para>
/// </remarks>
[ScalerInfo("Ultra 2xSCL", Author = "FNES Team", Year = 2000,
  Description = "Enhanced 2xSCL with unsharp mask", Category = ScalerCategory.PixelArt)]
public readonly struct Scl2xUltra : IPixelScaler {

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
    => callback.Invoke(new Scl2xUltraKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp));

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
  public static Scl2xUltra Default => new();
}

file readonly struct Scl2xUltraKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
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
    // Cardinal neighbors + center
    // NeighborWindow uses [Row][Column] naming: M1P0 = row -1, col 0 = TOP
    var n = window.M1P0; // north/top (row -1, col 0)
    var w = window.P0M1; // west/left (row 0, col -1)
    var c = window.P0P0; // center (row 0, col 0)
    var e = window.P0P1; // east/right (row 0, col +1)
    var s = window.P1P0; // south/bottom (row +1, col 0)

    var center = c.Work;
    var wWork = w.Work;
    var eWork = e.Work;

    // Pre-compute keys
    var kn = n.Key;
    var kw = w.Key;
    var ke = e.Key;
    var ks = s.Key;

    // Blended defaults (3:1 center:neighbor)
    var cw = lerp.Lerp(center, wWork, 0.25f);
    var ce = lerp.Lerp(center, eWork, 0.25f);

    // Compute corners like Super
    var p0 = equality.Equals(kw, kn) && !equality.Equals(kn, ke) && !equality.Equals(kw, ks) ? wWork : cw;
    var p1 = equality.Equals(kn, ke) && !equality.Equals(kn, kw) && !equality.Equals(ke, ks) ? eWork : ce;
    var p2 = equality.Equals(kw, ks) && !equality.Equals(kw, kn) && !equality.Equals(ks, ke) ? wWork : cw;
    var p3 = equality.Equals(ks, ke) && !equality.Equals(kw, ks) && !equality.Equals(kn, ke) ? eWork : ce;

    // Apply unmix (unsharp mask variant): enhances difference from center
    // unmix(c1, c2) = lerp((c1 + (c1 - c2)) clamped, c2, 0.5)
    // This boosts the difference between the corner and center
    p0 = Unmix(p0, center, lerp);
    p1 = Unmix(p1, center, lerp);
    p2 = Unmix(p2, center, lerp);
    p3 = Unmix(p3, center, lerp);

    // Write 2x2 output
    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;

    row0[0] = encoder.Encode(p0);
    row0[1] = encoder.Encode(p1);
    row1[0] = encoder.Encode(p2);
    row1[1] = encoder.Encode(p3);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork Unmix(TWork c1, TWork c2, TLerp lerp) {
    // Unsharp mask variant: boost difference from center, then average
    // Enhanced = c1 + (c1 - c2) = 2*c1 - c2
    // Result = (Enhanced + c2) / 2 = c1 (with some sharpening effect)
    // Using lerp: enhanced = lerp(c2, c1, 2.0) [extrapolate], then average
    // Since we can't easily extrapolate, we use: lerp(c1, enhanced, 0.5)
    // A simpler approximation: lerp back toward c1 after boosting
    var enhanced = lerp.Lerp(c2, c1, 2f); // This extrapolates: 2*c1 - c2
    return lerp.Lerp(enhanced, c2);        // Average: (enhanced + c2) / 2
  }
}
