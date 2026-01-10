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

using System.Globalization;

namespace System.Text.Json;

/// <summary>
/// Provides a high-performance API for forward-only, read-only access to UTF-8 encoded JSON text.
/// </summary>
public struct Utf8JsonReader {

  private readonly string _json;
  private int _position;

  /// <summary>
  /// Gets the current token type.
  /// </summary>
  public JsonTokenType TokenType { get; private set; }

  /// <summary>
  /// Gets the depth of the current token.
  /// </summary>
  public int CurrentDepth { get; private set; }

  /// <summary>
  /// Gets the current position within the JSON data.
  /// </summary>
  public long TokenStartIndex => this._position;

  /// <summary>
  /// Gets the total number of bytes consumed by the reader.
  /// </summary>
  public long BytesConsumed => this._position;

  /// <summary>
  /// Gets a value indicating whether the reader has finished reading.
  /// </summary>
  public bool IsFinalBlock { get; }

  private int _tokenStart;
  private int _tokenLength;

  /// <summary>
  /// Initializes a new instance of the <see cref="Utf8JsonReader"/> struct.
  /// </summary>
  /// <param name="jsonData">The UTF-8 encoded JSON text.</param>
  /// <param name="options">The options for reading.</param>
  public Utf8JsonReader(byte[] jsonData, JsonReaderOptions options = default) {
    this._json = Encoding.UTF8.GetString(jsonData);
    this._position = 0;
    this.TokenType = JsonTokenType.None;
    this.CurrentDepth = 0;
    this.IsFinalBlock = true;
    this._tokenStart = 0;
    this._tokenLength = 0;
  }

  internal Utf8JsonReader(string json) {
    this._json = json;
    this._position = 0;
    this.TokenType = JsonTokenType.None;
    this.CurrentDepth = 0;
    this.IsFinalBlock = true;
    this._tokenStart = 0;
    this._tokenLength = 0;
  }

  /// <summary>
  /// Reads the next JSON token.
  /// </summary>
  /// <returns><see langword="true"/> if the next token was read successfully; otherwise, <see langword="false"/>.</returns>
  public bool Read() {
    this._SkipWhitespace();
    if (this._position >= this._json.Length)
      return false;

    var c = this._json[this._position];
    switch (c) {
      case '{':
        this.TokenType = JsonTokenType.StartObject;
        ++this._position;
        ++this.CurrentDepth;
        return true;

      case '}':
        this.TokenType = JsonTokenType.EndObject;
        ++this._position;
        --this.CurrentDepth;
        return true;

      case '[':
        this.TokenType = JsonTokenType.StartArray;
        ++this._position;
        ++this.CurrentDepth;
        return true;

      case ']':
        this.TokenType = JsonTokenType.EndArray;
        ++this._position;
        --this.CurrentDepth;
        return true;

      case '"':
        return this._ReadString();

      case ',':
      case ':':
        ++this._position;
        return this.Read();

      case 't':
        this._Expect("true");
        this.TokenType = JsonTokenType.True;
        return true;

      case 'f':
        this._Expect("false");
        this.TokenType = JsonTokenType.False;
        return true;

      case 'n':
        this._Expect("null");
        this.TokenType = JsonTokenType.Null;
        return true;

      default:
        if (c == '-' || char.IsDigit(c))
          return this._ReadNumber();

        throw new JsonException($"Unexpected character '{c}' at position {this._position}.");
    }
  }

  private void _SkipWhitespace() {
    while (this._position < this._json.Length && char.IsWhiteSpace(this._json[this._position]))
      ++this._position;
  }

  private bool _ReadString() {
    this._tokenStart = this._position + 1;
    ++this._position;

    while (this._position < this._json.Length) {
      var c = this._json[this._position];
      if (c == '\\') {
        this._position += 2;
        continue;
      }
      if (c == '"') {
        this._tokenLength = this._position - this._tokenStart;
        ++this._position;
        this.TokenType = JsonTokenType.String;
        return true;
      }
      ++this._position;
    }

    throw new JsonException("Unterminated string.");
  }

  private bool _ReadNumber() {
    this._tokenStart = this._position;
    if (this._json[this._position] == '-')
      ++this._position;

    while (this._position < this._json.Length) {
      var c = this._json[this._position];
      if (char.IsDigit(c) || c == '.' || c == 'e' || c == 'E' || c == '+' || c == '-') {
        ++this._position;
        continue;
      }
      break;
    }

    this._tokenLength = this._position - this._tokenStart;
    this.TokenType = JsonTokenType.Number;
    return true;
  }

  private void _Expect(string expected) {
    if (this._position + expected.Length > this._json.Length ||
        this._json.Substring(this._position, expected.Length) != expected)
      throw new JsonException($"Expected '{expected}' at position {this._position}.");

    this._position += expected.Length;
  }

  /// <summary>
  /// Gets the string value of the current token.
  /// </summary>
  /// <returns>The string value.</returns>
  public string? GetString() {
    if (this.TokenType == JsonTokenType.Null)
      return null;
    if (this.TokenType != JsonTokenType.String && this.TokenType != JsonTokenType.PropertyName)
      throw new InvalidOperationException("Token is not a string.");

    var raw = this._json.Substring(this._tokenStart, this._tokenLength);
    return _UnescapeString(raw);
  }

  private static string _UnescapeString(string value) {
    if (value.IndexOf('\\') < 0)
      return value;

    var sb = new StringBuilder(value.Length);
    for (var i = 0; i < value.Length; ++i) {
      var c = value[i];
      if (c != '\\') {
        sb.Append(c);
        continue;
      }
      if (++i >= value.Length)
        throw new JsonException("Invalid escape sequence.");

      c = value[i];
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
          if (i + 4 >= value.Length)
            throw new JsonException("Invalid unicode escape sequence.");
          var hex = value.Substring(i + 1, 4);
          sb.Append((char)int.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
          i += 4;
          break;
        default:
          throw new JsonException($"Invalid escape character '\\{c}'.");
      }
    }
    return sb.ToString();
  }

  /// <summary>
  /// Gets the integer value of the current token.
  /// </summary>
  /// <returns>The integer value.</returns>
  public int GetInt32() {
    if (this.TokenType != JsonTokenType.Number)
      throw new InvalidOperationException("Token is not a number.");
    return int.Parse(this._json.Substring(this._tokenStart, this._tokenLength), CultureInfo.InvariantCulture);
  }

  /// <summary>
  /// Gets the long value of the current token.
  /// </summary>
  /// <returns>The long value.</returns>
  public long GetInt64() {
    if (this.TokenType != JsonTokenType.Number)
      throw new InvalidOperationException("Token is not a number.");
    return long.Parse(this._json.Substring(this._tokenStart, this._tokenLength), CultureInfo.InvariantCulture);
  }

  /// <summary>
  /// Gets the double value of the current token.
  /// </summary>
  /// <returns>The double value.</returns>
  public double GetDouble() {
    if (this.TokenType != JsonTokenType.Number)
      throw new InvalidOperationException("Token is not a number.");
    return double.Parse(this._json.Substring(this._tokenStart, this._tokenLength), CultureInfo.InvariantCulture);
  }

  /// <summary>
  /// Gets the decimal value of the current token.
  /// </summary>
  /// <returns>The decimal value.</returns>
  public decimal GetDecimal() {
    if (this.TokenType != JsonTokenType.Number)
      throw new InvalidOperationException("Token is not a number.");
    return decimal.Parse(this._json.Substring(this._tokenStart, this._tokenLength), CultureInfo.InvariantCulture);
  }

  /// <summary>
  /// Gets the boolean value of the current token.
  /// </summary>
  /// <returns>The boolean value.</returns>
  public bool GetBoolean() {
    if (this.TokenType == JsonTokenType.True)
      return true;
    if (this.TokenType == JsonTokenType.False)
      return false;
    throw new InvalidOperationException("Token is not a boolean.");
  }

  /// <summary>
  /// Gets the Guid value of the current token.
  /// </summary>
  /// <returns>The Guid value.</returns>
  public Guid GetGuid() {
    var s = this.GetString();
    return s != null ? Guid.Parse(s) : default;
  }

  /// <summary>
  /// Gets the DateTime value of the current token.
  /// </summary>
  /// <returns>The DateTime value.</returns>
  public DateTime GetDateTime() {
    var s = this.GetString();
    return s != null ? DateTime.Parse(s, CultureInfo.InvariantCulture) : default;
  }

  /// <summary>
  /// Tries to get the string value.
  /// </summary>
  /// <param name="value">The string value.</param>
  /// <returns><see langword="true"/> if successful; otherwise, <see langword="false"/>.</returns>
  public bool TryGetInt32(out int value) {
    if (this.TokenType != JsonTokenType.Number) {
      value = 0;
      return false;
    }
    return int.TryParse(this._json.Substring(this._tokenStart, this._tokenLength), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
  }

  /// <summary>
  /// Tries to get the long value.
  /// </summary>
  /// <param name="value">The long value.</param>
  /// <returns><see langword="true"/> if successful; otherwise, <see langword="false"/>.</returns>
  public bool TryGetInt64(out long value) {
    if (this.TokenType != JsonTokenType.Number) {
      value = 0;
      return false;
    }
    return long.TryParse(this._json.Substring(this._tokenStart, this._tokenLength), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
  }

  /// <summary>
  /// Tries to get the double value.
  /// </summary>
  /// <param name="value">The double value.</param>
  /// <returns><see langword="true"/> if successful; otherwise, <see langword="false"/>.</returns>
  public bool TryGetDouble(out double value) {
    if (this.TokenType != JsonTokenType.Number) {
      value = 0;
      return false;
    }
    return double.TryParse(this._json.Substring(this._tokenStart, this._tokenLength), NumberStyles.Float | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out value);
  }

  /// <summary>
  /// Skips the children of the current token.
  /// </summary>
  public void Skip() {
    if (this.TokenType == JsonTokenType.StartObject || this.TokenType == JsonTokenType.StartArray) {
      var depth = this.CurrentDepth;
      while (this.Read() && this.CurrentDepth > depth - 1)
        ;
    }
  }

  /// <summary>
  /// Tries to skip the children of the current token.
  /// </summary>
  /// <returns><see langword="true"/> if successful; otherwise, <see langword="false"/>.</returns>
  public bool TrySkip() {
    this.Skip();
    return true;
  }

  /// <summary>
  /// Checks if the current string matches the specified value.
  /// </summary>
  /// <param name="text">The text to compare.</param>
  /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
  public bool ValueTextEquals(string text) => this.GetString() == text;

}

/// <summary>
/// Provides options for reading JSON.
/// </summary>
public struct JsonReaderOptions {

  /// <summary>
  /// Gets or sets a value that defines how comments are handled during reading.
  /// </summary>
  public JsonCommentHandling CommentHandling { get; set; }

  /// <summary>
  /// Gets or sets a value that defines whether an extra comma at the end of a list of JSON values in an object or array is allowed.
  /// </summary>
  public bool AllowTrailingCommas { get; set; }

  /// <summary>
  /// Gets or sets the maximum depth allowed when reading JSON.
  /// </summary>
  public int MaxDepth { get; set; }

}

#endif
