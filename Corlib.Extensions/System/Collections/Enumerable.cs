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

using System.Linq;

namespace System.Collections; 

#if COMPILE_TO_EXTENSION_DLL
public
#else
internal
#endif
static partial class EnumerableExtensions {

  /// <summary>
  /// Counts the elements.
  /// </summary>
  /// <param name="this">The collection.</param>
  /// <returns>the number of elements</returns>
  public static int Count(this IEnumerable @this) {
    Guard.Against.ThisIsNull(@this);

    return Enumerable.Count(@this.Cast<object>());
  }

  /// <summary>
  /// Executes a callback for each item
  /// </summary>
  /// <typeparam name="TIn">The type of the items.</typeparam>
  /// <param name="this">The collection.</param>
  /// <param name="action">The call to execute.</param>
  public static void ForEach<TIn>(this IEnumerable @this, Action<TIn> action) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(action);

    foreach (var item in @this)
      action((TIn)item);
  }

  /// <summary>
  /// Converts all.
  /// </summary>
  /// <typeparam name="TIn">The type of the input.</typeparam>
  /// <typeparam name="TOut">The type of the output.</typeparam>
  /// <param name="this">The collection to convert.</param>
  /// <param name="converter">The conversion function.</param>
  /// <returns></returns>
  public static IEnumerable ConvertAll<TIn, TOut>(this IEnumerable @this, Func<TIn, TOut> converter) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(converter);

    // ReSharper disable LoopCanBeConvertedToQuery
    foreach (var item in @this)
      // ReSharper restore LoopCanBeConvertedToQuery
      yield return converter((TIn)item);
  }

  /// <summary>
  /// Converts a non-generic enumeration into an array of objects.
  /// </summary>
  /// <param name="this">This enumeration..</param>
  /// <returns>The array containing the elements.</returns>
  public static object[] ToObjectArray(this IEnumerable @this) => @this?.Cast<object>().ToArray();

  #region linq

  /// <summary>
  /// Determines whether elements are present or not.
  /// </summary>
  /// <param name="this">This enumeration.</param>
  /// <returns><c>true</c> if there is at least one element; otherwise, <c>false</c>.</returns>
  public static bool Any(this IEnumerable @this) {
    Guard.Against.ThisIsNull(@this);

    return @this.GetEnumerator().MoveNext();
  }

  #endregion
}