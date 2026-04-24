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

namespace Hawkynt.ColorProcessing.Metrics;

/// <summary>
/// Opt-in batch API for distance metrics that can compute multiple distances per
/// call — typically backed by SIMD intrinsics — and pick the minimum in one shot.
/// </summary>
/// <typeparam name="TKey">The colour type whose distance the metric measures.</typeparam>
/// <remarks>
/// <para>
/// <see cref="PaletteLookup{TWork,TMetric}"/> (and other nearest-neighbour consumers)
/// probe a metric for <see cref="IBatchDistance{TKey}"/> at construction time; when the
/// metric implements it, they group candidates into dense packed buffers and delegate
/// the inner "find closest of N" step to <see cref="FindMinDistance"/>. Metrics that
/// don't implement it keep using the scalar per-candidate <c>Distance</c> path.
/// </para>
/// <para>
/// <b>Packing contract for the <paramref name="candidates"/> span.</b>
/// The bytes are laid out back-to-back in whichever packed native format the metric
/// works in; for 4-byte metrics (e.g. <see cref="EuclideanSquared4B{TKey}"/>) that
/// is one 32-bit little-endian colour per candidate, i.e. 4 bytes per entry, in the
/// same component order as the underlying <typeparamref name="TKey"/> struct's
/// in-memory representation. Squared-difference metrics are order-independent across
/// component lanes so R/G/B/A vs B/G/R/A doesn't matter; other metrics that care
/// about component identity should document their expectation.
/// </para>
/// <para>
/// <b>Ties.</b> Implementors must return the <em>first</em> minimum-distance index
/// encountered on a linear scan (i.e. equivalent to a scalar loop that updates the
/// best only when <c>d &lt; best</c>, never on equal). This keeps the fast path
/// bit-exactly equivalent to the scalar fallback.
/// </para>
/// </remarks>
public interface IBatchDistance<TKey> where TKey : unmanaged, IColorSpace {

  /// <summary>
  /// Finds the index and distance of the candidate in <paramref name="candidates"/>
  /// closest to <paramref name="reference"/>.
  /// </summary>
  /// <param name="reference">The target colour.</param>
  /// <param name="candidates">Candidate colours packed per the per-metric contract
  /// documented on the interface. At least <c><paramref name="count"/> * sizeof(candidate)</c>
  /// bytes must be readable.</param>
  /// <param name="count">Number of candidate colours laid out in
  /// <paramref name="candidates"/>. Must be at least 1.</param>
  /// <param name="minIndex">Receives the zero-based index (within the packed
  /// candidate stream) of the closest candidate, with ties broken to the first
  /// occurrence.</param>
  /// <returns>The closest candidate's squared distance, expressed in the metric's
  /// native raw units (matching the scalar path's pre-normalisation quantity, so
  /// callers can compare/aggregate across batches without converting through
  /// <see cref="UNorm32"/>).</returns>
  int FindMinDistance(in TKey reference, ReadOnlySpan<byte> candidates, int count, out int minIndex);
}
