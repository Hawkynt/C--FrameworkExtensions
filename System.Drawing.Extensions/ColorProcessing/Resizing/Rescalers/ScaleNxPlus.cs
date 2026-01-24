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
/// Scale2x/Scale3x Plus variants with subpixel interpolation.
/// </summary>
/// <remarks>
/// <para>Reference: guest(r) 2007 (guest.r@gmail.com)</para>
/// <para>Based on: Andrea Mazzoleni's Scale2x (scale2x.sourceforge.net)</para>
/// <para>See: https://github.com/libretro/common-shaders/tree/master/scalenx</para>
/// <para>Combines Scale2x edge detection with bilinear interpolation for smooth subpixel transitions.</para>
/// </remarks>
[ScalerInfo("ScaleNxPlus", Author = "guest(r)", Year = 2007,
  Url = "https://github.com/libretro/common-shaders/tree/master/scalenx",
  Description = "ScaleNx with subpixel bilinear interpolation", Category = ScalerCategory.PixelArt)]
public readonly struct ScaleNxPlus : IPixelScaler {
  private readonly int _scale;

  /// <summary>
  /// Creates a ScaleNxPlus scaler with the specified factor.
  /// </summary>
  /// <param name="scale">Scale factor (2 or 3).</param>
  public ScaleNxPlus(int scale = 2) {
    if (scale is < 2 or > 3)
      throw new ArgumentOutOfRangeException(nameof(scale), scale, "ScaleNxPlus supports 2x or 3x scaling");
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
      0 or 2 => callback.Invoke(new Scale2xPlusKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      3 => callback.Invoke(new Scale3xPlusKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
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

  /// <summary>Gets a 2x ScaleNxPlus scaler.</summary>
  public static ScaleNxPlus X2 => new(2);

  /// <summary>Gets a 3x ScaleNxPlus scaler.</summary>
  public static ScaleNxPlus X3 => new(3);

  /// <summary>Gets the default ScaleNxPlus scaler (2x).</summary>
  public static ScaleNxPlus Default => X2;

  #endregion
}

#region ScaleNxPlus Helpers

file static class ScaleNxPlusHelpers {
  /// <summary>Scale factor for converting float weights to integer weights.</summary>
  public const int WeightScale = 256;

  /// <summary>
  /// Bilinear interpolation between 4 colors.
  /// </summary>
  /// <remarks>
  /// Formula: (e3*fx + e2*(1-fx))*fy + (e1*fx + e0*(1-fx))*(1-fy)
  /// Weights: w0=(1-fx)*(1-fy), w1=fx*(1-fy), w2=(1-fx)*fy, w3=fx*fy
  /// Layout: e0 e1
  ///         e2 e3
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TWork BilinearInterpolate<TWork, TLerp>(
    in TLerp lerp,
    in TWork e0, in TWork e1, in TWork e2, in TWork e3,
    float fx, float fy)
    where TWork : unmanaged, IColorSpace
    where TLerp : struct, ILerp<TWork> {
    // Convert to integer weights (0-256 scale)
    var ifx = (int)(fx * WeightScale);
    var ify = (int)(fy * WeightScale);
    var ifx1 = WeightScale - ifx;
    var ify1 = WeightScale - ify;

    // Horizontal interpolation of top row and bottom row
    var top = lerp.Lerp(e0, e1, ifx1, ifx);
    var bottom = lerp.Lerp(e2, e3, ifx1, ifx);

    // Vertical interpolation
    return lerp.Lerp(top, bottom, ify1, ify);
  }
}

#endregion

#region Scale2xPlus Kernel

file readonly struct Scale2xPlusKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(
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
    // Get the 5 relevant pixels: B(top), D(left), E(center), F(right), H(bottom)
    var b = window.M1P0;
    var d = window.P0M1;
    var e = window.P0P0;
    var f = window.P0P1;
    var h = window.P1P0;

    // Pre-compute comparisons
    var bh = equality.Equals(b.Key, h.Key);
    var df = equality.Equals(d.Key, f.Key);
    var db = equality.Equals(d.Key, b.Key);
    var bf = equality.Equals(b.Key, f.Key);
    var dh = equality.Equals(d.Key, h.Key);
    var hf = equality.Equals(h.Key, f.Key);

    // Apply Scale2x rules to get 4 candidates
    var e0 = db && !bh && !df ? d.Work : e.Work;
    var e1 = bf && !bh && !df ? f.Work : e.Work;
    var e2 = dh && !bh && !df ? d.Work : e.Work;
    var e3 = hf && !bh && !df ? f.Work : e.Work;

    // Bilinear interpolation at 4 subpixel positions
    dest[0] = encoder.Encode(ScaleNxPlusHelpers.BilinearInterpolate(lerp, e0, e1, e2, e3, 0.25f, 0.25f));
    dest[1] = encoder.Encode(ScaleNxPlusHelpers.BilinearInterpolate(lerp, e0, e1, e2, e3, 0.75f, 0.25f));
    dest[destStride] = encoder.Encode(ScaleNxPlusHelpers.BilinearInterpolate(lerp, e0, e1, e2, e3, 0.25f, 0.75f));
    dest[destStride + 1] = encoder.Encode(ScaleNxPlusHelpers.BilinearInterpolate(lerp, e0, e1, e2, e3, 0.75f, 0.75f));
  }
}

#endregion

#region Scale3xPlus Kernel

file readonly struct Scale3xPlusKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(
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
    // Get the 9 relevant pixels
    var a = window.M1M1;
    var b = window.M1P0;
    var c = window.M1P1;
    var d = window.P0M1;
    var e = window.P0P0;
    var f = window.P0P1;
    var g = window.P1M1;
    var h = window.P1P0;
    var i = window.P1P1;

    // Pre-compute comparisons
    var bh = equality.Equals(b.Key, h.Key);
    var df = equality.Equals(d.Key, f.Key);

    TWork e0, e1, e2, e3, e4, e5, e6, e7, e8;

    // Apply Scale3x rules to get 9 candidates
    if (!bh && !df) {
      var db = equality.Equals(d.Key, b.Key);
      var bf = equality.Equals(b.Key, f.Key);
      var dh = equality.Equals(d.Key, h.Key);
      var hf = equality.Equals(h.Key, f.Key);
      var ec = equality.Equals(e.Key, c.Key);
      var ea = equality.Equals(e.Key, a.Key);
      var eg = equality.Equals(e.Key, g.Key);
      var ei = equality.Equals(e.Key, i.Key);

      e0 = db ? d.Work : e.Work;
      e1 = db && !ec || bf && !ea ? b.Work : e.Work;
      e2 = bf ? f.Work : e.Work;
      e3 = db && !eg || dh && !ea ? d.Work : e.Work;
      e4 = e.Work;
      e5 = bf && !ei || hf && !ec ? f.Work : e.Work;
      e6 = dh ? d.Work : e.Work;
      e7 = dh && !ei || hf && !eg ? h.Work : e.Work;
      e8 = hf ? f.Work : e.Work;
    } else {
      e0 = e1 = e2 = e3 = e4 = e5 = e6 = e7 = e8 = e.Work;
    }

    // Fractional positions for 3x grid
    const float f1 = 1f / 6f;
    const float f5 = 5f / 6f;

    // Place 9 output pixels with subpixel interpolation
    dest[0] = encoder.Encode(ScaleNxPlusHelpers.BilinearInterpolate(lerp, e0, e1, e3, e4, f1, f1));
    dest[1] = encoder.Encode(ScaleNxPlusHelpers.BilinearInterpolate(lerp, e0, e2, e3, e5, 0.5f, f1));
    dest[2] = encoder.Encode(ScaleNxPlusHelpers.BilinearInterpolate(lerp, e1, e2, e4, e5, f5, f1));
    dest[destStride] = encoder.Encode(ScaleNxPlusHelpers.BilinearInterpolate(lerp, e0, e1, e6, e7, f1, 0.5f));
    dest[destStride + 1] = encoder.Encode(e4);
    dest[destStride + 2] = encoder.Encode(ScaleNxPlusHelpers.BilinearInterpolate(lerp, e1, e2, e7, e8, f5, 0.5f));
    dest[2 * destStride] = encoder.Encode(ScaleNxPlusHelpers.BilinearInterpolate(lerp, e3, e4, e6, e7, f1, f5));
    dest[2 * destStride + 1] = encoder.Encode(ScaleNxPlusHelpers.BilinearInterpolate(lerp, e3, e5, e6, e8, 0.5f, f5));
    dest[2 * destStride + 2] = encoder.Encode(ScaleNxPlusHelpers.BilinearInterpolate(lerp, e4, e5, e7, e8, f5, f5));
  }
}

#endregion
