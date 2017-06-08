#region (c)2010-2020 Hawkynt
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

using System.Diagnostics.Contracts;

namespace System.Reflection {
  internal static partial class PropertyInfoExtensions {
    /// <summary>
    /// Tries the set value.
    /// </summary>
    /// <param name="This">This PropertyInfo.</param>
    /// <param name="instance">The instance.</param>
    /// <param name="value">The value.</param>
    /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
    public static bool TrySetValue(this PropertyInfo This, object instance, object value) {
      Contract.Requires(This != null);
      try {
        This.SetValue(instance, value, null);
        return true;
      } catch {
        return false;
      }
    }
    /// <summary>
    /// Tries the set value.
    /// </summary>
    /// <param name="This">This PropertyInfo.</param>
    /// <param name="value">The value.</param>
    /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
    public static bool TrySetValue(this PropertyInfo This, object value) {
      Contract.Requires(This != null);
      return This.TrySetValue(null, value);
    }

    /// <summary>
    /// Gets the value or a default.
    /// </summary>
    /// <param name="This">This PropertyInfo.</param>
    /// <param name="value">The value.</param>
    /// <param name="index">The index.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The value of the property from the given object or the default value.</returns>
    public static object GetValueOrDefault(this PropertyInfo This, object value, object[] index = null, object defaultValue = null) {
      Contract.Requires(This != null);
      Contract.Requires(value != null);
      try {
        return This.GetValue(value, index);
      } catch {
        return defaultValue;
      }
    }
  }
}