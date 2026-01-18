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
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Dithering;

/// <summary>
/// Different dithering strategies for different types of image content.
/// </summary>
public enum DitheringStrategy {
  /// <summary>Preserve structural edges and boundaries.</summary>
  StructurePreserving,
  /// <summary>Optimize for smooth gradient transitions.</summary>
  SmoothGradient,
  /// <summary>Enhance fine details in textured regions.</summary>
  DetailEnhancing,
  /// <summary>Handle very dark or very bright regions.</summary>
  ExtremeLuminance,
  /// <summary>Balanced approach for typical content.</summary>
  Balanced
}

/// <summary>
/// Configuration for smart dithering strategies.
/// </summary>
public sealed class SmartDitheringConfig {
  /// <summary>Ditherer used for structure-preserving regions.</summary>
  public IDitherer StructurePreservingDitherer { get; init; } = ErrorDiffusion.Atkinson;

  /// <summary>Ditherer used for smooth gradient regions.</summary>
  public IDitherer SmoothGradientDitherer { get; init; } = ErrorDiffusion.Stucki;

  /// <summary>Ditherer used for detail-enhancing regions.</summary>
  public IDitherer DetailEnhancingDitherer { get; init; } = ErrorDiffusion.JarvisJudiceNinke;

  /// <summary>Ditherer used for extreme luminance regions.</summary>
  public IDitherer ExtremeLuminanceDitherer { get; init; } = ErrorDiffusion.FloydSteinberg;

  /// <summary>Ditherer used for balanced regions.</summary>
  public IDitherer BalancedDitherer { get; init; } = ErrorDiffusion.FloydSteinberg;

  /// <summary>Default configuration.</summary>
  public static SmartDitheringConfig Default { get; } = new();

  /// <summary>High quality configuration with specialized ditherers.</summary>
  public static SmartDitheringConfig HighQuality { get; } = new() {
    StructurePreservingDitherer = StructureAwareDitherer.Default,
    SmoothGradientDitherer = ErrorDiffusion.Stucki,
    DetailEnhancingDitherer = ErrorDiffusion.JarvisJudiceNinke,
    ExtremeLuminanceDitherer = ErrorDiffusion.Sierra,
    BalancedDitherer = ErrorDiffusion.FloydSteinberg
  };

  /// <summary>Fast configuration with simpler ditherers.</summary>
  public static SmartDitheringConfig Fast { get; } = new() {
    StructurePreservingDitherer = ErrorDiffusion.Simple,
    SmoothGradientDitherer = ErrorDiffusion.FalseFloydSteinberg,
    DetailEnhancingDitherer = ErrorDiffusion.FloydSteinberg,
    ExtremeLuminanceDitherer = ErrorDiffusion.Simple,
    BalancedDitherer = ErrorDiffusion.FalseFloydSteinberg
  };
}

/// <summary>
/// Smart content-aware ditherer that applies different algorithms to different image regions.
/// </summary>
/// <remarks>
/// <para>Analyzes image content to classify regions by edge strength, color variance, and brightness.</para>
/// <para>Applies specialized ditherers to different region types for optimal results.</para>
/// <para>Composites results from multiple ditherers based on region classification.</para>
/// </remarks>
[Ditherer("Smart", Description = "Content-aware region-based dithering", Type = DitheringType.Custom)]
public readonly struct SmartDitherer : IDitherer {

  private readonly SmartDitheringConfig _config;

  /// <summary>Pre-configured instance with default settings.</summary>
  public static SmartDitherer Default { get; } = new(SmartDitheringConfig.Default);

  /// <summary>Pre-configured instance with high quality settings.</summary>
  public static SmartDitherer HighQuality { get; } = new(SmartDitheringConfig.HighQuality);

  /// <summary>Pre-configured instance with fast settings.</summary>
  public static SmartDitherer Fast { get; } = new(SmartDitheringConfig.Fast);

  /// <summary>
  /// Creates a smart ditherer with the specified configuration.
  /// </summary>
  /// <param name="config">Configuration for dithering strategies.</param>
  public SmartDitherer(SmartDitheringConfig? config = null) => this._config = config ?? SmartDitheringConfig.Default;

  /// <inheritdoc />
  public bool RequiresSequentialProcessing => true;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Dither<TWork, TPixel, TDecode, TMetric>(
    TPixel* source,
    byte* indices,
    int width,
    int height,
    int sourceStride,
    int targetStride,
    int startY,
    in TDecode decoder,
    in TMetric metric,
    TWork[] palette)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TMetric : struct, IColorMetric<TWork> {

    // Handle default struct initialization (_config = null)
    var config = this._config ?? SmartDitheringConfig.Default;
    var endY = startY + height;

    // Analyze image to create strategy map
    var strategyMap = _AnalyzeImage<TWork, TPixel, TDecode>(source, width, height, sourceStride, startY, decoder);

    // Apply all ditherers
    var buffers = new Dictionary<DitheringStrategy, byte[]>();
    var ditherers = new Dictionary<DitheringStrategy, IDitherer> {
      [DitheringStrategy.StructurePreserving] = config.StructurePreservingDitherer,
      [DitheringStrategy.SmoothGradient] = config.SmoothGradientDitherer,
      [DitheringStrategy.DetailEnhancing] = config.DetailEnhancingDitherer,
      [DitheringStrategy.ExtremeLuminance] = config.ExtremeLuminanceDitherer,
      [DitheringStrategy.Balanced] = config.BalancedDitherer
    };

    foreach (var (strategy, ditherer) in ditherers) {
      var buffer = new byte[width * height];
      fixed (byte* bufferPtr = buffer) {
        ditherer.Dither(source, bufferPtr, width, height, sourceStride, width, startY, decoder, metric, palette);
      }
      buffers[strategy] = buffer;
    }

    // Composite results
    for (var y = startY; y < endY; ++y) {
      var localY = y - startY;
      for (var x = 0; x < width; ++x) {
        var strategy = strategyMap[x, localY];
        var localIdx = localY * width + x;
        indices[y * targetStride + x] = buffers.TryGetValue(strategy, out var buffer)
          ? buffer[localIdx]
          : buffers[DitheringStrategy.Balanced][localIdx];
      }
    }
  }

  private static unsafe DitheringStrategy[,] _AnalyzeImage<TWork, TPixel, TDecode>(
    TPixel* source,
    int width,
    int height,
    int stride,
    int startY,
    in TDecode decoder)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork> {

    var strategies = new DitheringStrategy[width, height];
    const int blockSize = 8;

    for (var blockY = 0; blockY < height; blockY += blockSize)
    for (var blockX = 0; blockX < width; blockX += blockSize) {
      var blockW = Math.Min(blockSize, width - blockX);
      var blockH = Math.Min(blockSize, height - blockY);

      var strategy = _AnalyzeBlock<TWork, TPixel, TDecode>(source, stride, blockX, startY + blockY, blockW, blockH, decoder);

      for (var y = blockY; y < blockY + blockH; ++y)
      for (var x = blockX; x < blockX + blockW; ++x)
        strategies[x, y] = strategy;
    }

    _ApplyRegionSmoothing(strategies, width, height);

    return strategies;
  }

  private static unsafe DitheringStrategy _AnalyzeBlock<TWork, TPixel, TDecode>(
    TPixel* source,
    int stride,
    int startX, int startY,
    int blockWidth, int blockHeight,
    in TDecode decoder)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork> {

    var edgeStrength = 0.0;
    var colorVariance = 0.0;
    var avgBrightness = 0.0;
    var pixelCount = blockWidth * blockHeight;

    var sumC1 = 0f;
    var sumC2 = 0f;
    var sumC3 = 0f;

    // First pass: calculate averages
    for (var y = 0; y < blockHeight; ++y)
    for (var x = 0; x < blockWidth; ++x) {
      var pixel = decoder.Decode(source[(startY + y) * stride + (startX + x)]);
      var (c1, c2, c3, _) = pixel.ToNormalized();
      sumC1 += c1.ToFloat();
      sumC2 += c2.ToFloat();
      sumC3 += c3.ToFloat();
      avgBrightness += 0.299 * c1.ToFloat() + 0.587 * c2.ToFloat() + 0.114 * c3.ToFloat();
    }

    var avgC1 = sumC1 / pixelCount;
    var avgC2 = sumC2 / pixelCount;
    var avgC3 = sumC3 / pixelCount;
    avgBrightness = avgBrightness / pixelCount * 255.0;

    // Second pass: calculate variance and edges
    for (var y = 0; y < blockHeight; ++y)
    for (var x = 0; x < blockWidth; ++x) {
      var pixel = decoder.Decode(source[(startY + y) * stride + (startX + x)]);
      var (c1, c2, c3, _) = pixel.ToNormalized();

      var diffC1 = c1.ToFloat() - avgC1;
      var diffC2 = c2.ToFloat() - avgC2;
      var diffC3 = c3.ToFloat() - avgC3;
      colorVariance += diffC1 * diffC1 + diffC2 * diffC2 + diffC3 * diffC3;

      if (x > 0 && y > 0 && x < blockWidth - 1 && y < blockHeight - 1)
        edgeStrength += _CalculateEdgeStrength<TWork, TPixel, TDecode>(source, stride, startX + x, startY + y, decoder);
    }

    colorVariance = Math.Sqrt(colorVariance / pixelCount) * 255.0;
    edgeStrength = edgeStrength / Math.Max(1, (blockWidth - 2) * (blockHeight - 2)) * 255.0;

    return _ClassifyRegion(edgeStrength, colorVariance, avgBrightness);
  }

  private static unsafe double _CalculateEdgeStrength<TWork, TPixel, TDecode>(
    TPixel* source,
    int stride,
    int x, int y,
    in TDecode decoder)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork> {

    var tl = _GetGrayscale<TWork, TPixel, TDecode>(decoder.Decode(source[(y - 1) * stride + (x - 1)]));
    var tm = _GetGrayscale<TWork, TPixel, TDecode>(decoder.Decode(source[(y - 1) * stride + x]));
    var tr = _GetGrayscale<TWork, TPixel, TDecode>(decoder.Decode(source[(y - 1) * stride + (x + 1)]));
    var ml = _GetGrayscale<TWork, TPixel, TDecode>(decoder.Decode(source[y * stride + (x - 1)]));
    var mr = _GetGrayscale<TWork, TPixel, TDecode>(decoder.Decode(source[y * stride + (x + 1)]));
    var bl = _GetGrayscale<TWork, TPixel, TDecode>(decoder.Decode(source[(y + 1) * stride + (x - 1)]));
    var bm = _GetGrayscale<TWork, TPixel, TDecode>(decoder.Decode(source[(y + 1) * stride + x]));
    var br = _GetGrayscale<TWork, TPixel, TDecode>(decoder.Decode(source[(y + 1) * stride + (x + 1)]));

    var gx = -tl + tr - 2 * ml + 2 * mr - bl + br;
    var gy = -tl - 2 * tm - tr + bl + 2 * bm + br;

    return Math.Sqrt(gx * gx + gy * gy);
  }

  private static float _GetGrayscale<TWork, TPixel, TDecode>(TWork pixel)
    where TWork : unmanaged, IColorSpace4<TWork> {
    var (c1, c2, c3, _) = pixel.ToNormalized();
    return (c1.ToFloat() + c2.ToFloat() + c3.ToFloat()) / 3f;
  }

  private static DitheringStrategy _ClassifyRegion(double edgeStrength, double colorVariance, double avgBrightness) {
    if (edgeStrength > 30)
      return DitheringStrategy.StructurePreserving;

    if (colorVariance < 20)
      return DitheringStrategy.SmoothGradient;

    if (colorVariance > 60)
      return DitheringStrategy.DetailEnhancing;

    if (avgBrightness < 30 || avgBrightness > 220)
      return DitheringStrategy.ExtremeLuminance;

    return DitheringStrategy.Balanced;
  }

  private static void _ApplyRegionSmoothing(DitheringStrategy[,] strategies, int width, int height) {
    var smoothed = new DitheringStrategy[width, height];
    Array.Copy(strategies, smoothed, strategies.Length);

    const int radius = 2;
    var windowSize = (2 * radius + 1) * (2 * radius + 1);

    for (var y = radius; y < height - radius; ++y)
    for (var x = radius; x < width - radius; ++x) {
      var counts = new int[5]; // 5 strategies

      for (var dy = -radius; dy <= radius; ++dy)
      for (var dx = -radius; dx <= radius; ++dx)
        ++counts[(int)strategies[x + dx, y + dy]];

      var maxCount = 0;
      var dominantStrategy = DitheringStrategy.Balanced;

      for (var i = 0; i < counts.Length; ++i)
        if (counts[i] > maxCount) {
          maxCount = counts[i];
          dominantStrategy = (DitheringStrategy)i;
        }

      if (maxCount > windowSize / 2)
        smoothed[x, y] = dominantStrategy;
    }

    Array.Copy(smoothed, strategies, strategies.Length);
  }
}
