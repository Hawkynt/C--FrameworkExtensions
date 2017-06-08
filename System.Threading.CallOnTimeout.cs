#region (c)2010-2020 Hawkynt
/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software: 
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that 
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied 
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.  
    If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

#if NETFX_4
using System.Diagnostics.Contracts;
#endif

namespace System.Threading {
  /// <summary>
  /// Allows a block of code to be flagged with a timeout callback.
  /// Note: The executed code will not be aborted, only the timeout gets called.
  /// </summary>
  internal class CallOnTimeout : IDisposable {

    private static readonly TimeSpan _TIME_BEFORE_CHECKS = TimeSpan.FromMilliseconds(100);
    private readonly DateTime _creationTime = DateTime.UtcNow;
    private readonly Timer _timer;
    private readonly Action<CallOnTimeout> _timeoutAction;

    public TimeSpan Timeout { get; }
    public TimeSpan TimeLeft => this.Timeout - this.ElapsedTime;
    public TimeSpan ElapsedTime => DateTime.UtcNow - this._creationTime;

    #region ctor,dtor
    public CallOnTimeout(TimeSpan timeout, Action<CallOnTimeout> timeoutAction) {
#if NETFX_4
      Contract.Requires(timeoutAction!=null);
#endif
      this.Timeout = timeout;
      this._timeoutAction = timeoutAction;
      this._timer = new Timer(this._CheckTimeout);
      Action<object> action = this._CheckTimeout;
      action.BeginInvoke(null, action.EndInvoke, null);
    }

    #region Implementation of IDisposable

    private volatile bool _isDisposed;
    public void Dispose() {
      if (this._isDisposed)
        return;

      this._isDisposed = true;
      this._timer.Change(Threading.Timeout.InfiniteTimeSpan, Threading.Timeout.InfiniteTimeSpan);
      this._timer.Dispose();
    }

    ~CallOnTimeout() {
      this.Dispose();
    }

    #endregion
    #endregion

    private void _CheckTimeout(object _) {
      while (!this._isDisposed) {

        var timeLeft = this.TimeLeft;
        if (timeLeft.TotalMilliseconds <= 0) {
          this._timeoutAction(this);
          return;
        }

        if (timeLeft > _TIME_BEFORE_CHECKS) {
          this._CallbackIn(timeLeft - _TIME_BEFORE_CHECKS);
          return;
        }

      }
    }

    private void _CallbackIn(TimeSpan when) => this._timer.Change(when, Threading.Timeout.InfiniteTimeSpan);
  }
}