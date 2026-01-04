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
[Category("DateTimeOffset")]
public class DateTimeOffsetDeconstructTests {

  #region Deconstruct(date, time, offset) - Official API (.NET 8+)

  [Test]
  [Category("HappyPath")]
  public void Deconstruct_DateTimeOffset_ReturnsCorrectValues() {
    var dto = new DateTimeOffset(2024, 6, 15, 10, 30, 45, TimeSpan.FromHours(2));
    dto.Deconstruct(out DateOnly date, out TimeOnly time, out TimeSpan offset);
    Assert.That(date, Is.EqualTo(new DateOnly(2024, 6, 15)));
    Assert.That(time, Is.EqualTo(new TimeOnly(10, 30, 45)));
    Assert.That(offset, Is.EqualTo(TimeSpan.FromHours(2)));
  }

  [Test]
  [Category("HappyPath")]
  public void Deconstruct_DateTimeOffset_MidnightUtc() {
    var dto = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
    dto.Deconstruct(out DateOnly date, out TimeOnly time, out TimeSpan offset);
    Assert.That(date, Is.EqualTo(new DateOnly(2024, 1, 1)));
    Assert.That(time, Is.EqualTo(new TimeOnly(0, 0, 0)));
    Assert.That(offset, Is.EqualTo(TimeSpan.Zero));
  }

  [Test]
  [Category("EdgeCase")]
  public void Deconstruct_DateTimeOffset_NegativeOffset() {
    var dto = new DateTimeOffset(2024, 6, 15, 14, 45, 30, TimeSpan.FromHours(-8));
    dto.Deconstruct(out DateOnly date, out TimeOnly time, out TimeSpan offset);
    Assert.That(date, Is.EqualTo(new DateOnly(2024, 6, 15)));
    Assert.That(time, Is.EqualTo(new TimeOnly(14, 45, 30)));
    Assert.That(offset, Is.EqualTo(TimeSpan.FromHours(-8)));
  }

  [Test]
  [Category("EdgeCase")]
  public void Deconstruct_DateTimeOffset_MaxOffset() {
    var dto = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.FromHours(14));
    dto.Deconstruct(out DateOnly date, out TimeOnly time, out TimeSpan offset);
    Assert.That(date, Is.EqualTo(new DateOnly(2024, 6, 15)));
    Assert.That(time, Is.EqualTo(new TimeOnly(12, 0, 0)));
    Assert.That(offset, Is.EqualTo(TimeSpan.FromHours(14)));
  }

  [Test]
  [Category("EdgeCase")]
  public void Deconstruct_DateTimeOffset_MinOffset() {
    var dto = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.FromHours(-12));
    dto.Deconstruct(out DateOnly date, out TimeOnly time, out TimeSpan offset);
    Assert.That(date, Is.EqualTo(new DateOnly(2024, 6, 15)));
    Assert.That(time, Is.EqualTo(new TimeOnly(12, 0, 0)));
    Assert.That(offset, Is.EqualTo(TimeSpan.FromHours(-12)));
  }

  [Test]
  [Category("HappyPath")]
  public void Deconstruct_DateTimeOffset_WithMilliseconds() {
    var dto = new DateTimeOffset(2024, 6, 15, 10, 30, 45, 123, TimeSpan.FromHours(1));
    dto.Deconstruct(out DateOnly date, out TimeOnly time, out TimeSpan offset);
    Assert.That(date, Is.EqualTo(new DateOnly(2024, 6, 15)));
    Assert.That(time.Hour, Is.EqualTo(10));
    Assert.That(time.Minute, Is.EqualTo(30));
    Assert.That(time.Second, Is.EqualTo(45));
    Assert.That(time.Millisecond, Is.EqualTo(123));
    Assert.That(offset, Is.EqualTo(TimeSpan.FromHours(1)));
  }

  #endregion

}
