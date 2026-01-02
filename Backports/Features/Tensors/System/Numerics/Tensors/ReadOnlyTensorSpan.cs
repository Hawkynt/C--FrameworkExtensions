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

#if !OFFICIAL_TENSORS

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Numerics.Tensors;

/// <summary>
/// Represents a read-only view over a contiguous region of arbitrary memory that can be interpreted as a tensor.
/// </summary>
/// <typeparam name="T">The type of items in the tensor span.</typeparam>
public readonly ref struct ReadOnlyTensorSpan<T> {

  private readonly ReadOnlySpan<T> _span;
  private readonly nint[] _lengths;
  private readonly nint[] _strides;
  private readonly bool _isPinned;

  /// <summary>
  /// Creates a new <see cref="ReadOnlyTensorSpan{T}"/> from the given span and shape.
  /// </summary>
  /// <param name="span">The underlying data span.</param>
  /// <param name="lengths">The lengths of each dimension.</param>
  /// <param name="strides">The strides for each dimension (optional).</param>
  public ReadOnlyTensorSpan(ReadOnlySpan<T> span, scoped ReadOnlySpan<nint> lengths, scoped ReadOnlySpan<nint> strides = default) {
    this._span = span;
    this._lengths = lengths.ToArray();
    this._strides = strides.IsEmpty ? _ComputeStrides(lengths) : strides.ToArray();
    this._isPinned = false;

    var expectedLength = _ComputeFlatLength(lengths);
    if (span.Length < expectedLength)
      throw new ArgumentException($"Span length {span.Length} is smaller than required length {expectedLength} for the given shape.");
  }

  /// <summary>
  /// Creates a new <see cref="ReadOnlyTensorSpan{T}"/> from the given array.
  /// </summary>
  /// <param name="array">The underlying data array.</param>
  public ReadOnlyTensorSpan(T[]? array)
    : this(array.AsSpan(), array == null ? [] : [array.Length]) { }

  /// <summary>
  /// Creates a new <see cref="ReadOnlyTensorSpan{T}"/> from the given array with specified lengths.
  /// </summary>
  /// <param name="array">The underlying data array.</param>
  /// <param name="lengths">The lengths of each dimension.</param>
  public ReadOnlyTensorSpan(T[]? array, scoped ReadOnlySpan<nint> lengths)
    : this(array.AsSpan(), lengths) { }

  /// <summary>
  /// Creates a new <see cref="ReadOnlyTensorSpan{T}"/> from the given array with specified start, lengths, and strides.
  /// </summary>
  /// <param name="array">The underlying data array.</param>
  /// <param name="start">The starting index in the array.</param>
  /// <param name="lengths">The lengths of each dimension.</param>
  /// <param name="strides">The strides for each dimension.</param>
  public ReadOnlyTensorSpan(T[]? array, int start, scoped ReadOnlySpan<nint> lengths, scoped ReadOnlySpan<nint> strides)
    : this(array.AsSpan(start), lengths, strides) { }

  /// <summary>
  /// Creates a new <see cref="ReadOnlyTensorSpan{T}"/> from a multi-dimensional array.
  /// </summary>
  /// <param name="array">The multi-dimensional array.</param>
  public ReadOnlyTensorSpan(Array? array) {
    if (array == null) {
      this._span = default;
      this._lengths = [];
      this._strides = [];
      this._isPinned = false;
      return;
    }

    var rank = array.Rank;
    var lengths = new nint[rank];
    for (var i = 0; i < rank; ++i)
      lengths[i] = array.GetLength(i);

    this._lengths = lengths;
    this._strides = _ComputeStrides(lengths);
    this._isPinned = false;

    var flatLength = (int)_ComputeFlatLength(lengths);
    var flat = new T[flatLength];
    var index = 0;
    foreach (T item in array)
      flat[index++] = item;

    this._span = flat;
  }

  /// <summary>
  /// Creates a new <see cref="ReadOnlyTensorSpan{T}"/> from a pointer.
  /// </summary>
  /// <param name="data">The pointer to the data.</param>
  /// <param name="dataLength">The length of the data.</param>
  /// <param name="lengths">The lengths of each dimension.</param>
  /// <param name="strides">The strides for each dimension.</param>
  /// <param name="isPinned">Whether the memory is pinned.</param>
  public unsafe ReadOnlyTensorSpan(T* data, nint dataLength, scoped ReadOnlySpan<nint> lengths, scoped ReadOnlySpan<nint> strides, bool isPinned = false) {
    this._span = new ReadOnlySpan<T>(data, (int)dataLength);
    this._lengths = lengths.ToArray();
    this._strides = strides.IsEmpty ? _ComputeStrides(lengths) : strides.ToArray();
    this._isPinned = isPinned;

    var expectedLength = _ComputeFlatLength(lengths);
    if (dataLength < expectedLength)
      throw new ArgumentException($"Data length {dataLength} is smaller than required length {expectedLength} for the given shape.");
  }

  /// <summary>Gets an empty <see cref="ReadOnlyTensorSpan{T}"/>.</summary>
  public static ReadOnlyTensorSpan<T> Empty => default;

  /// <summary>Gets a value indicating whether this span is empty.</summary>
  public bool IsEmpty => this.FlattenedLength == 0;

  /// <summary>Gets the lengths of each dimension.</summary>
  public ReadOnlySpan<nint> Lengths => this._lengths;

  /// <summary>Gets the strides for each dimension.</summary>
  public ReadOnlySpan<nint> Strides => this._strides;

  /// <summary>Gets the number of dimensions (rank).</summary>
  public int Rank => this._lengths?.Length ?? 0;

  /// <summary>Gets the total number of elements.</summary>
  public nint FlattenedLength => this._lengths == null ? 0 : _ComputeFlatLength(this._lengths);

  /// <summary>Gets a value indicating whether the tensor data is contiguous in memory.</summary>
  public bool IsDense => this._IsDense();

  /// <summary>Gets a value indicating whether this tensor span is pinned in memory.</summary>
  public bool IsPinned => this._isPinned;

  /// <summary>Gets a value indicating whether any dimension of this tensor is dense.</summary>
  public bool HasAnyDenseDimensions => this.Rank > 0 && this._strides[this.Rank - 1] == 1;

  /// <summary>Gets the element at the specified indices.</summary>
  /// <param name="indices">The indices for each dimension.</param>
  public ref readonly T this[params scoped ReadOnlySpan<nint> indices] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var flatIndex = this._ComputeFlatIndex(indices);
      return ref this._span[(int)flatIndex];
    }
  }

  /// <summary>Gets the element at the specified NIndex indices.</summary>
  /// <param name="indices">The indices for each dimension.</param>
  public ref readonly T this[params scoped ReadOnlySpan<NIndex> indices] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var nativeIndices = this._ConvertNIndicesToNint(indices);
      return ref this[nativeIndices];
    }
  }

  /// <summary>Gets a slice of this tensor span using NRange indices.</summary>
  /// <param name="ranges">The ranges for each dimension.</param>
  public ReadOnlyTensorSpan<T> this[params scoped ReadOnlySpan<NRange> ranges] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Slice(ranges);
  }

  /// <summary>Creates a slice of this tensor span from the specified start indices.</summary>
  /// <param name="startIndices">The starting indices for each dimension.</param>
  public ReadOnlyTensorSpan<T> Slice(scoped ReadOnlySpan<nint> startIndices) {
    var flatStart = this._ComputeFlatIndex(startIndices);
    var newLength = this._ComputeRemainingLength(startIndices);
    return new(this._span.Slice((int)flatStart, (int)newLength), this._ComputeRemainingLengths(startIndices), this._strides);
  }

  /// <summary>Creates a slice of this tensor span from the specified NIndex start indices.</summary>
  /// <param name="startIndices">The starting indices for each dimension.</param>
  public ReadOnlyTensorSpan<T> Slice(scoped ReadOnlySpan<NIndex> startIndices)
    => this.Slice(this._ConvertNIndicesToNint(startIndices));

  /// <summary>Creates a slice of this tensor span using the specified NRange ranges.</summary>
  /// <param name="ranges">The ranges for each dimension.</param>
  public ReadOnlyTensorSpan<T> Slice(scoped ReadOnlySpan<NRange> ranges) {
    if (ranges.Length != this.Rank)
      throw new ArgumentException($"Range count {ranges.Length} does not match rank {this.Rank}.");

    var startIndices = new nint[this.Rank];
    var newLengths = new nint[this.Rank];

    for (var i = 0; i < this.Rank; ++i) {
      var (offset, length) = ranges[i].GetOffsetAndLength(this._lengths[i]);
      startIndices[i] = offset;
      newLengths[i] = length;
    }

    var flatStart = this._ComputeFlatIndex(startIndices);
    var flatLength = _ComputeFlatLength(newLengths);
    return new(this._span.Slice((int)flatStart, (int)flatLength), newLengths, this._strides);
  }

  /// <summary>Flattens this tensor span to a destination span.</summary>
  public void FlattenTo(scoped Span<T> destination)
    => this._span.CopyTo(destination);

  /// <summary>Attempts to flatten this tensor span to a destination span.</summary>
  public bool TryFlattenTo(scoped Span<T> destination)
    => this._span.TryCopyTo(destination);

  /// <summary>Gets a span at the specified indices.</summary>
  public ReadOnlySpan<T> GetSpan(scoped ReadOnlySpan<nint> indices, int count) {
    var flatIndex = this._ComputeFlatIndex(indices);
    return this._span.Slice((int)flatIndex, count);
  }

  /// <summary>Gets a span at the specified NIndex indices.</summary>
  public ReadOnlySpan<T> GetSpan(scoped ReadOnlySpan<NIndex> indices, int count)
    => this.GetSpan(this._ConvertNIndicesToNint(indices), count);

  /// <summary>Attempts to get a span at the specified indices.</summary>
  public bool TryGetSpan(scoped ReadOnlySpan<nint> indices, int count, out ReadOnlySpan<T> span) {
    try {
      span = this.GetSpan(indices, count);
      return true;
    } catch {
      span = default;
      return false;
    }
  }

  /// <summary>Attempts to get a span at the specified NIndex indices.</summary>
  public bool TryGetSpan(scoped ReadOnlySpan<NIndex> indices, int count, out ReadOnlySpan<T> span)
    => this.TryGetSpan(this._ConvertNIndicesToNint(indices), count, out span);

  /// <summary>Gets a span for the specified dimension.</summary>
  /// <param name="dimension">The dimension to get a span for.</param>
  /// <returns>A ReadOnlyTensorDimensionSpan for iterating slices along the dimension.</returns>
  public ReadOnlyTensorDimensionSpan<T> GetDimensionSpan(int dimension)
    => new(this, dimension);

  /// <summary>Gets a slice at the specified index along a dimension.</summary>
  internal ReadOnlyTensorSpan<T> SliceAlongDimension(int dimension, int index) {
    if (this.Rank == 1)
      // For 1D tensor, return a scalar view
      return new(this._span.Slice(index * (int)this._strides[0], 1), [], []);

    // Compute new shape (same shape but with dimension removed)
    var newLengths = new nint[this.Rank - 1];
    var newStrides = new nint[this.Rank - 1];
    var idx = 0;
    for (var d = 0; d < this.Rank; ++d) {
      if (d != dimension) {
        newLengths[idx] = this._lengths[d];
        newStrides[idx] = this._strides[d];
        ++idx;
      }
    }

    // Calculate the starting offset for this slice
    var offset = index * (int)this._strides[dimension];

    // For non-contiguous slices, we need to be careful about the span length
    var maxOffset = offset;
    for (var d = 0; d < newLengths.Length; ++d)
      maxOffset += (int)((newLengths[d] - 1) * newStrides[d]);

    var spanLen = Math.Min(maxOffset + 1 - offset, this._span.Length - offset);

    return new(this._span.Slice(offset, spanLen), newLengths, newStrides);
  }

  /// <summary>Gets a reference to the first element that can be used for pinning.</summary>
  public ref readonly T GetPinnableReference()
    => ref this._span.GetPinnableReference();

  /// <summary>Copies the contents of this tensor span to a destination tensor span.</summary>
  /// <param name="destination">The destination tensor span.</param>
  public void CopyTo(scoped TensorSpan<T> destination) {
    if (destination.FlattenedLength < this.FlattenedLength)
      throw new ArgumentException("Destination is too small.");
    this._span.CopyTo(destination._GetSpan());
  }

  /// <summary>Attempts to copy the contents of this tensor span to a destination tensor span.</summary>
  /// <param name="destination">The destination tensor span.</param>
  /// <returns>True if the copy was successful; otherwise, false.</returns>
  public bool TryCopyTo(scoped TensorSpan<T> destination) {
    if (destination.FlattenedLength < this.FlattenedLength)
      return false;
    this._span.CopyTo(destination._GetSpan());
    return true;
  }

  /// <summary>Returns an enumerator for this tensor span.</summary>
  public Enumerator GetEnumerator() => new(this);

  /// <inheritdoc/>
  public override string ToString() => this.ToString([]);

  /// <summary>Returns a string representation of this tensor span with maximum lengths per dimension.</summary>
  public string ToString(scoped ReadOnlySpan<nint> maximumLengths)
    => $"ReadOnlyTensorSpan<{typeof(T).Name}>[{string.Join(", ", this._lengths ?? [])}]";

  /// <summary>Converts to a <see cref="ReadOnlyTensorSpan{T}"/>.</summary>
  public static implicit operator ReadOnlyTensorSpan<T>(TensorSpan<T> span)
    => new(span._GetSpan(), span.Lengths, span.Strides);


  /// <summary>Converts from an array.</summary>
  public static implicit operator ReadOnlyTensorSpan<T>(T[]? array)
    => new(array);

  /// <summary>Determines whether two tensor spans are equal.</summary>
  public static bool operator ==(ReadOnlyTensorSpan<T> left, ReadOnlyTensorSpan<T> right)
    => left._span.Length == right._span.Length
       && Unsafe.AreSame(ref Unsafe.AsRef(in left._span.GetPinnableReference()), ref Unsafe.AsRef(in right._span.GetPinnableReference()))
       && left.Lengths.SequenceEqual(right.Lengths);

  /// <summary>Determines whether two tensor spans are not equal.</summary>
  public static bool operator !=(ReadOnlyTensorSpan<T> left, ReadOnlyTensorSpan<T> right)
    => !(left == right);

  /// <summary>Gets the underlying span for internal use.</summary>
  internal ReadOnlySpan<T> _GetSpan() => this._span;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private nint _ComputeFlatIndex(ReadOnlySpan<nint> indices) {
    if (indices.Length != this.Rank)
      throw new ArgumentException($"Index count {indices.Length} does not match rank {this.Rank}.");

    nint flatIndex = 0;
    for (var i = 0; i < indices.Length; ++i)
      flatIndex += indices[i] * this._strides[i];

    return flatIndex;
  }

  private nint[] _ConvertNIndicesToNint(ReadOnlySpan<NIndex> indices) {
    var result = new nint[indices.Length];
    for (var i = 0; i < indices.Length; ++i)
      result[i] = indices[i].GetOffset(this._lengths[i]);
    return result;
  }

  private nint _ComputeRemainingLength(ReadOnlySpan<nint> startIndices) {
    nint remaining = 1;
    for (var i = 0; i < this.Rank; ++i)
      remaining *= this._lengths[i] - startIndices[i];
    return remaining;
  }

  private nint[] _ComputeRemainingLengths(ReadOnlySpan<nint> startIndices) {
    var result = new nint[this.Rank];
    for (var i = 0; i < this.Rank; ++i)
      result[i] = this._lengths[i] - startIndices[i];
    return result;
  }

  private bool _IsDense() {
    if (this.Rank == 0)
      return true;

    nint expectedStride = 1;
    for (var i = this.Rank - 1; i >= 0; --i) {
      if (this._strides[i] != expectedStride)
        return false;
      expectedStride *= this._lengths[i];
    }
    return true;
  }

  private static nint _ComputeFlatLength(ReadOnlySpan<nint> lengths) {
    if (lengths.IsEmpty)
      return 0;

    nint length = 1;
    foreach (var len in lengths)
      length *= len;

    return length;
  }

  private static nint[] _ComputeStrides(ReadOnlySpan<nint> lengths) {
    if (lengths.IsEmpty)
      return [];

    var strides = new nint[lengths.Length];
    nint stride = 1;
    for (var i = lengths.Length - 1; i >= 0; --i) {
      strides[i] = stride;
      stride *= lengths[i];
    }

    return strides;
  }

  /// <summary>Enumerator for <see cref="ReadOnlyTensorSpan{T}"/>.</summary>
  public ref struct Enumerator {
    private readonly ReadOnlyTensorSpan<T> _span;
    private int _index;

    internal Enumerator(ReadOnlyTensorSpan<T> span) {
      this._span = span;
      this._index = -1;
    }

    /// <summary>Gets the element at the current position of the enumerator.</summary>
    public ref readonly T Current => ref this._span._span[this._index];

    /// <summary>Advances the enumerator to the next element.</summary>
    public bool MoveNext() {
      var nextIndex = this._index + 1;
      if (nextIndex < (int)this._span.FlattenedLength) {
        this._index = nextIndex;
        return true;
      }
      return false;
    }
  }

}

#endif
