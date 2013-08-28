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
using System.Linq;

namespace System.Reflection {
  internal static class MemberInfoExtensions {

    /// <summary>
    /// Gets the custom attribute.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    /// <param name="This">This MemberInfo.</param>
    /// <param name="inherit">if set to <c>true</c> inherits attributes.</param>
    /// <returns>The attribute if present; otherwise, throws exception.</returns>
    public static TAttribute GetCustomAttribute<TAttribute>(this MemberInfo This, bool inherit = true) where TAttribute : Attribute {
      Contract.Requires(This != null);
      TAttribute result;
      if (!This.TryGetCustomAttribute(out result, inherit))
        throw new NullReferenceException();
      return (result);
    }

    /// <summary>
    /// Gets the custom attribute's value.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="This">This MemberInfo.</param>
    /// <param name="valueGetter">The value getter.</param>
    /// <param name="inherit">if set to <c>true</c> inherits attributes.</param>
    /// <returns>
    /// The attribute's value if present; otherwise, throws exception.
    /// </returns>
    public static TValue GetCustomAttributeValue<TAttribute, TValue>(this MemberInfo This, Func<TAttribute, TValue> valueGetter, bool inherit = true) where TAttribute : Attribute {
      Contract.Requires(This != null);
      Contract.Requires(valueGetter != null);
      return (valueGetter(This.GetCustomAttribute<TAttribute>()));
    }

    /// <summary>
    /// Gets the custom attribute or a default value.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    /// <param name="This">This MemberInfo.</param>
    /// <param name="inherit">if set to <c>true</c> inherits attributes.</param>
    /// <returns>The attribute if present; otherwise, the default value.</returns>
    public static TAttribute GetCustomAttributeOrDefault<TAttribute>(this MemberInfo This, TAttribute defaultValue = default(TAttribute), bool inherit = true) where TAttribute : Attribute {
      Contract.Requires(This != null);
      TAttribute result;
      return This.TryGetCustomAttribute(out result, inherit) ? result : defaultValue;
    }

    /// <summary>
    /// Gets the custom attribute's value or a default value.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="This">This MemberInfo.</param>
    /// <param name="valueGetter">The value getter.</param>
    /// <param name="inherit">if set to <c>true</c> inherits attributes.</param>
    /// <returns>
    /// The attribute's value if present; otherwise, the default value.
    /// </returns>
    public static TValue GetCustomAttributeValueOrDefault<TAttribute, TValue>(this MemberInfo This, Func<TAttribute, TValue> valueGetter, TValue defaultValue = default(TValue), bool inherit = true) where TAttribute : Attribute {
      Contract.Requires(This != null);
      Contract.Requires(valueGetter != null);
      TAttribute result;
      return This.TryGetCustomAttribute(out result, inherit) ? valueGetter(result) : defaultValue;
    }

    /// <summary>
    /// Gets the custom attribute or generates a default value.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    /// <param name="This">This MemberInfo.</param>
    /// <param name="inherit">if set to <c>true</c> inherits attributes.</param>
    /// <returns>The attribute if present; otherwise, the generated default value.</returns>
    public static TAttribute GetCustomAttributeOrDefault<TAttribute>(this MemberInfo This, Func<TAttribute> defaultValueFactory, bool inherit = true) where TAttribute : Attribute {
      Contract.Requires(This != null);
      Contract.Requires(defaultValueFactory != null);
      TAttribute result;
      return This.TryGetCustomAttribute(out result, inherit) ? result : defaultValueFactory();
    }

    /// <summary>
    /// Gets the custom attribute's value or a constructs a default value.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="This">This MemberInfo.</param>
    /// <param name="valueGetter">The value getter.</param>
    /// <param name="defaultValueFactory">The default value factory.</param>
    /// <param name="inherit">if set to <c>true</c> inherits attributes.</param>
    /// <returns>
    /// The attribute's value if present; otherwise, the default value.
    /// </returns>
    public static TValue GetCustomAttributeValueOrDefault<TAttribute, TValue>(this MemberInfo This, Func<TAttribute, TValue> valueGetter, Func<TValue> defaultValueFactory, bool inherit = true) where TAttribute : Attribute {
      Contract.Requires(This != null);
      Contract.Requires(valueGetter != null);
      Contract.Requires(defaultValueFactory != null);
      TAttribute result;
      return This.TryGetCustomAttribute(out result, inherit) ? valueGetter(result) : defaultValueFactory();
    }

    /// <summary>
    /// Tries to get the custom attribute.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    /// <param name="This">This MemberInfo.</param>
    /// <param name="result">The result.</param>
    /// <param name="inherit">if set to <c>true</c> inherits attributes.</param>
    /// <returns><c>true</c> if the given attribute was present; otherwise, <c>false</c>.</returns>
    public static bool TryGetCustomAttribute<TAttribute>(this MemberInfo This, out TAttribute result, bool inherit = true) where TAttribute : Attribute {
      Contract.Requires(This != null);
      var results = This.GetCustomAttributes<TAttribute>(inherit);
      if (results.Length > 0) {
        result = results[0];
        return (true);
      }

      result = default(TAttribute);
      return (false);
    }

    /// <summary>
    /// Gets the custom attributes.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    /// <param name="This">This MemberInfo.</param>
    /// <param name="inherit">if set to <c>true</c> inherits attributes.</param>
    /// <returns>The custom attributes of the given type.</returns>
    public static TAttribute[] GetCustomAttributes<TAttribute>(this MemberInfo This, bool inherit = true) where TAttribute : Attribute {
      Contract.Requires(This != null);
      return (This.GetCustomAttributes(typeof(TAttribute), inherit).Cast<TAttribute>().ToArray());
    }
  }
}