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

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class StringPolyfills {
  /// <param name="this">This string.</param>
  extension(string @this)
  {
    /// <summary>
    /// Replaces all newline sequences in the current string with <see cref="Environment.NewLine"/>.
    /// </summary>
    /// <returns>A string whose contents match this string, but with all newline sequences replaced with <see cref="Environment.NewLine"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReplaceLineEndings() => @this.ReplaceLineEndings(Environment.NewLine);

    /// <summary>
    /// Replaces all newline sequences in the current string with the specified replacement text.
    /// </summary>
    /// <param name="replacementText">The text to use as a replacement.</param>
    /// <returns>A string whose contents match this string, but with all newline sequences replaced with <paramref name="replacementText"/>.</returns>
    public string ReplaceLineEndings(string replacementText) {
      Against.ThisIsNull(@this);
      ArgumentNullException.ThrowIfNull(replacementText);

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

    /// <summary>
    /// Returns an enumeration of lines in this string, where lines are delimited by any newline sequence.
    /// </summary>
    /// <returns>An enumeration of lines.</returns>
    /// <remarks>
    /// <para>This method recognizes the following newline sequences as line terminators:</para>
    /// <list type="bullet">
    ///   <item><description>CR (\r, U+000D)</description></item>
    ///   <item><description>LF (\n, U+000A)</description></item>
    ///   <item><description>CRLF (\r\n, U+000D U+000A)</description></item>
    ///   <item><description>NEL (U+0085)</description></item>
    ///   <item><description>LS (U+2028)</description></item>
    ///   <item><description>PS (U+2029)</description></item>
    /// </list>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LineSplitEnumerator EnumerateLines() => new(@this);
  }
}

/// <summary>
/// Enumerates the lines of a string.
/// </summary>
public struct LineSplitEnumerator : IEnumerator<ReadOnlyMemory<char>>, IEnumerable<ReadOnlyMemory<char>> {

  private readonly string _source;
  private int _position;
  private ReadOnlyMemory<char> _current;
  private bool _isEnumeratorActive;

  internal LineSplitEnumerator(string source) {
    this._source = source ?? string.Empty;
    this._position = 0;
    this._current = default;
    this._isEnumeratorActive = false;
  }

  /// <summary>
  /// Gets the current line.
  /// </summary>
  public ReadOnlyMemory<char> Current => this._current;

  object IEnumerator.Current => this._current;

  /// <summary>
  /// Returns this instance as the enumerator.
  /// </summary>
  public LineSplitEnumerator GetEnumerator() => this;

  IEnumerator<ReadOnlyMemory<char>> IEnumerable<ReadOnlyMemory<char>>.GetEnumerator() => this;

  IEnumerator IEnumerable.GetEnumerator() => this;

  /// <summary>
  /// Advances the enumerator to the next line.
  /// </summary>
  /// <returns><see langword="true"/> if the enumerator was successfully advanced to the next line; <see langword="false"/> if the enumerator has passed the end of the string.</returns>
  public bool MoveNext() {
    if (!this._isEnumeratorActive) {
      this._isEnumeratorActive = true;
      this._position = 0;
    }

    if (this._position > this._source.Length)
      return false;

    var startPos = this._position;
    var endPos = this._source.Length;

    for (var i = startPos; i < this._source.Length; ++i) {
      var c = this._source[i];
      switch (c) {
        case '\r':
          endPos = i;
          this._position = i + 1;
          // Handle \r\n as single newline
          if (i + 1 < this._source.Length && this._source[i + 1] == '\n')
            ++this._position;
          this._current = this._source.AsMemory(startPos, endPos - startPos);
          return true;
        case '\n':
        case '\u0085': // Next Line
        case '\u2028': // Line Separator
        case '\u2029': // Paragraph Separator
          endPos = i;
          this._position = i + 1;
          this._current = this._source.AsMemory(startPos, endPos - startPos);
          return true;
      }
    }

    // No more newlines, return the rest of the string
    if (startPos <= this._source.Length) {
      this._current = this._source.AsMemory(startPos);
      this._position = this._source.Length + 1; // Mark as finished
      return true;
    }

    return false;
  }

  /// <summary>
  /// Resets the enumerator to the beginning.
  /// </summary>
  public void Reset() {
    this._position = 0;
    this._current = default;
    this._isEnumeratorActive = false;
  }

  /// <summary>
  /// Disposes the enumerator.
  /// </summary>
  public void Dispose() { }

}

#endif
