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

#if !SUPPORTS_FMADD

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics.X86;

/// <summary>
/// Software fallback implementation of FMA (Fused Multiply-Add) intrinsics for platforms without native support.
/// Provides fused multiply-add operations that compute (a*b+c) and variants with a single rounding step.
/// </summary>
public abstract class Fma : Avx {

  /// <summary>Gets a value indicating whether FMA instructions are supported.</summary>
  public new static bool IsSupported => false;

  #region MultiplyAdd (a*b+c)

  /// <summary>
  /// Multiplies packed single-precision (32-bit) floating-point elements in a and b,
  /// and adds the intermediate result to packed elements in c.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> MultiplyAdd(Vector128<float> a, Vector128<float> b, Vector128<float> c) {
    var result = Vector128<float>.Zero;
    for (var i = 0; i < Vector128<float>.Count; ++i)
      result = result.WithElement(i, (float)((double)a[i] * b[i] + c[i]));
    return result;
  }

  /// <summary>
  /// Multiplies packed double-precision (64-bit) floating-point elements in a and b,
  /// and adds the intermediate result to packed elements in c.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> MultiplyAdd(Vector128<double> a, Vector128<double> b, Vector128<double> c) {
    var result = Vector128<double>.Zero;
    for (var i = 0; i < Vector128<double>.Count; ++i)
      result = result.WithElement(i, a[i] * b[i] + c[i]);
    return result;
  }

  /// <summary>
  /// Multiplies packed single-precision (32-bit) floating-point elements in a and b,
  /// and adds the intermediate result to packed elements in c.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> MultiplyAdd(Vector256<float> a, Vector256<float> b, Vector256<float> c) {
    var result = Vector256<float>.Zero;
    for (var i = 0; i < Vector256<float>.Count; ++i)
      result = result.WithElement(i, (float)((double)a[i] * b[i] + c[i]));
    return result;
  }

  /// <summary>
  /// Multiplies packed double-precision (64-bit) floating-point elements in a and b,
  /// and adds the intermediate result to packed elements in c.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> MultiplyAdd(Vector256<double> a, Vector256<double> b, Vector256<double> c) {
    var result = Vector256<double>.Zero;
    for (var i = 0; i < Vector256<double>.Count; ++i)
      result = result.WithElement(i, a[i] * b[i] + c[i]);
    return result;
  }

  /// <summary>
  /// Multiplies the lowest single-precision (32-bit) floating-point elements in a and b,
  /// adds the intermediate result to the lowest element in c, and stores the result in the
  /// lowest element of the destination. The remaining elements are copied from a.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> MultiplyAddScalar(Vector128<float> a, Vector128<float> b, Vector128<float> c)
    => a.WithElement(0, (float)((double)a[0] * b[0] + c[0]));

  /// <summary>
  /// Multiplies the lowest double-precision (64-bit) floating-point elements in a and b,
  /// adds the intermediate result to the lowest element in c, and stores the result in the
  /// lowest element of the destination. The remaining elements are copied from a.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> MultiplyAddScalar(Vector128<double> a, Vector128<double> b, Vector128<double> c)
    => a.WithElement(0, a[0] * b[0] + c[0]);

  #endregion

  #region MultiplyAddNegated (-(a*b)+c)

  /// <summary>
  /// Multiplies packed single-precision (32-bit) floating-point elements in a and b,
  /// negates the intermediate result, and adds it to packed elements in c.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> MultiplyAddNegated(Vector128<float> a, Vector128<float> b, Vector128<float> c) {
    var result = Vector128<float>.Zero;
    for (var i = 0; i < Vector128<float>.Count; ++i)
      result = result.WithElement(i, (float)(-(double)a[i] * b[i] + c[i]));
    return result;
  }

  /// <summary>
  /// Multiplies packed double-precision (64-bit) floating-point elements in a and b,
  /// negates the intermediate result, and adds it to packed elements in c.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> MultiplyAddNegated(Vector128<double> a, Vector128<double> b, Vector128<double> c) {
    var result = Vector128<double>.Zero;
    for (var i = 0; i < Vector128<double>.Count; ++i)
      result = result.WithElement(i, -(a[i] * b[i]) + c[i]);
    return result;
  }

  /// <summary>
  /// Multiplies packed single-precision (32-bit) floating-point elements in a and b,
  /// negates the intermediate result, and adds it to packed elements in c.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> MultiplyAddNegated(Vector256<float> a, Vector256<float> b, Vector256<float> c) {
    var result = Vector256<float>.Zero;
    for (var i = 0; i < Vector256<float>.Count; ++i)
      result = result.WithElement(i, (float)(-(double)a[i] * b[i] + c[i]));
    return result;
  }

  /// <summary>
  /// Multiplies packed double-precision (64-bit) floating-point elements in a and b,
  /// negates the intermediate result, and adds it to packed elements in c.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> MultiplyAddNegated(Vector256<double> a, Vector256<double> b, Vector256<double> c) {
    var result = Vector256<double>.Zero;
    for (var i = 0; i < Vector256<double>.Count; ++i)
      result = result.WithElement(i, -(a[i] * b[i]) + c[i]);
    return result;
  }

  /// <summary>
  /// Multiplies the lowest single-precision (32-bit) floating-point elements in a and b,
  /// negates the intermediate result, adds it to the lowest element in c, and stores the result
  /// in the lowest element of the destination. The remaining elements are copied from a.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> MultiplyAddNegatedScalar(Vector128<float> a, Vector128<float> b, Vector128<float> c)
    => a.WithElement(0, (float)(-(double)a[0] * b[0] + c[0]));

  /// <summary>
  /// Multiplies the lowest double-precision (64-bit) floating-point elements in a and b,
  /// negates the intermediate result, adds it to the lowest element in c, and stores the result
  /// in the lowest element of the destination. The remaining elements are copied from a.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> MultiplyAddNegatedScalar(Vector128<double> a, Vector128<double> b, Vector128<double> c)
    => a.WithElement(0, -(a[0] * b[0]) + c[0]);

  #endregion

  #region MultiplyAddSubtract (a*b+c[i] for even i, a*b-c[i] for odd i)

  /// <summary>
  /// Multiplies packed single-precision (32-bit) floating-point elements in a and b,
  /// and alternatively adds and subtracts packed elements in c to/from the intermediate result.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> MultiplyAddSubtract(Vector128<float> a, Vector128<float> b, Vector128<float> c) {
    var result = Vector128<float>.Zero;
    for (var i = 0; i < Vector128<float>.Count; ++i)
      result = result.WithElement(i, (i & 1) == 0
        ? (float)((double)a[i] * b[i] + c[i])
        : (float)((double)a[i] * b[i] - c[i]));
    return result;
  }

  /// <summary>
  /// Multiplies packed double-precision (64-bit) floating-point elements in a and b,
  /// and alternatively adds and subtracts packed elements in c to/from the intermediate result.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> MultiplyAddSubtract(Vector128<double> a, Vector128<double> b, Vector128<double> c) {
    var result = Vector128<double>.Zero;
    for (var i = 0; i < Vector128<double>.Count; ++i)
      result = result.WithElement(i, (i & 1) == 0
        ? a[i] * b[i] + c[i]
        : a[i] * b[i] - c[i]);
    return result;
  }

  /// <summary>
  /// Multiplies packed single-precision (32-bit) floating-point elements in a and b,
  /// and alternatively adds and subtracts packed elements in c to/from the intermediate result.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> MultiplyAddSubtract(Vector256<float> a, Vector256<float> b, Vector256<float> c) {
    var result = Vector256<float>.Zero;
    for (var i = 0; i < Vector256<float>.Count; ++i)
      result = result.WithElement(i, (i & 1) == 0
        ? (float)((double)a[i] * b[i] + c[i])
        : (float)((double)a[i] * b[i] - c[i]));
    return result;
  }

  /// <summary>
  /// Multiplies packed double-precision (64-bit) floating-point elements in a and b,
  /// and alternatively adds and subtracts packed elements in c to/from the intermediate result.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> MultiplyAddSubtract(Vector256<double> a, Vector256<double> b, Vector256<double> c) {
    var result = Vector256<double>.Zero;
    for (var i = 0; i < Vector256<double>.Count; ++i)
      result = result.WithElement(i, (i & 1) == 0
        ? a[i] * b[i] + c[i]
        : a[i] * b[i] - c[i]);
    return result;
  }

  #endregion

  #region MultiplySubtract (a*b-c)

  /// <summary>
  /// Multiplies packed single-precision (32-bit) floating-point elements in a and b,
  /// and subtracts packed elements in c from the intermediate result.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> MultiplySubtract(Vector128<float> a, Vector128<float> b, Vector128<float> c) {
    var result = Vector128<float>.Zero;
    for (var i = 0; i < Vector128<float>.Count; ++i)
      result = result.WithElement(i, (float)((double)a[i] * b[i] - c[i]));
    return result;
  }

  /// <summary>
  /// Multiplies packed double-precision (64-bit) floating-point elements in a and b,
  /// and subtracts packed elements in c from the intermediate result.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> MultiplySubtract(Vector128<double> a, Vector128<double> b, Vector128<double> c) {
    var result = Vector128<double>.Zero;
    for (var i = 0; i < Vector128<double>.Count; ++i)
      result = result.WithElement(i, a[i] * b[i] - c[i]);
    return result;
  }

  /// <summary>
  /// Multiplies packed single-precision (32-bit) floating-point elements in a and b,
  /// and subtracts packed elements in c from the intermediate result.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> MultiplySubtract(Vector256<float> a, Vector256<float> b, Vector256<float> c) {
    var result = Vector256<float>.Zero;
    for (var i = 0; i < Vector256<float>.Count; ++i)
      result = result.WithElement(i, (float)((double)a[i] * b[i] - c[i]));
    return result;
  }

  /// <summary>
  /// Multiplies packed double-precision (64-bit) floating-point elements in a and b,
  /// and subtracts packed elements in c from the intermediate result.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> MultiplySubtract(Vector256<double> a, Vector256<double> b, Vector256<double> c) {
    var result = Vector256<double>.Zero;
    for (var i = 0; i < Vector256<double>.Count; ++i)
      result = result.WithElement(i, a[i] * b[i] - c[i]);
    return result;
  }

  /// <summary>
  /// Multiplies the lowest single-precision (32-bit) floating-point elements in a and b,
  /// subtracts the lowest element in c from the intermediate result, and stores the result
  /// in the lowest element of the destination. The remaining elements are copied from a.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> MultiplySubtractScalar(Vector128<float> a, Vector128<float> b, Vector128<float> c)
    => a.WithElement(0, (float)((double)a[0] * b[0] - c[0]));

  /// <summary>
  /// Multiplies the lowest double-precision (64-bit) floating-point elements in a and b,
  /// subtracts the lowest element in c from the intermediate result, and stores the result
  /// in the lowest element of the destination. The remaining elements are copied from a.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> MultiplySubtractScalar(Vector128<double> a, Vector128<double> b, Vector128<double> c)
    => a.WithElement(0, a[0] * b[0] - c[0]);

  #endregion

  #region MultiplySubtractAdd (a*b-c[i] for even i, a*b+c[i] for odd i)

  /// <summary>
  /// Multiplies packed single-precision (32-bit) floating-point elements in a and b,
  /// and alternatively subtracts and adds packed elements in c from/to the intermediate result.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> MultiplySubtractAdd(Vector128<float> a, Vector128<float> b, Vector128<float> c) {
    var result = Vector128<float>.Zero;
    for (var i = 0; i < Vector128<float>.Count; ++i)
      result = result.WithElement(i, (i & 1) == 0
        ? (float)((double)a[i] * b[i] - c[i])
        : (float)((double)a[i] * b[i] + c[i]));
    return result;
  }

  /// <summary>
  /// Multiplies packed double-precision (64-bit) floating-point elements in a and b,
  /// and alternatively subtracts and adds packed elements in c from/to the intermediate result.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> MultiplySubtractAdd(Vector128<double> a, Vector128<double> b, Vector128<double> c) {
    var result = Vector128<double>.Zero;
    for (var i = 0; i < Vector128<double>.Count; ++i)
      result = result.WithElement(i, (i & 1) == 0
        ? a[i] * b[i] - c[i]
        : a[i] * b[i] + c[i]);
    return result;
  }

  /// <summary>
  /// Multiplies packed single-precision (32-bit) floating-point elements in a and b,
  /// and alternatively subtracts and adds packed elements in c from/to the intermediate result.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> MultiplySubtractAdd(Vector256<float> a, Vector256<float> b, Vector256<float> c) {
    var result = Vector256<float>.Zero;
    for (var i = 0; i < Vector256<float>.Count; ++i)
      result = result.WithElement(i, (i & 1) == 0
        ? (float)((double)a[i] * b[i] - c[i])
        : (float)((double)a[i] * b[i] + c[i]));
    return result;
  }

  /// <summary>
  /// Multiplies packed double-precision (64-bit) floating-point elements in a and b,
  /// and alternatively subtracts and adds packed elements in c from/to the intermediate result.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> MultiplySubtractAdd(Vector256<double> a, Vector256<double> b, Vector256<double> c) {
    var result = Vector256<double>.Zero;
    for (var i = 0; i < Vector256<double>.Count; ++i)
      result = result.WithElement(i, (i & 1) == 0
        ? a[i] * b[i] - c[i]
        : a[i] * b[i] + c[i]);
    return result;
  }

  #endregion

  #region MultiplySubtractNegated (-(a*b-c))

  /// <summary>
  /// Multiplies packed single-precision (32-bit) floating-point elements in a and b,
  /// subtracts packed elements in c from the intermediate result, and negates the result.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> MultiplySubtractNegated(Vector128<float> a, Vector128<float> b, Vector128<float> c) {
    var result = Vector128<float>.Zero;
    for (var i = 0; i < Vector128<float>.Count; ++i)
      result = result.WithElement(i, (float)(-(double)a[i] * b[i] - c[i]));
    return result;
  }

  /// <summary>
  /// Multiplies packed double-precision (64-bit) floating-point elements in a and b,
  /// subtracts packed elements in c from the intermediate result, and negates the result.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> MultiplySubtractNegated(Vector128<double> a, Vector128<double> b, Vector128<double> c) {
    var result = Vector128<double>.Zero;
    for (var i = 0; i < Vector128<double>.Count; ++i)
      result = result.WithElement(i, -(a[i] * b[i]) - c[i]);
    return result;
  }

  /// <summary>
  /// Multiplies packed single-precision (32-bit) floating-point elements in a and b,
  /// subtracts packed elements in c from the intermediate result, and negates the result.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<float> MultiplySubtractNegated(Vector256<float> a, Vector256<float> b, Vector256<float> c) {
    var result = Vector256<float>.Zero;
    for (var i = 0; i < Vector256<float>.Count; ++i)
      result = result.WithElement(i, (float)(-(double)a[i] * b[i] - c[i]));
    return result;
  }

  /// <summary>
  /// Multiplies packed double-precision (64-bit) floating-point elements in a and b,
  /// subtracts packed elements in c from the intermediate result, and negates the result.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector256<double> MultiplySubtractNegated(Vector256<double> a, Vector256<double> b, Vector256<double> c) {
    var result = Vector256<double>.Zero;
    for (var i = 0; i < Vector256<double>.Count; ++i)
      result = result.WithElement(i, -(a[i] * b[i]) - c[i]);
    return result;
  }

  /// <summary>
  /// Multiplies the lowest single-precision (32-bit) floating-point elements in a and b,
  /// subtracts the lowest element in c from the intermediate result, negates the result,
  /// and stores it in the lowest element of the destination. The remaining elements are copied from a.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<float> MultiplySubtractNegatedScalar(Vector128<float> a, Vector128<float> b, Vector128<float> c)
    => a.WithElement(0, (float)(-(double)a[0] * b[0] - c[0]));

  /// <summary>
  /// Multiplies the lowest double-precision (64-bit) floating-point elements in a and b,
  /// subtracts the lowest element in c from the intermediate result, negates the result,
  /// and stores it in the lowest element of the destination. The remaining elements are copied from a.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector128<double> MultiplySubtractNegatedScalar(Vector128<double> a, Vector128<double> b, Vector128<double> c)
    => a.WithElement(0, -(a[0] * b[0]) - c[0]);

  #endregion

  /// <summary>Provides 64-bit specific FMA operations.</summary>
  public new abstract class X64 : Avx.X64 {

    /// <summary>Gets a value indicating whether 64-bit FMA instructions are supported.</summary>
    public new static bool IsSupported => false;
  }
}

#endif
