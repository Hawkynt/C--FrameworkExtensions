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
/// Void and Cluster dithering algorithm for generating blue noise patterns.
/// </summary>
/// <remarks>
/// <para>Reference: R.A. Ulichney 1993 "The Void-and-Cluster Method for Dither Array Generation"</para>
/// <para>SPIE/IS&amp;T Symposium on Electronic Imaging Science and Technology</para>
/// <para>This algorithm generates high-quality blue noise patterns for dithering.</para>
/// </remarks>
[Ditherer("Void and Cluster", Description = "Blue noise dithering using void and cluster method", Type = DitheringType.Ordered, Author = "Robert Ulichney", Year = 1993)]
public readonly struct VoidAndClusterDitherer : IDitherer {

  private const int _DEFAULT_MATRIX_SIZE = 4; // Small size for quick initialization (void-and-cluster is O(n^6))

  // Lazy initialization for all matrices to avoid slow static initialization
  private static readonly Lazy<float[,]> _DefaultMatrix = new(() => _GenerateVoidAndClusterMatrix(_DEFAULT_MATRIX_SIZE));
  private static readonly Lazy<float[,]> _Matrix8 = new(() => _GenerateVoidAndClusterMatrix(8));
  private static readonly Lazy<float[,]> _Matrix16 = new(() => _GenerateVoidAndClusterMatrix(16));
  private static readonly Lazy<float[,]> _Matrix32 = new(() => _GenerateVoidAndClusterMatrix(32));

  private readonly int _matrixSize;
  private readonly float[,] _ditherMatrix;

  /// <summary>Pre-configured instance with 4x4 matrix (default size).</summary>
  public static VoidAndClusterDitherer Size4x4 => new(_DEFAULT_MATRIX_SIZE, _DefaultMatrix.Value);

  /// <summary>Pre-configured instance with 8x8 matrix.</summary>
  public static VoidAndClusterDitherer Size8x8 => new(8, _Matrix8.Value);

  /// <summary>Pre-configured instance with 16x16 matrix.</summary>
  public static VoidAndClusterDitherer Size16x16 => new(16, _Matrix16.Value);

  /// <summary>Pre-configured instance with 32x32 matrix.</summary>
  public static VoidAndClusterDitherer Size32x32 => new(32, _Matrix32.Value);

  /// <summary>
  /// Creates a Void and Cluster ditherer with the specified matrix size.
  /// </summary>
  /// <param name="matrixSize">Size of the dither matrix (must be power of 2).</param>
  public VoidAndClusterDitherer(int matrixSize = _DEFAULT_MATRIX_SIZE) {
    this._matrixSize = matrixSize;
    // Reuse static default matrix if matching size, otherwise generate fresh
    this._ditherMatrix = matrixSize == _DEFAULT_MATRIX_SIZE ? _DefaultMatrix.Value : _GenerateVoidAndClusterMatrix(matrixSize);
  }

  /// <summary>
  /// Private constructor for lazy-initialized static instances.
  /// </summary>
  private VoidAndClusterDitherer(int matrixSize, float[,] precomputedMatrix) {
    this._matrixSize = matrixSize;
    this._ditherMatrix = precomputedMatrix;
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

    // Handle default struct initialization (matrixSize = 0, ditherMatrix = null)
    var matrixSize = this._matrixSize > 0 ? this._matrixSize : _DEFAULT_MATRIX_SIZE;
    var ditherMatrix = this._ditherMatrix ?? _DefaultMatrix.Value;

    for (var y = startY; y < endY; ++y) {
      var matrixY = y % matrixSize;

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

        // Get threshold from matrix (centered around 0)
        var threshold = ditherMatrix[x % matrixSize, matrixY] - 0.5f;

        // Apply threshold to each channel
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

  private static float[,] _GenerateVoidAndClusterMatrix(int size) {
    var matrix = new float[size, size];
    var binary = new bool[size, size];
    var rank = new int[size, size];
    var totalPixels = size * size;
    var initialOnes = totalPixels / 2;

    // Initialize with initial pattern (checkerboard-like)
    var count = 0;
    for (var y = 0; y < size; ++y)
    for (var x = 0; x < size; ++x)
      if ((x + y) % 2 == 0 && count < initialOnes) {
        binary[x, y] = true;
        ++count;
      }

    // Phase 1: Remove clusters (tightest clusters of 1s)
    var currentRank = initialOnes - 1;
    while (count > 0) {
      var (cx, cy) = _FindTightestCluster(binary, size, true);
      binary[cx, cy] = false;
      rank[cx, cy] = currentRank;
      --count;
      --currentRank;
    }

    // Restore initial pattern
    count = 0;
    for (var y = 0; y < size; ++y)
    for (var x = 0; x < size; ++x)
      if ((x + y) % 2 == 0 && count < initialOnes) {
        binary[x, y] = true;
        ++count;
      }

    // Phase 2: Fill voids (largest voids, add 1s)
    currentRank = initialOnes;
    while (count < totalPixels) {
      var (vx, vy) = _FindLargestVoid(binary, size);
      binary[vx, vy] = true;
      rank[vx, vy] = currentRank;
      ++count;
      ++currentRank;
    }

    // Convert rank to normalized threshold
    for (var y = 0; y < size; ++y)
    for (var x = 0; x < size; ++x)
      matrix[x, y] = (rank[x, y] + 0.5f) / totalPixels;

    return matrix;
  }

  private static (int x, int y) _FindTightestCluster(bool[,] binary, int size, bool findOnes) {
    var maxEnergy = float.MinValue;
    var bestX = 0;
    var bestY = 0;

    for (var y = 0; y < size; ++y)
    for (var x = 0; x < size; ++x) {
      if (binary[x, y] != findOnes)
        continue;

      var energy = _CalculateEnergy(binary, size, x, y, findOnes);
      if (energy > maxEnergy) {
        maxEnergy = energy;
        bestX = x;
        bestY = y;
      }
    }

    return (bestX, bestY);
  }

  private static (int x, int y) _FindLargestVoid(bool[,] binary, int size) {
    var maxEnergy = float.MinValue;
    var bestX = 0;
    var bestY = 0;

    for (var y = 0; y < size; ++y)
    for (var x = 0; x < size; ++x) {
      if (binary[x, y])
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

  private static float _CalculateEnergy(bool[,] binary, int size, int px, int py, bool findOnes) {
    var sigma = size / 4.0f;
    var sigmaSq2 = 2 * sigma * sigma;
    var energy = 0f;

    for (var dy = -size / 2; dy <= size / 2; ++dy)
    for (var dx = -size / 2; dx <= size / 2; ++dx) {
      if (dx == 0 && dy == 0)
        continue;

      var nx = (px + dx + size) % size;
      var ny = (py + dy + size) % size;

      if (binary[nx, ny] == findOnes) {
        var distSq = dx * dx + dy * dy;
        energy += (float)Math.Exp(-distSq / sigmaSq2);
      }
    }

    return energy;
  }

  private static float _CalculateVoidEnergy(bool[,] binary, int size, int px, int py) {
    var sigma = size / 4.0f;
    var sigmaSq2 = 2 * sigma * sigma;
    var energy = 0f;

    for (var dy = -size / 2; dy <= size / 2; ++dy)
    for (var dx = -size / 2; dx <= size / 2; ++dx) {
      var nx = (px + dx + size) % size;
      var ny = (py + dy + size) % size;

      if (!binary[nx, ny]) {
        var distSq = dx * dx + dy * dy;
        if (distSq > 0)
          energy += (float)Math.Exp(-distSq / sigmaSq2);
      }
    }

    return energy;
  }
}
