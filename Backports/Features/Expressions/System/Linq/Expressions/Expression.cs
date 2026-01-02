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

// Expression Trees were introduced in .NET 3.5 with LINQ (System.Core)
// Only polyfill for net20 where no expression trees exist
#if !SUPPORTS_LINQ

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace System.Linq.Expressions;

/// <summary>
/// Provides the base class from which the classes that represent expression tree nodes are derived.
/// </summary>
/// <remarks>
/// <para>
/// The following table summarizes the factory methods that are available for creating expression tree nodes.
/// </para>
/// <para>
/// Expression trees represent code in a tree-like data structure, where each node is an expression,
/// for example, a method call or a binary operation such as <c>x &lt; y</c>.
/// </para>
/// </remarks>
public abstract class Expression {

  /// <summary>
  /// Initializes a new instance of the <see cref="Expression"/> class.
  /// </summary>
  /// <param name="nodeType">The <see cref="ExpressionType"/> of the <see cref="Expression"/>.</param>
  /// <param name="type">The <see cref="Type"/> of the value this expression represents.</param>
  protected Expression(ExpressionType nodeType, Type type) {
    this.NodeType = nodeType;
    this.Type = type;
  }

  /// <summary>
  /// Gets the node type of this <see cref="Expression"/>.
  /// </summary>
  /// <value>One of the <see cref="ExpressionType"/> values.</value>
  public ExpressionType NodeType { get; }

  /// <summary>
  /// Gets the static type of the expression that this <see cref="Expression"/> represents.
  /// </summary>
  /// <value>The <see cref="System.Type"/> that represents the static type of the expression.</value>
  public Type Type { get; }

  /// <summary>
  /// Indicates that the node can be reduced to a simpler node.
  /// </summary>
  /// <value><c>true</c> if the node can be reduced; otherwise, <c>false</c>.</value>
  public virtual bool CanReduce => false;

  /// <summary>
  /// Reduces this node to a simpler expression. If <see cref="CanReduce"/> returns <c>true</c>,
  /// this should return a valid expression. This method can return another node which itself must be reduced.
  /// </summary>
  /// <returns>The reduced expression.</returns>
  public virtual Expression Reduce() => this.CanReduce ? throw new InvalidOperationException("Reducible nodes must override Reduce.") : this;

  /// <summary>
  /// Reduces this node to a simpler expression. If <see cref="CanReduce"/> returns <c>true</c>,
  /// this should return a valid expression. This method is guaranteed not to return another node which itself
  /// must be reduced.
  /// </summary>
  /// <returns>The reduced expression.</returns>
  public Expression ReduceAndCheck() {
    if (!this.CanReduce)
      throw new InvalidOperationException("Node cannot be reduced.");

    var reduced = this.Reduce();
    if (reduced == null || reduced == this)
      throw new InvalidOperationException("Reduce must produce a different node.");

    return reduced;
  }

  /// <summary>
  /// Dispatches to the specific visit method for this node type.
  /// </summary>
  /// <param name="visitor">The visitor to visit this node with.</param>
  /// <returns>The result of visiting this node.</returns>
  protected internal virtual Expression Accept(ExpressionVisitor visitor) => visitor.VisitExtension(this);

  #region Factory Methods

  /// <summary>
  /// Creates a <see cref="ConstantExpression"/> that has the <see cref="ConstantExpression.Value"/> property
  /// set to the specified value.
  /// </summary>
  /// <param name="value">The value to set the <see cref="ConstantExpression.Value"/> property equal to.</param>
  /// <returns>A <see cref="ConstantExpression"/> that has the <see cref="Expression.NodeType"/> property equal to
  /// <see cref="ExpressionType.Constant"/> and the <see cref="ConstantExpression.Value"/> property set to the specified value.</returns>
  public static ConstantExpression Constant(object? value) =>
    new(value, value?.GetType() ?? typeof(object));

  /// <summary>
  /// Creates a <see cref="ConstantExpression"/> that has the <see cref="ConstantExpression.Value"/> and
  /// <see cref="Expression.Type"/> properties set to the specified values.
  /// </summary>
  /// <param name="value">The value to set the <see cref="ConstantExpression.Value"/> property equal to.</param>
  /// <param name="type">The <see cref="System.Type"/> to set the <see cref="Expression.Type"/> property equal to.</param>
  /// <returns>A <see cref="ConstantExpression"/> that has the <see cref="Expression.NodeType"/> property equal to
  /// <see cref="ExpressionType.Constant"/> and the <see cref="ConstantExpression.Value"/> and <see cref="Expression.Type"/>
  /// properties set to the specified values.</returns>
  public static ConstantExpression Constant(object? value, Type type) {
    if (type == null)
      throw new ArgumentNullException(nameof(type));
    return new(value, type);
  }

  /// <summary>
  /// Creates a <see cref="ParameterExpression"/> node that can be used to identify a parameter or a variable in an expression tree.
  /// </summary>
  /// <param name="type">The type of the parameter or variable.</param>
  /// <returns>A <see cref="ParameterExpression"/> node with the specified name and type.</returns>
  public static ParameterExpression Parameter(Type type) => Parameter(type, null);

  /// <summary>
  /// Creates a <see cref="ParameterExpression"/> node that can be used to identify a parameter or a variable in an expression tree.
  /// </summary>
  /// <param name="type">The type of the parameter or variable.</param>
  /// <param name="name">The name of the parameter or variable, used for debugging or printing purpose only.</param>
  /// <returns>A <see cref="ParameterExpression"/> node with the specified name and type.</returns>
  public static ParameterExpression Parameter(Type type, string? name) {
    if (type == null)
      throw new ArgumentNullException(nameof(type));
    return new(type, name);
  }

  /// <summary>
  /// Creates a <see cref="ParameterExpression"/> node that can be used to identify a parameter or a variable in an expression tree.
  /// </summary>
  /// <param name="type">The type of the parameter or variable.</param>
  /// <returns>A <see cref="ParameterExpression"/> node with the specified name and type.</returns>
  public static ParameterExpression Variable(Type type) => Variable(type, null);

  /// <summary>
  /// Creates a <see cref="ParameterExpression"/> node that can be used to identify a parameter or a variable in an expression tree.
  /// </summary>
  /// <param name="type">The type of the parameter or variable.</param>
  /// <param name="name">The name of the parameter or variable, used for debugging or printing purpose only.</param>
  /// <returns>A <see cref="ParameterExpression"/> node with the specified name and type.</returns>
  public static ParameterExpression Variable(Type type, string? name) {
    if (type == null)
      throw new ArgumentNullException(nameof(type));
    if (type == typeof(void))
      throw new ArgumentException("Variable cannot be of type void.", nameof(type));
    return new(type, name);
  }

  #region Binary Expressions

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents an arithmetic addition operation that does not have overflow checking.
  /// </summary>
  public static BinaryExpression Add(Expression left, Expression right) =>
    MakeBinary(ExpressionType.Add, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents an arithmetic addition operation that has overflow checking.
  /// </summary>
  public static BinaryExpression AddChecked(Expression left, Expression right) =>
    MakeBinary(ExpressionType.AddChecked, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents an arithmetic subtraction operation that does not have overflow checking.
  /// </summary>
  public static BinaryExpression Subtract(Expression left, Expression right) =>
    MakeBinary(ExpressionType.Subtract, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents an arithmetic subtraction operation that has overflow checking.
  /// </summary>
  public static BinaryExpression SubtractChecked(Expression left, Expression right) =>
    MakeBinary(ExpressionType.SubtractChecked, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents an arithmetic multiplication operation that does not have overflow checking.
  /// </summary>
  public static BinaryExpression Multiply(Expression left, Expression right) =>
    MakeBinary(ExpressionType.Multiply, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents an arithmetic multiplication operation that has overflow checking.
  /// </summary>
  public static BinaryExpression MultiplyChecked(Expression left, Expression right) =>
    MakeBinary(ExpressionType.MultiplyChecked, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents an arithmetic division operation.
  /// </summary>
  public static BinaryExpression Divide(Expression left, Expression right) =>
    MakeBinary(ExpressionType.Divide, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents an arithmetic remainder operation.
  /// </summary>
  public static BinaryExpression Modulo(Expression left, Expression right) =>
    MakeBinary(ExpressionType.Modulo, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a bitwise <c>AND</c> operation.
  /// </summary>
  public static BinaryExpression And(Expression left, Expression right) =>
    MakeBinary(ExpressionType.And, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a conditional <c>AND</c> operation.
  /// </summary>
  public static BinaryExpression AndAlso(Expression left, Expression right) =>
    MakeBinary(ExpressionType.AndAlso, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a bitwise <c>OR</c> operation.
  /// </summary>
  public static BinaryExpression Or(Expression left, Expression right) =>
    MakeBinary(ExpressionType.Or, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a conditional <c>OR</c> operation.
  /// </summary>
  public static BinaryExpression OrElse(Expression left, Expression right) =>
    MakeBinary(ExpressionType.OrElse, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a bitwise <c>XOR</c> operation.
  /// </summary>
  public static BinaryExpression ExclusiveOr(Expression left, Expression right) =>
    MakeBinary(ExpressionType.ExclusiveOr, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents an equality comparison.
  /// </summary>
  public static BinaryExpression Equal(Expression left, Expression right) =>
    MakeBinary(ExpressionType.Equal, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents an inequality comparison.
  /// </summary>
  public static BinaryExpression NotEqual(Expression left, Expression right) =>
    MakeBinary(ExpressionType.NotEqual, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a "less than" numeric comparison.
  /// </summary>
  public static BinaryExpression LessThan(Expression left, Expression right) =>
    MakeBinary(ExpressionType.LessThan, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a "less than or equal" numeric comparison.
  /// </summary>
  public static BinaryExpression LessThanOrEqual(Expression left, Expression right) =>
    MakeBinary(ExpressionType.LessThanOrEqual, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a "greater than" numeric comparison.
  /// </summary>
  public static BinaryExpression GreaterThan(Expression left, Expression right) =>
    MakeBinary(ExpressionType.GreaterThan, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a "greater than or equal" numeric comparison.
  /// </summary>
  public static BinaryExpression GreaterThanOrEqual(Expression left, Expression right) =>
    MakeBinary(ExpressionType.GreaterThanOrEqual, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a bitwise left-shift operation.
  /// </summary>
  public static BinaryExpression LeftShift(Expression left, Expression right) =>
    MakeBinary(ExpressionType.LeftShift, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a bitwise right-shift operation.
  /// </summary>
  public static BinaryExpression RightShift(Expression left, Expression right) =>
    MakeBinary(ExpressionType.RightShift, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a coalescing operation.
  /// </summary>
  public static BinaryExpression Coalesce(Expression left, Expression right) =>
    MakeBinary(ExpressionType.Coalesce, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents raising a number to a power.
  /// </summary>
  public static BinaryExpression Power(Expression left, Expression right) =>
    MakeBinary(ExpressionType.Power, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents an array indexing operation.
  /// </summary>
  public static BinaryExpression ArrayIndex(Expression array, Expression index) =>
    MakeBinary(ExpressionType.ArrayIndex, array, index);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/>, given the left and right operands, by calling an appropriate factory method.
  /// </summary>
  public static BinaryExpression MakeBinary(ExpressionType binaryType, Expression left, Expression right) {
    if (left == null)
      throw new ArgumentNullException(nameof(left));
    if (right == null)
      throw new ArgumentNullException(nameof(right));

    var resultType = GetBinaryResultType(binaryType, left.Type, right.Type);
    return new(binaryType, left, right, resultType, null);
  }

  private static Type GetBinaryResultType(ExpressionType binaryType, Type left, Type right) {
    return binaryType switch {
      ExpressionType.Equal or ExpressionType.NotEqual or ExpressionType.LessThan or ExpressionType.LessThanOrEqual
        or ExpressionType.GreaterThan or ExpressionType.GreaterThanOrEqual or ExpressionType.AndAlso
        or ExpressionType.OrElse => typeof(bool),
      ExpressionType.Coalesce => right,
      ExpressionType.ArrayIndex => left.GetElementType() ?? typeof(object),
      // Assignment expressions return the left operand type
      ExpressionType.Assign or ExpressionType.AddAssign or ExpressionType.AddAssignChecked
        or ExpressionType.SubtractAssign or ExpressionType.SubtractAssignChecked
        or ExpressionType.MultiplyAssign or ExpressionType.MultiplyAssignChecked
        or ExpressionType.DivideAssign or ExpressionType.ModuloAssign
        or ExpressionType.AndAssign or ExpressionType.OrAssign or ExpressionType.ExclusiveOrAssign
        or ExpressionType.LeftShiftAssign or ExpressionType.RightShiftAssign
        or ExpressionType.PowerAssign => left,
      _ => left
    };
  }

  #endregion

  #region Assignment Expressions

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents an assignment operation.
  /// </summary>
  public static BinaryExpression Assign(Expression left, Expression right) =>
    MakeBinary(ExpressionType.Assign, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents an addition assignment operation that does not have overflow checking.
  /// </summary>
  public static BinaryExpression AddAssign(Expression left, Expression right) =>
    MakeBinary(ExpressionType.AddAssign, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents an addition assignment operation that does not have overflow checking.
  /// </summary>
  public static BinaryExpression AddAssign(Expression left, Expression right, MethodInfo? method) =>
    new(ExpressionType.AddAssign, left, right, left.Type, method);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents an addition assignment operation that has overflow checking.
  /// </summary>
  public static BinaryExpression AddAssignChecked(Expression left, Expression right) =>
    MakeBinary(ExpressionType.AddAssignChecked, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents an addition assignment operation that has overflow checking.
  /// </summary>
  public static BinaryExpression AddAssignChecked(Expression left, Expression right, MethodInfo? method) =>
    new(ExpressionType.AddAssignChecked, left, right, left.Type, method);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a subtraction assignment operation that does not have overflow checking.
  /// </summary>
  public static BinaryExpression SubtractAssign(Expression left, Expression right) =>
    MakeBinary(ExpressionType.SubtractAssign, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a subtraction assignment operation that does not have overflow checking.
  /// </summary>
  public static BinaryExpression SubtractAssign(Expression left, Expression right, MethodInfo? method) =>
    new(ExpressionType.SubtractAssign, left, right, left.Type, method);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a subtraction assignment operation that has overflow checking.
  /// </summary>
  public static BinaryExpression SubtractAssignChecked(Expression left, Expression right) =>
    MakeBinary(ExpressionType.SubtractAssignChecked, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a subtraction assignment operation that has overflow checking.
  /// </summary>
  public static BinaryExpression SubtractAssignChecked(Expression left, Expression right, MethodInfo? method) =>
    new(ExpressionType.SubtractAssignChecked, left, right, left.Type, method);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a multiplication assignment operation that does not have overflow checking.
  /// </summary>
  public static BinaryExpression MultiplyAssign(Expression left, Expression right) =>
    MakeBinary(ExpressionType.MultiplyAssign, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a multiplication assignment operation that does not have overflow checking.
  /// </summary>
  public static BinaryExpression MultiplyAssign(Expression left, Expression right, MethodInfo? method) =>
    new(ExpressionType.MultiplyAssign, left, right, left.Type, method);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a multiplication assignment operation that has overflow checking.
  /// </summary>
  public static BinaryExpression MultiplyAssignChecked(Expression left, Expression right) =>
    MakeBinary(ExpressionType.MultiplyAssignChecked, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a multiplication assignment operation that has overflow checking.
  /// </summary>
  public static BinaryExpression MultiplyAssignChecked(Expression left, Expression right, MethodInfo? method) =>
    new(ExpressionType.MultiplyAssignChecked, left, right, left.Type, method);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a division assignment operation.
  /// </summary>
  public static BinaryExpression DivideAssign(Expression left, Expression right) =>
    MakeBinary(ExpressionType.DivideAssign, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a division assignment operation.
  /// </summary>
  public static BinaryExpression DivideAssign(Expression left, Expression right, MethodInfo? method) =>
    new(ExpressionType.DivideAssign, left, right, left.Type, method);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a remainder assignment operation.
  /// </summary>
  public static BinaryExpression ModuloAssign(Expression left, Expression right) =>
    MakeBinary(ExpressionType.ModuloAssign, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a remainder assignment operation.
  /// </summary>
  public static BinaryExpression ModuloAssign(Expression left, Expression right, MethodInfo? method) =>
    new(ExpressionType.ModuloAssign, left, right, left.Type, method);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a bitwise AND assignment operation.
  /// </summary>
  public static BinaryExpression AndAssign(Expression left, Expression right) =>
    MakeBinary(ExpressionType.AndAssign, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a bitwise AND assignment operation.
  /// </summary>
  public static BinaryExpression AndAssign(Expression left, Expression right, MethodInfo? method) =>
    new(ExpressionType.AndAssign, left, right, left.Type, method);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a bitwise OR assignment operation.
  /// </summary>
  public static BinaryExpression OrAssign(Expression left, Expression right) =>
    MakeBinary(ExpressionType.OrAssign, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a bitwise OR assignment operation.
  /// </summary>
  public static BinaryExpression OrAssign(Expression left, Expression right, MethodInfo? method) =>
    new(ExpressionType.OrAssign, left, right, left.Type, method);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a bitwise XOR assignment operation.
  /// </summary>
  public static BinaryExpression ExclusiveOrAssign(Expression left, Expression right) =>
    MakeBinary(ExpressionType.ExclusiveOrAssign, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a bitwise XOR assignment operation.
  /// </summary>
  public static BinaryExpression ExclusiveOrAssign(Expression left, Expression right, MethodInfo? method) =>
    new(ExpressionType.ExclusiveOrAssign, left, right, left.Type, method);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a bitwise left-shift assignment operation.
  /// </summary>
  public static BinaryExpression LeftShiftAssign(Expression left, Expression right) =>
    MakeBinary(ExpressionType.LeftShiftAssign, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a bitwise left-shift assignment operation.
  /// </summary>
  public static BinaryExpression LeftShiftAssign(Expression left, Expression right, MethodInfo? method) =>
    new(ExpressionType.LeftShiftAssign, left, right, left.Type, method);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a bitwise right-shift assignment operation.
  /// </summary>
  public static BinaryExpression RightShiftAssign(Expression left, Expression right) =>
    MakeBinary(ExpressionType.RightShiftAssign, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents a bitwise right-shift assignment operation.
  /// </summary>
  public static BinaryExpression RightShiftAssign(Expression left, Expression right, MethodInfo? method) =>
    new(ExpressionType.RightShiftAssign, left, right, left.Type, method);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents raising an expression to a power and assigning the result back to the expression.
  /// </summary>
  public static BinaryExpression PowerAssign(Expression left, Expression right) =>
    MakeBinary(ExpressionType.PowerAssign, left, right);

  /// <summary>
  /// Creates a <see cref="BinaryExpression"/> that represents raising an expression to a power and assigning the result back to the expression.
  /// </summary>
  public static BinaryExpression PowerAssign(Expression left, Expression right, MethodInfo? method) =>
    new(ExpressionType.PowerAssign, left, right, left.Type, method);

  #endregion

  #region Unary Expressions

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/> that represents an arithmetic negation operation.
  /// </summary>
  public static UnaryExpression Negate(Expression expression) =>
    MakeUnary(ExpressionType.Negate, expression, expression.Type);

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/> that represents an arithmetic negation operation that has overflow checking.
  /// </summary>
  public static UnaryExpression NegateChecked(Expression expression) =>
    MakeUnary(ExpressionType.NegateChecked, expression, expression.Type);

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/> that represents a unary plus operation.
  /// </summary>
  public static UnaryExpression UnaryPlus(Expression expression) =>
    MakeUnary(ExpressionType.UnaryPlus, expression, expression.Type);

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/> that represents a bitwise complement operation.
  /// </summary>
  public static UnaryExpression Not(Expression expression) =>
    MakeUnary(ExpressionType.Not, expression, expression.Type);

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/> that represents a type conversion operation.
  /// </summary>
  public static UnaryExpression Convert(Expression expression, Type type) =>
    MakeUnary(ExpressionType.Convert, expression, type);

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/> that represents a type conversion operation that throws an exception if the target type is overflowed.
  /// </summary>
  public static UnaryExpression ConvertChecked(Expression expression, Type type) =>
    MakeUnary(ExpressionType.ConvertChecked, expression, type);

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/> that represents a reference conversion operation.
  /// </summary>
  public static UnaryExpression TypeAs(Expression expression, Type type) =>
    MakeUnary(ExpressionType.TypeAs, expression, type);

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/> that represents getting the length of an array.
  /// </summary>
  public static UnaryExpression ArrayLength(Expression array) =>
    MakeUnary(ExpressionType.ArrayLength, array, typeof(int));

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/> that represents the incrementing of the expression by 1.
  /// </summary>
  public static UnaryExpression Increment(Expression expression) =>
    MakeUnary(ExpressionType.Increment, expression, expression.Type);

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/> that represents the incrementing of the expression by 1.
  /// </summary>
  public static UnaryExpression Increment(Expression expression, MethodInfo? method) =>
    new(ExpressionType.Increment, expression, expression.Type, method);

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/> that represents the decrementing of the expression by 1.
  /// </summary>
  public static UnaryExpression Decrement(Expression expression) =>
    MakeUnary(ExpressionType.Decrement, expression, expression.Type);

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/> that represents the decrementing of the expression by 1.
  /// </summary>
  public static UnaryExpression Decrement(Expression expression, MethodInfo? method) =>
    new(ExpressionType.Decrement, expression, expression.Type, method);

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/> that represents the incrementing of the expression by 1 (prefix).
  /// </summary>
  public static UnaryExpression PreIncrementAssign(Expression expression) =>
    MakeUnary(ExpressionType.PreIncrementAssign, expression, expression.Type);

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/> that represents the incrementing of the expression by 1 (prefix).
  /// </summary>
  public static UnaryExpression PreIncrementAssign(Expression expression, MethodInfo? method) =>
    new(ExpressionType.PreIncrementAssign, expression, expression.Type, method);

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/> that represents the decrementing of the expression by 1 (prefix).
  /// </summary>
  public static UnaryExpression PreDecrementAssign(Expression expression) =>
    MakeUnary(ExpressionType.PreDecrementAssign, expression, expression.Type);

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/> that represents the decrementing of the expression by 1 (prefix).
  /// </summary>
  public static UnaryExpression PreDecrementAssign(Expression expression, MethodInfo? method) =>
    new(ExpressionType.PreDecrementAssign, expression, expression.Type, method);

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/> that represents the incrementing of the expression by 1 (postfix).
  /// </summary>
  public static UnaryExpression PostIncrementAssign(Expression expression) =>
    MakeUnary(ExpressionType.PostIncrementAssign, expression, expression.Type);

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/> that represents the incrementing of the expression by 1 (postfix).
  /// </summary>
  public static UnaryExpression PostIncrementAssign(Expression expression, MethodInfo? method) =>
    new(ExpressionType.PostIncrementAssign, expression, expression.Type, method);

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/> that represents the decrementing of the expression by 1 (postfix).
  /// </summary>
  public static UnaryExpression PostDecrementAssign(Expression expression) =>
    MakeUnary(ExpressionType.PostDecrementAssign, expression, expression.Type);

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/> that represents the decrementing of the expression by 1 (postfix).
  /// </summary>
  public static UnaryExpression PostDecrementAssign(Expression expression, MethodInfo? method) =>
    new(ExpressionType.PostDecrementAssign, expression, expression.Type, method);

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/> that represents a ones complement operation.
  /// </summary>
  public static UnaryExpression OnesComplement(Expression expression) =>
    MakeUnary(ExpressionType.OnesComplement, expression, expression.Type);

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/> that represents a ones complement operation.
  /// </summary>
  public static UnaryExpression OnesComplement(Expression expression, MethodInfo? method) =>
    new(ExpressionType.OnesComplement, expression, expression.Type, method);

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/> that returns true if the expression evaluates to true.
  /// </summary>
  public static UnaryExpression IsTrue(Expression expression) =>
    MakeUnary(ExpressionType.IsTrue, expression, typeof(bool));

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/> that returns true if the expression evaluates to true.
  /// </summary>
  public static UnaryExpression IsTrue(Expression expression, MethodInfo? method) =>
    new(ExpressionType.IsTrue, expression, typeof(bool), method);

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/> that returns true if the expression evaluates to false.
  /// </summary>
  public static UnaryExpression IsFalse(Expression expression) =>
    MakeUnary(ExpressionType.IsFalse, expression, typeof(bool));

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/> that returns true if the expression evaluates to false.
  /// </summary>
  public static UnaryExpression IsFalse(Expression expression, MethodInfo? method) =>
    new(ExpressionType.IsFalse, expression, typeof(bool), method);

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/> that represents an unbox value type operation.
  /// </summary>
  public static UnaryExpression Unbox(Expression expression, Type type) =>
    MakeUnary(ExpressionType.Unbox, expression, type);

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/>, given an operand, by calling the appropriate factory method.
  /// </summary>
  public static UnaryExpression MakeUnary(ExpressionType unaryType, Expression operand, Type type) {
    if (operand == null)
      throw new ArgumentNullException(nameof(operand));
    if (type == null)
      throw new ArgumentNullException(nameof(type));

    return new(unaryType, operand, type, null);
  }

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/>, given an operand and method, by calling the appropriate factory method.
  /// </summary>
  public static UnaryExpression MakeUnary(ExpressionType unaryType, Expression operand, Type type, MethodInfo? method) {
    if (operand == null)
      throw new ArgumentNullException(nameof(operand));
    if (type == null)
      throw new ArgumentNullException(nameof(type));

    return new(unaryType, operand, type, method);
  }

  #endregion

  #region Lambda Expressions

  /// <summary>
  /// Creates a <see cref="LambdaExpression"/> by first constructing a delegate type.
  /// </summary>
  public static LambdaExpression Lambda(Expression body, params ParameterExpression[] parameters) =>
    Lambda(body, (IEnumerable<ParameterExpression>)parameters);

  /// <summary>
  /// Creates a <see cref="LambdaExpression"/> by first constructing a delegate type.
  /// </summary>
  public static LambdaExpression Lambda(Expression body, IEnumerable<ParameterExpression> parameters) {
    if (body == null)
      throw new ArgumentNullException(nameof(body));

    var paramList = parameters as IList<ParameterExpression> ?? new List<ParameterExpression>(parameters ?? []);
    var delegateType = GetDelegateType(body.Type, paramList);
    return new LambdaExpression(delegateType, body, paramList);
  }

  /// <summary>
  /// Creates an <see cref="Expression{TDelegate}"/> where the delegate type is known at compile time.
  /// </summary>
  public static Expression<TDelegate> Lambda<TDelegate>(Expression body, params ParameterExpression[] parameters) =>
    Lambda<TDelegate>(body, (IEnumerable<ParameterExpression>)parameters);

  /// <summary>
  /// Creates an <see cref="Expression{TDelegate}"/> where the delegate type is known at compile time.
  /// </summary>
  public static Expression<TDelegate> Lambda<TDelegate>(Expression body, IEnumerable<ParameterExpression> parameters) {
    if (body == null)
      throw new ArgumentNullException(nameof(body));

    var paramList = parameters as IList<ParameterExpression> ?? new List<ParameterExpression>(parameters ?? []);
    return new Expression<TDelegate>(body, paramList);
  }

  private static Type GetDelegateType(Type returnType, IList<ParameterExpression> parameters) {
    var paramCount = parameters.Count;

    if (returnType == typeof(void)) {
      return paramCount switch {
        0 => typeof(Action),
        1 => typeof(Action<>).MakeGenericType(parameters[0].Type),
        2 => typeof(Action<,>).MakeGenericType(parameters[0].Type, parameters[1].Type),
        3 => typeof(Action<,,>).MakeGenericType(parameters[0].Type, parameters[1].Type, parameters[2].Type),
        4 => typeof(Action<,,,>).MakeGenericType(parameters[0].Type, parameters[1].Type, parameters[2].Type,
          parameters[3].Type),
        _ => throw new NotSupportedException($"Lambda with {paramCount} parameters is not supported.")
      };
    }

    return paramCount switch {
      0 => typeof(Func<>).MakeGenericType(returnType),
      1 => typeof(Func<,>).MakeGenericType(parameters[0].Type, returnType),
      2 => typeof(Func<,,>).MakeGenericType(parameters[0].Type, parameters[1].Type, returnType),
      3 => typeof(Func<,,,>).MakeGenericType(parameters[0].Type, parameters[1].Type, parameters[2].Type, returnType),
      4 => typeof(Func<,,,,>).MakeGenericType(parameters[0].Type, parameters[1].Type, parameters[2].Type,
        parameters[3].Type, returnType),
      _ => throw new NotSupportedException($"Lambda with {paramCount} parameters is not supported.")
    };
  }

  #endregion

  #region Method Call

  /// <summary>
  /// Creates a <see cref="MethodCallExpression"/> that represents a static method call.
  /// </summary>
  public static MethodCallExpression Call(MethodInfo method, params Expression[] arguments) =>
    Call(null, method, arguments);

  /// <summary>
  /// Creates a <see cref="MethodCallExpression"/> that represents a method call.
  /// </summary>
  public static MethodCallExpression Call(Expression? instance, MethodInfo method, params Expression[] arguments) =>
    Call(instance, method, (IEnumerable<Expression>)arguments);

  /// <summary>
  /// Creates a <see cref="MethodCallExpression"/> that represents a method call.
  /// </summary>
  public static MethodCallExpression Call(Expression? instance, MethodInfo method, IEnumerable<Expression> arguments) {
    if (method == null)
      throw new ArgumentNullException(nameof(method));

    var argList = arguments as IList<Expression> ?? new List<Expression>(arguments ?? []);
    return new(instance, method, argList);
  }

  #endregion

  #region Member Access

  /// <summary>
  /// Creates a <see cref="MemberExpression"/> that represents accessing a property.
  /// </summary>
  public static MemberExpression Property(Expression expression, PropertyInfo property) {
    if (property == null)
      throw new ArgumentNullException(nameof(property));
    return new(expression, property);
  }

  /// <summary>
  /// Creates a <see cref="MemberExpression"/> that represents accessing a property.
  /// </summary>
  public static MemberExpression Property(Expression expression, string propertyName) {
    if (expression == null)
      throw new ArgumentNullException(nameof(expression));
    if (propertyName == null)
      throw new ArgumentNullException(nameof(propertyName));

    var property = expression.Type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
    if (property == null)
      throw new ArgumentException($"Property '{propertyName}' not found on type '{expression.Type}'.", nameof(propertyName));

    return new(expression, property);
  }

  /// <summary>
  /// Creates a <see cref="MemberExpression"/> that represents accessing a field.
  /// </summary>
  public static MemberExpression Field(Expression expression, FieldInfo field) {
    if (field == null)
      throw new ArgumentNullException(nameof(field));
    return new(expression, field);
  }

  /// <summary>
  /// Creates a <see cref="MemberExpression"/> that represents accessing a field.
  /// </summary>
  public static MemberExpression Field(Expression expression, string fieldName) {
    if (expression == null)
      throw new ArgumentNullException(nameof(expression));
    if (fieldName == null)
      throw new ArgumentNullException(nameof(fieldName));

    var field = expression.Type.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
    if (field == null)
      throw new ArgumentException($"Field '{fieldName}' not found on type '{expression.Type}'.", nameof(fieldName));

    return new(expression, field);
  }

  /// <summary>
  /// Creates a <see cref="MemberExpression"/> that represents accessing a property or field.
  /// </summary>
  public static MemberExpression PropertyOrField(Expression expression, string propertyOrFieldName) {
    if (expression == null)
      throw new ArgumentNullException(nameof(expression));
    if (propertyOrFieldName == null)
      throw new ArgumentNullException(nameof(propertyOrFieldName));

    var property = expression.Type.GetProperty(propertyOrFieldName, BindingFlags.Public | BindingFlags.Instance);
    if (property != null)
      return new(expression, property);

    var field = expression.Type.GetField(propertyOrFieldName, BindingFlags.Public | BindingFlags.Instance);
    if (field != null)
      return new(expression, field);

    throw new ArgumentException($"Property or field '{propertyOrFieldName}' not found on type '{expression.Type}'.",
      nameof(propertyOrFieldName));
  }

  #endregion

  #region Conditional

  /// <summary>
  /// Creates a <see cref="ConditionalExpression"/>.
  /// </summary>
  public static ConditionalExpression Condition(Expression test, Expression ifTrue, Expression ifFalse) {
    if (test == null)
      throw new ArgumentNullException(nameof(test));
    if (ifTrue == null)
      throw new ArgumentNullException(nameof(ifTrue));
    if (ifFalse == null)
      throw new ArgumentNullException(nameof(ifFalse));

    return new(test, ifTrue, ifFalse, ifTrue.Type);
  }

  /// <summary>
  /// Creates a <see cref="ConditionalExpression"/> with an explicit type.
  /// </summary>
  public static ConditionalExpression Condition(Expression test, Expression ifTrue, Expression ifFalse, Type type) {
    if (test == null)
      throw new ArgumentNullException(nameof(test));
    if (ifTrue == null)
      throw new ArgumentNullException(nameof(ifTrue));
    if (ifFalse == null)
      throw new ArgumentNullException(nameof(ifFalse));
    if (type == null)
      throw new ArgumentNullException(nameof(type));

    return new(test, ifTrue, ifFalse, type);
  }

  /// <summary>
  /// Creates a <see cref="ConditionalExpression"/>.
  /// </summary>
  public static ConditionalExpression IfThen(Expression test, Expression ifTrue) =>
    Condition(test, ifTrue, Default(typeof(void)));

  /// <summary>
  /// Creates a <see cref="ConditionalExpression"/>.
  /// </summary>
  public static ConditionalExpression IfThenElse(Expression test, Expression ifTrue, Expression ifFalse) =>
    Condition(test, ifTrue, ifFalse);

  #endregion

  #region Default

  /// <summary>
  /// Creates a <see cref="DefaultExpression"/> that has the <see cref="Expression.Type"/> property set to the specified type.
  /// </summary>
  public static DefaultExpression Default(Type type) {
    if (type == null)
      throw new ArgumentNullException(nameof(type));
    return new(type);
  }

  #endregion

  #region New

  /// <summary>
  /// Creates a <see cref="NewExpression"/> that represents calling the specified constructor.
  /// </summary>
  public static NewExpression New(ConstructorInfo constructor) =>
    New(constructor, []);

  /// <summary>
  /// Creates a <see cref="NewExpression"/> that represents calling the specified constructor with the specified arguments.
  /// </summary>
  public static NewExpression New(ConstructorInfo constructor, params Expression[] arguments) =>
    New(constructor, (IEnumerable<Expression>)arguments);

  /// <summary>
  /// Creates a <see cref="NewExpression"/> that represents calling the specified constructor with the specified arguments.
  /// </summary>
  public static NewExpression New(ConstructorInfo constructor, IEnumerable<Expression> arguments) {
    if (constructor == null)
      throw new ArgumentNullException(nameof(constructor));

    var argList = arguments as IList<Expression> ?? new List<Expression>(arguments ?? []);
    return new(constructor, argList);
  }

  /// <summary>
  /// Creates a <see cref="NewExpression"/> that represents calling the parameterless constructor of the specified type.
  /// </summary>
  public static NewExpression New(Type type) {
    if (type == null)
      throw new ArgumentNullException(nameof(type));

    var ctor = type.GetConstructor([]);
    if (ctor == null)
      throw new ArgumentException($"Type '{type}' does not have a parameterless constructor.", nameof(type));

    return new(ctor, []);
  }

  #endregion

  #region Invoke

  /// <summary>
  /// Creates an <see cref="InvocationExpression"/>.
  /// </summary>
  public static InvocationExpression Invoke(Expression expression, params Expression[] arguments) =>
    Invoke(expression, (IEnumerable<Expression>)arguments);

  /// <summary>
  /// Creates an <see cref="InvocationExpression"/>.
  /// </summary>
  public static InvocationExpression Invoke(Expression expression, IEnumerable<Expression> arguments) {
    if (expression == null)
      throw new ArgumentNullException(nameof(expression));

    var argList = arguments as IList<Expression> ?? new List<Expression>(arguments ?? []);
    return new(expression, argList);
  }

  #endregion

  #region Type Binary

  /// <summary>
  /// Creates a <see cref="TypeBinaryExpression"/> for an <c>is</c> operation.
  /// </summary>
  public static TypeBinaryExpression TypeIs(Expression expression, Type type) {
    if (expression == null)
      throw new ArgumentNullException(nameof(expression));
    if (type == null)
      throw new ArgumentNullException(nameof(type));

    return new(expression, type, ExpressionType.TypeIs);
  }

  /// <summary>
  /// Creates a <see cref="TypeBinaryExpression"/> for an exact type comparison.
  /// </summary>
  public static TypeBinaryExpression TypeEqual(Expression expression, Type type) {
    if (expression == null)
      throw new ArgumentNullException(nameof(expression));
    if (type == null)
      throw new ArgumentNullException(nameof(type));

    return new(expression, type, ExpressionType.TypeEqual);
  }

  #endregion

  #region NewArray

  /// <summary>
  /// Creates a <see cref="NewArrayExpression"/> that represents creating an array that has a specified rank.
  /// </summary>
  public static NewArrayExpression NewArrayBounds(Type type, params Expression[] bounds) =>
    NewArrayBounds(type, (IEnumerable<Expression>)bounds);

  /// <summary>
  /// Creates a <see cref="NewArrayExpression"/> that represents creating an array that has a specified rank.
  /// </summary>
  public static NewArrayExpression NewArrayBounds(Type type, IEnumerable<Expression> bounds) {
    if (type == null)
      throw new ArgumentNullException(nameof(type));

    var boundsList = bounds as IList<Expression> ?? new List<Expression>(bounds ?? []);
    return new(ExpressionType.NewArrayBounds, type.MakeArrayType(), boundsList);
  }

  /// <summary>
  /// Creates a <see cref="NewArrayExpression"/> that represents creating a one-dimensional array and initializing it from a list of elements.
  /// </summary>
  public static NewArrayExpression NewArrayInit(Type type, params Expression[] initializers) =>
    NewArrayInit(type, (IEnumerable<Expression>)initializers);

  /// <summary>
  /// Creates a <see cref="NewArrayExpression"/> that represents creating a one-dimensional array and initializing it from a list of elements.
  /// </summary>
  public static NewArrayExpression NewArrayInit(Type type, IEnumerable<Expression> initializers) {
    if (type == null)
      throw new ArgumentNullException(nameof(type));

    var initList = initializers as IList<Expression> ?? new List<Expression>(initializers ?? []);
    return new(ExpressionType.NewArrayInit, type.MakeArrayType(), initList);
  }

  #endregion

  #region Quote

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/> that represents an expression that has a constant value of type <see cref="Expression"/>.
  /// </summary>
  public static UnaryExpression Quote(Expression expression) {
    if (expression == null)
      throw new ArgumentNullException(nameof(expression));

    return new(ExpressionType.Quote, expression, expression.GetType(), null);
  }

  #endregion

  #region Throw

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/> that represents throwing an exception.
  /// </summary>
  public static UnaryExpression Throw(Expression value) =>
    Throw(value, typeof(void));

  /// <summary>
  /// Creates a <see cref="UnaryExpression"/> that represents throwing an exception with a given type.
  /// </summary>
  public static UnaryExpression Throw(Expression? value, Type type) {
    if (type == null)
      throw new ArgumentNullException(nameof(type));

    return new(ExpressionType.Throw, value!, type, null);
  }

  #endregion

  #region Label

  /// <summary>
  /// Creates a <see cref="LabelTarget"/> representing a label with void type and no name.
  /// </summary>
  public static LabelTarget Label() => Label(typeof(void), null);

  /// <summary>
  /// Creates a <see cref="LabelTarget"/> representing a label with void type and the given name.
  /// </summary>
  public static LabelTarget Label(string? name) => Label(typeof(void), name);

  /// <summary>
  /// Creates a <see cref="LabelTarget"/> representing a label with the given type.
  /// </summary>
  public static LabelTarget Label(Type type) => Label(type, null);

  /// <summary>
  /// Creates a <see cref="LabelTarget"/> representing a label with the given type and name.
  /// </summary>
  public static LabelTarget Label(Type type, string? name) {
    if (type == null)
      throw new ArgumentNullException(nameof(type));
    return new(type, name);
  }

  /// <summary>
  /// Creates a <see cref="LabelExpression"/> representing a label with the given default value.
  /// </summary>
  public static LabelExpression Label(LabelTarget target) => Label(target, null);

  /// <summary>
  /// Creates a <see cref="LabelExpression"/> representing a label with the given default value.
  /// </summary>
  public static LabelExpression Label(LabelTarget target, Expression? defaultValue) {
    if (target == null)
      throw new ArgumentNullException(nameof(target));
    return new(target, defaultValue);
  }

  #endregion

  #region Goto

  /// <summary>
  /// Creates a <see cref="GotoExpression"/> representing a "go to" statement.
  /// </summary>
  public static GotoExpression Goto(LabelTarget target) =>
    MakeGoto(GotoExpressionKind.Goto, target, null, typeof(void));

  /// <summary>
  /// Creates a <see cref="GotoExpression"/> representing a "go to" statement with a value.
  /// </summary>
  public static GotoExpression Goto(LabelTarget target, Expression? value) =>
    MakeGoto(GotoExpressionKind.Goto, target, value, typeof(void));

  /// <summary>
  /// Creates a <see cref="GotoExpression"/> representing a "go to" statement with the specified type.
  /// </summary>
  public static GotoExpression Goto(LabelTarget target, Type type) =>
    MakeGoto(GotoExpressionKind.Goto, target, null, type);

  /// <summary>
  /// Creates a <see cref="GotoExpression"/> representing a "go to" statement with a value and the specified type.
  /// </summary>
  public static GotoExpression Goto(LabelTarget target, Expression? value, Type type) =>
    MakeGoto(GotoExpressionKind.Goto, target, value, type);

  /// <summary>
  /// Creates a <see cref="GotoExpression"/> representing a break statement.
  /// </summary>
  public static GotoExpression Break(LabelTarget target) =>
    MakeGoto(GotoExpressionKind.Break, target, null, typeof(void));

  /// <summary>
  /// Creates a <see cref="GotoExpression"/> representing a break statement with a value.
  /// </summary>
  public static GotoExpression Break(LabelTarget target, Expression? value) =>
    MakeGoto(GotoExpressionKind.Break, target, value, typeof(void));

  /// <summary>
  /// Creates a <see cref="GotoExpression"/> representing a break statement with the specified type.
  /// </summary>
  public static GotoExpression Break(LabelTarget target, Type type) =>
    MakeGoto(GotoExpressionKind.Break, target, null, type);

  /// <summary>
  /// Creates a <see cref="GotoExpression"/> representing a break statement with a value and the specified type.
  /// </summary>
  public static GotoExpression Break(LabelTarget target, Expression? value, Type type) =>
    MakeGoto(GotoExpressionKind.Break, target, value, type);

  /// <summary>
  /// Creates a <see cref="GotoExpression"/> representing a continue statement.
  /// </summary>
  public static GotoExpression Continue(LabelTarget target) =>
    MakeGoto(GotoExpressionKind.Continue, target, null, typeof(void));

  /// <summary>
  /// Creates a <see cref="GotoExpression"/> representing a continue statement with the specified type.
  /// </summary>
  public static GotoExpression Continue(LabelTarget target, Type type) =>
    MakeGoto(GotoExpressionKind.Continue, target, null, type);

  /// <summary>
  /// Creates a <see cref="GotoExpression"/> representing a return statement.
  /// </summary>
  public static GotoExpression Return(LabelTarget target) =>
    MakeGoto(GotoExpressionKind.Return, target, null, typeof(void));

  /// <summary>
  /// Creates a <see cref="GotoExpression"/> representing a return statement with a value.
  /// </summary>
  public static GotoExpression Return(LabelTarget target, Expression? value) =>
    MakeGoto(GotoExpressionKind.Return, target, value, typeof(void));

  /// <summary>
  /// Creates a <see cref="GotoExpression"/> representing a return statement with the specified type.
  /// </summary>
  public static GotoExpression Return(LabelTarget target, Type type) =>
    MakeGoto(GotoExpressionKind.Return, target, null, type);

  /// <summary>
  /// Creates a <see cref="GotoExpression"/> representing a return statement with a value and the specified type.
  /// </summary>
  public static GotoExpression Return(LabelTarget target, Expression? value, Type type) =>
    MakeGoto(GotoExpressionKind.Return, target, value, type);

  /// <summary>
  /// Creates a <see cref="GotoExpression"/> representing a jump of the specified <see cref="GotoExpressionKind"/>.
  /// </summary>
  public static GotoExpression MakeGoto(GotoExpressionKind kind, LabelTarget target, Expression? value, Type type) {
    if (target == null)
      throw new ArgumentNullException(nameof(target));
    if (type == null)
      throw new ArgumentNullException(nameof(type));
    return new(kind, target, value, type);
  }

  #endregion

  #region Block

  /// <summary>
  /// Creates a <see cref="BlockExpression"/> that contains the given expressions and has no variables.
  /// </summary>
  public static BlockExpression Block(params Expression[] expressions) =>
    Block((IEnumerable<Expression>)expressions);

  /// <summary>
  /// Creates a <see cref="BlockExpression"/> that contains the given expressions and has no variables.
  /// </summary>
  public static BlockExpression Block(IEnumerable<Expression> expressions) {
    var exprList = expressions as IList<Expression> ?? new List<Expression>(expressions ?? []);
    var resultType = exprList.Count > 0 ? exprList[exprList.Count - 1].Type : typeof(void);
    return new([], exprList, resultType);
  }

  /// <summary>
  /// Creates a <see cref="BlockExpression"/> that contains the given expressions, has no variables, and has the given type.
  /// </summary>
  public static BlockExpression Block(Type type, params Expression[] expressions) =>
    Block(type, (IEnumerable<Expression>)expressions);

  /// <summary>
  /// Creates a <see cref="BlockExpression"/> that contains the given expressions, has no variables, and has the given type.
  /// </summary>
  public static BlockExpression Block(Type type, IEnumerable<Expression> expressions) {
    if (type == null)
      throw new ArgumentNullException(nameof(type));
    var exprList = expressions as IList<Expression> ?? new List<Expression>(expressions ?? []);
    return new([], exprList, type);
  }

  /// <summary>
  /// Creates a <see cref="BlockExpression"/> that contains the given variables and expressions.
  /// </summary>
  public static BlockExpression Block(IEnumerable<ParameterExpression> variables, params Expression[] expressions) =>
    Block(variables, (IEnumerable<Expression>)expressions);

  /// <summary>
  /// Creates a <see cref="BlockExpression"/> that contains the given variables and expressions.
  /// </summary>
  public static BlockExpression Block(IEnumerable<ParameterExpression> variables, IEnumerable<Expression> expressions) {
    var varList = variables as IList<ParameterExpression> ?? new List<ParameterExpression>(variables ?? []);
    var exprList = expressions as IList<Expression> ?? new List<Expression>(expressions ?? []);
    var resultType = exprList.Count > 0 ? exprList[exprList.Count - 1].Type : typeof(void);
    return new(varList, exprList, resultType);
  }

  /// <summary>
  /// Creates a <see cref="BlockExpression"/> that contains the given variables and expressions with the given type.
  /// </summary>
  public static BlockExpression Block(Type type, IEnumerable<ParameterExpression> variables, params Expression[] expressions) =>
    Block(type, variables, (IEnumerable<Expression>)expressions);

  /// <summary>
  /// Creates a <see cref="BlockExpression"/> that contains the given variables and expressions with the given type.
  /// </summary>
  public static BlockExpression Block(Type type, IEnumerable<ParameterExpression> variables, IEnumerable<Expression> expressions) {
    if (type == null)
      throw new ArgumentNullException(nameof(type));
    var varList = variables as IList<ParameterExpression> ?? new List<ParameterExpression>(variables ?? []);
    var exprList = expressions as IList<Expression> ?? new List<Expression>(expressions ?? []);
    return new(varList, exprList, type);
  }

  /// <summary>
  /// Creates a <see cref="DefaultExpression"/> that has the <see cref="Type"/> property set to <see cref="void"/>.
  /// </summary>
  public static DefaultExpression Empty() => Default(typeof(void));

  #endregion

  #region Loop

  /// <summary>
  /// Creates a <see cref="LoopExpression"/> with the given body.
  /// </summary>
  public static LoopExpression Loop(Expression body) =>
    Loop(body, null, null);

  /// <summary>
  /// Creates a <see cref="LoopExpression"/> with the given body and break target.
  /// </summary>
  public static LoopExpression Loop(Expression body, LabelTarget? @break) =>
    Loop(body, @break, null);

  /// <summary>
  /// Creates a <see cref="LoopExpression"/> with the given body, break target, and continue target.
  /// </summary>
  public static LoopExpression Loop(Expression body, LabelTarget? @break, LabelTarget? @continue) {
    if (body == null)
      throw new ArgumentNullException(nameof(body));
    return new(body, @break, @continue);
  }

  #endregion

  #region Try/Catch

  /// <summary>
  /// Creates a <see cref="CatchBlock"/> representing a catch statement.
  /// </summary>
  public static CatchBlock Catch(Type type, Expression body) =>
    MakeCatchBlock(type, null, body, null);

  /// <summary>
  /// Creates a <see cref="CatchBlock"/> representing a catch statement with an exception variable.
  /// </summary>
  public static CatchBlock Catch(ParameterExpression variable, Expression body) =>
    MakeCatchBlock(variable?.Type ?? throw new ArgumentNullException(nameof(variable)), variable, body, null);

  /// <summary>
  /// Creates a <see cref="CatchBlock"/> representing a catch statement with a filter.
  /// </summary>
  public static CatchBlock Catch(Type type, Expression body, Expression? filter) =>
    MakeCatchBlock(type, null, body, filter);

  /// <summary>
  /// Creates a <see cref="CatchBlock"/> representing a catch statement with an exception variable and filter.
  /// </summary>
  public static CatchBlock Catch(ParameterExpression variable, Expression body, Expression? filter) =>
    MakeCatchBlock(variable?.Type ?? throw new ArgumentNullException(nameof(variable)), variable, body, filter);

  /// <summary>
  /// Creates a <see cref="CatchBlock"/> representing a catch statement with the specified elements.
  /// </summary>
  public static CatchBlock MakeCatchBlock(Type type, ParameterExpression? variable, Expression body, Expression? filter) {
    if (type == null)
      throw new ArgumentNullException(nameof(type));
    if (body == null)
      throw new ArgumentNullException(nameof(body));
    return new(type, variable, body, filter);
  }

  /// <summary>
  /// Creates a <see cref="TryExpression"/> representing a try block with a finally block but no catch statements.
  /// </summary>
  public static TryExpression TryFinally(Expression body, Expression? @finally) =>
    MakeTry(null, body, @finally, null, null);

  /// <summary>
  /// Creates a <see cref="TryExpression"/> representing a try block with any number of catch statements.
  /// </summary>
  public static TryExpression TryCatch(Expression body, params CatchBlock[] handlers) =>
    MakeTry(null, body, null, null, handlers);

  /// <summary>
  /// Creates a <see cref="TryExpression"/> representing a try block with any number of catch statements and a finally block.
  /// </summary>
  public static TryExpression TryCatchFinally(Expression body, Expression? @finally, params CatchBlock[] handlers) =>
    MakeTry(null, body, @finally, null, handlers);

  /// <summary>
  /// Creates a <see cref="TryExpression"/> representing a try block with a fault block but no catch statements.
  /// </summary>
  public static TryExpression TryFault(Expression body, Expression? fault) =>
    MakeTry(null, body, null, fault, null);

  /// <summary>
  /// Creates a <see cref="TryExpression"/> representing a try block with the specified elements.
  /// </summary>
  public static TryExpression MakeTry(Type? type, Expression body, Expression? @finally, Expression? fault, IEnumerable<CatchBlock>? handlers) {
    if (body == null)
      throw new ArgumentNullException(nameof(body));

    var handlerList = handlers as IList<CatchBlock> ?? new List<CatchBlock>(handlers ?? []);
    var resultType = type ?? body.Type;

    return new(resultType, body, @finally, fault, handlerList);
  }

  #endregion

  #region Switch

  /// <summary>
  /// Creates a <see cref="SwitchCase"/> for use in a <see cref="SwitchExpression"/>.
  /// </summary>
  public static SwitchCase SwitchCase(Expression body, params Expression[] testValues) =>
    SwitchCase(body, (IEnumerable<Expression>)testValues);

  /// <summary>
  /// Creates a <see cref="SwitchCase"/> for use in a <see cref="SwitchExpression"/>.
  /// </summary>
  public static SwitchCase SwitchCase(Expression body, IEnumerable<Expression> testValues) {
    if (body == null)
      throw new ArgumentNullException(nameof(body));
    var testList = testValues as IList<Expression> ?? new List<Expression>(testValues ?? []);
    return new(body, testList);
  }

  /// <summary>
  /// Creates a <see cref="SwitchExpression"/> that represents a switch statement without a default case.
  /// </summary>
  public static SwitchExpression Switch(Expression switchValue, params SwitchCase[] cases) =>
    Switch(switchValue, null, null, cases);

  /// <summary>
  /// Creates a <see cref="SwitchExpression"/> that represents a switch statement with a default case.
  /// </summary>
  public static SwitchExpression Switch(Expression switchValue, Expression? defaultBody, params SwitchCase[] cases) =>
    Switch(switchValue, defaultBody, null, cases);

  /// <summary>
  /// Creates a <see cref="SwitchExpression"/> that represents a switch statement with a default case and comparison method.
  /// </summary>
  public static SwitchExpression Switch(Expression switchValue, Expression? defaultBody, MethodInfo? comparison, params SwitchCase[] cases) =>
    Switch(switchValue, defaultBody, comparison, (IEnumerable<SwitchCase>)cases);

  /// <summary>
  /// Creates a <see cref="SwitchExpression"/> that represents a switch statement.
  /// </summary>
  public static SwitchExpression Switch(Expression switchValue, Expression? defaultBody, MethodInfo? comparison, IEnumerable<SwitchCase> cases) {
    if (switchValue == null)
      throw new ArgumentNullException(nameof(switchValue));
    var caseList = cases as IList<SwitchCase> ?? new List<SwitchCase>(cases ?? []);
    var resultType = caseList.Count > 0 ? caseList[0].Body.Type : typeof(void);
    if (defaultBody != null)
      resultType = defaultBody.Type;
    return new(resultType, switchValue, defaultBody, comparison, caseList);
  }

  /// <summary>
  /// Creates a <see cref="SwitchExpression"/> that represents a switch statement with the specified type.
  /// </summary>
  public static SwitchExpression Switch(Type type, Expression switchValue, Expression? defaultBody, MethodInfo? comparison, params SwitchCase[] cases) =>
    Switch(type, switchValue, defaultBody, comparison, (IEnumerable<SwitchCase>)cases);

  /// <summary>
  /// Creates a <see cref="SwitchExpression"/> that represents a switch statement with the specified type.
  /// </summary>
  public static SwitchExpression Switch(Type type, Expression switchValue, Expression? defaultBody, MethodInfo? comparison, IEnumerable<SwitchCase> cases) {
    if (type == null)
      throw new ArgumentNullException(nameof(type));
    if (switchValue == null)
      throw new ArgumentNullException(nameof(switchValue));
    var caseList = cases as IList<SwitchCase> ?? new List<SwitchCase>(cases ?? []);
    return new(type, switchValue, defaultBody, comparison, caseList);
  }

  #endregion

  #region Index

  /// <summary>
  /// Creates an <see cref="IndexExpression"/> that represents accessing an array element.
  /// </summary>
  public static IndexExpression ArrayAccess(Expression array, params Expression[] indexes) =>
    ArrayAccess(array, (IEnumerable<Expression>)indexes);

  /// <summary>
  /// Creates an <see cref="IndexExpression"/> that represents accessing an array element.
  /// </summary>
  public static IndexExpression ArrayAccess(Expression array, IEnumerable<Expression> indexes) {
    if (array == null)
      throw new ArgumentNullException(nameof(array));
    var indexList = indexes as IList<Expression> ?? new List<Expression>(indexes ?? []);
    return new(array, null, indexList);
  }

  /// <summary>
  /// Creates an <see cref="IndexExpression"/> representing the access to an indexed property.
  /// </summary>
  public static IndexExpression Property(Expression instance, PropertyInfo indexer, params Expression[] arguments) =>
    Property(instance, indexer, (IEnumerable<Expression>)arguments);

  /// <summary>
  /// Creates an <see cref="IndexExpression"/> representing the access to an indexed property.
  /// </summary>
  public static IndexExpression Property(Expression instance, PropertyInfo indexer, IEnumerable<Expression> arguments) {
    if (indexer == null)
      throw new ArgumentNullException(nameof(indexer));
    var argList = arguments as IList<Expression> ?? new List<Expression>(arguments ?? []);
    return new(instance, indexer, argList);
  }

  /// <summary>
  /// Creates an <see cref="IndexExpression"/> representing the access to an indexed property by name.
  /// </summary>
  public static IndexExpression Property(Expression instance, string propertyName, params Expression[] arguments) {
    if (instance == null)
      throw new ArgumentNullException(nameof(instance));
    if (propertyName == null)
      throw new ArgumentNullException(nameof(propertyName));
    var indexer = instance.Type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    if (indexer == null)
      throw new ArgumentException($"Property '{propertyName}' not found on type '{instance.Type}'", nameof(propertyName));
    var argList = arguments as IList<Expression> ?? new List<Expression>(arguments ?? []);
    return new(instance, indexer, argList);
  }

  /// <summary>
  /// Creates an <see cref="IndexExpression"/> to access a multidimensional array.
  /// </summary>
  public static MethodCallExpression ArrayIndex(Expression array, params Expression[] indexes) =>
    ArrayIndex(array, (IEnumerable<Expression>)indexes);

  /// <summary>
  /// Creates an <see cref="IndexExpression"/> to access a multidimensional array.
  /// </summary>
  public static MethodCallExpression ArrayIndex(Expression array, IEnumerable<Expression> indexes) {
    if (array == null)
      throw new ArgumentNullException(nameof(array));
    var indexList = indexes as IList<Expression> ?? new List<Expression>(indexes ?? []);
    var getMethod = array.Type.GetMethod("Get");
    if (getMethod == null)
      throw new ArgumentException("Array type must have a Get method", nameof(array));
    return Call(array, getMethod, indexList);
  }

  /// <summary>
  /// Creates an <see cref="IndexExpression"/> to access an array or indexed property.
  /// </summary>
  public static IndexExpression MakeIndex(Expression instance, PropertyInfo? indexer, IEnumerable<Expression>? arguments) {
    var argList = arguments as IList<Expression> ?? new List<Expression>(arguments ?? []);
    return new(instance, indexer, argList);
  }

  #endregion

  #region ElementInit

  /// <summary>
  /// Creates an <see cref="ElementInit"/>, given an <see cref="System.Reflection.MethodInfo"/> as the add method.
  /// </summary>
  public static ElementInit ElementInit(MethodInfo addMethod, params Expression[] arguments) =>
    ElementInit(addMethod, (IEnumerable<Expression>)arguments);

  /// <summary>
  /// Creates an <see cref="ElementInit"/>, given an <see cref="System.Reflection.MethodInfo"/> as the add method.
  /// </summary>
  public static ElementInit ElementInit(MethodInfo addMethod, IEnumerable<Expression> arguments) {
    if (addMethod == null)
      throw new ArgumentNullException(nameof(addMethod));
    var argList = arguments as IList<Expression> ?? new List<Expression>(arguments ?? []);
    return new(addMethod, argList);
  }

  #endregion

  #region ListInit

  /// <summary>
  /// Creates a <see cref="ListInitExpression"/> that uses a method named "Add" to add elements to a collection.
  /// </summary>
  public static ListInitExpression ListInit(NewExpression newExpression, params Expression[] initializers) {
    if (newExpression == null)
      throw new ArgumentNullException(nameof(newExpression));
    var addMethod = newExpression.Type.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
    if (addMethod == null)
      throw new InvalidOperationException($"Type '{newExpression.Type}' does not have an 'Add' method.");
    var elementInits = new List<ElementInit>();
    foreach (var expr in initializers)
      elementInits.Add(new ElementInit(addMethod, new[] { expr }));
    return new(newExpression, elementInits);
  }

  /// <summary>
  /// Creates a <see cref="ListInitExpression"/> that uses a specified method to add elements to a collection.
  /// </summary>
  public static ListInitExpression ListInit(NewExpression newExpression, MethodInfo addMethod, params Expression[] initializers) =>
    ListInit(newExpression, addMethod, (IEnumerable<Expression>)initializers);

  /// <summary>
  /// Creates a <see cref="ListInitExpression"/> that uses a specified method to add elements to a collection.
  /// </summary>
  public static ListInitExpression ListInit(NewExpression newExpression, MethodInfo addMethod, IEnumerable<Expression> initializers) {
    if (newExpression == null)
      throw new ArgumentNullException(nameof(newExpression));
    if (addMethod == null)
      throw new ArgumentNullException(nameof(addMethod));
    var elementInits = new List<ElementInit>();
    foreach (var expr in initializers)
      elementInits.Add(new ElementInit(addMethod, new[] { expr }));
    return new(newExpression, elementInits);
  }

  /// <summary>
  /// Creates a <see cref="ListInitExpression"/> that uses specified <see cref="ElementInit"/> objects to initialize a collection.
  /// </summary>
  public static ListInitExpression ListInit(NewExpression newExpression, params ElementInit[] initializers) =>
    ListInit(newExpression, (IEnumerable<ElementInit>)initializers);

  /// <summary>
  /// Creates a <see cref="ListInitExpression"/> that uses specified <see cref="ElementInit"/> objects to initialize a collection.
  /// </summary>
  public static ListInitExpression ListInit(NewExpression newExpression, IEnumerable<ElementInit> initializers) {
    if (newExpression == null)
      throw new ArgumentNullException(nameof(newExpression));
    var initList = initializers as IList<ElementInit> ?? new List<ElementInit>(initializers ?? []);
    return new(newExpression, initList);
  }

  #endregion

  #region MemberInit

  /// <summary>
  /// Creates a <see cref="MemberInitExpression"/>.
  /// </summary>
  public static MemberInitExpression MemberInit(NewExpression newExpression, params MemberBinding[] bindings) =>
    MemberInit(newExpression, (IEnumerable<MemberBinding>)bindings);

  /// <summary>
  /// Creates a <see cref="MemberInitExpression"/>.
  /// </summary>
  public static MemberInitExpression MemberInit(NewExpression newExpression, IEnumerable<MemberBinding> bindings) {
    if (newExpression == null)
      throw new ArgumentNullException(nameof(newExpression));
    var bindingList = bindings as IList<MemberBinding> ?? new List<MemberBinding>(bindings ?? []);
    return new(newExpression, bindingList);
  }

  /// <summary>
  /// Creates a <see cref="MemberAssignment"/> binding.
  /// </summary>
  public static MemberAssignment Bind(MemberInfo member, Expression expression) {
    if (member == null)
      throw new ArgumentNullException(nameof(member));
    if (expression == null)
      throw new ArgumentNullException(nameof(expression));
    return new(member, expression);
  }

  /// <summary>
  /// Creates a <see cref="MemberAssignment"/> binding using a property accessor method.
  /// </summary>
  public static MemberAssignment Bind(MethodInfo propertyAccessor, Expression expression) {
    if (propertyAccessor == null)
      throw new ArgumentNullException(nameof(propertyAccessor));
    if (expression == null)
      throw new ArgumentNullException(nameof(expression));

    // Try to find property for getter/setter
    var declaringType = propertyAccessor.DeclaringType;
    if (declaringType != null) {
      foreach (var prop in declaringType.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) {
        if (prop.GetGetMethod(true) == propertyAccessor || prop.GetSetMethod(true) == propertyAccessor)
          return new(prop, expression);
      }
    }
    throw new ArgumentException("The method is not a property accessor.", nameof(propertyAccessor));
  }

  /// <summary>
  /// Creates a <see cref="MemberMemberBinding"/> that represents recursive initialization of members of a member.
  /// </summary>
  public static MemberMemberBinding MemberBind(MemberInfo member, params MemberBinding[] bindings) =>
    MemberBind(member, (IEnumerable<MemberBinding>)bindings);

  /// <summary>
  /// Creates a <see cref="MemberMemberBinding"/> that represents recursive initialization of members of a member.
  /// </summary>
  public static MemberMemberBinding MemberBind(MemberInfo member, IEnumerable<MemberBinding> bindings) {
    if (member == null)
      throw new ArgumentNullException(nameof(member));
    var bindingList = bindings as IList<MemberBinding> ?? new List<MemberBinding>(bindings ?? []);
    return new(member, bindingList);
  }

  /// <summary>
  /// Creates a <see cref="MemberMemberBinding"/> using a property accessor method.
  /// </summary>
  public static MemberMemberBinding MemberBind(MethodInfo propertyAccessor, params MemberBinding[] bindings) =>
    MemberBind(propertyAccessor, (IEnumerable<MemberBinding>)bindings);

  /// <summary>
  /// Creates a <see cref="MemberMemberBinding"/> using a property accessor method.
  /// </summary>
  public static MemberMemberBinding MemberBind(MethodInfo propertyAccessor, IEnumerable<MemberBinding> bindings) {
    if (propertyAccessor == null)
      throw new ArgumentNullException(nameof(propertyAccessor));

    // Try to find property for getter/setter
    var declaringType = propertyAccessor.DeclaringType;
    if (declaringType != null) {
      foreach (var prop in declaringType.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) {
        if (prop.GetGetMethod(true) == propertyAccessor || prop.GetSetMethod(true) == propertyAccessor)
          return MemberBind(prop, bindings);
      }
    }
    throw new ArgumentException("The method is not a property accessor.", nameof(propertyAccessor));
  }

  /// <summary>
  /// Creates a <see cref="MemberListBinding"/> that represents initializing a member of type collection.
  /// </summary>
  public static MemberListBinding ListBind(MemberInfo member, params ElementInit[] initializers) =>
    ListBind(member, (IEnumerable<ElementInit>)initializers);

  /// <summary>
  /// Creates a <see cref="MemberListBinding"/> that represents initializing a member of type collection.
  /// </summary>
  public static MemberListBinding ListBind(MemberInfo member, IEnumerable<ElementInit> initializers) {
    if (member == null)
      throw new ArgumentNullException(nameof(member));
    var initList = initializers as IList<ElementInit> ?? new List<ElementInit>(initializers ?? []);
    return new(member, initList);
  }

  /// <summary>
  /// Creates a <see cref="MemberListBinding"/> using a property accessor method.
  /// </summary>
  public static MemberListBinding ListBind(MethodInfo propertyAccessor, params ElementInit[] initializers) =>
    ListBind(propertyAccessor, (IEnumerable<ElementInit>)initializers);

  /// <summary>
  /// Creates a <see cref="MemberListBinding"/> using a property accessor method.
  /// </summary>
  public static MemberListBinding ListBind(MethodInfo propertyAccessor, IEnumerable<ElementInit> initializers) {
    if (propertyAccessor == null)
      throw new ArgumentNullException(nameof(propertyAccessor));

    // Try to find property for getter/setter
    var declaringType = propertyAccessor.DeclaringType;
    if (declaringType != null) {
      foreach (var prop in declaringType.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) {
        if (prop.GetGetMethod(true) == propertyAccessor || prop.GetSetMethod(true) == propertyAccessor)
          return ListBind(prop, initializers);
      }
    }
    throw new ArgumentException("The method is not a property accessor.", nameof(propertyAccessor));
  }

  #endregion

  #endregion

  /// <summary>
  /// Returns a textual representation of the <see cref="Expression"/>.
  /// </summary>
  public override string ToString() => $"{this.NodeType}";

}

#endif
