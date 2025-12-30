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
using System.Buffers.Text;
using System.Text;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Base64")]
public class Base64IsValidTests {

  #region IsValid - ReadOnlySpan<char>

  [Test]
  [Category("HappyPath")]
  public void IsValid_ValidBase64String_ReturnsTrue() {
    var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes("Hello, World!"));

    var result = Base64.IsValid(encoded.AsSpan());

    Assert.That(result, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void IsValid_EmptyString_ReturnsTrue() {
    var result = Base64.IsValid(ReadOnlySpan<char>.Empty);

    Assert.That(result, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void IsValid_ValidBase64WithPadding_ReturnsTrue() {
    var encoded = Convert.ToBase64String([1, 2, 3]);

    var result = Base64.IsValid(encoded.AsSpan());

    Assert.That(result, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void IsValid_ValidBase64NoPadding_ReturnsTrue() {
    var encoded = Convert.ToBase64String([1, 2, 3, 4, 5, 6]);

    var result = Base64.IsValid(encoded.AsSpan());

    Assert.That(result, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void IsValid_InvalidCharacters_ReturnsFalse() {
    var result = Base64.IsValid("Invalid@Base64!".AsSpan());

    Assert.That(result, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void IsValid_InvalidLength_ReturnsFalse() {
    var result = Base64.IsValid("ABC".AsSpan());

    Assert.That(result, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void IsValid_WithDecodedLength_ReturnsCorrectLength() {
    var original = new byte[] { 1, 2, 3, 4, 5 };
    var encoded = Convert.ToBase64String(original);

    var result = Base64.IsValid(encoded.AsSpan(), out var decodedLength);

    Assert.That(result, Is.True);
    Assert.That(decodedLength, Is.EqualTo(original.Length));
  }

  #endregion

  #region IsValid - ReadOnlySpan<byte>

  [Test]
  [Category("HappyPath")]
  public void IsValid_ValidBase64Bytes_ReturnsTrue() {
    var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes("Hello, World!"));
    var bytes = Encoding.ASCII.GetBytes(encoded);

    var result = Base64.IsValid((ReadOnlySpan<byte>)bytes);

    Assert.That(result, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void IsValid_EmptyBytes_ReturnsTrue() {
    var result = Base64.IsValid(ReadOnlySpan<byte>.Empty);

    Assert.That(result, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void IsValid_InvalidBytes_ReturnsFalse() {
    var bytes = Encoding.ASCII.GetBytes("Invalid@Base64!");

    var result = Base64.IsValid((ReadOnlySpan<byte>)bytes);

    Assert.That(result, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void IsValid_Bytes_WithDecodedLength_ReturnsCorrectLength() {
    var original = new byte[] { 1, 2, 3, 4, 5 };
    var encoded = Convert.ToBase64String(original);
    var bytes = Encoding.ASCII.GetBytes(encoded);

    var result = Base64.IsValid((ReadOnlySpan<byte>)bytes, out var decodedLength);

    Assert.That(result, Is.True);
    Assert.That(decodedLength, Is.EqualTo(original.Length));
  }

  #endregion

  #region Large Data Tests

  [Test]
  [Category("HappyPath")]
  public void IsValid_LargeValidBase64_ReturnsTrue() {
    var largeData = new byte[1024];
    new Random(42).NextBytes(largeData);
    var encoded = Convert.ToBase64String(largeData);

    var result = Base64.IsValid(encoded.AsSpan());

    Assert.That(result, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void IsValid_LargeValidBase64Bytes_ReturnsTrue() {
    var largeData = new byte[1024];
    new Random(42).NextBytes(largeData);
    var encoded = Convert.ToBase64String(largeData);
    var bytes = Encoding.ASCII.GetBytes(encoded);

    var result = Base64.IsValid((ReadOnlySpan<byte>)bytes);

    Assert.That(result, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void IsValid_LargeInvalidBase64_ReturnsFalse() {
    var largeData = new byte[1024];
    new Random(42).NextBytes(largeData);
    var encoded = Convert.ToBase64String(largeData).ToCharArray();
    encoded[512] = '@';

    var result = Base64.IsValid(new string(encoded).AsSpan());

    Assert.That(result, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void IsValid_VeryLargeData_WorksCorrectly() {
    var largeData = new byte[10000];
    new Random(42).NextBytes(largeData);
    var encoded = Convert.ToBase64String(largeData);

    var result = Base64.IsValid(encoded.AsSpan());

    Assert.That(result, Is.True);
  }

  #endregion

  #region Edge Cases

  [Test]
  [Category("EdgeCase")]
  public void IsValid_AllValidChars_ReturnsTrue() {
    var allValidChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

    var result = Base64.IsValid(allValidChars.AsSpan());

    Assert.That(result, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void IsValid_OnlyPadding_ReturnsFalse() {
    var result = Base64.IsValid("====".AsSpan());

    Assert.That(result, Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  public void IsValid_PaddingInMiddle_ReturnsFalse() {
    var result = Base64.IsValid("AB==CD".AsSpan());

    Assert.That(result, Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  public void IsValid_SinglePaddedBlock_ReturnsTrue() {
    var result = Base64.IsValid("YQ==".AsSpan());

    Assert.That(result, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void IsValid_DoublePaddedBlock_ReturnsTrue() {
    var result = Base64.IsValid("YWI=".AsSpan());

    Assert.That(result, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void IsValid_WhitespaceOnly_ReturnsTrue() {
    // Whitespace is ignored, so whitespace-only is equivalent to empty string (valid)
    var result = Base64.IsValid("    ".AsSpan());

    Assert.That(result, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void IsValid_NewlinesInMiddle_ReturnsTrue() {
    // Whitespace is ignored, so "AAAA\nAAAA" is equivalent to "AAAAAAAA" (valid)
    var result = Base64.IsValid("AAAA\nAAAA".AsSpan());

    Assert.That(result, Is.True);
  }

  #endregion

}
