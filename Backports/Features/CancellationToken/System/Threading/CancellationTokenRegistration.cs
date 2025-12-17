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
/// Represents a callback delegate that has been registered with a <see cref="CancellationToken"/>.
/// </summary>
public readonly struct CancellationTokenRegistration : IDisposable {

  private readonly CancellationTokenSource _source;
  private readonly CancellationTokenSource._CallbackInfo _callback;

  internal CancellationTokenRegistration(CancellationTokenSource source, CancellationTokenSource._CallbackInfo callback) {
    this._source = source;
    this._callback = callback;
  }

  /// <summary>
  /// Releases all resources used by the current instance of the <see cref="CancellationTokenRegistration"/> class.
  /// </summary>
  public void Dispose() => this._source?.Unregister(this._callback);

  /// <inheritdoc />
  public override bool Equals(object obj) => obj is CancellationTokenRegistration other && this.Equals(other);

  /// <summary>
  /// Determines whether the current <see cref="CancellationTokenRegistration"/> instance is equal to the specified registration.
  /// </summary>
  public bool Equals(CancellationTokenRegistration other)
    => this._source == other._source && this._callback == other._callback;

  /// <inheritdoc />
  public override int GetHashCode() => (this._source?.GetHashCode() ?? 0) ^ (this._callback?.GetHashCode() ?? 0);

  /// <summary>
  /// Determines whether two <see cref="CancellationTokenRegistration"/> instances are equal.
  /// </summary>
  public static bool operator ==(CancellationTokenRegistration left, CancellationTokenRegistration right) => left.Equals(right);

  /// <summary>
  /// Determines whether two <see cref="CancellationTokenRegistration"/> instances are not equal.
  /// </summary>
  public static bool operator !=(CancellationTokenRegistration left, CancellationTokenRegistration right) => !left.Equals(right);

}

#endif
