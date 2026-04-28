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
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Metrics;

#region 3-Component Float

/// <summary>
/// Euclidean (L2) distance metric for 3-component color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3F.</typeparam>
/// <remarks>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
/// <para>Maximum raw distance: sqrt(3) ≈ 1.732.</para>
/// </remarks>
public readonly struct Euclidean3F<TKey> : IColorMetric<TKey>, INormalizedMetric
  where TKey : unmanaged, IColorSpace3F<TKey> {

  private const float InverseMaxDistance = 1f / 1.7320508075688772935f; // 1/sqrt(3)

  /// <summary>
  /// Calculates the Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b) {
    var raw = MathF.Sqrt(EuclideanSquared3F<TKey>._Calculate(a, b));
    return UNorm32.FromFloatClamped(raw * InverseMaxDistance);
  }
}

/// <summary>
/// Squared Euclidean distance metric for 3-component color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3F.</typeparam>
/// <remarks>
/// <para>Faster than Euclidean (no sqrt) when only relative comparison is needed.
/// Use for nearest-neighbor search where absolute distance isn't required.</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
/// <para>Maximum raw distance: 3.0.</para>
/// </remarks>
public readonly struct EuclideanSquared3F<TKey> : IColorMetric<TKey>, INormalizedMetric
  where TKey : unmanaged, IColorSpace3F<TKey> {

  private const float InverseMaxDistance = 1f / 3f; // 1/(3 × 1.0²)

  /// <summary>
  /// Internal squared distance calculation.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float _Calculate(in TKey a, in TKey b) {
    var d1 = a.C1 - b.C1;
    var d2 = a.C2 - b.C2;
    var d3 = a.C3 - b.C3;
    return d1 * d1 + d2 * d2 + d3 * d3;
  }

  /// <summary>
  /// Calculates the squared Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b)
    => UNorm32.FromFloatClamped(_Calculate(a, b) * InverseMaxDistance);
}

#endregion

#region 3-Component Byte

/// <summary>
/// Euclidean (L2) distance metric for 3-component byte color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3B.</typeparam>
/// <remarks>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
/// <para>Maximum raw distance: sqrt(195075) ≈ 441.67.</para>
/// </remarks>
public readonly struct Euclidean3B<TKey> : IColorMetric<TKey>, INormalizedMetric
  where TKey : unmanaged, IColorSpace3B<TKey> {

  private const float InverseMaxDistance = 1f / 441.6729559300637f; // 1/sqrt(3 × 255²)

  /// <summary>
  /// Calculates the Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b) {
    var raw = MathF.Sqrt(EuclideanSquared3B<TKey>._Calculate(a, b));
    return UNorm32.FromFloatClamped(raw * InverseMaxDistance);
  }
}

/// <summary>
/// Squared Euclidean distance metric for 3-component byte color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace3B.</typeparam>
/// <remarks>
/// <para>Faster than Euclidean (no sqrt) when only relative comparison is needed.
/// Use for nearest-neighbor search where absolute distance isn't required.</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
/// <para>Maximum raw distance: 3 × 255² = 195,075.</para>
/// </remarks>
public readonly struct EuclideanSquared3B<TKey> : IColorMetric<TKey>, INormalizedMetric
  where TKey : unmanaged, IColorSpace3B<TKey> {

  private const int MaxDistance = 195075; // 3 × 255²

  /// <summary>
  /// Internal squared distance calculation.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static int _Calculate(in TKey a, in TKey b) {
    var d1 = a.C1 - b.C1;
    var d2 = a.C2 - b.C2;
    var d3 = a.C3 - b.C3;
    return d1 * d1 + d2 * d2 + d3 * d3;
  }

  /// <summary>
  /// Calculates the squared Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b)
    => UNorm32.FromRaw((uint)((ulong)_Calculate(a, b) * uint.MaxValue / MaxDistance));
}

#endregion

#region 4-Component Float

/// <summary>
/// Euclidean (L2) distance metric for 4-component float color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4F.</typeparam>
/// <remarks>
/// <para>Calculates the geometric distance across all four components including alpha.
/// More accurate than Manhattan for perceptual comparisons.</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
/// <para>Maximum raw distance: sqrt(4) = 2.0.</para>
/// </remarks>
public readonly struct Euclidean4F<TKey> : IColorMetric<TKey>, INormalizedMetric
  where TKey : unmanaged, IColorSpace4F<TKey> {

  private const float InverseMaxDistance = 0.5f; // 1/sqrt(4)

  /// <summary>
  /// Calculates the Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b) {
    var raw = MathF.Sqrt(EuclideanSquared4F<TKey>._Calculate(a, b));
    return UNorm32.FromFloatClamped(raw * InverseMaxDistance);
  }
}

/// <summary>
/// Squared Euclidean distance metric for 4-component float color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4F.</typeparam>
/// <remarks>
/// <para>Omits the square root for faster comparisons when only relative distances matter.</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
/// <para>Maximum raw distance: 4.0.</para>
/// <para><b>Batch-distance opt-in (<see cref="IBatchDistance{TKey}"/>).</b>
/// Mirrors the byte-domain <see cref="EuclideanSquared4B{TKey}"/> opt-in for
/// the high-quality (LinearRgbaF / OklabaF) K-means and BisectingKMeans paths.
/// The candidate buffer holds <c>sizeof(TKey)</c>-sized colour records back-to-
/// back; the SIMD kernel reads each candidate as a <see cref="Vector128{Single}"/>
/// (AoS), subtracts the broadcast reference, squares, and horizontal-adds to one
/// scalar squared distance per candidate. The integer return value is the
/// IEEE-754 bit pattern of the float — monotonic for non-negative finite floats,
/// so the cross-call ordering invariant from <see cref="IBatchDistance{TKey}"/>
/// is preserved without needing a separate float-distance API. Argmin over
/// raw squared distance and over <c>sqrt(squared)</c> agree because sqrt is
/// monotonic on [0, +∞), so this metric's nearest-index answer matches every
/// scalar Euclidean-style consumer.</para>
/// </remarks>
public readonly struct EuclideanSquared4F<TKey> : IColorMetric<TKey>, INormalizedMetric, IBatchDistance<TKey>
  where TKey : unmanaged, IColorSpace4F<TKey> {

  private const float InverseMaxDistance = 0.25f; // 1/(4 × 1.0²)

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float _Calculate(in TKey a, in TKey b) {
    var d1 = a.C1 - b.C1;
    var d2 = a.C2 - b.C2;
    var d3 = a.C3 - b.C3;
    var da = a.A - b.A;
    return d1 * d1 + d2 * d2 + d3 * d3 + da * da;
  }

  /// <summary>
  /// Calculates the squared Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b)
    => UNorm32.FromFloatClamped(_Calculate(a, b) * InverseMaxDistance);

  /// <summary>
  /// Reinterprets the IEEE-754 bit pattern of a non-negative finite float as a
  /// non-negative int. Monotonic on [0, +∞): if 0 ≤ a ≤ b then bits(a) ≤ bits(b),
  /// so int comparison reproduces the float ordering. Used so the IBatchDistance
  /// API's int-typed distance can carry a raw squared-float distance without a
  /// separate float-flavoured interface.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _FloatBitsAsInt(float v) {
#if SUPPORTS_BITCONVERTER_UINT_CONVERSION
    return BitConverter.SingleToInt32Bits(v);
#else
    return Unsafe.As<float, int>(ref v);
#endif
  }

  /// <summary>
  /// Batch-distance implementation: scans a packed candidate buffer (each entry =
  /// <c>sizeof(TKey)</c> bytes, layout-equivalent to a <typeparamref name="TKey"/>
  /// value) and returns the smallest raw squared distance plus its index.
  /// </summary>
  /// <remarks>
  /// <para><b>SSE path.</b> Each candidate is loaded directly as a
  /// <see cref="Vector128{Single}"/> via an unaligned load; the reference is loaded
  /// once per call and reused. <c>(ref - cand)</c>, <c>mul</c>, then a
  /// horizontal-add of the four lanes yields one scalar squared distance per
  /// candidate. The horizontal-add path uses two shuffle+adds on plain SSE so
  /// every supported TFM with <c>System.Runtime.Intrinsics</c> takes the SIMD
  /// branch; SSE3 would have a one-instruction <c>haddps</c> but is intentionally
  /// not required.</para>
  /// <para>Tie-break: first-occurrence on equal distance, matching the scalar
  /// fallback contract.</para>
  /// </remarks>
  public int FindMinDistance(in TKey reference, ReadOnlySpan<byte> candidates, int count, out int minIndex) {
    var bestIdx = 0;
    var bestBits = int.MaxValue;

    unsafe {
      var elementSize = sizeof(TKey);
      fixed (byte* pb = candidates) {
        fixed (TKey* prefBase = &Unsafe.AsRef(in reference)) {
#if SUPPORTS_INTRINSICS
          if (Sse.IsSupported) {
            var refVec = Sse.LoadVector128((float*)prefBase);
            for (var i = 0; i < count; ++i) {
              var candVec = Sse.LoadVector128((float*)(pb + i * elementSize));
              var diff = Sse.Subtract(refVec, candVec);
              var sq = Sse.Multiply(diff, diff);
              // Horizontal-add of 4 lanes: shuffle high pair to low, add → low
              // 2 lanes contain (a+c, b+d); shuffle lane1 to lane0, add → lane0
              // contains (a+c+b+d). Avoiding SSE3 keeps the kernel on plain SSE.
              var shuf1 = Sse.Shuffle(sq, sq, 0b_00_00_11_10); // [c, d, _, _]
              var pair = Sse.Add(sq, shuf1);                    // lane0: a+c, lane1: b+d
              var shuf2 = Sse.Shuffle(pair, pair, 0b_00_00_00_01); // lane0: b+d
              var sum = Sse.AddScalar(pair, shuf2);             // lane0: a+b+c+d
              var d = sum.GetElement(0);
              var dBits = _FloatBitsAsInt(d);
              if (dBits < bestBits) {
                bestBits = dBits;
                bestIdx = i;
                if (dBits == 0) {
                  minIndex = bestIdx;
                  return 0;
                }
              }
            }
            minIndex = bestIdx;
            return bestBits == int.MaxValue ? 0 : bestBits;
          }
#endif

          // Scalar fallback. Identical sum order to the SIMD kernel modulo IEEE
          // associativity — the kernel does (a+c)+(b+d) lane-pair-then-cross,
          // while the scalar loop does ((a+b)+c)+d. Squared-distance values are
          // non-negative so both orders are well-conditioned, and only argmin
          // (an inequality) escapes this method, so any ±1 ULP wobble is
          // absorbed by the consumer's "is dist strictly less" comparison.
          var refLocal = *prefBase;
          for (var i = 0; i < count; ++i) {
            var c = *(TKey*)(pb + i * elementSize);
            var d = _Calculate(refLocal, c);
            var dBits = _FloatBitsAsInt(d);
            if (dBits < bestBits) {
              bestBits = dBits;
              bestIdx = i;
              if (dBits == 0)
                break;
            }
          }
        }
      }
    }

    minIndex = bestIdx;
    return bestBits == int.MaxValue ? 0 : bestBits;
  }

  /// <summary>
  /// Batch nearest-of-N for multiple reference colours. The candidate buffer is
  /// reused across all references (palette-pattern: every reference scans the
  /// same N candidates), so the inner candidate loop walks the buffer once per
  /// reference and the SIMD per-reference kernel pays the load cost once.
  /// </summary>
  /// <remarks>
  /// <para><b>SSE path.</b> 4 references at a time × 4 candidates at a time. Each
  /// inner iteration loads 4 candidate vectors and computes 4 squared distances
  /// per reference (<c>diff·diff</c> + horizontal-add per pair), updating per-
  /// reference best-distance and best-index in scalar registers so the first-
  /// occurrence tie-break stays bit-exact with <see cref="FindMinDistance"/>.</para>
  /// <para>The unaligned-load pattern (<c>Sse.LoadVector128</c>) means the buffer
  /// only needs to be naturally aligned to <c>sizeof(float) = 4</c>, which a
  /// managed <c>byte[]</c> satisfies trivially.</para>
  /// </remarks>
  public void FindMinDistanceBatch(
    ReadOnlySpan<TKey> references,
    ReadOnlySpan<byte> candidates, int candidateCount,
    Span<int> outIndices) {

    var refCount = references.Length;
    if (refCount <= 0 || candidateCount <= 0)
      return;

    unsafe {
      var elementSize = sizeof(TKey);
#if SUPPORTS_INTRINSICS
      if (Sse.IsSupported && refCount >= 4 && candidateCount >= 4) {
        var refTail = refCount & ~3;
        fixed (byte* pb = candidates) {
          fixed (TKey* prefs = &Unsafe.AsRef(in references[0])) {
            for (var r = 0; r < refTail; r += 4) {
              var v0 = Sse.LoadVector128((float*)(prefs + r + 0));
              var v1 = Sse.LoadVector128((float*)(prefs + r + 1));
              var v2 = Sse.LoadVector128((float*)(prefs + r + 2));
              var v3 = Sse.LoadVector128((float*)(prefs + r + 3));

              var b0 = int.MaxValue;
              var b1 = int.MaxValue;
              var b2 = int.MaxValue;
              var b3 = int.MaxValue;
              var i0 = 0;
              var i1 = 0;
              var i2 = 0;
              var i3 = 0;

              for (var c = 0; c < candidateCount; ++c) {
                var cv = Sse.LoadVector128((float*)(pb + c * elementSize));

                var d0 = _SquaredDistance(v0, cv);
                var d1 = _SquaredDistance(v1, cv);
                var d2 = _SquaredDistance(v2, cv);
                var d3 = _SquaredDistance(v3, cv);

                var k0 = _FloatBitsAsInt(d0);
                var k1 = _FloatBitsAsInt(d1);
                var k2 = _FloatBitsAsInt(d2);
                var k3 = _FloatBitsAsInt(d3);

                if (k0 < b0) { b0 = k0; i0 = c; }
                if (k1 < b1) { b1 = k1; i1 = c; }
                if (k2 < b2) { b2 = k2; i2 = c; }
                if (k3 < b3) { b3 = k3; i3 = c; }
              }

              outIndices[r + 0] = i0;
              outIndices[r + 1] = i1;
              outIndices[r + 2] = i2;
              outIndices[r + 3] = i3;
            }
          }
        }

        // Tail references (<4): fall through to scalar per-reference path below.
        for (var r = refTail; r < refCount; ++r) {
          this.FindMinDistance(references[r], candidates, candidateCount, out var idx);
          outIndices[r] = idx;
        }
        return;
      }
#endif

      // Scalar path: per-reference forwarding.
      for (var r = 0; r < refCount; ++r) {
        this.FindMinDistance(references[r], candidates, candidateCount, out var idx);
        outIndices[r] = idx;
      }
    }
  }

  /// <summary>
  /// Top-k closest-candidates scan. Forwards to the generic scalar fallback —
  /// the SIMD kernel above is single-min-only; FindNClosest's ditherer consumers
  /// (NClosest / Barycentric / NaturalNeighbour) have per-pixel cost dominated
  /// by other work so the scalar partial-selection is the right shape.
  /// </summary>
  public int FindNClosest(
    in TKey reference,
    ReadOnlySpan<byte> candidates, int candidateCount,
    int k,
    Span<int> outIndices, Span<int> outDistances)
    => BatchDistanceDefaults.FindNClosestScalar<TKey, EuclideanSquared4F<TKey>>(
      in this, reference, candidates, candidateCount, k, outIndices, outDistances);

#if SUPPORTS_INTRINSICS
  /// <summary>
  /// Computes the scalar squared distance between a reference vector and a
  /// candidate vector, both packed AoS (4 floats: c1, c2, c3, a). The SIMD kernel
  /// reuses this helper so every distance flows through the same horizontal-add
  /// shape.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _SquaredDistance(Vector128<float> reference, Vector128<float> candidate) {
    var diff = Sse.Subtract(reference, candidate);
    var sq = Sse.Multiply(diff, diff);
    var shuf1 = Sse.Shuffle(sq, sq, 0b_00_00_11_10);
    var pair = Sse.Add(sq, shuf1);
    var shuf2 = Sse.Shuffle(pair, pair, 0b_00_00_00_01);
    var sum = Sse.AddScalar(pair, shuf2);
    return sum.GetElement(0);
  }
#endif
}

#endregion

#region 4-Component Byte

/// <summary>
/// Euclidean (L2) distance metric for 4-component byte color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4B.</typeparam>
/// <remarks>
/// <para>Calculates the geometric distance across all four components including alpha.
/// More accurate than Manhattan for perceptual comparisons.</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
/// <para>Maximum raw distance: sqrt(260100) ≈ 510.</para>
/// </remarks>
public readonly struct Euclidean4B<TKey> : IColorMetric<TKey>, INormalizedMetric
  where TKey : unmanaged, IColorSpace4B<TKey> {

  private const float InverseMaxDistance = 1f / 510f; // 1/sqrt(4 × 255²)

  /// <summary>
  /// Calculates the Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b) {
    var raw = MathF.Sqrt(EuclideanSquared4B<TKey>._Calculate(a, b));
    return UNorm32.FromFloatClamped(raw * InverseMaxDistance);
  }
}

/// <summary>
/// Squared Euclidean distance metric for 4-component byte color spaces.
/// </summary>
/// <typeparam name="TKey">The key color type implementing IColorSpace4B.</typeparam>
/// <remarks>
/// <para>Omits the square root for faster comparisons when only relative distances matter.</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
/// <para>Maximum raw distance: 4 × 255² = 260,100.</para>
/// <para><b>Batch-distance opt-in (<see cref="IBatchDistance{TKey}"/>).</b>
/// This metric implements the batch API so nearest-neighbour consumers
/// (most notably <see cref="PaletteLookup{TWork,TMetric}"/>) can delegate the
/// inner "find closest of N" step to <see cref="FindMinDistance"/> and get a
/// four-at-a-time SSE2 kernel (+ <c>PMADDWD</c> and <c>SSSE3.HorizontalAdd</c>
/// where available) for free. The SIMD kernel is bit-exactly equivalent to the
/// scalar <see cref="Distance"/> path — squared byte differences are
/// order-independent so the ordering of the 4 bytes in the packed
/// <typeparamref name="TKey"/> doesn't matter. The SIMD path is skipped at
/// runtime when <c>Sse2.IsSupported</c> is false; the scalar tail inside
/// <see cref="FindMinDistance"/> handles the residual 0-3 candidates and is
/// also the complete implementation on TFMs without
/// <c>System.Runtime.Intrinsics</c> (net35/40/45/48/netcoreapp3.0−).</para>
/// </remarks>
public readonly struct EuclideanSquared4B<TKey> : IColorMetric<TKey>, INormalizedMetric, IBatchDistance<TKey>
  where TKey : unmanaged, IColorSpace4B<TKey> {

  private const int MaxDistance = 260100; // 4 × 255²

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static int _Calculate(in TKey a, in TKey b) {
    var d1 = a.C1 - b.C1;
    var d2 = a.C2 - b.C2;
    var d3 = a.C3 - b.C3;
    var da = a.A - b.A;
    return d1 * d1 + d2 * d2 + d3 * d3 + da * da;
  }

  /// <summary>
  /// Calculates the squared Euclidean distance between two colors.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in TKey a, in TKey b)
    => UNorm32.FromRaw((uint)((ulong)_Calculate(a, b) * uint.MaxValue / MaxDistance));

  /// <summary>
  /// Batch-distance implementation: scans a packed 4-byte-per-candidate buffer and
  /// returns the smallest raw squared distance plus its index.
  /// </summary>
  /// <remarks>
  /// <para><b>Packing.</b> Each candidate occupies 4 bytes in
  /// <paramref name="candidates"/> in the same in-memory component order as the
  /// underlying <typeparamref name="TKey"/> struct; little-endian reads as one
  /// <see cref="uint"/> per candidate. The reference colour is read via
  /// <see cref="Unsafe.As{TFrom,TTo}(ref TFrom)"/> from <paramref name="reference"/>
  /// in the same order. Because squared byte differences are commutative the
  /// ordering is irrelevant to the result.</para>
  /// <para><b>Algorithm (SSE2 path).</b></para>
  /// <list type="number">
  ///   <item><description>Broadcast the target colour's 4 bytes to a 16-byte vector (T T T T).</description></item>
  ///   <item><description>Gather 4 candidates' 4 bytes each into a 16-byte vector (C0 C1 C2 C3).</description></item>
  ///   <item><description>Byte-wise absolute difference via <c>(T sub-sat C) | (C sub-sat T)</c>.</description></item>
  ///   <item><description>Zero-extend to 16-bit, multiply-low to square (fits — max 65025).</description></item>
  ///   <item><description><c>PMADDWD</c> pair-sums adjacent int16 squares into 4 int32 pair-sums.</description></item>
  ///   <item><description>Horizontal-add (SSSE3 when available, else 2× shuffle-add) yields 4 int32 squared distances.</description></item>
  ///   <item><description>Track per-lane minimum and map back to the candidate index.</description></item>
  /// </list>
  /// <para>The 0-3-candidate tail runs scalar; so does every call when
  /// <c>Sse2.IsSupported</c> is false (and when the TFM has no
  /// <c>System.Runtime.Intrinsics</c> at all). Ties resolve to the first
  /// minimum encountered, matching the scalar path exactly.</para>
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int FindMinDistance(in TKey reference, ReadOnlySpan<byte> candidates, int count, out int minIndex) {
    var targetU32 = Unsafe.As<TKey, uint>(ref Unsafe.AsRef(in reference));

    var bestIdx = 0;
    var bestDist = uint.MaxValue;
    var start = 0;

#if SUPPORTS_INTRINSICS
    if (Sse2.IsSupported && count >= 4) {
      var target = Vector128.Create(targetU32).AsByte();
      var tail = count & ~3;

      // 4-at-a-time batches.
      unsafe {
        fixed (byte* pb = candidates) {
          for (var b = 0; b < tail; b += 4) {
            var c0 = *(uint*)(pb + (b + 0) * 4);
            var c1 = *(uint*)(pb + (b + 1) * 4);
            var c2 = *(uint*)(pb + (b + 2) * 4);
            var c3 = *(uint*)(pb + (b + 3) * 4);

            var cand = Vector128.Create(c0, c1, c2, c3).AsByte();

            // Absolute-difference byte-wise: |T - C| = max(T-C, C-T) implemented via
            // two saturating subtractions OR-ed. Faster than going through signed
            // subtract + abs on Sse2.
            var diff = Sse2.Or(Sse2.SubtractSaturate(target, cand), Sse2.SubtractSaturate(cand, target));

            // Zero-extend bytes to int16. Unpacking against zero is the canonical
            // Sse2 widening.
            var zero = Vector128<byte>.Zero;
            var diffLo16 = Sse2.UnpackLow(diff, zero).AsInt16();  // 8× int16 from lanes 0..7
            var diffHi16 = Sse2.UnpackHigh(diff, zero).AsInt16(); // 8× int16 from lanes 8..15

            // PMADDWD: (a0·b0 + a1·b1), (a2·b2 + a3·b3), ...  Using diff twice yields
            // pair-sums of squared differences. 255² = 65025 fits in u16, so
            // reinterpret-as-int16 is safe because the subsequent int32 sum is wide
            // enough to hold the result (4·65025 = 260100 < 2³¹).
            var pair0 = Sse2.MultiplyAddAdjacent(diffLo16, diffLo16); // 4 int32: [c0(ab), c0(cd), c1(ab), c1(cd)]
            var pair1 = Sse2.MultiplyAddAdjacent(diffHi16, diffHi16); // 4 int32: [c2(ab), c2(cd), c3(ab), c3(cd)]

            // Horizontal-add: [c0(ab)+c0(cd), c1(ab)+c1(cd), c2(ab)+c2(cd), c3(ab)+c3(cd)]
            // = [dist(C0), dist(C1), dist(C2), dist(C3)]. Prefer SSSE3 PHADDD when
            // available; otherwise shuffle the pair-sums into a pair and add.
            Vector128<int> sums;
            if (Ssse3.IsSupported) {
              sums = Ssse3.HorizontalAdd(pair0, pair1);
            } else {
              // Manual fold for plain Sse2: pair0 = [a0,a1,b0,b1], pair1 = [c0,c1,d0,d1]
              // We want [a0+a1, b0+b1, c0+c1, d0+d1].
              var lo = Sse2.UnpackLow(pair0.AsInt64(), pair1.AsInt64()).AsInt32();   // [a0,a1,c0,c1]
              var hi = Sse2.UnpackHigh(pair0.AsInt64(), pair1.AsInt64()).AsInt32();  // [b0,b1,d0,d1]
              var evens = Sse2.Shuffle(lo, 0b_10_00_10_00); // [a0,c0,a0,c0]
              var odds = Sse2.Shuffle(lo, 0b_11_01_11_01);  // [a1,c1,a1,c1]
              var loSum = Sse2.Add(evens, odds);            // [a0+a1, c0+c1, a0+a1, c0+c1]
              var evens2 = Sse2.Shuffle(hi, 0b_10_00_10_00);// [b0,d0,b0,d0]
              var odds2 = Sse2.Shuffle(hi, 0b_11_01_11_01); // [b1,d1,b1,d1]
              var hiSum = Sse2.Add(evens2, odds2);          // [b0+b1, d0+d1, ...]
              // Interleave loSum low-2 and hiSum low-2 to get [a0+a1, b0+b1, c0+c1, d0+d1].
              sums = Sse2.UnpackLow(loSum, hiSum);
            }

            // Per-lane compare against the running best.
            var d0 = (uint)sums.GetElement(0);
            var d1 = (uint)sums.GetElement(1);
            var d2 = (uint)sums.GetElement(2);
            var d3 = (uint)sums.GetElement(3);

            if (d0 < bestDist) { bestDist = d0; bestIdx = b + 0; }
            if (d1 < bestDist) { bestDist = d1; bestIdx = b + 1; }
            if (d2 < bestDist) { bestDist = d2; bestIdx = b + 2; }
            if (d3 < bestDist) { bestDist = d3; bestIdx = b + 3; }

            if (bestDist == 0u) {
              minIndex = bestIdx;
              return (int)bestDist;
            }
          }
        }
      }
      start = tail;
    }
#endif

    // Scalar path (complete loop when SIMD is unavailable, 0-3-candidate tail otherwise).
    unsafe {
      fixed (byte* pb = candidates) {
        for (var i = start; i < count; ++i) {
          var cU32 = *(uint*)(pb + i * 4);
          var dR = (int)(targetU32 & 0xFF) - (int)(cU32 & 0xFF);
          var dG = (int)((targetU32 >> 8) & 0xFF) - (int)((cU32 >> 8) & 0xFF);
          var dB = (int)((targetU32 >> 16) & 0xFF) - (int)((cU32 >> 16) & 0xFF);
          var dA = (int)((targetU32 >> 24) & 0xFF) - (int)((cU32 >> 24) & 0xFF);
          var d = (uint)(dR * dR + dG * dG + dB * dB + dA * dA);
          if (d >= bestDist)
            continue;

          bestDist = d;
          bestIdx = i;
          if (d == 0u)
            break;
        }
      }
    }

    minIndex = bestIdx;
    return (int)bestDist;
  }

  /// <summary>
  /// Batch nearest-of-N for <em>multiple</em> reference colours against the same
  /// packed candidate buffer. Each entry of <paramref name="references"/> picks one
  /// candidate index that ends up in <paramref name="outIndices"/> at the same offset.
  /// </summary>
  /// <remarks>
  /// <para><b>SSE2 path.</b> Processes 4 references at a time. For each batch of
  /// 4 references we walk the candidate buffer in a single pass: each iteration
  /// loads 4 candidates and computes a 4-by-4 distance grid (4 reference lanes ×
  /// 4 candidate lanes). The two PMADDWD multiplications already produce 4 squared
  /// pair-sums per byte-vector, and after a horizontal-add we have 4 candidate
  /// distances per reference. We track per-reference best-index/best-distance in
  /// scalar registers so the tie-break stays first-occurrence-on-equal.</para>
  /// <para>The implementation reuses the per-reference path (<see cref="FindMinDistance"/>)
  /// for the &lt;4 leftover and for the unsupported runtime case, so behaviour stays
  /// identical to the scalar fallback. For the common consumer pattern (M references,
  /// each scanning the same N candidates) the cross-product fan-out is a simple
  /// outer loop over candidates and an inner SSE2 reference-batch.</para>
  /// </remarks>
  public void FindMinDistanceBatch(
    ReadOnlySpan<TKey> references,
    ReadOnlySpan<byte> candidates, int candidateCount,
    Span<int> outIndices) {

    var refCount = references.Length;
    if (refCount <= 0 || candidateCount <= 0)
      return;

#if SUPPORTS_INTRINSICS
    if (Sse2.IsSupported && candidateCount >= 4 && refCount >= 4) {
      // Process 4 references at a time. The inner candidate loop walks the buffer
      // once per reference-batch; the candidate vector is reused across all 4
      // reference lanes to amortise the load.
      var refTail = refCount & ~3;
      unsafe {
        fixed (byte* pb = candidates) {
          for (var r = 0; r < refTail; r += 4) {
            // Broadcast each of the 4 references to its own 16-byte vector.
            var rk0 = references[r + 0];
            var rk1 = references[r + 1];
            var rk2 = references[r + 2];
            var rk3 = references[r + 3];
            var t0u = Unsafe.As<TKey, uint>(ref rk0);
            var t1u = Unsafe.As<TKey, uint>(ref rk1);
            var t2u = Unsafe.As<TKey, uint>(ref rk2);
            var t3u = Unsafe.As<TKey, uint>(ref rk3);

            var v0 = Vector128.Create(t0u).AsByte();
            var v1 = Vector128.Create(t1u).AsByte();
            var v2 = Vector128.Create(t2u).AsByte();
            var v3 = Vector128.Create(t3u).AsByte();

            var b0 = uint.MaxValue;
            var b1 = uint.MaxValue;
            var b2 = uint.MaxValue;
            var b3 = uint.MaxValue;
            var i0 = 0;
            var i1 = 0;
            var i2 = 0;
            var i3 = 0;

            var candTail = candidateCount & ~3;
            for (var c = 0; c < candTail; c += 4) {
              var c0 = *(uint*)(pb + (c + 0) * 4);
              var c1 = *(uint*)(pb + (c + 1) * 4);
              var c2 = *(uint*)(pb + (c + 2) * 4);
              var c3 = *(uint*)(pb + (c + 3) * 4);

              var cand = Vector128.Create(c0, c1, c2, c3).AsByte();

              // 4 distances per reference = 4 horizontal-adds; we compute them
              // four times (once per reference) but the candidate vector is hot
              // in registers so this is cheap.
              var d0 = _DistancesAgainstCandidates4(v0, cand);
              var d1 = _DistancesAgainstCandidates4(v1, cand);
              var d2 = _DistancesAgainstCandidates4(v2, cand);
              var d3 = _DistancesAgainstCandidates4(v3, cand);

              _UpdateBest4(d0, c, ref b0, ref i0);
              _UpdateBest4(d1, c, ref b1, ref i1);
              _UpdateBest4(d2, c, ref b2, ref i2);
              _UpdateBest4(d3, c, ref b3, ref i3);
            }

            // Scalar tail across the last 0-3 candidates for each reference.
            for (var c = candTail; c < candidateCount; ++c) {
              var cU32 = *(uint*)(pb + c * 4);
              var d0 = _ScalarSquaredDist(t0u, cU32);
              var d1 = _ScalarSquaredDist(t1u, cU32);
              var d2 = _ScalarSquaredDist(t2u, cU32);
              var d3 = _ScalarSquaredDist(t3u, cU32);
              if (d0 < b0) { b0 = d0; i0 = c; }
              if (d1 < b1) { b1 = d1; i1 = c; }
              if (d2 < b2) { b2 = d2; i2 = c; }
              if (d3 < b3) { b3 = d3; i3 = c; }
            }

            outIndices[r + 0] = i0;
            outIndices[r + 1] = i1;
            outIndices[r + 2] = i2;
            outIndices[r + 3] = i3;
          }
        }
      }

      // Tail references (<4): fall through to the scalar per-reference path below.
      for (var r = refTail; r < refCount; ++r) {
        this.FindMinDistance(references[r], candidates, candidateCount, out var idx);
        outIndices[r] = idx;
      }
      return;
    }
#endif

    // Scalar path: forward to the per-reference helper.
    for (var r = 0; r < refCount; ++r) {
      this.FindMinDistance(references[r], candidates, candidateCount, out var idx);
      outIndices[r] = idx;
    }
  }

  /// <summary>
  /// SIMD-backed top-k closest-candidates scan with first-occurrence tie-break. Forwards
  /// to the generic scalar fallback because the SIMD top-k machinery is more involved
  /// than its callers (NClosest / Barycentric / NaturalNeighbour ditherers) need — the
  /// per-pixel cost dominates anyway and the scalar path is correct.
  /// </summary>
  public int FindNClosest(
    in TKey reference,
    ReadOnlySpan<byte> candidates, int candidateCount,
    int k,
    Span<int> outIndices, Span<int> outDistances)
    => BatchDistanceDefaults.FindNClosestScalar<TKey, EuclideanSquared4B<TKey>>(
      in this, reference, candidates, candidateCount, k, outIndices, outDistances);

#if SUPPORTS_INTRINSICS
  /// <summary>
  /// Computes 4 squared distances [d(t,c0), d(t,c1), d(t,c2), d(t,c3)] given the
  /// broadcast-target vector and a 16-byte candidate vector packed C0|C1|C2|C3.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Vector128<int> _DistancesAgainstCandidates4(Vector128<byte> target, Vector128<byte> cand) {
    var diff = Sse2.Or(Sse2.SubtractSaturate(target, cand), Sse2.SubtractSaturate(cand, target));
    var zero = Vector128<byte>.Zero;
    var diffLo16 = Sse2.UnpackLow(diff, zero).AsInt16();
    var diffHi16 = Sse2.UnpackHigh(diff, zero).AsInt16();
    var pair0 = Sse2.MultiplyAddAdjacent(diffLo16, diffLo16);
    var pair1 = Sse2.MultiplyAddAdjacent(diffHi16, diffHi16);

    if (Ssse3.IsSupported) {
      return Ssse3.HorizontalAdd(pair0, pair1);
    }

    var lo = Sse2.UnpackLow(pair0.AsInt64(), pair1.AsInt64()).AsInt32();
    var hi = Sse2.UnpackHigh(pair0.AsInt64(), pair1.AsInt64()).AsInt32();
    var evens = Sse2.Shuffle(lo, 0b_10_00_10_00);
    var odds = Sse2.Shuffle(lo, 0b_11_01_11_01);
    var loSum = Sse2.Add(evens, odds);
    var evens2 = Sse2.Shuffle(hi, 0b_10_00_10_00);
    var odds2 = Sse2.Shuffle(hi, 0b_11_01_11_01);
    var hiSum = Sse2.Add(evens2, odds2);
    return Sse2.UnpackLow(loSum, hiSum);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _UpdateBest4(Vector128<int> sums, int baseC, ref uint best, ref int bestIdx) {
    var d0 = (uint)sums.GetElement(0);
    var d1 = (uint)sums.GetElement(1);
    var d2 = (uint)sums.GetElement(2);
    var d3 = (uint)sums.GetElement(3);
    if (d0 < best) { best = d0; bestIdx = baseC + 0; }
    if (d1 < best) { best = d1; bestIdx = baseC + 1; }
    if (d2 < best) { best = d2; bestIdx = baseC + 2; }
    if (d3 < best) { best = d3; bestIdx = baseC + 3; }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static uint _ScalarSquaredDist(uint t, uint c) {
    var dR = (int)(t & 0xFF) - (int)(c & 0xFF);
    var dG = (int)((t >> 8) & 0xFF) - (int)((c >> 8) & 0xFF);
    var dB = (int)((t >> 16) & 0xFF) - (int)((c >> 16) & 0xFF);
    var dA = (int)((t >> 24) & 0xFF) - (int)((c >> 24) & 0xFF);
    return (uint)(dR * dR + dG * dG + dB * dB + dA * dA);
  }
#endif
}

#endregion
