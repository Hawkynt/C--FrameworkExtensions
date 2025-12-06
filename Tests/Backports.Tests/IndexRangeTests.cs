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
[Category("IndexRange")]
public class IndexRangeTests {

  #region Index - Basic Construction

  [Test]
  [Category("HappyPath")]
  public void Index_FromStart_CreatesCorrectIndex() {
    var index = Index.FromStart(5);
    Assert.That(index.Value, Is.EqualTo(5));
    Assert.That(index.IsFromEnd, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Index_FromEnd_CreatesCorrectIndex() {
    var index = Index.FromEnd(3);
    Assert.That(index.Value, Is.EqualTo(3));
    Assert.That(index.IsFromEnd, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Index_Constructor_FromStart_CreatesCorrectIndex() {
    var index = new Index(5, fromEnd: false);
    Assert.That(index.Value, Is.EqualTo(5));
    Assert.That(index.IsFromEnd, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Index_Constructor_FromEnd_CreatesCorrectIndex() {
    var index = new Index(3, fromEnd: true);
    Assert.That(index.Value, Is.EqualTo(3));
    Assert.That(index.IsFromEnd, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Index_ImplicitConversion_FromInt() {
    Index index = 5;
    Assert.That(index.Value, Is.EqualTo(5));
    Assert.That(index.IsFromEnd, Is.False);
  }

  #endregion

  #region Index - Static Properties

  [Test]
  [Category("HappyPath")]
  public void Index_Start_ReturnsZeroFromStart() {
    var index = Index.Start;
    Assert.That(index.Value, Is.EqualTo(0));
    Assert.That(index.IsFromEnd, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Index_End_ReturnsZeroFromEnd() {
    var index = Index.End;
    Assert.That(index.Value, Is.EqualTo(0));
    Assert.That(index.IsFromEnd, Is.True);
  }

  #endregion

  #region Index - GetOffset

  [Test]
  [Category("HappyPath")]
  public void Index_GetOffset_FromStart_ReturnsValue() {
    var index = Index.FromStart(3);
    Assert.That(index.GetOffset(10), Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Index_GetOffset_FromEnd_CalculatesCorrectly() {
    var index = Index.FromEnd(2);
    Assert.That(index.GetOffset(10), Is.EqualTo(8));
  }

  [Test]
  [Category("HappyPath")]
  public void Index_GetOffset_End_ReturnsLength() {
    Assert.That(Index.End.GetOffset(10), Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Index_GetOffset_Start_ReturnsZero() {
    Assert.That(Index.Start.GetOffset(10), Is.EqualTo(0));
  }

  #endregion

  #region Index - Equality

  [Test]
  [Category("HappyPath")]
  public void Index_Equals_SameValues_ReturnsTrue() {
    var a = Index.FromStart(5);
    var b = Index.FromStart(5);
    Assert.That(a.Equals(b), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Index_Equals_DifferentValues_ReturnsFalse() {
    var a = Index.FromStart(5);
    var b = Index.FromStart(3);
    Assert.That(a.Equals(b), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Index_Equals_DifferentDirection_ReturnsFalse() {
    var a = Index.FromStart(5);
    var b = Index.FromEnd(5);
    Assert.That(a.Equals(b), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Index_GetHashCode_SameValues_SameHash() {
    var a = Index.FromStart(5);
    var b = Index.FromStart(5);
    Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
  }

  #endregion

  #region Index - ToString

  [Test]
  [Category("HappyPath")]
  public void Index_ToString_FromStart_ReturnsValue() {
    var index = Index.FromStart(5);
    Assert.That(index.ToString(), Is.EqualTo("5"));
  }

  [Test]
  [Category("HappyPath")]
  public void Index_ToString_FromEnd_ReturnsCaret() {
    var index = Index.FromEnd(3);
    Assert.That(index.ToString(), Is.EqualTo("^3"));
  }

  #endregion

  #region Index - Hat Operator (^)

  [Test]
  [Category("HappyPath")]
  public void Index_HatOperator_CreatesFromEndIndex() {
    var index = ^1;
    Assert.That(index.IsFromEnd, Is.True);
    Assert.That(index.Value, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Index_HatOperator_Zero_CreatesEnd() {
    var index = ^0;
    Assert.That(index.IsFromEnd, Is.True);
    Assert.That(index.Value, Is.EqualTo(0));
  }

  #endregion

  #region Range - Basic Construction

  [Test]
  [Category("HappyPath")]
  public void Range_Constructor_CreatesCorrectRange() {
    var range = new Range(Index.FromStart(2), Index.FromStart(5));
    Assert.That(range.Start.Value, Is.EqualTo(2));
    Assert.That(range.End.Value, Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void Range_StartAt_CreatesCorrectRange() {
    var range = Range.StartAt(3);
    Assert.That(range.Start.Value, Is.EqualTo(3));
    Assert.That(range.End, Is.EqualTo(Index.End));
  }

  [Test]
  [Category("HappyPath")]
  public void Range_EndAt_CreatesCorrectRange() {
    var range = Range.EndAt(5);
    Assert.That(range.Start, Is.EqualTo(Index.Start));
    Assert.That(range.End.Value, Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void Range_All_CreatesFullRange() {
    var range = Range.All;
    Assert.That(range.Start, Is.EqualTo(Index.Start));
    Assert.That(range.End, Is.EqualTo(Index.End));
  }

  #endregion

  #region Range - Syntax

  [Test]
  [Category("HappyPath")]
  public void Range_DotDot_CreatesRange() {
    var range = 1..5;
    Assert.That(range.Start.Value, Is.EqualTo(1));
    Assert.That(range.End.Value, Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void Range_DotDot_WithHat_CreatesRange() {
    var range = 1..^1;
    Assert.That(range.Start.Value, Is.EqualTo(1));
    Assert.That(range.End.Value, Is.EqualTo(1));
    Assert.That(range.End.IsFromEnd, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Range_DotDot_BothHat_CreatesRange() {
    var range = ^3..^1;
    Assert.That(range.Start.IsFromEnd, Is.True);
    Assert.That(range.End.IsFromEnd, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Range_DotDot_NoStart_CreatesFromStart() {
    var range = ..5;
    Assert.That(range.Start.Value, Is.EqualTo(0));
    Assert.That(range.End.Value, Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void Range_DotDot_NoEnd_CreatesToEnd() {
    var range = 2..;
    Assert.That(range.Start.Value, Is.EqualTo(2));
    Assert.That(range.End.IsFromEnd, Is.True);
    Assert.That(range.End.Value, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Range_DotDot_Empty_CreatesFullRange() {
    var range = ..;
    Assert.That(range.Start.Value, Is.EqualTo(0));
    Assert.That(range.End.IsFromEnd, Is.True);
    Assert.That(range.End.Value, Is.EqualTo(0));
  }

  #endregion

  #region Range - GetOffsetAndLength

  [Test]
  [Category("HappyPath")]
  public void Range_GetOffsetAndLength_SimpleRange() {
    var range = 2..5;
    var (offset, length) = range.GetOffsetAndLength(10);
    Assert.That(offset, Is.EqualTo(2));
    Assert.That(length, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Range_GetOffsetAndLength_FromEndRange() {
    var range = ^5..^2;
    var (offset, length) = range.GetOffsetAndLength(10);
    Assert.That(offset, Is.EqualTo(5));
    Assert.That(length, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Range_GetOffsetAndLength_FullRange() {
    var range = ..;
    var (offset, length) = range.GetOffsetAndLength(10);
    Assert.That(offset, Is.EqualTo(0));
    Assert.That(length, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Range_GetOffsetAndLength_EmptyRange() {
    var range = 5..5;
    var (offset, length) = range.GetOffsetAndLength(10);
    Assert.That(offset, Is.EqualTo(5));
    Assert.That(length, Is.EqualTo(0));
  }

  #endregion

  #region Range - Equality

  [Test]
  [Category("HappyPath")]
  public void Range_Equals_SameValues_ReturnsTrue() {
    var a = 1..5;
    var b = 1..5;
    Assert.That(a.Equals(b), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Range_Equals_DifferentValues_ReturnsFalse() {
    var a = 1..5;
    var b = 1..6;
    Assert.That(a.Equals(b), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Range_GetHashCode_SameValues_SameHash() {
    var a = 1..5;
    var b = 1..5;
    Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
  }

  #endregion

  #region Range - ToString

  [Test]
  [Category("HappyPath")]
  public void Range_ToString_SimpleRange() {
    var range = 1..5;
    Assert.That(range.ToString(), Is.EqualTo("1..5"));
  }

  [Test]
  [Category("HappyPath")]
  public void Range_ToString_FromEndRange() {
    var range = ^3..^1;
    Assert.That(range.ToString(), Is.EqualTo("^3..^1"));
  }

  #endregion

  #region Edge Cases

  [Test]
  [Category("Exception")]
  public void Index_FromStart_NegativeValue_ThrowsException() {
    Assert.Throws<ArgumentOutOfRangeException>(() => Index.FromStart(-1));
  }

  [Test]
  [Category("Exception")]
  public void Index_FromEnd_NegativeValue_ThrowsException() {
    Assert.Throws<ArgumentOutOfRangeException>(() => Index.FromEnd(-1));
  }

  #endregion

}
