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
/// N-Convex dithering using convex hull color mixing strategies.
/// </summary>
/// <remarks>
/// <para>Finds N closest palette colors forming a convex hull in color space.</para>
/// <para>Supports multiple selection strategies: Barycentric, Projection, SpatialPattern, WeightedRandom.</para>
/// <para>Barycentric uses inverse distance weighting; Projection projects onto geometry.</para>
/// </remarks>
[Ditherer("N-Convex", Description = "Convex hull color mixing with multiple strategies", Type = DitheringType.Random)]
public readonly struct NConvexDitherer : IDitherer {

  /// <summary>
  /// Strategy for selecting among N closest colors in convex hull.
  /// </summary>
  public enum ConvexStrategy {
    /// <summary>Select by inverse distance weighting (barycentric-like).</summary>
    Barycentric,
    /// <summary>Project onto line/polygon and select closest vertex.</summary>
    Projection,
    /// <summary>Use spatial position for deterministic pattern.</summary>
    SpatialPattern,
    /// <summary>Weighted random selection based on distance and similarity.</summary>
    WeightedRandom
  }

  private readonly int _n;
  private readonly ConvexStrategy _strategy;
  private readonly int _seed;

  /// <summary>Pre-configured instance with barycentric selection from 4 closest.</summary>
  public static NConvexDitherer Default { get; } = new(4, ConvexStrategy.Barycentric);

  /// <summary>Pre-configured instance with projection selection from 6 closest.</summary>
  public static NConvexDitherer Projection6 { get; } = new(6, ConvexStrategy.Projection);

  /// <summary>Pre-configured instance with spatial pattern selection from 3 closest.</summary>
  public static NConvexDitherer SpatialPattern3 { get; } = new(3, ConvexStrategy.SpatialPattern);

  /// <summary>Pre-configured instance with weighted random selection from 5 closest.</summary>
  public static NConvexDitherer WeightedRandom5 { get; } = new(5, ConvexStrategy.WeightedRandom);

  /// <summary>
  /// Creates an N-Convex ditherer with the specified settings.
  /// </summary>
  /// <param name="n">Number of closest colors to consider in convex hull.</param>
  /// <param name="strategy">Strategy for selecting among closest colors.</param>
  /// <param name="seed">Random seed for reproducibility (WeightedRandom strategy).</param>
  public NConvexDitherer(int n = 4, ConvexStrategy strategy = ConvexStrategy.Barycentric, int seed = 42) {
    this._n = Math.Max(1, n);
    this._strategy = strategy;
    this._seed = seed;
  }

  /// <inheritdoc />
  public bool RequiresSequentialProcessing => this._strategy == ConvexStrategy.WeightedRandom;

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

    var random = new Random(this._seed + startY);
    var endY = startY + height;

    // Precompute palette colors in normalized form
    var paletteColors = new (float c1, float c2, float c3, float a)[palette.Length];
    for (var i = 0; i < palette.Length; ++i) {
      var (c1, c2, c3, a) = palette[i].ToNormalized();
      paletteColors[i] = (c1.ToFloat(), c2.ToFloat(), c3.ToFloat(), a.ToFloat());
    }

    for (var y = startY; y < endY; ++y)
    for (var x = 0; x < width; ++x) {
      var pixel = decoder.Decode(source[y * sourceStride + x]);
      var (c1, c2, c3, alpha) = pixel.ToNormalized();
      var pixelNormalized = (c1.ToFloat(), c2.ToFloat(), c3.ToFloat(), alpha.ToFloat());

      var closestColors = _FindNClosestColors(pixel, palette, metric, this._n);

      if (closestColors.Count == 0) {
        indices[y * targetStride + x] = 0;
        continue;
      }

      if (closestColors.Count == 1) {
        indices[y * targetStride + x] = (byte)closestColors[0].index;
        continue;
      }

      var selectedIndex = this._SelectColorFromConvexHull(closestColors, pixelNormalized, paletteColors, x, y, random);
      indices[y * targetStride + x] = (byte)selectedIndex;
    }
  }

  private static List<(int index, double distance)> _FindNClosestColors<TWork, TMetric>(
    TWork color,
    TWork[] palette,
    in TMetric metric,
    int n)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TMetric : struct, IColorMetric<TWork> {

    if (palette.Length == 0) return [];

    var distances = new List<(int index, double distance)>(palette.Length);

    for (var i = 0; i < palette.Length; ++i) {
      var distance = (double)metric.Distance(color, palette[i]).ToFloat();
      distances.Add((i, distance));
    }

    distances.Sort((a, b) => a.distance.CompareTo(b.distance));

    var result = new List<(int index, double distance)>(Math.Min(n, distances.Count));
    for (var i = 0; i < n && i < distances.Count; ++i)
      result.Add(distances[i]);

    return result;
  }

  private int _SelectColorFromConvexHull(
    List<(int index, double distance)> closestColors,
    (float c1, float c2, float c3, float a) pixelNormalized,
    (float c1, float c2, float c3, float a)[] paletteColors,
    int x, int y,
    Random random) =>
    this._strategy switch {
      ConvexStrategy.Barycentric => _SelectByBarycentric(closestColors),
      ConvexStrategy.Projection => _SelectByProjection(closestColors, pixelNormalized, paletteColors),
      ConvexStrategy.SpatialPattern => _SelectBySpatialPattern(closestColors, x, y),
      ConvexStrategy.WeightedRandom => _SelectByWeightedRandom(closestColors, pixelNormalized, paletteColors, random),
      _ => closestColors[0].index
    };

  private static int _SelectByBarycentric(List<(int index, double distance)> closestColors) {
    var totalInverseDistance = 0.0;
    var weights = new double[closestColors.Count];

    for (var i = 0; i < closestColors.Count; ++i) {
      var weight = 1.0 / (closestColors[i].distance + 1.0);
      weights[i] = weight;
      totalInverseDistance += weight;
    }

    for (var i = 0; i < weights.Length; ++i)
      weights[i] /= totalInverseDistance;

    var bestIndex = 0;
    var bestWeight = weights[0];

    for (var i = 1; i < weights.Length; ++i)
      if (weights[i] > bestWeight) {
        bestWeight = weights[i];
        bestIndex = i;
      }

    return closestColors[bestIndex].index;
  }

  private static int _SelectByProjection(
    List<(int index, double distance)> closestColors,
    (float c1, float c2, float c3, float a) pixelNormalized,
    (float c1, float c2, float c3, float a)[] paletteColors) =>
    closestColors.Count switch {
      2 => _ProjectOntoLine(closestColors, pixelNormalized, paletteColors),
      >= 3 => _ProjectOntoPolygon(closestColors, pixelNormalized, paletteColors),
      _ => closestColors[0].index
    };

  private static int _ProjectOntoLine(
    List<(int index, double distance)> closestColors,
    (float c1, float c2, float c3, float a) pixelNormalized,
    (float c1, float c2, float c3, float a)[] paletteColors) {

    var color1 = paletteColors[closestColors[0].index];
    var color2 = paletteColors[closestColors[1].index];

    var dx = color2.c1 - color1.c1;
    var dy = color2.c2 - color1.c2;
    var dz = color2.c3 - color1.c3;

    var px = pixelNormalized.c1 - color1.c1;
    var py = pixelNormalized.c2 - color1.c2;
    var pz = pixelNormalized.c3 - color1.c3;

    var dotProduct = dx * px + dy * py + dz * pz;
    var lengthSquared = dx * dx + dy * dy + dz * dz;

    if (lengthSquared < 1e-10f) return closestColors[0].index;

    var t = dotProduct / lengthSquared;
    return t < 0.5f ? closestColors[0].index : closestColors[1].index;
  }

  private static int _ProjectOntoPolygon(
    List<(int index, double distance)> closestColors,
    (float c1, float c2, float c3, float a) pixelNormalized,
    (float c1, float c2, float c3, float a)[] paletteColors) {

    var centroidC1 = 0f;
    var centroidC2 = 0f;
    var centroidC3 = 0f;

    foreach (var (index, _) in closestColors) {
      var color = paletteColors[index];
      centroidC1 += color.c1;
      centroidC2 += color.c2;
      centroidC3 += color.c3;
    }

    centroidC1 /= closestColors.Count;
    centroidC2 /= closestColors.Count;
    centroidC3 /= closestColors.Count;

    var bestIndex = 0;
    var bestScore = double.MaxValue;

    for (var i = 0; i < closestColors.Count; ++i) {
      var color = paletteColors[closestColors[i].index];

      var dirC1 = pixelNormalized.c1 - centroidC1;
      var dirC2 = pixelNormalized.c2 - centroidC2;
      var dirC3 = pixelNormalized.c3 - centroidC3;

      var colorC1 = color.c1 - centroidC1;
      var colorC2 = color.c2 - centroidC2;
      var colorC3 = color.c3 - centroidC3;

      var dotProduct = dirC1 * colorC1 + dirC2 * colorC2 + dirC3 * colorC3;
      var dirMagnitude = Math.Sqrt(dirC1 * dirC1 + dirC2 * dirC2 + dirC3 * dirC3);
      var colorMagnitude = Math.Sqrt(colorC1 * colorC1 + colorC2 * colorC2 + colorC3 * colorC3);

      if (dirMagnitude > 1e-10 && colorMagnitude > 1e-10) {
        var similarity = dotProduct / (dirMagnitude * colorMagnitude);
        var score = 1.0 - similarity;

        if (score < bestScore) {
          bestScore = score;
          bestIndex = i;
        }
      }
    }

    return closestColors[bestIndex].index;
  }

  private static int _SelectBySpatialPattern(
    List<(int index, double distance)> closestColors,
    int x, int y) {
    var patternValue = (x * 73 + y * 97) % closestColors.Count;
    return closestColors[patternValue].index;
  }

  private static int _SelectByWeightedRandom(
    List<(int index, double distance)> closestColors,
    (float c1, float c2, float c3, float a) pixelNormalized,
    (float c1, float c2, float c3, float a)[] paletteColors,
    Random random) {

    var weights = new double[closestColors.Count];
    var totalWeight = 0.0;

    for (var i = 0; i < closestColors.Count; ++i) {
      var color = paletteColors[closestColors[i].index];

      var distanceWeight = 1.0 / (closestColors[i].distance + 1.0);

      var c1Diff = Math.Abs(pixelNormalized.c1 - color.c1);
      var c2Diff = Math.Abs(pixelNormalized.c2 - color.c2);
      var c3Diff = Math.Abs(pixelNormalized.c3 - color.c3);
      var colorSimilarity = 1.0 / (c1Diff + c2Diff + c3Diff + 1.0);

      weights[i] = distanceWeight * colorSimilarity;
      totalWeight += weights[i];
    }

    if (totalWeight == 0)
      return closestColors[0].index;

    var randomValue = random.NextDouble() * totalWeight;
    var cumulativeWeight = 0.0;

    for (var i = 0; i < weights.Length; ++i) {
      cumulativeWeight += weights[i];
      if (randomValue <= cumulativeWeight)
        return closestColors[i].index;
    }

    return closestColors[^1].index;
  }
}
