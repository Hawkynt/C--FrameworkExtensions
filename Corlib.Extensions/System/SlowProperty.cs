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

namespace System {
  /// <summary>
  /// Represents a slow property for INotifyPropertyChanged classes.
  /// Allows showing intermediate values while retrieving the values.
  /// </summary>
  /// <typeparam name="TValue">The type of the value.</typeparam>
  /// <typeparam name="TIntermediateValue">The type of the intermediate value.</typeparam>

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  class SlowProperty<TValue, TIntermediateValue> {
    private const int _TRUE = 1;
    private const int _FALSE = 0;

    #region fields
    /// <summary>
    /// Whether the value is currently generated. (eg. thread is running to fetch value)
    /// </summary>
    private int _isGeneratingValue = _FALSE;
    private TValue _value;
    private readonly TIntermediateValue _intermediateValue;
    private readonly Func<SlowProperty<TValue, TIntermediateValue>, TValue> _valueGetter;
    private readonly Action<SlowProperty<TValue, TIntermediateValue>> _valueGeneratedCallback;
    private readonly Func<TValue, TIntermediateValue> _valueConverter;
    private ManualResetEventSlim _valueWaiter = new();
    private readonly SynchronizationContext _context;

    #endregion

    #region ctor

    public SlowProperty(Func<SlowProperty<TValue, TIntermediateValue>, TValue> valueGetter, TIntermediateValue intermediateValue = default(TIntermediateValue), Action<SlowProperty<TValue, TIntermediateValue>> valueGeneratedCallback = null, bool captureSynchronizationContext = false) :
      this(captureSynchronizationContext ? SynchronizationContext.Current : null, valueGetter, intermediateValue, valueGeneratedCallback) { }

    public SlowProperty(SynchronizationContext context, Func<SlowProperty<TValue, TIntermediateValue>, TValue> valueGetter, TIntermediateValue intermediateValue = default(TIntermediateValue), Action<SlowProperty<TValue, TIntermediateValue>> valueGeneratedCallback = null) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(valueGetter != null);
      Contract.Requires(typeof(TIntermediateValue).IsAssignableFrom(typeof(TValue)));
#endif
      this._valueGetter = valueGetter;
      this._intermediateValue = intermediateValue;
      this._valueGeneratedCallback = valueGeneratedCallback;

      var itype = typeof(TIntermediateValue);
      var vtype = typeof(TValue);
      this._valueConverter =
        itype.IsGenericType && itype.GetGenericTypeDefinition() == typeof(Nullable<>) && itype.GetGenericArguments()[0] == vtype
        ? new(v => (TIntermediateValue)(object)v)
        : new Func<TValue, TIntermediateValue>(v => (TIntermediateValue)Convert.ChangeType(v, itype))
        ;

      this._context = context;
    }
    #endregion

    #region props
    /// <summary>
    /// Gets the value (returns intermediate value as long as the real value is not ready).
    /// </summary>
    /// <value>
    /// The value.
    /// </value>
    public TIntermediateValue Value {
      get {
        if (this._valueWaiter.IsSet)
          return (this._valueConverter(this._value));

        this._TryStartGeneratingValue();
        return (this._intermediateValue);
      }
    }

    /// <summary>
    /// Gets the raw value (waits for it to be created if not yet done).
    /// </summary>
    /// <value>
    /// The raw value.
    /// </value>
    public TValue RawValue {
      get {
        if (!this._valueWaiter.IsSet)
          this._TryStartGeneratingValue();

        this._valueWaiter.Wait();
        return (this._value);
      }
    }

    #endregion

    #region methods
    /// <summary>
    /// Resets this instance and clears the cached value.
    /// </summary>
    public void Reset() {
      this._value = default(TValue);
      this._valueWaiter.Reset();
    }

    /// <summary>
    /// Tries to start the thread to fetch the real value.
    /// </summary>
    private void _TryStartGeneratingValue() {
      if (Interlocked.CompareExchange(ref this._isGeneratingValue, _TRUE, _FALSE) == _TRUE)
        return;

      Action call = this._GenerateValue;
      call.BeginInvoke(call.EndInvoke, null);
    }

    /// <summary>
    /// Fetches the value from the given generator and executes callback when done.
    /// </summary>
    private void _GenerateValue() {
      try {
        this._value = this._valueGetter(this);
        this._valueWaiter.Set();
        ;
        if (this._context == null)
          this._valueGeneratedCallback?.Invoke(this);
        else
          this._context.Send(_ => this._valueGeneratedCallback?.Invoke(this), null);

      } finally {
        Interlocked.CompareExchange(ref this._isGeneratingValue, _FALSE, _TRUE);
      }
    }
    #endregion

    public static implicit operator TIntermediateValue(SlowProperty<TValue, TIntermediateValue> This) => This.Value;

    #region Overrides of Object

    public override string ToString() => string.Format("{0}", this.Value);

    #endregion
  }

  /// <summary>
  /// Represents a slow property for INotifyPropertyChanged classes.
  /// Allows showing intermediate values while retrieving the values.
  /// </summary>
  /// <typeparam name="TValue">The type of the value.</typeparam>

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  class SlowProperty<TValue> : SlowProperty<TValue, TValue> {
    public SlowProperty(Func<SlowProperty<TValue>, TValue> valueGetter, TValue intermediateValue = default(TValue), Action<SlowProperty<TValue>> valueGeneratedCallback = null, bool captureSynchronizationContext = false)
      : base(
        This => valueGetter((SlowProperty<TValue>)This),
        intermediateValue,
        valueGeneratedCallback == null ? (Action<SlowProperty<TValue, TValue>>)null : This => valueGeneratedCallback((SlowProperty<TValue>)This),
        captureSynchronizationContext
        ) { }

    public SlowProperty(SynchronizationContext context, Func<SlowProperty<TValue>, TValue> valueGetter, TValue intermediateValue = default(TValue), Action<SlowProperty<TValue>> valueGeneratedCallback = null)
      : base(
        context,
          This => valueGetter((SlowProperty<TValue>)This),
        intermediateValue,
        valueGeneratedCallback == null ? (Action<SlowProperty<TValue, TValue>>)null : This => valueGeneratedCallback((SlowProperty<TValue>)This)
      ) { }
  }
}
