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
/// Represents the dynamic invoke operation at the call site, providing the binding semantics
/// for dynamic invoke (delegate or callable object invocation) operations.
/// </summary>
public abstract class InvokeBinder : DynamicMetaObjectBinder {

  /// <summary>
  /// Initializes a new instance of the <see cref="InvokeBinder"/> class.
  /// </summary>
  /// <param name="callInfo">The <see cref="CallInfo"/> that describes the signature of the call.</param>
  /// <exception cref="ArgumentNullException"><paramref name="callInfo"/> is null.</exception>
  protected InvokeBinder(CallInfo callInfo) =>
    this.CallInfo = callInfo ?? throw new ArgumentNullException(nameof(callInfo));

  /// <summary>
  /// Gets the <see cref="CallInfo"/> that describes the signature of the call.
  /// </summary>
  public CallInfo CallInfo { get; }

  /// <summary>
  /// Performs the binding of the dynamic invoke operation.
  /// </summary>
  /// <param name="target">The target of the dynamic invoke operation (the callable object).</param>
  /// <param name="args">An array of arguments of the invoke operation.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args) {
    if (target == null)
      throw new ArgumentNullException(nameof(target));
    if (args == null)
      throw new ArgumentNullException(nameof(args));
    return target.BindInvoke(this, args);
  }

  /// <summary>
  /// Provides the implementation for the dynamic invoke operation.
  /// </summary>
  /// <param name="target">The target of the dynamic invoke operation.</param>
  /// <param name="args">The arguments of the invoke operation.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public abstract DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args);

  /// <summary>
  /// Provides the implementation for the dynamic invoke operation.
  /// </summary>
  /// <param name="target">The target of the dynamic invoke operation.</param>
  /// <param name="args">The arguments of the invoke operation.</param>
  /// <param name="errorSuggestion">The binding to use if binding fails, or null.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public virtual DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion) =>
    this.FallbackInvoke(target, args);

}

#endif
