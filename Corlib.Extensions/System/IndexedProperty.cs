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

namespace System;

#region one dimensional index

/// <summary>
///   A property that has an indexer.
/// </summary>
/// <typeparam name="TIndexer">The type of the indexer.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public class IndexedProperty<TIndexer, TResult>(Func<TIndexer, TResult> getter, Action<TIndexer, TResult> setter = null) {
  public TResult this[TIndexer index] {
    get {
      if (getter == null)
        throw new NotSupportedException("Has no getter");

      return getter(index);
    }
    set {
      if (setter == null)
        throw new NotSupportedException("Has no setter");

      setter(index, value);
    }
  }
}

/// <summary>
///   A property that has an indexer.
/// </summary>
/// <typeparam name="TIndexer">The type of the indexer.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public class ReadOnlyIndexedProperty<TIndexer, TResult>(Func<TIndexer, TResult> getter) {
  public TResult this[TIndexer index] => getter(index);
}

/// <summary>
///   A property that has an indexer.
/// </summary>
/// <typeparam name="TIndexer">The type of the indexer.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public class WriteOnlyIndexedProperty<TIndexer, TResult>(Action<TIndexer, TResult> setter) {
  public TResult this[TIndexer index] {
    set => setter(index, value);
  }
}

#endregion

public class IndexedProperty<TIndexer, TIndexer2, TResult>(Func<TIndexer, TIndexer2, TResult> getter, Action<TIndexer, TIndexer2, TResult> setter) {
  public TResult this[TIndexer index, TIndexer2 index2] {
    get => getter(index, index2);
    set => setter(index, index2, value);
  }
}

public class IndexedProperty<TIndexer, TIndexer2, TIndexer3, TResult>(Func<TIndexer, TIndexer2, TIndexer3, TResult> getter, Action<TIndexer, TIndexer2, TIndexer3, TResult> setter) {
  public TResult this[TIndexer index, TIndexer2 index2, TIndexer3 index3] {
    get => getter(index, index2, index3);
    set => setter(index, index2, index3, value);
  }
}
