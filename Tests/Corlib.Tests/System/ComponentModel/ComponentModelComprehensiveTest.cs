using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace System.ComponentModel;

[TestFixture]
public class ComponentModelComprehensiveTest {
  #region MinValueAttribute Tests

  [Test]
  public void MinValueAttribute_Constructor_WithDecimal_SetsValue() {
    var attribute = new MinValueAttribute(123.45m);
    Assert.That(attribute.Value, Is.EqualTo(123.45m));
  }

  [Test]
  public void MinValueAttribute_Constructor_WithInteger_ConvertsToDecimal() {
    var attribute = new MinValueAttribute(42);
    Assert.That(attribute.Value, Is.EqualTo(42m));
  }

  [Test]
  public void MinValueAttribute_Constructor_WithNegativeDecimal_SetsValue() {
    var attribute = new MinValueAttribute(-999.99m);
    Assert.That(attribute.Value, Is.EqualTo(-999.99m));
  }

  [Test]
  public void MinValueAttribute_Constructor_WithZero_SetsValue() {
    var attribute = new MinValueAttribute(0m);
    Assert.That(attribute.Value, Is.EqualTo(0m));
  }

  [Test]
  public void MinValueAttribute_Constructor_WithMaxDecimal_SetsValue() {
    var attribute = new MinValueAttribute(decimal.MaxValue);
    Assert.That(attribute.Value, Is.EqualTo(decimal.MaxValue));
  }

  [Test]
  public void MinValueAttribute_Constructor_WithMinDecimal_SetsValue() {
    var attribute = new MinValueAttribute(decimal.MinValue);
    Assert.That(attribute.Value, Is.EqualTo(decimal.MinValue));
  }

  #endregion

  #region MaxValueAttribute Tests

  [Test]
  public void MaxValueAttribute_Constructor_WithDecimal_SetsValue() {
    var attribute = new MaxValueAttribute(456.78m);
    Assert.That(attribute.Value, Is.EqualTo(456.78m));
  }

  [Test]
  public void MaxValueAttribute_Constructor_WithInteger_ConvertsToDecimal() {
    var attribute = new MaxValueAttribute(100);
    Assert.That(attribute.Value, Is.EqualTo(100m));
  }

  [Test]
  public void MaxValueAttribute_Constructor_WithNegativeDecimal_SetsValue() {
    var attribute = new MaxValueAttribute(-50.25m);
    Assert.That(attribute.Value, Is.EqualTo(-50.25m));
  }

  [Test]
  public void MaxValueAttribute_Constructor_WithZero_SetsValue() {
    var attribute = new MaxValueAttribute(0m);
    Assert.That(attribute.Value, Is.EqualTo(0m));
  }

  [Test]
  public void MaxValueAttribute_Constructor_WithMaxDecimal_SetsValue() {
    var attribute = new MaxValueAttribute(decimal.MaxValue);
    Assert.That(attribute.Value, Is.EqualTo(decimal.MaxValue));
  }

  [Test]
  public void MaxValueAttribute_Constructor_WithMinDecimal_SetsValue() {
    var attribute = new MaxValueAttribute(decimal.MinValue);
    Assert.That(attribute.Value, Is.EqualTo(decimal.MinValue));
  }

  #endregion

  #region EnumDisplayNameAttribute Tests

  public enum TestEnum {
    [EnumDisplayName("First Value")] FirstValue,

    [EnumDisplayName("Second Value")] SecondValue,

    ThirdValue, // No attribute

    [EnumDisplayName("")] EmptyName,

    [EnumDisplayName("  Spaced  ")] SpacedName
  }

  [Test]
  public void EnumDisplayNameAttribute_Constructor_SetsDisplayName() {
    var attribute = new EnumDisplayNameAttribute("Test Display");
    Assert.That(attribute.DisplayName, Is.EqualTo("Test Display"));
  }

  [Test]
  public void EnumDisplayNameAttribute_Constructor_WithEmptyString_SetsEmptyDisplayName() {
    var attribute = new EnumDisplayNameAttribute("");
    Assert.That(attribute.DisplayName, Is.EqualTo(""));
  }

  [Test]
  public void EnumDisplayNameAttribute_Constructor_WithSpaces_PreservesSpaces() {
    var attribute = new EnumDisplayNameAttribute("  Test  ");
    Assert.That(attribute.DisplayName, Is.EqualTo("  Test  "));
  }

  [Test]
  public void EnumDisplayNameAttribute_GetDisplayName_Generic_WithAttribute_ReturnsDisplayName() {
    var result = EnumDisplayNameAttribute.GetDisplayName(TestEnum.FirstValue);
    Assert.That(result, Is.EqualTo("First Value"));
  }

  [Test]
  public void EnumDisplayNameAttribute_GetDisplayName_Generic_WithoutAttribute_ReturnsNull() {
    var result = EnumDisplayNameAttribute.GetDisplayName(TestEnum.ThirdValue);
    Assert.That(result, Is.Null);
  }

  [Test]
  public void EnumDisplayNameAttribute_GetDisplayName_Generic_WithEmptyAttribute_ReturnsEmpty() {
    var result = EnumDisplayNameAttribute.GetDisplayName(TestEnum.EmptyName);
    Assert.That(result, Is.EqualTo(""));
  }

  [Test]
  public void EnumDisplayNameAttribute_GetDisplayName_Generic_WithSpacedAttribute_ReturnsSpaced() {
    var result = EnumDisplayNameAttribute.GetDisplayName(TestEnum.SpacedName);
    Assert.That(result, Is.EqualTo("  Spaced  "));
  }

  [Test]
  public void EnumDisplayNameAttribute_GetDisplayName_TypeObject_WithAttribute_ReturnsDisplayName() {
    var result = EnumDisplayNameAttribute.GetDisplayName(typeof(TestEnum), TestEnum.SecondValue);
    Assert.That(result, Is.EqualTo("Second Value"));
  }

  [Test]
  public void EnumDisplayNameAttribute_GetDisplayName_TypeObject_WithoutAttribute_ReturnsNull() {
    var result = EnumDisplayNameAttribute.GetDisplayName(typeof(TestEnum), TestEnum.ThirdValue);
    Assert.That(result, Is.Null);
  }

  [Test]
  public void EnumDisplayNameAttribute_GetDisplayNameOrDefault_Generic_WithAttribute_ReturnsDisplayName() {
    var result = EnumDisplayNameAttribute.GetDisplayNameOrDefault(TestEnum.FirstValue);
    Assert.That(result, Is.EqualTo("First Value"));
  }

  [Test]
  public void EnumDisplayNameAttribute_GetDisplayNameOrDefault_Generic_WithoutAttribute_ReturnsToString() {
    var result = EnumDisplayNameAttribute.GetDisplayNameOrDefault(TestEnum.ThirdValue);
    Assert.That(result, Is.EqualTo("ThirdValue"));
  }

  [Test]
  public void EnumDisplayNameAttribute_GetDisplayNameOrDefault_Generic_WithEmptyAttribute_ReturnsEmpty() {
    var result = EnumDisplayNameAttribute.GetDisplayNameOrDefault(TestEnum.EmptyName);
    Assert.That(result, Is.EqualTo(""));
  }

  [Test]
  public void EnumDisplayNameAttribute_GetDisplayNameOrDefault_TypeObject_WithAttribute_ReturnsDisplayName() {
    var result = EnumDisplayNameAttribute.GetDisplayNameOrDefault(typeof(TestEnum), TestEnum.SecondValue);
    Assert.That(result, Is.EqualTo("Second Value"));
  }

  [Test]
  public void EnumDisplayNameAttribute_GetDisplayNameOrDefault_TypeObject_WithoutAttribute_ReturnsToString() {
    var result = EnumDisplayNameAttribute.GetDisplayNameOrDefault(typeof(TestEnum), TestEnum.ThirdValue);
    Assert.That(result, Is.EqualTo("ThirdValue"));
  }

  #endregion

  #region BindingList Extensions Tests

  [Test]
  public void BindingListExtensions_Any_WithItems_ReturnsTrue() {
    var list = new BindingList<int> { 1, 2, 3 };
    Assert.That(list.Any(), Is.True);
  }

  [Test]
  public void BindingListExtensions_Any_WithEmptyList_ReturnsFalse() {
    var list = new BindingList<int>();
    Assert.That(list.Any(), Is.False);
  }

  [Test]
  public void BindingListExtensions_Any_WithNullList_ThrowsException() {
    BindingList<int>? list = null;
    Assert.Throws<NullReferenceException>(() => list.Any());
  }

  [Test]
  public void BindingListExtensions_ToArray_WithItems_ReturnsCorrectArray() {
    var list = new BindingList<string> { "a", "b", "c" };
    var result = list.ToArray();

    Assert.That(result, Is.EqualTo(new[] { "a", "b", "c" }));
    Assert.That(result.Length, Is.EqualTo(3));
  }

  [Test]
  public void BindingListExtensions_ToArray_WithEmptyList_ReturnsEmptyArray() {
    var list = new BindingList<string>();
    var result = list.ToArray();

    Assert.That(result, Is.Empty);
    Assert.That(result.Length, Is.EqualTo(0));
  }

  [Test]
  public void BindingListExtensions_ToArray_WithNullList_ThrowsException() {
    BindingList<string>? list = null;
    Assert.Throws<NullReferenceException>(() => list.ToArray());
  }

  [Test]
  public void BindingListExtensions_ToArray_ModifyingOriginalDoesNotAffectArray() {
    var list = new BindingList<int> { 1, 2, 3 };
    var result = list.ToArray();

    list.Add(4);
    Assert.That(result.Length, Is.EqualTo(3));
    Assert.That(result, Is.EqualTo(new[] { 1, 2, 3 }));
  }

  [Test]
  public void BindingListExtensions_AddRange_WithItems_AddsAllItems() {
    var list = new BindingList<int> { 1, 2 };
    list.AddRange(new[] { 3, 4, 5 });

    Assert.That(list.Count, Is.EqualTo(5));
    Assert.That(list.ToArray(), Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
  }

  [Test]
  public void BindingListExtensions_AddRange_WithEmptyEnumerable_DoesNotAddItems() {
    var list = new BindingList<int> { 1, 2 };
    list.AddRange(new int[0]);

    Assert.That(list.Count, Is.EqualTo(2));
    Assert.That(list.ToArray(), Is.EqualTo(new[] { 1, 2 }));
  }

  [Test]
  public void BindingListExtensions_AddRange_WithNullList_ThrowsException() {
    BindingList<int>? list = null;
    Assert.Throws<NullReferenceException>(() => list.AddRange(new[] { 1, 2 }));
  }

  [Test]
  public void BindingListExtensions_AddRange_WithNullItems_ThrowsException() {
    var list = new BindingList<int>();
    Assert.Throws<ArgumentNullException>(() => list.AddRange(null!));
  }

  [Test]
  public void BindingListExtensions_MoveToFront_WithExistingItems_MovesToFront() {
    var list = new BindingList<string> { "a", "b", "c", "d", "e" };
    list.MoveToFront(new[] { "c", "e" });

    Assert.That(list.ToArray(), Is.EqualTo(new[] { "c", "e", "a", "b", "d" }));
  }

  [Test]
  public void BindingListExtensions_MoveToFront_WithNonExistingItems_IgnoresNonExisting() {
    var list = new BindingList<string> { "a", "b", "c" };
    list.MoveToFront(new[] { "b", "x", "c" });

    Assert.That(list.ToArray(), Is.EqualTo(new[] { "b", "x", "c", "a" }));
  }

  [Test]
  public void BindingListExtensions_MoveToFront_WithEmptyItems_DoesNothing() {
    var list = new BindingList<int> { 1, 2, 3 };
    list.MoveToFront(new int[0]);

    Assert.That(list.ToArray(), Is.EqualTo(new[] { 1, 2, 3 }));
  }

  [Test]
  public void BindingListExtensions_MoveToBack_WithExistingItems_MovesToBack() {
    var list = new BindingList<string> { "a", "b", "c", "d", "e" };
    list.MoveToBack(new[] { "b", "d" });

    Assert.That(list.ToArray(), Is.EqualTo(new[] { "a", "c", "e", "b", "d" }));
  }

  [Test]
  public void BindingListExtensions_MoveToBack_WithNonExistingItems_IgnoresNonExisting() {
    var list = new BindingList<string> { "a", "b", "c" };
    list.MoveToBack(new[] { "a", "x", "c" });

    Assert.That(list.ToArray(), Is.EqualTo(new[] { "b", "a", "x", "c" }));
  }

  [Test]
  public void BindingListExtensions_MoveRelative_WithZeroDelta_DoesNothing() {
    var list = new BindingList<int> { 1, 2, 3, 4, 5 };
    list.MoveRelative(new[] { 2, 4 }, 0);

    Assert.That(list.ToArray(), Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
  }

  [Test]
  public void BindingListExtensions_MoveRelative_WithPositiveDelta_MovesForward() {
    var list = new BindingList<string> { "a", "b", "c", "d", "e" };
    list.MoveRelative(new[] { "b", "c" }, 2);

    Assert.That(list.ToArray(), Is.EqualTo(new[] { "a", "d", "e", "b", "c" }));
  }

  [Test]
  public void BindingListExtensions_MoveRelative_WithLargeDelta_ClampsToEdges() {
    var list = new BindingList<int> { 1, 2, 3, 4, 5 };
    list.MoveRelative(new[] { 3, 4 }, 10);

    Assert.That(list.ToArray(), Is.EqualTo(new[] { 1, 2, 5, 3, 4 }));
  }

  [Test]
  public void BindingListExtensions_ReplaceAll_WithNewItems_ReplacesAllItems() {
    var list = new BindingList<int> { 1, 2, 3 };
    list.ReplaceAll(new[] { 4, 5, 6, 7 });

    Assert.That(list.Count, Is.EqualTo(4));
    Assert.That(list.ToArray(), Is.EqualTo(new[] { 4, 5, 6, 7 }));
  }

  [Test]
  public void BindingListExtensions_ReplaceAll_WithEmptyItems_ClearsList() {
    var list = new BindingList<string> { "a", "b", "c" };
    list.ReplaceAll(new string[0]);

    Assert.That(list.Count, Is.EqualTo(0));
    Assert.That(list.ToArray(), Is.Empty);
  }

  [Test]
  public void BindingListExtensions_RefreshAll_WithUpdatedItems_RefreshesCorrectly() {
    var list = new BindingList<TestItem> { new("1", "Item1"), new("2", "Item2"), new("3", "Item3") };

    var updatedItems = new[] { new TestItem("2", "Updated2"), new TestItem("3", "Updated3"), new TestItem("4", "Item4") };

    list.RefreshAll(updatedItems, item => item.Id, (old, updated) => updated);

    Assert.That(list.Count, Is.EqualTo(3));
    Assert.That(list.Any(x => x.Id == "1"), Is.False); // Removed
    Assert.That(list.First(x => x.Id == "2").Name, Is.EqualTo("Updated2")); // Updated
    Assert.That(list.First(x => x.Id == "3").Name, Is.EqualTo("Updated3")); // Updated
    Assert.That(list.Any(x => x.Id == "4"), Is.True); // Added
  }

  [Test]
  public void BindingListExtensions_RefreshAll_WithSameReference_DoesNotReplace() {
    var item1 = new TestItem("1", "Item1");
    var item2 = new TestItem("2", "Item2");
    var list = new BindingList<TestItem> { item1, item2 };

    list.RefreshAll(new[] { item1, item2 }, item => item.Id, (old, updated) => old);

    Assert.That(list.Count, Is.EqualTo(2));
    Assert.That(ReferenceEquals(list[0], item1), Is.True);
    Assert.That(ReferenceEquals(list[1], item2), Is.True);
  }

  [Test]
  public void BindingListExtensions_RemoveWhere_WithMatchingItems_RemovesItems() {
    var list = new BindingList<int> { 1, 2, 3, 4, 5, 6 };
    var removedCount = list.RemoveWhere(x => x % 2 == 0);

    Assert.That(removedCount, Is.EqualTo(3));
    Assert.That(list.ToArray(), Is.EqualTo(new[] { 1, 3, 5 }));
  }

  [Test]
  public void BindingListExtensions_RemoveWhere_WithNoMatches_RemovesNothing() {
    var list = new BindingList<int> { 1, 3, 5 };
    var removedCount = list.RemoveWhere(x => x % 2 == 0);

    Assert.That(removedCount, Is.EqualTo(0));
    Assert.That(list.ToArray(), Is.EqualTo(new[] { 1, 3, 5 }));
  }

  [Test]
  public void BindingListExtensions_RemoveWhere_WithAllMatches_RemovesAll() {
    var list = new BindingList<string> { "test", "hello", "world" };
    var removedCount = list.RemoveWhere(x => x.Length > 0);

    Assert.That(removedCount, Is.EqualTo(3));
    Assert.That(list.Count, Is.EqualTo(0));
  }

  [Test]
  public void BindingListExtensions_Overhaul_ExecutesActionAndRestoresEvents() {
    var list = new BindingList<int> { 1, 2, 3 };
    var eventFired = false;

    list.ListChanged += (s, e) => eventFired = true;

    list.Overhaul(
      l => {
        l.Add(4);
        l.Add(5);
        // Events should be disabled during overhaul
        Assert.That(eventFired, Is.False);
      }
    );

    // Event should fire after overhaul completes
    Assert.That(eventFired, Is.True);
    Assert.That(list.Count, Is.EqualTo(5));
  }

  [Test]
  public void BindingListExtensions_Overhaul_WithEventsDisabled_DoesNotRestoreEvents() {
    var list = new BindingList<int> { 1, 2, 3 };
    list.RaiseListChangedEvents = false;

    var eventFired = false;
    list.ListChanged += (s, e) => eventFired = true;

    list.Overhaul(l => l.Add(4));

    Assert.That(eventFired, Is.False);
    Assert.That(list.RaiseListChangedEvents, Is.False);
  }

  [Test]
  public void BindingListExtensions_Overhaul_WithException_RestoresEventState() {
    var list = new BindingList<int> { 1, 2, 3 };
    var originalEventState = list.RaiseListChangedEvents;

    Assert.Throws<InvalidOperationException>(() => { list.Overhaul(l => throw new InvalidOperationException("Test exception")); });

    Assert.That(list.RaiseListChangedEvents, Is.EqualTo(originalEventState));
  }

  #endregion

  #region Performance Tests

  [Test]
  public void BindingListExtensions_Any_PerformanceTest_LargeList() {
    var list = new BindingList<int>(Enumerable.Range(1, 100000).ToList());

    var sw = Stopwatch.StartNew();
    var result = list.Any();
    sw.Stop();

    Assert.That(result, Is.True);
    Assert.That(sw.ElapsedMilliseconds, Is.LessThan(10)); // Should be very fast
  }

  [Test]
  public void BindingListExtensions_ToArray_PerformanceTest_LargeList() {
    var sourceArray = Enumerable.Range(1, 50000).ToArray();
    var list = new BindingList<int>(sourceArray);

    var sw = Stopwatch.StartNew();
    var result = list.ToArray();
    sw.Stop();

    Assert.That(result.Length, Is.EqualTo(50000));
    Assert.That(sw.ElapsedMilliseconds, Is.LessThan(100));
  }

  [Test]
  public void BindingListExtensions_AddRange_PerformanceTest_LargeRange() {
    var list = new BindingList<int>();
    var itemsToAdd = Enumerable.Range(1, 10000).ToArray();

    var sw = Stopwatch.StartNew();
    list.AddRange(itemsToAdd);
    sw.Stop();

    Assert.That(list.Count, Is.EqualTo(10000));
    Assert.That(sw.ElapsedMilliseconds, Is.LessThan(1000));
  }

  #endregion

  #region Helper Classes

  private class TestItem {
    public string Id { get; }
    public string Name { get; }

    public TestItem(string id, string name) {
      this.Id = id;
      this.Name = name;
    }
  }

  #endregion

  #region DefaultValueAttribute Extensions Tests

  private class TestClassWithDefaultValues {
    [DefaultValue(42)]
    public int IntProperty { get; set; }

    [DefaultValue(3.14)]
    public double DoubleProperty { get; set; }

    [DefaultValue("DefaultString")]
    public string StringProperty { get; set; }

    [DefaultValue(true)]
    public bool BoolProperty { get; set; }

    [DefaultValue(123L)]
    public long LongProperty { get; set; }

    [DefaultValue(99.99f)]
    public float FloatProperty { get; set; }

    [DefaultValue(null)]
    public object NullableProperty { get; set; }

    // Property without DefaultValue attribute
    public int NoDefaultProperty { get; set; }

    // Read-only property with DefaultValue (should be skipped)
    [DefaultValue(100)]
    public int ReadOnlyProperty { get; private set; }

    // Private property with DefaultValue
    [DefaultValue(50)]
    private int PrivateProperty { get; set; }

    public int GetPrivateProperty() => this.PrivateProperty;
  }

  private class TestClassWithNumericConversions {
    [DefaultValue(42)] // int to byte
    public byte ByteProperty { get; set; }

    [DefaultValue(100)] // int to short
    public short ShortProperty { get; set; }

    [DefaultValue(1000)] // int to long
    public long LongProperty { get; set; }

    [DefaultValue(5.5f)] // float to double
    public double DoubleFromFloatProperty { get; set; }

    [DefaultValue(10)] // int to decimal
    public decimal DecimalProperty { get; set; }

    [DefaultValue(-5)] // Negative int to signed type
    public sbyte SignedByteProperty { get; set; }

    [DefaultValue(65)] // int to char
    public char CharProperty { get; set; }
  }

  private class InheritedTestClass : TestClassWithDefaultValues {
    [DefaultValue("InheritedDefault")]
    public string InheritedProperty { get; set; }
  }

  [Test]
  public void DefaultValueAttributeExtensions_SetPropertiesToDefaultValues_SetsAllPublicProperties() {
    var instance = new TestClassWithDefaultValues { IntProperty = 0, DoubleProperty = 0.0, StringProperty = null, BoolProperty = false, LongProperty = 0L, FloatProperty = 0.0f, NullableProperty = "NotNull", NoDefaultProperty = 999 };

    instance.SetPropertiesToDefaultValues();

    Assert.That(instance.IntProperty, Is.EqualTo(42));
    Assert.That(instance.DoubleProperty, Is.EqualTo(3.14));
    Assert.That(instance.StringProperty, Is.EqualTo("DefaultString"));
    Assert.That(instance.BoolProperty, Is.True);
    Assert.That(instance.LongProperty, Is.EqualTo(123L));
    Assert.That(instance.FloatProperty, Is.EqualTo(99.99f));
    Assert.That(instance.NullableProperty, Is.Null);
    Assert.That(instance.NoDefaultProperty, Is.EqualTo(999)); // Unchanged
  }

  [Test]
  public void DefaultValueAttributeExtensions_SetPropertiesToDefaultValues_WithNullInstance_DoesNotThrow() {
    TestClassWithDefaultValues? instance = null;
    Assert.DoesNotThrow(() => instance.SetPropertiesToDefaultValues());
  }

  [Test]
  public void DefaultValueAttributeExtensions_SetPropertiesToDefaultValues_WithAlsoNonPublic_SetsPrivateProperties() {
    var instance = new TestClassWithDefaultValues();
    instance.SetPropertiesToDefaultValues(alsoNonPublic: true);

    Assert.That(instance.GetPrivateProperty(), Is.EqualTo(50));
  }

  [Test]
  public void DefaultValueAttributeExtensions_SetPropertiesToDefaultValues_WithoutAlsoNonPublic_IgnoresPrivateProperties() {
    var instance = new TestClassWithDefaultValues();
    instance.SetPropertiesToDefaultValues(alsoNonPublic: false);

    Assert.That(instance.GetPrivateProperty(), Is.EqualTo(0)); // Unchanged
  }

  [Test]
  public void DefaultValueAttributeExtensions_SetPropertiesToDefaultValues_WithNumericConversions_ConvertsCorrectly() {
    var instance = new TestClassWithNumericConversions();
    instance.SetPropertiesToDefaultValues();

    Assert.That(instance.ByteProperty, Is.EqualTo((byte)42));
    Assert.That(instance.ShortProperty, Is.EqualTo((short)100));
    Assert.That(instance.LongProperty, Is.EqualTo(1000L));
    Assert.That(instance.DoubleFromFloatProperty, Is.EqualTo(5.5));
    Assert.That(instance.DecimalProperty, Is.EqualTo(10m));
    Assert.That(instance.SignedByteProperty, Is.EqualTo((sbyte)-5));
    Assert.That(instance.CharProperty, Is.EqualTo('A')); // ASCII 65
  }

  [Test]
  public void DefaultValueAttributeExtensions_SetPropertiesToDefaultValues_WithInheritance_SetsInheritedProperties() {
    var instance = new InheritedTestClass();
    instance.SetPropertiesToDefaultValues(flattenHierarchies: true);

    Assert.That(instance.IntProperty, Is.EqualTo(42)); // From base class
    Assert.That(instance.StringProperty, Is.EqualTo("DefaultString")); // From base class
    Assert.That(instance.InheritedProperty, Is.EqualTo("InheritedDefault")); // From derived class
  }

  [Test]
  public void DefaultValueAttributeExtensions_SetPropertiesToDefaultValues_WithoutFlattenHierarchies_OnlySetsDeclaredProperties() {
    var instance = new InheritedTestClass();
    instance.SetPropertiesToDefaultValues(flattenHierarchies: false);

    Assert.That(instance.IntProperty, Is.EqualTo(0)); // Unchanged from base
    Assert.That(instance.InheritedProperty, Is.EqualTo("InheritedDefault")); // Set from derived
  }

  #endregion

  #region SynchronizeInvoke Extensions Tests

  private class MockSynchronizeInvoke : ISynchronizeInvoke {
    public bool InvokeRequired { get; set; }
    public bool InvokeCalled { get; private set; }
    public bool BeginInvokeCalled { get; private set; }
    public Action<Delegate, object[]> InvokeCallback { get; set; }
    public Action<Delegate, object[]> BeginInvokeCallback { get; set; }

    public IAsyncResult BeginInvoke(Delegate method, object[] args) {
      this.BeginInvokeCalled = true;
      this.BeginInvokeCallback?.Invoke(method, args);
      return null;
    }

    public object EndInvoke(IAsyncResult result) => null;

    public object Invoke(Delegate method, object[] args) {
      this.InvokeCalled = true;
      this.InvokeCallback?.Invoke(method, args);
      return null;
    }
  }

  [Test]
  public void SynchronizeInvokeExtensions_SafeInvoke_WhenInvokeNotRequired_CallsDirectly() {
    var mock = new MockSynchronizeInvoke { InvokeRequired = false };
    var actionCalled = false;

    var result = mock.SafeInvoke(m => actionCalled = true);

    Assert.That(result, Is.True); // Returns true when called directly
    Assert.That(actionCalled, Is.True);
    Assert.That(mock.InvokeCalled, Is.False);
    Assert.That(mock.BeginInvokeCalled, Is.False);
  }

  [Test]
  public void SynchronizeInvokeExtensions_SafeInvoke_WhenInvokeRequired_Synchronous_CallsInvoke() {
    var mock = new MockSynchronizeInvoke { InvokeRequired = true };

    mock.InvokeCallback = (method, args) => { method.DynamicInvoke(args); };

    var result = mock.SafeInvoke(_ => { }, async: false);

    Assert.That(result, Is.False); // Returns false when marshalled
    Assert.That(mock.InvokeCalled, Is.True);
    Assert.That(mock.BeginInvokeCalled, Is.False);
  }

  [Test]
  public void SynchronizeInvokeExtensions_SafeInvoke_WhenInvokeRequired_Asynchronous_CallsBeginInvoke() {
    var mock = new MockSynchronizeInvoke { InvokeRequired = true };

    mock.BeginInvokeCallback = (method, args) => { method.DynamicInvoke(args); };

    var result = mock.SafeInvoke(_ => { }, async: true);

    Assert.That(result, Is.False); // Returns false when marshalled
    Assert.That(mock.BeginInvokeCalled, Is.True);
    Assert.That(mock.InvokeCalled, Is.False);
  }

  [Test]
  public void SynchronizeInvokeExtensions_SafeInvoke_PassesCorrectInstance() {
    var mock = new MockSynchronizeInvoke { InvokeRequired = false };
    MockSynchronizeInvoke? passedInstance = null;

    mock.SafeInvoke(m => passedInstance = m);

    Assert.That(passedInstance, Is.SameAs(mock));
  }

  [Test]
  public void SynchronizeInvokeExtensions_SafeInvoke_WithComplexAction_ExecutesCorrectly() {
    var mock = new MockSynchronizeInvoke { InvokeRequired = false };
    var values = new List<int>();

    mock.SafeInvoke(
      m => {
        values.Add(1);
        values.Add(2);
        values.Add(3);
      }
    );

    Assert.That(values, Is.EqualTo(new[] { 1, 2, 3 }));
  }

  #endregion

  #region Additional ComponentModel Classes Tests

#if NET40_OR_GREATER
  [Test]
  public void SortableBindingList_Constructor_CreatesEmptyList() {
    var list = new SortableBindingList<int>();
    
    Assert.That(list.Count, Is.EqualTo(0));
    Assert.That(list.AllowNew, Is.True);
    Assert.That(list.AllowEdit, Is.True);
    Assert.That(list.AllowRemove, Is.True);
    Assert.That(list.RaiseListChangedEvents, Is.True);
  }

  [Test]
  public void SortableBindingList_ImplementsINotifyCollectionChanged() {
    var list = new SortableBindingList<string>();
    
    Assert.That(list, Is.InstanceOf<INotifyCollectionChanged>());
  }

  [Test]
  public void SortableBindingList_AddItem_RaisesCollectionChangedEvent() {
    var list = new SortableBindingList<string>();
    var eventRaised = false;
    NotifyCollectionChangedEventArgs? eventArgs = null;
    
    ((INotifyCollectionChanged)list).CollectionChanged += (sender, args) => {
      eventRaised = true;
      eventArgs = args;
    };
    
    list.Add("test");
    
    Assert.That(eventRaised, Is.True);
    Assert.That(eventArgs?.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
  }

#endif

  #endregion
}
