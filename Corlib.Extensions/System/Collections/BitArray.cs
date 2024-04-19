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

using System.Collections.Generic;
using Guard;
// ReSharper disable PartialTypeWithSinglePart

namespace System.Collections; 

#if COMPILE_TO_EXTENSION_DLL
public
#else
internal
#endif
static partial class BitArrayExtensions {

  /// <summary>
  /// Get the set bit positions.
  /// </summary>
  /// <param name="this">This <see cref="BitArray"/></param>
  /// <returns>An enumeration of indexes.</returns>
  public static IEnumerable<int> GetSetBits(this BitArray @this) {
    Against.ThisIsNull(@this);

    return Invoke(@this);
    
    static IEnumerable<int> Invoke(BitArray @this) {
      for (var i = 0; i < @this.Length; ++i) {
        if (@this[i])
          yield return i;
      }
    }
  }

  /// <summary>
  /// Get the unset bit positions.
  /// </summary>
  /// <param name="this">This <see cref="BitArray"/></param>
  /// <returns>An enumeration of indexes.</returns>
  public static IEnumerable<int> GetUnsetBits(this BitArray @this) {
    Against.ThisIsNull(@this);

    return Invoke(@this);
    
    static IEnumerable<int> Invoke(BitArray @this) {
      for (var i = 0; i < @this.Length; ++i) {
        if (!@this[i])
          yield return i;
      }
    }
  }

}