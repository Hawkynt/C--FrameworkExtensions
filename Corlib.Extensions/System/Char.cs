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

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class CharExtensions {
  /// <summary>
  ///   Determines whether the given <see cref="Char" /> is a whitespace.
  /// </summary>
  /// <param name="this">This <see cref="Char" />.</param>
  /// <returns><see langword="true" /> if it is; otherwise, <see langword="false" /></returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsWhiteSpace(this char @this) => char.IsWhiteSpace(@this);

  /// <summary>
  ///   Determines whether the given <see cref="Char" /> is the null character or a whitespace.
  /// </summary>
  /// <param name="this">This <see cref="Char" />.</param>
  /// <returns><see langword="true" /> if it is; otherwise, <see langword="false" /></returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNullOrWhiteSpace(this char @this) => @this == default(char) || char.IsWhiteSpace(@this);

  /// <summary>
  ///   Determines whether the given <see cref="Char" /> is not the null character or a whitespace.
  /// </summary>
  /// <param name="this">This <see cref="Char" />.</param>
  /// <returns><see langword="true" /> if it is; otherwise, <see langword="false" /></returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNotNullOrWhiteSpace(this char @this) => @this != default(char) && !char.IsWhiteSpace(@this);

  /// <summary>
  ///   Determines whether the specified <see cref="Char" /> is a digit.
  /// </summary>
  /// <param name="this">This <see cref="Char" />.</param>
  /// <returns><see langword="true" /> if the specified <see cref="Char" /> is a digit; otherwise, <see langword="false" />.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsDigit(this char @this) => char.IsDigit(@this);

  /// <summary>
  ///   Determines whether the specified <see cref="Char" /> is not a digit.
  /// </summary>
  /// <param name="this">This <see cref="Char" />.</param>
  /// <returns>
  ///   <see langword="true" /> if the specified <see cref="Char" /> is not a digit; otherwise,
  ///   <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNotDigit(this char @this) => !char.IsDigit(@this);

  /// <summary>
  ///   Determines whether the specified <see cref="Char" /> is upper-case.
  /// </summary>
  /// <param name="this">This <see cref="Char" />.</param>
  /// <returns>
  ///   <see langword="true" /> if the specified <see cref="Char" /> is upper-case; otherwise,
  ///   <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsUpper(this char @this) => char.IsUpper(@this);

  /// <summary>
  ///   Determines whether the specified <see cref="Char" /> is not upper-case.
  /// </summary>
  /// <param name="this">This <see cref="Char" />.</param>
  /// <returns>
  ///   <see langword="true" /> if the specified <see cref="Char" /> is not upper-case; otherwise,
  ///   <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNotUpper(this char @this) => !char.IsUpper(@this);

  /// <summary>
  ///   Determines whether the specified <see cref="Char" /> is lower-case.
  /// </summary>
  /// <param name="this">This <see cref="Char" />.</param>
  /// <returns>
  ///   <see langword="true" /> if the specified <see cref="Char" /> is lower-case; otherwise,
  ///   <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsLower(this char @this) => char.IsLower(@this);

  /// <summary>
  ///   Determines whether the specified <see cref="Char" /> is not lower-case.
  /// </summary>
  /// <param name="this">This <see cref="Char" />.</param>
  /// <returns>
  ///   <see langword="true" /> if the specified <see cref="Char" /> is not lower-case; otherwise,
  ///   <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNotLower(this char @this) => !char.IsLower(@this);

  /// <summary>
  ///   Determines whether the specified <see cref="Char" /> is a letter.
  /// </summary>
  /// <param name="this">This <see cref="Char" />.</param>
  /// <returns><see langword="true" /> if the specified <see cref="Char" /> is a letter; otherwise, <see langword="false" />.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsLetter(this char @this) => char.IsLetter(@this);

  /// <summary>
  ///   Determines whether the specified <see cref="Char" /> is not a letter.
  /// </summary>
  /// <param name="this">This <see cref="Char" />.</param>
  /// <returns>
  ///   <see langword="true" /> if the specified <see cref="Char" /> is not a letter; otherwise,
  ///   <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNotLetter(this char @this) => !char.IsLetter(@this);

  /// <summary>
  ///   Determines whether the specified <see cref="Char" /> is a control character.
  /// </summary>
  /// <param name="this">This <see cref="Char" />.</param>
  /// <returns>
  ///   <see langword="true" /> if the specified <see cref="Char" /> is a control character; otherwise,
  ///   <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsControl(this char @this) => char.IsControl(@this);

  /// <summary>
  ///   Determines whether the specified <see cref="Char" /> is not a control character.
  /// </summary>
  /// <param name="this">This <see cref="Char" />.</param>
  /// <returns>
  ///   <see langword="true" /> if the specified <see cref="Char" /> is not a control character; otherwise,
  ///   <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNotControl(this char @this) => !char.IsControl(@this);

  /// <summary>
  ///   Checks whether this is a control character but not a whitespace.
  /// </summary>
  /// <param name="this">This <see cref="Char" />.</param>
  /// <returns>
  ///   <see langword="true" /> if the specified <see cref="Char" /> is a control character but not a whitespace;
  ///   otherwise, <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsControlButNoWhiteSpace(this char @this) => char.IsControl(@this) && !char.IsWhiteSpace(@this);

  /// <summary>
  ///   converts the given <see cref="Char" /> to uppercase.
  /// </summary>
  /// <param name="this">This <see cref="Char" />.</param>
  /// <returns>The upper-case <see cref="Char" />.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static char ToUpper(this char @this) => char.ToUpper(@this);

  /// <summary>
  ///   converts the given <see cref="Char" /> to uppercase.
  /// </summary>
  /// <param name="this">This <see cref="Char" />.</param>
  /// <param name="culture">The <see cref="CultureInfo" /> to use.</param>
  /// <returns>The upper-case <see cref="Char" />.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static char ToUpper(this char @this, CultureInfo culture) => char.ToUpper(@this, culture);

  /// <summary>
  ///   converts the given <see cref="Char" /> to lowercase.
  /// </summary>
  /// <param name="this">This <see cref="Char" />.</param>
  /// <returns>The lower-case <see cref="Char" />.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static char ToLower(this char @this) => char.ToLower(@this);

  /// <summary>
  ///   converts the given <see cref="Char" /> to lowercase.
  /// </summary>
  /// <param name="this">This <see cref="Char" />.</param>
  /// <param name="culture">The <see cref="CultureInfo" /> to use.</param>
  /// <returns>The lower-case <see cref="Char" />.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static char ToLower(this char @this, CultureInfo culture) => char.ToLower(@this, culture);

  /// <summary>
  ///   Determines whether the given <see cref="Char" /> is any of the ones in the list.
  /// </summary>
  /// <param name="this">This <see cref="Char" />.</param>
  /// <param name="list">The list.</param>
  /// <returns>
  ///   <see langword="true" /> if the given <see cref="Char" /> is in the list; otherwise, <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsAnyOf(this char @this, params char[] list) => list.Any(c => c == @this);

  /// <summary>
  ///   Determines whether the given <see cref="Char" /> is any of the ones in the list.
  /// </summary>
  /// <param name="this">This <see cref="Char" />.</param>
  /// <param name="list">The list.</param>
  /// <returns>
  ///   <see langword="true" /> if the given <see cref="Char" /> is in the list; otherwise, <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsAnyOf(this char @this, IEnumerable<char> list) => list.Any(c => c == @this);

  /// <summary>
  ///   Repeats this <see cref="Char" /> several times.
  /// </summary>
  /// <param name="this">This <see cref="Char" />.</param>
  /// <param name="count">The count.</param>
  /// <returns>A new string containing the given <see cref="Char" /> the specified times.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string Repeat(this char @this, int count) => new(@this, count);
}
