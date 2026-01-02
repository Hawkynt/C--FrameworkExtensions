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

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Expressions")]
public class ExpressionVisitorTests {

  #region Visit All Node Types

  [Test]
  public void Visit_ConstantExpression_CallsVisitConstant() {
    var visitor = new TrackingVisitor();
    var expr = Expression.Constant(42);

    visitor.Visit(expr);

    Assert.That(visitor.VisitedTypes, Does.Contain(ExpressionType.Constant));
  }

  [Test]
  public void Visit_ParameterExpression_CallsVisitParameter() {
    var visitor = new TrackingVisitor();
    var expr = Expression.Parameter(typeof(int), "x");

    visitor.Visit(expr);

    Assert.That(visitor.VisitedTypes, Does.Contain(ExpressionType.Parameter));
  }

  [Test]
  public void Visit_BinaryExpression_CallsVisitBinary() {
    var visitor = new TrackingVisitor();
    var left = Expression.Constant(1);
    var right = Expression.Constant(2);
    var expr = Expression.Add(left, right);

    visitor.Visit(expr);

    Assert.That(visitor.VisitedTypes, Does.Contain(ExpressionType.Add));
  }

  [Test]
  public void Visit_UnaryExpression_CallsVisitUnary() {
    var visitor = new TrackingVisitor();
    var operand = Expression.Constant(5);
    var expr = Expression.Negate(operand);

    visitor.Visit(expr);

    Assert.That(visitor.VisitedTypes, Does.Contain(ExpressionType.Negate));
  }

  [Test]
  public void Visit_ConditionalExpression_CallsVisitConditional() {
    var visitor = new TrackingVisitor();
    var expr = Expression.Condition(
      Expression.Constant(true),
      Expression.Constant(1),
      Expression.Constant(2)
    );

    visitor.Visit(expr);

    Assert.That(visitor.VisitedTypes, Does.Contain(ExpressionType.Conditional));
  }

  [Test]
  public void Visit_LambdaExpression_CallsVisitLambda() {
    var visitor = new TrackingVisitor();
    var param = Expression.Parameter(typeof(int), "x");
    var expr = Expression.Lambda(param, param);

    visitor.Visit(expr);

    Assert.That(visitor.VisitedTypes, Does.Contain(ExpressionType.Lambda));
  }

  [Test]
  public void Visit_MethodCallExpression_CallsVisitMethodCall() {
    var visitor = new TrackingVisitor();
    var method = typeof(string).GetMethod(nameof(string.ToUpper), Type.EmptyTypes)!;
    var expr = Expression.Call(Expression.Constant("hello"), method);

    visitor.Visit(expr);

    Assert.That(visitor.VisitedTypes, Does.Contain(ExpressionType.Call));
  }

  [Test]
  public void Visit_MemberExpression_CallsVisitMember() {
    var visitor = new TrackingVisitor();
    var expr = Expression.Property(Expression.Constant("hello"), "Length");

    visitor.Visit(expr);

    Assert.That(visitor.VisitedTypes, Does.Contain(ExpressionType.MemberAccess));
  }

  [Test]
  public void Visit_NewExpression_CallsVisitNew() {
    var visitor = new TrackingVisitor();
    var expr = Expression.New(typeof(object));

    visitor.Visit(expr);

    Assert.That(visitor.VisitedTypes, Does.Contain(ExpressionType.New));
  }

  [Test]
  public void Visit_NewArrayExpression_CallsVisitNewArray() {
    var visitor = new TrackingVisitor();
    var expr = Expression.NewArrayInit(typeof(int), Expression.Constant(1), Expression.Constant(2));

    visitor.Visit(expr);

    Assert.That(visitor.VisitedTypes, Does.Contain(ExpressionType.NewArrayInit));
  }

  [Test]
  public void Visit_TypeBinaryExpression_CallsVisitTypeBinary() {
    var visitor = new TrackingVisitor();
    var expr = Expression.TypeIs(Expression.Constant("hello"), typeof(string));

    visitor.Visit(expr);

    Assert.That(visitor.VisitedTypes, Does.Contain(ExpressionType.TypeIs));
  }

  [Test]
  public void Visit_InvocationExpression_CallsVisitInvocation() {
    var visitor = new TrackingVisitor();
    Expression<Func<int, int>> lambda = x => x + 1;
    var expr = Expression.Invoke(lambda, Expression.Constant(5));

    visitor.Visit(expr);

    Assert.That(visitor.VisitedTypes, Does.Contain(ExpressionType.Invoke));
  }

  [Test]
  public void Visit_DefaultExpression_CallsVisitDefault() {
    var visitor = new TrackingVisitor();
    var expr = Expression.Default(typeof(int));

    visitor.Visit(expr);

    Assert.That(visitor.VisitedTypes, Does.Contain(ExpressionType.Default));
  }

  #endregion

  #region Recursive Traversal

  [Test]
  public void Visit_NestedExpression_VisitsAllNodes() {
    var visitor = new TrackingVisitor();
    var param = Expression.Parameter(typeof(int), "x");
    var expr = Expression.Lambda(
      Expression.Add(
        Expression.Multiply(param, Expression.Constant(2)),
        Expression.Constant(1)
      ),
      param
    );

    visitor.Visit(expr);

    Assert.That(visitor.VisitedTypes, Does.Contain(ExpressionType.Lambda));
    Assert.That(visitor.VisitedTypes, Does.Contain(ExpressionType.Add));
    Assert.That(visitor.VisitedTypes, Does.Contain(ExpressionType.Multiply));
    Assert.That(visitor.VisitedTypes, Does.Contain(ExpressionType.Parameter));
    Assert.That(visitor.VisitedTypes, Does.Contain(ExpressionType.Constant));
  }

  [Test]
  public void Visit_ComplexExpression_VisitsEachNodeOnce() {
    var visitor = new CountingVisitor();
    var param = Expression.Parameter(typeof(int), "x");
    var expr = Expression.Add(param, Expression.Constant(1));

    visitor.Visit(expr);

    Assert.That(visitor.VisitCount, Is.EqualTo(3));
  }

  #endregion

  #region Expression Transformation

  [Test]
  public void Visit_TransformConstant_ModifiesExpression() {
    var visitor = new ConstantMultiplierVisitor(2);
    var expr = Expression.Constant(5);

    var result = visitor.Visit(expr);

    Assert.That(result, Is.TypeOf<ConstantExpression>());
    Assert.That(((ConstantExpression)result!).Value, Is.EqualTo(10));
  }

  [Test]
  public void Visit_TransformBinaryOperands_ModifiesExpression() {
    var visitor = new ConstantMultiplierVisitor(2);
    var expr = Expression.Add(Expression.Constant(3), Expression.Constant(4));

    var result = visitor.Visit(expr);

    var compiled = Expression.Lambda<Func<int>>((BinaryExpression)result!).Compile();
    Assert.That(compiled(), Is.EqualTo(14));
  }

  [Test]
  public void Visit_ReplaceParameter_ModifiesExpression() {
    var originalParam = Expression.Parameter(typeof(int), "x");
    var newParam = Expression.Parameter(typeof(int), "y");
    var visitor = new ParameterReplacerVisitor(originalParam, newParam);

    var expr = Expression.Add(originalParam, Expression.Constant(1));
    var result = (BinaryExpression)visitor.Visit(expr)!;

    Assert.That(result.Left, Is.SameAs(newParam));
  }

  [Test]
  public void Visit_NoModification_ReturnsSameExpression() {
    var visitor = new IdentityVisitor();
    var expr = Expression.Add(Expression.Constant(1), Expression.Constant(2));

    var result = visitor.Visit(expr);

    Assert.That(result, Is.SameAs(expr));
  }

  #endregion

  #region Null Handling

  [Test]
  public void Visit_NullExpression_ReturnsNull() {
    var visitor = new TrackingVisitor();

    var result = visitor.Visit((Expression?)null);

    Assert.That(result, Is.Null);
  }

  #endregion

  #region Expression Collection Visiting

  [Test]
  public void Visit_MethodCallWithMultipleArgs_VisitsAllArguments() {
    var visitor = new CountingVisitor();
    var method = typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(int), typeof(int) })!;
    var expr = Expression.Call(method, Expression.Constant(1), Expression.Constant(2));

    visitor.Visit(expr);

    Assert.That(visitor.ConstantCount, Is.EqualTo(2));
  }

  [Test]
  public void Visit_NewArrayWithMultipleElements_VisitsAllElements() {
    var visitor = new CountingVisitor();
    var expr = Expression.NewArrayInit(typeof(int),
      Expression.Constant(1),
      Expression.Constant(2),
      Expression.Constant(3)
    );

    visitor.Visit(expr);

    Assert.That(visitor.ConstantCount, Is.EqualTo(3));
  }

  [Test]
  public void Visit_LambdaWithMultipleParameters_VisitsAllParameters() {
    var visitor = new CountingVisitor();
    var param1 = Expression.Parameter(typeof(int), "x");
    var param2 = Expression.Parameter(typeof(int), "y");
    var expr = Expression.Lambda(Expression.Add(param1, param2), param1, param2);

    visitor.Visit(expr);

    Assert.That(visitor.ParameterCount, Is.EqualTo(4));
  }

  #endregion

  #region VisitAndConvert

  [Test]
  public void VisitAndConvert_WithValidType_ReturnsConvertedExpression() {
    var visitor = new IdentityVisitor();
    var expr = Expression.Constant(42);

    var result = visitor.TestVisitAndConvert(expr, "test");

    Assert.That(result, Is.TypeOf<ConstantExpression>());
    Assert.That(result, Is.SameAs(expr));
  }

  #endregion

  #region Helper Visitors

  private class TrackingVisitor : ExpressionVisitor {
    public List<ExpressionType> VisitedTypes { get; } = new();

    public override Expression? Visit(Expression? node) {
      if (node != null)
        VisitedTypes.Add(node.NodeType);

      return base.Visit(node);
    }
  }

  private class CountingVisitor : ExpressionVisitor {
    public int VisitCount { get; private set; }
    public int ConstantCount { get; private set; }
    public int ParameterCount { get; private set; }

    public override Expression? Visit(Expression? node) {
      if (node != null)
        ++VisitCount;

      return base.Visit(node);
    }

    protected override Expression VisitConstant(ConstantExpression node) {
      ++ConstantCount;
      return base.VisitConstant(node);
    }

    protected override Expression VisitParameter(ParameterExpression node) {
      ++ParameterCount;
      return base.VisitParameter(node);
    }
  }

  private class ConstantMultiplierVisitor : ExpressionVisitor {
    private readonly int _multiplier;

    public ConstantMultiplierVisitor(int multiplier) => this._multiplier = multiplier;

    protected override Expression VisitConstant(ConstantExpression node) {
      if (node.Value is int intValue)
        return Expression.Constant(intValue * this._multiplier);

      return base.VisitConstant(node);
    }
  }

  private class ParameterReplacerVisitor : ExpressionVisitor {
    private readonly ParameterExpression _original;
    private readonly ParameterExpression _replacement;

    public ParameterReplacerVisitor(ParameterExpression original, ParameterExpression replacement) {
      this._original = original;
      this._replacement = replacement;
    }

    protected override Expression VisitParameter(ParameterExpression node) =>
      node == this._original ? this._replacement : base.VisitParameter(node);
  }

  private class IdentityVisitor : ExpressionVisitor {
    public T? TestVisitAndConvert<T>(T? node, string callerName) where T : Expression =>
      this.VisitAndConvert(node, callerName);
  }

  #endregion

}
