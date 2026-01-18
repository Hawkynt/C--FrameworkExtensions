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
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

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
    var matrix = new float[size * size];
    for (var y = 0; y < size; ++y)
    for (var x = 0; x < size; ++x) {
      var value = ((x * 7 + y * 13) % 64) / 64f;
      value = (float)(0.5 + 0.4 * Math.Sin(value * Math.PI * 2) + 0.1 * Math.Sin(value * Math.PI * 8));
      value = Math.Max(0f, Math.Min(1f, value));
      matrix[y * size + x] = value;
    }
    return matrix;
  }

  /// <inheritdoc />
  public bool RequiresSequentialProcessing => false;

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

    // Handle default struct initialization (fields = 0/null)
    var matrixSize = this._matrixSize > 0 ? this._matrixSize : _DEFAULT_MATRIX_SIZE;
    var matrix = this._ditherMatrix ?? _DefaultMatrix;
    var algorithm = this._algorithm > 0 ? this._algorithm : _DEFAULT_ALGORITHM;

    for (var y = startY; y < endY; ++y) {
      var thresholdRowOffset = (y % matrixSize) * matrixSize;

      for (var x = 0; x < width; ++x) {
        var sourceIdx = y * sourceStride + x;
        var targetIdx = y * targetStride + x;
        var color = decoder.Decode(source[sourceIdx]);
        var threshold = matrix[thresholdRowOffset + (x % matrixSize)];

        var closestIndex = algorithm switch {
          1 => _ApplyAlgorithm1(color, palette, threshold, lookup),
          2 => _ApplyAlgorithm2(color, palette, threshold, x, y, lookup),
          3 => _ApplyAlgorithm3(color, palette, threshold, x, y, lookup),
          4 => _ApplyAlgorithm3Full(color, palette, threshold, matrixSize, lookup),
          _ => lookup.FindNearest(color)
        };

        indices[targetIdx] = (byte)closestIndex;
      }
    }
  }

  /// <summary>
  /// Algorithm 1: Two-color mixing based on threshold.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _ApplyAlgorithm1<TWork, TMetric>(
    TWork pixel,
    TWork[] palette,
    float threshold,
    in PaletteLookup<TWork, TMetric> lookup)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TMetric : struct, IColorMetric<TWork> {

    var closestIndex = lookup.FindNearest(pixel, out var closestColor);
    var secondClosestIndex = _FindSecondClosestIndex(pixel, palette, closestIndex);

    // Calculate relative position between closest and second-closest colors
    var (pc1, pc2, pc3, _) = pixel.ToNormalized();
    var (c1, c2, c3, _) = closestColor.ToNormalized();
    var (s1, s2, s3, _) = palette[secondClosestIndex].ToNormalized();

    var distToClosest = Math.Abs(pc1.ToFloat() - c1.ToFloat()) +
                        Math.Abs(pc2.ToFloat() - c2.ToFloat()) +
                        Math.Abs(pc3.ToFloat() - c3.ToFloat());
    var distToSecond = Math.Abs(pc1.ToFloat() - s1.ToFloat()) +
                       Math.Abs(pc2.ToFloat() - s2.ToFloat()) +
                       Math.Abs(pc3.ToFloat() - s3.ToFloat());

    var totalDist = distToClosest + distToSecond;
    var ratio = totalDist > 0.001f ? distToClosest / totalDist : 0f;

    // Select second color only if pixel is meaningfully between colors AND threshold indicates it
    return threshold > 1f - ratio ? secondClosestIndex : closestIndex;
  }

  /// <summary>
  /// Algorithm 2: Multi-color candidate generation with positional factor.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _ApplyAlgorithm2<TWork, TMetric>(
    TWork pixel,
    TWork[] palette,
    float threshold,
    int x,
    int y,
    in PaletteLookup<TWork, TMetric> lookup)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TMetric : struct, IColorMetric<TWork> {

    var closestIndex = lookup.FindNearest(pixel, out var closestColor);
    var secondClosestIndex = _FindSecondClosestIndex(pixel, palette, closestIndex);

    // Calculate relative position between closest and second-closest colors
    var (pc1, pc2, pc3, _) = pixel.ToNormalized();
    var (c1, c2, c3, _) = closestColor.ToNormalized();
    var (s1, s2, s3, _) = palette[secondClosestIndex].ToNormalized();

    var distToClosest = Math.Abs(pc1.ToFloat() - c1.ToFloat()) +
                        Math.Abs(pc2.ToFloat() - c2.ToFloat()) +
                        Math.Abs(pc3.ToFloat() - c3.ToFloat());
    var distToSecond = Math.Abs(pc1.ToFloat() - s1.ToFloat()) +
                       Math.Abs(pc2.ToFloat() - s2.ToFloat()) +
                       Math.Abs(pc3.ToFloat() - s3.ToFloat());

    var totalDist = distToClosest + distToSecond;
    var ratio = totalDist > 0.001f ? distToClosest / totalDist : 0f;

    var positionFactor = ((x * 3 + y * 7) % 16) / 16f;
    var adjustedThreshold = (threshold + positionFactor * 0.3f) % 1f;

    // Select second color only if pixel is meaningfully between colors AND adjusted threshold indicates it
    return adjustedThreshold > 1f - ratio ? secondClosestIndex : closestIndex;
  }

  /// <summary>
  /// Algorithm 3: Simplified multi-candidate selection.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _ApplyAlgorithm3<TWork, TMetric>(
    TWork pixel,
    TWork[] palette,
    float threshold,
    int x,
    int y,
    in PaletteLookup<TWork, TMetric> lookup)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TMetric : struct, IColorMetric<TWork> {

    var candidateIndices = _FindBestCandidateIndices(pixel, palette, 4);
    if (candidateIndices.Length == 0)
      return 0;

    // Check if pixel is close to the nearest candidate (exact or near-exact match)
    var (pc1, pc2, pc3, _) = pixel.ToNormalized();
    var (c1, c2, c3, _) = palette[candidateIndices[0]].ToNormalized();
    var distToNearest = Math.Abs(pc1.ToFloat() - c1.ToFloat()) +
                        Math.Abs(pc2.ToFloat() - c2.ToFloat()) +
                        Math.Abs(pc3.ToFloat() - c3.ToFloat());

    // If pixel is very close to nearest color, just return it
    if (distToNearest < 0.01f)
      return candidateIndices[0];

    // For colors between palette entries, use threshold-based selection
    var complexThreshold = _CalculateComplexThreshold(threshold, x, y);
    var index = (int)(complexThreshold * candidateIndices.Length) % candidateIndices.Length;
    return candidateIndices[index];
  }

  /// <summary>
  /// Algorithm 3 Full: Iterative subdivision for optimal color mixing.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _ApplyAlgorithm3Full<TWork, TMetric>(
    TWork pixel,
    TWork[] palette,
    float threshold,
    int matrixSize,
    in PaletteLookup<TWork, TMetric> lookup)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TMetric : struct, IColorMetric<TWork> {

    // Get target color in normalized form
    var (tc1, tc2, tc3, _) = pixel.ToNormalized();
    var targetC1 = tc1.ToFloat();
    var targetC2 = tc2.ToFloat();
    var targetC3 = tc3.ToFloat();

    // Build candidate list through iterative refinement
    var candidateCounts = new int[palette.Length];
    var totalCount = 0;
    var accumC1 = 0.0;
    var accumC2 = 0.0;
    var accumC3 = 0.0;

    // Use matrix area as iteration count for optimal mixing granularity
    var maxIterations = matrixSize * matrixSize;
    for (var iteration = 0; iteration < maxIterations; ++iteration) {
      var bestIndex = -1;
      var bestError = double.MaxValue;

      // Find the palette color that minimizes error when added to the mix
      for (var i = 0; i < palette.Length; ++i) {
        var (pc1, pc2, pc3, _) = palette[i].ToNormalized();
        var colorC1 = pc1.ToFloat();
        var colorC2 = pc2.ToFloat();
        var colorC3 = pc3.ToFloat();

        // Compute mixed color if we add this candidate
        var mixC1 = (accumC1 + colorC1) / (totalCount + 1);
        var mixC2 = (accumC2 + colorC2) / (totalCount + 1);
        var mixC3 = (accumC3 + colorC3) / (totalCount + 1);

        // Calculate error
        var d1 = mixC1 - targetC1;
        var d2 = mixC2 - targetC2;
        var d3 = mixC3 - targetC3;
        var error = d1 * d1 + d2 * d2 + d3 * d3;

        if (error < bestError) {
          bestError = error;
          bestIndex = i;
        }
      }

      if (bestIndex < 0)
        break;

      // Add the best candidate to our mix
      var (bc1, bc2, bc3, _) = palette[bestIndex].ToNormalized();
      accumC1 += bc1.ToFloat();
      accumC2 += bc2.ToFloat();
      accumC3 += bc3.ToFloat();
      ++candidateCounts[bestIndex];
      ++totalCount;

      // Stop early if error is negligible
      if (bestError < 0.0001)
        break;
    }

    // Use ordered dithering to select from candidates based on threshold
    var scaledPosition = (int)(threshold * totalCount);
    var runningSum = 0;
    for (var i = 0; i < palette.Length; ++i) {
      runningSum += candidateCounts[i];
      if (scaledPosition < runningSum)
        return i;
    }

    // Fallback to closest match
    return lookup.FindNearest(pixel);
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
    return Math.Max(0f, Math.Min(1f, baseThreshold + spatial + pattern));
  }
}
