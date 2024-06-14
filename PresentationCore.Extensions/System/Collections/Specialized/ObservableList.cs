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

#undef USEREADERWRITERLOCKSLIM
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Threading;

namespace System.Collections.Specialized;

public class ObservableList<T>(Dispatcher dispatcher = null) : IList<T>, IList, INotifyCollectionChanged
  where T : INotifyPropertyChanged {
  public Dispatcher Dispatcher { get; } = dispatcher ?? Dispatcher.CurrentDispatcher;

  private readonly List<T> _arrList = [];
#if USEREADERWRITERLOCKSLIM
    private const int _intTimeout = Timeout.Infinite;
    private readonly ReaderWriterLockSlim _objRWLS = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
#else
  private readonly object _objLock = new();
#endif

  public event NotifyCollectionChangedEventHandler CollectionChanged;

  private void _voidTriggerCollectionChanged(NotifyCollectionChangedEventArgs objEA) => this.CollectionChanged?.Invoke(this, objEA);

  public int IndexOf(T item) {
    int intRet;
#if USEREADERWRITERLOCKSLIM
      bool boolTaken = false;
      try {
        boolTaken = this._objRWLS.TryEnterReadLock(_intTimeout);
#else
    lock (this._objLock) {
#endif
      intRet = this._arrList.IndexOf(item);
#if USEREADERWRITERLOCKSLIM
      } finally {
        if (boolTaken)
          this._objRWLS.ExitReadLock();
#endif
    }

    return intRet;
  }

  public void Insert(int index, T item) {
    if (this.Dispatcher.CheckAccess()) {
#if USEREADERWRITERLOCKSLIM
        bool boolTaken = false;
        try {
          boolTaken = this._objRWLS.TryEnterWriteLock(_intTimeout);
#else
      lock (this._objLock) {
#endif
        this._arrList.Insert(index, item);
        this._voidTriggerCollectionChanged(new(NotifyCollectionChangedAction.Add, item, index));
#if USEREADERWRITERLOCKSLIM
        } finally {
          if (boolTaken)
            this._objRWLS.ExitWriteLock();
#endif
      }
    } else
      this.Dispatcher.Invoke(new Action<int, T>(this.Insert), DispatcherPriority.Send, index, item);
  }

  public void RemoveAt(int index) {
    if (this.Dispatcher.CheckAccess()) {
#if USEREADERWRITERLOCKSLIM
        bool boolTaken = false;
        try {
          boolTaken = this._objRWLS.TryEnterWriteLock(_intTimeout);
#else
      lock (this._objLock) {
#endif
        var item = this._arrList[index];
        this._arrList.RemoveAt(index);
        this._voidTriggerCollectionChanged(new(NotifyCollectionChangedAction.Remove, item, index));
#if USEREADERWRITERLOCKSLIM
        } finally {
          if(boolTaken)
            this._objRWLS.ExitWriteLock();
#endif
      }
    } else
      this.Dispatcher.Invoke(new Action<int>(this.RemoveAt), DispatcherPriority.Send, index);
  }

  public T this[int index] {
    get {
      T varRet;
#if USEREADERWRITERLOCKSLIM
        bool boolTaken = false;
        try {
          boolTaken = this._objRWLS.TryEnterReadLock(_intTimeout);
#else
      lock (this._objLock) {
#endif
        varRet = this._arrList[index];
#if USEREADERWRITERLOCKSLIM
        } finally {
          if (boolTaken)
            this._objRWLS.ExitReadLock();
#endif
      }

      return varRet;
    }
    set => this._voidSetItemInternal(index, value);
  }

  private void _voidSetItemInternal(int index, T item) {
    if (this.Dispatcher.CheckAccess()) {
#if USEREADERWRITERLOCKSLIM
        bool boolTaken = false;
        try {
          boolTaken = this._objRWLS.TryEnterWriteLock(_intTimeout);
#else
      lock (this._objLock) {
#endif
        var olditem = this._arrList[index];
        this._arrList[index] = item;
        this._voidTriggerCollectionChanged(new(NotifyCollectionChangedAction.Replace, item, olditem, index));
#if USEREADERWRITERLOCKSLIM
        } finally {
          if(boolTaken)
            this._objRWLS.ExitWriteLock();
#endif
      }
    } else
      this.Dispatcher.Invoke(new Action<int, T>(this._voidSetItemInternal), DispatcherPriority.Send, index, item);
  }

  public void Add(T item) {
    if (this.Dispatcher.CheckAccess()) {
#if USEREADERWRITERLOCKSLIM
        bool boolTaken = false;
        try {
          boolTaken = this._objRWLS.TryEnterWriteLock(_intTimeout);
#else
      lock (this._objLock) {
#endif
        this._arrList.Add(item);
        this._voidTriggerCollectionChanged(new(NotifyCollectionChangedAction.Add, item));
#if USEREADERWRITERLOCKSLIM
        } finally {
          if (boolTaken)
            this._objRWLS.ExitWriteLock();
#endif
      }
    } else
      this.Dispatcher.Invoke(new Action<T>(this.Add), DispatcherPriority.Send, item);
  }

  public void AddRange(IEnumerable<T> items) {
    if (this.Dispatcher.CheckAccess()) {
#if USEREADERWRITERLOCKSLIM
        bool boolTaken = false;
        try {
          boolTaken = this._objRWLS.TryEnterWriteLock(_intTimeout);
#else
      lock (this._objLock) {
#endif
        this._arrList.AddRange(items);
        this._voidTriggerCollectionChanged(new(NotifyCollectionChangedAction.Reset));
#if USEREADERWRITERLOCKSLIM
        } finally {
          if (boolTaken)
            this._objRWLS.ExitWriteLock();
#endif
      }
    } else
      this.Dispatcher.Invoke(new Action<IEnumerable<T>>(this.AddRange), DispatcherPriority.Send, items);
  }

  public void Clear() {
    if (this.Dispatcher.CheckAccess()) {
#if USEREADERWRITERLOCKSLIM
        bool boolTaken = false;
        try {
          boolTaken = this._objRWLS.TryEnterWriteLock(_intTimeout);
#else
      lock (this._objLock) {
#endif
        this._arrList.Clear();
        this._voidTriggerCollectionChanged(new(NotifyCollectionChangedAction.Reset));
#if USEREADERWRITERLOCKSLIM
        } finally {
          if (boolTaken)
            this._objRWLS.ExitWriteLock();
#endif
      }
    } else
      this.Dispatcher.Invoke(new Action(this.Clear), DispatcherPriority.Send);
  }

  public bool Contains(T item) {
    bool boolRet;
#if USEREADERWRITERLOCKSLIM
      bool boolTaken = false;
      try {
        boolTaken = this._objRWLS.TryEnterReadLock(_intTimeout);
#else
    lock (this._objLock) {
#endif
      boolRet = this._arrList.Contains(item);
#if USEREADERWRITERLOCKSLIM
      } finally {
        if (boolTaken)
          this._objRWLS.ExitReadLock();
#endif
    }

    return boolRet;
  }

  public void CopyTo(T[] array, int arrayIndex) {
#if USEREADERWRITERLOCKSLIM
      bool boolTaken = false;
      try {
        boolTaken = this._objRWLS.TryEnterReadLock(_intTimeout);
#else
    lock (this._objLock) {
#endif
      this._arrList.CopyTo(array, arrayIndex);
#if USEREADERWRITERLOCKSLIM
      } finally {
        if (boolTaken)
          this._objRWLS.ExitReadLock();
#endif
    }
  }

  public int Count {
    get {
      int intRet;
#if USEREADERWRITERLOCKSLIM
        bool boolTaken = false;
        try {
          boolTaken = this._objRWLS.TryEnterReadLock(_intTimeout);
#else
      lock (this._objLock) {
#endif
        intRet = this._arrList.Count;
#if USEREADERWRITERLOCKSLIM
        } finally {
          if (boolTaken)
            this._objRWLS.ExitReadLock();
#endif
      }

      return intRet;
    }
  }

  public bool IsReadOnly => false;

  public bool Remove(T item) {
    bool boolRet;
    if (this.Dispatcher.CheckAccess()) {
#if USEREADERWRITERLOCKSLIM
        bool boolTaken = false;
        try {
          boolTaken = this._objRWLS.TryEnterWriteLock(_intTimeout);
#else
      lock (this._objLock) {
#endif
        var index = this._arrList.IndexOf(item);
        boolRet = this._arrList.Remove(item);
        this._voidTriggerCollectionChanged(new(NotifyCollectionChangedAction.Remove, item, index));
#if USEREADERWRITERLOCKSLIM
        } finally {
          if (boolTaken)
            this._objRWLS.ExitWriteLock();
#endif
      }
    } else
      boolRet = (bool)this.Dispatcher.Invoke(new Func<T, bool>(this.Remove), DispatcherPriority.Send, item);

    return boolRet;
  }

  public IEnumerator<T> GetEnumerator() {
    IEnumerator<T> objRet;
#if USEREADERWRITERLOCKSLIM
      bool boolTaken = false;
      try {
        boolTaken = this._objRWLS.TryEnterReadLock(_intTimeout);
#else
    lock (this._objLock) {
#endif
      objRet = this._arrList.GetEnumerator();
#if USEREADERWRITERLOCKSLIM
      } finally {
        this._objRWLS.ExitReadLock();
#endif
    }

    return objRet;
  }

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  #region IList Members

  public int Add(object item) {
    int intRet;
    if (this.Dispatcher.CheckAccess()) {
#if USEREADERWRITERLOCKSLIM
        bool boolTaken = false;
        try {
          boolTaken = this._objRWLS.TryEnterWriteLock(_intTimeout);
#else
      lock (this._objLock) {
#endif
        this._arrList.Add((T)item);
        intRet = this._arrList.Count;
        this._voidTriggerCollectionChanged(new(NotifyCollectionChangedAction.Add, item));
#if USEREADERWRITERLOCKSLIM
        } finally {
          if (boolTaken)
            this._objRWLS.ExitWriteLock();
#endif
      }
    } else
      intRet = (int)this.Dispatcher.Invoke(new Action<T>(this.Add), DispatcherPriority.Send, item);

    return intRet;
  }

  public bool Contains(object value) => this.Contains((T)value);

  public int IndexOf(object value) => this.IndexOf((T)value);

  public void Insert(int index, object value) => this.Insert(index, (T)value);

  public bool IsFixedSize => false;

  public void Remove(object value) => this.Remove((T)value);

  object IList.this[int index] {
    get => this[index];
    set => this[index] = (T)value;
  }

  #endregion

  #region ICollection Members

  public void CopyTo(Array array, int index) {
#if USEREADERWRITERLOCKSLIM
      bool boolTaken = false;
      try {
        boolTaken = this._objRWLS.TryEnterReadLock(_intTimeout);
#else
    lock (this._objLock) {
#endif
      ((ICollection)this._arrList).CopyTo(array, index);
#if USEREADERWRITERLOCKSLIM
      } finally {
        if (boolTaken)
          this._objRWLS.ExitReadLock();
#endif
    }
  }

  public bool IsSynchronized => false;

  public object SyncRoot => null;

  #endregion

  public T[] ToArray() {
    T[] arrRet;
#if USEREADERWRITERLOCKSLIM
      bool boolTaken = false;
      try {
        boolTaken = this._objRWLS.TryEnterReadLock(_intTimeout);
#else
    lock (this._objLock) {
#endif
      arrRet = this._arrList.ToArray();
#if USEREADERWRITERLOCKSLIM
      } finally {
        if (boolTaken)
          this._objRWLS.ExitReadLock();
#endif
    }

    return arrRet;
  }
}
