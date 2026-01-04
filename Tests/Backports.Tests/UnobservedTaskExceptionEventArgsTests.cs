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
using System.Threading.Tasks;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("UnobservedTaskExceptionEventArgs")]
public class UnobservedTaskExceptionEventArgsTests {

  #region Constructor

  [Test]
  [Category("HappyPath")]
  public void UnobservedTaskExceptionEventArgs_Constructor_StoresException() {
    var innerException = new InvalidOperationException("Test");
    var aggregateException = new AggregateException(innerException);
    var args = new UnobservedTaskExceptionEventArgs(aggregateException);
    Assert.That(args.Exception, Is.SameAs(aggregateException));
  }

  [Test]
  [Category("HappyPath")]
  public void UnobservedTaskExceptionEventArgs_Constructor_ObservedIsFalseByDefault() {
    var aggregateException = new AggregateException(new Exception("Test"));
    var args = new UnobservedTaskExceptionEventArgs(aggregateException);
    Assert.That(args.Observed, Is.False);
  }

  #endregion

  #region SetObserved

  [Test]
  [Category("HappyPath")]
  public void UnobservedTaskExceptionEventArgs_SetObserved_SetsObservedToTrue() {
    var aggregateException = new AggregateException(new Exception("Test"));
    var args = new UnobservedTaskExceptionEventArgs(aggregateException);
    args.SetObserved();
    Assert.That(args.Observed, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void UnobservedTaskExceptionEventArgs_SetObserved_CanBeCalledMultipleTimes() {
    var aggregateException = new AggregateException(new Exception("Test"));
    var args = new UnobservedTaskExceptionEventArgs(aggregateException);
    args.SetObserved();
    args.SetObserved();
    Assert.That(args.Observed, Is.True);
  }

  #endregion

  #region Exception Property

  [Test]
  [Category("HappyPath")]
  public void UnobservedTaskExceptionEventArgs_Exception_ContainsInnerExceptions() {
    var innerException1 = new ArgumentException("Arg1");
    var innerException2 = new InvalidOperationException("Invalid");
    var aggregateException = new AggregateException(innerException1, innerException2);
    var args = new UnobservedTaskExceptionEventArgs(aggregateException);

    Assert.That(args.Exception.InnerExceptions.Count, Is.EqualTo(2));
    Assert.That(args.Exception.InnerExceptions[0], Is.SameAs(innerException1));
    Assert.That(args.Exception.InnerExceptions[1], Is.SameAs(innerException2));
  }

  [Test]
  [Category("HappyPath")]
  public void UnobservedTaskExceptionEventArgs_Exception_WithSingleInnerException() {
    var innerException = new ArgumentNullException("param");
    var aggregateException = new AggregateException(innerException);
    var args = new UnobservedTaskExceptionEventArgs(aggregateException);

    Assert.That(args.Exception.InnerExceptions.Count, Is.EqualTo(1));
    Assert.That(args.Exception.InnerExceptions[0], Is.SameAs(innerException));
  }

  #endregion

  #region Inheritance

  [Test]
  [Category("HappyPath")]
  public void UnobservedTaskExceptionEventArgs_InheritsFromEventArgs() {
    var aggregateException = new AggregateException(new Exception("Test"));
    var args = new UnobservedTaskExceptionEventArgs(aggregateException);
    Assert.That(args, Is.InstanceOf<EventArgs>());
  }

  #endregion

}
