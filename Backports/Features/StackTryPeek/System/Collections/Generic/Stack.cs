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

#if !SUPPORTS_STACK_TRYPEEK

namespace System.Collections.Generic;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
public static partial class StackPolyfills {

  /// <summary>
  /// Returns a value that indicates whether there is an object at the top of the <see cref="Stack{T}"/>, and if one is present, copies it to the <paramref name="result"/> parameter. The object is not removed from the <see cref="Stack{T}"/>.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This <see cref="Stack{T}"/></param>
  /// <param name="result">If present, the object at the top of the <see cref="Stack{T}"/>; otherwise, the default value of T.</param>
  /// <returns><see langword="true"/> if there is an object at the top of the <see cref="Stack{T}"/>; <see langword="false"/> if the <see cref="Stack{T}"/> is empty.</returns>
  public static bool TryPeek<TItem>(this Stack<TItem> @this, out TItem result) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));
  
    if (@this.Count < 1) {
      result = default;
      return false;
    }

    result = @this.Pop();
    return true;
  }

}

#endif
