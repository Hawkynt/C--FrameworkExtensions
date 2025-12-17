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
[Category("AggregateException")]
public class AggregateExceptionTests {

  #region Constructors

  [Test]
  [Category("HappyPath")]
  public void AggregateException_DefaultConstructor_HasDefaultMessage() {
    var ex = new AggregateException();
    Assert.That(ex.Message, Is.Not.Null.And.Not.Empty);
    Assert.That(ex.InnerExceptions, Is.Not.Null);
    Assert.That(ex.InnerExceptions.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void AggregateException_MessageConstructor_HasMessage() {
    var ex = new AggregateException("Custom message");
    Assert.That(ex.Message, Is.EqualTo("Custom message"));
    Assert.That(ex.InnerExceptions.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void AggregateException_MessageAndInnerException_HasBoth() {
    var inner = new InvalidOperationException("Inner");
    var ex = new AggregateException("Outer", inner);
    Assert.That(ex.Message, Does.StartWith("Outer")); // Newer .NET appends inner messages
    Assert.That(ex.InnerException, Is.SameAs(inner));
    Assert.That(ex.InnerExceptions.Count, Is.EqualTo(1));
    Assert.That(ex.InnerExceptions[0], Is.SameAs(inner));
  }

  [Test]
  [Category("HappyPath")]
  public void AggregateException_ParamsConstructor_ContainsAllExceptions() {
    var ex1 = new InvalidOperationException("Ex1");
    var ex2 = new ArgumentException("Ex2");
    var ex3 = new NotSupportedException("Ex3");

    var aggregate = new AggregateException(ex1, ex2, ex3);
    Assert.That(aggregate.InnerExceptions.Count, Is.EqualTo(3));
    Assert.That(aggregate.InnerExceptions[0], Is.SameAs(ex1));
    Assert.That(aggregate.InnerExceptions[1], Is.SameAs(ex2));
    Assert.That(aggregate.InnerExceptions[2], Is.SameAs(ex3));
  }

  [Test]
  [Category("HappyPath")]
  public void AggregateException_EnumerableConstructor_ContainsAllExceptions() {
    var exceptions = new List<Exception> {
      new InvalidOperationException("Ex1"),
      new ArgumentException("Ex2")
    };

    var aggregate = new AggregateException(exceptions);
    Assert.That(aggregate.InnerExceptions.Count, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void AggregateException_MessageAndParams_HasMessageAndExceptions() {
    var ex1 = new InvalidOperationException("Ex1");
    var ex2 = new ArgumentException("Ex2");

    var aggregate = new AggregateException("Custom message", ex1, ex2);
    Assert.That(aggregate.Message, Does.StartWith("Custom message")); // Newer .NET appends inner messages
    Assert.That(aggregate.InnerExceptions.Count, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void AggregateException_MessageAndEnumerable_HasMessageAndExceptions() {
    var exceptions = new List<Exception> {
      new InvalidOperationException("Ex1")
    };

    var aggregate = new AggregateException("Custom message", exceptions);
    Assert.That(aggregate.Message, Does.StartWith("Custom message")); // Newer .NET appends inner messages
    Assert.That(aggregate.InnerExceptions.Count, Is.EqualTo(1));
  }

  #endregion

  #region InnerException

  [Test]
  [Category("HappyPath")]
  public void AggregateException_InnerException_IsFirstInnerException() {
    var ex1 = new InvalidOperationException("First");
    var ex2 = new ArgumentException("Second");

    var aggregate = new AggregateException(ex1, ex2);
    Assert.That(aggregate.InnerException, Is.SameAs(ex1));
  }

  [Test]
  [Category("EdgeCase")]
  public void AggregateException_EmptyExceptions_InnerExceptionIsNull() {
    var aggregate = new AggregateException();
    Assert.That(aggregate.InnerException, Is.Null);
  }

  #endregion

  #region Flatten

  [Test]
  [Category("HappyPath")]
  public void AggregateException_Flatten_FlattensNestedAggregates() {
    var innermost = new InvalidOperationException("Innermost");
    var nested = new AggregateException(innermost);
    var outer = new AggregateException(nested);

    var flattened = outer.Flatten();

    Assert.That(flattened.InnerExceptions.Count, Is.EqualTo(1));
    Assert.That(flattened.InnerExceptions[0], Is.SameAs(innermost));
  }

  [Test]
  [Category("HappyPath")]
  public void AggregateException_Flatten_PreservesNonAggregateExceptions() {
    var ex1 = new InvalidOperationException("Ex1");
    var ex2 = new ArgumentException("Ex2");
    var aggregate = new AggregateException(ex1, ex2);

    var flattened = aggregate.Flatten();

    Assert.That(flattened.InnerExceptions.Count, Is.EqualTo(2));
    Assert.That(flattened.InnerExceptions, Contains.Item(ex1));
    Assert.That(flattened.InnerExceptions, Contains.Item(ex2));
  }

  [Test]
  [Category("HappyPath")]
  public void AggregateException_Flatten_MultipleNestedLevels() {
    var ex1 = new InvalidOperationException("Ex1");
    var ex2 = new ArgumentException("Ex2");
    var ex3 = new NotSupportedException("Ex3");

    var level2 = new AggregateException(ex2, ex3);
    var level1 = new AggregateException(ex1, level2);

    var flattened = level1.Flatten();

    Assert.That(flattened.InnerExceptions.Count, Is.EqualTo(3));
    Assert.That(flattened.InnerExceptions, Contains.Item(ex1));
    Assert.That(flattened.InnerExceptions, Contains.Item(ex2));
    Assert.That(flattened.InnerExceptions, Contains.Item(ex3));
  }

  [Test]
  [Category("EdgeCase")]
  public void AggregateException_Flatten_EmptyAggregate_ReturnsEmptyAggregate() {
    var aggregate = new AggregateException();
    var flattened = aggregate.Flatten();
    Assert.That(flattened.InnerExceptions.Count, Is.EqualTo(0));
  }

  #endregion

  #region Handle

  [Test]
  [Category("HappyPath")]
  public void AggregateException_Handle_AllHandled_DoesNotThrow() {
    var ex1 = new InvalidOperationException("Ex1");
    var ex2 = new InvalidOperationException("Ex2");
    var aggregate = new AggregateException(ex1, ex2);

    Assert.DoesNotThrow(() => aggregate.Handle(e => e is InvalidOperationException));
  }

  [Test]
  [Category("HappyPath")]
  public void AggregateException_Handle_SomeHandled_ThrowsWithUnhandled() {
    var ex1 = new InvalidOperationException("Ex1");
    var ex2 = new ArgumentException("Ex2");
    var aggregate = new AggregateException(ex1, ex2);

    var thrown = Assert.Throws<AggregateException>(() =>
      aggregate.Handle(e => e is InvalidOperationException)
    );

    Assert.That(thrown.InnerExceptions.Count, Is.EqualTo(1));
    Assert.That(thrown.InnerExceptions[0], Is.SameAs(ex2));
  }

  [Test]
  [Category("HappyPath")]
  public void AggregateException_Handle_NoneHandled_ThrowsAll() {
    var ex1 = new InvalidOperationException("Ex1");
    var ex2 = new ArgumentException("Ex2");
    var aggregate = new AggregateException(ex1, ex2);

    var thrown = Assert.Throws<AggregateException>(() =>
      aggregate.Handle(e => false)
    );

    Assert.That(thrown.InnerExceptions.Count, Is.EqualTo(2));
  }

  [Test]
  [Category("Exception")]
  public void AggregateException_Handle_NullPredicate_ThrowsArgumentNullException() {
    var aggregate = new AggregateException(new InvalidOperationException());
    Assert.Throws<ArgumentNullException>(() => aggregate.Handle(null));
  }

  #endregion

  #region ToString

  [Test]
  [Category("HappyPath")]
  public void AggregateException_ToString_ContainsInnerExceptions() {
    var inner = new InvalidOperationException("Inner message");
    var aggregate = new AggregateException("Outer message", inner);

    var result = aggregate.ToString();

    Assert.That(result, Does.Contain("Outer message"));
    Assert.That(result, Does.Contain("Inner message"));
    Assert.That(result, Does.Contain("InvalidOperationException"));
  }

  [Test]
  [Category("HappyPath")]
  public void AggregateException_ToString_MultipleInnerExceptions_ContainsAll() {
    var ex1 = new InvalidOperationException("First");
    var ex2 = new ArgumentException("Second");
    var aggregate = new AggregateException(ex1, ex2);

    var result = aggregate.ToString();

    Assert.That(result, Does.Contain("#1")); // Second exception should have #1 index
    Assert.That(result, Does.Contain("First"));
    Assert.That(result, Does.Contain("Second"));
  }

  #endregion

  #region Exception Behavior

  [Test]
  [Category("Exception")]
  public void AggregateException_WithNullInCollection_ThrowsArgumentException() {
    Exception[] exceptions = { new InvalidOperationException(), null };
    Assert.Throws<ArgumentException>(() => new AggregateException(exceptions));
  }

  [Test]
  [Category("Exception")]
  public void AggregateException_NullEnumerable_ThrowsArgumentNullException() {
    IEnumerable<Exception> nullExceptions = null;
    Assert.Throws<ArgumentNullException>(() => new AggregateException(nullExceptions));
  }

  #endregion

}
