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

#if !SUPPORTS_VERSION_TRYPARSE

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class VersionPolyfills {

  extension(Version) {

    /// <summary>
    /// Converts the string representation of a version number to an equivalent <see cref="Version"/> object.
    /// </summary>
    /// <param name="input">A string that contains a version number to convert.</param>
    /// <returns>An object that is equivalent to the version number specified in the <paramref name="input"/> parameter.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="input"/> has fewer than two or more than four components.</exception>
    /// <exception cref="ArgumentOutOfRangeException">At least one component is less than zero.</exception>
    /// <exception cref="FormatException">At least one component is not an integer.</exception>
    public static Version Parse(string input) {
      ArgumentNullException.ThrowIfNull(input);

      if (string.IsNullOrEmpty(input))
        throw new ArgumentException("Version string portion was too short or too long.", nameof(input));

      var parts = input.Split('.');
      if (parts.Length is < 2 or > 4)
        throw new ArgumentException("Version string portion was too short or too long.", nameof(input));

      var components = new int[parts.Length];
      for (var i = 0; i < parts.Length; ++i) {
        if (!int.TryParse(parts[i], out components[i]))
          throw new ArgumentException("Version string portion was too short or too long.", nameof(input));
        if (components[i] < 0)
          throw new ArgumentOutOfRangeException(nameof(input), "Version's parameters must be greater than or equal to zero.");
      }

      return parts.Length switch {
        2 => new(components[0], components[1]),
        3 => new(components[0], components[1], components[2]),
        _ => new(components[0], components[1], components[2], components[3])
      };
    }

    /// <summary>
    /// Tries to convert the string representation of a version number to an equivalent <see cref="Version"/> object.
    /// </summary>
    /// <param name="input">A string that contains a version number to convert.</param>
    /// <param name="result">When this method returns, contains the <see cref="Version"/> equivalent of the number contained in <paramref name="input"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if the <paramref name="input"/> parameter was converted successfully; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string input, out Version result) {
      result = null;

      if (string.IsNullOrEmpty(input))
        return false;

      try {
        result = new(input);
        return true;
      } catch {
        return false;
      }
    }

  }

}

#endif
