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

using Guard;

namespace System.Threading;

/// <summary>
///   Allows a block of code to be flagged with a timeout callback.
///   Note: The executed code will not be aborted, only the timeout gets called.
/// </summary>
public class CallOnTimeout : IDisposable {
  private static readonly TimeSpan _TIME_BEFORE_CHECKS = TimeSpan.FromMilliseconds(100);
  private readonly DateTime _creationTime = DateTime.UtcNow;
  private readonly Timer _timer;
  private readonly Action<CallOnTimeout> _timeoutAction;

  public TimeSpan Timeout { get; }
  public TimeSpan TimeLeft => this.Timeout - this.ElapsedTime;
  public TimeSpan ElapsedTime => DateTime.UtcNow - this._creationTime;

  #region ctor,dtor

  public CallOnTimeout(TimeSpan timeout, Action<CallOnTimeout> timeoutAction) {
    Against.ArgumentIsNull(timeoutAction);

    this.Timeout = timeout;
    this._timeoutAction = timeoutAction;
    var action = this._CheckTimeout;
    this._timer = new(this._CheckTimeout);
    action.BeginInvoke(null, action.EndInvoke, null);
  }

  #region Implementation of IDisposable

  private volatile bool _isDisposed;

  public void Dispose() {
    if (this._isDisposed)
      return;

    this._isDisposed = true;
    this._timer.Change(Threading.Timeout.Infinite, Threading.Timeout.Infinite);
    this._timer.Dispose();
  }

  ~CallOnTimeout() => this.Dispose();

  #endregion

  #endregion

  private void _CheckTimeout(object _) {
    while (!this._isDisposed) {
      var timeLeft = this.TimeLeft;
      if (timeLeft.TotalMilliseconds <= 0) {
        this._timeoutAction(this);
        return;
      }

      if (timeLeft <= _TIME_BEFORE_CHECKS)
        continue;

      this._CallbackIn(timeLeft - _TIME_BEFORE_CHECKS);
      return;
    }
  }

  private void _CallbackIn(TimeSpan when) => this._timer.Change((long)when.TotalMilliseconds, Threading.Timeout.Infinite);
}
