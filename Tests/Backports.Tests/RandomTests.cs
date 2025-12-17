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
using System.Linq;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Random")]
public class RandomTests {

  #region Random.Shuffle

  [Test]
  [Category("HappyPath")]
  public void Shuffle_ArrayIsModified() {
    var random = new Random(42);
    var array = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    var original = (int[])array.Clone();
    random.Shuffle(array);
    Assert.That(array, Is.Not.EqualTo(original));
  }

  [Test]
  [Category("HappyPath")]
  public void Shuffle_ContainsSameElements() {
    var random = new Random(42);
    var array = new[] { 1, 2, 3, 4, 5 };
    var originalSum = array.Sum();
    random.Shuffle(array);
    Assert.That(array.Sum(), Is.EqualTo(originalSum));
  }

  [Test]
  [Category("HappyPath")]
  public void Shuffle_PreservesLength() {
    var random = new Random(42);
    var array = new[] { 1, 2, 3, 4, 5 };
    random.Shuffle(array);
    Assert.That(array.Length, Is.EqualTo(5));
  }

  [Test]
  [Category("EdgeCase")]
  public void Shuffle_SingleElement_DoesNotChange() {
    var random = new Random(42);
    var array = new[] { 42 };
    random.Shuffle(array);
    Assert.That(array[0], Is.EqualTo(42));
  }

  [Test]
  [Category("EdgeCase")]
  public void Shuffle_EmptyArray_DoesNotThrow() {
    var random = new Random(42);
    var array = new int[0];
    Assert.DoesNotThrow(() => random.Shuffle(array));
  }

  [Test]
  [Category("Exception")]
  public void Shuffle_NullArray_ThrowsArgumentNullException() {
    var random = new Random(42);
    int[] array = null;
    Assert.Throws<ArgumentNullException>(() => random.Shuffle(array));
  }

  #endregion

  #region Random.GetItems

  [Test]
  [Category("HappyPath")]
  public void GetItems_ReturnsCorrectLength() {
    var random = new Random(42);
    var choices = new[] { 1, 2, 3, 4, 5 };
    var result = random.GetItems(choices, 10);
    Assert.That(result.Length, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void GetItems_AllElementsFromChoices() {
    var random = new Random(42);
    var choices = new[] { 'a', 'b', 'c' };
    var result = random.GetItems(choices, 100);
    foreach (var item in result)
      Assert.That(choices, Does.Contain(item));
  }

  [Test]
  [Category("HappyPath")]
  public void GetItems_ZeroLength_ReturnsEmptyArray() {
    var random = new Random(42);
    var choices = new[] { 1, 2, 3 };
    var result = random.GetItems(choices, 0);
    Assert.That(result, Is.Empty);
  }

  [Test]
  [Category("Exception")]
  public void GetItems_NullChoices_ThrowsArgumentNullException() {
    var random = new Random(42);
    int[] choices = null;
    Assert.Throws<ArgumentNullException>(() => random.GetItems(choices, 5));
  }

  [Test]
  [Category("Exception")]
  public void GetItems_EmptyChoices_ThrowsArgumentException() {
    var random = new Random(42);
    var choices = new int[0];
    Assert.Throws<ArgumentException>(() => random.GetItems(choices, 5));
  }

  [Test]
  [Category("Exception")]
  public void GetItems_NegativeLength_ThrowsArgumentOutOfRangeException() {
    var random = new Random(42);
    var choices = new[] { 1, 2, 3 };
    Assert.Throws<ArgumentOutOfRangeException>(() => random.GetItems(choices, -1));
  }

  #endregion

  #region Random.NextSingle

  [Test]
  [Category("HappyPath")]
  public void NextSingle_ReturnsValueBetweenZeroAndOne() {
    var random = new Random(42);
    for (var i = 0; i < 100; ++i) {
      var result = random.NextSingle();
      Assert.That(result, Is.GreaterThanOrEqualTo(0.0f));
      Assert.That(result, Is.LessThan(1.0f));
    }
  }

  #endregion

  #region Random.NextInt64

  [Test]
  [Category("HappyPath")]
  public void NextInt64_ReturnsNonNegativeValue() {
    var random = new Random(42);
    for (var i = 0; i < 100; ++i) {
      var result = random.NextInt64();
      Assert.That(result, Is.GreaterThanOrEqualTo(0L));
    }
  }

  [Test]
  [Category("HappyPath")]
  public void NextInt64_WithMaxValue_ReturnsValueLessThanMax() {
    var random = new Random(42);
    var max = 100L;
    for (var i = 0; i < 100; ++i) {
      var result = random.NextInt64(max);
      Assert.That(result, Is.GreaterThanOrEqualTo(0L));
      Assert.That(result, Is.LessThan(max));
    }
  }

  [Test]
  [Category("HappyPath")]
  public void NextInt64_WithRange_ReturnsValueInRange() {
    var random = new Random(42);
    var min = 50L;
    var max = 100L;
    for (var i = 0; i < 100; ++i) {
      var result = random.NextInt64(min, max);
      Assert.That(result, Is.GreaterThanOrEqualTo(min));
      Assert.That(result, Is.LessThan(max));
    }
  }

  #endregion

  #region Random.NextBytes(Span<byte>)

  [Test]
  [Category("HappyPath")]
  public void NextBytes_Span_FillsBuffer() {
    var random = new Random(42);
    Span<byte> buffer = stackalloc byte[10];
    random.NextBytes(buffer);
    Assert.That(buffer.ToArray().Length, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void NextBytes_Span_ProducesNonZeroBytes() {
    var random = new Random(42);
    Span<byte> buffer = stackalloc byte[100];
    random.NextBytes(buffer);
    var hasNonZero = false;
    for (var i = 0; i < buffer.Length; ++i)
      if (buffer[i] != 0) {
        hasNonZero = true;
        break;
      }
    Assert.That(hasNonZero, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void NextBytes_EmptySpan_DoesNotThrow() {
    var random = new Random(42);
    Span<byte> buffer = stackalloc byte[0];
    random.NextBytes(buffer);
    Assert.Pass();
  }

  #endregion

}
