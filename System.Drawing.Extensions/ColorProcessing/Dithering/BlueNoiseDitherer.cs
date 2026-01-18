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
/// Blue noise dithering using precomputed noise textures for high-quality spatial distribution.
/// </summary>
/// <remarks>
/// <para>
/// Blue noise dithering produces visually pleasing results by distributing error using
/// a precomputed noise pattern that has most of its energy at high frequencies.
/// This eliminates low-frequency patterns that are more noticeable to the human eye.
/// </para>
/// <para>
/// The blue noise pattern is tiled across the image, providing consistent high-quality
/// results without the directional artifacts common in ordered dithering.
/// </para>
/// <para>
/// Reference: Various authors - Blue noise patterns can be generated using void-and-cluster
/// or other methods. The patterns used here are precomputed for optimal performance.
/// </para>
/// </remarks>
[Ditherer("Blue Noise", Description = "High-quality dithering with blue noise distribution", Type = DitheringType.Noise)]
public readonly struct BlueNoiseDitherer : IDitherer {

  private const int _DEFAULT_MATRIX_SIZE = 8; // Small size for quick initialization; use Size64x64/Size128x128 for higher quality

  // Lazy initialization for all matrices to avoid slow static initialization (void-and-cluster is O(n^6))
  private static readonly Lazy<float[]> _DefaultMatrix = new(() => _GenerateBlueNoiseMatrix(_DEFAULT_MATRIX_SIZE));
  private static readonly Lazy<float[]> _Matrix64 = new(() => _GenerateBlueNoiseMatrix(64));
  private static readonly Lazy<float[]> _Matrix128 = new(() => _GenerateBlueNoiseMatrix(128));

  private readonly int _matrixSize;
  private readonly float[] _noiseMatrix;

  /// <summary>Pre-configured instance with 8x8 noise matrix (default size).</summary>
  public static BlueNoiseDitherer Size8x8 => new(_DEFAULT_MATRIX_SIZE, _DefaultMatrix.Value);

  /// <summary>Pre-configured instance with 64x64 noise matrix.</summary>
  public static BlueNoiseDitherer Size64x64 => new(64, _Matrix64.Value);

  /// <summary>Pre-configured instance with 128x128 noise matrix (higher quality).</summary>
  public static BlueNoiseDitherer Size128x128 => new(128, _Matrix128.Value);

  /// <summary>
  /// Creates a blue noise ditherer with the specified matrix size.
  /// </summary>
  /// <param name="matrixSize">Size of the noise matrix (e.g., 64, 128).</param>
  public BlueNoiseDitherer(int matrixSize = _DEFAULT_MATRIX_SIZE) {
    this._matrixSize = matrixSize;
    // Reuse static default matrix if matching size, otherwise generate fresh
    this._noiseMatrix = matrixSize == _DEFAULT_MATRIX_SIZE ? _DefaultMatrix.Value : _GenerateBlueNoiseMatrix(matrixSize);
  }

  /// <summary>
  /// Private constructor for lazy-initialized static instances.
  /// </summary>
  private BlueNoiseDitherer(int matrixSize, float[] precomputedMatrix) {
    this._matrixSize = matrixSize;
    this._noiseMatrix = precomputedMatrix;
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

    // Handle default struct initialization (matrixSize = 0, noiseMatrix = null)
    var matrixSize = this._matrixSize > 0 ? this._matrixSize : _DEFAULT_MATRIX_SIZE;
    var noiseMatrix = this._noiseMatrix ?? _DefaultMatrix.Value;

    for (var y = startY; y < endY; ++y) {
      var matrixRowOffset = (y % matrixSize) * matrixSize;

      for (var x = 0; x < width; ++x) {
        var sourceIdx = y * sourceStride + x;
        var targetIdx = y * targetStride + x;

        // Decode source pixel
        var color = decoder.Decode(source[sourceIdx]);

        // First check if this color is very close to a palette entry (exact or near-exact match)
        // In that case, skip threshold adjustment to preserve exact matches
        var nearestIdx = lookup.FindNearest(color, out var nearestColor);
        var (c1, c2, c3, a) = color.ToNormalized();
        var (n1, n2, n3, na) = nearestColor.ToNormalized();
        var distToNearest = Math.Abs(c1.ToFloat() - n1.ToFloat()) +
                            Math.Abs(c2.ToFloat() - n2.ToFloat()) +
                            Math.Abs(c3.ToFloat() - n3.ToFloat()) +
                            Math.Abs(a.ToFloat() - na.ToFloat());

        // If the color is very close to the nearest palette entry, use it directly
        if (distToNearest < 0.02f) {
          indices[targetIdx] = (byte)nearestIdx;
          continue;
        }

        // Get threshold from blue noise matrix (range: -0.5 to +0.5)
        var threshold = noiseMatrix[matrixRowOffset + (x % matrixSize)];

        // Apply threshold to each color channel (scaled appropriately for normalized values)
        var adjustedC1 = Math.Max(0f, Math.Min(1f, c1.ToFloat() + threshold));
        var adjustedC2 = Math.Max(0f, Math.Min(1f, c2.ToFloat() + threshold));
        var adjustedC3 = Math.Max(0f, Math.Min(1f, c3.ToFloat() + threshold));

        // Create adjusted color
        var adjustedColor = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped(adjustedC1),
          UNorm32.FromFloatClamped(adjustedC2),
          UNorm32.FromFloatClamped(adjustedC3),
          a
        );

        // Find nearest palette color
        indices[targetIdx] = (byte)lookup.FindNearest(adjustedColor);
      }
    }
  }

  /// <summary>
  /// Generates a blue noise matrix using a simplified void-and-cluster approach.
  /// </summary>
  private static float[] _GenerateBlueNoiseMatrix(int size) {
    var matrix = new float[size * size];
    var binary = new bool[size * size];
    var rank = new int[size * size];
    var totalPixels = size * size;
    var initialOnes = totalPixels / 2;

    // Initialize with checkerboard pattern for initial blue noise characteristics
    var count = 0;
    for (var y = 0; y < size; ++y)
    for (var x = 0; x < size; ++x) {
      var idx = y * size + x;
      if ((x + y) % 2 == 0 && count < initialOnes) {
        binary[idx] = true;
        ++count;
      }
    }

    // Phase 1: Remove tightest clusters
    var currentRank = initialOnes - 1;
    while (count > 0) {
      var (cx, cy) = _FindTightestCluster(binary, size);
      var cIdx = cy * size + cx;
      binary[cIdx] = false;
      rank[cIdx] = currentRank;
      --count;
      --currentRank;
    }

    // Restore initial pattern
    count = 0;
    for (var y = 0; y < size; ++y)
    for (var x = 0; x < size; ++x) {
      var idx = y * size + x;
      if ((x + y) % 2 == 0 && count < initialOnes) {
        binary[idx] = true;
        ++count;
      }
    }

    // Phase 2: Fill largest voids
    currentRank = initialOnes;
    while (count < totalPixels) {
      var (vx, vy) = _FindLargestVoid(binary, size);
      var vIdx = vy * size + vx;
      binary[vIdx] = true;
      rank[vIdx] = currentRank;
      ++count;
      ++currentRank;
    }

    // Convert rank to normalized threshold (range: -0.5 to +0.5)
    for (var y = 0; y < size; ++y)
    for (var x = 0; x < size; ++x) {
      var idx = y * size + x;
      matrix[idx] = (rank[idx] + 0.5f) / totalPixels - 0.5f;
    }

    return matrix;
  }

  private static (int x, int y) _FindTightestCluster(bool[] binary, int size) {
    var maxEnergy = float.MinValue;
    var bestX = 0;
    var bestY = 0;

    for (var y = 0; y < size; ++y)
    for (var x = 0; x < size; ++x) {
      var idx = y * size + x;
      if (!binary[idx])
        continue;

      var energy = _CalculateClusterEnergy(binary, size, x, y);
      if (energy > maxEnergy) {
        maxEnergy = energy;
        bestX = x;
        bestY = y;
      }
    }

    return (bestX, bestY);
  }

  private static (int x, int y) _FindLargestVoid(bool[] binary, int size) {
    var maxEnergy = float.MinValue;
    var bestX = 0;
    var bestY = 0;

    for (var y = 0; y < size; ++y)
    for (var x = 0; x < size; ++x) {
      var idx = y * size + x;
      if (binary[idx])
        continue;

      var energy = _CalculateVoidEnergy(binary, size, x, y);
      if (energy > maxEnergy) {
        maxEnergy = energy;
        bestX = x;
        bestY = y;
      }
    }

    return (bestX, bestY);
  }

  private static float _CalculateClusterEnergy(bool[] binary, int size, int px, int py) {
    var sigma = size / 4.0f;
    var sigmaSq2 = 2 * sigma * sigma;
    var energy = 0f;

    for (var dy = -size / 2; dy <= size / 2; ++dy)
    for (var dx = -size / 2; dx <= size / 2; ++dx) {
      if (dx == 0 && dy == 0)
        continue;

      var nx = (px + dx + size) % size;
      var ny = (py + dy + size) % size;
      var nIdx = ny * size + nx;

      if (binary[nIdx]) {
        var distSq = dx * dx + dy * dy;
        energy += (float)Math.Exp(-distSq / sigmaSq2);
      }
    }

    return energy;
  }

  private static float _CalculateVoidEnergy(bool[] binary, int size, int px, int py) {
    var sigma = size / 4.0f;
    var sigmaSq2 = 2 * sigma * sigma;
    var energy = 0f;

    for (var dy = -size / 2; dy <= size / 2; ++dy)
    for (var dx = -size / 2; dx <= size / 2; ++dx) {
      var nx = (px + dx + size) % size;
      var ny = (py + dy + size) % size;
      var nIdx = ny * size + nx;

      if (!binary[nIdx]) {
        var distSq = dx * dx + dy * dy;
        if (distSq > 0)
          energy += (float)Math.Exp(-distSq / sigmaSq2);
      }
    }

    return energy;
  }
}
