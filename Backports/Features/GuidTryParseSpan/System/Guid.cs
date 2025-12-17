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

#if !SUPPORTS_GUID_PARSE_SPAN

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class GuidPolyfills {

  extension(Guid) {

    /// <summary>
    /// Converts a read-only character span that represents a GUID to the equivalent <see cref="Guid"/> structure.
    /// </summary>
    /// <param name="input">A read-only span containing the characters representing the GUID to convert.</param>
    /// <returns>A structure that contains the value that was parsed.</returns>
    /// <exception cref="FormatException"><paramref name="input"/> is not in a recognized format.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid Parse(ReadOnlySpan<char> input)
      => Guid.Parse(input.ToString());

    /// <summary>
    /// Converts the read-only character span representation of a GUID to the equivalent <see cref="Guid"/> structure.
    /// </summary>
    /// <param name="input">A read-only span containing the characters representing the GUID to convert.</param>
    /// <param name="result">
    /// When this method returns, contains the parsed value on success, or a default value on failure.
    /// </param>
    /// <returns><see langword="true"/> if the parse operation was successful; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> input, out Guid result)
      => Guid.TryParse(input.ToString(), out result);

    /// <summary>
    /// Converts a read-only character span that represents a GUID to the equivalent <see cref="Guid"/> structure, provided that the string is in the specified format.
    /// </summary>
    /// <param name="input">A read-only span containing the characters representing the GUID to convert.</param>
    /// <param name="format">One of the following specifiers that indicates the exact format to use when interpreting <paramref name="input"/>: "N", "D", "B", "P", or "X".</param>
    /// <returns>A structure that contains the value that was parsed.</returns>
    /// <exception cref="FormatException"><paramref name="input"/> is not in the format specified by <paramref name="format"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid ParseExact(ReadOnlySpan<char> input, ReadOnlySpan<char> format)
      => Guid.ParseExact(input.ToString(), format.ToString());

    /// <summary>
    /// Converts the read-only character span representation of a GUID to the equivalent <see cref="Guid"/> structure, provided that the string is in the specified format.
    /// </summary>
    /// <param name="input">A read-only span containing the characters representing the GUID to convert.</param>
    /// <param name="format">One of the following specifiers that indicates the exact format to use when interpreting <paramref name="input"/>: "N", "D", "B", "P", or "X".</param>
    /// <param name="result">
    /// When this method returns, contains the parsed value on success, or a default value on failure.
    /// </param>
    /// <returns><see langword="true"/> if the parse operation was successful; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseExact(ReadOnlySpan<char> input, ReadOnlySpan<char> format, out Guid result)
      => Guid.TryParseExact(input.ToString(), format.ToString(), out result);

  }

}

#endif
