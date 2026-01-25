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
/// NTSC video simulation scaler simulating composite video artifacts.
/// </summary>
/// <remarks>
/// <para>Reference: blargg (Shay Green) md_ntsc library</para>
/// <para>Algorithm: Simulates NTSC composite video color bleeding by blending neighbors.</para>
/// <para>Composite video causes horizontal color bleeding and chroma artifacts.</para>
/// </remarks>
[ScalerInfo("NTSC", Author = "blargg",
  Description = "NTSC composite video color bleeding simulation", Category = ScalerCategory.Resampler)]
public readonly struct Ntsc : IPixelScaler {

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
    => callback.Invoke(new NtscKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp));

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

  /// <summary>Gets the default NTSC scaler.</summary>
  public static Ntsc Default => new();
}

#region NTSC Helpers

file static class NtscHelpers {
  /// <summary>Weight scale for integer lerp operations.</summary>
  public const int WeightScale = 1000;

  /// <summary>Center weight for E0 position (70%).</summary>
  public const int CenterWeight = 700;

  /// <summary>Neighbor weight for E0 position (30%).</summary>
  public const int NeighborWeight = 300;

  /// <summary>Weak blend weight (85%).</summary>
  public const int WeakCenter = 850;

  /// <summary>Weak neighbor weight (15%).</summary>
  public const int WeakNeighbor = 150;
}

#endregion

#region NTSC 2x Kernel

/// <summary>
/// Kernel for NTSC composite video simulation.
/// </summary>
/// <remarks>
/// Uses 2x2 neighborhood to simulate NTSC color bleeding:
///
/// P0 P1      (center, right)
/// P2 P3      (bottom, bottom-right)
///
/// Output 2x2 block with color bleeding:
/// E0 = blend(center, right, 70%, 30%)
/// E1 = blend(center, right, 30%, 70%)
/// E2 = blend(bottom, bottom-right, 70%, 30%)
/// E3 = blend(bottom, bottom-right, 30%, 70%)
///
/// This simulates the horizontal color bleeding of composite video signals.
/// </remarks>
file readonly struct NtscKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    // Get 2x2 neighborhood
    var center = window.P0P0.Work;     // center
    var right = window.P0P1.Work;      // right neighbor
    var bottom = window.P1P0.Work;     // bottom neighbor
    var bottomRight = window.P1P1.Work; // bottom-right neighbor

    // Simulate horizontal color bleeding (composite artifact)
    // E0: mostly center, some right
    var e0 = lerp.Lerp(center, right, NtscHelpers.CenterWeight, NtscHelpers.NeighborWeight);

    // E1: mostly right, some center
    var e1 = lerp.Lerp(right, center, NtscHelpers.CenterWeight, NtscHelpers.NeighborWeight);

    // E2: blend center with bottom, then add some horizontal bleeding
    var centerBottom = lerp.Lerp(center, bottom, NtscHelpers.WeakCenter, NtscHelpers.WeakNeighbor);
    var rightBottom = lerp.Lerp(right, bottomRight, NtscHelpers.WeakCenter, NtscHelpers.WeakNeighbor);
    var e2 = lerp.Lerp(centerBottom, rightBottom, NtscHelpers.CenterWeight, NtscHelpers.NeighborWeight);

    // E3: blend right/bottom-right with some center influence
    var e3 = lerp.Lerp(rightBottom, centerBottom, NtscHelpers.CenterWeight, NtscHelpers.NeighborWeight);

    // Write output
    dest[0] = encoder.Encode(e0);
    dest[1] = encoder.Encode(e1);
    dest[destStride] = encoder.Encode(e2);
    dest[destStride + 1] = encoder.Encode(e3);
  }
}

#endregion
