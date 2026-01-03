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
/// Represents the dynamic binary operation at the call site, providing the binding semantics
/// for dynamic binary operations like addition, subtraction, and comparison.
/// </summary>
public abstract class BinaryOperationBinder : DynamicMetaObjectBinder {

  /// <summary>
  /// Initializes a new instance of the <see cref="BinaryOperationBinder"/> class.
  /// </summary>
  /// <param name="operation">The binary operation type.</param>
  protected BinaryOperationBinder(System.Linq.Expressions.ExpressionType operation) {
    if (!IsValidBinaryOperation(operation))
      throw new ArgumentException($"Operation {operation} is not a valid binary operation.", nameof(operation));
    this.Operation = operation;
  }

  /// <summary>
  /// Gets the binary operation type.
  /// </summary>
  public System.Linq.Expressions.ExpressionType Operation { get; }

  /// <summary>
  /// Performs the binding of the dynamic binary operation.
  /// </summary>
  /// <param name="target">The left operand of the dynamic binary operation.</param>
  /// <param name="args">An array containing the right operand as the only element.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args) {
    if (target == null)
      throw new ArgumentNullException(nameof(target));
    if (args == null || args.Length != 1)
      throw new ArgumentException("BinaryOperationBinder requires exactly one argument (the right operand).", nameof(args));
    return target.BindBinaryOperation(this, args[0]);
  }

  /// <summary>
  /// Provides the implementation for the dynamic binary operation.
  /// </summary>
  /// <param name="target">The left operand of the dynamic binary operation.</param>
  /// <param name="arg">The right operand of the dynamic binary operation.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public abstract DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg);

  /// <summary>
  /// Provides the implementation for the dynamic binary operation.
  /// </summary>
  /// <param name="target">The left operand of the dynamic binary operation.</param>
  /// <param name="arg">The right operand of the dynamic binary operation.</param>
  /// <param name="errorSuggestion">The binding to use if binding fails, or null.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public virtual DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion) =>
    this.FallbackBinaryOperation(target, arg);

  private static bool IsValidBinaryOperation(System.Linq.Expressions.ExpressionType operation) =>
    operation switch {
      ExpressionType.Add => true,
      ExpressionType.AddChecked => true,
      ExpressionType.Subtract => true,
      ExpressionType.SubtractChecked => true,
      ExpressionType.Multiply => true,
      ExpressionType.MultiplyChecked => true,
      ExpressionType.Divide => true,
      ExpressionType.Modulo => true,
      ExpressionType.Power => true,
      ExpressionType.And => true,
      ExpressionType.Or => true,
      ExpressionType.ExclusiveOr => true,
      ExpressionType.LeftShift => true,
      ExpressionType.RightShift => true,
      ExpressionType.AndAlso => true,
      ExpressionType.OrElse => true,
      ExpressionType.Equal => true,
      ExpressionType.NotEqual => true,
      ExpressionType.GreaterThan => true,
      ExpressionType.GreaterThanOrEqual => true,
      ExpressionType.LessThan => true,
      ExpressionType.LessThanOrEqual => true,
      ExpressionType.AddAssign => true,
      ExpressionType.AddAssignChecked => true,
      ExpressionType.SubtractAssign => true,
      ExpressionType.SubtractAssignChecked => true,
      ExpressionType.MultiplyAssign => true,
      ExpressionType.MultiplyAssignChecked => true,
      ExpressionType.DivideAssign => true,
      ExpressionType.ModuloAssign => true,
      ExpressionType.PowerAssign => true,
      ExpressionType.AndAssign => true,
      ExpressionType.OrAssign => true,
      ExpressionType.ExclusiveOrAssign => true,
      ExpressionType.LeftShiftAssign => true,
      ExpressionType.RightShiftAssign => true,
      _ => false
    };

}

#endif
