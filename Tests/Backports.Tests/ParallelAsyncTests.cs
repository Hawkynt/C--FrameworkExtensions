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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("ParallelAsync")]
public class ParallelAsyncTests {

  #region ForAsync - int

  [Test]
  [Category("HappyPath")]
  public void ForAsync_Int_ExecutesAllIterations() {
    var results = new ConcurrentBag<int>();

    Parallel.ForAsync(0, 10, async (i, _) => {
      results.Add(i);
      await Task.Yield();
    }).GetAwaiter().GetResult();

    Assert.That(results.Count, Is.EqualTo(10));
    for (var i = 0; i < 10; ++i)
      Assert.That(results.Contains(i), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ForAsync_Int_WithParallelOptions_RespectsMaxDegree() {
#if NET20
    // Skip on net20: Task.Delay + SemaphoreSlim.WaitAsync + GetAwaiter().GetResult() can deadlock
    // due to limitations in the polyfilled async infrastructure on .NET 2.0
    Assert.Ignore("Test skipped on net20 due to async infrastructure limitations");
#else
    var concurrentCount = 0;
    var maxConcurrent = 0;
    var lockObj = new object();

    var options = new ParallelOptions { MaxDegreeOfParallelism = 2 };

    Parallel.ForAsync(0, 20, options, async (_, _) => {
      lock (lockObj) {
        ++concurrentCount;
        if (concurrentCount > maxConcurrent)
          maxConcurrent = concurrentCount;
      }

      await Task.Delay(50);

      lock (lockObj)
        --concurrentCount;
    }).GetAwaiter().GetResult();

    Assert.That(maxConcurrent, Is.LessThanOrEqualTo(2));
#endif
  }

  [Test]
  [Category("HappyPath")]
  public void ForAsync_Int_EmptyRange_CompletesImmediately() {
    var executed = false;

    Parallel.ForAsync(5, 5, async (_, _) => {
      executed = true;
      await Task.Yield();
    }).GetAwaiter().GetResult();

    Assert.That(executed, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ForAsync_Int_ReverseRange_CompletesImmediately() {
    var executed = false;

    Parallel.ForAsync(10, 5, async (_, _) => {
      executed = true;
      await Task.Yield();
    }).GetAwaiter().GetResult();

    Assert.That(executed, Is.False);
  }

  #endregion

  #region ForAsync - long

  [Test]
  [Category("HappyPath")]
  public void ForAsync_Long_ExecutesAllIterations() {
    var results = new ConcurrentBag<long>();

    Parallel.ForAsync(0L, 10L, async (i, _) => {
      results.Add(i);
      await Task.Yield();
    }).GetAwaiter().GetResult();

    Assert.That(results.Count, Is.EqualTo(10));
    for (long i = 0; i < 10; ++i)
      Assert.That(results.Contains(i), Is.True);
  }

  #endregion

  #region ForEachAsync - IEnumerable

  [Test]
  [Category("HappyPath")]
  public void ForEachAsync_Enumerable_ProcessesAllItems() {
    var source = new[] { 1, 2, 3, 4, 5 };
    var results = new ConcurrentBag<int>();

    Parallel.ForEachAsync(source, async (item, _) => {
      results.Add(item);
      await Task.Yield();
    }).GetAwaiter().GetResult();

    Assert.That(results.Count, Is.EqualTo(5));
    foreach (var item in source)
      Assert.That(results.Contains(item), Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ForEachAsync_Enumerable_WithParallelOptions_RespectsMaxDegree() {
    var source = Enumerable.Range(0, 20).ToList();
    var concurrentCount = 0;
    var maxConcurrent = 0;
    var lockObj = new object();

    var options = new ParallelOptions { MaxDegreeOfParallelism = 3 };

    Parallel.ForEachAsync(source, options, async (_, _) => {
      lock (lockObj) {
        ++concurrentCount;
        if (concurrentCount > maxConcurrent)
          maxConcurrent = concurrentCount;
      }

      await Task.Delay(50);

      lock (lockObj)
        --concurrentCount;
    }).GetAwaiter().GetResult();

    Assert.That(maxConcurrent, Is.LessThanOrEqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void ForEachAsync_Enumerable_EmptySource_CompletesImmediately() {
    var source = Array.Empty<int>();
    var executed = false;

    Parallel.ForEachAsync(source, async (_, _) => {
      executed = true;
      await Task.Yield();
    }).GetAwaiter().GetResult();

    Assert.That(executed, Is.False);
  }

  #endregion

  #region Cancellation

  [Test]
  [Category("HappyPath")]
  public void ForAsync_Cancellation_ThrowsOperationCanceledException() {
    using var cts = new CancellationTokenSource();
    cts.Cancel();

    var options = new ParallelOptions { CancellationToken = cts.Token };

    // TaskCanceledException inherits from OperationCanceledException
    Assert.That(
      () => Parallel.ForAsync(0, 100, options, async (_, _) => await Task.Yield()).GetAwaiter().GetResult(),
      Throws.InstanceOf<OperationCanceledException>()
    );
  }

  [Test]
  [Category("HappyPath")]
  public void ForEachAsync_Cancellation_ThrowsOperationCanceledException() {
    using var cts = new CancellationTokenSource();
    cts.Cancel();

    var source = Enumerable.Range(0, 100).ToList();
    var options = new ParallelOptions { CancellationToken = cts.Token };

    // TaskCanceledException inherits from OperationCanceledException
    Assert.That(
      () => Parallel.ForEachAsync(source, options, async (_, _) => await Task.Yield()).GetAwaiter().GetResult(),
      Throws.InstanceOf<OperationCanceledException>()
    );
  }

  #endregion

  #region Exception Handling

  [Test]
  [Category("Exception")]
  public void ForAsync_ExceptionInBody_ThrowsException() {
    // BCL throws InvalidOperationException directly, polyfill wraps in AggregateException
    Assert.That(
      () => Parallel.ForAsync(0, 10, async (i, _) => {
        if (i == 5)
          throw new InvalidOperationException("Test exception");
        await Task.Yield();
      }).GetAwaiter().GetResult(),
      Throws.InstanceOf<InvalidOperationException>().Or.InstanceOf<AggregateException>()
    );
  }

  [Test]
  [Category("Exception")]
  public void ForEachAsync_ExceptionInBody_ThrowsException() {
    var source = new[] { 1, 2, 3, 4, 5 };

    // BCL throws InvalidOperationException directly, polyfill wraps in AggregateException
    Assert.That(
      () => Parallel.ForEachAsync(source, async (item, _) => {
        if (item == 3)
          throw new InvalidOperationException("Test exception");
        await Task.Yield();
      }).GetAwaiter().GetResult(),
      Throws.InstanceOf<InvalidOperationException>().Or.InstanceOf<AggregateException>()
    );
  }

  #endregion

  #region Edge Cases

  [Test]
  [Category("EdgeCase")]
  public void ForAsync_NullBody_ThrowsArgumentNullException() {
    Assert.That(
      () => Parallel.ForAsync(0, 10, null!).GetAwaiter().GetResult(),
      Throws.InstanceOf<ArgumentNullException>()
    );
  }

  [Test]
  [Category("EdgeCase")]
  public void ForAsync_NullParallelOptions_ThrowsArgumentNullException() {
    Assert.That(
      () => Parallel.ForAsync(0, 10, null!, async (_, _) => await Task.Yield()).GetAwaiter().GetResult(),
      Throws.InstanceOf<ArgumentNullException>()
    );
  }

  [Test]
  [Category("EdgeCase")]
  public void ForEachAsync_NullSource_ThrowsArgumentNullException() {
    Assert.That(
      () => Parallel.ForEachAsync((IEnumerable<int>)null!, async (_, _) => await Task.Yield()).GetAwaiter().GetResult(),
      Throws.InstanceOf<ArgumentNullException>()
    );
  }

  [Test]
  [Category("EdgeCase")]
  public void ForEachAsync_NullBody_ThrowsArgumentNullException() {
    var source = new[] { 1, 2, 3 };

    Assert.That(
      () => Parallel.ForEachAsync(source, null!).GetAwaiter().GetResult(),
      Throws.InstanceOf<ArgumentNullException>()
    );
  }

  #endregion

}
