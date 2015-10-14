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
using System.Runtime.CompilerServices;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace System {
  internal static partial class CharExtensions {
    /// <summary>
    /// Determines whether the specified char is a digit.
    /// </summary>
    /// <param name="This">This Char.</param>
    /// <returns>
    ///   <c>true</c> if the specified char is a digit; otherwise, <c>false</c>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining), Pure]
    public static bool IsDigit(this char This) => This >= '0' && This <= '9';

    /// <summary>
    /// converts the given character to uppercase.
    /// </summary>
    /// <param name="This">This Char.</param>
    /// <returns>The upper-case char.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining), Pure]
    public static char ToUpper(this char This) => char.ToUpper(This);

    /// <summary>
    /// converts the given character to uppercase.
    /// </summary>
    /// <param name="This">This Char.</param>
    /// <param name="culture">The culture ot use.</param>
    /// <returns>The upper-case char.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char ToUpper(this char This, CultureInfo culture) => char.ToUpper(This, culture);

    /// <summary>
    /// converts the given character to lowercase.
    /// </summary>
    /// <param name="This">This Char.</param>
    /// <returns>The lower-case char.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining), Pure]
    public static char ToLower(this char This) => char.ToLower(This);

    /// <summary>
    /// converts the given character to lowercase.
    /// </summary>
    /// <param name="This">This Char.</param>
    /// <param name="culture">The culture ot use.</param>
    /// <returns>The lower-case char.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char ToLower(this char This, CultureInfo culture) => char.ToLower(This, culture);

    /// <summary>
    /// Determines whether the given char is any of the ones in the list.
    /// </summary>
    /// <param name="This">This Char.</param>
    /// <param name="list">The list.</param>
    /// <returns>
    ///   <c>true</c> if the given char is in the list; otherwise, <c>false</c>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining), Pure]
    public static bool IsAnyOf(this char This, params char[] list) => list.Any(c => c == This);

    /// <summary>
    /// Repeats this character several times.
    /// </summary>
    /// <param name="This">This Char.</param>
    /// <param name="count">The count.</param>
    /// <returns>A new string containing the given character the specified times.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining), Pure]
    public static string Repeat(this char This, int count) => new string(This, count);
  }
}