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


}
