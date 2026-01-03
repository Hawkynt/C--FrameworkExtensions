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

using System.Linq.Expressions;
// On net35, BCL ExpressionType lacks net40+ values - use our utility that provides raw casts
using ExpressionType = Utilities.ExpressionType;

namespace System.Dynamic;

/// <summary>
/// Represents the dynamic unary operation at the call site, providing the binding semantics
/// for dynamic unary operations like negation, increment, and logical NOT.
/// </summary>
public abstract class UnaryOperationBinder : DynamicMetaObjectBinder {

  /// <summary>
  /// Initializes a new instance of the <see cref="UnaryOperationBinder"/> class.
  /// </summary>
  /// <param name="operation">The unary operation type.</param>
  protected UnaryOperationBinder(System.Linq.Expressions.ExpressionType operation) {
    if (!IsValidUnaryOperation(operation))
      throw new ArgumentException($"Operation {operation} is not a valid unary operation.", nameof(operation));
    this.Operation = operation;
  }

  /// <summary>
  /// Gets the unary operation type.
  /// </summary>
  public System.Linq.Expressions.ExpressionType Operation { get; }

  /// <summary>
  /// Performs the binding of the dynamic unary operation.
  /// </summary>
  /// <param name="target">The target of the dynamic unary operation.</param>
  /// <param name="args">An array of arguments (should be empty).</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args) {
    if (target == null)
      throw new ArgumentNullException(nameof(target));
    return target.BindUnaryOperation(this);
  }

  /// <summary>
  /// Provides the implementation for the dynamic unary operation.
  /// </summary>
  /// <param name="target">The target of the dynamic unary operation.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public abstract DynamicMetaObject FallbackUnaryOperation(DynamicMetaObject target);

  /// <summary>
  /// Provides the implementation for the dynamic unary operation.
  /// </summary>
  /// <param name="target">The target of the dynamic unary operation.</param>
  /// <param name="errorSuggestion">The binding to use if binding fails, or null.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public virtual DynamicMetaObject FallbackUnaryOperation(DynamicMetaObject target, DynamicMetaObject errorSuggestion) =>
    this.FallbackUnaryOperation(target);

  private static bool IsValidUnaryOperation(System.Linq.Expressions.ExpressionType operation) =>
    operation switch {
      ExpressionType.Negate => true,
      ExpressionType.UnaryPlus => true,
      ExpressionType.Not => true,
      ExpressionType.Decrement => true,
      ExpressionType.Increment => true,
      ExpressionType.OnesComplement => true,
      ExpressionType.IsTrue => true,
      ExpressionType.IsFalse => true,
      ExpressionType.NegateChecked => true,
      _ => false
    };

}

#endif
