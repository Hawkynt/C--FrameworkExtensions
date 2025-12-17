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

namespace System.Threading;

/// <summary>
/// Propagates notification that operations should be canceled.
/// </summary>
public readonly struct CancellationToken {

  private readonly CancellationTokenSource _source;

  internal CancellationToken(CancellationTokenSource source) => this._source = source;

  /// <summary>
  /// Gets an empty <see cref="CancellationToken"/> value.
  /// </summary>
  public static CancellationToken None => default;

  /// <summary>
  /// Gets whether cancellation has been requested for this token.
  /// </summary>
  public bool IsCancellationRequested => this._source?.IsCancellationRequested ?? false;

  /// <summary>
  /// Gets whether this token is capable of being in the canceled state.
  /// </summary>
  public bool CanBeCanceled => this._source != null;

  /// <summary>
  /// Gets a <see cref="WaitHandle"/> that is signaled when the token is canceled.
  /// </summary>
  public WaitHandle WaitHandle => this._source?.WaitHandle ?? _NeverCanceledSource.WaitHandle;

  private static readonly CancellationTokenSource _NeverCanceledSource = new();

  /// <summary>
  /// Throws an <see cref="OperationCanceledException"/> if this token has had cancellation requested.
  /// </summary>
  public void ThrowIfCancellationRequested() {
    if (this.IsCancellationRequested)
      throw new OperationCanceledException("The operation was canceled.");
  }

  /// <summary>
  /// Registers a delegate that will be called when this <see cref="CancellationToken"/> is canceled.
  /// </summary>
  /// <param name="callback">The delegate to be executed when the <see cref="CancellationToken"/> is canceled.</param>
  /// <returns>The <see cref="CancellationTokenRegistration"/> instance that can be used to unregister the callback.</returns>
  public CancellationTokenRegistration Register(Action callback)
    => this._source?.Register(callback) ?? default;

  /// <summary>
  /// Registers a delegate that will be called when this <see cref="CancellationToken"/> is canceled.
  /// </summary>
  /// <param name="callback">The delegate to be executed when the <see cref="CancellationToken"/> is canceled.</param>
  /// <param name="state">The state to pass to the callback.</param>
  /// <returns>The <see cref="CancellationTokenRegistration"/> instance that can be used to unregister the callback.</returns>
  public CancellationTokenRegistration Register(Action<object> callback, object state)
    => this._source?.Register(callback, state) ?? default;

  /// <inheritdoc />
  public override bool Equals(object obj) => obj is CancellationToken other && this.Equals(other);

  /// <summary>
  /// Determines whether the current <see cref="CancellationToken"/> instance is equal to the specified token.
  /// </summary>
  public bool Equals(CancellationToken other) => this._source == other._source;

  /// <inheritdoc />
  public override int GetHashCode() => this._source?.GetHashCode() ?? 0;

  /// <summary>
  /// Determines whether two <see cref="CancellationToken"/> instances are equal.
  /// </summary>
  public static bool operator ==(CancellationToken left, CancellationToken right) => left.Equals(right);

  /// <summary>
  /// Determines whether two <see cref="CancellationToken"/> instances are not equal.
  /// </summary>
  public static bool operator !=(CancellationToken left, CancellationToken right) => !left.Equals(right);

}

#endif
