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

namespace System.Numerics.Tensors;

/// <summary>
/// Represents the slices that exist within a dimension of a tensor span.
/// </summary>
/// <typeparam name="T">The type of elements in the tensor.</typeparam>
public readonly ref struct TensorDimensionSpan<T> {

  private readonly TensorSpan<T> _tensor;
  private readonly int _dimension;
  private readonly nint _length;

  /// <summary>
  /// Creates a new TensorDimensionSpan for the specified dimension.
  /// </summary>
  internal TensorDimensionSpan(TensorSpan<T> tensor, int dimension) {
    if (dimension < 0 || dimension >= tensor.Rank)
      throw new ArgumentOutOfRangeException(nameof(dimension));

    this._tensor = tensor;
    this._dimension = dimension;

    // Length is the product of all lengths from dimension 0 through the specified dimension
    nint length = 1;
    for (var d = 0; d <= dimension; ++d)
      length *= tensor.Lengths[d];
    this._length = length;
  }

  /// <summary>
  /// Gets whether the slices that exist within the tracked dimension are dense.
  /// </summary>
  public bool IsDense {
    get {
      // A dimension span is dense if the underlying tensor is dense
      // and slicing along this dimension produces contiguous slices
      if (!this._tensor.IsDense)
        return false;

      // For a dense tensor, slices along dimension 0 are always dense
      // Slices along other dimensions may not be contiguous
      return this._dimension == 0;
    }
  }

  /// <summary>
  /// Gets the length of the tensor dimension span (number of slices).
  /// </summary>
  public nint Length => this._length;

  /// <summary>
  /// Gets the tensor span representing a slice at the specified index.
  /// </summary>
  /// <param name="index">The index of the slice.</param>
  /// <returns>The tensor span for the slice.</returns>
  public TensorSpan<T> this[int index] {
    get {
      if (index < 0 || index >= (int)this._length)
        throw new ArgumentOutOfRangeException(nameof(index));

      return this._tensor.SliceAlongDimension(this._dimension, index);
    }
  }

  /// <summary>
  /// Gets an enumerator for iterating through slices.
  /// </summary>
  public Enumerator GetEnumerator() => new(this);

  /// <summary>
  /// Converts to a read-only tensor dimension span.
  /// </summary>
  public static implicit operator ReadOnlyTensorDimensionSpan<T>(TensorDimensionSpan<T> span)
    => new(span._tensor.AsReadOnlyTensorSpan(), span._dimension);

  /// <summary>
  /// Enumerator for TensorDimensionSpan.
  /// </summary>
  public ref struct Enumerator {

    private readonly TensorDimensionSpan<T> _span;
    private int _index;

    internal Enumerator(TensorDimensionSpan<T> span) {
      this._span = span;
      this._index = -1;
    }

    /// <summary>
    /// Gets the current slice.
    /// </summary>
    public TensorSpan<T> Current => this._span[this._index];

    /// <summary>
    /// Moves to the next slice.
    /// </summary>
    public bool MoveNext() {
      var next = this._index + 1;
      if (next < (int)this._span._length) {
        this._index = next;
        return true;
      }
      return false;
    }

    /// <summary>
    /// Resets the enumerator.
    /// </summary>
    public void Reset() => this._index = -1;

  }

}

#endif
