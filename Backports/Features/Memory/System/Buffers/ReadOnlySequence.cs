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

using Guard;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Buffers;

/// <summary>
/// Represents a sequence that can read a sequential series of <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the elements in the sequence.</typeparam>
public readonly struct ReadOnlySequence<T> {

  private readonly SequencePosition _start;
  private readonly SequencePosition _end;

  /// <summary>
  /// Creates a <see cref="ReadOnlySequence{T}"/> from a <see cref="ReadOnlyMemory{T}"/>.
  /// </summary>
  public ReadOnlySequence(ReadOnlyMemory<T> memory) {
    var segment = new SingleSegment(memory);
    this._start = new(segment, 0);
    this._end = new(segment, memory.Length);
  }

  /// <summary>
  /// Creates a <see cref="ReadOnlySequence{T}"/> from an array.
  /// </summary>
  public ReadOnlySequence(T[] array) : this(new ReadOnlyMemory<T>(array)) { }

  /// <summary>
  /// Creates a <see cref="ReadOnlySequence{T}"/> from an array with start and length.
  /// </summary>
  public ReadOnlySequence(T[] array, int start, int length) : this(new ReadOnlyMemory<T>(array, start, length)) { }

  /// <summary>
  /// Creates a <see cref="ReadOnlySequence{T}"/> from linked memory segments.
  /// </summary>
  public ReadOnlySequence(ReadOnlySequenceSegment<T> startSegment, int startIndex, ReadOnlySequenceSegment<T> endSegment, int endIndex) {
    ArgumentNullException.ThrowIfNull(startSegment);
    ArgumentNullException.ThrowIfNull(endSegment);
    
    this._start = new(startSegment, startIndex);
    this._end = new(endSegment, endIndex);
  }

  /// <summary>
  /// Returns an empty <see cref="ReadOnlySequence{T}"/>.
  /// </summary>
  public static ReadOnlySequence<T> Empty => default;

  /// <summary>
  /// Gets the position at the start of the sequence.
  /// </summary>
  public SequencePosition Start => this._start;

  /// <summary>
  /// Gets the position at the end of the sequence.
  /// </summary>
  public SequencePosition End => this._end;

  /// <summary>
  /// Gets the length of the sequence.
  /// </summary>
  public long Length {
    get {
      var startObj = this._start.GetObject();
      if (startObj == null)
        return 0;

      if (startObj == this._end.GetObject())
        return this._end.GetInteger() - this._start.GetInteger();

      if (startObj is ReadOnlySequenceSegment<T> startSegment) {
        var endSegment = (ReadOnlySequenceSegment<T>)this._end.GetObject();
        return (endSegment.RunningIndex + this._end.GetInteger()) - (startSegment.RunningIndex + this._start.GetInteger());
      }

      return this._end.GetInteger() - this._start.GetInteger();
    }
  }

  /// <summary>
  /// Gets a value indicating whether the sequence is empty.
  /// </summary>
  public bool IsEmpty => this.Length == 0;

  /// <summary>
  /// Gets a value indicating whether the sequence has a single segment.
  /// </summary>
  public bool IsSingleSegment => this._start.GetObject() == this._end.GetObject();

  /// <summary>
  /// Gets the first segment of the sequence.
  /// </summary>
  public ReadOnlyMemory<T> First {
    get {
      var startObj = this._start.GetObject();
      if (startObj == null)
        return default;

      var startIndex = this._start.GetInteger();

      if (startObj is ReadOnlySequenceSegment<T> segment) {
        var memory = segment.Memory;
        if (this._start.GetObject() == this._end.GetObject())
          return memory.Slice(startIndex, this._end.GetInteger() - startIndex);
        return memory.Slice(startIndex);
      }

      if (startObj is SingleSegment single) {
        var memory = single.Memory;
        if (this._start.GetObject() == this._end.GetObject())
          return memory.Slice(startIndex, this._end.GetInteger() - startIndex);
        return memory.Slice(startIndex);
      }

      return default;
    }
  }

  /// <summary>
  /// Gets the first span of the sequence.
  /// </summary>
  public ReadOnlySpan<T> FirstSpan => this.First.Span;

  /// <summary>
  /// Forms a slice out of the sequence.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ReadOnlySequence<T> Slice(long start, long length) => this.Slice(this.GetPosition(start), this.GetPosition(start + length));

  /// <summary>
  /// Forms a slice out of the sequence.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ReadOnlySequence<T> Slice(long start) => this.Slice(this.GetPosition(start), this._end);

  /// <summary>
  /// Forms a slice out of the sequence.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ReadOnlySequence<T> Slice(SequencePosition start, SequencePosition end) {
    var result = new ReadOnlySequence<T>();
    Unsafe.AsRef(in result._start) = start;
    Unsafe.AsRef(in result._end) = end;
    return result;
  }

  /// <summary>
  /// Forms a slice out of the sequence.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ReadOnlySequence<T> Slice(SequencePosition start) => this.Slice(start, this._end);

  /// <summary>
  /// Forms a slice out of the sequence.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ReadOnlySequence<T> Slice(int start, int length) => this.Slice((long)start, length);

  /// <summary>
  /// Forms a slice out of the sequence.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ReadOnlySequence<T> Slice(int start) => this.Slice((long)start);

  /// <summary>
  /// Gets a position at the specified offset.
  /// </summary>
  public SequencePosition GetPosition(long offset) => this.GetPosition(offset, this._start);

  /// <summary>
  /// Gets a position at the specified offset from the origin.
  /// </summary>
  public SequencePosition GetPosition(long offset, SequencePosition origin) {
    if (offset < 0)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(offset));

    var obj = origin.GetObject();
    var index = origin.GetInteger();

    if (obj is ReadOnlySequenceSegment<T> segment) {
      while (segment != null) {
        var length = segment.Memory.Length - index;
        if (offset < length)
          return new(segment, index + (int)offset);

        offset -= length;
        segment = segment.Next;
        index = 0;
      }

      if (offset == 0)
        return this._end;

      AlwaysThrow.ArgumentOutOfRangeException(nameof(offset));
    }

    if (obj is SingleSegment single) {
      var available = single.Memory.Length - index;
      if (offset <= available)
        return new(single, index + (int)offset);

      AlwaysThrow.ArgumentOutOfRangeException(nameof(offset));
    }

    if (offset == 0)
      return origin;

    AlwaysThrow.ArgumentOutOfRangeException(nameof(offset));
    return default;
  }

  /// <summary>
  /// Tries to get the data from a single segment.
  /// </summary>
  public bool TryGet(ref SequencePosition position, out ReadOnlyMemory<T> memory, bool advance = true) {
    var obj = position.GetObject();

    if (obj == null || position.Equals(this._end)) {
      memory = default;
      return false;
    }

    if (obj is ReadOnlySequenceSegment<T> segment) {
      var index = position.GetInteger();
      memory = obj == this._end.GetObject()
        ? segment.Memory.Slice(index, this._end.GetInteger() - index)
        : segment.Memory.Slice(index);

      if (advance)
        position = segment.Next != null ? new(segment.Next, 0) : this._end;

      return true;
    }

    if (obj is SingleSegment single) {
      var index = position.GetInteger();
      memory = single.Memory.Slice(index, this._end.GetInteger() - index);
      if (advance)
        position = this._end;
      return true;
    }

    memory = default;
    return false;
  }

  /// <summary>
  /// Copies the sequence to an array.
  /// </summary>
  public void CopyTo(Span<T> destination) {
    if (this.IsSingleSegment) {
      this.FirstSpan.CopyTo(destination);
      return;
    }

    var position = this._start;
    while (this.TryGet(ref position, out var memory))
      memory.Span.CopyTo(destination.Slice(0, memory.Length));
  }

  /// <summary>
  /// Creates an array from the sequence.
  /// </summary>
  public T[] ToArray() {
    var result = new T[this.Length];
    this.CopyTo(result);
    return result;
  }

  /// <summary>
  /// Returns an enumerator for this sequence.
  /// </summary>
  public Enumerator GetEnumerator() => new(this);

  /// <summary>
  /// Internal segment wrapper for single-segment sequences.
  /// </summary>
  private sealed class SingleSegment : ReadOnlySequenceSegment<T> {
    public SingleSegment(ReadOnlyMemory<T> memory) {
      this.Memory = memory;
      this.RunningIndex = 0;
    }
  }

  /// <summary>
  /// Enumerator for iterating over segments.
  /// </summary>
  public struct Enumerator {
    private readonly ReadOnlySequence<T> _sequence;
    private SequencePosition _position;
    private ReadOnlyMemory<T> _current;

    internal Enumerator(ReadOnlySequence<T> sequence) {
      this._sequence = sequence;
      this._position = sequence.Start;
      this._current = default;
    }

    /// <summary>
    /// Gets the current segment.
    /// </summary>
    public ReadOnlyMemory<T> Current => this._current;

    /// <summary>
    /// Moves to the next segment.
    /// </summary>
    public bool MoveNext() => this._sequence.TryGet(ref this._position, out this._current);
  }
}

#endif
