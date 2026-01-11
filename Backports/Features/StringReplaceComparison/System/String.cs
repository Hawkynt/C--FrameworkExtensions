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

#if !SUPPORTS_STRING_REPLACE_COMPARISON

using System.Text;
using System.Runtime.CompilerServices;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class StringPolyfills {
  /// <param name="this">This <see cref="string" /></param>
  extension(string @this) {

    /// <summary>
    /// Returns a new string in which all occurrences of a specified string in the current instance are replaced with another specified string, using the provided comparison type.
    /// </summary>
    /// <param name="oldValue">The string to be replaced.</param>
    /// <param name="newValue">The string to replace all occurrences of <paramref name="oldValue"/>.</param>
    /// <param name="comparisonType">One of the enumeration values that determines how <paramref name="oldValue"/> is searched within this instance.</param>
    /// <returns>
    /// A string that is equivalent to the current string except that all instances of <paramref name="oldValue"/>
    /// are replaced with <paramref name="newValue"/>. If <paramref name="oldValue"/> is not found in the current instance,
    /// the method returns the current instance unchanged.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="this"/> or <paramref name="oldValue"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="oldValue"/> is the empty string ("").</exception>
    public string Replace(string oldValue, string? newValue, StringComparison comparisonType) {
      Against.ThisIsNull(@this);
      ArgumentNullException.ThrowIfNull(oldValue);

      if (oldValue.Length == 0)
        throw new ArgumentException("String cannot be of zero length.", nameof(oldValue));

      newValue ??= string.Empty;

      var result = new StringBuilder();
      var startIndex = 0;
      int index;

      while ((index = @this.IndexOf(oldValue, startIndex, comparisonType)) >= 0) {
        result.Append(@this, startIndex, index - startIndex);
        result.Append(newValue);
        startIndex = index + oldValue.Length;
      }

      result.Append(@this, startIndex, @this.Length - startIndex);
      return result.ToString();
    }

  }
}

#endif
