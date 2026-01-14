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
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Task")]
public class TaskTests {

  #region Task Creation and Execution

  [Test]
  [Category("HappyPath")]
  public void Task_RunsAction() {
    var executed = false;
    var task = new Task(() => executed = true);
    task.Start();
    task.Wait();
    Assert.That(executed, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Task_WithState_RunsActionWithState() {
    var result = 0;
    var task = new Task(state => result = (int)state, 42);
    task.Start();
    task.Wait();
    Assert.That(result, Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void TaskOfT_ReturnsResult() {
    var task = new Task<int>(() => 42);
    task.Start();
    var result = task.Result;
    Assert.That(result, Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void TaskOfT_WithState_ReturnsResult() {
    var task = new Task<int>(state => (int)state * 2, 21);
    task.Start();
    var result = task.Result;
    Assert.That(result, Is.EqualTo(42));
  }

  #endregion

  #region Task Status

  [Test]
  [Category("HappyPath")]
  public void Task_Status_Created_BeforeStart() {
    var task = new Task(() => { });
    Assert.That(task.Status, Is.EqualTo(TaskStatus.Created));
  }

  [Test]
  [Category("HappyPath")]
  public void Task_Status_RanToCompletion_AfterWait() {
    var task = new Task(() => { });
    task.Start();
    task.Wait();
    Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
  }

  [Test]
  [Category("HappyPath")]
  public void Task_IsCompleted_True_AfterCompletion() {
    var task = new Task(() => { });
    task.Start();
    task.Wait();
    Assert.That(task.IsCompleted, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Task_IsCompleted_And_NotFaulted_AfterSuccess() {
    var task = new Task(() => { });
    task.Start();
    task.Wait();
    Assert.That(task.IsCompleted, Is.True);
    Assert.That(task.IsFaulted, Is.False);
  }

  #endregion

  #region Task.Factory.StartNew

  [Test]
  [Category("HappyPath")]
  public void TaskFactory_StartNew_RunsImmediately() {
    var executed = false;
    var task = Task.Factory.StartNew(() => executed = true);
    task.Wait();
    Assert.That(executed, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void TaskFactory_StartNew_WithResult_ReturnsValue() {
    var task = Task.Factory.StartNew(() => 42);
    var result = task.Result;
    Assert.That(result, Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void TaskFactory_StartNew_WithState_PassesState() {
    var task = Task.Factory.StartNew(state => (int)state * 2, 21);
    var result = task.Result;
    Assert.That(result, Is.EqualTo(42));
  }

  #endregion

  #region Task.Wait

  [Test]
  [Category("HappyPath")]
  public void Task_Wait_BlocksUntilCompletion() {
    var completed = false;
    var task = new Task(() => {
      Thread.Sleep(50);
      completed = true;
    });
    task.Start();
    task.Wait();
    Assert.That(completed, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Task_Wait_WithTimeout_ReturnsTrue_WhenCompleted() {
    var task = new Task(() => { });
    task.Start();
    var result = task.Wait(1000);
    Assert.That(result, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void Task_Wait_WithTimeout_ReturnsFalse_WhenTimedOut() {
    var task = new Task(() => Thread.Sleep(5000));
    task.Start();
    var result = task.Wait(10);
    Assert.That(result, Is.False);
  }

  #endregion

  #region ContinueWith

  [Test]
  [Category("HappyPath")]
  public void Task_ContinueWith_ExecutesAfterAntecedent() {
    var sequence = new int[2];
    var index = 0;

    var task = new Task(() => sequence[index++] = 1);
    var continuation = task.ContinueWith(_ => sequence[index++] = 2);

    task.Start();
    continuation.Wait();

    Assert.That(sequence, Is.EqualTo(new[] { 1, 2 }));
  }

  [Test]
  [Category("HappyPath")]
  public void TaskOfT_ContinueWith_ReceivesResult() {
    var task = Task.Factory.StartNew(() => 21);
    var continuation = task.ContinueWith(t => t.Result * 2);
    var result = continuation.Result;
    Assert.That(result, Is.EqualTo(42));
  }

  #endregion

  #region Task.Delay

  [Test]
  [Category("HappyPath")]
  public void Task_Delay_CompletesAfterSpecifiedTime() {
    var sw = System.Diagnostics.Stopwatch.StartNew();
    var task = Task.Delay(100);
    task.Wait();
    sw.Stop();
    Assert.That(sw.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(90)); // Allow some tolerance
  }

  [Test]
  [Category("HappyPath")]
  public void Task_Delay_WithTimeSpan_CompletesAfterSpecifiedTime() {
    var sw = System.Diagnostics.Stopwatch.StartNew();
    var task = Task.Delay(TimeSpan.FromMilliseconds(100));
    task.Wait();
    sw.Stop();
    Assert.That(sw.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(90));
  }

  [Test]
  [Category("EdgeCase")]
  public void Task_Delay_ZeroMilliseconds_CompletesImmediately() {
    var task = Task.Delay(0);
    task.Wait();
    Assert.That(task.IsCompleted, Is.True);
  }

  #endregion

  #region Task.FromResult

  [Test]
  [Category("HappyPath")]
  public void Task_FromResult_ReturnsCompletedTask() {
    var task = Task.FromResult(42);
    Assert.That(task.IsCompleted, Is.True);
    Assert.That(task.Result, Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void Task_FromResult_WithNull_ReturnsCompletedTask() {
    var task = Task.FromResult<string>(null);
    Assert.That(task.IsCompleted, Is.True);
    Assert.That(task.Result, Is.Null);
  }

  #endregion

  #region Task.Run

  [Test]
  [Category("HappyPath")]
  public void Task_Run_ExecutesAction() {
    var executed = false;
    var task = Task.Run(() => executed = true);
    task.Wait();
    Assert.That(executed, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Task_Run_WithFunc_ReturnsResult() {
    var task = Task.Run(() => 42);
    var result = task.Result;
    Assert.That(result, Is.EqualTo(42));
  }

  #endregion

  #region Task.WhenAll

  [Test]
  [Category("HappyPath")]
  public void Task_WhenAll_WaitsForAllTasks() {
    var count = 0;
    var lockObj = new object();

    var tasks = new Task[3];
    for (var i = 0; i < 3; ++i)
      tasks[i] = Task.Run(() => {
        Thread.Sleep(10);
        lock (lockObj)
          ++count;
      });

    Task.WhenAll(tasks).Wait();
    Assert.That(count, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void Task_WhenAll_WithResults_ReturnsAllResults() {
    var tasks = new[] {
      Task.FromResult(1),
      Task.FromResult(2),
      Task.FromResult(3)
    };

    var whenAll = Task.WhenAll(tasks);
    whenAll.Wait();
    var results = whenAll.Result;

    Assert.That(results, Is.EqualTo(new[] { 1, 2, 3 }));
  }

  #endregion

  #region Task.WhenAny

  [Test]
  [Category("HappyPath")]
  public void Task_WhenAny_CompletesWhenFirstTaskCompletes() {
    var tasks = new[] {
      Task.Delay(1000),
      Task.Delay(10),
      Task.Delay(1000)
    };

    var whenAny = Task.WhenAny(tasks);
    whenAny.Wait();

    Assert.That(whenAny.Result.IsCompleted, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Task_WhenAny_WithResults_ReturnsFirstCompletedTask() {
    var task1 = Task.Run(() => {
      Thread.Sleep(1000);
      return 1;
    });
    var task2 = Task.FromResult(2);

    var whenAny = Task.WhenAny((Task)task1, (Task)task2);
    whenAny.Wait();

    Assert.That(whenAny.Result.IsCompleted, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Task_WhenAnyT_ReturnsTypedTask() {
    var task1 = Task.FromResult(1);
    var task2 = Task.FromResult(2);

    Task<Task<int>> whenAny = Task.WhenAny<int>(task1, task2);
    whenAny.Wait();

    Assert.That(whenAny.Result, Is.InstanceOf<Task<int>>());
    Assert.That(whenAny.Result.Result, Is.EqualTo(1).Or.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Task_WhenAnyT_ReturnsFirstCompleted() {
    var tcs1 = new TaskCompletionSource<int>();
    var tcs2 = new TaskCompletionSource<int>();

    var whenAny = Task.WhenAny<int>(tcs1.Task, tcs2.Task);

    tcs2.SetResult(42);
    whenAny.Wait();

    Assert.That(whenAny.Result.Result, Is.EqualTo(42));
    Assert.That(whenAny.Result, Is.SameAs(tcs2.Task));
  }

  [Test]
  [Category("HappyPath")]
  public void Task_WhenAnyT_WithArray_ReturnsFirstCompleted() {
    var tasks = new[] {
      Task.Run(() => {
        Thread.Sleep(100);
        return "slow";
      }),
      Task.FromResult("fast")
    };

    var whenAny = Task.WhenAny(tasks);
    whenAny.Wait();

    Assert.That(whenAny.Result.Result, Is.EqualTo("fast"));
  }

  [Test]
  [Category("Exception")]
  public void Task_WhenAnyT_WithNull_ThrowsArgumentNullException() {
    Assert.Throws<ArgumentNullException>(() => Task.WhenAny((Task<int>[])null!));
  }

  [Test]
  [Category("Exception")]
  public void Task_WhenAnyT_WithEmpty_ThrowsArgumentException() {
    Assert.Throws<ArgumentException>(() => Task.WhenAny(Array.Empty<Task<int>>()));
  }

  #endregion

  #region Task.WhenEach

  // Helper method to synchronously enumerate IAsyncEnumerable (for NUnit compatibility on older frameworks)
  private static System.Collections.Generic.List<T> EnumerateAsync<T>(global::System.Collections.Generic.IAsyncEnumerable<T> source) {
    var results = new System.Collections.Generic.List<T>();
    var enumerator = source.GetAsyncEnumerator();
    try {
      while (enumerator.MoveNextAsync().AsTask().GetAwaiter().GetResult())
        results.Add(enumerator.Current);
    } finally {
      enumerator.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
    return results;
  }

  [Test]
  [Category("HappyPath")]
  public void Task_WhenEach_YieldsTasksAsTheyComplete() {
    var tcs1 = new TaskCompletionSource<int>();
    var tcs2 = new TaskCompletionSource<int>();
    var tcs3 = new TaskCompletionSource<int>();

    var tasks = new[] { tcs1.Task, tcs2.Task, tcs3.Task };

    // Complete in order: 2, 3, 1
    tcs2.SetResult(2);
    tcs3.SetResult(3);
    tcs1.SetResult(1);

    var completedTasks = EnumerateAsync(Task.WhenEach(tasks));
    var completionOrder = new System.Collections.Generic.List<int>();
    foreach (var task in completedTasks)
      completionOrder.Add(task.Result);

    Assert.That(completionOrder, Has.Count.EqualTo(3));
    Assert.That(completionOrder, Is.EquivalentTo(new[] { 1, 2, 3 }));
  }

  [Test]
  [Category("HappyPath")]
  public void Task_WhenEach_NonGeneric_YieldsTasksAsTheyComplete() {
    var tcs1 = new TaskCompletionSource<object>();
    var tcs2 = new TaskCompletionSource<object>();

    var tasks = new Task[] { tcs1.Task, tcs2.Task };

    tcs2.SetResult(null);
    tcs1.SetResult(null);

    var completedTasks = EnumerateAsync(Task.WhenEach(tasks));
    var completedCount = 0;

    foreach (var task in completedTasks) {
      Assert.That(task.IsCompleted, Is.True);
      ++completedCount;
    }

    Assert.That(completedCount, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Task_WhenEach_WithImmediatelyCompletedTasks_YieldsAll() {
    var tasks = new[] {
      Task.FromResult(1),
      Task.FromResult(2),
      Task.FromResult(3)
    };

    var completedTasks = EnumerateAsync(Task.WhenEach(tasks));
    var results = new System.Collections.Generic.List<int>();
    foreach (var task in completedTasks)
      results.Add(task.Result);

    Assert.That(results, Has.Count.EqualTo(3));
    Assert.That(results, Is.EquivalentTo(new[] { 1, 2, 3 }));
  }

  [Test]
  [Category("HappyPath")]
  public void Task_WhenEach_WithEmptyArray_YieldsNothing() {
    var tasks = Array.Empty<Task<int>>();

    var completedTasks = EnumerateAsync(Task.WhenEach(tasks));

    Assert.That(completedTasks, Has.Count.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Task_WhenEach_YieldsAllTasks() {
    // Create tasks that complete immediately with different values
    var tasks = new[] {
      Task.FromResult(10),
      Task.FromResult(20),
      Task.FromResult(30),
      Task.FromResult(40),
      Task.FromResult(50)
    };

    var completedTasks = EnumerateAsync(Task.WhenEach(tasks));
    var results = new System.Collections.Generic.List<int>();
    foreach (var task in completedTasks)
      results.Add(task.Result);

    Assert.That(results, Has.Count.EqualTo(5));
    // Verify all results are present (order is not guaranteed for pre-completed tasks)
    Assert.That(results, Does.Contain(10));
    Assert.That(results, Does.Contain(20));
    Assert.That(results, Does.Contain(30));
    Assert.That(results, Does.Contain(40));
    Assert.That(results, Does.Contain(50));
  }

  [Test]
  [Category("HappyPath")]
  public void Task_WhenEach_WithSingleTask_YieldsThatTask() {
    var task = Task.FromResult(42);

    var completedTasks = EnumerateAsync(Task.WhenEach<int>(task));
    var results = new System.Collections.Generic.List<int>();
    foreach (var completed in completedTasks)
      results.Add(completed.Result);

    Assert.That(results, Has.Count.EqualTo(1));
    Assert.That(results[0], Is.EqualTo(42));
  }

  [Test]
  [Category("Exception")]
  public void Task_WhenEach_WithNull_ThrowsArgumentNullException() {
    Assert.Throws<ArgumentNullException>(() => Task.WhenEach((Task<int>[])null!));
  }

  [Test]
  [Category("Exception")]
  public void Task_WhenEach_NonGeneric_WithNull_ThrowsArgumentNullException() {
    Assert.Throws<ArgumentNullException>(() => Task.WhenEach((Task[])null!));
  }

  [Test]
  [Category("HappyPath")]
  public void Task_WhenEach_WithFaultedTask_YieldsFaultedTask() {
    var tcs1 = new TaskCompletionSource<int>();
    var tcs2 = new TaskCompletionSource<int>();

    tcs1.SetException(new InvalidOperationException("Test error"));
    tcs2.SetResult(2);

    var tasks = new[] { tcs1.Task, tcs2.Task };
    var faultedCount = 0;
    var succeededCount = 0;

    var completedTasks = EnumerateAsync(Task.WhenEach(tasks));
    foreach (var task in completedTasks) {
      if (task.IsFaulted)
        ++faultedCount;
      else
        ++succeededCount;
    }

    Assert.That(faultedCount, Is.EqualTo(1));
    Assert.That(succeededCount, Is.EqualTo(1));
  }

  #endregion

  #region Exception Handling

  [Test]
  [Category("Exception")]
  public void Task_Exception_PropagatesOnWait() {
    var task = Task.Run(() => throw new InvalidOperationException("Test exception"));

    var ex = Assert.Throws<AggregateException>(() => task.Wait());
    Assert.That(ex.InnerExceptions[0], Is.TypeOf<InvalidOperationException>());
  }

  [Test]
  [Category("Exception")]
  public void Task_IsFaulted_True_WhenExceptionOccurs() {
    var task = Task.Run(() => throw new InvalidOperationException());
    try { task.Wait(); } catch { }
    Assert.That(task.IsFaulted, Is.True);
  }

  [Test]
  [Category("Exception")]
  public void TaskOfT_Result_ThrowsAggregateException_WhenFaulted() {
    Func<int> func = () => throw new InvalidOperationException();
    var task = Task.Run(func);
    Assert.Throws<AggregateException>(() => _ = task.Result);
  }

  #endregion

  #region TaskCompletionSource

  [Test]
  [Category("HappyPath")]
  public void TaskCompletionSource_SetResult_CompletesTask() {
    var tcs = new TaskCompletionSource<int>();
    tcs.SetResult(42);
    Assert.That(tcs.Task.IsCompleted, Is.True);
    Assert.That(tcs.Task.Result, Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void TaskCompletionSource_TrySetResult_ReturnsTrue_FirstTime() {
    var tcs = new TaskCompletionSource<int>();
    var result = tcs.TrySetResult(42);
    Assert.That(result, Is.True);
    Assert.That(tcs.Task.Result, Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void TaskCompletionSource_TrySetResult_ReturnsFalse_SecondTime() {
    var tcs = new TaskCompletionSource<int>();
    tcs.TrySetResult(42);
    var result = tcs.TrySetResult(100);
    Assert.That(result, Is.False);
    Assert.That(tcs.Task.Result, Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void TaskCompletionSource_SetCanceled_CancelsTask() {
    var tcs = new TaskCompletionSource<int>();
    tcs.SetCanceled();
    Assert.That(tcs.Task.IsCanceled, Is.True);
  }

  [Test]
  [Category("Exception")]
  public void TaskCompletionSource_SetException_FaultsTask() {
    var tcs = new TaskCompletionSource<int>();
    tcs.SetException(new InvalidOperationException());
    Assert.That(tcs.Task.IsFaulted, Is.True);
  }

  [Test]
  [Category("Exception")]
  public void TaskCompletionSource_SetResult_Twice_ThrowsInvalidOperationException() {
    var tcs = new TaskCompletionSource<int>();
    tcs.SetResult(42);
    Assert.Throws<InvalidOperationException>(() => tcs.SetResult(100));
  }

  #endregion

  #region CancellationToken

  [Test]
  [Category("HappyPath")]
  public void CancellationTokenSource_Cancel_SetsCancellationRequested() {
    var cts = new CancellationTokenSource();
    Assert.That(cts.Token.IsCancellationRequested, Is.False);
    cts.Cancel();
    Assert.That(cts.Token.IsCancellationRequested, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void CancellationToken_None_IsNeverCanceled() {
    var token = CancellationToken.None;
    Assert.That(token.IsCancellationRequested, Is.False);
    Assert.That(token.CanBeCanceled, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void CancellationToken_Register_CallsCallbackOnCancel() {
    var cts = new CancellationTokenSource();
    var callbackCalled = false;

    cts.Token.Register(() => callbackCalled = true);
    cts.Cancel();

    Assert.That(callbackCalled, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void CancellationTokenSource_CancelAfter_CancelsAfterDelay() {
    var cts = new CancellationTokenSource();
    cts.CancelAfter(100);

    // Use generous timeout for slower runtimes like .NET 3.5
    Thread.Sleep(500);
    Assert.That(cts.Token.IsCancellationRequested, Is.True);
  }

  [Test]
  [Category("Exception")]
  public void CancellationToken_ThrowIfCancellationRequested_Throws_WhenCanceled() {
    var cts = new CancellationTokenSource();
    cts.Cancel();
    Assert.Throws<OperationCanceledException>(() => cts.Token.ThrowIfCancellationRequested());
  }

  #endregion

  #region Task.FromCanceled

  [Test]
  [Category("HappyPath")]
  public void Task_FromCanceled_ReturnsCompletedTask() {
    using var cts = new CancellationTokenSource();
    cts.Cancel();
    var task = Task.FromCanceled(cts.Token);
    Assert.That(task.IsCompleted, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Task_FromCanceled_ReturnsCanceledTask() {
    using var cts = new CancellationTokenSource();
    cts.Cancel();
    var task = Task.FromCanceled(cts.Token);
    Assert.That(task.IsCanceled, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Task_FromCanceled_IsNotFaulted() {
    using var cts = new CancellationTokenSource();
    cts.Cancel();
    var task = Task.FromCanceled(cts.Token);
    Assert.That(task.IsFaulted, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Task_FromCanceledT_ReturnsCompletedTask() {
    using var cts = new CancellationTokenSource();
    cts.Cancel();
    var task = Task.FromCanceled<int>(cts.Token);
    Assert.That(task.IsCompleted, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Task_FromCanceledT_ReturnsCanceledTask() {
    using var cts = new CancellationTokenSource();
    cts.Cancel();
    var task = Task.FromCanceled<int>(cts.Token);
    Assert.That(task.IsCanceled, Is.True);
  }

  [Test]
  [Category("Exception")]
  public void Task_FromCanceledT_ThrowsOnResult() {
    using var cts = new CancellationTokenSource();
    cts.Cancel();
    var task = Task.FromCanceled<int>(cts.Token);
    Assert.Throws<AggregateException>(() => _ = task.Result);
  }

  [Test]
  [Category("Exception")]
  public void Task_FromCanceled_ThrowsOnWait() {
    using var cts = new CancellationTokenSource();
    cts.Cancel();
    var task = Task.FromCanceled(cts.Token);
    var ex = Assert.Throws<AggregateException>(() => task.Wait());
    Assert.That(ex.InnerException, Is.TypeOf<TaskCanceledException>());
  }

  #endregion

}
