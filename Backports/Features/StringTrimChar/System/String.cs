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

#if !SUPPORTS_STRING_TRIM_CHAR

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class StringPolyfills {

  /// <summary>
  /// Removes all leading and trailing instances of a character from the current string.
  /// </summary>
  /// <param name="this">This string.</param>
  /// <param name="trimChar">The character to remove.</param>
  /// <returns>The string that remains after all instances of the trimChar character are removed from the start and end of the current string.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string Trim(this string @this, char trimChar) {
    if (@this == null)
      throw new NullReferenceException();
    return @this.Trim(new[] { trimChar });
  }

  /// <summary>
  /// Removes all leading instances of a character from the current string.
  /// </summary>
  /// <param name="this">This string.</param>
  /// <param name="trimChar">The character to remove.</param>
  /// <returns>The string that remains after all instances of the trimChar character are removed from the start of the current string.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string TrimStart(this string @this, char trimChar) {
    if (@this == null)
      throw new NullReferenceException();
    return @this.TrimStart(new[] { trimChar });
  }

  /// <summary>
  /// Removes all trailing instances of a character from the current string.
  /// </summary>
  /// <param name="this">This string.</param>
  /// <param name="trimChar">The character to remove.</param>
  /// <returns>The string that remains after all instances of the trimChar character are removed from the end of the current string.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string TrimEnd(this string @this, char trimChar) {
    if (@this == null)
      throw new NullReferenceException();
    return @this.TrimEnd(new[] { trimChar });
  }

}

#endif
