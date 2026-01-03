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

namespace System.Dynamic;

/// <summary>
/// Represents the dynamic create instance (new) operation at the call site, providing the binding semantics
/// for dynamic constructor invocation operations.
/// </summary>
public abstract class CreateInstanceBinder : DynamicMetaObjectBinder {

  /// <summary>
  /// Initializes a new instance of the <see cref="CreateInstanceBinder"/> class.
  /// </summary>
  /// <param name="callInfo">The <see cref="CallInfo"/> that describes the signature of the constructor call.</param>
  /// <exception cref="ArgumentNullException"><paramref name="callInfo"/> is null.</exception>
  protected CreateInstanceBinder(CallInfo callInfo) =>
    this.CallInfo = callInfo ?? throw new ArgumentNullException(nameof(callInfo));

  /// <summary>
  /// Gets the <see cref="CallInfo"/> that describes the signature of the constructor call.
  /// </summary>
  public CallInfo CallInfo { get; }

  /// <summary>
  /// Performs the binding of the dynamic create instance operation.
  /// </summary>
  /// <param name="target">The target of the dynamic create instance operation (the type or constructor).</param>
  /// <param name="args">An array of arguments of the create instance operation.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args) {
    if (target == null)
      throw new ArgumentNullException(nameof(target));
    if (args == null)
      throw new ArgumentNullException(nameof(args));
    return target.BindCreateInstance(this, args);
  }

  /// <summary>
  /// Provides the implementation for the dynamic create instance operation.
  /// </summary>
  /// <param name="target">The target of the dynamic create instance operation.</param>
  /// <param name="args">The arguments of the create instance operation.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public abstract DynamicMetaObject FallbackCreateInstance(DynamicMetaObject target, DynamicMetaObject[] args);

  /// <summary>
  /// Provides the implementation for the dynamic create instance operation.
  /// </summary>
  /// <param name="target">The target of the dynamic create instance operation.</param>
  /// <param name="args">The arguments of the create instance operation.</param>
  /// <param name="errorSuggestion">The binding to use if binding fails, or null.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public virtual DynamicMetaObject FallbackCreateInstance(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion) =>
    this.FallbackCreateInstance(target, args);

}

#endif
