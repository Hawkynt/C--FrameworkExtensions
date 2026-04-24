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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hawkynt.ColorProcessing.SourceGenerators;

/// <summary>
/// Value-equatable wrapper around an immutable array so Roslyn's incremental pipeline
/// can cache generator outputs correctly. A raw T[] compares by reference and defeats caching.
/// </summary>
internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T> where T : IEquatable<T> {

  private readonly T[]? _array;

  public EquatableArray(T[] array) => this._array = array;

  public int Count => this._array?.Length ?? 0;

  public T this[int index] => this._array![index];

  public bool Equals(EquatableArray<T> other) {
    if (this._array is null) return other._array is null || other._array.Length == 0;
    if (other._array is null) return this._array.Length == 0;
    if (this._array.Length != other._array.Length) return false;
    for (var i = 0; i < this._array.Length; ++i)
      if (!EqualityComparer<T>.Default.Equals(this._array[i], other._array[i]))
        return false;
    return true;
  }

  public override bool Equals(object? obj) => obj is EquatableArray<T> other && this.Equals(other);

  public override int GetHashCode() {
    if (this._array is null) return 0;
    var hash = 17;
    unchecked {
      foreach (var item in this._array)
        hash = hash * 31 + (item?.GetHashCode() ?? 0);
    }
    return hash;
  }

  public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)(this._array ?? Array.Empty<T>())).GetEnumerator();

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  public static EquatableArray<T> Empty { get; } = new(Array.Empty<T>());

  public static EquatableArray<T> FromEnumerable(IEnumerable<T> source) => new(source.ToArray());
}
