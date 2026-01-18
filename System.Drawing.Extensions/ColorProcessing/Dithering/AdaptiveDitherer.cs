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
/// Adaptive dithering system that automatically selects the best dithering algorithm
/// based on image characteristics analysis.
/// </summary>
/// <remarks>
/// <para>Analyzes image for color complexity, edge density, gradient smoothness, and noise.</para>
/// <para>Supports multiple strategies: Quality, Balanced, Performance, and Smart selection.</para>
/// <para>Delegates to appropriate ditherer based on analysis results.</para>
/// </remarks>
[Ditherer("Adaptive", Description = "Auto-selects optimal dithering algorithm based on image analysis", Type = DitheringType.Custom)]
public readonly struct AdaptiveDitherer : IDitherer {

  /// <summary>
  /// Strategy for selecting the dithering algorithm.
  /// </summary>
  public enum AdaptiveStrategy {
    /// <summary>Prioritize visual quality over performance.</summary>
    QualityOptimized,
    /// <summary>Balance quality and performance.</summary>
    Balanced,
    /// <summary>Prioritize performance over quality.</summary>
    PerformanceOptimized,
    /// <summary>Use smart scoring to select optimal algorithm.</summary>
    SmartSelection
  }

  /// <summary>
  /// Image characteristics used for algorithm selection.
  /// </summary>
  public readonly struct ImageCharacteristics {
    public double ColorComplexity { get; init; }
    public double EdgeDensity { get; init; }
    public double GradientSmoothness { get; init; }
    public double NoiseLevel { get; init; }
    public double DetailLevel { get; init; }
    public int ImageSize { get; init; }
    public int PaletteSize { get; init; }
  }

  private readonly AdaptiveStrategy _strategy;

  /// <summary>Pre-configured instance optimized for quality.</summary>
  public static AdaptiveDitherer QualityOptimized { get; } = new(AdaptiveStrategy.QualityOptimized);

  /// <summary>Pre-configured instance with balanced settings.</summary>
  public static AdaptiveDitherer Balanced { get; } = new(AdaptiveStrategy.Balanced);

  /// <summary>Pre-configured instance optimized for performance.</summary>
  public static AdaptiveDitherer PerformanceOptimized { get; } = new(AdaptiveStrategy.PerformanceOptimized);

  /// <summary>Pre-configured instance using smart algorithm selection.</summary>
  public static AdaptiveDitherer SmartSelection { get; } = new(AdaptiveStrategy.SmartSelection);

  /// <summary>
  /// Creates an adaptive ditherer with the specified strategy.
  /// </summary>
  /// <param name="strategy">The strategy for selecting dithering algorithms.</param>
  public AdaptiveDitherer(AdaptiveStrategy strategy = AdaptiveStrategy.SmartSelection)
    => this._strategy = strategy;

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

    if (palette.Length == 0)
      return;

    var characteristics = _AnalyzeImage(source, width, height, sourceStride, startY, decoder, palette);
    var selectedDitherer = this._strategy switch {
      AdaptiveStrategy.QualityOptimized => _SelectForQuality(characteristics),
      AdaptiveStrategy.Balanced => _SelectForBalance(characteristics),
      AdaptiveStrategy.PerformanceOptimized => _SelectForPerformance(characteristics),
      AdaptiveStrategy.SmartSelection => _SelectSmart(characteristics),
      _ => ErrorDiffusion.FloydSteinberg
    };
    selectedDitherer.Dither(source, indices, width, height, sourceStride, targetStride, startY, decoder, metric, palette);
  }

  private static unsafe ImageCharacteristics _AnalyzeImage<TWork, TPixel, TDecode>(
    TPixel* source,
    int width,
    int height,
    int stride,
    int startY,
    in TDecode decoder,
    TWork[] palette)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork> {

    var totalPixels = width * height;
    var sampleSize = Math.Max(1, Math.Min(10000, totalPixels / 4));
    var sampleStep = Math.Max(1, totalPixels / sampleSize);
    var stepY = Math.Max(1, (int)Math.Sqrt(sampleStep));
    var stepX = Math.Max(1, (int)Math.Sqrt(sampleStep));
    var endY = startY + height;

    var colorSet = new HashSet<int>();
    var edgeCount = 0;
    var gradientVariance = 0.0;
    var noiseSum = 0.0;
    var detailSum = 0.0;
    var sampleCount = 0;

    for (var y = startY; y < endY; y += stepY)
    for (var x = 0; x < width; x += stepX) {
      var pixel = decoder.Decode(source[y * stride + x]);
      var (c1, c2, c3, _) = pixel.ToNormalized();

      // Create a hash from normalized color
      var hash = ((int)(c1.ToFloat() * 255) << 16) | ((int)(c2.ToFloat() * 255) << 8) | (int)(c3.ToFloat() * 255);
      colorSet.Add(hash);

      if (x > 0 && y > startY && x < width - 1 && y < endY - 1) {
        var edgeStrength = _CalculateEdgeStrength<TWork, TPixel, TDecode>(source, stride, x, y, decoder);
        if (edgeStrength > 0.12f) ++edgeCount;
        detailSum += edgeStrength;
      }

      if (x > 1 && y > startY + 1 && x < width - 2 && y < endY - 2) {
        var localVariance = _CalculateLocalVariance<TWork, TPixel, TDecode>(source, stride, x, y, decoder);
        gradientVariance += localVariance;
      }

      if (x > 0 && y > startY) {
        var prevPixel = decoder.Decode(source[(y - 1) * stride + (x - 1)]);
        var (pc1, pc2, pc3, _) = prevPixel.ToNormalized();
        var colorDistance = Math.Abs(c1.ToFloat() - pc1.ToFloat()) +
                            Math.Abs(c2.ToFloat() - pc2.ToFloat()) +
                            Math.Abs(c3.ToFloat() - pc3.ToFloat());
        noiseSum += colorDistance;
      }

      ++sampleCount;
    }

    var colorComplexity = Math.Min(1.0, colorSet.Count / (double)Math.Min(1000, totalPixels / 10));
    var edgeDensity = edgeCount / (double)sampleCount;
    var gradientSmoothness = 1.0 - Math.Min(1.0, gradientVariance / (sampleCount * 0.1));
    var noiseLevel = Math.Min(1.0, noiseSum / (sampleCount * 3.0));
    var detailLevel = Math.Min(1.0, detailSum / (sampleCount * 0.4));

    return new ImageCharacteristics {
      ColorComplexity = colorComplexity,
      EdgeDensity = edgeDensity,
      GradientSmoothness = gradientSmoothness,
      NoiseLevel = noiseLevel,
      DetailLevel = detailLevel,
      ImageSize = totalPixels,
      PaletteSize = palette.Length
    };
  }

  private static unsafe float _CalculateEdgeStrength<TWork, TPixel, TDecode>(
    TPixel* source,
    int stride,
    int x, int y,
    in TDecode decoder)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork> {

    var center = decoder.Decode(source[y * stride + x]);
    var right = decoder.Decode(source[y * stride + x + 1]);
    var down = decoder.Decode(source[(y + 1) * stride + x]);
    var diag = decoder.Decode(source[(y + 1) * stride + x + 1]);

    var (cc1, cc2, cc3, _) = center.ToNormalized();
    var (rc1, rc2, rc3, _) = right.ToNormalized();
    var (dc1, dc2, dc3, _) = down.ToNormalized();
    var (dgc1, dgc2, dgc3, _) = diag.ToNormalized();

    var horizontalGrad = Math.Abs(cc1.ToFloat() - rc1.ToFloat()) + Math.Abs(cc2.ToFloat() - rc2.ToFloat()) + Math.Abs(cc3.ToFloat() - rc3.ToFloat());
    var verticalGrad = Math.Abs(cc1.ToFloat() - dc1.ToFloat()) + Math.Abs(cc2.ToFloat() - dc2.ToFloat()) + Math.Abs(cc3.ToFloat() - dc3.ToFloat());
    var diagonalGrad = Math.Abs(cc1.ToFloat() - dgc1.ToFloat()) + Math.Abs(cc2.ToFloat() - dgc2.ToFloat()) + Math.Abs(cc3.ToFloat() - dgc3.ToFloat());

    return (float)Math.Sqrt(horizontalGrad * horizontalGrad + verticalGrad * verticalGrad + diagonalGrad * diagonalGrad);
  }

  private static unsafe float _CalculateLocalVariance<TWork, TPixel, TDecode>(
    TPixel* source,
    int stride,
    int x, int y,
    in TDecode decoder)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork> {

    var values = new float[9];
    var index = 0;
    var sum = 0f;

    for (var dy = -1; dy <= 1; ++dy)
    for (var dx = -1; dx <= 1; ++dx) {
      var pixel = decoder.Decode(source[(y + dy) * stride + (x + dx)]);
      var (c1, c2, c3, _) = pixel.ToNormalized();
      var luminance = 0.299f * c1.ToFloat() + 0.587f * c2.ToFloat() + 0.114f * c3.ToFloat();
      values[index++] = luminance;
      sum += luminance;
    }

    var mean = sum / 9f;
    var variance = 0f;
    for (var i = 0; i < 9; ++i) {
      var diff = values[i] - mean;
      variance += diff * diff;
    }

    return variance / 9f;
  }

  private static IDitherer _SelectForQuality(ImageCharacteristics characteristics) {
    if (characteristics is { GradientSmoothness: > 0.7, EdgeDensity: < 0.3 })
      return characteristics.PaletteSize > 64
        ? KnollDitherer.HighQuality
        : RiemersmaDitherer.Large;

    if (characteristics.DetailLevel > 0.6 || characteristics.EdgeDensity > 0.5)
      return characteristics.ColorComplexity switch {
        > 0.8 => ErrorDiffusion.StevensonArce,
        > 0.6 => ErrorDiffusion.JarvisJudiceNinke,
        > 0.4 => ErrorDiffusion.Stucki,
        _ => ErrorDiffusion.Sierra
      };

    if (characteristics.ColorComplexity > 0.8)
      return NConvexDitherer.Default;

    if (characteristics.NoiseLevel > 0.4)
      return NoiseDitherer.BlueNoise;

    return ErrorDiffusion.FloydSteinberg;
  }

  private static IDitherer _SelectForBalance(ImageCharacteristics characteristics) {
    if (characteristics.ImageSize > 1000000)
      return characteristics.GradientSmoothness > 0.6
        ? OrderedDitherer.Bayer8x8
        : characteristics.DetailLevel switch {
          > 0.7 => ErrorDiffusion.TwoRowSierra,
          > 0.4 => ErrorDiffusion.Atkinson,
          _ => ErrorDiffusion.SierraLite
        };

    if (characteristics is { ColorComplexity: > 0.5, DetailLevel: > 0.4 })
      return characteristics.EdgeDensity switch {
        > 0.6 => ErrorDiffusion.Burkes,
        > 0.3 => ErrorDiffusion.FloydSteinberg,
        _ => ErrorDiffusion.FalseFloydSteinberg
      };

    if (characteristics.ColorComplexity < 0.3)
      return OrderedDitherer.Bayer4x4;

    return characteristics.NoiseLevel > 0.3
      ? NoiseDitherer.BlueNoise
      : NClosestDitherer.Default;
  }

  private static IDitherer _SelectForPerformance(ImageCharacteristics characteristics) {
    if (characteristics.GradientSmoothness > 0.5)
      return OrderedDitherer.Bayer8x8;

    if (characteristics.DetailLevel > 0.6)
      return characteristics.ImageSize switch {
        > 2000000 => ErrorDiffusion.Simple,
        > 500000 => ErrorDiffusion.SierraLite,
        _ => ErrorDiffusion.Atkinson
      };

    if (characteristics.NoiseLevel > 0.4)
      return NoiseDitherer.WhiteNoise;

    return OrderedDitherer.Bayer4x4;
  }

  private static IDitherer _SelectSmart(ImageCharacteristics characteristics) {
    var candidates = new Dictionary<IDitherer, double>();

    var fsScore = 0.7 + characteristics.DetailLevel * 0.3;
    candidates[ErrorDiffusion.FloydSteinberg] = fsScore;

    var jjnScore = characteristics.DetailLevel * 0.8 + characteristics.EdgeDensity * 0.4;
    if (characteristics.ImageSize > 1000000) jjnScore *= 0.8;
    candidates[ErrorDiffusion.JarvisJudiceNinke] = jjnScore;

    var stuckiScore = characteristics.DetailLevel * 0.6 + characteristics.ColorComplexity * 0.4;
    if (characteristics.ImageSize > 1000000) stuckiScore *= 0.9;
    candidates[ErrorDiffusion.Stucki] = stuckiScore;

    var atkinsonScore = characteristics.EdgeDensity * 0.7 + (1.0 - characteristics.GradientSmoothness) * 0.5;
    if (characteristics.ImageSize > 1000000) atkinsonScore *= 1.1;
    candidates[ErrorDiffusion.Atkinson] = atkinsonScore;

    var sierraScore = characteristics.DetailLevel * 0.5 + characteristics.ColorComplexity * 0.3;
    candidates[ErrorDiffusion.Sierra] = sierraScore;
    candidates[ErrorDiffusion.TwoRowSierra] = sierraScore * 1.1;
    candidates[ErrorDiffusion.SierraLite] = sierraScore * 1.2;

    var burkesScore = characteristics.EdgeDensity * 0.8 + characteristics.DetailLevel * 0.4;
    candidates[ErrorDiffusion.Burkes] = burkesScore;

    var stevensonScore = characteristics.ColorComplexity * 0.6 + characteristics.DetailLevel * 0.5;
    if (characteristics.ImageSize > 500000) stevensonScore *= 0.6;
    candidates[ErrorDiffusion.StevensonArce] = stevensonScore;

    if (characteristics.ImageSize > 2000000) {
      var simpleScore = 0.8 + (1.0 - characteristics.ColorComplexity) * 0.3;
      candidates[ErrorDiffusion.Simple] = simpleScore;
      candidates[ErrorDiffusion.FalseFloydSteinberg] = simpleScore * 0.9;
    }

    var orderScore = characteristics.GradientSmoothness * 0.8 +
                     (1.0 - characteristics.ColorComplexity) * 0.2;
    candidates[OrderedDitherer.Bayer8x8] = orderScore;

    var noiseScore = characteristics.NoiseLevel * 0.5 +
                     characteristics.ColorComplexity * 0.3 +
                     (1.0 - characteristics.EdgeDensity) * 0.2;
    candidates[NoiseDitherer.BlueNoise] = noiseScore;

    var knollScore = characteristics.ColorComplexity * 0.4 +
                     characteristics.DetailLevel * 0.3 +
                     characteristics.GradientSmoothness * 0.3;
    if (characteristics.ImageSize > 500000) knollScore *= 0.7;
    candidates[KnollDitherer.Default] = knollScore;

    var nClosestScore = characteristics.ColorComplexity * 0.6 +
                        (1.0 - characteristics.NoiseLevel) * 0.4;
    candidates[NClosestDitherer.Default] = nClosestScore;

    var riemersmaScore = characteristics.DetailLevel * 0.5 +
                         characteristics.EdgeDensity * 0.3 +
                         (1.0 - characteristics.NoiseLevel) * 0.2;
    candidates[RiemersmaDitherer.Default] = riemersmaScore;

    // Find best candidate
    IDitherer? bestDitherer = null;
    var bestScore = double.MinValue;
    foreach (var (ditherer, score) in candidates) {
      if (score > bestScore) {
        bestScore = score;
        bestDitherer = ditherer;
      }
    }

    return bestDitherer ?? ErrorDiffusion.FloydSteinberg;
  }
}
