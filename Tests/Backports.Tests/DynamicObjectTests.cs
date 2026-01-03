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

// DynamicObject is available in net40+ via BCL, and in net20 via our polyfill.
// Since NUnit requires net35+, these tests run against the BCL implementation on net40+.
// They serve to verify expected behavior and document the API contract.
#if SUPPORTS_DYNAMIC

using System;
using System.Collections.Generic;
using System.Dynamic;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Dynamic")]
[Category("DynamicObject")]
public class DynamicObjectTests {

  #region TryGetMember / TrySetMember

  [Test]
  [Category("HappyPath")]
  public void DynamicObject_TryGetMember_ReturnsOverriddenValue() {
    dynamic obj = new TestDynamicObject();
    obj.Name = "Test";
    Assert.That((string)obj.Name, Is.EqualTo("Test"));
  }

  [Test]
  [Category("HappyPath")]
  public void DynamicObject_TrySetMember_StoresValue() {
    dynamic obj = new TestDynamicObject();
    obj.Value = 42;
    Assert.That((int)obj.Value, Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void DynamicObject_MultipleProperties_AllStored() {
    dynamic obj = new TestDynamicObject();
    obj.First = "A";
    obj.Second = "B";
    obj.Third = "C";

    Assert.That((string)obj.First, Is.EqualTo("A"));
    Assert.That((string)obj.Second, Is.EqualTo("B"));
    Assert.That((string)obj.Third, Is.EqualTo("C"));
  }

  #endregion

  #region TryInvokeMember

  [Test]
  [Category("HappyPath")]
  public void DynamicObject_TryInvokeMember_CallsCustomMethod() {
    dynamic obj = new MethodDynamicObject();
    var result = obj.Add(3, 5);
    Assert.That((int)result, Is.EqualTo(8));
  }

  [Test]
  [Category("HappyPath")]
  public void DynamicObject_TryInvokeMember_WithNoArgs() {
    dynamic obj = new MethodDynamicObject();
    var result = obj.GetGreeting();
    Assert.That((string)result, Is.EqualTo("Hello!"));
  }

  [Test]
  [Category("HappyPath")]
  public void DynamicObject_TryInvokeMember_WithMultipleArgs() {
    dynamic obj = new MethodDynamicObject();
    var result = obj.Concat("Hello", " ", "World", "!");
    Assert.That((string)result, Is.EqualTo("Hello World!"));
  }

  #endregion

  #region TryInvoke

  [Test]
  [Category("HappyPath")]
  public void DynamicObject_TryInvoke_CallsAsFunction() {
    dynamic obj = new InvokableDynamicObject();
    var result = obj(10, 20);
    Assert.That((int)result, Is.EqualTo(30));
  }

  #endregion

  #region TryConvert

  [Test]
  [Category("HappyPath")]
  public void DynamicObject_TryConvert_ConvertsToDifferentType() {
    dynamic obj = new ConvertibleDynamicObject(42);
    int intValue = obj;
    Assert.That(intValue, Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void DynamicObject_TryConvert_ConvertsToString() {
    dynamic obj = new ConvertibleDynamicObject(42);
    string stringValue = obj;
    Assert.That(stringValue, Is.EqualTo("42"));
  }

  #endregion

  #region TryGetIndex / TrySetIndex

  [Test]
  [Category("HappyPath")]
  public void DynamicObject_TryGetIndex_ReturnsIndexedValue() {
    dynamic obj = new IndexableDynamicObject();
    obj[0] = "Zero";
    obj[1] = "One";
    obj[2] = "Two";

    Assert.That((string)obj[0], Is.EqualTo("Zero"));
    Assert.That((string)obj[1], Is.EqualTo("One"));
    Assert.That((string)obj[2], Is.EqualTo("Two"));
  }

  [Test]
  [Category("HappyPath")]
  public void DynamicObject_TryGetIndex_WithStringKey() {
    dynamic obj = new StringIndexableDynamicObject();
    obj["key1"] = "value1";
    obj["key2"] = "value2";

    Assert.That((string)obj["key1"], Is.EqualTo("value1"));
    Assert.That((string)obj["key2"], Is.EqualTo("value2"));
  }

  #endregion

  #region TryBinaryOperation

  [Test]
  [Category("HappyPath")]
  public void DynamicObject_TryBinaryOperation_Add() {
    dynamic obj = new ArithmeticDynamicObject(10);
    dynamic result = obj + 5;
    Assert.That((int)result, Is.EqualTo(15));
  }

  [Test]
  [Category("HappyPath")]
  public void DynamicObject_TryBinaryOperation_Subtract() {
    dynamic obj = new ArithmeticDynamicObject(10);
    dynamic result = obj - 3;
    Assert.That((int)result, Is.EqualTo(7));
  }

  [Test]
  [Category("HappyPath")]
  public void DynamicObject_TryBinaryOperation_Multiply() {
    dynamic obj = new ArithmeticDynamicObject(10);
    dynamic result = obj * 4;
    Assert.That((int)result, Is.EqualTo(40));
  }

  #endregion

  #region TryUnaryOperation

  [Test]
  [Category("HappyPath")]
  public void DynamicObject_TryUnaryOperation_Negate() {
    dynamic obj = new ArithmeticDynamicObject(10);
    dynamic result = -obj;
    Assert.That((int)result, Is.EqualTo(-10));
  }

  [Test]
  [Category("HappyPath")]
  public void DynamicObject_TryUnaryOperation_Not() {
    dynamic obj = new BooleanDynamicObject(true);
    dynamic result = !obj;
    Assert.That((bool)result, Is.False);
  }

  #endregion

  #region GetDynamicMemberNames

  [Test]
  [Category("HappyPath")]
  public void DynamicObject_GetDynamicMemberNames_ReturnsStoredKeys() {
    dynamic obj = new TestDynamicObject();
    obj.Alpha = 1;
    obj.Beta = 2;
    obj.Gamma = 3;

    var memberNames = ((TestDynamicObject)obj).GetDynamicMemberNames();
    Assert.That(memberNames, Is.EquivalentTo(new[] { "Alpha", "Beta", "Gamma" }));
  }

  #endregion

  #region Helper Classes

  private class TestDynamicObject : DynamicObject {
    private readonly Dictionary<string, object> _storage = new();

    public override bool TryGetMember(GetMemberBinder binder, out object result) {
      return _storage.TryGetValue(binder.Name, out result);
    }

    public override bool TrySetMember(SetMemberBinder binder, object value) {
      _storage[binder.Name] = value;
      return true;
    }

    public override IEnumerable<string> GetDynamicMemberNames() => _storage.Keys;
  }

  private class MethodDynamicObject : DynamicObject {
    public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
      switch (binder.Name) {
        case "Add":
          result = (int)args[0] + (int)args[1];
          return true;
        case "GetGreeting":
          result = "Hello!";
          return true;
        case "Concat":
          result = string.Concat(args);
          return true;
        default:
          result = null;
          return false;
      }
    }
  }

  private class InvokableDynamicObject : DynamicObject {
    public override bool TryInvoke(InvokeBinder binder, object[] args, out object result) {
      result = (int)args[0] + (int)args[1];
      return true;
    }
  }

  private class ConvertibleDynamicObject : DynamicObject {
    private readonly int _value;

    public ConvertibleDynamicObject(int value) => _value = value;

    public override bool TryConvert(ConvertBinder binder, out object result) {
      if (binder.Type == typeof(int)) {
        result = _value;
        return true;
      }
      if (binder.Type == typeof(string)) {
        result = _value.ToString();
        return true;
      }
      result = null;
      return false;
    }
  }

  private class IndexableDynamicObject : DynamicObject {
    private readonly Dictionary<int, object> _storage = new();

    public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result) {
      return _storage.TryGetValue((int)indexes[0], out result);
    }

    public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value) {
      _storage[(int)indexes[0]] = value;
      return true;
    }
  }

  private class StringIndexableDynamicObject : DynamicObject {
    private readonly Dictionary<string, object> _storage = new();

    public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result) {
      return _storage.TryGetValue((string)indexes[0], out result);
    }

    public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value) {
      _storage[(string)indexes[0]] = value;
      return true;
    }
  }

  private class ArithmeticDynamicObject : DynamicObject {
    private readonly int _value;

    public ArithmeticDynamicObject(int value) => _value = value;

    public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result) {
      var other = (int)arg;
      result = binder.Operation switch {
        System.Linq.Expressions.ExpressionType.Add => _value + other,
        System.Linq.Expressions.ExpressionType.Subtract => _value - other,
        System.Linq.Expressions.ExpressionType.Multiply => _value * other,
        System.Linq.Expressions.ExpressionType.Divide => _value / other,
        _ => throw new NotSupportedException()
      };
      return true;
    }

    public override bool TryUnaryOperation(UnaryOperationBinder binder, out object result) {
      result = binder.Operation switch {
        System.Linq.Expressions.ExpressionType.Negate => -_value,
        System.Linq.Expressions.ExpressionType.UnaryPlus => _value,
        _ => throw new NotSupportedException()
      };
      return true;
    }
  }

  private class BooleanDynamicObject : DynamicObject {
    private readonly bool _value;

    public BooleanDynamicObject(bool value) => _value = value;

    public override bool TryUnaryOperation(UnaryOperationBinder binder, out object result) {
      if (binder.Operation == System.Linq.Expressions.ExpressionType.Not) {
        result = !_value;
        return true;
      }
      result = null;
      return false;
    }
  }

  #endregion

}

#endif
