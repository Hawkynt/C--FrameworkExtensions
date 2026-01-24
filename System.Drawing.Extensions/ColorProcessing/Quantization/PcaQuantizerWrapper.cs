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
/// Wrapper that applies PCA-based color space transformation before quantization.
/// </summary>
/// <remarks>
/// <para>
/// This wrapper transforms colors into a coordinate system aligned with their principal
/// components (axes of maximum variance) before quantization, then transforms the
/// resulting palette back to the original space.
/// </para>
/// <para>
/// PCA preprocessing can improve quantization quality for color distributions that
/// are elongated or tilted in color space, as it aligns the quantization with the
/// natural structure of the data.
/// </para>
/// </remarks>
/// <typeparam name="TInner">The type of the wrapped quantizer.</typeparam>
[Quantizer(QuantizationType.Clustering, DisplayName = "PCA Preprocessor", QualityRating = 0)]
public readonly struct PcaQuantizerWrapper<TInner> : IQuantizer
  where TInner : struct, IQuantizer {

  private readonly TInner _inner;

  /// <summary>
  /// Creates a PCA quantizer wrapper around the specified quantizer.
  /// </summary>
  /// <param name="inner">The quantizer to wrap.</param>
  public PcaQuantizerWrapper(TInner inner) => this._inner = inner;

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>()
    => new Kernel<TWork>(((IQuantizer)this._inner).CreateKernel<TWork>());

  private sealed class Kernel<TWork>(IQuantizer<TWork> innerKernel) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount) {
      var histArray = histogram.ToArray();
      if (histArray.Length == 0)
        return [];

      // Compute PCA transform from histogram
      var (transform, inverseTransform) = _ComputePcaTransform(histArray);

      // Transform colors to PCA space
      var transformedHist = histArray
        .Select(h => (color: transform(h.color), h.count))
        .ToArray();

      // Quantize in PCA space
      var quantizedPalette = innerKernel.GeneratePalette(transformedHist, colorCount);

      // Transform palette back to original space
      return quantizedPalette.Select(inverseTransform).ToArray();
    }

    private static (Func<TWork, TWork> transform, Func<TWork, TWork> inverseTransform) _ComputePcaTransform(
      (TWork color, uint count)[] histogram) {
      // Convert to float arrays
      var points = histogram.Select(h => {
        var (c1, c2, c3, a) = h.color.ToNormalized();
        return (c1: c1.ToFloat(), c2: c2.ToFloat(), c3: c3.ToFloat(), a: a.ToFloat(), count: h.count);
      }).ToArray();

      // Compute weighted centroid
      var totalWeight = points.Sum(p => (double)p.count);
      if (totalWeight <= 0)
        return (c => c, c => c); // Identity transform

      var centroid = (
        c1: points.Sum(p => p.c1 * p.count) / totalWeight,
        c2: points.Sum(p => p.c2 * p.count) / totalWeight,
        c3: points.Sum(p => p.c3 * p.count) / totalWeight,
        a: points.Sum(p => p.a * p.count) / totalWeight
      );

      // Compute covariance matrix
      var cov = new double[3, 3];
      foreach (var (c1, c2, c3, _, count) in points) {
        var d = new[] { c1 - centroid.c1, c2 - centroid.c2, c3 - centroid.c3 };
        for (var i = 0; i < 3; ++i)
          for (var j = 0; j < 3; ++j)
            cov[i, j] += d[i] * d[j] * count;
      }

      for (var i = 0; i < 3; ++i)
        for (var j = 0; j < 3; ++j)
          cov[i, j] /= totalWeight;

      // Compute eigenvectors using power iteration
      var eigenvectors = _ComputeEigenvectors(cov);

      // Build rotation matrices
      var rotMatrix = eigenvectors;
      var invRotMatrix = _Transpose(eigenvectors);

      // Compute scale factors (standard deviations along principal axes)
      var stdDev = new double[3];
      foreach (var (c1, c2, c3, _, count) in points) {
        var centered = new[] { c1 - centroid.c1, c2 - centroid.c2, c3 - centroid.c3 };
        for (var i = 0; i < 3; ++i) {
          var projected = 0.0;
          for (var j = 0; j < 3; ++j)
            projected += centered[j] * rotMatrix[i, j];
          stdDev[i] += projected * projected * count;
        }
      }
      for (var i = 0; i < 3; ++i)
        stdDev[i] = Math.Sqrt(stdDev[i] / totalWeight);

      // Transform: center, rotate, scale to unit variance
      TWork Transform(TWork color) {
        var (c1N, c2N, c3N, aN) = color.ToNormalized();
        var c = new[] { c1N.ToFloat() - centroid.c1, c2N.ToFloat() - centroid.c2, c3N.ToFloat() - centroid.c3 };

        var rotated = new double[3];
        for (var i = 0; i < 3; ++i)
          for (var j = 0; j < 3; ++j)
            rotated[i] += c[j] * rotMatrix[i, j];

        // Scale to unit variance (avoid division by zero)
        for (var i = 0; i < 3; ++i)
          rotated[i] = stdDev[i] > 0.001 ? rotated[i] / stdDev[i] : rotated[i];

        // Shift to [0,1] range (assuming roughly normalized data)
        return ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)(rotated[0] * 0.25 + 0.5)),
          UNorm32.FromFloatClamped((float)(rotated[1] * 0.25 + 0.5)),
          UNorm32.FromFloatClamped((float)(rotated[2] * 0.25 + 0.5)),
          aN
        );
      }

      TWork InverseTransform(TWork color) {
        var (c1N, c2N, c3N, aN) = color.ToNormalized();
        var scaled = new[] {
          (c1N.ToFloat() - 0.5) / 0.25,
          (c2N.ToFloat() - 0.5) / 0.25,
          (c3N.ToFloat() - 0.5) / 0.25
        };

        // Unscale
        for (var i = 0; i < 3; ++i)
          scaled[i] *= stdDev[i] > 0.001 ? stdDev[i] : 1;

        // Inverse rotation
        var unrotated = new double[3];
        for (var i = 0; i < 3; ++i)
          for (var j = 0; j < 3; ++j)
            unrotated[i] += scaled[j] * invRotMatrix[i, j];

        // Uncenter
        return ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped((float)(unrotated[0] + centroid.c1)),
          UNorm32.FromFloatClamped((float)(unrotated[1] + centroid.c2)),
          UNorm32.FromFloatClamped((float)(unrotated[2] + centroid.c3)),
          aN
        );
      }

      return (Transform, InverseTransform);
    }

    private static double[,] _ComputeEigenvectors(double[,] covariance) {
      const int maxIterations = 100;
      const double tolerance = 1e-10;
      var n = 3;
      var result = new double[n, n];
      var deflatedCov = (double[,])covariance.Clone();

      for (var comp = 0; comp < n; ++comp) {
        var v = new double[n];
        for (var i = 0; i < n; ++i)
          v[i] = 1.0 / Math.Sqrt(n);

        for (var iter = 0; iter < maxIterations; ++iter) {
          var w = new double[n];
          for (var i = 0; i < n; ++i)
            for (var j = 0; j < n; ++j)
              w[i] += deflatedCov[i, j] * v[j];

          var norm = Math.Sqrt(w.Sum(x => x * x));
          if (norm < tolerance) {
            for (var i = 0; i < n; ++i)
              w[i] = i == comp ? 1.0 : 0.0;
            norm = 1.0;
          }

          var newV = new double[n];
          for (var i = 0; i < n; ++i)
            newV[i] = w[i] / norm;

          var diff = 0.0;
          for (var i = 0; i < n; ++i)
            diff += (newV[i] - v[i]) * (newV[i] - v[i]);

          v = newV;
          if (diff < tolerance)
            break;
        }

        for (var i = 0; i < n; ++i)
          result[comp, i] = v[i];

        // Deflate
        var eigenvalue = 0.0;
        for (var i = 0; i < n; ++i)
          for (var j = 0; j < n; ++j)
            eigenvalue += v[i] * deflatedCov[i, j] * v[j];

        for (var i = 0; i < n; ++i)
          for (var j = 0; j < n; ++j)
            deflatedCov[i, j] -= eigenvalue * v[i] * v[j];
      }

      return result;
    }

    private static double[,] _Transpose(double[,] matrix) {
      var n = matrix.GetLength(0);
      var m = matrix.GetLength(1);
      var result = new double[m, n];
      for (var i = 0; i < n; ++i)
        for (var j = 0; j < m; ++j)
          result[j, i] = matrix[i, j];
      return result;
    }
  }
}
