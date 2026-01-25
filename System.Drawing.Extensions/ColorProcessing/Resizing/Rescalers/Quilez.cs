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
/// Quilez interpolation scaler - improved texture interpolation using quintic smoothstep.
/// </summary>
/// <remarks>
/// <para>Based on "Improved texture interpolation" by Inigo Quilez.</para>
/// <para>Uses a smooth interpolation curve (quintic polynomial) instead of linear interpolation.</para>
/// <para>Produces smoother gradients than standard bilinear while avoiding ringing artifacts.</para>
/// <para>Reference: http://www.iquilezles.org/www/articles/texture/texture.htm</para>
/// </remarks>
[ScalerInfo("Quilez", Author = "Inigo Quilez",
  Url = "http://www.iquilezles.org/www/articles/texture/texture.htm",
  Description = "Quintic smoothstep interpolation for smooth gradients", Category = ScalerCategory.Resampler)]
public readonly struct Quilez : IPixelScaler {
  private readonly int _scale;

  /// <summary>
  /// Creates a Quilez scaler with the specified factor.
  /// </summary>
  /// <param name="scale">Scale factor (2, 3, or 4).</param>
  public Quilez(int scale = 2) {
    if (scale is < 2 or > 4)
      throw new ArgumentOutOfRangeException(nameof(scale), scale, "Quilez supports 2x, 3x, 4x scaling");
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
      0 or 2 => callback.Invoke(new Quilez2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      3 => callback.Invoke(new Quilez3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      4 => callback.Invoke(new Quilez4xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
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

  /// <summary>Gets a 2x Quilez scaler.</summary>
  public static Quilez X2 => new(2);

  /// <summary>Gets a 3x Quilez scaler.</summary>
  public static Quilez X3 => new(3);

  /// <summary>Gets a 4x Quilez scaler.</summary>
  public static Quilez X4 => new(4);

  /// <summary>Gets the default Quilez scaler (2x).</summary>
  public static Quilez Default => X2;

  #endregion
}

#region Quilez Helpers

/// <summary>
/// Helper methods for Quilez quintic smoothstep interpolation.
/// </summary>
file static class QuilezHelpers {
  /// <summary>
  /// Quintic smoothstep function: f(x) = x^3 * (x * (x * 6 - 15) + 10)
  /// Maps [0,1] to [0,1] with zero first and second derivatives at endpoints.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float QuinticSmooth(float t)
    => t * t * t * (t * (t * 6f - 15f) + 10f);

  /// <summary>
  /// Pre-computed weights for 2x scaling sub-pixels.
  /// Each tuple is (row, col, wx, wy) where wx/wy are quintic-smoothed weights (0-256 scale).
  /// </summary>
  public static readonly (int wx, int wy)[] Weights2x = ComputeWeights(2);

  /// <summary>
  /// Pre-computed weights for 3x scaling sub-pixels.
  /// </summary>
  public static readonly (int wx, int wy)[] Weights3x = ComputeWeights(3);

  /// <summary>
  /// Pre-computed weights for 4x scaling sub-pixels.
  /// </summary>
  public static readonly (int wx, int wy)[] Weights4x = ComputeWeights(4);

  private static (int wx, int wy)[] ComputeWeights(int scale) {
    var result = new (int wx, int wy)[scale * scale];
    for (var y = 0; y < scale; ++y)
    for (var x = 0; x < scale; ++x) {
      // Subpixel position (0.5 offset for pixel centers)
      var fx = (x + 0.5f) / scale;
      var fy = (y + 0.5f) / scale;

      // Apply quintic smoothstep
      var sfx = QuinticSmooth(fx);
      var sfy = QuinticSmooth(fy);

      // Convert to integer weights (0-256)
      result[y * scale + x] = ((int)(sfx * 256f), (int)(sfy * 256f));
    }
    return result;
  }
}

#endregion

#region Quilez 2x Kernel

file readonly struct Quilez2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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
    // Get the four corners for bilinear sampling
    var c00 = window.P0P0.Work;  // top-left (center pixel)
    var c10 = window.P0P1.Work;  // top-right
    var c01 = window.P1P0.Work;  // bottom-left
    var c11 = window.P1P1.Work;  // bottom-right

    // Write each sub-pixel with quintic smoothstep interpolation
    var weights = QuilezHelpers.Weights2x;

    for (var y = 0; y < 2; ++y) {
      var row = dest + y * destStride;
      for (var x = 0; x < 2; ++x) {
        var (wx, wy) = weights[y * 2 + x];

        // Bilinear blend with smoothstepped weights
        var top = lerp.Lerp(c00, c10, 256 - wx, wx);
        var bottom = lerp.Lerp(c01, c11, 256 - wx, wx);
        var result = lerp.Lerp(top, bottom, 256 - wy, wy);

        row[x] = encoder.Encode(result);
      }
    }
  }
}

#endregion

#region Quilez 3x Kernel

file readonly struct Quilez3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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
    var c00 = window.P0P0.Work;
    var c10 = window.P0P1.Work;
    var c01 = window.P1P0.Work;
    var c11 = window.P1P1.Work;

    var weights = QuilezHelpers.Weights3x;

    for (var y = 0; y < 3; ++y) {
      var row = dest + y * destStride;
      for (var x = 0; x < 3; ++x) {
        var (wx, wy) = weights[y * 3 + x];

        var top = lerp.Lerp(c00, c10, 256 - wx, wx);
        var bottom = lerp.Lerp(c01, c11, 256 - wx, wx);
        var result = lerp.Lerp(top, bottom, 256 - wy, wy);

        row[x] = encoder.Encode(result);
      }
    }
  }
}

#endregion

#region Quilez 4x Kernel

file readonly struct Quilez4xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
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
    var c00 = window.P0P0.Work;
    var c10 = window.P0P1.Work;
    var c01 = window.P1P0.Work;
    var c11 = window.P1P1.Work;

    var weights = QuilezHelpers.Weights4x;

    for (var y = 0; y < 4; ++y) {
      var row = dest + y * destStride;
      for (var x = 0; x < 4; ++x) {
        var (wx, wy) = weights[y * 4 + x];

        var top = lerp.Lerp(c00, c10, 256 - wx, wx);
        var bottom = lerp.Lerp(c01, c11, 256 - wx, wx);
        var result = lerp.Lerp(top, bottom, 256 - wy, wy);

        row[x] = encoder.Encode(result);
      }
    }
  }
}

#endregion
