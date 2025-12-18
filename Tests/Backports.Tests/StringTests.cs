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
[Category("String")]
public class StringTests {

  #region String.Contains with StringComparison

  [Test]
  [Category("HappyPath")]
  public void Contains_WithOrdinal_FindsSubstring() {
    var source = "Hello World";
    Assert.That(source.Contains("World", StringComparison.Ordinal), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Contains_WithOrdinalIgnoreCase_FindsCaseInsensitive() {
    var source = "Hello World";
    Assert.That(source.Contains("WORLD", StringComparison.OrdinalIgnoreCase), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Contains_WithOrdinal_NotFound() {
    var source = "Hello World";
    Assert.That(source.Contains("WORLD", StringComparison.Ordinal), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Contains_WithChar_FindsCharacter() {
    var source = "Hello World";
    Assert.That(source.Contains('W'), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Contains_WithChar_NotFound() {
    var source = "Hello World";
    Assert.That(source.Contains('x'), Is.False);
  }

  #endregion


  #region String.Split with single char

  [Test]
  [Category("HappyPath")]
  public void Split_SingleChar_SplitsString() {
    var source = "a,b,c,d";
    var result = source.Split(',');
    Assert.That(result, Is.EqualTo(new[] { "a", "b", "c", "d" }));
  }

  [Test]
  [Category("HappyPath")]
  public void Split_NoSeparator_ReturnsSingleElement() {
    var source = "abcd";
    var result = source.Split(',');
    Assert.That(result, Is.EqualTo(new[] { "abcd" }));
  }

  #endregion

  #region String.Join with char separator

  [Test]
  [Category("HappyPath")]
  public void Join_CharSeparator_JoinsStrings() {
    var result = string.Join(',', "a", "b", "c");
    Assert.That(result, Is.EqualTo("a,b,c"));
  }

  [Test]
  [Category("HappyPath")]
  public void Join_CharSeparator_EmptyArray_ReturnsEmpty() {
    var result = string.Join(',', new string[0]);
    Assert.That(result, Is.EqualTo(string.Empty));
  }

  [Test]
  [Category("HappyPath")]
  public void Join_CharSeparator_SingleElement_NoSeparator() {
    var result = string.Join(',', "single");
    Assert.That(result, Is.EqualTo("single"));
  }

  #endregion

  #region String.Concat with ReadOnlySpan<char>

  [Test]
  [Category("HappyPath")]
  public void Concat_TwoSpans_ConcatenatesCorrectly() {
    ReadOnlySpan<char> span1 = "Hello".AsSpan();
    ReadOnlySpan<char> span2 = " World".AsSpan();
    var result = string.Concat(span1, span2);
    Assert.That(result, Is.EqualTo("Hello World"));
  }

  [Test]
  [Category("HappyPath")]
  public void Concat_ThreeSpans_ConcatenatesCorrectly() {
    ReadOnlySpan<char> span1 = "Hello".AsSpan();
    ReadOnlySpan<char> span2 = " ".AsSpan();
    ReadOnlySpan<char> span3 = "World".AsSpan();
    var result = string.Concat(span1, span2, span3);
    Assert.That(result, Is.EqualTo("Hello World"));
  }

  [Test]
  [Category("HappyPath")]
  public void Concat_FourSpans_ConcatenatesCorrectly() {
    ReadOnlySpan<char> span1 = "A".AsSpan();
    ReadOnlySpan<char> span2 = "B".AsSpan();
    ReadOnlySpan<char> span3 = "C".AsSpan();
    ReadOnlySpan<char> span4 = "D".AsSpan();
    var result = string.Concat(span1, span2, span3, span4);
    Assert.That(result, Is.EqualTo("ABCD"));
  }

  [Test]
  [Category("EdgeCase")]
  public void Concat_EmptySpans_ReturnsEmptyString() {
    ReadOnlySpan<char> span1 = ReadOnlySpan<char>.Empty;
    ReadOnlySpan<char> span2 = ReadOnlySpan<char>.Empty;
    var result = string.Concat(span1, span2);
    Assert.That(result, Is.EqualTo(string.Empty));
  }

  [Test]
  [Category("EdgeCase")]
  public void Concat_OneEmptySpan_ReturnsConcatenation() {
    ReadOnlySpan<char> span1 = "Hello".AsSpan();
    ReadOnlySpan<char> span2 = ReadOnlySpan<char>.Empty;
    var result = string.Concat(span1, span2);
    Assert.That(result, Is.EqualTo("Hello"));
  }

  #endregion

}
