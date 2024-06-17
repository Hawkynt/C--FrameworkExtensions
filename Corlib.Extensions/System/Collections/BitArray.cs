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

using System.Collections.Generic;
using Guard;

namespace System.Collections;

public static partial class BitArrayExtensions {
  /// <summary>
  ///   Get the set bit positions.
  /// </summary>
  /// <param name="this">This <see cref="BitArray" /></param>
  /// <returns>An enumeration of indexes.</returns>
  public static IEnumerable<int> GetSetBits(this BitArray @this) {
    Against.ThisIsNull(@this);

    return Invoke(@this);

    static IEnumerable<int> Invoke(BitArray @this) {
      for (var i = 0; i < @this.Length; ++i)
        if (@this[i])
          yield return i;
    }
  }

  /// <summary>
  ///   Get the unset bit positions.
  /// </summary>
  /// <param name="this">This <see cref="BitArray" /></param>
  /// <returns>An enumeration of indexes.</returns>
  public static IEnumerable<int> GetUnsetBits(this BitArray @this) {
    Against.ThisIsNull(@this);

    return Invoke(@this);

    static IEnumerable<int> Invoke(BitArray @this) {
      for (var i = 0; i < @this.Length; ++i)
        if (!@this[i])
          yield return i;
    }
  }
}
