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

#if !SUPPORTS_STRING_CONTAINS_COMPARISON_TYPE

using MethodImplOptions = Utilities.MethodImplOptions;
using Guard;
using System.Runtime.CompilerServices;

namespace System;

public static partial class StringPolyfills {
  /// <param name="this">This <see cref="string" /></param>
  extension(string @this)
  {
    /// <summary>
    ///   Returns a value indicating whether a specified string occurs within this string, using the specified comparison
    ///   rules.
    /// </summary>
    /// <param name="value">The string to seek.</param>
    /// <param name="comparisonType">One of the enumeration values that specifies the rules to use in the comparison.</param>
    /// <returns>
    ///   <see langword="true" /> if the <paramref name="value" /> parameter occurs within this string, or if
    ///   <paramref name="value" /> is the empty string (""); otherwise, <see langword="false" />.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(string value, StringComparison comparisonType) {
      Against.ThisIsNull(@this);
      if (value == null)
        AlwaysThrow.ArgumentNullException(nameof(value));

      if (value.Length <= 0)
        return true;

      if (value.Length > @this.Length)
        return false;

      return @this.IndexOf(value, comparisonType) >= 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(char value) => @this.IndexOf(value) >= 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(char value, StringComparison comparisonType) => @this.IndexOf(value.ToString(), comparisonType) >= 0;
  }
}

#endif
