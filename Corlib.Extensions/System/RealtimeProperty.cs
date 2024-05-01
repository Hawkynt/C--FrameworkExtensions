#region (c)2010-2042 Hawkynt
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

#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif
using System.Threading;

namespace System;

public class RealtimeProperty<TType> {

  private const int _IS_IDLE = 0;
  private const int _IS_BUSY = 1;
  private static readonly TimeSpan _DEFAULT_TIMEOUT = TimeSpan.FromMilliseconds(100);

  private readonly Func<TType> _getter;
  private readonly Action<TType> _setter;
  private TType _lastKnownValue;
  private readonly bool _isAsyncSetter;
  private int _isGettingValue = _IS_IDLE;
  private readonly ManualResetEventSlim _manualResetEventSlim = new(false);
  private bool _gotValueAtLeastOnce;

  public RealtimeProperty(Func<TType> getter, Action<TType> setter = null, TimeSpan? timeout = null, bool isAsyncSetter = false) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(getter != null);
#endif
    this._getter = getter;
    this._setter = setter;
    this._isAsyncSetter = isAsyncSetter;
    this.Timeout = timeout ?? _DEFAULT_TIMEOUT;
  }

  public TType Value {
    get => this.GetValue(null);
    set {
      var setter = this._setter;
      if (setter == null)
        throw new NotSupportedException("No setter!");

      this._lastKnownValue = value;

      if (this._isAsyncSetter)
        setter.BeginInvoke(value, setter.EndInvoke, null);
      else
        setter(value);

    }
  }

  public TimeSpan Timeout { get; set; }

  public bool GotValue => this._gotValueAtLeastOnce;

  public TType GetValue(TimeSpan? timeout) {
    this._TryGetValue(timeout);
    return this._lastKnownValue;
  }

  private bool _TryGetValue(TimeSpan? timeout) {
    var getter = this._getter;
    var manualResetEventSlim = this._manualResetEventSlim;

    // if already getting value, wait for it
    if (Interlocked.CompareExchange(ref this._isGettingValue, _IS_BUSY, _IS_IDLE) != _IS_IDLE)
      return manualResetEventSlim.Wait(timeout ?? this.Timeout);

    // reset value flag
    manualResetEventSlim.Reset();
    Action call = () => {
      try {
        this._lastKnownValue = getter();
        manualResetEventSlim.Set();
        this._gotValueAtLeastOnce = true;
      } finally {
        Interlocked.CompareExchange(ref this._isGettingValue, _IS_IDLE, _IS_BUSY);
      }
    };

    // get in background
    call.BeginInvoke(call.EndInvoke, null);

    return manualResetEventSlim.Wait(timeout ?? this.Timeout);
  }

}