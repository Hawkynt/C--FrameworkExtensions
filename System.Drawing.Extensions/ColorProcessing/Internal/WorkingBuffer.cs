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
#if SUPPORTS_ARRAYPOOL
using System.Buffers;
#endif
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Internal;

/// <summary>
/// RAII-shaped buffer rental for a <typeparamref name="TWork"/>[] that callers use as the
/// destination of a batched decode. Returns to <see cref="ArrayPool{T}.Shared"/> on
/// <see cref="Dispose"/> when the runtime supports it; falls back to plain
/// <c>new TWork[size]</c> on legacy TFMs (net35/40/45) where the type isn't available.
/// </summary>
/// <typeparam name="TWork">The element type (matches the working colour space).</typeparam>
/// <remarks>
/// <para>
/// Typical use pattern:
/// </para>
/// <code>
/// using var work = WorkingBuffer&lt;LinearRgbaF&gt;.Rent(width * stripeRows);
/// batch.DecodeBatch(srcSpan, work.AsSpan(0, width * stripeRows));
/// // ... loop over work[i] in inner kernel ...
/// </code>
/// <para>
/// The rented array may be larger than the requested size (ArrayPool semantics). Always
/// slice via <see cref="AsSpan(int,int)"/> with the exact length you intend to use; do not
/// read past <see cref="Length"/>. The buffer is NOT zeroed on rent — callers writing only
/// part of the span MUST treat the rest as garbage.
/// </para>
/// </remarks>
internal readonly struct WorkingBuffer<TWork> : IDisposable
  where TWork : unmanaged {

  private readonly TWork[] _array;
  private readonly int _length;
  private readonly bool _pooled;

  /// <summary>The exact number of elements requested at <see cref="Rent"/> time.</summary>
  public int Length {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._length;
  }

  /// <summary>The underlying array (may be larger than <see cref="Length"/> when pooled).</summary>
  public TWork[] Array {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._array;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private WorkingBuffer(TWork[] array, int length, bool pooled) {
    this._array = array;
    this._length = length;
    this._pooled = pooled;
  }

  /// <summary>Rents a buffer with at least <paramref name="length"/> elements.</summary>
  /// <param name="length">The number of elements required. Must be non-negative.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static WorkingBuffer<TWork> Rent(int length) {
#if SUPPORTS_ARRAYPOOL
    var array = ArrayPool<TWork>.Shared.Rent(length);
    return new WorkingBuffer<TWork>(array, length, pooled: true);
#else
    var array = new TWork[length];
    return new WorkingBuffer<TWork>(array, length, pooled: false);
#endif
  }

  /// <summary>Returns a span over the first <see cref="Length"/> elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Span<TWork> AsSpan() => new(this._array, 0, this._length);

  /// <summary>Returns a span over <paramref name="count"/> elements starting at <paramref name="start"/>.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Span<TWork> AsSpan(int start, int count) => new(this._array, start, count);

  /// <summary>Index access (no bounds check beyond array bounds).</summary>
  public ref TWork this[int index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => ref this._array[index];
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Dispose() {
#if SUPPORTS_ARRAYPOOL
    if (this._pooled && this._array != null)
      ArrayPool<TWork>.Shared.Return(this._array, clearArray: false);
#else
    // No pool on legacy TFMs; rely on GC to reclaim.
    _ = this._pooled;
#endif
  }
}
