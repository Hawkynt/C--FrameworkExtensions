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

using System.Runtime.CompilerServices;
#if SUPPORTS_INTRINSICS
using System.Runtime.Intrinsics.X86;
#endif
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Storage;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Dithering;

/// <summary>
/// Hard 50% threshold "line-art" dithering — snaps every channel to 0 or 1 before
/// palette lookup, then maps the resulting corner of the unit cube to the nearest
/// palette colour.
/// </summary>
/// <remarks>
/// <para>
/// This is the classic 1-bit / line-art operator: if the channel value is below
/// 0.5 it becomes 0, otherwise 1. For an RGB image this collapses every pixel to
/// one of the 8 corners of the RGB cube (white, black, primaries, secondaries)
/// which is then snapped to the nearest entry in the supplied palette.
/// </para>
/// <para>
/// Unlike <see cref="NoDithering"/> (which feeds the raw colour to the palette
/// matcher) this ditherer *pre-quantises* to the corners of the cube, producing
/// the crisp look expected for scanned line-art, text, ink drawings and faxable
/// documents. Unlike ordered or error-diffusion ditherers no spatial pattern is
/// introduced — mid-tones are resolved by clean hard edges rather than shading.
/// </para>
/// <para>
/// Reference: ITU-T T.4 / T.6 (Group 3/Group 4 fax) facsimile encoding uses a
/// 50% luminance threshold as its canonical pre-palette operator. See also
/// <a href="https://en.wikipedia.org/wiki/Thresholding_(image_processing)">
/// Thresholding (image processing)</a>.
/// </para>
/// <para>
/// Parallel-friendly (per-pixel operation, no state). Deterministic.
/// </para>
/// </remarks>
[Ditherer("Threshold 50%", Description = "Hard 50% per-channel threshold for line-art / 1-bit output", Type = DitheringType.None)]
public readonly struct Threshold50Ditherer : IDitherer {

  /// <summary>Default instance.</summary>
  public static Threshold50Ditherer Instance { get; } = new();

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

#if SUPPORTS_INTRINSICS
    // byte-domain SIMD shortcut for the common Bgra8888 TWork. Stackalloc'd once
    // (outside the y/x loops) to avoid CA2014 (potential stack overflow on repeated allocs).
    // Eligibility check is loop-invariant — hoisted out of the y-loop so the JIT sees
    // two distinct hot loops, and so legacy TFMs (net35/40/45/48) don't pay a per-row
    // Sse2.IsSupported field load.
    var simdEligible = Sse2.IsSupported && typeof(TWork) == typeof(Bgra8888) && width >= 4;
    if (simdEligible) {
      var simdEnd = width & ~3;
      var thresholdedQuad = stackalloc byte[16];

      // 50% per-channel threshold collapses to "byte ≥ 128 → 255, else 0" which is
      // bit-exact with the scalar UNorm32.FromFloatClamped path (no float rounding
      // boundary involved). Alpha lanes preserved verbatim. Goldens stay byte-exact.
      for (var y = startY; y < endY; ++y) {
        var rowSource = source + y * sourceStride;
        var x = 0;
        var targetIdx = y * targetStride;

        var srcBase = (byte*)rowSource;
        for (; x < simdEnd; x += 4) {
          var srcQuad = srcBase + x * 4;
          ThresholdDithererSimd.ApplyHardThreshold50_4Pixels(srcQuad, thresholdedQuad);
          for (var lane = 0; lane < 4; ++lane) {
            var px = ((Bgra8888*)thresholdedQuad)[lane];
            indices[targetIdx + x + lane] = (byte)lookup.FindNearest(Unsafe.As<Bgra8888, TWork>(ref px));
          }
        }
        targetIdx += x;

        // Tail: width-mod-4 leftover lanes.
        for (; x < width; ++x, ++targetIdx) {
          var color = rowSource[x];
          var (c1, c2, c3, alpha) = color.ToNormalized();

          var t1 = c1.ToFloat() < 0.5f ? 0f : 1f;
          var t2 = c2.ToFloat() < 0.5f ? 0f : 1f;
          var t3 = c3.ToFloat() < 0.5f ? 0f : 1f;

          var thresholded = ColorFactory.FromNormalized_4<TWork>(
            UNorm32.FromFloatClamped(t1),
            UNorm32.FromFloatClamped(t2),
            UNorm32.FromFloatClamped(t3),
            alpha
          );

          indices[targetIdx] = (byte)lookup.FindNearest(thresholded);
        }
      }
    } else
#endif
    {
      for (var y = startY; y < endY; ++y) {
        var rowSource = source + y * sourceStride;
        var targetIdx = y * targetStride;

        for (var x = 0; x < width; ++x, ++targetIdx) {
          var color = rowSource[x];
          var (c1, c2, c3, alpha) = color.ToNormalized();

          // Hard 50% threshold per channel (alpha is preserved unchanged so
          // transparent-friendly palettes still work as expected).
          var t1 = c1.ToFloat() < 0.5f ? 0f : 1f;
          var t2 = c2.ToFloat() < 0.5f ? 0f : 1f;
          var t3 = c3.ToFloat() < 0.5f ? 0f : 1f;

          var thresholded = ColorFactory.FromNormalized_4<TWork>(
            UNorm32.FromFloatClamped(t1),
            UNorm32.FromFloatClamped(t2),
            UNorm32.FromFloatClamped(t3),
            alpha
          );

          indices[targetIdx] = (byte)lookup.FindNearest(thresholded);
        }
      }
    }
  }
}
