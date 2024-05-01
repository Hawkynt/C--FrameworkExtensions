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
#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif

namespace System;

public static partial class EnumExtensions {

  /// <summary>
  /// Gets the field description.
  /// </summary>
  /// <typeparam name="TEnum">The type of the enum.</typeparam>
  /// <param name="field">The field.</param>
  /// <returns>The content of the description attribute or <c>null</c>.</returns>
  public static string GetFieldDescription<TEnum>(this TEnum field) where TEnum : struct
    => GetFieldAttribute<TEnum, DescriptionAttribute>(field)?.Description
  ;

  /// <summary>
  /// Gets the field description.
  /// </summary>
  /// <typeparam name="TEnum">The type of the enum.</typeparam>
  /// <param name="field">The field.</param>
  /// <returns>The content of the description attribute or <c>null</c>.</returns>
  public static string GetFieldDisplayName<TEnum>(this TEnum field) where TEnum : struct
    => GetFieldAttribute<TEnum, DisplayNameAttribute>(field)?.DisplayName
  ;

  /// <summary>
  /// Gets the field description.
  /// </summary>
  /// <typeparam name="TEnum">The type of the enum.</typeparam>
  /// <param name="field">The field.</param>
  /// <returns>The content of the description attribute or <c>null</c>.</returns>
  public static string GetFieldDisplayNameOrDefault<TEnum>(this TEnum field) where TEnum : struct
    => GetFieldAttribute<TEnum, DisplayNameAttribute>(field)?.DisplayName ?? field.ToString()
  ;
      
  /// <summary>
  /// Gets the attribute of an enumeration field.
  /// </summary>
  /// <typeparam name="TEnum">The type of the enum.</typeparam>
  /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
  /// <param name="field">The field.</param>
  /// <returns>The attribute or <c>null</c>.</returns>
  public static TAttribute GetFieldAttribute<TEnum, TAttribute>(this TEnum field)
    where TEnum : struct
    where TAttribute : Attribute {

    var type = field.GetType();
#if SUPPORTS_CONTRACTS
    Contract.Assert(type.IsEnum, "Only supported on enumerations");
#endif

    return
      (TAttribute)type
        .GetField(field.ToString())?
        .GetCustomAttributes(typeof(TAttribute), false)
        .FirstOrDefault()
      ;
  }
}