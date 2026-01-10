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

// System.Text.Json polyfill for older frameworks that don't have the official package
#if !SUPPORTS_SYSTEM_TEXT_JSON && !OFFICIAL_SYSTEM_TEXT_JSON

using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace System.Text.Json;

/// <summary>
/// Provides a mechanism for examining the structural content of a JSON value without automatically instantiating data values.
/// </summary>
public sealed class JsonDocument : IDisposable {

  private readonly JsonElement _root;
  private bool _disposed;

  private JsonDocument(JsonElement root) => this._root = root;

  /// <summary>
  /// Gets the root element of this JSON document.
  /// </summary>
  public JsonElement RootElement {
    get {
      if (this._disposed)
        throw new ObjectDisposedException(nameof(JsonDocument));
      return this._root;
    }
  }

  /// <summary>
  /// Parses a string as JSON.
  /// </summary>
  /// <param name="json">The JSON string.</param>
  /// <param name="options">Options for parsing.</param>
  /// <returns>A <see cref="JsonDocument"/>.</returns>
  public static JsonDocument Parse(string json, JsonDocumentOptions options = default) {
    ArgumentNullException.ThrowIfNull(json);
    var root = _ParseValue(json, 0, out _);
    return new JsonDocument(root);
  }

  /// <summary>
  /// Parses a stream as JSON.
  /// </summary>
  /// <param name="utf8Json">The stream to parse.</param>
  /// <param name="options">Options for parsing.</param>
  /// <returns>A <see cref="JsonDocument"/>.</returns>
  public static JsonDocument Parse(Stream utf8Json, JsonDocumentOptions options = default) {
    ArgumentNullException.ThrowIfNull(utf8Json);
    using var reader = new StreamReader(utf8Json, Encoding.UTF8);
    var json = reader.ReadToEnd();
    return Parse(json, options);
  }

  /// <summary>
  /// Parses a byte array as JSON.
  /// </summary>
  /// <param name="utf8Json">The bytes to parse.</param>
  /// <param name="options">Options for parsing.</param>
  /// <returns>A <see cref="JsonDocument"/>.</returns>
  public static JsonDocument Parse(byte[] utf8Json, JsonDocumentOptions options = default) {
    ArgumentNullException.ThrowIfNull(utf8Json);
    var json = Encoding.UTF8.GetString(utf8Json);
    return Parse(json, options);
  }

  private static JsonElement _ParseValue(string json, int start, out int end) {
    _SkipWhitespace(json, ref start);

    if (start >= json.Length)
      throw new JsonException("Unexpected end of JSON.");

    var c = json[start];
    switch (c) {
      case '{':
        return _ParseObject(json, start, out end);
      case '[':
        return _ParseArray(json, start, out end);
      case '"':
        return _ParseString(json, start, out end);
      case 't':
        _Expect(json, start, "true", out end);
        return new JsonElement(null, JsonValueKind.True);
      case 'f':
        _Expect(json, start, "false", out end);
        return new JsonElement(null, JsonValueKind.False);
      case 'n':
        _Expect(json, start, "null", out end);
        return new JsonElement(null, JsonValueKind.Null);
      default:
        if (c == '-' || char.IsDigit(c))
          return _ParseNumber(json, start, out end);
        throw new JsonException($"Unexpected character '{c}' at position {start}.");
    }
  }

  private static void _SkipWhitespace(string json, ref int pos) {
    while (pos < json.Length && char.IsWhiteSpace(json[pos]))
      ++pos;
  }

  private static JsonElement _ParseObject(string json, int start, out int end) {
    var dict = new Dictionary<string, JsonElement>();
    var pos = start + 1;
    _SkipWhitespace(json, ref pos);

    if (pos < json.Length && json[pos] == '}') {
      end = pos + 1;
      return new JsonElement(dict, JsonValueKind.Object);
    }

    for (;;) {
      _SkipWhitespace(json, ref pos);
      if (json[pos] != '"')
        throw new JsonException($"Expected property name at position {pos}.");

      var nameElement = _ParseString(json, pos, out pos);
      var name = nameElement.GetString()!;

      _SkipWhitespace(json, ref pos);
      if (json[pos] != ':')
        throw new JsonException($"Expected ':' at position {pos}.");
      ++pos;

      var value = _ParseValue(json, pos, out pos);
      dict[name] = value;

      _SkipWhitespace(json, ref pos);
      if (json[pos] == '}') {
        end = pos + 1;
        return new JsonElement(dict, JsonValueKind.Object);
      }
      if (json[pos] != ',')
        throw new JsonException($"Expected ',' or '}}' at position {pos}.");
      ++pos;
    }
  }

  private static JsonElement _ParseArray(string json, int start, out int end) {
    var list = new List<JsonElement>();
    var pos = start + 1;
    _SkipWhitespace(json, ref pos);

    if (pos < json.Length && json[pos] == ']') {
      end = pos + 1;
      return new JsonElement(list, JsonValueKind.Array);
    }

    for (;;) {
      var value = _ParseValue(json, pos, out pos);
      list.Add(value);

      _SkipWhitespace(json, ref pos);
      if (json[pos] == ']') {
        end = pos + 1;
        return new JsonElement(list, JsonValueKind.Array);
      }
      if (json[pos] != ',')
        throw new JsonException($"Expected ',' or ']' at position {pos}.");
      ++pos;
    }
  }

  private static JsonElement _ParseString(string json, int start, out int end) {
    var sb = new StringBuilder();
    var pos = start + 1;

    while (pos < json.Length) {
      var c = json[pos];
      if (c == '\\') {
        ++pos;
        if (pos >= json.Length)
          throw new JsonException("Unterminated string escape.");
        c = json[pos];
        switch (c) {
          case '"':
            sb.Append('"');
            break;
          case '\\':
            sb.Append('\\');
            break;
          case '/':
            sb.Append('/');
            break;
          case 'b':
            sb.Append('\b');
            break;
          case 'f':
            sb.Append('\f');
            break;
          case 'n':
            sb.Append('\n');
            break;
          case 'r':
            sb.Append('\r');
            break;
          case 't':
            sb.Append('\t');
            break;
          case 'u':
            if (pos + 4 >= json.Length)
              throw new JsonException("Invalid unicode escape sequence.");
            var hex = json.Substring(pos + 1, 4);
            sb.Append((char)int.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
            pos += 4;
            break;
          default:
            throw new JsonException($"Invalid escape character '\\{c}'.");
        }
        ++pos;
        continue;
      }
      if (c == '"') {
        end = pos + 1;
        return new JsonElement(sb.ToString(), JsonValueKind.String);
      }
      sb.Append(c);
      ++pos;
    }

    throw new JsonException("Unterminated string.");
  }

  private static JsonElement _ParseNumber(string json, int start, out int end) {
    var pos = start;
    if (json[pos] == '-')
      ++pos;

    var hasDecimal = false;
    var hasExponent = false;

    while (pos < json.Length) {
      var c = json[pos];
      if (char.IsDigit(c)) {
        ++pos;
        continue;
      }
      if (c == '.' && !hasDecimal) {
        hasDecimal = true;
        ++pos;
        continue;
      }
      if ((c == 'e' || c == 'E') && !hasExponent) {
        hasExponent = true;
        ++pos;
        if (pos < json.Length && (json[pos] == '+' || json[pos] == '-'))
          ++pos;
        continue;
      }
      break;
    }

    end = pos;
    var numStr = json.Substring(start, pos - start);

    if (hasDecimal || hasExponent) {
      var dbl = double.Parse(numStr, CultureInfo.InvariantCulture);
      return new JsonElement(dbl, JsonValueKind.Number);
    }

    if (long.TryParse(numStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var lng))
      return new JsonElement(lng, JsonValueKind.Number);

    var dblFallback = double.Parse(numStr, CultureInfo.InvariantCulture);
    return new JsonElement(dblFallback, JsonValueKind.Number);
  }

  private static void _Expect(string json, int start, string expected, out int end) {
    if (start + expected.Length > json.Length ||
        json.Substring(start, expected.Length) != expected)
      throw new JsonException($"Expected '{expected}' at position {start}.");
    end = start + expected.Length;
  }

  /// <summary>
  /// Disposes the document and releases all associated resources.
  /// </summary>
  public void Dispose() => this._disposed = true;

}

/// <summary>
/// Provides options for reading JSON with <see cref="JsonDocument"/>.
/// </summary>
public struct JsonDocumentOptions {

  /// <summary>
  /// Gets or sets a value that defines how comments are handled during reading.
  /// </summary>
  public JsonCommentHandling CommentHandling { get; set; }

  /// <summary>
  /// Gets or sets a value that defines whether an extra comma at the end of a list of JSON values is allowed.
  /// </summary>
  public bool AllowTrailingCommas { get; set; }

  /// <summary>
  /// Gets or sets the maximum depth allowed when reading JSON.
  /// </summary>
  public int MaxDepth { get; set; }

}

#endif
