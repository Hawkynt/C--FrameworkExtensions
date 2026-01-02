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

  #region Basic Aggregations

  /// <summary>Computes the sum of all elements.</summary>
  public static T Sum<T>(in ReadOnlyTensorSpan<T> x) {
    var xFlat = _GetFlatSpan(x);
    return TensorPrimitives.Sum(xFlat);
  }

  /// <summary>Computes the product of all elements.</summary>
  public static T Product<T>(in ReadOnlyTensorSpan<T> x) {
    var xFlat = _GetFlatSpan(x);
    return TensorPrimitives.Product(xFlat);
  }

  /// <summary>Computes the average of all elements.</summary>
  public static T Average<T>(in ReadOnlyTensorSpan<T> x) {
    var xFlat = _GetFlatSpan(x);
    return TensorPrimitives.Average(xFlat);
  }

  #endregion

  #region Min/Max

  /// <summary>Returns the minimum element.</summary>
  public static T Min<T>(in ReadOnlyTensorSpan<T> x) {
    var xFlat = _GetFlatSpan(x);
    return TensorPrimitives.Min(xFlat);
  }

  /// <summary>Returns the maximum element.</summary>
  public static T Max<T>(in ReadOnlyTensorSpan<T> x) {
    var xFlat = _GetFlatSpan(x);
    return TensorPrimitives.Max(xFlat);
  }

  /// <summary>Returns the element with minimum magnitude (absolute value).</summary>
  public static T MinMagnitude<T>(in ReadOnlyTensorSpan<T> x) {
    var xFlat = _GetFlatSpan(x);
    return TensorPrimitives.MinMagnitude(xFlat);
  }

  /// <summary>Returns the element with maximum magnitude (absolute value).</summary>
  public static T MaxMagnitude<T>(in ReadOnlyTensorSpan<T> x) {
    var xFlat = _GetFlatSpan(x);
    return TensorPrimitives.MaxMagnitude(xFlat);
  }

  /// <summary>Returns the minimum element, ignoring NaN.</summary>
  public static T MinNumber<T>(in ReadOnlyTensorSpan<T> x) {
    var xFlat = _GetFlatSpan(x);
    return TensorPrimitives.MinNumber(xFlat);
  }

  /// <summary>Returns the maximum element, ignoring NaN.</summary>
  public static T MaxNumber<T>(in ReadOnlyTensorSpan<T> x) {
    var xFlat = _GetFlatSpan(x);
    return TensorPrimitives.MaxNumber(xFlat);
  }

  #endregion

  #region Index Operations

  /// <summary>Returns the index of the minimum element.</summary>
  public static nint IndexOfMin<T>(in ReadOnlyTensorSpan<T> x) {
    var xFlat = _GetFlatSpan(x);
    return TensorPrimitives.IndexOfMin(xFlat);
  }

  /// <summary>Returns the index of the maximum element.</summary>
  public static nint IndexOfMax<T>(in ReadOnlyTensorSpan<T> x) {
    var xFlat = _GetFlatSpan(x);
    return TensorPrimitives.IndexOfMax(xFlat);
  }

  /// <summary>Returns the index of the element with minimum magnitude.</summary>
  public static nint IndexOfMinMagnitude<T>(in ReadOnlyTensorSpan<T> x) {
    var xFlat = _GetFlatSpan(x);
    return TensorPrimitives.IndexOfMinMagnitude(xFlat);
  }

  /// <summary>Returns the index of the element with maximum magnitude.</summary>
  public static nint IndexOfMaxMagnitude<T>(in ReadOnlyTensorSpan<T> x) {
    var xFlat = _GetFlatSpan(x);
    return TensorPrimitives.IndexOfMaxMagnitude(xFlat);
  }

  #endregion

  #region Vector Operations

  /// <summary>Computes the dot product of two tensors.</summary>
  public static T Dot<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);
    return TensorPrimitives.Dot(xFlat, yFlat);
  }

  /// <summary>Computes the Euclidean distance between two tensors.</summary>
  public static T Distance<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);
    return TensorPrimitives.Distance(xFlat, yFlat);
  }

  /// <summary>Computes the L2 (Euclidean) norm of a tensor.</summary>
  public static T Norm<T>(in ReadOnlyTensorSpan<T> x) {
    var xFlat = _GetFlatSpan(x);
    return TensorPrimitives.Norm(xFlat);
  }

  /// <summary>Computes the cosine similarity between two tensors.</summary>
  public static T CosineSimilarity<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);
    return TensorPrimitives.CosineSimilarity(xFlat, yFlat);
  }

  #endregion

  #region Element-wise Min/Max

  /// <summary>Computes element-wise minimum of two tensors.</summary>
  public static Tensor<T> Min<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var result = CreateFromShape<T>(x.Lengths);
    Min(x, y, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise minimum of two tensors into destination.</summary>
  public static void Min<T>(scoped in ReadOnlyTensorSpan<T> x, scoped in ReadOnlyTensorSpan<T> y, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);
    TensorPrimitives.Min(xFlat, yFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise maximum of two tensors.</summary>
  public static Tensor<T> Max<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var result = CreateFromShape<T>(x.Lengths);
    Max(x, y, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise maximum of two tensors into destination.</summary>
  public static void Max<T>(scoped in ReadOnlyTensorSpan<T> x, scoped in ReadOnlyTensorSpan<T> y, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);
    TensorPrimitives.Max(xFlat, yFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise minimum magnitude of two tensors.</summary>
  public static Tensor<T> MinMagnitude<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var result = CreateFromShape<T>(x.Lengths);
    MinMagnitude(x, y, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise minimum magnitude of two tensors into destination.</summary>
  public static void MinMagnitude<T>(scoped in ReadOnlyTensorSpan<T> x, scoped in ReadOnlyTensorSpan<T> y, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);
    TensorPrimitives.MinMagnitude(xFlat, yFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise maximum magnitude of two tensors.</summary>
  public static Tensor<T> MaxMagnitude<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var result = CreateFromShape<T>(x.Lengths);
    MaxMagnitude(x, y, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise maximum magnitude of two tensors into destination.</summary>
  public static void MaxMagnitude<T>(scoped in ReadOnlyTensorSpan<T> x, scoped in ReadOnlyTensorSpan<T> y, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);
    TensorPrimitives.MaxMagnitude(xFlat, yFlat, _GetFlatSpan(destination));
  }

  #endregion

}

#endif
