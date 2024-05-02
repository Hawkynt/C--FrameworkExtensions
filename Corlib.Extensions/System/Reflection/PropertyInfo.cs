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

using Guard;

namespace System.Reflection;

public static partial class PropertyInfoExtensions {
  /// <summary>
  /// Tries the set value.
  /// </summary>
  /// <param name="this">This PropertyInfo.</param>
  /// <param name="instance">The instance.</param>
  /// <param name="value">The value.</param>
  /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
  public static bool TrySetValue(this PropertyInfo @this, object instance, object value) {
    Against.ThisIsNull(@this);

    try {
      @this.SetValue(instance, value, null);
      return true;
    } catch {
      return false;
    }
  }
  /// <summary>
  /// Tries the set value.
  /// </summary>
  /// <param name="this">This PropertyInfo.</param>
  /// <param name="value">The value.</param>
  /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
  public static bool TrySetValue(this PropertyInfo @this, object value) {
    Against.ThisIsNull(@this);

    return @this.TrySetValue(null, value);
  }

  /// <summary>
  /// Gets the value or a default.
  /// </summary>
  /// <param name="this">This PropertyInfo.</param>
  /// <param name="value">The value.</param>
  /// <param name="index">The index.</param>
  /// <param name="defaultValue">The default value.</param>
  /// <returns>The value of the property from the given object or the default value.</returns>
  public static object GetValueOrDefault(this PropertyInfo @this, object value, object[] index = null, object defaultValue = null) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(value);

    try {
      return @this.GetValue(value, index);
    } catch {
      return defaultValue;
    }
  }
}