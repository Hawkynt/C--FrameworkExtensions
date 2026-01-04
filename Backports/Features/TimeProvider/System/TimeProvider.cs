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

#if !SUPPORTS_TIME_PROVIDER && SUPPORTS_TIMEZONEINFO

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace System;

/// <summary>
/// Provides an abstraction for time.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="TimeProvider"/> class allows for abstracting time-based operations,
/// making it easier to test time-dependent code by providing a way to substitute the
/// system clock with a custom implementation.
/// </para>
/// <para>
/// Use <see cref="System"/> to get the default system time provider.
/// </para>
/// </remarks>
public abstract class TimeProvider {

  /// <summary>
  /// Gets a <see cref="TimeProvider"/> that provides a clock based on <see cref="DateTimeOffset.UtcNow"/>,
  /// a time zone based on <see cref="TimeZoneInfo.Local"/>, and a high-performance time stamp based on
  /// <see cref="Stopwatch"/>.
  /// </summary>
  public static TimeProvider System { get; } = new SystemTimeProvider();

  /// <summary>
  /// Initializes the <see cref="TimeProvider"/>.
  /// </summary>
  protected TimeProvider() { }

  /// <summary>
  /// Gets a <see cref="TimeZoneInfo"/> object that represents the local time zone according to this <see cref="TimeProvider"/>'s notion of time.
  /// </summary>
  /// <value>
  /// The time zone that represents the local time zone.
  /// </value>
  public virtual TimeZoneInfo LocalTimeZone => TimeZoneInfo.Local;

  /// <summary>
  /// Gets the frequency of <see cref="GetTimestamp"/> of high-frequency value per second.
  /// </summary>
  /// <value>
  /// The frequency in ticks per second.
  /// </value>
  public virtual long TimestampFrequency => Stopwatch.Frequency;

  /// <summary>
  /// Gets the current UTC date and time according to this <see cref="TimeProvider"/>'s notion of time.
  /// </summary>
  /// <returns>
  /// The current UTC date and time.
  /// </returns>
  public virtual DateTimeOffset GetUtcNow() => DateTimeOffset.UtcNow;

  /// <summary>
  /// Gets the current local date and time according to this <see cref="TimeProvider"/>'s notion of time.
  /// </summary>
  /// <returns>
  /// The current local date and time.
  /// </returns>
  public DateTimeOffset GetLocalNow() {
    var utcNow = this.GetUtcNow();
    var localTimeZone = this.LocalTimeZone;
    return TimeZoneInfo.ConvertTime(utcNow, localTimeZone);
  }

  /// <summary>
  /// Gets the current high-frequency value designed to measure small time intervals with high accuracy.
  /// </summary>
  /// <returns>
  /// A long integer representing the high-frequency counter value of the underlying timer mechanism.
  /// </returns>
  public virtual long GetTimestamp() => Stopwatch.GetTimestamp();

  /// <summary>
  /// Gets the elapsed time since the specified <paramref name="startingTimestamp"/> value.
  /// </summary>
  /// <param name="startingTimestamp">The timestamp marking the beginning of the time period.</param>
  /// <returns>
  /// The elapsed time since the specified starting timestamp.
  /// </returns>
  public TimeSpan GetElapsedTime(long startingTimestamp)
    => this.GetElapsedTime(startingTimestamp, this.GetTimestamp());

  /// <summary>
  /// Gets the elapsed time between two timestamps.
  /// </summary>
  /// <param name="startingTimestamp">The timestamp marking the beginning of the time period.</param>
  /// <param name="endingTimestamp">The timestamp marking the end of the time period.</param>
  /// <returns>
  /// The elapsed time between the two specified timestamps.
  /// </returns>
  public TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp) {
    var frequency = this.TimestampFrequency;
    if (frequency <= 0)
      throw new InvalidOperationException("The timestamp frequency must be greater than zero.");

    var delta = endingTimestamp - startingTimestamp;
    return TimeSpan.FromTicks((long)(delta * ((double)TimeSpan.TicksPerSecond / frequency)));
  }

  /// <summary>
  /// Creates a new <see cref="ITimer"/> instance using the specified callback, state, due time, and period.
  /// </summary>
  /// <param name="callback">
  /// A delegate representing a method to be executed when the timer fires.
  /// The method specified for callback should be reentrant, as it may be invoked simultaneously on two threads
  /// if the timer interval is less than the time required to execute the method.
  /// </param>
  /// <param name="state">
  /// An object to be passed to the callback, or null.
  /// </param>
  /// <param name="dueTime">
  /// The amount of time to delay before callback is invoked.
  /// Specify <see cref="Timeout.InfiniteTimeSpan"/> to prevent the timer from starting.
  /// Specify <see cref="TimeSpan.Zero"/> to start the timer immediately.
  /// </param>
  /// <param name="period">
  /// The time interval between invocations of callback.
  /// Specify <see cref="Timeout.InfiniteTimeSpan"/> to disable periodic signaling.
  /// </param>
  /// <returns>
  /// The newly created <see cref="ITimer"/> instance.
  /// </returns>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="callback"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="dueTime"/> or <paramref name="period"/> is less than -1 milliseconds.
  /// </exception>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="dueTime"/> or <paramref name="period"/> is greater than 4294967294 milliseconds.
  /// </exception>
  public virtual ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period) {
    ArgumentNullException.ThrowIfNull(callback);
    
    return new SystemTimer(callback, state, dueTime, period);
  }

  /// <summary>
  /// The default system time provider implementation.
  /// </summary>
  private sealed class SystemTimeProvider : TimeProvider {
    internal SystemTimeProvider() { }
  }

  /// <summary>
  /// A wrapper around <see cref="Timer"/> that implements <see cref="ITimer"/>.
  /// </summary>
  private sealed class SystemTimer : ITimer {

    private Timer? _timer;
    private readonly object _lock = new();

    public SystemTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period) {
      _ValidateTimeSpan(dueTime, nameof(dueTime));
      _ValidateTimeSpan(period, nameof(period));
      this._timer = new(callback, state, dueTime, period);
    }

    public bool Change(TimeSpan dueTime, TimeSpan period) {
      _ValidateTimeSpan(dueTime, nameof(dueTime));
      _ValidateTimeSpan(period, nameof(period));

      lock (this._lock) {
        var timer = this._timer;
        if (timer == null)
          return false;

        return timer.Change(dueTime, period);
      }
    }

    public void Dispose() {
      lock (this._lock) {
        var timer = this._timer;
        this._timer = null;
        timer?.Dispose();
      }
    }

    public ValueTask DisposeAsync() {
      this.Dispose();
      return default;
    }

    private static void _ValidateTimeSpan(TimeSpan value, string paramName) {
      var milliseconds = (long)value.TotalMilliseconds;
      if (milliseconds < -1)
        throw new ArgumentOutOfRangeException(paramName, value, "The value must be greater than or equal to -1 milliseconds.");

      // Timer.Change uses uint internally, max value is UInt32.MaxValue - 1
      const long maxMilliseconds = 4294967294;
      if (milliseconds > maxMilliseconds)
        throw new ArgumentOutOfRangeException(paramName, value, $"The value must be less than or equal to {maxMilliseconds} milliseconds.");
    }

  }

}

#endif
