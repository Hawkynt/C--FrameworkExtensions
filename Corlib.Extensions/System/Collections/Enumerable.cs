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

using System.Linq;
using Guard;

namespace System.Collections;

public static partial class EnumerableExtensions {
  /// <summary>
  ///   Counts the elements.
  /// </summary>
  /// <param name="this">The collection.</param>
  /// <returns>the number of elements</returns>
  public static int Count(this IEnumerable @this) {
    Against.ThisIsNull(@this);

    return Enumerable.Count(@this.Cast<object>());
  }

  /// <summary>
  ///   Executes a callback for each item
  /// </summary>
  /// <typeparam name="TIn">The type of the items.</typeparam>
  /// <param name="this">The collection.</param>
  /// <param name="action">The call to execute.</param>
  public static void ForEach<TIn>(this IEnumerable @this, Action<TIn> action) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(action);

    foreach (var item in @this)
      action((TIn)item);
  }

  /// <summary>
  ///   Converts all.
  /// </summary>
  /// <typeparam name="TIn">The type of the input.</typeparam>
  /// <typeparam name="TOut">The type of the output.</typeparam>
  /// <param name="this">The collection to convert.</param>
  /// <param name="converter">The conversion function.</param>
  /// <returns></returns>
  public static IEnumerable ConvertAll<TIn, TOut>(this IEnumerable @this, Func<TIn, TOut> converter) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(converter);

    // ReSharper disable LoopCanBeConvertedToQuery
    foreach (var item in @this)
      // ReSharper restore LoopCanBeConvertedToQuery
      yield return converter((TIn)item);
  }

  /// <summary>
  ///   Converts a non-generic enumeration into an array of objects.
  /// </summary>
  /// <param name="this">This enumeration..</param>
  /// <returns>The array containing the elements.</returns>
  public static object[] ToObjectArray(this IEnumerable @this) => @this?.Cast<object>().ToArray();

  #region linq

  /// <summary>
  ///   Determines whether elements are present or not.
  /// </summary>
  /// <param name="this">This enumeration.</param>
  /// <returns><c>true</c> if there is at least one element; otherwise, <c>false</c>.</returns>
  public static bool Any(this IEnumerable @this) {
    Against.ThisIsNull(@this);

    IEnumerator enumerator = null;
    try {
      enumerator = @this.GetEnumerator();
      return enumerator.MoveNext();
    } finally {
      (enumerator as IDisposable)?.Dispose();
    }
  }

  #endregion
}
