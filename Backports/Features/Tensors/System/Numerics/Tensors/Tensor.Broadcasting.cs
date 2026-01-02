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

public static partial class Tensor {

  /// <summary>Broadcasts the tensor to the specified shape.</summary>
  public static Tensor<T> Broadcast<T>(in ReadOnlyTensorSpan<T> tensor, scoped ReadOnlySpan<nint> lengths) {
    if (!_CanBroadcast(tensor.Lengths, lengths))
      throw new ArgumentException("Tensor cannot be broadcast to the specified shape");

    var result = CreateFromShape<T>(lengths);
    BroadcastTo(tensor, result.AsTensorSpan());
    return result;
  }

  /// <summary>Broadcasts the tensor to the shape of the destination.</summary>
  public static void BroadcastTo<T>(scoped in ReadOnlyTensorSpan<T> tensor, in TensorSpan<T> destination) {
    if (!_CanBroadcast(tensor.Lengths, destination.Lengths))
      throw new ArgumentException("Tensor cannot be broadcast to the destination shape");

    _BroadcastCopy(tensor, destination);
  }

  /// <summary>Attempts to broadcast the tensor to the shape of the destination.</summary>
  public static bool TryBroadcastTo<T>(scoped in ReadOnlyTensorSpan<T> tensor, in TensorSpan<T> destination) {
    if (!_CanBroadcast(tensor.Lengths, destination.Lengths))
      return false;

    _BroadcastCopy(tensor, destination);
    return true;
  }

  /// <summary>Gets the broadcast shape for two tensors.</summary>
  public static nint[] GetBroadcastedLengths(ReadOnlySpan<nint> shape1, ReadOnlySpan<nint> shape2) {
    var maxRank = Math.Max(shape1.Length, shape2.Length);
    var result = new nint[maxRank];

    for (var i = 0; i < maxRank; ++i) {
      var dim1 = i < shape1.Length ? shape1[shape1.Length - 1 - i] : 1;
      var dim2 = i < shape2.Length ? shape2[shape2.Length - 1 - i] : 1;

      if (dim1 != dim2 && dim1 != 1 && dim2 != 1)
        throw new ArgumentException($"Shapes are not broadcastable at dimension {maxRank - 1 - i}: {dim1} vs {dim2}");

      result[maxRank - 1 - i] = dim1 > dim2 ? dim1 : dim2;
    }

    return result;
  }

  /// <summary>Checks if source shape can be broadcast to target shape.</summary>
  private static bool _CanBroadcast(ReadOnlySpan<nint> source, ReadOnlySpan<nint> target) {
    // Source rank must be <= target rank
    if (source.Length > target.Length)
      return false;

    // Check each dimension from right to left
    var srcOffset = source.Length - 1;
    var tgtOffset = target.Length - 1;

    for (var i = 0; i < source.Length; ++i) {
      var srcDim = source[srcOffset - i];
      var tgtDim = target[tgtOffset - i];

      // Dimensions are compatible if equal or source is 1
      if (srcDim != tgtDim && srcDim != 1)
        return false;
    }

    return true;
  }

  /// <summary>Copies data from source to destination with broadcasting.</summary>
  private static void _BroadcastCopy<T>(in ReadOnlyTensorSpan<T> source, in TensorSpan<T> destination) {
    var srcLengths = source.Lengths;
    var dstLengths = destination.Lengths;
    var dstRank = dstLengths.Length;
    var srcRank = srcLengths.Length;

    var totalElements = (int)_ComputeFlatLength(dstLengths);
    var dstIndices = new nint[dstRank];
    var srcIndices = new nint[srcRank];

    for (var flatIdx = 0; flatIdx < totalElements; ++flatIdx) {
      // Compute destination indices from flat index
      var temp = flatIdx;
      for (var d = dstRank - 1; d >= 0; --d) {
        dstIndices[d] = temp % (int)dstLengths[d];
        temp /= (int)dstLengths[d];
      }

      // Compute source indices with broadcasting
      // Map destination indices to source indices, wrapping dimensions of size 1
      for (var d = 0; d < srcRank; ++d) {
        var dstDim = dstRank - srcRank + d;
        var srcDimLen = srcLengths[d];
        srcIndices[d] = srcDimLen == 1 ? 0 : dstIndices[dstDim];
      }

      destination[dstIndices] = source[srcIndices];
    }
  }

}

#endif
