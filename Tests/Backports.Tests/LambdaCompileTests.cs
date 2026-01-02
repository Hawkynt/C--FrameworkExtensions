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
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Expressions")]
public class LambdaCompileTests {

  #region Constant Expressions

  [Test]
  public void Compile_ConstantExpression_ReturnsValue() {
    var expr = Expression.Lambda<Func<int>>(Expression.Constant(42));
    var func = expr.Compile();

    Assert.That(func(), Is.EqualTo(42));
  }

  [Test]
  public void Compile_NullConstant_ReturnsNull() {
    var expr = Expression.Lambda<Func<string?>>(Expression.Constant(null, typeof(string)));
    var func = expr.Compile();

    Assert.That(func(), Is.Null);
  }

  [Test]
  public void Compile_StringConstant_ReturnsString() {
    var expr = Expression.Lambda<Func<string>>(Expression.Constant("hello world"));
    var func = expr.Compile();

    Assert.That(func(), Is.EqualTo("hello world"));
  }

  #endregion

  #region Parameter Expressions

  [Test]
  public void Compile_SingleParameter_ReturnsParameterValue() {
    var param = Expression.Parameter(typeof(int), "x");
    var expr = Expression.Lambda<Func<int, int>>(param, param);
    var func = expr.Compile();

    Assert.That(func(42), Is.EqualTo(42));
  }

  [Test]
  public void Compile_TwoParameters_ReturnsSecondParameter() {
    var param1 = Expression.Parameter(typeof(int), "x");
    var param2 = Expression.Parameter(typeof(int), "y");
    var expr = Expression.Lambda<Func<int, int, int>>(param2, param1, param2);
    var func = expr.Compile();

    Assert.That(func(1, 42), Is.EqualTo(42));
  }

  #endregion

  #region Arithmetic Operations

  [Test]
  public void Compile_Add_ReturnsSum() {
    var param = Expression.Parameter(typeof(int), "x");
    var body = Expression.Add(param, Expression.Constant(10));
    var expr = Expression.Lambda<Func<int, int>>(body, param);
    var func = expr.Compile();

    Assert.That(func(5), Is.EqualTo(15));
  }

  [Test]
  public void Compile_Subtract_ReturnsDifference() {
    var param = Expression.Parameter(typeof(int), "x");
    var body = Expression.Subtract(param, Expression.Constant(3));
    var expr = Expression.Lambda<Func<int, int>>(body, param);
    var func = expr.Compile();

    Assert.That(func(10), Is.EqualTo(7));
  }

  [Test]
  public void Compile_Multiply_ReturnsProduct() {
    var param = Expression.Parameter(typeof(int), "x");
    var body = Expression.Multiply(param, Expression.Constant(3));
    var expr = Expression.Lambda<Func<int, int>>(body, param);
    var func = expr.Compile();

    Assert.That(func(4), Is.EqualTo(12));
  }

  [Test]
  public void Compile_Divide_ReturnsQuotient() {
    var param = Expression.Parameter(typeof(int), "x");
    var body = Expression.Divide(param, Expression.Constant(2));
    var expr = Expression.Lambda<Func<int, int>>(body, param);
    var func = expr.Compile();

    Assert.That(func(10), Is.EqualTo(5));
  }

  [Test]
  public void Compile_Modulo_ReturnsRemainder() {
    var param = Expression.Parameter(typeof(int), "x");
    var body = Expression.Modulo(param, Expression.Constant(3));
    var expr = Expression.Lambda<Func<int, int>>(body, param);
    var func = expr.Compile();

    Assert.That(func(10), Is.EqualTo(1));
  }

  [Test]
  public void Compile_Power_ReturnsPower() {
    var param = Expression.Parameter(typeof(double), "x");
    var body = Expression.Power(param, Expression.Constant(2.0));
    var expr = Expression.Lambda<Func<double, double>>(body, param);
    var func = expr.Compile();

    Assert.That(func(3.0), Is.EqualTo(9.0));
  }

  [Test]
  public void Compile_Negate_ReturnsNegatedValue() {
    var param = Expression.Parameter(typeof(int), "x");
    var body = Expression.Negate(param);
    var expr = Expression.Lambda<Func<int, int>>(body, param);
    var func = expr.Compile();

    Assert.That(func(5), Is.EqualTo(-5));
  }

  [Test]
  public void Compile_UnaryPlus_ReturnsValue() {
    var param = Expression.Parameter(typeof(int), "x");
    var body = Expression.UnaryPlus(param);
    var expr = Expression.Lambda<Func<int, int>>(body, param);
    var func = expr.Compile();

    Assert.That(func(-5), Is.EqualTo(-5));
  }

  #endregion

  #region Comparison Operations

  [Test]
  public void Compile_Equal_ReturnsTrue_WhenEqual() {
    var param = Expression.Parameter(typeof(int), "x");
    var body = Expression.Equal(param, Expression.Constant(5));
    var expr = Expression.Lambda<Func<int, bool>>(body, param);
    var func = expr.Compile();

    Assert.That(func(5), Is.True);
    Assert.That(func(6), Is.False);
  }

  [Test]
  public void Compile_NotEqual_ReturnsTrue_WhenNotEqual() {
    var param = Expression.Parameter(typeof(int), "x");
    var body = Expression.NotEqual(param, Expression.Constant(5));
    var expr = Expression.Lambda<Func<int, bool>>(body, param);
    var func = expr.Compile();

    Assert.That(func(6), Is.True);
    Assert.That(func(5), Is.False);
  }

  [Test]
  public void Compile_LessThan_ReturnsCorrectResult() {
    var param = Expression.Parameter(typeof(int), "x");
    var body = Expression.LessThan(param, Expression.Constant(5));
    var expr = Expression.Lambda<Func<int, bool>>(body, param);
    var func = expr.Compile();

    Assert.That(func(3), Is.True);
    Assert.That(func(5), Is.False);
    Assert.That(func(7), Is.False);
  }

  [Test]
  public void Compile_LessThanOrEqual_ReturnsCorrectResult() {
    var param = Expression.Parameter(typeof(int), "x");
    var body = Expression.LessThanOrEqual(param, Expression.Constant(5));
    var expr = Expression.Lambda<Func<int, bool>>(body, param);
    var func = expr.Compile();

    Assert.That(func(3), Is.True);
    Assert.That(func(5), Is.True);
    Assert.That(func(7), Is.False);
  }

  [Test]
  public void Compile_GreaterThan_ReturnsCorrectResult() {
    var param = Expression.Parameter(typeof(int), "x");
    var body = Expression.GreaterThan(param, Expression.Constant(5));
    var expr = Expression.Lambda<Func<int, bool>>(body, param);
    var func = expr.Compile();

    Assert.That(func(7), Is.True);
    Assert.That(func(5), Is.False);
    Assert.That(func(3), Is.False);
  }

  [Test]
  public void Compile_GreaterThanOrEqual_ReturnsCorrectResult() {
    var param = Expression.Parameter(typeof(int), "x");
    var body = Expression.GreaterThanOrEqual(param, Expression.Constant(5));
    var expr = Expression.Lambda<Func<int, bool>>(body, param);
    var func = expr.Compile();

    Assert.That(func(7), Is.True);
    Assert.That(func(5), Is.True);
    Assert.That(func(3), Is.False);
  }

  #endregion

  #region Logical Operations

  [Test]
  public void Compile_Not_InvertsBoolean() {
    var param = Expression.Parameter(typeof(bool), "x");
    var body = Expression.Not(param);
    var expr = Expression.Lambda<Func<bool, bool>>(body, param);
    var func = expr.Compile();

    Assert.That(func(true), Is.False);
    Assert.That(func(false), Is.True);
  }

  [Test]
  public void Compile_And_ReturnsLogicalAnd() {
    var param1 = Expression.Parameter(typeof(bool), "x");
    var param2 = Expression.Parameter(typeof(bool), "y");
    var body = Expression.And(param1, param2);
    var expr = Expression.Lambda<Func<bool, bool, bool>>(body, param1, param2);
    var func = expr.Compile();

    Assert.That(func(true, true), Is.True);
    Assert.That(func(true, false), Is.False);
    Assert.That(func(false, true), Is.False);
    Assert.That(func(false, false), Is.False);
  }

  [Test]
  public void Compile_Or_ReturnsLogicalOr() {
    var param1 = Expression.Parameter(typeof(bool), "x");
    var param2 = Expression.Parameter(typeof(bool), "y");
    var body = Expression.Or(param1, param2);
    var expr = Expression.Lambda<Func<bool, bool, bool>>(body, param1, param2);
    var func = expr.Compile();

    Assert.That(func(true, true), Is.True);
    Assert.That(func(true, false), Is.True);
    Assert.That(func(false, true), Is.True);
    Assert.That(func(false, false), Is.False);
  }

  [Test]
  public void Compile_ExclusiveOr_ReturnsXor() {
    var param1 = Expression.Parameter(typeof(bool), "x");
    var param2 = Expression.Parameter(typeof(bool), "y");
    var body = Expression.ExclusiveOr(param1, param2);
    var expr = Expression.Lambda<Func<bool, bool, bool>>(body, param1, param2);
    var func = expr.Compile();

    Assert.That(func(true, true), Is.False);
    Assert.That(func(true, false), Is.True);
    Assert.That(func(false, true), Is.True);
    Assert.That(func(false, false), Is.False);
  }

  [Test]
  public void Compile_AndAlso_ShortCircuits() {
    var callCount = 0;
    Func<bool> sideEffect = () => { ++callCount; return true; };

    var param = Expression.Parameter(typeof(bool), "x");
    var methodCall = Expression.Call(Expression.Constant(sideEffect.Target), sideEffect.Method);
    var body = Expression.AndAlso(param, methodCall);
    var expr = Expression.Lambda<Func<bool, bool>>(body, param);
    var func = expr.Compile();

    callCount = 0;
    func(false);
    Assert.That(callCount, Is.EqualTo(0), "AndAlso should short-circuit when left is false");

    callCount = 0;
    func(true);
    Assert.That(callCount, Is.EqualTo(1), "AndAlso should evaluate right when left is true");
  }

  [Test]
  public void Compile_OrElse_ShortCircuits() {
    var callCount = 0;
    Func<bool> sideEffect = () => { ++callCount; return false; };

    var param = Expression.Parameter(typeof(bool), "x");
    var methodCall = Expression.Call(Expression.Constant(sideEffect.Target), sideEffect.Method);
    var body = Expression.OrElse(param, methodCall);
    var expr = Expression.Lambda<Func<bool, bool>>(body, param);
    var func = expr.Compile();

    callCount = 0;
    func(true);
    Assert.That(callCount, Is.EqualTo(0), "OrElse should short-circuit when left is true");

    callCount = 0;
    func(false);
    Assert.That(callCount, Is.EqualTo(1), "OrElse should evaluate right when left is false");
  }

  #endregion

  #region Bitwise Operations

  [Test]
  public void Compile_BitwiseAnd_ReturnsCorrectResult() {
    var param1 = Expression.Parameter(typeof(int), "x");
    var param2 = Expression.Parameter(typeof(int), "y");
    var body = Expression.And(param1, param2);
    var expr = Expression.Lambda<Func<int, int, int>>(body, param1, param2);
    var func = expr.Compile();

    Assert.That(func(0b1100, 0b1010), Is.EqualTo(0b1000));
  }

  [Test]
  public void Compile_BitwiseOr_ReturnsCorrectResult() {
    var param1 = Expression.Parameter(typeof(int), "x");
    var param2 = Expression.Parameter(typeof(int), "y");
    var body = Expression.Or(param1, param2);
    var expr = Expression.Lambda<Func<int, int, int>>(body, param1, param2);
    var func = expr.Compile();

    Assert.That(func(0b1100, 0b1010), Is.EqualTo(0b1110));
  }

  [Test]
  public void Compile_BitwiseXor_ReturnsCorrectResult() {
    var param1 = Expression.Parameter(typeof(int), "x");
    var param2 = Expression.Parameter(typeof(int), "y");
    var body = Expression.ExclusiveOr(param1, param2);
    var expr = Expression.Lambda<Func<int, int, int>>(body, param1, param2);
    var func = expr.Compile();

    Assert.That(func(0b1100, 0b1010), Is.EqualTo(0b0110));
  }

  [Test]
  public void Compile_BitwiseNot_ReturnsCorrectResult() {
    var param = Expression.Parameter(typeof(int), "x");
    var body = Expression.Not(param);
    var expr = Expression.Lambda<Func<int, int>>(body, param);
    var func = expr.Compile();

    Assert.That(func(0), Is.EqualTo(~0));
    Assert.That(func(0b1010), Is.EqualTo(~0b1010));
  }

  [Test]
  public void Compile_LeftShift_ReturnsCorrectResult() {
    var param = Expression.Parameter(typeof(int), "x");
    var body = Expression.LeftShift(param, Expression.Constant(2));
    var expr = Expression.Lambda<Func<int, int>>(body, param);
    var func = expr.Compile();

    Assert.That(func(1), Is.EqualTo(4));
    Assert.That(func(3), Is.EqualTo(12));
  }

  [Test]
  public void Compile_RightShift_ReturnsCorrectResult() {
    var param = Expression.Parameter(typeof(int), "x");
    var body = Expression.RightShift(param, Expression.Constant(2));
    var expr = Expression.Lambda<Func<int, int>>(body, param);
    var func = expr.Compile();

    Assert.That(func(8), Is.EqualTo(2));
    Assert.That(func(12), Is.EqualTo(3));
  }

  #endregion

  #region Conditional Expressions

  [Test]
  public void Compile_Condition_ReturnsIfTrueValue_WhenTestIsTrue() {
    var param = Expression.Parameter(typeof(bool), "cond");
    var body = Expression.Condition(param, Expression.Constant(1), Expression.Constant(2));
    var expr = Expression.Lambda<Func<bool, int>>(body, param);
    var func = expr.Compile();

    Assert.That(func(true), Is.EqualTo(1));
  }

  [Test]
  public void Compile_Condition_ReturnsIfFalseValue_WhenTestIsFalse() {
    var param = Expression.Parameter(typeof(bool), "cond");
    var body = Expression.Condition(param, Expression.Constant(1), Expression.Constant(2));
    var expr = Expression.Lambda<Func<bool, int>>(body, param);
    var func = expr.Compile();

    Assert.That(func(false), Is.EqualTo(2));
  }

  [Test]
  public void Compile_NestedCondition_EvaluatesCorrectly() {
    var param = Expression.Parameter(typeof(int), "x");
    var body = Expression.Condition(
      Expression.LessThan(param, Expression.Constant(0)),
      Expression.Constant("negative"),
      Expression.Condition(
        Expression.Equal(param, Expression.Constant(0)),
        Expression.Constant("zero"),
        Expression.Constant("positive")
      )
    );
    var expr = Expression.Lambda<Func<int, string>>(body, param);
    var func = expr.Compile();

    Assert.That(func(-1), Is.EqualTo("negative"));
    Assert.That(func(0), Is.EqualTo("zero"));
    Assert.That(func(1), Is.EqualTo("positive"));
  }

  #endregion

  #region Type Conversions

  [Test]
  public void Compile_Convert_IntToDouble() {
    var param = Expression.Parameter(typeof(int), "x");
    var body = Expression.Convert(param, typeof(double));
    var expr = Expression.Lambda<Func<int, double>>(body, param);
    var func = expr.Compile();

    Assert.That(func(42), Is.EqualTo(42.0));
  }

  [Test]
  public void Compile_Convert_DoubleToInt() {
    var param = Expression.Parameter(typeof(double), "x");
    var body = Expression.Convert(param, typeof(int));
    var expr = Expression.Lambda<Func<double, int>>(body, param);
    var func = expr.Compile();

    Assert.That(func(42.7), Is.EqualTo(42));
  }

  [Test]
  public void Compile_TypeAs_ReturnsValue_WhenTypeMatches() {
    var param = Expression.Parameter(typeof(object), "x");
    var body = Expression.TypeAs(param, typeof(string));
    var expr = Expression.Lambda<Func<object, string?>>(body, param);
    var func = expr.Compile();

    Assert.That(func("hello"), Is.EqualTo("hello"));
  }

  [Test]
  public void Compile_TypeAs_ReturnsNull_WhenTypeDoesNotMatch() {
    var param = Expression.Parameter(typeof(object), "x");
    var body = Expression.TypeAs(param, typeof(string));
    var expr = Expression.Lambda<Func<object, string?>>(body, param);
    var func = expr.Compile();

    Assert.That(func(42), Is.Null);
  }

  [Test]
  public void Compile_TypeIs_ReturnsTrue_WhenTypeMatches() {
    var param = Expression.Parameter(typeof(object), "x");
    var body = Expression.TypeIs(param, typeof(string));
    var expr = Expression.Lambda<Func<object, bool>>(body, param);
    var func = expr.Compile();

    Assert.That(func("hello"), Is.True);
    Assert.That(func(42), Is.False);
  }

  #endregion

  #region Method Calls

  [Test]
  public void Compile_StaticMethodCall_ReturnsResult() {
    var param = Expression.Parameter(typeof(string), "s");
    var method = typeof(int).GetMethod(nameof(int.Parse), new[] { typeof(string) })!;
    var body = Expression.Call(method, param);
    var expr = Expression.Lambda<Func<string, int>>(body, param);
    var func = expr.Compile();

    Assert.That(func("42"), Is.EqualTo(42));
  }

  [Test]
  public void Compile_InstanceMethodCall_ReturnsResult() {
    var param = Expression.Parameter(typeof(string), "s");
    var method = typeof(string).GetMethod(nameof(string.ToUpper), Type.EmptyTypes)!;
    var body = Expression.Call(param, method);
    var expr = Expression.Lambda<Func<string, string>>(body, param);
    var func = expr.Compile();

    Assert.That(func("hello"), Is.EqualTo("HELLO"));
  }

  [Test]
  public void Compile_InstanceMethodWithArgs_ReturnsResult() {
    var param = Expression.Parameter(typeof(string), "s");
    var method = typeof(string).GetMethod(nameof(string.Substring), new[] { typeof(int), typeof(int) })!;
    var body = Expression.Call(param, method, Expression.Constant(1), Expression.Constant(3));
    var expr = Expression.Lambda<Func<string, string>>(body, param);
    var func = expr.Compile();

    Assert.That(func("hello"), Is.EqualTo("ell"));
  }

  #endregion

  #region Member Access

  [Test]
  public void Compile_PropertyAccess_ReturnsPropertyValue() {
    var param = Expression.Parameter(typeof(string), "s");
    var property = typeof(string).GetProperty(nameof(string.Length))!;
    var body = Expression.Property(param, property);
    var expr = Expression.Lambda<Func<string, int>>(body, param);
    var func = expr.Compile();

    Assert.That(func("hello"), Is.EqualTo(5));
  }

  [Test]
  public void Compile_FieldAccess_ReturnsFieldValue() {
    var param = Expression.Parameter(typeof(TestClass), "obj");
    var field = typeof(TestClass).GetField(nameof(TestClass.Value))!;
    var body = Expression.Field(param, field);
    var expr = Expression.Lambda<Func<TestClass, int>>(body, param);
    var func = expr.Compile();

    var obj = new TestClass { Value = 42 };
    Assert.That(func(obj), Is.EqualTo(42));
  }

  [Test]
  public void Compile_StaticFieldAccess_ReturnsFieldValue() {
    TestClass.StaticValue = 100;
    var field = typeof(TestClass).GetField(nameof(TestClass.StaticValue))!;
    var body = Expression.Field(null, field);
    var expr = Expression.Lambda<Func<int>>(body);
    var func = expr.Compile();

    Assert.That(func(), Is.EqualTo(100));
  }

  #endregion

  #region Object and Array Creation

  [Test]
  public void Compile_NewObject_CreatesInstance() {
    var ctor = typeof(TestClass).GetConstructor(Type.EmptyTypes)!;
    var body = Expression.New(ctor);
    var expr = Expression.Lambda<Func<TestClass>>(body);
    var func = expr.Compile();

    var result = func();
    Assert.That(result, Is.Not.Null);
    Assert.That(result, Is.TypeOf<TestClass>());
  }

  [Test]
  public void Compile_NewArrayInit_CreatesArrayWithElements() {
    var body = Expression.NewArrayInit(typeof(int),
      Expression.Constant(1),
      Expression.Constant(2),
      Expression.Constant(3));
    var expr = Expression.Lambda<Func<int[]>>(body);
    var func = expr.Compile();

    var result = func();
    Assert.That(result, Is.EqualTo(new[] { 1, 2, 3 }));
  }

  [Test]
  public void Compile_NewArrayBounds_CreatesArrayWithSize() {
    var param = Expression.Parameter(typeof(int), "size");
    var body = Expression.NewArrayBounds(typeof(int), param);
    var expr = Expression.Lambda<Func<int, int[]>>(body, param);
    var func = expr.Compile();

    var result = func(5);
    Assert.That(result.Length, Is.EqualTo(5));
  }

  [Test]
  public void Compile_ArrayIndex_ReturnsElementAtIndex() {
    var param = Expression.Parameter(typeof(int[]), "arr");
    var body = Expression.ArrayIndex(param, Expression.Constant(1));
    var expr = Expression.Lambda<Func<int[], int>>(body, param);
    var func = expr.Compile();

    Assert.That(func(new[] { 10, 20, 30 }), Is.EqualTo(20));
  }

  [Test]
  public void Compile_ArrayLength_ReturnsLength() {
    var param = Expression.Parameter(typeof(int[]), "arr");
    var body = Expression.ArrayLength(param);
    var expr = Expression.Lambda<Func<int[], int>>(body, param);
    var func = expr.Compile();

    Assert.That(func(new[] { 1, 2, 3, 4, 5 }), Is.EqualTo(5));
  }

  #endregion

  #region Coalesce

  [Test]
  public void Compile_Coalesce_ReturnsLeft_WhenNotNull() {
    var param = Expression.Parameter(typeof(string), "s");
    var body = Expression.Coalesce(param, Expression.Constant("default"));
    var expr = Expression.Lambda<Func<string?, string>>(body, param);
    var func = expr.Compile();

    Assert.That(func("hello"), Is.EqualTo("hello"));
  }

  [Test]
  public void Compile_Coalesce_ReturnsRight_WhenLeftIsNull() {
    var param = Expression.Parameter(typeof(string), "s");
    var body = Expression.Coalesce(param, Expression.Constant("default"));
    var expr = Expression.Lambda<Func<string?, string>>(body, param);
    var func = expr.Compile();

    Assert.That(func(null), Is.EqualTo("default"));
  }

  #endregion

  #region Complex Expressions

  [Test]
  public void Compile_ComplexArithmeticExpression_EvaluatesCorrectly() {
    var x = Expression.Parameter(typeof(int), "x");
    var y = Expression.Parameter(typeof(int), "y");

    var body = Expression.Add(
      Expression.Multiply(x, x),
      Expression.Multiply(Expression.Constant(2), Expression.Multiply(x, y))
    );

    var expr = Expression.Lambda<Func<int, int, int>>(body, x, y);
    var func = expr.Compile();

    Assert.That(func(3, 4), Is.EqualTo(33));
  }

  [Test]
  public void Compile_NestedMethodCalls_EvaluatesCorrectly() {
    var param = Expression.Parameter(typeof(string), "s");

    var toUpperMethod = typeof(string).GetMethod(nameof(string.ToUpper), Type.EmptyTypes)!;
    var trimMethod = typeof(string).GetMethod(nameof(string.Trim), Type.EmptyTypes)!;

    var body = Expression.Call(
      Expression.Call(param, toUpperMethod),
      trimMethod
    );

    var expr = Expression.Lambda<Func<string, string>>(body, param);
    var func = expr.Compile();

    Assert.That(func("  hello  "), Is.EqualTo("HELLO"));
  }

  [Test]
  public void Compile_ChainedPropertyAccess_EvaluatesCorrectly() {
    var param = Expression.Parameter(typeof(OuterClass), "obj");

    var innerProp = typeof(OuterClass).GetProperty(nameof(OuterClass.Inner))!;
    var valueProp = typeof(InnerClass).GetProperty(nameof(InnerClass.Value))!;

    var body = Expression.Property(Expression.Property(param, innerProp), valueProp);

    var expr = Expression.Lambda<Func<OuterClass, int>>(body, param);
    var func = expr.Compile();

    var obj = new OuterClass { Inner = new InnerClass { Value = 42 } };
    Assert.That(func(obj), Is.EqualTo(42));
  }

  #endregion

  #region Default Values

  [Test]
  public void Compile_Default_ReturnsDefaultForValueType() {
    var body = Expression.Default(typeof(int));
    var expr = Expression.Lambda<Func<int>>(body);
    var func = expr.Compile();

    Assert.That(func(), Is.EqualTo(0));
  }

  [Test]
  public void Compile_Default_ReturnsNullForReferenceType() {
    var body = Expression.Default(typeof(string));
    var expr = Expression.Lambda<Func<string?>>(body);
    var func = expr.Compile();

    Assert.That(func(), Is.Null);
  }

  #endregion

  #region Invocation

  [Test]
  public void Compile_Invoke_CallsInnerLambda() {
    Expression<Func<int, int>> innerLambda = x => x * 2;

    var param = Expression.Parameter(typeof(int), "y");
    var body = Expression.Invoke(innerLambda, param);
    var expr = Expression.Lambda<Func<int, int>>(body, param);
    var func = expr.Compile();

    Assert.That(func(5), Is.EqualTo(10));
  }

  #endregion

  #region Helper Types

  public class TestClass {
    public int Value;
    public static int StaticValue;
  }

  public class OuterClass {
    public InnerClass Inner { get; set; } = new();
  }

  public class InnerClass {
    public int Value { get; set; }
  }

  #endregion

}
