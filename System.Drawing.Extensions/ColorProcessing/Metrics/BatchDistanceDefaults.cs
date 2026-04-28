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
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Metrics;

/// <summary>
/// Generic scalar fallbacks for <see cref="IBatchDistance{TKey}"/> batch entrypoints.
/// </summary>
/// <remarks>
/// <para>
/// C# 8 default interface methods aren't available across all the supported TFMs
/// (net35..net48 don't ship runtime support), so the fallbacks live here as static
/// helpers. A metric that doesn't have a specialised SIMD path implements
/// <see cref="IBatchDistance{TKey}.FindMinDistanceBatch"/> /
/// <see cref="IBatchDistance{TKey}.FindNClosest"/> by forwarding to these helpers.
/// </para>
/// <para>
/// All helpers preserve the first-occurrence tie-break contract documented on
/// <see cref="IBatchDistance{TKey}"/>.
/// </para>
/// </remarks>
public static class BatchDistanceDefaults {

  /// <summary>
  /// Generic scalar fallback for <see cref="IBatchDistance{TKey}.FindMinDistanceBatch"/>:
  /// loops over references and calls the per-reference
  /// <see cref="IBatchDistance{TKey}.FindMinDistance"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void FindMinDistanceBatchScalar<TKey, TMetric>(
    in TMetric metric,
    ReadOnlySpan<TKey> references,
    ReadOnlySpan<byte> candidates, int candidateCount,
    Span<int> outIndices)
    where TKey : unmanaged, IColorSpace
    where TMetric : struct, IBatchDistance<TKey> {
    for (var i = 0; i < references.Length; ++i) {
      metric.FindMinDistance(references[i], candidates, candidateCount, out var idx);
      outIndices[i] = idx;
    }
  }

  /// <summary>
  /// Generic scalar fallback for <see cref="IBatchDistance{TKey}.FindNClosest"/>:
  /// computes squared distance to every candidate (via a 1-entry
  /// <see cref="IBatchDistance{TKey}.FindMinDistance"/> probe) and maintains a top-k
  /// list with first-occurrence tie-break.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The probe-per-candidate approach lets the helper work for any per-metric byte
  /// layout: it slices the candidate buffer into single-entry views and lets the metric
  /// itself do the per-entry decode. This is correctness-only — metrics that need real
  /// perf override the interface method with a SIMD implementation.
  /// </para>
  /// </remarks>
  public static int FindNClosestScalar<TKey, TMetric>(
    in TMetric metric,
    in TKey reference,
    ReadOnlySpan<byte> candidates, int candidateCount,
    int k,
    Span<int> outIndices, Span<int> outDistances)
    where TKey : unmanaged, IColorSpace
    where TMetric : struct, IBatchDistance<TKey> {

    if (candidateCount <= 0 || k <= 0)
      return 0;
    if (k > candidateCount)
      k = candidateCount;

    // Per-candidate stride: the candidates buffer holds candidateCount entries packed
    // back-to-back; stride = total bytes / count.
    var stride = candidates.Length / candidateCount;

    // Initialize top-k list: indices = -1, distances = MaxValue. Maintain the list
    // sorted ascending by (distance, first-seen index).
    for (var s = 0; s < k; ++s) {
      outIndices[s] = -1;
      outDistances[s] = int.MaxValue;
    }

    for (var i = 0; i < candidateCount; ++i) {
      var slice = candidates.Slice(i * stride, stride);
      var d = metric.FindMinDistance(reference, slice, 1, out _);

      // Insert into sorted top-k list. Strict-less to preserve first-occurrence ties
      // (a candidate with the same distance as an existing entry doesn't displace it).
      if (d >= outDistances[k - 1])
        continue;

      // Find insertion point.
      var pos = k - 1;
      while (pos > 0 && d < outDistances[pos - 1])
        --pos;

      // Shift right.
      for (var s = k - 1; s > pos; --s) {
        outDistances[s] = outDistances[s - 1];
        outIndices[s] = outIndices[s - 1];
      }
      outDistances[pos] = d;
      outIndices[pos] = i;
    }

    return k;
  }
}
