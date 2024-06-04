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

#if !SUPPORTS_FIRSTLASTSINGLE_DEFAULT

using System.Collections.Generic;

namespace System.Linq;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
public static partial class EnumerablePolyfills {
  
  public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> @this, TSource defaultValue) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));

    foreach (var item in @this)
      return item;

    return defaultValue;
  }
  
  public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> @this, Func<TSource, bool> predicate, TSource defaultValue) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));
    if (predicate == null)
      throw new ArgumentNullException(nameof(predicate));

    foreach (var item in @this)
      if (predicate(item))
        return item;

    return defaultValue;
  }

  public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> @this, TSource defaultValue) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));

    var result = defaultValue;
    var found = false;
    foreach (var item in @this) {
      if (found)
        throw new InvalidOperationException("Sequence contains more than one element");

      result = item;
      found = true;
    }

    if (!found)
      return defaultValue;

    return result;
  }

  public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> @this, Func<TSource, bool> predicate, TSource defaultValue) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));
    if (predicate == null)
      throw new ArgumentNullException(nameof(predicate));

    var result = defaultValue;
    var found = false;
    foreach (var item in @this) {
      if (!predicate(item))
        continue;

      if (found)
        throw new InvalidOperationException("Sequence contains more than one element");

      result = item;
      found = true;
    }

    if (!found)
      return defaultValue;

    return result;
  }


  public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> @this, TSource defaultValue) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));

    var result = defaultValue;
    foreach (var item in @this)
      result = item;

    return result;
  }

  public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> @this, Func<TSource, bool> predicate, TSource defaultValue) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));
    if (predicate == null)
      throw new ArgumentNullException(nameof(predicate));

    var result = defaultValue;
    foreach (var item in @this)
      if (predicate(item))
        result = item;

    return result;
  }

}

#endif

