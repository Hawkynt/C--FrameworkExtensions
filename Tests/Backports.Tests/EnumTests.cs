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
public class EnumTests {

  private enum TestEnum { One = 1, Two = 2, Three = 3 }

  #region GetValues

  [Test]
  [Category("HappyPath")]
  public void GetValues_ReturnsAllValues() {
    var values = Enum.GetValues<TestEnum>();
    Assert.That(values, Is.EquivalentTo(new[] { TestEnum.One, TestEnum.Two, TestEnum.Three }));
  }

  #endregion

  #region GetNames

  [Test]
  [Category("HappyPath")]
  public void GetNames_ReturnsAllNames() {
    var names = Enum.GetNames<TestEnum>();
    Assert.That(names, Is.EquivalentTo(new[] { "One", "Two", "Three" }));
  }

  #endregion

  #region GetName

  [Test]
  [Category("HappyPath")]
  public void GetName_ReturnsCorrectName() {
    Assert.That(Enum.GetName(TestEnum.Two), Is.EqualTo("Two"));
  }

  #endregion

  #region IsDefined

  [Test]
  [Category("HappyPath")]
  public void IsDefined_DefinedValue_ReturnsTrue() {
    Assert.That(Enum.IsDefined(TestEnum.One), Is.True);
  }

  #endregion

}
