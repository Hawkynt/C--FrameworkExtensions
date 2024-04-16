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

#if !SUPPORTS_READ_ONLY_COLLECTIONS

namespace System.Collections.ObjectModel;

// ReSharper disable UnusedMember.Global
#if COMPILE_TO_EXTENSION_DLL
public
#else
internal
#endif
interface IReadOnlyDictionary<K, V> : IDictionary<K, V> { }

#if COMPILE_TO_EXTENSION_DLL
public
#else
internal
#endif
class ReadOnlyDictionary<K, V> : Dictionary<K, V>, IReadOnlyDictionary<K, V> {

  public ReadOnlyDictionary(IDictionary<K,V> dictionary) : base(dictionary) {}
}
#endif