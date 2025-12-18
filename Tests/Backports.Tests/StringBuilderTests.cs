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

}
