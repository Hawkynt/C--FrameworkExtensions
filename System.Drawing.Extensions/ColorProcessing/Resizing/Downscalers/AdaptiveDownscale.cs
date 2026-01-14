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
using Hawkynt.ColorProcessing.Storage;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Downscalers;

/// <summary>
/// Adaptive content-aware downscaling algorithm.
/// </summary>
/// <remarks>
/// <para>Reduces image size using edge-aware weighting to preserve important details.</para>
/// <para>Uses gradient magnitude to identify edge pixels and weight them higher.</para>
/// <para>Produces sharper edges compared to box filtering while avoiding aliasing.</para>
/// <para>Gaussian falloff ensures smooth transitions in non-edge regions.</para>
/// </remarks>
[ScalerInfo("Adaptive Downscale", Description = "Content-aware edge-preserving downscaling", Category = ScalerCategory.Resampler)]
public readonly struct AdaptiveDownscale : IDownscaler {
  private readonly float _edgeSensitivity;

  /// <summary>
  /// Creates an adaptive downscaler with the specified ratio.
  /// </summary>
  /// <param name="ratioX">Horizontal downscale ratio (2-5).</param>
  /// <param name="ratioY">Vertical downscale ratio (2-5).</param>
  /// <param name="edgeSensitivity">Sensitivity to edges (0-1). Higher values preserve more edges.</param>
  public AdaptiveDownscale(int ratioX, int ratioY, float edgeSensitivity = 0.5f) {
    ArgumentOutOfRangeException.ThrowIfLessThan(ratioX, 2);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(ratioX, 5);
    ArgumentOutOfRangeException.ThrowIfLessThan(ratioY, 2);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(ratioY, 5);

    this.RatioX = ratioX;
    this.RatioY = ratioY;
    this._edgeSensitivity = Math.Clamp(edgeSensitivity, 0f, 1f);
  }

  /// <summary>
  /// Creates an adaptive downscaler with uniform ratio.
  /// </summary>
  /// <param name="ratio">Downscale ratio for both dimensions (2-5).</param>
  /// <param name="edgeSensitivity">Sensitivity to edges (0-1).</param>
  public AdaptiveDownscale(int ratio, float edgeSensitivity = 0.5f) : this(ratio, ratio, edgeSensitivity) { }

  /// <inheritdoc />
  public int RatioX { get; }

  /// <inheritdoc />
  public int RatioY { get; }

  /// <inheritdoc />
  public ScaleFactor Scale => new(1, 1);

  /// <summary>
  /// Gets the list of commonly used downscale ratios.
  /// </summary>
  public static int[] SupportedRatios { get; } = [2, 3, 4, 5];

  /// <summary>
  /// Determines whether AdaptiveDownscale supports the specified ratio.
  /// </summary>
  public static bool SupportsRatio(int ratio) => ratio is >= 2 and <= 5;

  /// <summary>
  /// Enumerates common target dimensions for AdaptiveDownscale.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    if (sourceWidth >= 4 && sourceHeight >= 4)
      yield return (sourceWidth / 2, sourceHeight / 2);
    if (sourceWidth >= 6 && sourceHeight >= 6)
      yield return (sourceWidth / 3, sourceHeight / 3);
    if (sourceWidth >= 8 && sourceHeight >= 8)
      yield return (sourceWidth / 4, sourceHeight / 4);
    if (sourceWidth >= 10 && sourceHeight >= 10)
      yield return (sourceWidth / 5, sourceHeight / 5);
  }

  #region Static Presets

  /// <summary>Gets an adaptive downscaler at 1/2 scale.</summary>
  public static AdaptiveDownscale Ratio2 => new(2);

  /// <summary>Gets an adaptive downscaler at 1/3 scale.</summary>
  public static AdaptiveDownscale Ratio3 => new(3);

  /// <summary>Gets an adaptive downscaler at 1/4 scale.</summary>
  public static AdaptiveDownscale Ratio4 => new(4);

  /// <summary>Gets an adaptive downscaler at 1/5 scale.</summary>
  public static AdaptiveDownscale Ratio5 => new(5);

  /// <summary>Gets the default configuration (1/2 scale).</summary>
  public static AdaptiveDownscale Default => Ratio2;

  /// <summary>Gets a configuration with high edge sensitivity.</summary>
  public static AdaptiveDownscale EdgePreserving => new(2, 0.8f);

  /// <summary>Gets a configuration with low edge sensitivity for smoother results.</summary>
  public static AdaptiveDownscale Smooth => new(2, 0.2f);

  #endregion

  /// <inheritdoc />
  public TResult InvokeKernel<TWork, TKey, TPixel, TEncode, TResult>(
    IDownscaleKernelCallback<TWork, TKey, TPixel, TEncode, TResult> callback)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TEncode : struct, IEncode<TWork, TPixel>
    => (this.RatioX, this.RatioY) switch {
      (2, 2) => callback.Invoke(new Adaptive2x2Kernel<TWork, TKey, TPixel, TEncode>(this._edgeSensitivity)),
      (3, 3) => callback.Invoke(new Adaptive3x3Kernel<TWork, TKey, TPixel, TEncode>(this._edgeSensitivity)),
      (4, 4) => callback.Invoke(new Adaptive4x4Kernel<TWork, TKey, TPixel, TEncode>(this._edgeSensitivity)),
      (5, 5) => callback.Invoke(new Adaptive5x5Kernel<TWork, TKey, TPixel, TEncode>(this._edgeSensitivity)),
      _ => throw new InvalidOperationException($"No kernel available for ratio {this.RatioX}x{this.RatioY}")
    };
}

#region Adaptive Kernels

/// <summary>
/// Adaptive 2x2 kernel for 2:1 downscaling using gradient-based weighting.
/// </summary>
file readonly struct Adaptive2x2Kernel<TWork, TKey, TPixel, TEncode>(float edgeSensitivity)
  : IDownscaleKernel<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int RatioX => 2;
  public int RatioY => 2;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TPixel Average(in NeighborWindow<TWork, TKey> w, in TEncode encoder) {
    var c00 = w.P0P0.Work;
    var c10 = w.P0P1.Work;
    var c01 = w.P1P0.Work;
    var c11 = w.P1P1.Work;

    // Compute luminance
    var l00 = ColorConverter.GetLuminance(in c00);
    var l10 = ColorConverter.GetLuminance(in c10);
    var l01 = ColorConverter.GetLuminance(in c01);
    var l11 = ColorConverter.GetLuminance(in c11);

    // Compute local gradients for each pixel
    // Use simple differences to neighbors in the 2x2 block
    var g00 = MathF.Abs(l00 - l10) + MathF.Abs(l00 - l01);
    var g10 = MathF.Abs(l10 - l00) + MathF.Abs(l10 - l11);
    var g01 = MathF.Abs(l01 - l00) + MathF.Abs(l01 - l11);
    var g11 = MathF.Abs(l11 - l10) + MathF.Abs(l11 - l01);

    // Apply sigmoid to gradient for smooth edge response
    g00 = Sigmoid(g00 * edgeSensitivity * 4f);
    g10 = Sigmoid(g10 * edgeSensitivity * 4f);
    g01 = Sigmoid(g01 * edgeSensitivity * 4f);
    g11 = Sigmoid(g11 * edgeSensitivity * 4f);

    // Combine uniform and gradient-based weights
    var baseWeight = 0.25f;
    var w00 = baseWeight + g00 * edgeSensitivity;
    var w10 = baseWeight + g10 * edgeSensitivity;
    var w01 = baseWeight + g01 * edgeSensitivity;
    var w11 = baseWeight + g11 * edgeSensitivity;

    // Normalize
    var total = w00 + w10 + w01 + w11;
    var inv = 1f / total;
    w00 *= inv;
    w10 *= inv;
    w01 *= inv;
    w11 *= inv;

    Accum4F<TWork> acc = default;
    acc.AddMul(c00, w00);
    acc.AddMul(c10, w10);
    acc.AddMul(c01, w01);
    acc.AddMul(c11, w11);
    return encoder.Encode(acc.Result);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float Sigmoid(float x) => x / (1f + MathF.Abs(x));
}

/// <summary>
/// Adaptive 3x3 kernel for 3:1 downscaling using Sobel gradient weighting.
/// </summary>
file readonly struct Adaptive3x3Kernel<TWork, TKey, TPixel, TEncode>(float edgeSensitivity)
  : IDownscaleKernel<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int RatioX => 3;
  public int RatioY => 3;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe TPixel Average(in NeighborWindow<TWork, TKey> w, in TEncode encoder) {
    var colors = stackalloc TWork[9];
    colors[0] = w.M1M1.Work;
    colors[1] = w.M1P0.Work;
    colors[2] = w.M1P1.Work;
    colors[3] = w.P0M1.Work;
    colors[4] = w.P0P0.Work;
    colors[5] = w.P0P1.Work;
    colors[6] = w.P1M1.Work;
    colors[7] = w.P1P0.Work;
    colors[8] = w.P1P1.Work;

    var lumas = stackalloc float[9];
    for (var i = 0; i < 9; ++i)
      lumas[i] = ColorConverter.GetLuminance(in colors[i]);

    // Sobel gradients at center (index 4)
    var gx = -lumas[0] + lumas[2] - 2f * lumas[3] + 2f * lumas[5] - lumas[6] + lumas[8];
    var gy = -lumas[0] - 2f * lumas[1] - lumas[2] + lumas[6] + 2f * lumas[7] + lumas[8];
    var centerGrad = MathF.Sqrt(gx * gx + gy * gy);

    // Compute gradients for each position using simple neighbor differences
    var grads = stackalloc float[9];
    for (var i = 0; i < 9; ++i) {
      var sum = 0f;
      var count = 0;
      for (var j = 0; j < 9; ++j)
        if (i != j) {
          sum += MathF.Abs(lumas[i] - lumas[j]);
          ++count;
        }

      grads[i] = sum / count;
    }

    // Apply sigmoid and combine with uniform weights
    var weights = stackalloc float[9];
    var totalWeight = 0f;
    var baseWeight = 1f / 9f;
    for (var i = 0; i < 9; ++i) {
      var grad = Sigmoid(grads[i] * edgeSensitivity * 4f);
      weights[i] = baseWeight + grad * edgeSensitivity * 0.5f;
      totalWeight += weights[i];
    }

    // Extra weight for center if high gradient
    weights[4] += Sigmoid(centerGrad * edgeSensitivity * 2f) * edgeSensitivity * 0.2f;
    totalWeight += Sigmoid(centerGrad * edgeSensitivity * 2f) * edgeSensitivity * 0.2f;

    var invWeight = 1f / totalWeight;
    Accum4F<TWork> acc = default;
    for (var i = 0; i < 9; ++i)
      acc.AddMul(colors[i], weights[i] * invWeight);

    return encoder.Encode(acc.Result);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float Sigmoid(float x) => x / (1f + MathF.Abs(x));
}

/// <summary>
/// Adaptive 4x4 kernel for 4:1 downscaling using gradient weighting.
/// </summary>
file readonly struct Adaptive4x4Kernel<TWork, TKey, TPixel, TEncode>(float edgeSensitivity)
  : IDownscaleKernel<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int RatioX => 4;
  public int RatioY => 4;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe TPixel Average(in NeighborWindow<TWork, TKey> w, in TEncode encoder) {
    var colors = stackalloc TWork[16];
    colors[0] = w.M1M1.Work;
    colors[1] = w.M1P0.Work;
    colors[2] = w.M1P1.Work;
    colors[3] = w.M1P2.Work;
    colors[4] = w.P0M1.Work;
    colors[5] = w.P0P0.Work;
    colors[6] = w.P0P1.Work;
    colors[7] = w.P0P2.Work;
    colors[8] = w.P1M1.Work;
    colors[9] = w.P1P0.Work;
    colors[10] = w.P1P1.Work;
    colors[11] = w.P1P2.Work;
    colors[12] = w.P2M1.Work;
    colors[13] = w.P2P0.Work;
    colors[14] = w.P2P1.Work;
    colors[15] = w.P2P2.Work;

    var lumas = stackalloc float[16];
    var mean = 0f;
    for (var i = 0; i < 16; ++i) {
      lumas[i] = ColorConverter.GetLuminance(in colors[i]);
      mean += lumas[i];
    }
    mean /= 16f;

    // Compute gradient as deviation from mean + neighbor differences
    var grads = stackalloc float[16];
    for (var i = 0; i < 16; ++i) {
      var deviance = MathF.Abs(lumas[i] - mean);
      var neighborSum = 0f;
      var count = 0;

      // Check 4-connected neighbors
      var row = i / 4;
      var col = i % 4;
      if (row > 0) {
        neighborSum += MathF.Abs(lumas[i] - lumas[i - 4]);
        ++count;
      }
      if (row < 3) {
        neighborSum += MathF.Abs(lumas[i] - lumas[i + 4]);
        ++count;
      }
      if (col > 0) {
        neighborSum += MathF.Abs(lumas[i] - lumas[i - 1]);
        ++count;
      }
      if (col < 3) {
        neighborSum += MathF.Abs(lumas[i] - lumas[i + 1]);
        ++count;
      }

      grads[i] = deviance + (count > 0 ? neighborSum / count : 0f);
    }

    // Convert gradients to weights
    var weights = stackalloc float[16];
    var totalWeight = 0f;
    var baseWeight = 1f / 16f;
    for (var i = 0; i < 16; ++i) {
      var grad = Sigmoid(grads[i] * edgeSensitivity * 4f);
      weights[i] = baseWeight + grad * edgeSensitivity * 0.3f;
      totalWeight += weights[i];
    }

    var invWeight = 1f / totalWeight;
    Accum4F<TWork> acc = default;
    for (var i = 0; i < 16; ++i)
      acc.AddMul(colors[i], weights[i] * invWeight);

    return encoder.Encode(acc.Result);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float Sigmoid(float x) => x / (1f + MathF.Abs(x));
}

/// <summary>
/// Adaptive 5x5 kernel for 5:1 downscaling using gradient weighting.
/// </summary>
file readonly struct Adaptive5x5Kernel<TWork, TKey, TPixel, TEncode>(float edgeSensitivity)
  : IDownscaleKernel<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int RatioX => 5;
  public int RatioY => 5;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe TPixel Average(in NeighborWindow<TWork, TKey> w, in TEncode encoder) {
    var colors = stackalloc TWork[25];
    colors[0] = w.M2M2.Work;
    colors[1] = w.M2M1.Work;
    colors[2] = w.M2P0.Work;
    colors[3] = w.M2P1.Work;
    colors[4] = w.M2P2.Work;
    colors[5] = w.M1M2.Work;
    colors[6] = w.M1M1.Work;
    colors[7] = w.M1P0.Work;
    colors[8] = w.M1P1.Work;
    colors[9] = w.M1P2.Work;
    colors[10] = w.P0M2.Work;
    colors[11] = w.P0M1.Work;
    colors[12] = w.P0P0.Work;
    colors[13] = w.P0P1.Work;
    colors[14] = w.P0P2.Work;
    colors[15] = w.P1M2.Work;
    colors[16] = w.P1M1.Work;
    colors[17] = w.P1P0.Work;
    colors[18] = w.P1P1.Work;
    colors[19] = w.P1P2.Work;
    colors[20] = w.P2M2.Work;
    colors[21] = w.P2M1.Work;
    colors[22] = w.P2P0.Work;
    colors[23] = w.P2P1.Work;
    colors[24] = w.P2P2.Work;

    var lumas = stackalloc float[25];
    var mean = 0f;
    for (var i = 0; i < 25; ++i) {
      lumas[i] = ColorConverter.GetLuminance(in colors[i]);
      mean += lumas[i];
    }
    mean /= 25f;

    // Compute gradient as deviation from mean + neighbor differences
    var grads = stackalloc float[25];
    for (var i = 0; i < 25; ++i) {
      var deviance = MathF.Abs(lumas[i] - mean);
      var neighborSum = 0f;
      var count = 0;

      var row = i / 5;
      var col = i % 5;
      if (row > 0) {
        neighborSum += MathF.Abs(lumas[i] - lumas[i - 5]);
        ++count;
      }
      if (row < 4) {
        neighborSum += MathF.Abs(lumas[i] - lumas[i + 5]);
        ++count;
      }
      if (col > 0) {
        neighborSum += MathF.Abs(lumas[i] - lumas[i - 1]);
        ++count;
      }
      if (col < 4) {
        neighborSum += MathF.Abs(lumas[i] - lumas[i + 1]);
        ++count;
      }

      grads[i] = deviance + (count > 0 ? neighborSum / count : 0f);
    }

    // Convert gradients to weights
    var weights = stackalloc float[25];
    var totalWeight = 0f;
    var baseWeight = 1f / 25f;
    for (var i = 0; i < 25; ++i) {
      var grad = Sigmoid(grads[i] * edgeSensitivity * 4f);
      weights[i] = baseWeight + grad * edgeSensitivity * 0.25f;
      totalWeight += weights[i];
    }

    var invWeight = 1f / totalWeight;
    Accum4F<TWork> acc = default;
    for (var i = 0; i < 25; ++i)
      acc.AddMul(colors[i], weights[i] * invWeight);

    return encoder.Encode(acc.Result);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float Sigmoid(float x) => x / (1f + MathF.Abs(x));
}

#endregion
