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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Numerics.Tensors;

/// <summary>
/// Represents a multi-dimensional tensor that owns its data.
/// </summary>
/// <typeparam name="T">The type of items in the tensor.</typeparam>
public sealed partial class Tensor<T> : IEnumerable<T> {

  private readonly T[] _data;
  private readonly nint[] _lengths;
  private readonly nint[] _strides;
  private readonly int _start;
  private readonly bool _isPinned;
  private GCHandle _pinnedHandle;

  /// <summary>
  /// Internal constructor used by factory methods.
  /// </summary>
  internal Tensor(T[] array, int start, ReadOnlySpan<nint> lengths, ReadOnlySpan<nint> strides, bool pinned = false) {
    this._data = array ?? throw new ArgumentNullException(nameof(array));
    this._start = start;
    this._lengths = lengths.ToArray();
    this._strides = strides.IsEmpty ? _ComputeStrides(lengths) : strides.ToArray();
    this._isPinned = pinned;

    var expectedLength = _ComputeFlatLength(lengths);
    if (array.Length - start < expectedLength)
      throw new ArgumentException($"Array length {array.Length - start} is smaller than required length {expectedLength} for the given shape.");
  }

  /// <summary>Gets an empty <see cref="Tensor{T}"/>.</summary>
  public static Tensor<T> Empty { get; } = new([], 0, ReadOnlySpan<nint>.Empty, ReadOnlySpan<nint>.Empty);

  /// <summary>Gets a value indicating whether this tensor is empty.</summary>
  public bool IsEmpty => this.FlattenedLength == 0;

  /// <summary>Gets the lengths of each dimension.</summary>
  public ReadOnlySpan<nint> Lengths => this._lengths;

  /// <summary>Gets the strides for each dimension.</summary>
  public ReadOnlySpan<nint> Strides => this._strides;

  /// <summary>Gets the number of dimensions (rank).</summary>
  public int Rank => this._lengths.Length;

  /// <summary>Gets the total number of elements.</summary>
  public nint FlattenedLength => _ComputeFlatLength(this._lengths);

  /// <summary>Gets a value indicating whether the tensor data is contiguous in memory.</summary>
  public bool IsDense => this._IsDense();

  /// <summary>Gets a value indicating whether this tensor is pinned in memory.</summary>
  public bool IsPinned => this._isPinned || this._pinnedHandle.IsAllocated;

  /// <summary>Gets a value indicating whether any dimension of this tensor is dense.</summary>
  public bool HasAnyDenseDimensions => this.Rank > 0 && this._strides[this.Rank - 1] == 1;

  /// <summary>Gets or sets the element at the specified indices.</summary>
  /// <param name="indices">The indices for each dimension.</param>
  public ref T this[params scoped ReadOnlySpan<nint> indices] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var flatIndex = this._ComputeFlatIndex(indices);
      return ref this._data[this._start + flatIndex];
    }
  }

  /// <summary>Gets or sets the element at the specified NIndex indices.</summary>
  /// <param name="indices">The NIndex indices for each dimension.</param>
  public ref T this[params scoped ReadOnlySpan<NIndex> indices] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var nativeIndices = this._ConvertNIndicesToNint(indices);
      return ref this[nativeIndices];
    }
  }

  /// <summary>Gets a slice of this tensor using NRange indices.</summary>
  /// <param name="ranges">The range for each dimension.</param>
  public Tensor<T> this[params scoped ReadOnlySpan<NRange> ranges] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Slice(ranges);
  }

  /// <summary>Creates a tensor span view of this tensor.</summary>
  public TensorSpan<T> AsTensorSpan()
    => new(this._data.AsSpan(this._start), this._lengths, this._strides);

  /// <summary>Creates a tensor span view of this tensor starting at the specified indices.</summary>
  public TensorSpan<T> AsTensorSpan(scoped ReadOnlySpan<nint> startIndices) {
    var flatStart = this._ComputeFlatIndex(startIndices);
    var newLength = this._ComputeRemainingLength(startIndices);
    return new(this._data.AsSpan(this._start + (int)flatStart, (int)newLength), this._ComputeRemainingLengths(startIndices), this._strides);
  }

  /// <summary>Creates a read-only tensor span view of this tensor.</summary>
  public ReadOnlyTensorSpan<T> AsReadOnlyTensorSpan()
    => new((ReadOnlySpan<T>)this._data.AsSpan(this._start), this._lengths, this._strides);

  /// <summary>Creates a read-only tensor span view of this tensor starting at the specified indices.</summary>
  public ReadOnlyTensorSpan<T> AsReadOnlyTensorSpan(scoped ReadOnlySpan<nint> startIndices) {
    var flatStart = this._ComputeFlatIndex(startIndices);
    var newLength = this._ComputeRemainingLength(startIndices);
    return new((ReadOnlySpan<T>)this._data.AsSpan(this._start + (int)flatStart, (int)newLength), this._ComputeRemainingLengths(startIndices), this._strides);
  }

  /// <summary>Creates a tensor span view of this tensor starting at the specified NIndex indices.</summary>
  public TensorSpan<T> AsTensorSpan(scoped ReadOnlySpan<NIndex> startIndices) {
    var nativeIndices = this._ConvertNIndicesToNint(startIndices);
    return this.AsTensorSpan(nativeIndices);
  }

  /// <summary>Creates a tensor span view of this tensor for the specified ranges.</summary>
  public TensorSpan<T> AsTensorSpan(scoped ReadOnlySpan<NRange> ranges) {
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
    return new(this._data.AsSpan(this._start + (int)flatStart, (int)newLength), newLengths, this._strides);
  }

  /// <summary>Creates a read-only tensor span view of this tensor starting at the specified NIndex indices.</summary>
  public ReadOnlyTensorSpan<T> AsReadOnlyTensorSpan(scoped ReadOnlySpan<NIndex> startIndices) {
    var nativeIndices = this._ConvertNIndicesToNint(startIndices);
    return this.AsReadOnlyTensorSpan(nativeIndices);
  }

  /// <summary>Creates a read-only tensor span view of this tensor for the specified ranges.</summary>
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
    return new((ReadOnlySpan<T>)this._data.AsSpan(this._start + (int)flatStart, (int)newLength), newLengths, this._strides);
  }

  /// <summary>Fills the tensor with the specified value.</summary>
  public void Fill(T value) => Array.Fill(this._data, value, this._start, (int)this.FlattenedLength);

  /// <summary>Clears the tensor, setting all elements to their default value.</summary>
  public void Clear() => Array.Clear(this._data, this._start, (int)this.FlattenedLength);

  /// <summary>Copies the contents of this tensor to a destination tensor span.</summary>
  public void CopyTo(TensorSpan<T> destination) {
    if (destination.FlattenedLength < this.FlattenedLength)
      throw new ArgumentException("Destination is too small.");
    this._data.AsSpan(this._start, (int)this.FlattenedLength).CopyTo(destination._GetSpan());
  }

  /// <summary>Attempts to copy the contents of this tensor to a destination tensor span.</summary>
  public bool TryCopyTo(TensorSpan<T> destination) {
    if (destination.FlattenedLength < this.FlattenedLength)
      return false;
    this._data.AsSpan(this._start, (int)this.FlattenedLength).CopyTo(destination._GetSpan());
    return true;
  }

  /// <summary>Flattens this tensor to a destination span.</summary>
  public void FlattenTo(Span<T> destination)
    => this._data.AsSpan(this._start, (int)this.FlattenedLength).CopyTo(destination);

  /// <summary>Attempts to flatten this tensor to a destination span.</summary>
  public bool TryFlattenTo(Span<T> destination)
    => this._data.AsSpan(this._start, (int)this.FlattenedLength).TryCopyTo(destination);

  /// <summary>Gets a slice of this tensor.</summary>
  public Tensor<T> Slice(scoped ReadOnlySpan<nint> startIndices) {
    var flatStart = this._ComputeFlatIndex(startIndices);
    return new(this._data, this._start + (int)flatStart, this._ComputeRemainingLengths(startIndices), this._strides);
  }

  /// <summary>Gets a slice of this tensor starting at the specified NIndex indices.</summary>
  public Tensor<T> Slice(scoped ReadOnlySpan<NIndex> startIndices) {
    var nativeIndices = this._ConvertNIndicesToNint(startIndices);
    return this.Slice(nativeIndices);
  }

  /// <summary>Gets a slice of this tensor using the specified ranges.</summary>
  public Tensor<T> Slice(scoped ReadOnlySpan<NRange> ranges) {
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
    return new(this._data, this._start + (int)flatStart, newLengths, this._strides);
  }

  /// <summary>Gets the slices that exist within the specified dimension.</summary>
  /// <param name="dimension">The dimension index.</param>
  /// <returns>A dimension span for iterating through slices along the specified dimension.</returns>
  public ReadOnlyTensorDimensionSpan<T> GetDimensionSpan(int dimension)
    => this.AsReadOnlyTensorSpan().GetDimensionSpan(dimension);

  /// <summary>Gets a span at the specified indices.</summary>
  public Span<T> GetSpan(scoped ReadOnlySpan<nint> indices, int count) {
    var flatIndex = this._ComputeFlatIndex(indices);
    return this._data.AsSpan(this._start + (int)flatIndex, count);
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
  public ref T GetPinnableReference()
    => ref this._data[this._start];

  /// <summary>Gets a pinned memory handle for this tensor.</summary>
  public MemoryHandle GetPinnedHandle() {
    if (!this._pinnedHandle.IsAllocated)
      this._pinnedHandle = GCHandle.Alloc(this._data, GCHandleType.Pinned);
    unsafe {
      return new(Unsafe.AsPointer(ref this._data[this._start]), this._pinnedHandle);
    }
  }

  /// <summary>Returns a dense copy of this tensor if it is not already dense.</summary>
  public Tensor<T> ToDenseTensor() {
    if (this.IsDense)
      return this;

    var dense = new T[(int)this.FlattenedLength];
    this.FlattenTo(dense);
    return Tensor.Create(dense, this._lengths);
  }

  /// <inheritdoc/>
  public IEnumerator<T> GetEnumerator() {
    var length = (int)this.FlattenedLength;
    for (var i = 0; i < length; ++i)
      yield return this._data[this._start + i];
  }

  /// <inheritdoc/>
  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  /// <inheritdoc/>
  public override string ToString() => this.ToString([]);

  /// <summary>Returns a string representation of this tensor with maximum lengths per dimension.</summary>
  public string ToString(scoped ReadOnlySpan<nint> maximumLengths)
    => $"Tensor<{typeof(T).Name}>[{string.Join(", ", this._lengths)}]";

  /// <summary>Converts an array to a tensor.</summary>
  public static implicit operator Tensor<T>(T[] array) => Tensor.Create(array);

  /// <summary>Converts a tensor to a tensor span.</summary>
  public static implicit operator TensorSpan<T>(Tensor<T> tensor) => tensor.AsTensorSpan();

  /// <summary>Converts a tensor to a read-only tensor span.</summary>
  public static implicit operator ReadOnlyTensorSpan<T>(Tensor<T> tensor) => tensor.AsReadOnlyTensorSpan();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private nint _ComputeFlatIndex(ReadOnlySpan<nint> indices) {
    if (indices.Length != this.Rank)
      throw new ArgumentException($"Index count {indices.Length} does not match rank {this.Rank}.");

    nint flatIndex = 0;
    for (var i = 0; i < indices.Length; ++i)
      flatIndex += indices[i] * this._strides[i];

    return flatIndex;
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

  private nint[] _ConvertNIndicesToNint(ReadOnlySpan<NIndex> indices) {
    var result = new nint[indices.Length];
    for (var i = 0; i < indices.Length; ++i)
      result[i] = indices[i].GetOffset(this._lengths[i]);
    return result;
  }

}

#endif
