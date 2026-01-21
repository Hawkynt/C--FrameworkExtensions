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
/// SNES9x EPX3 - Eric's Pixel Expansion adapted for 3x scaling.
/// </summary>
/// <remarks>
/// <para>Scales images by 3x using advanced edge detection.</para>
/// <para>
/// Uses a 3x3 source neighborhood to determine corner and edge interpolation.
/// Corners are interpolated when diagonal neighbors match adjacent neighbors.
/// Edges are interpolated based on multiple corner conditions.
/// </para>
/// <para>From SNES9x emulator, modified by Hawkynt for threshold support.</para>
/// </remarks>
[ScalerInfo("EPX 3x", Author = "SNES9x Team", Year = 2003,
  Description = "Eric's Pixel Expansion extended to 3x scaling", Category = ScalerCategory.PixelArt)]
public readonly struct Epx3 : IPixelScaler {

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
    => callback.Invoke(new Epx3Kernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp));

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
  public static Epx3 Default => new();
}

file readonly struct Epx3Kernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
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

    // Default all outputs to center
    var e00 = center;
    var e01 = center;
    var e02 = center;
    var e10 = center;
    var e12 = center;
    var e20 = center;
    var e21 = center;
    var e22 = center;

    // EPX3 logic: only process if cardinal neighbors differ
    if (!equality.Equals(k3, k5) && !equality.Equals(k7, k1)) {
      // Check if center differs from each neighbor
      var neq40 = !equality.Equals(k4, k0);
      var neq41 = !equality.Equals(k4, k1);
      var neq42 = !equality.Equals(k4, k2);
      var neq43 = !equality.Equals(k4, k3);
      var neq45 = !equality.Equals(k4, k5);
      var neq46 = !equality.Equals(k4, k6);
      var neq47 = !equality.Equals(k4, k7);
      var neq48 = !equality.Equals(k4, k8);

      // Corner edge conditions
      var eq13 = equality.Equals(k1, k3) && (neq40 || neq48 || !equality.Equals(k1, k2) || !equality.Equals(k3, k6));
      var eq37 = equality.Equals(k3, k7) && (neq46 || neq42 || !equality.Equals(k3, k0) || !equality.Equals(k7, k8));
      var eq75 = equality.Equals(k7, k5) && (neq48 || neq40 || !equality.Equals(k7, k6) || !equality.Equals(k5, k2));
      var eq51 = equality.Equals(k5, k1) && (neq42 || neq46 || !equality.Equals(k5, k8) || !equality.Equals(k1, k0));

      // Check if any neighbor differs from center
      var anyDiff = !neq40 || !neq41 || !neq42 || !neq43 || !neq45 || !neq46 || !neq47 || !neq48;

      if (anyDiff) {
        // Corners
        if (eq13)
          e00 = lerp.Lerp(c1.Work, c3.Work);
        if (eq51)
          e02 = lerp.Lerp(c5.Work, c1.Work);
        if (eq37)
          e20 = lerp.Lerp(c3.Work, c7.Work);
        if (eq75)
          e22 = lerp.Lerp(c7.Work, c5.Work);

        // Edges
        if (eq51 && neq40 && eq13 && neq42)
          e01 = lerp.Lerp(lerp.Lerp(c1.Work, c3.Work), c5.Work, 2, 1);
        else if (eq51 && neq40)
          e01 = lerp.Lerp(c1.Work, c5.Work);
        else if (eq13 && neq42)
          e01 = lerp.Lerp(c1.Work, c3.Work);

        if (eq13 && neq46 && eq37 && neq40)
          e10 = lerp.Lerp(lerp.Lerp(c3.Work, c1.Work), c7.Work, 2, 1);
        else if (eq13 && neq46)
          e10 = lerp.Lerp(c3.Work, c1.Work);
        else if (eq37 && neq40)
          e10 = lerp.Lerp(c3.Work, c7.Work);

        if (eq75 && neq42 && eq51 && neq48)
          e12 = lerp.Lerp(lerp.Lerp(c5.Work, c1.Work), c7.Work, 2, 1);
        else if (eq75 && neq42)
          e12 = lerp.Lerp(c5.Work, c7.Work);
        else if (eq51 && neq48)
          e12 = lerp.Lerp(c5.Work, c1.Work);

        if (eq37 && neq48 && eq75 && neq46)
          e21 = lerp.Lerp(lerp.Lerp(c7.Work, c3.Work), c5.Work, 2, 1);
        else if (eq75 && neq46)
          e21 = lerp.Lerp(c7.Work, c5.Work);
        else if (eq37 && neq48)
          e21 = lerp.Lerp(c7.Work, c3.Work);
      } else {
        // Simplified corner-only processing
        if (eq13)
          e00 = lerp.Lerp(c1.Work, c3.Work);
        if (eq51)
          e02 = lerp.Lerp(c5.Work, c1.Work);
        if (eq37)
          e20 = lerp.Lerp(c3.Work, c7.Work);
        if (eq75)
          e22 = lerp.Lerp(c7.Work, c5.Work);
      }
    }

    // Write 3x3 output
    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;
    var row2 = row1 + destStride;

    row0[0] = encoder.Encode(e00);
    row0[1] = encoder.Encode(e01);
    row0[2] = encoder.Encode(e02);
    row1[0] = encoder.Encode(e10);
    row1[1] = encodedCenter;
    row1[2] = encoder.Encode(e12);
    row2[0] = encoder.Encode(e20);
    row2[1] = encoder.Encode(e21);
    row2[2] = encoder.Encode(e22);
  }
}
