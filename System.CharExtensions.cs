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
using System.Globalization;
using System.Linq;
#if NETFX_45
using System.Runtime.CompilerServices;
#endif

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace System {
  internal static partial class CharExtensions {

    /// <summary>
    /// Determines whether the given character is a whitespace.
    /// </summary>
    /// <param name="this">This Char.</param>
    /// <returns><c>true</c> if it is; otherwise, <c>false</c></returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [Pure]
    public static bool IsWhiteSpace(this char @this) => char.IsWhiteSpace(@this);

    /// <summary>
    /// Determines whether the given char is the null character or a whitespace.
    /// </summary>
    /// <param name="this">This Char.</param>
    /// <returns><c>true</c> if it is; otherwise, <c>false</c></returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [Pure]
    public static bool IsNullWhiteSpace(this char @this) => @this == default(char) || char.IsWhiteSpace(@this);

    /// <summary>
    /// Determines whether the given char is not the null character or a whitespace.
    /// </summary>
    /// <param name="this">This Char.</param>
    /// <returns><c>true</c> if it is; otherwise, <c>false</c></returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [Pure]
    public static bool IsNotNullWhiteSpace(this char @this) => @this != default(char) && !char.IsWhiteSpace(@this);

    /// <summary>
    /// Determines whether the specified char is a digit.
    /// </summary>
    /// <param name="this">This Char.</param>
    /// <returns>
    ///   <c>true</c> if the specified char is a digit; otherwise, <c>false</c>.
    /// </returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [Pure]
    public static bool IsDigit(this char @this) => @this >= '0' && @this <= '9';

    /// <summary>
    /// Determines whether the specified char is upper case.
    /// </summary>
    /// <param name="this">This Char.</param>
    /// <returns>
    ///   <c>true</c> if the specified char is upper case; otherwise, <c>false</c>.
    /// </returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [Pure]
    public static bool IsUpper(this char @this) => char.IsUpper(@this);

    /// <summary>
    /// Determines whether the specified char is lower case.
    /// </summary>
    /// <param name="this">This Char.</param>
    /// <returns>
    ///   <c>true</c> if the specified char is lower case; otherwise, <c>false</c>.
    /// </returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [Pure]
    public static bool IsLower(this char @this) => char.IsLower(@this);

    /// <summary>
    /// Determines whether the specified char is a letter.
    /// </summary>
    /// <param name="this">This Char.</param>
    /// <returns>
    ///   <c>true</c> if the specified char is a letter; otherwise, <c>false</c>.
    /// </returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [Pure]
    public static bool IsLetter(this char @this) => char.IsLetter(@this);

    /// <summary>
    /// converts the given character to uppercase.
    /// </summary>
    /// <param name="this">This Char.</param>
    /// <returns>The upper-case char.</returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [Pure]
    public static char ToUpper(this char @this) => char.ToUpper(@this);

    /// <summary>
    /// converts the given character to uppercase.
    /// </summary>
    /// <param name="this">This Char.</param>
    /// <param name="culture">The culture ot use.</param>
    /// <returns>The upper-case char.</returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [Pure]
    public static char ToUpper(this char @this, CultureInfo culture) => char.ToUpper(@this, culture);

    /// <summary>
    /// converts the given character to lowercase.
    /// </summary>
    /// <param name="this">This Char.</param>
    /// <returns>The lower-case char.</returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [Pure]
    public static char ToLower(this char @this) => char.ToLower(@this);

    /// <summary>
    /// converts the given character to lowercase.
    /// </summary>
    /// <param name="this">This Char.</param>
    /// <param name="culture">The culture ot use.</param>
    /// <returns>The lower-case char.</returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [Pure]
    public static char ToLower(this char @this, CultureInfo culture) => char.ToLower(@this, culture);

    /// <summary>
    /// Determines whether the given char is any of the ones in the list.
    /// </summary>
    /// <param name="this">This Char.</param>
    /// <param name="list">The list.</param>
    /// <returns>
    ///   <c>true</c> if the given char is in the list; otherwise, <c>false</c>.
    /// </returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [Pure]
    public static bool IsAnyOf(this char @this, params char[] list) => list.Any(c => c == @this);

    /// <summary>
    /// Repeats this character several times.
    /// </summary>
    /// <param name="this">This Char.</param>
    /// <param name="count">The count.</param>
    /// <returns>A new string containing the given character the specified times.</returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [Pure]
    public static string Repeat(this char @this, int count) => new string(@this, count);
  }
}