#nullable enable

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
//

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Collections;

// Convention: the bit-order strategy is ALWAYS the final type parameter.
//   PackedBuffer<T, TBitOrder>          — interface codec + zero-cost order
//   PackedBuffer<T, TCodec, TBitOrder>  — value-type codec + zero-cost order (fully inlinable)

/// <summary>
/// A typed view over a <see cref="PackedBitBuffer{TBitOrder}"/> using an <see cref="IBitCodec{T}"/> to map
/// codes to values. The codec is held as an interface reference (works on every target framework); the bit
/// order is a zero-cost type parameter. For hot loops, prefer <see cref="PackedBuffer{T,TCodec,TBitOrder}"/>.
/// </summary>
/// <typeparam name="T">The logical value type.</typeparam>
/// <typeparam name="TBitOrder">The bit layout strategy (<see cref="LsbFirst"/> or <see cref="MsbFirst"/>).</typeparam>
/// <remarks>
/// There is no <c>Span&lt;T&gt;</c> over the packed storage (sub-byte elements are not addressable); use the
/// indexer for single values and <see cref="DecodeTo"/>/<see cref="EncodeFrom"/> for bulk transfer.
/// </remarks>
public sealed class PackedBuffer<T, TBitOrder> where TBitOrder : struct, IBitOrder {
  private readonly PackedBitBuffer<TBitOrder> _buffer;
  private readonly IBitCodec<T> _codec;

  /// <summary>
  /// Creates an all-zero buffer for <paramref name="count"/> values using the given codec.
  /// </summary>
  public PackedBuffer(int count, IBitCodec<T> codec) {
    this._codec = codec ?? throw new ArgumentNullException(nameof(codec));
    this._buffer = new(count, codec.BitWidth);
  }

  /// <summary>
  /// Wraps an existing <see cref="PackedBitBuffer{TBitOrder}"/> with a codec of matching width.
  /// </summary>
  public PackedBuffer(PackedBitBuffer<TBitOrder> buffer, IBitCodec<T> codec) {
    ArgumentNullException.ThrowIfNull(buffer);
    ArgumentNullException.ThrowIfNull(codec);
    if (buffer.BitsPerElement != codec.BitWidth)
      throw new ArgumentException($"Codec width {codec.BitWidth} does not match buffer width {buffer.BitsPerElement}.", nameof(codec));
    this._buffer = buffer;
    this._codec = codec;
  }

  /// <summary>
  /// Gets the number of values stored.
  /// </summary>
  public int Count => this._buffer.Count;

  /// <summary>
  /// Gets the underlying packed storage (for interop or serialization).
  /// </summary>
  public PackedBitBuffer<TBitOrder> Storage => this._buffer;

  /// <summary>
  /// Gets or sets the value at the given index.
  /// </summary>
  public T this[int index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._codec.Decode(this._buffer.GetBits(index));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set => this._buffer.SetBits(index, this._codec.Encode(value));
  }

  /// <summary>
  /// Decodes all values into <paramref name="destination"/>.
  /// </summary>
  public void DecodeTo(Span<T> destination) {
    var n = this._buffer.Count;
    ArgumentOutOfRangeException.ThrowIfLessThan(destination.Length, n, nameof(destination));
    for (var i = 0; i < n; ++i)
      destination[i] = this._codec.Decode(this._buffer.GetBits(i));
  }

  /// <summary>
  /// Encodes values from <paramref name="source"/> (up to <see cref="Count"/>) into the buffer.
  /// </summary>
  public void EncodeFrom(ReadOnlySpan<T> source) {
    var n = Math.Min(source.Length, this._buffer.Count);
    for (var i = 0; i < n; ++i)
      this._buffer.SetBits(i, this._codec.Encode(source[i]));
  }

  /// <summary>
  /// Returns all decoded values as a new array.
  /// </summary>
  public T[] ToArray() {
    var result = new T[this._buffer.Count];
    for (var i = 0; i < result.Length; ++i)
      result[i] = this._codec.Decode(this._buffer.GetBits(i));
    return result;
  }

  /// <summary>
  /// Enumerates the decoded values.
  /// </summary>
  public IEnumerator<T> GetEnumerator() {
    var n = this._buffer.Count;
    for (var i = 0; i < n; ++i)
      yield return this._codec.Decode(this._buffer.GetBits(i));
  }
}

/// <summary>
/// A fully zero-cost typed view: both the codec and the bit order are value-type parameters, so the JIT can
/// inline <see cref="IBitCodec{T}.Decode"/>/<see cref="IBitCodec{T}.Encode"/> and the bit read/write logic
/// (no virtual calls, no order branch on modern runtimes).
/// </summary>
/// <typeparam name="T">The logical value type.</typeparam>
/// <typeparam name="TCodec">The value-type codec.</typeparam>
/// <typeparam name="TBitOrder">The bit layout strategy (<see cref="LsbFirst"/> or <see cref="MsbFirst"/>).</typeparam>
public sealed class PackedBuffer<T, TCodec, TBitOrder>
  where TCodec : struct, IBitCodec<T>
  where TBitOrder : struct, IBitOrder {
  private readonly PackedBitBuffer<TBitOrder> _buffer;
  private readonly TCodec _codec;

  /// <summary>
  /// Creates an all-zero buffer for <paramref name="count"/> values using the given codec.
  /// </summary>
  public PackedBuffer(int count, TCodec codec = default) {
    this._codec = codec;
    this._buffer = new(count, codec.BitWidth);
  }

  /// <summary>
  /// Wraps an existing <see cref="PackedBitBuffer{TBitOrder}"/> with a codec of matching width.
  /// </summary>
  public PackedBuffer(PackedBitBuffer<TBitOrder> buffer, TCodec codec = default) {
    ArgumentNullException.ThrowIfNull(buffer);
    if (buffer.BitsPerElement != codec.BitWidth)
      throw new ArgumentException($"Codec width {codec.BitWidth} does not match buffer width {buffer.BitsPerElement}.", nameof(buffer));
    this._buffer = buffer;
    this._codec = codec;
  }

  /// <summary>
  /// Gets the number of values stored.
  /// </summary>
  public int Count => this._buffer.Count;

  /// <summary>
  /// Gets the underlying packed storage (for interop or serialization).
  /// </summary>
  public PackedBitBuffer<TBitOrder> Storage => this._buffer;

  /// <summary>
  /// Gets or sets the value at the given index.
  /// </summary>
  public T this[int index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._codec.Decode(this._buffer.GetBits(index));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set => this._buffer.SetBits(index, this._codec.Encode(value));
  }

  /// <summary>
  /// Decodes all values into <paramref name="destination"/>.
  /// </summary>
  public void DecodeTo(Span<T> destination) {
    var n = this._buffer.Count;
    ArgumentOutOfRangeException.ThrowIfLessThan(destination.Length, n, nameof(destination));
    for (var i = 0; i < n; ++i)
      destination[i] = this._codec.Decode(this._buffer.GetBits(i));
  }

  /// <summary>
  /// Encodes values from <paramref name="source"/> (up to <see cref="Count"/>) into the buffer.
  /// </summary>
  public void EncodeFrom(ReadOnlySpan<T> source) {
    var n = Math.Min(source.Length, this._buffer.Count);
    for (var i = 0; i < n; ++i)
      this._buffer.SetBits(i, this._codec.Encode(source[i]));
  }

  /// <summary>
  /// Returns all decoded values as a new array.
  /// </summary>
  public T[] ToArray() {
    var result = new T[this._buffer.Count];
    for (var i = 0; i < result.Length; ++i)
      result[i] = this._codec.Decode(this._buffer.GetBits(i));
    return result;
  }

  /// <summary>
  /// Enumerates the decoded values.
  /// </summary>
  public IEnumerator<T> GetEnumerator() {
    var n = this._buffer.Count;
    for (var i = 0; i < n; ++i)
      yield return this._codec.Decode(this._buffer.GetBits(i));
  }
}
