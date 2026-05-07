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
using System.Linq;
using System.Runtime.CompilerServices;
#if SUPPORTS_INTRINSICS
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif
using Hawkynt.ColorProcessing.Internal;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Storage;
using Hawkynt.ColorProcessing.Working;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Quantization;

/// <summary>
/// K-Means clustering color quantizer (Lloyd's algorithm with K-means++ seeding).
/// </summary>
/// <remarks>
/// <para>Standard Lloyd-Forgy iteration: assign each pixel to the nearest cluster
/// centre, then recompute centres as the mean of their assigned pixels; repeat
/// until convergence. Initialised with K-means++ for fast convergence and
/// near-optimal local minima.</para>
/// <para>References:
/// J. MacQueen, "Some methods for classification and analysis of multivariate
/// observations", Proc. 5th Berkeley Symposium 1:281-297, 1967.
/// S. P. Lloyd, "Least squares quantization in PCM", IEEE Trans. Information
/// Theory 28(2):129-137, 1982.
/// D. Arthur &amp; S. Vassilvitskii, "k-means++: The Advantages of Careful Seeding",
/// SODA '07 Proceedings, pp. 1027-1035, 2007.</para>
/// </remarks>
[Quantizer(QuantizationType.Clustering, DisplayName = "K-Means", Author = "J. MacQueen", Year = 1967, QualityRating = 7)]
public struct KMeansQuantizer : IQuantizer {

  /// <summary>
  /// Gets or sets the maximum number of iterations before stopping.
  /// </summary>
  public int MaxIterations { get; set; } = 100;

  /// <summary>
  /// Gets or sets the convergence threshold (normalized 0-1).
  /// </summary>
  public float ConvergenceThreshold { get; set; } = 0.001f;

  public KMeansQuantizer() { }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>() => new Kernel<TWork>(this.MaxIterations, this.ConvergenceThreshold);

  internal sealed class Kernel<TWork>(int maxIterations, float convergenceThreshold) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount)
      => QuantizerHelper.GeneratePaletteWithReduction(histogram, colorCount, this._ReduceColorsTo);

    private IEnumerable<TWork> _ReduceColorsTo(int colorCount, (TWork color, uint count)[] colors) {
      if (colors.Length == 0)
        return [];

      if (colors.Length <= colorCount)
        return colors.Select(c => c.color);

      var metric = new Euclidean4N<TWork>();
      var centroids = _InitializeCentroidsKMeansPlusPlus(colors, colorCount, metric);
      var assignments = new int[colors.Length];
      var threshold = UNorm32.FromFloatClamped(convergenceThreshold);

      // Scratch buffer for the counting-sort pre-sort in centroid recalc.
      // Keeping it outside the loop avoids per-iteration allocation. The sort+sum
      // only changes the order in which clusters' running sums are written into
      // sums[k] — within each cluster, the original colour-array order is preserved,
      // so floating-point summation is bit-exact with the scatter version.
      var sortedIdx = new int[colors.Length];

      // Pre-pack centroids into a contiguous byte buffer for the SSE2
      // batch-distance kernel when TWork is the byte-domain Bgra8888 type. The
      // argmin produced by EuclideanSquared4B<Bgra8888> matches Euclidean4N<Bgra8888>'s
      // argmin (squared distance vs sqrt of squared distance is monotonic), so the
      // assignment indices are bit-exact and the resulting centroids match.
      //
      // Parallel branch for the high-quality LinearRgbaF (float-domain) path —
      // EuclideanSquared4F<LinearRgbaF> implements IBatchDistance<LinearRgbaF> with
      // an SSE Vector128<float> AoS kernel. Argmin again matches Euclidean4N<LinearRgbaF>
      // because both are monotonic transforms of the same squared sum (sqrt vs.
      // identity on non-negative). The two checks fold to a single constant-true
      // branch in each JIT-specialised generic instantiation, so dispatch is free.
      var useByteSimd = typeof(TWork) == typeof(Bgra8888);
      var useFloatSimd = typeof(TWork) == typeof(LinearRgbaF);
      byte[]? packedCentroids = null;
      EuclideanSquared4B<Bgra8888> simdMetricB = default;
      EuclideanSquared4F<LinearRgbaF> simdMetricF = default;
      if (useByteSimd)
        packedCentroids = new byte[colorCount * 4];
      else if (useFloatSimd)
        packedCentroids = new byte[colorCount * Unsafe.SizeOf<LinearRgbaF>()];

      for (var iteration = 0; iteration < maxIterations; ++iteration) {
        // Assign each color to nearest centroid
        if (useByteSimd) {
          _PackBgra8888Centroids(centroids, packedCentroids!);
          _AssignBgra8888Batch(colors, packedCentroids!, colorCount, assignments, simdMetricB);
        } else if (useFloatSimd) {
          _PackLinearRgbaFCentroids(centroids, packedCentroids!);
          _AssignLinearRgbaFBatch(colors, packedCentroids!, colorCount, assignments, simdMetricF);
        } else {
          for (var i = 0; i < colors.Length; ++i)
            assignments[i] = _FindNearestCentroid(colors[i].color, centroids, metric);
        }

        // Calculate new centroids
        var newCentroids = _CalculateCentroids(colors, assignments, colorCount, sortedIdx);

        // Check for convergence
        var maxMovement = UNorm32.Zero;
        for (var i = 0; i < colorCount; ++i) {
          var movement = metric.Distance(centroids[i], newCentroids[i]);
          if (movement > maxMovement)
            maxMovement = movement;
        }

        centroids = newCentroids;

        if (maxMovement < threshold)
          break;
      }

      return centroids;
    }

    /// <summary>
    /// K-means++ centroid initialisation. The expensive parts of this routine are the
    /// two distance loops: the bootstrap fill after picking centroid #0, and the
    /// per-iteration "update distances (keep minimum)" pass after each subsequent
    /// centroid pick. Both have the same shape — distance from N points to ONE
    /// centroid — so they share an inner kernel that picks a SIMD-batched specialisation
    /// when <typeparamref name="TWork"/> is <see cref="Bgra8888"/> or
    /// <see cref="LinearRgbaF"/>, and falls back to scalar for every other working
    /// space (where the kernel mirrors the original scalar code 1:1).
    /// </summary>
    /// <remarks>
    /// <para>SIMD batching for the init phase, mirroring M5/P2's per-iteration
    /// assignment SIMD. The scalar kernel walks one point at a time calling
    /// <see cref="Euclidean4N{TWork}.Distance"/>; the SIMD kernels short-circuit
    /// the squared-distance computation through vector instructions (PMADDWD for
    /// bytes, plain SSE for floats) and do the per-point sqrt+UNorm32 conversion
    /// scalarly. Argmin behaviour is preserved (this loop doesn't argmin — it just
    /// produces the running per-point min distance), and the cumulative-distribution
    /// sample is driven by exactly the same RNG sequence as before.</para>
    /// <para><b>Determinism.</b> Same <c>seed=42</c> and the same number of
    /// <c>random.NextDouble()</c>/<c>random.Next(...)</c> calls, in the same order,
    /// guarantees the same RNG draw indices regardless of the SIMD/scalar split.
    /// What CAN differ is the per-point UNorm32 distance value: the SIMD byte path
    /// computes integer squared distance exactly then converts to UNorm32 via
    /// <c>sqrt(int_sq / 65025f) * 0.5f</c> — one float division — whereas the scalar
    /// <see cref="Euclidean4N{TWork}.Distance"/> path squares 4 normalised float
    /// differences and sums in float. Both arrive at the same real-number result,
    /// but float associativity means the UNorm32 raw value can differ by ±1 LSB
    /// in pathological cases. That ULP wobble can in principle flip the cumulative
    /// sample at a tie boundary; in practice the K-means iteration that follows
    /// is self-correcting, so the final palette stays byte-exact across the
    /// 82-image golden suite (verified). Same caveat for the float path
    /// (LinearRgbaF) — the scalar Euclidean4N path summed per-component, the SIMD
    /// kernel uses lane-pair-then-cross horizontal-add; ULP-level differences
    /// possible. The high-quality K-means path has no golden, so this is not
    /// guarded against.</para>
    /// </remarks>
    private static TWork[] _InitializeCentroidsKMeansPlusPlus((TWork color, uint count)[] colors, int k, Euclidean4N<TWork> metric) {
      var random = new Random(42);
      var centroids = new TWork[k];
      var distances = new UNorm32[colors.Length];

      // SIMD-dispatch flags — both checks fold to a single constant-true branch
      // in each JIT-specialised generic instantiation, mirroring the assignment
      // path's typeof discrimination.
      var useByteSimd = typeof(TWork) == typeof(Bgra8888);
      var useFloatSimd = typeof(TWork) == typeof(LinearRgbaF);

      // Choose first centroid randomly (weighted by count)
      var totalWeight = colors.Sum(c => (long)c.count);
      var target = random.NextDouble() * totalWeight;
      long cumulative = 0;
      for (var i = 0; i < colors.Length; ++i) {
        cumulative += colors[i].count;
        if (!(cumulative >= target))
          continue;

        centroids[0] = colors[i].color;
        break;
      }

      // Initialize distances (SIMD-batched min-init — first centroid only, so the
      // running min is just the new distance; we pass a sentinel "old min = One" so
      // the min(old, new) result is always new).
      _UpdateRunningMinDistances(colors, centroids[0], distances, isFirstCentroid: true, metric, useByteSimd, useFloatSimd);

      // Choose remaining centroids
      for (var c = 1; c < k; ++c) {
        var totalDist = 0.0;
        for (var i = 0; i < colors.Length; ++i)
          totalDist += (float)distances[i] * colors[i].count;

        if (totalDist <= 0) {
          centroids[c] = colors[random.Next(colors.Length)].color;
          continue;
        }

        target = random.NextDouble() * totalDist;
        double cumulativeDist = 0;
        var selectedIndex = 0;
        for (var i = 0; i < colors.Length; ++i) {
          cumulativeDist += (float)distances[i] * colors[i].count;
          if (cumulativeDist >= target) {
            selectedIndex = i;
            break;
          }
        }

        centroids[c] = colors[selectedIndex].color;

        // Update distances (keep minimum) — SIMD-batched.
        _UpdateRunningMinDistances(colors, centroids[c], distances, isFirstCentroid: false, metric, useByteSimd, useFloatSimd);
      }

      return centroids;
    }

    /// <summary>
    /// Computes <c>distances[i] = min(distances[i], metric.Distance(colors[i].color, centroid))</c>
    /// for every <c>i</c>. Dispatches to a SIMD specialisation when <typeparamref name="TWork"/>
    /// is <see cref="Bgra8888"/> (PMADDWD-based int squared distance) or
    /// <see cref="LinearRgbaF"/> (SSE float squared distance), otherwise falls
    /// through to the scalar metric.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void _UpdateRunningMinDistances(
      (TWork color, uint count)[] colors,
      TWork centroid,
      UNorm32[] distances,
      bool isFirstCentroid,
      Euclidean4N<TWork> metric,
      bool useByteSimd,
      bool useFloatSimd) {

#if SUPPORTS_INTRINSICS
      if (useByteSimd && Sse2.IsSupported) {
        _UpdateRunningMinDistancesBgra8888(colors, centroid, distances, isFirstCentroid);
        return;
      }

      if (useFloatSimd && Sse.IsSupported) {
        _UpdateRunningMinDistancesLinearRgbaF(colors, centroid, distances, isFirstCentroid);
        return;
      }
#endif

      // Scalar fallback: bit-exact match for the original loop.
      for (var i = 0; i < colors.Length; ++i) {
        var newDist = metric.Distance(colors[i].color, centroid);
        if (isFirstCentroid || newDist < distances[i])
          distances[i] = newDist;
      }
    }

#if SUPPORTS_INTRINSICS
    /// <summary>
    /// Byte-domain SIMD specialisation. SIMD-computes the integer squared distance
    /// from each point to the new centroid (4 points per PMADDWD batch), then converts
    /// each int32 squared distance to a <see cref="UNorm32"/> via
    /// <c>sqrt(d² / 65025f) * 0.5f</c>. Result is stored as the running min.
    /// </summary>
    /// <remarks>
    /// <para>Bit-exactness vs. <see cref="Euclidean4N{TWork}.Distance"/>: integer
    /// arithmetic (byte-difference, square, sum) is exact. The conversion to UNorm32
    /// uses ONE float division (<c>d² / 65025f</c>) whereas the scalar metric squares
    /// four normalised float differences and sums them. Both yield the same
    /// real-number value, but IEEE-754 associativity allows ±1 LSB drift in the
    /// resulting UNorm32. The 82-image Bgra8888 golden suite verifies the cumulative
    /// post-convergence palette is unaffected.</para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void _UpdateRunningMinDistancesBgra8888(
      (TWork color, uint count)[] colors,
      TWork centroid,
      UNorm32[] distances,
      bool isFirstCentroid) {

      // 1/(255*255) — pre-divide so the per-point conversion is one MUL not one DIV.
      // sqrt(int_sq * inv_65025) = sqrt(int_sq) * sqrt(inv_65025), but doing the
      // multiply inside the sqrt argument is one float op cheaper and matches
      // the precision of the scalar's pre-square per-component normalisation
      // closely enough to keep the goldens stable.
      const float Inv65025 = 1f / 65025f; // == 1/(255*255)
      const float HalfFactor = 0.5f;      // matches Euclidean4N's *0.5f post-sqrt

      var n = colors.Length;
      var centroidU32 = Unsafe.As<TWork, uint>(ref centroid);

      unsafe {
        // SIMD path: 4 points per iteration via PMADDWD, mirroring
        // EuclideanSquared4B<Bgra8888>.FindMinDistance's hot kernel. The per-point
        // post-process (sqrt + UNorm32) stays scalar.
        var simdTail = n & ~3;
        var i = 0;

        if (Sse2.IsSupported && n >= 4) {
          var target = Vector128.Create(centroidU32).AsByte();

          for (; i < simdTail; i += 4) {
            // Pack 4 points into a 16-byte vector. The TWork copy is unavoidable
            // (colors[i].color is a value, not a ref) but the JIT lifts each into a
            // register; reinterpret-as-uint is a no-op at machine level.
            var c0 = colors[i + 0].color;
            var c1 = colors[i + 1].color;
            var c2 = colors[i + 2].color;
            var c3 = colors[i + 3].color;
            var p0 = Unsafe.As<TWork, uint>(ref c0);
            var p1 = Unsafe.As<TWork, uint>(ref c1);
            var p2 = Unsafe.As<TWork, uint>(ref c2);
            var p3 = Unsafe.As<TWork, uint>(ref c3);

            var cand = Vector128.Create(p0, p1, p2, p3).AsByte();

            // |T - C| via two saturating subtracts OR-ed (matches EuclideanSquared4B).
            var diff = Sse2.Or(Sse2.SubtractSaturate(target, cand), Sse2.SubtractSaturate(cand, target));

            // Zero-extend bytes to int16 (low/high halves), then PMADDWD to get
            // pair-sums of squared differences.
            var zero = Vector128<byte>.Zero;
            var diffLo16 = Sse2.UnpackLow(diff, zero).AsInt16();
            var diffHi16 = Sse2.UnpackHigh(diff, zero).AsInt16();
            var pair0 = Sse2.MultiplyAddAdjacent(diffLo16, diffLo16);
            var pair1 = Sse2.MultiplyAddAdjacent(diffHi16, diffHi16);

            // Horizontal-add of pair-sums to get 4 int32 squared distances.
            Vector128<int> sums;
            if (Ssse3.IsSupported) {
              sums = Ssse3.HorizontalAdd(pair0, pair1);
            } else {
              var lo = Sse2.UnpackLow(pair0.AsInt64(), pair1.AsInt64()).AsInt32();
              var hi = Sse2.UnpackHigh(pair0.AsInt64(), pair1.AsInt64()).AsInt32();
              var evens = Sse2.Shuffle(lo, 0b_10_00_10_00);
              var odds = Sse2.Shuffle(lo, 0b_11_01_11_01);
              var loSum = Sse2.Add(evens, odds);
              var evens2 = Sse2.Shuffle(hi, 0b_10_00_10_00);
              var odds2 = Sse2.Shuffle(hi, 0b_11_01_11_01);
              var hiSum = Sse2.Add(evens2, odds2);
              sums = Sse2.UnpackLow(loSum, hiSum);
            }

            // Per-lane: int sq dist -> UNorm32 normalised distance, then min-update.
            var d0 = sums.GetElement(0);
            var d1 = sums.GetElement(1);
            var d2 = sums.GetElement(2);
            var d3 = sums.GetElement(3);

            var u0 = UNorm32.FromFloatClamped((float)Math.Sqrt(d0 * Inv65025) * HalfFactor);
            var u1 = UNorm32.FromFloatClamped((float)Math.Sqrt(d1 * Inv65025) * HalfFactor);
            var u2 = UNorm32.FromFloatClamped((float)Math.Sqrt(d2 * Inv65025) * HalfFactor);
            var u3 = UNorm32.FromFloatClamped((float)Math.Sqrt(d3 * Inv65025) * HalfFactor);

            if (isFirstCentroid) {
              distances[i + 0] = u0;
              distances[i + 1] = u1;
              distances[i + 2] = u2;
              distances[i + 3] = u3;
            } else {
              if (u0 < distances[i + 0]) distances[i + 0] = u0;
              if (u1 < distances[i + 1]) distances[i + 1] = u1;
              if (u2 < distances[i + 2]) distances[i + 2] = u2;
              if (u3 < distances[i + 3]) distances[i + 3] = u3;
            }
          }
        }

        // Scalar tail (0-3 points).
        for (; i < n; ++i) {
          var ci = colors[i].color;
          var pU32 = Unsafe.As<TWork, uint>(ref ci);
          var dR = (int)(centroidU32 & 0xFF) - (int)(pU32 & 0xFF);
          var dG = (int)((centroidU32 >> 8) & 0xFF) - (int)((pU32 >> 8) & 0xFF);
          var dB = (int)((centroidU32 >> 16) & 0xFF) - (int)((pU32 >> 16) & 0xFF);
          var dA = (int)((centroidU32 >> 24) & 0xFF) - (int)((pU32 >> 24) & 0xFF);
          var dInt = dR * dR + dG * dG + dB * dB + dA * dA;
          var u = UNorm32.FromFloatClamped((float)Math.Sqrt(dInt * Inv65025) * HalfFactor);
          if (isFirstCentroid || u < distances[i])
            distances[i] = u;
        }
      }
    }

    /// <summary>
    /// Float-domain SIMD specialisation. SIMD-computes the float squared distance
    /// from each point to the new centroid (one Vector128&lt;float&gt; per point), then
    /// converts each scalar squared distance to a <see cref="UNorm32"/> via
    /// <c>sqrt(d²) * 0.5f</c>. Mirrors <see cref="EuclideanSquared4F{TKey}"/>'s
    /// horizontal-add path so the math is the same shape as the M6 assignment kernel.
    /// </summary>
    /// <remarks>
    /// The horizontal-add reorders associativity vs. the scalar Euclidean4N path
    /// (lane-pair-then-cross vs. left-to-right). Could differ by ULP in the
    /// resulting UNorm32, which could (very rarely) flip the cumulative-sample
    /// pick. There is no LinearRgbaF K-means golden, so this is not test-guarded.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void _UpdateRunningMinDistancesLinearRgbaF(
      (TWork color, uint count)[] colors,
      TWork centroid,
      UNorm32[] distances,
      bool isFirstCentroid) {

      const float HalfFactor = 0.5f; // matches Euclidean4N's *0.5f post-sqrt
      var n = colors.Length;

      // Broadcast the centroid as a Vector128<float> (R, G, B, A).
      ref var centroidRef = ref centroid;
      var refVec = Sse.LoadVector128((float*)Unsafe.AsPointer(ref centroidRef));

      for (var i = 0; i < n; ++i) {
        var pColor = colors[i].color;
        var candVec = Sse.LoadVector128((float*)Unsafe.AsPointer(ref pColor));
        var diff = Sse.Subtract(refVec, candVec);
        var sq = Sse.Multiply(diff, diff);
        // Horizontal-add of 4 lanes (matches EuclideanSquared4F._SquaredDistance):
        // 2-shuffle-add fold to lane0 = a+b+c+d.
        var shuf1 = Sse.Shuffle(sq, sq, 0b_00_00_11_10);
        var pair = Sse.Add(sq, shuf1);
        var shuf2 = Sse.Shuffle(pair, pair, 0b_00_00_00_01);
        var sum = Sse.AddScalar(pair, shuf2);
        var dSquared = sum.GetElement(0);

        var u = UNorm32.FromFloatClamped((float)Math.Sqrt(dSquared) * HalfFactor);
        if (isFirstCentroid || u < distances[i])
          distances[i] = u;
      }
    }
#endif

    private static int _FindNearestCentroid(TWork color, TWork[] centroids, Euclidean4N<TWork> metric) {
      var nearest = 0;
      var minDist = metric.Distance(color, centroids[0]);

      for (var i = 1; i < centroids.Length; ++i) {
        var dist = metric.Distance(color, centroids[i]);
        if (!(dist < minDist))
          continue;

        minDist = dist;
        nearest = i;
      }

      return nearest;
    }

    /// <summary>
    /// Counting-sort the colour entries by their assigned cluster, then sum
    /// each cluster's contributions in original colour-array order. The scatter pattern
    /// <c>sums[assignments[i]] += ...</c> is replaced by a sequential walk that's
    /// prefetch-friendly and trivially auto-vectorisable.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Bit-exactness: within each cluster, the entries are visited in their original
    /// <c>colors[i]</c> order (the counting sort is a stable bucket of the per-i
    /// assignment, and we iterate i ascending so the bucket fills in ascending i).
    /// Inter-cluster ordering of the <c>sums[k]</c> updates is irrelevant because
    /// each cluster's sum is independent of the others. So the floating-point
    /// addition order within every <c>c1Sums[k]</c> is identical to the scatter
    /// version, giving byte-exact goldens.
    /// </para>
    /// </remarks>
    private static TWork[] _CalculateCentroids((TWork color, uint count)[] colors, int[] assignments, int k, int[] sortedIdx) {
      // Accumulate in normalized float space
      var c1Sums = new double[k];
      var c2Sums = new double[k];
      var c3Sums = new double[k];
      var aSums = new double[k];
      var weights = new double[k];

      var n = colors.Length;

      // Pass 1: counting sort by cluster -- count, then prefix-sum.
      // bucketStart[k] = start offset in sortedIdx for cluster k.
      // We use a (k+1)-sized array so bucketStart[k+1] is the end-of-bucket sentinel.
      var bucketStart = new int[k + 1];
      for (var i = 0; i < n; ++i)
        ++bucketStart[assignments[i] + 1];
      for (var c = 0; c < k; ++c)
        bucketStart[c + 1] += bucketStart[c];

      // Pass 2: scatter into sortedIdx using a copy of bucketStart as the cursor.
      // After this, for every cluster k the entries in sortedIdx[bucketStart[k]..bucketStart[k+1]]
      // are colour-array indices in ascending order.
      // We keep bucketStart as the per-cluster start; we walk a separate cursor[] copy
      // so the original prefix-sum stays intact for Pass 3.
      var cursor = new int[k];
      for (var c = 0; c < k; ++c)
        cursor[c] = bucketStart[c];
      for (var i = 0; i < n; ++i) {
        var cl = assignments[i];
        sortedIdx[cursor[cl]++] = i;
      }

      // Pass 3: per-cluster sequential sum. Each cluster's sums[c1Sums[c],...] is
      // accumulated in original-i order, so float-add associativity is identical to
      // the scatter version.
      for (var c = 0; c < k; ++c) {
        var start = bucketStart[c];
        var end = bucketStart[c + 1];
        if (start == end)
          continue;

        var s1 = 0.0;
        var s2 = 0.0;
        var s3 = 0.0;
        var sa = 0.0;
        var sw = 0.0;
        for (var j = start; j < end; ++j) {
          var i = sortedIdx[j];
          var (c1, c2, c3, a) = colors[i].color.ToNormalized();
          var weight = colors[i].count;
          s1 += c1.ToFloat() * weight;
          s2 += c2.ToFloat() * weight;
          s3 += c3.ToFloat() * weight;
          sa += a.ToFloat() * weight;
          sw += weight;
        }
        c1Sums[c] = s1;
        c2Sums[c] = s2;
        c3Sums[c] = s3;
        aSums[c] = sa;
        weights[c] = sw;
      }

      var centroids = new TWork[k];
      for (var i = 0; i < k; ++i) {
        if (weights[i] > 0)
          centroids[i] = ColorFactory.FromNormalized_4<TWork>(
            UNorm32.FromFloatClamped((float)(c1Sums[i] / weights[i])),
            UNorm32.FromFloatClamped((float)(c2Sums[i] / weights[i])),
            UNorm32.FromFloatClamped((float)(c3Sums[i] / weights[i])),
            UNorm32.FromFloatClamped((float)(aSums[i] / weights[i]))
          );
      }

      return centroids;
    }

    /// <summary>
    /// Copy the K Bgra8888 centroids (4 bytes each) into the packed byte buffer
    /// the SSE2 <see cref="EuclideanSquared4B{TKey}.FindMinDistanceBatch"/> kernel expects.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void _PackBgra8888Centroids(TWork[] centroids, byte[] packed) {
      // TWork == Bgra8888 here (caller-checked); reinterpret centroids[i] as uint and
      // write directly. We don't use Buffer.BlockCopy because TWork is a generic
      // unmanaged struct of size 4 in this branch but the JIT can't prove it inside the
      // generic method body without typeof discrimination.
      for (var i = 0; i < centroids.Length; ++i) {
        var c = centroids[i];
        var u = Unsafe.As<TWork, uint>(ref c);
        packed[i * 4 + 0] = (byte)u;
        packed[i * 4 + 1] = (byte)(u >> 8);
        packed[i * 4 + 2] = (byte)(u >> 16);
        packed[i * 4 + 3] = (byte)(u >> 24);
      }
    }

    /// <summary>
    /// SSE2-backed assignment of every histogram entry to its nearest centroid.
    /// Forwards to the metric's batched <see cref="IBatchDistance{TKey}.FindMinDistanceBatch"/>
    /// implementation in chunks of 64 references at a time so the working set stays L1-resident.
    /// </summary>
    private static void _AssignBgra8888Batch(
      (TWork color, uint count)[] colors,
      byte[] packedCentroids,
      int colorCount,
      int[] assignments,
      EuclideanSquared4B<Bgra8888> metric) {
      const int batchSize = 64;
      var n = colors.Length;
      var refsBuf = new Bgra8888[batchSize];
      Span<int> outBuf = stackalloc int[batchSize];

      var i = 0;
      while (i < n) {
        var take = n - i < batchSize ? n - i : batchSize;
        // Copy the references for this chunk (tight loop; the JIT folds the typeof check).
        for (var j = 0; j < take; ++j) {
          var c = colors[i + j].color;
          refsBuf[j] = Unsafe.As<TWork, Bgra8888>(ref c);
        }

        ReadOnlySpan<Bgra8888> refs = refsBuf.AsSpan(0, take);
        Span<int> outIdx = outBuf.Slice(0, take);
        metric.FindMinDistanceBatch(refs, packedCentroids, colorCount, outIdx);

        for (var j = 0; j < take; ++j)
          assignments[i + j] = outIdx[j];

        i += take;
      }
    }

    /// <summary>
    /// Copy the K LinearRgbaF centroids (16 bytes each) into the packed byte buffer
    /// the SSE <see cref="EuclideanSquared4F{TKey}.FindMinDistanceBatch"/> kernel expects.
    /// LinearRgbaF is <c>StructLayout(Sequential, Pack=4, Size=16)</c> so a raw byte
    /// memcpy preserves the AoS (R,G,B,A) layout the kernel reads.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void _PackLinearRgbaFCentroids(TWork[] centroids, byte[] packed) {
      // TWork == LinearRgbaF here (caller-checked). Reinterpret each centroid as a
      // 16-byte block and copy via Unsafe.CopyBlock — dst alignment is byte-level
      // (managed byte[]); src alignment is float-level (LinearRgbaF Pack=4).
      var elementSize = sizeof(LinearRgbaF);
      fixed (byte* dstBase = packed) {
        for (var i = 0; i < centroids.Length; ++i) {
          var c = centroids[i];
          ref var srcRef = ref Unsafe.As<TWork, byte>(ref c);
          Unsafe.CopyBlockUnaligned(dstBase + i * elementSize, (byte*)Unsafe.AsPointer(ref srcRef), (uint)elementSize);
        }
      }
    }

    /// <summary>
    /// SSE-backed assignment for the high-quality LinearRgbaF path. Mirrors
    /// <see cref="_AssignBgra8888Batch"/>: chunks of 64 references → metric's
    /// <see cref="IBatchDistance{TKey}.FindMinDistanceBatch"/>. The metric's SIMD
    /// kernel processes 4 references × N candidates, with the candidate buffer
    /// held hot in registers across the inner loop. Tie-break is first-occurrence
    /// (matching the scalar Euclidean4N path).
    /// </summary>
    private static void _AssignLinearRgbaFBatch(
      (TWork color, uint count)[] colors,
      byte[] packedCentroids,
      int colorCount,
      int[] assignments,
      EuclideanSquared4F<LinearRgbaF> metric) {
      const int batchSize = 64;
      var n = colors.Length;
      var refsBuf = new LinearRgbaF[batchSize];
      Span<int> outBuf = stackalloc int[batchSize];

      var i = 0;
      while (i < n) {
        var take = n - i < batchSize ? n - i : batchSize;
        for (var j = 0; j < take; ++j) {
          var c = colors[i + j].color;
          refsBuf[j] = Unsafe.As<TWork, LinearRgbaF>(ref c);
        }

        ReadOnlySpan<LinearRgbaF> refs = refsBuf.AsSpan(0, take);
        Span<int> outIdx = outBuf.Slice(0, take);
        metric.FindMinDistanceBatch(refs, packedCentroids, colorCount, outIdx);

        for (var j = 0; j < take; ++j)
          assignments[i + j] = outIdx[j];

        i += take;
      }
    }

  }
}

/// <summary>
/// Euclidean distance metric for IColorSpace4 using normalized values.
/// </summary>
internal readonly struct Euclidean4N<TWork> : IColorMetric<TWork>
  where TWork : unmanaged, IColorSpace4<TWork> {

  public UNorm32 Distance(in TWork a, in TWork b) {
    var (a1, a2, a3, aa) = a.ToNormalized();
    var (b1, b2, b3, ba) = b.ToNormalized();

    var d1 = a1.ToFloat() - b1.ToFloat();
    var d2 = a2.ToFloat() - b2.ToFloat();
    var d3 = a3.ToFloat() - b3.ToFloat();
    var da = aa.ToFloat() - ba.ToFloat();

    var distSquared = d1 * d1 + d2 * d2 + d3 * d3 + da * da;
    return UNorm32.FromFloatClamped((float)Math.Sqrt(distSquared) * 0.5f);
  }
}
