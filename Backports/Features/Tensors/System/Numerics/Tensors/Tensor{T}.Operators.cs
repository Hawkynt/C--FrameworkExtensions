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

public sealed partial class Tensor<T> {

  #region Arithmetic Operators

  /// <summary>Adds two tensors element-wise.</summary>
  public static Tensor<T> operator +(Tensor<T> left, Tensor<T> right)
    => Tensor.Add<T>(left, right);

  /// <summary>Adds a scalar to each element of a tensor.</summary>
  public static Tensor<T> operator +(Tensor<T> left, T right)
    => Tensor.Add(left.AsReadOnlyTensorSpan(), right);

  /// <summary>Adds a scalar to each element of a tensor.</summary>
  public static Tensor<T> operator +(T left, Tensor<T> right)
    => Tensor.Add(right.AsReadOnlyTensorSpan(), left);

  /// <summary>Subtracts two tensors element-wise.</summary>
  public static Tensor<T> operator -(Tensor<T> left, Tensor<T> right)
    => Tensor.Subtract<T>(left, right);

  /// <summary>Subtracts a scalar from each element of a tensor.</summary>
  public static Tensor<T> operator -(Tensor<T> left, T right)
    => Tensor.Subtract(left.AsReadOnlyTensorSpan(), right);

  /// <summary>Subtracts each element of a tensor from a scalar.</summary>
  public static Tensor<T> operator -(T left, Tensor<T> right)
    => Tensor.Subtract(left, right.AsReadOnlyTensorSpan());

  /// <summary>Negates each element of a tensor.</summary>
  public static Tensor<T> operator -(Tensor<T> tensor)
    => Tensor.Negate<T>(tensor);

  /// <summary>Returns the tensor unchanged (unary plus).</summary>
  public static Tensor<T> operator +(Tensor<T> tensor)
    => tensor;

  /// <summary>Multiplies two tensors element-wise.</summary>
  public static Tensor<T> operator *(Tensor<T> left, Tensor<T> right)
    => Tensor.Multiply<T>(left, right);

  /// <summary>Multiplies each element of a tensor by a scalar.</summary>
  public static Tensor<T> operator *(Tensor<T> left, T right)
    => Tensor.Multiply(left.AsReadOnlyTensorSpan(), right);

  /// <summary>Multiplies each element of a tensor by a scalar.</summary>
  public static Tensor<T> operator *(T left, Tensor<T> right)
    => Tensor.Multiply(right.AsReadOnlyTensorSpan(), left);

  /// <summary>Divides two tensors element-wise.</summary>
  public static Tensor<T> operator /(Tensor<T> left, Tensor<T> right)
    => Tensor.Divide<T>(left, right);

  /// <summary>Divides each element of a tensor by a scalar.</summary>
  public static Tensor<T> operator /(Tensor<T> left, T right)
    => Tensor.Divide(left.AsReadOnlyTensorSpan(), right);

  /// <summary>Divides a scalar by each element of a tensor.</summary>
  public static Tensor<T> operator /(T left, Tensor<T> right)
    => Tensor.Divide(left, right.AsReadOnlyTensorSpan());

  #endregion

  #region Bitwise Operators

  /// <summary>Computes element-wise bitwise AND.</summary>
  public static Tensor<T> operator &(Tensor<T> left, Tensor<T> right)
    => Tensor.BitwiseAnd(left.AsReadOnlyTensorSpan(), right.AsReadOnlyTensorSpan());

  /// <summary>Computes element-wise bitwise OR.</summary>
  public static Tensor<T> operator |(Tensor<T> left, Tensor<T> right)
    => Tensor.BitwiseOr(left.AsReadOnlyTensorSpan(), right.AsReadOnlyTensorSpan());

  /// <summary>Computes element-wise bitwise XOR.</summary>
  public static Tensor<T> operator ^(Tensor<T> left, Tensor<T> right)
    => Tensor.Xor(left.AsReadOnlyTensorSpan(), right.AsReadOnlyTensorSpan());

  /// <summary>Computes element-wise bitwise NOT (ones complement).</summary>
  public static Tensor<T> operator ~(Tensor<T> tensor)
    => Tensor.OnesComplement(tensor.AsReadOnlyTensorSpan());

  #endregion

  #region Shift Operators

  /// <summary>Shifts each element left by the specified amount.</summary>
  public static Tensor<T> operator <<(Tensor<T> tensor, int shiftAmount)
    => Tensor.ShiftLeft(tensor.AsReadOnlyTensorSpan(), shiftAmount);

  /// <summary>Shifts each element right arithmetically by the specified amount.</summary>
  public static Tensor<T> operator >>(Tensor<T> tensor, int shiftAmount)
    => Tensor.ShiftRightArithmetic(tensor.AsReadOnlyTensorSpan(), shiftAmount);

  /// <summary>Shifts each element right logically by the specified amount.</summary>
  public static Tensor<T> operator >>>(Tensor<T> tensor, int shiftAmount)
    => Tensor.ShiftRightLogical(tensor.AsReadOnlyTensorSpan(), shiftAmount);

  #endregion

}

#endif
