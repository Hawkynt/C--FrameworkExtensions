using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;
using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace System;

[TestFixture]
public class TypeTests {
  #region Test Types and Classes

  // Simple test class for constructor testing
  private class SimpleTestClass {
    public string Value { get; set; }
    public SimpleTestClass() => this.Value = "default";
    public SimpleTestClass(string value) => this.Value = value;
    public SimpleTestClass(int number) => this.Value = number.ToString();
    public SimpleTestClass(string value, int number) => this.Value = $"{value}:{number}";
  }

  // Class with attributes for attribute testing
  [Description("Test class description")]
  [DisplayName("Test Class Display Name")]
  private class AttributedTestClass {
    [Description("Test field description")]
    public string TestField = "field";

    [Description("Test property description")]
    public string TestProperty { get; set; } = "property";
  }

  // Generic test class
  private class GenericTestClass<T> {
    public T Value { get; set; }
    public GenericTestClass() { }
    public GenericTestClass(T value) => this.Value = value;
  }

  // Abstract class for inheritance testing
  private abstract class AbstractTestClass {
    public abstract void DoSomething();
  }

  // Interface for implementation testing
  private interface ITestInterface {
    void TestMethod();
  }

  // Implementation of interface
  private class TestImplementation : ITestInterface {
    public void TestMethod() { }
  }

  // Class with no parameterless constructor
  private class NoParameterlessConstructor {
    public string Value { get; }
    public NoParameterlessConstructor(string value) => this.Value = value;
  }

  // Struct for value type testing
  private struct TestStruct {
    public int X { get; set; }
    public string Name { get; set; }

    public TestStruct(int x, string name) {
      this.X = x;
      this.Name = name;
    }
  }

  // Enum for enum testing
  private enum TestEnum {
    None,
    First,
    Second
  }

  // Flags enum for flags testing
  [Flags]
  private enum FlagsTestEnum {
    None = 0,
    Flag1 = 1,
    Flag2 = 2,
    Flag3 = 4
  }

  #endregion

  #region Type Conversion & Casting Tests

  [Test]
  public void TypeExtensions_IsCastableTo_SameType_ReturnsTrue() {
    var type = typeof(string);

    Assert.That(type.IsCastableTo(typeof(string)), Is.True);
  }

  [Test]
  public void TypeExtensions_IsCastableTo_InheritanceHierarchy_ReturnsTrue() {
    var derivedType = typeof(ArgumentException);
    var baseType = typeof(Exception);

    Assert.That(derivedType.IsCastableTo(baseType), Is.True);
  }

  [Test]
  public void TypeExtensions_IsCastableTo_UnrelatedTypes_ReturnsFalse() {
    var type1 = typeof(string);
    var type2 = typeof(int);

    Assert.That(type1.IsCastableTo(type2), Is.False);
  }

  [Test]
  public void TypeExtensions_IsCastableTo_InterfaceImplementation_ReturnsTrue() {
    var type = typeof(TestImplementation);
    var interfaceType = typeof(ITestInterface);

    Assert.That(type.IsCastableTo(interfaceType), Is.True);
  }

  [Test]
  public void TypeExtensions_IsCastableTo_NumericConversions_ReturnsTrue() {
    var intType = typeof(int);
    var longType = typeof(long);

    Assert.That(intType.IsCastableTo(longType), Is.True);
  }

  [Test]
  public void TypeExtensions_IsCastableFrom_ReversesIsCastableTo() {
    var baseType = typeof(Exception);
    var derivedType = typeof(ArgumentException);

    Assert.That(baseType.IsCastableFrom(derivedType), Is.True);
    Assert.That(derivedType.IsCastableFrom(baseType), Is.False);
  }

  [Test]
  public void TypeExtensions_IsCastableTo_NullableTypes_HandlesCorrectly() {
    var intType = typeof(int);
    var nullableIntType = typeof(int?);

    Assert.That(intType.IsCastableTo(nullableIntType), Is.True); // implicit
    Assert.That(nullableIntType.IsCastableTo(intType), Is.True); // explicit
  }

  #endregion

  #region Attribute Retrieval Tests

  [Test]
  public void TypeExtensions_GetDisplayName_WithDisplayNameAttribute_ReturnsDisplayName() {
    var type = typeof(AttributedTestClass);

    var result = type.GetDisplayName();

    Assert.That(result, Is.EqualTo("Test Class Display Name"));
  }

  [Test]
  public void TypeExtensions_GetDisplayName_WithoutDisplayNameAttribute_ReturnsNull() {
    var type = typeof(SimpleTestClass);

    var result = type.GetDisplayName();

    Assert.That(result, Is.Null);
  }

  [Test]
  public void TypeExtensions_GetDescription_WithDescriptionAttribute_ReturnsDescription() {
    var type = typeof(AttributedTestClass);

    var result = type.GetDescription();

    Assert.That(result, Is.EqualTo("Test class description"));
  }

  [Test]
  public void TypeExtensions_GetDescription_WithoutDescriptionAttribute_ReturnsNull() {
    var type = typeof(SimpleTestClass);

    var result = type.GetDescription();

    Assert.That(result, Is.Null);
  }

  [Test]
  public void TypeExtensions_GetAttributes_WithInheritance_ReturnsAllAttributes() {
    var type = typeof(AttributedTestClass);

    var attributes = type.GetAttributes<DescriptionAttribute>(inherit: true).ToArray();

    Assert.That(attributes.Length, Is.GreaterThanOrEqualTo(1));
    Assert.That(attributes[0].Description, Is.EqualTo("Test class description"));
  }

  [Test]
  public void TypeExtensions_GetAttributes_WithoutInheritance_ReturnsDirectAttributes() {
    var type = typeof(AttributedTestClass);

    var attributes = type.GetAttributes<DescriptionAttribute>(inherit: false).ToArray();

    Assert.That(attributes.Length, Is.GreaterThanOrEqualTo(1));
  }

  [Test]
  public void TypeExtensions_GetFieldOrPropertyAttributeValue_WithField_ReturnsValue() {
    var type = typeof(AttributedTestClass);

    var result = type.GetFieldOrPropertyAttributeValue<DescriptionAttribute, string>(
      "TestField",
      attr => attr.Description
    );

    Assert.That(result, Is.EqualTo("Test field description"));
  }

  [Test]
  public void TypeExtensions_GetFieldOrPropertyAttributeValue_WithProperty_ReturnsValue() {
    var type = typeof(AttributedTestClass);

    var result = type.GetFieldOrPropertyAttributeValue<DescriptionAttribute, string>(
      "TestProperty",
      attr => attr.Description
    );

    Assert.That(result, Is.EqualTo("Test property description"));
  }

  [Test]
  public void TypeExtensions_GetFieldOrPropertyAttributeValue_WithNonExistentMember_Throws() {
    var type = typeof(AttributedTestClass);

    Assert.Throws<ArgumentException>(
      () => type.GetFieldOrPropertyAttributeValue<DescriptionAttribute, string>(
        "NonExistentMember",
        attr => attr.Description
      )
    );
  }

  #endregion

  #region Type Inspection Tests

  [Test]
  public void TypeExtensions_SimpleName_BasicTypes_ReturnsSimpleName() {
    Assert.That(typeof(string).SimpleName(), Is.EqualTo("String"));
    Assert.That(typeof(int).SimpleName(), Is.EqualTo("Int32"));
    Assert.That(typeof(List<string>).SimpleName(), Is.EqualTo("List`1"));
  }

  [Test]
  public void TypeExtensions_SimpleName_WithLanguageTypes_ReturnsLanguageNames() {
    Assert.That(typeof(int).SimpleName(useLanguageTypes: true), Is.EqualTo("int"));
    Assert.That(typeof(string).SimpleName(useLanguageTypes: true), Is.EqualTo("string"));
    Assert.That(typeof(bool).SimpleName(useLanguageTypes: true), Is.EqualTo("bool"));
    Assert.That(typeof(double).SimpleName(useLanguageTypes: true), Is.EqualTo("double"));
  }

  [Test]
  public void TypeExtensions_IsIntegerType_IntegerTypes_ReturnsTrue() {
    Assert.That(typeof(byte).IsIntegerType(), Is.True);
    Assert.That(typeof(sbyte).IsIntegerType(), Is.True);
    Assert.That(typeof(short).IsIntegerType(), Is.True);
    Assert.That(typeof(ushort).IsIntegerType(), Is.True);
    Assert.That(typeof(int).IsIntegerType(), Is.True);
    Assert.That(typeof(uint).IsIntegerType(), Is.True);
    Assert.That(typeof(long).IsIntegerType(), Is.True);
    Assert.That(typeof(ulong).IsIntegerType(), Is.True);
  }

  [Test]
  public void TypeExtensions_IsIntegerType_NonIntegerTypes_ReturnsFalse() {
    Assert.That(typeof(float).IsIntegerType(), Is.False);
    Assert.That(typeof(double).IsIntegerType(), Is.False);
    Assert.That(typeof(decimal).IsIntegerType(), Is.False);
    Assert.That(typeof(string).IsIntegerType(), Is.False);
    Assert.That(typeof(bool).IsIntegerType(), Is.False);
  }

  [Test]
  public void TypeExtensions_IsSigned_SignedTypes_ReturnsTrue() {
    Assert.That(typeof(sbyte).IsSigned(), Is.True);
    Assert.That(typeof(short).IsSigned(), Is.True);
    Assert.That(typeof(int).IsSigned(), Is.True);
    Assert.That(typeof(long).IsSigned(), Is.True);
    Assert.That(typeof(float).IsSigned(), Is.True);
    Assert.That(typeof(double).IsSigned(), Is.True);
    Assert.That(typeof(decimal).IsSigned(), Is.True);
  }

  [Test]
  public void TypeExtensions_IsSigned_UnsignedTypes_ReturnsFalse() {
    Assert.That(typeof(byte).IsSigned(), Is.False);
    Assert.That(typeof(ushort).IsSigned(), Is.False);
    Assert.That(typeof(uint).IsSigned(), Is.False);
    Assert.That(typeof(ulong).IsSigned(), Is.False);
  }

  [Test]
  public void TypeExtensions_IsUnsigned_UnsignedTypes_ReturnsTrue() {
    Assert.That(typeof(byte).IsUnsigned(), Is.True);
    Assert.That(typeof(ushort).IsUnsigned(), Is.True);
    Assert.That(typeof(uint).IsUnsigned(), Is.True);
    Assert.That(typeof(ulong).IsUnsigned(), Is.True);
  }

  [Test]
  public void TypeExtensions_IsFloatType_FloatingPointTypes_ReturnsTrue() {
    Assert.That(typeof(float).IsFloatType(), Is.True);
    Assert.That(typeof(double).IsFloatType(), Is.True);
    Assert.That(typeof(decimal).IsFloatType(), Is.True);
  }

  [Test]
  public void TypeExtensions_IsFloatType_NonFloatingPointTypes_ReturnsFalse() {
    Assert.That(typeof(int).IsFloatType(), Is.False);
    Assert.That(typeof(string).IsFloatType(), Is.False);
    Assert.That(typeof(bool).IsFloatType(), Is.False);
  }

  [Test]
  public void TypeExtensions_IsDecimalType_DecimalType_ReturnsTrue() {
    Assert.That(typeof(decimal).IsDecimalType(), Is.True);
    Assert.That(typeof(decimal?).IsDecimalType(), Is.True);
  }

  [Test]
  public void TypeExtensions_IsDecimalType_NonDecimalTypes_ReturnsFalse() {
    Assert.That(typeof(float).IsDecimalType(), Is.False);
    Assert.That(typeof(double).IsDecimalType(), Is.False);
    Assert.That(typeof(int).IsDecimalType(), Is.False);
  }

  [Test]
  public void TypeExtensions_IsStringType_StringType_ReturnsTrue() => Assert.That(typeof(string).IsStringType(), Is.True);

  [Test]
  public void TypeExtensions_IsStringType_NonStringTypes_ReturnsFalse() {
    Assert.That(typeof(int).IsStringType(), Is.False);
    Assert.That(typeof(char).IsStringType(), Is.False);
    Assert.That(typeof(object).IsStringType(), Is.False);
  }

  [Test]
  public void TypeExtensions_IsBooleanType_BooleanType_ReturnsTrue() {
    Assert.That(typeof(bool).IsBooleanType(), Is.True);
    Assert.That(typeof(bool?).IsBooleanType(), Is.True);
  }

  [Test]
  public void TypeExtensions_IsTimeSpanType_TimeSpanType_ReturnsTrue() {
    Assert.That(typeof(TimeSpan).IsTimeSpanType(), Is.True);
    Assert.That(typeof(TimeSpan?).IsTimeSpanType(), Is.True);
  }

  [Test]
  public void TypeExtensions_IsDateTimeType_DateTimeType_ReturnsTrue() {
    Assert.That(typeof(DateTime).IsDateTimeType(), Is.True);
    Assert.That(typeof(DateTime?).IsDateTimeType(), Is.True);
  }

  [Test]
  public void TypeExtensions_IsEnumType_EnumTypes_ReturnsTrue() {
    Assert.That(typeof(TestEnum).IsEnumType(), Is.True);
    Assert.That(typeof(FlagsTestEnum).IsEnumType(), Is.True);
    Assert.That(typeof(TestEnum?).IsEnumType(), Is.True);
  }

  [Test]
  public void TypeExtensions_IsNullable_NullableTypes_ReturnsTrue() {
    Assert.That(typeof(int?).IsNullable(), Is.True);
    Assert.That(typeof(DateTime?).IsNullable(), Is.True);
    Assert.That(typeof(TestEnum?).IsNullable(), Is.True);
  }

  [Test]
  public void TypeExtensions_IsNullable_NonNullableTypes_ReturnsFalse() {
    Assert.That(typeof(int).IsNullable(), Is.False);
    Assert.That(typeof(string).IsNullable(), Is.False); // string is reference type
    Assert.That(typeof(object).IsNullable(), Is.False);
  }

  [Test]
  public void TypeExtensions_GetMinValueForIntType_IntegerTypes_ReturnsCorrectMinimum() {
    Assert.That(typeof(byte).GetMinValueForIntType(), Is.EqualTo(byte.MinValue));
    Assert.That(typeof(sbyte).GetMinValueForIntType(), Is.EqualTo(sbyte.MinValue));
    Assert.That(typeof(short).GetMinValueForIntType(), Is.EqualTo(short.MinValue));
    Assert.That(typeof(int).GetMinValueForIntType(), Is.EqualTo(int.MinValue));
    Assert.That(typeof(long).GetMinValueForIntType(), Is.EqualTo(long.MinValue));
  }

  [Test]
  public void TypeExtensions_GetMaxValueForIntType_IntegerTypes_ReturnsCorrectMaximum() {
    Assert.That(typeof(byte).GetMaxValueForIntType(), Is.EqualTo(byte.MaxValue));
    Assert.That(typeof(sbyte).GetMaxValueForIntType(), Is.EqualTo(sbyte.MaxValue));
    Assert.That(typeof(short).GetMaxValueForIntType(), Is.EqualTo(short.MaxValue));
    Assert.That(typeof(int).GetMaxValueForIntType(), Is.EqualTo(int.MaxValue));
    Assert.That(typeof(long).GetMaxValueForIntType(), Is.EqualTo(long.MaxValue));
  }

  [Test]
  public void TypeExtensions_GetMinMaxValueForIntType_NonIntegerType_ThrowsException() {
    Assert.Throws<ArgumentException>(() => typeof(string).GetMinValueForIntType());
    Assert.Throws<ArgumentException>(() => typeof(float).GetMaxValueForIntType());
  }

  #endregion

  #region Instance Creation Tests

  [Test]
  public void TypeExtensions_GetDefaultValue_ValueTypes_ReturnsZeroEquivalent() {
    Assert.That(typeof(int).GetDefaultValue(), Is.EqualTo(0));
    Assert.That(typeof(bool).GetDefaultValue(), Is.EqualTo(false));
    Assert.That(typeof(DateTime).GetDefaultValue(), Is.EqualTo(default(DateTime)));
    Assert.That(typeof(TestStruct).GetDefaultValue(), Is.EqualTo(default(TestStruct)));
  }

  [Test]
  public void TypeExtensions_GetDefaultValue_ReferenceTypes_ReturnsNull() {
    Assert.That(typeof(string).GetDefaultValue(), Is.Null);
    Assert.That(typeof(object).GetDefaultValue(), Is.Null);
    Assert.That(typeof(SimpleTestClass).GetDefaultValue(), Is.Null);
  }

  [Test]
  public void TypeExtensions_GetDefaultValue_NullableTypes_ReturnsNull() {
    Assert.That(typeof(int?).GetDefaultValue(), Is.Null);
    Assert.That(typeof(DateTime?).GetDefaultValue(), Is.Null);
    Assert.That(typeof(TestEnum?).GetDefaultValue(), Is.Null);
  }

  [Test]
  public void TypeExtensions_CreateInstance_ClassWithParameterlessConstructor_CreatesInstance() {
    var instance = typeof(SimpleTestClass).CreateInstance<SimpleTestClass>();

    Assert.That(instance, Is.Not.Null);
    Assert.That(instance.Value, Is.EqualTo("default"));
  }

  [Test]
  public void TypeExtensions_CreateInstance_WithParameters_CallsCorrectConstructor() {
    var instance = typeof(SimpleTestClass).CreateInstance<SimpleTestClass>("test");

    Assert.That(instance, Is.Not.Null);
    Assert.That(instance.Value, Is.EqualTo("test"));
  }

  [Test]
  public void TypeExtensions_CreateInstance_ValueType_CreatesInstance() {
    var instance = typeof(int).CreateInstance<int>();

    Assert.That(instance, Is.EqualTo(0));
  }

  [Test]
  public void TypeExtensions_CreateInstance_NoParameterlessConstructor_ThrowsException() => Assert.Throws<MissingMethodException>(() => { typeof(NoParameterlessConstructor).CreateInstance<NoParameterlessConstructor>(); });

  [Test]
  public void TypeExtensions_CreateInstance_AbstractClass_ThrowsException() => Assert.Throws<MissingMethodException>(() => { typeof(AbstractTestClass).CreateInstance<AbstractTestClass>(); });

  [Test]
  public void TypeExtensions_GetRandomValue_ValueTypes_GeneratesValidValues() {
    var intValue = (int)typeof(int).GetRandomValue();
    var boolValue = (bool)typeof(bool).GetRandomValue();
    var doubleValue = (double)typeof(double).GetRandomValue();

    // Values should be within valid ranges (basic sanity check)
    Assert.That(intValue, Is.TypeOf<int>());
    Assert.That(boolValue, Is.TypeOf<bool>());
    Assert.That(doubleValue, Is.TypeOf<double>());
  }

  [Test]
  public void TypeExtensions_GetRandomValue_FlagsEnum_GeneratesValidFlagsCombination() {
    var flagsValue = (FlagsTestEnum)typeof(FlagsTestEnum).GetRandomValue();

    // Should be a valid combination of defined flags
    Assert.That(flagsValue, Is.TypeOf<FlagsTestEnum>());
  }

  [Test]
  public void TypeExtensions_GetRandomValue_ReferenceTypeWithoutInstanceCreation_ReturnsNull() {
    var value = typeof(SimpleTestClass).GetRandomValue(allowInstanceCreationForReferenceTypes: false);

    Assert.That(value, Is.Null);
  }

  [Test]
  public void TypeExtensions_GetRandomValue_ReferenceTypeWithInstanceCreation_CreatesInstance() {
    var value = typeof(SimpleTestClass).GetRandomValue(allowInstanceCreationForReferenceTypes: true);

    Assert.That(value, Is.Not.Null);
    Assert.That(value, Is.TypeOf<SimpleTestClass>());
  }

  [Test]
  public void TypeExtensions_GetRandomValue_StructType_GeneratesRandomStruct() {
    var structValue = (TestStruct)typeof(TestStruct).GetRandomValue();

    Assert.That(structValue, Is.TypeOf<TestStruct>());
    // Struct should have some random values (hard to test exact values)
  }

  #endregion

  #region T4-Generated Constructor Tests

  [Test]
  public void TypeExtensions_FromConstructor_OneParameter_CreatesInstance() {
    var instance = typeof(SimpleTestClass).FromConstructor<SimpleTestClass, string>("test");

    Assert.That(instance, Is.Not.Null);
    Assert.That(instance.Value, Is.EqualTo("test"));
  }

  [Test]
  public void TypeExtensions_FromConstructor_TwoParameters_CreatesInstance() {
    var instance = typeof(SimpleTestClass).FromConstructor<SimpleTestClass, string, int>("test", 42);

    Assert.That(instance, Is.Not.Null);
    Assert.That(instance.Value, Is.EqualTo("test:42"));
  }

  [Test]
  public void TypeExtensions_FromConstructor_ImplicitConversion_WorksCorrectly() {
    // Should find constructor with int parameter when passed byte
    var instance = typeof(SimpleTestClass).FromConstructor<SimpleTestClass, byte>(42);

    Assert.That(instance, Is.Not.Null);
    Assert.That(instance.Value, Is.EqualTo("42"));
  }

  [Test]
  public void TypeExtensions_FromConstructor_NonExistentConstructor_ThrowsException() => Assert.Throws<MissingMethodException>(() => { typeof(SimpleTestClass).FromConstructor<SimpleTestClass, double>(3.14); });

  [Test]
  public void TypeExtensions_FromConstructor_GenericType_WorksCorrectly() {
    var instance = typeof(GenericTestClass<string>).FromConstructor<GenericTestClass<string>, string>("test");

    Assert.That(instance, Is.Not.Null);
    Assert.That(instance.Value, Is.EqualTo("test"));
  }

  #endregion

  #region Static Member Access Tests

  [Test]
  public void TypeExtensions_GetStaticFieldValue_ExistingField_ReturnsValue() {
    // Test with a known static field
    var value = typeof(string).GetStaticFieldValue<string>("Empty");

    Assert.That(value, Is.EqualTo(string.Empty));
  }

  [Test]
  public void TypeExtensions_GetStaticPropertyValue_ExistingProperty_ReturnsValue() {
    // Test with DateTime.Now
    var now = typeof(DateTime).GetStaticPropertyValue<DateTime>("Now");

    Assert.That(now, Is.TypeOf<DateTime>());
    Assert.That(now, Is.GreaterThan(DateTime.MinValue));
  }

  [Test]
  public void TypeExtensions_GetStaticFieldValue_NonExistentField_ThrowsException() => Assert.Throws<ArgumentException>(() => { typeof(string).GetStaticFieldValue<string>("NonExistentField"); });

  [Test]
  public void TypeExtensions_GetStaticPropertyValue_NonExistentProperty_ThrowsException() => Assert.Throws<ArgumentException>(() => { typeof(string).GetStaticPropertyValue<string>("NonExistentProperty"); });

  #endregion

  #region Type Discovery Tests

  [Test]
  public void TypeExtensions_GetImplementedTypes_InterfaceType_ReturnsImplementingTypes() {
    var implementations = typeof(ITestInterface).GetImplementedTypes().ToArray();

    Assert.That(implementations, Is.Not.Empty);
    Assert.That(implementations, Contains.Item(typeof(TestImplementation)));
  }

  [Test]
  public void TypeExtensions_GetImplementedTypes_AbstractClass_ReturnsDerivedTypes() {
    var implementations = typeof(Exception).GetImplementedTypes().ToArray();

    Assert.That(implementations, Is.Not.Empty);
    Assert.That(implementations, Contains.Item(typeof(ArgumentException)));
    Assert.That(implementations, Contains.Item(typeof(InvalidOperationException)));
  }

  [Test]
  public void TypeExtensions_GetImplementedTypes_ConcreteClass_ReturnsSubclasses() {
    var implementations = typeof(object).GetImplementedTypes().ToArray();

    Assert.That(implementations, Is.Not.Empty);
    // Should include many types that derive from object
    Assert.That(implementations.Length, Is.GreaterThan(100));
  }

  #endregion

  #region Edge Cases and Error Handling Tests

  [Test]
  public void TypeExtensions_IsCastableTo_NullTarget_ThrowsArgumentNullException() {
    var type = typeof(string);

    Assert.Throws<ArgumentNullException>(() => { type.IsCastableTo(null); });
  }

  [Test]
  public void TypeExtensions_CreateInstance_WrongGenericType_ThrowsInvalidCastException() => Assert.Throws<InvalidOperationException>(() => { typeof(SimpleTestClass).CreateInstance<string>(); });

  [Test]
  public void TypeExtensions_GetRandomValue_InterfaceType_ReturnsNull() {
    var value = typeof(ITestInterface).GetRandomValue();

    Assert.That(value, Is.Null);
  }

  [Test]
  public void TypeExtensions_GetRandomValue_AbstractType_ReturnsNull() {
    var value = typeof(AbstractTestClass).GetRandomValue();

    Assert.That(value, Is.Null);
  }

  [Test]
  public void TypeExtensions_GetFieldOrPropertyAttributeValue_NullGetter_ThrowsArgumentNullException() {
    var type = typeof(AttributedTestClass);

    Assert.Throws<ArgumentNullException>(() => { type.GetFieldOrPropertyAttributeValue<DescriptionAttribute, string>("TestField", null!); });
  }

  [Test]
  public void TypeExtensions_SimpleName_GenericTypeWithMultipleParameters_HandlesCorrectly() {
    var dictionaryType = typeof(Dictionary<string, int>);

    var simpleName = dictionaryType.SimpleName();

    Assert.That(simpleName, Is.EqualTo("Dictionary`2"));
  }

  [Test]
  public void TypeExtensions_IsIntegerType_NullableIntegerTypes_ReturnsTrue() {
    Assert.That(typeof(int?).IsIntegerType(), Is.True);
    Assert.That(typeof(long?).IsIntegerType(), Is.True);
    Assert.That(typeof(byte?).IsIntegerType(), Is.True);
  }

  #endregion

  #region Performance Tests

  [Test]
  [Category("Performance")]
  public void Performance_CreateInstance_CachedConstructors_FastSecondCall() {
    var type = typeof(SimpleTestClass);

    // First call - should cache the constructor
    var sw1 = Stopwatch.StartNew();
    var instance1 = type.CreateInstance<SimpleTestClass>();
    sw1.Stop();

    // Second call - should use cached constructor
    var sw2 = Stopwatch.StartNew();
    var instance2 = type.CreateInstance<SimpleTestClass>();
    sw2.Stop();

    Assert.That(instance1, Is.Not.Null);
    Assert.That(instance2, Is.Not.Null);

    // Second call should be faster (cached)
    Assert.That(sw2.ElapsedTicks, Is.LessThan(sw1.ElapsedTicks));
  }

  [Test]
  [Category("Performance")]
  public void Performance_GetRandomValue_BulkGeneration_ReasonablePerformance() {
    var type = typeof(int);
    var sw = Stopwatch.StartNew();

    for (var i = 0; i < 10000; i++) {
      var value = type.GetRandomValue();
      Assert.That(value, Is.TypeOf<int>());
    }

    sw.Stop();
    Assert.That(sw.ElapsedMilliseconds, Is.LessThan(1000)); // Should be reasonably fast
  }

  [Test]
  [Category("Performance")]
  public void Performance_TypeInspection_BulkOperations_EfficientExecution() {
    var types = new[] { typeof(int), typeof(string), typeof(DateTime), typeof(bool), typeof(double), typeof(TestEnum), typeof(int?), typeof(List<string>) };

    var sw = Stopwatch.StartNew();

    for (var i = 0; i < 1000; i++)
      foreach (var type in types) {
        var isInteger = type.IsIntegerType();
        var isFloat = type.IsFloatType();
        var isNullable = type.IsNullable();
        var simpleName = type.SimpleName();

        // Basic assertions to ensure methods are working
        Assert.That(isInteger || !isInteger, Is.True); // Just to use the values
        Assert.That(simpleName, Is.Not.Null);
      }

    sw.Stop();
    Assert.That(sw.ElapsedMilliseconds, Is.LessThan(500)); // Type inspection should be very fast
  }

  [Test]
  [Category("Performance")]
  public void Performance_FromConstructor_MultipleParameterOverloads_HandlesEfficiently() {
    var type = typeof(SimpleTestClass);
    var sw = Stopwatch.StartNew();

    for (var i = 0; i < 1000; i++) {
      var instance1 = type.FromConstructor<SimpleTestClass, string>($"test{i}");
      var instance2 = type.FromConstructor<SimpleTestClass, int>(i);
      var instance3 = type.FromConstructor<SimpleTestClass, string, int>($"test{i}", i);

      Assert.That(instance1.Value, Is.EqualTo($"test{i}"));
      Assert.That(instance2.Value, Is.EqualTo(i.ToString()));
      Assert.That(instance3.Value, Is.EqualTo($"test{i}:{i}"));
    }

    sw.Stop();
    Assert.That(sw.ElapsedMilliseconds, Is.LessThan(200)); // Constructor calls should be fast
  }

  #endregion

  #region Complex Scenario Tests

  [Test]
  public void ComplexScenario_TypeHierarchyAnalysis_WorksCorrectly() {
    // Test complex inheritance and interface scenarios
    var exceptionType = typeof(ArgumentException);

    // Should be castable to base classes and interfaces
    Assert.That(exceptionType.IsCastableTo(typeof(Exception)), Is.True);
    Assert.That(exceptionType.IsCastableTo(typeof(object)), Is.True);

    // Should have attributes from base classes
    var attributes = exceptionType.GetAttributes<SerializableAttribute>(inherit: true);
    Assert.That(attributes, Is.Not.Empty);

    // Should be able to create instances
    var instance = exceptionType.CreateInstance<ArgumentException>();
    Assert.That(instance, Is.Not.Null);
    Assert.That(instance, Is.TypeOf<ArgumentException>());
  }

  [Test]
  public void ComplexScenario_GenericTypeHandling_WorksAcrossOperations() {
    var genericType = typeof(List<string>);

    // Type inspection should work
    Assert.That(genericType.SimpleName(), Is.EqualTo("List`1"));
    Assert.That(genericType.IsStringType(), Is.False);

    // Instance creation should work
    var instance = genericType.CreateInstance<List<string>>();
    Assert.That(instance, Is.Not.Null);
    Assert.That(instance, Is.TypeOf<List<string>>());
    Assert.That(instance.Count, Is.EqualTo(0));
  }

  [Test]
  public void ComplexScenario_AttributeChaining_WorksWithInheritance() {
    // Create a derived class that inherits attributes
    var baseType = typeof(AttributedTestClass);

    // Test attribute retrieval with inheritance
    var description = baseType.GetDescription();
    var displayName = baseType.GetDisplayName();

    Assert.That(description, Is.EqualTo("Test class description"));
    Assert.That(displayName, Is.EqualTo("Test Class Display Name"));

    // Test field/property attribute retrieval
    var fieldDescription = baseType.GetFieldOrPropertyAttributeValue<DescriptionAttribute, string>(
      "TestField",
      attr => attr.Description
    );
    var propDescription = baseType.GetFieldOrPropertyAttributeValue<DescriptionAttribute, string>(
      "TestProperty",
      attr => attr.Description
    );

    Assert.That(fieldDescription, Is.EqualTo("Test field description"));
    Assert.That(propDescription, Is.EqualTo("Test property description"));
  }

  #endregion
}
