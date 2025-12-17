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
[Category("Span")]
public class SpanTests {

  #region Span<T> - Construction

  [Test]
  [Category("HappyPath")]
  public void Span_FromArray_HasCorrectLength() {
    var array = new[] { 1, 2, 3, 4, 5 };
    var span = new Span<int>(array);
    Assert.That(span.Length, Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void Span_FromArraySlice_HasCorrectLength() {
    var array = new[] { 1, 2, 3, 4, 5 };
    var span = new Span<int>(array, 1, 3);
    Assert.That(span.Length, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Span_Empty_HasZeroLength() {
    var span = Span<int>.Empty;
    Assert.That(span.Length, Is.EqualTo(0));
    Assert.That(span.IsEmpty, Is.True);
  }

  #endregion

  #region Span<T> - Indexer

  [Test]
  [Category("HappyPath")]
  public void Span_Indexer_ReturnsCorrectElement() {
    var array = new[] { 10, 20, 30, 40, 50 };
    var span = new Span<int>(array);
    Assert.That(span[0], Is.EqualTo(10));
    Assert.That(span[2], Is.EqualTo(30));
    Assert.That(span[4], Is.EqualTo(50));
  }

  [Test]
  [Category("HappyPath")]
  public void Span_Indexer_Set_ModifiesElement() {
    var array = new[] { 1, 2, 3 };
    var span = new Span<int>(array);
    span[1] = 99;
    Assert.That(array[1], Is.EqualTo(99));
  }

  [Test]
  [Category("Exception")]
  public void Span_Indexer_OutOfRange_ThrowsException() {
    var array = new[] { 1, 2, 3 };
    var span = new Span<int>(array);
    var threw = false;
    try {
      var _ = span[5];
    } catch (IndexOutOfRangeException) {
      threw = true;
    }
    Assert.That(threw, Is.True);
  }

  #endregion

  #region Span<T> - Slice

  [Test]
  [Category("HappyPath")]
  public void Span_Slice_ReturnsSubSpan() {
    var array = new[] { 1, 2, 3, 4, 5 };
    var span = new Span<int>(array);
    var slice = span.Slice(1, 3);
    Assert.That(slice.Length, Is.EqualTo(3));
    Assert.That(slice[0], Is.EqualTo(2));
    Assert.That(slice[2], Is.EqualTo(4));
  }

  [Test]
  [Category("HappyPath")]
  public void Span_Slice_ModifyingSliceModifiesOriginal() {
    var array = new[] { 1, 2, 3, 4, 5 };
    var span = new Span<int>(array);
    var slice = span.Slice(1, 3);
    slice[0] = 99;
    Assert.That(array[1], Is.EqualTo(99));
  }

  #endregion

  #region Span<T> - ToArray

  [Test]
  [Category("HappyPath")]
  public void Span_ToArray_ReturnsArrayCopy() {
    var original = new[] { 1, 2, 3 };
    var span = new Span<int>(original);
    var copy = span.ToArray();
    Assert.That(copy, Is.EqualTo(original));
    Assert.That(copy, Is.Not.SameAs(original));
  }

  #endregion

  #region Span<T> - CopyTo

  [Test]
  [Category("HappyPath")]
  public void Span_CopyTo_CopiesElements() {
    var source = new[] { 1, 2, 3 };
    var dest = new int[5];
    var span = new Span<int>(source);
    span.CopyTo(new Span<int>(dest));
    Assert.That(dest[0], Is.EqualTo(1));
    Assert.That(dest[1], Is.EqualTo(2));
    Assert.That(dest[2], Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Span_TryCopyTo_Succeeds_WhenDestinationLargeEnough() {
    var source = new[] { 1, 2, 3 };
    var dest = new int[5];
    var span = new Span<int>(source);
    var result = span.TryCopyTo(new Span<int>(dest));
    Assert.That(result, Is.True);
    Assert.That(dest[0], Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Span_TryCopyTo_Fails_WhenDestinationTooSmall() {
    var source = new[] { 1, 2, 3, 4, 5 };
    var dest = new int[2];
    var span = new Span<int>(source);
    var result = span.TryCopyTo(new Span<int>(dest));
    Assert.That(result, Is.False);
  }

  #endregion

  #region Span<T> - Clear and Fill

  [Test]
  [Category("HappyPath")]
  public void Span_Clear_SetsAllToDefault() {
    var array = new[] { 1, 2, 3, 4, 5 };
    var span = new Span<int>(array);
    span.Clear();
    Assert.That(array, Is.All.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Span_Fill_SetsAllToValue() {
    var array = new int[5];
    var span = new Span<int>(array);
    span.Fill(42);
    Assert.That(array, Is.All.EqualTo(42));
  }

  #endregion

  #region Span<T> - Enumeration

  [Test]
  [Category("HappyPath")]
  public void Span_GetEnumerator_EnumeratesAllElements() {
    var array = new[] { 1, 2, 3 };
    var span = new Span<int>(array);
    var sum = 0;
    foreach (var item in span)
      sum += item;
    Assert.That(sum, Is.EqualTo(6));
  }

  #endregion

  #region Span<T> - Implicit Conversion

  [Test]
  [Category("HappyPath")]
  public void Span_ImplicitFromArray_Works() {
    var array = new[] { 1, 2, 3 };
    Span<int> span = array;
    Assert.That(span.Length, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Span_ImplicitToReadOnlySpan_Works() {
    var array = new[] { 1, 2, 3 };
    var span = new Span<int>(array);
    ReadOnlySpan<int> readOnly = span;
    Assert.That(readOnly.Length, Is.EqualTo(3));
  }

  #endregion

  #region ReadOnlySpan<T> - Construction

  [Test]
  [Category("HappyPath")]
  public void ReadOnlySpan_FromArray_HasCorrectLength() {
    var array = new[] { 1, 2, 3, 4, 5 };
    var span = new ReadOnlySpan<int>(array);
    Assert.That(span.Length, Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlySpan_FromArraySlice_HasCorrectLength() {
    var array = new[] { 1, 2, 3, 4, 5 };
    var span = new ReadOnlySpan<int>(array, 1, 3);
    Assert.That(span.Length, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlySpan_Empty_HasZeroLength() {
    var span = ReadOnlySpan<int>.Empty;
    Assert.That(span.Length, Is.EqualTo(0));
    Assert.That(span.IsEmpty, Is.True);
  }

  #endregion

  #region ReadOnlySpan<T> - Indexer

  [Test]
  [Category("HappyPath")]
  public void ReadOnlySpan_Indexer_ReturnsCorrectElement() {
    var array = new[] { 10, 20, 30, 40, 50 };
    var span = new ReadOnlySpan<int>(array);
    Assert.That(span[0], Is.EqualTo(10));
    Assert.That(span[2], Is.EqualTo(30));
    Assert.That(span[4], Is.EqualTo(50));
  }

  [Test]
  [Category("Exception")]
  public void ReadOnlySpan_Indexer_OutOfRange_ThrowsException() {
    var array = new[] { 1, 2, 3 };
    var span = new ReadOnlySpan<int>(array);
    var threw = false;
    try {
      var _ = span[5];
    } catch (IndexOutOfRangeException) {
      threw = true;
    }
    Assert.That(threw, Is.True);
  }

  #endregion

  #region ReadOnlySpan<T> - Slice

  [Test]
  [Category("HappyPath")]
  public void ReadOnlySpan_Slice_ReturnsSubSpan() {
    var array = new[] { 1, 2, 3, 4, 5 };
    var span = new ReadOnlySpan<int>(array);
    var slice = span.Slice(1, 3);
    Assert.That(slice.Length, Is.EqualTo(3));
    Assert.That(slice[0], Is.EqualTo(2));
    Assert.That(slice[2], Is.EqualTo(4));
  }

  #endregion

  #region ReadOnlySpan<T> - ToArray

  [Test]
  [Category("HappyPath")]
  public void ReadOnlySpan_ToArray_ReturnsArrayCopy() {
    var original = new[] { 1, 2, 3 };
    var span = new ReadOnlySpan<int>(original);
    var copy = span.ToArray();
    Assert.That(copy, Is.EqualTo(original));
    Assert.That(copy, Is.Not.SameAs(original));
  }

  #endregion

  #region ReadOnlySpan<T> - CopyTo

  [Test]
  [Category("HappyPath")]
  public void ReadOnlySpan_CopyTo_CopiesElements() {
    var source = new[] { 1, 2, 3 };
    var dest = new int[5];
    var span = new ReadOnlySpan<int>(source);
    span.CopyTo(new Span<int>(dest));
    Assert.That(dest[0], Is.EqualTo(1));
    Assert.That(dest[1], Is.EqualTo(2));
    Assert.That(dest[2], Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlySpan_TryCopyTo_Succeeds_WhenDestinationLargeEnough() {
    var source = new[] { 1, 2, 3 };
    var dest = new int[5];
    var span = new ReadOnlySpan<int>(source);
    var result = span.TryCopyTo(new Span<int>(dest));
    Assert.That(result, Is.True);
    Assert.That(dest[0], Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadOnlySpan_TryCopyTo_Fails_WhenDestinationTooSmall() {
    var source = new[] { 1, 2, 3, 4, 5 };
    var dest = new int[2];
    var span = new ReadOnlySpan<int>(source);
    var result = span.TryCopyTo(new Span<int>(dest));
    Assert.That(result, Is.False);
  }

  #endregion

  #region ReadOnlySpan<T> - Enumeration

  [Test]
  [Category("HappyPath")]
  public void ReadOnlySpan_GetEnumerator_EnumeratesAllElements() {
    var array = new[] { 1, 2, 3 };
    var span = new ReadOnlySpan<int>(array);
    var sum = 0;
    foreach (var item in span)
      sum += item;
    Assert.That(sum, Is.EqualTo(6));
  }

  #endregion

  #region ReadOnlySpan<T> - Implicit Conversion

  [Test]
  [Category("HappyPath")]
  public void ReadOnlySpan_ImplicitFromArray_Works() {
    var array = new[] { 1, 2, 3 };
    ReadOnlySpan<int> span = array;
    Assert.That(span.Length, Is.EqualTo(3));
  }

  #endregion

  #region Span<char> - ToString

  [Test]
  [Category("HappyPath")]
  public void SpanChar_ToString_ReturnsString() {
    var array = new[] { 'H', 'e', 'l', 'l', 'o' };
    var span = new Span<char>(array);
    Assert.That(span.ToString(), Is.EqualTo("Hello"));
  }

  #endregion

  #region ReadOnlySpan<char> - ToString

  [Test]
  [Category("HappyPath")]
  public void ReadOnlySpanChar_ToString_ReturnsString() {
    var array = new[] { 'H', 'e', 'l', 'l', 'o' };
    var span = new ReadOnlySpan<char>(array);
    Assert.That(span.ToString(), Is.EqualTo("Hello"));
  }

  #endregion

  #region Span<T> - Slice (single argument)

  [Test]
  [Category("HappyPath")]
  public void Span_Slice_SingleArg_ReturnsRemainder() {
    Span<int> span = stackalloc int[] { 1, 2, 3, 4, 5 };
    var sliced = span.Slice(2);
    Assert.That(sliced.Length, Is.EqualTo(3));
    Assert.That(sliced[0], Is.EqualTo(3));
    Assert.That(sliced[2], Is.EqualTo(5));
  }

  [Test]
  [Category("EdgeCase")]
  public void Span_Slice_SingleArg_AtEnd_ReturnsEmpty() {
    var array = new[] { 1, 2, 3 };
    var span = new Span<int>(array);
    var sliced = span.Slice(3);
    Assert.That(sliced.Length, Is.EqualTo(0));
    Assert.That(sliced.IsEmpty, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void Span_Slice_SingleArg_AtStart_ReturnsAll() {
    var array = new[] { 1, 2, 3 };
    var span = new Span<int>(array);
    var sliced = span.Slice(0);
    Assert.That(sliced.Length, Is.EqualTo(3));
  }

  #endregion

  #region ReadOnlySpan<T> - Slice (single argument)

  [Test]
  [Category("HappyPath")]
  public void ReadOnlySpan_Slice_SingleArg_ReturnsRemainder() {
    ReadOnlySpan<char> span = "Hello World".AsSpan();
    var sliced = span.Slice(6);
    Assert.That(sliced.ToString(), Is.EqualTo("World"));
  }

  [Test]
  [Category("EdgeCase")]
  public void ReadOnlySpan_Slice_SingleArg_AtEnd_ReturnsEmpty() {
    var array = new[] { 1, 2, 3 };
    var span = new ReadOnlySpan<int>(array);
    var sliced = span.Slice(3);
    Assert.That(sliced.Length, Is.EqualTo(0));
    Assert.That(sliced.IsEmpty, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void ReadOnlySpan_Slice_SingleArg_AtStart_ReturnsAll() {
    var array = new[] { 1, 2, 3 };
    var span = new ReadOnlySpan<int>(array);
    var sliced = span.Slice(0);
    Assert.That(sliced.Length, Is.EqualTo(3));
  }

  #endregion

}
