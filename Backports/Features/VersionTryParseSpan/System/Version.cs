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

#if !SUPPORTS_VERSION_PARSE_SPAN

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class VersionPolyfills {

  extension(Version) {

    /// <summary>
    /// Converts the span representation of a version number to an equivalent <see cref="Version"/> object.
    /// </summary>
    /// <param name="input">A read-only span of characters that contains a version number to convert.</param>
    /// <returns>An object that is equivalent to the version number specified in the <paramref name="input"/> parameter.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Version Parse(ReadOnlySpan<char> input)
      => Version.Parse(input.ToString());

    /// <summary>
    /// Tries to convert the span representation of a version number to an equivalent <see cref="Version"/> object.
    /// </summary>
    /// <param name="input">A read-only span of characters that contains a version number to convert.</param>
    /// <param name="result">When this method returns, contains the <see cref="Version"/> equivalent of the number contained in <paramref name="input"/>, if the conversion succeeded. If <paramref name="input"/> is <see langword="null"/>, <see cref="string.Empty"/>, or if the conversion fails, <paramref name="result"/> is <see langword="null"/> when the method returns.</param>
    /// <returns><see langword="true"/> if the <paramref name="input"/> parameter was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> input, out Version result)
      => Version.TryParse(input.ToString(), out result);

  }

}

#endif
