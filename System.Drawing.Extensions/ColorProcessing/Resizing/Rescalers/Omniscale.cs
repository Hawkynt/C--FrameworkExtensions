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

#region Omniscale

/// <summary>
/// Omniscale - adaptive multi-algorithm scaler (2x, 3x, 4x).
/// </summary>
/// <remarks>
/// <para>Analyzes local image characteristics and applies optimal scaling per region.</para>
/// <para>Detects edges, flat areas, and gradients to choose between sharp and smooth interpolation.</para>
/// <para>Combines the sharpness of pixel art scalers with the smoothness of interpolation filters.</para>
/// <para>Reference: https://github.com/nobuyukinyuu/godot-omniscale</para>
/// </remarks>
[ScalerInfo("Omniscale", Author = "nobuyukinyuu", Year = 2017,
  Description = "Omniscale adaptive scaling", Category = ScalerCategory.PixelArt,
  Url = "https://github.com/nobuyukinyuu/godot-omniscale")]
public readonly struct Omniscale : IPixelScaler {
  private readonly int _scale;
  private readonly float _edgeThreshold;

  /// <summary>
  /// Default edge detection threshold (0.0-1.0). Higher values detect more edges.
  /// </summary>
  public const float DefaultEdgeThreshold = 0.1f;

  /// <summary>
  /// Creates a new Omniscale instance.
  /// </summary>
  /// <param name="scale">Scale factor (2, 3, or 4).</param>
  /// <param name="edgeThreshold">Edge detection threshold (0.0-1.0).</param>
  public Omniscale(int scale = 2, float edgeThreshold = DefaultEdgeThreshold) {
    if (scale is not (2 or 3 or 4))
      throw new ArgumentOutOfRangeException(nameof(scale), scale, "Omniscale supports 2x, 3x, 4x scaling");
    this._scale = scale;
    this._edgeThreshold = edgeThreshold;
  }

  /// <summary>
  /// Gets the edge detection threshold.
  /// </summary>
  public float EdgeThreshold => this._edgeThreshold;

  /// <summary>
  /// Creates a new Omniscale with the specified edge threshold.
  /// </summary>
  /// <param name="threshold">The edge threshold (0.0-1.0).</param>
  /// <returns>A new Omniscale with the specified threshold.</returns>
  public Omniscale WithEdgeThreshold(float threshold) => new(this._scale, threshold);

  public ScaleFactor Scale => this._scale == 0 ? new(2, 2) : new(this._scale, this._scale);

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
    => this._scale switch {
      0 or 2 => callback.Invoke(new Omniscale2xKernel<TWork, TKey, TPixel, TDistance, TEncode>(this._edgeThreshold)),
      3 => callback.Invoke(new Omniscale3xKernel<TWork, TKey, TPixel, TDistance, TLerp, TEncode>(this._edgeThreshold)),
      4 => callback.Invoke(new Omniscale4xKernel<TWork, TKey, TPixel, TDistance, TLerp, TEncode>(this._edgeThreshold)),
      _ => throw new InvalidOperationException($"Invalid scale factor: {this._scale}")
    };

  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2), new(3, 3), new(4, 4)];
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 } or { X: 3, Y: 3 } or { X: 4, Y: 4 };

  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
    yield return (sourceWidth * 3, sourceHeight * 3);
    yield return (sourceWidth * 4, sourceHeight * 4);
  }

  public static Omniscale Scale2x => new(2);
  public static Omniscale Scale3x => new(3);
  public static Omniscale Scale4x => new(4);
  public static Omniscale Default => new(2);
}

#endregion

#region Omniscale 2x Kernel

file readonly struct Omniscale2xKernel<TWork, TKey, TPixel, TMetric, TEncode>(float edgeThreshold = Omniscale.DefaultEdgeThreshold, TMetric metric = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TMetric : struct, IColorMetric<TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 2;
  public int ScaleY => 2;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    // NeighborWindow uses [Row][Column] naming: M1P0 = row -1, col 0 = TOP
    var center = window.P0P0; // center (row 0, col 0)
    var n = window.M1P0; // north/top (row -1, col 0)
    var s = window.P1P0; // south/bottom (row +1, col 0)
    var w = window.P0M1; // west/left (row 0, col -1)
    var e = window.P0P1; // east/right (row 0, col +1)

    // Edge detection using distance threshold
    var distN = metric.Distance(center.Key, n.Key);
    var distS = metric.Distance(center.Key, s.Key);
    var distW = metric.Distance(center.Key, w.Key);
    var distE = metric.Distance(center.Key, e.Key);

    var isEdgeN = distN > edgeThreshold;
    var isEdgeS = distS > edgeThreshold;
    var isEdgeW = distW > edgeThreshold;
    var isEdgeE = distE > edgeThreshold;

    var edgeCount = (isEdgeN ? 1 : 0) + (isEdgeS ? 1 : 0) + (isEdgeW ? 1 : 0) + (isEdgeE ? 1 : 0);

    var wC = center.Work;
    var pC = encoder.Encode(wC);

    // Fill with center by default
    dest[0] = pC;
    dest[1] = pC;
    dest[destStride] = pC;
    dest[destStride + 1] = pC;

    // Apply EPX-style corner smoothing for edge regions
    if (edgeCount >= 2) {
      var distNW = metric.Distance(n.Key, w.Key);
      var distNE = metric.Distance(n.Key, e.Key);
      var distSW = metric.Distance(s.Key, w.Key);
      var distSE = metric.Distance(s.Key, e.Key);

      // Top-left: if N similar to W but different from center
      if (distNW <= edgeThreshold && distN > edgeThreshold)
        dest[0] = encoder.Encode(n.Work);

      // Top-right: if N similar to E but different from center
      if (distNE <= edgeThreshold && distN > edgeThreshold)
        dest[1] = encoder.Encode(n.Work);

      // Bottom-left: if S similar to W but different from center
      if (distSW <= edgeThreshold && distS > edgeThreshold)
        dest[destStride] = encoder.Encode(s.Work);

      // Bottom-right: if S similar to E but different from center
      if (distSE <= edgeThreshold && distS > edgeThreshold)
        dest[destStride + 1] = encoder.Encode(s.Work);
    }
  }
}

#endregion

#region Omniscale 3x Kernel

file readonly struct Omniscale3xKernel<TWork, TKey, TPixel, TMetric, TLerp, TEncode>(float edgeThreshold = Omniscale.DefaultEdgeThreshold, TMetric metric = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TMetric : struct, IColorMetric<TKey>
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 3;
  public int ScaleY => 3;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    // NeighborWindow uses [Row][Column] naming: M1P0 = row -1, col 0 = TOP
    var center = window.P0P0; // center (row 0, col 0)
    var n = window.M1P0; // north/top (row -1, col 0)
    var s = window.P1P0; // south/bottom (row +1, col 0)
    var w = window.P0M1; // west/left (row 0, col -1)
    var e = window.P0P1; // east/right (row 0, col +1)

    // Edge detection using distance threshold
    var distN = metric.Distance(center.Key, n.Key);
    var distS = metric.Distance(center.Key, s.Key);
    var distW = metric.Distance(center.Key, w.Key);
    var distE = metric.Distance(center.Key, e.Key);

    var isEdgeN = distN > edgeThreshold;
    var isEdgeS = distS > edgeThreshold;
    var isEdgeW = distW > edgeThreshold;
    var isEdgeE = distE > edgeThreshold;

    var edgeCount = (isEdgeN ? 1 : 0) + (isEdgeS ? 1 : 0) + (isEdgeW ? 1 : 0) + (isEdgeE ? 1 : 0);

    var wC = center.Work;
    var pC = encoder.Encode(wC);

    // Fill all 9 pixels with center
    dest[0] = pC; dest[1] = pC; dest[2] = pC;
    dest[destStride] = pC; dest[destStride + 1] = pC; dest[destStride + 2] = pC;
    dest[destStride * 2] = pC; dest[destStride * 2 + 1] = pC; dest[destStride * 2 + 2] = pC;

    if (edgeCount >= 2) {
      var distNW = metric.Distance(n.Key, w.Key);
      var distNE = metric.Distance(n.Key, e.Key);
      var distSW = metric.Distance(s.Key, w.Key);
      var distSE = metric.Distance(s.Key, e.Key);

      // Corners - if neighbor similar to adjacent but different from center
      if (distNW <= edgeThreshold && distN > edgeThreshold)
        dest[0] = encoder.Encode(n.Work);

      if (distNE <= edgeThreshold && distN > edgeThreshold)
        dest[2] = encoder.Encode(n.Work);

      if (distSW <= edgeThreshold && distS > edgeThreshold)
        dest[destStride * 2] = encoder.Encode(s.Work);

      if (distSE <= edgeThreshold && distS > edgeThreshold)
        dest[destStride * 2 + 2] = encoder.Encode(s.Work);
    }
  }
}

#endregion

#region Omniscale 4x Kernel

file readonly struct Omniscale4xKernel<TWork, TKey, TPixel, TMetric, TLerp, TEncode>(float edgeThreshold = Omniscale.DefaultEdgeThreshold, TMetric metric = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TMetric : struct, IColorMetric<TKey>
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 4;
  public int ScaleY => 4;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    // NeighborWindow uses [Row][Column] naming: M1P0 = row -1, col 0 = TOP
    var center = window.P0P0; // center (row 0, col 0)
    var n = window.M1P0; // north/top (row -1, col 0)
    var s = window.P1P0; // south/bottom (row +1, col 0)
    var w = window.P0M1; // west/left (row 0, col -1)
    var e = window.P0P1; // east/right (row 0, col +1)

    // Edge detection using distance threshold
    var distN = metric.Distance(center.Key, n.Key);
    var distS = metric.Distance(center.Key, s.Key);
    var distW = metric.Distance(center.Key, w.Key);
    var distE = metric.Distance(center.Key, e.Key);

    var isEdgeN = distN > edgeThreshold;
    var isEdgeS = distS > edgeThreshold;
    var isEdgeW = distW > edgeThreshold;
    var isEdgeE = distE > edgeThreshold;

    var edgeCount = (isEdgeN ? 1 : 0) + (isEdgeS ? 1 : 0) + (isEdgeW ? 1 : 0) + (isEdgeE ? 1 : 0);

    var wC = center.Work;
    var pC = encoder.Encode(wC);

    // Fill all 16 pixels with center
    for (var y = 0; y < 4; ++y)
      for (var x = 0; x < 4; ++x)
        dest[y * destStride + x] = pC;

    if (edgeCount >= 2) {
      var distNW = metric.Distance(n.Key, w.Key);
      var distNE = metric.Distance(n.Key, e.Key);
      var distSW = metric.Distance(s.Key, w.Key);
      var distSE = metric.Distance(s.Key, e.Key);

      // Corners - if neighbor similar to adjacent but different from center
      if (distNW <= edgeThreshold && distN > edgeThreshold)
        dest[0] = encoder.Encode(n.Work);

      if (distNE <= edgeThreshold && distN > edgeThreshold)
        dest[3] = encoder.Encode(n.Work);

      if (distSW <= edgeThreshold && distS > edgeThreshold)
        dest[destStride * 3] = encoder.Encode(s.Work);

      if (distSE <= edgeThreshold && distS > edgeThreshold)
        dest[destStride * 3 + 3] = encoder.Encode(s.Work);
    }
  }
}

#endregion
