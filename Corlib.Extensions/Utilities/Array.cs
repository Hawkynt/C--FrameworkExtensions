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

#if !SUPPORTS_ARRAY_FILL
using Guard;
#endif

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Utilities;

internal static class Array {
#if !SUPPORTS_ARRAY_EMPTY

  private static class EmptyArray<T> {
    public static readonly T[] Empty = [];
  }

#endif

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T[] Empty<T>()
#if SUPPORTS_ARRAY_EMPTY
    => []
#else
    => EmptyArray<T>.Empty
#endif
  ;

#if SUPPORTS_ARRAY_FILL
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Fill<T>(T[] array, T value) => System.Array.Fill(array, value);
#else
  public static void Fill<T>(T[] array, T value) {
    Against.ArgumentIsNull(array);
    for (var index = 0; index < array.Length; ++index)
      array[index] = value;
  }
#endif

}
