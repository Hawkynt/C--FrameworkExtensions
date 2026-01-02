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
using System.Runtime.Intrinsics;

namespace System.Numerics.Tensors;

public static partial class Tensor {

  #region Equality Comparisons

  /// <summary>Returns true if all corresponding elements are equal.</summary>
  public static bool EqualsAll<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);

    var comparer = EqualityComparer<T>.Default;
    for (var i = 0; i < xFlat.Length; ++i)
      if (!comparer.Equals(xFlat[i], yFlat[i]))
        return false;

    return true;
  }

  /// <summary>Returns true if any corresponding elements are equal.</summary>
  public static bool EqualsAny<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);

    var comparer = EqualityComparer<T>.Default;
    for (var i = 0; i < xFlat.Length; ++i)
      if (comparer.Equals(xFlat[i], yFlat[i]))
        return true;

    return false;
  }

  #endregion

  #region Less Than Comparisons

  /// <summary>Returns a boolean tensor where each element indicates if x[i] less than y[i].</summary>
  public static Tensor<bool> LessThan<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var result = CreateFromShape<bool>(x.Lengths);
    LessThan(x, y, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise less-than comparison into destination.</summary>
  public static void LessThan<T>(scoped in ReadOnlyTensorSpan<T> x, scoped in ReadOnlyTensorSpan<T> y, in TensorSpan<bool> destination) {
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);
    var dest = _GetFlatSpan(destination);

    for (var i = 0; i < xFlat.Length; ++i)
      dest[i] = Scalar<T>.LessThan(xFlat[i], yFlat[i]);
  }

  /// <summary>Returns true if all elements of x are less than corresponding elements of y.</summary>
  public static bool LessThanAll<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);

    for (var i = 0; i < xFlat.Length; ++i)
      if (!Scalar<T>.LessThan(xFlat[i], yFlat[i]))
        return false;

    return true;
  }

  /// <summary>Returns true if any element of x is less than corresponding element of y.</summary>
  public static bool LessThanAny<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);

    for (var i = 0; i < xFlat.Length; ++i)
      if (Scalar<T>.LessThan(xFlat[i], yFlat[i]))
        return true;

    return false;
  }

  #endregion

  #region Less Than Or Equal Comparisons

  /// <summary>Returns a boolean tensor where each element indicates if x[i] less than or equal to y[i].</summary>
  public static Tensor<bool> LessThanOrEqual<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var result = CreateFromShape<bool>(x.Lengths);
    LessThanOrEqual(x, y, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise less-than-or-equal comparison into destination.</summary>
  public static void LessThanOrEqual<T>(scoped in ReadOnlyTensorSpan<T> x, scoped in ReadOnlyTensorSpan<T> y, in TensorSpan<bool> destination) {
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);
    var dest = _GetFlatSpan(destination);

    for (var i = 0; i < xFlat.Length; ++i)
      dest[i] = Scalar<T>.LessThanOrEqual(xFlat[i], yFlat[i]);
  }

  /// <summary>Returns true if all elements of x are less than or equal to corresponding elements of y.</summary>
  public static bool LessThanOrEqualAll<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);

    for (var i = 0; i < xFlat.Length; ++i)
      if (!Scalar<T>.LessThanOrEqual(xFlat[i], yFlat[i]))
        return false;

    return true;
  }

  /// <summary>Returns true if any element of x is less than or equal to corresponding element of y.</summary>
  public static bool LessThanOrEqualAny<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);

    for (var i = 0; i < xFlat.Length; ++i)
      if (Scalar<T>.LessThanOrEqual(xFlat[i], yFlat[i]))
        return true;

    return false;
  }

  #endregion

  #region Greater Than Comparisons

  /// <summary>Returns a boolean tensor where each element indicates if x[i] greater than y[i].</summary>
  public static Tensor<bool> GreaterThan<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var result = CreateFromShape<bool>(x.Lengths);
    GreaterThan(x, y, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise greater-than comparison into destination.</summary>
  public static void GreaterThan<T>(scoped in ReadOnlyTensorSpan<T> x, scoped in ReadOnlyTensorSpan<T> y, in TensorSpan<bool> destination) {
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);
    var dest = _GetFlatSpan(destination);

    for (var i = 0; i < xFlat.Length; ++i)
      dest[i] = Scalar<T>.GreaterThan(xFlat[i], yFlat[i]);
  }

  /// <summary>Returns true if all elements of x are greater than corresponding elements of y.</summary>
  public static bool GreaterThanAll<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);

    for (var i = 0; i < xFlat.Length; ++i)
      if (!Scalar<T>.GreaterThan(xFlat[i], yFlat[i]))
        return false;

    return true;
  }

  /// <summary>Returns true if any element of x is greater than corresponding element of y.</summary>
  public static bool GreaterThanAny<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);

    for (var i = 0; i < xFlat.Length; ++i)
      if (Scalar<T>.GreaterThan(xFlat[i], yFlat[i]))
        return true;

    return false;
  }

  #endregion

  #region Greater Than Or Equal Comparisons

  /// <summary>Returns a boolean tensor where each element indicates if x[i] greater than or equal to y[i].</summary>
  public static Tensor<bool> GreaterThanOrEqual<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var result = CreateFromShape<bool>(x.Lengths);
    GreaterThanOrEqual(x, y, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise greater-than-or-equal comparison into destination.</summary>
  public static void GreaterThanOrEqual<T>(scoped in ReadOnlyTensorSpan<T> x, scoped in ReadOnlyTensorSpan<T> y, in TensorSpan<bool> destination) {
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);
    var dest = _GetFlatSpan(destination);

    for (var i = 0; i < xFlat.Length; ++i)
      dest[i] = Scalar<T>.GreaterThanOrEqual(xFlat[i], yFlat[i]);
  }

  /// <summary>Returns true if all elements of x are greater than or equal to corresponding elements of y.</summary>
  public static bool GreaterThanOrEqualAll<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);

    for (var i = 0; i < xFlat.Length; ++i)
      if (!Scalar<T>.GreaterThanOrEqual(xFlat[i], yFlat[i]))
        return false;

    return true;
  }

  /// <summary>Returns true if any element of x is greater than or equal to corresponding element of y.</summary>
  public static bool GreaterThanOrEqualAny<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);

    for (var i = 0; i < xFlat.Length; ++i)
      if (Scalar<T>.GreaterThanOrEqual(xFlat[i], yFlat[i]))
        return true;

    return false;
  }

  #endregion

}

#endif
