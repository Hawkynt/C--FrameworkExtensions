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

using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace System.Collections.ObjectModel {
  // ReSharper disable once UnusedMember.Global

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyCollectionChanged,
    INotifyPropertyChanged {
    private readonly IDictionary<TKey, TValue> _dictionary;

    private readonly Dispatcher _dispatcher;

    #region Constructor

    public ObservableDictionary(Dispatcher dispatcher = null) {
      this._dictionary = new Dictionary<TKey, TValue>();
      this._dispatcher = dispatcher ??
                         (Application.Current != null
                           ? Application.Current.Dispatcher
                           : Dispatcher.CurrentDispatcher);
    }

    public ObservableDictionary(IDictionary<TKey, TValue> dictionary) {
      this._dictionary = new Dictionary<TKey, TValue>(dictionary);
    }

    public ObservableDictionary(int capacity) {
      this._dictionary = new Dictionary<TKey, TValue>(capacity);
    }

    #endregion

    public void Add(KeyValuePair<TKey, TValue> item) {
      if (this._dispatcher.CheckAccess()) {
        this._dictionary.Add(item);

        this.OnCollectionChanged(NotifyCollectionChangedAction.Add, item.Value);
        this.AddNotifyEvents(item.Value);
      } else {
        this._dispatcher.Invoke(() => this.Add(item));
      }
    }

    public void Clear() {
      if (this._dispatcher.CheckAccess()) {
        foreach (var value in this.Values)
          this.RemoveNotifyEvents(value);

        this._dictionary.Clear();
        this.OnCollectionChanged(NotifyCollectionChangedAction.Reset);
      } else {
        this._dispatcher.Invoke(this.Clear);
      }
    }

    public bool Contains(KeyValuePair<TKey, TValue> item) => this._dictionary.Contains(item);

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
      this._dictionary.CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<TKey, TValue> item) {
      if (this._dispatcher.CheckAccess()) {
        var result = this._dictionary.Remove(item);
        if (result) {
          this.RemoveNotifyEvents(item.Value);
          this.OnCollectionChanged(NotifyCollectionChangedAction.Remove, item.Value);
        }

        return result;
      } else {
        return (bool)this._dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => this.Remove(item)));
      }
    }

    public int Count => this._dictionary.Count;

    public bool IsReadOnly => this._dictionary.IsReadOnly;

    public bool ContainsKey(TKey key) {
      return this._dictionary.ContainsKey(key);
    }

    public void Add(TKey key, TValue value) {
      if (this._dispatcher.CheckAccess()) {
        this._dictionary.Add(key, value);
        this.OnCollectionChanged(NotifyCollectionChangedAction.Add, value);
        this.AddNotifyEvents(value);
      } else {
        this._dispatcher.Invoke(() => this.Add(key, value));
      }
    }

    public bool Remove(TKey key) {
      if (this._dispatcher.CheckAccess()) {
        if (this._dictionary.ContainsKey(key)) {
          var value = this._dictionary[key];
          this._dictionary.Remove(key);
          this.RemoveNotifyEvents(value);
          this.OnCollectionChanged(NotifyCollectionChangedAction.Remove, value);

          return true;
        }

        return false;
      }

      return (bool)this._dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => this.Remove(key)));
    }

    public bool TryGetValue(TKey key, out TValue value) => this._dictionary.TryGetValue(key, out value);

    public TValue this[TKey key] {
      get => this._dictionary[key];
      set {
        if (this._dispatcher.CheckAccess()) {
          if (this._dictionary.ContainsKey(key))
            throw new ArgumentException("Unknown key: " + key);

          this.OnCollectionChanged(NotifyCollectionChangedAction.Replace, this._dictionary[key], value);
          this.RemoveNotifyEvents(this._dictionary[key]);
          this.AddNotifyEvents(value);

          this._dictionary[key] = value;
        } else {
          this._dispatcher.Invoke(new Action(() => this[key] = value));
        }
      }
    }

    public ICollection<TKey> Keys => this._dictionary.Keys;

    public ICollection<TValue> Values => this._dictionary.Values;

    #region Enumerator

    IEnumerator IEnumerable.GetEnumerator() {
      return this.GetEnumerator();
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
      return this._dictionary.GetEnumerator();
    }

    #endregion

    #region Notify

    public event NotifyCollectionChangedEventHandler CollectionChanged;
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(String property) {
      if (this.PropertyChanged != null)
        this.PropertyChanged(this, new PropertyChangedEventArgs(property));
    }

    protected void OnPropertyChanged(PropertyChangedEventArgs e) {
      if (this.PropertyChanged != null)
        this.PropertyChanged(this, e);
    }

    protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
      if (this.CollectionChanged != null)
        this.CollectionChanged(this, e);
    }

    protected void OnCollectionChanged(NotifyCollectionChangedAction action) {
      if (this.CollectionChanged != null)
        this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(action));
    }

    protected void OnCollectionChanged(NotifyCollectionChangedAction action, TValue item) {
      if (this.CollectionChanged != null)
        this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item));
    }

    protected void OnCollectionChanged(NotifyCollectionChangedAction action, TValue oldItem, TValue newItem) {
      if (this.CollectionChanged != null)
        this.CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, newItem, oldItem));
    }

    #endregion

    #region Private

    private void AddNotifyEvents(TValue item) {
      if (item is INotifyCollectionChanged collectionChanged)
        collectionChanged.CollectionChanged += this.OnCollectionChangedEventHandler;

      if (item is INotifyPropertyChanged propertyChanged)
        propertyChanged.PropertyChanged += this.OnPropertyChangedEventHandler;
    }

    private void RemoveNotifyEvents(TValue item) {
      if (item is INotifyCollectionChanged collectionChanged)
        collectionChanged.CollectionChanged -= this.OnCollectionChangedEventHandler;

      if (item is INotifyPropertyChanged propertyChanged)
        propertyChanged.PropertyChanged -= this.OnPropertyChangedEventHandler;
    }

    private void OnPropertyChangedEventHandler(object s, PropertyChangedEventArgs e) => this.OnPropertyChanged(e);

    private void OnCollectionChangedEventHandler(object s, NotifyCollectionChangedEventArgs e) =>
      this.OnPropertyChanged("value");

    #endregion
  }
}
