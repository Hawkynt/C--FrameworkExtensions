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
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Lazy")]
public class LazyTests {

  #region Basic Initialization

  [Test]
  [Category("HappyPath")]
  public void Lazy_DefaultConstructor_CreatesInstance() {
    var lazy = new Lazy<TestClass>();
    Assert.That(lazy.IsValueCreated, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Lazy_WithFactory_CreatesInstance() {
    var lazy = new Lazy<int>(() => 42);
    Assert.That(lazy.IsValueCreated, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Lazy_Value_InvokesFactory() {
    var callCount = 0;
    var lazy = new Lazy<int>(() => {
      ++callCount;
      return 42;
    });

    var value = lazy.Value;
    Assert.That(value, Is.EqualTo(42));
    Assert.That(callCount, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Lazy_Value_CachesResult() {
    var callCount = 0;
    var lazy = new Lazy<int>(() => {
      ++callCount;
      return 42;
    });

    _ = lazy.Value;
    _ = lazy.Value;
    _ = lazy.Value;

    Assert.That(callCount, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Lazy_IsValueCreated_ReturnsTrueAfterAccess() {
    var lazy = new Lazy<int>(() => 42);
    Assert.That(lazy.IsValueCreated, Is.False);

    _ = lazy.Value;

    Assert.That(lazy.IsValueCreated, Is.True);
  }

  #endregion

  #region Reference Types

  [Test]
  [Category("HappyPath")]
  public void Lazy_ReferenceType_CreatesCorrectInstance() {
    var lazy = new Lazy<TestClass>(() => new TestClass { Value = 100 });
    Assert.That(lazy.Value.Value, Is.EqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void Lazy_ReferenceType_ReturnsSameInstance() {
    var lazy = new Lazy<TestClass>(() => new TestClass());
    var first = lazy.Value;
    var second = lazy.Value;
    Assert.That(first, Is.SameAs(second));
  }

  [Test]
  [Category("HappyPath")]
  public void Lazy_DefaultConstructor_UsesParameterlessConstructor() {
    var lazy = new Lazy<TestClass>();
    Assert.That(lazy.Value, Is.Not.Null);
    Assert.That(lazy.Value.Value, Is.EqualTo(0));
  }

  #endregion

  #region Value Types

  [Test]
  [Category("HappyPath")]
  public void Lazy_ValueType_ReturnsCorrectValue() {
    var lazy = new Lazy<int>(() => 123);
    Assert.That(lazy.Value, Is.EqualTo(123));
  }

  [Test]
  [Category("HappyPath")]
  public void Lazy_Struct_ReturnsCorrectValue() {
    var lazy = new Lazy<TestStruct>(() => new TestStruct { X = 10, Y = 20 });
    Assert.That(lazy.Value.X, Is.EqualTo(10));
    Assert.That(lazy.Value.Y, Is.EqualTo(20));
  }

  #endregion

  #region ToString

  [Test]
  [Category("HappyPath")]
  public void Lazy_ToString_BeforeValueCreated_ReturnsNotCreatedMessage() {
    var lazy = new Lazy<int>(() => 42);
    var str = lazy.ToString();
    // The message varies by locale, so just check it's not the value
    Assert.That(str, Is.Not.EqualTo("42"));
  }

  [Test]
  [Category("HappyPath")]
  public void Lazy_ToString_AfterValueCreated_ReturnsValueString() {
    var lazy = new Lazy<int>(() => 42);
    _ = lazy.Value;
    var str = lazy.ToString();
    Assert.That(str, Is.EqualTo("42"));
  }

  #endregion

  #region Exception Handling

  [Test]
  [Category("Exception")]
  public void Lazy_FactoryThrows_ExceptionPropagates() {
    var lazy = new Lazy<int>(() => throw new InvalidOperationException("Test exception"));
    Assert.Throws<InvalidOperationException>(() => _ = lazy.Value);
  }

  [Test]
  [Category("Exception")]
  public void Lazy_NullFactory_ThrowsArgumentNullException() {
    Assert.Throws<ArgumentNullException>(() => new Lazy<int>(null));
  }

  #endregion

  #region Nullable Types

  [Test]
  [Category("HappyPath")]
  public void Lazy_NullableValue_ReturnsNull() {
    var lazy = new Lazy<string>(() => null);
    Assert.That(lazy.Value, Is.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void Lazy_NullableValue_ReturnsNonNull() {
    var lazy = new Lazy<string>(() => "hello");
    Assert.That(lazy.Value, Is.EqualTo("hello"));
  }

  #endregion

  #region Complex Factory Logic

  [Test]
  [Category("HappyPath")]
  public void Lazy_ComplexFactory_ExecutesCorrectly() {
    var sequence = new int[3];
    var index = 0;

    var lazy = new Lazy<int>(() => {
      for (var i = 0; i < sequence.Length; ++i)
        sequence[i] = ++index;
      return sequence[0] + sequence[1] + sequence[2];
    });

    Assert.That(lazy.Value, Is.EqualTo(6));
    Assert.That(sequence, Is.EqualTo(new[] { 1, 2, 3 }));
  }

  #endregion

  #region Helper Types

  private class TestClass {
    public int Value { get; set; }
  }

  private struct TestStruct {
    public int X;
    public int Y;
  }

  #endregion

}
