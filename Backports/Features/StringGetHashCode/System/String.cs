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

#if !SUPPORTS_STRING_GETHASHCODE_COMPARISON

using System.Globalization;
using System.Runtime.CompilerServices;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class StringPolyfills {
  /// <param name="value">The string.</param>
  extension(string value)
  {
    /// <summary>
    /// Returns the hash code for this string using the specified rules.
    /// </summary>
    /// <param name="comparisonType">One of the enumeration values that specifies the rules to use in the comparison.</param>
    /// <returns>A 32-bit signed integer hash code.</returns>
    /// <exception cref="ArgumentException"><paramref name="comparisonType"/> is not a <see cref="StringComparison"/> value.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetHashCode(StringComparison comparisonType) {
      ArgumentNullException.ThrowIfNull(value);
      
      return comparisonType switch {
        StringComparison.CurrentCulture => CultureInfo.CurrentCulture.CompareInfo.GetHashCode(value, CompareOptions.None),
        StringComparison.CurrentCultureIgnoreCase => CultureInfo.CurrentCulture.CompareInfo.GetHashCode(value, CompareOptions.IgnoreCase),
        StringComparison.InvariantCulture => CultureInfo.InvariantCulture.CompareInfo.GetHashCode(value, CompareOptions.None),
        StringComparison.InvariantCultureIgnoreCase => CultureInfo.InvariantCulture.CompareInfo.GetHashCode(value, CompareOptions.IgnoreCase),
        StringComparison.Ordinal => value.GetHashCode(),
        StringComparison.OrdinalIgnoreCase => StringComparer.OrdinalIgnoreCase.GetHashCode(value),
        _ => AlwaysThrow.ArgumentOutOfRangeException<int>(nameof(comparisonType), $"The value '{comparisonType}' is not a valid StringComparison value."),
      };
    }
  }
}

#endif
