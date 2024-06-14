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

using System.ComponentModel;

namespace System;

public static partial class EnumExtensions {
  /// <summary>
  ///   Gets the field description.
  /// </summary>
  /// <typeparam name="TEnum">The type of the enum.</typeparam>
  /// <param name="field">The field.</param>
  /// <returns>The content of the description attribute or <c>null</c>.</returns>
  public static string GetFieldDescription<TEnum>(this TEnum field) where TEnum : Enum
    => GetFieldAttribute<TEnum, DescriptionAttribute>(field)?.Description;

  /// <summary>
  ///   Gets the field description.
  /// </summary>
  /// <typeparam name="TEnum">The type of the enum.</typeparam>
  /// <param name="field">The field.</param>
  /// <returns>The content of the description attribute or <c>null</c>.</returns>
  public static string GetFieldDisplayName<TEnum>(this TEnum field) where TEnum : Enum
    => GetFieldAttribute<TEnum, DisplayNameAttribute>(field)?.DisplayName;

  /// <summary>
  ///   Gets the field description.
  /// </summary>
  /// <typeparam name="TEnum">The type of the enum.</typeparam>
  /// <param name="field">The field.</param>
  /// <returns>The content of the description attribute or <c>null</c>.</returns>
  public static string GetFieldDisplayNameOrDefault<TEnum>(this TEnum field) where TEnum : Enum
    => GetFieldAttribute<TEnum, DisplayNameAttribute>(field)?.DisplayName ?? field.ToString();

  /// <summary>
  ///   Gets the attribute of an enumeration field.
  /// </summary>
  /// <typeparam name="TEnum">The type of the enum.</typeparam>
  /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
  /// <param name="field">The field.</param>
  /// <returns>The attribute or <c>null</c>.</returns>
  public static TAttribute GetFieldAttribute<TEnum, TAttribute>(this TEnum field)
    where TEnum : Enum
    where TAttribute : Attribute
    => (TAttribute)field
      .GetType()
      .GetField(field.ToString())
      ?
      .GetCustomAttributes(typeof(TAttribute), false)
      .FirstOrDefault();
}
