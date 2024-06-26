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

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System.Timers;

// Summary:
//     Represents the method that will handle the
//     System.Timers.HighPrecisionTimer.Elapsed event
//     of a System.Timers.HighPrecisionTimer.
//
// Parameters:
//   sender:
//     The source of the event.
//
//   e:
//     An System.Timers.HighPrecisionTimerElapsedEventHandler object that contains
//     the event data.
public delegate void HighPrecisionTimerElapsedEventHandler(object sender, HighPrecisionTimerElapsedEventArgs e);

// Summary:
//     Provides data for the System.Timers.Timer.Elapsed event.
public class HighPrecisionTimerElapsedEventArgs : EventArgs {
  // Summary:
  //     Gets the time the System.Timers.Multimedia.Elapsed event was 
  //     raised.
  //
  // Returns:
  //     The time the System.Timers.Multimedia.Elapsed event was raised.
  public DateTime SignalTime { get; internal set; }

  internal HighPrecisionTimerElapsedEventArgs() => this.SignalTime = DateTime.Now;
}

// Summary:
//     Generates recurring events in an application.
public class HighPrecisionTimer : IDisposable {
  //Lib API declarations
  /// <summary>
  ///   Times the set event.
  /// </summary>
  /// <param name="uDelay">The u delay.</param>
  /// <param name="uResolution">The u resolution.</param>
  /// <param name="lpTimeProc">The lp time proc.</param>
  /// <param name="dwUser">The dw user.</param>
  /// <param name="fuEvent">The fu event.</param>
  /// <returns></returns>
  [DllImport("Winmm.dll", CharSet = CharSet.Auto)]
  private static extern uint timeSetEvent(uint uDelay, uint uResolution, TimerCallback lpTimeProc, UIntPtr dwUser, uint fuEvent);

  /// <summary>
  ///   Times the kill event.
  /// </summary>
  /// <param name="uTimerID">The u timer ID.</param>
  /// <returns></returns>
  [DllImport("Winmm.dll", CharSet = CharSet.Auto)]
  private static extern uint timeKillEvent(uint uTimerID);

  /// <summary>
  ///   Times the get time.
  /// </summary>
  /// <returns></returns>
  [DllImport("Winmm.dll", CharSet = CharSet.Auto)]
  private static extern uint timeGetTime();

  /// <summary>
  ///   Times the begin period.
  /// </summary>
  /// <param name="uPeriod">The u period.</param>
  /// <returns></returns>
  [DllImport("Winmm.dll", CharSet = CharSet.Auto)]
  private static extern uint timeBeginPeriod(uint uPeriod);

  /// <summary>
  ///   Times the end period.
  /// </summary>
  /// <param name="uPeriod">The u period.</param>
  /// <returns></returns>
  [DllImport("Winmm.dll", CharSet = CharSet.Auto)]
  private static extern uint timeEndPeriod(uint uPeriod);


  /// <summary>
  ///   Timer type definitions
  /// </summary>
  [Flags]
  private enum fuEvent : uint {
    /// <summary>
    ///   OneHzSignalEvent occurs once, after uDelay milliseconds.
    /// </summary>
    TIME_ONESHOT = 0,

    /// <summary>
    ///   Event occurs periodically.
    /// </summary>
    TIME_PERIODIC = 1,

    /// <summary>
    ///   callback is function
    /// </summary>
    TIME_CALLBACK_FUNCTION = 0x0000,
  }

  /// <summary>
  ///   Delegate definition for the API callback
  /// </summary>
  private delegate void TimerCallback(uint uTimerID, uint uMsg, UIntPtr dwUser, UIntPtr dw1, UIntPtr dw2);

  /// <summary>
  ///   The current timer instance ID
  /// </summary>
  private uint id;

  /// <summary>
  ///   To assure the callback is pinned by the GC, we use this handle.
  /// </summary>
  private GCHandle __gcHandle;

  /// <summary>
  ///   The callback used by the the API
  /// </summary>
  private readonly TimerCallback timerCallback;

  public HighPrecisionTimer()
    : this(100) { }

  /// <summary>
  ///   Initializes a new instance of the <see cref="HighPrecisionTimer" /> class.
  /// </summary>
  /// <param name="interval">The interval.</param>
  /// <param name="action">The action.</param>
  public HighPrecisionTimer(uint interval, HighPrecisionTimerElapsedEventHandler action)
    : this(interval) =>
    this.Elapsed += action;

  /// <summary>
  ///   Initializes a new instance of the <see cref="HighPrecisionTimer" /> class.
  /// </summary>
  /// <param name="interval">The interval.</param>
  public HighPrecisionTimer(TimeSpan interval)
    : this((uint)interval.TotalMilliseconds) { }

  /// <summary>
  ///   Initializes a new instance of the <see cref="HighPrecisionTimer" /> class.
  /// </summary>
  /// <param name="interval">The interval.</param>
  /// <param name="action">The action.</param>
  public HighPrecisionTimer(TimeSpan interval, HighPrecisionTimerElapsedEventHandler action)
    : this((uint)interval.TotalMilliseconds, action) { }

  /// <summary>
  ///   Initializes a new instance of the System.Timers.HighPrecisionTimer
  ///   class, and sets the
  ///   System.Timers.HighPrecisionTimer.Interval property to the specified
  ///   time period.
  ///   Parameters:
  ///   interval:
  ///   The time, in milliseconds, between events.
  ///   Exceptions:
  ///   System.ArgumentException:
  ///   The value of the interval parameter is less than or equal to
  ///   zero, or greater than System.Int32.MaxValue.
  /// </summary>
  /// <param name="interval">The interval.</param>
  public HighPrecisionTimer(uint interval) {
    this.Interval = interval;
    this.AutoReset = true;
    this.Enabled = false;
    //Initialize the API callback
    this.timerCallback = this.CallbackFunction;
    this.__gcHandle = GCHandle.Alloc(this.timerCallback);
  }


  /// <summary>
  ///   Gets or sets a value indicating whether the
  ///   System.Timers.HighPrecisionTimer should raise
  ///   the System.Timers.HighPrecisionTimer.Elapsed event each time the
  ///   specified interval elapses
  ///   or only after the first time it elapses.
  ///   Returns:
  ///   true if the System.Timers.HighPrecisionTimer should raise the
  ///   System.Timers.HighPrecisionTimer.Elapsed
  ///   event each time the interval elapses; false if it should raise
  ///   the System.Timers.HighPrecisionTimer.Elapsed
  ///   event only once, after the first time the interval elapses. The
  ///   default is true.
  /// </summary>
  /// <value><c>true</c> if [auto reset]; otherwise, <c>false</c>.</value>
  public bool AutoReset { get; set; }

  /// <summary>
  ///   Gets or sets a value indicating whether the
  ///   System.Timers.HighPrecisionTimer should raise
  ///   the System.Timers.HighPrecisionTimer.Elapsed event.
  ///   Returns:
  ///   true if the System.Timers.HighPrecisionTimer should raise the
  ///   System.Timers.HighPrecisionTimer.Elapsed
  ///   event; otherwise, false. The default is false.
  /// </summary>
  /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
  public bool Enabled { get; private set; }

  private readonly object syncLock = new();

  /// <summary>
  ///   Gets or sets the interval at which to raise the
  ///   System.Timers.HighPrecisionTimer.Elapsed event.
  ///   Returns:
  ///   The time, in milliseconds, between raisings of the
  ///   System.Timers.HighPrecisionTimer.Elapsed
  ///   event. The default is 100 milliseconds.
  ///   Exceptions:
  ///   System.ArgumentException:
  ///   The interval is less than or equal to zero.
  /// </summary>
  /// <value>The interval.</value>
  public uint Interval { get; set; }


  /// <summary>
  ///   Occurs when the interval elapses.
  /// </summary>
  public event HighPrecisionTimerElapsedEventHandler Elapsed;


  /// <summary>
  ///   Releases the resources used by the System.Timers.HighPrecisionTimer.
  /// </summary>
  public void Close() => this.Dispose();


  /// <summary>
  ///   Starts raising the System.Timers.HighPrecisionTimer.Elapsed event by
  ///   setting System.Timers.HighPrecisionTimer.Enabled
  ///   to true.
  ///   Exceptions:
  ///   System.ArgumentOutOfRangeException:
  ///   The System.Timers.HighPrecisionTimer is created with an interval
  ///   equal to or greater than
  ///   System.Int32.MaxValue + 1, or set to an interval less than zero.
  /// </summary>
  public void Start() {
    lock (this.syncLock) {
      //Kill any existing timer
      this.Stop();
      this.Enabled = false;

      //Set the timer type flags
      var f = fuEvent.TIME_CALLBACK_FUNCTION | (this.AutoReset ? fuEvent.TIME_PERIODIC : fuEvent.TIME_ONESHOT);

      this.id = timeSetEvent(this.Interval, 0, this.timerCallback, UIntPtr.Zero, (uint)f);
      if (this.id == 0)
        throw new("timeSetEvent error");

      Debug.WriteLine("HighPrecisionTimer " + this.id + " started");
      this.Enabled = true;
    }
  }


  /// <summary>
  ///   Stops raising the System.Timers.HighPrecisionTimer.Elapsed event by
  ///   setting System.Timers.HighPrecisionTimer.Enabled
  ///   to false.
  /// </summary>
  public void Stop() {
    lock (this.syncLock) {
      if (this.id == 0)
        return;
      timeKillEvent(this.id);
      Debug.WriteLine("HighPrecisionTimer " + this.id + " stopped");
      this.id = 0;
      this.Enabled = false;
    }
  }

  /// <summary>
  ///   Called when [timer].
  /// </summary>
  protected virtual void OnTimer() => this.Elapsed?.Invoke(this, new());

  /// <summary>
  ///   CBs the func.
  /// </summary>
  /// <param name="uTimerID">The u timer ID.</param>
  /// <param name="uMsg">The u MSG.</param>
  /// <param name="dwUser">The dw user.</param>
  /// <param name="dw1">The DW1.</param>
  /// <param name="dw2">The DW2.</param>
  private void CallbackFunction(uint uTimerID, uint uMsg, UIntPtr dwUser, UIntPtr dw1, UIntPtr dw2) =>
    //Callback from the HighPrecisionTimer API that fires the Timer event. 
    // Note we are in a different thread here
    this.OnTimer();

  #region IDisposable Members

  private bool _disposed;


  /// <summary>
  ///   Performs application-defined tasks associated with freeing,
  ///   releasing, or resetting unmanaged resources.
  ///   Releases all resources used by the current
  ///   System.Timers.HighPrecisionTimer.
  ///   Parameters:
  ///   disposing:
  ///   true to release both managed and unmanaged resources; false to
  ///   release only
  ///   unmanaged resources.
  /// </summary>
  public void Dispose() {
    this._Dispose();
    GC.SuppressFinalize(this);
  }

  /// <summary>
  ///   Releases unmanaged and - optionally - managed resources
  /// </summary>
  private void _Dispose() {
    if (!this._disposed) {
      this.Stop();
      this.__gcHandle.Free();
    }

    this._disposed = true;
  }

  /// <summary>
  ///   Releases unmanaged resources and performs other cleanup operations
  ///   before the
  ///   <see cref="HighPrecisionTimer" /> is reclaimed by garbage collection.
  /// </summary>
  ~HighPrecisionTimer() => this._Dispose();

  #endregion
}
