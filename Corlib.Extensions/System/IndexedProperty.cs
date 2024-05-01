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

namespace System;

#region one dimensional index
/// <summary>
/// A property that has an indexer.
/// </summary>
/// <typeparam name="TIndexer">The type of the indexer.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>

public class IndexedProperty<TIndexer, TResult> {

  private readonly Func<TIndexer, TResult> _getter;
  private readonly Action<TIndexer, TResult> _setter;
  public IndexedProperty(Func<TIndexer, TResult> getter, Action<TIndexer, TResult> setter = null) {
    this._getter = getter;
    this._setter = setter;
  }

  public TResult this[TIndexer index] {
    get {
      if (this._getter == null)
        throw new NotSupportedException("Has no getter");

      return (this._getter(index));
    }
    set {
      if (this._setter == null)
        throw new NotSupportedException("Has no setter");

      this._setter(index, value);
    }
  }
}

/// <summary>
/// A property that has an indexer.
/// </summary>
/// <typeparam name="TIndexer">The type of the indexer.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public class ReadOnlyIndexedProperty<TIndexer, TResult> {

  private readonly Func<TIndexer, TResult> _getter;
  public ReadOnlyIndexedProperty(Func<TIndexer, TResult> getter) => this._getter = getter;
  public TResult this[TIndexer index] => (this._getter(index));
}

/// <summary>
/// A property that has an indexer.
/// </summary>
/// <typeparam name="TIndexer">The type of the indexer.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public class WriteOnlyIndexedProperty<TIndexer, TResult> {

  private readonly Action<TIndexer, TResult> _setter;
  public WriteOnlyIndexedProperty(Action<TIndexer, TResult> setter) => this._setter = setter;

  public TResult this[TIndexer index] {
    set => this._setter(index, value);
  }
}
#endregion

public class IndexedProperty<TIndexer, TIndexer2, TResult> {
  private readonly Func<TIndexer, TIndexer2, TResult> _getter;
  private readonly Action<TIndexer, TIndexer2, TResult> _setter;
  public IndexedProperty(Func<TIndexer, TIndexer2, TResult> getter, Action<TIndexer, TIndexer2, TResult> setter) {
    this._getter = getter;
    this._setter = setter;
  }

  public TResult this[TIndexer index, TIndexer2 index2] {
    get => this._getter(index, index2);
    set => this._setter(index, index2, value);
  }
}

public class IndexedProperty<TIndexer, TIndexer2, TIndexer3, TResult> {
  private readonly Func<TIndexer, TIndexer2, TIndexer3, TResult> _getter;
  private readonly Action<TIndexer, TIndexer2, TIndexer3, TResult> _setter;
  
  public IndexedProperty(Func<TIndexer, TIndexer2, TIndexer3, TResult> getter, Action<TIndexer, TIndexer2, TIndexer3, TResult> setter) {
    this._getter = getter;
    this._setter = setter;
  }

  public TResult this[TIndexer index, TIndexer2 index2, TIndexer3 index3] {
    get => (this._getter(index, index2, index3));
    set => this._setter(index, index2, index3, value);
  }
}