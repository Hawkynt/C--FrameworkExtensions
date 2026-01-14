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

// Provides Expression factory methods that don't exist in net35 BCL
// On net20 we have our full Expression polyfill, on net40+ the BCL has everything
#if SUPPORTS_LINQ && !SUPPORTS_DYNAMIC

using System.Linq.Expressions;
using System.Reflection;

namespace System.Dynamic;

/// <summary>
/// Provides Expression factory methods for net35 that don't exist in the BCL.
/// These create minimal stub expressions that support basic Dynamic functionality.
/// </summary>
internal static class Expr {

  // LabelTarget construction helper - uses the LabelTarget from our Expressions polyfill
  private static readonly System.Reflection.ConstructorInfo _labelTargetCtor =
    typeof(System.Linq.Expressions.LabelTarget).GetConstructor(
      System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
      null,
      [typeof(Type), typeof(string)],
      null
    );

  /// <summary>
  /// Creates a LabelTarget for control flow.
  /// </summary>
  public static System.Linq.Expressions.LabelTarget Label(Type type, string name) =>
    (System.Linq.Expressions.LabelTarget)_labelTargetCtor.Invoke([type, name]);

  /// <summary>
  /// Creates a LabelTarget for void control flow.
  /// </summary>
  public static System.Linq.Expressions.LabelTarget Label(string name) =>
    (System.Linq.Expressions.LabelTarget)_labelTargetCtor.Invoke([typeof(void), name]);

  /// <summary>
  /// Creates a LabelTarget for void control flow.
  /// </summary>
  public static System.Linq.Expressions.LabelTarget Label() =>
    (System.Linq.Expressions.LabelTarget)_labelTargetCtor.Invoke([typeof(void), null]);

  /// <summary>
  /// Creates a DefaultExpression that represents the default value of a type.
  /// </summary>
  public static DefaultExpression Default(Type type) => new(type);

  /// <summary>
  /// Creates a GotoExpression for control flow.
  /// </summary>
  public static GotoExpression Goto(System.Linq.Expressions.LabelTarget target) => new(GotoExpressionKind.Goto, target, null!, typeof(void));

  /// <summary>
  /// Creates a GotoExpression with a value.
  /// </summary>
  public static GotoExpression Goto(System.Linq.Expressions.LabelTarget target, Expression value) => new(GotoExpressionKind.Goto, target, value, typeof(void));

  /// <summary>
  /// Creates a GotoExpression with a value and type.
  /// </summary>
  public static GotoExpression Goto(System.Linq.Expressions.LabelTarget target, Expression value, Type type) => new(GotoExpressionKind.Goto, target, value, type);

  /// <summary>
  /// Creates a LabelExpression.
  /// </summary>
  public static LabelExpression Label(System.Linq.Expressions.LabelTarget target) => new(target, null!);

  /// <summary>
  /// Creates a LabelExpression with a default value.
  /// </summary>
  public static LabelExpression Label(System.Linq.Expressions.LabelTarget target, Expression defaultValue) => new(target, defaultValue);

  /// <summary>
  /// Creates a BlockExpression with the given expressions.
  /// </summary>
  public static BlockExpression Block(params Expression[] expressions) => new(typeof(void), Array.Empty<ParameterExpression>(), expressions);

  /// <summary>
  /// Creates a BlockExpression with the given type and expressions.
  /// </summary>
  public static BlockExpression Block(Type type, params Expression[] expressions) => new(type, Array.Empty<ParameterExpression>(), expressions);

  /// <summary>
  /// Creates a BlockExpression with variables and expressions.
  /// </summary>
  public static BlockExpression Block(ParameterExpression[] variables, params Expression[] expressions) => new(typeof(void), variables, expressions);

  /// <summary>
  /// Creates a ThrowExpression.
  /// </summary>
  public static UnaryExpression Throw(Expression value) => Expression.MakeUnary(Utilities.ExpressionType.Throw, value, typeof(void), null);

  /// <summary>
  /// Creates a ThrowExpression with a type.
  /// </summary>
  public static UnaryExpression Throw(Expression value, Type type) => Expression.MakeUnary(Utilities.ExpressionType.Throw, value, type, null);

  /// <summary>
  /// Creates an assignment expression.
  /// </summary>
  public static BinaryExpression Assign(Expression left, Expression right) => Expression.MakeBinary(Utilities.ExpressionType.Assign, left, right);

  /// <summary>
  /// Creates a type equality test expression.
  /// </summary>
  public static TypeBinaryExpression TypeEqual(Expression expression, Type type) => new(expression, type, true);

  /// <summary>
  /// Creates a property access expression with an object, property, and arguments (for indexed properties).
  /// </summary>
  public static Expression Property(Expression instance, PropertyInfo property, params Expression[] arguments) {
    // For non-indexed properties, just use regular Property
    if (arguments == null || arguments.Length == 0)
      return Expression.Property(instance, property);

    // For indexed properties, we need to call the get method
    var getMethod = property.GetGetMethod(true);
    if (getMethod != null)
      return Expression.Call(instance, getMethod, arguments);

    throw new ArgumentException("Property must have a getter.", nameof(property));
  }

  /// <summary>
  /// Creates a conditional expression with an explicit result type.
  /// </summary>
  /// <remarks>
  /// The BCL's Expression.Condition in net35 only takes 3 arguments.
  /// This helper supports the 4-argument signature needed for void-typed conditionals.
  /// </remarks>
  public static ConditionExpression Condition(Expression test, Expression ifTrue, Expression ifFalse, Type type) =>
    new(test, ifTrue, ifFalse, type);

  /// <summary>
  /// Creates a conditional expression.
  /// </summary>
  public static ConditionExpression Condition(Expression test, Expression ifTrue, Expression ifFalse) =>
    new(test, ifTrue, ifFalse, ifTrue.Type);
}

/// <summary>
/// Represents a label in the expression tree for control flow.
/// </summary>
internal sealed class LabelExpression : Expression {
  internal LabelExpression(System.Linq.Expressions.LabelTarget target, Expression defaultValue)
    : base(Utilities.ExpressionType.Label, target?.Type ?? typeof(void)) {
    this.Target = target ?? throw new ArgumentNullException(nameof(target));
    this.DefaultValue = defaultValue;
  }

  public System.Linq.Expressions.LabelTarget Target { get; }
  public Expression DefaultValue { get; }
}

/// <summary>
/// Represents a goto/break/continue/return expression.
/// </summary>
internal sealed class GotoExpression : Expression {
  internal GotoExpression(GotoExpressionKind kind, System.Linq.Expressions.LabelTarget target, Expression value, Type type)
    : base(Utilities.ExpressionType.Goto, type ?? typeof(void)) {
    this.Kind = kind;
    this.Target = target ?? throw new ArgumentNullException(nameof(target));
    this.Value = value;
  }

  public GotoExpressionKind Kind { get; }
  public System.Linq.Expressions.LabelTarget Target { get; }
  public Expression Value { get; }
}

/// <summary>
/// The kind of goto expression.
/// </summary>
internal enum GotoExpressionKind {
  Goto = 0,
  Return = 1,
  Break = 2,
  Continue = 3
}

/// <summary>
/// Represents the default value of a type.
/// </summary>
internal sealed class DefaultExpression : Expression {
  internal DefaultExpression(Type type)
    : base(Utilities.ExpressionType.Default, type ?? throw new ArgumentNullException(nameof(type))) {
  }
}

/// <summary>
/// Represents a block of expressions.
/// </summary>
internal sealed class BlockExpression : Expression {
  internal BlockExpression(Type type, ParameterExpression[] variables, Expression[] expressions)
    : base(Utilities.ExpressionType.Block, type ?? typeof(void)) {
    this.Variables = variables ?? Array.Empty<ParameterExpression>();
    this.Expressions = expressions ?? Array.Empty<Expression>();
  }

  public ParameterExpression[] Variables { get; }
  public Expression[] Expressions { get; }
  public Expression? Result => this.Expressions.Length > 0 ? this.Expressions[this.Expressions.Length - 1] : null;
}

/// <summary>
/// Represents a TypeEqual expression (checks exact type equality, not inheritance).
/// </summary>
internal sealed class TypeBinaryExpression : Expression {
  internal TypeBinaryExpression(Expression expression, Type typeOperand, bool isTypeEqual)
    : base(isTypeEqual ? Utilities.ExpressionType.TypeEqual : System.Linq.Expressions.ExpressionType.TypeIs, typeof(bool)) {
    this.Expression = expression ?? throw new ArgumentNullException(nameof(expression));
    this.TypeOperand = typeOperand ?? throw new ArgumentNullException(nameof(typeOperand));
  }

  public Expression Expression { get; }
  public Type TypeOperand { get; }
}

/// <summary>
/// Represents a conditional expression (ternary operator) with explicit result type.
/// </summary>
/// <remarks>
/// This is needed for net35 where the BCL's Expression.Condition only supports 3 arguments
/// and cannot handle void-typed branches properly.
/// </remarks>
internal sealed class ConditionExpression : Expression {
  internal ConditionExpression(Expression test, Expression ifTrue, Expression ifFalse, Type type)
    : base(ExpressionType.Conditional, type ?? typeof(void)) {
    this.Test = test ?? throw new ArgumentNullException(nameof(test));
    this.IfTrue = ifTrue ?? throw new ArgumentNullException(nameof(ifTrue));
    this.IfFalse = ifFalse ?? throw new ArgumentNullException(nameof(ifFalse));
  }

  public Expression Test { get; }
  public Expression IfTrue { get; }
  public Expression IfFalse { get; }
}

#endif
