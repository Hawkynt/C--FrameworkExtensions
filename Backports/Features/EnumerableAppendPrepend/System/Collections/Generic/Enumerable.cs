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

#if !SUPPORTS_ENUMERABLE_APPENDPREPEND

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Linq;

public static partial class EnumerablePolyfills {

  extension<TItem>(IEnumerable<TItem> @this) {

    /// <summary>
    ///   Appends a single item to the beginning of the <see cref="IEnumerable{T}" />.
    /// </summary>
    /// <param name="item">The item to prepend</param>
    /// <returns>A new <see cref="IEnumerable{T}" /> with the added item</returns>
    /// <exception cref="ArgumentNullException">
    ///   When the given <see cref="IEnumerable{T}" /> is <see langword="null" />
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TItem> Prepend(TItem item) {
      ArgumentNullException.ThrowIfNull(@this);

      return Invoke(@this, item);

      static IEnumerable<TItem> Invoke(IEnumerable<TItem> source, TItem item) {
        yield return item;

        foreach (var i in source)
          yield return i;
      }
    }

    /// <summary>
    ///   Appends a single item to the end of the <see cref="IEnumerable{T}" />.
    /// </summary>
    /// <param name="item">The item to append</param>
    /// <returns>A new <see cref="IEnumerable{T}" /> with the added item</returns>
    /// <exception cref="ArgumentNullException">
    ///   When the given <see cref="IEnumerable{T}" /> is <see langword="null" />
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<TItem> Append(TItem item) {
      ArgumentNullException.ThrowIfNull(@this);

      return Invoke(@this, item);

      static IEnumerable<TItem> Invoke(IEnumerable<TItem> source, TItem item) {
        foreach (var i in source)
          yield return i;

        yield return item;
      }
    }

  }

}

#endif
