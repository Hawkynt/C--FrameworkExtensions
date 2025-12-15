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

using System.Collections.Generic;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("KeyValuePair")]
public class KeyValuePairTests {

  #region KeyValuePair.Create

  [Test]
  [Category("HappyPath")]
  public void Create_WithStringAndInt_ReturnsCorrectPair() {
    var kvp = KeyValuePair.Create("key", 42);
    Assert.That(kvp.Key, Is.EqualTo("key"));
    Assert.That(kvp.Value, Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void Create_WithIntAndString_ReturnsCorrectPair() {
    var kvp = KeyValuePair.Create(123, "value");
    Assert.That(kvp.Key, Is.EqualTo(123));
    Assert.That(kvp.Value, Is.EqualTo("value"));
  }

  [Test]
  [Category("EdgeCase")]
  public void Create_WithNullKey_ReturnsCorrectPair() {
    var kvp = KeyValuePair.Create<string?, int>(null, 42);
    Assert.That(kvp.Key, Is.Null);
    Assert.That(kvp.Value, Is.EqualTo(42));
  }

  [Test]
  [Category("EdgeCase")]
  public void Create_WithNullValue_ReturnsCorrectPair() {
    var kvp = KeyValuePair.Create<int, string?>(42, null);
    Assert.That(kvp.Key, Is.EqualTo(42));
    Assert.That(kvp.Value, Is.Null);
  }

  #endregion

  #region KeyValuePair.Deconstruct

  [Test]
  [Category("HappyPath")]
  public void Deconstruct_StringAndInt_ReturnsCorrectValues() {
    var kvp = new KeyValuePair<string, int>("key", 42);
    var (key, value) = kvp;
    Assert.That(key, Is.EqualTo("key"));
    Assert.That(value, Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void Deconstruct_IntAndString_ReturnsCorrectValues() {
    var kvp = new KeyValuePair<int, string>(123, "value");
    var (key, value) = kvp;
    Assert.That(key, Is.EqualTo(123));
    Assert.That(value, Is.EqualTo("value"));
  }

  [Test]
  [Category("EdgeCase")]
  public void Deconstruct_WithNullKey_ReturnsNullKey() {
    var kvp = new KeyValuePair<string?, int>(null, 42);
    var (key, value) = kvp;
    Assert.That(key, Is.Null);
    Assert.That(value, Is.EqualTo(42));
  }

  [Test]
  [Category("EdgeCase")]
  public void Deconstruct_WithNullValue_ReturnsNullValue() {
    var kvp = new KeyValuePair<int, string?>(42, null);
    var (key, value) = kvp;
    Assert.That(key, Is.EqualTo(42));
    Assert.That(value, Is.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void Deconstruct_InForeach_WorksCorrectly() {
    var dict = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };
    var count = 0;
    foreach (var (key, value) in dict) {
      Assert.That(key, Is.Not.Null);
      Assert.That(value, Is.GreaterThan(0));
      ++count;
    }
    Assert.That(count, Is.EqualTo(2));
  }

  #endregion

}
