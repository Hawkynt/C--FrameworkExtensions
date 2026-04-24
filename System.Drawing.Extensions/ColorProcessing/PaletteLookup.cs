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
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing;

/// <summary>
/// Provides nearest-neighbor lookup for a color palette with optional caching.
/// </summary>
/// <typeparam name="TWork">The working color space type.</typeparam>
/// <typeparam name="TMetric">The color distance metric type.</typeparam>
/// <remarks>
/// <para>Wraps a palette array and metric to provide efficient nearest-color lookup.</para>
/// <para>Uses a color cube acceleration structure for palettes with more than 8 entries,
/// reducing per-lookup cost from O(palette) to O(candidates) where candidates is typically 1-5.</para>
/// <para>Use <see cref="FindNearest(in TWork)"/> to get the index of the closest palette color.</para>
/// <para><b>Batch-distance opt-in.</b> If <typeparamref name="TMetric"/> implements
/// <see cref="IBatchDistance{TKey}"/> the lookup pre-packs each cube cell's
/// candidates into a dense byte buffer at construction time and delegates the
/// inner "find closest of N" step to the metric's batch method — which typically
/// fans out to a SIMD kernel (e.g. SSE2 for <see cref="EuclideanSquared4B{T}"/>).
/// Discovery is a single <c>is</c>-cast at construction, so per-pixel dispatch
/// is free and the scalar fallback is automatic for metrics that opt out. Metrics
/// that don't opt in go through the per-candidate scalar <c>Distance</c> loop
/// exactly as before.</para>
/// </remarks>
public readonly struct PaletteLookup<TWork, TMetric>
  where TWork : unmanaged, IColorSpace4<TWork>
  where TMetric : struct, IColorMetric<TWork> {

  private readonly TWork[] _palette;
  private readonly TMetric _metric;
  private readonly Dictionary<TWork, int> _cache;
  private readonly byte[][]? _cube;
  // Non-null iff the metric opted into IBatchDistance<TWork>. The cast is done
  // once at construction so the per-pixel hot path can do a cheap null-check.
  // Struct metrics box exactly once per PaletteLookup instance lifetime.
  private readonly IBatchDistance<TWork>? _batchMetric;
  // Per-cube-cell dense packed view of the cell's candidate colours — laid out
  // back-to-back at sizeof(TWork) bytes per entry — so the batch kernel reads
  // linearly and doesn't need a gather indirection. Null unless
  // <see cref="_batchMetric"/> is non-null (and consequently unless there's a
  // cube at all). Each inner array is parallel to the index array at the same
  // <c>_cube</c> slot: <c>_cube[cell][localIdx]</c> is the palette index, and
  // <c>_cubePackedBytes[cell][localIdx * sizeof(TWork) .. +sizeof(TWork)]</c>
  // holds the same palette colour's raw bytes.
  private readonly byte[][]? _cubePackedBytes;

  private const int _CUBE_BITS = 4;
  private const int _CUBE_SIZE = 1 << _CUBE_BITS;
  private const int _CUBE_SHIFT = 32 - _CUBE_BITS;
  private const int _CUBE_CELLS = _CUBE_SIZE * _CUBE_SIZE * _CUBE_SIZE;
  private const int _CUBE_MIN_PALETTE = 9;

  [ThreadStatic]
  private static TWork[]? _cachedPalette;
  [ThreadStatic]
  private static byte[][]? _cachedCube;

  /// <summary>
  /// Creates a new palette lookup with optional caching.
  /// </summary>
  /// <param name="palette">The palette colors in TWork space.</param>
  /// <param name="metric">The distance metric.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public PaletteLookup(TWork[] palette, TMetric metric) {
    this._palette = palette;
    this._metric = metric;
    // Pre-populate cache with palette colors for O(1) exact match lookups
    this._cache = new(palette.Length);
    for (var i = 0; i < palette.Length; ++i)
      this._cache[palette[i]] = i;

    this._cube = palette.Length is > _CUBE_MIN_PALETTE and <= 256
      ? _GetOrBuildCube(palette, metric)
      : null;

    // One-time box-via-interface: struct metrics that implement IBatchDistance<TWork>
    // surface their SIMD-capable FindMinDistance here. Null otherwise -> scalar path.
    this._batchMetric = metric as IBatchDistance<TWork>;

    // Pre-pack each cube cell's candidates into a contiguous byte buffer the
    // batch kernel can read linearly. Only built when we actually have a batch
    // metric AND a cube; both together describe the fast path.
    this._cubePackedBytes = this._batchMetric != null && this._cube != null
      ? _BuildCubePackedBytes(this._cube, palette)
      : null;
  }

  private static unsafe byte[][] _BuildCubePackedBytes(byte[][] cube, TWork[] palette) {
    var packed = new byte[cube.Length][];
    var elementSize = sizeof(TWork);
    for (var cell = 0; cell < cube.Length; ++cell) {
      var candidates = cube[cell];
      var buf = new byte[candidates.Length * elementSize];
      fixed (byte* dstBase = buf) {
        for (var i = 0; i < candidates.Length; ++i) {
          fixed (TWork* srcBase = &palette[candidates[i]]) {
            var srcBytes = (byte*)srcBase;
            var dst = dstBase + i * elementSize;
            for (var b = 0; b < elementSize; ++b)
              dst[b] = srcBytes[b];
          }
        }
      }
      packed[cell] = buf;
    }
    return packed;
  }

  /// <summary>
  /// Gets the palette color at the specified index.
  /// </summary>
  public TWork this[int index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._palette[index];
  }

  /// <summary>
  /// Gets the number of colors in the palette.
  /// </summary>
  public int Count {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._palette.Length;
  }

  /// <summary>
  /// Finds the index of the nearest palette color.
  /// </summary>
  /// <param name="color">The color to match.</param>
  /// <returns>The zero-based index of the closest palette color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int FindNearest(in TWork color) {
    if (this._cache.TryGetValue(color, out var cached))
      return cached;

    var nearestIdx = this._FindNearestUncached(color);
    this._cache.TryAdd(color, nearestIdx);
    return nearestIdx;
  }

  /// <summary>
  /// Finds the index of the nearest palette color without consulting or updating the
  /// internal dictionary cache. Intended for hot paths where the input colour is
  /// effectively unique per call (e.g. the per-pixel adjusted colour inside error
  /// diffusion) so the cache adds overhead instead of amortising work.
  /// </summary>
  /// <param name="color">The color to match.</param>
  /// <returns>The zero-based index of the closest palette color.</returns>
  /// <remarks>
  /// Returns the same answer as <see cref="FindNearest(in TWork)"/> — the cube and
  /// linear-scan tie-break rules are identical. The cache is pure memoization.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int FindNearestNoCache(in TWork color) => this._FindNearestUncached(color);

  /// <summary>
  /// Finds the nearest palette color without cache interaction and returns both
  /// index and colour. Companion to <see cref="FindNearestNoCache(in TWork)"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int FindNearestNoCache(in TWork color, out TWork nearestColor) {
    var idx = this._FindNearestUncached(color);
    nearestColor = this._palette[idx];
    return idx;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private int _FindNearestUncached(in TWork color) {
    var palette = this._palette;
    var metric = this._metric;
    int nearestIdx;

    var cube = this._cube;
    if (cube != null) {
      var (c1, c2, c3, _) = color.ToNormalized();
      var i1 = c1.RawValue >> _CUBE_SHIFT;
      var i2 = c2.RawValue >> _CUBE_SHIFT;
      var i3 = c3.RawValue >> _CUBE_SHIFT;
      if (i1 >= _CUBE_SIZE) i1 = _CUBE_SIZE - 1;
      if (i2 >= _CUBE_SIZE) i2 = _CUBE_SIZE - 1;
      if (i3 >= _CUBE_SIZE) i3 = _CUBE_SIZE - 1;

      var cellIdx = (i1 << (_CUBE_BITS + _CUBE_BITS)) | (i2 << _CUBE_BITS) | i3;
      var candidates = cube[cellIdx];

      // Batch fast path: the metric knows how to find the min-distance candidate
      // in a packed byte buffer in one shot (typically SIMD). Pre-built per-cell
      // packed buffers avoid a per-call gather and make the kernel's reads
      // strictly linear. Cell-local index maps back to palette index via the
      // existing candidate array. Falls through to the scalar loop when either
      // the metric didn't opt into IBatchDistance or candidates is too short for
      // the batch path to be worthwhile (the batch method's own threshold — we
      // keep the call to defer the decision to the metric implementation).
      var batch = this._batchMetric;
      if (batch != null) {
        var packed = this._cubePackedBytes![cellIdx];
        batch.FindMinDistance(color, packed, candidates.Length, out var localIdx);
        return candidates[localIdx];
      }

      var minDist = UNorm32.One;
      nearestIdx = candidates[0];

      for (var i = 0; i < candidates.Length; ++i) {
        var ci = candidates[i];
        var dist = metric.Distance(color, palette[ci]);
        if (dist >= minDist)
          continue;

        minDist = dist;
        nearestIdx = ci;

        if (dist == UNorm32.Zero)
          break;
      }
    } else {
      var minDist = UNorm32.One;
      nearestIdx = 0;

      for (var i = 0; i < palette.Length; ++i) {
        var dist = metric.Distance(color, palette[i]);
        if (dist >= minDist)
          continue;

        minDist = dist;
        nearestIdx = i;

        if (dist == UNorm32.Zero)
          break;
      }
    }

    return nearestIdx;
  }

  /// <summary>
  /// Finds the nearest palette color.
  /// </summary>
  /// <param name="color">The color to match.</param>
  /// <returns>The closest palette color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork FindNearestColor(in TWork color) => this._palette[this.FindNearest(color)];

  /// <summary>
  /// Finds the nearest palette color and returns both index and color.
  /// </summary>
  /// <param name="color">The color to match.</param>
  /// <param name="nearestColor">The closest palette color.</param>
  /// <returns>The zero-based index of the closest palette color.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int FindNearest(in TWork color, out TWork nearestColor) {
    var idx = this.FindNearest(color);
    nearestColor = this._palette[idx];
    return idx;
  }

  #region Color Cube Acceleration

  private static byte[][] _GetOrBuildCube(TWork[] palette, TMetric metric) {
    if (ReferenceEquals(_cachedPalette, palette) && _cachedCube != null)
      return _cachedCube;

    var cube = _BuildCube(palette, metric);
    _cachedPalette = palette;
    _cachedCube = cube;
    return cube;
  }

  private static byte[][] _BuildCube(TWork[] palette, TMetric metric) {
    var cube = new byte[_CUBE_CELLS][];
    var distances = new UNorm32[palette.Length];

    // Compute max alpha contribution for conservative candidate bounds;
    // for opaque-only palettes this is zero and has no effect
    var maxAlphaContrib = metric.Distance(
      ColorFactory.FromNormalized_4<TWork>(UNorm32.Half, UNorm32.Half, UNorm32.Half, UNorm32.Zero),
      ColorFactory.FromNormalized_4<TWork>(UNorm32.Half, UNorm32.Half, UNorm32.Half, UNorm32.One)
    );

    for (var c1 = 0; c1 < _CUBE_SIZE; ++c1)
    for (var c2 = 0; c2 < _CUBE_SIZE; ++c2)
    for (var c3 = 0; c3 < _CUBE_SIZE; ++c3) {
      var uc1 = (uint)c1;
      var uc2 = (uint)c2;
      var uc3 = (uint)c3;

      var center = ColorFactory.FromNormalized_4<TWork>(
        UNorm32.FromRaw((uc1 << _CUBE_SHIFT) | (1u << (_CUBE_SHIFT - 1))),
        UNorm32.FromRaw((uc2 << _CUBE_SHIFT) | (1u << (_CUBE_SHIFT - 1))),
        UNorm32.FromRaw((uc3 << _CUBE_SHIFT) | (1u << (_CUBE_SHIFT - 1))),
        UNorm32.One
      );

      var diagonal = metric.Distance(
        ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromRaw(uc1 << _CUBE_SHIFT),
          UNorm32.FromRaw(uc2 << _CUBE_SHIFT),
          UNorm32.FromRaw(uc3 << _CUBE_SHIFT),
          UNorm32.One
        ),
        ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromRaw(((uc1 + 1u) << _CUBE_SHIFT) - 1u),
          UNorm32.FromRaw(((uc2 + 1u) << _CUBE_SHIFT) - 1u),
          UNorm32.FromRaw(((uc3 + 1u) << _CUBE_SHIFT) - 1u),
          UNorm32.One
        )
      );

      // Find distances from center to all palette colors and track the minimum
      var minDist = UNorm32.One;
      for (var i = 0; i < palette.Length; ++i) {
        distances[i] = metric.Distance(center, palette[i]);
        if (distances[i] < minDist)
          minDist = distances[i];
      }

      // Any palette color within D_min + 2*diagonal + 2*alphaContrib could be
      // the nearest for some point in this cell (triangle inequality bound)
      var threshold = (ulong)minDist.RawValue + 2UL * diagonal.RawValue + 2UL * maxAlphaContrib.RawValue;
      var thresholdClamped = threshold > uint.MaxValue ? uint.MaxValue : (uint)threshold;

      var count = 0;
      for (var i = 0; i < palette.Length; ++i)
        if (distances[i].RawValue <= thresholdClamped)
          ++count;

      var candidates = new byte[count];
      var idx = 0;
      for (var i = 0; i < palette.Length; ++i)
        if (distances[i].RawValue <= thresholdClamped)
          candidates[idx++] = (byte)i;

      cube[(c1 << (_CUBE_BITS + _CUBE_BITS)) | (c2 << _CUBE_BITS) | c3] = candidates;
    }

    return cube;
  }

  #endregion
}
