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
/// Provides static factory methods for creating tensors.
/// </summary>
public static partial class Tensor {

  /// <summary>
  /// Creates a 1D tensor from the given array.
  /// </summary>
  /// <typeparam name="T">The type of elements in the tensor.</typeparam>
  /// <param name="array">The source array.</param>
  /// <returns>A new tensor containing a copy of the array data.</returns>
  public static Tensor<T> Create<T>(T[] array)
    => new(array, 0, [array.Length], default);

  /// <summary>
  /// Creates a tensor from the given array with specified lengths.
  /// </summary>
  /// <typeparam name="T">The type of elements in the tensor.</typeparam>
  /// <param name="array">The source array.</param>
  /// <param name="lengths">The lengths of each dimension.</param>
  /// <returns>A new tensor with the specified shape.</returns>
  public static Tensor<T> Create<T>(T[] array, scoped ReadOnlySpan<nint> lengths)
    => new(array, 0, lengths, default);

  /// <summary>
  /// Creates a tensor from the given array with specified lengths and strides.
  /// </summary>
  /// <typeparam name="T">The type of elements in the tensor.</typeparam>
  /// <param name="array">The source array.</param>
  /// <param name="lengths">The lengths of each dimension.</param>
  /// <param name="strides">The strides for each dimension.</param>
  /// <returns>A new tensor with the specified shape and strides.</returns>
  public static Tensor<T> Create<T>(T[] array, scoped ReadOnlySpan<nint> lengths, scoped ReadOnlySpan<nint> strides)
    => new(array, 0, lengths, strides);

  /// <summary>
  /// Creates a tensor from the given array with specified start index, lengths, and strides.
  /// </summary>
  /// <typeparam name="T">The type of elements in the tensor.</typeparam>
  /// <param name="array">The source array.</param>
  /// <param name="start">The starting index in the array.</param>
  /// <param name="lengths">The lengths of each dimension.</param>
  /// <param name="strides">The strides for each dimension.</param>
  /// <returns>A new tensor with the specified shape and strides.</returns>
  public static Tensor<T> Create<T>(T[] array, int start, scoped ReadOnlySpan<nint> lengths, scoped ReadOnlySpan<nint> strides)
    => new(array, start, lengths, strides);

  /// <summary>
  /// Creates a tensor with the specified shape, initialized to default values.
  /// </summary>
  /// <typeparam name="T">The type of elements in the tensor.</typeparam>
  /// <param name="lengths">The lengths of each dimension.</param>
  /// <param name="pinned">Whether to pin the underlying array in memory.</param>
  /// <returns>A new tensor with the specified shape.</returns>
  public static Tensor<T> CreateFromShape<T>(scoped ReadOnlySpan<nint> lengths, bool pinned = false) {
    var flatLength = _ComputeFlatLength(lengths);
    var array = pinned ? GC.AllocateArray<T>((int)flatLength, pinned: true) : new T[(int)flatLength];
    return new Tensor<T>(array, 0, lengths, default, pinned);
  }

  /// <summary>
  /// Creates a tensor with the specified shape and strides, initialized to default values.
  /// </summary>
  /// <typeparam name="T">The type of elements in the tensor.</typeparam>
  /// <param name="lengths">The lengths of each dimension.</param>
  /// <param name="strides">The strides for each dimension.</param>
  /// <param name="pinned">Whether to pin the underlying array in memory.</param>
  /// <returns>A new tensor with the specified shape and strides.</returns>
  public static Tensor<T> CreateFromShape<T>(scoped ReadOnlySpan<nint> lengths, scoped ReadOnlySpan<nint> strides, bool pinned = false) {
    var flatLength = _ComputeFlatLength(lengths);
    var array = pinned ? GC.AllocateArray<T>((int)flatLength, pinned: true) : new T[(int)flatLength];
    return new Tensor<T>(array, 0, lengths, strides, pinned);
  }

  /// <summary>
  /// Creates a tensor with the specified shape without initializing the elements.
  /// </summary>
  /// <typeparam name="T">The type of elements in the tensor.</typeparam>
  /// <param name="lengths">The lengths of each dimension.</param>
  /// <param name="pinned">Whether to pin the underlying array in memory.</param>
  /// <returns>A new tensor with the specified shape.</returns>
  public static Tensor<T> CreateFromShapeUninitialized<T>(scoped ReadOnlySpan<nint> lengths, bool pinned = false) {
    var flatLength = _ComputeFlatLength(lengths);
    var array = GC.AllocateUninitializedArray<T>((int)flatLength, pinned);
    return new Tensor<T>(array, 0, lengths, default, pinned);
  }

  /// <summary>
  /// Creates a tensor with the specified shape and strides without initializing the elements.
  /// </summary>
  /// <typeparam name="T">The type of elements in the tensor.</typeparam>
  /// <param name="lengths">The lengths of each dimension.</param>
  /// <param name="strides">The strides for each dimension.</param>
  /// <param name="pinned">Whether to pin the underlying array in memory.</param>
  /// <returns>A new tensor with the specified shape and strides.</returns>
  public static Tensor<T> CreateFromShapeUninitialized<T>(scoped ReadOnlySpan<nint> lengths, scoped ReadOnlySpan<nint> strides, bool pinned = false) {
    var flatLength = _ComputeFlatLength(lengths);
    var array = GC.AllocateUninitializedArray<T>((int)flatLength, pinned);
    return new Tensor<T>(array, 0, lengths, strides, pinned);
  }

  /// <summary>
  /// Creates a tensor span from the given array.
  /// </summary>
  /// <typeparam name="T">The type of elements in the tensor.</typeparam>
  /// <param name="array">The source array.</param>
  /// <returns>A tensor span view of the array.</returns>
  public static TensorSpan<T> AsTensorSpan<T>(T[] array)
    => new(array);

  /// <summary>
  /// Creates a tensor span from the given array with specified lengths.
  /// </summary>
  /// <typeparam name="T">The type of elements in the tensor.</typeparam>
  /// <param name="array">The source array.</param>
  /// <param name="lengths">The lengths of each dimension.</param>
  /// <returns>A tensor span view of the array with the specified shape.</returns>
  public static TensorSpan<T> AsTensorSpan<T>(T[] array, scoped ReadOnlySpan<nint> lengths)
    => new(array, lengths);

  /// <summary>
  /// Creates a tensor span from the given array with specified lengths and strides.
  /// </summary>
  /// <typeparam name="T">The type of elements in the tensor.</typeparam>
  /// <param name="array">The source array.</param>
  /// <param name="lengths">The lengths of each dimension.</param>
  /// <param name="strides">The strides for each dimension.</param>
  /// <returns>A tensor span view of the array with the specified shape and strides.</returns>
  public static TensorSpan<T> AsTensorSpan<T>(T[] array, scoped ReadOnlySpan<nint> lengths, scoped ReadOnlySpan<nint> strides)
    => new(array, lengths, strides);

  /// <summary>
  /// Creates a tensor span from the given array with specified start, lengths, and strides.
  /// </summary>
  /// <typeparam name="T">The type of elements in the tensor.</typeparam>
  /// <param name="array">The source array.</param>
  /// <param name="start">The starting index in the array.</param>
  /// <param name="lengths">The lengths of each dimension.</param>
  /// <param name="strides">The strides for each dimension.</param>
  /// <returns>A tensor span view of the array with the specified shape and strides.</returns>
  public static TensorSpan<T> AsTensorSpan<T>(T[] array, int start, scoped ReadOnlySpan<nint> lengths, scoped ReadOnlySpan<nint> strides)
    => new(array, start, lengths, strides);

  /// <summary>
  /// Creates a read-only tensor span from the given array.
  /// </summary>
  /// <typeparam name="T">The type of elements in the tensor.</typeparam>
  /// <param name="array">The source array.</param>
  /// <returns>A read-only tensor span view of the array.</returns>
  public static ReadOnlyTensorSpan<T> AsReadOnlyTensorSpan<T>(T[] array)
    => new(array);

  /// <summary>
  /// Creates a read-only tensor span from the given array with specified lengths.
  /// </summary>
  /// <typeparam name="T">The type of elements in the tensor.</typeparam>
  /// <param name="array">The source array.</param>
  /// <param name="lengths">The lengths of each dimension.</param>
  /// <returns>A read-only tensor span view of the array with the specified shape.</returns>
  public static ReadOnlyTensorSpan<T> AsReadOnlyTensorSpan<T>(T[] array, scoped ReadOnlySpan<nint> lengths)
    => new(array, lengths);

  /// <summary>
  /// Creates a read-only tensor span from the given array with specified lengths and strides.
  /// </summary>
  /// <typeparam name="T">The type of elements in the tensor.</typeparam>
  /// <param name="array">The source array.</param>
  /// <param name="lengths">The lengths of each dimension.</param>
  /// <param name="strides">The strides for each dimension.</param>
  /// <returns>A read-only tensor span view of the array with the specified shape and strides.</returns>
  public static ReadOnlyTensorSpan<T> AsReadOnlyTensorSpan<T>(T[] array, scoped ReadOnlySpan<nint> lengths, scoped ReadOnlySpan<nint> strides)
    => new((ReadOnlySpan<T>)array, lengths, strides);

  /// <summary>
  /// Creates a read-only tensor span from the given array with specified start, lengths, and strides.
  /// </summary>
  /// <typeparam name="T">The type of elements in the tensor.</typeparam>
  /// <param name="array">The source array.</param>
  /// <param name="start">The starting index in the array.</param>
  /// <param name="lengths">The lengths of each dimension.</param>
  /// <param name="strides">The strides for each dimension.</param>
  /// <returns>A read-only tensor span view of the array with the specified shape and strides.</returns>
  public static ReadOnlyTensorSpan<T> AsReadOnlyTensorSpan<T>(T[] array, int start, scoped ReadOnlySpan<nint> lengths, scoped ReadOnlySpan<nint> strides)
    => new(array, start, lengths, strides);

  private static nint _ComputeFlatLength(ReadOnlySpan<nint> lengths) {
    if (lengths.IsEmpty)
      return 0;

    nint length = 1;
    foreach (var len in lengths)
      length *= len;

    return length;
  }

}

#endif
