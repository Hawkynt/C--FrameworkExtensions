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

#if !SUPPORTS_RANDOM_GETSTRING

using Guard;

namespace System;

public static partial class RandomPolyfills {
  extension(Random @this) {
    /// <summary>
    /// Creates a string filled with random characters chosen from the specified set of choices.
    /// </summary>
    /// <param name="choices">The set of characters to choose from.</param>
    /// <param name="length">The length of the string to create.</param>
    /// <returns>A string of the specified length filled with random characters from <paramref name="choices"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="choices"/> is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
    public string GetString(ReadOnlySpan<char> choices, int length) {
      if (choices.IsEmpty)
        AlwaysThrow.ArgumentException("The choices span is empty.", nameof(choices));
      ArgumentOutOfRangeException.ThrowIfNegative(length);

      if (length == 0)
        return string.Empty;

      var result = new char[length];
      for (var i = 0; i < length; ++i)
        result[i] = choices[@this.Next(choices.Length)];

      return new(result);
    }
  }
}

#endif
