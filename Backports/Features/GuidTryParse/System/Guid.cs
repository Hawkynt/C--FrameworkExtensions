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

#if !SUPPORTS_GUID_TRYPARSE

using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class GuidPolyfills {

  extension(Guid) {

    /// <summary>
    /// Converts the string representation of a GUID to the equivalent <see cref="Guid"/> structure.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>A structure that contains the value that was parsed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException"><paramref name="input"/> is not in a recognized format.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid Parse(string input) {
      if (input == null)
        throw new ArgumentNullException(nameof(input));

      if (TryParse(input, out var result))
        return result;

      throw new FormatException("Input string was not in a correct format.");
    }

    /// <summary>
    /// Converts the string representation of a GUID to the equivalent <see cref="Guid"/> structure.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <param name="result">When this method returns, contains the parsed value on success, or a default value on failure.</param>
    /// <returns><see langword="true"/> if the parse operation was successful; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string input, out Guid result) {
      result = Guid.Empty;

      if (string.IsNullOrEmpty(input))
        return false;

      input = input.Trim();

      // Try each format
      return _TryParseFormat(input, "D", out result) // 00000000-0000-0000-0000-000000000000
        || _TryParseFormat(input, "N", out result)   // 00000000000000000000000000000000
        || _TryParseFormat(input, "B", out result)   // {00000000-0000-0000-0000-000000000000}
        || _TryParseFormat(input, "P", out result)   // (00000000-0000-0000-0000-000000000000)
        || _TryParseFormat(input, "X", out result);  // {0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}}
    }

    /// <summary>
    /// Converts the string representation of a GUID to the equivalent <see cref="Guid"/> structure, provided that the string is in the specified format.
    /// </summary>
    /// <param name="input">The GUID to convert.</param>
    /// <param name="format">One of the following specifiers: "N", "D", "B", "P", or "X".</param>
    /// <returns>A structure that contains the value that was parsed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException"><paramref name="input"/> is not in the format specified by <paramref name="format"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid ParseExact(string input, string format) {
      if (input == null)
        throw new ArgumentNullException(nameof(input));
      if (format == null)
        throw new ArgumentNullException(nameof(format));

      if (TryParseExact(input, format, out var result))
        return result;

      throw new FormatException("Input string was not in a correct format.");
    }

    /// <summary>
    /// Converts the string representation of a GUID to the equivalent <see cref="Guid"/> structure, provided that the string is in the specified format.
    /// </summary>
    /// <param name="input">The GUID to convert.</param>
    /// <param name="format">One of the following specifiers: "N", "D", "B", "P", or "X".</param>
    /// <param name="result">When this method returns, contains the parsed value on success, or a default value on failure.</param>
    /// <returns><see langword="true"/> if the parse operation was successful; otherwise, <see langword="false"/>.</returns>
    public static bool TryParseExact(string input, string format, out Guid result) {
      result = Guid.Empty;

      if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(format))
        return false;

      input = input.Trim();

      if (format.Length != 1)
        return false;

      return _TryParseFormat(input, format.ToUpperInvariant(), out result);
    }

  }

  // D format: 00000000-0000-0000-0000-000000000000
  private static readonly Regex _FormatD = new(@"^([0-9a-fA-F]{8})-([0-9a-fA-F]{4})-([0-9a-fA-F]{4})-([0-9a-fA-F]{4})-([0-9a-fA-F]{12})$", RegexOptions.Compiled);

  // N format: 00000000000000000000000000000000
  private static readonly Regex _FormatN = new(@"^([0-9a-fA-F]{32})$", RegexOptions.Compiled);

  // B format: {00000000-0000-0000-0000-000000000000}
  private static readonly Regex _FormatB = new(@"^\{([0-9a-fA-F]{8})-([0-9a-fA-F]{4})-([0-9a-fA-F]{4})-([0-9a-fA-F]{4})-([0-9a-fA-F]{12})\}$", RegexOptions.Compiled);

  // P format: (00000000-0000-0000-0000-000000000000)
  private static readonly Regex _FormatP = new(@"^\(([0-9a-fA-F]{8})-([0-9a-fA-F]{4})-([0-9a-fA-F]{4})-([0-9a-fA-F]{4})-([0-9a-fA-F]{12})\)$", RegexOptions.Compiled);

  // X format: {0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}}
  private static readonly Regex _FormatX = new(@"^\{0x([0-9a-fA-F]{1,8}),0x([0-9a-fA-F]{1,4}),0x([0-9a-fA-F]{1,4}),\{0x([0-9a-fA-F]{1,2}),0x([0-9a-fA-F]{1,2}),0x([0-9a-fA-F]{1,2}),0x([0-9a-fA-F]{1,2}),0x([0-9a-fA-F]{1,2}),0x([0-9a-fA-F]{1,2}),0x([0-9a-fA-F]{1,2}),0x([0-9a-fA-F]{1,2})\}\}$", RegexOptions.Compiled);

  private static bool _TryParseFormat(string input, string format, out Guid result) {
    result = Guid.Empty;

    try {
      switch (format) {
        case "D": {
          var match = _FormatD.Match(input);
          if (!match.Success)
            return false;

          result = new Guid(input);
          return true;
        }
        case "N": {
          var match = _FormatN.Match(input);
          if (!match.Success)
            return false;

          // Convert to D format for the Guid constructor
          var hex = match.Groups[1].Value;
          var formatted = $"{hex.Substring(0, 8)}-{hex.Substring(8, 4)}-{hex.Substring(12, 4)}-{hex.Substring(16, 4)}-{hex.Substring(20, 12)}";
          result = new Guid(formatted);
          return true;
        }
        case "B": {
          var match = _FormatB.Match(input);
          if (!match.Success)
            return false;

          // Remove braces and parse
          result = new Guid(input.Substring(1, input.Length - 2));
          return true;
        }
        case "P": {
          var match = _FormatP.Match(input);
          if (!match.Success)
            return false;

          // Remove parentheses and parse
          result = new Guid(input.Substring(1, input.Length - 2));
          return true;
        }
        case "X": {
          var match = _FormatX.Match(input);
          if (!match.Success)
            return false;

          var a = Convert.ToInt32(match.Groups[1].Value, 16);
          var b = Convert.ToInt16(match.Groups[2].Value, 16);
          var c = Convert.ToInt16(match.Groups[3].Value, 16);
          var d = Convert.ToByte(match.Groups[4].Value, 16);
          var e = Convert.ToByte(match.Groups[5].Value, 16);
          var f = Convert.ToByte(match.Groups[6].Value, 16);
          var g = Convert.ToByte(match.Groups[7].Value, 16);
          var h = Convert.ToByte(match.Groups[8].Value, 16);
          var i = Convert.ToByte(match.Groups[9].Value, 16);
          var j = Convert.ToByte(match.Groups[10].Value, 16);
          var k = Convert.ToByte(match.Groups[11].Value, 16);

          result = new Guid(a, b, c, d, e, f, g, h, i, j, k);
          return true;
        }
        default:
          return false;
      }
    } catch {
      return false;
    }
  }

}

#endif
