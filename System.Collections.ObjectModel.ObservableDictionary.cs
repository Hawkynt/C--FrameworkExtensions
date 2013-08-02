using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace System.Collections.ObjectModel {

  public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged {
    private readonly IDictionary<TKey, TValue> _dictionary;

    private Dispatcher _dispatcher;

    #region Constructor

    public ObservableDictionary(Dispatcher dispatcher = null) {
      _dictionary = new Dictionary<TKey, TValue>();
      this._dispatcher = dispatcher ??
                              (Application.Current != null
                                   ? Application.Current.Dispatcher
                                   : Dispatcher.CurrentDispatcher);
    }

    public ObservableDictionary(IDictionary<TKey, TValue> dictionary) {
      this._dictionary = new Dictionary<TKey, TValue>(dictionary);
    }

    public ObservableDictionary(int capacity) {
      _dictionary = new Dictionary<TKey, TValue>(capacity);
    }

    #endregion

    public void Add(KeyValuePair<TKey, TValue> item) {
      if(_dispatcher.CheckAccess()) {
        _dictionary.Add(item);

        OnCollectionChanged(NotifyCollectionChangedAction.Add, item.Value);
        AddNotifyEvents(item.Value);
      } else {
        _dispatcher.Invoke(new Action(() => Add(item)));
      }
    }

    public void Clear() {
      if(_dispatcher.CheckAccess()) {
        foreach(var value in Values) {
          RemoveNotifyEvents(value);
        }

        _dictionary.Clear();
        OnCollectionChanged(NotifyCollectionChangedAction.Reset);
      } else {
        _dispatcher.Invoke(new Action(Clear));
      }
    }

    public bool Contains(KeyValuePair<TKey, TValue> item) {
      return _dictionary.Contains(item);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
      _dictionary.CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<TKey, TValue> item) {
      if(_dispatcher.CheckAccess()) {
        bool result = _dictionary.Remove(item);
        if(result) {
          RemoveNotifyEvents(item.Value);
          OnCollectionChanged(NotifyCollectionChangedAction.Remove, item.Value);
        }

        return result;
      } else {
        return (bool)_dispatcher.Invoke(new Action(() => Remove(item)));
      }
    }

    public int Count {
      get {
        return _dictionary.Count;
      }
    }

    public bool IsReadOnly {
      get {
        return _dictionary.IsReadOnly;
      }
    }

    public bool ContainsKey(TKey key) {
      return _dictionary.ContainsKey(key);
    }

    public void Add(TKey key, TValue value) {
      if(_dispatcher.CheckAccess()) {
        _dictionary.Add(key, value);
        OnCollectionChanged(NotifyCollectionChangedAction.Add, value);
        AddNotifyEvents(value);
      } else {
        _dispatcher.Invoke(new Action(() => Add(key, value)));
      }
    }

    public bool Remove(TKey key) {
      if(_dispatcher.CheckAccess()) {
        if(_dictionary.ContainsKey(key)) {
          TValue value = _dictionary[key];
          _dictionary.Remove(key);
          RemoveNotifyEvents(value);
          OnCollectionChanged(NotifyCollectionChangedAction.Remove, value);

          return true;
        }

        return false;
      } else {
        return (bool)_dispatcher.Invoke(new Action(() => Remove(key)));
      }
    }

    public bool TryGetValue(TKey key, out TValue value) {
      return _dictionary.TryGetValue(key, out value);
    }

    public TValue this[TKey key] {
      get {
        return _dictionary[key];
      }
      set {
        if(_dispatcher.CheckAccess()) {
          if(_dictionary.ContainsKey(key))
            throw new ArgumentException("Unknown key: " + key);

          OnCollectionChanged(NotifyCollectionChangedAction.Replace, _dictionary[key], value);
          RemoveNotifyEvents(_dictionary[key]);
          AddNotifyEvents(value);

          _dictionary[key] = value;
        } else {
          _dispatcher.Invoke(new Action(() => this[key] = value));
        }
      }
    }

    public ICollection<TKey> Keys {
      get {
        return _dictionary.Keys;
      }
    }

    public ICollection<TValue> Values {
      get {
        return _dictionary.Values;
      }
    }

    #region Enumerator

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
      return _dictionary.GetEnumerator();
    }

    #endregion

    #region Notify

    public event NotifyCollectionChangedEventHandler CollectionChanged;
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(String property) {
      if(PropertyChanged != null) {
        PropertyChanged(this, new PropertyChangedEventArgs(property));
      }
    }

    protected void OnPropertyChanged(PropertyChangedEventArgs e) {
      if(PropertyChanged != null) {
        PropertyChanged(this, e);
      }
    }

    protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
      if(CollectionChanged != null) {
        CollectionChanged(this, e);
      }
    }

    protected void OnCollectionChanged(NotifyCollectionChangedAction action) {
      if(CollectionChanged != null) {
        CollectionChanged(this, new NotifyCollectionChangedEventArgs(action));
      }
    }

    protected void OnCollectionChanged(NotifyCollectionChangedAction action, TValue item) {
      if(CollectionChanged != null) {
        CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item));
      }
    }

    protected void OnCollectionChanged(NotifyCollectionChangedAction action, TValue oldItem, TValue newItem) {
      if(CollectionChanged != null) {
        CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, newItem, oldItem));
      }
    }

    #endregion

    #region Private

    private void AddNotifyEvents(TValue item) {
      if(item is INotifyCollectionChanged) {
        (item as INotifyCollectionChanged).CollectionChanged += OnCollectionChangedEventHandler;
      }
      if(item is INotifyPropertyChanged) {
        (item as INotifyPropertyChanged).PropertyChanged += OnPropertyChangedEventHandler;
      }
    }

    private void RemoveNotifyEvents(TValue item) {
      if(item is INotifyCollectionChanged) {
        (item as INotifyCollectionChanged).CollectionChanged -= OnCollectionChangedEventHandler;
      }
      if(item is INotifyPropertyChanged) {
        (item as INotifyPropertyChanged).PropertyChanged -= OnPropertyChangedEventHandler;
      }
    }

    private void OnPropertyChangedEventHandler(object s, PropertyChangedEventArgs e) {
      OnPropertyChanged(e);
    }

    private void OnCollectionChangedEventHandler(object s, NotifyCollectionChangedEventArgs e) {
      OnPropertyChanged("value");
    }

    #endregion
  }
}