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
[Category("StringBuilder")]
public class StringBuilderTests {

  #region StringBuilder.Append(ReadOnlySpan<char>)

  [Test]
  [Category("HappyPath")]
  public void Append_Span_AppendsCharacters() {
    var sb = new StringBuilder("Hello");
    ReadOnlySpan<char> span = " World".AsSpan();
    sb.Append(span);
    Assert.That(sb.ToString(), Is.EqualTo("Hello World"));
  }

  [Test]
  [Category("HappyPath")]
  public void Append_Span_ReturnsBuilderForChaining() {
    var sb = new StringBuilder();
    ReadOnlySpan<char> span = "Test".AsSpan();
    var result = sb.Append(span);
    Assert.That(result, Is.SameAs(sb));
  }

  [Test]
  [Category("EdgeCase")]
  public void Append_EmptySpan_NoChange() {
    var sb = new StringBuilder("Hello");
    ReadOnlySpan<char> span = ReadOnlySpan<char>.Empty;
    sb.Append(span);
    Assert.That(sb.ToString(), Is.EqualTo("Hello"));
  }

  #endregion

  #region StringBuilder.Insert(int, ReadOnlySpan<char>)

  [Test]
  [Category("HappyPath")]
  public void Insert_Span_InsertsAtPosition() {
    var sb = new StringBuilder("Hello World");
    ReadOnlySpan<char> span = "Beautiful ".AsSpan();
    sb.Insert(6, span);
    Assert.That(sb.ToString(), Is.EqualTo("Hello Beautiful World"));
  }

  [Test]
  [Category("HappyPath")]
  public void Insert_Span_AtStart() {
    var sb = new StringBuilder("World");
    ReadOnlySpan<char> span = "Hello ".AsSpan();
    sb.Insert(0, span);
    Assert.That(sb.ToString(), Is.EqualTo("Hello World"));
  }

  [Test]
  [Category("HappyPath")]
  public void Insert_Span_ReturnsBuilderForChaining() {
    var sb = new StringBuilder("Test");
    ReadOnlySpan<char> span = "X".AsSpan();
    var result = sb.Insert(0, span);
    Assert.That(result, Is.SameAs(sb));
  }

  [Test]
  [Category("EdgeCase")]
  public void Insert_EmptySpan_NoChange() {
    var sb = new StringBuilder("Hello");
    ReadOnlySpan<char> span = ReadOnlySpan<char>.Empty;
    sb.Insert(2, span);
    Assert.That(sb.ToString(), Is.EqualTo("Hello"));
  }

  #endregion

  #region GetChunks

  [Test]
  [Category("HappyPath")]
  public void GetChunks_NonEmptyBuilder_ReturnsChunk() {
    var sb = new StringBuilder("Hello World");
    var chunks = sb.GetChunks();
    var hasChunk = false;
    foreach (var chunk in chunks) {
      hasChunk = true;
      Assert.That(chunk.Length, Is.GreaterThan(0));
    }
    Assert.That(hasChunk, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void GetChunks_ContainsFullContent() {
    var sb = new StringBuilder("Hello World");
    var chunks = sb.GetChunks();
    var content = "";
    foreach (var chunk in chunks)
      content += chunk.ToString();
    Assert.That(content, Is.EqualTo("Hello World"));
  }

  [Test]
  [Category("EdgeCase")]
  public void GetChunks_EmptyBuilder_NoContent() {
    var sb = new StringBuilder();
    var chunks = sb.GetChunks();
    var totalLength = 0;
    foreach (var chunk in chunks)
      totalLength += chunk.Length;
    // Note: Native .NET 6.0+ returns 1 empty chunk, polyfill returns 0 chunks
    // Both correctly result in zero total content length
    Assert.That(totalLength, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void GetChunks_LargeContent_ReturnsAllContent() {
    var sb = new StringBuilder();
    var expected = new string('X', 10000);
    sb.Append(expected);
    var chunks = sb.GetChunks();
    var content = "";
    foreach (var chunk in chunks)
      content += chunk.ToString();
    Assert.That(content, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void GetChunks_MultipleAppends_ReturnsAllContent() {
    var sb = new StringBuilder();
    sb.Append("Hello");
    sb.Append(" ");
    sb.Append("World");
    var chunks = sb.GetChunks();
    var content = "";
    foreach (var chunk in chunks)
      content += chunk.ToString();
    Assert.That(content, Is.EqualTo("Hello World"));
  }

  #endregion

  #region AppendJoin

  [Test]
  [Category("HappyPath")]
  public void AppendJoin_StringSeparator_JoinsValues() {
    var sb = new StringBuilder();
    sb.AppendJoin(", ", "a", "b", "c");
    Assert.That(sb.ToString(), Is.EqualTo("a, b, c"));
  }

  [Test]
  [Category("HappyPath")]
  public void AppendJoin_CharSeparator_JoinsValues() {
    var sb = new StringBuilder();
    sb.AppendJoin(',', "a", "b", "c");
    Assert.That(sb.ToString(), Is.EqualTo("a,b,c"));
  }

  [Test]
  [Category("HappyPath")]
  public void AppendJoin_EmptyArray_AppendsNothing() {
    var sb = new StringBuilder("prefix");
    sb.AppendJoin(", ", Array.Empty<string>());
    Assert.That(sb.ToString(), Is.EqualTo("prefix"));
  }

  [Test]
  [Category("HappyPath")]
  public void AppendJoin_SingleElement_NoSeparator() {
    var sb = new StringBuilder();
    sb.AppendJoin(", ", "single");
    Assert.That(sb.ToString(), Is.EqualTo("single"));
  }

  [Test]
  [Category("EdgeCase")]
  public void AppendJoin_NullSeparator_UsesEmpty() {
    var sb = new StringBuilder();
    sb.AppendJoin((string?)null, "a", "b", "c");
    Assert.That(sb.ToString(), Is.EqualTo("abc"));
  }

  #endregion

}
