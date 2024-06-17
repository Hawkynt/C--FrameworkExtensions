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

#define SUPPORTTHREADTIMERS

using System.Collections.Concurrent;
using Guard;

namespace System.Threading.Tasks;

/// <summary>
///   Creates a scheduled task, that combines all calls within a given timespan and than executes only once with the last
///   known value.
/// </summary>
/// <typeparam name="TValue">The type of item to pass for execution.</typeparam>
public class ScheduledTask<TValue> {
  private TValue _currentValue;
  private readonly Action<TValue> _action;
  private readonly int _deferredTime;
  private readonly bool _waitUntilTaskReturnedBeforeNextSchedule;
  private readonly object _lock = new();
  private int _taskIsRunning;
  private int _dataAvailable;

#if SUPPORTTHREADTIMERS
  private readonly bool _allowThreadSleep = true;
  private readonly Timer _timer;
#endif

  /// <summary>
  ///   Initializes a new instance of the <see cref="ScheduledTask&lt;TValue&gt;" /> class.
  /// </summary>
  /// <param name="action">The task to execute.</param>
  /// <param name="deferredTime">The default time the task is deferred by.</param>
  /// <param name="waitUntilTaskReturnedBeforeNextSchedule">if set to <c>true</c> waits till executed before next schedule.</param>
  public ScheduledTask(
    Action<TValue> action,
    TimeSpan deferredTime,
    bool waitUntilTaskReturnedBeforeNextSchedule = false
  ) : this(
    action,
    (int)deferredTime.TotalMilliseconds,
    waitUntilTaskReturnedBeforeNextSchedule
  ) { }

  /// <summary>
  ///   Initializes a new instance of the <see cref="ScheduledTask&lt;TValue&gt;" /> class.
  /// </summary>
  /// <param name="action">The task to execute.</param>
  /// <param name="deferredTime">The default time in ms the task is deferred by.</param>
  /// <param name="waitUntilTaskReturnedBeforeNextSchedule">if set to <c>true</c> waits till executed before next schedule.</param>
  public ScheduledTask(Action<TValue> action, int deferredTime = 500, bool waitUntilTaskReturnedBeforeNextSchedule = false) {
    Against.ArgumentIsNull(action);

    this._action = action;
    this._deferredTime = deferredTime;
    this._waitUntilTaskReturnedBeforeNextSchedule = waitUntilTaskReturnedBeforeNextSchedule;
#if SUPPORTTHREADTIMERS
    if (deferredTime < 500)
      this._allowThreadSleep = true;
    else {
      this._allowThreadSleep = false;
      this._timer = new(this._OnTimeIsUp, null, Timeout.Infinite, Timeout.Infinite);
    }
#endif
  }

  /// <summary>
  ///   Schedule an execution with the specified value.
  /// </summary>
  /// <param name="value">The value.</param>
  public void Execute(TValue value) => this._Schedule(value, this._deferredTime);

  /// <summary>
  ///   Schedule an execution with the specified value.
  /// </summary>
  /// <param name="value">The value.</param>
  public void Restart(TValue value) => this._Schedule(value, this._deferredTime);

  /// <summary>
  ///   Schedule an execution with the specified value.
  /// </summary>
  /// <param name="value">The value.</param>
  public void Schedule(TValue value) => this._Schedule(value, this._deferredTime);

  /// <summary>
  ///   Executes the handler immediately with the given value.
  /// </summary>
  /// <param name="value">The value.</param>
  public void Now(TValue value) => this._action(value);

  /// <summary>
  ///   Schedule an execution with the specified value in at least this timespan.
  /// </summary>
  /// <param name="value">The value.</param>
  /// <param name="deferredBy">The time to defer by.</param>
  public void Schedule(TValue value, TimeSpan deferredBy) => this._Schedule(value, (int)deferredBy.TotalMilliseconds);

  /// <summary>
  ///   Schedule an execution with the specified value in at least this timespan.
  /// </summary>
  /// <param name="value">The value.</param>
  /// <param name="deferredBy">The time in ms to defer by.</param>
  public void Schedule(TValue value, int deferredBy) => this._Schedule(value, deferredBy);

  /// <summary>
  ///   Is called when the time is over.
  /// </summary>
  /// <param name="sleepTime">The sleep time.</param>
  private void _OnTimeIsUp(object sleepTime) {
    // as long as there is fresh data available, re-use the thread
    while (this._dataAvailable != 0) {
      // sleep if needed);
      if (sleepTime != null)
        Thread.Sleep((int)sleepTime);

      // refresh current value
      TValue currentValue;
      lock (this._lock) {
        currentValue = this._currentValue;

        // clear fresh data
        Interlocked.CompareExchange(ref this._dataAvailable, 0, 1);
      }


      // reset scheduler so more tasks can be scheduled
      if (!this._waitUntilTaskReturnedBeforeNextSchedule)
        Interlocked.CompareExchange(ref this._taskIsRunning, 0, 1);

      // execute task
      try {
        this._action(currentValue);
      } finally {
        // reset scheduler so more tasks can be scheduled
        if (this._waitUntilTaskReturnedBeforeNextSchedule)
          Interlocked.CompareExchange(ref this._taskIsRunning, 0, 1);
      }
    }
  }

  /// <summary>
  ///   Schedules the task to be run with this value.
  /// </summary>
  /// <param name="value">The value.</param>
  /// <param name="sleepTime">The sleep time to wait before the task is executed.</param>
  private void _Schedule(TValue value, int sleepTime) {
    lock (this._lock) {
      this._currentValue = value;

      // indicate fresh data
      Interlocked.CompareExchange(ref this._dataAvailable, 1, 0);
    }

    // return if its already running
    if (Interlocked.CompareExchange(ref this._taskIsRunning, 1, 0) != 0)
      return;

#if SUPPORTTHREADTIMERS
    if (this._allowThreadSleep) {
#endif
      var action = this._OnTimeIsUp;
      action.BeginInvoke(sleepTime, action.EndInvoke, null);
#if SUPPORTTHREADTIMERS
    } else
      this._timer.Change(sleepTime, Timeout.Infinite);
#endif
  }
}

/// <summary>
///   Creates a scheduled task, that combines all calls within a given timespan and than executes only once.
/// </summary>
public class ScheduledTask {
  private readonly Action _action;
  private readonly int _deferredTime;
  private readonly bool _waitUntilTaskReturnedBeforeNextSchedule;
  private int _taskIsRunning;

#if SUPPORTTHREADTIMERS
  private readonly bool _allowThreadSleep = true;
  private readonly Timer _timer;
#endif

  public ManualResetEventSlim WaitHandle { get; } = new(true);

  /// <summary>
  ///   Initializes a new instance of the <see cref="ScheduledTask" /> class.
  /// </summary>
  /// <param name="action">The task to execute.</param>
  /// <param name="deferredTime">The default time the task is deferred by.</param>
  /// <param name="waitUntilTaskReturnedBeforeNextSchedule">if set to <c>true</c> waits till executed before next schedule.</param>
  public ScheduledTask(
    Action action,
    TimeSpan deferredTime,
    bool waitUntilTaskReturnedBeforeNextSchedule = false
  ) : this(
    action,
    (int)deferredTime.TotalMilliseconds,
    waitUntilTaskReturnedBeforeNextSchedule
  ) { }

  /// <summary>
  ///   Initializes a new instance of the <see cref="ScheduledTask" /> class.
  /// </summary>
  /// <param name="action">The task to execute.</param>
  /// <param name="deferredTime">The default time in ms the task is deferred by.</param>
  /// <param name="waitUntilTaskReturnedBeforeNextSchedule">if set to <c>true</c> waits till executed before next schedule.</param>
  public ScheduledTask(Action action, int deferredTime = 500, bool waitUntilTaskReturnedBeforeNextSchedule = false) {
    Against.ArgumentIsNull(action);

    this._action = action;
    this._deferredTime = deferredTime;
    this._waitUntilTaskReturnedBeforeNextSchedule = waitUntilTaskReturnedBeforeNextSchedule;
#if SUPPORTTHREADTIMERS
    if (deferredTime < 500)
      this._allowThreadSleep = true;
    else {
      this._allowThreadSleep = false;
      this._timer = new(this._OnTimeIsUp, null, Timeout.Infinite, Timeout.Infinite);
    }
#endif
  }

  /// <summary>
  ///   Forces the execution now.
  /// </summary>
  public void ForceExecuteNow() => this._action();

  /// <summary>
  ///   Schedule an execution.
  /// </summary>
  public void Execute() => this._Schedule(this._deferredTime);

  /// <summary>
  ///   Schedule an execution.
  /// </summary>
  public void Restart() => this._Schedule(this._deferredTime);

  /// <summary>
  ///   Schedule an execution.
  /// </summary>
  public void Schedule() => this._Schedule(this._deferredTime);

  /// <summary>
  ///   Schedule an execution.
  /// </summary>
  /// <param name="deferredBy">The ms to defer by at least.</param>
  public void Schedule(int deferredBy) => this._Schedule(deferredBy);

  /// <summary>
  ///   Schedule an execution.
  /// </summary>
  /// <param name="deferredBy">The time to defer by at least.</param>
  public void Schedule(TimeSpan deferredBy) => this._Schedule((int)deferredBy.TotalMilliseconds);

  /// <summary>
  ///   Is called when the time is up.
  /// </summary>
  /// <param name="sleepTime">The sleep time.</param>
  private void _OnTimeIsUp(object sleepTime) {
    // sleep if needed
    if (sleepTime != null)
      Thread.Sleep((int)sleepTime);

    // reset scheduler so more tasks can be scheduled
    if (!this._waitUntilTaskReturnedBeforeNextSchedule)
      Interlocked.CompareExchange(ref this._taskIsRunning, 0, 1);
    // execute task
    try {
      this._action();
    } finally {
      // reset scheduler so more tasks can be scheduled
      if (this._waitUntilTaskReturnedBeforeNextSchedule)
        Interlocked.CompareExchange(ref this._taskIsRunning, 0, 1);

      this.WaitHandle.Set();
    }
  }

  /// <summary>
  ///   Schedules a new task.
  /// </summary>
  /// <param name="sleepTime">The sleep time.</param>
  private void _Schedule(int sleepTime) {
    if (Interlocked.CompareExchange(ref this._taskIsRunning, 1, 0) != 0)
      // already running
      return;

    this.WaitHandle.Reset();

#if SUPPORTTHREADTIMERS
    if (this._allowThreadSleep) {
#endif
      var action = this._OnTimeIsUp;
      action.BeginInvoke(sleepTime, action.EndInvoke, null);
#if SUPPORTTHREADTIMERS
    } else
      this._timer.Change(sleepTime, Timeout.Infinite);
#endif
  }
}

/// <summary>
///   Creates a scheduled task that collects all values between different calls until the time is up and then executes them
///   all.
///   Note: does not guarantee that the values are in the right order.
/// </summary>
/// <typeparam name="TValue">The type of the values.</typeparam>
public class ScheduledCombinedTask<TValue> {
  private readonly ConcurrentBag<TValue> _scheduledValues = new();
  private readonly Action<TValue[]> _action;
  private readonly int _deferredTime;
  private readonly bool _waitUntilTaskReturnedBeforeNextSchedule;
  private readonly object _lock = new();
  private int _taskIsRunning;
  private int _taskIsAborted;

#if SUPPORTTHREADTIMERS
  private readonly bool _allowThreadSleep = true;
  private readonly Timer _timer;
#endif

  /// <summary>
  ///   Initializes a new instance of the <see cref="ScheduledCombinedTask&lt;TValue&gt;" /> class.
  /// </summary>
  /// <param name="action">The task to execute.</param>
  /// <param name="deferredTime">The default time in ms the task is deferred by.</param>
  /// <param name="waitUntilTaskReturnedBeforeNextSchedule">if set to <c>true</c> waits till executed before next schedule.</param>
  public ScheduledCombinedTask(
    Action<TValue[]> action,
    TimeSpan deferredTime,
    bool waitUntilTaskReturnedBeforeNextSchedule = false
  ) : this(
    action,
    (int)deferredTime.TotalMilliseconds,
    waitUntilTaskReturnedBeforeNextSchedule
  ) { }

  /// <summary>
  ///   Initializes a new instance of the <see cref="ScheduledCombinedTask&lt;TValue&gt;" /> class.
  /// </summary>
  /// <param name="action">The task to execute.</param>
  /// <param name="deferredTime">The default time in ms the task is deferred by.</param>
  /// <param name="waitUntilTaskReturnedBeforeNextSchedule">if set to <c>true</c> waits till executed before next schedule.</param>
  public ScheduledCombinedTask(Action<TValue[]> action, int deferredTime = 500, bool waitUntilTaskReturnedBeforeNextSchedule = false) {
    Against.ArgumentIsNull(action);

    this._action = action;
    this._deferredTime = deferredTime;
    this._waitUntilTaskReturnedBeforeNextSchedule = waitUntilTaskReturnedBeforeNextSchedule;
#if SUPPORTTHREADTIMERS
    if (deferredTime < 500)
      this._allowThreadSleep = true;
    else {
      this._allowThreadSleep = false;
      this._timer = new(this._OnTimeIsUp, null, Timeout.Infinite, Timeout.Infinite);
    }
#endif
  }

  /// <summary>
  ///   Schedule an execution with the specified value.
  /// </summary>
  /// <param name="value">The value.</param>
  public void Execute(TValue value) => this._Schedule(value, this._deferredTime);

  /// <summary>
  ///   Schedule an execution with the specified value.
  /// </summary>
  /// <param name="value">The value.</param>
  public void Restart(TValue value) => this._Schedule(value, this._deferredTime);

  /// <summary>
  ///   Schedule an execution with the specified value.
  /// </summary>
  /// <param name="value">The value.</param>
  public void Schedule(TValue value) => this._Schedule(value, this._deferredTime);

  /// <summary>
  ///   Schedule an execution with the specified value in at least this timespan.
  /// </summary>
  /// <param name="value">The value.</param>
  /// <param name="deferredBy">The time to defer by.</param>
  public void Schedule(TValue value, TimeSpan deferredBy) => this._Schedule(value, (int)deferredBy.TotalMilliseconds);

  /// <summary>
  ///   Schedule an execution with the specified value in at least this timespan.
  /// </summary>
  /// <param name="value">The value.</param>
  /// <param name="deferredBy">The time in ms to defer by.</param>
  public void Schedule(TValue value, int deferredBy) => this._Schedule(value, deferredBy);

  /// <summary>
  ///   Aborts a running schedule if any.
  /// </summary>
  public void Abort() => Interlocked.CompareExchange(ref this._taskIsAborted, 1, 0);

  /// <summary>
  ///   Is called the the time is up.
  /// </summary>
  /// <param name="sleepTime">The sleep time.</param>
  private void _OnTimeIsUp(object sleepTime) {
    // sleep if needed););
    if (sleepTime != null)
      Thread.Sleep((int)sleepTime);

    // refresh current value
    TValue[] scheduledValues;
    lock (this._lock) {
      scheduledValues = this._scheduledValues.ToArray();

      // clear the bag
      while (this._scheduledValues.TryTake(out _)) { }
    }

    // reset scheduler so more task can be scheduled
    if (!this._waitUntilTaskReturnedBeforeNextSchedule)
      Interlocked.CompareExchange(ref this._taskIsRunning, 0, 1);
    // execute task
    try {
      if (Interlocked.CompareExchange(ref this._taskIsAborted, 1, 0) == 0)
        this._action(scheduledValues);
    } finally {
      // reset scheduler so more tasks can be scheduled
      if (this._waitUntilTaskReturnedBeforeNextSchedule)
        Interlocked.CompareExchange(ref this._taskIsRunning, 0, 1);
    }
  }

  /// <summary>
  ///   Schedules a value to be used.
  /// </summary>
  /// <param name="value">The value.</param>
  /// <param name="sleepTime">The sleep time.</param>
  private void _Schedule(TValue value, int sleepTime) {
    Interlocked.CompareExchange(ref this._taskIsAborted, 0, 1);

    lock (this._lock)
      this._scheduledValues.Add(value);

    if (Interlocked.CompareExchange(ref this._taskIsRunning, 1, 0) != 0)
      // already running
      return;
#if SUPPORTTHREADTIMERS
    if (this._allowThreadSleep) {
#endif
      var action = this._OnTimeIsUp;
      action.BeginInvoke(sleepTime, action.EndInvoke, null);
#if SUPPORTTHREADTIMERS
    } else
      this._timer.Change(sleepTime, Timeout.Infinite);
#endif
  }
}
