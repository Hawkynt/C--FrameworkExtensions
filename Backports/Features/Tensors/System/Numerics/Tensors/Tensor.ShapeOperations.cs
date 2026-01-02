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

using System.Collections.Generic;

namespace System.Numerics.Tensors;

public static partial class Tensor {

  #region Reshape

  /// <summary>Reshapes the tensor to the specified shape.</summary>
  public static Tensor<T> Reshape<T>(in ReadOnlyTensorSpan<T> tensor, scoped ReadOnlySpan<nint> lengths) {
    var currentLength = _ComputeFlatLength(tensor.Lengths);
    var newLength = _ComputeFlatLength(lengths);
    if (currentLength != newLength)
      throw new ArgumentException($"Cannot reshape tensor of size {currentLength} to size {newLength}");

    var data = _DensifyToArray(tensor);
    return Create(data, lengths);
  }

  /// <summary>Reshapes the tensor to the specified shape into destination.</summary>
  public static void Reshape<T>(scoped in ReadOnlyTensorSpan<T> tensor, in TensorSpan<T> destination) {
    var srcLength = _ComputeFlatLength(tensor.Lengths);
    var dstLength = _ComputeFlatLength(destination.Lengths);
    if (srcLength != dstLength)
      throw new ArgumentException($"Cannot reshape tensor of size {srcLength} to size {dstLength}");

    tensor.FlattenTo(_GetFlatSpan(destination));
  }

  #endregion

  #region Squeeze/Unsqueeze

  /// <summary>Removes all dimensions of size 1 from the tensor.</summary>
  public static Tensor<T> Squeeze<T>(in ReadOnlyTensorSpan<T> tensor) {
    var newLengths = new List<nint>();
    foreach (var len in tensor.Lengths)
      if (len != 1)
        newLengths.Add(len);

    if (newLengths.Count == 0)
      newLengths.Add(1); // Scalar case

    var data = _DensifyToArray(tensor);
    return Create(data, newLengths.ToArray());
  }

  /// <summary>Removes a specific dimension of size 1 from the tensor.</summary>
  public static Tensor<T> SqueezeDimension<T>(in ReadOnlyTensorSpan<T> tensor, int dimension) {
    if (dimension < 0 || dimension >= tensor.Rank)
      throw new ArgumentOutOfRangeException(nameof(dimension));

    if (tensor.Lengths[dimension] != 1)
      throw new ArgumentException($"Cannot squeeze dimension {dimension} with size {tensor.Lengths[dimension]} (must be 1)");

    var newLengths = new nint[tensor.Rank - 1];
    var j = 0;
    for (var i = 0; i < tensor.Rank; ++i)
      if (i != dimension)
        newLengths[j++] = tensor.Lengths[i];

    var data = _DensifyToArray(tensor);
    return Create(data, newLengths);
  }

  /// <summary>Adds a dimension of size 1 at the specified position.</summary>
  public static Tensor<T> Unsqueeze<T>(in ReadOnlyTensorSpan<T> tensor, int dimension) {
    if (dimension < 0 || dimension > tensor.Rank)
      throw new ArgumentOutOfRangeException(nameof(dimension));

    var newLengths = new nint[tensor.Rank + 1];
    var j = 0;
    for (var i = 0; i <= tensor.Rank; ++i)
      if (i == dimension)
        newLengths[i] = 1;
      else
        newLengths[i] = tensor.Lengths[j++];

    var data = _DensifyToArray(tensor);
    return Create(data, newLengths);
  }

  #endregion

  #region Permute/Transpose

  /// <summary>Permutes the dimensions of the tensor according to the specified order.</summary>
  public static Tensor<T> PermuteDimensions<T>(in ReadOnlyTensorSpan<T> tensor, scoped ReadOnlySpan<int> dimensions) {
    if (dimensions.Length != tensor.Rank)
      throw new ArgumentException($"Permutation length {dimensions.Length} does not match tensor rank {tensor.Rank}");

    // Validate permutation is valid
    var seen = new bool[tensor.Rank];
    foreach (var dim in dimensions) {
      if (dim < 0 || dim >= tensor.Rank)
        throw new ArgumentOutOfRangeException(nameof(dimensions), $"Invalid dimension index {dim}");
      if (seen[dim])
        throw new ArgumentException($"Duplicate dimension index {dim}");
      seen[dim] = true;
    }

    // Compute new shape
    var newLengths = new nint[tensor.Rank];
    for (var i = 0; i < tensor.Rank; ++i)
      newLengths[i] = tensor.Lengths[dimensions[i]];

    // Create result and copy data with permuted indices
    var result = CreateFromShape<T>(newLengths);
    var srcSpan = tensor;
    var dstSpan = result.AsTensorSpan();

    // Iterate through all indices in the destination and copy from source
    _PermuteData(srcSpan, dstSpan, dimensions);

    return result;
  }

  private static void _PermuteData<T>(in ReadOnlyTensorSpan<T> source, in TensorSpan<T> destination, ReadOnlySpan<int> permutation) {
    var rank = source.Rank;
    var srcLengths = source.Lengths;
    var dstLengths = destination.Lengths;

    // Simple element-by-element copy for small tensors
    var totalElements = (int)_ComputeFlatLength(dstLengths);
    var dstIndices = new nint[rank];
    var srcIndices = new nint[rank];

    for (var flatIdx = 0; flatIdx < totalElements; ++flatIdx) {
      // Compute destination indices from flat index
      var temp = flatIdx;
      for (var d = rank - 1; d >= 0; --d) {
        dstIndices[d] = temp % (int)dstLengths[d];
        temp /= (int)dstLengths[d];
      }

      // Compute source indices from permutation
      for (var d = 0; d < rank; ++d)
        srcIndices[permutation[d]] = dstIndices[d];

      destination[dstIndices] = source[srcIndices];
    }
  }

  #endregion

  #region Reverse

  /// <summary>Reverses the tensor along all dimensions.</summary>
  public static Tensor<T> Reverse<T>(in ReadOnlyTensorSpan<T> tensor) {
    var result = CreateFromShape<T>(tensor.Lengths);
    var srcSpan = tensor;
    var dstSpan = result.AsTensorSpan();

    var rank = tensor.Rank;
    var lengths = tensor.Lengths;
    var totalElements = (int)_ComputeFlatLength(lengths);
    var indices = new nint[rank];
    var reversedIndices = new nint[rank];

    for (var flatIdx = 0; flatIdx < totalElements; ++flatIdx) {
      // Compute indices from flat index
      var temp = flatIdx;
      for (var d = rank - 1; d >= 0; --d) {
        indices[d] = temp % (int)lengths[d];
        temp /= (int)lengths[d];
      }

      // Reverse all indices
      for (var d = 0; d < rank; ++d)
        reversedIndices[d] = lengths[d] - 1 - indices[d];

      dstSpan[indices] = srcSpan[reversedIndices];
    }

    return result;
  }

  /// <summary>Reverses the tensor along the specified dimension.</summary>
  public static Tensor<T> ReverseDimension<T>(in ReadOnlyTensorSpan<T> tensor, int dimension) {
    if (dimension < 0 || dimension >= tensor.Rank)
      throw new ArgumentOutOfRangeException(nameof(dimension));

    var result = CreateFromShape<T>(tensor.Lengths);
    var srcSpan = tensor;
    var dstSpan = result.AsTensorSpan();

    var rank = tensor.Rank;
    var lengths = tensor.Lengths;
    var totalElements = (int)_ComputeFlatLength(lengths);
    var indices = new nint[rank];
    var reversedIndices = new nint[rank];

    for (var flatIdx = 0; flatIdx < totalElements; ++flatIdx) {
      // Compute indices from flat index
      var temp = flatIdx;
      for (var d = rank - 1; d >= 0; --d) {
        indices[d] = temp % (int)lengths[d];
        temp /= (int)lengths[d];
      }

      // Reverse only the specified dimension
      for (var d = 0; d < rank; ++d)
        reversedIndices[d] = d == dimension ? lengths[d] - 1 - indices[d] : indices[d];

      dstSpan[indices] = srcSpan[reversedIndices];
    }

    return result;
  }

  #endregion

  #region Concatenate

  /// <summary>Concatenates multiple tensors along the first dimension.</summary>
  public static Tensor<T> Concatenate<T>(scoped ReadOnlySpan<Tensor<T>> tensors) {
    return ConcatenateOnDimension(tensors, 0);
  }

  /// <summary>Concatenates multiple tensors along the specified dimension.</summary>
  public static Tensor<T> ConcatenateOnDimension<T>(int dimension, params Tensor<T>[] tensors)
    => ConcatenateOnDimension<T>((ReadOnlySpan<Tensor<T>>)tensors, dimension);

  /// <summary>Concatenates multiple tensors along the specified dimension.</summary>
  public static Tensor<T> ConcatenateOnDimension<T>(scoped ReadOnlySpan<Tensor<T>> tensors, int dimension) {
    if (tensors.Length == 0)
      throw new ArgumentException("At least one tensor is required");

    var firstTensor = tensors[0];
    if (dimension < 0 || dimension >= firstTensor.Rank)
      throw new ArgumentOutOfRangeException(nameof(dimension));

    // Validate all tensors have compatible shapes
    nint totalDimLength = 0;
    foreach (var tensor in tensors) {
      if (tensor.Rank != firstTensor.Rank)
        throw new ArgumentException("All tensors must have the same rank");

      for (var d = 0; d < firstTensor.Rank; ++d)
        if (d != dimension && tensor.Lengths[d] != firstTensor.Lengths[d])
          throw new ArgumentException($"Tensors must have the same size in all dimensions except {dimension}");

      totalDimLength += tensor.Lengths[dimension];
    }

    // Create result tensor
    var resultLengths = new nint[firstTensor.Rank];
    for (var d = 0; d < firstTensor.Rank; ++d)
      resultLengths[d] = d == dimension ? totalDimLength : firstTensor.Lengths[d];

    var result = CreateFromShape<T>(resultLengths);
    var dstSpan = result.AsTensorSpan();

    // Copy each tensor into the result
    nint offset = 0;
    foreach (var tensor in tensors) {
      var srcSpan = tensor.AsReadOnlyTensorSpan();
      _CopyToSlice(srcSpan, dstSpan, dimension, offset);
      offset += tensor.Lengths[dimension];
    }

    return result;
  }

  private static void _CopyToSlice<T>(in ReadOnlyTensorSpan<T> source, in TensorSpan<T> destination, int dimension, nint offset) {
    var rank = source.Rank;
    var srcLengths = source.Lengths;
    var totalElements = (int)_ComputeFlatLength(srcLengths);
    var srcIndices = new nint[rank];
    var dstIndices = new nint[rank];

    for (var flatIdx = 0; flatIdx < totalElements; ++flatIdx) {
      // Compute source indices from flat index
      var temp = flatIdx;
      for (var d = rank - 1; d >= 0; --d) {
        srcIndices[d] = temp % (int)srcLengths[d];
        temp /= (int)srcLengths[d];
      }

      // Compute destination indices (offset in the concat dimension)
      for (var d = 0; d < rank; ++d)
        dstIndices[d] = d == dimension ? srcIndices[d] + offset : srcIndices[d];

      destination[dstIndices] = source[srcIndices];
    }
  }

  /// <summary>Stacks tensors along a new dimension.</summary>
  public static Tensor<T> StackAlongDimension<T>(int dimension, params Tensor<T>[] tensors)
    => StackAlongDimension<T>((ReadOnlySpan<Tensor<T>>)tensors, dimension);

  /// <summary>Stacks tensors along a new dimension.</summary>
  public static Tensor<T> StackAlongDimension<T>(scoped ReadOnlySpan<Tensor<T>> tensors, int dimension) {
    if (tensors.Length == 0)
      throw new ArgumentException("At least one tensor is required");

    var firstTensor = tensors[0];
    if (dimension < 0 || dimension > firstTensor.Rank)
      throw new ArgumentOutOfRangeException(nameof(dimension));

    // Validate all tensors have the same shape
    foreach (var tensor in tensors) {
      if (tensor.Rank != firstTensor.Rank)
        throw new ArgumentException("All tensors must have the same rank");

      for (var d = 0; d < firstTensor.Rank; ++d)
        if (tensor.Lengths[d] != firstTensor.Lengths[d])
          throw new ArgumentException("All tensors must have the same shape");
    }

    // Create result shape with new dimension
    var resultLengths = new nint[firstTensor.Rank + 1];
    var srcIdx = 0;
    for (var d = 0; d <= firstTensor.Rank; ++d)
      if (d == dimension)
        resultLengths[d] = tensors.Length;
      else
        resultLengths[d] = firstTensor.Lengths[srcIdx++];

    var result = CreateFromShape<T>(resultLengths);
    var dstSpan = result.AsTensorSpan();

    // Copy each tensor into the result
    for (var i = 0; i < tensors.Length; ++i) {
      var srcSpan = tensors[i].AsReadOnlyTensorSpan();
      _CopyToStackSlice(srcSpan, dstSpan, dimension, i);
    }

    return result;
  }

  private static void _CopyToStackSlice<T>(in ReadOnlyTensorSpan<T> source, in TensorSpan<T> destination, int stackDimension, int stackIndex) {
    var srcRank = source.Rank;
    var srcLengths = source.Lengths;
    var totalElements = (int)_ComputeFlatLength(srcLengths);
    var srcIndices = new nint[srcRank];
    var dstIndices = new nint[srcRank + 1];

    for (var flatIdx = 0; flatIdx < totalElements; ++flatIdx) {
      // Compute source indices from flat index
      var temp = flatIdx;
      for (var d = srcRank - 1; d >= 0; --d) {
        srcIndices[d] = temp % (int)srcLengths[d];
        temp /= (int)srcLengths[d];
      }

      // Compute destination indices (insert stack dimension)
      var srcIdx = 0;
      for (var d = 0; d <= srcRank; ++d)
        if (d == stackDimension)
          dstIndices[d] = stackIndex;
        else
          dstIndices[d] = srcIndices[srcIdx++];

      destination[dstIndices] = source[srcIndices];
    }
  }

  #endregion

  #region Split

  /// <summary>Splits the tensor into the specified number of equal parts along the first dimension.</summary>
  public static Tensor<T>[] Split<T>(in ReadOnlyTensorSpan<T> tensor, int splitCount) {
    return SplitOnDimension(tensor, splitCount, 0);
  }

  /// <summary>Splits the tensor into the specified number of equal parts along the specified dimension.</summary>
  public static Tensor<T>[] Split<T>(in ReadOnlyTensorSpan<T> tensor, int splitCount, int dimension)
    => SplitOnDimension(tensor, splitCount, dimension);

  /// <summary>Splits the tensor into the specified number of equal parts along the specified dimension.</summary>
  public static Tensor<T>[] SplitOnDimension<T>(in ReadOnlyTensorSpan<T> tensor, int splitCount, int dimension) {
    if (splitCount <= 0)
      throw new ArgumentOutOfRangeException(nameof(splitCount));
    if (dimension < 0 || dimension >= tensor.Rank)
      throw new ArgumentOutOfRangeException(nameof(dimension));

    var dimLength = tensor.Lengths[dimension];
    if (dimLength % splitCount != 0)
      throw new ArgumentException($"Cannot split dimension of size {dimLength} into {splitCount} equal parts");

    var splitSize = dimLength / splitCount;
    var results = new Tensor<T>[splitCount];

    // Create shape for split tensors
    var splitLengths = new nint[tensor.Rank];
    for (var d = 0; d < tensor.Rank; ++d)
      splitLengths[d] = d == dimension ? splitSize : tensor.Lengths[d];

    for (var i = 0; i < splitCount; ++i) {
      var result = CreateFromShape<T>(splitLengths);
      var dstSpan = result.AsTensorSpan();
      _CopyFromSlice(tensor, dstSpan, dimension, i * splitSize);
      results[i] = result;
    }

    return results;
  }

  private static void _CopyFromSlice<T>(in ReadOnlyTensorSpan<T> source, in TensorSpan<T> destination, int dimension, nint offset) {
    var rank = destination.Rank;
    var dstLengths = destination.Lengths;
    var totalElements = (int)_ComputeFlatLength(dstLengths);
    var srcIndices = new nint[rank];
    var dstIndices = new nint[rank];

    for (var flatIdx = 0; flatIdx < totalElements; ++flatIdx) {
      // Compute destination indices from flat index
      var temp = flatIdx;
      for (var d = rank - 1; d >= 0; --d) {
        dstIndices[d] = temp % (int)dstLengths[d];
        temp /= (int)dstLengths[d];
      }

      // Compute source indices (offset in the split dimension)
      for (var d = 0; d < rank; ++d)
        srcIndices[d] = d == dimension ? dstIndices[d] + offset : dstIndices[d];

      destination[dstIndices] = source[srcIndices];
    }
  }

  #endregion

}

#endif
