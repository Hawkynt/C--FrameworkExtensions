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
using System.IO;

namespace System.Text.Json;

/// <summary>
/// Provides a high-performance API for forward-only, non-cached writing of UTF-8 encoded JSON text.
/// </summary>
public sealed class Utf8JsonWriter : IDisposable {

  private readonly StringBuilder _buffer = new();
  private readonly Stream? _stream;
  private readonly bool _indented;
  private int _depth;
  private bool _needsComma;
  private bool _disposed;

  /// <summary>
  /// Gets the total number of bytes written by the <see cref="Utf8JsonWriter"/> so far.
  /// </summary>
  public long BytesCommitted { get; private set; }

  /// <summary>
  /// Gets the total number of bytes written and buffered by the <see cref="Utf8JsonWriter"/> so far.
  /// </summary>
  public long BytesPending => this._buffer.Length;

  /// <summary>
  /// Gets the current depth of the writer.
  /// </summary>
  public int CurrentDepth => this._depth;

  /// <summary>
  /// Initializes a new instance of the <see cref="Utf8JsonWriter"/> class.
  /// </summary>
  /// <param name="utf8Json">The stream to write to.</param>
  /// <param name="options">The options for writing.</param>
  public Utf8JsonWriter(Stream utf8Json, JsonWriterOptions options = default) {
    this._stream = utf8Json ?? throw new ArgumentNullException(nameof(utf8Json));
    this._indented = options.Indented;
  }

  internal Utf8JsonWriter(bool indented) => this._indented = indented;

  private void _WriteIndent() {
    if (!this._indented)
      return;
    this._buffer.AppendLine();
    this._buffer.Append(' ', this._depth * 2);
  }

  private void _WriteCommaIfNeeded() {
    if (this._needsComma)
      this._buffer.Append(',');
    this._needsComma = false;
  }

  /// <summary>
  /// Writes the beginning of a JSON object.
  /// </summary>
  public void WriteStartObject() {
    this._WriteCommaIfNeeded();
    this._WriteIndent();
    this._buffer.Append('{');
    ++this._depth;
    this._needsComma = false;
  }

  /// <summary>
  /// Writes the end of a JSON object.
  /// </summary>
  public void WriteEndObject() {
    --this._depth;
    this._WriteIndent();
    this._buffer.Append('}');
    this._needsComma = true;
  }

  /// <summary>
  /// Writes the beginning of a JSON array.
  /// </summary>
  public void WriteStartArray() {
    this._WriteCommaIfNeeded();
    this._WriteIndent();
    this._buffer.Append('[');
    ++this._depth;
    this._needsComma = false;
  }

  /// <summary>
  /// Writes the end of a JSON array.
  /// </summary>
  public void WriteEndArray() {
    --this._depth;
    this._WriteIndent();
    this._buffer.Append(']');
    this._needsComma = true;
  }

  /// <summary>
  /// Writes a property name specified as a string.
  /// </summary>
  /// <param name="propertyName">The name of the property to write.</param>
  public void WritePropertyName(string propertyName) {
    this._WriteCommaIfNeeded();
    this._WriteIndent();
    this._WriteEscapedString(propertyName);
    this._buffer.Append(':');
    if (this._indented)
      this._buffer.Append(' ');
    this._needsComma = false;
  }

  /// <summary>
  /// Writes a string text value.
  /// </summary>
  /// <param name="value">The value to write.</param>
  public void WriteStringValue(string? value) {
    this._WriteCommaIfNeeded();
    if (value == null)
      this._buffer.Append("null");
    else
      this._WriteEscapedString(value);
    this._needsComma = true;
  }

  /// <summary>
  /// Writes an integer value.
  /// </summary>
  /// <param name="value">The value to write.</param>
  public void WriteNumberValue(int value) {
    this._WriteCommaIfNeeded();
    this._buffer.Append(value.ToString(CultureInfo.InvariantCulture));
    this._needsComma = true;
  }

  /// <summary>
  /// Writes an integer value.
  /// </summary>
  /// <param name="value">The value to write.</param>
  public void WriteNumberValue(long value) {
    this._WriteCommaIfNeeded();
    this._buffer.Append(value.ToString(CultureInfo.InvariantCulture));
    this._needsComma = true;
  }

  /// <summary>
  /// Writes a double value.
  /// </summary>
  /// <param name="value">The value to write.</param>
  public void WriteNumberValue(double value) {
    this._WriteCommaIfNeeded();
    this._buffer.Append(value.ToString("G17", CultureInfo.InvariantCulture));
    this._needsComma = true;
  }

  /// <summary>
  /// Writes a decimal value.
  /// </summary>
  /// <param name="value">The value to write.</param>
  public void WriteNumberValue(decimal value) {
    this._WriteCommaIfNeeded();
    this._buffer.Append(value.ToString(CultureInfo.InvariantCulture));
    this._needsComma = true;
  }

  /// <summary>
  /// Writes a boolean value.
  /// </summary>
  /// <param name="value">The value to write.</param>
  public void WriteBooleanValue(bool value) {
    this._WriteCommaIfNeeded();
    this._buffer.Append(value ? "true" : "false");
    this._needsComma = true;
  }

  /// <summary>
  /// Writes a null value.
  /// </summary>
  public void WriteNullValue() {
    this._WriteCommaIfNeeded();
    this._buffer.Append("null");
    this._needsComma = true;
  }

  /// <summary>
  /// Writes a string property.
  /// </summary>
  /// <param name="propertyName">The name of the property.</param>
  /// <param name="value">The value to write.</param>
  public void WriteString(string propertyName, string? value) {
    this.WritePropertyName(propertyName);
    this.WriteStringValue(value);
  }

  /// <summary>
  /// Writes an integer property.
  /// </summary>
  /// <param name="propertyName">The name of the property.</param>
  /// <param name="value">The value to write.</param>
  public void WriteNumber(string propertyName, int value) {
    this.WritePropertyName(propertyName);
    this.WriteNumberValue(value);
  }

  /// <summary>
  /// Writes an integer property.
  /// </summary>
  /// <param name="propertyName">The name of the property.</param>
  /// <param name="value">The value to write.</param>
  public void WriteNumber(string propertyName, long value) {
    this.WritePropertyName(propertyName);
    this.WriteNumberValue(value);
  }

  /// <summary>
  /// Writes a double property.
  /// </summary>
  /// <param name="propertyName">The name of the property.</param>
  /// <param name="value">The value to write.</param>
  public void WriteNumber(string propertyName, double value) {
    this.WritePropertyName(propertyName);
    this.WriteNumberValue(value);
  }

  /// <summary>
  /// Writes a boolean property.
  /// </summary>
  /// <param name="propertyName">The name of the property.</param>
  /// <param name="value">The value to write.</param>
  public void WriteBoolean(string propertyName, bool value) {
    this.WritePropertyName(propertyName);
    this.WriteBooleanValue(value);
  }

  /// <summary>
  /// Writes a null property.
  /// </summary>
  /// <param name="propertyName">The name of the property.</param>
  public void WriteNull(string propertyName) {
    this.WritePropertyName(propertyName);
    this.WriteNullValue();
  }

  /// <summary>
  /// Writes a JSON value from a <see cref="JsonElement"/>.
  /// </summary>
  /// <param name="element">The element to write.</param>
  public void WriteValue(JsonElement element) {
    this._WriteCommaIfNeeded();
    this._buffer.Append(element.GetRawText());
    this._needsComma = true;
  }

  /// <summary>
  /// Writes a raw JSON value.
  /// </summary>
  /// <param name="json">The raw JSON to write.</param>
  public void WriteRawValue(string json) {
    this._WriteCommaIfNeeded();
    this._buffer.Append(json);
    this._needsComma = true;
  }

  private void _WriteEscapedString(string value) {
    this._buffer.Append('"');
    foreach (var c in value)
      switch (c) {
        case '"':
          this._buffer.Append("\\\"");
          break;
        case '\\':
          this._buffer.Append("\\\\");
          break;
        case '\b':
          this._buffer.Append("\\b");
          break;
        case '\f':
          this._buffer.Append("\\f");
          break;
        case '\n':
          this._buffer.Append("\\n");
          break;
        case '\r':
          this._buffer.Append("\\r");
          break;
        case '\t':
          this._buffer.Append("\\t");
          break;
        default:
          if (c < ' ')
            this._buffer.Append($"\\u{(int)c:X4}");
          else
            this._buffer.Append(c);
          break;
      }
    this._buffer.Append('"');
  }

  /// <summary>
  /// Flushes any data that may have been buffered.
  /// </summary>
  public void Flush() {
    if (this._stream == null || this._buffer.Length == 0)
      return;

    var bytes = Encoding.UTF8.GetBytes(this._buffer.ToString());
    this._stream.Write(bytes, 0, bytes.Length);
    this.BytesCommitted += bytes.Length;
    this._buffer.Clear();
  }

  /// <summary>
  /// Releases the resources used by the <see cref="Utf8JsonWriter"/>.
  /// </summary>
  public void Dispose() {
    if (this._disposed)
      return;
    this.Flush();
    this._disposed = true;
  }

  /// <summary>
  /// Returns the current JSON output as a string.
  /// </summary>
  /// <returns>The JSON string.</returns>
  public override string ToString() => this._buffer.ToString();

}

/// <summary>
/// Provides options for writing JSON.
/// </summary>
public struct JsonWriterOptions {

  /// <summary>
  /// Gets or sets a value that indicates whether the writer should format the output with indentation.
  /// </summary>
  public bool Indented { get; set; }

  /// <summary>
  /// Gets or sets a value that indicates whether the writer should skip structural validation.
  /// </summary>
  public bool SkipValidation { get; set; }

}

#endif
