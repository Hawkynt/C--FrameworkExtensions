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
using System.Threading;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("ThreadLocal")]
public class ThreadLocalTests {

  #region Constructor

  [Test]
  [Category("HappyPath")]
  public void ThreadLocal_DefaultConstructor_CreatesInstance() {
    using var threadLocal = new ThreadLocal<int>();
    Assert.That(threadLocal.IsValueCreated, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ThreadLocal_WithFactory_CreatesInstance() {
    using var threadLocal = new ThreadLocal<int>(() => 42);
    Assert.That(threadLocal.IsValueCreated, Is.False);
  }

  #endregion

  #region Value Property

  [Test]
  [Category("HappyPath")]
  public void ThreadLocal_Value_ReturnsDefaultWhenNoFactory() {
    using var threadLocal = new ThreadLocal<int>();
    var value = threadLocal.Value;
    Assert.That(value, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void ThreadLocal_Value_InvokesFactory() {
    using var threadLocal = new ThreadLocal<int>(() => 42);
    var value = threadLocal.Value;
    Assert.That(value, Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void ThreadLocal_Value_CachesResult() {
    var callCount = 0;
    using var threadLocal = new ThreadLocal<int>(() => {
      ++callCount;
      return 42;
    });

    _ = threadLocal.Value;
    _ = threadLocal.Value;
    _ = threadLocal.Value;

    Assert.That(callCount, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void ThreadLocal_Value_SetAndGet() {
    using var threadLocal = new ThreadLocal<int>();
    threadLocal.Value = 100;
    Assert.That(threadLocal.Value, Is.EqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void ThreadLocal_Value_OverwritesPreviousValue() {
    using var threadLocal = new ThreadLocal<int>(() => 42);
    _ = threadLocal.Value;
    threadLocal.Value = 100;
    Assert.That(threadLocal.Value, Is.EqualTo(100));
  }

  #endregion

  #region IsValueCreated Property

  [Test]
  [Category("HappyPath")]
  public void ThreadLocal_IsValueCreated_FalseBeforeAccess() {
    using var threadLocal = new ThreadLocal<int>(() => 42);
    Assert.That(threadLocal.IsValueCreated, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ThreadLocal_IsValueCreated_TrueAfterGet() {
    using var threadLocal = new ThreadLocal<int>(() => 42);
    _ = threadLocal.Value;
    Assert.That(threadLocal.IsValueCreated, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ThreadLocal_IsValueCreated_TrueAfterSet() {
    using var threadLocal = new ThreadLocal<int>();
    threadLocal.Value = 100;
    Assert.That(threadLocal.IsValueCreated, Is.True);
  }

  #endregion

  #region Thread Isolation

  [Test]
  [Category("Integration")]
  public void ThreadLocal_Value_IsolatedPerThread() {
    using var threadLocal = new ThreadLocal<int>();
    var results = new Dictionary<int, int>();
    var threads = new Thread[5];

    for (var i = 0; i < threads.Length; ++i) {
      var threadIndex = i;
      threads[i] = new Thread(() => {
        threadLocal.Value = threadIndex * 10;
        Thread.Sleep(10);
        lock (results)
          results[Thread.CurrentThread.ManagedThreadId] = threadLocal.Value;
      });
    }

    foreach (var thread in threads)
      thread.Start();
    foreach (var thread in threads)
      thread.Join();

    Assert.That(results.Count, Is.EqualTo(5));
    var expectedValues = new HashSet<int> { 0, 10, 20, 30, 40 };
    foreach (var value in results.Values)
      Assert.That(expectedValues.Contains(value), Is.True);
  }

  [Test]
  [Category("Integration")]
  public void ThreadLocal_Value_FactoryCalledPerThread() {
    var callCount = 0;
    using var threadLocal = new ThreadLocal<int>(() => {
      Interlocked.Increment(ref callCount);
      return Thread.CurrentThread.ManagedThreadId;
    });

    var threads = new Thread[3];
    for (var i = 0; i < threads.Length; ++i) {
      threads[i] = new Thread(() => _ = threadLocal.Value);
      threads[i].Start();
    }

    foreach (var thread in threads)
      thread.Join();

    _ = threadLocal.Value;

    Assert.That(callCount, Is.EqualTo(4));
  }

  #endregion

  #region Dispose

  [Test]
  [Category("HappyPath")]
  public void ThreadLocal_Dispose_CanBeCalledMultipleTimes() {
    var threadLocal = new ThreadLocal<int>(() => 42);
    threadLocal.Dispose();
    Assert.DoesNotThrow(() => threadLocal.Dispose());
  }

  [Test]
  [Category("Exception")]
  public void ThreadLocal_Value_ThrowsAfterDispose() {
    var threadLocal = new ThreadLocal<int>(() => 42);
    threadLocal.Dispose();
    Assert.Throws<ObjectDisposedException>(() => _ = threadLocal.Value);
  }

  [Test]
  [Category("Exception")]
  public void ThreadLocal_SetValue_ThrowsAfterDispose() {
    var threadLocal = new ThreadLocal<int>();
    threadLocal.Dispose();
    Assert.Throws<ObjectDisposedException>(() => threadLocal.Value = 42);
  }

  [Test]
  [Category("Exception")]
  public void ThreadLocal_IsValueCreated_ThrowsAfterDispose() {
    var threadLocal = new ThreadLocal<int>();
    threadLocal.Dispose();
    Assert.Throws<ObjectDisposedException>(() => _ = threadLocal.IsValueCreated);
  }

  #endregion

  #region ToString

  [Test]
  [Category("HappyPath")]
  public void ThreadLocal_ToString_ReturnsValueString() {
    using var threadLocal = new ThreadLocal<int>(() => 42);
    _ = threadLocal.Value;
    var str = threadLocal.ToString();
    Assert.That(str, Is.EqualTo("42"));
  }

  [Test]
  [Category("HappyPath")]
  public void ThreadLocal_ToString_WithReferenceType() {
    using var threadLocal = new ThreadLocal<string>(() => "Hello");
    _ = threadLocal.Value;
    var str = threadLocal.ToString();
    Assert.That(str, Is.EqualTo("Hello"));
  }

  #endregion

  #region Reference Types

  [Test]
  [Category("HappyPath")]
  public void ThreadLocal_ReferenceType_ReturnsCorrectInstance() {
    using var threadLocal = new ThreadLocal<TestClass>(() => new TestClass { Value = 100 });
    Assert.That(threadLocal.Value.Value, Is.EqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void ThreadLocal_ReferenceType_ReturnsSameInstance() {
    using var threadLocal = new ThreadLocal<TestClass>(() => new TestClass());
    var first = threadLocal.Value;
    var second = threadLocal.Value;
    Assert.That(first, Is.SameAs(second));
  }

  [Test]
  [Category("HappyPath")]
  public void ThreadLocal_ReferenceType_DifferentInstancesPerThread() {
    using var threadLocal = new ThreadLocal<TestClass>(() => new TestClass());
    TestClass mainValue = null;
    TestClass threadValue = null;

    mainValue = threadLocal.Value;

    var thread = new Thread(() => threadValue = threadLocal.Value);
    thread.Start();
    thread.Join();

    Assert.That(mainValue, Is.Not.SameAs(threadValue));
  }

  #endregion

  #region Edge Cases

  [Test]
  [Category("EdgeCase")]
  public void ThreadLocal_NullableValue_CanBeNull() {
    using var threadLocal = new ThreadLocal<string>(() => null);
    var value = threadLocal.Value;
    Assert.That(value, Is.Null);
    Assert.That(threadLocal.IsValueCreated, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void ThreadLocal_CanSetToNull() {
    using var threadLocal = new ThreadLocal<string>(() => "initial");
    _ = threadLocal.Value;
    threadLocal.Value = null;
    Assert.That(threadLocal.Value, Is.Null);
  }

  #endregion

  #region Helper Types

  private class TestClass {
    public int Value { get; set; }
  }

  #endregion

  // ThreadLocal(bool trackAllValues) constructor and Values property were added in .NET 4.5
  // Our polyfill provides them for net35 and earlier; BCL provides them for net45+
  // On net40, BCL ThreadLocal exists but lacks these features and we can't polyfill them
#if !SUPPORTS_THREADLOCAL || SUPPORTS_THREADLOCAL_WAVE1

  #region Values Property and TrackAllValues Constructor (net45+)

  [Test]
  [Category("HappyPath")]
  public void ThreadLocal_WithTrackAllValues_CreatesInstance() {
    using var threadLocal = new ThreadLocal<int>(true);
    Assert.That(threadLocal.IsValueCreated, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ThreadLocal_WithFactoryAndTrackAllValues_CreatesInstance() {
    using var threadLocal = new ThreadLocal<int>(() => 42, true);
    Assert.That(threadLocal.IsValueCreated, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ThreadLocal_Values_ReturnsAllTrackedValues() {
    using var threadLocal = new ThreadLocal<int>(() => Thread.CurrentThread.ManagedThreadId, true);

    var threads = new Thread[3];
    for (var i = 0; i < threads.Length; ++i) {
      threads[i] = new Thread(() => _ = threadLocal.Value);
      threads[i].Start();
    }

    foreach (var thread in threads)
      thread.Join();

    _ = threadLocal.Value;

    var values = threadLocal.Values;
    Assert.That(values.Count, Is.EqualTo(4));
  }

  [Test]
  [Category("Exception")]
  public void ThreadLocal_Values_ThrowsWhenTrackingDisabled() {
    using var threadLocal = new ThreadLocal<int>(() => 42, false);
    Assert.Throws<InvalidOperationException>(() => _ = threadLocal.Values);
  }

  [Test]
  [Category("HappyPath")]
  public void ThreadLocal_Values_EmptyWhenNoValuesSet() {
    using var threadLocal = new ThreadLocal<int>(true);
    var values = threadLocal.Values;
    Assert.That(values.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("Exception")]
  public void ThreadLocal_Values_ThrowsAfterDispose() {
    var threadLocal = new ThreadLocal<int>(true);
    threadLocal.Dispose();
    Assert.Throws<ObjectDisposedException>(() => _ = threadLocal.Values);
  }

  [Test]
  [Category("EdgeCase")]
  public void ThreadLocal_Values_UpdatesWhenValuesChange() {
    using var threadLocal = new ThreadLocal<int>(true);
    threadLocal.Value = 10;

    var values1 = threadLocal.Values;
    Assert.That(values1, Contains.Item(10));

    threadLocal.Value = 20;
    var values2 = threadLocal.Values;
    Assert.That(values2, Contains.Item(20));
  }

  #endregion

#endif

}
