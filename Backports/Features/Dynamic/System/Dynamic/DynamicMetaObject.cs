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

namespace System.Dynamic;

/// <summary>
/// Represents the dynamic binding and a binding logic of an object participating in the dynamic binding.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="DynamicMetaObject"/> is one of the key concepts in the Dynamic Language Runtime (DLR).
/// It represents a dynamic object along with its binding restrictions and provides virtual methods
/// for binding various dynamic operations.
/// </para>
/// <para>
/// Each dynamic operation (get member, set member, invoke, etc.) corresponds to a <c>Bind*</c> method
/// that returns a new <see cref="DynamicMetaObject"/> representing the result of the operation.
/// </para>
/// </remarks>
public class DynamicMetaObject {

  /// <summary>
  /// Represents an empty array of <see cref="DynamicMetaObject"/> instances.
  /// </summary>
  public static readonly DynamicMetaObject[] EmptyMetaObjects = Array.Empty<DynamicMetaObject>();

  private readonly object _value;
  private readonly bool _hasValue;

  /// <summary>
  /// Initializes a new instance of the <see cref="DynamicMetaObject"/> class.
  /// </summary>
  /// <param name="expression">
  /// The expression representing this <see cref="DynamicMetaObject"/> during the dynamic binding process.
  /// </param>
  /// <param name="restrictions">The set of binding restrictions under which the binding is valid.</param>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="expression"/> or <paramref name="restrictions"/> is null.
  /// </exception>
  public DynamicMetaObject(Expression expression, BindingRestrictions restrictions) {
    this.Expression = expression ?? throw new ArgumentNullException(nameof(expression));
    this.Restrictions = restrictions ?? throw new ArgumentNullException(nameof(restrictions));
    this._hasValue = false;
    this._value = null;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="DynamicMetaObject"/> class.
  /// </summary>
  /// <param name="expression">
  /// The expression representing this <see cref="DynamicMetaObject"/> during the dynamic binding process.
  /// </param>
  /// <param name="restrictions">The set of binding restrictions under which the binding is valid.</param>
  /// <param name="value">The runtime value represented by the <see cref="DynamicMetaObject"/>.</param>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="expression"/> or <paramref name="restrictions"/> is null.
  /// </exception>
  public DynamicMetaObject(Expression expression, BindingRestrictions restrictions, object value) {
    this.Expression = expression ?? throw new ArgumentNullException(nameof(expression));
    this.Restrictions = restrictions ?? throw new ArgumentNullException(nameof(restrictions));
    this._hasValue = true;
    this._value = value;
  }

  /// <summary>
  /// Gets the expression representing the <see cref="DynamicMetaObject"/> during the dynamic binding process.
  /// </summary>
  public Expression Expression { get; }

  /// <summary>
  /// Gets the set of binding restrictions under which the binding is valid.
  /// </summary>
  public BindingRestrictions Restrictions { get; }

  /// <summary>
  /// Gets the runtime value represented by this <see cref="DynamicMetaObject"/>.
  /// </summary>
  /// <exception cref="InvalidOperationException">
  /// <see cref="HasValue"/> is <see langword="false"/>.
  /// </exception>
  public object Value {
    get {
      if (!this._hasValue)
        throw new InvalidOperationException("No value is associated with this meta-object.");
      return this._value;
    }
  }

  /// <summary>
  /// Gets a value indicating whether the <see cref="DynamicMetaObject"/> has a runtime value.
  /// </summary>
  public bool HasValue => this._hasValue;

  /// <summary>
  /// Gets the <see cref="Type"/> of the runtime value or null if this object doesn't have a value.
  /// </summary>
  /// <remarks>
  /// When <see cref="HasValue"/> is <see langword="true"/>, this returns the actual runtime type of
  /// <see cref="Value"/>. Otherwise, it returns <see cref="Expression.Type"/>.
  /// </remarks>
  public Type RuntimeType => this._hasValue ? this._value?.GetType() : null;

  /// <summary>
  /// Gets the limit type of the <see cref="DynamicMetaObject"/>.
  /// </summary>
  /// <remarks>
  /// The limit type is the most specific type known about the object. When <see cref="HasValue"/> is
  /// <see langword="true"/>, this is the runtime type of the value. Otherwise, it's the expression type.
  /// </remarks>
  public Type LimitType => this.RuntimeType ?? this.Expression.Type;

  /// <summary>
  /// Performs the binding of the dynamic get member operation.
  /// </summary>
  /// <param name="binder">An instance of the <see cref="GetMemberBinder"/> that represents the operation.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public virtual DynamicMetaObject BindGetMember(GetMemberBinder binder) =>
    binder.FallbackGetMember(this);

  /// <summary>
  /// Performs the binding of the dynamic set member operation.
  /// </summary>
  /// <param name="binder">An instance of the <see cref="SetMemberBinder"/> that represents the operation.</param>
  /// <param name="value">The <see cref="DynamicMetaObject"/> representing the value to set.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public virtual DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value) =>
    binder.FallbackSetMember(this, value);

  /// <summary>
  /// Performs the binding of the dynamic delete member operation.
  /// </summary>
  /// <param name="binder">An instance of the <see cref="DeleteMemberBinder"/> that represents the operation.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public virtual DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder) =>
    binder.FallbackDeleteMember(this);

  /// <summary>
  /// Performs the binding of the dynamic get index operation.
  /// </summary>
  /// <param name="binder">An instance of the <see cref="GetIndexBinder"/> that represents the operation.</param>
  /// <param name="indexes">The <see cref="DynamicMetaObject"/> instances representing the indexes.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public virtual DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes) =>
    binder.FallbackGetIndex(this, indexes);

  /// <summary>
  /// Performs the binding of the dynamic set index operation.
  /// </summary>
  /// <param name="binder">An instance of the <see cref="SetIndexBinder"/> that represents the operation.</param>
  /// <param name="indexes">The <see cref="DynamicMetaObject"/> instances representing the indexes.</param>
  /// <param name="value">The <see cref="DynamicMetaObject"/> representing the value to set.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public virtual DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value) =>
    binder.FallbackSetIndex(this, indexes, value);

  /// <summary>
  /// Performs the binding of the dynamic delete index operation.
  /// </summary>
  /// <param name="binder">An instance of the <see cref="DeleteIndexBinder"/> that represents the operation.</param>
  /// <param name="indexes">The <see cref="DynamicMetaObject"/> instances representing the indexes.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public virtual DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes) =>
    binder.FallbackDeleteIndex(this, indexes);

  /// <summary>
  /// Performs the binding of the dynamic invoke member operation.
  /// </summary>
  /// <param name="binder">An instance of the <see cref="InvokeMemberBinder"/> that represents the operation.</param>
  /// <param name="args">The <see cref="DynamicMetaObject"/> instances representing the arguments.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public virtual DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args) =>
    binder.FallbackInvokeMember(this, args);

  /// <summary>
  /// Performs the binding of the dynamic invoke operation.
  /// </summary>
  /// <param name="binder">An instance of the <see cref="InvokeBinder"/> that represents the operation.</param>
  /// <param name="args">The <see cref="DynamicMetaObject"/> instances representing the arguments.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public virtual DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args) =>
    binder.FallbackInvoke(this, args);

  /// <summary>
  /// Performs the binding of the dynamic create instance operation.
  /// </summary>
  /// <param name="binder">An instance of the <see cref="CreateInstanceBinder"/> that represents the operation.</param>
  /// <param name="args">The <see cref="DynamicMetaObject"/> instances representing the constructor arguments.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public virtual DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args) =>
    binder.FallbackCreateInstance(this, args);

  /// <summary>
  /// Performs the binding of the dynamic unary operation.
  /// </summary>
  /// <param name="binder">An instance of the <see cref="UnaryOperationBinder"/> that represents the operation.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public virtual DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder) =>
    binder.FallbackUnaryOperation(this);

  /// <summary>
  /// Performs the binding of the dynamic binary operation.
  /// </summary>
  /// <param name="binder">An instance of the <see cref="BinaryOperationBinder"/> that represents the operation.</param>
  /// <param name="arg">The <see cref="DynamicMetaObject"/> representing the right-hand operand.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public virtual DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg) =>
    binder.FallbackBinaryOperation(this, arg);

  /// <summary>
  /// Performs the binding of the dynamic convert operation.
  /// </summary>
  /// <param name="binder">An instance of the <see cref="ConvertBinder"/> that represents the operation.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public virtual DynamicMetaObject BindConvert(ConvertBinder binder) =>
    binder.FallbackConvert(this);

  /// <summary>
  /// Returns a sequence of all dynamic member names for this object.
  /// </summary>
  /// <returns>A sequence of dynamic member names.</returns>
  public virtual IEnumerable<string> GetDynamicMemberNames() =>
    Array.Empty<string>();

  /// <summary>
  /// Creates a <see cref="DynamicMetaObject"/> for a given object.
  /// </summary>
  /// <param name="value">The object to create a <see cref="DynamicMetaObject"/> for.</param>
  /// <param name="expression">The expression representing the object.</param>
  /// <returns>
  /// A <see cref="DynamicMetaObject"/> representing the object.
  /// </returns>
  public static DynamicMetaObject Create(object value, Expression expression) {
    if (value is IDynamicMetaObjectProvider provider)
      return provider.GetMetaObject(expression);
    return new DynamicMetaObject(expression, BindingRestrictions.Empty, value);
  }

}

#endif
