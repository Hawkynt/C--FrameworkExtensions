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
using System.Collections.ObjectModel;
using System.Linq.Expressions;
#if SUPPORTS_LINQ
// net35: Use our helper for missing Expression methods
using System.Dynamic;
#endif

namespace System.Runtime.CompilerServices;

/// <summary>
/// Represents the base class for all call site binders in the Dynamic Language Runtime.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="CallSiteBinder"/> is the foundation for dynamic binding in the DLR. Derived classes
/// implement the <see cref="Bind"/> method to specify how dynamic operations are resolved.
/// </para>
/// <para>
/// The binder produces an expression tree that represents the binding logic, which can then
/// be compiled and cached for efficient execution.
/// </para>
/// </remarks>
public abstract class CallSiteBinder {

  /// <summary>
  /// A label that can be used by binders to signal that the binding should be updated.
  /// </summary>
  /// <remarks>
  /// When a binding fails at runtime, the target can jump to this label to trigger a rebind.
  /// </remarks>
#if !SUPPORTS_LINQ
  // net20: Use our Expression polyfill
  public static LabelTarget UpdateLabel { get; } = Expression.Label("CallSiteBinder.UpdateLabel");
#else
  // net35: Use our Expr helper
  public static LabelTarget UpdateLabel { get; } = Expr.Label("CallSiteBinder.UpdateLabel");
#endif

  /// <summary>
  /// Initializes a new instance of the <see cref="CallSiteBinder"/> class.
  /// </summary>
  protected CallSiteBinder() { }

  /// <summary>
  /// Performs the runtime binding of the dynamic operation.
  /// </summary>
  /// <param name="args">An array of arguments to the dynamic operation.</param>
  /// <param name="parameters">The parameters representing the arguments to the dynamic operation.</param>
  /// <param name="returnLabel">The label used to return from the dynamic operation.</param>
  /// <returns>
  /// An <see cref="Expression"/> that performs tests on the dynamic operation arguments and
  /// performs the operation if the tests succeed, or jumps to <see cref="UpdateLabel"/> otherwise.
  /// </returns>
  public abstract Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel);

  /// <summary>
  /// Provides a low-level runtime binding of the dynamic operation for caching.
  /// </summary>
  /// <typeparam name="T">The target type of the <see cref="CallSite{T}"/>.</typeparam>
  /// <param name="site">The <see cref="CallSite{T}"/> performing the binding.</param>
  /// <param name="args">The arguments to the dynamic operation.</param>
  /// <returns>A delegate representing the bound operation.</returns>
  /// <remarks>
  /// This method is called by the DLR when a binding needs to be cached. The base implementation
  /// compiles the expression returned by <see cref="Bind"/> into a delegate.
  /// </remarks>
  public virtual T BindDelegate<T>(CallSite<T> site, object[] args) where T : class =>
    null; // Default implementation returns null, forcing the standard binding path

  /// <summary>
  /// Gets the type used to cache binding rules.
  /// </summary>
  /// <remarks>
  /// This property returns a type that uniquely identifies rules that can be shared.
  /// The default implementation returns <see langword="null"/>, indicating that rules
  /// should not be shared across binders.
  /// </remarks>
  public virtual Type CacheIdentity => null;

}

#endif
