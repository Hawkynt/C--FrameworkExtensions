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

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace System.Collections.Generic {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class EnumeratorExtensions {

    /// <summary>
    /// Takes the specified amount of elements.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="this">This Enumerator.</param>
    /// <param name="count">The number of values to take.</param>
    /// <returns>An enumeration of values</returns>
    public static IEnumerable<TValue> Take<TValue>(this IEnumerator<TValue> @this, int count) {
      for (var i = 0; i < count && @this.MoveNext(); ++i)
        yield return @this.Current;
    }

    /// <summary>
    /// Gets the next element from the enumeration.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="this">This Enumeration.</param>
    /// <returns></returns>
    /// <exception cref="System.IndexOutOfRangeException">Enumeration ended</exception>
    public static TValue Next<TValue>(this IEnumerator<TValue> @this) {
      if (@this.MoveNext())
        return @this.Current;

      throw new IndexOutOfRangeException("Enumeration ended");
    }

  }
}