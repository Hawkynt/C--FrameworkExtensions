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
/// VBA's Bilinear Plus Original algorithm - weighted bilinear interpolation.
/// </summary>
/// <remarks>
/// <para>Scales images by 2x using weighted bilinear interpolation.</para>
/// <para>
/// For each pixel with its right and bottom neighbors:
/// - Top-left: weighted blend (5:2:1) of center, bottom, right
/// - Bottom-left: average of center and bottom
/// - Top-right: average of center and right
/// - Bottom-right: average of all four pixels
/// </para>
/// <para>From Visual Boy Advance emulator.</para>
/// </remarks>
[ScalerInfo("Bilinear Plus Original", Author = "VBA Team", Year = 2004,
  Description = "VBA weighted bilinear interpolation (5:2:1)", Category = ScalerCategory.PixelArt)]
public readonly struct BilinearPlusOriginal : IPixelScaler {

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
    => callback.Invoke(new BilinearPlusOriginalKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp));

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
  public static BilinearPlusOriginal Default => new();
}

file readonly struct BilinearPlusOriginalKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
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
    // Source pixels
    var c00 = window.P0P0.Work; // center
    var c10 = window.P1P0.Work; // below
    var c01 = window.P0P1.Work; // right
    var c11 = window.P1P1.Work; // below-right

    // Weighted interpolation (5:2:1 for top-left)
    // e00 = (5*c00 + 2*c10 + 1*c01) / 8
    // = c00 * 5/8 + c10 * 2/8 + c01 * 1/8
    // c10 weight = 2, c01 weight = 1, so c10/(c10+c01) = 2/3
    var neighbor = lerp.Lerp(c01, c10, 2, 1); // weighted average of neighbors (2:1 c10:c01)
    var e00 = lerp.Lerp(c00, neighbor, 5, 3); // 5/8 c00 + 3/8 neighbor

    // e01 = (c00 + c10) / 2
    var e01 = lerp.Lerp(c00, c10);

    // e10 = (c00 + c01) / 2
    var e10 = lerp.Lerp(c00, c01);

    // e11 = (c00 + c01 + c10 + c11) / 4
    var e11 = lerp.Lerp(lerp.Lerp(c00, c01), lerp.Lerp(c10, c11));

    // Write output
    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;
    row0[0] = encoder.Encode(e00);
    row0[1] = encoder.Encode(e10);
    row1[0] = encoder.Encode(e01);
    row1[1] = encoder.Encode(e11);
  }
}

/// <summary>
/// VBA's Bilinear Plus algorithm with gamma correction.
/// </summary>
/// <remarks>
/// <para>Enhanced version of Bilinear Plus Original with darker top-left corner.</para>
/// <para>Uses 14/16 gamma factor on the top-left pixel for CRT-like appearance.</para>
/// </remarks>
[ScalerInfo("Bilinear Plus", Author = "VBA Team", Year = 2004,
  Description = "VBA bilinear with gamma correction (14/16)", Category = ScalerCategory.PixelArt)]
public readonly struct BilinearPlus : IPixelScaler {

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
    => callback.Invoke(new BilinearPlusKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp));

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
  public static BilinearPlus Default => new();
}

file readonly struct BilinearPlusKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  private const int GammaW1 = 2;  // 14/16 = 7/8, so w1=1, w2=7 (or 2:14)
  private const int GammaW2 = 14;

  public int ScaleX => 2;
  public int ScaleY => 2;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    // Source pixels
    var c00 = window.P0P0.Work; // center
    var c10 = window.P1P0.Work; // below
    var c01 = window.P0P1.Work; // right
    var c11 = window.P1P1.Work; // below-right

    // e00 = ((10*c00 + 2*c10 + 2*c01) / 14) * gamma
    // First compute weighted average: 10:2:2 = 5:1:1 relative weights
    // neighbor = (c10 + c01) / 2, then blend 5:1 = c00 * 5/7 + neighbor * 2/7
    var neighbor = lerp.Lerp(c10, c01); // average of c10 and c01
    var weighted = lerp.Lerp(c00, neighbor, 5, 2); // 5/7 c00 + 2/7 neighbor
    var e00 = lerp.Lerp(default(TWork), weighted, GammaW1, GammaW2); // apply gamma darkening

    // e01 = (c00 + c10) / 2
    var e01 = lerp.Lerp(c00, c10);

    // e10 = (c00 + c01) / 2
    var e10 = lerp.Lerp(c00, c01);

    // e11 = (c00 + c01 + c10 + c11) / 4
    var e11 = lerp.Lerp(lerp.Lerp(c00, c01), lerp.Lerp(c10, c11));

    // Write output
    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;
    row0[0] = encoder.Encode(e00);
    row0[1] = encoder.Encode(e10);
    row1[0] = encoder.Encode(e01);
    row1[1] = encoder.Encode(e11);
  }
}
