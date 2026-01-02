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

using System.Runtime.Intrinsics;

namespace System.Numerics.Tensors;

public static partial class Tensor {

  #region Arithmetic Operations

  /// <summary>Computes element-wise addition of two tensors.</summary>
  public static Tensor<T> Add<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var result = CreateFromShape<T>(x.Lengths);
    Add(x, y, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise addition of two tensors into destination.</summary>
  public static void Add<T>(scoped in ReadOnlyTensorSpan<T> x, scoped in ReadOnlyTensorSpan<T> y, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);
    TensorPrimitives.Add(xFlat, yFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise addition of a tensor and a scalar.</summary>
  public static Tensor<T> Add<T>(in ReadOnlyTensorSpan<T> x, T y) {
    var result = CreateFromShape<T>(x.Lengths);
    Add(x, y, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise addition of a tensor and a scalar into destination.</summary>
  public static void Add<T>(scoped in ReadOnlyTensorSpan<T> x, T y, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Add(xFlat, y, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise subtraction of two tensors.</summary>
  public static Tensor<T> Subtract<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var result = CreateFromShape<T>(x.Lengths);
    Subtract(x, y, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise subtraction of two tensors into destination.</summary>
  public static void Subtract<T>(scoped in ReadOnlyTensorSpan<T> x, scoped in ReadOnlyTensorSpan<T> y, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);
    TensorPrimitives.Subtract(xFlat, yFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise subtraction of a scalar from a tensor.</summary>
  public static Tensor<T> Subtract<T>(in ReadOnlyTensorSpan<T> x, T y) {
    var result = CreateFromShape<T>(x.Lengths);
    Subtract(x, y, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise subtraction of a scalar from a tensor into destination.</summary>
  public static void Subtract<T>(scoped in ReadOnlyTensorSpan<T> x, T y, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Subtract(xFlat, y, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise subtraction of a tensor from a scalar.</summary>
  public static Tensor<T> Subtract<T>(T x, in ReadOnlyTensorSpan<T> y) {
    var result = CreateFromShape<T>(y.Lengths);
    Subtract(x, y, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise subtraction of a tensor from a scalar into destination.</summary>
  public static void Subtract<T>(T x, scoped in ReadOnlyTensorSpan<T> y, in TensorSpan<T> destination) {
    // x - y = -(y - x)
    var yFlat = _GetFlatSpan(y);
    var dest = _GetFlatSpan(destination);
    TensorPrimitives.Subtract(yFlat, x, dest);
    TensorPrimitives.Negate(dest, dest);
  }

  /// <summary>Computes element-wise multiplication of two tensors.</summary>
  public static Tensor<T> Multiply<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var result = CreateFromShape<T>(x.Lengths);
    Multiply(x, y, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise multiplication of two tensors into destination.</summary>
  public static void Multiply<T>(scoped in ReadOnlyTensorSpan<T> x, scoped in ReadOnlyTensorSpan<T> y, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);
    TensorPrimitives.Multiply(xFlat, yFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise multiplication of a tensor and a scalar.</summary>
  public static Tensor<T> Multiply<T>(in ReadOnlyTensorSpan<T> x, T y) {
    var result = CreateFromShape<T>(x.Lengths);
    Multiply(x, y, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise multiplication of a tensor and a scalar into destination.</summary>
  public static void Multiply<T>(scoped in ReadOnlyTensorSpan<T> x, T y, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Multiply(xFlat, y, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise division of two tensors.</summary>
  public static Tensor<T> Divide<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var result = CreateFromShape<T>(x.Lengths);
    Divide(x, y, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise division of two tensors into destination.</summary>
  public static void Divide<T>(scoped in ReadOnlyTensorSpan<T> x, scoped in ReadOnlyTensorSpan<T> y, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);
    TensorPrimitives.Divide(xFlat, yFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise division of a tensor by a scalar.</summary>
  public static Tensor<T> Divide<T>(in ReadOnlyTensorSpan<T> x, T y) {
    var result = CreateFromShape<T>(x.Lengths);
    Divide(x, y, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise division of a tensor by a scalar into destination.</summary>
  public static void Divide<T>(scoped in ReadOnlyTensorSpan<T> x, T y, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Divide(xFlat, y, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise division of a scalar by a tensor.</summary>
  public static Tensor<T> Divide<T>(T x, in ReadOnlyTensorSpan<T> y) {
    var result = CreateFromShape<T>(y.Lengths);
    Divide(x, y, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise division of a scalar by a tensor into destination.</summary>
  public static void Divide<T>(T x, scoped in ReadOnlyTensorSpan<T> y, in TensorSpan<T> destination) {
    // x / y for each element
    var yFlat = _GetFlatSpan(y);
    var dest = _GetFlatSpan(destination);
    // Create temp array with x values, then divide
    var xArray = new T[yFlat.Length];
    Array.Fill(xArray, x);
    TensorPrimitives.Divide<T>(xArray, yFlat, dest);
  }

  /// <summary>Computes element-wise negation.</summary>
  public static Tensor<T> Negate<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    Negate(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise negation into destination.</summary>
  public static void Negate<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Negate(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise absolute value.</summary>
  public static Tensor<T> Abs<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    Abs(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise absolute value into destination.</summary>
  public static void Abs<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Abs(xFlat, _GetFlatSpan(destination));
  }

  #endregion

  #region Mathematical Functions

  /// <summary>Computes element-wise square root.</summary>
  public static Tensor<T> Sqrt<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    Sqrt(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise square root into destination.</summary>
  public static void Sqrt<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Sqrt(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise cube root.</summary>
  public static Tensor<T> Cbrt<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    Cbrt(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise cube root into destination.</summary>
  public static void Cbrt<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Cbrt(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise n-th root.</summary>
  public static Tensor<T> RootN<T>(in ReadOnlyTensorSpan<T> x, int n) {
    var result = CreateFromShape<T>(x.Lengths);
    RootN(x, n, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise n-th root into destination.</summary>
  public static void RootN<T>(scoped in ReadOnlyTensorSpan<T> x, int n, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.RootN(xFlat, n, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise exponential (e^x).</summary>
  public static Tensor<T> Exp<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    Exp(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise exponential (e^x) into destination.</summary>
  public static void Exp<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Exp(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise 2^x.</summary>
  public static Tensor<T> Exp2<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    Exp2(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise 2^x into destination.</summary>
  public static void Exp2<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Exp2(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise 10^x.</summary>
  public static Tensor<T> Exp10<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    Exp10(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise 10^x into destination.</summary>
  public static void Exp10<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Exp10(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise e^x - 1.</summary>
  public static Tensor<T> ExpM1<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    ExpM1(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise e^x - 1 into destination.</summary>
  public static void ExpM1<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.ExpM1(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise natural logarithm.</summary>
  public static Tensor<T> Log<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    Log(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise natural logarithm into destination.</summary>
  public static void Log<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Log(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise base-2 logarithm.</summary>
  public static Tensor<T> Log2<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    Log2(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise base-2 logarithm into destination.</summary>
  public static void Log2<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Log2(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise base-10 logarithm.</summary>
  public static Tensor<T> Log10<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    Log10(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise base-10 logarithm into destination.</summary>
  public static void Log10<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Log10(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise ln(1 + x).</summary>
  public static Tensor<T> LogP1<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    LogP1(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise ln(1 + x) into destination.</summary>
  public static void LogP1<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.LogP1(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise log2(1 + x).</summary>
  public static Tensor<T> Log2P1<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    Log2P1(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise log2(1 + x) into destination.</summary>
  public static void Log2P1<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Log2P1(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise log10(1 + x).</summary>
  public static Tensor<T> Log10P1<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    Log10P1(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise log10(1 + x) into destination.</summary>
  public static void Log10P1<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Log10P1(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise power.</summary>
  public static Tensor<T> Pow<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var result = CreateFromShape<T>(x.Lengths);
    Pow(x, y, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise power into destination.</summary>
  public static void Pow<T>(scoped in ReadOnlyTensorSpan<T> x, scoped in ReadOnlyTensorSpan<T> y, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);
    TensorPrimitives.Pow(xFlat, yFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise power with scalar exponent.</summary>
  public static Tensor<T> Pow<T>(in ReadOnlyTensorSpan<T> x, T y) {
    var result = CreateFromShape<T>(x.Lengths);
    Pow(x, y, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise power with scalar exponent into destination.</summary>
  public static void Pow<T>(scoped in ReadOnlyTensorSpan<T> x, T y, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Pow(xFlat, y, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise reciprocal (1/x).</summary>
  public static Tensor<T> Reciprocal<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    Reciprocal(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise reciprocal (1/x) into destination.</summary>
  public static void Reciprocal<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Reciprocal(xFlat, _GetFlatSpan(destination));
  }

  #endregion

  #region Rounding Functions

  /// <summary>Computes element-wise floor.</summary>
  public static Tensor<T> Floor<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    Floor(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise floor into destination.</summary>
  public static void Floor<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Floor(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise ceiling.</summary>
  public static Tensor<T> Ceiling<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    Ceiling(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise ceiling into destination.</summary>
  public static void Ceiling<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Ceiling(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise round to nearest integer.</summary>
  public static Tensor<T> Round<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    Round(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise round to nearest integer into destination.</summary>
  public static void Round<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Round(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise truncation toward zero.</summary>
  public static Tensor<T> Truncate<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    Truncate(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise truncation toward zero into destination.</summary>
  public static void Truncate<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Truncate(xFlat, _GetFlatSpan(destination));
  }

  #endregion

  #region Trigonometric Functions

  /// <summary>Computes element-wise sine.</summary>
  public static Tensor<T> Sin<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    Sin(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise sine into destination.</summary>
  public static void Sin<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Sin(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise cosine.</summary>
  public static Tensor<T> Cos<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    Cos(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise cosine into destination.</summary>
  public static void Cos<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Cos(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise tangent.</summary>
  public static Tensor<T> Tan<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    Tan(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise tangent into destination.</summary>
  public static void Tan<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Tan(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise hyperbolic sine.</summary>
  public static Tensor<T> Sinh<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    Sinh(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise hyperbolic sine into destination.</summary>
  public static void Sinh<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Sinh(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise hyperbolic cosine.</summary>
  public static Tensor<T> Cosh<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    Cosh(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise hyperbolic cosine into destination.</summary>
  public static void Cosh<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Cosh(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise hyperbolic tangent.</summary>
  public static Tensor<T> Tanh<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    Tanh(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise hyperbolic tangent into destination.</summary>
  public static void Tanh<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Tanh(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise arc sine.</summary>
  public static Tensor<T> Asin<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    Asin(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise arc sine into destination.</summary>
  public static void Asin<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Asin(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise arc cosine.</summary>
  public static Tensor<T> Acos<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    Acos(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise arc cosine into destination.</summary>
  public static void Acos<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Acos(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise arc tangent.</summary>
  public static Tensor<T> Atan<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    Atan(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise arc tangent into destination.</summary>
  public static void Atan<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Atan(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise arc tangent of y/x.</summary>
  public static Tensor<T> Atan2<T>(in ReadOnlyTensorSpan<T> y, in ReadOnlyTensorSpan<T> x) {
    _ValidateShapesMatch(y.Lengths, x.Lengths);
    var result = CreateFromShape<T>(y.Lengths);
    Atan2(y, x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise arc tangent of y/x into destination.</summary>
  public static void Atan2<T>(scoped in ReadOnlyTensorSpan<T> y, scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var yFlat = _GetFlatSpan(y);
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Atan2(yFlat, xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise inverse hyperbolic sine.</summary>
  public static Tensor<T> Asinh<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    Asinh(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise inverse hyperbolic sine into destination.</summary>
  public static void Asinh<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Asinh(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise inverse hyperbolic cosine.</summary>
  public static Tensor<T> Acosh<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    Acosh(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise inverse hyperbolic cosine into destination.</summary>
  public static void Acosh<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Acosh(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise inverse hyperbolic tangent.</summary>
  public static Tensor<T> Atanh<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    Atanh(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise inverse hyperbolic tangent into destination.</summary>
  public static void Atanh<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Atanh(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise sin(x * pi).</summary>
  public static Tensor<T> SinPi<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    SinPi(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise sin(x * pi) into destination.</summary>
  public static void SinPi<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.SinPi(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise cos(x * pi).</summary>
  public static Tensor<T> CosPi<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    CosPi(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise cos(x * pi) into destination.</summary>
  public static void CosPi<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.CosPi(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise tan(x * pi).</summary>
  public static Tensor<T> TanPi<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    TanPi(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise tan(x * pi) into destination.</summary>
  public static void TanPi<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.TanPi(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise asin(x) / pi.</summary>
  public static Tensor<T> AsinPi<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    AsinPi(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise asin(x) / pi into destination.</summary>
  public static void AsinPi<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.AsinPi(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise acos(x) / pi.</summary>
  public static Tensor<T> AcosPi<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    AcosPi(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise acos(x) / pi into destination.</summary>
  public static void AcosPi<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.AcosPi(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise atan(x) / pi.</summary>
  public static Tensor<T> AtanPi<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    AtanPi(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise atan(x) / pi into destination.</summary>
  public static void AtanPi<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.AtanPi(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise atan2(y, x) / pi.</summary>
  public static Tensor<T> Atan2Pi<T>(in ReadOnlyTensorSpan<T> y, in ReadOnlyTensorSpan<T> x) {
    _ValidateShapesMatch(y.Lengths, x.Lengths);
    var result = CreateFromShape<T>(y.Lengths);
    Atan2Pi(y, x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise atan2(y, x) / pi into destination.</summary>
  public static void Atan2Pi<T>(scoped in ReadOnlyTensorSpan<T> y, scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var yFlat = _GetFlatSpan(y);
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Atan2Pi(yFlat, xFlat, _GetFlatSpan(destination));
  }

  #endregion

  #region Other Mathematical Functions

  /// <summary>Computes element-wise hypotenuse sqrt(x^2 + y^2).</summary>
  public static Tensor<T> Hypot<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var result = CreateFromShape<T>(x.Lengths);
    Hypot(x, y, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise hypotenuse sqrt(x^2 + y^2) into destination.</summary>
  public static void Hypot<T>(scoped in ReadOnlyTensorSpan<T> x, scoped in ReadOnlyTensorSpan<T> y, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);
    TensorPrimitives.Hypot(xFlat, yFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise IEEE 754 remainder.</summary>
  public static Tensor<T> Ieee754Remainder<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var result = CreateFromShape<T>(x.Lengths);
    Ieee754Remainder(x, y, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise IEEE 754 remainder into destination.</summary>
  public static void Ieee754Remainder<T>(scoped in ReadOnlyTensorSpan<T> x, scoped in ReadOnlyTensorSpan<T> y, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);
    TensorPrimitives.IEEERemainder(xFlat, yFlat, _GetFlatSpan(destination));
  }

  /// <summary>Copies the sign from y to x element-wise.</summary>
  public static Tensor<T> CopySign<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> sign) {
    _ValidateShapesMatch(x.Lengths, sign.Lengths);
    var result = CreateFromShape<T>(x.Lengths);
    CopySign(x, sign, result.AsTensorSpan());
    return result;
  }

  /// <summary>Copies the sign from y to x element-wise into destination.</summary>
  public static void CopySign<T>(scoped in ReadOnlyTensorSpan<T> x, scoped in ReadOnlyTensorSpan<T> sign, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    var signFlat = _GetFlatSpan(sign);
    TensorPrimitives.CopySign(xFlat, signFlat, _GetFlatSpan(destination));
  }

  /// <summary>Converts degrees to radians element-wise.</summary>
  public static Tensor<T> DegreesToRadians<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    DegreesToRadians(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Converts degrees to radians element-wise into destination.</summary>
  public static void DegreesToRadians<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.DegreesToRadians(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Converts radians to degrees element-wise.</summary>
  public static Tensor<T> RadiansToDegrees<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    RadiansToDegrees(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Converts radians to degrees element-wise into destination.</summary>
  public static void RadiansToDegrees<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.RadiansToDegrees(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise integer logarithm base 2.</summary>
  public static Tensor<int> ILogB<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<int>(x.Lengths);
    ILogB(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise integer logarithm base 2 into destination.</summary>
  public static void ILogB<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<int> destination) {
    var xFlat = _GetFlatSpan(x);
    var dest = _GetFlatSpan(destination);
    for (var i = 0; i < xFlat.Length; ++i)
      dest[i] = _ILogB(xFlat[i]);
  }

  #endregion

  #region Bitwise Operations

  /// <summary>Computes element-wise bitwise AND.</summary>
  public static Tensor<T> BitwiseAnd<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var result = CreateFromShape<T>(x.Lengths);
    BitwiseAnd(x, y, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise bitwise AND into destination.</summary>
  public static void BitwiseAnd<T>(scoped in ReadOnlyTensorSpan<T> x, scoped in ReadOnlyTensorSpan<T> y, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);
    _BitwiseAndUnconstrained(xFlat, yFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise bitwise OR.</summary>
  public static Tensor<T> BitwiseOr<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var result = CreateFromShape<T>(x.Lengths);
    BitwiseOr(x, y, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise bitwise OR into destination.</summary>
  public static void BitwiseOr<T>(scoped in ReadOnlyTensorSpan<T> x, scoped in ReadOnlyTensorSpan<T> y, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);
    _BitwiseOrUnconstrained(xFlat, yFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise bitwise XOR.</summary>
  public static Tensor<T> Xor<T>(in ReadOnlyTensorSpan<T> x, in ReadOnlyTensorSpan<T> y) {
    _ValidateShapesMatch(x.Lengths, y.Lengths);
    var result = CreateFromShape<T>(x.Lengths);
    Xor(x, y, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise bitwise XOR into destination.</summary>
  public static void Xor<T>(scoped in ReadOnlyTensorSpan<T> x, scoped in ReadOnlyTensorSpan<T> y, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    var yFlat = _GetFlatSpan(y);
    _BitwiseXorUnconstrained(xFlat, yFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise ones complement.</summary>
  public static Tensor<T> OnesComplement<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    OnesComplement(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise ones complement into destination.</summary>
  public static void OnesComplement<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    _OnesComplementUnconstrained(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes element-wise population count.</summary>
  public static Tensor<T> PopCount<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    PopCount(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise population count into destination.</summary>
  public static void PopCount<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    var dest = _GetFlatSpan(destination);
    for (var i = 0; i < xFlat.Length; ++i)
      dest[i] = _PopCount(xFlat[i]);
  }

  /// <summary>Computes element-wise leading zero count.</summary>
  public static Tensor<T> LeadingZeroCount<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    LeadingZeroCount(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise leading zero count into destination.</summary>
  public static void LeadingZeroCount<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    var dest = _GetFlatSpan(destination);
    for (var i = 0; i < xFlat.Length; ++i)
      dest[i] = _LeadingZeroCount(xFlat[i]);
  }

  /// <summary>Computes element-wise trailing zero count.</summary>
  public static Tensor<T> TrailingZeroCount<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    TrailingZeroCount(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise trailing zero count into destination.</summary>
  public static void TrailingZeroCount<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    var dest = _GetFlatSpan(destination);
    for (var i = 0; i < xFlat.Length; ++i)
      dest[i] = _TrailingZeroCount(xFlat[i]);
  }

  /// <summary>Shifts each element left by the specified amount.</summary>
  public static Tensor<T> ShiftLeft<T>(in ReadOnlyTensorSpan<T> x, int shiftAmount) {
    var result = CreateFromShape<T>(x.Lengths);
    ShiftLeft(x, shiftAmount, result.AsTensorSpan());
    return result;
  }

  /// <summary>Shifts each element left by the specified amount into destination.</summary>
  public static void ShiftLeft<T>(scoped in ReadOnlyTensorSpan<T> x, int shiftAmount, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    _ShiftLeftUnconstrained(xFlat, shiftAmount, _GetFlatSpan(destination));
  }

  /// <summary>Shifts each element right arithmetically by the specified amount.</summary>
  public static Tensor<T> ShiftRightArithmetic<T>(in ReadOnlyTensorSpan<T> x, int shiftAmount) {
    var result = CreateFromShape<T>(x.Lengths);
    ShiftRightArithmetic(x, shiftAmount, result.AsTensorSpan());
    return result;
  }

  /// <summary>Shifts each element right arithmetically by the specified amount into destination.</summary>
  public static void ShiftRightArithmetic<T>(scoped in ReadOnlyTensorSpan<T> x, int shiftAmount, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    _ShiftRightArithmeticUnconstrained(xFlat, shiftAmount, _GetFlatSpan(destination));
  }

  /// <summary>Shifts each element right logically by the specified amount.</summary>
  public static Tensor<T> ShiftRightLogical<T>(in ReadOnlyTensorSpan<T> x, int shiftAmount) {
    var result = CreateFromShape<T>(x.Lengths);
    ShiftRightLogical(x, shiftAmount, result.AsTensorSpan());
    return result;
  }

  /// <summary>Shifts each element right logically by the specified amount into destination.</summary>
  public static void ShiftRightLogical<T>(scoped in ReadOnlyTensorSpan<T> x, int shiftAmount, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    _ShiftRightLogicalUnconstrained(xFlat, shiftAmount, _GetFlatSpan(destination));
  }

  #endregion

  #region ML/Activation Functions

  /// <summary>Computes element-wise sigmoid 1/(1+e^-x).</summary>
  public static Tensor<T> Sigmoid<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    Sigmoid(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes element-wise sigmoid 1/(1+e^-x) into destination.</summary>
  public static void Sigmoid<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.Sigmoid(xFlat, _GetFlatSpan(destination));
  }

  /// <summary>Computes softmax normalization.</summary>
  public static Tensor<T> SoftMax<T>(in ReadOnlyTensorSpan<T> x) {
    var result = CreateFromShape<T>(x.Lengths);
    SoftMax(x, result.AsTensorSpan());
    return result;
  }

  /// <summary>Computes softmax normalization into destination.</summary>
  public static void SoftMax<T>(scoped in ReadOnlyTensorSpan<T> x, in TensorSpan<T> destination) {
    var xFlat = _GetFlatSpan(x);
    TensorPrimitives.SoftMax(xFlat, _GetFlatSpan(destination));
  }

  #endregion

  #region Distribution Fill

  /// <summary>Fills the tensor with values from a uniform distribution.</summary>
  public static void FillUniformDistribution<T>(in TensorSpan<T> destination, Random? random = null) {
    random ??= Random.Shared;
    var dest = _GetFlatSpan(destination);
    for (var i = 0; i < dest.Length; ++i)
      dest[i] = _FromDouble<T>(random.NextDouble());
  }

  /// <summary>Fills the tensor with values from a Gaussian normal distribution.</summary>
  public static void FillGaussianNormalDistribution<T>(in TensorSpan<T> destination, Random? random = null) {
    random ??= Random.Shared;
    var dest = _GetFlatSpan(destination);
    for (var i = 0; i < dest.Length; ++i) {
      // Box-Muller transform
      var u1 = random.NextDouble();
      var u2 = random.NextDouble();
      var z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
      dest[i] = _FromDouble<T>(z);
    }
  }

  #endregion

}

#endif
