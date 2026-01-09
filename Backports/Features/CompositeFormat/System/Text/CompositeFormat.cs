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

// CompositeFormat was added in .NET 8.0
#if !SUPPORTS_COMPOSITE_FORMAT

using Guard;

namespace System.Text;

/// <summary>
/// Represents a parsed composite format string.
/// </summary>
public sealed class CompositeFormat {
  private CompositeFormat(string format, int minimumArgumentCount) {
    this.Format = format;
    this.MinimumArgumentCount = minimumArgumentCount;
  }

  /// <summary>
  /// Gets the original format string.
  /// </summary>
  public string Format { get; }

  /// <summary>
  /// Gets the minimum number of arguments that must be passed to a formatting operation using this format.
  /// </summary>
  public int MinimumArgumentCount { get; }

  /// <summary>
  /// Parses a format string.
  /// </summary>
  /// <param name="format">The composite format string.</param>
  /// <returns>A <see cref="CompositeFormat"/> instance.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="format"/> is <see langword="null"/>.</exception>
  /// <exception cref="FormatException"><paramref name="format"/> is not a valid composite format string.</exception>
  public static CompositeFormat Parse(string format) {
    ArgumentNullException.ThrowIfNull(format);
    var minArgCount = _CalculateMinimumArgumentCount(format);
    return new(format, minArgCount);
  }

  /// <summary>
  /// Tries to parse a format string.
  /// </summary>
  /// <param name="format">The composite format string.</param>
  /// <param name="compositeFormat">When this method returns, contains the parsed <see cref="CompositeFormat"/>, if successful.</param>
  /// <returns><see langword="true"/> if parsing succeeded; otherwise, <see langword="false"/>.</returns>
  public static bool TryParse(string? format, out CompositeFormat? compositeFormat) {
    compositeFormat = null;
    if (format == null)
      return false;

    try {
      var minArgCount = _CalculateMinimumArgumentCount(format);
      compositeFormat = new(format, minArgCount);
      return true;
    } catch {
      return false;
    }
  }

  private static int _CalculateMinimumArgumentCount(string format) {
    var maxIndex = -1;
    var i = 0;
    var length = format.Length;

    while (i < length) {
      var ch = format[i];

      switch (ch) {
        case '{' when i + 1 < length && format[i + 1] == '{':
          i += 2;
          continue;
        case '{': {
          ++i;
          var indexStart = i;

          while (i < length && char.IsDigit(format[i]))
            ++i;

          if (i == indexStart)
            throw new FormatException("Input string was not in a correct format.");

          var indexStr = format.Substring(indexStart, i - indexStart);
          if (!int.TryParse(indexStr, out var index))
            throw new FormatException("Input string was not in a correct format.");

          if (index > maxIndex)
            maxIndex = index;

          while (i < length && format[i] != '}')
            ++i;

          if (i >= length)
            throw new FormatException("Input string was not in a correct format.");

          ++i;
          break;
        }
        case '}' when i + 1 < length && format[i + 1] == '}':
          i += 2;
          continue;
        case '}':
          throw new FormatException("Input string was not in a correct format.");
        default:
          ++i;
          break;
      }
    }

    return maxIndex + 1;
  }

}

#endif
