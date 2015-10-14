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

namespace System.Threading {
  /// <summary>
  /// A value that is calculated on an available pool thread or immediately on first access.
  /// </summary>
  /// <typeparam name="TValue">The type of the value.</typeparam>
  public class Future<TValue> {
    private Exception _exception;
    private readonly ManualResetEventSlim _HasValueAlready = new ManualResetEventSlim(false);
    private TValue _value;
    #region ctor
    public Future(Func<TValue> function, Func<TValue, TValue> converter, Action<TValue> callback = null)
      : this(function, callback, converter) {
    }
    public Future(Func<TValue> function, Action<TValue> callback = null, Func<TValue, TValue> converter = null) {
      function.BeginInvoke(asyncResult => {
        try {
          this._value = function.EndInvoke(asyncResult);
          if (converter != null)
            this._value = converter(this._value);
        } catch (Exception exception) {
          this._exception = exception;
        } finally {
          ((ManualResetEventSlim)asyncResult.AsyncState).Set();

          // check if there is a callback
          if (callback != null) {
            try {
              callback(this.Value);
            } catch (Exception exception) {
              Diagnostics.Trace.WriteLine("Error executing callback in future:" + exception.Message);
#if DEBUG
              throw;
#endif
            }
          }
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
    public bool HasValue => this._HasValueAlready.IsSet;

    /// <summary>
    /// Gets a value indicating whether this instance is completed.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is completed; otherwise, <c>false</c>.
    /// </value>
    public bool IsCompleted => this.HasValue;

    /// <summary>
    /// Gets the value.
    /// </summary>
    public TValue Value {
      get {
        if (!this.HasValue)
          this._HasValueAlready.Wait();

        if (this._exception != null)
          throw (this._exception);
        return (this._value);
      }
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
  public class Future {
    private readonly Future<byte> _future;
    public Future(Action action, Action callback = null) {
      this._future = new Future<byte>(() => {
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
