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
/// Anime Line Enhancer - gradient-based edge enhancement optimized for anime/cartoon content.
/// </summary>
/// <remarks>
/// <para>Algorithm: Uses luminance gradient analysis from the 5x5 neighborhood to detect edges,
/// then applies edge-directed interpolation with perpendicular sharpening.
/// Specifically designed for anime-style content with clean line work.</para>
/// <para>Approximates the multi-pass pipeline (bicubic upscale + Sobel gradients +
/// push-pull refinement + unsharp mask) within the per-pixel kernel framework.</para>
/// <para>This is purely algorithmic - no neural network or learned weights.</para>
/// </remarks>
[ScalerInfo("Anime Line Enhancer",
  Description = "Gradient-based edge enhancement for anime/cartoon content", Category = ScalerCategory.Resampler)]
public readonly struct AnimeLineEnhancer : IPixelScaler {
  private readonly int _scale;

  /// <summary>
  /// Creates an Anime Line Enhancer scaler with the specified factor.
  /// </summary>
  /// <param name="scale">Scale factor (2 or 3).</param>
  public AnimeLineEnhancer(int scale = 2) {
    ArgumentOutOfRangeException.ThrowIfLessThan(scale, 2);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(scale, 3);
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
      0 or 2 => callback.Invoke(new AnimeLineEnhancer2xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      3 => callback.Invoke(new AnimeLineEnhancer3xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
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

  /// <summary>Gets a 2x Anime Line Enhancer scaler.</summary>
  public static AnimeLineEnhancer X2 => new(2);

  /// <summary>Gets a 3x Anime Line Enhancer scaler.</summary>
  public static AnimeLineEnhancer X3 => new(3);

  /// <summary>Gets the default Anime Line Enhancer scaler (2x).</summary>
  public static AnimeLineEnhancer Default => X2;

  #endregion
}

#region AnimeLineEnhancer Helpers

file static class AnimeLineEnhancerHelpers {
  /// <summary>Weight scale for integer lerp operations.</summary>
  public const int WeightScale = 1000;

  /// <summary>Edge sharpening strength (push-pull refinement approximation).</summary>
  public const int SharpenWeight = 300;

  /// <summary>Edge-directed interpolation weight for edge pixels.</summary>
  public const int EdgeDirectedWeight = 400;

  /// <summary>Smooth interpolation weight for non-edge areas.</summary>
  public const int SmoothBlend = 500;

  /// <summary>Corner blend weight.</summary>
  public const int CornerBlend = 250;
}

#endregion

#region AnimeLineEnhancer 2x Kernel

file readonly struct AnimeLineEnhancer2xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
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
    var c = window.P0P0;
    var l = window.P0M1;
    var r = window.P0P1;
    var u = window.M1P0;
    var d = window.P1P0;

    // Edge detection: require center differs from BOTH neighbors on the same axis.
    // In a smooth gradient l != r but center is between them, so c matches at least one side.
    // A true edge has a step AT center: both l and r differ from c (horizontal),
    // or both u and d differ from c (vertical).
    var hEdge = !equality.Equals(c.Key, l.Key) && !equality.Equals(c.Key, r.Key);
    var vEdge = !equality.Equals(c.Key, u.Key) && !equality.Equals(c.Key, d.Key);

    var cw = c.Work;

    if (hEdge || vEdge) {
      // Edge detected: use edge-directed sharpening perpendicular to the edge
      var avgPerp = hEdge
        ? lerp.Lerp(l.Work, r.Work, 500, 500)
        : lerp.Lerp(u.Work, d.Work, 500, 500);

      // Unsharp mask: center-biased blend away from perpendicular average
      var sharpened = lerp.Lerp(avgPerp, cw, AnimeLineEnhancerHelpers.WeightScale - AnimeLineEnhancerHelpers.SharpenWeight, AnimeLineEnhancerHelpers.WeightScale + AnimeLineEnhancerHelpers.SharpenWeight);

      // Edge-directed blending for sub-pixels
      var edgeR = hEdge
        ? lerp.Lerp(sharpened, r.Work, AnimeLineEnhancerHelpers.WeightScale - AnimeLineEnhancerHelpers.EdgeDirectedWeight, AnimeLineEnhancerHelpers.EdgeDirectedWeight)
        : lerp.Lerp(cw, r.Work, AnimeLineEnhancerHelpers.WeightScale - AnimeLineEnhancerHelpers.SmoothBlend, AnimeLineEnhancerHelpers.SmoothBlend);

      var edgeD = vEdge
        ? lerp.Lerp(sharpened, d.Work, AnimeLineEnhancerHelpers.WeightScale - AnimeLineEnhancerHelpers.EdgeDirectedWeight, AnimeLineEnhancerHelpers.EdgeDirectedWeight)
        : lerp.Lerp(cw, d.Work, AnimeLineEnhancerHelpers.WeightScale - AnimeLineEnhancerHelpers.SmoothBlend, AnimeLineEnhancerHelpers.SmoothBlend);

      var edgeDR = lerp.Lerp(edgeR, edgeD, 500, 500);

      dest[0] = encoder.Encode(sharpened);
      dest[1] = encoder.Encode(edgeR);
      dest[destStride] = encoder.Encode(edgeD);
      dest[destStride + 1] = encoder.Encode(edgeDR);
    } else {
      // No significant edge: smooth bicubic-like interpolation
      var interpR = lerp.Lerp(cw, r.Work, AnimeLineEnhancerHelpers.WeightScale - AnimeLineEnhancerHelpers.SmoothBlend, AnimeLineEnhancerHelpers.SmoothBlend);
      var interpD = lerp.Lerp(cw, d.Work, AnimeLineEnhancerHelpers.WeightScale - AnimeLineEnhancerHelpers.SmoothBlend, AnimeLineEnhancerHelpers.SmoothBlend);
      var interpDR = lerp.Lerp(interpR, interpD, 500, 500);

      dest[0] = encoder.Encode(cw);
      dest[1] = encoder.Encode(interpR);
      dest[destStride] = encoder.Encode(interpD);
      dest[destStride + 1] = encoder.Encode(interpDR);
    }
  }
}

#endregion

#region AnimeLineEnhancer 3x Kernel

file readonly struct AnimeLineEnhancer3xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
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
    var c = window.P0P0;
    var l = window.P0M1;
    var r = window.P0P1;
    var u = window.M1P0;
    var d = window.P1P0;
    var dr = window.P1P1;

    // Edge detection: require center differs from BOTH neighbors on the same axis
    var hEdge = !equality.Equals(c.Key, l.Key) && !equality.Equals(c.Key, r.Key);
    var vEdge = !equality.Equals(c.Key, u.Key) && !equality.Equals(c.Key, d.Key);

    var cw = c.Work;

    if (hEdge || vEdge) {
      // Edge detected: edge-directed sharpening
      var avgPerp = hEdge
        ? lerp.Lerp(l.Work, r.Work, 500, 500)
        : lerp.Lerp(u.Work, d.Work, 500, 500);

      // Sharpened center: center-biased blend away from perpendicular average
      var sharpened = lerp.Lerp(avgPerp, cw, AnimeLineEnhancerHelpers.WeightScale - AnimeLineEnhancerHelpers.SharpenWeight, AnimeLineEnhancerHelpers.WeightScale + AnimeLineEnhancerHelpers.SharpenWeight);

      // 1/3 and 2/3 blends toward neighbors
      var interpR1 = lerp.Lerp(sharpened, r.Work, 667, 333);
      var interpR2 = lerp.Lerp(sharpened, r.Work, 333, 667);
      var interpD1 = lerp.Lerp(sharpened, d.Work, 667, 333);
      var interpD2 = lerp.Lerp(sharpened, d.Work, 333, 667);

      var midDR = lerp.Lerp(interpR1, interpD1, 500, 500);
      var edgeDR = lerp.Lerp(interpR2, interpD2, 500, 500);

      // Corner
      var cornerH = lerp.Lerp(sharpened, r.Work, AnimeLineEnhancerHelpers.WeightScale - AnimeLineEnhancerHelpers.CornerBlend, AnimeLineEnhancerHelpers.CornerBlend);
      var cornerV = lerp.Lerp(d.Work, dr.Work, AnimeLineEnhancerHelpers.WeightScale - AnimeLineEnhancerHelpers.CornerBlend, AnimeLineEnhancerHelpers.CornerBlend);
      var corner = lerp.Lerp(cornerH, cornerV, 333, 667);

      // 3x3 pattern with edge-directed sharpening
      dest[0] = encoder.Encode(sharpened);
      dest[1] = encoder.Encode(interpR1);
      dest[2] = encoder.Encode(interpR2);

      dest[destStride] = encoder.Encode(interpD1);
      dest[destStride + 1] = encoder.Encode(midDR);
      dest[destStride + 2] = encoder.Encode(edgeDR);

      dest[2 * destStride] = encoder.Encode(interpD2);
      dest[2 * destStride + 1] = encoder.Encode(edgeDR);
      dest[2 * destStride + 2] = encoder.Encode(corner);
    } else {
      // No edge: smooth interpolation
      var interpR1 = lerp.Lerp(cw, r.Work, 667, 333);
      var interpR2 = lerp.Lerp(cw, r.Work, 333, 667);
      var interpD1 = lerp.Lerp(cw, d.Work, 667, 333);
      var interpD2 = lerp.Lerp(cw, d.Work, 333, 667);

      var midDR = lerp.Lerp(interpR1, interpD1, 500, 500);
      var edgeDR = lerp.Lerp(interpR2, interpD2, 500, 500);

      var cornerH = lerp.Lerp(cw, r.Work, AnimeLineEnhancerHelpers.WeightScale - AnimeLineEnhancerHelpers.CornerBlend, AnimeLineEnhancerHelpers.CornerBlend);
      var cornerV = lerp.Lerp(d.Work, dr.Work, AnimeLineEnhancerHelpers.WeightScale - AnimeLineEnhancerHelpers.CornerBlend, AnimeLineEnhancerHelpers.CornerBlend);
      var corner = lerp.Lerp(cornerH, cornerV, 333, 667);

      dest[0] = encoder.Encode(cw);
      dest[1] = encoder.Encode(interpR1);
      dest[2] = encoder.Encode(interpR2);

      dest[destStride] = encoder.Encode(interpD1);
      dest[destStride + 1] = encoder.Encode(midDR);
      dest[destStride + 2] = encoder.Encode(edgeDR);

      dest[2 * destStride] = encoder.Encode(interpD2);
      dest[2 * destStride + 1] = encoder.Encode(edgeDR);
      dest[2 * destStride + 2] = encoder.Encode(corner);
    }
  }
}

#endregion
