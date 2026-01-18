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
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Parallel")]
public class ParallelTests {

  #region Parallel.For Basic

  [Test]
  [Category("HappyPath")]
  public void Parallel_For_ExecutesAllIterations() {
    var results = new int[10];
    Parallel.For(0, 10, i => results[i] = i * 2);

    for (var i = 0; i < 10; ++i)
      Assert.That(results[i], Is.EqualTo(i * 2));
  }

  [Test]
  [Category("HappyPath")]
  public void Parallel_For_ReturnsCompletedResult() {
    var result = Parallel.For(0, 10, i => { });
    Assert.That(result.IsCompleted, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void Parallel_For_EmptyRange_DoesNothing() {
    var executed = false;
    var result = Parallel.For(0, 0, i => executed = true);
    Assert.That(executed, Is.False);
    Assert.That(result.IsCompleted, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void Parallel_For_NegativeRange_DoesNothing() {
    var executed = false;
    var result = Parallel.For(10, 0, i => executed = true);
    Assert.That(executed, Is.False);
    Assert.That(result.IsCompleted, Is.True);
  }

  #endregion

  #region Parallel.For with Long

  [Test]
  [Category("HappyPath")]
  public void Parallel_For_Long_ExecutesAllIterations() {
    var count = 0;
    var lockObj = new object();

    Parallel.For(0L, 100L, i => {
      lock (lockObj)
        ++count;
    });

    Assert.That(count, Is.EqualTo(100));
  }

  #endregion

  #region Parallel.For with ParallelLoopState

  [Test]
  [Category("HappyPath")]
  public void Parallel_For_WithState_ReceivesState() {
    var stateReceived = false;
    Parallel.For(0, 1, (i, state) => stateReceived = state != null);
    Assert.That(stateReceived, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Parallel_For_Stop_StopsExecution() {
    var count = 0;
    var lockObj = new object();

    var result = Parallel.For(0, 1000, (i, state) => {
      if (i == 50)
        state.Stop();

      lock (lockObj)
        ++count;
    });

    Assert.That(result.IsCompleted, Is.False);
    Assert.That(count, Is.LessThan(1000)); // Not all iterations should complete
  }

  [Test]
  [Category("HappyPath")]
  public void Parallel_For_Break_SetsLowestBreakIteration() {
    var result = Parallel.For(0, 1000, (i, state) => {
      if (i == 50)
        state.Break();
    });

    Assert.That(result.IsCompleted, Is.False);
    Assert.That(result.LowestBreakIteration, Is.Not.Null);
  }

  #endregion

  #region Parallel.For with ParallelOptions

  [Test]
  [Category("HappyPath")]
  public void Parallel_For_WithOptions_RespectsMaxDegreeOfParallelism() {
    var maxConcurrent = 0;
    var currentConcurrent = 0;
    var lockObj = new object();

    var options = new ParallelOptions { MaxDegreeOfParallelism = 2 };

    Parallel.For(0, 100, options, i => {
      lock (lockObj) {
        ++currentConcurrent;
        if (currentConcurrent > maxConcurrent)
          maxConcurrent = currentConcurrent;
      }

      Thread.Sleep(10);

      lock (lockObj)
        --currentConcurrent;
    });

    // With batching, we might see more concurrent executions within batches
    // but the number of concurrent threads should be limited
    Assert.That(maxConcurrent, Is.GreaterThan(0));
  }

  [Test]
  [Category("Exception")]
  public void Parallel_For_WithCanceledToken_ThrowsOperationCanceledException() {
    var cts = new CancellationTokenSource();
    cts.Cancel();

    var options = new ParallelOptions { CancellationToken = cts.Token };

    Assert.Throws<OperationCanceledException>(() =>
      Parallel.For(0, 10, options, i => { })
    );
  }

  #endregion

  #region Parallel.ForEach Basic

  [Test]
  [Category("HappyPath")]
  public void Parallel_ForEach_ExecutesForAllItems() {
    var items = new List<int> { 1, 2, 3, 4, 5 };
    var results = new int[5];
    var lockObj = new object();

    Parallel.ForEach(items, item => {
      lock (lockObj)
        results[item - 1] = item * 2;
    });

    Assert.That(results, Is.EqualTo(new[] { 2, 4, 6, 8, 10 }));
  }

  [Test]
  [Category("HappyPath")]
  public void Parallel_ForEach_ReturnsCompletedResult() {
    var result = Parallel.ForEach(new[] { 1, 2, 3 }, item => { });
    Assert.That(result.IsCompleted, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void Parallel_ForEach_EmptyCollection_DoesNothing() {
    var executed = false;
    var result = Parallel.ForEach(new int[0], item => executed = true);
    Assert.That(executed, Is.False);
    Assert.That(result.IsCompleted, Is.True);
  }

  #endregion

  #region Parallel.ForEach with ParallelLoopState

  [Test]
  [Category("HappyPath")]
  public void Parallel_ForEach_WithState_ReceivesState() {
    var stateReceived = false;
    Parallel.ForEach(new[] { 1 }, (item, state) => stateReceived = state != null);
    Assert.That(stateReceived, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Parallel_ForEach_Stop_StopsExecution() {
    var count = 0;
    var lockObj = new object();
    var items = new List<int>();
    for (var i = 0; i < 1000; ++i)
      items.Add(i);

    var result = Parallel.ForEach(items, (item, state) => {
      if (item == 50)
        state.Stop();

      lock (lockObj)
        ++count;
    });

    Assert.That(result.IsCompleted, Is.False);
    Assert.That(count, Is.LessThan(1000));
  }

  #endregion

  #region Parallel.ForEach with Index

  [Test]
  [Category("HappyPath")]
  public void Parallel_ForEach_WithIndex_ProvidesCorrectIndex() {
    var items = new[] { "a", "b", "c" };
    var indices = new long[3];
    var lockObj = new object();

    Parallel.ForEach(items, (item, state, index) => {
      lock (lockObj)
        indices[(int)index] = index;
    });

    Assert.That(indices, Is.EqualTo(new long[] { 0, 1, 2 }));
  }

  #endregion

  #region Parallel.ForEach with ParallelOptions

  [Test]
  [Category("HappyPath")]
  public void Parallel_ForEach_WithOptions_ExecutesCorrectly() {
    var options = new ParallelOptions { MaxDegreeOfParallelism = 2 };
    var count = 0;
    var lockObj = new object();

    Parallel.ForEach(new[] { 1, 2, 3, 4, 5 }, options, item => {
      lock (lockObj)
        ++count;
    });

    Assert.That(count, Is.EqualTo(5));
  }

  #endregion

  #region Parallel.ForEach with ThreadLocal and Partitioner

  [Test]
  [Category("HappyPath")]
  public void Parallel_ForEach_WithPartitionerAndLocal_InitializesLocalState() {
    var items = new[] { 1, 2, 3, 4, 5 };
    var localInitCalled = 0;
    var localFinallyCalled = 0;

    Parallel.ForEach(
      Partitioner.Create(items),
      () => {
        Interlocked.Increment(ref localInitCalled);
        return 0;
      },
      (item, state, local) => local + item,
      local => Interlocked.Increment(ref localFinallyCalled)
    );

    Assert.That(localInitCalled, Is.GreaterThan(0));
    Assert.That(localFinallyCalled, Is.EqualTo(localInitCalled));
  }

  [Test]
  [Category("HappyPath")]
  public void Parallel_ForEach_WithPartitionerAndLocal_AccumulatesValues() {
    var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    var totalSum = 0;
    var lockObj = new object();

    Parallel.ForEach(
      Partitioner.Create(items),
      () => 0,
      (item, state, local) => local + item,
      local => {
        lock (lockObj)
          totalSum += local;
      }
    );

    Assert.That(totalSum, Is.EqualTo(55)); // Sum of 1..10
  }

  [Test]
  [Category("HappyPath")]
  public void Parallel_ForEach_WithPartitionerAndLocal_ReturnsCompletedResult() {
    var items = new[] { 1, 2, 3 };

    var result = Parallel.ForEach(
      Partitioner.Create(items),
      () => 0,
      (item, state, local) => local,
      local => { }
    );

    Assert.That(result.IsCompleted, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Parallel_ForEach_WithPartitionerAndLocal_ProvidesLoopState() {
    var items = new[] { 1, 2, 3 };
    var stateReceived = false;

    Parallel.ForEach(
      Partitioner.Create(items),
      () => 0,
      (item, state, local) => {
        stateReceived = state != null;
        return local;
      },
      local => { }
    );

    Assert.That(stateReceived, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Parallel_ForEach_WithPartitionerAndLocal_Stop_StopsExecution() {
    var items = new List<int>();
    for (var i = 0; i < 1000; ++i)
      items.Add(i);

    var count = 0;

    var result = Parallel.ForEach(
      Partitioner.Create(items),
      () => 0,
      (item, state, local) => {
        if (item == 50)
          state.Stop();
        Interlocked.Increment(ref count);
        return local;
      },
      local => { }
    );

    Assert.That(result.IsCompleted, Is.False);
    Assert.That(count, Is.LessThan(1000));
  }

  [Test]
  [Category("HappyPath")]
  public void Parallel_ForEach_WithPartitionerLocalAndOptions_RespectsMaxDegreeOfParallelism() {
    var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    var maxConcurrent = 0;
    var currentConcurrent = 0;
    var lockObj = new object();

    var options = new ParallelOptions { MaxDegreeOfParallelism = 2 };

    Parallel.ForEach(
      Partitioner.Create(items),
      options,
      () => 0,
      (item, state, local) => {
        lock (lockObj) {
          ++currentConcurrent;
          if (currentConcurrent > maxConcurrent)
            maxConcurrent = currentConcurrent;
        }

        Thread.Sleep(10);

        lock (lockObj)
          --currentConcurrent;

        return local;
      },
      local => { }
    );

    Assert.That(maxConcurrent, Is.GreaterThan(0));
  }

  [Test]
  [Category("EdgeCase")]
  public void Parallel_ForEach_WithPartitionerAndLocal_EmptySource_CallsNoBody() {
    var items = new int[0];
    var bodyCalled = false;
    var finallyCalled = 0;

    var result = Parallel.ForEach(
      Partitioner.Create(items),
      () => 0,
      (item, state, local) => {
        bodyCalled = true;
        return local;
      },
      local => Interlocked.Increment(ref finallyCalled)
    );

    Assert.That(bodyCalled, Is.False);
    Assert.That(result.IsCompleted, Is.True);
  }

  [Test]
  [Category("Exception")]
  public void Parallel_ForEach_WithPartitionerAndLocal_ExceptionInBody_ThrowsAggregateException() {
    var items = new[] { 1, 2, 3 };

    Assert.Throws<AggregateException>(() =>
      Parallel.ForEach(
        Partitioner.Create(items),
        () => 0,
        (item, state, local) => {
          if (item == 2)
            throw new InvalidOperationException("Test exception");
          return local;
        },
        local => { }
      )
    );
  }

  [Test]
  [Category("Exception")]
  public void Parallel_ForEach_WithPartitionerAndLocal_ExceptionInFinally_ThrowsAggregateException() {
    var items = new[] { 1, 2, 3 };

    Assert.Throws<AggregateException>(() =>
      Parallel.ForEach(
        Partitioner.Create(items),
        () => 0,
        (item, state, local) => local,
        local => throw new InvalidOperationException("Finally exception")
      )
    );
  }

  [Test]
  [Category("Exception")]
  public void Parallel_ForEach_WithPartitionerAndLocal_NullSource_ThrowsArgumentNullException() {
    Partitioner<int> nullSource = null;

    Assert.Throws<ArgumentNullException>(() =>
      Parallel.ForEach(
        nullSource,
        () => 0,
        (item, state, local) => local,
        local => { }
      )
    );
  }

  [Test]
  [Category("Exception")]
  public void Parallel_ForEach_WithPartitionerAndLocal_NullLocalInit_ThrowsArgumentNullException() {
    var items = new[] { 1, 2, 3 };
    Func<int> nullInit = null;

    Assert.Throws<ArgumentNullException>(() =>
      Parallel.ForEach(
        Partitioner.Create(items),
        nullInit,
        (item, state, local) => local,
        local => { }
      )
    );
  }

  [Test]
  [Category("Exception")]
  public void Parallel_ForEach_WithPartitionerAndLocal_NullBody_ThrowsArgumentNullException() {
    var items = new[] { 1, 2, 3 };
    Func<int, ParallelLoopState, int, int> nullBody = null;

    Assert.Throws<ArgumentNullException>(() =>
      Parallel.ForEach(
        Partitioner.Create(items),
        () => 0,
        nullBody,
        local => { }
      )
    );
  }

  [Test]
  [Category("Exception")]
  public void Parallel_ForEach_WithPartitionerAndLocal_NullLocalFinally_ThrowsArgumentNullException() {
    var items = new[] { 1, 2, 3 };
    Action<int> nullFinally = null;

    Assert.Throws<ArgumentNullException>(() =>
      Parallel.ForEach(
        Partitioner.Create(items),
        () => 0,
        (item, state, local) => local,
        nullFinally
      )
    );
  }

  [Test]
  [Category("Exception")]
  public void Parallel_ForEach_WithPartitionerLocalAndOptions_WithCanceledToken_ThrowsOperationCanceledException() {
    var items = new[] { 1, 2, 3 };
    var cts = new CancellationTokenSource();
    cts.Cancel();

    var options = new ParallelOptions { CancellationToken = cts.Token };

    Assert.Throws<OperationCanceledException>(() =>
      Parallel.ForEach(
        Partitioner.Create(items),
        options,
        () => 0,
        (item, state, local) => local,
        local => { }
      )
    );
  }

  [Test]
  [Category("HappyPath")]
  public void Parallel_ForEach_WithRangePartitioner_AccumulatesCorrectly() {
    var totalSum = 0;
    var lockObj = new object();

    Parallel.ForEach(
      Partitioner.Create(0, 101),
      () => 0,
      (range, state, local) => {
        var (from, to) = range;
        for (var i = from; i < to; ++i)
          local += i;
        return local;
      },
      local => {
        lock (lockObj)
          totalSum += local;
      }
    );

    Assert.That(totalSum, Is.EqualTo(5050)); // Sum of 0..100
  }

  #endregion

  #region Parallel.Invoke

  [Test]
  [Category("HappyPath")]
  public void Parallel_Invoke_ExecutesAllActions() {
    var results = new bool[3];

    Parallel.Invoke(
      () => results[0] = true,
      () => results[1] = true,
      () => results[2] = true
    );

    Assert.That(results, Is.EqualTo(new[] { true, true, true }));
  }

  [Test]
  [Category("HappyPath")]
  public void Parallel_Invoke_ExecutesConcurrently() {
    var timestamps = new long[3];
    var baseTime = DateTime.UtcNow.Ticks;

    Parallel.Invoke(
      () => { Thread.Sleep(50); timestamps[0] = DateTime.UtcNow.Ticks - baseTime; },
      () => { Thread.Sleep(50); timestamps[1] = DateTime.UtcNow.Ticks - baseTime; },
      () => { Thread.Sleep(50); timestamps[2] = DateTime.UtcNow.Ticks - baseTime; }
    );

    // All timestamps should be roughly similar if running in parallel
    // Sequential would take ~150ms, parallel ~50ms
    var maxDiff = Math.Max(
      Math.Abs(timestamps[0] - timestamps[1]),
      Math.Max(Math.Abs(timestamps[1] - timestamps[2]), Math.Abs(timestamps[0] - timestamps[2]))
    );

    // Difference should be small (< 100ms = 1_000_000 ticks)
    Assert.That(maxDiff, Is.LessThan(TimeSpan.FromMilliseconds(100).Ticks));
  }

  [Test]
  [Category("EdgeCase")]
  public void Parallel_Invoke_EmptyArray_DoesNothing() {
    Parallel.Invoke(new Action[0]); // Should not throw
  }

  [Test]
  [Category("Exception")]
  public void Parallel_Invoke_WithNullArray_ThrowsArgumentNullException() {
    Action[] nullActions = null;
    Assert.Throws<ArgumentNullException>(() => Parallel.Invoke(nullActions));
  }

  [Test]
  [Category("Exception")]
  public void Parallel_Invoke_WithNullAction_ThrowsArgumentException() {
    Assert.Throws<ArgumentException>(() =>
      Parallel.Invoke(() => { }, null, () => { })
    );
  }

  #endregion

  #region Parallel.Invoke with ParallelOptions

  [Test]
  [Category("HappyPath")]
  public void Parallel_Invoke_WithOptions_ExecutesAllActions() {
    var options = new ParallelOptions { MaxDegreeOfParallelism = 2 };
    var count = 0;
    var lockObj = new object();

    Parallel.Invoke(
      options,
      () => { lock (lockObj) ++count; },
      () => { lock (lockObj) ++count; },
      () => { lock (lockObj) ++count; }
    );

    Assert.That(count, Is.EqualTo(3));
  }

  [Test]
  [Category("Exception")]
  public void Parallel_Invoke_WithCanceledToken_ThrowsOperationCanceledException() {
    var cts = new CancellationTokenSource();
    cts.Cancel();

    var options = new ParallelOptions { CancellationToken = cts.Token };

    Assert.Throws<OperationCanceledException>(() =>
      Parallel.Invoke(options, () => { }, () => { })
    );
  }

  #endregion

  #region Exception Handling

  [Test]
  [Category("Exception")]
  public void Parallel_For_ExceptionInBody_ThrowsAggregateException() {
    Assert.Throws<AggregateException>(() =>
      Parallel.For(0, 10, i => {
        if (i == 5)
          throw new InvalidOperationException("Test exception");
      })
    );
  }

  [Test]
  [Category("Exception")]
  public void Parallel_ForEach_ExceptionInBody_ThrowsAggregateException() {
    Assert.Throws<AggregateException>(() =>
      Parallel.ForEach(new[] { 1, 2, 3 }, item => {
        if (item == 2)
          throw new InvalidOperationException("Test exception");
      })
    );
  }

  [Test]
  [Category("Exception")]
  public void Parallel_Invoke_ExceptionInAction_ThrowsAggregateException() {
    Assert.Throws<AggregateException>(() =>
      Parallel.Invoke(
        () => { },
        () => throw new InvalidOperationException("Test exception"),
        () => { }
      )
    );
  }

  [Test]
  [Category("Exception")]
  public void Parallel_For_NullBody_ThrowsArgumentNullException() {
    Assert.Throws<ArgumentNullException>(() =>
      Parallel.For(0, 10, (Action<int>)null)
    );
  }

  [Test]
  [Category("Exception")]
  public void Parallel_ForEach_NullSource_ThrowsArgumentNullException() {
    IEnumerable<int> nullSource = null;
    Assert.Throws<ArgumentNullException>(() =>
      Parallel.ForEach(nullSource, i => { })
    );
  }

  [Test]
  [Category("Exception")]
  public void Parallel_ForEach_NullBody_ThrowsArgumentNullException() {
    Action<int> nullAction = null;
    Assert.Throws<ArgumentNullException>(() =>
      Parallel.ForEach(new[] { 1, 2, 3 }, nullAction)
    );
  }

  #endregion

  #region ParallelOptions

  [Test]
  [Category("HappyPath")]
  public void ParallelOptions_DefaultValues_AreCorrect() {
    var options = new ParallelOptions();
    Assert.That(options.MaxDegreeOfParallelism, Is.EqualTo(-1));
    Assert.That(options.CancellationToken, Is.EqualTo(CancellationToken.None));
    Assert.That(options.TaskScheduler, Is.EqualTo(TaskScheduler.Default));
  }

  [Test]
  [Category("HappyPath")]
  public void ParallelOptions_CanSetProperties() {
    var cts = new CancellationTokenSource();
    var options = new ParallelOptions {
      MaxDegreeOfParallelism = 4,
      CancellationToken = cts.Token
    };

    Assert.That(options.MaxDegreeOfParallelism, Is.EqualTo(4));
    Assert.That(options.CancellationToken, Is.EqualTo(cts.Token));
  }

  #endregion

  #region ParallelLoopState

  [Test]
  [Category("HappyPath")]
  public void ParallelLoopState_IsStopped_False_Initially() {
    Parallel.For(0, 1, (i, state) =>
      Assert.That(state.IsStopped, Is.False)
    );
  }

  [Test]
  [Category("HappyPath")]
  public void ParallelLoopState_ShouldExitCurrentIteration_False_Initially() {
    Parallel.For(0, 1, (i, state) =>
      Assert.That(state.ShouldExitCurrentIteration, Is.False)
    );
  }

  [Test]
  [Category("HappyPath")]
  public void ParallelLoopState_Stop_SetsIsStopped() {
    var wasStopped = false;
    Parallel.For(0, 100, (i, state) => {
      if (i == 50) {
        state.Stop();
        wasStopped = state.IsStopped;
      }
    });
    Assert.That(wasStopped, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void ParallelLoopState_Break_SetsLowestBreakIteration() {
    var result = Parallel.For(0, 100, (i, state) => {
      if (i == 50)
        state.Break();
    });
    Assert.That(result.LowestBreakIteration, Is.Not.Null);
  }

  #endregion

}
