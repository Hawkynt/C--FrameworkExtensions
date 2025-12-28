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
using System.Buffers;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("SearchValues")]
public class SearchValuesTests {

  #region SearchValues.Create

  [Test]
  [Category("HappyPath")]
  public void Create_ByteValues_CreatesInstance() {
    var values = SearchValues.Create("abc"u8);
    Assert.That(values, Is.Not.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void Create_CharValues_CreatesInstance() {
    var values = SearchValues.Create("abc");
    Assert.That(values, Is.Not.Null);
  }

  [Test]
  [Category("EdgeCase")]
  public void Create_EmptyByteValues_CreatesInstance() {
    var values = SearchValues.Create(ReadOnlySpan<byte>.Empty);
    Assert.That(values, Is.Not.Null);
  }

  [Test]
  [Category("EdgeCase")]
  public void Create_EmptyCharValues_CreatesInstance() {
    var values = SearchValues.Create(ReadOnlySpan<char>.Empty);
    Assert.That(values, Is.Not.Null);
  }

  #endregion

  #region Contains

  [Test]
  [Category("HappyPath")]
  public void Contains_ValuePresent_ReturnsTrue() {
    var values = SearchValues.Create("abc");
    Assert.That(values.Contains('b'), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Contains_ValueAbsent_ReturnsFalse() {
    var values = SearchValues.Create("abc");
    Assert.That(values.Contains('d'), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Contains_ByteValuePresent_ReturnsTrue() {
    var values = SearchValues.Create("xyz"u8);
    Assert.That(values.Contains((byte)'y'), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Contains_ByteValueAbsent_ReturnsFalse() {
    var values = SearchValues.Create("xyz"u8);
    Assert.That(values.Contains((byte)'a'), Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  public void Contains_EmptyValues_ReturnsFalse() {
    var values = SearchValues.Create(ReadOnlySpan<char>.Empty);
    Assert.That(values.Contains('a'), Is.False);
  }

  #endregion

  #region IndexOfAny

  [Test]
  [Category("HappyPath")]
  public void IndexOfAny_ValueFound_ReturnsFirstIndex() {
    var values = SearchValues.Create("aeiou");
    ReadOnlySpan<char> text = "hello world";
    var index = text.IndexOfAny(values);
    Assert.That(index, Is.EqualTo(1)); // 'e' at index 1
  }

  [Test]
  [Category("HappyPath")]
  public void IndexOfAny_ValueNotFound_ReturnsMinusOne() {
    var values = SearchValues.Create("xyz");
    ReadOnlySpan<char> text = "hello world";
    var index = text.IndexOfAny(values);
    Assert.That(index, Is.EqualTo(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void IndexOfAny_MultipleMatches_ReturnsFirst() {
    var values = SearchValues.Create("lo");
    ReadOnlySpan<char> text = "hello";
    var index = text.IndexOfAny(values);
    Assert.That(index, Is.EqualTo(2)); // 'l' at index 2
  }

  [Test]
  [Category("HappyPath")]
  public void IndexOfAny_Span_ValueFound_ReturnsIndex() {
    var values = SearchValues.Create("aeiou");
    Span<char> text = "testing".ToCharArray();
    var index = text.IndexOfAny(values);
    Assert.That(index, Is.EqualTo(1)); // 'e' at index 1
  }

  [Test]
  [Category("EdgeCase")]
  public void IndexOfAny_EmptySpan_ReturnsMinusOne() {
    var values = SearchValues.Create("abc");
    var text = ReadOnlySpan<char>.Empty;
    var index = text.IndexOfAny(values);
    Assert.That(index, Is.EqualTo(-1));
  }

  [Test]
  [Category("EdgeCase")]
  public void IndexOfAny_EmptyValues_ReturnsMinusOne() {
    var values = SearchValues.Create(ReadOnlySpan<char>.Empty);
    ReadOnlySpan<char> text = "hello";
    var index = text.IndexOfAny(values);
    Assert.That(index, Is.EqualTo(-1));
  }

  #endregion

  #region LastIndexOfAny

  [Test]
  [Category("HappyPath")]
  public void LastIndexOfAny_ValueFound_ReturnsLastIndex() {
    var values = SearchValues.Create("lo");
    ReadOnlySpan<char> text = "hello";
    var index = text.LastIndexOfAny(values);
    Assert.That(index, Is.EqualTo(4)); // 'o' at index 4
  }

  [Test]
  [Category("HappyPath")]
  public void LastIndexOfAny_ValueNotFound_ReturnsMinusOne() {
    var values = SearchValues.Create("xyz");
    ReadOnlySpan<char> text = "hello";
    var index = text.LastIndexOfAny(values);
    Assert.That(index, Is.EqualTo(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void LastIndexOfAny_Span_ValueFound_ReturnsIndex() {
    var values = SearchValues.Create("aeiou");
    Span<char> text = "testing".ToCharArray();
    var index = text.LastIndexOfAny(values);
    Assert.That(index, Is.EqualTo(4)); // 'i' at index 4
  }

  [Test]
  [Category("EdgeCase")]
  public void LastIndexOfAny_EmptySpan_ReturnsMinusOne() {
    var values = SearchValues.Create("abc");
    var text = ReadOnlySpan<char>.Empty;
    var index = text.LastIndexOfAny(values);
    Assert.That(index, Is.EqualTo(-1));
  }

  #endregion

  #region ContainsAny

  [Test]
  [Category("HappyPath")]
  public void ContainsAny_ValuePresent_ReturnsTrue() {
    var values = SearchValues.Create("aeiou");
    ReadOnlySpan<char> text = "hello";
    Assert.That(text.ContainsAny(values), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ContainsAny_ValueAbsent_ReturnsFalse() {
    var values = SearchValues.Create("xyz");
    ReadOnlySpan<char> text = "hello";
    Assert.That(text.ContainsAny(values), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ContainsAny_Span_ValuePresent_ReturnsTrue() {
    var values = SearchValues.Create("aeiou");
    Span<char> text = "hello".ToCharArray();
    Assert.That(text.ContainsAny(values), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void ContainsAny_EmptySpan_ReturnsFalse() {
    var values = SearchValues.Create("abc");
    var text = ReadOnlySpan<char>.Empty;
    Assert.That(text.ContainsAny(values), Is.False);
  }

  #endregion

  #region IndexOfAnyExcept

  [Test]
  [Category("HappyPath")]
  public void IndexOfAnyExcept_ValueFound_ReturnsFirstNonMatchIndex() {
    var values = SearchValues.Create("abc");
    ReadOnlySpan<char> text = "aabcdef";
    var index = text.IndexOfAnyExcept(values);
    Assert.That(index, Is.EqualTo(4)); // 'd' at index 4
  }

  [Test]
  [Category("HappyPath")]
  public void IndexOfAnyExcept_AllMatch_ReturnsMinusOne() {
    var values = SearchValues.Create("abc");
    ReadOnlySpan<char> text = "aabbcc";
    var index = text.IndexOfAnyExcept(values);
    Assert.That(index, Is.EqualTo(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void IndexOfAnyExcept_NoneMatch_ReturnsZero() {
    var values = SearchValues.Create("xyz");
    ReadOnlySpan<char> text = "hello";
    var index = text.IndexOfAnyExcept(values);
    Assert.That(index, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void IndexOfAnyExcept_Span_ReturnsCorrectIndex() {
    var values = SearchValues.Create("abc");
    Span<char> text = "aabcdef".ToCharArray();
    var index = text.IndexOfAnyExcept(values);
    Assert.That(index, Is.EqualTo(4));
  }

  [Test]
  [Category("EdgeCase")]
  public void IndexOfAnyExcept_EmptySpan_ReturnsMinusOne() {
    var values = SearchValues.Create("abc");
    var text = ReadOnlySpan<char>.Empty;
    var index = text.IndexOfAnyExcept(values);
    Assert.That(index, Is.EqualTo(-1));
  }

  [Test]
  [Category("EdgeCase")]
  public void IndexOfAnyExcept_EmptyValues_ReturnsZero() {
    var values = SearchValues.Create(ReadOnlySpan<char>.Empty);
    ReadOnlySpan<char> text = "hello";
    var index = text.IndexOfAnyExcept(values);
    Assert.That(index, Is.EqualTo(0));
  }

  #endregion

  #region LastIndexOfAnyExcept

  [Test]
  [Category("HappyPath")]
  public void LastIndexOfAnyExcept_ValueFound_ReturnsLastNonMatchIndex() {
    var values = SearchValues.Create("def");
    ReadOnlySpan<char> text = "abcdef";
    var index = text.LastIndexOfAnyExcept(values);
    Assert.That(index, Is.EqualTo(2)); // 'c' at index 2
  }

  [Test]
  [Category("HappyPath")]
  public void LastIndexOfAnyExcept_AllMatch_ReturnsMinusOne() {
    var values = SearchValues.Create("abcdef");
    ReadOnlySpan<char> text = "abcdef";
    var index = text.LastIndexOfAnyExcept(values);
    Assert.That(index, Is.EqualTo(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void LastIndexOfAnyExcept_Span_ReturnsCorrectIndex() {
    var values = SearchValues.Create("def");
    Span<char> text = "abcdef".ToCharArray();
    var index = text.LastIndexOfAnyExcept(values);
    Assert.That(index, Is.EqualTo(2));
  }

  [Test]
  [Category("EdgeCase")]
  public void LastIndexOfAnyExcept_EmptySpan_ReturnsMinusOne() {
    var values = SearchValues.Create("abc");
    var text = ReadOnlySpan<char>.Empty;
    var index = text.LastIndexOfAnyExcept(values);
    Assert.That(index, Is.EqualTo(-1));
  }

  #endregion

  #region ContainsAnyExcept

  [Test]
  [Category("HappyPath")]
  public void ContainsAnyExcept_HasNonMatchingValue_ReturnsTrue() {
    var values = SearchValues.Create("abc");
    ReadOnlySpan<char> text = "abcdef";
    Assert.That(text.ContainsAnyExcept(values), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ContainsAnyExcept_AllMatch_ReturnsFalse() {
    var values = SearchValues.Create("abcdef");
    ReadOnlySpan<char> text = "abcdef";
    Assert.That(text.ContainsAnyExcept(values), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ContainsAnyExcept_Span_ReturnsCorrectResult() {
    var values = SearchValues.Create("abc");
    Span<char> text = "abc".ToCharArray();
    Assert.That(text.ContainsAnyExcept(values), Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  public void ContainsAnyExcept_EmptySpan_ReturnsFalse() {
    var values = SearchValues.Create("abc");
    var text = ReadOnlySpan<char>.Empty;
    Assert.That(text.ContainsAnyExcept(values), Is.False);
  }

  #endregion

  #region Byte-specific tests

  [Test]
  [Category("HappyPath")]
  public void IndexOfAny_ByteSpan_FindsValue() {
    var values = SearchValues.Create("\n\r"u8);
    ReadOnlySpan<byte> data = "Hello\nW"u8;
    var index = data.IndexOfAny(values);
    Assert.That(index, Is.EqualTo(5)); // 0x0A at index 5
  }

  [Test]
  [Category("HappyPath")]
  public void LastIndexOfAny_ByteSpan_FindsLastValue() {
    var values = SearchValues.Create([0x6C]); // 'l'
    var data = "hello"u8;
    var index = data.LastIndexOfAny(values);
    Assert.That(index, Is.EqualTo(3)); // last 'l' at index 3
  }

  #endregion

}
