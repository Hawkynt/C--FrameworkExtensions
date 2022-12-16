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

namespace System.Threading {
  /// <summary>
  /// Creates a value that is only calculated on first access and then cached.
  /// </summary>
  /// <typeparam name="TValue">The type of the result.</typeparam>

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  class Lazy<TValue> {
    private readonly ManualResetEventSlim _valueIsReady = new ManualResetEventSlim(false);
    private readonly object _lock = new object();
    private TValue _value;
    private readonly Func<TValue> _function;
    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="Lazy&lt;TValue&gt;"/> class.
    /// </summary>
    /// <param name="function">The function that should create the value.</param>
    public Lazy(Func<TValue> function) {
      this._function = function;
    }
    #endregion
    /// <summary>
    /// Gets a value indicating whether this instance has a value already calculated.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance has value; otherwise, <c>false</c>.
    /// </value>
    public bool HasValue {
      get {
        return (this._valueIsReady.IsSet);
      }
    }
    /// <summary>
    /// Gets a value indicating whether this instance completed calculation.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is completed; otherwise, <c>false</c>.
    /// </value>
    public bool IsCompleted {
      get {
        return (this.HasValue);
      }
    }
    /// <summary>
    /// Gets the value.
    /// </summary>
    public TValue Value {
      get {
        if (!this.HasValue)
          lock (this._lock)
            if (!this.HasValue) {
              this._value = this._function();
              this._valueIsReady.Set();
            }
        return (this._value);
      }
    }
    /// <summary>
    /// Performs an implicit conversion from <see cref="System.Threading.Lazy&lt;TValue&gt;"/> to <see cref="TValue"/>.
    /// </summary>
    /// <param name="lazy">The obj lazy.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator TValue(Lazy<TValue> lazy) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(lazy != null);
#endif
      return (lazy.Value);
    }
    /// <summary>
    /// Returns a <see cref="System.String"/> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="System.String"/> that represents this instance.
    /// </returns>
    public override string ToString() {
      return (this.HasValue ? this.Value.ToString() : ("Lazy of type:" + typeof(TValue).Name));
    }
  }

  /// <summary>
  /// A class that only calls it's action on access.
  /// </summary>

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  class Lazy {
    private readonly Lazy<byte> _lazy;
    public Lazy(Action action) {
      this._lazy = new Lazy<byte>(() => {
        action();
        return (byte.MaxValue);
      });
    }
    /// <summary>
    /// Gets a value indicating whether this instance is completed.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is completed; otherwise, <c>false</c>.
    /// </value>
    public bool IsCompleted {
      get {
        return (this._lazy.HasValue);
      }
    }
    /// <summary>
    /// Triggers the action.
    /// </summary>
    public byte Value {
      get {
        return (this._lazy.Value);
      }
    }
  }
}
