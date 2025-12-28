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

#if !SUPPORTS_RANDOM_GETHEXSTRING

namespace System;

public static partial class RandomPolyfills {
  private const string UpperHexChars = "0123456789ABCDEF";
  private const string LowerHexChars = "0123456789abcdef";

  extension(Random @this) {
    /// <summary>
    /// Creates a string filled with random hexadecimal characters.
    /// </summary>
    /// <param name="length">The length of the string to create.</param>
    /// <param name="lowercase">
    /// <see langword="true"/> to use lowercase hex characters ('a'-'f');
    /// <see langword="false"/> to use uppercase hex characters ('A'-'F').
    /// </param>
    /// <returns>A string of the specified length filled with random hex characters.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
    public string GetHexString(int length, bool lowercase = false) {
      ArgumentOutOfRangeException.ThrowIfNegative(length);
      if (length == 0)
        return string.Empty;

      var hexChars = lowercase ? LowerHexChars : UpperHexChars;
      var result = new char[length];
      for (var i = 0; i < length; ++i)
        result[i] = hexChars[@this.Next(16)];

      return new(result);
    }

    /// <summary>
    /// Fills the specified span with random hexadecimal characters.
    /// </summary>
    /// <param name="destination">The span to fill with random hex characters.</param>
    /// <param name="lowercase">
    /// <see langword="true"/> to use lowercase hex characters ('a'-'f');
    /// <see langword="false"/> to use uppercase hex characters ('A'-'F').
    /// </param>
    public void GetHexString(Span<char> destination, bool lowercase = false) {
      var hexChars = lowercase ? LowerHexChars : UpperHexChars;
      for (var i = 0; i < destination.Length; ++i)
        destination[i] = hexChars[@this.Next(16)];
    }
  }
}

#endif
