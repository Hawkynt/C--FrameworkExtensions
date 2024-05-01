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

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace System;

/// <summary>
/// Creates a value that is only calculated on first access and then cached.
/// </summary>
/// <typeparam name="TValue">The type of the result.</typeparam>
public class FastLazy<TValue> {

  private readonly Func<TValue> _factory;
  private Func<TValue> _getter;
  private TValue _value;

  /// <summary>
  /// Initializes a new instance of the <see cref="FastLazy&lt;TValue&gt;"/> class.
  /// </summary>
  /// <param name="factory">The factory.</param>
  public FastLazy(Func<TValue> factory) {
#if SUPPORTS_CONTRACTS
    Contract.Requires(factory != null);
#endif

    this._factory = factory;
    this.Reset();
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="FastLazy&lt;TValue&gt;"/> class.
  /// </summary>
  /// <param name="value">The value.</param>
  public FastLazy(TValue value) {

    // initialize the getter with a method that returns the current value.
    this._value = value;
    this.HasValue = true;
    this._getter = this._GetValue;
    this._factory = this._GetValue;
  }

  /// <summary>
  /// Initializes the value, stores it for later use and changes the getter method pointer to something that just returns the value.
  /// </summary>
  /// <returns></returns>
  private TValue _InitializeValue() {

    // locking keeps value creation thread-safe and it only occurs when initializing the value
    lock (this) {

      // in case another thread already initialized
      if (this.HasValue)
        return (this._value);

      // create value exactly once
      var value = this._factory();
      this._value = value;
      this._getter = this._GetValue;
      this.HasValue = true;
      return (value);
    }
  }

  /// <summary>
  /// Gets value stored earlier.
  /// </summary>
  /// <returns></returns>
  private TValue _GetValue() => this._value;

  /// <summary>
  /// Gets the value.
  /// </summary>
  public TValue Value => this._getter();

  /// <summary>
  /// Gets a value indicating whether this instance has a value.
  /// </summary>
  /// <value>
  ///   <c>true</c> if this instance has value; otherwise, <c>false</c>.
  /// </value>
  public bool HasValue { get; private set; }

  /// <summary>
  /// Resets the value cached from the factory and triggers to call the factory, next time a value is needed.
  /// </summary>
  public void Reset() {
    lock (this) {
      this.HasValue = false;

      // initialize the getter with something that creates the value and then replaces the getter with a method that returns the current value.
      this._getter = this._InitializeValue;
    }
  }

  /// <summary>
  /// Performs an implicit conversion from <see cref="System.FastLazy&lt;TValue&gt;"/> to <see cref="TValue"/>.
  /// </summary>
  /// <param name="this">The this.</param>
  /// <returns>
  /// The result of the conversion.
  /// </returns>
  public static implicit operator TValue(FastLazy<TValue> @this) => @this.Value;

  /// <summary>
  /// Performs an implicit conversion from <see cref="TValue"/> to <see cref="System.FastLazy&lt;TValue&gt;"/>.
  /// </summary>
  /// <param name="this">The this.</param>
  /// <returns>
  /// The result of the conversion.
  /// </returns>
  public static implicit operator FastLazy<TValue>(TValue @this) => new(@this);

  /// <summary>
  /// Performs an implicit conversion from <see cref="System.Func&lt;TValue&gt;"/> to <see cref="System.FastLazy&lt;TValue&gt;"/>.
  /// </summary>
  /// <param name="this">The this.</param>
  /// <returns>
  /// The result of the conversion.
  /// </returns>
  public static implicit operator FastLazy<TValue>(Func<TValue> @this) => new(@this);
}