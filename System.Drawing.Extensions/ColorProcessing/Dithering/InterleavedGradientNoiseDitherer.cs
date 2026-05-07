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
#if SUPPORTS_INTRINSICS
using System.Runtime.Intrinsics.X86;
#endif
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Storage;
using MethodImplOptions = Utilities.MethodImplOptions;

using Hawkynt.ColorProcessing.ColorMath;

namespace Hawkynt.ColorProcessing.Dithering;

/// <summary>
/// Interleaved Gradient Noise (IGN) dithering algorithm.
/// </summary>
/// <remarks>
/// <para>
/// IGN is a pseudo-random noise function commonly used in modern graphics pipelines,
/// particularly for temporal anti-aliasing and screen-space effects. The algorithm
/// generates a deterministic noise pattern based on pixel coordinates using the formula:
/// IGN(x,y) = frac(52.9829189 * frac(0.06711056*x + 0.00583715*y))
/// </para>
/// <para>
/// This dithering technique is especially useful for:
/// - Real-time graphics and temporal anti-aliasing
/// - Screen-space dithering with good spatial distribution
/// - Reducing banding in gradients with minimal pattern visibility
/// </para>
/// </remarks>
[Ditherer("Interleaved Gradient Noise", Description = "Deterministic noise for smooth dithering", Type = DitheringType.Noise)]
public readonly struct InterleavedGradientNoiseDitherer : IDitherer {

  private readonly float _intensity;

  /// <summary>Pre-configured instance with standard intensity (0.5).</summary>
  public static InterleavedGradientNoiseDitherer Instance { get; } = new(0.5f);

  /// <summary>Pre-configured instance with light intensity (0.3).</summary>
  public static InterleavedGradientNoiseDitherer Light { get; } = new(0.3f);

  /// <summary>Pre-configured instance with strong intensity (0.7).</summary>
  public static InterleavedGradientNoiseDitherer Strong { get; } = new(0.7f);

  /// <summary>
  /// Creates an IGN ditherer with the specified intensity.
  /// </summary>
  /// <param name="intensity">Noise intensity (0-1).</param>
  public InterleavedGradientNoiseDitherer(float intensity = 0.5f) {
    this._intensity = ColorConverter.Saturate(intensity);
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
    var intensity = this._intensity;
    var endY = startY + height;
#if SUPPORTS_INTRINSICS
    // float-domain SIMD across 4 pixels of B/G/R add+clamp; scalar quantize.
    // Eligibility check is loop-invariant — hoisted out of the y-loop so the JIT sees
    // two distinct hot loops, and so legacy TFMs (net35/40/45/48) don't pay a per-row
    // Sse2.IsSupported field load.
    var simdEligible = Sse2.IsSupported && typeof(TWork) == typeof(Bgra8888) && width >= 4;
    if (simdEligible) {
      var simdEnd = width & ~3;
      var bChannels = stackalloc float[4];
      var gChannels = stackalloc float[4];
      var rChannels = stackalloc float[4];
      var alphaBytes = stackalloc byte[4];
      var thresholds4 = stackalloc float[4];

      for (var y = startY; y < endY; ++y) {
        var rowSource = source + y * sourceStride;
        var x = 0;
        var targetIdx = y * targetStride;

        var srcBase = (byte*)rowSource;
        for (; x < simdEnd; x += 4) {
          ThresholdDithererSimd.DecodeBgra4Pixels(srcBase + x * 4, bChannels, gChannels, rChannels, alphaBytes);

          // Compute 4 per-pixel IGN thresholds (double-precision frac, scalar).
          for (var lane = 0; lane < 4; ++lane) {
            var noiseValue = (float)_ComputeIGN(x + lane, y);
            thresholds4[lane] = noiseValue * intensity;
          }

          ThresholdDithererSimd.AddThresholdAndClamp_4Pixels(bChannels, gChannels, rChannels, thresholds4);

          for (var lane = 0; lane < 4; ++lane) {
            // Bgra8888 component convention: (C1, C2, C3, A) = (R, G, B, A).
            var adjustedColor = ColorFactory.FromNormalized_4<TWork>(
              UNorm32.FromFloatClamped(rChannels[lane]),
              UNorm32.FromFloatClamped(gChannels[lane]),
              UNorm32.FromFloatClamped(bChannels[lane]),
              UNorm32.FromByte(alphaBytes[lane])
            );
            indices[targetIdx + x + lane] = (byte)lookup.FindNearest(adjustedColor);
          }
        }
        targetIdx += x;

        // Tail: width-mod-4 leftover lanes.
        for (; x < width; ++x, ++targetIdx) {
          var pixel = rowSource[x];
          var (c1, c2, c3, alpha) = pixel.ToNormalized();
          var pixelC1 = c1.ToFloat();
          var pixelC2 = c2.ToFloat();
          var pixelC3 = c3.ToFloat();
          var pixelA = alpha.ToFloat();

          var noiseValue = (float)_ComputeIGN(x, y);
          var threshold = noiseValue * intensity;

          var adjustedC1 = ColorConverter.Saturate(pixelC1 + threshold);
          var adjustedC2 = ColorConverter.Saturate(pixelC2 + threshold);
          var adjustedC3 = ColorConverter.Saturate(pixelC3 + threshold);

          var adjustedColor = ColorFactory.FromNormalized_4<TWork>(
            UNorm32.FromFloatClamped(adjustedC1),
            UNorm32.FromFloatClamped(adjustedC2),
            UNorm32.FromFloatClamped(adjustedC3),
            UNorm32.FromFloatClamped(pixelA)
          );

          indices[targetIdx] = (byte)lookup.FindNearest(adjustedColor);
        }
      }
    } else
#endif
    {
      for (var y = startY; y < endY; ++y) {
        var rowSource = source + y * sourceStride;
        var targetIdx = y * targetStride;

        for (var x = 0; x < width; ++x, ++targetIdx) {
          // Read pre-decoded source pixel from the working buffer.
          var pixel = rowSource[x];
          var (c1, c2, c3, alpha) = pixel.ToNormalized();
          var pixelC1 = c1.ToFloat();
          var pixelC2 = c2.ToFloat();
          var pixelC3 = c3.ToFloat();
          var pixelA = alpha.ToFloat();

          // Compute IGN value (-1 to 1) and scale by intensity
          var noiseValue = (float)_ComputeIGN(x, y);
          var threshold = noiseValue * intensity;

          // Apply noise to each channel
          var adjustedC1 = ColorConverter.Saturate(pixelC1 + threshold);
          var adjustedC2 = ColorConverter.Saturate(pixelC2 + threshold);
          var adjustedC3 = ColorConverter.Saturate(pixelC3 + threshold);

          // Create adjusted color
          var adjustedColor = ColorFactory.FromNormalized_4<TWork>(
            UNorm32.FromFloatClamped(adjustedC1),
            UNorm32.FromFloatClamped(adjustedC2),
            UNorm32.FromFloatClamped(adjustedC3),
            UNorm32.FromFloatClamped(pixelA)
          );

          // Find nearest palette color
          indices[targetIdx] = (byte)lookup.FindNearest(adjustedColor);
        }
      }
    }
  }

  /// <summary>
  /// Computes the Interleaved Gradient Noise value for the given coordinates.
  /// Formula: IGN(x,y) = frac(52.9829189 * frac(0.06711056*x + 0.00583715*y))
  /// </summary>
  /// <param name="x">The x coordinate.</param>
  /// <param name="y">The y coordinate.</param>
  /// <returns>A noise value in the range [-1, 1].</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static double _ComputeIGN(int x, int y) {
    var frac1 = (0.06711056 * x + 0.00583715 * y) % 1.0;
    var frac2 = (52.9829189 * frac1) % 1.0;
    return frac2 * 2.0 - 1.0;
  }
}
