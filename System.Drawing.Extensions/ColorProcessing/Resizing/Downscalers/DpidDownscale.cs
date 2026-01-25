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
/// DPID (Detail-Preserving Image Downscaling) algorithm.
/// </summary>
/// <remarks>
/// <para>Reference: Weber et al. "Rapid, Detail-Preserving Image Downscaling"</para>
/// <para>Algorithm: Weights pixels by deviation from local average to preserve edges.</para>
/// <para>Pixels that differ more from the local mean get higher weights.</para>
/// <para>Formula: weight = 1 + lambda * (deviation / 128)</para>
/// </remarks>
[ScalerInfo("DPID", Description = "Detail-Preserving Image Downscaling", Category = ScalerCategory.Resampler)]
public readonly struct DpidDownscale : IDownscaler {
  private readonly float _lambda;

  /// <summary>
  /// Creates a DPID downscaler with the specified ratio.
  /// </summary>
  /// <param name="ratioX">Horizontal downscale ratio (2-5).</param>
  /// <param name="ratioY">Vertical downscale ratio (2-5).</param>
  /// <param name="lambda">Detail preservation strength (0-2). Higher values preserve more detail.</param>
  public DpidDownscale(int ratioX, int ratioY, float lambda = 1f) {
    ArgumentOutOfRangeException.ThrowIfLessThan(ratioX, 2);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(ratioX, 5);
    ArgumentOutOfRangeException.ThrowIfLessThan(ratioY, 2);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(ratioY, 5);

    this.RatioX = ratioX;
    this.RatioY = ratioY;
    this._lambda = Math.Clamp(lambda, 0f, 2f);
  }

  /// <summary>
  /// Creates a DPID downscaler with uniform ratio.
  /// </summary>
  /// <param name="ratio">Downscale ratio for both dimensions (2-5).</param>
  /// <param name="lambda">Detail preservation strength (0-2).</param>
  public DpidDownscale(int ratio, float lambda = 1f) : this(ratio, ratio, lambda) { }

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
  /// Determines whether DPID supports the specified ratio.
  /// </summary>
  public static bool SupportsRatio(int ratio) => ratio is >= 2 and <= 5;

  /// <summary>
  /// Enumerates common target dimensions for DPID.
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

  /// <summary>Gets a DPID downscaler at 1/2 scale.</summary>
  public static DpidDownscale Ratio2 => new(2);

  /// <summary>Gets a DPID downscaler at 1/3 scale.</summary>
  public static DpidDownscale Ratio3 => new(3);

  /// <summary>Gets a DPID downscaler at 1/4 scale.</summary>
  public static DpidDownscale Ratio4 => new(4);

  /// <summary>Gets a DPID downscaler at 1/5 scale.</summary>
  public static DpidDownscale Ratio5 => new(5);

  /// <summary>Gets the default configuration (1/2 scale, lambda=1).</summary>
  public static DpidDownscale Default => Ratio2;

  /// <summary>Gets a configuration with high detail preservation.</summary>
  public static DpidDownscale HighDetail => new(2, 1.5f);

  /// <summary>Gets a configuration with low detail preservation for smoother results.</summary>
  public static DpidDownscale Smooth => new(2, 0.5f);

  #endregion

  /// <inheritdoc />
  public TResult InvokeKernel<TWork, TKey, TPixel, TEncode, TResult>(
    IDownscaleKernelCallback<TWork, TKey, TPixel, TEncode, TResult> callback)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TEncode : struct, IEncode<TWork, TPixel>
    => (this.RatioX, this.RatioY) switch {
      (2, 2) => callback.Invoke(new Dpid2x2Kernel<TWork, TKey, TPixel, TEncode>(this._lambda)),
      (3, 3) => callback.Invoke(new Dpid3x3Kernel<TWork, TKey, TPixel, TEncode>(this._lambda)),
      (4, 4) => callback.Invoke(new Dpid4x4Kernel<TWork, TKey, TPixel, TEncode>(this._lambda)),
      (5, 5) => callback.Invoke(new Dpid5x5Kernel<TWork, TKey, TPixel, TEncode>(this._lambda)),
      _ => throw new InvalidOperationException($"No kernel available for ratio {this.RatioX}x{this.RatioY}")
    };
}

#region DPID Kernels

/// <summary>
/// DPID 2x2 kernel for 2:1 downscaling using deviation-based weighting.
/// </summary>
file readonly struct Dpid2x2Kernel<TWork, TKey, TPixel, TEncode>(float lambda)
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

    // Calculate local average luminance
    var avgLuma = (l00 + l10 + l01 + l11) * 0.25f;

    // Calculate deviations from average
    var dev00 = MathF.Abs(l00 - avgLuma);
    var dev10 = MathF.Abs(l10 - avgLuma);
    var dev01 = MathF.Abs(l01 - avgLuma);
    var dev11 = MathF.Abs(l11 - avgLuma);

    // DPID formula: weight = 1 + lambda * (deviation / 128)
    var w00 = 1f + lambda * (dev00 / 128f);
    var w10 = 1f + lambda * (dev10 / 128f);
    var w01 = 1f + lambda * (dev01 / 128f);
    var w11 = 1f + lambda * (dev11 / 128f);

    // Normalize weights
    var totalWeight = w00 + w10 + w01 + w11;
    var inv = 1f / totalWeight;
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
}

/// <summary>
/// DPID 3x3 kernel for 3:1 downscaling using deviation-based weighting.
/// </summary>
file readonly struct Dpid3x3Kernel<TWork, TKey, TPixel, TEncode>(float lambda)
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

    // First pass: compute average luminance
    var lumas = stackalloc float[9];
    var avgLuma = 0f;
    for (var i = 0; i < 9; ++i) {
      lumas[i] = ColorConverter.GetLuminance(in colors[i]);
      avgLuma += lumas[i];
    }
    avgLuma /= 9f;

    // Second pass: compute weights based on deviation
    var weights = stackalloc float[9];
    var totalWeight = 0f;
    for (var i = 0; i < 9; ++i) {
      var deviation = MathF.Abs(lumas[i] - avgLuma);
      weights[i] = 1f + lambda * (deviation / 128f);
      totalWeight += weights[i];
    }

    // Normalize and accumulate
    var inv = 1f / totalWeight;
    Accum4F<TWork> acc = default;
    for (var i = 0; i < 9; ++i)
      acc.AddMul(colors[i], weights[i] * inv);

    return encoder.Encode(acc.Result);
  }
}

/// <summary>
/// DPID 4x4 kernel for 4:1 downscaling using deviation-based weighting.
/// </summary>
file readonly struct Dpid4x4Kernel<TWork, TKey, TPixel, TEncode>(float lambda)
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

    // First pass: compute average luminance
    var lumas = stackalloc float[16];
    var avgLuma = 0f;
    for (var i = 0; i < 16; ++i) {
      lumas[i] = ColorConverter.GetLuminance(in colors[i]);
      avgLuma += lumas[i];
    }
    avgLuma /= 16f;

    // Second pass: compute weights based on deviation
    var weights = stackalloc float[16];
    var totalWeight = 0f;
    for (var i = 0; i < 16; ++i) {
      var deviation = MathF.Abs(lumas[i] - avgLuma);
      weights[i] = 1f + lambda * (deviation / 128f);
      totalWeight += weights[i];
    }

    // Normalize and accumulate
    var inv = 1f / totalWeight;
    Accum4F<TWork> acc = default;
    for (var i = 0; i < 16; ++i)
      acc.AddMul(colors[i], weights[i] * inv);

    return encoder.Encode(acc.Result);
  }
}

/// <summary>
/// DPID 5x5 kernel for 5:1 downscaling using deviation-based weighting.
/// </summary>
file readonly struct Dpid5x5Kernel<TWork, TKey, TPixel, TEncode>(float lambda)
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

    // First pass: compute average luminance
    var lumas = stackalloc float[25];
    var avgLuma = 0f;
    for (var i = 0; i < 25; ++i) {
      lumas[i] = ColorConverter.GetLuminance(in colors[i]);
      avgLuma += lumas[i];
    }
    avgLuma /= 25f;

    // Second pass: compute weights based on deviation
    var weights = stackalloc float[25];
    var totalWeight = 0f;
    for (var i = 0; i < 25; ++i) {
      var deviation = MathF.Abs(lumas[i] - avgLuma);
      weights[i] = 1f + lambda * (deviation / 128f);
      totalWeight += weights[i];
    }

    // Normalize and accumulate
    var inv = 1f / totalWeight;
    Accum4F<TWork> acc = default;
    for (var i = 0; i < 25; ++i)
      acc.AddMul(colors[i], weights[i] * inv);

    return encoder.Encode(acc.Result);
  }
}

#endregion
