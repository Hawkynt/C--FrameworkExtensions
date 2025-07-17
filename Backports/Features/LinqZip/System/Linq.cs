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

#if !SUPPORTS_LINQ_ZIP
using Guard;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Linq;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
public static partial class EnumerablePolyfills {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IEnumerable<TFirst> @this, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector) {
    if (@this == null)
      AlwaysThrow.ArgumentNullException(nameof(@this));
    if (second == null)
      AlwaysThrow.ArgumentNullException(nameof(second));
    if (resultSelector == null)
      AlwaysThrow.ArgumentNullException(nameof(resultSelector));

    return Invoke(@this, second, resultSelector);


    static IEnumerable<TResult> Invoke(IEnumerable<TFirst> @this, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector) {
      using var e1 = @this.GetEnumerator();
      using var e2 = second.GetEnumerator();
      while (e1.MoveNext() && e2.MoveNext())
        yield return resultSelector(e1.Current, e2.Current);
    }
  }

}

#endif
