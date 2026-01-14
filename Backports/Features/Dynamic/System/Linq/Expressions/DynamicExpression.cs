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
using System.Runtime.CompilerServices;
// On net35, BCL ExpressionType lacks net40+ values - use our utility that provides raw casts
using ExpressionType = Utilities.ExpressionType;

namespace System.Linq.Expressions;

/// <summary>
/// Represents a dynamic operation performed by a <see cref="CallSiteBinder"/>.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="DynamicExpression"/> represents a node in an expression tree that performs
/// a dynamic operation. The operation is bound at runtime by the <see cref="Binder"/>.
/// </para>
/// <para>
/// Common dynamic operations include property access, method invocation, and
/// arithmetic operations on dynamically typed objects.
/// </para>
/// </remarks>
public class DynamicExpression : Expression {

  /// <summary>
  /// The arguments to the dynamic operation.
  /// </summary>
  private readonly ReadOnlyCollection<Expression> _arguments;

  /// <summary>
  /// Initializes a new instance of the <see cref="DynamicExpression"/> class.
  /// </summary>
  /// <param name="delegateType">The type of the delegate used by the call site.</param>
  /// <param name="binder">The <see cref="CallSiteBinder"/> that binds the dynamic operation.</param>
  /// <param name="arguments">The arguments to the dynamic operation.</param>
#if !SUPPORTS_LINQ
  // net20: Our Expression polyfill has parameterless constructor
  internal DynamicExpression(Type delegateType, CallSiteBinder binder, IEnumerable<Expression> arguments) {
#else
  // net35: BCL Expression requires nodeType and type in constructor
  internal DynamicExpression(Type delegateType, CallSiteBinder binder, IEnumerable<Expression> arguments)
    : base(Utilities.ExpressionType.Dynamic, GetReturnType(delegateType)) {
#endif
    this.DelegateType = delegateType ?? throw new ArgumentNullException(nameof(delegateType));
    this.Binder = binder ?? throw new ArgumentNullException(nameof(binder));
    this._arguments = arguments != null
      ? new ReadOnlyCollection<Expression>(new List<Expression>(arguments))
      : new ReadOnlyCollection<Expression>(Array.Empty<Expression>());
  }

  /// <summary>
  /// Gets the node type of this <see cref="Expression"/>.
  /// </summary>
  /// <value>Returns <see cref="ExpressionType.Dynamic"/>.</value>
#if !SUPPORTS_LINQ
  // net20: Our Expression polyfill has virtual NodeType
  public sealed override ExpressionType NodeType => ExpressionType.Dynamic;
#else
  // net35: BCL Expression.NodeType is not virtual, use new to shadow
  public new System.Linq.Expressions.ExpressionType NodeType => Utilities.ExpressionType.Dynamic;
#endif

  /// <summary>
  /// Gets the static type of the expression that this <see cref="Expression"/> represents.
  /// </summary>
  /// <value>The return type of the dynamic operation.</value>
#if !SUPPORTS_LINQ
  // net20: Our Expression polyfill has virtual Type
  public override Type Type => GetReturnType(this.DelegateType);
#else
  // net35: BCL Expression.Type is not virtual, use new to shadow
  public new Type Type => GetReturnType(this.DelegateType);
#endif

  /// <summary>
  /// Gets the return type from a delegate type.
  /// </summary>
  private static Type GetReturnType(Type delegateType) {
    var invokeMethod = delegateType?.GetMethod("Invoke");
    return invokeMethod?.ReturnType ?? typeof(object);
  }

  /// <summary>
  /// Gets the <see cref="CallSiteBinder"/> that binds the dynamic operation.
  /// </summary>
  /// <value>The binder for this dynamic expression.</value>
  public CallSiteBinder Binder { get; }

  /// <summary>
  /// Gets the type of the delegate used by the <see cref="CallSite"/>.
  /// </summary>
  /// <value>The delegate type for this dynamic expression.</value>
  public Type DelegateType { get; }

  /// <summary>
  /// Gets the arguments to the dynamic operation.
  /// </summary>
  /// <value>A read-only collection of expressions representing the arguments.</value>
  public ReadOnlyCollection<Expression> Arguments => this._arguments;

  // Accept method only available when we have ExpressionVisitor
#if !SUPPORTS_LINQ
  /// <summary>
  /// Dispatches to the specific visit method for this node type.
  /// </summary>
  /// <param name="visitor">The visitor to visit this node with.</param>
  /// <returns>The result of visiting this node.</returns>
  protected internal override Expression Accept(ExpressionVisitor visitor) => visitor.VisitDynamic(this);
#endif

  /// <summary>
  /// Creates a new expression that is like this one, but using the supplied children.
  /// </summary>
  /// <param name="arguments">The <see cref="Arguments"/> property of the result.</param>
  /// <returns>
  /// This expression if no children changed, or an expression with the updated children.
  /// </returns>
  public DynamicExpression Update(IEnumerable<Expression> arguments) {
    var newArgs = arguments != null ? new List<Expression>(arguments) : new List<Expression>();

    if (SameElements(this._arguments, newArgs))
      return this;

    return MakeDynamic(this.DelegateType, this.Binder, newArgs);
  }

  /// <summary>
  /// Checks if two collections have the same elements.
  /// </summary>
  private static bool SameElements(IList<Expression> first, IList<Expression> second) {
    if (first.Count != second.Count)
      return false;

    for (var i = 0; i < first.Count; ++i)
      if (!ReferenceEquals(first[i], second[i]))
        return false;

    return true;
  }

  // CS0108: Member hides inherited member (net20 polyfill Expression has these methods)
  // CS0109: Member does not hide inherited member (net35 BCL Expression lacks these methods)
#pragma warning disable CS0108, CS0109
  #region Factory Methods

  /// <summary>
  /// Creates a <see cref="DynamicExpression"/> that represents a dynamic operation
  /// bound by the provided <see cref="CallSiteBinder"/>.
  /// </summary>
  /// <param name="binder">The <see cref="CallSiteBinder"/> that binds the dynamic operation.</param>
  /// <param name="returnType">The return type of the dynamic expression.</param>
  /// <param name="arguments">The arguments to the dynamic operation.</param>
  /// <returns>A <see cref="DynamicExpression"/> that has the specified binder, return type, and arguments.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="binder"/> is <see langword="null"/>.</exception>
  public new static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, params Expression[] arguments) =>
    Dynamic(binder, returnType, (IEnumerable<Expression>)arguments);

  /// <summary>
  /// Creates a <see cref="DynamicExpression"/> that represents a dynamic operation
  /// bound by the provided <see cref="CallSiteBinder"/>.
  /// </summary>
  /// <param name="binder">The <see cref="CallSiteBinder"/> that binds the dynamic operation.</param>
  /// <param name="returnType">The return type of the dynamic expression.</param>
  /// <param name="arguments">The arguments to the dynamic operation.</param>
  /// <returns>A <see cref="DynamicExpression"/> that has the specified binder, return type, and arguments.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="binder"/> is <see langword="null"/>.</exception>
  public new static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, IEnumerable<Expression> arguments) {
    if (binder == null)
      throw new ArgumentNullException(nameof(binder));
    if (returnType == null)
      throw new ArgumentNullException(nameof(returnType));

    // Create appropriate delegate type based on argument count and return type
    var delegateType = CreateDelegateType(returnType, arguments);
    return new DynamicExpression(delegateType, binder, arguments);
  }

  /// <summary>
  /// Creates a <see cref="DynamicExpression"/> that represents a dynamic operation
  /// bound by the provided <see cref="CallSiteBinder"/> with no arguments.
  /// </summary>
  /// <param name="binder">The <see cref="CallSiteBinder"/> that binds the dynamic operation.</param>
  /// <param name="returnType">The return type of the dynamic expression.</param>
  /// <returns>A <see cref="DynamicExpression"/> that has the specified binder and return type.</returns>
  public new static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType) =>
    Dynamic(binder, returnType, Array.Empty<Expression>());

  /// <summary>
  /// Creates a <see cref="DynamicExpression"/> that represents a dynamic operation
  /// bound by the provided <see cref="CallSiteBinder"/> with one argument.
  /// </summary>
  /// <param name="binder">The <see cref="CallSiteBinder"/> that binds the dynamic operation.</param>
  /// <param name="returnType">The return type of the dynamic expression.</param>
  /// <param name="arg0">The first argument to the dynamic operation.</param>
  /// <returns>A <see cref="DynamicExpression"/> that has the specified binder, return type, and argument.</returns>
  public new static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0) =>
    Dynamic(binder, returnType, [arg0]);

  /// <summary>
  /// Creates a <see cref="DynamicExpression"/> that represents a dynamic operation
  /// bound by the provided <see cref="CallSiteBinder"/> with two arguments.
  /// </summary>
  /// <param name="binder">The <see cref="CallSiteBinder"/> that binds the dynamic operation.</param>
  /// <param name="returnType">The return type of the dynamic expression.</param>
  /// <param name="arg0">The first argument to the dynamic operation.</param>
  /// <param name="arg1">The second argument to the dynamic operation.</param>
  /// <returns>A <see cref="DynamicExpression"/> that has the specified binder, return type, and arguments.</returns>
  public new static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1) =>
    Dynamic(binder, returnType, [arg0, arg1]);

  /// <summary>
  /// Creates a <see cref="DynamicExpression"/> that represents a dynamic operation
  /// bound by the provided <see cref="CallSiteBinder"/> with three arguments.
  /// </summary>
  /// <param name="binder">The <see cref="CallSiteBinder"/> that binds the dynamic operation.</param>
  /// <param name="returnType">The return type of the dynamic expression.</param>
  /// <param name="arg0">The first argument to the dynamic operation.</param>
  /// <param name="arg1">The second argument to the dynamic operation.</param>
  /// <param name="arg2">The third argument to the dynamic operation.</param>
  /// <returns>A <see cref="DynamicExpression"/> that has the specified binder, return type, and arguments.</returns>
  public new static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1, Expression arg2) =>
    Dynamic(binder, returnType, [arg0, arg1, arg2]);

  /// <summary>
  /// Creates a <see cref="DynamicExpression"/> that represents a dynamic operation
  /// bound by the provided <see cref="CallSiteBinder"/> with four arguments.
  /// </summary>
  /// <param name="binder">The <see cref="CallSiteBinder"/> that binds the dynamic operation.</param>
  /// <param name="returnType">The return type of the dynamic expression.</param>
  /// <param name="arg0">The first argument to the dynamic operation.</param>
  /// <param name="arg1">The second argument to the dynamic operation.</param>
  /// <param name="arg2">The third argument to the dynamic operation.</param>
  /// <param name="arg3">The fourth argument to the dynamic operation.</param>
  /// <returns>A <see cref="DynamicExpression"/> that has the specified binder, return type, and arguments.</returns>
  public new static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1, Expression arg2, Expression arg3) =>
    Dynamic(binder, returnType, [arg0, arg1, arg2, arg3]);

  /// <summary>
  /// Creates a <see cref="DynamicExpression"/> that represents a dynamic operation
  /// using the specified delegate type.
  /// </summary>
  /// <param name="delegateType">The type of the delegate used by the call site.</param>
  /// <param name="binder">The <see cref="CallSiteBinder"/> that binds the dynamic operation.</param>
  /// <param name="arguments">The arguments to the dynamic operation.</param>
  /// <returns>A <see cref="DynamicExpression"/> that has the specified delegate type, binder, and arguments.</returns>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="delegateType"/> or <paramref name="binder"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="ArgumentException">
  /// <paramref name="delegateType"/> is not a delegate type, or its first parameter is not assignable to <see cref="CallSite"/>.
  /// </exception>
  public new static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, IEnumerable<Expression> arguments) {
    if (delegateType == null)
      throw new ArgumentNullException(nameof(delegateType));
    if (binder == null)
      throw new ArgumentNullException(nameof(binder));
    if (!typeof(Delegate).IsAssignableFrom(delegateType))
      throw new ArgumentException("Type must be a delegate type.", nameof(delegateType));

    return new DynamicExpression(delegateType, binder, arguments);
  }

  /// <summary>
  /// Creates a <see cref="DynamicExpression"/> that represents a dynamic operation
  /// using the specified delegate type.
  /// </summary>
  /// <param name="delegateType">The type of the delegate used by the call site.</param>
  /// <param name="binder">The <see cref="CallSiteBinder"/> that binds the dynamic operation.</param>
  /// <param name="arguments">The arguments to the dynamic operation.</param>
  /// <returns>A <see cref="DynamicExpression"/> that has the specified delegate type, binder, and arguments.</returns>
  public new static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, params Expression[] arguments) =>
    MakeDynamic(delegateType, binder, (IEnumerable<Expression>)arguments);

  /// <summary>
  /// Creates the appropriate delegate type for the dynamic expression.
  /// </summary>
  private static Type CreateDelegateType(Type returnType, IEnumerable<Expression> arguments) {
    var argList = arguments != null ? new List<Expression>(arguments) : new List<Expression>();
    var argCount = argList.Count;

    // The delegate signature is: delegate TReturn(CallSite, T0, T1, ...)
    // We need to add CallSite as the first parameter

    if (returnType == typeof(void)) {
      // Use Action-like delegate
      return argCount switch {
        0 => typeof(Action<CallSite>),
        1 => typeof(Action<,>).MakeGenericType(typeof(CallSite), argList[0].Type),
        2 => typeof(Action<,,>).MakeGenericType(typeof(CallSite), argList[0].Type, argList[1].Type),
        3 => typeof(Action<,,,>).MakeGenericType(typeof(CallSite), argList[0].Type, argList[1].Type, argList[2].Type),
        4 => typeof(Action<,,,,>).MakeGenericType(typeof(CallSite), argList[0].Type, argList[1].Type, argList[2].Type, argList[3].Type),
        _ => throw new NotSupportedException($"Dynamic expressions with {argCount} arguments are not supported.")
      };
    } else {
      // Use Func-like delegate
      return argCount switch {
        0 => typeof(Func<,>).MakeGenericType(typeof(CallSite), returnType),
        1 => typeof(Func<,,>).MakeGenericType(typeof(CallSite), argList[0].Type, returnType),
        2 => typeof(Func<,,,>).MakeGenericType(typeof(CallSite), argList[0].Type, argList[1].Type, returnType),
        3 => typeof(Func<,,,,>).MakeGenericType(typeof(CallSite), argList[0].Type, argList[1].Type, argList[2].Type, returnType),
        4 => typeof(Func<,,,,,>).MakeGenericType(typeof(CallSite), argList[0].Type, argList[1].Type, argList[2].Type, argList[3].Type, returnType),
        _ => throw new NotSupportedException($"Dynamic expressions with {argCount} arguments are not supported.")
      };
    }
  }

  #endregion
#pragma warning restore CS0108, CS0109

}

#endif
