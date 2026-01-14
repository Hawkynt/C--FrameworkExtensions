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
/// Structural Similarity (SSIM) aware downscaling algorithm.
/// </summary>
/// <remarks>
/// <para>Reduces image size while preserving structural detail using perceptual weighting.</para>
/// <para>Pixels with high local contrast are weighted more heavily to preserve edges.</para>
/// <para>Uses variance-based weighting to prioritize structurally important pixels.</para>
/// <para>Produces sharper results than box filtering while maintaining natural appearance.</para>
/// </remarks>
[ScalerInfo("SSIM Downscale", Description = "Perceptually-aware downscaling with structure preservation", Category = ScalerCategory.Resampler)]
public readonly struct SsimDownscale : IDownscaler {
  private readonly float _structureWeight;

  /// <summary>
  /// Creates an SSIM downscaler with the specified ratio.
  /// </summary>
  /// <param name="ratioX">Horizontal downscale ratio (2-5).</param>
  /// <param name="ratioY">Vertical downscale ratio (2-5).</param>
  /// <param name="structureWeight">Weight for structural detail (0-1). Higher preserves more detail.</param>
  public SsimDownscale(int ratioX, int ratioY, float structureWeight = 0.5f) {
    ArgumentOutOfRangeException.ThrowIfLessThan(ratioX, 2);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(ratioX, 5);
    ArgumentOutOfRangeException.ThrowIfLessThan(ratioY, 2);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(ratioY, 5);

    this.RatioX = ratioX;
    this.RatioY = ratioY;
    this._structureWeight = Math.Clamp(structureWeight, 0f, 1f);
  }

  /// <summary>
  /// Creates an SSIM downscaler with uniform ratio.
  /// </summary>
  /// <param name="ratio">Downscale ratio for both dimensions (2-5).</param>
  /// <param name="structureWeight">Weight for structural detail (0-1).</param>
  public SsimDownscale(int ratio, float structureWeight = 0.5f) : this(ratio, ratio, structureWeight) { }

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
  /// Determines whether SsimDownscale supports the specified ratio.
  /// </summary>
  public static bool SupportsRatio(int ratio) => ratio is >= 2 and <= 5;

  /// <summary>
  /// Enumerates common target dimensions for SsimDownscale.
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

  /// <summary>Gets an SSIM downscaler at 1/2 scale.</summary>
  public static SsimDownscale Ratio2 => new(2);

  /// <summary>Gets an SSIM downscaler at 1/3 scale.</summary>
  public static SsimDownscale Ratio3 => new(3);

  /// <summary>Gets an SSIM downscaler at 1/4 scale.</summary>
  public static SsimDownscale Ratio4 => new(4);

  /// <summary>Gets an SSIM downscaler at 1/5 scale.</summary>
  public static SsimDownscale Ratio5 => new(5);

  /// <summary>Gets the default configuration (1/2 scale).</summary>
  public static SsimDownscale Default => Ratio2;

  /// <summary>Gets a configuration optimized for sharp detail preservation.</summary>
  public static SsimDownscale Sharp => new(2, 0.8f);

  /// <summary>Gets a configuration optimized for smooth results.</summary>
  public static SsimDownscale Smooth => new(2, 0.2f);

  #endregion

  /// <inheritdoc />
  public TResult InvokeKernel<TWork, TKey, TPixel, TEncode, TResult>(
    IDownscaleKernelCallback<TWork, TKey, TPixel, TEncode, TResult> callback)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TEncode : struct, IEncode<TWork, TPixel>
    => (this.RatioX, this.RatioY) switch {
      (2, 2) => callback.Invoke(new Ssim2x2Kernel<TWork, TKey, TPixel, TEncode>(this._structureWeight)),
      (3, 3) => callback.Invoke(new Ssim3x3Kernel<TWork, TKey, TPixel, TEncode>(this._structureWeight)),
      (4, 4) => callback.Invoke(new Ssim4x4Kernel<TWork, TKey, TPixel, TEncode>(this._structureWeight)),
      (5, 5) => callback.Invoke(new Ssim5x5Kernel<TWork, TKey, TPixel, TEncode>(this._structureWeight)),
      _ => throw new InvalidOperationException($"No kernel available for ratio {this.RatioX}x{this.RatioY}")
    };
}

#region SSIM Kernels

/// <summary>
/// SSIM 2x2 kernel for 2:1 downscaling.
/// </summary>
file readonly struct Ssim2x2Kernel<TWork, TKey, TPixel, TEncode>(float structureWeight)
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

    // Compute luminance for each pixel
    var l00 = ColorConverter.GetLuminance(in c00);
    var l10 = ColorConverter.GetLuminance(in c10);
    var l01 = ColorConverter.GetLuminance(in c01);
    var l11 = ColorConverter.GetLuminance(in c11);

    // Compute local variance as structure indicator
    var mean = (l00 + l10 + l01 + l11) * 0.25f;
    var v00 = (l00 - mean) * (l00 - mean);
    var v10 = (l10 - mean) * (l10 - mean);
    var v01 = (l01 - mean) * (l01 - mean);
    var v11 = (l11 - mean) * (l11 - mean);

    // Normalize variance to get weight contribution
    var totalVar = v00 + v10 + v01 + v11 + 0.0001f;

    // Blend between uniform and variance-weighted
    var uniformWeight = 0.25f * (1f - structureWeight);
    var w00 = uniformWeight + structureWeight * (v00 / totalVar);
    var w10 = uniformWeight + structureWeight * (v10 / totalVar);
    var w01 = uniformWeight + structureWeight * (v01 / totalVar);
    var w11 = uniformWeight + structureWeight * (v11 / totalVar);

    // Normalize weights
    var totalWeight = w00 + w10 + w01 + w11;
    var invWeight = 1f / totalWeight;
    w00 *= invWeight;
    w10 *= invWeight;
    w01 *= invWeight;
    w11 *= invWeight;

    Accum4F<TWork> acc = default;
    acc.AddMul(c00, w00);
    acc.AddMul(c10, w10);
    acc.AddMul(c01, w01);
    acc.AddMul(c11, w11);
    return encoder.Encode(acc.Result);
  }

}

/// <summary>
/// SSIM 3x3 kernel for 3:1 downscaling.
/// </summary>
file readonly struct Ssim3x3Kernel<TWork, TKey, TPixel, TEncode>(float structureWeight)
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
    var mean = 0f;
    for (var i = 0; i < 9; ++i) {
      lumas[i] = ColorConverter.GetLuminance(in colors[i]);
      mean += lumas[i];
    }
    mean /= 9f;

    var weights = stackalloc float[9];
    var totalVar = 0f;
    for (var i = 0; i < 9; ++i) {
      var diff = lumas[i] - mean;
      weights[i] = diff * diff;
      totalVar += weights[i];
    }
    totalVar += 0.0001f;

    var uniformWeight = (1f / 9f) * (1f - structureWeight);
    var totalWeight = 0f;
    for (var i = 0; i < 9; ++i) {
      weights[i] = uniformWeight + structureWeight * (weights[i] / totalVar);
      totalWeight += weights[i];
    }

    var invWeight = 1f / totalWeight;
    Accum4F<TWork> acc = default;
    for (var i = 0; i < 9; ++i)
      acc.AddMul(colors[i], weights[i] * invWeight);

    return encoder.Encode(acc.Result);
  }

}

/// <summary>
/// SSIM 4x4 kernel for 4:1 downscaling.
/// </summary>
file readonly struct Ssim4x4Kernel<TWork, TKey, TPixel, TEncode>(float structureWeight)
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
    // Row M1
    colors[0] = w.M1M1.Work;
    colors[1] = w.M1P0.Work;
    colors[2] = w.M1P1.Work;
    colors[3] = w.M1P2.Work;
    // Row P0
    colors[4] = w.P0M1.Work;
    colors[5] = w.P0P0.Work;
    colors[6] = w.P0P1.Work;
    colors[7] = w.P0P2.Work;
    // Row P1
    colors[8] = w.P1M1.Work;
    colors[9] = w.P1P0.Work;
    colors[10] = w.P1P1.Work;
    colors[11] = w.P1P2.Work;
    // Row P2
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

    var weights = stackalloc float[16];
    var totalVar = 0f;
    for (var i = 0; i < 16; ++i) {
      var diff = lumas[i] - mean;
      weights[i] = diff * diff;
      totalVar += weights[i];
    }
    totalVar += 0.0001f;

    var uniformWeight = (1f / 16f) * (1f - structureWeight);
    var totalWeight = 0f;
    for (var i = 0; i < 16; ++i) {
      weights[i] = uniformWeight + structureWeight * (weights[i] / totalVar);
      totalWeight += weights[i];
    }

    var invWeight = 1f / totalWeight;
    Accum4F<TWork> acc = default;
    for (var i = 0; i < 16; ++i)
      acc.AddMul(colors[i], weights[i] * invWeight);

    return encoder.Encode(acc.Result);
  }

}

/// <summary>
/// SSIM 5x5 kernel for 5:1 downscaling.
/// </summary>
file readonly struct Ssim5x5Kernel<TWork, TKey, TPixel, TEncode>(float structureWeight)
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
    // Row M2
    colors[0] = w.M2M2.Work;
    colors[1] = w.M2M1.Work;
    colors[2] = w.M2P0.Work;
    colors[3] = w.M2P1.Work;
    colors[4] = w.M2P2.Work;
    // Row M1
    colors[5] = w.M1M2.Work;
    colors[6] = w.M1M1.Work;
    colors[7] = w.M1P0.Work;
    colors[8] = w.M1P1.Work;
    colors[9] = w.M1P2.Work;
    // Row P0
    colors[10] = w.P0M2.Work;
    colors[11] = w.P0M1.Work;
    colors[12] = w.P0P0.Work;
    colors[13] = w.P0P1.Work;
    colors[14] = w.P0P2.Work;
    // Row P1
    colors[15] = w.P1M2.Work;
    colors[16] = w.P1M1.Work;
    colors[17] = w.P1P0.Work;
    colors[18] = w.P1P1.Work;
    colors[19] = w.P1P2.Work;
    // Row P2
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

    var weights = stackalloc float[25];
    var totalVar = 0f;
    for (var i = 0; i < 25; ++i) {
      var diff = lumas[i] - mean;
      weights[i] = diff * diff;
      totalVar += weights[i];
    }
    totalVar += 0.0001f;

    var uniformWeight = (1f / 25f) * (1f - structureWeight);
    var totalWeight = 0f;
    for (var i = 0; i < 25; ++i) {
      weights[i] = uniformWeight + structureWeight * (weights[i] / totalVar);
      totalWeight += weights[i];
    }

    var invWeight = 1f / totalWeight;
    Accum4F<TWork> acc = default;
    for (var i = 0; i < 25; ++i)
      acc.AddMul(colors[i], weights[i] * invWeight);

    return encoder.Encode(acc.Result);
  }

}

#endregion
