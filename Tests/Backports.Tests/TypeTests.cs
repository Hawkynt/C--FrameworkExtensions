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
using System.Collections.Generic;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Type")]
public class TypeTests {

  #region Type.IsAssignableTo

  [Test]
  [Category("HappyPath")]
  public void IsAssignableTo_DerivedToBase_ReturnsTrue() {
    var derivedType = typeof(List<int>);
    var baseType = typeof(IEnumerable<int>);
    Assert.That(derivedType.IsAssignableTo(baseType), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void IsAssignableTo_SameType_ReturnsTrue() {
    var type = typeof(string);
    Assert.That(type.IsAssignableTo(typeof(string)), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void IsAssignableTo_UnrelatedTypes_ReturnsFalse() {
    var type1 = typeof(string);
    var type2 = typeof(int);
    Assert.That(type1.IsAssignableTo(type2), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void IsAssignableTo_ToObject_ReturnsTrue() {
    var type = typeof(string);
    Assert.That(type.IsAssignableTo(typeof(object)), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void IsAssignableTo_NullTargetType_ReturnsFalse() {
    var type = typeof(string);
    Assert.That(type.IsAssignableTo(null), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void IsAssignableTo_ClassToInterface_ReturnsTrue() {
    var type = typeof(string);
    Assert.That(type.IsAssignableTo(typeof(IComparable)), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void IsAssignableTo_InterfaceToClass_ReturnsFalse() {
    var type = typeof(IComparable);
    Assert.That(type.IsAssignableTo(typeof(string)), Is.False);
  }

  #endregion

}
