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

// System.Dynamic was introduced in .NET 4.0
// Only polyfill for net20/net35 where no DLR exists
#if !SUPPORTS_DYNAMIC

using System.Collections.Generic;
using System.Linq.Expressions;
#if SUPPORTS_LINQ
// net35: Use our helper for missing Expression methods
using Expr = System.Dynamic.Expr;
#endif

namespace System.Dynamic;

/// <summary>
/// Represents a set of binding restrictions on the <see cref="DynamicMetaObject"/> under which the dynamic binding is valid.
/// </summary>
/// <remarks>
/// <para>
/// Binding restrictions are conditions that must be true for a binding rule to be valid. They are used
/// to ensure that cached binding rules are only applied when appropriate.
/// </para>
/// <para>
/// Restrictions can be combined using the <see cref="Merge"/> method to create compound restrictions.
/// </para>
/// </remarks>
public abstract class BindingRestrictions {

  private static readonly BindingRestrictions _empty = new CustomRestriction(Expression.Constant(true));

  /// <summary>
  /// Represents an empty set of binding restrictions which are always satisfied.
  /// </summary>
  /// <value>
  /// A <see cref="BindingRestrictions"/> object that represents no restrictions.
  /// </value>
  public static BindingRestrictions Empty => _empty;

  // Private constructor to prevent external inheritance
  private BindingRestrictions() { }

  /// <summary>
  /// Creates a binding restriction that checks the expression for object identity.
  /// </summary>
  /// <param name="expression">The expression to test.</param>
  /// <param name="instance">The exact object instance to test for.</param>
  /// <returns>A new <see cref="BindingRestrictions"/> instance.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="expression"/> is null.</exception>
  public static BindingRestrictions GetInstanceRestriction(Expression expression, object instance) {
    if (expression == null)
      throw new ArgumentNullException(nameof(expression));
    return new InstanceRestriction(expression, instance);
  }

  /// <summary>
  /// Creates a binding restriction that checks the expression for a type restriction.
  /// </summary>
  /// <param name="expression">The expression to test.</param>
  /// <param name="type">The exact type to test for.</param>
  /// <returns>A new <see cref="BindingRestrictions"/> instance.</returns>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="expression"/> or <paramref name="type"/> is null.
  /// </exception>
  public static BindingRestrictions GetTypeRestriction(Expression expression, Type type) {
    if (expression == null)
      throw new ArgumentNullException(nameof(expression));
    if (type == null)
      throw new ArgumentNullException(nameof(type));
    return new TypeRestriction(expression, type);
  }

  /// <summary>
  /// Creates a binding restriction that checks a custom expression for truthiness.
  /// </summary>
  /// <param name="expression">An expression that evaluates to a Boolean.</param>
  /// <returns>A new <see cref="BindingRestrictions"/> instance.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="expression"/> is null.</exception>
  /// <exception cref="ArgumentException"><paramref name="expression"/> does not have a Boolean type.</exception>
  public static BindingRestrictions GetExpressionRestriction(Expression expression) {
    if (expression == null)
      throw new ArgumentNullException(nameof(expression));
    if (expression.Type != typeof(bool))
      throw new ArgumentException("Expression must be of Boolean type.", nameof(expression));
    return new CustomRestriction(expression);
  }

  /// <summary>
  /// Merges this set of binding restrictions with the given binding restrictions.
  /// </summary>
  /// <param name="restrictions">The set of binding restrictions to merge with.</param>
  /// <returns>
  /// A new <see cref="BindingRestrictions"/> instance that represents the merged restrictions.
  /// </returns>
  /// <exception cref="ArgumentNullException"><paramref name="restrictions"/> is null.</exception>
  public BindingRestrictions Merge(BindingRestrictions restrictions) {
    if (restrictions == null)
      throw new ArgumentNullException(nameof(restrictions));
    if (this == Empty)
      return restrictions;
    if (restrictions == Empty)
      return this;
    return new MergedRestriction(this, restrictions);
  }

  /// <summary>
  /// Creates the <see cref="Expression"/> representing the binding restrictions.
  /// </summary>
  /// <returns>An expression tree that evaluates to true when the restrictions are satisfied.</returns>
  public abstract Expression ToExpression();

  /// <summary>
  /// Combines a list of <see cref="DynamicMetaObject"/> objects' restrictions into a single set.
  /// </summary>
  /// <param name="contributingObjects">The list of meta-objects whose restrictions should be combined.</param>
  /// <returns>
  /// A <see cref="BindingRestrictions"/> representing the merged restrictions.
  /// </returns>
  public static BindingRestrictions Combine(IList<DynamicMetaObject> contributingObjects) {
    var result = Empty;
    if (contributingObjects != null)
      foreach (var mo in contributingObjects)
        if (mo != null)
          result = result.Merge(mo.Restrictions);
    return result;
  }

  #region Nested Restriction Types

  private sealed class InstanceRestriction : BindingRestrictions {
    private readonly Expression _expression;
    private readonly object _instance;

    internal InstanceRestriction(Expression expression, object instance) {
      this._expression = expression;
      this._instance = instance;
    }

    public override Expression ToExpression() =>
      Expression.Equal(
        Expression.Convert(this._expression, typeof(object)),
        Expression.Constant(this._instance, typeof(object))
      );
  }

  private sealed class TypeRestriction : BindingRestrictions {
    private readonly Expression _expression;
    private readonly Type _type;

    internal TypeRestriction(Expression expression, Type type) {
      this._expression = expression;
      this._type = type;
    }

    public override Expression ToExpression() {
      if (this._type.IsValueType)
        // For value types, just check the type matches
        return Expression.TypeIs(this._expression, this._type);
      // For reference types, check both non-null and exact type match
      return Expression.AndAlso(
        Expression.NotEqual(
          Expression.Convert(this._expression, typeof(object)),
          Expression.Constant(null, typeof(object))
        ),
#if !SUPPORTS_LINQ
        Expression.TypeEqual(this._expression, this._type)
#else
        Expr.TypeEqual(this._expression, this._type)
#endif
      );
    }
  }

  private sealed class CustomRestriction : BindingRestrictions {
    private readonly Expression _expression;

    internal CustomRestriction(Expression expression) => this._expression = expression;

    public override Expression ToExpression() => this._expression;
  }

  private sealed class MergedRestriction : BindingRestrictions {
    private readonly BindingRestrictions _left;
    private readonly BindingRestrictions _right;

    internal MergedRestriction(BindingRestrictions left, BindingRestrictions right) {
      this._left = left;
      this._right = right;
    }

    public override Expression ToExpression() =>
      Expression.AndAlso(this._left.ToExpression(), this._right.ToExpression());
  }

  #endregion

}

#endif
