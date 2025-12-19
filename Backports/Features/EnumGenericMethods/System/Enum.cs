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

#if !SUPPORTS_ENUM_GENERIC_GETVALUES

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class EnumPolyfills {

  extension(Enum) {

    /// <summary>
    /// Retrieves an array of the values of the constants in a specified enumeration type.
    /// </summary>
    /// <typeparam name="TEnum">The enumeration type.</typeparam>
    /// <returns>An array of the values of the constants in <typeparamref name="TEnum"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TEnum[] GetValues<TEnum>() where TEnum : struct, Enum
      => (TEnum[])Enum.GetValues(typeof(TEnum));

    /// <summary>
    /// Retrieves an array of the names of the constants in a specified enumeration type.
    /// </summary>
    /// <typeparam name="TEnum">The enumeration type.</typeparam>
    /// <returns>An array of the names of the constants in <typeparamref name="TEnum"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string[] GetNames<TEnum>() where TEnum : struct, Enum
      => Enum.GetNames(typeof(TEnum));

    /// <summary>
    /// Retrieves the name of the constant in the specified enumeration type that has the specified value.
    /// </summary>
    /// <typeparam name="TEnum">The enumeration type.</typeparam>
    /// <param name="value">The value of a particular enumerated constant in terms of its underlying type.</param>
    /// <returns>A string containing the name of the enumerated constant in <typeparamref name="TEnum"/> whose value is <paramref name="value"/>; or <see langword="null"/> if no such constant is found.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? GetName<TEnum>(TEnum value) where TEnum : struct, Enum
      => Enum.GetName(typeof(TEnum), value);

    /// <summary>
    /// Returns a <see cref="bool"/> indicating whether a given integral value, or its name as a string, exists in a specified enumeration.
    /// </summary>
    /// <typeparam name="TEnum">The enumeration type.</typeparam>
    /// <param name="value">The value or name of a constant in <typeparamref name="TEnum"/>.</param>
    /// <returns><see langword="true"/> if a constant in <typeparamref name="TEnum"/> has a value equal to <paramref name="value"/>; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDefined<TEnum>(TEnum value) where TEnum : struct, Enum
      => Enum.IsDefined(typeof(TEnum), value);

  }

}

#endif
