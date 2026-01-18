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
/// Thomas Knoll's ordered dithering algorithm with color candidate generation.
/// </summary>
/// <remarks>
/// <para>Uses Bayer matrix thresholds to select from multiple candidate colors.</para>
/// <para>Generates candidates by iteratively finding closest colors and accumulating error.</para>
/// </remarks>
[Ditherer("Knoll", Description = "Pattern dithering with candidate generation", Type = DitheringType.Ordered, Author = "Thomas Knoll")]
public readonly struct KnollDitherer : IDitherer {

  private const int _DEFAULT_MATRIX_SIZE = 4;
  private const int _DEFAULT_CANDIDATE_COUNT = 16;
  private const float _DEFAULT_ERROR_MULTIPLIER = 0.5f;
  private static readonly byte[] _DefaultMatrix = _GenerateBayerMatrix(_DEFAULT_MATRIX_SIZE);

  private readonly byte[] _matrix;
  private readonly int _matrixSize;
  private readonly int _candidateCount;
  private readonly float _errorMultiplier;

  /// <summary>Pre-configured instance with 4x4 Bayer matrix (default).</summary>
  public static KnollDitherer Default { get; } = new(_GenerateBayerMatrix(4), 4, 16, 0.5f);

  /// <summary>Pre-configured instance with 8x8 Bayer matrix.</summary>
  public static KnollDitherer Bayer8x8 { get; } = new(_GenerateBayerMatrix(8), 8, 16, 0.5f);

  /// <summary>Pre-configured instance for high quality (more candidates, higher error multiplier).</summary>
  public static KnollDitherer HighQuality { get; } = new(_GenerateBayerMatrix(4), 4, 32, 0.75f);

  /// <summary>Pre-configured instance for fast processing (smaller matrix, fewer candidates).</summary>
  public static KnollDitherer Fast { get; } = new(_GenerateBayerMatrix(2), 2, 8, 0.25f);

  /// <summary>
  /// Creates a Knoll ditherer with custom settings.
  /// </summary>
  /// <param name="bayerSize">Size of the Bayer matrix (must be power of 2).</param>
  /// <param name="candidateCount">Number of color candidates to generate.</param>
  /// <param name="errorMultiplier">Error multiplier for candidate generation (0-1).</param>
  public KnollDitherer(int bayerSize = 4, int candidateCount = 16, float errorMultiplier = 0.5f) {
    this._matrix = _GenerateBayerMatrix(bayerSize);
    this._matrixSize = bayerSize;
    this._candidateCount = candidateCount;
    this._errorMultiplier = errorMultiplier;
  }

  private KnollDitherer(byte[] matrix, int matrixSize, int candidateCount, float errorMultiplier) {
    this._matrix = matrix;
    this._matrixSize = matrixSize;
    this._candidateCount = candidateCount;
    this._errorMultiplier = errorMultiplier;
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
    var matrix = this._matrix ?? _DefaultMatrix;
    var matrixSize = this._matrixSize > 0 ? this._matrixSize : _DEFAULT_MATRIX_SIZE;
    var maxThreshold = matrixSize * matrixSize - 1;
    var candidateCount = this._candidateCount > 0 ? this._candidateCount : _DEFAULT_CANDIDATE_COUNT;
    var errorMultiplier = this._errorMultiplier > 0f ? this._errorMultiplier : _DEFAULT_ERROR_MULTIPLIER;

    for (var y = startY; y < endY; ++y) {
      var matrixRowOffset = (y % matrixSize) * matrixSize;

      for (var x = 0; x < width; ++x) {
        var sourceIdx = y * sourceStride + x;
        var targetIdx = y * targetStride + x;

        // Decode source pixel
        var pixel = decoder.Decode(source[sourceIdx]);
        var (c1, c2, c3, alpha) = pixel.ToNormalized();
        var originalC1 = c1.ToFloat();
        var originalC2 = c2.ToFloat();
        var originalC3 = c3.ToFloat();
        var originalA = alpha.ToFloat();

        // Generate candidates
        var candidates = _GenerateCandidates(
          originalC1, originalC2, originalC3, originalA,
          palette, lookup, candidateCount, errorMultiplier
        );

        if (candidates.Count == 0) {
          indices[targetIdx] = 0;
          continue;
        }

        // Use matrix threshold to select candidate
        var thresholdValue = matrix[matrixRowOffset + (x % matrixSize)];
        var candidateIndex = (thresholdValue * candidates.Count) / (maxThreshold + 1);
        candidateIndex = Math.Min(candidateIndex, candidates.Count - 1);

        indices[targetIdx] = (byte)candidates[candidateIndex];
      }
    }
  }

  private static List<int> _GenerateCandidates<TWork, TMetric>(
    float originalC1, float originalC2, float originalC3, float originalA,
    TWork[] palette,
    in PaletteLookup<TWork, TMetric> lookup,
    int candidateCount,
    float errorMultiplier)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TMetric : struct, IColorMetric<TWork> {

    var candidates = new List<int>(candidateCount);

    if (palette.Length == 0)
      return candidates;

    var goalC1 = originalC1;
    var goalC2 = originalC2;
    var goalC3 = originalC3;
    var goalA = originalA;

    for (var i = 0; i < candidateCount; ++i) {
      var goalColor = ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromFloatClamped(goalC1),
        UNorm32.FromFloatClamped(goalC2),
        UNorm32.FromFloatClamped(goalC3),
        UNorm32.FromFloatClamped(goalA)
      );

      var closestIndex = lookup.FindNearest(goalColor, out var closestColor);
      candidates.Add(closestIndex);

      // Calculate error and accumulate
      var (cc1, cc2, cc3, ca) = closestColor.ToNormalized();
      var errorC1 = (originalC1 - cc1.ToFloat()) * errorMultiplier;
      var errorC2 = (originalC2 - cc2.ToFloat()) * errorMultiplier;
      var errorC3 = (originalC3 - cc3.ToFloat()) * errorMultiplier;
      var errorA = (originalA - ca.ToFloat()) * errorMultiplier;

      goalC1 += errorC1;
      goalC2 += errorC2;
      goalC3 += errorC3;
      goalA += errorA;
    }

    // Sort candidates by luminance for smooth transitions
    candidates.Sort((a, b) => {
      var colorA = palette[a];
      var colorB = palette[b];
      var (ac1, ac2, ac3, _) = colorA.ToNormalized();
      var (bc1, bc2, bc3, _) = colorB.ToNormalized();
      // Use standard luminance formula
      var luminanceA = 0.299 * ac1.ToFloat() + 0.587 * ac2.ToFloat() + 0.114 * ac3.ToFloat();
      var luminanceB = 0.299 * bc1.ToFloat() + 0.587 * bc2.ToFloat() + 0.114 * bc3.ToFloat();
      return luminanceA.CompareTo(luminanceB);
    });

    return candidates;
  }

  /// <summary>
  /// Generates a Bayer dithering matrix using recursive construction.
  /// </summary>
  private static byte[] _GenerateBayerMatrix(int size) {
    if (size < 2 || (size & (size - 1)) != 0)
      throw new ArgumentException("Size must be a power of 2 and at least 2", nameof(size));

    return _GenerateBayerRecursive(size);
  }

  private static byte[] _GenerateBayerRecursive(int size) {
    if (size == 2)
      return [0, 2, 3, 1];

    var halfSize = size / 2;
    var smallMatrix = _GenerateBayerRecursive(halfSize);
    var result = new byte[size * size];

    for (var y = 0; y < halfSize; ++y) {
      var smallRowOffset = y * halfSize;
      var topRowOffset = y * size;
      var bottomRowOffset = (y + halfSize) * size;

      for (var x = 0; x < halfSize; ++x) {
        var baseValue = (byte)(4 * smallMatrix[smallRowOffset + x]);

        // Top-left: 4*B(n) + 0
        result[topRowOffset + x] = baseValue;

        // Top-right: 4*B(n) + 2
        result[topRowOffset + x + halfSize] = (byte)(baseValue + 2);

        // Bottom-left: 4*B(n) + 3
        result[bottomRowOffset + x] = (byte)(baseValue + 3);

        // Bottom-right: 4*B(n) + 1
        result[bottomRowOffset + x + halfSize] = (byte)(baseValue + 1);
      }
    }

    return result;
  }
}
