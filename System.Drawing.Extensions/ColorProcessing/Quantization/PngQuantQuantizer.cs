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
using System.Linq;
using Hawkynt.ColorProcessing.Internal;
using Hawkynt.ColorProcessing.Metrics;

namespace Hawkynt.ColorProcessing.Quantization;

/// <summary>
/// PngQuant-style color quantizer combining variance-based Median Cut with K-means refinement.
/// </summary>
/// <remarks>
/// <para>This quantizer implements the key techniques from the pngquant algorithm:</para>
/// <list type="number">
/// <item><description>Modified Median Cut using variance minimization (selects boxes to minimize variance from median)</description></item>
/// <item><description>Iterative weight adjustment (gradient descent-like) giving more weight to poorly represented colors</description></item>
/// <item><description>Voronoi iteration (K-means) for locally optimal palette refinement</description></item>
/// <item><description>Premultiplied alpha handling for better transparency blending</description></item>
/// <item><description>Color distance considering blending on both black and white backgrounds</description></item>
/// </list>
/// <para>Reference: <see href="https://pngquant.org/"/> and <see href="https://github.com/pornel/pngquant"/></para>
/// </remarks>
[Quantizer(QuantizationType.Variance, DisplayName = "PngQuant", Author = "Kornel Lesiński", Year = 2009, QualityRating = 9)]
public struct PngQuantQuantizer : IQuantizer {

  /// <summary>
  /// Gets or sets the number of Median Cut iterations with weight adjustment.
  /// Higher values improve quality for images with subtle gradients.
  /// </summary>
  public int MedianCutIterations { get; set; } = 3;

  /// <summary>
  /// Gets or sets the number of K-means refinement iterations.
  /// </summary>
  public int KMeansIterations { get; set; } = 10;

  /// <summary>
  /// Gets or sets the K-means convergence threshold (normalized 0-1).
  /// </summary>
  public float ConvergenceThreshold { get; set; } = 0.0001f;

  /// <summary>
  /// Gets or sets the weight boost factor for underrepresented colors.
  /// Higher values give more emphasis to colors that don't quantize well.
  /// </summary>
  public float ErrorBoostFactor { get; set; } = 2.0f;

  public PngQuantQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>()
    => new Kernel<TWork>(this.MedianCutIterations, this.KMeansIterations, this.ConvergenceThreshold, this.ErrorBoostFactor);

  internal sealed class Kernel<TWork>(
    int medianCutIterations,
    int kMeansIterations,
    float convergenceThreshold,
    float errorBoostFactor
  ) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int colorCount, (TWork color, uint count)[] colors) {
      if (colors.Length == 0)
        return [];

      if (colors.Length <= colorCount)
        return colors.Select(c => c.color);

      // Convert to premultiplied alpha working space
      var premultiplied = _ToPremultipliedAlpha(colors);

      // Stage 1: Iterative variance-based Median Cut with weight adjustment
      var palette = _VarianceMedianCutWithWeightAdjustment(premultiplied, colorCount);

      // Stage 2: K-means (Voronoi) refinement
      palette = _KMeansRefinement(premultiplied, palette);

      return palette;
    }

    /// <summary>
    /// Converts colors to premultiplied alpha representation for better blending.
    /// </summary>
    private static (float r, float g, float b, float a, uint count)[] _ToPremultipliedAlpha((TWork color, uint count)[] colors) {
      var result = new (float r, float g, float b, float a, uint count)[colors.Length];
      for (var i = 0; i < colors.Length; ++i) {
        var (c1, c2, c3, a) = colors[i].color.ToNormalized();
        var r = c1.ToFloat();
        var g = c2.ToFloat();
        var b = c3.ToFloat();
        var alpha = a.ToFloat();

        // Store as premultiplied alpha (color * alpha)
        result[i] = (r * alpha, g * alpha, b * alpha, alpha, colors[i].count);
      }

      return result;
    }

    /// <summary>
    /// Performs iterative variance-based Median Cut with weight adjustment
    /// for poorly represented colors (gradient descent-like refinement).
    /// </summary>
    private TWork[] _VarianceMedianCutWithWeightAdjustment(
      (float r, float g, float b, float a, uint count)[] colors,
      int colorCount
    ) {
      // Start with uniform weights
      var weights = new float[colors.Length];
      for (var i = 0; i < colors.Length; ++i)
        weights[i] = 1.0f;

      TWork[] bestPalette = null!;
      var bestError = double.MaxValue;

      // Repeat Median Cut with adjusted weights
      for (var iteration = 0; iteration < medianCutIterations; ++iteration) {
        // Apply current weights to histogram
        var weightedColors = new (float r, float g, float b, float a, uint count)[colors.Length];
        for (var i = 0; i < colors.Length; ++i) {
          var adjustedCount = (uint)Math.Max(1, colors[i].count * weights[i]);
          weightedColors[i] = (colors[i].r, colors[i].g, colors[i].b, colors[i].a, adjustedCount);
        }

        // Perform variance-based Median Cut
        var palette = _VarianceMedianCut(weightedColors, colorCount);

        // Evaluate palette quality and compute per-color errors
        var (totalError, colorErrors) = _EvaluatePaletteWithErrors(colors, palette);

        if (totalError < bestError) {
          bestError = totalError;
          bestPalette = palette;
        }

        // Adjust weights: boost poorly represented colors
        if (iteration < medianCutIterations - 1) {
          var avgError = colorErrors.Average();
          for (var i = 0; i < colors.Length; ++i) {
            if (colorErrors[i] > avgError)
              // Boost weight for colors with above-average error
              weights[i] *= 1.0f + errorBoostFactor * (float)((colorErrors[i] - avgError) / (avgError + 0.001));
            else
              // Slightly reduce weight for well-represented colors
              weights[i] *= 0.95f;

            // Clamp weights to reasonable range
            weights[i] = Math.Max(0.1f, Math.Min(10.0f, weights[i]));
          }
        }
      }

      return bestPalette;
    }

    /// <summary>
    /// Performs variance-based Median Cut (boxes split by maximum variance).
    /// </summary>
    private static TWork[] _VarianceMedianCut(
      (float r, float g, float b, float a, uint count)[] colors,
      int colorCount
    ) {
      var cubes = new List<ColorCube> { new(colors.ToList()) };

      while (cubes.Count < colorCount) {
        // Find cube with highest sum of squared error (variance * count)
        ColorCube? largestCube = null;
        var maxSse = -1.0;
        foreach (var cube in cubes) {
          if (cube.ColorCount <= 1)
            continue;
          if (cube.SumOfSquaredError > maxSse) {
            maxSse = cube.SumOfSquaredError;
            largestCube = cube;
          }
        }

        if (largestCube == null)
          break;

        cubes.Remove(largestCube);
        cubes.AddRange(largestCube.Split());
      }

      // Convert cube averages back to TWork
      return cubes
        .Select(c => c.GetAverageColor<TWork>())
        .ToArray();
    }

    /// <summary>
    /// K-means refinement (Voronoi iteration) for locally optimal palette.
    /// </summary>
    private TWork[] _KMeansRefinement(
      (float r, float g, float b, float a, uint count)[] colors,
      TWork[] initialPalette
    ) {
      var palette = (TWork[])initialPalette.Clone();
      var assignments = new int[colors.Length];
      var thresholdSq = convergenceThreshold * convergenceThreshold;

      for (var iteration = 0; iteration < kMeansIterations; ++iteration) {
        // Assign each color to nearest palette entry (using PngQuant distance)
        for (var i = 0; i < colors.Length; ++i)
          assignments[i] = _FindNearestPaletteEntry(colors[i], palette);

        // Recalculate palette entries as weighted centroids
        var newPalette = _CalculateCentroids(colors, assignments, palette.Length, palette);

        // Check for convergence
        var maxMovementSq = 0.0f;
        for (var i = 0; i < palette.Length; ++i) {
          var (o1, o2, o3, oa) = palette[i].ToNormalized();
          var (n1, n2, n3, na) = newPalette[i].ToNormalized();

          var d1 = o1.ToFloat() - n1.ToFloat();
          var d2 = o2.ToFloat() - n2.ToFloat();
          var d3 = o3.ToFloat() - n3.ToFloat();
          var da = oa.ToFloat() - na.ToFloat();

          var movementSq = d1 * d1 + d2 * d2 + d3 * d3 + da * da;
          if (movementSq > maxMovementSq)
            maxMovementSq = movementSq;
        }

        palette = newPalette;

        if (maxMovementSq < thresholdSq)
          break;
      }

      return palette;
    }

    /// <summary>
    /// Finds the nearest palette entry using PngQuant-style distance
    /// (considering blending on black and white backgrounds).
    /// </summary>
    private static int _FindNearestPaletteEntry(
      (float r, float g, float b, float a, uint count) color,
      TWork[] palette
    ) {
      var nearest = 0;
      var minDist = float.MaxValue;

      for (var i = 0; i < palette.Length; ++i) {
        var (p1, p2, p3, pa) = palette[i].ToNormalized();
        var pr = p1.ToFloat();
        var pg = p2.ToFloat();
        var pb = p3.ToFloat();
        var palpha = pa.ToFloat();

        // PngQuant distance: consider blending on black and white
        var dist = _PngQuantDistance(
          color.r, color.g, color.b, color.a,
          pr * palpha, pg * palpha, pb * palpha, palpha
        );

        if (dist < minDist) {
          minDist = dist;
          nearest = i;
        }
      }

      return nearest;
    }

    /// <summary>
    /// PngQuant-style distance considering blending on both black and white backgrounds.
    /// Colors are expected in premultiplied alpha format.
    /// </summary>
    private static float _PngQuantDistance(
      float r1, float g1, float b1, float a1,
      float r2, float g2, float b2, float a2
    ) {
      // Distance when blended on black background (premultiplied values)
      var drBlack = r1 - r2;
      var dgBlack = g1 - g2;
      var dbBlack = b1 - b2;

      // Distance when blended on white: result = 1 - alpha + premultiplied
      var drWhite = (1f - a1 + r1) - (1f - a2 + r2);
      var dgWhite = (1f - a1 + g1) - (1f - a2 + g2);
      var dbWhite = (1f - a1 + b1) - (1f - a2 + b2);

      // Combine distances with luminance weighting
      const float rWeight = 0.299f;
      const float gWeight = 0.587f;
      const float bWeight = 0.114f;

      var blackDist = rWeight * drBlack * drBlack +
                      gWeight * dgBlack * dgBlack +
                      bWeight * dbBlack * dbBlack;

      var whiteDist = rWeight * drWhite * drWhite +
                      gWeight * dgWhite * dgWhite +
                      bWeight * dbWhite * dbWhite;

      // Return maximum of the two (worst case blending)
      return Math.Max(blackDist, whiteDist);
    }

    /// <summary>
    /// Calculates centroids from color assignments.
    /// </summary>
    private static TWork[] _CalculateCentroids(
      (float r, float g, float b, float a, uint count)[] colors,
      int[] assignments,
      int paletteSize,
      TWork[] fallbackPalette
    ) {
      var rSums = new double[paletteSize];
      var gSums = new double[paletteSize];
      var bSums = new double[paletteSize];
      var aSums = new double[paletteSize];
      var weights = new double[paletteSize];

      for (var i = 0; i < colors.Length; ++i) {
        var cluster = assignments[i];
        var weight = colors[i].count;
        var alpha = colors[i].a;

        // Accumulate in straight alpha (unpremultiply)
        if (alpha > 0.001f) {
          rSums[cluster] += (colors[i].r / alpha) * weight;
          gSums[cluster] += (colors[i].g / alpha) * weight;
          bSums[cluster] += (colors[i].b / alpha) * weight;
        }

        aSums[cluster] += alpha * weight;
        weights[cluster] += weight;
      }

      var palette = new TWork[paletteSize];
      for (var i = 0; i < paletteSize; ++i) {
        if (weights[i] > 0)
          palette[i] = ColorFactory.FromNormalized_4<TWork>(
            UNorm32.FromFloatClamped((float)(rSums[i] / weights[i])),
            UNorm32.FromFloatClamped((float)(gSums[i] / weights[i])),
            UNorm32.FromFloatClamped((float)(bSums[i] / weights[i])),
            UNorm32.FromFloatClamped((float)(aSums[i] / weights[i]))
          );
        else
          palette[i] = fallbackPalette[i];
      }

      return palette;
    }

    /// <summary>
    /// Evaluates palette quality and returns per-color quantization errors.
    /// </summary>
    private static (double totalError, double[] colorErrors) _EvaluatePaletteWithErrors(
      (float r, float g, float b, float a, uint count)[] colors,
      TWork[] palette
    ) {
      var colorErrors = new double[colors.Length];
      var totalError = 0.0;
      var totalWeight = 0.0;

      for (var i = 0; i < colors.Length; ++i) {
        var minDist = float.MaxValue;
        for (var j = 0; j < palette.Length; ++j) {
          var (p1, p2, p3, pa) = palette[j].ToNormalized();
          var dist = _PngQuantDistance(
            colors[i].r, colors[i].g, colors[i].b, colors[i].a,
            p1.ToFloat() * pa.ToFloat(), p2.ToFloat() * pa.ToFloat(), p3.ToFloat() * pa.ToFloat(), pa.ToFloat()
          );
          if (dist < minDist)
            minDist = dist;
        }

        colorErrors[i] = minDist;
        totalError += minDist * colors[i].count;
        totalWeight += colors[i].count;
      }

      return (totalError / totalWeight, colorErrors);
    }

    /// <summary>
    /// Color cube for variance-based Median Cut.
    /// </summary>
    private sealed class ColorCube {
      private readonly List<(float r, float g, float b, float a, uint count)> _colors;
      private readonly float _avgR, _avgG, _avgB, _avgA;
      private readonly double _varR, _varG, _varB;
      private readonly double _sse;
      private readonly long _totalCount;

      public ColorCube(List<(float r, float g, float b, float a, uint count)> colors) {
        this._colors = colors;

        if (colors.Count == 0) {
          this._avgR = this._avgG = this._avgB = this._avgA = 0;
          this._varR = this._varG = this._varB = 0;
          this._sse = 0;
          this._totalCount = 0;
          return;
        }

        // Calculate weighted sums for average
        double sumR = 0, sumG = 0, sumB = 0, sumA = 0;
        long totalCount = 0;

        foreach (var (r, g, b, a, count) in colors) {
          sumR += r * count;
          sumG += g * count;
          sumB += b * count;
          sumA += a * count;
          totalCount += count;
        }

        this._totalCount = totalCount;
        if (totalCount == 0) {
          this._avgR = this._avgG = this._avgB = this._avgA = 0;
          this._varR = this._varG = this._varB = 0;
          this._sse = 0;
          return;
        }

        this._avgR = (float)(sumR / totalCount);
        this._avgG = (float)(sumG / totalCount);
        this._avgB = (float)(sumB / totalCount);
        this._avgA = (float)(sumA / totalCount);

        // Calculate variance and SSE with luminance weighting
        const float rWeight = 0.299f;
        const float gWeight = 0.587f;
        const float bWeight = 0.114f;

        double varR = 0, varG = 0, varB = 0, sse = 0;
        foreach (var (r, g, b, _, count) in colors) {
          var diffR = r - this._avgR;
          var diffG = g - this._avgG;
          var diffB = b - this._avgB;

          varR += diffR * diffR * count;
          varG += diffG * diffG * count;
          varB += diffB * diffB * count;
          sse += (rWeight * diffR * diffR + gWeight * diffG * diffG + bWeight * diffB * diffB) * count;
        }

        this._varR = varR / totalCount;
        this._varG = varG / totalCount;
        this._varB = varB / totalCount;
        this._sse = sse;
      }

      public double SumOfSquaredError => this._sse;
      public int ColorCount => this._colors.Count;

      public TColor GetAverageColor<TColor>()
        where TColor : unmanaged, IColorSpace4<TColor> {
        if (this._colors.Count == 0)
          return default;

        // Unpremultiply alpha for output
        float r, g, b;
        if (this._avgA > 0.001f) {
          r = this._avgR / this._avgA;
          g = this._avgG / this._avgA;
          b = this._avgB / this._avgA;
        } else {
          r = this._avgR;
          g = this._avgG;
          b = this._avgB;
        }

        return ColorFactory.FromNormalized_4<TColor>(
          UNorm32.FromFloatClamped(r),
          UNorm32.FromFloatClamped(g),
          UNorm32.FromFloatClamped(b),
          UNorm32.FromFloatClamped(this._avgA)
        );
      }

      public IEnumerable<ColorCube> Split() {
        if (this._colors.Count <= 1)
          return [this];

        // Find axis with maximum variance (using luminance-weighted variance)
        Func<(float r, float g, float b, float a, uint count), float> getComponent;
        float meanValue;

        if (this._varR >= this._varG && this._varR >= this._varB) {
          getComponent = item => item.r;
          meanValue = this._avgR;
        } else if (this._varG >= this._varR && this._varG >= this._varB) {
          getComponent = item => item.g;
          meanValue = this._avgG;
        } else {
          getComponent = item => item.b;
          meanValue = this._avgB;
        }

        // Sort by selected component
        this._colors.Sort((a, b) => getComponent(a).CompareTo(getComponent(b)));

        // Split at mean value (minimizes variance within resulting boxes)
        var splitIndex = this._colors.FindIndex(item => getComponent(item) >= meanValue);

        // Fallback to median if mean-based split doesn't work
        if (splitIndex <= 0 || splitIndex >= this._colors.Count)
          splitIndex = this._colors.Count / 2;

        // Ensure progress
        if (splitIndex == 0)
          splitIndex = 1;
        if (splitIndex >= this._colors.Count)
          splitIndex = this._colors.Count - 1;

        return [
          new ColorCube(this._colors.Take(splitIndex).ToList()),
          new ColorCube(this._colors.Skip(splitIndex).ToList())
        ];
      }
    }
  }
}
