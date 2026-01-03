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

using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
#if SUPPORTS_LINQ
// net35: Use our helper for missing Expression methods
using Expr = System.Dynamic.Expr;
#endif

namespace System.Dynamic;

/// <summary>
/// The dynamic binding of an object that uses <see cref="DynamicMetaObject"/>.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="DynamicMetaObjectBinder"/> is the base class for all binders that perform dynamic binding
/// using the <see cref="DynamicMetaObject"/> protocol. This includes operations like getting/setting members,
/// invoking methods, performing arithmetic operations, and more.
/// </para>
/// <para>
/// Derived classes implement the <see cref="Bind"/> method which delegates to the appropriate
/// <c>Bind*</c> method on the <see cref="DynamicMetaObject"/>.
/// </para>
/// </remarks>
public abstract class DynamicMetaObjectBinder : CallSiteBinder {

  /// <summary>
  /// Initializes a new instance of the <see cref="DynamicMetaObjectBinder"/> class.
  /// </summary>
  protected DynamicMetaObjectBinder() { }

  /// <summary>
  /// Gets the return type of the operation.
  /// </summary>
  /// <remarks>
  /// The default implementation returns <see cref="object"/>. Derived classes may override
  /// this property to return a more specific type when known.
  /// </remarks>
  public virtual Type ReturnType => typeof(object);

  /// <summary>
  /// Performs the runtime binding of the dynamic operation.
  /// </summary>
  /// <param name="args">An array of arguments to the dynamic operation.</param>
  /// <param name="parameters">The parameters representing the arguments to the dynamic operation.</param>
  /// <param name="returnLabel">The label used to return from the dynamic operation.</param>
  /// <returns>
  /// An <see cref="Expression"/> that performs tests on the dynamic operation arguments and
  /// performs the operation if the tests succeed, or jumps to <see cref="CallSiteBinder.UpdateLabel"/> otherwise.
  /// </returns>
  public sealed override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) {
    if (args == null)
      throw new ArgumentNullException(nameof(args));
    if (parameters == null)
      throw new ArgumentNullException(nameof(parameters));
    if (args.Length == 0)
      throw new ArgumentException("Args array must have at least one element.", nameof(args));
    if (parameters.Count == 0)
      throw new ArgumentException("Parameters collection must have at least one element.", nameof(parameters));
    if (args.Length != parameters.Count)
      throw new ArgumentException("Args and parameters must have the same count.");

    // Create DynamicMetaObjects for all arguments
    var target = DynamicMetaObject.Create(args[0], parameters[0]);
    var metaArgs = new DynamicMetaObject[args.Length - 1];
    for (var i = 1; i < args.Length; ++i)
      metaArgs[i - 1] = DynamicMetaObject.Create(args[i], parameters[i]);

    // Perform the binding
    var binding = this.Bind(target, metaArgs);
    if (binding == null)
      throw new InvalidOperationException("Bind cannot return null.");

    // Build the expression with restrictions
    var restrictions = binding.Restrictions.ToExpression();
    var body = binding.Expression;

    // Convert to return type if needed
    if (body.Type != this.ReturnType && this.ReturnType != typeof(void))
      body = Expression.Convert(body, this.ReturnType);

    // Build the conditional expression:
    // if (restrictions) { return body; } else { goto UpdateLabel; }
#if !SUPPORTS_LINQ
    var result = Expression.Condition(
      restrictions,
      Expression.Block(
        Expression.Label(returnLabel, body)
      ),
      Expression.Goto(UpdateLabel),
      typeof(void)
    );
#else
    var result = Expr.Condition(
      restrictions,
      Expr.Block(
        Expr.Label(returnLabel, body)
      ),
      Expr.Goto(UpdateLabel),
      typeof(void)
    );
#endif

    return result;
  }

  /// <summary>
  /// When overridden in the derived class, performs the binding of the dynamic operation.
  /// </summary>
  /// <param name="target">The target of the dynamic operation.</param>
  /// <param name="args">An array of arguments of the dynamic operation.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public abstract DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args);

  /// <summary>
  /// Defers the binding of the operation to runtime.
  /// </summary>
  /// <param name="target">The target of the dynamic operation.</param>
  /// <param name="args">The arguments of the dynamic operation.</param>
  /// <returns>
  /// A <see cref="DynamicMetaObject"/> representing the deferred binding.
  /// </returns>
  /// <remarks>
  /// This method is used when binding cannot be completed statically and must be deferred
  /// until the actual runtime values are known.
  /// </remarks>
  public DynamicMetaObject Defer(DynamicMetaObject target, params DynamicMetaObject[] args) {
    if (target == null)
      throw new ArgumentNullException(nameof(target));

    // Combine all restrictions
    var restrictions = target.Restrictions;
    if (args != null)
      foreach (var arg in args)
        if (arg != null)
          restrictions = restrictions.Merge(arg.Restrictions);

    // Return a meta-object that will re-bind at runtime
    return new DynamicMetaObject(
#if !SUPPORTS_LINQ
      Expression.Empty(),
#else
      Expression.Constant(null, typeof(object)),
#endif
      restrictions
    );
  }

  /// <summary>
  /// Defers the binding of the operation to runtime.
  /// </summary>
  /// <param name="args">The arguments of the dynamic operation.</param>
  /// <returns>
  /// A <see cref="DynamicMetaObject"/> representing the deferred binding.
  /// </returns>
  public DynamicMetaObject Defer(params DynamicMetaObject[] args) {
    if (args == null || args.Length == 0)
      throw new ArgumentException("Args array must have at least one element.", nameof(args));
    return this.Defer(args[0], args.Length > 1 ? args[1..] : Array.Empty<DynamicMetaObject>());
  }

  /// <summary>
  /// Gets an expression that will cause the binding to be updated.
  /// </summary>
  /// <param name="type">The type of the expression.</param>
  /// <returns>An expression that triggers a binding update.</returns>
#if !SUPPORTS_LINQ
  public Expression GetUpdateExpression(Type type) =>
    Expression.Goto(UpdateLabel, type != typeof(void) ? Expression.Default(type) : null);
#else
  public Expression GetUpdateExpression(Type type) =>
    Expr.Goto(UpdateLabel, type != typeof(void) ? Expr.Default(type) : null);
#endif

}

#endif
