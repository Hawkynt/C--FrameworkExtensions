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

#if !SUPPORTS_CANCELLATIONTOKENSOURCE

using System.Collections.Generic;

namespace System.Threading;

/// <summary>
/// Signals to a <see cref="CancellationToken"/> that it should be canceled.
/// </summary>
public sealed class CancellationTokenSource : IDisposable {

  private volatile bool _isCancellationRequested;
  private volatile bool _isDisposed;
  private readonly ManualResetEvent _waitHandle = new(false);
  private readonly List<_CallbackInfo> _callbacks = [];
  private readonly object _lock = new();

  /// <summary>
  /// Gets whether cancellation has been requested for this <see cref="CancellationTokenSource"/>.
  /// </summary>
  public bool IsCancellationRequested => this._isCancellationRequested;

  /// <summary>
  /// Gets the <see cref="CancellationToken"/> associated with this <see cref="CancellationTokenSource"/>.
  /// </summary>
  public CancellationToken Token => new(this);

  /// <summary>
  /// Gets a <see cref="System.Threading.WaitHandle"/> that is signaled when the token is canceled.
  /// </summary>
  internal WaitHandle WaitHandle {
    get {
      this._ThrowIfDisposed();
      return this._waitHandle;
    }
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="CancellationTokenSource"/> class.
  /// </summary>
  public CancellationTokenSource() { }

  /// <summary>
  /// Initializes a new instance of the <see cref="CancellationTokenSource"/> class that will be canceled after the specified delay.
  /// </summary>
  /// <param name="millisecondsDelay">The time interval in milliseconds to wait before canceling this <see cref="CancellationTokenSource"/>.</param>
  public CancellationTokenSource(int millisecondsDelay) {
    if (millisecondsDelay < -1)
      throw new ArgumentOutOfRangeException(nameof(millisecondsDelay));

    if (millisecondsDelay != Timeout.Infinite)
      this._StartTimer(millisecondsDelay);
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="CancellationTokenSource"/> class that will be canceled after the specified time span.
  /// </summary>
  /// <param name="delay">The time span to wait before canceling this <see cref="CancellationTokenSource"/>.</param>
  public CancellationTokenSource(TimeSpan delay)
    : this((int)delay.TotalMilliseconds) { }

  private Timer? _timer;

  private void _StartTimer(int millisecondsDelay) => this._timer = new(_ => this.Cancel(), null, millisecondsDelay, Timeout.Infinite);

  /// <summary>
  /// Communicates a request for cancellation.
  /// </summary>
  public void Cancel() => this.Cancel(false);

  /// <summary>
  /// Communicates a request for cancellation.
  /// </summary>
  /// <param name="throwOnFirstException">Specifies whether exceptions should immediately propagate.</param>
  public void Cancel(bool throwOnFirstException) {
    this._ThrowIfDisposed();

    if (this._isCancellationRequested)
      return;

    this._isCancellationRequested = true;
    this._waitHandle.Set();

    List<Exception>? exceptions = null;
    _CallbackInfo[] callbacksCopy;

    lock (this._lock)
      callbacksCopy = [.. this._callbacks];

    foreach (var callback in callbacksCopy) {
      try {
        callback.Invoke();
      } catch (Exception ex) {
        if (throwOnFirstException)
          throw;
        exceptions ??= [];
        exceptions.Add(ex);
      }
    }

    if (exceptions is { Count: > 0 })
      throw new AggregateException(exceptions);
  }

  /// <summary>
  /// Schedules a cancel operation on this <see cref="CancellationTokenSource"/> after the specified time span.
  /// </summary>
  /// <param name="delay">The time span to wait before canceling this <see cref="CancellationTokenSource"/>.</param>
  public void CancelAfter(TimeSpan delay) => this.CancelAfter((int)delay.TotalMilliseconds);

  /// <summary>
  /// Schedules a cancel operation on this <see cref="CancellationTokenSource"/> after the specified number of milliseconds.
  /// </summary>
  /// <param name="millisecondsDelay">The time span to wait before canceling this <see cref="CancellationTokenSource"/>.</param>
  public void CancelAfter(int millisecondsDelay) {
    this._ThrowIfDisposed();

    if (millisecondsDelay < -1)
      throw new ArgumentOutOfRangeException(nameof(millisecondsDelay));

    if (this._isCancellationRequested)
      return;

    this._timer?.Dispose();
    if (millisecondsDelay != Timeout.Infinite)
      this._StartTimer(millisecondsDelay);
  }

  internal CancellationTokenRegistration Register(Action callback) {
    this._ThrowIfDisposed();

    var info = new _CallbackInfo(callback);

    if (this._isCancellationRequested) {
      callback();
      return default;
    }

    lock (this._lock)
      this._callbacks.Add(info);

    return new(this, info);
  }

  internal CancellationTokenRegistration Register(Action<object> callback, object state) {
    this._ThrowIfDisposed();

    var info = new _CallbackInfo(callback, state);

    if (this._isCancellationRequested) {
      callback(state);
      return default;
    }

    lock (this._lock)
      this._callbacks.Add(info);

    return new(this, info);
  }

  internal void Unregister(_CallbackInfo callback) {
    lock (this._lock)
      this._callbacks.Remove(callback);
  }

  private void _ThrowIfDisposed() {
    if (this._isDisposed)
      throw new ObjectDisposedException(nameof(CancellationTokenSource));
  }

  /// <summary>
  /// Releases the resources used by this <see cref="CancellationTokenSource"/>.
  /// </summary>
  public void Dispose() {
    if (this._isDisposed)
      return;

    this._isDisposed = true;
    this._timer?.Dispose();
    this._waitHandle.Close();

    lock (this._lock)
      this._callbacks.Clear();
  }

  /// <summary>
  /// Creates a <see cref="CancellationTokenSource"/> that will be in the canceled state when any of the source tokens are in the canceled state.
  /// </summary>
  public static CancellationTokenSource CreateLinkedTokenSource(CancellationToken token1, CancellationToken token2) {
    var cts = new CancellationTokenSource();

    if (token1.CanBeCanceled)
      token1.Register(() => cts.Cancel());

    if (token2.CanBeCanceled)
      token2.Register(() => cts.Cancel());

    return cts;
  }

  /// <summary>
  /// Creates a <see cref="CancellationTokenSource"/> that will be in the canceled state when any of the source tokens are in the canceled state.
  /// </summary>
  public static CancellationTokenSource CreateLinkedTokenSource(params CancellationToken[] tokens) {
    var cts = new CancellationTokenSource();

    foreach (var token in tokens)
      if (token.CanBeCanceled)
        token.Register(() => cts.Cancel());

    return cts;
  }

  internal sealed class _CallbackInfo {
    private readonly Action? _callback;
    private readonly Action<object>? _callbackWithState;
    private readonly object? _state;

    public _CallbackInfo(Action callback) => this._callback = callback;

    public _CallbackInfo(Action<object> callback, object state) {
      this._callbackWithState = callback;
      this._state = state;
    }

    public void Invoke() {
      if (this._callback != null)
        this._callback();
      else
        this._callbackWithState?.Invoke(this._state!);
    }
  }

}

#endif
