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
/// ScaleHQ - High Quality edge-aware scaling algorithm.
/// </summary>
/// <remarks>
/// <para>ScaleHQ is an edge-aware scaling algorithm that uses weighted interpolation</para>
/// <para>based on color differences in a 3x3 neighborhood.</para>
/// <para>Algorithm:</para>
/// <list type="number">
/// <item>Get 3x3 neighborhood around source pixel</item>
/// <item>Calculate edge weights from color differences</item>
/// <item>Apply weighted interpolation based on edge detection</item>
/// <item>Use smoothstep for blending</item>
/// </list>
/// </remarks>
[ScalerInfo("ScaleHQ",
  Description = "High quality edge-aware scaling with weighted interpolation", Category = ScalerCategory.PixelArt)]
public readonly struct ScaleHq : IPixelScaler {
  private readonly int _scale;

  /// <summary>
  /// Creates a ScaleHQ scaler with the specified factor.
  /// </summary>
  /// <param name="scale">Scale factor (2 or 4).</param>
  public ScaleHq(int scale = 2) {
    if (scale is not (2 or 4))
      throw new ArgumentOutOfRangeException(nameof(scale), scale, "ScaleHQ supports 2x or 4x scaling");
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
      0 or 2 => callback.Invoke(new ScaleHq2xKernel<TWork, TKey, TPixel, TDistance, TLerp, TEncode>(lerp)),
      4 => callback.Invoke(new ScaleHq4xKernel<TWork, TKey, TPixel, TDistance, TLerp, TEncode>(lerp)),
      _ => throw new InvalidOperationException($"Invalid scale factor: {this._scale}")
    };

  /// <summary>
  /// Gets the list of scale factors supported.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2), new(4, 4)];

  /// <summary>
  /// Determines whether the specified scale factor is supported.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 } or { X: 4, Y: 4 };

  /// <summary>
  /// Enumerates all possible target dimensions.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
    yield return (sourceWidth * 4, sourceHeight * 4);
  }

  #region Static Presets

  /// <summary>Gets a 2x ScaleHQ scaler.</summary>
  public static ScaleHq X2 => new(2);

  /// <summary>Gets a 4x ScaleHQ scaler.</summary>
  public static ScaleHq X4 => new(4);

  /// <summary>Gets the default ScaleHQ scaler (2x).</summary>
  public static ScaleHq Default => X2;

  #endregion
}

#region ScaleHQ Helpers

file static class ScaleHqHelpers {
  /// <summary>Scale factor for converting float weights to integer weights.</summary>
  public const int WeightScale = 1000;

  /// <summary>
  /// Applies smoothstep transformation: 3t² - 2t³
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Smoothstep(float t)
    => t * t * (3f - 2f * t);

  /// <summary>
  /// Gets the color distance using the provided metric.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float GetDistance<TKey, TDistance>(in TDistance metric, in TKey a, in TKey b)
    where TKey : unmanaged, IColorSpace
    where TDistance : struct, IColorMetric<TKey>, INormalizedMetric
    => metric.Distance(a, b).ToFloat();

  /// <summary>
  /// Linear interpolation between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TWork LerpColors<TWork, TLerp>(in TLerp lerp, in TWork c1, in TWork c2, float t)
    where TWork : unmanaged, IColorSpace
    where TLerp : struct, ILerp<TWork> {
    var w2 = (int)(t * WeightScale);
    var w1 = WeightScale - w2;
    return lerp.Lerp(c1, c2, w1, w2);
  }

  /// <summary>
  /// Weighted blend of 4 colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TWork WeightedBlend4<TWork, TLerp>(
    in TLerp lerp,
    in TWork c1, float w1,
    in TWork c2, float w2,
    in TWork c3, float w3,
    in TWork c4, float w4)
    where TWork : unmanaged, IColorSpace
    where TLerp : struct, ILerp<TWork> {
    // Normalize weights to sum to 1
    var total = w1 + w2 + w3 + w4;
    if (total < 0.001f)
      return c1; // Fallback

    w1 /= total;
    w2 /= total;
    w3 /= total;
    w4 /= total;

    // Blend in pairs, then blend results
    var iw1 = (int)(w1 * WeightScale);
    var iw2 = (int)(w2 * WeightScale);
    var iw3 = (int)(w3 * WeightScale);
    var iw4 = (int)(w4 * WeightScale);

    var blend12 = lerp.Lerp(c1, c2, iw1, iw2);
    var blend34 = lerp.Lerp(c3, c4, iw3, iw4);
    return lerp.Lerp(blend12, blend34, iw1 + iw2, iw3 + iw4);
  }
}

#endregion

#region ScaleHQ 2x Kernel

file readonly struct ScaleHq2xKernel<TWork, TKey, TPixel, TDistance, TLerp, TEncode>(TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TDistance : struct, IColorMetric<TKey>, INormalizedMetric
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
    TDistance metric = default;

    // Get 3x3 neighborhood
    var a = window.M1M1;
    var b = window.M1P0;
    var c = window.M1P1;
    var d = window.P0M1;
    var e = window.P0P0; // Center pixel
    var f = window.P0P1;
    var g = window.P1M1;
    var h = window.P1P0;
    var i = window.P1P1;

    // Calculate edge weights from color differences
    var diffH = ScaleHqHelpers.GetDistance(metric, d.Key, f.Key);
    var diffV = ScaleHqHelpers.GetDistance(metric, b.Key, h.Key);
    var diffD1 = ScaleHqHelpers.GetDistance(metric, a.Key, i.Key);
    var diffD2 = ScaleHqHelpers.GetDistance(metric, c.Key, g.Key);

    // Calculate edge weights (inverse of difference)
    var wH = 1f / (1f + diffH);
    var wV = 1f / (1f + diffV);
    var wD1 = 1f / (1f + diffD1);
    var wD2 = 1f / (1f + diffD2);

    // Process each of the 4 output pixels
    for (var sy = 0; sy < 2; ++sy) {
      var row = dest + sy * destStride;
      for (var sx = 0; sx < 2; ++sx) {
        var fx = ScaleHqHelpers.Smoothstep((sx + 0.5f) / 2f);
        var fy = ScaleHqHelpers.Smoothstep((sy + 0.5f) / 2f);

        // Interpolate along each direction
        var cH = ScaleHqHelpers.LerpColors(lerp, d.Work, f.Work, fx);
        var cV = ScaleHqHelpers.LerpColors(lerp, b.Work, h.Work, fy);
        var t1 = (fx + fy) / 2f;
        var cD1 = ScaleHqHelpers.LerpColors(lerp, a.Work, i.Work, t1);
        var t2 = (fx + (1f - fy)) / 2f;
        var cD2 = ScaleHqHelpers.LerpColors(lerp, g.Work, c.Work, t2);

        // Weighted blend
        row[sx] = encoder.Encode(ScaleHqHelpers.WeightedBlend4(lerp, cH, wH, cV, wV, cD1, wD1, cD2, wD2));
      }
    }
  }
}

#endregion

#region ScaleHQ 4x Kernel

file readonly struct ScaleHq4xKernel<TWork, TKey, TPixel, TDistance, TLerp, TEncode>(TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TDistance : struct, IColorMetric<TKey>, INormalizedMetric
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 4;
  public int ScaleY => 4;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    TDistance metric = default;

    // Get 3x3 neighborhood
    var a = window.M1M1;
    var b = window.M1P0;
    var c = window.M1P1;
    var d = window.P0M1;
    var e = window.P0P0;
    var f = window.P0P1;
    var g = window.P1M1;
    var h = window.P1P0;
    var i = window.P1P1;

    // Calculate edge weights from color differences
    var diffH = ScaleHqHelpers.GetDistance(metric, d.Key, f.Key);
    var diffV = ScaleHqHelpers.GetDistance(metric, b.Key, h.Key);
    var diffD1 = ScaleHqHelpers.GetDistance(metric, a.Key, i.Key);
    var diffD2 = ScaleHqHelpers.GetDistance(metric, c.Key, g.Key);

    // Calculate edge weights (inverse of difference)
    var wH = 1f / (1f + diffH);
    var wV = 1f / (1f + diffV);
    var wD1 = 1f / (1f + diffD1);
    var wD2 = 1f / (1f + diffD2);

    // Process each of the 16 output pixels
    for (var sy = 0; sy < 4; ++sy) {
      var row = dest + sy * destStride;
      for (var sx = 0; sx < 4; ++sx) {
        var fx = ScaleHqHelpers.Smoothstep((sx + 0.5f) / 4f);
        var fy = ScaleHqHelpers.Smoothstep((sy + 0.5f) / 4f);

        // Interpolate along each direction
        var cH = ScaleHqHelpers.LerpColors(lerp, d.Work, f.Work, fx);
        var cV = ScaleHqHelpers.LerpColors(lerp, b.Work, h.Work, fy);
        var t1 = (fx + fy) / 2f;
        var cD1 = ScaleHqHelpers.LerpColors(lerp, a.Work, i.Work, t1);
        var t2 = (fx + (1f - fy)) / 2f;
        var cD2 = ScaleHqHelpers.LerpColors(lerp, g.Work, c.Work, t2);

        // Weighted blend
        row[sx] = encoder.Encode(ScaleHqHelpers.WeightedBlend4(lerp, cH, wH, cV, wV, cD1, wD1, cD2, wD2));
      }
    }
  }
}

#endregion
