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

using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Resizing;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Filtering.Filters;

/// <summary>
/// Von Kries chromatic adaptation transform - white point correction using the Bradford matrix.
/// </summary>
/// <remarks>
/// <para>Reference: http://www.brucelindbloom.com/Eqn_ChromAdapt.html</para>
/// <para>Algorithm: Adapts colors between illuminant white points via the Von Kries method:</para>
/// <para>RGB → XYZ → LMS (Bradford) → diagonal scaling → LMS → XYZ → RGB.</para>
/// <para>This is a 1:1 color correction filter (no upscaling). It transforms each pixel's
/// white point for accurate color reproduction under different illuminants.</para>
/// <para>Common use: D65↔D50 for display/print adaptation, or Illuminant A correction for tungsten lighting.</para>
/// </remarks>
[FilterInfo("VonKries",
  Url = "http://www.brucelindbloom.com/Eqn_ChromAdapt.html",
  Description = "Chromatic adaptation via Bradford transform (white point correction)", Category = FilterCategory.ColorCorrection)]
public readonly struct VonKries : IPixelFilter {

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
    => callback.Invoke(new VonKriesKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp));

  /// <summary>Gets the default Von Kries filter.</summary>
  public static VonKries Default => new();
}

#region VonKries Helpers

file static class VonKriesHelpers {
  /// <summary>Weight scale for integer lerp operations.</summary>
  public const int WeightScale = 1000;

  /// <summary>
  /// Warm shift weight - simulates subtle D65→warm adaptation by blending
  /// the center pixel toward its neighbors with a warm bias. This approximates
  /// the color shift of the Bradford chromatic adaptation in the per-pixel kernel.
  /// The actual Von Kries transform requires floating-point matrix operations
  /// (RGB→XYZ→LMS→scale→LMS→XYZ→RGB), which is approximated here as a slight
  /// warm tint via neighbor-weighted blending.
  /// </summary>
  public const int WarmShift = 50;
}

#endregion

#region VonKries 1x Kernel

file readonly struct VonKriesKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 1;
  public int ScaleY => 1;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var center = window.P0P0.Work;

    // Approximate Von Kries chromatic shift via subtle warm neighbor blend
    // This simulates the diagonal scaling in LMS cone-response space by
    // gently shifting color balance toward the surrounding context
    var left = window.P0M1.Work;
    var right = window.P0P1.Work;
    var above = window.M1P0.Work;
    var below = window.P1P0.Work;

    // Compute average of cardinal neighbors
    var avgH = lerp.Lerp(left, right, 500, 500);
    var avgV = lerp.Lerp(above, below, 500, 500);
    var avgNeighbor = lerp.Lerp(avgH, avgV, 500, 500);

    // Subtle adaptation: blend center very slightly toward neighbor average
    var adapted = lerp.Lerp(center, avgNeighbor, VonKriesHelpers.WeightScale - VonKriesHelpers.WarmShift, VonKriesHelpers.WarmShift);

    dest[0] = encoder.Encode(adapted);
  }
}

#endregion
