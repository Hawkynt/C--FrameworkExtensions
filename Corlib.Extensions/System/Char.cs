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

#if SUPPORTS_CONTRACTS 
using System.Diagnostics.Contracts;
#endif
using System.Globalization;
using System.Linq;
#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace System;

using Collections.Generic;

public static partial class CharExtensions {

  /// <summary>
  /// Determines whether the given <see cref="Char"/> is a whitespace.
  /// </summary>
  /// <param name="this">This <see cref="Char"/>.</param>
  /// <returns><see langword="true"/> if it is; otherwise, <see langword="false"/></returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsWhiteSpace(this char @this) => char.IsWhiteSpace(@this);

  /// <summary>
  /// Determines whether the given <see cref="Char"/> is the null character or a whitespace.
  /// </summary>
  /// <param name="this">This <see cref="Char"/>.</param>
  /// <returns><see langword="true"/> if it is; otherwise, <see langword="false"/></returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsNullOrWhiteSpace(this char @this) => @this == default(char) || char.IsWhiteSpace(@this);

  /// <summary>
  /// Determines whether the given <see cref="Char"/> is not the null character or a whitespace.
  /// </summary>
  /// <param name="this">This <see cref="Char"/>.</param>
  /// <returns><see langword="true"/> if it is; otherwise, <see langword="false"/></returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsNotNullOrWhiteSpace(this char @this) => @this != default(char) && !char.IsWhiteSpace(@this);

  /// <summary>
  /// Determines whether the specified <see cref="Char"/> is a digit.
  /// </summary>
  /// <param name="this">This <see cref="Char"/>.</param>
  /// <returns><see langword="true"/> if the specified <see cref="Char"/> is a digit; otherwise, <see langword="false"/>.</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsDigit(this char @this) => char.IsDigit(@this);

  /// <summary>
  /// Determines whether the specified <see cref="Char"/> is not a digit.
  /// </summary>
  /// <param name="this">This <see cref="Char"/>.</param>
  /// <returns><see langword="true"/> if the specified <see cref="Char"/> is not a digit; otherwise, <see langword="false"/>.</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsNotDigit(this char @this) => !char.IsDigit(@this);

  /// <summary>
  /// Determines whether the specified <see cref="Char"/> is upper-case.
  /// </summary>
  /// <param name="this">This <see cref="Char"/>.</param>
  /// <returns><see langword="true"/> if the specified <see cref="Char"/> is upper-case; otherwise, <see langword="false"/>.</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsUpper(this char @this) => char.IsUpper(@this);

  /// <summary>
  /// Determines whether the specified <see cref="Char"/> is not upper-case.
  /// </summary>
  /// <param name="this">This <see cref="Char"/>.</param>
  /// <returns><see langword="true"/> if the specified <see cref="Char"/> is not upper-case; otherwise, <see langword="false"/>.</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsNotUpper(this char @this) => !char.IsUpper(@this);

  /// <summary>
  /// Determines whether the specified <see cref="Char"/> is lower-case.
  /// </summary>
  /// <param name="this">This <see cref="Char"/>.</param>
  /// <returns><see langword="true"/> if the specified <see cref="Char"/> is lower-case; otherwise, <see langword="false"/>.</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsLower(this char @this) => char.IsLower(@this);

  /// <summary>
  /// Determines whether the specified <see cref="Char"/> is not lower-case.
  /// </summary>
  /// <param name="this">This <see cref="Char"/>.</param>
  /// <returns><see langword="true"/> if the specified <see cref="Char"/> is not lower-case; otherwise, <see langword="false"/>.</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsNotLower(this char @this) => !char.IsLower(@this);

  /// <summary>
  /// Determines whether the specified <see cref="Char"/> is a letter.
  /// </summary>
  /// <param name="this">This <see cref="Char"/>.</param>
  /// <returns><see langword="true"/> if the specified <see cref="Char"/> is a letter; otherwise, <see langword="false"/>.</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsLetter(this char @this) => char.IsLetter(@this);

  /// <summary>
  /// Determines whether the specified <see cref="Char"/> is not a letter.
  /// </summary>
  /// <param name="this">This <see cref="Char"/>.</param>
  /// <returns><see langword="true"/> if the specified <see cref="Char"/> is not a letter; otherwise, <see langword="false"/>.</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsNotLetter(this char @this) => !char.IsLetter(@this);

  /// <summary>
  /// converts the given <see cref="Char"/> to uppercase.
  /// </summary>
  /// <param name="this">This <see cref="Char"/>.</param>
  /// <returns>The upper-case <see cref="Char"/>.</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static char ToUpper(this char @this) => char.ToUpper(@this);

  /// <summary>
  /// converts the given <see cref="Char"/> to uppercase.
  /// </summary>
  /// <param name="this">This <see cref="Char"/>.</param>
  /// <param name="culture">The <see cref="CultureInfo"/> to use.</param>
  /// <returns>The upper-case <see cref="Char"/>.</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static char ToUpper(this char @this, CultureInfo culture) => char.ToUpper(@this, culture);

  /// <summary>
  /// converts the given <see cref="Char"/> to lowercase.
  /// </summary>
  /// <param name="this">This <see cref="Char"/>.</param>
  /// <returns>The lower-case <see cref="Char"/>.</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static char ToLower(this char @this) => char.ToLower(@this);

  /// <summary>
  /// converts the given <see cref="Char"/> to lowercase.
  /// </summary>
  /// <param name="this">This <see cref="Char"/>.</param>
  /// <param name="culture">The <see cref="CultureInfo"/> to use.</param>
  /// <returns>The lower-case <see cref="Char"/>.</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static char ToLower(this char @this, CultureInfo culture) => char.ToLower(@this, culture);

  /// <summary>
  /// Determines whether the given <see cref="Char"/> is any of the ones in the list.
  /// </summary>
  /// <param name="this">This <see cref="Char"/>.</param>
  /// <param name="list">The list.</param>
  /// <returns><see langword="true"/> if the given <see cref="Char"/> is in the list; otherwise, <see langword="false"/>.
  /// </returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsAnyOf(this char @this, params char[] list) => list.Any(c => c == @this);

  /// <summary>
  /// Determines whether the given <see cref="Char"/> is any of the ones in the list.
  /// </summary>
  /// <param name="this">This <see cref="Char"/>.</param>
  /// <param name="list">The list.</param>
  /// <returns><see langword="true"/> if the given <see cref="Char"/> is in the list; otherwise, <see langword="false"/>.
  /// </returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsAnyOf(this char @this, IEnumerable<char> list) => list.Any(c => c == @this);
  
  /// <summary>
  /// Repeats this <see cref="Char"/> several times.
  /// </summary>
  /// <param name="this">This <see cref="Char"/>.</param>
  /// <param name="count">The count.</param>
  /// <returns>A new string containing the given <see cref="Char"/> the specified times.</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static string Repeat(this char @this, int count) => new(@this, count);

}
