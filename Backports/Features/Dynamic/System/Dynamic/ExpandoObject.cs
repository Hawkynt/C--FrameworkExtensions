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

// System.Dynamic was introduced in .NET 4.0
// Only polyfill for net20/net35 where no DLR exists
#if !SUPPORTS_DYNAMIC

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
#if SUPPORTS_LINQ
// net35: Use our helper for missing Expression methods
using Expr = System.Dynamic.Expr;
#endif

namespace System.Dynamic;

/// <summary>
/// Represents an object whose members can be dynamically added and removed at runtime.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ExpandoObject"/> enables adding, removing, and accessing members dynamically
/// at runtime. It implements <see cref="IDictionary{TKey, TValue}"/> for dictionary-style
/// access to its members, and <see cref="INotifyPropertyChanged"/> for property change notifications.
/// </para>
/// <para>
/// This class is thread-safe for read operations. Write operations are protected by locking.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// dynamic expando = new ExpandoObject();
/// expando.Name = "John";
/// expando.Age = 30;
/// Console.WriteLine($"{expando.Name} is {expando.Age} years old");
///
/// // Dictionary-style access
/// var dict = (IDictionary&lt;string, object&gt;)expando;
/// dict["City"] = "Seattle";
/// </code>
/// </example>
public sealed class ExpandoObject : IDynamicMetaObjectProvider, IDictionary<string, object?>, INotifyPropertyChanged {

  /// <summary>
  /// The internal dictionary that stores the dynamic members.
  /// </summary>
  private readonly Dictionary<string, object?> _data = new(StringComparer.Ordinal);

  /// <summary>
  /// Lock for thread-safe access to the data.
  /// </summary>
  private readonly object _lock = new();

  /// <summary>
  /// Initializes a new instance of the <see cref="ExpandoObject"/> class.
  /// </summary>
  public ExpandoObject() { }

  /// <summary>
  /// Occurs when a property value changes.
  /// </summary>
  public event PropertyChangedEventHandler? PropertyChanged;

  /// <summary>
  /// Raises the <see cref="PropertyChanged"/> event.
  /// </summary>
  /// <param name="propertyName">The name of the property that changed.</param>
  private void OnPropertyChanged(string propertyName) =>
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

  #region IDynamicMetaObjectProvider

  /// <summary>
  /// Returns the <see cref="DynamicMetaObject"/> responsible for binding operations performed on this object.
  /// </summary>
  /// <param name="parameter">The expression tree representation of the runtime value.</param>
  /// <returns>
  /// The <see cref="DynamicMetaObject"/> to bind this object.
  /// </returns>
  DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) =>
    new MetaExpando(parameter, this);

  #endregion

  #region IDictionary<string, object?>

  /// <summary>
  /// Gets or sets the element with the specified key.
  /// </summary>
  /// <param name="key">The key of the element to get or set.</param>
  /// <returns>The element with the specified key.</returns>
  /// <exception cref="KeyNotFoundException">The property is retrieved and <paramref name="key"/> is not found.</exception>
  public object? this[string key] {
    get {
      lock (this._lock)
        return this._data[key];
    }
    set {
      lock (this._lock)
        this._data[key] = value;
      this.OnPropertyChanged(key);
    }
  }

  /// <summary>
  /// Gets a collection containing the keys of the <see cref="ExpandoObject"/>.
  /// </summary>
  public ICollection<string> Keys {
    get {
      lock (this._lock)
        return new List<string>(this._data.Keys);
    }
  }

  /// <summary>
  /// Gets a collection containing the values in the <see cref="ExpandoObject"/>.
  /// </summary>
  public ICollection<object?> Values {
    get {
      lock (this._lock)
        return new List<object?>(this._data.Values);
    }
  }

  /// <summary>
  /// Gets the number of elements contained in the <see cref="ExpandoObject"/>.
  /// </summary>
  public int Count {
    get {
      lock (this._lock)
        return this._data.Count;
    }
  }

  /// <summary>
  /// Gets a value indicating whether the <see cref="ExpandoObject"/> is read-only.
  /// </summary>
  bool ICollection<KeyValuePair<string, object?>>.IsReadOnly => false;

  /// <summary>
  /// Adds an element with the provided key and value to the <see cref="ExpandoObject"/>.
  /// </summary>
  /// <param name="key">The object to use as the key of the element to add.</param>
  /// <param name="value">The object to use as the value of the element to add.</param>
  public void Add(string key, object? value) {
    lock (this._lock)
      this._data.Add(key, value);
    this.OnPropertyChanged(key);
  }

  /// <summary>
  /// Adds an item to the <see cref="ExpandoObject"/>.
  /// </summary>
  /// <param name="item">The object to add to the <see cref="ExpandoObject"/>.</param>
  void ICollection<KeyValuePair<string, object?>>.Add(KeyValuePair<string, object?> item) =>
    this.Add(item.Key, item.Value);

  /// <summary>
  /// Removes all items from the <see cref="ExpandoObject"/>.
  /// </summary>
  public void Clear() {
    string[] keys;
    lock (this._lock) {
      keys = new string[this._data.Count];
      this._data.Keys.CopyTo(keys, 0);
      this._data.Clear();
    }

    foreach (var key in keys)
      this.OnPropertyChanged(key);
  }

  /// <summary>
  /// Determines whether the <see cref="ExpandoObject"/> contains a specific value.
  /// </summary>
  /// <param name="item">The object to locate in the <see cref="ExpandoObject"/>.</param>
  /// <returns>
  /// <see langword="true"/> if <paramref name="item"/> is found in the <see cref="ExpandoObject"/>; otherwise, <see langword="false"/>.
  /// </returns>
  bool ICollection<KeyValuePair<string, object?>>.Contains(KeyValuePair<string, object?> item) {
    lock (this._lock)
      return ((ICollection<KeyValuePair<string, object?>>)this._data).Contains(item);
  }

  /// <summary>
  /// Determines whether the <see cref="ExpandoObject"/> contains an element with the specified key.
  /// </summary>
  /// <param name="key">The key to locate in the <see cref="ExpandoObject"/>.</param>
  /// <returns>
  /// <see langword="true"/> if the <see cref="ExpandoObject"/> contains an element with the key; otherwise, <see langword="false"/>.
  /// </returns>
  public bool ContainsKey(string key) {
    lock (this._lock)
      return this._data.ContainsKey(key);
  }

  /// <summary>
  /// Copies the elements of the <see cref="ExpandoObject"/> to an <see cref="Array"/>, starting at a particular <see cref="Array"/> index.
  /// </summary>
  /// <param name="array">
  /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="ExpandoObject"/>.
  /// The <see cref="Array"/> must have zero-based indexing.
  /// </param>
  /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
  void ICollection<KeyValuePair<string, object?>>.CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex) {
    lock (this._lock)
      ((ICollection<KeyValuePair<string, object?>>)this._data).CopyTo(array, arrayIndex);
  }

  /// <summary>
  /// Returns an enumerator that iterates through the collection.
  /// </summary>
  /// <returns>An enumerator that can be used to iterate through the collection.</returns>
  public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() {
    List<KeyValuePair<string, object?>> snapshot;
    lock (this._lock)
      snapshot = new List<KeyValuePair<string, object?>>(this._data);
    return snapshot.GetEnumerator();
  }

  /// <summary>
  /// Returns an enumerator that iterates through a collection.
  /// </summary>
  /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  /// <summary>
  /// Removes the element with the specified key from the <see cref="ExpandoObject"/>.
  /// </summary>
  /// <param name="key">The key of the element to remove.</param>
  /// <returns>
  /// <see langword="true"/> if the element is successfully removed; otherwise, <see langword="false"/>.
  /// This method also returns <see langword="false"/> if <paramref name="key"/> was not found in the original <see cref="ExpandoObject"/>.
  /// </returns>
  public bool Remove(string key) {
    bool removed;
    lock (this._lock)
      removed = this._data.Remove(key);

    if (removed)
      this.OnPropertyChanged(key);

    return removed;
  }

  /// <summary>
  /// Removes the first occurrence of a specific object from the <see cref="ExpandoObject"/>.
  /// </summary>
  /// <param name="item">The object to remove from the <see cref="ExpandoObject"/>.</param>
  /// <returns>
  /// <see langword="true"/> if <paramref name="item"/> was successfully removed from the <see cref="ExpandoObject"/>; otherwise, <see langword="false"/>.
  /// </returns>
  bool ICollection<KeyValuePair<string, object?>>.Remove(KeyValuePair<string, object?> item) {
    bool removed;
    lock (this._lock) {
      if (!((ICollection<KeyValuePair<string, object?>>)this._data).Contains(item))
        return false;
      removed = this._data.Remove(item.Key);
    }

    if (removed)
      this.OnPropertyChanged(item.Key);

    return removed;
  }

  /// <summary>
  /// Gets the value associated with the specified key.
  /// </summary>
  /// <param name="key">The key whose value to get.</param>
  /// <param name="value">
  /// When this method returns, the value associated with the specified key, if the key is found;
  /// otherwise, the default value for the type of the <paramref name="value"/> parameter.
  /// </param>
  /// <returns>
  /// <see langword="true"/> if the object that implements <see cref="ExpandoObject"/> contains an element with the specified key;
  /// otherwise, <see langword="false"/>.
  /// </returns>
  public bool TryGetValue(string key, out object? value) {
    lock (this._lock)
      return this._data.TryGetValue(key, out value);
  }

  #endregion

  #region MetaExpando

  /// <summary>
  /// The DynamicMetaObject implementation for ExpandoObject.
  /// </summary>
  private sealed class MetaExpando : DynamicMetaObject {

    /// <summary>
    /// Initializes a new instance of the <see cref="MetaExpando"/> class.
    /// </summary>
    internal MetaExpando(Expression expression, ExpandoObject value)
      : base(expression, BindingRestrictions.Empty, value) { }

    /// <summary>
    /// Gets the ExpandoObject value.
    /// </summary>
    private new ExpandoObject Value => (ExpandoObject)base.Value!;

    /// <summary>
    /// Creates the binding restrictions for this ExpandoObject.
    /// </summary>
    private BindingRestrictions GetRestrictions() =>
      BindingRestrictions.GetTypeRestriction(this.Expression, typeof(ExpandoObject));

    /// <summary>
    /// Performs the binding of the dynamic get member operation.
    /// </summary>
    public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
      // Call TryGetValue on the expando and return the value if found
      var self = Expression.Convert(this.Expression, typeof(ExpandoObject));
      var dictInterface = Expression.Convert(self, typeof(IDictionary<string, object>));
      var key = Expression.Constant(binder.Name);
      var result = Expression.Parameter(typeof(object), "result");

      var tryGetValue = typeof(IDictionary<string, object>).GetMethod(nameof(IDictionary<string, object>.TryGetValue));
      var containsKey = typeof(IDictionary<string, object>).GetMethod(nameof(IDictionary<string, object>.ContainsKey));

      // Build: dict.ContainsKey(name) ? dict[name] : throw
      var indexer = typeof(IDictionary<string, object>).GetProperty("Item");

#if !SUPPORTS_LINQ
      var body = Expression.Condition(
        Expression.Call(dictInterface, containsKey!, key),
        Expression.Property(dictInterface, indexer!, key),
        Expression.Throw(
          Expression.New(
            typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) })!,
            Expression.Constant($"'{binder.Name}' is not defined")
          ),
          typeof(object)
        )
      );
#else
      var body = Expression.Condition(
        Expression.Call(dictInterface, containsKey!, key),
        Expr.Property(dictInterface, indexer!, key),
        Expr.Throw(
          Expression.New(
            typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) })!,
            Expression.Constant($"'{binder.Name}' is not defined")
          ),
          typeof(object)
        )
      );
#endif

      return new DynamicMetaObject(body, this.GetRestrictions());
    }

    /// <summary>
    /// Performs the binding of the dynamic set member operation.
    /// </summary>
    public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value) {
      // Set the value in the dictionary
      var self = Expression.Convert(this.Expression, typeof(ExpandoObject));
      var indexer = typeof(ExpandoObject).GetProperty("Item");

#if !SUPPORTS_LINQ
      var setCall = Expression.Assign(
        Expression.Property(self, indexer!, Expression.Constant(binder.Name)),
        Expression.Convert(value.Expression, typeof(object))
      );
#else
      var setCall = Expr.Assign(
        Expr.Property(self, indexer!, Expression.Constant(binder.Name)),
        Expression.Convert(value.Expression, typeof(object))
      );
#endif

      return new DynamicMetaObject(setCall, this.GetRestrictions().Merge(value.Restrictions));
    }

    /// <summary>
    /// Performs the binding of the dynamic delete member operation.
    /// </summary>
    public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder) {
      // Remove the member from the dictionary
      var self = Expression.Convert(this.Expression, typeof(ExpandoObject));
      var removeMethod = typeof(ExpandoObject).GetMethod(nameof(ExpandoObject.Remove), new[] { typeof(string) });

      var removeCall = Expression.Call(self, removeMethod!, Expression.Constant(binder.Name));
#if !SUPPORTS_LINQ
      var body = Expression.Block(removeCall, Expression.Default(typeof(object)));
#else
      var body = Expr.Block(removeCall, Expr.Default(typeof(object)));
#endif

      return new DynamicMetaObject(body, this.GetRestrictions());
    }

    /// <summary>
    /// Returns the enumeration of all dynamic member names.
    /// </summary>
    public override IEnumerable<string> GetDynamicMemberNames() => this.Value.Keys;

  }

  #endregion

}

#endif
