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
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Numerics.Tensors;

/// <summary>
/// Represents a mutable view over a contiguous region of arbitrary memory that can be interpreted as a tensor.
/// </summary>
/// <typeparam name="T">The type of items in the tensor span.</typeparam>
public readonly ref struct TensorSpan<T> {

  private readonly Span<T> _span;
  private readonly nint[] _lengths;
  private readonly nint[] _strides;

  #region Constructors

  /// <summary>
  /// Creates a new <see cref="TensorSpan{T}"/> from the given span and shape.
  /// </summary>
  public TensorSpan(Span<T> span, scoped ReadOnlySpan<nint> lengths, scoped ReadOnlySpan<nint> strides = default) {
    this._span = span;
    this._lengths = lengths.ToArray();
    this._strides = strides.IsEmpty ? _ComputeStrides(lengths) : strides.ToArray();

    var expectedLength = _ComputeFlatLength(lengths);
    if (span.Length < expectedLength)
      throw new ArgumentException($"Span length {span.Length} is smaller than required length {expectedLength} for the given shape.");
  }

  /// <summary>
  /// Creates a new <see cref="TensorSpan{T}"/> from the given array and shape.
  /// </summary>
  public TensorSpan(T[] array, scoped ReadOnlySpan<nint> lengths)
    : this(array.AsSpan(), lengths) { }

  /// <summary>
  /// Creates a new <see cref="TensorSpan{T}"/> from the given array with shape and strides.
  /// </summary>
  public TensorSpan(T[] array, scoped ReadOnlySpan<nint> lengths, scoped ReadOnlySpan<nint> strides)
    : this(array.AsSpan(), lengths, strides) { }

  /// <summary>
  /// Creates a new <see cref="TensorSpan{T}"/> from the given array with start index, shape, and strides.
  /// </summary>
  public TensorSpan(T[] array, int startIndex, scoped ReadOnlySpan<nint> lengths, scoped ReadOnlySpan<nint> strides)
    : this(array.AsSpan(startIndex), lengths, strides) { }

  /// <summary>
  /// Creates a new 1D <see cref="TensorSpan{T}"/> from the given span.
  /// </summary>
  public TensorSpan(Span<T> span) {
    this._span = span;
    this._lengths = [span.Length];
    this._strides = [1];
  }

  /// <summary>
  /// Creates a new 1D <see cref="TensorSpan{T}"/> from the given array.
  /// </summary>
  public TensorSpan(T[] array)
    : this(array.AsSpan()) { }

  /// <summary>
  /// Creates a new <see cref="TensorSpan{T}"/> from the given multi-dimensional array.
  /// </summary>
  public TensorSpan(Array array) {
    if (array == null)
      throw new ArgumentNullException(nameof(array));

    var rank = array.Rank;
    this._lengths = new nint[rank];
    for (var i = 0; i < rank; ++i)
      this._lengths[i] = array.GetLength(i);

    this._strides = _ComputeStrides(this._lengths);

    // For simplicity, we only support 1D arrays directly
    if (rank == 1 && array is T[] typedArray)
      this._span = typedArray.AsSpan();
    else
      throw new NotSupportedException("Multi-dimensional arrays are not directly supported. Use a flattened array.");
  }

  /// <summary>
  /// Creates a new <see cref="TensorSpan{T}"/> from the given pointer and shape.
  /// </summary>
  public unsafe TensorSpan(T* data, nint dataLength) {
    this._span = new Span<T>(data, (int)dataLength);
    this._lengths = [dataLength];
    this._strides = [1];
  }

  /// <summary>
  /// Creates a new <see cref="TensorSpan{T}"/> from the given pointer, length, and shape.
  /// </summary>
  public unsafe TensorSpan(T* data, nint dataLength, scoped ReadOnlySpan<nint> lengths) {
    this._span = new Span<T>(data, (int)dataLength);
    this._lengths = lengths.ToArray();
    this._strides = _ComputeStrides(lengths);
  }

  /// <summary>Internal constructor for creating slices without validation.</summary>
  internal TensorSpan(Span<T> span, nint[] lengths, nint[] strides) {
    this._span = span;
    this._lengths = lengths;
    this._strides = strides;
  }

  /// <summary>
  /// Creates a new <see cref="TensorSpan{T}"/> from the given pointer with length, shape, and strides.
  /// </summary>
  public unsafe TensorSpan(T* data, nint dataLength, scoped ReadOnlySpan<nint> lengths, scoped ReadOnlySpan<nint> strides) {
    this._span = new Span<T>(data, (int)dataLength);
    this._lengths = lengths.ToArray();
    this._strides = strides.IsEmpty ? _ComputeStrides(lengths) : strides.ToArray();
  }

  #endregion

  #region Static Members

  /// <summary>Gets an empty <see cref="TensorSpan{T}"/>.</summary>
  public static TensorSpan<T> Empty => default;

  #endregion

  #region Properties

  /// <summary>Gets a value indicating whether this span is empty.</summary>
  public bool IsEmpty => this._span.IsEmpty;

  /// <summary>Gets the lengths of each dimension.</summary>
  public ReadOnlySpan<nint> Lengths => this._lengths ?? [];

  /// <summary>Gets the strides for each dimension.</summary>
  public ReadOnlySpan<nint> Strides => this._strides ?? [];

  /// <summary>Gets the number of dimensions (rank).</summary>
  public int Rank => this._lengths?.Length ?? 0;

  /// <summary>Gets the total number of elements.</summary>
  public nint FlattenedLength => this._lengths == null ? 0 : _ComputeFlatLength(this._lengths);

  /// <summary>Gets a value indicating whether the tensor data is contiguous in memory.</summary>
  public bool IsDense => this._IsDense();

  /// <summary>Gets a value indicating whether this tensor span is pinned.</summary>
  public bool IsPinned => false;

  /// <summary>Gets a value indicating whether any dimension of this tensor is dense.</summary>
  public bool HasAnyDenseDimensions => this.Rank > 0 && this._strides[this.Rank - 1] == 1;

  #endregion

  #region Indexers

  /// <summary>Gets or sets the element at the specified indices.</summary>
  public ref T this[params scoped ReadOnlySpan<nint> indices] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var flatIndex = this._ComputeFlatIndex(indices);
      return ref this._span[(int)flatIndex];
    }
  }

  /// <summary>Gets or sets the element at the specified flat index.</summary>
  public ref T this[nint index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => ref this._span[(int)index];
  }

  /// <summary>Gets or sets the element at the specified NIndex indices.</summary>
  public ref T this[params scoped ReadOnlySpan<NIndex> indices] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var nativeIndices = this._ConvertNIndicesToNint(indices);
      return ref this[nativeIndices];
    }
  }

  /// <summary>Gets a slice of this tensor span using NRange indices.</summary>
  public TensorSpan<T> this[params scoped ReadOnlySpan<NRange> ranges] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Slice(ranges);
  }

  #endregion

  #region Conversion Methods

  /// <summary>Creates a read-only tensor span view of this tensor span.</summary>
  public ReadOnlyTensorSpan<T> AsReadOnlyTensorSpan()
    => new((ReadOnlySpan<T>)this._span, this._lengths, this._strides);

  /// <summary>Creates a read-only tensor span view starting at the specified indices.</summary>
  public ReadOnlyTensorSpan<T> AsReadOnlyTensorSpan(scoped ReadOnlySpan<nint> startIndices) {
    var flatStart = this._ComputeFlatIndex(startIndices);
    var newLength = this._ComputeRemainingLength(startIndices);
    return new((ReadOnlySpan<T>)this._span.Slice((int)flatStart, (int)newLength), this._ComputeRemainingLengths(startIndices), this._strides);
  }

  /// <summary>Creates a read-only tensor span view starting at the specified NIndex indices.</summary>
  public ReadOnlyTensorSpan<T> AsReadOnlyTensorSpan(scoped ReadOnlySpan<NIndex> startIndices) {
    var nativeIndices = this._ConvertNIndicesToNint(startIndices);
    return this.AsReadOnlyTensorSpan(nativeIndices);
  }

  /// <summary>Creates a read-only tensor span view for the specified ranges.</summary>
  public ReadOnlyTensorSpan<T> AsReadOnlyTensorSpan(scoped ReadOnlySpan<NRange> ranges) {
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
    var newLength = _ComputeFlatLength(newLengths);
    return new((ReadOnlySpan<T>)this._span.Slice((int)flatStart, (int)newLength), newLengths, this._strides);
  }

  #endregion

  #region Slice Methods

  /// <summary>Creates a slice of this tensor span.</summary>
  public TensorSpan<T> Slice(scoped ReadOnlySpan<nint> startIndices) {
    var flatStart = this._ComputeFlatIndex(startIndices);
    var newLength = this._ComputeRemainingLength(startIndices);
    return new(this._span.Slice((int)flatStart, (int)newLength), this._ComputeRemainingLengths(startIndices), this._strides);
  }

  /// <summary>Creates a slice of this tensor span starting at the specified NIndex indices.</summary>
  public TensorSpan<T> Slice(scoped ReadOnlySpan<NIndex> startIndices) {
    var nativeIndices = this._ConvertNIndicesToNint(startIndices);
    return this.Slice(nativeIndices);
  }

  /// <summary>Creates a slice of this tensor span using the specified ranges.</summary>
  public TensorSpan<T> Slice(scoped ReadOnlySpan<NRange> ranges) {
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
    var newLength = _ComputeFlatLength(newLengths);
    return new(this._span.Slice((int)flatStart, (int)newLength), newLengths, this._strides);
  }

  /// <summary>Gets a view of slices along the specified dimension.</summary>
  /// <param name="dimension">The dimension index.</param>
  /// <returns>A TensorDimensionSpan for iterating slices along the dimension.</returns>
  public TensorDimensionSpan<T> GetDimensionSpan(int dimension)
    => new(this, dimension);

  /// <summary>Gets a slice at the specified index along a dimension.</summary>
  internal TensorSpan<T> SliceAlongDimension(int dimension, int index) {
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

    // Compute the span length needed for the slice
    var sliceLen = 1;
    for (var d = 0; d < newLengths.Length; ++d)
      sliceLen *= (int)newLengths[d];

    // For non-contiguous slices, we need to be careful about the span length
    var maxOffset = offset;
    for (var d = 0; d < newLengths.Length; ++d)
      maxOffset += (int)((newLengths[d] - 1) * newStrides[d]);

    var spanLen = Math.Min(maxOffset + 1 - offset, this._span.Length - offset);

    return new(this._span.Slice(offset, spanLen), newLengths, newStrides);
  }

  #endregion

  #region Copy Methods

  /// <summary>Copies the contents of this tensor span to a destination tensor span.</summary>
  public void CopyTo(TensorSpan<T> destination) {
    if (destination.FlattenedLength < this.FlattenedLength)
      throw new ArgumentException("Destination is too small.");
    this._span.CopyTo(destination._span);
  }

  /// <summary>Attempts to copy the contents of this tensor span to a destination tensor span.</summary>
  public bool TryCopyTo(TensorSpan<T> destination) {
    if (destination.FlattenedLength < this.FlattenedLength)
      return false;
    this._span.CopyTo(destination._span);
    return true;
  }

  /// <summary>Flattens this tensor span to a destination span.</summary>
  public void FlattenTo(Span<T> destination)
    => this._span.CopyTo(destination);

  /// <summary>Attempts to flatten this tensor span to a destination span.</summary>
  public bool TryFlattenTo(Span<T> destination)
    => this._span.TryCopyTo(destination);

  #endregion

  #region Span Access Methods

  /// <summary>Gets a span at the specified indices.</summary>
  public Span<T> GetSpan(scoped ReadOnlySpan<nint> indices, int count) {
    var flatIndex = this._ComputeFlatIndex(indices);
    return this._span.Slice((int)flatIndex, count);
  }

  /// <summary>Gets a span at the specified NIndex indices.</summary>
  public Span<T> GetSpan(scoped ReadOnlySpan<NIndex> indices, int count) {
    var nativeIndices = this._ConvertNIndicesToNint(indices);
    return this.GetSpan(nativeIndices, count);
  }

  /// <summary>Attempts to get a span at the specified indices.</summary>
  public bool TryGetSpan(scoped ReadOnlySpan<nint> indices, int count, out Span<T> span) {
    try {
      span = this.GetSpan(indices, count);
      return true;
    } catch {
      span = default;
      return false;
    }
  }

  /// <summary>Attempts to get a span at the specified indices.</summary>
  public bool TryGetSpan(scoped ReadOnlySpan<nint> indices, int count, out ReadOnlySpan<T> span) {
    if (this.TryGetSpan(indices, count, out Span<T> s)) {
      span = s;
      return true;
    }
    span = default;
    return false;
  }

  /// <summary>Attempts to get a span at the specified NIndex indices.</summary>
  public bool TryGetSpan(scoped ReadOnlySpan<NIndex> indices, int count, out Span<T> span) {
    var nativeIndices = this._ConvertNIndicesToNint(indices);
    return this.TryGetSpan(nativeIndices, count, out span);
  }

  /// <summary>Attempts to get a span at the specified NIndex indices.</summary>
  public bool TryGetSpan(scoped ReadOnlySpan<NIndex> indices, int count, out ReadOnlySpan<T> span) {
    var nativeIndices = this._ConvertNIndicesToNint(indices);
    return this.TryGetSpan(nativeIndices, count, out span);
  }

  /// <summary>Gets a reference to the first element that can be used for pinning.</summary>
  public ref T GetPinnableReference() => ref this._span.GetPinnableReference();

  #endregion

  #region Modification Methods

  /// <summary>Fills the tensor span with the specified value.</summary>
  public void Fill(T value) => this._span.Fill(value);

  /// <summary>Clears the tensor span, setting all elements to their default value.</summary>
  public void Clear() => this._span.Clear();

  #endregion

  #region Enumeration

  /// <summary>Returns an enumerator for this tensor span.</summary>
  public Enumerator GetEnumerator() => new(this);

  /// <summary>Enumerator for <see cref="TensorSpan{T}"/>.</summary>
  public ref struct Enumerator {
    private readonly TensorSpan<T> _span;
    private int _index;

    internal Enumerator(TensorSpan<T> span) {
      this._span = span;
      this._index = -1;
    }

    /// <summary>Gets the element at the current position of the enumerator.</summary>
    public ref T Current => ref this._span[(nint)this._index];

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

  #endregion

  #region Object Overrides

  /// <inheritdoc/>
  public override string ToString() => this.ToString([]);

  /// <summary>Returns a string representation with maximum lengths.</summary>
  public string ToString(scoped ReadOnlySpan<nint> maximumLengths)
    => $"TensorSpan<{typeof(T).Name}>[{string.Join(", ", this._lengths ?? [])}]";

  #endregion

  #region Operators

  /// <summary>Converts an array to a tensor span.</summary>
  public static implicit operator TensorSpan<T>(T[] array) => new(array);

  /// <summary>Converts a tensor span to a read-only tensor span.</summary>
  public static implicit operator ReadOnlyTensorSpan<T>(TensorSpan<T> span) => span.AsReadOnlyTensorSpan();

  /// <summary>Determines whether two tensor spans are equal.</summary>
  public static bool operator ==(TensorSpan<T> left, TensorSpan<T> right)
    => left._span.Length == right._span.Length
       && Unsafe.AreSame(ref left._span.GetPinnableReference(), ref right._span.GetPinnableReference())
       && left.Lengths.SequenceEqual(right.Lengths);

  /// <summary>Determines whether two tensor spans are not equal.</summary>
  public static bool operator !=(TensorSpan<T> left, TensorSpan<T> right)
    => !(left == right);

  #endregion

  #region Internal Methods

  /// <summary>Gets the underlying span for internal use.</summary>
  internal Span<T> _GetSpan() => this._span;

  #endregion

  #region Private Methods

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private nint _ComputeFlatIndex(ReadOnlySpan<nint> indices) {
    if (indices.Length != this.Rank)
      throw new ArgumentException($"Index count {indices.Length} does not match rank {this.Rank}.");

    nint flatIndex = 0;
    for (var i = 0; i < indices.Length; ++i)
      flatIndex += indices[i] * this._strides[i];

    return flatIndex;
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

  private nint[] _ConvertNIndicesToNint(ReadOnlySpan<NIndex> indices) {
    var result = new nint[indices.Length];
    for (var i = 0; i < indices.Length; ++i)
      result[i] = indices[i].GetOffset(this._lengths[i]);
    return result;
  }

  #endregion

}

#endif
