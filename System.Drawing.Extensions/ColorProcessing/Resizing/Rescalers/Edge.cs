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
/// ScummVM Edge scaler - simple edge enhancement during scaling.
/// </summary>
/// <remarks>
/// <para>Detects edges by comparing corner pixels and fills diagonals accordingly.</para>
/// <para>When corners match but are different from center, creates diagonal fills for smoother edges.</para>
/// <para>A simple pixel-art scaler that improves diagonal appearance without complex algorithms.</para>
/// <para>Reference: ScummVM project (https://wiki.scummvm.org/index.php/Scalers)</para>
/// </remarks>
[ScalerInfo("Edge", Author = "ScummVM Team", Year = 2001,
  Url = "https://wiki.scummvm.org/index.php/Scalers",
  Description = "Simple edge enhancement for smoother diagonals", Category = ScalerCategory.PixelArt)]
public readonly struct Edge : IPixelScaler {
  private readonly int _scale;

  /// <summary>
  /// Creates an Edge scaler with the specified factor.
  /// </summary>
  /// <param name="scale">Scale factor (2 or 3).</param>
  public Edge(int scale = 2) {
    if (scale is < 2 or > 3)
      throw new ArgumentOutOfRangeException(nameof(scale), scale, "Edge supports 2x, 3x scaling");
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
      0 or 2 => callback.Invoke(new Edge2xKernel<TWork, TKey, TPixel, TEquality, TEncode>(equality)),
      3 => callback.Invoke(new Edge3xKernel<TWork, TKey, TPixel, TEquality, TEncode>(equality)),
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

  /// <summary>Gets a 2x Edge scaler.</summary>
  public static Edge X2 => new(2);

  /// <summary>Gets a 3x Edge scaler.</summary>
  public static Edge X3 => new(3);

  /// <summary>Gets the default Edge scaler (2x).</summary>
  public static Edge Default => X2;

  #endregion
}

#region Edge 2x Kernel

file readonly struct Edge2xKernel<TWork, TKey, TPixel, TEquality, TEncode>(TEquality equality = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 2;
  public int ScaleY => 2;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    // Get the center pixel and neighbors
    var center = window.P0P0;
    var topLeft = window.M1M1;
    var top = window.P0M1;
    var topRight = window.P1M1;
    var left = window.M1P0;
    var right = window.P1P0;
    var bottomLeft = window.M1P1;
    var bottom = window.P0P1;
    var bottomRight = window.P1P1;

    // Default: all output pixels get center
    var e00 = center.Work;
    var e01 = center.Work;
    var e10 = center.Work;
    var e11 = center.Work;

    // Top-left: edge detection
    if (!equality.Equals(topLeft.Key, center.Key) && !equality.Equals(top.Key, center.Key) && !equality.Equals(left.Key, center.Key))
      if (equality.Equals(top.Key, left.Key))
        e00 = top.Work;

    // Top-right
    if (!equality.Equals(topRight.Key, center.Key) && !equality.Equals(top.Key, center.Key) && !equality.Equals(right.Key, center.Key))
      if (equality.Equals(top.Key, right.Key))
        e01 = top.Work;

    // Bottom-left
    if (!equality.Equals(bottomLeft.Key, center.Key) && !equality.Equals(bottom.Key, center.Key) && !equality.Equals(left.Key, center.Key))
      if (equality.Equals(bottom.Key, left.Key))
        e10 = bottom.Work;

    // Bottom-right
    if (!equality.Equals(bottomRight.Key, center.Key) && !equality.Equals(bottom.Key, center.Key) && !equality.Equals(right.Key, center.Key))
      if (equality.Equals(bottom.Key, right.Key))
        e11 = bottom.Work;

    // Write output
    dest[0] = encoder.Encode(e00);
    dest[1] = encoder.Encode(e01);
    dest[destStride] = encoder.Encode(e10);
    dest[destStride + 1] = encoder.Encode(e11);
  }
}

#endregion

#region Edge 3x Kernel

file readonly struct Edge3xKernel<TWork, TKey, TPixel, TEquality, TEncode>(TEquality equality = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 3;
  public int ScaleY => 3;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    // Get neighbors
    var center = window.P0P0;
    var topLeft = window.M1M1;
    var top = window.P0M1;
    var topRight = window.P1M1;
    var left = window.M1P0;
    var right = window.P1P0;
    var bottomLeft = window.M1P1;
    var bottom = window.P0P1;
    var bottomRight = window.P1P1;

    // Pre-encode center pixel
    var pc = encoder.Encode(center.Work);

    // Fill 3x3 with center
    dest[0] = pc;
    dest[1] = pc;
    dest[2] = pc;
    var row1 = dest + destStride;
    row1[0] = pc;
    row1[1] = pc;
    row1[2] = pc;
    var row2 = row1 + destStride;
    row2[0] = pc;
    row2[1] = pc;
    row2[2] = pc;

    // Apply edge enhancements at corners
    if (equality.Equals(top.Key, left.Key) && !equality.Equals(topLeft.Key, center.Key))
      dest[0] = encoder.Encode(top.Work);

    if (equality.Equals(top.Key, right.Key) && !equality.Equals(topRight.Key, center.Key))
      dest[2] = encoder.Encode(top.Work);

    if (equality.Equals(bottom.Key, left.Key) && !equality.Equals(bottomLeft.Key, center.Key))
      row2[0] = encoder.Encode(bottom.Work);

    if (equality.Equals(bottom.Key, right.Key) && !equality.Equals(bottomRight.Key, center.Key))
      row2[2] = encoder.Encode(bottom.Work);
  }
}

#endregion
