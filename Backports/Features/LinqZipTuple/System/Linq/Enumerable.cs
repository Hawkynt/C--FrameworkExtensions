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

// Enumerable.Zip tuple overloads added in .NET Core 3.0 / .NET 6.0
#if !SUPPORTS_LINQ_ZIP_TUPLE

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Linq;

public static partial class EnumerablePolyfills {

  extension<TFirst>(IEnumerable<TFirst> @this) {

    /// <summary>
    /// Produces a sequence of tuples with elements from the two specified sequences.
    /// </summary>
    /// <typeparam name="TSecond">The type of the elements of the second input sequence.</typeparam>
    /// <param name="second">The second sequence to merge.</param>
    /// <returns>A sequence of tuples with elements taken from the first and second sequences, in that order.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="second"/> is <see langword="null"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<(TFirst First, TSecond Second)> Zip<TSecond>(IEnumerable<TSecond> second) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(second);

      return Invoke(@this, second);

      static IEnumerable<(TFirst First, TSecond Second)> Invoke(IEnumerable<TFirst> first, IEnumerable<TSecond> second) {
        using var e1 = first.GetEnumerator();
        using var e2 = second.GetEnumerator();
        while (e1.MoveNext() && e2.MoveNext())
          yield return (e1.Current, e2.Current);
      }
    }

    /// <summary>
    /// Produces a sequence of tuples with elements from the three specified sequences.
    /// </summary>
    /// <typeparam name="TSecond">The type of the elements of the second input sequence.</typeparam>
    /// <typeparam name="TThird">The type of the elements of the third input sequence.</typeparam>
    /// <param name="second">The second sequence to merge.</param>
    /// <param name="third">The third sequence to merge.</param>
    /// <returns>A sequence of tuples with elements taken from the first, second, and third sequences, in that order.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="second"/> or <paramref name="third"/> is <see langword="null"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<(TFirst First, TSecond Second, TThird Third)> Zip<TSecond, TThird>(IEnumerable<TSecond> second, IEnumerable<TThird> third) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentNullException.ThrowIfNull(second);
      ArgumentNullException.ThrowIfNull(third);

      return Invoke(@this, second, third);

      static IEnumerable<(TFirst First, TSecond Second, TThird Third)> Invoke(IEnumerable<TFirst> first, IEnumerable<TSecond> second, IEnumerable<TThird> third) {
        using var e1 = first.GetEnumerator();
        using var e2 = second.GetEnumerator();
        using var e3 = third.GetEnumerator();
        while (e1.MoveNext() && e2.MoveNext() && e3.MoveNext())
          yield return (e1.Current, e2.Current, e3.Current);
      }
    }

  }

}

#endif
