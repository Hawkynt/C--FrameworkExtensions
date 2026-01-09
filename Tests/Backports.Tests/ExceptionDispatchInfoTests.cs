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
using System.Runtime.ExceptionServices;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("ExceptionDispatchInfo")]
public class ExceptionDispatchInfoTests {

  #region Capture

  [Test]
  [Category("HappyPath")]
  public void Capture_WithException_ReturnsExceptionDispatchInfo() {
    var originalException = new InvalidOperationException("Test exception");
    var edi = ExceptionDispatchInfo.Capture(originalException);
    Assert.That(edi, Is.Not.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void Capture_SourceException_ReturnsSameException() {
    var originalException = new InvalidOperationException("Test exception");
    var edi = ExceptionDispatchInfo.Capture(originalException);
    Assert.That(edi.SourceException, Is.SameAs(originalException));
  }

  [Test]
  [Category("Exception")]
  public void Capture_WithNull_ThrowsArgumentNullException() {
    Assert.Throws<ArgumentNullException>(() => ExceptionDispatchInfo.Capture(null!));
  }

  #endregion

  #region Throw

  [Test]
  [Category("HappyPath")]
  public void Throw_RethrowsCapturedException() {
    var originalException = new InvalidOperationException("Test exception");
    var edi = ExceptionDispatchInfo.Capture(originalException);

    var thrownException = Assert.Throws<InvalidOperationException>(() => edi.Throw());
    Assert.That(thrownException, Is.SameAs(originalException));
  }

  [Test]
  [Category("HappyPath")]
  public void Throw_WithDifferentExceptionTypes_RethrowsCorrectType() {
    var argumentException = new ArgumentException("Argument error");
    var edi = ExceptionDispatchInfo.Capture(argumentException);

    var thrown = Assert.Throws<ArgumentException>(() => edi.Throw());
    Assert.That(thrown.Message, Is.EqualTo("Argument error"));
  }

  [Test]
  [Category("HappyPath")]
  public void Throw_Static_ThrowsProvidedException() {
    var originalException = new InvalidOperationException("Test exception");

    var thrown = Assert.Throws<InvalidOperationException>(() => ExceptionDispatchInfo.Throw(originalException));
    Assert.That(thrown, Is.SameAs(originalException));
  }

  [Test]
  [Category("Exception")]
  public void Throw_Static_WithNull_ThrowsArgumentNullException() {
    Assert.Throws<ArgumentNullException>(() => ExceptionDispatchInfo.Throw(null!));
  }

  #endregion

  #region Real World Scenarios

  [Test]
  [Category("HappyPath")]
  public void ExceptionDispatchInfo_CaptureAndRethrow_InDifferentContext() {
    ExceptionDispatchInfo? captured = null;

    try {
      ThrowException();
    } catch (InvalidOperationException ex) {
      captured = ExceptionDispatchInfo.Capture(ex);
    }

    Assert.That(captured, Is.Not.Null);
    var thrown = Assert.Throws<InvalidOperationException>(() => captured!.Throw());
    Assert.That(thrown.Message, Is.EqualTo("Original exception"));
  }

  [Test]
  [Category("HappyPath")]
  public void ExceptionDispatchInfo_PreservesInnerException() {
    var innerException = new ArgumentException("Inner");
    var outerException = new InvalidOperationException("Outer", innerException);
    var edi = ExceptionDispatchInfo.Capture(outerException);

    var thrown = Assert.Throws<InvalidOperationException>(() => edi.Throw());
    Assert.That(thrown.InnerException, Is.SameAs(innerException));
  }

  [Test]
  [Category("HappyPath")]
  public void ExceptionDispatchInfo_WorksWithCustomExceptions() {
    var customException = new CustomTestException("Custom message", 42);
    var edi = ExceptionDispatchInfo.Capture(customException);

    var thrown = Assert.Throws<CustomTestException>(() => edi.Throw());
    Assert.That(thrown.CustomValue, Is.EqualTo(42));
    Assert.That(thrown.Message, Is.EqualTo("Custom message"));
  }

  #endregion

  #region Helper Methods

  private static void ThrowException() => throw new InvalidOperationException("Original exception");

  private sealed class CustomTestException(string message, int customValue) : Exception(message) {
    public int CustomValue { get; } = customValue;
  }

  #endregion

}
