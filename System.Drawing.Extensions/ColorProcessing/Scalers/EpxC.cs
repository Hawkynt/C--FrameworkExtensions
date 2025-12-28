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
using Hawkynt.ColorProcessing.Storage;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Scalers;

/// <summary>
/// SNES9x EPX-C pixel-art scaling algorithm.
/// </summary>
/// <remarks>
/// <para>Scales images by 2x using enhanced corner detection with weighted blending.</para>
/// <para>
/// EPX-C is an enhanced version of EPX that uses more sophisticated edge detection
/// and weighted blending to produce smoother results on complex pixel patterns.
/// </para>
/// <para>Based on the SNES9x emulator's EPXC implementation.</para>
/// </remarks>
[ScalerInfo("EPX-C", Year = 2000, Url = "https://en.wikipedia.org/wiki/Pixel-art_scaling_algorithms#EPX",
  Description = "SNES9x enhanced EPX with weighted blending", Category = ScalerCategory.PixelArt)]
public readonly struct EpxC : IPixelScaler {

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
    => callback.Invoke(new EpxCKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp));

  /// <summary>
  /// Gets the list of scale factors supported by EPX-C.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2)];

  /// <summary>
  /// Determines whether EPX-C supports the specified scale factor.
  /// </summary>
  /// <param name="scale">The scale factor to check.</param>
  /// <returns><c>true</c> if the scale is 2x2; otherwise, <c>false</c>.</returns>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 };

  /// <summary>
  /// Enumerates all possible target dimensions for EPX-C.
  /// </summary>
  /// <param name="sourceWidth">The source image width.</param>
  /// <param name="sourceHeight">The source image height.</param>
  /// <returns>The target dimensions (2x in both dimensions).</returns>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
  }

  /// <summary>
  /// Gets the default EPX-C configuration.
  /// </summary>
  public static EpxC Default => new();
}

/// <summary>
/// Internal kernel for EPX-C (SNES9x EPXC) algorithm.
/// </summary>
/// <remarks>
/// EPX-C pattern (uses 3x3 neighborhood):
///
/// C0 C1 C2      (top-left, top, top-right)
/// C3 C4 C5      (left, center, right)
/// C6 C7 C8      (bottom-left, bottom, bottom-right)
///
/// Output 2x2 block:
/// E00 E01
/// E10 E11
///
/// EPX-C is an enhanced version of EPX with more sophisticated edge detection
/// and weighted blending that produces smoother results on complex patterns.
/// </remarks>
file readonly struct EpxCKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
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

    // Weight for 2-color blend: 3:1 ratio = 1/4 for secondary
    private const float QuarterWeight = 0.25f;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void Scale(
      in NeighborWindow<TWork, TKey> window,
      TPixel* destTopLeft,
      int destStride,
      in TEncode encoder
    ) {
      // Get the 3x3 source neighborhood
      var c0 = window.M1M1; // top-left
      var c1 = window.P0M1; // top
      var c2 = window.P1M1; // top-right
      var c3 = window.M1P0; // left
      var c4 = window.P0P0; // center
      var c5 = window.P1P0; // right
      var c6 = window.M1P1; // bottom-left
      var c7 = window.P0P1; // bottom
      var c8 = window.P1P1; // bottom-right

      var c4Work = c4.Work;

      // Default all outputs to center pixel
      var e00 = c4Work;
      var e01 = c4Work;
      var e10 = c4Work;
      var e11 = c4Work;

      // Core condition: left != right and bottom != top
      if (!equality.Equals(c3.Key, c5.Key) && !equality.Equals(c7.Key, c1.Key)) {
        // Pre-compute inequality tests: c4 != each neighbor
        var neq40 = !equality.Equals(c4.Key, c0.Key);
        var neq41 = !equality.Equals(c4.Key, c1.Key);
        var neq42 = !equality.Equals(c4.Key, c2.Key);
        var neq43 = !equality.Equals(c4.Key, c3.Key);
        var neq45 = !equality.Equals(c4.Key, c5.Key);
        var neq46 = !equality.Equals(c4.Key, c6.Key);
        var neq47 = !equality.Equals(c4.Key, c7.Key);
        var neq48 = !equality.Equals(c4.Key, c8.Key);

        // Corner matching conditions with additional requirements
        var eq13 = equality.Equals(c1.Key, c3.Key) && (neq40 || neq48 || !equality.Equals(c1.Key, c2.Key) || !equality.Equals(c3.Key, c6.Key));
        var eq37 = equality.Equals(c3.Key, c7.Key) && (neq46 || neq42 || !equality.Equals(c3.Key, c0.Key) || !equality.Equals(c7.Key, c8.Key));
        var eq75 = equality.Equals(c7.Key, c5.Key) && (neq48 || neq40 || !equality.Equals(c7.Key, c6.Key) || !equality.Equals(c5.Key, c2.Key));
        var eq51 = equality.Equals(c5.Key, c1.Key) && (neq42 || neq46 || !equality.Equals(c5.Key, c8.Key) || !equality.Equals(c1.Key, c0.Key));

        // Check if center differs from any neighbor
        var anyNeighborMatches = !neq40 || !neq41 || !neq42 || !neq43 || !neq45 || !neq46 || !neq47 || !neq48;

        if (anyNeighborMatches) {
          // Enhanced blending path with weighted interpolation

          // Compute left edge blend (c3A)
          TWork c3A;
          if (eq13 && neq46 && eq37 && neq40)
            c3A = lerp.Lerp(c3.Work, c1.Work, c7.Work);
          else if (eq13 && neq46)
            c3A = lerp.Lerp(c3.Work, c1.Work);
          else if (eq37 && neq40)
            c3A = lerp.Lerp(c3.Work, c7.Work);
          else
            c3A = c4Work;

          // Compute bottom edge blend (c7B)
          TWork c7B;
          if (eq37 && neq48 && eq75 && neq46)
            c7B = lerp.Lerp(c7.Work, c3.Work, c5.Work);
          else if (eq37 && neq48)
            c7B = lerp.Lerp(c7.Work, c3.Work);
          else if (eq75 && neq46)
            c7B = lerp.Lerp(c7.Work, c5.Work);
          else
            c7B = c4Work;

          // Compute right edge blend (c5C)
          TWork c5C;
          if (eq75 && neq42 && eq51 && neq48)
            c5C = lerp.Lerp(c5.Work, c1.Work, c7.Work);
          else if (eq75 && neq42)
            c5C = lerp.Lerp(c5.Work, c7.Work);
          else if (eq51 && neq48)
            c5C = lerp.Lerp(c5.Work, c1.Work);
          else
            c5C = c4Work;

          // Compute top edge blend (c1D)
          TWork c1D;
          if (eq51 && neq40 && eq13 && neq42)
            c1D = lerp.Lerp(c1.Work, c3.Work, c5.Work);
          else if (eq51 && neq40)
            c1D = lerp.Lerp(c1.Work, c5.Work);
          else if (eq13 && neq42)
            c1D = lerp.Lerp(c1.Work, c3.Work);
          else
            c1D = c4Work;

          // Corner blends
          if (eq13)
            e00 = lerp.Lerp(c1.Work, c3.Work);
          if (eq51)
            e01 = lerp.Lerp(c5.Work, c1.Work);
          if (eq37)
            e10 = lerp.Lerp(c3.Work, c7.Work);
          if (eq75)
            e11 = lerp.Lerp(c7.Work, c5.Work);

          // Final weighted blend: 5:1:1:1 ratio
          e00 = lerp.Lerp(e00, c1D, c3A, c4Work, 5, 1, 1, 1);
          e01 = lerp.Lerp(e01, c7B, c5C, c4Work, 5, 1, 1, 1);
          e10 = lerp.Lerp(e10, c3A, c7B, c4Work, 5, 1, 1, 1);
          e11 = lerp.Lerp(e11, c5C, c1D, c4Work, 5, 1, 1, 1);
        } else {
          // Simple blending path (3:1 ratio with center)
          if (eq13)
            e00 = lerp.Lerp(c1.Work, c3.Work);
          if (eq51)
            e01 = lerp.Lerp(c5.Work, c1.Work);
          if (eq37)
            e10 = lerp.Lerp(c3.Work, c7.Work);
          if (eq75)
            e11 = lerp.Lerp(c7.Work, c5.Work);

          // Blend with center at 3:1 ratio
          e00 = lerp.Lerp(c4Work, e00, QuarterWeight);
          e01 = lerp.Lerp(c4Work, e01, QuarterWeight);
          e10 = lerp.Lerp(c4Work, e10, QuarterWeight);
          e11 = lerp.Lerp(c4Work, e11, QuarterWeight);
        }
      }

      // Write directly to destination with encoding
      var row0 = destTopLeft;
      var row1 = destTopLeft + destStride;
      row0[0] = encoder.Encode(e00);
      row0[1] = encoder.Encode(e01);
      row1[0] = encoder.Encode(e10);
      row1[1] = encoder.Encode(e11);
    }
}
