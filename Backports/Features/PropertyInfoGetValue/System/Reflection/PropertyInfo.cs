#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
//
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
//
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY without even the implied
// warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the LICENSE file for more details.
//
// You should have received a copy of the License along with Hawkynt's
// .NET Framework extensions. If not, see
// <https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/LICENSE>.

#endregion

#if !SUPPORTS_PROPERTYINFO_GETVALUE_SINGLE

namespace System.Reflection;

/// <summary>
/// Provides extension methods for <see cref="PropertyInfo"/> to backport
/// the single-parameter <c>GetValue</c> overload added in .NET 4.5.
/// </summary>
public static class PropertyInfoPolyfills {
  /// <param name="this">The property info.</param>
  extension(PropertyInfo @this) {
    /// <summary>
    /// Gets the value of the property for the specified object.
    /// </summary>
    /// <param name="obj">The object whose property value will be returned.</param>
    /// <returns>The property value for the <paramref name="obj"/> parameter.</returns>
    /// <remarks>
    /// This is a polyfill for the <c>PropertyInfo.GetValue(object)</c> overload
    /// that was added in .NET Framework 4.5.
    /// </remarks>
    public object GetValue(object obj)
      => @this.GetValue(obj, null);

    /// <summary>
    /// Sets the value of the property for the specified object.
    /// </summary>
    /// <param name="obj">The object whose property value will be set.</param>
    /// <param name="value">The new property value.</param>
    /// <remarks>
    /// This is a polyfill for the <c>PropertyInfo.SetValue(object, object)</c> overload
    /// that was added in .NET Framework 4.5.
    /// </remarks>
    public void SetValue(object obj, object value)
      => @this.SetValue(obj, value, null);
  }
}

#endif
