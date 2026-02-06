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
/// CUT (Cheap Upscaling Triangulation) scaler.
/// </summary>
/// <remarks>
/// <para>Geometry-aware pixel art upscaler using triangulation.</para>
/// <para>Analyzes luma orientation in 2x2 blocks to determine diagonal splits.</para>
/// <para>Efficient algorithm designed for retro game content.</para>
/// <para>Reference: Swordfish90's CUT implementation</para>
/// </remarks>
[ScalerInfo("CUT", Author = "Swordfish90",
  Url = "https://github.com/Swordfish90/cheap-upscaling-triangulation",
  Description = "Cheap Upscaling Triangulation for pixel art", Category = ScalerCategory.PixelArt)]
public readonly struct Cut : IPixelScaler {
  private readonly int _scale;

  /// <summary>
  /// Creates a CUT scaler with the specified factor.
  /// </summary>
  /// <param name="scale">Scale factor (2, 3, or 4).</param>
  public Cut(int scale = 2) {
    ArgumentOutOfRangeException.ThrowIfLessThan(scale, 2);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(scale, 4);
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
      0 or 2 => callback.Invoke(new Cut2xKernel<TWork, TKey, TPixel, TDistance, TLerp, TEncode>(lerp)),
      3 => callback.Invoke(new Cut3xKernel<TWork, TKey, TPixel, TDistance, TLerp, TEncode>(lerp)),
      4 => callback.Invoke(new Cut4xKernel<TWork, TKey, TPixel, TDistance, TLerp, TEncode>(lerp)),
      _ => throw new InvalidOperationException($"Invalid scale factor: {this._scale}")
    };

  /// <summary>
  /// Gets the list of scale factors supported.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2), new(3, 3), new(4, 4)];

  /// <summary>
  /// Determines whether the specified scale factor is supported.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 } or { X: 3, Y: 3 } or { X: 4, Y: 4 };

  /// <summary>
  /// Enumerates all possible target dimensions.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
    yield return (sourceWidth * 3, sourceHeight * 3);
    yield return (sourceWidth * 4, sourceHeight * 4);
  }

  #region Static Presets

  /// <summary>Gets a 2x CUT scaler.</summary>
  public static Cut X2 => new(2);

  /// <summary>Gets a 3x CUT scaler.</summary>
  public static Cut X3 => new(3);

  /// <summary>Gets a 4x CUT scaler.</summary>
  public static Cut X4 => new(4);

  /// <summary>Gets the default CUT scaler (2x).</summary>
  public static Cut Default => X2;

  #endregion
}

#region CUT Helpers

/// <summary>
/// Helper methods for CUT algorithm.
/// </summary>
file static class CutHelpers {
  /// <summary>
  /// Edge detection threshold for distance comparison.
  /// </summary>
  public const float EdgeThreshold = 0.15f;

  /// <summary>
  /// Blend weight for edge-detected pixels.
  /// </summary>
  public const float EdgeBlendWeight = 0.8f;

  /// <summary>
  /// Blend weight for directional edge pixels.
  /// </summary>
  public const float DirectionalBlendWeight = 0.7f;

  /// <summary>
  /// Gets the color distance using the provided metric.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float GetDistance<TKey, TDistance>(in TDistance metric, in TKey a, in TKey b)
    where TKey : unmanaged, IColorSpace
    where TDistance : struct, IColorMetric<TKey>, INormalizedMetric
    => metric.Distance(a, b).ToFloat();

  /// <summary>
  /// Determines the output pixel based on edge detection and triangulation.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TWork ProcessSubpixel<TWork, TKey, TDistance, TLerp>(
    in TDistance metric,
    in TWork center, in TKey centerKey,
    in TWork n, in TKey nKey, in TWork s, in TKey sKey,
    in TWork e, in TKey eKey, in TWork w, in TKey wKey,
    in TWork ne, in TKey neKey, in TWork nw, in TKey nwKey,
    in TWork se, in TKey seKey, in TWork sw, in TKey swKey,
    float fx, float fy,
    in TLerp lerp)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TDistance : struct, IColorMetric<TKey>, INormalizedMetric
    where TLerp : struct, ILerp<TWork> {
    // Use metric distances instead of luma
    // Determine diagonal orientation using color distances
    var diagNeSw = GetDistance(metric, neKey, swKey);
    var diagNwSe = GetDistance(metric, nwKey, seKey);

    // Calculate edge detection using distances from center
    var distN = GetDistance(metric, centerKey, nKey);
    var distS = GetDistance(metric, centerKey, sKey);
    var distE = GetDistance(metric, centerKey, eKey);
    var distW = GetDistance(metric, centerKey, wKey);

    // Determine edge directions
    var isHorizontalEdge = (distN > EdgeThreshold || distS > EdgeThreshold) && distN + distS > distE + distW;
    var isVerticalEdge = (distE > EdgeThreshold || distW > EdgeThreshold) && distE + distW > distN + distS;
    var isDiagonalNeSw = diagNeSw > EdgeThreshold && diagNeSw > diagNwSe;
    var isDiagonalNwSe = diagNwSe > EdgeThreshold && diagNwSe > diagNeSw;

    // Blend weights
    var edgeWeight = (int)(EdgeBlendWeight * 256);
    var invEdgeWeight = 256 - edgeWeight;
    var dirWeight = (int)(DirectionalBlendWeight * 256);
    var invDirWeight = 256 - dirWeight;

    if (isDiagonalNeSw) {
      // NE-SW diagonal detected: blend based on which triangle
      if (fx + fy < 1f)
        return lerp.Lerp(center, nw, edgeWeight, invEdgeWeight);
      return lerp.Lerp(center, se, edgeWeight, invEdgeWeight);
    }

    if (isDiagonalNwSe) {
      // NW-SE diagonal detected
      if (fx > fy)
        return lerp.Lerp(center, ne, edgeWeight, invEdgeWeight);
      return lerp.Lerp(center, sw, edgeWeight, invEdgeWeight);
    }

    if (isHorizontalEdge) {
      // Horizontal edge: blend based on vertical position
      if (fy < 0.5f)
        return lerp.Lerp(center, n, dirWeight, invDirWeight);
      return lerp.Lerp(center, s, dirWeight, invDirWeight);
    }

    if (isVerticalEdge) {
      // Vertical edge: blend based on horizontal position
      if (fx < 0.5f)
        return lerp.Lerp(center, w, dirWeight, invDirWeight);
      return lerp.Lerp(center, e, dirWeight, invDirWeight);
    }

    // No significant edge - use center color
    return center;
  }
}

#endregion

#region CUT 2x Kernel

file readonly struct Cut2xKernel<TWork, TKey, TPixel, TDistance, TLerp, TEncode>(TLerp lerp = default)
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
    // Get 3x3 neighborhood
    var c = window.P0P0;
    var n = window.M1P0;
    var s = window.P1P0;
    var e = window.P0P1;
    var w = window.P0M1;
    var ne = window.M1P1;
    var nw = window.M1M1;
    var se = window.P1P1;
    var sw = window.P1M1;

    TDistance metric = default;

    for (var py = 0; py < 2; ++py) {
      var row = dest + py * destStride;
      for (var px = 0; px < 2; ++px) {
        var fx = (px + 0.5f) / 2f;
        var fy = (py + 0.5f) / 2f;

        var result = CutHelpers.ProcessSubpixel<TWork, TKey, TDistance, TLerp>(
          metric,
          c.Work, c.Key,
          n.Work, n.Key, s.Work, s.Key,
          e.Work, e.Key, w.Work, w.Key,
          ne.Work, ne.Key, nw.Work, nw.Key,
          se.Work, se.Key, sw.Work, sw.Key,
          fx, fy, lerp);

        row[px] = encoder.Encode(result);
      }
    }
  }
}

#endregion

#region CUT 3x Kernel

file readonly struct Cut3xKernel<TWork, TKey, TPixel, TDistance, TLerp, TEncode>(TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TDistance : struct, IColorMetric<TKey>, INormalizedMetric
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
    var c = window.P0P0;
    var n = window.M1P0;
    var s = window.P1P0;
    var e = window.P0P1;
    var w = window.P0M1;
    var ne = window.M1P1;
    var nw = window.M1M1;
    var se = window.P1P1;
    var sw = window.P1M1;

    TDistance metric = default;

    for (var py = 0; py < 3; ++py) {
      var row = dest + py * destStride;
      for (var px = 0; px < 3; ++px) {
        var fx = (px + 0.5f) / 3f;
        var fy = (py + 0.5f) / 3f;

        var result = CutHelpers.ProcessSubpixel<TWork, TKey, TDistance, TLerp>(
          metric,
          c.Work, c.Key,
          n.Work, n.Key, s.Work, s.Key,
          e.Work, e.Key, w.Work, w.Key,
          ne.Work, ne.Key, nw.Work, nw.Key,
          se.Work, se.Key, sw.Work, sw.Key,
          fx, fy, lerp);

        row[px] = encoder.Encode(result);
      }
    }
  }
}

#endregion

#region CUT 4x Kernel

file readonly struct Cut4xKernel<TWork, TKey, TPixel, TDistance, TLerp, TEncode>(TLerp lerp = default)
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
    var c = window.P0P0;
    var n = window.M1P0;
    var s = window.P1P0;
    var e = window.P0P1;
    var w = window.P0M1;
    var ne = window.M1P1;
    var nw = window.M1M1;
    var se = window.P1P1;
    var sw = window.P1M1;

    TDistance metric = default;

    for (var py = 0; py < 4; ++py) {
      var row = dest + py * destStride;
      for (var px = 0; px < 4; ++px) {
        var fx = (px + 0.5f) / 4f;
        var fy = (py + 0.5f) / 4f;

        var result = CutHelpers.ProcessSubpixel<TWork, TKey, TDistance, TLerp>(
          metric,
          c.Work, c.Key,
          n.Work, n.Key, s.Work, s.Key,
          e.Work, e.Key, w.Work, w.Key,
          ne.Work, ne.Key, nw.Work, nw.Key,
          se.Work, se.Key, sw.Work, sw.Key,
          fx, fy, lerp);

        row[px] = encoder.Encode(result);
      }
    }
  }
}

#endregion
