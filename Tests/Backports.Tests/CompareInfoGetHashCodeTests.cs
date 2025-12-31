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
using System.Globalization;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("CompareInfo")]
public class CompareInfoGetHashCodeTests {

  #region Ordinal Comparison

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_Ordinal_SameStrings_ReturnsSameHash() {
    var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
    var hash1 = compareInfo.GetHashCode("Hello", CompareOptions.Ordinal);
    var hash2 = compareInfo.GetHashCode("Hello", CompareOptions.Ordinal);
    Assert.That(hash1, Is.EqualTo(hash2));
  }

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_Ordinal_DifferentStrings_ReturnsDifferentHash() {
    var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
    var hash1 = compareInfo.GetHashCode("Hello", CompareOptions.Ordinal);
    var hash2 = compareInfo.GetHashCode("World", CompareOptions.Ordinal);
    Assert.That(hash1, Is.Not.EqualTo(hash2));
  }

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_Ordinal_CaseSensitive() {
    var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
    var hashLower = compareInfo.GetHashCode("hello", CompareOptions.Ordinal);
    var hashUpper = compareInfo.GetHashCode("HELLO", CompareOptions.Ordinal);
    Assert.That(hashLower, Is.Not.EqualTo(hashUpper));
  }

  #endregion

  #region OrdinalIgnoreCase Comparison

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_OrdinalIgnoreCase_SameStrings_ReturnsSameHash() {
    var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
    var hash1 = compareInfo.GetHashCode("Hello", CompareOptions.OrdinalIgnoreCase);
    var hash2 = compareInfo.GetHashCode("Hello", CompareOptions.OrdinalIgnoreCase);
    Assert.That(hash1, Is.EqualTo(hash2));
  }

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_OrdinalIgnoreCase_DifferentCase_ReturnsSameHash() {
    var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
    var hashLower = compareInfo.GetHashCode("hello", CompareOptions.OrdinalIgnoreCase);
    var hashUpper = compareInfo.GetHashCode("HELLO", CompareOptions.OrdinalIgnoreCase);
    Assert.That(hashLower, Is.EqualTo(hashUpper));
  }

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_OrdinalIgnoreCase_MixedCase_ReturnsSameHash() {
    var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
    var hash1 = compareInfo.GetHashCode("HeLLo", CompareOptions.OrdinalIgnoreCase);
    var hash2 = compareInfo.GetHashCode("hEllO", CompareOptions.OrdinalIgnoreCase);
    Assert.That(hash1, Is.EqualTo(hash2));
  }

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_OrdinalIgnoreCase_DifferentStrings_ReturnsDifferentHash() {
    var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
    var hash1 = compareInfo.GetHashCode("Hello", CompareOptions.OrdinalIgnoreCase);
    var hash2 = compareInfo.GetHashCode("World", CompareOptions.OrdinalIgnoreCase);
    Assert.That(hash1, Is.Not.EqualTo(hash2));
  }

  #endregion

  #region Culture-Sensitive Comparison (None)

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_None_SameStrings_ReturnsSameHash() {
    var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
    var hash1 = compareInfo.GetHashCode("Hello", CompareOptions.None);
    var hash2 = compareInfo.GetHashCode("Hello", CompareOptions.None);
    Assert.That(hash1, Is.EqualTo(hash2));
  }

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_None_DifferentStrings_ReturnsDifferentHash() {
    var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
    var hash1 = compareInfo.GetHashCode("Hello", CompareOptions.None);
    var hash2 = compareInfo.GetHashCode("World", CompareOptions.None);
    Assert.That(hash1, Is.Not.EqualTo(hash2));
  }

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_None_CaseSensitive() {
    var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
    var hashLower = compareInfo.GetHashCode("hello", CompareOptions.None);
    var hashUpper = compareInfo.GetHashCode("HELLO", CompareOptions.None);
    Assert.That(hashLower, Is.Not.EqualTo(hashUpper));
  }

  #endregion

  #region Culture-Sensitive Comparison (IgnoreCase)

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_IgnoreCase_SameStrings_ReturnsSameHash() {
    var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
    var hash1 = compareInfo.GetHashCode("Hello", CompareOptions.IgnoreCase);
    var hash2 = compareInfo.GetHashCode("Hello", CompareOptions.IgnoreCase);
    Assert.That(hash1, Is.EqualTo(hash2));
  }

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_IgnoreCase_DifferentCase_ReturnsSameHash() {
    var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
    var hashLower = compareInfo.GetHashCode("hello", CompareOptions.IgnoreCase);
    var hashUpper = compareInfo.GetHashCode("HELLO", CompareOptions.IgnoreCase);
    Assert.That(hashLower, Is.EqualTo(hashUpper));
  }

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_IgnoreCase_DifferentStrings_ReturnsDifferentHash() {
    var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
    var hash1 = compareInfo.GetHashCode("Hello", CompareOptions.IgnoreCase);
    var hash2 = compareInfo.GetHashCode("World", CompareOptions.IgnoreCase);
    Assert.That(hash1, Is.Not.EqualTo(hash2));
  }

  #endregion

  #region Different Cultures

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_CurrentCulture_ReturnsConsistentHash() {
    var compareInfo = CultureInfo.CurrentCulture.CompareInfo;
    var hash1 = compareInfo.GetHashCode("Test", CompareOptions.None);
    var hash2 = compareInfo.GetHashCode("Test", CompareOptions.None);
    Assert.That(hash1, Is.EqualTo(hash2));
  }

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_InvariantCulture_ReturnsConsistentHash() {
    var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
    var hash1 = compareInfo.GetHashCode("Test", CompareOptions.None);
    var hash2 = compareInfo.GetHashCode("Test", CompareOptions.None);
    Assert.That(hash1, Is.EqualTo(hash2));
  }

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_SpecificCulture_ReturnsConsistentHash() {
    var compareInfo = CultureInfo.GetCultureInfo("en-US").CompareInfo;
    var hash1 = compareInfo.GetHashCode("Test", CompareOptions.None);
    var hash2 = compareInfo.GetHashCode("Test", CompareOptions.None);
    Assert.That(hash1, Is.EqualTo(hash2));
  }

  #endregion

  #region Edge Cases

  [Test]
  [Category("EdgeCase")]
  public void GetHashCode_EmptyString_ReturnsHash() {
    var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
    var hash = compareInfo.GetHashCode(string.Empty, CompareOptions.None);
    // Just verify it doesn't throw and returns something
    Assert.That(hash, Is.Not.Null);
  }

  [Test]
  [Category("EdgeCase")]
  public void GetHashCode_WhitespaceString_ReturnsHash() {
    var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
    var hash = compareInfo.GetHashCode("   ", CompareOptions.None);
    Assert.That(hash, Is.Not.Null);
  }

  [Test]
  [Category("EdgeCase")]
  public void GetHashCode_SpecialCharacters_ReturnsHash() {
    var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
    var hash = compareInfo.GetHashCode("!@#$%^&*()", CompareOptions.None);
    Assert.That(hash, Is.Not.Null);
  }

  [Test]
  [Category("EdgeCase")]
  public void GetHashCode_UnicodeString_ReturnsHash() {
    var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
    var hash = compareInfo.GetHashCode("こんにちは", CompareOptions.None);
    Assert.That(hash, Is.Not.Null);
  }

  [Test]
  [Category("Exception")]
  public void GetHashCode_NullSource_ThrowsArgumentNullException() {
    var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
    Assert.Throws<ArgumentNullException>(() => compareInfo.GetHashCode(null!, CompareOptions.None));
  }

  [Test]
  [Category("Exception")]
  public void GetHashCode_OrdinalWithOtherOptions_ThrowsArgumentException() {
    var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
    Assert.Throws<ArgumentException>(() => compareInfo.GetHashCode("test", CompareOptions.Ordinal | CompareOptions.IgnoreCase));
  }

  [Test]
  [Category("Exception")]
  public void GetHashCode_OrdinalIgnoreCaseWithOtherOptions_ThrowsArgumentException() {
    var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
    Assert.Throws<ArgumentException>(() => compareInfo.GetHashCode("test", CompareOptions.OrdinalIgnoreCase | CompareOptions.IgnoreSymbols));
  }

  #endregion

  #region Hash Consistency with Compare

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_ConsistentWithCompare_OrdinalIgnoreCase() {
    var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
    var s1 = "Hello";
    var s2 = "HELLO";

    // If Compare says they're equal, GetHashCode should return same value
    var compareResult = compareInfo.Compare(s1, s2, CompareOptions.OrdinalIgnoreCase);
    var hash1 = compareInfo.GetHashCode(s1, CompareOptions.OrdinalIgnoreCase);
    var hash2 = compareInfo.GetHashCode(s2, CompareOptions.OrdinalIgnoreCase);

    Assert.That(compareResult, Is.EqualTo(0));
    Assert.That(hash1, Is.EqualTo(hash2));
  }

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_ConsistentWithCompare_IgnoreCase() {
    var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
    var s1 = "Test";
    var s2 = "TEST";

    var compareResult = compareInfo.Compare(s1, s2, CompareOptions.IgnoreCase);
    var hash1 = compareInfo.GetHashCode(s1, CompareOptions.IgnoreCase);
    var hash2 = compareInfo.GetHashCode(s2, CompareOptions.IgnoreCase);

    Assert.That(compareResult, Is.EqualTo(0));
    Assert.That(hash1, Is.EqualTo(hash2));
  }

  #endregion

}
