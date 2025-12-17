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
[Category("Array")]
public class ArrayTests {

  #region Array.Empty<T>

  [Test]
  [Category("HappyPath")]
  public void ArrayEmpty_ReturnsEmptyArray() {
    var result = Array.Empty<int>();
    Assert.That(result, Is.Empty);
  }

  [Test]
  [Category("HappyPath")]
  public void ArrayEmpty_ReturnsSameInstance() {
    var result1 = Array.Empty<int>();
    var result2 = Array.Empty<int>();
    Assert.That(result1, Is.SameAs(result2));
  }

  [Test]
  [Category("HappyPath")]
  public void ArrayEmpty_DifferentTypesReturnDifferentInstances() {
    var intArray = Array.Empty<int>();
    var stringArray = Array.Empty<string>();
    Assert.That(intArray, Is.Not.SameAs(stringArray));
  }

  [Test]
  [Category("HappyPath")]
  public void ArrayEmpty_ReturnsZeroLengthArray() {
    var result = Array.Empty<object>();
    Assert.That(result.Length, Is.EqualTo(0));
  }

  #endregion

  #region Array.Fill<T>

  [Test]
  [Category("HappyPath")]
  public void ArrayFill_FillsEntireArray() {
    var array = new int[5];
    Array.Fill(array, 42);
    Assert.That(array, Is.All.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void ArrayFill_FillsWithDifferentTypes() {
    var array = new string[3];
    Array.Fill(array, "test");
    Assert.That(array, Is.All.EqualTo("test"));
  }

  [Test]
  [Category("HappyPath")]
  public void ArrayFill_FillsWithNull() {
    var array = new object[3];
    array[0] = new object();
    array[1] = new object();
    array[2] = new object();
    Array.Fill(array, (object)null);
    Assert.That(array, Is.All.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void ArrayFill_WithRange_FillsSpecifiedRange() {
    var array = new int[] { 1, 2, 3, 4, 5 };
    Array.Fill(array, 99, 1, 3);
    Assert.That(array[0], Is.EqualTo(1));
    Assert.That(array[1], Is.EqualTo(99));
    Assert.That(array[2], Is.EqualTo(99));
    Assert.That(array[3], Is.EqualTo(99));
    Assert.That(array[4], Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void ArrayFill_WithZeroCount_DoesNothing() {
    var array = new int[] { 1, 2, 3 };
    Array.Fill(array, 99, 1, 0);
    Assert.That(array, Is.EqualTo(new[] { 1, 2, 3 }));
  }

  [Test]
  [Category("Exception")]
  public void ArrayFill_WithNullArray_ThrowsArgumentNullException() {
    int[] array = null;
    Assert.Throws<ArgumentNullException>(() => Array.Fill(array, 42));
  }

  [Test]
  [Category("Exception")]
  public void ArrayFill_WithNegativeIndex_ThrowsArgumentOutOfRangeException() {
    var array = new int[5];
    Assert.Throws<ArgumentOutOfRangeException>(() => Array.Fill(array, 42, -1, 1));
  }

  [Test]
  [Category("Exception")]
  public void ArrayFill_WithNegativeCount_ThrowsArgumentOutOfRangeException() {
    var array = new int[5];
    Assert.Throws<ArgumentOutOfRangeException>(() => Array.Fill(array, 42, 0, -1));
  }

  [Test]
  [Category("EdgeCase")]
  public void ArrayFill_WithEmptyArray_DoesNothing() {
    var array = new int[0];
    Assert.DoesNotThrow(() => Array.Fill(array, 42));
  }

  #endregion

  #region Array.MaxLength

  [Test]
  [Category("HappyPath")]
  public void MaxLength_ReturnsCorrectValue() {
    Assert.That(Array.MaxLength, Is.EqualTo(0x7FFFFFC7));
  }

  #endregion

}
