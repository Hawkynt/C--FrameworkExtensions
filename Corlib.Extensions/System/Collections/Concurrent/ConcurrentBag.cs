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
#if SUPPORTS_CONCURRENT_COLLECTIONS

namespace System.Collections.Concurrent;

#if COMPILE_TO_EXTENSION_DLL
public
#else
internal
#endif
static partial class ConcurrentBagExtensions {
  /// <summary>
  /// Clears the specified Bag.
  /// </summary>
  /// <typeparam name="TValue">The type of the values.</typeparam>
  /// <param name="this">This <see cref="ConcurrentBag{T}"/></param>
  public static void Clear<TValue>(this ConcurrentBag<TValue> @this) {
    Guard.Against.ThisIsNull(@this);

    while (@this.TryTake(out _)) { }
  }
}

#endif