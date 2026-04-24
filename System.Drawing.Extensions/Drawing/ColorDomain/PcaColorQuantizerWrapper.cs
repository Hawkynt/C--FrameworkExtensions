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
using System.Drawing;
using System.Linq;

namespace Hawkynt.Drawing.ColorDomain;

/// <summary>
/// Wraps any <see cref="IColorQuantizer"/> with PCA preprocessing: rotates the input
/// histogram into its principal-component basis, quantizes there, and rotates the
/// resulting palette back. Useful when the histogram's color cluster is elongated along
/// non-axis-aligned directions (e.g. a sky gradient or a sepia photograph) — the inner
/// quantizer can split more efficiently in PCA space.
/// </summary>
/// <remarks>
/// Color-domain analogue of upstream's typed
/// <see cref="Hawkynt.ColorProcessing.Quantization.PcaQuantizerWrapper{TInner}"/>.
/// Uses power iteration over the 3×3 RGB covariance matrix; no MathNet dependency.
/// </remarks>
public sealed class PcaColorQuantizerWrapper : IColorQuantizer {

  private readonly IColorQuantizer _inner;

  public PcaColorQuantizerWrapper(IColorQuantizer inner)
    => this._inner = inner ?? throw new ArgumentNullException(nameof(inner));

  public Color[] ReduceColorsTo(ushort numberOfColors, IEnumerable<Color> usedColors)
    => this.ReduceColorsTo(numberOfColors, usedColors.Select(c => (c, 1u)));

  public Color[] ReduceColorsTo(ushort numberOfColors, IEnumerable<(Color color, uint count)> histogram) {
    var snapshot = histogram.ToList();
    if (snapshot.Count == 0)
      return [];

    var pca = new _Pca(snapshot);
    var transformed = snapshot.Select(h => (pca.Transform(h.color), h.count));
    var quantized = this._inner.ReduceColorsTo(numberOfColors, transformed);
    return quantized.Select(pca.InverseTransform).ToArray();
  }

  /// <summary>
  /// Power-iteration PCA over R/G/B in the [0,1] normalized color cube. Centers,
  /// rotates, and scales each axis to its observed [min,max] range so the inner
  /// quantizer sees colors uniformly spread across the byte range.
  /// </summary>
  private sealed class _Pca {

    private readonly double _meanR, _meanG, _meanB;
    private readonly double[,] _rotation;       // 3x3, row-major (rows = principal axes)
    private readonly double[,] _inverseRotation;
    private readonly double[] _min;             // per-axis min in PCA space
    private readonly double[] _max;             // per-axis max in PCA space

    public _Pca(IList<(Color color, uint count)> histogram) {
      // Weighted centroid in [0,1] RGB.
      double totalWeight = 0, mR = 0, mG = 0, mB = 0;
      foreach (var (c, w) in histogram) {
        totalWeight += w;
        mR += c.R / 255.0 * w;
        mG += c.G / 255.0 * w;
        mB += c.B / 255.0 * w;
      }
      if (totalWeight <= 0) {
        this._meanR = this._meanG = this._meanB = 0;
        this._rotation = _Identity();
        this._inverseRotation = _Identity();
        this._min = [0, 0, 0];
        this._max = [1, 1, 1];
        return;
      }
      this._meanR = mR / totalWeight;
      this._meanG = mG / totalWeight;
      this._meanB = mB / totalWeight;

      // Weighted covariance.
      var cov = new double[3, 3];
      foreach (var (c, w) in histogram) {
        var dr = c.R / 255.0 - this._meanR;
        var dg = c.G / 255.0 - this._meanG;
        var db = c.B / 255.0 - this._meanB;
        var d = new[] { dr, dg, db };
        for (var i = 0; i < 3; ++i)
        for (var j = 0; j < 3; ++j)
          cov[i, j] += d[i] * d[j] * w;
      }
      for (var i = 0; i < 3; ++i)
      for (var j = 0; j < 3; ++j)
        cov[i, j] /= totalWeight;

      this._rotation = _ComputeEigenvectors(cov);
      this._inverseRotation = _Transpose(this._rotation);

      // Compute per-axis min/max in PCA space so we can scale each component to fill
      // the byte range — improves the inner quantizer's ability to split.
      this._min = [double.MaxValue, double.MaxValue, double.MaxValue];
      this._max = [double.MinValue, double.MinValue, double.MinValue];
      foreach (var (c, _) in histogram) {
        var p = this._RotateOnly(c.R / 255.0 - this._meanR, c.G / 255.0 - this._meanG, c.B / 255.0 - this._meanB);
        for (var i = 0; i < 3; ++i) {
          if (p[i] < this._min[i]) this._min[i] = p[i];
          if (p[i] > this._max[i]) this._max[i] = p[i];
        }
      }
      for (var i = 0; i < 3; ++i)
        if (this._min[i] >= this._max[i]) {
          this._min[i] = 0;
          this._max[i] = 1;
        }
    }

    public Color Transform(Color color) {
      var p = this._RotateOnly(color.R / 255.0 - this._meanR, color.G / 255.0 - this._meanG, color.B / 255.0 - this._meanB);
      var r = _ToByte((p[0] - this._min[0]) / (this._max[0] - this._min[0]) * 255.0);
      var g = _ToByte((p[1] - this._min[1]) / (this._max[1] - this._min[1]) * 255.0);
      var b = _ToByte((p[2] - this._min[2]) / (this._max[2] - this._min[2]) * 255.0);
      return Color.FromArgb(r, g, b);
    }

    public Color InverseTransform(Color color) {
      var p = new double[3];
      p[0] = color.R / 255.0 * (this._max[0] - this._min[0]) + this._min[0];
      p[1] = color.G / 255.0 * (this._max[1] - this._min[1]) + this._min[1];
      p[2] = color.B / 255.0 * (this._max[2] - this._min[2]) + this._min[2];

      var unrotated = new double[3];
      for (var i = 0; i < 3; ++i)
      for (var j = 0; j < 3; ++j)
        unrotated[i] += p[j] * this._inverseRotation[i, j];

      return Color.FromArgb(
        _ToByte((unrotated[0] + this._meanR) * 255.0),
        _ToByte((unrotated[1] + this._meanG) * 255.0),
        _ToByte((unrotated[2] + this._meanB) * 255.0));
    }

    private double[] _RotateOnly(double r, double g, double b) {
      var v = new[] { r, g, b };
      var p = new double[3];
      for (var i = 0; i < 3; ++i)
      for (var j = 0; j < 3; ++j)
        p[i] += v[j] * this._rotation[i, j];
      return p;
    }

    private static double[,] _Identity() {
      var m = new double[3, 3];
      m[0, 0] = m[1, 1] = m[2, 2] = 1;
      return m;
    }

    private static double[,] _Transpose(double[,] m) {
      var t = new double[3, 3];
      for (var i = 0; i < 3; ++i)
      for (var j = 0; j < 3; ++j)
        t[i, j] = m[j, i];
      return t;
    }

    private static int _ToByte(double v) => v < 0 ? 0 : v > 255 ? 255 : (int)Math.Round(v);

    private static double[,] _ComputeEigenvectors(double[,] cov) {
      const int maxIterations = 100;
      const double tol = 1e-10;
      var result = new double[3, 3];
      var deflated = (double[,])cov.Clone();

      for (var comp = 0; comp < 3; ++comp) {
        var v = new double[] { 1.0 / Math.Sqrt(3), 1.0 / Math.Sqrt(3), 1.0 / Math.Sqrt(3) };
        for (var iter = 0; iter < maxIterations; ++iter) {
          var w = new double[3];
          for (var i = 0; i < 3; ++i)
          for (var j = 0; j < 3; ++j)
            w[i] += deflated[i, j] * v[j];
          var norm = Math.Sqrt(w[0] * w[0] + w[1] * w[1] + w[2] * w[2]);
          if (norm < tol)
            break;
          for (var i = 0; i < 3; ++i)
            w[i] /= norm;
          var diff = Math.Abs(w[0] - v[0]) + Math.Abs(w[1] - v[1]) + Math.Abs(w[2] - v[2]);
          v = w;
          if (diff < tol)
            break;
        }
        for (var i = 0; i < 3; ++i)
          result[comp, i] = v[i];

        // Deflate: subtract λvvᵀ to find next eigenvector.
        var lambda = 0.0;
        for (var i = 0; i < 3; ++i)
        for (var j = 0; j < 3; ++j)
          lambda += v[i] * deflated[i, j] * v[j];
        for (var i = 0; i < 3; ++i)
        for (var j = 0; j < 3; ++j)
          deflated[i, j] -= lambda * v[i] * v[j];
      }
      return result;
    }
  }
}
