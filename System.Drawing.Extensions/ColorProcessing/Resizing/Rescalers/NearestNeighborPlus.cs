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
/// Nearest Neighbor Plus (NN+) - enhanced nearest-neighbor with edge detection.
/// </summary>
/// <remarks>
/// <para>NN+ preserves the sharp look of pixel art while reducing jaggies on diagonal lines.</para>
/// <para>Uses edge detection to smooth diagonal edges via controlled blending at corners.</para>
/// <para>A good compromise between pure nearest-neighbor and more complex scalers like Scale2x.</para>
/// </remarks>
[ScalerInfo("NN+", Author = "Unknown",
  Description = "Enhanced nearest-neighbor with diagonal smoothing", Category = ScalerCategory.PixelArt)]
public readonly struct NearestNeighborPlus : IPixelScaler {
  private readonly int _scale;

  /// <summary>
  /// Creates a NN+ scaler with the specified factor.
  /// </summary>
  /// <param name="scale">Scale factor (2, 3, or 4).</param>
  public NearestNeighborPlus(int scale = 2) {
    if (scale is < 2 or > 4)
      throw new ArgumentOutOfRangeException(nameof(scale), scale, "NN+ supports 2x, 3x, 4x scaling");
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
      0 or 2 => callback.Invoke(new NearestNeighborPlus2xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      3 => callback.Invoke(new NearestNeighborPlus3xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      4 => callback.Invoke(new NearestNeighborPlus4xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
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

  /// <summary>Gets a 2x NN+ scaler.</summary>
  public static NearestNeighborPlus X2 => new(2);

  /// <summary>Gets a 3x NN+ scaler.</summary>
  public static NearestNeighborPlus X3 => new(3);

  /// <summary>Gets a 4x NN+ scaler.</summary>
  public static NearestNeighborPlus X4 => new(4);

  /// <summary>Gets the default NN+ scaler (2x).</summary>
  public static NearestNeighborPlus Default => X2;

  #endregion
}

#region NN+ 2x Kernel

file readonly struct NearestNeighborPlus2xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(
  TEquality equality = default,
  TLerp lerp = default)
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
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var p = window.P0P0;
    var tl = window.M1M1;
    var t = window.P0M1;
    var tr = window.P1M1;
    var l = window.M1P0;
    var r = window.P1P0;
    var bl = window.M1P1;
    var b = window.P0P1;
    var br = window.P1P1;

    var e00 = p.Work;
    var e01 = p.Work;
    var e10 = p.Work;
    var e11 = p.Work;

    // Top-left corner: check if we should smooth
    if (equality.Equals(tl.Key, t.Key) && equality.Equals(tl.Key, l.Key) && !equality.Equals(tl.Key, p.Key))
      e00 = lerp.Lerp(p.Work, tl.Work, 3, 1);
    else if (equality.Equals(t.Key, l.Key) && !equality.Equals(t.Key, p.Key))
      e00 = lerp.Lerp(p.Work, t.Work, 7, 1);

    // Top-right corner
    if (equality.Equals(tr.Key, t.Key) && equality.Equals(tr.Key, r.Key) && !equality.Equals(tr.Key, p.Key))
      e01 = lerp.Lerp(p.Work, tr.Work, 3, 1);
    else if (equality.Equals(t.Key, r.Key) && !equality.Equals(t.Key, p.Key))
      e01 = lerp.Lerp(p.Work, t.Work, 7, 1);

    // Bottom-left corner
    if (equality.Equals(bl.Key, b.Key) && equality.Equals(bl.Key, l.Key) && !equality.Equals(bl.Key, p.Key))
      e10 = lerp.Lerp(p.Work, bl.Work, 3, 1);
    else if (equality.Equals(b.Key, l.Key) && !equality.Equals(b.Key, p.Key))
      e10 = lerp.Lerp(p.Work, b.Work, 7, 1);

    // Bottom-right corner
    if (equality.Equals(br.Key, b.Key) && equality.Equals(br.Key, r.Key) && !equality.Equals(br.Key, p.Key))
      e11 = lerp.Lerp(p.Work, br.Work, 3, 1);
    else if (equality.Equals(b.Key, r.Key) && !equality.Equals(b.Key, p.Key))
      e11 = lerp.Lerp(p.Work, b.Work, 7, 1);

    dest[0] = encoder.Encode(e00);
    dest[1] = encoder.Encode(e01);
    dest[destStride] = encoder.Encode(e10);
    dest[destStride + 1] = encoder.Encode(e11);
  }
}

#endregion

#region NN+ 3x Kernel

file readonly struct NearestNeighborPlus3xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(
  TEquality equality = default,
  TLerp lerp = default)
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
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var p = window.P0P0;
    var tl = window.M1M1;
    var t = window.P0M1;
    var tr = window.P1M1;
    var l = window.M1P0;
    var r = window.P1P0;
    var bl = window.M1P1;
    var b = window.P0P1;
    var br = window.P1P1;

    // Pre-encode center
    var pc = encoder.Encode(p.Work);

    // Initialize 3x3 grid with center pixel
    var e00 = p.Work;
    var e01 = p.Work;
    var e02 = p.Work;
    var e10 = p.Work;
    var e12 = p.Work;
    var e20 = p.Work;
    var e21 = p.Work;
    var e22 = p.Work;

    // Corners
    if (equality.Equals(tl.Key, t.Key) && equality.Equals(tl.Key, l.Key) && !equality.Equals(tl.Key, p.Key))
      e00 = lerp.Lerp(p.Work, tl.Work, 2, 1);
    if (equality.Equals(tr.Key, t.Key) && equality.Equals(tr.Key, r.Key) && !equality.Equals(tr.Key, p.Key))
      e02 = lerp.Lerp(p.Work, tr.Work, 2, 1);
    if (equality.Equals(bl.Key, b.Key) && equality.Equals(bl.Key, l.Key) && !equality.Equals(bl.Key, p.Key))
      e20 = lerp.Lerp(p.Work, bl.Work, 2, 1);
    if (equality.Equals(br.Key, b.Key) && equality.Equals(br.Key, r.Key) && !equality.Equals(br.Key, p.Key))
      e22 = lerp.Lerp(p.Work, br.Work, 2, 1);

    // Edges - top/bottom
    if (equality.Equals(t.Key, l.Key) && !equality.Equals(t.Key, p.Key))
      e01 = lerp.Lerp(p.Work, t.Work, 3, 1);
    else if (equality.Equals(t.Key, r.Key) && !equality.Equals(t.Key, p.Key))
      e01 = lerp.Lerp(p.Work, t.Work, 3, 1);

    if (equality.Equals(b.Key, l.Key) && !equality.Equals(b.Key, p.Key))
      e21 = lerp.Lerp(p.Work, b.Work, 3, 1);
    else if (equality.Equals(b.Key, r.Key) && !equality.Equals(b.Key, p.Key))
      e21 = lerp.Lerp(p.Work, b.Work, 3, 1);

    // Edges - left/right
    if (equality.Equals(l.Key, t.Key) && !equality.Equals(l.Key, p.Key))
      e10 = lerp.Lerp(p.Work, l.Work, 3, 1);
    else if (equality.Equals(l.Key, b.Key) && !equality.Equals(l.Key, p.Key))
      e10 = lerp.Lerp(p.Work, l.Work, 3, 1);

    if (equality.Equals(r.Key, t.Key) && !equality.Equals(r.Key, p.Key))
      e12 = lerp.Lerp(p.Work, r.Work, 3, 1);
    else if (equality.Equals(r.Key, b.Key) && !equality.Equals(r.Key, p.Key))
      e12 = lerp.Lerp(p.Work, r.Work, 3, 1);

    // Write output
    dest[0] = encoder.Encode(e00);
    dest[1] = encoder.Encode(e01);
    dest[2] = encoder.Encode(e02);
    var row1 = dest + destStride;
    row1[0] = encoder.Encode(e10);
    row1[1] = pc;
    row1[2] = encoder.Encode(e12);
    var row2 = row1 + destStride;
    row2[0] = encoder.Encode(e20);
    row2[1] = encoder.Encode(e21);
    row2[2] = encoder.Encode(e22);
  }
}

#endregion

#region NN+ 4x Kernel

file readonly struct NearestNeighborPlus4xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(
  TEquality equality = default,
  TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
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
    var p = window.P0P0;
    var tl = window.M1M1;
    var t = window.P0M1;
    var tr = window.P1M1;
    var l = window.M1P0;
    var r = window.P1P0;
    var bl = window.M1P1;
    var b = window.P0P1;
    var br = window.P1P1;

    // Pre-encode center
    var pc = encoder.Encode(p.Work);

    // Fill entire 4x4 with center first
    for (var dy = 0; dy < 4; ++dy) {
      var row = dest + dy * destStride;
      row[0] = pc;
      row[1] = pc;
      row[2] = pc;
      row[3] = pc;
    }

    // Corner blending - top-left
    if (equality.Equals(tl.Key, t.Key) && equality.Equals(tl.Key, l.Key) && !equality.Equals(tl.Key, p.Key)) {
      dest[0] = encoder.Encode(lerp.Lerp(p.Work, tl.Work, 1, 1));
      dest[1] = encoder.Encode(lerp.Lerp(p.Work, tl.Work, 3, 1));
      dest[destStride] = encoder.Encode(lerp.Lerp(p.Work, tl.Work, 3, 1));
    }

    // Corner blending - top-right
    if (equality.Equals(tr.Key, t.Key) && equality.Equals(tr.Key, r.Key) && !equality.Equals(tr.Key, p.Key)) {
      dest[3] = encoder.Encode(lerp.Lerp(p.Work, tr.Work, 1, 1));
      dest[2] = encoder.Encode(lerp.Lerp(p.Work, tr.Work, 3, 1));
      dest[destStride + 3] = encoder.Encode(lerp.Lerp(p.Work, tr.Work, 3, 1));
    }

    // Corner blending - bottom-left
    var row3 = dest + 3 * destStride;
    var row2 = dest + 2 * destStride;
    if (equality.Equals(bl.Key, b.Key) && equality.Equals(bl.Key, l.Key) && !equality.Equals(bl.Key, p.Key)) {
      row3[0] = encoder.Encode(lerp.Lerp(p.Work, bl.Work, 1, 1));
      row3[1] = encoder.Encode(lerp.Lerp(p.Work, bl.Work, 3, 1));
      row2[0] = encoder.Encode(lerp.Lerp(p.Work, bl.Work, 3, 1));
    }

    // Corner blending - bottom-right
    if (equality.Equals(br.Key, b.Key) && equality.Equals(br.Key, r.Key) && !equality.Equals(br.Key, p.Key)) {
      row3[3] = encoder.Encode(lerp.Lerp(p.Work, br.Work, 1, 1));
      row3[2] = encoder.Encode(lerp.Lerp(p.Work, br.Work, 3, 1));
      row2[3] = encoder.Encode(lerp.Lerp(p.Work, br.Work, 3, 1));
    }
  }
}

#endregion
