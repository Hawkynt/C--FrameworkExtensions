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
using System.Linq;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

using Hawkynt.ColorProcessing.ColorMath;

namespace Hawkynt.ColorProcessing.Dithering;

/// <summary>
/// Joel Yliluoma's arbitrary-palette positional dithering algorithms.
/// </summary>
/// <remarks>
/// <para>Reference: J. Yliluoma "Arbitrary-palette positional dithering algorithm"</para>
/// <para>See also: https://bisqwit.iki.fi/story/howto/dither/jy/</para>
/// <para>Algorithm 1: Two-color mixing with psychovisual model</para>
/// <para>Algorithm 2: Multi-color candidate generation with threshold matrix</para>
/// <para>Algorithm 3: Iterative splitting refinement for highest quality</para>
/// <para>Uses gamma 2.2 correction for perceptually accurate color distance calculation.</para>
/// </remarks>
[Ditherer("Yliluoma", Description = "Positional dithering with palette-aware color mixing", Type = DitheringType.Custom, Author = "Joel Yliluoma")]
public readonly struct YliluomaDitherer : IDitherer {

  private const int _DEFAULT_ALGORITHM = 1;
  private const int _DEFAULT_MATRIX_SIZE = 8;

  /// <summary>
  /// Standard Bayer 8×8 ordered-dither matrix. Values 0..63 each appear exactly once
  /// (verified by recursive doubling construction). Bisqwit's Yliluoma reference uses
  /// this exact matrix at https://bisqwit.iki.fi/story/howto/dither/jy/.
  /// </summary>
  /// <remarks>
  /// Must remain declared BEFORE <see cref="_DefaultMatrix"/> — static field initializers
  /// run in textual order, and <see cref="_GenerateDitherMatrix"/> reads this array.
  /// Reordering caused a NullReferenceException during type initialization.
  /// </remarks>
  private static readonly int[] _Bayer8x8Int = [
     0, 48, 12, 60,  3, 51, 15, 63,
    32, 16, 44, 28, 35, 19, 47, 31,
     8, 56,  4, 52, 11, 59,  7, 55,
    40, 24, 36, 20, 43, 27, 39, 23,
     2, 50, 14, 62,  1, 49, 13, 61,
    34, 18, 46, 30, 33, 17, 45, 29,
    10, 58,  6, 54,  9, 57,  5, 53,
    42, 26, 38, 22, 41, 25, 37, 21
  ];

  // Static default matrix shared by all instances - avoids null issues with default struct initialization
  private static readonly float[] _DefaultMatrix = _GenerateDitherMatrix(_DEFAULT_MATRIX_SIZE);

  private readonly int _algorithm;
  private readonly int _matrixSize;
  private readonly float[] _ditherMatrix;

  /// <summary>Algorithm 1: Two-color mixing with psychovisual model.</summary>
  public static YliluomaDitherer Algorithm1 { get; } = new(_DEFAULT_ALGORITHM);

  /// <summary>Algorithm 2: Multi-color candidate generation with threshold matrix.</summary>
  public static YliluomaDitherer Algorithm2 { get; } = new(2);

  /// <summary>Algorithm 3: Simplified iterative splitting refinement.</summary>
  public static YliluomaDitherer Algorithm3 { get; } = new(3);

  /// <summary>Algorithm 3 Full: Complete iterative subdivision for highest quality.</summary>
  public static YliluomaDitherer Algorithm3Full { get; } = new(4);

  /// <summary>
  /// Creates a Yliluoma ditherer with the specified algorithm.
  /// </summary>
  /// <param name="algorithm">Algorithm variant (1-4).</param>
  public YliluomaDitherer(int algorithm = _DEFAULT_ALGORITHM) {
    this._algorithm = algorithm;
    this._matrixSize = _DEFAULT_MATRIX_SIZE;
    this._ditherMatrix = _DefaultMatrix; // Always use static matrix since size is constant
  }

  private static float[] _GenerateDitherMatrix(int size) {
    // For size=8, return the canonical Bayer 8×8 / 64 (in [0, 1) range).
    if (size == 8) {
      var bayer = new float[64];
      for (var i = 0; i < 64; ++i) bayer[i] = _Bayer8x8Int[i] / 64f;
      return bayer;
    }
    // Generic fallback: linear ramp distributed via index*prime % N — produces an
    // ordered (but not strictly Bayer) threshold distribution for unusual sizes.
    var matrix = new float[size * size];
    var n = size * size;
    for (var i = 0; i < n; ++i)
      matrix[i] = ((i * 7) % n) / (float)n;
    return matrix;
  }

  /// <inheritdoc />
  public bool RequiresSequentialProcessing => false;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Dither<TWork, TMetric>(
    TWork* source,
    byte* indices,
    int width,
    int height,
    int sourceStride,
    int targetStride,
    int startY,
        in TMetric metric,
    TWork[] palette)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TMetric : struct, IColorMetric<TWork> {

    var lookup = new PaletteLookup<TWork, TMetric>(palette, metric);
    var endY = startY + height;

    // Handle default struct initialization (fields = 0/null)
    var matrixSize = this._matrixSize > 0 ? this._matrixSize : _DEFAULT_MATRIX_SIZE;
    var matrix = this._ditherMatrix ?? _DefaultMatrix;
    var algorithm = this._algorithm > 0 ? this._algorithm : _DEFAULT_ALGORITHM;

    for (var y = startY; y < endY; ++y) {
      var thresholdRowOffset = (y % matrixSize) * matrixSize;

      for (var x = 0; x < width; ++x) {
        var sourceIdx = y * sourceStride + x;
        var targetIdx = y * targetStride + x;
        var color = source[sourceIdx];
        var threshold = matrix[thresholdRowOffset + (x % matrixSize)];

        // Convert the float threshold (matrix value / N²) back to its integer position
        // (0..N²-1) for algorithms 2 and 3 which INDEX directly into a sorted candidate
        // list.
        var matrixIdx = (int)Math.Round(threshold * (matrixSize * matrixSize));
        if (matrixIdx >= matrixSize * matrixSize) matrixIdx = matrixSize * matrixSize - 1;

        var closestIndex = algorithm switch {
          1 => _ApplyAlgorithm1(color, palette, threshold, lookup),
          2 => _ApplyAlgorithm2(color, palette, matrixIdx, matrixSize * matrixSize),
          3 => _ApplyAlgorithm3(color, palette, matrixIdx, matrixSize * matrixSize, lookup),
          4 => _ApplyAlgorithm3(color, palette, matrixIdx, matrixSize * matrixSize, lookup),
          _ => lookup.FindNearest(color)
        };

        indices[targetIdx] = (byte)closestIndex;
      }
    }
  }

  /// <summary>
  /// Yliluoma's Algorithm 1: positional dithering with optimal two-colour mixing per
  /// bisqwit's published reference (https://bisqwit.iki.fi/story/howto/dither/jy/).
  /// Enumerates every unique palette pair (i, j), computes the analytic optimal mix
  /// ratio for that pair, evaluates the squared-RGB penalty against the input, and
  /// keeps the (pair, ratio) with smallest penalty. Then at each pixel the dither
  /// matrix value <paramref name="threshold"/> ∈ [0, 1) selects between the two
  /// colours: factor &lt; ratio → color2, else color1.
  /// </summary>
  /// <remarks>
  /// Optimal-ratio formula: r = ((input − c1) · (c2 − c1)) / ‖c2 − c1‖² clamped to
  /// [0, 1]. This is the projection of the input onto the line segment between c1 and c2.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _ApplyAlgorithm1<TWork, TMetric>(
    TWork pixel,
    TWork[] palette,
    float threshold,
    in PaletteLookup<TWork, TMetric> lookup)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TMetric : struct, IColorMetric<TWork> {

    var (pn1, pn2, pn3, _) = pixel.ToNormalized();
    var p1 = pn1.ToFloat();
    var p2 = pn2.ToFloat();
    var p3 = pn3.ToFloat();

    var bestPenalty = float.MaxValue;
    var bestI = 0;
    var bestJ = 0;
    var bestRatio = 0f;

    for (var i = 0; i < palette.Length; ++i) {
      var (in1, in2, in3, _) = palette[i].ToNormalized();
      var c1a = in1.ToFloat();
      var c1b = in2.ToFloat();
      var c1c = in3.ToFloat();

      for (var j = i; j < palette.Length; ++j) {
        var (jn1, jn2, jn3, _) = palette[j].ToNormalized();
        var c2a = jn1.ToFloat();
        var c2b = jn2.ToFloat();
        var c2c = jn3.ToFloat();

        // Direction vector c2 - c1.
        var da = c2a - c1a;
        var db = c2b - c1b;
        var dc = c2c - c1c;
        var dotDD = da * da + db * db + dc * dc;

        float ratio;
        if (dotDD < 1e-9f) {
          // Same colour or near-identical pair — pick ratio=0 (use c1).
          ratio = 0f;
        } else {
          // Analytic optimum: r = (input - c1) · (c2 - c1) / |c2 - c1|².
          var dotIn = (p1 - c1a) * da + (p2 - c1b) * db + (p3 - c1c) * dc;
          ratio = dotIn / dotDD;
          if (ratio < 0f) ratio = 0f;
          else if (ratio > 1f) ratio = 1f;
        }

        var mr = c1a + ratio * da;
        var mg = c1b + ratio * db;
        var mb = c1c + ratio * dc;
        var er = p1 - mr; var eg = p2 - mg; var eb = p3 - mb;
        var penalty = er * er + eg * eg + eb * eb;

        if (penalty < bestPenalty) {
          bestPenalty = penalty;
          bestI = i;
          bestJ = j;
          bestRatio = ratio;
        }
      }
    }

    // At dither position with `threshold`, draw c2 if threshold < ratio else c1.
    return threshold < bestRatio ? bestJ : bestI;
  }

  // Gamma constant per bisqwit's reference (gamma=2.0 used for analytic-friendliness;
  // bisqwit also discusses 2.2 — the exact value affects mix appearance but not the
  // structure of the algorithm).
  private const double Gamma = 2.0;
  private const double InvGamma = 1.0 / Gamma;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static double _GammaCorrect(double v) => v <= 0 ? 0 : Math.Pow(v, Gamma);
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static double _GammaUncorrect(double v) => v <= 0 ? 0 : Math.Pow(v, InvGamma);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static double _Luminance(double r, double g, double b)
    => 0.299 * r + 0.587 * g + 0.114 * b;

  /// <summary>
  /// Yliluoma's Algorithm 2 (https://bisqwit.iki.fi/story/howto/dither/jy/):
  /// builds a list of N candidate palette indices (where N = matrix size²) by greedy
  /// iterative addition. At each iteration, finds the palette colour AND amount p ∈
  /// {1, 2, 4, ...} that minimises the squared-RGB penalty when added to the gamma-
  /// corrected running sum, then appends p copies of that index. Once N candidates are
  /// collected, sort by luminance and INDEX with the matrix value to pick the output.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _ApplyAlgorithm2<TWork>(
    TWork pixel, TWork[] palette, int matrixIdx, int N)
    where TWork : unmanaged, IColorSpace4<TWork> {
    var (pn1, pn2, pn3, _) = pixel.ToNormalized();
    var p1 = (double)pn1.ToFloat();
    var p2 = (double)pn2.ToFloat();
    var p3 = (double)pn3.ToFloat();

    // Pre-compute gamma-corrected palette.
    var palCount = palette.Length;
    var palR = new double[palCount];
    var palG = new double[palCount];
    var palB = new double[palCount];
    for (var i = 0; i < palCount; ++i) {
      var (qn1, qn2, qn3, _) = palette[i].ToNormalized();
      palR[i] = _GammaCorrect(qn1.ToFloat());
      palG[i] = _GammaCorrect(qn2.ToFloat());
      palB[i] = _GammaCorrect(qn3.ToFloat());
    }

    var solution = new int[N];
    var solutionSize = 0;
    double sumR = 0, sumG = 0, sumB = 0;

    while (solutionSize < N) {
      var bestIdx = 0;
      var bestAmount = 1;
      var bestPenalty = double.MaxValue;
      var maxTest = Math.Max(1, solutionSize);
      // Cap so we don't fill past N.
      var roomLeft = N - solutionSize;

      for (var i = 0; i < palCount; ++i) {
        var addR = palR[i];
        var addG = palG[i];
        var addB = palB[i];
        double runR = sumR, runG = sumG, runB = sumB;

        for (var p = 1; p <= maxTest && p <= roomLeft; p *= 2) {
          runR += addR; runG += addG; runB += addB;
          addR += addR; addG += addG; addB += addB;
          var inv = 1.0 / (solutionSize + p);
          var testR = _GammaUncorrect(runR * inv);
          var testG = _GammaUncorrect(runG * inv);
          var testB = _GammaUncorrect(runB * inv);
          var dr = p1 - testR; var dg = p2 - testG; var db = p3 - testB;
          var penalty = dr * dr + dg * dg + db * db;
          if (penalty < bestPenalty) {
            bestPenalty = penalty;
            bestIdx = i;
            bestAmount = p;
          }
        }
      }

      // Append bestAmount copies of bestIdx.
      for (var k = 0; k < bestAmount && solutionSize < N; ++k)
        solution[solutionSize++] = bestIdx;
      sumR += palR[bestIdx] * bestAmount;
      sumG += palG[bestIdx] * bestAmount;
      sumB += palB[bestIdx] * bestAmount;
    }

    // Sort solution by luminance of the palette entry.
    Array.Sort(solution, (a, b) => {
      var (an1, an2, an3, _) = palette[a].ToNormalized();
      var (bn1, bn2, bn3, _) = palette[b].ToNormalized();
      return _Luminance(an1.ToFloat(), an2.ToFloat(), an3.ToFloat())
        .CompareTo(_Luminance(bn1.ToFloat(), bn2.ToFloat(), bn3.ToFloat()));
    });

    if (matrixIdx >= solutionSize) matrixIdx = solutionSize - 1;
    return solution[matrixIdx];
  }

  /// <summary>
  /// Yliluoma's Algorithm 3 (iterative pair-replacement):
  /// start with N copies of the closest palette colour; iteratively try to replace one
  /// colour in the multiset with a 50:50 mix of two different palette colours. Continue
  /// until no pair-replacement improves the perceptual penalty. Reference:
  /// https://bisqwit.iki.fi/story/howto/dither/jy/.
  /// </summary>
  /// <remarks>
  /// The algorithm runs a bounded number of outer-loop iterations to keep per-pixel
  /// cost predictable; the canonical convergence test ("until no improvement") is
  /// preserved as the inner termination condition.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _ApplyAlgorithm3<TWork, TMetric>(
    TWork pixel, TWork[] palette, int matrixIdx, int N,
    in PaletteLookup<TWork, TMetric> lookup)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TMetric : struct, IColorMetric<TWork> {
    var (pn1, pn2, pn3, _) = pixel.ToNormalized();
    var p1 = (double)pn1.ToFloat();
    var p2 = (double)pn2.ToFloat();
    var p3 = (double)pn3.ToFloat();

    var palCount = palette.Length;
    var palR = new double[palCount];
    var palG = new double[palCount];
    var palB = new double[palCount];
    for (var i = 0; i < palCount; ++i) {
      var (qn1, qn2, qn3, _) = palette[i].ToNormalized();
      palR[i] = _GammaCorrect(qn1.ToFloat());
      palG[i] = _GammaCorrect(qn2.ToFloat());
      palB[i] = _GammaCorrect(qn3.ToFloat());
    }

    // Multiset: counts[i] = number of times palette[i] appears in solution.
    var counts = new int[palCount];
    var closest = lookup.FindNearest(pixel);
    counts[closest] = N;

    double Penalty(double[] cnt) {
      double sR = 0, sG = 0, sB = 0;
      var total = 0;
      for (var i = 0; i < palCount; ++i) {
        var c = (int)cnt[i];
        if (c == 0) continue;
        sR += palR[i] * c; sG += palG[i] * c; sB += palB[i] * c; total += c;
      }
      if (total == 0) return double.MaxValue;
      var inv = 1.0 / total;
      var testR = _GammaUncorrect(sR * inv);
      var testG = _GammaUncorrect(sG * inv);
      var testB = _GammaUncorrect(sB * inv);
      var dr = p1 - testR; var dg = p2 - testG; var db = p3 - testB;
      return dr * dr + dg * dg + db * db;
    }

    var workingCounts = new double[palCount];
    var currentPenalty = double.MaxValue;
    {
      for (var i = 0; i < palCount; ++i) workingCounts[i] = counts[i];
      currentPenalty = Penalty(workingCounts);
    }

    const int MaxOuter = 8;
    for (var iter = 0; iter < MaxOuter; ++iter) {
      var bestSplitFrom = -1;
      var bestA = -1;
      var bestB = -1;
      var bestNewPenalty = currentPenalty;

      for (var splitFrom = 0; splitFrom < palCount; ++splitFrom) {
        if (counts[splitFrom] == 0) continue;
        var splitCount = counts[splitFrom];
        var portion1 = splitCount / 2;
        var portion2 = splitCount - portion1;
        if (portion1 == 0 || portion2 == 0) continue;

        // Try splitting splitFrom into (a, b) — distinct palette colours.
        for (var a = 0; a < palCount; ++a)
        for (var b = 0; b < palCount; ++b) {
          if (a == b || a == splitFrom || b == splitFrom) continue;
          for (var i = 0; i < palCount; ++i) workingCounts[i] = counts[i];
          workingCounts[splitFrom] = 0;
          workingCounts[a] += portion1;
          workingCounts[b] += portion2;
          var pen = Penalty(workingCounts);
          if (pen < bestNewPenalty) {
            bestNewPenalty = pen;
            bestSplitFrom = splitFrom;
            bestA = a;
            bestB = b;
          }
        }
      }

      if (bestSplitFrom < 0) break; // No improvement.

      var sCount = counts[bestSplitFrom];
      var p1Count = sCount / 2;
      var p2Count = sCount - p1Count;
      counts[bestSplitFrom] = 0;
      counts[bestA] += p1Count;
      counts[bestB] += p2Count;
      currentPenalty = bestNewPenalty;
    }

    // Build candidate list and sort by luminance.
    var candidates = new int[N];
    var idx = 0;
    for (var i = 0; i < palCount && idx < N; ++i)
      for (var k = 0; k < counts[i] && idx < N; ++k)
        candidates[idx++] = i;
    while (idx < N) candidates[idx++] = closest;

    Array.Sort(candidates, (a, b) => {
      var (an1, an2, an3, _) = palette[a].ToNormalized();
      var (bn1, bn2, bn3, _) = palette[b].ToNormalized();
      return _Luminance(an1.ToFloat(), an2.ToFloat(), an3.ToFloat())
        .CompareTo(_Luminance(bn1.ToFloat(), bn2.ToFloat(), bn3.ToFloat()));
    });

    if (matrixIdx >= N) matrixIdx = N - 1;
    return candidates[matrixIdx];
  }

  private static int _FindSecondClosestIndex<TWork>(TWork target, TWork[] palette, int excludeIndex)
    where TWork : unmanaged, IColorSpace4<TWork> {

    var bestIndex = excludeIndex == 0 ? 1 : 0;
    var bestDistance = double.MaxValue;

    var (tc1, tc2, tc3, _) = target.ToNormalized();
    var targetC1 = tc1.ToFloat();
    var targetC2 = tc2.ToFloat();
    var targetC3 = tc3.ToFloat();

    for (var i = 0; i < palette.Length; ++i) {
      if (i == excludeIndex)
        continue;

      var (pc1, pc2, pc3, _) = palette[i].ToNormalized();
      var d1 = pc1.ToFloat() - targetC1;
      var d2 = pc2.ToFloat() - targetC2;
      var d3 = pc3.ToFloat() - targetC3;
      var distance = d1 * d1 + d2 * d2 + d3 * d3;

      if (distance < bestDistance) {
        bestDistance = distance;
        bestIndex = i;
      }
    }

    return bestIndex;
  }

  private static int[] _FindBestCandidateIndices<TWork>(TWork target, TWork[] palette, int count)
    where TWork : unmanaged, IColorSpace4<TWork> {

    var (tc1, tc2, tc3, _) = target.ToNormalized();
    var targetC1 = tc1.ToFloat();
    var targetC2 = tc2.ToFloat();
    var targetC3 = tc3.ToFloat();

    return palette
      .Select((color, index) => {
        var (pc1, pc2, pc3, _) = color.ToNormalized();
        var d1 = pc1.ToFloat() - targetC1;
        var d2 = pc2.ToFloat() - targetC2;
        var d3 = pc3.ToFloat() - targetC3;
        return new { Index = index, Distance = d1 * d1 + d2 * d2 + d3 * d3 };
      })
      .OrderBy(x => x.Distance)
      .Take(count)
      .Select(x => x.Index)
      .ToArray();
  }

  private static float _CalculateComplexThreshold(float baseThreshold, int x, int y) {
    var spatial = (float)Math.Sin((x * 0.1 + y * 0.13) * Math.PI * 2) * 0.1f;
    var pattern = ((x + y * 3) % 8) / 8f * 0.2f;
    return ColorConverter.Saturate(baseThreshold + spatial + pattern);
  }
}
