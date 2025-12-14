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

#if !SUPPORTS_STRING_REPLACE_LINE_ENDINGS

using System.Runtime.CompilerServices;
using System.Text;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class StringPolyfills {

  /// <summary>
  /// Replaces all newline sequences in the current string with <see cref="Environment.NewLine"/>.
  /// </summary>
  /// <param name="this">This string.</param>
  /// <returns>A string whose contents match this string, but with all newline sequences replaced with <see cref="Environment.NewLine"/>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ReplaceLineEndings(this string @this) => @this.ReplaceLineEndings(Environment.NewLine);

  /// <summary>
  /// Replaces all newline sequences in the current string with the specified replacement text.
  /// </summary>
  /// <param name="this">This string.</param>
  /// <param name="replacementText">The text to use as a replacement.</param>
  /// <returns>A string whose contents match this string, but with all newline sequences replaced with <paramref name="replacementText"/>.</returns>
  public static string ReplaceLineEndings(this string @this, string replacementText) {
    if (@this == null)
      throw new NullReferenceException();
    if (replacementText == null)
      throw new ArgumentNullException(nameof(replacementText));

    if (@this.Length == 0)
      return @this;

    var sb = new StringBuilder(@this.Length);
    for (var i = 0; i < @this.Length; ++i) {
      var c = @this[i];
      switch (c) {
        case '\r':
          sb.Append(replacementText);
          // Handle \r\n as single newline
          if (i + 1 < @this.Length && @this[i + 1] == '\n')
            ++i;
          break;
        case '\n':
        case '\v':
        case '\f':
        case '\u0085': // Next Line
        case '\u2028': // Line Separator
        case '\u2029': // Paragraph Separator
          sb.Append(replacementText);
          break;
        default:
          sb.Append(c);
          break;
      }
    }

    return sb.ToString();
  }

}

#endif
