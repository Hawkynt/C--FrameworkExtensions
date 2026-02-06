#region (c)2010-2042 Hawkynt

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

#endregion

#if !SUPPORTS_GENERIC_MATH

namespace System.Numerics;

/// <summary>
/// Defines a mechanism for computing the sum of two values.
/// </summary>
/// <remarks>
/// This is a marker interface for pre-.NET 7 compatibility.
/// On .NET 7+, the BCL's interface with static abstract members is used.
/// </remarks>
public interface IAdditionOperators<TSelf, TOther, TResult> where TSelf : IAdditionOperators<TSelf, TOther, TResult>?;

/// <summary>
/// Defines a mechanism for getting the additive identity of a given type.
/// </summary>
public interface IAdditiveIdentity<TSelf, TResult> where TSelf : IAdditiveIdentity<TSelf, TResult>?;

/// <summary>
/// Defines a type that represents a binary integer.
/// </summary>
public interface IBinaryInteger<TSelf>
  : IBinaryNumber<TSelf>,
    IShiftOperators<TSelf, int, TSelf>
  where TSelf : IBinaryInteger<TSelf>?;

/// <summary>
/// Defines a type that represents a binary number.
/// </summary>
public interface IBinaryNumber<TSelf>
  : IBitwiseOperators<TSelf, TSelf, TSelf>,
    INumber<TSelf>
  where TSelf : IBinaryNumber<TSelf>?;

/// <summary>
/// Defines a mechanism for performing bitwise operations over two values.
/// </summary>
public interface IBitwiseOperators<TSelf, TOther, TResult> where TSelf : IBitwiseOperators<TSelf, TOther, TResult>?;

/// <summary>
/// Defines a mechanism for comparing two values to determine relative order.
/// </summary>
public interface IComparisonOperators<TSelf, TOther, TResult>
  : IEqualityOperators<TSelf, TOther, TResult>
  where TSelf : IComparisonOperators<TSelf, TOther, TResult>?;

/// <summary>
/// Defines a mechanism for decrementing a given value.
/// </summary>
public interface IDecrementOperators<TSelf> where TSelf : IDecrementOperators<TSelf>?;

/// <summary>
/// Defines a mechanism for computing the quotient of two values.
/// </summary>
public interface IDivisionOperators<TSelf, TOther, TResult> where TSelf : IDivisionOperators<TSelf, TOther, TResult>?;

/// <summary>
/// Defines a mechanism for comparing two values to determine equality.
/// </summary>
public interface IEqualityOperators<TSelf, TOther, TResult> where TSelf : IEqualityOperators<TSelf, TOther, TResult>?;

/// <summary>
/// Defines support for exponential functions.
/// </summary>
public interface IExponentialFunctions<TSelf> where TSelf : IExponentialFunctions<TSelf>?;

/// <summary>
/// Defines a floating-point type.
/// </summary>
public interface IFloatingPoint<TSelf>
  : IFloatingPointConstants<TSelf>,
    INumber<TSelf>,
    ISignedNumber<TSelf>
  where TSelf : IFloatingPoint<TSelf>?;

/// <summary>
/// Defines support for floating-point constants.
/// </summary>
public interface IFloatingPointConstants<TSelf> where TSelf : IFloatingPointConstants<TSelf>?;

/// <summary>
/// Defines an IEEE 754 floating-point type.
/// </summary>
public interface IFloatingPointIeee754<TSelf>
  : IFloatingPoint<TSelf>,
    IExponentialFunctions<TSelf>,
    IHyperbolicFunctions<TSelf>,
    ILogarithmicFunctions<TSelf>,
    IPowerFunctions<TSelf>,
    IRootFunctions<TSelf>,
    ITrigonometricFunctions<TSelf>
  where TSelf : IFloatingPointIeee754<TSelf>?;

/// <summary>
/// Defines support for hyperbolic functions.
/// </summary>
public interface IHyperbolicFunctions<TSelf> where TSelf : IHyperbolicFunctions<TSelf>?;

/// <summary>
/// Defines a mechanism for incrementing a given value.
/// </summary>
public interface IIncrementOperators<TSelf> where TSelf : IIncrementOperators<TSelf>?;

/// <summary>
/// Defines support for logarithmic functions.
/// </summary>
public interface ILogarithmicFunctions<TSelf> where TSelf : ILogarithmicFunctions<TSelf>?;

/// <summary>
/// Defines a type that has a minimum and maximum value.
/// </summary>
public interface IMinMaxValue<TSelf> where TSelf : IMinMaxValue<TSelf>?;

/// <summary>
/// Defines a mechanism for computing the modulus or remainder of two values.
/// </summary>
public interface IModulusOperators<TSelf, TOther, TResult> where TSelf : IModulusOperators<TSelf, TOther, TResult>?;

/// <summary>
/// Defines a mechanism for getting the multiplicative identity of a given type.
/// </summary>
public interface IMultiplicativeIdentity<TSelf, TResult> where TSelf : IMultiplicativeIdentity<TSelf, TResult>?;

/// <summary>
/// Defines a mechanism for computing the product of two values.
/// </summary>
public interface IMultiplyOperators<TSelf, TOther, TResult> where TSelf : IMultiplyOperators<TSelf, TOther, TResult>?;

/// <summary>
/// Defines a number type.
/// </summary>
public interface INumber<TSelf>
  : IComparable,
    IComparable<TSelf>,
    IComparisonOperators<TSelf, TSelf, bool>,
    IModulusOperators<TSelf, TSelf, TSelf>,
    INumberBase<TSelf>
  where TSelf : INumber<TSelf>?;

/// <summary>
/// Defines the base of other number types.
/// </summary>
public interface INumberBase<TSelf>
  : IAdditionOperators<TSelf, TSelf, TSelf>,
    IAdditiveIdentity<TSelf, TSelf>,
    IDecrementOperators<TSelf>,
    IDivisionOperators<TSelf, TSelf, TSelf>,
    IEquatable<TSelf>,
    IEqualityOperators<TSelf, TSelf, bool>,
    IIncrementOperators<TSelf>,
    IMultiplicativeIdentity<TSelf, TSelf>,
    IMultiplyOperators<TSelf, TSelf, TSelf>,
    ISubtractionOperators<TSelf, TSelf, TSelf>,
    IUnaryPlusOperators<TSelf, TSelf>,
    IUnaryNegationOperators<TSelf, TSelf>
  where TSelf : INumberBase<TSelf>?;

/// <summary>
/// Defines support for power functions.
/// </summary>
public interface IPowerFunctions<TSelf> where TSelf : IPowerFunctions<TSelf>?;

/// <summary>
/// Defines support for root functions.
/// </summary>
public interface IRootFunctions<TSelf> where TSelf : IRootFunctions<TSelf>?;

/// <summary>
/// Defines a mechanism for shifting a value by another value.
/// </summary>
public interface IShiftOperators<TSelf, TOther, TResult> where TSelf : IShiftOperators<TSelf, TOther, TResult>?;

/// <summary>
/// Defines a number type which can represent both positive and negative values.
/// </summary>
public interface ISignedNumber<TSelf> : INumberBase<TSelf> where TSelf : ISignedNumber<TSelf>?;

/// <summary>
/// Defines a mechanism for computing the difference of two values.
/// </summary>
public interface ISubtractionOperators<TSelf, TOther, TResult> where TSelf : ISubtractionOperators<TSelf, TOther, TResult>?;

/// <summary>
/// Defines support for trigonometric functions.
/// </summary>
public interface ITrigonometricFunctions<TSelf> where TSelf : ITrigonometricFunctions<TSelf>?;

/// <summary>
/// Defines a mechanism for computing the unary negation of a value.
/// </summary>
public interface IUnaryNegationOperators<TSelf, TResult> where TSelf : IUnaryNegationOperators<TSelf, TResult>?;

/// <summary>
/// Defines a mechanism for computing the unary plus of a value.
/// </summary>
public interface IUnaryPlusOperators<TSelf, TResult> where TSelf : IUnaryPlusOperators<TSelf, TResult>?;

/// <summary>
/// Defines a number type which can only represent positive values.
/// </summary>
public interface IUnsignedNumber<TSelf> : INumberBase<TSelf> where TSelf : IUnsignedNumber<TSelf>?;

#endif
