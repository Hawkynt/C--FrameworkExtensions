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

#if !SUPPORTS_TIMESPAN_TRYFORMAT

namespace System;

public static partial class TimeSpanPolyfills {

  extension(TimeSpan @this) {

    /// <summary>
    /// Tries to format the TimeSpan into the provided span of characters.
    /// </summary>
    /// <param name="destination">The span in which to write the TimeSpan's value formatted as a span of characters.</param>
    /// <param name="charsWritten">When this method returns, contains the number of characters written to <paramref name="destination"/>.</param>
    /// <param name="format">A span containing the characters that represent a standard or custom format string.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <returns><see langword="true"/> if the formatting was successful; otherwise, <see langword="false"/>.</returns>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) {
      var str = format.Length == 0
        ? @this.ToString()
        : @this.ToString(format.ToString(), provider);

      if (str.Length > destination.Length) {
        charsWritten = 0;
        return false;
      }
      str.AsSpan().CopyTo(destination);
      charsWritten = str.Length;
      return true;
    }

  }

}

#endif
