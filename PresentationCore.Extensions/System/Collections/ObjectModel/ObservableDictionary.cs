#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY

#endregion

using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace System.Collections.ObjectModel;

public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged {
  private readonly IDictionary<TKey, TValue> _dictionary;

  private readonly Dispatcher _dispatcher;

  #region Constructor

  public ObservableDictionary(Dispatcher dispatcher = null) {
    this._dictionary = new Dictionary<TKey, TValue>();
    this._dispatcher = dispatcher
                       ?? (Application.Current != null
                         ? Application.Current.Dispatcher
                         : Dispatcher.CurrentDispatcher);
  }

  public ObservableDictionary(IDictionary<TKey, TValue> dictionary) => this._dictionary = new Dictionary<TKey, TValue>(dictionary);

  public ObservableDictionary(int capacity) => this._dictionary = new Dictionary<TKey, TValue>(capacity);

  #endregion

  public void Add(KeyValuePair<TKey, TValue> item) {
    if (this._dispatcher.CheckAccess()) {
      this._dictionary.Add(item);

      this.OnCollectionChanged(NotifyCollectionChangedAction.Add, item.Value);
      this.AddNotifyEvents(item.Value);
    } else {
      void _Add() => this.Add(item);
      this._dispatcher.Invoke(new Action(_Add));
    }
  }

  public void Clear() {
    if (this._dispatcher.CheckAccess()) {
      foreach (var value in this.Values)
        this.RemoveNotifyEvents(value);

      this._dictionary.Clear();
      this.OnCollectionChanged(NotifyCollectionChangedAction.Reset);
    } else
      this._dispatcher.Invoke(new Action(this.Clear));
  }


  public bool Contains(KeyValuePair<TKey, TValue> item) => this._dictionary.Contains(item);

  public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => this._dictionary.CopyTo(array, arrayIndex);

  public bool Remove(KeyValuePair<TKey, TValue> item) {
    if (this._dispatcher.CheckAccess()) {
      var result = this._dictionary.Remove(item);
      if (result) {
        this.RemoveNotifyEvents(item.Value);
        this.OnCollectionChanged(NotifyCollectionChangedAction.Remove, item.Value);
      }

      return result;
    }

    return (bool)this._dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => this.Remove(item)));
  }

  public int Count => this._dictionary.Count;

  public bool IsReadOnly => this._dictionary.IsReadOnly;

  public bool ContainsKey(TKey key) => this._dictionary.ContainsKey(key);

  public void Add(TKey key, TValue value) {
    if (this._dispatcher.CheckAccess()) {
      this._dictionary.Add(key, value);
      this.OnCollectionChanged(NotifyCollectionChangedAction.Add, value);
      this.AddNotifyEvents(value);
    } else {
      void _Add() => this.Add(key, value);
      this._dispatcher.Invoke(new Action(_Add));
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
      } else
        this._dispatcher.Invoke(new Action(() => this[key] = value));
    }
  }

  public ICollection<TKey> Keys => this._dictionary.Keys;

  public ICollection<TValue> Values => this._dictionary.Values;

  #region Enumerator

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => this._dictionary.GetEnumerator();

  #endregion

  #region Notify

  public event NotifyCollectionChangedEventHandler CollectionChanged;
  public event PropertyChangedEventHandler PropertyChanged;

  protected void OnPropertyChanged(string property) => this.PropertyChanged?.Invoke(this, new(property));

  protected void OnPropertyChanged(PropertyChangedEventArgs e) => this.PropertyChanged?.Invoke(this, e);

  protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e) => this.CollectionChanged?.Invoke(this, e);

  protected void OnCollectionChanged(NotifyCollectionChangedAction action) => this.CollectionChanged?.Invoke(this, new(action));

  protected void OnCollectionChanged(NotifyCollectionChangedAction action, TValue item) => this.CollectionChanged?.Invoke(this, new(action, item));

  protected void OnCollectionChanged(NotifyCollectionChangedAction action, TValue oldItem, TValue newItem) => this.CollectionChanged?.Invoke(this, new(action, newItem, oldItem));

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

  private void OnCollectionChangedEventHandler(object s, NotifyCollectionChangedEventArgs e) 
    => this.OnPropertyChanged("value");

  #endregion
}
