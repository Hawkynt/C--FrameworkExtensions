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

namespace System.Threading {
  /// <summary>
  /// A value that is calculated on an available pool thread or immediately on first access.
  /// </summary>
  /// <typeparam name="TValue">The type of the value.</typeparam>

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  class Future<TValue> {
    private readonly ManualResetEventSlim _HasValueAlready = new(false);
    private Exception _exception;
    private TValue _value;
    private Func<TValue> _getter;

    #region ctor

    public Future(Func<TValue> function, Action<TValue> callback = null) {
      this._getter = this._WaitForValueCreation;
      function.BeginInvoke(asyncResult => {
        try {
          this._value = function.EndInvoke(asyncResult);
          callback?.Invoke(this._value);
          Interlocked.Exchange(ref this._getter, this._GetRawValue);
        } catch (Exception exception) {
          this._exception = exception;
          Interlocked.Exchange(ref this._getter, this._ThrowException);
        } finally {
          ((ManualResetEventSlim)asyncResult.AsyncState).Set();
        }
      }, this._HasValueAlready);
    }

    #endregion
    /// <summary>
    /// Gets a value indicating whether this instance has value.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance has value; otherwise, <c>false</c>.
    /// </value>
    public bool HasValue => Interlocked.CompareExchange(ref this._getter, this._GetRawValue, this._GetRawValue) == this._GetRawValue;

    /// <summary>
    /// Gets a value indicating whether this instance is completed.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is completed; otherwise, <c>false</c>.
    /// </value>
    public bool IsCompleted => this._HasValueAlready.IsSet;

    /// <summary>
    /// Gets the value.
    /// </summary>
    public TValue Value => this._getter();

    private TValue _GetRawValue() => this._value;

    private TValue _ThrowException() {
      throw this._exception;
    }

    private TValue _WaitForValueCreation() {
      this._HasValueAlready.Wait();
      return this.Value;
    }


    /// <summary>
    /// Performs an implicit conversion from <see cref="System.Threading.Future&lt;TValue&gt;"/> to <see cref="TValue"/>.
    /// </summary>
    /// <param name="future">The future.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator TValue(Future<TValue> future) => future.Value;

    /// <summary>
    /// Returns a <see cref="System.String"/> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="System.String"/> that represents this instance.
    /// </returns>
    public override string ToString() => this.HasValue ? this.Value.ToString() : ("Future of type:" + typeof(TValue).Name);
  }

  /// <summary>
  /// An action that is executed on an available pool thread or immediately on first access.
  /// </summary>

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  class Future {
    private readonly Future<byte> _future;
    public Future(Action action, Action callback = null) {
      this._future = new(() => {
        action();
        return (byte.MaxValue);
      }, _ => {
        callback?.Invoke();
      });
    }
    /// <summary>
    /// Gets a value indicating whether this instance is completed.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is completed; otherwise, <c>false</c>.
    /// </value>
    public bool IsCompleted => this._future.HasValue;
  }
}
