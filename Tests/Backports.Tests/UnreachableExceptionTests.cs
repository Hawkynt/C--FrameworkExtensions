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
using System.Diagnostics;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("UnreachableException")]
public class UnreachableExceptionTests {

  #region Constructor Tests

  [Test]
  [Category("HappyPath")]
  public void DefaultConstructor_HasNonEmptyMessage() {
    var exception = new UnreachableException();
    Assert.That(exception.Message, Is.Not.Null.And.Not.Empty);
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultConstructor_HasNoInnerException() {
    var exception = new UnreachableException();
    Assert.That(exception.InnerException, Is.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void MessageConstructor_WithMessage_SetsMessage() {
    const string customMessage = "Custom unreachable message";
    var exception = new UnreachableException(customMessage);
    Assert.That(exception.Message, Is.EqualTo(customMessage));
  }

  [Test]
  [Category("EdgeCase")]
  public void MessageConstructor_WithNullMessage_HasNonNullMessage() {
    var exception = new UnreachableException(null);
    Assert.That(exception.Message, Is.Not.Null.And.Not.Empty);
  }

  [Test]
  [Category("HappyPath")]
  public void MessageConstructor_HasNoInnerException() {
    var exception = new UnreachableException("test");
    Assert.That(exception.InnerException, Is.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void MessageAndInnerExceptionConstructor_SetsMessage() {
    const string customMessage = "Custom unreachable message";
    var inner = new InvalidOperationException("inner");
    var exception = new UnreachableException(customMessage, inner);
    Assert.That(exception.Message, Is.EqualTo(customMessage));
  }

  [Test]
  [Category("HappyPath")]
  public void MessageAndInnerExceptionConstructor_SetsInnerException() {
    const string customMessage = "Custom unreachable message";
    var inner = new InvalidOperationException("inner");
    var exception = new UnreachableException(customMessage, inner);
    Assert.That(exception.InnerException, Is.SameAs(inner));
  }

  [Test]
  [Category("EdgeCase")]
  public void MessageAndInnerExceptionConstructor_WithNullMessage_HasNonNullMessage() {
    var inner = new InvalidOperationException("inner");
    var exception = new UnreachableException(null, inner);
    Assert.That(exception.Message, Is.Not.Null.And.Not.Empty);
  }

  [Test]
  [Category("EdgeCase")]
  public void MessageAndInnerExceptionConstructor_WithNullInnerException_HasNullInnerException() {
    var exception = new UnreachableException("test", null);
    Assert.That(exception.InnerException, Is.Null);
  }

  #endregion

  #region Inheritance Tests

  [Test]
  [Category("HappyPath")]
  public void UnreachableException_InheritsFromException() {
    var exception = new UnreachableException();
    Assert.That(exception, Is.InstanceOf<Exception>());
  }

  [Test]
  [Category("HappyPath")]
  public void UnreachableException_CanBeCaughtAsException() {
    Assert.Catch<Exception>(() => throw new UnreachableException());
  }

  [Test]
  [Category("HappyPath")]
  public void UnreachableException_CanBeCaughtAsUnreachableException() {
    Assert.Throws<UnreachableException>(() => throw new UnreachableException());
  }

  #endregion

  #region Usage Pattern Tests

  [Test]
  [Category("HappyPath")]
  public void UnreachableException_ThrowAndCatch_PreservesMessage() {
    const string message = "This code path should never execute";
    try {
      throw new UnreachableException(message);
    } catch (UnreachableException ex) {
      Assert.That(ex.Message, Is.EqualTo(message));
    }
  }

  [Test]
  [Category("HappyPath")]
  public void UnreachableException_ThrowAndCatch_PreservesInnerException() {
    var inner = new ArgumentException("Original error");
    try {
      throw new UnreachableException("Wrapper", inner);
    } catch (UnreachableException ex) {
      Assert.That(ex.InnerException, Is.SameAs(inner));
      Assert.That(ex.InnerException?.Message, Is.EqualTo("Original error"));
    }
  }

  #endregion

}
