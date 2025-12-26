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
using System.Reflection;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("PropertyInfo")]
public class PropertyInfoPolyfillTests {

  #region Test Helper Classes

  private class TestClass {
    public int IntProperty { get; set; }
    public string StringProperty { get; set; }
    public object ObjectProperty { get; set; }
    public int ReadOnlyProperty { get; } = 42;
    private int _writeOnlyBacking;
    public int WriteOnlyProperty { set => _writeOnlyBacking = value; }
    public int GetWriteOnlyBacking() => _writeOnlyBacking;
  }

  #endregion

  #region GetValue Tests

  [Test]
  [Category("HappyPath")]
  public void GetValue_IntProperty_ReturnsValue() {
    var obj = new TestClass { IntProperty = 123 };
    var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.IntProperty));
    var result = propertyInfo.GetValue(obj);
    Assert.That(result, Is.EqualTo(123));
  }

  [Test]
  [Category("HappyPath")]
  public void GetValue_StringProperty_ReturnsValue() {
    var obj = new TestClass { StringProperty = "test" };
    var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.StringProperty));
    var result = propertyInfo.GetValue(obj);
    Assert.That(result, Is.EqualTo("test"));
  }

  [Test]
  [Category("HappyPath")]
  public void GetValue_NullStringProperty_ReturnsNull() {
    var obj = new TestClass { StringProperty = null };
    var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.StringProperty));
    var result = propertyInfo.GetValue(obj);
    Assert.That(result, Is.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void GetValue_ObjectProperty_ReturnsValue() {
    var expectedValue = new object();
    var obj = new TestClass { ObjectProperty = expectedValue };
    var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.ObjectProperty));
    var result = propertyInfo.GetValue(obj);
    Assert.That(result, Is.SameAs(expectedValue));
  }

  [Test]
  [Category("HappyPath")]
  public void GetValue_ReadOnlyProperty_ReturnsValue() {
    var obj = new TestClass();
    var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.ReadOnlyProperty));
    var result = propertyInfo.GetValue(obj);
    Assert.That(result, Is.EqualTo(42));
  }

  [Test]
  [Category("EdgeCase")]
  public void GetValue_DefaultIntValue_ReturnsZero() {
    var obj = new TestClass();
    var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.IntProperty));
    var result = propertyInfo.GetValue(obj);
    Assert.That(result, Is.EqualTo(0));
  }

  #endregion

  #region SetValue Tests

  [Test]
  [Category("HappyPath")]
  public void SetValue_IntProperty_SetsValue() {
    var obj = new TestClass();
    var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.IntProperty));
    propertyInfo.SetValue(obj, 456);
    Assert.That(obj.IntProperty, Is.EqualTo(456));
  }

  [Test]
  [Category("HappyPath")]
  public void SetValue_StringProperty_SetsValue() {
    var obj = new TestClass();
    var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.StringProperty));
    propertyInfo.SetValue(obj, "hello");
    Assert.That(obj.StringProperty, Is.EqualTo("hello"));
  }

  [Test]
  [Category("HappyPath")]
  public void SetValue_StringPropertyToNull_SetsNull() {
    var obj = new TestClass { StringProperty = "initial" };
    var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.StringProperty));
    propertyInfo.SetValue(obj, null);
    Assert.That(obj.StringProperty, Is.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void SetValue_ObjectProperty_SetsValue() {
    var obj = new TestClass();
    var newValue = new object();
    var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.ObjectProperty));
    propertyInfo.SetValue(obj, newValue);
    Assert.That(obj.ObjectProperty, Is.SameAs(newValue));
  }

  [Test]
  [Category("HappyPath")]
  public void SetValue_WriteOnlyProperty_SetsValue() {
    var obj = new TestClass();
    var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.WriteOnlyProperty));
    propertyInfo.SetValue(obj, 789);
    Assert.That(obj.GetWriteOnlyBacking(), Is.EqualTo(789));
  }

  [Test]
  [Category("HappyPath")]
  public void SetValue_OverwritesExistingValue() {
    var obj = new TestClass { IntProperty = 100 };
    var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.IntProperty));
    propertyInfo.SetValue(obj, 200);
    Assert.That(obj.IntProperty, Is.EqualTo(200));
  }

  #endregion

  #region Round-Trip Tests

  [Test]
  [Category("HappyPath")]
  public void GetSetValue_RoundTrip_WorksCorrectly() {
    var obj = new TestClass();
    var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.IntProperty));
    propertyInfo.SetValue(obj, 999);
    var result = propertyInfo.GetValue(obj);
    Assert.That(result, Is.EqualTo(999));
  }

  #endregion

}
