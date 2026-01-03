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

// DynamicExpression is available in net40+ via BCL, and in net20 via our polyfill.
// Since NUnit requires net35+, these tests run against the BCL implementation on net40+.
// They serve to verify expected behavior and document the API contract.
#if SUPPORTS_DYNAMIC

using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Dynamic")]
[Category("DynamicExpression")]
public class DynamicExpressionTests {

  #region Factory Methods

  [Test]
  [Category("HappyPath")]
  public void Expression_Dynamic_CreatesExpression() {
    var binder = new TestBinder();
    var arg = Expression.Constant("test", typeof(object));
    var expr = Expression.Dynamic(binder, typeof(object), arg);
    Assert.That(expr, Is.Not.Null);
    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Dynamic));
  }

  [Test]
  [Category("HappyPath")]
  public void Expression_Dynamic_HasCorrectBinder() {
    var binder = new TestBinder();
    var arg = Expression.Constant("test", typeof(object));
    var expr = Expression.Dynamic(binder, typeof(object), arg);
    Assert.That(expr.Binder, Is.SameAs(binder));
  }

  [Test]
  [Category("HappyPath")]
  public void Expression_Dynamic_HasCorrectReturnType() {
    var binder = new TestBinder();
    var arg = Expression.Constant("test", typeof(object));
    var expr = Expression.Dynamic(binder, typeof(int), arg);
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
  }

  [Test]
  [Category("HappyPath")]
  public void Expression_Dynamic_WithOneArg_HasCorrectArguments() {
    var binder = new TestBinder();
    var arg = Expression.Constant(42);
    var expr = Expression.Dynamic(binder, typeof(object), arg);
    Assert.That(expr.Arguments.Count, Is.EqualTo(1));
    Assert.That(expr.Arguments[0], Is.SameAs(arg));
  }

  [Test]
  [Category("HappyPath")]
  public void Expression_Dynamic_WithTwoArgs_HasCorrectArguments() {
    var binder = new TestBinder();
    var arg0 = Expression.Constant("Hello");
    var arg1 = Expression.Constant(123);
    var expr = Expression.Dynamic(binder, typeof(object), arg0, arg1);
    Assert.That(expr.Arguments.Count, Is.EqualTo(2));
    Assert.That(expr.Arguments[0], Is.SameAs(arg0));
    Assert.That(expr.Arguments[1], Is.SameAs(arg1));
  }

  [Test]
  [Category("HappyPath")]
  public void Expression_Dynamic_WithThreeArgs_HasCorrectArguments() {
    var binder = new TestBinder();
    var arg0 = Expression.Constant(1);
    var arg1 = Expression.Constant(2);
    var arg2 = Expression.Constant(3);
    var expr = Expression.Dynamic(binder, typeof(object), arg0, arg1, arg2);
    Assert.That(expr.Arguments.Count, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Expression_Dynamic_WithFourArgs_HasCorrectArguments() {
    var binder = new TestBinder();
    var arg0 = Expression.Constant(1);
    var arg1 = Expression.Constant(2);
    var arg2 = Expression.Constant(3);
    var arg3 = Expression.Constant(4);
    var expr = Expression.Dynamic(binder, typeof(object), arg0, arg1, arg2, arg3);
    Assert.That(expr.Arguments.Count, Is.EqualTo(4));
  }

  #endregion

  #region MakeDynamic

  [Test]
  [Category("HappyPath")]
  public void Expression_MakeDynamic_CreatesExpression() {
    var binder = new TestBinder();
    var delegateType = typeof(Func<CallSite, object, object>);
    var expr = Expression.MakeDynamic(delegateType, binder, Expression.Constant("test", typeof(object)));
    Assert.That(expr, Is.Not.Null);
    Assert.That(expr.DelegateType, Is.EqualTo(delegateType));
  }

  #endregion

  #region Properties

  [Test]
  [Category("HappyPath")]
  public void DynamicExpression_NodeType_IsDynamic() {
    var binder = new TestBinder();
    var arg = Expression.Constant("test", typeof(object));
    var expr = Expression.Dynamic(binder, typeof(object), arg);
    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Dynamic));
  }

  [Test]
  [Category("HappyPath")]
  public void DynamicExpression_DelegateType_IsNotNull() {
    var binder = new TestBinder();
    var arg = Expression.Constant("test", typeof(object));
    var expr = Expression.Dynamic(binder, typeof(object), arg);
    Assert.That(expr.DelegateType, Is.Not.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void DynamicExpression_Arguments_IsReadOnlyCollection() {
    var binder = new TestBinder();
    var expr = Expression.Dynamic(binder, typeof(object), Expression.Constant(1));
    Assert.That(expr.Arguments, Is.InstanceOf<ReadOnlyCollection<Expression>>());
  }

  #endregion

  #region Update

  [Test]
  [Category("HappyPath")]
  public void DynamicExpression_Update_SameArgs_PreservesBinder() {
    var binder = new TestBinder();
    var arg = Expression.Constant("test", typeof(object));
    var expr = Expression.Dynamic(binder, typeof(object), arg);
    var updated = expr.Update(new[] { arg });
    // BCL may or may not return same instance, but binder should be preserved
    Assert.That(updated.Binder, Is.SameAs(binder));
    Assert.That(updated.Arguments.Count, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void DynamicExpression_Update_DifferentArgs_ReturnsNewInstance() {
    var binder = new TestBinder();
    var arg = Expression.Constant("test", typeof(object));
    var expr = Expression.Dynamic(binder, typeof(object), arg);
    var newArg = Expression.Constant("updated", typeof(object));
    var updated = expr.Update(new[] { newArg });
    Assert.That(updated, Is.Not.SameAs(expr));
    Assert.That(updated.Arguments[0], Is.SameAs(newArg));
  }

  #endregion

  #region Exception Cases

  [Test]
  [Category("Exception")]
  public void Expression_Dynamic_NullBinder_ThrowsArgumentNullException() {
    var arg = Expression.Constant("test", typeof(object));
    Assert.Throws<ArgumentNullException>(() => Expression.Dynamic(null, typeof(object), arg));
  }

  [Test]
  [Category("Exception")]
  public void Expression_Dynamic_NullReturnType_ThrowsArgumentNullException() {
    var binder = new TestBinder();
    var arg = Expression.Constant("test", typeof(object));
    Assert.Throws<ArgumentNullException>(() => Expression.Dynamic(binder, null, arg));
  }

  #endregion

  #region ExpressionVisitor Integration

  [Test]
  [Category("HappyPath")]
  public void DynamicExpression_AcceptVisitor_TraversesExpression() {
    var binder = new TestBinder();
    var expr = Expression.Dynamic(binder, typeof(object), Expression.Constant(42));
    var visitor = new CountingVisitor();
    visitor.Visit(expr);
    // Visitor traverses the expression - the constant argument should be visited
    // Note: VisitDynamic may or may not be called depending on runtime version
    Assert.That(visitor.ConstantCount, Is.GreaterThanOrEqualTo(1));
  }

  #endregion

  #region Helper Classes

  private class TestBinder : CallSiteBinder {
    public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) =>
      Expression.Return(returnLabel, Expression.Constant(args.Length > 0 ? args[0] : null), typeof(object));
  }

  private class CountingVisitor : ExpressionVisitor {
    public int DynamicCount { get; private set; }
    public int ConstantCount { get; private set; }

    protected override Expression VisitDynamic(DynamicExpression node) {
      ++DynamicCount;
      return base.VisitDynamic(node);
    }

    protected override Expression VisitConstant(ConstantExpression node) {
      ++ConstantCount;
      return base.VisitConstant(node);
    }
  }

  #endregion

}

#endif
