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
/// Anti-Aliased Nearest Neighbor (AANN) scaler - gamma-corrected bilinear interpolation.
/// </summary>
/// <remarks>
/// <para>Reference: https://github.com/libretro/glsl-shaders/tree/master/interpolation/shaders/aann</para>
/// <para>Algorithm: Combines nearest-neighbor pixel selection with gamma-corrected bilinear smoothing
/// for perceptually correct anti-aliased interpolation. Uses neighbor blending within the output
/// block to approximate gamma-aware bilinear filtering.</para>
/// <para>Developed by jimbo1qaz and wareya.</para>
/// </remarks>
[ScalerInfo("AANN", Author = "jimbo1qaz/wareya",
  Url = "https://github.com/libretro/glsl-shaders/tree/master/interpolation/shaders/aann",
  Description = "Anti-aliased nearest neighbor with gamma-corrected bilinear interpolation", Category = ScalerCategory.Resampler)]
public readonly struct Aann : IPixelScaler {
  private readonly int _scale;

  /// <summary>
  /// Creates an AANN scaler with the specified factor.
  /// </summary>
  /// <param name="scale">Scale factor (2 or 3).</param>
  public Aann(int scale = 2) {
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
      0 or 2 => callback.Invoke(new Aann2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      3 => callback.Invoke(new Aann3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
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

  /// <summary>Gets a 2x AANN scaler.</summary>
  public static Aann X2 => new(2);

  /// <summary>Gets a 3x AANN scaler.</summary>
  public static Aann X3 => new(3);

  /// <summary>Gets the default AANN scaler (2x).</summary>
  public static Aann Default => X2;

  #endregion
}

#region Aann Helpers

file static class AannHelpers {
  /// <summary>Weight scale for integer lerp operations.</summary>
  public const int WeightScale = 1000;

  /// <summary>Corner blend weight for 2x (25% neighbor contribution).</summary>
  public const int CornerBlend2x = 250;

  /// <summary>Edge blend weight for 2x (50% neighbor contribution).</summary>
  public const int EdgeBlend2x = 500;

  /// <summary>Edge blend weight for 3x outer pixels (33% neighbor).</summary>
  public const int EdgeBlend3x = 333;

  /// <summary>Corner blend weight for 3x (17% neighbor).</summary>
  public const int CornerBlend3x = 167;
}

#endregion

#region Aann 2x Kernel

file readonly struct Aann2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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
    var c = window.P0P0.Work;
    var r = window.P0P1.Work;
    var d = window.P1P0.Work;
    var dr = window.P1P1.Work;

    // Anti-aliased bilinear: each output pixel is a weighted blend
    // Top-left: pure center pixel
    var topLeft = c;

    // Top-right: blend center with right neighbor
    var topRight = lerp.Lerp(c, r, AannHelpers.WeightScale - AannHelpers.EdgeBlend2x, AannHelpers.EdgeBlend2x);

    // Bottom-left: blend center with below neighbor
    var bottomLeft = lerp.Lerp(c, d, AannHelpers.WeightScale - AannHelpers.EdgeBlend2x, AannHelpers.EdgeBlend2x);

    // Bottom-right: blend all four (center-weighted)
    var midH = lerp.Lerp(c, r, AannHelpers.WeightScale - AannHelpers.CornerBlend2x, AannHelpers.CornerBlend2x);
    var midV = lerp.Lerp(d, dr, AannHelpers.WeightScale - AannHelpers.CornerBlend2x, AannHelpers.CornerBlend2x);
    var bottomRight = lerp.Lerp(midH, midV, AannHelpers.WeightScale - AannHelpers.CornerBlend2x, AannHelpers.CornerBlend2x);

    dest[0] = encoder.Encode(topLeft);
    dest[1] = encoder.Encode(topRight);
    dest[destStride] = encoder.Encode(bottomLeft);
    dest[destStride + 1] = encoder.Encode(bottomRight);
  }
}

#endregion

#region Aann 3x Kernel

file readonly struct Aann3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
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
    var c = window.P0P0.Work;
    var r = window.P0P1.Work;
    var d = window.P1P0.Work;
    var dr = window.P1P1.Work;

    // Center pixel (pure)
    var centerEncoded = encoder.Encode(c);

    // Edge blends (33% neighbor)
    var edgeR = lerp.Lerp(c, r, AannHelpers.WeightScale - AannHelpers.EdgeBlend3x, AannHelpers.EdgeBlend3x);
    var edgeD = lerp.Lerp(c, d, AannHelpers.WeightScale - AannHelpers.EdgeBlend3x, AannHelpers.EdgeBlend3x);

    // Corner blend (17% each neighbor direction)
    var cornerH = lerp.Lerp(c, r, AannHelpers.WeightScale - AannHelpers.CornerBlend3x, AannHelpers.CornerBlend3x);
    var cornerV = lerp.Lerp(d, dr, AannHelpers.WeightScale - AannHelpers.CornerBlend3x, AannHelpers.CornerBlend3x);
    var corner = lerp.Lerp(cornerH, cornerV, AannHelpers.WeightScale - AannHelpers.EdgeBlend3x, AannHelpers.EdgeBlend3x);

    // 3x3 pattern with bilinear anti-aliasing:
    // Row 0: [center] [edge-right] [edge-right-more]
    dest[0] = centerEncoded;
    dest[1] = centerEncoded;
    dest[2] = encoder.Encode(edgeR);

    // Row 1: [center] [center] [edge-right]
    dest[destStride] = centerEncoded;
    dest[destStride + 1] = centerEncoded;
    dest[destStride + 2] = encoder.Encode(edgeR);

    // Row 2: [edge-down] [edge-down] [corner]
    dest[2 * destStride] = encoder.Encode(edgeD);
    dest[2 * destStride + 1] = encoder.Encode(edgeD);
    dest[2 * destStride + 2] = encoder.Encode(corner);
  }
}

#endregion
