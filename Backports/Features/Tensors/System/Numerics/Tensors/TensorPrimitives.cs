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

using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Numerics.Tensors;

/// <summary>
/// Provides static methods for performing tensor primitive operations on spans of values.
/// </summary>
public static class TensorPrimitives {

  #region Arithmetic Operations

  /// <summary>Computes the element-wise addition of two spans.</summary>
  /// <param name="x">The first span.</param>
  /// <param name="y">The second span.</param>
  /// <param name="destination">The destination span.</param>
  /// <exception cref="ArgumentException">Length of spans must match.</exception>
  public static void Add<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) {
    _ValidateLengths(x.Length, y.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Add(x[i], y[i]);
  }

  /// <summary>Computes the element-wise addition of a span and a scalar.</summary>
  /// <param name="x">The span.</param>
  /// <param name="y">The scalar value.</param>
  /// <param name="destination">The destination span.</param>
  public static void Add<T>(ReadOnlySpan<T> x, T y, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Add(x[i], y);
  }

  /// <summary>Computes the element-wise subtraction of two spans.</summary>
  /// <param name="x">The first span.</param>
  /// <param name="y">The second span.</param>
  /// <param name="destination">The destination span.</param>
  public static void Subtract<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) {
    _ValidateLengths(x.Length, y.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Subtract(x[i], y[i]);
  }

  /// <summary>Computes the element-wise subtraction of a span and a scalar.</summary>
  /// <param name="x">The span.</param>
  /// <param name="y">The scalar value.</param>
  /// <param name="destination">The destination span.</param>
  public static void Subtract<T>(ReadOnlySpan<T> x, T y, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Subtract(x[i], y);
  }

  /// <summary>Computes the element-wise multiplication of two spans.</summary>
  /// <param name="x">The first span.</param>
  /// <param name="y">The second span.</param>
  /// <param name="destination">The destination span.</param>
  public static void Multiply<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) {
    _ValidateLengths(x.Length, y.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Multiply(x[i], y[i]);
  }

  /// <summary>Computes the element-wise multiplication of a span and a scalar.</summary>
  /// <param name="x">The span.</param>
  /// <param name="y">The scalar value.</param>
  /// <param name="destination">The destination span.</param>
  public static void Multiply<T>(ReadOnlySpan<T> x, T y, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Multiply(x[i], y);
  }

  /// <summary>Computes the element-wise division of two spans.</summary>
  /// <param name="x">The first span.</param>
  /// <param name="y">The second span.</param>
  /// <param name="destination">The destination span.</param>
  public static void Divide<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) {
    _ValidateLengths(x.Length, y.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Divide(x[i], y[i]);
  }

  /// <summary>Computes the element-wise division of a span by a scalar.</summary>
  /// <param name="x">The span.</param>
  /// <param name="y">The scalar divisor.</param>
  /// <param name="destination">The destination span.</param>
  public static void Divide<T>(ReadOnlySpan<T> x, T y, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Divide(x[i], y);
  }

  /// <summary>Computes the element-wise negation of a span.</summary>
  /// <param name="x">The source span.</param>
  /// <param name="destination">The destination span.</param>
  public static void Negate<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Negate(x[i]);
  }

  /// <summary>Computes the element-wise absolute value of a span.</summary>
  /// <param name="x">The source span.</param>
  /// <param name="destination">The destination span.</param>
  public static void Abs<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Abs(x[i]);
  }

  /// <summary>Computes the element-wise addition of three spans (x + y + z).</summary>
  public static void AddMultiply<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, ReadOnlySpan<T> multiplier, Span<T> destination) {
    _ValidateLengths(x.Length, y.Length, multiplier.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Multiply(Scalar<T>.Add(x[i], y[i]), multiplier[i]);
  }

  /// <summary>Computes (x + y) * multiplier for each element.</summary>
  public static void AddMultiply<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, T multiplier, Span<T> destination) {
    _ValidateLengths(x.Length, y.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Multiply(Scalar<T>.Add(x[i], y[i]), multiplier);
  }

  /// <summary>Computes (x + y) * multiplier for each element.</summary>
  public static void AddMultiply<T>(ReadOnlySpan<T> x, T y, ReadOnlySpan<T> multiplier, Span<T> destination) {
    _ValidateLengths(x.Length, multiplier.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Multiply(Scalar<T>.Add(x[i], y), multiplier[i]);
  }

  /// <summary>Computes (x * y) + addend for each element.</summary>
  public static void MultiplyAdd<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, ReadOnlySpan<T> addend, Span<T> destination) {
    _ValidateLengths(x.Length, y.Length, addend.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Add(Scalar<T>.Multiply(x[i], y[i]), addend[i]);
  }

  /// <summary>Computes (x * y) + addend for each element.</summary>
  public static void MultiplyAdd<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, T addend, Span<T> destination) {
    _ValidateLengths(x.Length, y.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Add(Scalar<T>.Multiply(x[i], y[i]), addend);
  }

  /// <summary>Computes (x * y) + addend for each element.</summary>
  public static void MultiplyAdd<T>(ReadOnlySpan<T> x, T y, ReadOnlySpan<T> addend, Span<T> destination) {
    _ValidateLengths(x.Length, addend.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Add(Scalar<T>.Multiply(x[i], y), addend[i]);
  }

  #endregion

  #region Math Operations

  /// <summary>Computes the element-wise exponential function.</summary>
  public static void Exp<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Exp(x[i]);
  }

  /// <summary>Computes the element-wise natural logarithm.</summary>
  public static void Log<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Log(x[i]);
  }

  /// <summary>Computes the element-wise base-2 logarithm.</summary>
  public static void Log2<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Log2(x[i]);
  }

  /// <summary>Computes the element-wise square root.</summary>
  public static void Sqrt<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Sqrt(x[i]);
  }

  /// <summary>Computes the element-wise floor.</summary>
  public static void Floor<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Floor(x[i]);
  }

  /// <summary>Computes the element-wise ceiling.</summary>
  public static void Ceiling<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Ceiling(x[i]);
  }

  /// <summary>Computes the element-wise truncation.</summary>
  public static void Truncate<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Truncate(x[i]);
  }

  /// <summary>Computes the element-wise rounding.</summary>
  public static void Round<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Round(x[i]);
  }

  /// <summary>Computes the element-wise hypotenuse (sqrt(x² + y²)).</summary>
  public static void Hypot<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) {
    _ValidateLengths(x.Length, y.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i) {
      var xSq = Scalar<T>.Multiply(x[i], x[i]);
      var ySq = Scalar<T>.Multiply(y[i], y[i]);
      destination[i] = Scalar<T>.Sqrt(Scalar<T>.Add(xSq, ySq));
    }
  }

  /// <summary>Computes the element-wise reciprocal (1/x).</summary>
  public static void Reciprocal<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    var one = Scalar<T>.One;
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Divide(one, x[i]);
  }

  /// <summary>Computes the element-wise reciprocal of the square root (1/sqrt(x)).</summary>
  public static void ReciprocalSqrt<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    var one = Scalar<T>.One;
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Divide(one, Scalar<T>.Sqrt(x[i]));
  }

  /// <summary>Computes the element-wise base-10 logarithm.</summary>
  public static void Log10<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Log10(x[i]);
  }

  /// <summary>Computes the element-wise cube root.</summary>
  public static void Cbrt<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Cbrt(x[i]);
  }

  /// <summary>Computes the element-wise n-th root.</summary>
  public static void RootN<T>(ReadOnlySpan<T> x, int n, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    // Compute n as type T by adding One n times
    var nT = Scalar<T>.Zero();
    for (var j = 0; j < n; ++j)
      nT = Scalar<T>.Add(nT, Scalar<T>.One);
    var reciprocal = Scalar<T>.Divide(Scalar<T>.One, nT);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Pow(x[i], reciprocal);
  }

  /// <summary>Computes the element-wise power function.</summary>
  public static void Pow<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) {
    _ValidateLengths(x.Length, y.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Pow(x[i], y[i]);
  }

  /// <summary>Computes the element-wise power function with scalar exponent.</summary>
  public static void Pow<T>(ReadOnlySpan<T> x, T y, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Pow(x[i], y);
  }

  #endregion

  #region Trigonometric Operations

  /// <summary>Computes the element-wise sine.</summary>
  public static void Sin<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Sin(x[i]);
  }

  /// <summary>Computes the element-wise cosine.</summary>
  public static void Cos<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Cos(x[i]);
  }

  /// <summary>Computes the element-wise tangent.</summary>
  public static void Tan<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Tan(x[i]);
  }

  /// <summary>Computes the element-wise hyperbolic sine.</summary>
  public static void Sinh<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Sinh(x[i]);
  }

  /// <summary>Computes the element-wise hyperbolic cosine.</summary>
  public static void Cosh<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Cosh(x[i]);
  }

  /// <summary>Computes the element-wise arc sine.</summary>
  public static void Asin<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Asin(x[i]);
  }

  /// <summary>Computes the element-wise arc cosine.</summary>
  public static void Acos<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Acos(x[i]);
  }

  /// <summary>Computes the element-wise arc tangent.</summary>
  public static void Atan<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Atan(x[i]);
  }

  /// <summary>Computes the element-wise arc tangent of y/x.</summary>
  public static void Atan2<T>(ReadOnlySpan<T> y, ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateLengths(y.Length, x.Length, destination.Length);
    for (var i = 0; i < y.Length; ++i)
      destination[i] = Scalar<T>.Atan2(y[i], x[i]);
  }

  /// <summary>Computes the element-wise inverse hyperbolic sine.</summary>
  public static void Asinh<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Asinh(x[i]);
  }

  /// <summary>Computes the element-wise inverse hyperbolic cosine.</summary>
  public static void Acosh<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Acosh(x[i]);
  }

  /// <summary>Computes the element-wise inverse hyperbolic tangent.</summary>
  public static void Atanh<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Atanh(x[i]);
  }

  /// <summary>Computes the element-wise sine and cosine.</summary>
  public static void SinCos<T>(ReadOnlySpan<T> x, Span<T> sinDestination, Span<T> cosDestination) {
    _ValidateDestination(x.Length, sinDestination.Length);
    _ValidateDestination(x.Length, cosDestination.Length);
    for (var i = 0; i < x.Length; ++i) {
      sinDestination[i] = Scalar<T>.Sin(x[i]);
      cosDestination[i] = Scalar<T>.Cos(x[i]);
    }
  }

  /// <summary>Computes the element-wise sine of Pi times x.</summary>
  public static void SinPi<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    var pi = Scalar<T>.Pi;
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Sin(Scalar<T>.Multiply(x[i], pi));
  }

  /// <summary>Computes the element-wise cosine of Pi times x.</summary>
  public static void CosPi<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    var pi = Scalar<T>.Pi;
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Cos(Scalar<T>.Multiply(x[i], pi));
  }

  /// <summary>Computes the element-wise tangent of Pi times x.</summary>
  public static void TanPi<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    var pi = Scalar<T>.Pi;
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Tan(Scalar<T>.Multiply(x[i], pi));
  }

  /// <summary>Computes the element-wise arc sine divided by Pi.</summary>
  public static void AsinPi<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    var pi = Scalar<T>.Pi;
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Divide(Scalar<T>.Asin(x[i]), pi);
  }

  /// <summary>Computes the element-wise arc cosine divided by Pi.</summary>
  public static void AcosPi<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    var pi = Scalar<T>.Pi;
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Divide(Scalar<T>.Acos(x[i]), pi);
  }

  /// <summary>Computes the element-wise arc tangent divided by Pi.</summary>
  public static void AtanPi<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    var pi = Scalar<T>.Pi;
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Divide(Scalar<T>.Atan(x[i]), pi);
  }

  /// <summary>Computes the element-wise arc tangent of y/x divided by Pi.</summary>
  public static void Atan2Pi<T>(ReadOnlySpan<T> y, ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateLengths(y.Length, x.Length, destination.Length);
    var pi = Scalar<T>.Pi;
    for (var i = 0; i < y.Length; ++i)
      destination[i] = Scalar<T>.Divide(Scalar<T>.Atan2(y[i], x[i]), pi);
  }

  /// <summary>Computes the element-wise degrees to radians conversion.</summary>
  public static void DegreesToRadians<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    var factor = Scalar<T>.Divide(Scalar<T>.Pi, Scalar<T>.From<int>(180));
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Multiply(x[i], factor);
  }

  /// <summary>Computes the element-wise radians to degrees conversion.</summary>
  public static void RadiansToDegrees<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    var factor = Scalar<T>.Divide(Scalar<T>.From<int>(180), Scalar<T>.Pi);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Multiply(x[i], factor);
  }

  #endregion

  #region Additional Math Operations

  /// <summary>Computes the element-wise copy sign function.</summary>
  public static void CopySign<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> sign, Span<T> destination) {
    _ValidateLengths(x.Length, sign.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.CopySign(x[i], sign[i]);
  }

  /// <summary>Computes the element-wise copy sign with scalar sign.</summary>
  public static void CopySign<T>(ReadOnlySpan<T> x, T sign, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.CopySign(x[i], sign);
  }

  /// <summary>Computes the element-wise fused multiply-add (x * y + z).</summary>
  public static void FusedMultiplyAdd<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, ReadOnlySpan<T> addend, Span<T> destination) {
    _ValidateLengths(x.Length, y.Length, addend.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.FusedMultiplyAdd(x[i], y[i], addend[i]);
  }

  /// <summary>Computes the element-wise fused multiply-add with scalar addend.</summary>
  public static void FusedMultiplyAdd<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, T addend, Span<T> destination) {
    _ValidateLengths(x.Length, y.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.FusedMultiplyAdd(x[i], y[i], addend);
  }

  /// <summary>Computes the element-wise fused multiply-add with scalar multiplier.</summary>
  public static void FusedMultiplyAdd<T>(ReadOnlySpan<T> x, T y, ReadOnlySpan<T> addend, Span<T> destination) {
    _ValidateLengths(x.Length, addend.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.FusedMultiplyAdd(x[i], y, addend[i]);
  }

  /// <summary>Computes the element-wise IEEE remainder.</summary>
  public static void IEEERemainder<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) {
    _ValidateLengths(x.Length, y.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.IEEERemainder(x[i], y[i]);
  }

  /// <summary>Computes the element-wise IEEE remainder with scalar divisor.</summary>
  public static void IEEERemainder<T>(ReadOnlySpan<T> x, T y, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.IEEERemainder(x[i], y);
  }

  /// <summary>Computes the element-wise base-2 exponential.</summary>
  public static void Exp2<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    var two = Scalar<T>.Add(Scalar<T>.One, Scalar<T>.One);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Pow(two, x[i]);
  }

  /// <summary>Computes the element-wise base-10 exponential.</summary>
  public static void Exp10<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    var ten = Scalar<T>.From<int>(10);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Pow(ten, x[i]);
  }

  /// <summary>Computes the element-wise log(1 + x).</summary>
  public static void LogP1<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    var one = Scalar<T>.One;
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Log(Scalar<T>.Add(one, x[i]));
  }

  /// <summary>Computes the element-wise log2(1 + x).</summary>
  public static void Log2P1<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    var one = Scalar<T>.One;
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Log2(Scalar<T>.Add(one, x[i]));
  }

  /// <summary>Computes the element-wise log10(1 + x).</summary>
  public static void Log10P1<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    var one = Scalar<T>.One;
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Log10(Scalar<T>.Add(one, x[i]));
  }

  /// <summary>Computes the element-wise exp(x) - 1.</summary>
  public static void ExpM1<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    var one = Scalar<T>.One;
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Subtract(Scalar<T>.Exp(x[i]), one);
  }

  /// <summary>Computes the element-wise 2^x - 1.</summary>
  public static void Exp2M1<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    var one = Scalar<T>.One;
    var two = Scalar<T>.Add(one, one);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Subtract(Scalar<T>.Pow(two, x[i]), one);
  }

  /// <summary>Computes the element-wise 10^x - 1.</summary>
  public static void Exp10M1<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    var one = Scalar<T>.One;
    var ten = Scalar<T>.From<int>(10);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Subtract(Scalar<T>.Pow(ten, x[i]), one);
  }

  /// <summary>Computes the element-wise logarithm with specified base.</summary>
  public static void Log<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) {
    _ValidateLengths(x.Length, y.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Divide(Scalar<T>.Log(x[i]), Scalar<T>.Log(y[i]));
  }

  /// <summary>Computes the element-wise logarithm with scalar base.</summary>
  public static void Log<T>(ReadOnlySpan<T> x, T y, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    var logBase = Scalar<T>.Log(y);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Divide(Scalar<T>.Log(x[i]), logBase);
  }

  /// <summary>Computes the element-wise modulus (remainder).</summary>
  public static void Remainder<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) {
    _ValidateLengths(x.Length, y.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Remainder(x[i], y[i]);
  }

  /// <summary>Computes the element-wise modulus with scalar divisor.</summary>
  public static void Remainder<T>(ReadOnlySpan<T> x, T y, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Remainder(x[i], y);
  }

  /// <summary>Computes the element-wise square (x * x).</summary>
  public static void Square<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Multiply(x[i], x[i]);
  }

  /// <summary>Computes the element-wise reciprocal estimate.</summary>
  public static void ReciprocalEstimate<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    var one = Scalar<T>.One;
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Divide(one, x[i]);
  }

  /// <summary>Computes the element-wise reciprocal square root estimate.</summary>
  public static void ReciprocalSqrtEstimate<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    var one = Scalar<T>.One;
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Divide(one, Scalar<T>.Sqrt(x[i]));
  }

  /// <summary>Computes the element-wise rounding to nearest integer.</summary>
  public static void Round<T>(ReadOnlySpan<T> x, int digits, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Round(x[i], digits);
  }

  /// <summary>Computes the element-wise rounding with mode.</summary>
  public static void Round<T>(ReadOnlySpan<T> x, MidpointRounding mode, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Round(x[i], mode);
  }

  /// <summary>Computes the element-wise rounding with digits and mode.</summary>
  public static void Round<T>(ReadOnlySpan<T> x, int digits, MidpointRounding mode, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Round(x[i], digits, mode);
  }

  /// <summary>Computes the element-wise scale by power of 2.</summary>
  public static void ScaleB<T>(ReadOnlySpan<T> x, ReadOnlySpan<int> n, Span<T> destination) {
    _ValidateLengths(x.Length, n.Length, destination.Length);
    var two = Scalar<T>.Add(Scalar<T>.One, Scalar<T>.One);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Multiply(x[i], Scalar<T>.Pow(two, Scalar<T>.From<int>(n[i])));
  }

  #endregion

  #region Aggregation Operations

  /// <summary>Computes the sum of all elements in the span.</summary>
  /// <param name="x">The source span.</param>
  /// <returns>The sum of all elements.</returns>
  public static T Sum<T>(ReadOnlySpan<T> x) {
    var sum = Scalar<T>.Zero();
    for (var i = 0; i < x.Length; ++i)
      sum = Scalar<T>.Add(sum, x[i]);
    return sum;
  }

  /// <summary>Computes the product of all elements in the span.</summary>
  /// <param name="x">The source span.</param>
  /// <returns>The product of all elements.</returns>
  public static T Product<T>(ReadOnlySpan<T> x) {
    if (x.Length == 0)
      return Scalar<T>.Zero();
    var product = x[0];
    for (var i = 1; i < x.Length; ++i)
      product = Scalar<T>.Multiply(product, x[i]);
    return product;
  }

  /// <summary>Computes the sum of the absolute values of all elements.</summary>
  public static T SumOfMagnitudes<T>(ReadOnlySpan<T> x) {
    var sum = Scalar<T>.Zero();
    for (var i = 0; i < x.Length; ++i)
      sum = Scalar<T>.Add(sum, Scalar<T>.Abs(x[i]));
    return sum;
  }

  /// <summary>Computes the sum of the squares of all elements.</summary>
  public static T SumOfSquares<T>(ReadOnlySpan<T> x) {
    var sum = Scalar<T>.Zero();
    for (var i = 0; i < x.Length; ++i)
      sum = Scalar<T>.Add(sum, Scalar<T>.Multiply(x[i], x[i]));
    return sum;
  }

  /// <summary>Computes the Euclidean norm (L2 norm) of the span.</summary>
  public static T Norm<T>(ReadOnlySpan<T> x) => Scalar<T>.Sqrt(SumOfSquares(x));

  /// <summary>Computes the average of all elements in the span.</summary>
  public static T Average<T>(ReadOnlySpan<T> x) {
    if (x.Length == 0)
      throw new ArgumentException("Span must not be empty.", nameof(x));
    return Scalar<T>.Divide(Sum(x), Scalar<T>.From<int>(x.Length));
  }

  #endregion

  #region Vector Operations

  /// <summary>Computes the dot product of two spans.</summary>
  /// <param name="x">The first span.</param>
  /// <param name="y">The second span.</param>
  /// <returns>The dot product.</returns>
  public static T Dot<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y) {
    _ValidateLengths(x.Length, y.Length);
    var dot = Scalar<T>.Zero();
    for (var i = 0; i < x.Length; ++i)
      dot = Scalar<T>.Add(dot, Scalar<T>.Multiply(x[i], y[i]));
    return dot;
  }

  /// <summary>Computes the Euclidean distance between two spans.</summary>
  public static T Distance<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y) {
    _ValidateLengths(x.Length, y.Length);
    var sumSquares = Scalar<T>.Zero();
    for (var i = 0; i < x.Length; ++i) {
      var diff = Scalar<T>.Subtract(x[i], y[i]);
      sumSquares = Scalar<T>.Add(sumSquares, Scalar<T>.Multiply(diff, diff));
    }
    return Scalar<T>.Sqrt(sumSquares);
  }

  /// <summary>Computes the squared Euclidean distance between two spans.</summary>
  public static T DistanceSquared<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y) {
    _ValidateLengths(x.Length, y.Length);
    var sumSquares = Scalar<T>.Zero();
    for (var i = 0; i < x.Length; ++i) {
      var diff = Scalar<T>.Subtract(x[i], y[i]);
      sumSquares = Scalar<T>.Add(sumSquares, Scalar<T>.Multiply(diff, diff));
    }
    return sumSquares;
  }

  /// <summary>Computes the cosine similarity between two spans.</summary>
  public static T CosineSimilarity<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y) {
    _ValidateLengths(x.Length, y.Length);
    var dotProduct = Scalar<T>.Zero();
    var normX = Scalar<T>.Zero();
    var normY = Scalar<T>.Zero();
    for (var i = 0; i < x.Length; ++i) {
      dotProduct = Scalar<T>.Add(dotProduct, Scalar<T>.Multiply(x[i], y[i]));
      normX = Scalar<T>.Add(normX, Scalar<T>.Multiply(x[i], x[i]));
      normY = Scalar<T>.Add(normY, Scalar<T>.Multiply(y[i], y[i]));
    }
    var denominator = Scalar<T>.Multiply(Scalar<T>.Sqrt(normX), Scalar<T>.Sqrt(normY));
    return Scalar<T>.Divide(dotProduct, denominator);
  }

  #endregion

  #region Comparison Operations

  /// <summary>Computes the element-wise maximum of two spans.</summary>
  public static void Max<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) {
    _ValidateLengths(x.Length, y.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Max(x[i], y[i]);
  }

  /// <summary>Computes the element-wise minimum of two spans.</summary>
  public static void Min<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) {
    _ValidateLengths(x.Length, y.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Min(x[i], y[i]);
  }

  /// <summary>Returns the maximum value in the span.</summary>
  public static T Max<T>(ReadOnlySpan<T> x) {
    if (x.Length == 0)
      throw new ArgumentException("Span must not be empty.", nameof(x));
    var max = x[0];
    for (var i = 1; i < x.Length; ++i)
      if (Scalar<T>.GreaterThan(x[i], max))
        max = x[i];
    return max;
  }

  /// <summary>Returns the minimum value in the span.</summary>
  public static T Min<T>(ReadOnlySpan<T> x) {
    if (x.Length == 0)
      throw new ArgumentException("Span must not be empty.", nameof(x));
    var min = x[0];
    for (var i = 1; i < x.Length; ++i)
      if (Scalar<T>.LessThan(x[i], min))
        min = x[i];
    return min;
  }

  /// <summary>Returns the maximum magnitude (absolute value) in the span.</summary>
  public static T MaxMagnitude<T>(ReadOnlySpan<T> x) {
    if (x.Length == 0)
      throw new ArgumentException("Span must not be empty.", nameof(x));
    var maxMag = Scalar<T>.Abs(x[0]);
    var maxVal = x[0];
    for (var i = 1; i < x.Length; ++i) {
      var mag = Scalar<T>.Abs(x[i]);
      if (Scalar<T>.GreaterThan(mag, maxMag)) {
        maxMag = mag;
        maxVal = x[i];
      }
    }
    return maxVal;
  }

  /// <summary>Returns the minimum magnitude (absolute value) in the span.</summary>
  public static T MinMagnitude<T>(ReadOnlySpan<T> x) {
    if (x.Length == 0)
      throw new ArgumentException("Span must not be empty.", nameof(x));
    var minMag = Scalar<T>.Abs(x[0]);
    var minVal = x[0];
    for (var i = 1; i < x.Length; ++i) {
      var mag = Scalar<T>.Abs(x[i]);
      if (Scalar<T>.LessThan(mag, minMag)) {
        minMag = mag;
        minVal = x[i];
      }
    }
    return minVal;
  }

  /// <summary>Returns the index of the maximum value in the span.</summary>
  public static int IndexOfMax<T>(ReadOnlySpan<T> x) {
    if (x.Length == 0)
      throw new ArgumentException("Span must not be empty.", nameof(x));
    var maxIndex = 0;
    var max = x[0];
    for (var i = 1; i < x.Length; ++i)
      if (Scalar<T>.GreaterThan(x[i], max)) {
        max = x[i];
        maxIndex = i;
      }
    return maxIndex;
  }

  /// <summary>Returns the index of the minimum value in the span.</summary>
  public static int IndexOfMin<T>(ReadOnlySpan<T> x) {
    if (x.Length == 0)
      throw new ArgumentException("Span must not be empty.", nameof(x));
    var minIndex = 0;
    var min = x[0];
    for (var i = 1; i < x.Length; ++i)
      if (Scalar<T>.LessThan(x[i], min)) {
        min = x[i];
        minIndex = i;
      }
    return minIndex;
  }

  /// <summary>Returns the index of the maximum magnitude in the span.</summary>
  public static int IndexOfMaxMagnitude<T>(ReadOnlySpan<T> x) {
    if (x.Length == 0)
      throw new ArgumentException("Span must not be empty.", nameof(x));
    var maxIndex = 0;
    var maxMag = Scalar<T>.Abs(x[0]);
    for (var i = 1; i < x.Length; ++i) {
      var mag = Scalar<T>.Abs(x[i]);
      if (Scalar<T>.GreaterThan(mag, maxMag)) {
        maxMag = mag;
        maxIndex = i;
      }
    }
    return maxIndex;
  }

  /// <summary>Returns the index of the minimum magnitude in the span.</summary>
  public static int IndexOfMinMagnitude<T>(ReadOnlySpan<T> x) {
    if (x.Length == 0)
      throw new ArgumentException("Span must not be empty.", nameof(x));
    var minIndex = 0;
    var minMag = Scalar<T>.Abs(x[0]);
    for (var i = 1; i < x.Length; ++i) {
      var mag = Scalar<T>.Abs(x[i]);
      if (Scalar<T>.LessThan(mag, minMag)) {
        minMag = mag;
        minIndex = i;
      }
    }
    return minIndex;
  }

  /// <summary>Computes the element-wise maximum of a span and a scalar.</summary>
  public static void Max<T>(ReadOnlySpan<T> x, T y, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Max(x[i], y);
  }

  /// <summary>Computes the element-wise minimum of a span and a scalar.</summary>
  public static void Min<T>(ReadOnlySpan<T> x, T y, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Min(x[i], y);
  }

  /// <summary>Computes the element-wise maximum magnitude of two spans.</summary>
  public static void MaxMagnitude<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) {
    _ValidateLengths(x.Length, y.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i) {
      var xMag = Scalar<T>.Abs(x[i]);
      var yMag = Scalar<T>.Abs(y[i]);
      destination[i] = Scalar<T>.GreaterThan(xMag, yMag) ? x[i] : y[i];
    }
  }

  /// <summary>Computes the element-wise minimum magnitude of two spans.</summary>
  public static void MinMagnitude<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) {
    _ValidateLengths(x.Length, y.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i) {
      var xMag = Scalar<T>.Abs(x[i]);
      var yMag = Scalar<T>.Abs(y[i]);
      destination[i] = Scalar<T>.LessThan(xMag, yMag) ? x[i] : y[i];
    }
  }

  /// <summary>Computes the element-wise maximum magnitude of a span and a scalar.</summary>
  public static void MaxMagnitude<T>(ReadOnlySpan<T> x, T y, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    var yMag = Scalar<T>.Abs(y);
    for (var i = 0; i < x.Length; ++i) {
      var xMag = Scalar<T>.Abs(x[i]);
      destination[i] = Scalar<T>.GreaterThan(xMag, yMag) ? x[i] : y;
    }
  }

  /// <summary>Computes the element-wise minimum magnitude of a span and a scalar.</summary>
  public static void MinMagnitude<T>(ReadOnlySpan<T> x, T y, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    var yMag = Scalar<T>.Abs(y);
    for (var i = 0; i < x.Length; ++i) {
      var xMag = Scalar<T>.Abs(x[i]);
      destination[i] = Scalar<T>.LessThan(xMag, yMag) ? x[i] : y;
    }
  }

  /// <summary>Computes the element-wise maximum number of two spans (NaN-propagating).</summary>
  public static void MaxNumber<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) {
    _ValidateLengths(x.Length, y.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i) {
      if (Scalar<T>.IsNaN(x[i]))
        destination[i] = y[i];
      else if (Scalar<T>.IsNaN(y[i]))
        destination[i] = x[i];
      else
        destination[i] = Scalar<T>.Max(x[i], y[i]);
    }
  }

  /// <summary>Computes the element-wise minimum number of two spans (NaN-propagating).</summary>
  public static void MinNumber<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) {
    _ValidateLengths(x.Length, y.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i) {
      if (Scalar<T>.IsNaN(x[i]))
        destination[i] = y[i];
      else if (Scalar<T>.IsNaN(y[i]))
        destination[i] = x[i];
      else
        destination[i] = Scalar<T>.Min(x[i], y[i]);
    }
  }

  /// <summary>Computes the element-wise maximum number of a span and a scalar.</summary>
  public static void MaxNumber<T>(ReadOnlySpan<T> x, T y, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    var yIsNaN = Scalar<T>.IsNaN(y);
    for (var i = 0; i < x.Length; ++i) {
      if (Scalar<T>.IsNaN(x[i]))
        destination[i] = y;
      else if (yIsNaN)
        destination[i] = x[i];
      else
        destination[i] = Scalar<T>.Max(x[i], y);
    }
  }

  /// <summary>Computes the element-wise minimum number of a span and a scalar.</summary>
  public static void MinNumber<T>(ReadOnlySpan<T> x, T y, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    var yIsNaN = Scalar<T>.IsNaN(y);
    for (var i = 0; i < x.Length; ++i) {
      if (Scalar<T>.IsNaN(x[i]))
        destination[i] = y;
      else if (yIsNaN)
        destination[i] = x[i];
      else
        destination[i] = Scalar<T>.Min(x[i], y);
    }
  }

  /// <summary>Returns the maximum number in the span (NaN-propagating).</summary>
  public static T MaxNumber<T>(ReadOnlySpan<T> x) {
    if (x.Length == 0)
      throw new ArgumentException("Span must not be empty.", nameof(x));
    var max = x[0];
    for (var i = 1; i < x.Length; ++i) {
      if (Scalar<T>.IsNaN(max))
        max = x[i];
      else if (!Scalar<T>.IsNaN(x[i]) && Scalar<T>.GreaterThan(x[i], max))
        max = x[i];
    }
    return max;
  }

  /// <summary>Returns the minimum number in the span (NaN-propagating).</summary>
  public static T MinNumber<T>(ReadOnlySpan<T> x) {
    if (x.Length == 0)
      throw new ArgumentException("Span must not be empty.", nameof(x));
    var min = x[0];
    for (var i = 1; i < x.Length; ++i) {
      if (Scalar<T>.IsNaN(min))
        min = x[i];
      else if (!Scalar<T>.IsNaN(x[i]) && Scalar<T>.LessThan(x[i], min))
        min = x[i];
    }
    return min;
  }

  /// <summary>Returns the maximum magnitude number in the span (NaN-propagating).</summary>
  public static T MaxMagnitudeNumber<T>(ReadOnlySpan<T> x) {
    if (x.Length == 0)
      throw new ArgumentException("Span must not be empty.", nameof(x));
    var maxVal = x[0];
    var maxMag = Scalar<T>.Abs(x[0]);
    for (var i = 1; i < x.Length; ++i) {
      var mag = Scalar<T>.Abs(x[i]);
      if (Scalar<T>.IsNaN(maxMag)) {
        maxVal = x[i];
        maxMag = mag;
      } else if (!Scalar<T>.IsNaN(mag) && Scalar<T>.GreaterThan(mag, maxMag)) {
        maxVal = x[i];
        maxMag = mag;
      }
    }
    return maxVal;
  }

  /// <summary>Returns the minimum magnitude number in the span (NaN-propagating).</summary>
  public static T MinMagnitudeNumber<T>(ReadOnlySpan<T> x) {
    if (x.Length == 0)
      throw new ArgumentException("Span must not be empty.", nameof(x));
    var minVal = x[0];
    var minMag = Scalar<T>.Abs(x[0]);
    for (var i = 1; i < x.Length; ++i) {
      var mag = Scalar<T>.Abs(x[i]);
      if (Scalar<T>.IsNaN(minMag)) {
        minVal = x[i];
        minMag = mag;
      } else if (!Scalar<T>.IsNaN(mag) && Scalar<T>.LessThan(mag, minMag)) {
        minVal = x[i];
        minMag = mag;
      }
    }
    return minVal;
  }

  /// <summary>Clamps each element in the span to the specified minimum and maximum values.</summary>
  public static void Clamp<T>(ReadOnlySpan<T> x, T min, T max, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i) {
      var val = x[i];
      if (Scalar<T>.LessThan(val, min))
        destination[i] = min;
      else if (Scalar<T>.GreaterThan(val, max))
        destination[i] = max;
      else
        destination[i] = val;
    }
  }

  /// <summary>Clamps each element in the span between per-element min and max.</summary>
  public static void Clamp<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> min, ReadOnlySpan<T> max, Span<T> destination) {
    _ValidateLengths(x.Length, min.Length, max.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i) {
      var val = x[i];
      if (Scalar<T>.LessThan(val, min[i]))
        destination[i] = min[i];
      else if (Scalar<T>.GreaterThan(val, max[i]))
        destination[i] = max[i];
      else
        destination[i] = val;
    }
  }

  #endregion

  #region ML/Activation Functions

  /// <summary>Computes the element-wise sigmoid function: 1 / (1 + exp(-x)).</summary>
  public static void Sigmoid<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    var one = Scalar<T>.One;
    for (var i = 0; i < x.Length; ++i) {
      var expNegX = Scalar<T>.Exp(Scalar<T>.Negate(x[i]));
      destination[i] = Scalar<T>.Divide(one, Scalar<T>.Add(one, expNegX));
    }
  }

  /// <summary>Computes the element-wise hyperbolic tangent.</summary>
  public static void Tanh<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    var one = Scalar<T>.One;
    var two = Scalar<T>.Add(one, one);
    for (var i = 0; i < x.Length; ++i) {
      // tanh(x) = (exp(2x) - 1) / (exp(2x) + 1)
      var exp2x = Scalar<T>.Exp(Scalar<T>.Multiply(two, x[i]));
      var numerator = Scalar<T>.Subtract(exp2x, one);
      var denominator = Scalar<T>.Add(exp2x, one);
      destination[i] = Scalar<T>.Divide(numerator, denominator);
    }
  }

  /// <summary>Computes the softmax function across the span.</summary>
  public static void SoftMax<T>(ReadOnlySpan<T> x, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    if (x.Length == 0)
      return;

    // Find max for numerical stability
    var max = Max(x);

    // Compute exp(x - max) and sum
    var sum = Scalar<T>.Zero();
    for (var i = 0; i < x.Length; ++i) {
      destination[i] = Scalar<T>.Exp(Scalar<T>.Subtract(x[i], max));
      sum = Scalar<T>.Add(sum, destination[i]);
    }

    // Normalize
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.Divide(destination[i], sum);
  }

  #endregion

  #region Conversion Operations

  /// <summary>Converts elements from TFrom to TTo.</summary>
  public static void ConvertChecked<TFrom, TTo>(ReadOnlySpan<TFrom> source, Span<TTo> destination) {
    _ValidateDestination(source.Length, destination.Length);
    for (var i = 0; i < source.Length; ++i)
      destination[i] = Scalar<TFrom>.To<TTo>(source[i]);
  }

  /// <summary>Converts elements from TFrom to TTo (saturating).</summary>
  public static void ConvertSaturating<TFrom, TTo>(ReadOnlySpan<TFrom> source, Span<TTo> destination) {
    _ValidateDestination(source.Length, destination.Length);
    for (var i = 0; i < source.Length; ++i)
      destination[i] = Scalar<TFrom>.To<TTo>(source[i]);
  }

  /// <summary>Converts elements from TFrom to TTo (truncating).</summary>
  public static void ConvertTruncating<TFrom, TTo>(ReadOnlySpan<TFrom> source, Span<TTo> destination) {
    _ValidateDestination(source.Length, destination.Length);
    for (var i = 0; i < source.Length; ++i)
      destination[i] = Scalar<TFrom>.To<TTo>(source[i]);
  }

  #endregion

  #region Bitwise Operations

  /// <summary>Computes the element-wise bitwise AND of two spans.</summary>
  public static void BitwiseAnd<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) where T : struct {
    _ValidateLengths(x.Length, y.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = _BitwiseAnd(x[i], y[i]);
  }

  /// <summary>Computes the element-wise bitwise OR of two spans.</summary>
  public static void BitwiseOr<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) where T : struct {
    _ValidateLengths(x.Length, y.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = _BitwiseOr(x[i], y[i]);
  }

  /// <summary>Computes the element-wise bitwise XOR of two spans.</summary>
  public static void Xor<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination) where T : struct {
    _ValidateLengths(x.Length, y.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = _BitwiseXor(x[i], y[i]);
  }

  /// <summary>Computes the element-wise bitwise NOT of a span.</summary>
  public static void OnesComplement<T>(ReadOnlySpan<T> x, Span<T> destination) where T : struct {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = _BitwiseNot(x[i]);
  }

  #endregion

  #region Shift Operations

  /// <summary>Computes the element-wise left shift.</summary>
  public static void ShiftLeft<T>(ReadOnlySpan<T> x, int shiftAmount, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.ShiftLeft(x[i], shiftAmount);
  }

  /// <summary>Computes the element-wise arithmetic right shift.</summary>
  public static void ShiftRightArithmetic<T>(ReadOnlySpan<T> x, int shiftAmount, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.ShiftRightArithmetic(x[i], shiftAmount);
  }

  /// <summary>Computes the element-wise logical right shift.</summary>
  public static void ShiftRightLogical<T>(ReadOnlySpan<T> x, int shiftAmount, Span<T> destination) {
    _ValidateDestination(x.Length, destination.Length);
    for (var i = 0; i < x.Length; ++i)
      destination[i] = Scalar<T>.ShiftRightLogical(x[i], shiftAmount);
  }

  #endregion

  #region Private Helpers

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _ValidateDestination(int sourceLength, int destinationLength) {
    if (sourceLength > destinationLength)
      throw new ArgumentException("Destination span is too short.");
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _ValidateLengths(int xLength, int yLength) {
    if (xLength != yLength)
      throw new ArgumentException("Span lengths must match.");
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _ValidateLengths(int xLength, int yLength, int destinationLength) {
    if (xLength != yLength)
      throw new ArgumentException("Span lengths must match.");
    if (xLength > destinationLength)
      throw new ArgumentException("Destination span is too short.");
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _ValidateLengths(int xLength, int yLength, int zLength, int destinationLength) {
    if (xLength != yLength || yLength != zLength)
      throw new ArgumentException("Span lengths must match.");
    if (xLength > destinationLength)
      throw new ArgumentException("Destination span is too short.");
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe T _BitwiseAnd<T>(T left, T right) where T : struct {
    var size = Unsafe.SizeOf<T>();
    var result = default(T);
    var pLeft = (byte*)Unsafe.AsPointer(ref left);
    var pRight = (byte*)Unsafe.AsPointer(ref right);
    var pResult = (byte*)Unsafe.AsPointer(ref result);
    for (var i = 0; i < size; ++i)
      pResult[i] = (byte)(pLeft[i] & pRight[i]);
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe T _BitwiseOr<T>(T left, T right) where T : struct {
    var size = Unsafe.SizeOf<T>();
    var result = default(T);
    var pLeft = (byte*)Unsafe.AsPointer(ref left);
    var pRight = (byte*)Unsafe.AsPointer(ref right);
    var pResult = (byte*)Unsafe.AsPointer(ref result);
    for (var i = 0; i < size; ++i)
      pResult[i] = (byte)(pLeft[i] | pRight[i]);
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe T _BitwiseXor<T>(T left, T right) where T : struct {
    var size = Unsafe.SizeOf<T>();
    var result = default(T);
    var pLeft = (byte*)Unsafe.AsPointer(ref left);
    var pRight = (byte*)Unsafe.AsPointer(ref right);
    var pResult = (byte*)Unsafe.AsPointer(ref result);
    for (var i = 0; i < size; ++i)
      pResult[i] = (byte)(pLeft[i] ^ pRight[i]);
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe T _BitwiseNot<T>(T value) where T : struct {
    var size = Unsafe.SizeOf<T>();
    var result = default(T);
    var pValue = (byte*)Unsafe.AsPointer(ref value);
    var pResult = (byte*)Unsafe.AsPointer(ref result);
    for (var i = 0; i < size; ++i)
      pResult[i] = (byte)~pValue[i];
    return result;
  }

  #endregion

}

#endif
