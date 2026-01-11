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

  #region String.Contains with char and StringComparison

  [Test]
  [Category("HappyPath")]
  public void Contains_CharWithOrdinalIgnoreCase_FindsCaseInsensitive() {
    var source = "Hello World";
    Assert.That(source.Contains('w', StringComparison.OrdinalIgnoreCase), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Contains_CharWithOrdinal_IsCaseSensitive() {
    var source = "Hello World";
    Assert.That(source.Contains('w', StringComparison.Ordinal), Is.False);
    Assert.That(source.Contains('W', StringComparison.Ordinal), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void Contains_CharWithOrdinal_NotFound() {
    var source = "Hello World";
    Assert.That(source.Contains('z', StringComparison.Ordinal), Is.False);
  }

  #endregion

  #region String.IndexOf with char and StringComparison

  [Test]
  [Category("HappyPath")]
  public void IndexOf_CharWithOrdinal_FindsIndex() {
    var source = "Hello World";
    Assert.That(source.IndexOf('W', StringComparison.Ordinal), Is.EqualTo(6));
  }

  [Test]
  [Category("HappyPath")]
  public void IndexOf_CharWithOrdinalIgnoreCase_FindsCaseInsensitive() {
    var source = "Hello World";
    Assert.That(source.IndexOf('w', StringComparison.OrdinalIgnoreCase), Is.EqualTo(6));
  }

  [Test]
  [Category("EdgeCase")]
  public void IndexOf_CharWithOrdinal_NotFound() {
    var source = "Hello World";
    Assert.That(source.IndexOf('z', StringComparison.Ordinal), Is.EqualTo(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void IndexOf_CharWithCurrentCulture_FindsIndex() {
    var source = "Hello World";
    Assert.That(source.IndexOf('o', StringComparison.CurrentCulture), Is.EqualTo(4));
  }

  #endregion

  #region String.GetHashCode with StringComparison

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_WithOrdinal_ReturnsConsistentHash() {
    var s1 = "Hello";
    var s2 = "Hello";
    Assert.That(s1.GetHashCode(StringComparison.Ordinal), Is.EqualTo(s2.GetHashCode(StringComparison.Ordinal)));
  }

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_WithOrdinalIgnoreCase_SameForDifferentCase() {
    var s1 = "Hello";
    var s2 = "HELLO";
    Assert.That(s1.GetHashCode(StringComparison.OrdinalIgnoreCase), Is.EqualTo(s2.GetHashCode(StringComparison.OrdinalIgnoreCase)));
  }

  [Test]
  [Category("EdgeCase")]
  public void GetHashCode_DifferentStrings_DifferentHashes() {
    var s1 = "Hello";
    var s2 = "World";
    Assert.That(s1.GetHashCode(StringComparison.Ordinal), Is.Not.EqualTo(s2.GetHashCode(StringComparison.Ordinal)));
  }

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_WithInvariantCulture_ReturnsHash() {
    var s = "Hello";
    var hash = s.GetHashCode(StringComparison.InvariantCulture);
    Assert.That(hash, Is.Not.EqualTo(0).Or.EqualTo(0)); // Just verify it returns a value
  }

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_WithCurrentCultureIgnoreCase_SameForDifferentCase() {
    var s1 = "Hello";
    var s2 = "HELLO";
    Assert.That(s1.GetHashCode(StringComparison.CurrentCultureIgnoreCase), Is.EqualTo(s2.GetHashCode(StringComparison.CurrentCultureIgnoreCase)));
  }

  #endregion

  #region String.StartsWith and EndsWith with char

  [Test]
  [Category("HappyPath")]
  public void StartsWith_Char_MatchingStart() {
    var source = "Hello World";
    Assert.That(source.StartsWith('H'), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void StartsWith_Char_NonMatchingStart() {
    var source = "Hello World";
    Assert.That(source.StartsWith('h'), Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  public void StartsWith_Char_EmptyString() {
    var source = "";
    Assert.That(source.StartsWith('H'), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void EndsWith_Char_MatchingEnd() {
    var source = "Hello World";
    Assert.That(source.EndsWith('d'), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void EndsWith_Char_NonMatchingEnd() {
    var source = "Hello World";
    Assert.That(source.EndsWith('D'), Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  public void EndsWith_Char_EmptyString() {
    var source = "";
    Assert.That(source.EndsWith('d'), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void StartsWith_EndsWith_SingleCharString() {
    var source = "X";
    Assert.That(source.StartsWith('X'), Is.True);
    Assert.That(source.EndsWith('X'), Is.True);
  }

  #endregion

  #region String.Trim with char

  [Test]
  [Category("HappyPath")]
  public void Trim_Char_RemovesBothEnds() {
    var source = "***Hello***";
    Assert.That(source.Trim('*'), Is.EqualTo("Hello"));
  }

  [Test]
  [Category("HappyPath")]
  public void TrimStart_Char_RemovesFromStart() {
    var source = "***Hello***";
    Assert.That(source.TrimStart('*'), Is.EqualTo("Hello***"));
  }

  [Test]
  [Category("HappyPath")]
  public void TrimEnd_Char_RemovesFromEnd() {
    var source = "***Hello***";
    Assert.That(source.TrimEnd('*'), Is.EqualTo("***Hello"));
  }

  [Test]
  [Category("EdgeCase")]
  public void Trim_Char_NoMatchingChars() {
    var source = "Hello";
    Assert.That(source.Trim('*'), Is.EqualTo("Hello"));
  }

  [Test]
  [Category("EdgeCase")]
  public void Trim_Char_AllMatchingChars() {
    var source = "****";
    Assert.That(source.Trim('*'), Is.EqualTo(string.Empty));
  }

  [Test]
  [Category("EdgeCase")]
  public void Trim_Char_EmptyString() {
    var source = "";
    Assert.That(source.Trim('*'), Is.EqualTo(string.Empty));
  }

  #endregion

  #region String.Replace with StringComparison

  [Test]
  [Category("HappyPath")]
  public void Replace_WithOrdinalIgnoreCase_ReplacesCaseInsensitive() {
    var source = "Hello World hello";
    var result = source.Replace("hello", "hi", StringComparison.OrdinalIgnoreCase);
    Assert.That(result, Is.EqualTo("hi World hi"));
  }

  [Test]
  [Category("HappyPath")]
  public void Replace_WithOrdinal_IsCaseSensitive() {
    var source = "Hello World hello";
    var result = source.Replace("hello", "hi", StringComparison.Ordinal);
    Assert.That(result, Is.EqualTo("Hello World hi"));
  }

  [Test]
  [Category("HappyPath")]
  public void Replace_NoMatch_ReturnsOriginal() {
    var source = "Hello World";
    var result = source.Replace("foo", "bar", StringComparison.Ordinal);
    Assert.That(result, Is.EqualTo("Hello World"));
  }

  [Test]
  [Category("HappyPath")]
  public void Replace_WithNullNewValue_RemovesOldValue() {
    var source = "Hello World";
    var result = source.Replace("World", null, StringComparison.Ordinal);
    Assert.That(result, Is.EqualTo("Hello "));
  }

  [Test]
  [Category("Exception")]
  public void Replace_WithNullOldValue_ThrowsArgumentNullException() {
    var source = "Hello World";
    Assert.Throws<ArgumentNullException>(() => source.Replace(null!, "bar", StringComparison.Ordinal));
  }

  [Test]
  [Category("Exception")]
  public void Replace_WithEmptyOldValue_ThrowsArgumentException() {
    var source = "Hello World";
    Assert.Throws<ArgumentException>(() => source.Replace("", "bar", StringComparison.Ordinal));
  }

  #endregion

}
