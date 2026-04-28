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
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Dithering;

/// <summary>
/// Ulichney's 1987 Pattern-Dependent Threshold (PDT) ditherer — an ordered
/// screen whose per-pixel threshold is modulated by the <i>local</i> bit
/// pattern of source luminance to bias decisions toward forming locally
/// isotropic dot textures.
/// </summary>
/// <remarks>
/// <para>
/// The plain 8×8 Bayer screen produces visible diagonal cross-hatch on flat
/// mid-tone regions because each pixel's threshold decision is independent
/// of its neighbours. Ulichney's PDT modifies the threshold by a small
/// correction derived from the already-thresholded neighbours in a 3×3
/// causal window (above-row + left-of-current on the current row): if the
/// window is locally brighter than the target density, the threshold is
/// nudged upward (making the current pixel more likely to be "off") and
/// vice-versa. The correction amplitude is typically 1/16 of the screen
/// range so the dominant ordered pattern is preserved on structured
/// content.
/// </para>
/// <para>
/// The practical effect on flat regions is that long rows / columns of
/// same-value pixels are broken up: the characteristic Bayer 8×8 "X" pattern
/// softens into a more isotropic stippled grain, without the tile-matching
/// cost of a true blue-noise screen. On high-frequency / edge content the
/// PDT correction is dominated by the base Bayer threshold and the output
/// is indistinguishable from plain Bayer.
/// </para>
/// <para>
/// Because the PDT correction reads already-thresholded neighbours, this is
/// a <i>sequential</i> ditherer (like error diffusion) — the causal window
/// only references pixels already processed in scan order. The cost is one
/// extra scalar add + 3–4 byte loads per pixel, very cheap.
/// </para>
/// <para>
/// Artefact profile: halfway between plain Bayer 8×8 and void-and-cluster
/// blue noise. Flat mid-tone areas look stippled instead of cross-hatched;
/// ramps look the same as Bayer; edges look the same as Bayer. No 64-KB
/// table, no precomputed matrix, O(1) extra memory.
/// </para>
/// <para>Sequential (reads already-processed neighbours via causal window).</para>
/// <para>
/// References: R. Ulichney 1987, <i>Digital Halftoning</i>, MIT Press,
/// Chapter 5 "Ordered dither", §5.7 "Pattern-dependent threshold". See
/// also R. Ulichney 1988, "Dithering with blue noise", <i>Proc. IEEE</i>
/// 76(1), pp. 56-79.
/// </para>
/// </remarks>
[Ditherer("Pattern-Dependent Threshold", Description = "Ulichney 1987 8x8 Bayer screen with causal-neighbourhood threshold correction", Type = DitheringType.Custom, Author = "Robert Ulichney", Year = 1987)]
public readonly struct PatternDependentThresholdDitherer : IDitherer {

  private const int _SIZE = 8;
  private static readonly float[] _Thresholds = _BuildThresholds();

  private readonly float _strength;
  private readonly float _pdtGain;

  /// <summary>Default instance (strength 1.0, PDT gain 0.0625 = 1/16 of screen range).</summary>
  public static PatternDependentThresholdDitherer Instance { get; } = new();

  /// <summary>Creates a PDT ditherer.</summary>
  /// <param name="strength">Overall strength in [0, 1]. Default 1.</param>
  /// <param name="pdtGain">Gain of the causal-neighbour correction, as a
  /// fraction of the screen range. Default 1/16.</param>
  public PatternDependentThresholdDitherer(float strength = 1f, float pdtGain = 0.0625f) {
    this._strength = Math.Max(0f, Math.Min(1f, strength));
    this._pdtGain = Math.Max(0f, Math.Min(0.5f, pdtGain));
  }

  /// <summary>Returns this ditherer with the specified strength.</summary>
  public PatternDependentThresholdDitherer WithStrength(float strength) => new(strength, this._pdtGain);

  /// <summary>Returns this ditherer with the specified PDT gain.</summary>
  public PatternDependentThresholdDitherer WithPdtGain(float pdtGain) => new(this._strength, pdtGain);

  /// <inheritdoc />
  public bool RequiresSequentialProcessing => true;

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
    var strength = this._strength > 0 ? this._strength : 1f;
    var pdtGain = this._pdtGain;
    var endY = startY + height;

    // Luminance buffer of already-written pixels — used for the causal 3x3
    // window lookup. Only two rows are needed at any time (current + prev).
    var lumBuffer = new float[2 * width];
    var prevRow = 0;
    var currRow = 1;

    for (var y = startY; y < endY; ++y) {
      var rowOffset = (y & (_SIZE - 1)) * _SIZE;
      for (int x = 0, sourceIdx = y * sourceStride, targetIdx = y * targetStride; x < width; ++x, ++sourceIdx, ++targetIdx) {
        var color = source[sourceIdx];
        var (c1, c2, c3, alpha) = color.ToNormalized();

        // Source luminance (Rec. 601) as the target local density signal.
        var cf1 = c1.ToFloat();
        var cf2 = c2.ToFloat();
        var cf3 = c3.ToFloat();
        var targetLum = 0.299f * cf1 + 0.587f * cf2 + 0.114f * cf3;

        // Mean luminance of already-quantised causal neighbours (up-left,
        // up, up-right on prev row; left on current row).
        var localSum = 0f;
        var localCount = 0;
        if (y > startY) {
          if (x > 0) { localSum += lumBuffer[prevRow * width + x - 1]; ++localCount; }
          localSum += lumBuffer[prevRow * width + x]; ++localCount;
          if (x + 1 < width) { localSum += lumBuffer[prevRow * width + x + 1]; ++localCount; }
        }
        if (x > 0) { localSum += lumBuffer[currRow * width + x - 1]; ++localCount; }
        var localLum = localCount > 0 ? localSum / localCount : targetLum;

        // PDT correction: if local luminance exceeds target by Δ, push
        // threshold up by pdtGain·Δ so the current pixel is less likely to
        // also be bright. Opposite sign makes darker neighbourhoods nudge
        // current decision brighter.
        var pdtCorrection = (localLum - targetLum) * pdtGain;

        var baseThreshold = _Thresholds[rowOffset + (x & (_SIZE - 1))];
        var threshold = (baseThreshold + pdtCorrection) * strength;

        var adjustedColor = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped(cf1 + threshold),
          UNorm32.FromFloatClamped(cf2 + threshold),
          UNorm32.FromFloatClamped(cf3 + threshold),
          alpha
        );

        var nearestIdx = lookup.FindNearest(adjustedColor, out var nearestColor);
        indices[targetIdx] = (byte)nearestIdx;

        var (n1, n2, n3, _) = nearestColor.ToNormalized();
        lumBuffer[currRow * width + x] = 0.299f * n1.ToFloat() + 0.587f * n2.ToFloat() + 0.114f * n3.ToFloat();
      }

      // Swap rows (ring buffer of length 2).
      (prevRow, currRow) = (currRow, prevRow);
    }
  }

  private static float[] _BuildThresholds() {
    var matrix = BayerMatrix.Generate(_SIZE);
    var flat = new float[_SIZE * _SIZE];

    var min = float.MaxValue;
    var max = float.MinValue;
    for (var y = 0; y < _SIZE; ++y)
    for (var x = 0; x < _SIZE; ++x) {
      var v = matrix[y, x];
      if (v < min) min = v;
      if (v > max) max = v;
    }
    var range = max - min;
    if (!(range > 0))
      return flat;

    for (var y = 0; y < _SIZE; ++y)
    for (var x = 0; x < _SIZE; ++x)
      flat[y * _SIZE + x] = (matrix[y, x] - min) / range - 0.5f;
    return flat;
  }
}
