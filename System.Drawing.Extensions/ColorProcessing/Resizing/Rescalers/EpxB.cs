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
/// SNES9x EPX-B - Enhanced EPX with complex edge detection.
/// </summary>
/// <remarks>
/// <para>Scales images by 2x using more sophisticated edge detection than standard EPX.</para>
/// <para>
/// Uses a 3x3 source neighborhood with complex conditions to determine
/// when corner interpolation should occur. Includes checks against diagonal
/// neighbors to prevent artifacts.
/// </para>
/// <para>From SNES9x emulator, modified by Hawkynt for threshold support.</para>
/// </remarks>
[ScalerInfo("EPX-B", Author = "SNES9x Team", Year = 2003,
  Description = "Enhanced EPX with complex edge detection", Category = ScalerCategory.PixelArt)]
public readonly struct EpxB : IPixelScaler {

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
    => callback.Invoke(new EpxBKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp));

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
  public static EpxB Default => new();
}

file readonly struct EpxBKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
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

    // Default all outputs to center
    var e00 = center;
    var e01 = center;
    var e10 = center;
    var e11 = center;

    // Pre-compute keys
    var k0 = c0.Key;
    var k1 = c1.Key;
    var k2 = c2.Key;
    var k3 = c3.Key;
    var k4 = c4.Key;
    var k5 = c5.Key;
    var k6 = c6.Key;
    var k7 = c7.Key;
    var k8 = c8.Key;

    // EPX-B uses complex conditions to determine when to interpolate
    // Main condition: left != right AND top != bottom
    // Plus additional checks against center and diagonals
    if (!equality.Equals(k3, k5) && !equality.Equals(k1, k7) &&
        (equality.Equals(k4, k3) ||
         equality.Equals(k4, k7) ||
         equality.Equals(k4, k5) ||
         equality.Equals(k4, k1) ||
         ((!equality.Equals(k0, k8) || equality.Equals(k4, k6) || equality.Equals(k4, k2)) &&
          (!equality.Equals(k6, k2) || equality.Equals(k4, k0) || equality.Equals(k4, k8))))) {

      // Top-left corner
      if (equality.Equals(k1, k3) &&
          (!equality.Equals(k4, k0) || !equality.Equals(k4, k8) ||
           !equality.Equals(k1, k2) || !equality.Equals(k3, k6))) {
        e00 = lerp.Lerp(c1.Work, c3.Work);
      }

      // Top-right corner
      if (equality.Equals(k5, k1) &&
          (!equality.Equals(k4, k2) || !equality.Equals(k4, k6) ||
           !equality.Equals(k5, k8) || !equality.Equals(k1, k0))) {
        e01 = lerp.Lerp(c5.Work, c1.Work);
      }

      // Bottom-left corner
      if (equality.Equals(k3, k7) &&
          (!equality.Equals(k4, k6) || !equality.Equals(k4, k2) ||
           !equality.Equals(k3, k0) || !equality.Equals(k7, k8))) {
        e10 = lerp.Lerp(c3.Work, c7.Work);
      }

      // Bottom-right corner
      if (equality.Equals(k7, k5) &&
          (!equality.Equals(k4, k8) || !equality.Equals(k4, k0) ||
           !equality.Equals(k7, k6) || !equality.Equals(k5, k2))) {
        e11 = lerp.Lerp(c7.Work, c5.Work);
      }
    }

    // Write 2x2 output
    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;

    row0[0] = encoder.Encode(e00);
    row0[1] = encoder.Encode(e01);
    row1[0] = encoder.Encode(e10);
    row1[1] = encoder.Encode(e11);
  }
}
