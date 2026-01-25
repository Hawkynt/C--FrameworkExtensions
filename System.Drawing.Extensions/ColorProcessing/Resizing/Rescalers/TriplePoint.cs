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

using System;
using System.Collections.Generic;
using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Rescalers;

/// <summary>
/// Triple Point scaler - 3-point interpolation for smooth edges.
/// </summary>
/// <remarks>
/// <para>Uses 3-point average at corners where edges meet.</para>
/// <para>Provides smoother diagonal edges than pure nearest-neighbor while preserving flat areas.</para>
/// <para>Similar to EPX/Scale2x but with blending instead of binary decisions.</para>
/// </remarks>
[ScalerInfo("TriplePoint",
  Description = "3-point interpolation for smooth edges", Category = ScalerCategory.PixelArt)]
public readonly struct TriplePoint : IPixelScaler {
  private readonly int _scale;

  /// <summary>
  /// Creates a TriplePoint scaler with the specified factor.
  /// </summary>
  /// <param name="scale">Scale factor (2 or 3).</param>
  public TriplePoint(int scale = 2) {
    if (scale is < 2 or > 3)
      throw new ArgumentOutOfRangeException(nameof(scale), scale, "TriplePoint supports 2x or 3x scaling");
    this._scale = scale;
  }

  /// <inheritdoc />
  public ScaleFactor Scale => this._scale == 0 ? new(2, 2) : new(this._scale, this._scale);

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
    => this._scale switch {
      0 or 2 => callback.Invoke(new TriplePoint2xKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode>(equality, lerp)),
      3 => callback.Invoke(new TriplePoint3xKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode>(equality, lerp)),
      _ => throw new InvalidOperationException($"Invalid scale factor: {this._scale}")
    };

  /// <summary>
  /// Gets the list of scale factors supported.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2), new(3, 3)];

  /// <summary>
  /// Determines whether the specified scale factor is supported.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 } or { X: 3, Y: 3 };

  /// <summary>
  /// Enumerates all possible target dimensions.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
    yield return (sourceWidth * 3, sourceHeight * 3);
  }

  #region Static Presets

  /// <summary>Gets a 2x TriplePoint scaler.</summary>
  public static TriplePoint X2 => new(2);

  /// <summary>Gets a 3x TriplePoint scaler.</summary>
  public static TriplePoint X3 => new(3);

  /// <summary>Gets the default TriplePoint scaler (2x).</summary>
  public static TriplePoint Default => X2;

  #endregion
}

#region TriplePoint Helpers

file static class TriplePointHelpers {
  /// <summary>
  /// Checks if two colors are similar using the provided equality comparer.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool AreColorsSimilar<TKey, TEquality>(in TEquality equality, in TKey a, in TKey b)
    where TKey : unmanaged, IColorSpace
    where TEquality : struct, IColorEquality<TKey>
    => equality.Equals(a, b);

  /// <summary>
  /// Blends three colors with equal weight using chained lerps.
  /// </summary>
  /// <remarks>
  /// First lerp: (c1 + c2) / 2
  /// Then: (result + c3) / 2 with adjusted weights to get (c1 + c2 + c3) / 3
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TWork Blend3<TWork, TLerp>(in TLerp lerp, in TWork c1, in TWork c2, in TWork c3)
    where TWork : unmanaged, IColorSpace
    where TLerp : struct, ILerp<TWork> {
    // Equal weight blend: (c1 + c2 + c3) / 3
    // Using lerp: first combine c1 and c2, then add c3 with 1:2 ratio
    var combined = lerp.Lerp(c1, c2);
    return lerp.Lerp(combined, c3, 2, 1);
  }
}

#endregion

#region TriplePoint 2x Kernel

file readonly struct TriplePoint2xKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode>(
  TEquality equality = default,
  TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TDistance : struct, IColorMetric<TKey>, INormalizedMetric
  where TEquality : struct, IColorEquality<TKey>
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
    // Get center and edge neighbors
    var center = window.P0P0;
    var top = window.M1P0;
    var bottom = window.P1P0;
    var left = window.P0M1;
    var right = window.P0P1;

    // Default all 4 output pixels to center
    var p00 = center.Work;
    var p10 = center.Work;
    var p01 = center.Work;
    var p11 = center.Work;

    // At each corner, if the two adjacent edge pixels are similar,
    // blend center with those two edge pixels (3-point average)
    if (TriplePointHelpers.AreColorsSimilar(equality, top.Key, left.Key))
      p00 = TriplePointHelpers.Blend3(lerp, center.Work, top.Work, left.Work);

    if (TriplePointHelpers.AreColorsSimilar(equality, top.Key, right.Key))
      p10 = TriplePointHelpers.Blend3(lerp, center.Work, top.Work, right.Work);

    if (TriplePointHelpers.AreColorsSimilar(equality, bottom.Key, left.Key))
      p01 = TriplePointHelpers.Blend3(lerp, center.Work, bottom.Work, left.Work);

    if (TriplePointHelpers.AreColorsSimilar(equality, bottom.Key, right.Key))
      p11 = TriplePointHelpers.Blend3(lerp, center.Work, bottom.Work, right.Work);

    // Write output pixels
    dest[0] = encoder.Encode(p00);
    dest[1] = encoder.Encode(p10);
    dest[destStride] = encoder.Encode(p01);
    dest[destStride + 1] = encoder.Encode(p11);
  }
}

#endregion

#region TriplePoint 3x Kernel

file readonly struct TriplePoint3xKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode>(
  TEquality equality = default,
  TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TDistance : struct, IColorMetric<TKey>, INormalizedMetric
  where TEquality : struct, IColorEquality<TKey>
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 3;
  public int ScaleY => 3;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    // Get center and edge neighbors
    var center = window.P0P0;
    var top = window.M1P0;
    var bottom = window.P1P0;
    var left = window.P0M1;
    var right = window.P0P1;

    var centerEncoded = encoder.Encode(center.Work);

    // Fill entire 3x3 with center by default
    for (var dy = 0; dy < 3; ++dy) {
      var row = dest + dy * destStride;
      for (var dx = 0; dx < 3; ++dx)
        row[dx] = centerEncoded;
    }

    // Apply 3-point blending at corners only
    if (TriplePointHelpers.AreColorsSimilar(equality, top.Key, left.Key))
      dest[0] = encoder.Encode(TriplePointHelpers.Blend3(lerp, center.Work, top.Work, left.Work));

    if (TriplePointHelpers.AreColorsSimilar(equality, top.Key, right.Key))
      dest[2] = encoder.Encode(TriplePointHelpers.Blend3(lerp, center.Work, top.Work, right.Work));

    if (TriplePointHelpers.AreColorsSimilar(equality, bottom.Key, left.Key))
      dest[2 * destStride] = encoder.Encode(TriplePointHelpers.Blend3(lerp, center.Work, bottom.Work, left.Work));

    if (TriplePointHelpers.AreColorsSimilar(equality, bottom.Key, right.Key))
      dest[2 * destStride + 2] = encoder.Encode(TriplePointHelpers.Blend3(lerp, center.Work, bottom.Work, right.Work));
  }
}

#endregion
