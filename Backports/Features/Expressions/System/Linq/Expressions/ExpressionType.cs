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

// ExpressionType enum exists in System.Core since .NET 3.5
// Only polyfill for net20 where no expression trees exist
#if !SUPPORTS_LINQ

namespace System.Linq.Expressions;

/// <summary>
/// Describes the node types for the nodes of an expression tree.
/// </summary>
public enum ExpressionType {

  /// <summary>
  /// An addition operation, such as <c>a + b</c>, without overflow checking.
  /// </summary>
  Add = 0,

  /// <summary>
  /// An addition operation, such as <c>(a + b)</c>, with overflow checking, for numeric operands.
  /// </summary>
  AddChecked = 1,

  /// <summary>
  /// A bitwise or logical <c>AND</c> operation, such as <c>(a &amp; b)</c>.
  /// </summary>
  And = 2,

  /// <summary>
  /// A conditional <c>AND</c> operation that evaluates the second operand only if the first operand evaluates to <c>true</c>.
  /// </summary>
  AndAlso = 3,

  /// <summary>
  /// An operation that obtains the length of a one-dimensional array, such as <c>array.Length</c>.
  /// </summary>
  ArrayLength = 4,

  /// <summary>
  /// An indexing operation in a one-dimensional array, such as <c>array[index]</c>.
  /// </summary>
  ArrayIndex = 5,

  /// <summary>
  /// A method call, such as in the <c>obj.Method()</c> expression.
  /// </summary>
  Call = 6,

  /// <summary>
  /// A node that represents a null coalescing operation, such as <c>(a ?? b)</c>.
  /// </summary>
  Coalesce = 7,

  /// <summary>
  /// A conditional operation, such as <c>a &gt; b ? a : b</c>.
  /// </summary>
  Conditional = 8,

  /// <summary>
  /// A constant value.
  /// </summary>
  Constant = 9,

  /// <summary>
  /// A cast or conversion operation, such as <c>(SampleType)obj</c>. For a numeric conversion, if the converted value
  /// is too large for the destination type, no exception is thrown.
  /// </summary>
  Convert = 10,

  /// <summary>
  /// A cast or conversion operation, such as <c>(SampleType)obj</c>. For a numeric conversion, if the converted value
  /// does not fit the destination type, an exception is thrown.
  /// </summary>
  ConvertChecked = 11,

  /// <summary>
  /// A division operation, such as <c>(a / b)</c>.
  /// </summary>
  Divide = 12,

  /// <summary>
  /// A node that represents an equality comparison, such as <c>(a == b)</c>.
  /// </summary>
  Equal = 13,

  /// <summary>
  /// A bitwise or logical <c>XOR</c> operation, such as <c>(a ^ b)</c>.
  /// </summary>
  ExclusiveOr = 14,

  /// <summary>
  /// A "greater than" comparison, such as <c>(a &gt; b)</c>.
  /// </summary>
  GreaterThan = 15,

  /// <summary>
  /// A "greater than or equal to" comparison, such as <c>(a &gt;= b)</c>.
  /// </summary>
  GreaterThanOrEqual = 16,

  /// <summary>
  /// An operation that invokes a delegate or lambda expression, such as <c>sampleDelegate.Invoke()</c>.
  /// </summary>
  Invoke = 17,

  /// <summary>
  /// A lambda expression, such as <c>a =&gt; a + a</c>.
  /// </summary>
  Lambda = 18,

  /// <summary>
  /// A bitwise left-shift operation, such as <c>(a &lt;&lt; b)</c>.
  /// </summary>
  LeftShift = 19,

  /// <summary>
  /// A "less than" comparison, such as <c>(a &lt; b)</c>.
  /// </summary>
  LessThan = 20,

  /// <summary>
  /// A "less than or equal to" comparison, such as <c>(a &lt;= b)</c>.
  /// </summary>
  LessThanOrEqual = 21,

  /// <summary>
  /// An operation that creates a new <see cref="System.Collections.IEnumerable"/> object and initializes it from a
  /// list of elements, such as <c>new List&lt;SampleType&gt;() { a, b, c }</c>.
  /// </summary>
  ListInit = 22,

  /// <summary>
  /// An operation that reads from a field or property, such as <c>obj.Field</c> or <c>obj.Property</c>.
  /// </summary>
  MemberAccess = 23,

  /// <summary>
  /// An operation that creates a new object and initializes one or more of its members, such as
  /// <c>new Point { X = 1, Y = 2 }</c>.
  /// </summary>
  MemberInit = 24,

  /// <summary>
  /// An arithmetic remainder operation, such as <c>(a % b)</c>.
  /// </summary>
  Modulo = 25,

  /// <summary>
  /// A multiplication operation, such as <c>(a * b)</c>, without overflow checking.
  /// </summary>
  Multiply = 26,

  /// <summary>
  /// A multiplication operation, such as <c>(a * b)</c>, that has overflow checking, for numeric operands.
  /// </summary>
  MultiplyChecked = 27,

  /// <summary>
  /// An arithmetic negation operation, such as <c>(-a)</c>. The object <c>a</c> should not be modified in place.
  /// </summary>
  Negate = 28,

  /// <summary>
  /// A unary plus operation, such as <c>(+a)</c>. The result of a predefined unary plus operation is the value
  /// of the operand, but user-defined implementations might have unusual results.
  /// </summary>
  UnaryPlus = 29,

  /// <summary>
  /// An arithmetic negation operation, such as <c>(-a)</c>, that has overflow checking.
  /// </summary>
  NegateChecked = 30,

  /// <summary>
  /// An operation that calls a constructor to create a new object, such as <c>new SampleType()</c>.
  /// </summary>
  New = 31,

  /// <summary>
  /// An operation that creates a new one-dimensional array and initializes it from a list of elements,
  /// such as <c>new SampleType[] { a, b, c }</c>.
  /// </summary>
  NewArrayInit = 32,

  /// <summary>
  /// An operation that creates a new array, in which the bounds for each dimension are specified,
  /// such as <c>new SampleType[dim1, dim2]</c>.
  /// </summary>
  NewArrayBounds = 33,

  /// <summary>
  /// A bitwise complement or logical negation operation. In C#, it is equivalent to <c>(~a)</c> for integral types
  /// and to <c>(!a)</c> for Boolean values.
  /// </summary>
  Not = 34,

  /// <summary>
  /// An inequality comparison, such as <c>(a != b)</c>.
  /// </summary>
  NotEqual = 35,

  /// <summary>
  /// A bitwise or logical <c>OR</c> operation, such as <c>(a | b)</c>.
  /// </summary>
  Or = 36,

  /// <summary>
  /// A short-circuiting conditional <c>OR</c> operation, such as <c>(a || b)</c>.
  /// </summary>
  OrElse = 37,

  /// <summary>
  /// A reference to a parameter or variable defined in the context of the expression.
  /// </summary>
  Parameter = 38,

  /// <summary>
  /// A mathematical operation that raises a number to a power, such as <c>(a ^ b)</c>.
  /// </summary>
  Power = 39,

  /// <summary>
  /// An expression that has a constant value of type <see cref="Expression"/>. A <see cref="ExpressionType.Quote"/>
  /// node can contain references to parameters that are defined in the context of the expression it represents.
  /// </summary>
  Quote = 40,

  /// <summary>
  /// A bitwise right-shift operation, such as <c>(a &gt;&gt; b)</c>.
  /// </summary>
  RightShift = 41,

  /// <summary>
  /// A subtraction operation, such as <c>(a - b)</c>, without overflow checking.
  /// </summary>
  Subtract = 42,

  /// <summary>
  /// A subtraction operation, such as <c>(a - b)</c>, that has overflow checking, for numeric operands.
  /// </summary>
  SubtractChecked = 43,

  /// <summary>
  /// An explicit reference or boxing conversion in which <c>null</c> is supplied if the conversion fails,
  /// such as <c>(obj as SampleType)</c>.
  /// </summary>
  TypeAs = 44,

  /// <summary>
  /// A type test, such as <c>obj is SampleType</c>.
  /// </summary>
  TypeIs = 45,

  /// <summary>
  /// An assignment operation, such as <c>(a = b)</c>.
  /// </summary>
  Assign = 46,

  /// <summary>
  /// A block of expressions.
  /// </summary>
  Block = 47,

  /// <summary>
  /// Debugging information.
  /// </summary>
  DebugInfo = 48,

  /// <summary>
  /// A unary decrement operation, such as <c>(a - 1)</c>. The object <c>a</c> should not be modified in place.
  /// </summary>
  Decrement = 49,

  /// <summary>
  /// A dynamic operation.
  /// </summary>
  Dynamic = 50,

  /// <summary>
  /// A default value.
  /// </summary>
  Default = 51,

  /// <summary>
  /// An extension expression.
  /// </summary>
  Extension = 52,

  /// <summary>
  /// A "go to" expression, such as <c>goto Label</c>.
  /// </summary>
  Goto = 53,

  /// <summary>
  /// A unary increment operation, such as <c>(a + 1)</c>. The object <c>a</c> should not be modified in place.
  /// </summary>
  Increment = 54,

  /// <summary>
  /// An index operation or an operation that accesses a property that takes arguments.
  /// </summary>
  Index = 55,

  /// <summary>
  /// A label.
  /// </summary>
  Label = 56,

  /// <summary>
  /// A list of run-time variables.
  /// </summary>
  RuntimeVariables = 57,

  /// <summary>
  /// A loop, such as <c>for</c> or <c>while</c>.
  /// </summary>
  Loop = 58,

  /// <summary>
  /// A switch operation, such as <c>switch</c>.
  /// </summary>
  Switch = 59,

  /// <summary>
  /// An operation that throws an exception, such as <c>throw new Exception()</c>.
  /// </summary>
  Throw = 60,

  /// <summary>
  /// A <c>try-catch</c> expression.
  /// </summary>
  Try = 61,

  /// <summary>
  /// An unbox value type operation, such as <c>unbox</c> and <c>unbox.any</c> instructions in MSIL.
  /// </summary>
  Unbox = 62,

  /// <summary>
  /// An addition compound assignment operation, such as <c>(a += b)</c>, without overflow checking.
  /// </summary>
  AddAssign = 63,

  /// <summary>
  /// A bitwise or logical <c>AND</c> compound assignment operation, such as <c>(a &amp;= b)</c>.
  /// </summary>
  AndAssign = 64,

  /// <summary>
  /// A division compound assignment operation, such as <c>(a /= b)</c>.
  /// </summary>
  DivideAssign = 65,

  /// <summary>
  /// A bitwise or logical <c>XOR</c> compound assignment operation, such as <c>(a ^= b)</c>.
  /// </summary>
  ExclusiveOrAssign = 66,

  /// <summary>
  /// A bitwise left-shift compound assignment, such as <c>(a &lt;&lt;= b)</c>.
  /// </summary>
  LeftShiftAssign = 67,

  /// <summary>
  /// An arithmetic remainder compound assignment operation, such as <c>(a %= b)</c>.
  /// </summary>
  ModuloAssign = 68,

  /// <summary>
  /// A multiplication compound assignment operation, such as <c>(a *= b)</c>, without overflow checking.
  /// </summary>
  MultiplyAssign = 69,

  /// <summary>
  /// A bitwise or logical <c>OR</c> compound assignment operation, such as <c>(a |= b)</c>.
  /// </summary>
  OrAssign = 70,

  /// <summary>
  /// A compound assignment operation that raises a number to a power, such as <c>(a ^= b)</c>.
  /// </summary>
  PowerAssign = 71,

  /// <summary>
  /// A bitwise right-shift compound assignment operation, such as <c>(a &gt;&gt;= b)</c>.
  /// </summary>
  RightShiftAssign = 72,

  /// <summary>
  /// A subtraction compound assignment operation, such as <c>(a -= b)</c>, without overflow checking.
  /// </summary>
  SubtractAssign = 73,

  /// <summary>
  /// An addition compound assignment operation, such as <c>(a += b)</c>, with overflow checking.
  /// </summary>
  AddAssignChecked = 74,

  /// <summary>
  /// A multiplication compound assignment operation, such as <c>(a *= b)</c>, that has overflow checking.
  /// </summary>
  MultiplyAssignChecked = 75,

  /// <summary>
  /// A subtraction compound assignment operation, such as <c>(a -= b)</c>, that has overflow checking.
  /// </summary>
  SubtractAssignChecked = 76,

  /// <summary>
  /// A unary prefix increment, such as <c>(++a)</c>. The object <c>a</c> should be modified in place.
  /// </summary>
  PreIncrementAssign = 77,

  /// <summary>
  /// A unary prefix decrement, such as <c>(--a)</c>. The object <c>a</c> should be modified in place.
  /// </summary>
  PreDecrementAssign = 78,

  /// <summary>
  /// A unary postfix increment, such as <c>(a++)</c>. The object <c>a</c> should be modified in place.
  /// </summary>
  PostIncrementAssign = 79,

  /// <summary>
  /// A unary postfix decrement, such as <c>(a--)</c>. The object <c>a</c> should be modified in place.
  /// </summary>
  PostDecrementAssign = 80,

  /// <summary>
  /// An exact type test.
  /// </summary>
  TypeEqual = 81,

  /// <summary>
  /// A ones complement operation, such as <c>(~a)</c>.
  /// </summary>
  OnesComplement = 82,

  /// <summary>
  /// A true condition value.
  /// </summary>
  IsTrue = 83,

  /// <summary>
  /// A false condition value.
  /// </summary>
  IsFalse = 84

}

#endif
