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

#if !SUPPORTS_STRING_STARTS_ENDS_CHAR

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class StringPolyfills {

  /// <summary>
  /// Determines whether this string instance starts with the specified character.
  /// </summary>
  /// <param name="this">This string.</param>
  /// <param name="value">The character to compare.</param>
  /// <returns>true if value matches the beginning of this string; otherwise, false.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsWith(this string @this, char value) {
    if (@this == null)
      throw new NullReferenceException();
    return @this.Length > 0 && @this[0] == value;
  }

  /// <summary>
  /// Determines whether the end of this string instance matches the specified character.
  /// </summary>
  /// <param name="this">This string.</param>
  /// <param name="value">The character to compare.</param>
  /// <returns>true if value matches the end of this string; otherwise, false.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsWith(this string @this, char value) {
    if (@this == null)
      throw new NullReferenceException();
    return @this.Length > 0 && @this[@this.Length - 1] == value;
  }

}

#endif
