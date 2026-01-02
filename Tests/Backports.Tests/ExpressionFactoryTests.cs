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
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Expressions")]
public class ExpressionFactoryTests {

  #region Constant

  [Test]
  public void Constant_WithValue_ReturnsConstantExpression() {
    var expr = Expression.Constant(42);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Constant));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
    Assert.That(expr.Value, Is.EqualTo(42));
  }

  [Test]
  public void Constant_WithNullValue_ReturnsConstantExpressionWithObjectType() {
    var expr = Expression.Constant(null);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Constant));
    Assert.That(expr.Type, Is.EqualTo(typeof(object)));
    Assert.That(expr.Value, Is.Null);
  }

  [Test]
  public void Constant_WithValueAndExplicitType_ReturnsConstantExpressionWithSpecifiedType() {
    var expr = Expression.Constant(42, typeof(object));

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Constant));
    Assert.That(expr.Type, Is.EqualTo(typeof(object)));
    Assert.That(expr.Value, Is.EqualTo(42));
  }

  [Test]
  public void Constant_WithString_ReturnsConstantExpression() {
    var expr = Expression.Constant("hello");

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Constant));
    Assert.That(expr.Type, Is.EqualTo(typeof(string)));
    Assert.That(expr.Value, Is.EqualTo("hello"));
  }

  #endregion

  #region Parameter

  [Test]
  public void Parameter_WithType_ReturnsParameterExpression() {
    var expr = Expression.Parameter(typeof(int));

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Parameter));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
    Assert.That(expr.Name, Is.Null);
  }

  [Test]
  public void Parameter_WithTypeAndName_ReturnsParameterExpressionWithName() {
    var expr = Expression.Parameter(typeof(string), "x");

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Parameter));
    Assert.That(expr.Type, Is.EqualTo(typeof(string)));
    Assert.That(expr.Name, Is.EqualTo("x"));
  }

  #endregion

  #region Binary Arithmetic

  [Test]
  public void Add_WithTwoIntegers_ReturnsAddBinaryExpression() {
    var left = Expression.Constant(1);
    var right = Expression.Constant(2);

    var expr = Expression.Add(left, right);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Add));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
    Assert.That(expr.Left, Is.SameAs(left));
    Assert.That(expr.Right, Is.SameAs(right));
  }

  [Test]
  public void AddChecked_WithTwoIntegers_ReturnsAddCheckedBinaryExpression() {
    var left = Expression.Constant(1);
    var right = Expression.Constant(2);

    var expr = Expression.AddChecked(left, right);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.AddChecked));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
  }

  [Test]
  public void Subtract_WithTwoIntegers_ReturnsSubtractBinaryExpression() {
    var left = Expression.Constant(5);
    var right = Expression.Constant(3);

    var expr = Expression.Subtract(left, right);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Subtract));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
  }

  [Test]
  public void SubtractChecked_WithTwoIntegers_ReturnsSubtractCheckedBinaryExpression() {
    var left = Expression.Constant(5);
    var right = Expression.Constant(3);

    var expr = Expression.SubtractChecked(left, right);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.SubtractChecked));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
  }

  [Test]
  public void Multiply_WithTwoIntegers_ReturnsMultiplyBinaryExpression() {
    var left = Expression.Constant(4);
    var right = Expression.Constant(5);

    var expr = Expression.Multiply(left, right);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Multiply));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
  }

  [Test]
  public void MultiplyChecked_WithTwoIntegers_ReturnsMultiplyCheckedBinaryExpression() {
    var left = Expression.Constant(4);
    var right = Expression.Constant(5);

    var expr = Expression.MultiplyChecked(left, right);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.MultiplyChecked));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
  }

  [Test]
  public void Divide_WithTwoIntegers_ReturnsDivideBinaryExpression() {
    var left = Expression.Constant(10);
    var right = Expression.Constant(2);

    var expr = Expression.Divide(left, right);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Divide));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
  }

  [Test]
  public void Modulo_WithTwoIntegers_ReturnsModuloBinaryExpression() {
    var left = Expression.Constant(10);
    var right = Expression.Constant(3);

    var expr = Expression.Modulo(left, right);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Modulo));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
  }

  [Test]
  public void Power_WithTwoDoubles_ReturnsPowerBinaryExpression() {
    var left = Expression.Constant(2.0);
    var right = Expression.Constant(3.0);

    var expr = Expression.Power(left, right);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Power));
    Assert.That(expr.Type, Is.EqualTo(typeof(double)));
  }

  #endregion

  #region Binary Comparison

  [Test]
  public void Equal_WithTwoIntegers_ReturnsEqualBinaryExpression() {
    var left = Expression.Constant(1);
    var right = Expression.Constant(1);

    var expr = Expression.Equal(left, right);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Equal));
    Assert.That(expr.Type, Is.EqualTo(typeof(bool)));
  }

  [Test]
  public void NotEqual_WithTwoIntegers_ReturnsNotEqualBinaryExpression() {
    var left = Expression.Constant(1);
    var right = Expression.Constant(2);

    var expr = Expression.NotEqual(left, right);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.NotEqual));
    Assert.That(expr.Type, Is.EqualTo(typeof(bool)));
  }

  [Test]
  public void LessThan_WithTwoIntegers_ReturnsLessThanBinaryExpression() {
    var left = Expression.Constant(1);
    var right = Expression.Constant(2);

    var expr = Expression.LessThan(left, right);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.LessThan));
    Assert.That(expr.Type, Is.EqualTo(typeof(bool)));
  }

  [Test]
  public void LessThanOrEqual_WithTwoIntegers_ReturnsLessThanOrEqualBinaryExpression() {
    var left = Expression.Constant(1);
    var right = Expression.Constant(2);

    var expr = Expression.LessThanOrEqual(left, right);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.LessThanOrEqual));
    Assert.That(expr.Type, Is.EqualTo(typeof(bool)));
  }

  [Test]
  public void GreaterThan_WithTwoIntegers_ReturnsGreaterThanBinaryExpression() {
    var left = Expression.Constant(2);
    var right = Expression.Constant(1);

    var expr = Expression.GreaterThan(left, right);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.GreaterThan));
    Assert.That(expr.Type, Is.EqualTo(typeof(bool)));
  }

  [Test]
  public void GreaterThanOrEqual_WithTwoIntegers_ReturnsGreaterThanOrEqualBinaryExpression() {
    var left = Expression.Constant(2);
    var right = Expression.Constant(1);

    var expr = Expression.GreaterThanOrEqual(left, right);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.GreaterThanOrEqual));
    Assert.That(expr.Type, Is.EqualTo(typeof(bool)));
  }

  #endregion

  #region Binary Logical

  [Test]
  public void And_WithTwoBooleans_ReturnsAndBinaryExpression() {
    var left = Expression.Constant(true);
    var right = Expression.Constant(false);

    var expr = Expression.And(left, right);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.And));
    Assert.That(expr.Type, Is.EqualTo(typeof(bool)));
  }

  [Test]
  public void AndAlso_WithTwoBooleans_ReturnsAndAlsoBinaryExpression() {
    var left = Expression.Constant(true);
    var right = Expression.Constant(false);

    var expr = Expression.AndAlso(left, right);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.AndAlso));
    Assert.That(expr.Type, Is.EqualTo(typeof(bool)));
  }

  [Test]
  public void Or_WithTwoBooleans_ReturnsOrBinaryExpression() {
    var left = Expression.Constant(true);
    var right = Expression.Constant(false);

    var expr = Expression.Or(left, right);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Or));
    Assert.That(expr.Type, Is.EqualTo(typeof(bool)));
  }

  [Test]
  public void OrElse_WithTwoBooleans_ReturnsOrElseBinaryExpression() {
    var left = Expression.Constant(true);
    var right = Expression.Constant(false);

    var expr = Expression.OrElse(left, right);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.OrElse));
    Assert.That(expr.Type, Is.EqualTo(typeof(bool)));
  }

  [Test]
  public void ExclusiveOr_WithTwoBooleans_ReturnsExclusiveOrBinaryExpression() {
    var left = Expression.Constant(true);
    var right = Expression.Constant(false);

    var expr = Expression.ExclusiveOr(left, right);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.ExclusiveOr));
    Assert.That(expr.Type, Is.EqualTo(typeof(bool)));
  }

  #endregion

  #region Binary Bitwise

  [Test]
  public void LeftShift_WithTwoIntegers_ReturnsLeftShiftBinaryExpression() {
    var left = Expression.Constant(1);
    var right = Expression.Constant(2);

    var expr = Expression.LeftShift(left, right);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.LeftShift));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
  }

  [Test]
  public void RightShift_WithTwoIntegers_ReturnsRightShiftBinaryExpression() {
    var left = Expression.Constant(8);
    var right = Expression.Constant(2);

    var expr = Expression.RightShift(left, right);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.RightShift));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
  }

  [Test]
  public void And_WithTwoIntegers_ReturnsAndBinaryExpression() {
    var left = Expression.Constant(0b1100);
    var right = Expression.Constant(0b1010);

    var expr = Expression.And(left, right);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.And));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
  }

  [Test]
  public void Or_WithTwoIntegers_ReturnsOrBinaryExpression() {
    var left = Expression.Constant(0b1100);
    var right = Expression.Constant(0b1010);

    var expr = Expression.Or(left, right);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Or));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
  }

  [Test]
  public void ExclusiveOr_WithTwoIntegers_ReturnsExclusiveOrBinaryExpression() {
    var left = Expression.Constant(0b1100);
    var right = Expression.Constant(0b1010);

    var expr = Expression.ExclusiveOr(left, right);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.ExclusiveOr));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
  }

  #endregion

  #region Binary Coalesce and ArrayIndex

  [Test]
  public void Coalesce_WithTwoNullables_ReturnsCoalesceBinaryExpression() {
    var left = Expression.Constant(null, typeof(int?));
    var right = Expression.Constant(42, typeof(int?));

    var expr = Expression.Coalesce(left, right);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Coalesce));
    Assert.That(expr.Type, Is.EqualTo(typeof(int?)));
  }

  [Test]
  public void ArrayIndex_WithArrayAndIndex_ReturnsArrayIndexBinaryExpression() {
    var array = Expression.Constant(new[] { 1, 2, 3 });
    var index = Expression.Constant(1);

    var expr = Expression.ArrayIndex(array, index);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.ArrayIndex));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
  }

  #endregion

  #region Unary

  [Test]
  public void Negate_WithInteger_ReturnsNegateUnaryExpression() {
    var operand = Expression.Constant(5);

    var expr = Expression.Negate(operand);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Negate));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
    Assert.That(expr.Operand, Is.SameAs(operand));
  }

  [Test]
  public void NegateChecked_WithInteger_ReturnsNegateCheckedUnaryExpression() {
    var operand = Expression.Constant(5);

    var expr = Expression.NegateChecked(operand);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.NegateChecked));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
  }

  [Test]
  public void UnaryPlus_WithInteger_ReturnsUnaryPlusExpression() {
    var operand = Expression.Constant(5);

    var expr = Expression.UnaryPlus(operand);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.UnaryPlus));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
  }

  [Test]
  public void Not_WithBoolean_ReturnsNotUnaryExpression() {
    var operand = Expression.Constant(true);

    var expr = Expression.Not(operand);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Not));
    Assert.That(expr.Type, Is.EqualTo(typeof(bool)));
  }

  [Test]
  public void Not_WithInteger_ReturnsNotUnaryExpressionForBitwise() {
    var operand = Expression.Constant(0b1010);

    var expr = Expression.Not(operand);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Not));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
  }

  [Test]
  public void Convert_ToAnotherType_ReturnsConvertUnaryExpression() {
    var operand = Expression.Constant(42);

    var expr = Expression.Convert(operand, typeof(double));

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Convert));
    Assert.That(expr.Type, Is.EqualTo(typeof(double)));
  }

  [Test]
  public void ConvertChecked_ToAnotherType_ReturnsConvertCheckedUnaryExpression() {
    var operand = Expression.Constant(42);

    var expr = Expression.ConvertChecked(operand, typeof(short));

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.ConvertChecked));
    Assert.That(expr.Type, Is.EqualTo(typeof(short)));
  }

  [Test]
  public void TypeAs_WithReferenceType_ReturnsTypeAsUnaryExpression() {
    var operand = Expression.Constant("hello", typeof(object));

    var expr = Expression.TypeAs(operand, typeof(string));

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.TypeAs));
    Assert.That(expr.Type, Is.EqualTo(typeof(string)));
  }

  [Test]
  public void ArrayLength_WithArray_ReturnsArrayLengthUnaryExpression() {
    var array = Expression.Constant(new[] { 1, 2, 3 });

    var expr = Expression.ArrayLength(array);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.ArrayLength));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
  }

  [Test]
  public void Quote_WithLambda_ReturnsQuoteUnaryExpression() {
    Expression<Func<int, int>> innerLambda = x => x + 1;

    var expr = Expression.Quote(innerLambda);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Quote));
    Assert.That(expr.Type, Is.EqualTo(typeof(Expression<Func<int, int>>)));
  }

  #endregion

  #region Conditional

  [Test]
  public void Condition_WithThreeExpressions_ReturnsConditionalExpression() {
    var test = Expression.Constant(true);
    var ifTrue = Expression.Constant(1);
    var ifFalse = Expression.Constant(2);

    var expr = Expression.Condition(test, ifTrue, ifFalse);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Conditional));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
    Assert.That(expr.Test, Is.SameAs(test));
    Assert.That(expr.IfTrue, Is.SameAs(ifTrue));
    Assert.That(expr.IfFalse, Is.SameAs(ifFalse));
  }

  [Test]
  public void Condition_WithExplicitType_ReturnsConditionalExpressionWithType() {
    var test = Expression.Constant(true);
    var ifTrue = Expression.Constant("hello");
    var ifFalse = Expression.Constant("world");

    var expr = Expression.Condition(test, ifTrue, ifFalse, typeof(object));

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Conditional));
    Assert.That(expr.Type, Is.EqualTo(typeof(object)));
  }

  #endregion

  #region TypeBinary

  [Test]
  public void TypeIs_WithExpression_ReturnsTypeBinaryExpression() {
    var operand = Expression.Constant("hello");

    var expr = Expression.TypeIs(operand, typeof(string));

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.TypeIs));
    Assert.That(expr.Type, Is.EqualTo(typeof(bool)));
    Assert.That(expr.Expression, Is.SameAs(operand));
    Assert.That(expr.TypeOperand, Is.EqualTo(typeof(string)));
  }

  [Test]
  public void TypeEqual_WithExpression_ReturnsTypeBinaryExpression() {
    var operand = Expression.Constant("hello");

    var expr = Expression.TypeEqual(operand, typeof(string));

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.TypeEqual));
    Assert.That(expr.Type, Is.EqualTo(typeof(bool)));
    Assert.That(expr.TypeOperand, Is.EqualTo(typeof(string)));
  }

  #endregion

  #region Lambda

  [Test]
  public void Lambda_WithBodyAndNoParameters_ReturnsLambdaExpression() {
    var body = Expression.Constant(42);

    var expr = Expression.Lambda(body);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Lambda));
    Assert.That(expr.Type, Is.EqualTo(typeof(Func<int>)));
    Assert.That(expr.Body, Is.SameAs(body));
    Assert.That(expr.Parameters.Count, Is.EqualTo(0));
  }

  [Test]
  public void Lambda_WithBodyAndOneParameter_ReturnsLambdaExpression() {
    var param = Expression.Parameter(typeof(int), "x");
    var body = Expression.Add(param, Expression.Constant(1));

    var expr = Expression.Lambda(body, param);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Lambda));
    Assert.That(expr.Type, Is.EqualTo(typeof(Func<int, int>)));
    Assert.That(expr.Parameters.Count, Is.EqualTo(1));
    Assert.That(expr.Parameters[0], Is.SameAs(param));
  }

  [Test]
  public void Lambda_WithDelegateType_ReturnsTypedLambdaExpression() {
    var param = Expression.Parameter(typeof(int), "x");
    var body = Expression.Add(param, Expression.Constant(1));

    var expr = Expression.Lambda<Func<int, int>>(body, param);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Lambda));
    Assert.That(expr.Type, Is.EqualTo(typeof(Func<int, int>)));
  }

  [Test]
  public void Lambda_WithTwoParameters_ReturnsLambdaExpression() {
    var param1 = Expression.Parameter(typeof(int), "x");
    var param2 = Expression.Parameter(typeof(int), "y");
    var body = Expression.Add(param1, param2);

    var expr = Expression.Lambda(body, param1, param2);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Lambda));
    Assert.That(expr.Type, Is.EqualTo(typeof(Func<int, int, int>)));
    Assert.That(expr.Parameters.Count, Is.EqualTo(2));
  }

  [Test]
  public void Lambda_WithAction_ReturnsActionLambda() {
    var body = Expression.Default(typeof(void));

    var expr = Expression.Lambda<Action>(body);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Lambda));
    Assert.That(expr.Type, Is.EqualTo(typeof(Action)));
  }

  #endregion

  #region MethodCall

  [Test]
  public void Call_StaticMethod_ReturnsMethodCallExpression() {
    var method = typeof(int).GetMethod(nameof(int.Parse), new[] { typeof(string) })!;
    var arg = Expression.Constant("42");

    var expr = Expression.Call(method, arg);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Call));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
    Assert.That(expr.Object, Is.Null);
    Assert.That(expr.Method, Is.EqualTo(method));
  }

  [Test]
  public void Call_InstanceMethod_ReturnsMethodCallExpression() {
    var method = typeof(string).GetMethod(nameof(string.ToUpper), Type.EmptyTypes)!;
    var instance = Expression.Constant("hello");

    var expr = Expression.Call(instance, method);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Call));
    Assert.That(expr.Type, Is.EqualTo(typeof(string)));
    Assert.That(expr.Object, Is.SameAs(instance));
    Assert.That(expr.Method, Is.EqualTo(method));
  }

  [Test]
  public void Call_InstanceMethodWithArguments_ReturnsMethodCallExpression() {
    var method = typeof(string).GetMethod(nameof(string.Substring), new[] { typeof(int) })!;
    var instance = Expression.Constant("hello");
    var arg = Expression.Constant(1);

    var expr = Expression.Call(instance, method, arg);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Call));
    Assert.That(expr.Type, Is.EqualTo(typeof(string)));
    Assert.That(expr.Arguments.Count, Is.EqualTo(1));
  }

  [Test]
  public void Call_ByMethodName_ReturnsMethodCallExpression() {
    var instance = Expression.Constant("hello");

    var expr = Expression.Call(instance, "ToUpper", null);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Call));
    Assert.That(expr.Type, Is.EqualTo(typeof(string)));
  }

  #endregion

  #region MemberAccess

  [Test]
  public void PropertyOrField_WithPropertyName_ReturnsMemberExpression() {
    var instance = Expression.Constant("hello");

    var expr = Expression.PropertyOrField(instance, "Length");

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.MemberAccess));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
  }

  [Test]
  public void Property_WithPropertyInfo_ReturnsMemberExpression() {
    var property = typeof(string).GetProperty(nameof(string.Length))!;
    var instance = Expression.Constant("hello");

    var expr = Expression.Property(instance, property);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.MemberAccess));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
    Assert.That(expr.Member, Is.EqualTo(property));
  }

  [Test]
  public void Property_WithPropertyName_ReturnsMemberExpression() {
    var instance = Expression.Constant("hello");

    var expr = Expression.Property(instance, "Length");

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.MemberAccess));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
  }

  [Test]
  public void Field_WithFieldInfo_ReturnsMemberExpression() {
    var field = typeof(TestClass).GetField(nameof(TestClass.PublicField))!;
    var instance = Expression.Constant(new TestClass());

    var expr = Expression.Field(instance, field);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.MemberAccess));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
    Assert.That(expr.Member, Is.EqualTo(field));
  }

  [Test]
  public void Field_WithFieldName_ReturnsMemberExpression() {
    var instance = Expression.Constant(new TestClass());

    var expr = Expression.Field(instance, "PublicField");

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.MemberAccess));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
  }

  [Test]
  public void Field_StaticField_ReturnsMemberExpression() {
    var field = typeof(TestClass).GetField(nameof(TestClass.StaticField))!;

    var expr = Expression.Field(null, field);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.MemberAccess));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
    Assert.That(expr.Expression, Is.Null);
  }

  #endregion

  #region New

  [Test]
  public void New_WithConstructor_ReturnsNewExpression() {
    var ctor = typeof(object).GetConstructor(Type.EmptyTypes)!;

    var expr = Expression.New(ctor);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.New));
    Assert.That(expr.Type, Is.EqualTo(typeof(object)));
    Assert.That(expr.Constructor, Is.EqualTo(ctor));
  }

  [Test]
  public void New_WithConstructorAndArguments_ReturnsNewExpression() {
    var ctor = typeof(string).GetConstructor(new[] { typeof(char), typeof(int) })!;
    var arg1 = Expression.Constant('a');
    var arg2 = Expression.Constant(5);

    var expr = Expression.New(ctor, arg1, arg2);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.New));
    Assert.That(expr.Type, Is.EqualTo(typeof(string)));
    Assert.That(expr.Arguments.Count, Is.EqualTo(2));
  }

  [Test]
  public void New_WithType_ReturnsNewExpressionWithDefaultConstructor() {
    var expr = Expression.New(typeof(object));

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.New));
    Assert.That(expr.Type, Is.EqualTo(typeof(object)));
  }

  #endregion

  #region NewArray

  [Test]
  public void NewArrayInit_WithElements_ReturnsNewArrayExpression() {
    var elements = new[] {
      Expression.Constant(1),
      Expression.Constant(2),
      Expression.Constant(3)
    };

    var expr = Expression.NewArrayInit(typeof(int), elements);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.NewArrayInit));
    Assert.That(expr.Type, Is.EqualTo(typeof(int[])));
    Assert.That(expr.Expressions.Count, Is.EqualTo(3));
  }

  [Test]
  public void NewArrayBounds_WithBounds_ReturnsNewArrayExpression() {
    var length = Expression.Constant(5);

    var expr = Expression.NewArrayBounds(typeof(int), length);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.NewArrayBounds));
    Assert.That(expr.Type, Is.EqualTo(typeof(int[])));
    Assert.That(expr.Expressions.Count, Is.EqualTo(1));
  }

  [Test]
  public void NewArrayBounds_MultiDimensional_ReturnsNewArrayExpression() {
    var dim1 = Expression.Constant(2);
    var dim2 = Expression.Constant(3);

    var expr = Expression.NewArrayBounds(typeof(int), dim1, dim2);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.NewArrayBounds));
    Assert.That(expr.Type, Is.EqualTo(typeof(int[,])));
    Assert.That(expr.Expressions.Count, Is.EqualTo(2));
  }

  #endregion

  #region Invoke

  [Test]
  public void Invoke_WithDelegate_ReturnsInvocationExpression() {
    Expression<Func<int, int>> lambda = x => x + 1;
    var arg = Expression.Constant(5);

    var expr = Expression.Invoke(lambda, arg);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Invoke));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
    Assert.That(expr.Arguments.Count, Is.EqualTo(1));
  }

  [Test]
  public void Invoke_WithNoArguments_ReturnsInvocationExpression() {
    Expression<Func<int>> lambda = () => 42;

    var expr = Expression.Invoke(lambda);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Invoke));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
    Assert.That(expr.Arguments.Count, Is.EqualTo(0));
  }

  #endregion

  #region MakeBinary and MakeUnary

  [Test]
  public void MakeBinary_WithValidType_ReturnsBinaryExpression() {
    var left = Expression.Constant(1);
    var right = Expression.Constant(2);

    var expr = Expression.MakeBinary(ExpressionType.Add, left, right);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Add));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
  }

  [Test]
  public void MakeUnary_WithValidType_ReturnsUnaryExpression() {
    var operand = Expression.Constant(5);

    var expr = Expression.MakeUnary(ExpressionType.Negate, operand, null);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Negate));
    Assert.That(expr.Type, Is.EqualTo(typeof(int)));
  }

  #endregion

  #region ReferenceEqual and ReferenceNotEqual

  [Test]
  public void ReferenceEqual_WithTwoObjects_ReturnsEqualBinaryExpression() {
    var left = Expression.Constant("hello", typeof(object));
    var right = Expression.Constant("hello", typeof(object));

    var expr = Expression.ReferenceEqual(left, right);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.Equal));
    Assert.That(expr.Type, Is.EqualTo(typeof(bool)));
  }

  [Test]
  public void ReferenceNotEqual_WithTwoObjects_ReturnsNotEqualBinaryExpression() {
    var left = Expression.Constant("hello", typeof(object));
    var right = Expression.Constant("world", typeof(object));

    var expr = Expression.ReferenceNotEqual(left, right);

    Assert.That(expr.NodeType, Is.EqualTo(ExpressionType.NotEqual));
    Assert.That(expr.Type, Is.EqualTo(typeof(bool)));
  }

  #endregion

  #region Helper Types

  private class TestClass {
    public int PublicField = 42;
    public static int StaticField = 100;
    public int PublicProperty { get; set; } = 10;
  }

  #endregion

}
