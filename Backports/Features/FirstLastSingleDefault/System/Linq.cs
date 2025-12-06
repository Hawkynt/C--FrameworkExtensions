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

#if !SUPPORTS_FIRSTLASTSINGLE_DEFAULT

using Guard;
using System.Collections.Generic;

namespace System.Linq;

public static partial class EnumerablePolyfills {

  extension<TSource>(IEnumerable<TSource> @this) {

    public TSource FirstOrDefault(TSource defaultValue) {
      ArgumentNullException.ThrowIfNull(@this);

      foreach (var item in @this)
        return item;

      return defaultValue;
    }

    public TSource FirstOrDefault(Func<TSource, bool> predicate, TSource defaultValue) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(predicate);

      foreach (var item in @this)
        if (predicate(item))
          return item;

      return defaultValue;
    }

    public TSource SingleOrDefault(TSource defaultValue) {
      ArgumentNullException.ThrowIfNull(@this);

      var result = defaultValue;
      var found = false;
      foreach (var item in @this) {
        if (found)
          AlwaysThrow.InvalidOperationException("Sequence contains more than one element");

        result = item;
        found = true;
      }

      return found ? result : defaultValue;
    }

    public TSource SingleOrDefault(Func<TSource, bool> predicate, TSource defaultValue) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(predicate);

      var result = defaultValue;
      var found = false;
      foreach (var item in @this) {
        if (!predicate(item))
          continue;

        if (found)
          AlwaysThrow.InvalidOperationException("Sequence contains more than one element");

        result = item;
        found = true;
      }

      return found ? result : defaultValue;
    }

    public TSource LastOrDefault(TSource defaultValue) {
      ArgumentNullException.ThrowIfNull(@this);

      var result = defaultValue;
      foreach (var item in @this)
        result = item;

      return result;
    }

    public TSource LastOrDefault(Func<TSource, bool> predicate, TSource defaultValue) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(predicate);

      var result = defaultValue;
      foreach (var item in @this)
        if (predicate(item))
          result = item;

      return result;
    }

  }

}

#endif
