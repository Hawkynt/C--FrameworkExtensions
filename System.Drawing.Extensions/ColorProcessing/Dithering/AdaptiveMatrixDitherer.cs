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
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Dithering;

/// <summary>
/// Specifies the content type for adaptive dithering strategy selection.
/// </summary>
public enum ContentStrategy {
  /// <summary>For edges, text, and structural elements - preserve sharpness.</summary>
  StructurePreserving,
  /// <summary>For smooth gradients and sky - minimize visible patterns.</summary>
  SmoothGradient,
  /// <summary>For complex textures - enhance detail and avoid loss.</summary>
  DetailEnhancing,
  /// <summary>For very dark or very bright regions - special handling.</summary>
  ExtremeLuminance,
  /// <summary>Balanced approach for mixed content.</summary>
  Balanced
}

/// <summary>
/// Configuration for adaptive matrix dithering.
/// </summary>
public sealed class AdaptiveMatrixConfig {
  /// <summary>Edge detection threshold (0-255 scale).</summary>
  public float EdgeThreshold { get; set; } = 25f;

  /// <summary>Gradient stretch factor for smooth areas.</summary>
  public float GradientStretchFactor { get; set; } = 2.0f;

  /// <summary>Edge stretch factor for edge-adjacent areas.</summary>
  public float EdgeStretchFactor { get; set; } = 0.5f;

  /// <summary>Edge compression factor for edge pixels.</summary>
  public float EdgeCompressionFactor { get; set; } = 0.7f;

  /// <summary>Detail stretch factor for textured areas.</summary>
  public float DetailStretchFactor { get; set; } = 1.5f;

  /// <summary>Default configuration.</summary>
  public static AdaptiveMatrixConfig Default { get; } = new();

  /// <summary>Aggressive configuration with stronger adaptation.</summary>
  public static AdaptiveMatrixConfig Aggressive { get; } = new() {
    GradientStretchFactor = 3.0f,
    EdgeCompressionFactor = 0.5f,
    DetailStretchFactor = 2.0f
  };

  /// <summary>Conservative configuration with subtler adaptation.</summary>
  public static AdaptiveMatrixConfig Conservative { get; } = new() {
    GradientStretchFactor = 1.5f,
    EdgeCompressionFactor = 0.8f,
    DetailStretchFactor = 1.2f
  };
}

/// <summary>
/// Adaptive matrix dithering with edge-aware error diffusion blocking.
/// </summary>
/// <remarks>
/// <para>This ditherer dynamically adapts error diffusion based on local image content:</para>
/// <list type="bullet">
///   <item><description>Multi-scale edge detection using Sobel operators</description></item>
///   <item><description>Gradient direction analysis for oriented error diffusion</description></item>
///   <item><description>Content classification into structure/gradient/detail/luminance regions</description></item>
///   <item><description>Adaptive matrix scaling based on content type</description></item>
///   <item><description>Edge-aware blocking to prevent error diffusion across edges</description></item>
/// </list>
/// </remarks>
[Ditherer("Adaptive Matrix", Description = "Edge-aware adaptive matrix scaling with diffusion blocking", Type = DitheringType.ErrorDiffusion)]
public readonly struct AdaptiveMatrixDitherer : IDitherer {

  private readonly AdaptiveMatrixConfig _config;

  /// <summary>Pre-configured instance with default settings.</summary>
  public static AdaptiveMatrixDitherer Default { get; } = new(AdaptiveMatrixConfig.Default);

  /// <summary>Pre-configured instance with aggressive settings.</summary>
  public static AdaptiveMatrixDitherer Aggressive { get; } = new(AdaptiveMatrixConfig.Aggressive);

  /// <summary>Pre-configured instance with conservative settings.</summary>
  public static AdaptiveMatrixDitherer Conservative { get; } = new(AdaptiveMatrixConfig.Conservative);

  /// <summary>
  /// Creates an adaptive matrix ditherer with the specified configuration.
  /// </summary>
  /// <param name="config">The configuration to use.</param>
  public AdaptiveMatrixDitherer(AdaptiveMatrixConfig? config = null)
    => this._config = config ?? AdaptiveMatrixConfig.Default;

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

    var lookup = new PaletteLookup<TWork, TMetric>(palette, metric);
    var endY = startY + height;
    var localHeight = height;

    // Store source pixels in normalized form for analysis
    var sourceColors = new (float c1, float c2, float c3, float a)[localHeight, width];
    var grayscale = new float[localHeight, width];

    for (var y = startY; y < endY; ++y)
    for (var x = 0; x < width; ++x) {
      var pixel = decoder.Decode(source[y * sourceStride + x]);
      var (c1, c2, c3, alpha) = pixel.ToNormalized();
      var c1f = c1.ToFloat();
      var c2f = c2.ToFloat();
      var c3f = c3.ToFloat();
      sourceColors[y - startY, x] = (c1f, c2f, c3f, alpha.ToFloat());
      grayscale[y - startY, x] = 0.299f * c1f + 0.587f * c2f + 0.114f * c3f;
    }

    // Precompute palette colors
    var paletteColors = new (float c1, float c2, float c3, float a)[palette.Length];
    for (var i = 0; i < palette.Length; ++i) {
      var (c1, c2, c3, a) = palette[i].ToNormalized();
      paletteColors[i] = (c1.ToFloat(), c2.ToFloat(), c3.ToFloat(), a.ToFloat());
    }

    // Create analysis maps
    var edgeMap = _CreateEdgeMap(grayscale, width, localHeight, this._config.EdgeThreshold / 255f);
    var gradientMap = _CreateGradientDirectionMap(grayscale, width, localHeight);
    var contentMap = _AnalyzeContent(grayscale, sourceColors, width, localHeight);

    // Error buffers
    var errorC1 = new float[width, localHeight];
    var errorC2 = new float[width, localHeight];
    var errorC3 = new float[width, localHeight];

    // Process each pixel
    for (var y = startY; y < endY; ++y) {
      var localY = y - startY;

      for (var x = 0; x < width; ++x) {
        var pixel = sourceColors[localY, x];

        // Apply accumulated error
        var newC1 = Math.Max(0f, Math.Min(1f, pixel.c1 + errorC1[x, localY]));
        var newC2 = Math.Max(0f, Math.Min(1f, pixel.c2 + errorC2[x, localY]));
        var newC3 = Math.Max(0f, Math.Min(1f, pixel.c3 + errorC3[x, localY]));

        var correctedColor = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped(newC1),
          UNorm32.FromFloatClamped(newC2),
          UNorm32.FromFloatClamped(newC3),
          UNorm32.FromFloatClamped(pixel.a)
        );

        var closestIndex = lookup.FindNearest(correctedColor);
        indices[y * targetStride + x] = (byte)closestIndex;

        var closestColor = paletteColors[closestIndex];

        // Calculate quantization error
        var errC1 = newC1 - closestColor.c1;
        var errC2 = newC2 - closestColor.c2;
        var errC3 = newC3 - closestColor.c3;

        // Get adaptive parameters
        var strategy = contentMap[x, localY];
        var isNearEdge = edgeMap[x, localY];
        var gradientDirection = gradientMap[x, localY];

        // Create adaptive matrix and distribute error
        _DistributeAdaptiveError(
          errorC1, errorC2, errorC3,
          errC1, errC2, errC3,
          x, localY, width, localHeight,
          strategy, isNearEdge, gradientDirection,
          edgeMap, this._config
        );
      }
    }
  }

  private static bool[,] _CreateEdgeMap(float[,] grayscale, int width, int height, float threshold) {
    var edgeMap = new bool[width, height];
    var scales = new[] { 1, 2, 3 };

    for (var y = 1; y < height - 1; ++y)
    for (var x = 1; x < width - 1; ++x) {
      var totalStrength = 0f;

      foreach (var scale in scales) {
        if (x < scale || y < scale || x >= width - scale || y >= height - scale)
          continue;

        var gx = 0f;
        var gy = 0f;

        for (var dy = -scale; dy <= scale; ++dy)
        for (var dx = -scale; dx <= scale; ++dx) {
          var pixel = grayscale[y + dy, x + dx];
          var sobelX = dx == 0 ? 0 : (dx > 0 ? 1 : -1) * (Math.Abs(dy) == scale ? 1 : 2);
          var sobelY = dy == 0 ? 0 : (dy > 0 ? 1 : -1) * (Math.Abs(dx) == scale ? 1 : 2);
          gx += pixel * sobelX;
          gy += pixel * sobelY;
        }

        totalStrength += (float)Math.Sqrt(gx * gx + gy * gy) / (scale * scale);
      }

      edgeMap[x, y] = totalStrength > threshold;
    }

    return edgeMap;
  }

  private static float[,] _CreateGradientDirectionMap(float[,] grayscale, int width, int height) {
    var gradientMap = new float[width, height];

    for (var y = 1; y < height - 1; ++y)
    for (var x = 1; x < width - 1; ++x) {
      var gx = 0f;
      var gy = 0f;

      for (var dy = -1; dy <= 1; ++dy)
      for (var dx = -1; dx <= 1; ++dx) {
        var pixel = grayscale[y + dy, x + dx];
        var sobelX = dx * (Math.Abs(dy) == 1 ? 1 : 2);
        var sobelY = dy * (Math.Abs(dx) == 1 ? 1 : 2);
        gx += pixel * sobelX;
        gy += pixel * sobelY;
      }

      gradientMap[x, y] = (float)Math.Atan2(gy, gx);
    }

    return gradientMap;
  }

  private static ContentStrategy[,] _AnalyzeContent(
    float[,] grayscale,
    (float c1, float c2, float c3, float a)[,] sourceColors,
    int width,
    int height
  ) {
    var strategies = new ContentStrategy[width, height];
    const int blockSize = 8;

    for (var by = 0; by < height; by += blockSize)
    for (var bx = 0; bx < width; bx += blockSize) {
      var blockWidth = Math.Min(blockSize, width - bx);
      var blockHeight = Math.Min(blockSize, height - by);
      var strategy = _AnalyzeBlock(grayscale, sourceColors, bx, by, blockWidth, blockHeight);

      for (var dy = 0; dy < blockHeight; ++dy)
      for (var dx = 0; dx < blockWidth; ++dx)
        strategies[bx + dx, by + dy] = strategy;
    }

    return strategies;
  }

  private static ContentStrategy _AnalyzeBlock(
    float[,] grayscale,
    (float c1, float c2, float c3, float a)[,] sourceColors,
    int startX, int startY,
    int blockWidth, int blockHeight
  ) {
    var edgeStrength = 0f;
    var avgBrightness = 0f;
    var sumC1 = 0f;
    var sumC2 = 0f;
    var sumC3 = 0f;
    var pixelCount = blockWidth * blockHeight;

    for (var y = 0; y < blockHeight; ++y)
    for (var x = 0; x < blockWidth; ++x) {
      var color = sourceColors[startY + y, startX + x];
      sumC1 += color.c1;
      sumC2 += color.c2;
      sumC3 += color.c3;
      avgBrightness += grayscale[startY + y, startX + x];

      // Edge detection
      if (x <= 0 || y <= 0 || x >= blockWidth - 1 || y >= blockHeight - 1 || startX + x <= 0 || startY + y <= 0 || startX + x >= grayscale.GetLength(1) - 1 || startY + y >= grayscale.GetLength(0) - 1)
        continue;

      var tl = grayscale[startY + y - 1, startX + x - 1];
      var tm = grayscale[startY + y - 1, startX + x];
      var tr = grayscale[startY + y - 1, startX + x + 1];
      var ml = grayscale[startY + y, startX + x - 1];
      var mr = grayscale[startY + y, startX + x + 1];
      var bl = grayscale[startY + y + 1, startX + x - 1];
      var bm = grayscale[startY + y + 1, startX + x];
      var br = grayscale[startY + y + 1, startX + x + 1];

      var gx = -tl + tr - 2 * ml + 2 * mr - bl + br;
      var gy = -tl - 2 * tm - tr + bl + 2 * bm + br;
      edgeStrength += (float)Math.Sqrt(gx * gx + gy * gy);
    }

    avgBrightness /= pixelCount;
    edgeStrength /= pixelCount;

    var avgC1 = sumC1 / pixelCount;
    var avgC2 = sumC2 / pixelCount;
    var avgC3 = sumC3 / pixelCount;

    // Calculate color variance
    var colorVariance = 0f;
    for (var y = 0; y < blockHeight; ++y)
    for (var x = 0; x < blockWidth; ++x) {
      var color = sourceColors[startY + y, startX + x];
      var d1 = color.c1 - avgC1;
      var d2 = color.c2 - avgC2;
      var d3 = color.c3 - avgC3;
      colorVariance += d1 * d1 + d2 * d2 + d3 * d3;
    }
    colorVariance = (float)Math.Sqrt(colorVariance / pixelCount);

    // Classify region
    if (edgeStrength > 0.12f)
      return ContentStrategy.StructurePreserving;

    if (colorVariance < 0.08f)
      return ContentStrategy.SmoothGradient;

    if (colorVariance > 0.24f)
      return ContentStrategy.DetailEnhancing;

    if (avgBrightness < 0.12f || avgBrightness > 0.86f)
      return ContentStrategy.ExtremeLuminance;

    return ContentStrategy.Balanced;
  }

  private static void _DistributeAdaptiveError(
    float[,] errorC1, float[,] errorC2, float[,] errorC3,
    float errC1, float errC2, float errC3,
    int x, int y, int width, int height,
    ContentStrategy strategy, bool isNearEdge, float gradientDirection,
    bool[,] edgeMap, AdaptiveMatrixConfig config
  ) {
    // Base Floyd-Steinberg positions and weights
    var positions = new (int dx, int dy, float weight)[] {
      (1, 0, 7f / 16f),
      (-1, 1, 3f / 16f),
      (0, 1, 5f / 16f),
      (1, 1, 1f / 16f)
    };

    // Calculate adaptive scale factors
    float scaleX, scaleY, coeffMod;
    var blockAcrossEdges = false;

    switch (strategy) {
      case ContentStrategy.SmoothGradient:
        var stretchFactor = isNearEdge ? config.EdgeStretchFactor : config.GradientStretchFactor;
        scaleX = (float)(Math.Abs(Math.Cos(gradientDirection)) * stretchFactor + 1.0);
        scaleY = (float)(Math.Abs(Math.Sin(gradientDirection)) * stretchFactor + 1.0);
        coeffMod = 1f;
        blockAcrossEdges = isNearEdge;
        break;

      case ContentStrategy.StructurePreserving:
        scaleX = scaleY = config.EdgeCompressionFactor;
        coeffMod = 0.7f;
        blockAcrossEdges = true;
        break;

      case ContentStrategy.DetailEnhancing:
        scaleX = scaleY = config.DetailStretchFactor;
        coeffMod = 1.3f;
        blockAcrossEdges = false;
        break;

      case ContentStrategy.ExtremeLuminance:
        scaleX = scaleY = isNearEdge ? 0.8f : 1.2f;
        coeffMod = 1f;
        blockAcrossEdges = isNearEdge;
        break;

      default: // Balanced
        scaleX = scaleY = isNearEdge ? 0.9f : 1.1f;
        coeffMod = 1f;
        blockAcrossEdges = isNearEdge;
        break;
    }

    foreach (var (dx, dy, weight) in positions) {
      var scaledDx = (int)Math.Round(dx * scaleX);
      var scaledDy = (int)Math.Round(dy * scaleY);

      if (scaledDx == 0 && scaledDy == 0)
        continue;

      var newX = x + scaledDx;
      var newY = y + scaledDy;

      // Bounds check
      if (newX < 0 || newX >= width || newY < 0 || newY >= height)
        continue;

      // Edge-aware blocking
      if (blockAcrossEdges && _WouldCrossEdge(x, y, newX, newY, edgeMap, width, height))
        continue;

      var scaledWeight = weight * coeffMod;
      errorC1[newX, newY] += errC1 * scaledWeight;
      errorC2[newX, newY] += errC2 * scaledWeight;
      errorC3[newX, newY] += errC3 * scaledWeight;
    }
  }

  private static bool _WouldCrossEdge(int x1, int y1, int x2, int y2, bool[,] edgeMap, int width, int height) {
    var dx = Math.Abs(x2 - x1);
    var dy = Math.Abs(y2 - y1);

    if (dx <= 1 && dy <= 1) {
      // Adjacent pixels - check if either is an edge
      if (x1 >= 0 && x1 < width && y1 >= 0 && y1 < height && edgeMap[x1, y1])
        return true;
      if (x2 >= 0 && x2 < width && y2 >= 0 && y2 < height && edgeMap[x2, y2])
        return true;
      return false;
    }

    // For longer distances, check intermediate points
    var steps = Math.Max(dx, dy);
    for (var i = 1; i < steps; ++i) {
      var checkX = x1 + (x2 - x1) * i / steps;
      var checkY = y1 + (y2 - y1) * i / steps;

      if (checkX >= 0 && checkX < width && checkY >= 0 && checkY < height && edgeMap[checkX, checkY])
        return true;
    }

    return false;
  }
}
