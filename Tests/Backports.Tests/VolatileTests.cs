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
using System.Threading;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Volatile")]
public class VolatileTests {

  #region Read Bool

  [Test]
  [Category("HappyPath")]
  public void Volatile_Read_Bool_ReturnsCorrectValue() {
    var value = true;
    var result = Volatile.Read(ref value);
    Assert.That(result, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Volatile_Read_Bool_False_ReturnsCorrectValue() {
    var value = false;
    var result = Volatile.Read(ref value);
    Assert.That(result, Is.False);
  }

  #endregion

  #region Read Byte

  [Test]
  [Category("HappyPath")]
  public void Volatile_Read_Byte_ReturnsCorrectValue() {
    byte value = 42;
    var result = Volatile.Read(ref value);
    Assert.That(result, Is.EqualTo((byte)42));
  }

  [Test]
  [Category("EdgeCase")]
  public void Volatile_Read_Byte_MaxValue_ReturnsCorrectValue() {
    var value = byte.MaxValue;
    var result = Volatile.Read(ref value);
    Assert.That(result, Is.EqualTo(byte.MaxValue));
  }

  #endregion

  #region Read Int16

  [Test]
  [Category("HappyPath")]
  public void Volatile_Read_Short_ReturnsCorrectValue() {
    short value = 1234;
    var result = Volatile.Read(ref value);
    Assert.That(result, Is.EqualTo((short)1234));
  }

  [Test]
  [Category("EdgeCase")]
  public void Volatile_Read_Short_NegativeValue_ReturnsCorrectValue() {
    short value = -1234;
    var result = Volatile.Read(ref value);
    Assert.That(result, Is.EqualTo((short)-1234));
  }

  #endregion

  #region Read Int32

  [Test]
  [Category("HappyPath")]
  public void Volatile_Read_Int_ReturnsCorrectValue() {
    var value = 12345678;
    var result = Volatile.Read(ref value);
    Assert.That(result, Is.EqualTo(12345678));
  }

  [Test]
  [Category("EdgeCase")]
  public void Volatile_Read_Int_MinValue_ReturnsCorrectValue() {
    var value = int.MinValue;
    var result = Volatile.Read(ref value);
    Assert.That(result, Is.EqualTo(int.MinValue));
  }

  #endregion

  #region Read Int64

  [Test]
  [Category("HappyPath")]
  public void Volatile_Read_Long_ReturnsCorrectValue() {
    long value = 123456789012345L;
    var result = Volatile.Read(ref value);
    Assert.That(result, Is.EqualTo(123456789012345L));
  }

  [Test]
  [Category("EdgeCase")]
  public void Volatile_Read_Long_MaxValue_ReturnsCorrectValue() {
    var value = long.MaxValue;
    var result = Volatile.Read(ref value);
    Assert.That(result, Is.EqualTo(long.MaxValue));
  }

  #endregion

  #region Read Float

  [Test]
  [Category("HappyPath")]
  public void Volatile_Read_Float_ReturnsCorrectValue() {
    var value = 3.14f;
    var result = Volatile.Read(ref value);
    Assert.That(result, Is.EqualTo(3.14f));
  }

  [Test]
  [Category("EdgeCase")]
  public void Volatile_Read_Float_NaN_ReturnsNaN() {
    var value = float.NaN;
    var result = Volatile.Read(ref value);
    Assert.That(float.IsNaN(result), Is.True);
  }

  #endregion

  #region Read Double

  [Test]
  [Category("HappyPath")]
  public void Volatile_Read_Double_ReturnsCorrectValue() {
    var value = 3.14159265359;
    var result = Volatile.Read(ref value);
    Assert.That(result, Is.EqualTo(3.14159265359));
  }

  [Test]
  [Category("EdgeCase")]
  public void Volatile_Read_Double_Infinity_ReturnsCorrectValue() {
    var value = double.PositiveInfinity;
    var result = Volatile.Read(ref value);
    Assert.That(double.IsPositiveInfinity(result), Is.True);
  }

  #endregion

  #region Read IntPtr

  [Test]
  [Category("HappyPath")]
  public void Volatile_Read_IntPtr_ReturnsCorrectValue() {
    var value = new IntPtr(12345);
    var result = Volatile.Read(ref value);
    Assert.That(result, Is.EqualTo(new IntPtr(12345)));
  }

  #endregion

  #region Read Reference Type

  [Test]
  [Category("HappyPath")]
  public void Volatile_Read_ReferenceType_ReturnsCorrectValue() {
    var obj = new TestClass { Value = 42 };
    var result = Volatile.Read(ref obj);
    Assert.That(result, Is.SameAs(obj));
    Assert.That(result.Value, Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void Volatile_Read_ReferenceType_NullValue_ReturnsNull() {
    TestClass obj = null;
    var result = Volatile.Read(ref obj);
    Assert.That(result, Is.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void Volatile_Read_String_ReturnsCorrectValue() {
    var value = "Hello, World!";
    var result = Volatile.Read(ref value);
    Assert.That(result, Is.EqualTo("Hello, World!"));
  }

  #endregion

  #region Write Bool

  [Test]
  [Category("HappyPath")]
  public void Volatile_Write_Bool_SetsCorrectValue() {
    var value = false;
    Volatile.Write(ref value, true);
    Assert.That(value, Is.True);
  }

  #endregion

  #region Write Byte

  [Test]
  [Category("HappyPath")]
  public void Volatile_Write_Byte_SetsCorrectValue() {
    byte value = 0;
    Volatile.Write(ref value, 42);
    Assert.That(value, Is.EqualTo((byte)42));
  }

  #endregion

  #region Write Int16

  [Test]
  [Category("HappyPath")]
  public void Volatile_Write_Short_SetsCorrectValue() {
    short value = 0;
    Volatile.Write(ref value, 1234);
    Assert.That(value, Is.EqualTo((short)1234));
  }

  #endregion

  #region Write Int32

  [Test]
  [Category("HappyPath")]
  public void Volatile_Write_Int_SetsCorrectValue() {
    var value = 0;
    Volatile.Write(ref value, 12345678);
    Assert.That(value, Is.EqualTo(12345678));
  }

  #endregion

  #region Write Int64

  [Test]
  [Category("HappyPath")]
  public void Volatile_Write_Long_SetsCorrectValue() {
    long value = 0;
    Volatile.Write(ref value, 123456789012345L);
    Assert.That(value, Is.EqualTo(123456789012345L));
  }

  #endregion

  #region Write Float

  [Test]
  [Category("HappyPath")]
  public void Volatile_Write_Float_SetsCorrectValue() {
    var value = 0f;
    Volatile.Write(ref value, 3.14f);
    Assert.That(value, Is.EqualTo(3.14f));
  }

  #endregion

  #region Write Double

  [Test]
  [Category("HappyPath")]
  public void Volatile_Write_Double_SetsCorrectValue() {
    var value = 0.0;
    Volatile.Write(ref value, 3.14159265359);
    Assert.That(value, Is.EqualTo(3.14159265359));
  }

  #endregion

  #region Write IntPtr

  [Test]
  [Category("HappyPath")]
  public void Volatile_Write_IntPtr_SetsCorrectValue() {
    var value = IntPtr.Zero;
    Volatile.Write(ref value, new IntPtr(12345));
    Assert.That(value, Is.EqualTo(new IntPtr(12345)));
  }

  #endregion

  #region Write Reference Type

  [Test]
  [Category("HappyPath")]
  public void Volatile_Write_ReferenceType_SetsCorrectValue() {
    TestClass value = null;
    var obj = new TestClass { Value = 42 };
    Volatile.Write(ref value, obj);
    Assert.That(value, Is.SameAs(obj));
  }

  [Test]
  [Category("HappyPath")]
  public void Volatile_Write_ReferenceType_SetToNull() {
    var value = new TestClass { Value = 42 };
    Volatile.Write(ref value, null);
    Assert.That(value, Is.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void Volatile_Write_String_SetsCorrectValue() {
    var value = "";
    Volatile.Write(ref value, "Hello, World!");
    Assert.That(value, Is.EqualTo("Hello, World!"));
  }

  #endregion

  #region Thread Safety

  [Test]
  [Category("Integration")]
  public void Volatile_ReadWrite_ThreadSafety() {
    var sharedValue = 0;
    var iterations = 10000;
    var writerDone = false;

    var writer = new Thread(() => {
      for (var i = 1; i <= iterations; ++i)
        Volatile.Write(ref sharedValue, i);
      Volatile.Write(ref writerDone, true);
    });

    var lastRead = 0;
    var monotonic = true;
    var reader = new Thread(() => {
      while (!Volatile.Read(ref writerDone)) {
        var current = Volatile.Read(ref sharedValue);
        if (current < lastRead)
          monotonic = false;
        lastRead = current;
      }
    });

    writer.Start();
    reader.Start();
    writer.Join();
    reader.Join();

    Assert.That(monotonic, Is.True);
    Assert.That(Volatile.Read(ref sharedValue), Is.EqualTo(iterations));
  }

  #endregion

  #region Unsigned Types

  [Test]
  [Category("HappyPath")]
  public void Volatile_Read_UShort_ReturnsCorrectValue() {
    ushort value = 65000;
    var result = Volatile.Read(ref value);
    Assert.That(result, Is.EqualTo((ushort)65000));
  }

  [Test]
  [Category("HappyPath")]
  public void Volatile_Write_UShort_SetsCorrectValue() {
    ushort value = 0;
    Volatile.Write(ref value, 65000);
    Assert.That(value, Is.EqualTo((ushort)65000));
  }

  [Test]
  [Category("HappyPath")]
  public void Volatile_Read_UInt_ReturnsCorrectValue() {
    uint value = 4000000000;
    var result = Volatile.Read(ref value);
    Assert.That(result, Is.EqualTo(4000000000U));
  }

  [Test]
  [Category("HappyPath")]
  public void Volatile_Write_UInt_SetsCorrectValue() {
    uint value = 0;
    Volatile.Write(ref value, 4000000000);
    Assert.That(value, Is.EqualTo(4000000000U));
  }

  [Test]
  [Category("HappyPath")]
  public void Volatile_Read_ULong_ReturnsCorrectValue() {
    ulong value = 18000000000000000000UL;
    var result = Volatile.Read(ref value);
    Assert.That(result, Is.EqualTo(18000000000000000000UL));
  }

  [Test]
  [Category("HappyPath")]
  public void Volatile_Write_ULong_SetsCorrectValue() {
    ulong value = 0;
    Volatile.Write(ref value, 18000000000000000000UL);
    Assert.That(value, Is.EqualTo(18000000000000000000UL));
  }

  [Test]
  [Category("HappyPath")]
  public void Volatile_Read_SByte_ReturnsCorrectValue() {
    sbyte value = -100;
    var result = Volatile.Read(ref value);
    Assert.That(result, Is.EqualTo((sbyte)-100));
  }

  [Test]
  [Category("HappyPath")]
  public void Volatile_Write_SByte_SetsCorrectValue() {
    sbyte value = 0;
    Volatile.Write(ref value, -100);
    Assert.That(value, Is.EqualTo((sbyte)-100));
  }

  [Test]
  [Category("HappyPath")]
  public void Volatile_Read_UIntPtr_ReturnsCorrectValue() {
    var value = new UIntPtr(12345);
    var result = Volatile.Read(ref value);
    Assert.That(result, Is.EqualTo(new UIntPtr(12345)));
  }

  [Test]
  [Category("HappyPath")]
  public void Volatile_Write_UIntPtr_SetsCorrectValue() {
    var value = UIntPtr.Zero;
    Volatile.Write(ref value, new UIntPtr(12345));
    Assert.That(value, Is.EqualTo(new UIntPtr(12345)));
  }

  #endregion

  #region Helper Types

  private class TestClass {
    public int Value { get; set; }
  }

  #endregion

}
