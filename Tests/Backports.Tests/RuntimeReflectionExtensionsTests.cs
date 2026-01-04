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
[Category("RuntimeReflectionExtensions")]
public class RuntimeReflectionExtensionsTests {

  #region GetMethodInfo

  [Test]
  [Category("HappyPath")]
  public void GetMethodInfo_FromAction_ReturnsCorrectMethod() {
    Action action = TestMethod;
    var methodInfo = action.GetMethodInfo();
    Assert.That(methodInfo, Is.Not.Null);
    Assert.That(methodInfo.Name, Is.EqualTo(nameof(TestMethod)));
  }

  [Test]
  [Category("HappyPath")]
  public void GetMethodInfo_FromFunc_ReturnsCorrectMethod() {
    Func<int> func = TestFuncMethod;
    var methodInfo = func.GetMethodInfo();
    Assert.That(methodInfo, Is.Not.Null);
    Assert.That(methodInfo.Name, Is.EqualTo(nameof(TestFuncMethod)));
  }

  [Test]
  [Category("HappyPath")]
  public void GetMethodInfo_FromLambda_ReturnsMethod() {
    Action action = () => { };
    var methodInfo = action.GetMethodInfo();
    Assert.That(methodInfo, Is.Not.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void GetMethodInfo_FromInstanceMethod_ReturnsCorrectMethod() {
    var instance = new TestClass();
    Action action = instance.InstanceMethod;
    var methodInfo = action.GetMethodInfo();
    Assert.That(methodInfo, Is.Not.Null);
    Assert.That(methodInfo.Name, Is.EqualTo(nameof(TestClass.InstanceMethod)));
  }

  #endregion

  #region GetTypeInfo

  [Test]
  [Category("HappyPath")]
  public void GetTypeInfo_FromType_ReturnsTypeInfo() {
    var type = typeof(string);
    var typeInfo = type.GetTypeInfo();
    Assert.That(typeInfo, Is.Not.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void GetTypeInfo_AsType_ReturnsOriginalType() {
    var type = typeof(int);
    var typeInfo = type.GetTypeInfo();
    Assert.That(typeInfo.AsType(), Is.EqualTo(type));
  }

  [Test]
  [Category("HappyPath")]
  public void GetTypeInfo_IsValueType_CorrectForValueType() {
    var typeInfo = typeof(int).GetTypeInfo();
    Assert.That(typeInfo.IsValueType, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void GetTypeInfo_IsValueType_CorrectForReferenceType() {
    var typeInfo = typeof(string).GetTypeInfo();
    Assert.That(typeInfo.IsValueType, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void GetTypeInfo_IsClass_CorrectForClass() {
    var typeInfo = typeof(TestClass).GetTypeInfo();
    Assert.That(typeInfo.IsClass, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void GetTypeInfo_IsInterface_CorrectForInterface() {
    var typeInfo = typeof(IDisposable).GetTypeInfo();
    Assert.That(typeInfo.IsInterface, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void GetTypeInfo_IsEnum_CorrectForEnum() {
    var typeInfo = typeof(TestEnum).GetTypeInfo();
    Assert.That(typeInfo.IsEnum, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void GetTypeInfo_IsArray_CorrectForArray() {
    var typeInfo = typeof(int[]).GetTypeInfo();
    Assert.That(typeInfo.IsArray, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void GetTypeInfo_IsGenericType_CorrectForGeneric() {
    var typeInfo = typeof(System.Collections.Generic.List<int>).GetTypeInfo();
    Assert.That(typeInfo.IsGenericType, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void GetTypeInfo_Name_ReturnsCorrectName() {
    var typeInfo = typeof(TestClass).GetTypeInfo();
    Assert.That(typeInfo.Name, Is.EqualTo(nameof(TestClass)));
  }

  [Test]
  [Category("HappyPath")]
  public void GetTypeInfo_Namespace_ReturnsCorrectNamespace() {
    var typeInfo = typeof(RuntimeReflectionExtensionsTests).GetTypeInfo();
    Assert.That(typeInfo.Namespace, Is.EqualTo("Backports.Tests"));
  }

  [Test]
  [Category("HappyPath")]
  public void GetTypeInfo_BaseType_ReturnsCorrectBaseType() {
    var typeInfo = typeof(TestDerivedClass).GetTypeInfo();
    Assert.That(typeInfo.BaseType, Is.EqualTo(typeof(TestClass)));
  }

  [Test]
  [Category("HappyPath")]
  public void GetTypeInfo_DeclaredMethods_ContainsDeclaredMethods() {
    var typeInfo = typeof(TestClass).GetTypeInfo();
    var methods = typeInfo.DeclaredMethods;
    Assert.That(methods, Is.Not.Empty);
  }

  [Test]
  [Category("HappyPath")]
  public void GetTypeInfo_DeclaredProperties_ContainsDeclaredProperties() {
    var typeInfo = typeof(TestClassWithMembers).GetTypeInfo();
    var properties = typeInfo.DeclaredProperties;
    Assert.That(properties, Is.Not.Empty);
  }

  [Test]
  [Category("HappyPath")]
  public void GetTypeInfo_DeclaredFields_ContainsDeclaredFields() {
    var typeInfo = typeof(TestClassWithMembers).GetTypeInfo();
    var fields = typeInfo.DeclaredFields;
    Assert.That(fields, Is.Not.Empty);
  }

  [Test]
  [Category("HappyPath")]
  public void GetTypeInfo_ImplementedInterfaces_ContainsInterfaces() {
    var typeInfo = typeof(TestClassWithInterface).GetTypeInfo();
    var interfaces = typeInfo.ImplementedInterfaces;
    Assert.That(interfaces, Contains.Item(typeof(IDisposable)));
  }

  [Test]
  [Category("HappyPath")]
  public void GetTypeInfo_GenericTypeArguments_ReturnsTypeArguments() {
    var typeInfo = typeof(System.Collections.Generic.Dictionary<string, int>).GetTypeInfo();
    var args = typeInfo.GenericTypeArguments;
    Assert.That(args.Length, Is.EqualTo(2));
    Assert.That(args[0], Is.EqualTo(typeof(string)));
    Assert.That(args[1], Is.EqualTo(typeof(int)));
  }

  [Test]
  [Category("HappyPath")]
  public void GetTypeInfo_IsAssignableFrom_ReturnsTrueForCompatibleTypes() {
    var baseTypeInfo = typeof(TestClass).GetTypeInfo();
    var derivedTypeInfo = typeof(TestDerivedClass).GetTypeInfo();
    Assert.That(baseTypeInfo.IsAssignableFrom(derivedTypeInfo), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void GetTypeInfo_IsSubclassOf_ReturnsTrueForDerivedClass() {
    var typeInfo = typeof(TestDerivedClass).GetTypeInfo();
    Assert.That(typeInfo.IsSubclassOf(typeof(TestClass)), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void GetTypeInfo_IsInstanceOfType_ReturnsTrueForInstance() {
    var typeInfo = typeof(TestClass).GetTypeInfo();
    var instance = new TestClass();
    Assert.That(typeInfo.IsInstanceOfType(instance), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void GetTypeInfo_GetCustomAttributes_ReturnsAttributes() {
    var typeInfo = typeof(TestClassWithAttribute).GetTypeInfo();
    var attrs = typeInfo.GetCustomAttributes(typeof(SerializableAttribute), false);
    Assert.That(attrs.Length, Is.GreaterThan(0));
  }

  [Test]
  [Category("HappyPath")]
  public void GetTypeInfo_IsDefined_ReturnsTrueForDefinedAttribute() {
    var typeInfo = typeof(TestClassWithAttribute).GetTypeInfo();
    Assert.That(typeInfo.IsDefined(typeof(SerializableAttribute), false), Is.True);
  }

  #endregion

  #region Helper Types and Methods

  private static void TestMethod() { }

  private static int TestFuncMethod() => 42;

  private class TestClass {
    public void InstanceMethod() { }
  }

  private class TestDerivedClass : TestClass { }

  private class TestClassWithMembers {
    public int TestField = 42;
    public string TestProperty { get; set; }
  }

  private class TestClassWithInterface : IDisposable {
    public void Dispose() { }
  }

  [Serializable]
  private class TestClassWithAttribute { }

  private enum TestEnum {
    Value1,
    Value2
  }

  #endregion

}
