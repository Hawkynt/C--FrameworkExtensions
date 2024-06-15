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

using Guard;

namespace System.Collections.Generic;

public static partial class EnumeratorExtensions {
  /// <summary>
  ///   Takes the specified amount of elements.
  /// </summary>
  /// <typeparam name="TValue">The type of the value.</typeparam>
  /// <param name="this">This Enumerator.</param>
  /// <param name="count">The number of values to take.</param>
  /// <returns>An enumeration of values</returns>
  public static IEnumerable<TValue> Take<TValue>(this IEnumerator<TValue> @this, int count) {
    Against.ThisIsNull(@this);

    return Invoke(@this, count);

    static IEnumerable<TValue> Invoke(IEnumerator<TValue> @this, int count) {
      for (var i = 0; i < count && @this.MoveNext(); ++i)
        yield return @this.Current;
    }
  }

  /// <summary>
  ///   Gets the next element from the enumeration.
  /// </summary>
  /// <typeparam name="TValue">The type of the value.</typeparam>
  /// <param name="this">This Enumeration.</param>
  /// <returns></returns>
  /// <exception cref="System.IndexOutOfRangeException">Enumeration ended</exception>
  public static TValue Next<TValue>(this IEnumerator<TValue> @this) {
    Against.ThisIsNull(@this);

    if (@this.MoveNext())
      return @this.Current;

    AlwaysThrow.IndexOutOfRangeException();
    return default;
  }
}
