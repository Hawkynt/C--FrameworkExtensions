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

#if !SUPPORTS_STRING_CONCAT_SPAN

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class StringPolyfills {

  extension(string) {

    /// <summary>
    /// Concatenates the string representations of two specified read-only character spans.
    /// </summary>
    /// <param name="str0">The first read-only character span to concatenate.</param>
    /// <param name="str1">The second read-only character span to concatenate.</param>
    /// <returns>The concatenated string representations of the values of <paramref name="str0"/> and <paramref name="str1"/>.</returns>
    public static string Concat(ReadOnlySpan<char> str0, ReadOnlySpan<char> str1) {
      var length = str0.Length + str1.Length;
      if (length == 0)
        return string.Empty;

      var result = new char[length];
      str0.CopyTo(new(result, 0, str0.Length));
      str1.CopyTo(new(result, str0.Length, str1.Length));
      return new(result);
    }

    /// <summary>
    /// Concatenates the string representations of three specified read-only character spans.
    /// </summary>
    /// <param name="str0">The first read-only character span to concatenate.</param>
    /// <param name="str1">The second read-only character span to concatenate.</param>
    /// <param name="str2">The third read-only character span to concatenate.</param>
    /// <returns>The concatenated string representations of the values of <paramref name="str0"/>, <paramref name="str1"/>, and <paramref name="str2"/>.</returns>
    public static string Concat(ReadOnlySpan<char> str0, ReadOnlySpan<char> str1, ReadOnlySpan<char> str2) {
      var length = str0.Length + str1.Length + str2.Length;
      if (length == 0)
        return string.Empty;

      var result = new char[length];
      var offset = 0;
      str0.CopyTo(new(result, offset, str0.Length));
      offset += str0.Length;
      str1.CopyTo(new(result, offset, str1.Length));
      offset += str1.Length;
      str2.CopyTo(new(result, offset, str2.Length));
      return new(result);
    }

    /// <summary>
    /// Concatenates the string representations of four specified read-only character spans.
    /// </summary>
    /// <param name="str0">The first read-only character span to concatenate.</param>
    /// <param name="str1">The second read-only character span to concatenate.</param>
    /// <param name="str2">The third read-only character span to concatenate.</param>
    /// <param name="str3">The fourth read-only character span to concatenate.</param>
    /// <returns>The concatenated string representations of the values of <paramref name="str0"/>, <paramref name="str1"/>, <paramref name="str2"/>, and <paramref name="str3"/>.</returns>
    public static string Concat(ReadOnlySpan<char> str0, ReadOnlySpan<char> str1, ReadOnlySpan<char> str2, ReadOnlySpan<char> str3) {
      var length = str0.Length + str1.Length + str2.Length + str3.Length;
      if (length == 0)
        return string.Empty;

      var result = new char[length];
      var offset = 0;
      str0.CopyTo(new(result, offset, str0.Length));
      offset += str0.Length;
      str1.CopyTo(new(result, offset, str1.Length));
      offset += str1.Length;
      str2.CopyTo(new(result, offset, str2.Length));
      offset += str2.Length;
      str3.CopyTo(new(result, offset, str3.Length));
      return new(result);
    }

  }

}

#endif
