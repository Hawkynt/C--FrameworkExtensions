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
/// Provides a base class for specifying dynamic behavior at runtime.
/// </summary>
/// <remarks>
/// <para>
/// Inherit from <see cref="DynamicObject"/> when you want to create objects that dynamically
/// respond to member access, method calls, and other operations at runtime.
/// </para>
/// <para>
/// Override the Try* methods to specify the dynamic behavior. The default implementations
/// return <see langword="false"/>, indicating that the operation is not supported.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class MyDynamicObject : DynamicObject
/// {
///     private Dictionary&lt;string, object&gt; _properties = new();
///
///     public override bool TryGetMember(GetMemberBinder binder, out object result)
///     {
///         return _properties.TryGetValue(binder.Name, out result);
///     }
///
///     public override bool TrySetMember(SetMemberBinder binder, object value)
///     {
///         _properties[binder.Name] = value;
///         return true;
///     }
/// }
/// </code>
/// </example>
public class DynamicObject : IDynamicMetaObjectProvider {

  /// <summary>
  /// Initializes a new instance of the <see cref="DynamicObject"/> class.
  /// </summary>
  protected DynamicObject() { }

  /// <summary>
  /// Returns the <see cref="DynamicMetaObject"/> responsible for binding operations performed on this object.
  /// </summary>
  /// <param name="parameter">The expression tree representation of the runtime value.</param>
  /// <returns>
  /// The <see cref="DynamicMetaObject"/> to bind this object.
  /// </returns>
  public virtual DynamicMetaObject GetMetaObject(Expression parameter) => new MetaDynamic(parameter, this);

  #region Try* Methods

  /// <summary>
  /// Provides the implementation for operations that get member values.
  /// </summary>
  /// <param name="binder">Provides information about the object that called the dynamic operation.</param>
  /// <param name="result">The result of the get operation.</param>
  /// <returns>
  /// <see langword="true"/> if the operation is successful; otherwise, <see langword="false"/>.
  /// If this method returns <see langword="false"/>, the run-time binder determines the behavior.
  /// </returns>
  public virtual bool TryGetMember(GetMemberBinder binder, out object? result) {
    result = null;
    return false;
  }

  /// <summary>
  /// Provides the implementation for operations that set member values.
  /// </summary>
  /// <param name="binder">Provides information about the object that called the dynamic operation.</param>
  /// <param name="value">The value to set to the member.</param>
  /// <returns>
  /// <see langword="true"/> if the operation is successful; otherwise, <see langword="false"/>.
  /// If this method returns <see langword="false"/>, the run-time binder determines the behavior.
  /// </returns>
  public virtual bool TrySetMember(SetMemberBinder binder, object? value) => false;

  /// <summary>
  /// Provides the implementation for operations that delete an object member.
  /// </summary>
  /// <param name="binder">Provides information about the delete operation.</param>
  /// <returns>
  /// <see langword="true"/> if the operation is successful; otherwise, <see langword="false"/>.
  /// If this method returns <see langword="false"/>, the run-time binder determines the behavior.
  /// </returns>
  public virtual bool TryDeleteMember(DeleteMemberBinder binder) => false;

  /// <summary>
  /// Provides the implementation for operations that get a value by index.
  /// </summary>
  /// <param name="binder">Provides information about the operation.</param>
  /// <param name="indexes">The indexes that are used in the operation.</param>
  /// <param name="result">The result of the index operation.</param>
  /// <returns>
  /// <see langword="true"/> if the operation is successful; otherwise, <see langword="false"/>.
  /// If this method returns <see langword="false"/>, the run-time binder determines the behavior.
  /// </returns>
  public virtual bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object? result) {
    result = null;
    return false;
  }

  /// <summary>
  /// Provides the implementation for operations that set a value by index.
  /// </summary>
  /// <param name="binder">Provides information about the operation.</param>
  /// <param name="indexes">The indexes that are used in the operation.</param>
  /// <param name="value">The value to set.</param>
  /// <returns>
  /// <see langword="true"/> if the operation is successful; otherwise, <see langword="false"/>.
  /// If this method returns <see langword="false"/>, the run-time binder determines the behavior.
  /// </returns>
  public virtual bool TrySetIndex(SetIndexBinder binder, object[] indexes, object? value) => false;

  /// <summary>
  /// Provides the implementation for operations that delete an index value.
  /// </summary>
  /// <param name="binder">Provides information about the delete operation.</param>
  /// <param name="indexes">The indexes to be deleted.</param>
  /// <returns>
  /// <see langword="true"/> if the operation is successful; otherwise, <see langword="false"/>.
  /// If this method returns <see langword="false"/>, the run-time binder determines the behavior.
  /// </returns>
  public virtual bool TryDeleteIndex(DeleteIndexBinder binder, object[] indexes) => false;

  /// <summary>
  /// Provides the implementation for operations that invoke a member.
  /// </summary>
  /// <param name="binder">Provides information about the dynamic operation.</param>
  /// <param name="args">The arguments that are passed to the object member during the invoke operation.</param>
  /// <param name="result">The result of the member invocation.</param>
  /// <returns>
  /// <see langword="true"/> if the operation is successful; otherwise, <see langword="false"/>.
  /// If this method returns <see langword="false"/>, the run-time binder determines the behavior.
  /// </returns>
  public virtual bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result) {
    result = null;
    return false;
  }

  /// <summary>
  /// Provides the implementation for operations that invoke an object.
  /// </summary>
  /// <param name="binder">Provides information about the invoke operation.</param>
  /// <param name="args">The arguments that are passed to the object during the invoke operation.</param>
  /// <param name="result">The result of the object invocation.</param>
  /// <returns>
  /// <see langword="true"/> if the operation is successful; otherwise, <see langword="false"/>.
  /// If this method returns <see langword="false"/>, the run-time binder determines the behavior.
  /// </returns>
  public virtual bool TryInvoke(InvokeBinder binder, object?[]? args, out object? result) {
    result = null;
    return false;
  }

  /// <summary>
  /// Provides the implementation for operations that create an instance of the object.
  /// </summary>
  /// <param name="binder">Provides information about the create instance operation.</param>
  /// <param name="args">The arguments that are passed to the constructor.</param>
  /// <param name="result">The result of the create instance operation.</param>
  /// <returns>
  /// <see langword="true"/> if the operation is successful; otherwise, <see langword="false"/>.
  /// If this method returns <see langword="false"/>, the run-time binder determines the behavior.
  /// </returns>
  public virtual bool TryCreateInstance(CreateInstanceBinder binder, object?[]? args, out object? result) {
    result = null;
    return false;
  }

  /// <summary>
  /// Provides implementation for unary operations.
  /// </summary>
  /// <param name="binder">Provides information about the unary operation.</param>
  /// <param name="result">The result of the unary operation.</param>
  /// <returns>
  /// <see langword="true"/> if the operation is successful; otherwise, <see langword="false"/>.
  /// If this method returns <see langword="false"/>, the run-time binder determines the behavior.
  /// </returns>
  public virtual bool TryUnaryOperation(UnaryOperationBinder binder, out object? result) {
    result = null;
    return false;
  }

  /// <summary>
  /// Provides implementation for binary operations.
  /// </summary>
  /// <param name="binder">Provides information about the binary operation.</param>
  /// <param name="arg">The right operand for the binary operation.</param>
  /// <param name="result">The result of the binary operation.</param>
  /// <returns>
  /// <see langword="true"/> if the operation is successful; otherwise, <see langword="false"/>.
  /// If this method returns <see langword="false"/>, the run-time binder determines the behavior.
  /// </returns>
  public virtual bool TryBinaryOperation(BinaryOperationBinder binder, object? arg, out object? result) {
    result = null;
    return false;
  }

  /// <summary>
  /// Provides implementation for type conversion operations.
  /// </summary>
  /// <param name="binder">Provides information about the conversion operation.</param>
  /// <param name="result">The result of the type conversion operation.</param>
  /// <returns>
  /// <see langword="true"/> if the operation is successful; otherwise, <see langword="false"/>.
  /// If this method returns <see langword="false"/>, the run-time binder determines the behavior.
  /// </returns>
  public virtual bool TryConvert(ConvertBinder binder, out object? result) {
    result = null;
    return false;
  }

  #endregion

  /// <summary>
  /// Returns the enumeration of all dynamic member names.
  /// </summary>
  /// <returns>A sequence that contains dynamic member names.</returns>
  public virtual IEnumerable<string> GetDynamicMemberNames() => Array.Empty<string>();

  #region MetaDynamic

  /// <summary>
  /// The DynamicMetaObject implementation for DynamicObject.
  /// </summary>
  private sealed class MetaDynamic : DynamicMetaObject {

    /// <summary>
    /// Initializes a new instance of the <see cref="MetaDynamic"/> class.
    /// </summary>
    internal MetaDynamic(Expression expression, DynamicObject value)
      : base(expression, BindingRestrictions.Empty, value) { }

    /// <summary>
    /// Gets the DynamicObject value.
    /// </summary>
    private new DynamicObject Value => (DynamicObject)base.Value!;

    /// <summary>
    /// Creates a DynamicMetaObject that calls the specified Try* method.
    /// </summary>
    private DynamicMetaObject BuildCallMethodWithResult<TBinder>(
      string methodName,
      TBinder binder,
      Expression[] args,
      DynamicMetaObject fallback) where TBinder : DynamicMetaObjectBinder {

      // Build the expression that calls TryXxx and returns the result
      var value = this.Expression;
      var valueAsType = Expression.Convert(value, typeof(DynamicObject));

      // Create the out parameter for the result
      var resultParam = Expression.Parameter(typeof(object), "result");

      // Build the arguments for the Try method call
      var methodArgs = new List<Expression> { Expression.Constant(binder) };
      methodArgs.AddRange(args);
      methodArgs.Add(resultParam);

      // Get the method info
      var method = typeof(DynamicObject).GetMethod(methodName);
      if (method == null)
        return fallback;

      // Build: if (TryXxx(binder, args, out result)) return result; else return fallback
      var callExpression = Expression.Call(valueAsType, method, methodArgs.ToArray());

#if !SUPPORTS_LINQ
      var body = Expression.Block(
        new[] { resultParam },
        Expression.Condition(
          callExpression,
          resultParam,
          Expression.Convert(fallback.Expression, typeof(object))
        )
      );
#else
      var body = Expr.Block(
        new[] { resultParam },
        Expression.Condition(
          callExpression,
          resultParam,
          Expression.Convert(fallback.Expression, typeof(object))
        )
      );
#endif

      return new DynamicMetaObject(
        body,
        this.Restrictions.Merge(fallback.Restrictions).Merge(
          BindingRestrictions.GetTypeRestriction(this.Expression, this.Value.GetType())
        )
      );
    }

    /// <summary>
    /// Creates a DynamicMetaObject that calls the specified Try* method that returns void.
    /// </summary>
    private DynamicMetaObject BuildCallMethodVoid<TBinder>(
      string methodName,
      TBinder binder,
      Expression[] args,
      DynamicMetaObject fallback) where TBinder : DynamicMetaObjectBinder {

      var value = this.Expression;
      var valueAsType = Expression.Convert(value, typeof(DynamicObject));

      // Build the arguments for the Try method call
      var methodArgs = new List<Expression> { Expression.Constant(binder) };
      methodArgs.AddRange(args);

      // Get the method info
      var method = typeof(DynamicObject).GetMethod(methodName);
      if (method == null)
        return fallback;

      // Build: if (TryXxx(binder, args)) return default; else return fallback
      var callExpression = Expression.Call(valueAsType, method, methodArgs.ToArray());

#if !SUPPORTS_LINQ
      var body = Expression.Condition(
        callExpression,
        Expression.Default(typeof(object)),
        fallback.Expression
      );
#else
      var body = Expression.Condition(
        callExpression,
        Expr.Default(typeof(object)),
        fallback.Expression
      );
#endif

      return new DynamicMetaObject(
        body,
        this.Restrictions.Merge(fallback.Restrictions).Merge(
          BindingRestrictions.GetTypeRestriction(this.Expression, this.Value.GetType())
        )
      );
    }

    /// <summary>
    /// Performs the binding of the dynamic get member operation.
    /// </summary>
    public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
      var fallback = binder.FallbackGetMember(this);
      return this.BuildCallMethodWithResult(
        nameof(DynamicObject.TryGetMember),
        binder,
        Array.Empty<Expression>(),
        fallback
      );
    }

    /// <summary>
    /// Performs the binding of the dynamic set member operation.
    /// </summary>
    public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value) {
      var fallback = binder.FallbackSetMember(this, value);
      return this.BuildCallMethodVoid(
        nameof(DynamicObject.TrySetMember),
        binder,
        new[] { value.Expression },
        fallback
      );
    }

    /// <summary>
    /// Performs the binding of the dynamic delete member operation.
    /// </summary>
    public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder) {
      var fallback = binder.FallbackDeleteMember(this);
      return this.BuildCallMethodVoid(
        nameof(DynamicObject.TryDeleteMember),
        binder,
        Array.Empty<Expression>(),
        fallback
      );
    }

    /// <summary>
    /// Performs the binding of the dynamic get index operation.
    /// </summary>
    public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes) {
      var fallback = binder.FallbackGetIndex(this, indexes);
      var indexExprs = new Expression[indexes.Length];
      for (var i = 0; i < indexes.Length; ++i)
        indexExprs[i] = indexes[i].Expression;

      return this.BuildCallMethodWithResult(
        nameof(DynamicObject.TryGetIndex),
        binder,
        new[] { Expression.NewArrayInit(typeof(object), indexExprs) },
        fallback
      );
    }

    /// <summary>
    /// Performs the binding of the dynamic set index operation.
    /// </summary>
    public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value) {
      var fallback = binder.FallbackSetIndex(this, indexes, value);
      var indexExprs = new Expression[indexes.Length];
      for (var i = 0; i < indexes.Length; ++i)
        indexExprs[i] = indexes[i].Expression;

      return this.BuildCallMethodVoid(
        nameof(DynamicObject.TrySetIndex),
        binder,
        new[] { Expression.NewArrayInit(typeof(object), indexExprs), value.Expression },
        fallback
      );
    }

    /// <summary>
    /// Performs the binding of the dynamic delete index operation.
    /// </summary>
    public override DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes) {
      var fallback = binder.FallbackDeleteIndex(this, indexes);
      var indexExprs = new Expression[indexes.Length];
      for (var i = 0; i < indexes.Length; ++i)
        indexExprs[i] = indexes[i].Expression;

      return this.BuildCallMethodVoid(
        nameof(DynamicObject.TryDeleteIndex),
        binder,
        new[] { Expression.NewArrayInit(typeof(object), indexExprs) },
        fallback
      );
    }

    /// <summary>
    /// Performs the binding of the dynamic invoke member operation.
    /// </summary>
    public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args) {
      var fallback = binder.FallbackInvokeMember(this, args);
      var argExprs = new Expression[args.Length];
      for (var i = 0; i < args.Length; ++i)
        argExprs[i] = args[i].Expression;

      return this.BuildCallMethodWithResult(
        nameof(DynamicObject.TryInvokeMember),
        binder,
        new[] { Expression.NewArrayInit(typeof(object), argExprs) },
        fallback
      );
    }

    /// <summary>
    /// Performs the binding of the dynamic invoke operation.
    /// </summary>
    public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args) {
      var fallback = binder.FallbackInvoke(this, args);
      var argExprs = new Expression[args.Length];
      for (var i = 0; i < args.Length; ++i)
        argExprs[i] = args[i].Expression;

      return this.BuildCallMethodWithResult(
        nameof(DynamicObject.TryInvoke),
        binder,
        new[] { Expression.NewArrayInit(typeof(object), argExprs) },
        fallback
      );
    }

    /// <summary>
    /// Performs the binding of the dynamic create instance operation.
    /// </summary>
    public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args) {
      var fallback = binder.FallbackCreateInstance(this, args);
      var argExprs = new Expression[args.Length];
      for (var i = 0; i < args.Length; ++i)
        argExprs[i] = args[i].Expression;

      return this.BuildCallMethodWithResult(
        nameof(DynamicObject.TryCreateInstance),
        binder,
        new[] { Expression.NewArrayInit(typeof(object), argExprs) },
        fallback
      );
    }

    /// <summary>
    /// Performs the binding of the dynamic unary operation.
    /// </summary>
    public override DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder) {
      var fallback = binder.FallbackUnaryOperation(this);
      return this.BuildCallMethodWithResult(
        nameof(DynamicObject.TryUnaryOperation),
        binder,
        Array.Empty<Expression>(),
        fallback
      );
    }

    /// <summary>
    /// Performs the binding of the dynamic binary operation.
    /// </summary>
    public override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg) {
      var fallback = binder.FallbackBinaryOperation(this, arg);
      return this.BuildCallMethodWithResult(
        nameof(DynamicObject.TryBinaryOperation),
        binder,
        new[] { arg.Expression },
        fallback
      );
    }

    /// <summary>
    /// Performs the binding of the dynamic convert operation.
    /// </summary>
    public override DynamicMetaObject BindConvert(ConvertBinder binder) {
      var fallback = binder.FallbackConvert(this);
      return this.BuildCallMethodWithResult(
        nameof(DynamicObject.TryConvert),
        binder,
        Array.Empty<Expression>(),
        fallback
      );
    }

  }

  #endregion

}

#endif
