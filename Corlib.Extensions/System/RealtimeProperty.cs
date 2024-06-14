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

using System.Threading;
using Guard;

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

  public RealtimeProperty(Func<TType> getter, Action<TType> setter = null, TimeSpan? timeout = null, bool isAsyncSetter = false) {
    Against.ArgumentIsNull(getter);

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

  public bool GotValue { get; private set; }

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

    var call = Invoke;
    call.BeginInvoke(call.EndInvoke, null);

    return manualResetEventSlim.Wait(timeout ?? this.Timeout);

    void Invoke() {
      try {
        this._lastKnownValue = getter();
        manualResetEventSlim.Set();
        this.GotValue = true;
      } finally {
        Interlocked.CompareExchange(ref this._isGettingValue, _IS_IDLE, _IS_BUSY);
      }
    }
  }
}
