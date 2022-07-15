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

using System.Diagnostics;

namespace System.Collections.Specialized {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class StringDictionaryExtensions {
    /// <summary>
    /// Adds or updates the specified key.
    /// </summary>
    /// <param name="This">This StringDictionary.</param>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    public static void AddOrUpdate(this StringDictionary This, string key, string value) {
      Debug.Assert(This != null);
      Debug.Assert(key != null);
      if (This.ContainsKey(key))
        This[key] = value;
      else
        This.Add(key, value);
    }
  }
}