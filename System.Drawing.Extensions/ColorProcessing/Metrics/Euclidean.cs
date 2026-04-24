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
/// </remarks>
public readonly struct EuclideanSquared4F<TKey> : IColorMetric<TKey>, INormalizedMetric
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
}

#endregion
