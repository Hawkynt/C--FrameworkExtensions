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

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Threading;

// Wave 1: Full PeriodicTimer implementation for frameworks before .NET 6.0
#if !SUPPORTS_PERIODIC_TIMER_WAVE1

/// <summary>
/// Provides a periodic timer that enables waiting asynchronously for timer ticks.
/// </summary>
/// <remarks>
/// <para>
/// This timer is intended for use only by a single consumer at a time: only one call to <see cref="WaitForNextTickAsync"/>
/// may be in flight at any given moment. <see cref="Dispose"/> may be used concurrently with an active
/// <see cref="WaitForNextTickAsync"/> to interrupt it and cause it to return <see langword="false"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
/// while (await timer.WaitForNextTickAsync()) {
///   Console.WriteLine("Tick!");
/// }
/// </code>
/// </example>
public sealed class PeriodicTimer : IDisposable {

  private TimeSpan _period;
  private readonly object _lock = new();
  private bool _disposed;
  private TaskCompletionSource<bool>? _waitingTcs;
  private CancellationTokenRegistration _ctr;
  private Timer? _timer;

  /// <summary>
  /// Initializes the timer with the specified period.
  /// </summary>
  /// <param name="period">
  /// The time interval between invocations of callback methods.
  /// The value must be greater than <see cref="TimeSpan.Zero"/>.
  /// </param>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="period"/> is less than or equal to <see cref="TimeSpan.Zero"/>.
  /// </exception>
  public PeriodicTimer(TimeSpan period) {
    if (period <= TimeSpan.Zero)
      throw new ArgumentOutOfRangeException(nameof(period), "The period must be greater than zero.");

    this._period = period;
  }

  /// <summary>
  /// Gets or sets the period of the timer.
  /// </summary>
  /// <value>
  /// The time interval between invocations of callback methods.
  /// </value>
  /// <exception cref="ArgumentOutOfRangeException">
  /// The value being set is less than or equal to <see cref="TimeSpan.Zero"/>.
  /// </exception>
  /// <exception cref="ObjectDisposedException">
  /// The timer has been disposed.
  /// </exception>
  public TimeSpan Period {
    get => this._period;
    set {
      if (value <= TimeSpan.Zero)
        throw new ArgumentOutOfRangeException(nameof(value), "The period must be greater than zero.");

      lock (this._lock) {
        if (this._disposed)
          throw new ObjectDisposedException(nameof(PeriodicTimer));

        this._period = value;
        this._timer?.Change(value, value);
      }
    }
  }

  /// <summary>
  /// Waits for the next tick of the timer.
  /// </summary>
  /// <param name="cancellationToken">
  /// A <see cref="CancellationToken"/> to observe while waiting for a tick.
  /// </param>
  /// <returns>
  /// A <see cref="ValueTask{TResult}"/> that will be completed due to the timer firing, the timer being disposed,
  /// or the cancellation token being canceled.
  /// </returns>
  /// <remarks>
  /// <para>
  /// This method returns <see langword="true"/> when the timer fires, and <see langword="false"/> when the timer has been disposed.
  /// </para>
  /// <para>
  /// This method may only be called by one consumer at a time; it is not safe to call this method concurrently from multiple consumers.
  /// </para>
  /// </remarks>
  /// <exception cref="OperationCanceledException">
  /// The <paramref name="cancellationToken"/> was canceled.
  /// </exception>
  public ValueTask<bool> WaitForNextTickAsync(CancellationToken cancellationToken = default) {
    lock (this._lock) {
      if (this._disposed)
        return new(false);

      if (cancellationToken.IsCancellationRequested)
        return new(Task.FromCanceled<bool>(cancellationToken));

      var tcs = new TaskCompletionSource<bool>((TaskCreationOptions)64); // RunContinuationsAsynchronously
      this._waitingTcs = tcs;

      if (cancellationToken.CanBeCanceled)
        this._ctr = cancellationToken.Register(
          static state => {
            var s = (PeriodicTimer)state!;
            lock (s._lock) {
              s._waitingTcs?.TrySetCanceled();
              s._waitingTcs = null;
            }
          },
          this
        );

      this._timer ??= new(
        static state => ((PeriodicTimer)state!)._OnTimerCallback(),
        this,
        this._period,
        this._period
      );

      return new(tcs.Task);
    }
  }

  private void _OnTimerCallback() {
    lock (this._lock) {
      if (this._disposed)
        return;

      var tcs = this._waitingTcs;
      if (tcs == null)
        return;

      this._waitingTcs = null;
      this._ctr.Dispose();
      this._ctr = default;
      tcs.TrySetResult(true);
    }
  }

  /// <summary>
  /// Stops the timer and releases all resources used by the current instance of <see cref="PeriodicTimer"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <see cref="Dispose"/> will cause an in-flight <see cref="WaitForNextTickAsync"/> call to return <see langword="false"/>.
  /// All other resources used by the <see cref="PeriodicTimer"/> will be released.
  /// </para>
  /// </remarks>
  public void Dispose() {
    Timer? timer;
    TaskCompletionSource<bool>? tcs;

    lock (this._lock) {
      if (this._disposed)
        return;

      this._disposed = true;
      timer = this._timer;
      this._timer = null;
      tcs = this._waitingTcs;
      this._waitingTcs = null;
      this._ctr.Dispose();
      this._ctr = default;
    }

    timer?.Dispose();
    tcs?.TrySetResult(false);
  }

}

#endif

// Wave 2: Period property polyfill for .NET 6.0/7.0
// The BCL's PeriodicTimer exists but lacks the Period property (added in .NET 8.0)
// We use reflection to access the internal timer and change its period
#if !SUPPORTS_PERIODIC_TIMER_WAVE2

public static partial class PeriodicTimerPeriodPolyfill {

  private const BindingFlags _FLAGS = BindingFlags.NonPublic | BindingFlags.Instance;

  // Lazy-initialized callbacks for reflection operations
  private static Func<PeriodicTimer, object?>? _getTimerCallback;
  private static Func<PeriodicTimer, TimeSpan?>? _getPeriodCallback;
  private static Func<PeriodicTimer, bool>? _isDisposedCallback;
  private static Action<object, TimeSpan>? _invokeChangeCallback;
  private static Action<PeriodicTimer, TimeSpan>? _setPeriodCallback;

  private static readonly ConditionalWeakTable<PeriodicTimer, TimeSpanBox> _periods = new();

  #region Reflection Helpers

  private static FieldInfo? _FindFieldByType(Type declaringType, Type fieldType)
    => Array.Find(declaringType.GetFields(_FLAGS), f => f.FieldType == fieldType || fieldType.IsAssignableFrom(f.FieldType));

  private static FieldInfo? _FindFieldByTypeName(Type declaringType, string typeName)
    => Array.Find(declaringType.GetFields(_FLAGS), f => f.FieldType.Name == typeName || f.FieldType.FullName?.Contains(typeName) == true);

  private static FieldInfo? _FindBoolFieldByPattern(Type declaringType, string pattern)
    => Array.Find(declaringType.GetFields(_FLAGS), f => f.FieldType == typeof(bool) && f.Name.Contains(pattern, StringComparison.OrdinalIgnoreCase));

  #endregion

  #region Lazy Reflection Initializers

  private static Func<PeriodicTimer, object?> _ReflectGetTimerCallback() {
    var timerType = typeof(PeriodicTimer);
    var field = timerType.GetField("_timer", _FLAGS)
                ?? timerType.GetField("timer", _FLAGS)
                ?? timerType.GetField("m_timer", _FLAGS)
                ?? _FindFieldByType(timerType, typeof(Timer))
                ?? _FindFieldByTypeName(timerType, "ITimer");

    return field == null
      ? static _ => null
      : timer => { try { return field.GetValue(timer); } catch { return null; } };
  }

  private static Func<PeriodicTimer, TimeSpan?> _ReflectGetPeriodCallback() {
    var timerType = typeof(PeriodicTimer);
    var field = timerType.GetField("_period", _FLAGS)
                ?? timerType.GetField("period", _FLAGS)
                ?? timerType.GetField("m_period", _FLAGS)
                ?? _FindFieldByType(timerType, typeof(TimeSpan));

    return field == null
      ? static _ => null
      : timer => { try { return field.GetValue(timer) as TimeSpan?; } catch { return null; } };
  }

  private static Action<PeriodicTimer, TimeSpan> _ReflectSetPeriodCallback() {
    var timerType = typeof(PeriodicTimer);
    var field = timerType.GetField("_period", _FLAGS)
                ?? timerType.GetField("period", _FLAGS)
                ?? timerType.GetField("m_period", _FLAGS)
                ?? _FindFieldByType(timerType, typeof(TimeSpan));

    return field == null
      ? static (_, _) => { }
      : (timer, value) => { try { field.SetValue(timer, value); } catch { /* ignore */ } };
  }

  private static Func<PeriodicTimer, bool> _ReflectIsDisposedCallback() {
    var timerType = typeof(PeriodicTimer);

    // Try to find the state field which contains _stopped (BCL pattern: nested State class)
    var stateField = timerType.GetField("_state", _FLAGS)
                     ?? timerType.GetField("state", _FLAGS)
                     ?? timerType.GetField("m_state", _FLAGS);

    if (stateField != null) {
      var stateType = stateField.FieldType;
      var stoppedField = stateType.GetField("_stopped", _FLAGS)
                         ?? stateType.GetField("stopped", _FLAGS)
                         ?? stateType.GetField("_disposed", _FLAGS)
                         ?? stateType.GetField("disposed", _FLAGS)
                         ?? _FindBoolFieldByPattern(stateType, "stop")
                         ?? _FindBoolFieldByPattern(stateType, "dispose");

      if (stoppedField != null)
        return timer => {
          try {
            var stateObj = stateField.GetValue(timer);
            return stateObj != null && stoppedField.GetValue(stateObj) is bool isStopped && isStopped;
          } catch {
            return false;
          }
        };
    }

    // Fallback: check if the timer field is null (common disposal pattern)
    var timerField = timerType.GetField("_timer", _FLAGS)
                     ?? timerType.GetField("timer", _FLAGS)
                     ?? timerType.GetField("m_timer", _FLAGS)
                     ?? _FindFieldByType(timerType, typeof(Timer))
                     ?? _FindFieldByTypeName(timerType, "ITimer");

    if (timerField != null)
      return timer => {
        try {
          return timerField.GetValue(timer) == null;
        } catch {
          return false;
        }
      };

    return static _ => false;
  }

  private static Action<object, TimeSpan> _ReflectInvokeChangeCallback() {
    var timerType = typeof(PeriodicTimer);
    var timerField = timerType.GetField("_timer", _FLAGS)
                     ?? timerType.GetField("timer", _FLAGS)
                     ?? timerType.GetField("m_timer", _FLAGS)
                     ?? _FindFieldByType(timerType, typeof(Timer))
                     ?? _FindFieldByTypeName(timerType, "ITimer");

    if (timerField != null) {
      var fieldType = timerField.FieldType;
      var changeMethod = fieldType.GetMethod("Change", [typeof(TimeSpan), typeof(TimeSpan)]);
      if (changeMethod != null)
        return (timerObj, period) => {
          try {
            changeMethod.Invoke(timerObj, [period, period]);
          } catch { /* ignore */ }
        };

      changeMethod = fieldType.GetMethod("Change", [typeof(int), typeof(int)]);
      if (changeMethod != null)
        return (timerObj, period) => {
          try {
            changeMethod.Invoke(timerObj, [(int)period.TotalMilliseconds, (int)period.TotalMilliseconds]);
          } catch { /* ignore */ }
        };

      // Fallback: if field type is Timer, use direct cast
      if (typeof(Timer).IsAssignableFrom(fieldType))
        return (timerObj, period) => ((Timer)timerObj).Change(period, period);
    }

    // Last resort: runtime type check and method lookup (shouldn't normally happen)
    return (timerObj, period) => {
      if (timerObj is Timer timer) {
        timer.Change(period, period);
        return;
      }

      var objType = timerObj.GetType();
      var runtimeChangeMethod = objType.GetMethod("Change", [typeof(TimeSpan), typeof(TimeSpan)]);
      if (runtimeChangeMethod != null) {
        try {
          runtimeChangeMethod.Invoke(timerObj, [period, period]);
        } catch { /* ignore */ }
        return;
      }

      runtimeChangeMethod = objType.GetMethod("Change", [typeof(int), typeof(int)]);
      if (runtimeChangeMethod != null) {
        try {
          runtimeChangeMethod.Invoke(timerObj, [(int)period.TotalMilliseconds, (int)period.TotalMilliseconds]);
        } catch { /* ignore */ }
      }
    };
  }

  #endregion

  private static void _ThrowIfDisposed(PeriodicTimer timer) {
    if ((_isDisposedCallback ??= _ReflectIsDisposedCallback())(timer))
      throw new ObjectDisposedException(nameof(PeriodicTimer));
  }

  private sealed class TimeSpanBox(TimeSpan value) {
    public TimeSpan Value { get; set; } = value;
  }

  extension(PeriodicTimer @this) {

    /// <summary>
    /// Gets or sets the period of the timer.
    /// </summary>
    /// <value>
    /// The time interval between invocations of callback methods.
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The value being set is less than or equal to <see cref="TimeSpan.Zero"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The period cannot be retrieved because it was not set after construction.
    /// </exception>
    /// <remarks>
    /// <para>
    /// <b>POLYFILL NOTE:</b> On .NET 6.0 and 7.0, this polyfill uses reflection to access
    /// the internal timer and change its period. Getting the period returns the value
    /// either from the internal BCL field (if accessible) or from our tracking table
    /// (if the period was set via this property).
    /// </para>
    /// </remarks>
    public TimeSpan Period {
      get {
        _ThrowIfDisposed(@this);

        // First, try to get the period from the BCL's internal field via reflection
        var period = (_getPeriodCallback ??= _ReflectGetPeriodCallback())(@this);
        if (period.HasValue)
          return period.Value;

        // Fall back to our tracked values
        if (_periods.TryGetValue(@this, out var box))
          return box.Value;

        throw new InvalidOperationException(
          "The period cannot be retrieved. On .NET 6.0/7.0, the PeriodicTimer.Period property is a polyfill " +
          "that tracks periods set via this property. To get the initial period, set Period after construction."
        );
      }
      set {
        _ThrowIfDisposed(@this);

        if (value <= TimeSpan.Zero)
          throw new ArgumentOutOfRangeException(nameof(value), "The period must be greater than zero.");

        // Try to actually change the timer period via reflection
        var timerObj = (_getTimerCallback ??= _ReflectGetTimerCallback())(@this);
        if (timerObj != null)
          (_invokeChangeCallback ??= _ReflectInvokeChangeCallback())(timerObj, value);

        // Store the value in our tracking table (for retrieval and in case timer change failed)
        if (_periods.TryGetValue(@this, out var existingBox))
          existingBox.Value = value;
        else
          _periods.Add(@this, new TimeSpanBox(value));

        // Also try to set the internal _period field if it exists
        (_setPeriodCallback ??= _ReflectSetPeriodCallback())(@this, value);
      }
    }

  }

}

#endif
