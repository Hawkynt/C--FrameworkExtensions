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

using System.ComponentModel;
using System.Linq;
using Guard;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace System.Reflection;

public static class MemberInfoExtensions {

  /// <summary>
  /// Gets the custom attribute.
  /// </summary>
  /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
  /// <param name="this">This MemberInfo.</param>
  /// <param name="inherit">if set to <c>true</c> inherits attributes.</param>
  /// <param name="inheritInterfaces">if set to <c>true</c> inherits interfaces' attributes.</param>
  /// <returns>The attribute if present; otherwise, throws exception.</returns>
  public static TAttribute GetCustomAttribute<TAttribute>(this MemberInfo @this, bool inherit = true, bool inheritInterfaces = false) where TAttribute : Attribute {
    Against.ThisIsNull(@this);

    if (!TryGetCustomAttribute(@this, out TAttribute result, inherit, inheritInterfaces))
      throw new NullReferenceException();

    return result;
  }

  /// <summary>
  /// Gets the custom attribute's value.
  /// </summary>
  /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
  /// <typeparam name="TValue">The type of the value.</typeparam>
  /// <param name="this">This MemberInfo.</param>
  /// <param name="valueGetter">The value getter.</param>
  /// <param name="inherit">if set to <c>true</c> inherits attributes.</param>
  /// <param name="inheritInterfaces">if set to <c>true</c> inherits interfaces' attributes.</param>
  /// <returns>
  /// The attribute's value if present; otherwise, throws exception.
  /// </returns>
  public static TValue GetCustomAttributeValue<TAttribute, TValue>(this MemberInfo @this, Func<TAttribute, TValue> valueGetter, bool inherit = true, bool inheritInterfaces = false) where TAttribute : Attribute {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(valueGetter);

    return valueGetter(GetCustomAttribute<TAttribute>(@this, inherit, inheritInterfaces));
  }

  /// <summary>
  /// Gets the custom attribute or a default value.
  /// </summary>
  /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
  /// <param name="this">This MemberInfo.</param>
  /// <param name="defaultValue">The used default value</param>
  /// <param name="inherit">if set to <c>true</c> inherits attributes.</param>
  /// <param name="inheritInterfaces">if set to <c>true</c> inherits interfaces' attributes.</param>
  /// <returns>The attribute if present; otherwise, the default value.</returns>
  public static TAttribute GetCustomAttributeOrDefault<TAttribute>(this MemberInfo @this, TAttribute defaultValue = default, bool inherit = true, bool inheritInterfaces = false) where TAttribute : Attribute {
    Against.ThisIsNull(@this);

    return TryGetCustomAttribute(@this, out TAttribute result, inherit, inheritInterfaces) ? result : defaultValue;
  }

  /// <summary>
  /// Gets the custom attribute's value or a default value.
  /// </summary>
  /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
  /// <typeparam name="TValue">The type of the value.</typeparam>
  /// <param name="this">This MemberInfo.</param>
  /// <param name="valueGetter">The value getter.</param>
  /// <param name="inherit">if set to <c>true</c> inherits attributes.</param>
  /// <param name="inheritInterfaces">if set to <c>true</c> inherits interfaces' attributes.</param>
  /// <returns>
  /// The attribute's value if present; otherwise, the default value.
  /// </returns>
  public static TValue GetCustomAttributeValueOrDefault<TAttribute, TValue>(this MemberInfo @this, Func<TAttribute, TValue> valueGetter, TValue defaultValue = default, bool inherit = true, bool inheritInterfaces = false) where TAttribute : Attribute {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(valueGetter);

    return TryGetCustomAttribute(@this, out TAttribute result, inherit, inheritInterfaces) ? valueGetter(result) : defaultValue;
  }

  /// <summary>
  /// Gets the custom attribute or generates a default value.
  /// </summary>
  /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
  /// <param name="this">This MemberInfo.</param>
  /// <param name="inherit">if set to <c>true</c> inherits attributes.</param>
  /// <param name="inheritInterfaces">if set to <c>true</c> inherits interfaces' attributes.</param>
  /// <returns>The attribute if present; otherwise, the generated default value.</returns>
  public static TAttribute GetCustomAttributeOrDefault<TAttribute>(this MemberInfo @this, Func<TAttribute> defaultValueFactory, bool inherit = true, bool inheritInterfaces = false) where TAttribute : Attribute {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(defaultValueFactory);

    return TryGetCustomAttribute(@this, out TAttribute result, inherit, inheritInterfaces) ? result : defaultValueFactory();
  }

  /// <summary>
  /// Gets the custom attribute's value or a constructs a default value.
  /// </summary>
  /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
  /// <typeparam name="TValue">The type of the value.</typeparam>
  /// <param name="this">This MemberInfo.</param>
  /// <param name="valueGetter">The value getter.</param>
  /// <param name="defaultValueFactory">The default value factory.</param>
  /// <param name="inherit">if set to <c>true</c> inherits attributes.</param>
  /// <param name="inheritInterfaces">if set to <c>true</c> inherits interfaces' attributes.</param>
  /// <returns>
  /// The attribute's value if present; otherwise, the default value.
  /// </returns>
  public static TValue GetCustomAttributeValueOrDefault<TAttribute, TValue>(this MemberInfo @this, Func<TAttribute, TValue> valueGetter, Func<TValue> defaultValueFactory, bool inherit = true, bool inheritInterfaces = false) where TAttribute : Attribute {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(valueGetter);
    Against.ArgumentIsNull(defaultValueFactory);

    return TryGetCustomAttribute(@this, out TAttribute result, inherit, inheritInterfaces) ? valueGetter(result) : defaultValueFactory();
  }

  /// <summary>
  /// Tries to get the custom attribute.
  /// </summary>
  /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
  /// <param name="this">This MemberInfo.</param>
  /// <param name="result">The result.</param>
  /// <param name="inherit">if set to <c>true</c> inherits attributes.</param>
  /// <param name="inheritInterfaces">if set to <c>true</c> inherits interfaces' attributes.</param>
  /// <returns><c>true</c> if the given attribute was present; otherwise, <c>false</c>.</returns>
  public static bool TryGetCustomAttribute<TAttribute>(this MemberInfo @this, out TAttribute result, bool inherit = true, bool inheritInterfaces = false) where TAttribute : Attribute {
    Against.ThisIsNull(@this);

    var results = GetCustomAttributes<TAttribute>(@this, inherit, inheritInterfaces);
    if (results.Length > 0) {
      result = results[0];
      return true;
    }

    result = default;
    return false;
  }

  /// <summary>
  /// Gets the custom attributes.
  /// </summary>
  /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
  /// <param name="this">This MemberInfo.</param>
  /// <param name="inherit">if set to <c>true</c> inherits attributes.</param>
  /// <param name="inheritInterfaces">if set to <c>true</c> inherits interfaces' attributes.</param>
  /// <returns>
  /// The custom attributes of the given type.
  /// </returns>
  public static TAttribute[] GetCustomAttributes<TAttribute>(this MemberInfo @this, bool inherit = true, bool inheritInterfaces = false) where TAttribute : Attribute {
    Against.ThisIsNull(@this);

    return @this.GetCustomAttributes(typeof(TAttribute), inherit).Union(inheritInterfaces ? @this.DeclaringType.GetInterfaces().Select(i => i.GetMember(@this.Name).FirstOrDefault()).Where(i => i != null).SelectMany(m => m.GetCustomAttributes(typeof(TAttribute), inherit)) : new object[0]).Cast<TAttribute>().ToArray();
  }

  #region designer-relevant attributes

  /// <summary>
  /// Gets the display name attribute value or a default.
  /// </summary>
  /// <param name="this">This MemberInfo.</param>
  /// <param name="defaultValue">The default value.</param>
  /// <param name="inherit">if set to <c>true</c> inherits attributes.</param>
  /// <param name="inheritInterfaces">if set to <c>true</c> inherits from interfaces also.</param>
  /// <returns>The value of the display name attribute if available or the given default value.</returns>
  public static string GetDisplayNameOrDefault(this MemberInfo @this, string defaultValue = null, bool inherit = true, bool inheritInterfaces = false) => GetCustomAttributeValueOrDefault<DisplayNameAttribute, string>(@this, d => d.DisplayName, defaultValue, inherit, inheritInterfaces);

  /// <summary>
  /// Gets the display name attribute value or a default.
  /// </summary>
  /// <param name="this">This MemberInfo.</param>
  /// <param name="defaultValueFactory">The default value factory.</param>
  /// <param name="inherit">if set to <c>true</c> inherits attributes.</param>
  /// <param name="inheritInterfaces">if set to <c>true</c> inherits from interfaces also.</param>
  /// <returns>The value of the display name attribute if available or the given default value.</returns>
  public static string GetDisplayNameOrDefault(this MemberInfo @this, Func<string> defaultValueFactory, bool inherit = true, bool inheritInterfaces = false) => GetCustomAttributeValueOrDefault<DisplayNameAttribute, string>(@this, d => d.DisplayName, defaultValueFactory, inherit, inheritInterfaces);

  /// <summary>
  /// Gets the description attribute value or a default.
  /// </summary>
  /// <param name="this">This MemberInfo.</param>
  /// <param name="defaultValue">The default value.</param>
  /// <param name="inherit">if set to <c>true</c> inherits attributes.</param>
  /// <param name="inheritInterfaces">if set to <c>true</c> inherits from interfaces also.</param>
  /// <returns>The value of the description attribute if available or the given default value.</returns>
  public static string GetDescriptionOrDefault(this MemberInfo @this, string defaultValue = null, bool inherit = true, bool inheritInterfaces = false) => GetCustomAttributeValueOrDefault<DescriptionAttribute, string>(@this, d => d.Description, defaultValue, inherit, inheritInterfaces);

  /// <summary>
  /// Gets the description attribute value or a default.
  /// </summary>
  /// <param name="this">This MemberInfo.</param>
  /// <param name="defaultValueFactory">The default value factory.</param>
  /// <param name="inherit">if set to <c>true</c> inherits attributes.</param>
  /// <param name="inheritInterfaces">if set to <c>true</c> inherits from interfaces also.</param>
  /// <returns>The value of the description attribute if available or the given default value.</returns>
  public static string GetDescriptionOrDefault(this MemberInfo @this, Func<string> defaultValueFactory, bool inherit = true, bool inheritInterfaces = false) => GetCustomAttributeValueOrDefault<DescriptionAttribute, string>(@this, d => d.Description, defaultValueFactory, inherit, inheritInterfaces);

  /// <summary>
  /// Gets the category attribute value or a default.
  /// </summary>
  /// <param name="this">This MemberInfo.</param>
  /// <param name="defaultValue">The default value.</param>
  /// <param name="inherit">if set to <c>true</c> inherits attributes.</param>
  /// <param name="inheritInterfaces">if set to <c>true</c> inherits from interfaces also.</param>
  /// <returns>The value of the category attribute if available or the given default value.</returns>
  public static string GetCategoryOrDefault(this MemberInfo @this, string defaultValue = null, bool inherit = true, bool inheritInterfaces = false) => GetCustomAttributeValueOrDefault<CategoryAttribute, string>(@this, d => d.Category, defaultValue, inherit, inheritInterfaces);

  /// <summary>
  /// Gets the category attribute value or a default.
  /// </summary>
  /// <param name="this">This MemberInfo.</param>
  /// <param name="defaultValueFactory">The default value factory.</param>
  /// <param name="inherit">if set to <c>true</c> inherits attributes.</param>
  /// <param name="inheritInterfaces">if set to <c>true</c> inherits from interfaces also.</param>
  /// <returns>The value of the category attribute if available or the given default value.</returns>
  public static string GetCategoryOrDefault(this MemberInfo @this, Func<string> defaultValueFactory, bool inherit = true, bool inheritInterfaces = false) => GetCustomAttributeValueOrDefault<CategoryAttribute, string>(@this, d => d.Category, defaultValueFactory, inherit, inheritInterfaces);

  /// <summary>
  /// Gets the browsable attribute value or a default.
  /// </summary>
  /// <param name="this">This MemberInfo.</param>
  /// <param name="defaultValue">The default value.</param>
  /// <param name="inherit">if set to <c>true</c> inherits attributes.</param>
  /// <param name="inheritInterfaces">if set to <c>true</c> inherits from interfaces also.</param>
  /// <returns>The value of the browsable attribute if available or the given default value.</returns>
  public static bool GetBrowsableOrDefault(this MemberInfo @this, bool defaultValue = false, bool inherit = true, bool inheritInterfaces = false) => GetCustomAttributeValueOrDefault<BrowsableAttribute, bool>(@this, d => d.Browsable, defaultValue, inherit, inheritInterfaces);

  /// <summary>
  /// Gets the browsable attribute value or a default.
  /// </summary>
  /// <param name="this">This MemberInfo.</param>
  /// <param name="defaultValueFactory">The default value factory.</param>
  /// <param name="inherit">if set to <c>true</c> inherits attributes.</param>
  /// <param name="inheritInterfaces">if set to <c>true</c> inherits from interfaces also.</param>
  /// <returns>The value of the browsable attribute if available or the given default value.</returns>
  public static bool GetBrowsableOrDefault(this MemberInfo @this, Func<bool> defaultValueFactory, bool inherit = true, bool inheritInterfaces = false) => GetCustomAttributeValueOrDefault<BrowsableAttribute, bool>(@this, d => d.Browsable, defaultValueFactory, inherit, inheritInterfaces);

  /// <summary>
  /// Gets the read-only attribute value or a default.
  /// </summary>
  /// <param name="this">This MemberInfo.</param>
  /// <param name="defaultValue">The default value.</param>
  /// <param name="inherit">if set to <c>true</c> inherits attributes.</param>
  /// <param name="inheritInterfaces">if set to <c>true</c> inherits from interfaces also.</param>
  /// <returns>The value of the read-only attribute if available or the given default value.</returns>
  public static bool GetReadOnlyOrDefault(this MemberInfo @this, bool defaultValue = false, bool inherit = true, bool inheritInterfaces = false) => GetCustomAttributeValueOrDefault<ReadOnlyAttribute, bool>(@this, d => d.IsReadOnly, defaultValue, inherit, inheritInterfaces);

  /// <summary>
  /// Gets the read-only attribute value or a default.
  /// </summary>
  /// <param name="this">This MemberInfo.</param>
  /// <param name="defaultValueFactory">The default value factory.</param>
  /// <param name="inherit">if set to <c>true</c> inherits attributes.</param>
  /// <param name="inheritInterfaces">if set to <c>true</c> inherits from interfaces also.</param>
  /// <returns>The value of the read-only attribute if available or the given default value.</returns>
  public static bool GetReadOnlyOrDefault(this MemberInfo @this, Func<bool> defaultValueFactory, bool inherit = true, bool inheritInterfaces = false) => GetCustomAttributeValueOrDefault<ReadOnlyAttribute, bool>(@this, d => d.IsReadOnly, defaultValueFactory, inherit, inheritInterfaces);

  #endregion
}