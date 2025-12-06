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

#if !SUPPORTS_MEMORY && !OFFICIAL_MEMORY

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Buffers;

/// <summary>
/// Provides methods for reading sequential data from a <see cref="ReadOnlySequence{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the elements in the sequence.</typeparam>
public ref struct SequenceReader<T> where T : unmanaged, IEquatable<T> {

  private readonly ReadOnlySequence<T> _sequence;
  private SequencePosition _currentPosition;
  private SequencePosition _nextPosition;
  private ReadOnlySpan<T> _currentSpan;
  private int _currentSpanIndex;
  private long _consumed;

  /// <summary>
  /// Creates a new <see cref="SequenceReader{T}"/> over the given <see cref="ReadOnlySequence{T}"/>.
  /// </summary>
  public SequenceReader(ReadOnlySequence<T> sequence) {
    this._sequence = sequence;
    this._currentPosition = sequence.Start;
    this._nextPosition = sequence.Start;
    this._currentSpan = default;
    this._currentSpanIndex = 0;
    this._consumed = 0;

    if (sequence.TryGet(ref this._nextPosition, out var memory))
      this._currentSpan = memory.Span;
  }

  /// <summary>
  /// Gets the underlying <see cref="ReadOnlySequence{T}"/>.
  /// </summary>
  public readonly ReadOnlySequence<T> Sequence => this._sequence;

  /// <summary>
  /// Gets the current position within the sequence.
  /// </summary>
  public readonly SequencePosition Position => this._sequence.GetPosition(this._currentSpanIndex, this._currentPosition);

  /// <summary>
  /// Gets the total number of elements consumed.
  /// </summary>
  public readonly long Consumed => this._consumed;

  /// <summary>
  /// Gets the number of elements remaining in the sequence.
  /// </summary>
  public readonly long Remaining => this._sequence.Length - this._consumed;

  /// <summary>
  /// Gets a value indicating whether there are no more elements to read.
  /// </summary>
  public readonly bool End => this._consumed >= this._sequence.Length;

  /// <summary>
  /// Gets the current span.
  /// </summary>
  public readonly ReadOnlySpan<T> CurrentSpan => this._currentSpan;

  /// <summary>
  /// Gets the current index within the current span.
  /// </summary>
  public readonly int CurrentSpanIndex => this._currentSpanIndex;

  /// <summary>
  /// Gets the unread portion of the current span.
  /// </summary>
  public readonly ReadOnlySpan<T> UnreadSpan => this._currentSpan.Slice(this._currentSpanIndex, this._currentSpan.Length - this._currentSpanIndex);

  /// <summary>
  /// Tries to peek at the next element without advancing.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly bool TryPeek(out T value) {
    if (this._currentSpanIndex < this._currentSpan.Length) {
      value = this._currentSpan[this._currentSpanIndex];
      return true;
    }

    value = default;
    return false;
  }

  /// <summary>
  /// Tries to read the next element.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool TryRead(out T value) {
    if (this._currentSpanIndex < this._currentSpan.Length) {
      value = this._currentSpan[this._currentSpanIndex];
      ++this._currentSpanIndex;
      ++this._consumed;
      return true;
    }

    return this._TryReadMultiSegment(out value);
  }

  private bool _TryReadMultiSegment(out T value) {
    while (this._sequence.TryGet(ref this._nextPosition, out var memory)) {
      this._currentPosition = this._nextPosition;
      this._currentSpan = memory.Span;
      this._currentSpanIndex = 0;

      if (this._currentSpan.Length > 0) {
        value = this._currentSpan[0];
        ++this._currentSpanIndex;
        ++this._consumed;
        return true;
      }
    }

    value = default;
    return false;
  }

  /// <summary>
  /// Advances the reader by the specified count.
  /// </summary>
  public void Advance(long count) {
    if (count < 0)
      throw new ArgumentOutOfRangeException(nameof(count));

    if (count == 0)
      return;

    var remaining = this._currentSpan.Length - this._currentSpanIndex;
    if (count <= remaining) {
      this._currentSpanIndex += (int)count;
      this._consumed += count;
      return;
    }

    this._consumed += remaining;
    count -= remaining;

    while (this._sequence.TryGet(ref this._nextPosition, out var memory)) {
      this._currentPosition = this._nextPosition;
      this._currentSpan = memory.Span;

      if (count <= this._currentSpan.Length) {
        this._currentSpanIndex = (int)count;
        this._consumed += count;
        return;
      }

      this._consumed += this._currentSpan.Length;
      count -= this._currentSpan.Length;
    }

    this._currentSpanIndex = this._currentSpan.Length;
  }

  /// <summary>
  /// Rewinds the reader by the specified count.
  /// </summary>
  public void Rewind(long count) {
    if (count < 0)
      throw new ArgumentOutOfRangeException(nameof(count));

    if (count == 0)
      return;

    if (count > this._consumed)
      throw new ArgumentOutOfRangeException(nameof(count));

    this._consumed -= count;

    if (count <= this._currentSpanIndex) {
      this._currentSpanIndex -= (int)count;
      return;
    }

    // Need to recalculate position from start
    var newConsumed = this._consumed;
    this._currentPosition = this._sequence.Start;
    this._nextPosition = this._sequence.Start;
    this._currentSpanIndex = 0;
    this._consumed = 0;

    if (this._sequence.TryGet(ref this._nextPosition, out var memory))
      this._currentSpan = memory.Span;

    if (newConsumed > 0)
      this.Advance(newConsumed);
  }

  /// <summary>
  /// Tries to copy the specified count of elements to the destination.
  /// </summary>
  public bool TryCopyTo(Span<T> destination) {
    var unread = this.UnreadSpan;
    if (unread.Length >= destination.Length) {
      unread.Slice(0, destination.Length).CopyTo(destination);
      return true;
    }

    return this._TryCopyToMultiSegment(destination);
  }

  private bool _TryCopyToMultiSegment(Span<T> destination) {
    if (this.Remaining < destination.Length)
      return false;

    var unread = this.UnreadSpan;
    unread.CopyTo(destination);
    var copied = unread.Length;

    var nextPosition = this._nextPosition;
    while (this._sequence.TryGet(ref nextPosition, out var memory)) {
      var span = memory.Span;
      var toCopy = Math.Min(span.Length, destination.Length - copied);
      span.Slice(0, toCopy).CopyTo(destination.Slice(copied, destination.Length - copied));
      copied += toCopy;

      if (copied >= destination.Length)
        return true;
    }

    return false;
  }

  /// <summary>
  /// Tries to read an exact number of elements.
  /// </summary>
  public bool TryReadExact(int count, out ReadOnlySequence<T> sequence) {
    if (count < 0)
      throw new ArgumentOutOfRangeException(nameof(count));

    if (this.Remaining < count) {
      sequence = default;
      return false;
    }

    var start = this.Position;
    this.Advance(count);
    sequence = this._sequence.Slice(start, this.Position);
    return true;
  }
}

#endif
