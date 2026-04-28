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
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Storage;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Dithering;

/// <summary>
/// SSE2-backed inner loop helpers for the Tier A primary threshold ditherers
/// (Ordered, Bayer, BlueNoise, V&amp;C, Threshold50, GoldNoise, IGN). Operates on a
/// pre-decoded <see cref="Bgra8888"/> working buffer (the canonical TWork in the
/// default-quality path of the goldens harness).
/// </summary>
/// <remarks>
/// <para>
/// <b>Approach A (bit-exact).</b> SIMD parallelises only the per-pixel <c>byte → float
/// → +threshold → clamp[0,1]</c> stage; the final <see cref="UNorm32.FromFloatClamped"/>
/// quantisation per channel stays scalar so the post-lookup goldens stay byte-exact.
/// The SIMD float arithmetic is IEEE-754 32-bit identical to the scalar float path,
/// which means the floats fed to <see cref="UNorm32.FromFloatClamped"/> are bit-exact
/// across paths and the resulting bytes match scalar verbatim.
/// </para>
/// <para>
/// <b>Why not byte-domain.</b> Byte-domain SIMD via <c>Sse2.PackUnsignedSaturate</c>
/// after <c>add(int16)</c> can diff from the scalar
/// <c>floor((b/255 + t) · 4294967295.0 / 16777216)</c> by ±1 LSB at boundary values
/// where the float-then-double truncation lands on the integer boundary. While the
/// per-channel byte diff stays within 1 LSB, those 1-LSB shifts can cross an
/// adjacent palette boundary and produce post-lookup diffs of 16-32 LSB on small
/// palettes. The plan's binding rule rejects such diffs ("don't bump tolerance
/// higher to paper over it"), so we keep <see cref="UNorm32.FromFloatClamped"/>
/// scalar to retain bit-exact post-lookup output.
/// </para>
/// <para>
/// <b>Threshold50 special case.</b> Hard 50% per-channel threshold is genuinely
/// byte-domain bit-exact (no float-precision boundary involved — just <c>byte ≥
/// 128 → 255 else 0</c>) so <see cref="ApplyHardThreshold50_4Pixels"/> uses the
/// pure byte-domain SSE2 path with no fallback to scalar.
/// </para>
/// </remarks>
internal static class ThresholdDithererSimd {

#if SUPPORTS_INTRINSICS

  /// <summary>
  /// Loads 4 contiguous Bgra8888 pixels (16 bytes) and converts the BGR channels of each
  /// to <see cref="float"/> via <c>byte * 0x01010101u * (1f / uint.MaxValue)</c> — i.e.
  /// the exact arithmetic that <see cref="UNorm32.FromByte"/> + <see cref="UNorm32.ToFloat"/>
  /// produce. Writes 4 floats per channel to <paramref name="bDst"/>/<paramref name="gDst"/>/<paramref name="rDst"/>
  /// (BGR order) and 4 alpha bytes verbatim to <paramref name="alphaBytes"/>.
  /// </summary>
  /// <remarks>
  /// The float multiplier <c>1f / uint.MaxValue</c> is used by <see cref="UNorm32.ToFloat"/>;
  /// performing the same multiplication in the SIMD lane reproduces the scalar value bit-exactly,
  /// since SSE2 single-precision arithmetic is IEEE-754 32-bit and the JIT does not promote to
  /// x87 80-bit on Sse2 codegen paths.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void DecodeBgra4Pixels(
    byte* srcBgra,
    float* bDst, float* gDst, float* rDst,
    byte* alphaBytes) {
    // Read 4 bytes per channel directly. Could be SIMD'd via shuffles but the codegen
    // for explicit per-byte loads is already efficient — the heavy lifting is the
    // multiply-by-0x01010101 and the float convert below.
    for (var i = 0; i < 4; ++i) {
      var b = srcBgra[i * 4 + 0];
      var g = srcBgra[i * 4 + 1];
      var r = srcBgra[i * 4 + 2];
      var a = srcBgra[i * 4 + 3];
      // Replicate UNorm32.FromByte(b).ToFloat() exactly: (uint)b * 0x01010101u * (1f/uint.MaxValue).
      // Cast through (float) of the uint product so the implicit conversion matches the scalar
      // path's float-promotion of UNorm32._value (a uint).
      bDst[i] = (float)((uint)b * 0x01010101u) * _UnormToFloat;
      gDst[i] = (float)((uint)g * 0x01010101u) * _UnormToFloat;
      rDst[i] = (float)((uint)r * 0x01010101u) * _UnormToFloat;
      alphaBytes[i] = a;
    }
  }

  private const float _UnormToFloat = 1f / uint.MaxValue;

  /// <summary>
  /// Adds 4 broadcast threshold values (one per pixel) to the BGR channel arrays in place
  /// and clamps to [0, 1] via SSE2 min/max. The SIMD parallelism is across pixels, not
  /// across channels — channels are kept in separate <c>float[4]</c> buffers so each
  /// <see cref="Vector128{Single}"/> holds 4 pixels of a single channel.
  /// </summary>
  /// <param name="b4">Pointer to 4 floats (B channel of pixels 0..3).</param>
  /// <param name="g4">Pointer to 4 floats (G channel of pixels 0..3).</param>
  /// <param name="r4">Pointer to 4 floats (R channel of pixels 0..3).</param>
  /// <param name="thresholds4">Pointer to 4 floats (threshold per pixel).</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void AddThresholdAndClamp_4Pixels(
    float* b4, float* g4, float* r4, float* thresholds4) {
    var thresh = Sse.LoadVector128(thresholds4);
    var zero = Vector128<float>.Zero;
    var one = Vector128.Create(1f);

    var bv = Sse.LoadVector128(b4);
    bv = Sse.Add(bv, thresh);
    bv = Sse.Min(Sse.Max(bv, zero), one);
    Sse.Store(b4, bv);

    var gv = Sse.LoadVector128(g4);
    gv = Sse.Add(gv, thresh);
    gv = Sse.Min(Sse.Max(gv, zero), one);
    Sse.Store(g4, gv);

    var rv = Sse.LoadVector128(r4);
    rv = Sse.Add(rv, thresh);
    rv = Sse.Min(Sse.Max(rv, zero), one);
    Sse.Store(r4, rv);
  }

  /// <summary>
  /// Applies the 50% per-channel hard threshold to 4 pixels in a single 16-byte SSE2 op.
  /// Bytes &lt; 128 → 0, bytes ≥ 128 → 255, alpha lanes preserved verbatim.
  /// </summary>
  /// <param name="srcBgra">Pointer to 4 contiguous Bgra8888 pixels (16 bytes).</param>
  /// <param name="dstBgra">Pointer to where the thresholded 4 pixels are written.</param>
  /// <remarks>
  /// Bit-exactly equivalent to the scalar
  /// <c>UNorm32.FromByte(b).ToFloat() &lt; 0.5f ? 0f : 1f</c> path: the float threshold
  /// at 0.5 is identical (in IEEE-754 binary) to the byte threshold at 128 because
  /// <c>(uint)128 * 0x01010101 / uint.MaxValue ≈ 0.50196078...</c> ≥ 0.5 and
  /// <c>(uint)127 * 0x01010101 / uint.MaxValue ≈ 0.49803921...</c> &lt; 0.5. So the
  /// <c>byte ≥ 128</c> SIMD predicate is exact, no rounding boundary involved.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void ApplyHardThreshold50_4Pixels(byte* srcBgra, byte* dstBgra) {
    var pixVec = Sse2.LoadVector128(srcBgra);

    // For each unsigned byte b, we want 0 if b<128 else 255, except alpha lanes (every
    // 4th byte) which must be preserved. Trick: SubtractSaturate(b, 128) is 0 if b<128,
    // (b-128) if b>=128 — a positive value. CompareGreaterThan against signed zero on
    // .AsSByte() yields 0xFF where positive, 0 elsewhere (anything ≥ 1 is > 0 in signed).
    var sub = Sse2.SubtractSaturate(pixVec, Vector128.Create((byte)128));
    var ge128 = Sse2.CompareGreaterThan(sub.AsSByte(), Vector128<sbyte>.Zero).AsByte();

    // Construct an alpha mask (every 4th byte = 0xFF, others = 0x00).
    var alphaMask = Vector128.Create(
      (byte)0x00, 0x00, 0x00, 0xFF,
      0x00, 0x00, 0x00, 0xFF,
      0x00, 0x00, 0x00, 0xFF,
      0x00, 0x00, 0x00, 0xFF);

    // Where alphaMask is set: keep original byte; otherwise: take the thresholded value.
    var keepAlpha = Sse2.And(pixVec, alphaMask);
    var keepThresholded = Sse2.AndNot(alphaMask, ge128);
    var result = Sse2.Or(keepAlpha, keepThresholded);

    Sse2.Store(dstBgra, result);
  }

#endif

}
