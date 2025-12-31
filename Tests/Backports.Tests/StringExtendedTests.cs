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
using System.Text;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("String")]
public class StringExtendedTests {

  #region String.Create<TState>

  [Test]
  [Category("HappyPath")]
  public void StringCreate_WithState_CreatesString() {
    var result = string.Create(5, 'X', (span, ch) => {
      for (var i = 0; i < span.Length; ++i)
        span[i] = ch;
    });
    Assert.That(result, Is.EqualTo("XXXXX"));
  }

  [Test]
  [Category("HappyPath")]
  public void StringCreate_WithIntState_CreatesNumberedString() {
    var result = string.Create(5, 0, (span, start) => {
      for (var i = 0; i < span.Length; ++i)
        span[i] = (char)('0' + start + i);
    });
    Assert.That(result, Is.EqualTo("01234"));
  }

  [Test]
  [Category("EdgeCase")]
  public void StringCreate_ZeroLength_ReturnsEmptyString() {
    var result = string.Create(0, 'X', (span, ch) => { });
    Assert.That(result, Is.EqualTo(string.Empty));
  }

  [Test]
  [Category("Exception")]
  public void StringCreate_NegativeLength_ThrowsArgumentOutOfRangeException() {
    Assert.Throws<ArgumentOutOfRangeException>(() =>
      string.Create(-1, 'X', (span, ch) => { })
    );
  }

  [Test]
  [Category("Exception")]
  public void StringCreate_NullAction_ThrowsArgumentNullException() {
    Assert.Throws<ArgumentNullException>(() =>
      string.Create<char>(5, 'X', null)
    );
  }

  #endregion

  #region String.ReplaceLineEndings

  [Test]
  [Category("HappyPath")]
  public void ReplaceLineEndings_LF_ReplacesWithEnvironmentNewLine() {
    var input = "Hello\nWorld";
    var result = input.ReplaceLineEndings();
    Assert.That(result, Is.EqualTo("Hello" + Environment.NewLine + "World"));
  }

  [Test]
  [Category("HappyPath")]
  public void ReplaceLineEndings_CR_ReplacesWithEnvironmentNewLine() {
    var input = "Hello\rWorld";
    var result = input.ReplaceLineEndings();
    Assert.That(result, Is.EqualTo("Hello" + Environment.NewLine + "World"));
  }

  [Test]
  [Category("HappyPath")]
  public void ReplaceLineEndings_CRLF_ReplacesWithSingleNewLine() {
    var input = "Hello\r\nWorld";
    var result = input.ReplaceLineEndings();
    Assert.That(result, Is.EqualTo("Hello" + Environment.NewLine + "World"));
  }

  [Test]
  [Category("HappyPath")]
  public void ReplaceLineEndings_CustomReplacement_UsesProvidedString() {
    var input = "Hello\nWorld\nTest";
    var result = input.ReplaceLineEndings("<BR>");
    Assert.That(result, Is.EqualTo("Hello<BR>World<BR>Test"));
  }

  [Test]
  [Category("EdgeCase")]
  public void ReplaceLineEndings_EmptyString_ReturnsEmpty() {
    var input = string.Empty;
    var result = input.ReplaceLineEndings();
    Assert.That(result, Is.EqualTo(string.Empty));
  }

  [Test]
  [Category("EdgeCase")]
  public void ReplaceLineEndings_NoLineEndings_ReturnsSameString() {
    var input = "HelloWorld";
    var result = input.ReplaceLineEndings();
    Assert.That(result, Is.EqualTo("HelloWorld"));
  }

  #endregion

  #region StringBuilder.AppendJoin

  [Test]
  [Category("HappyPath")]
  public void AppendJoin_StringSeparator_JoinsStrings() {
    var sb = new StringBuilder();
    sb.AppendJoin(", ", "a", "b", "c");
    Assert.That(sb.ToString(), Is.EqualTo("a, b, c"));
  }

  [Test]
  [Category("HappyPath")]
  public void AppendJoin_CharSeparator_JoinsStrings() {
    var sb = new StringBuilder();
    sb.AppendJoin('-', "a", "b", "c");
    Assert.That(sb.ToString(), Is.EqualTo("a-b-c"));
  }

  [Test]
  [Category("HappyPath")]
  public void AppendJoin_Objects_JoinsToStrings() {
    var sb = new StringBuilder();
    sb.AppendJoin(", ", 1, 2, 3);
    Assert.That(sb.ToString(), Is.EqualTo("1, 2, 3"));
  }

  [Test]
  [Category("HappyPath")]
  public void AppendJoin_Enumerable_JoinsElements() {
    var sb = new StringBuilder();
    var list = new[] { 1, 2, 3 };
    sb.AppendJoin("-", list);
    Assert.That(sb.ToString(), Is.EqualTo("1-2-3"));
  }

  [Test]
  [Category("EdgeCase")]
  public void AppendJoin_SingleElement_NoSeparator() {
    var sb = new StringBuilder();
    sb.AppendJoin(", ", "only");
    Assert.That(sb.ToString(), Is.EqualTo("only"));
  }

  [Test]
  [Category("EdgeCase")]
  public void AppendJoin_EmptyArray_AppendsNothing() {
    var sb = new StringBuilder("prefix");
    sb.AppendJoin(", ", Array.Empty<string>());
    Assert.That(sb.ToString(), Is.EqualTo("prefix"));
  }

  [Test]
  [Category("HappyPath")]
  public void AppendJoin_ReturnsStringBuilder_ForChaining() {
    var sb = new StringBuilder();
    var result = sb.AppendJoin(", ", "a", "b").Append("!");
    Assert.That(result.ToString(), Is.EqualTo("a, b!"));
  }

  #endregion

  #region StringBuilder.Clear

  [Test]
  [Category("HappyPath")]
  public void Clear_RemovesAllContent() {
    var sb = new StringBuilder("Hello World");
    sb.Clear();
    Assert.That(sb.Length, Is.EqualTo(0));
    Assert.That(sb.ToString(), Is.EqualTo(string.Empty));
  }

  [Test]
  [Category("EdgeCase")]
  public void Clear_AlreadyEmpty_RemainsEmpty() {
    var sb = new StringBuilder();
    sb.Clear();
    Assert.That(sb.Length, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Clear_CanAppendAfterClear() {
    var sb = new StringBuilder("Old");
    sb.Clear();
    sb.Append("New");
    Assert.That(sb.ToString(), Is.EqualTo("New"));
  }

  [Test]
  [Category("HappyPath")]
  public void Clear_MultipleCalls_Works() {
    var sb = new StringBuilder("Test");
    sb.Clear();
    sb.Append("Again");
    sb.Clear();
    Assert.That(sb.Length, Is.EqualTo(0));
  }

  #endregion

}
