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

namespace System.Text.Json;

/// <summary>
/// The exception thrown when JSON parsing fails.
/// </summary>
public class JsonException : Exception {

  /// <summary>
  /// Initializes a new instance of <see cref="JsonException"/>.
  /// </summary>
  public JsonException() { }

  /// <summary>
  /// Initializes a new instance of <see cref="JsonException"/> with a specified error message.
  /// </summary>
  /// <param name="message">The error message.</param>
  public JsonException(string? message) : base(message) { }

  /// <summary>
  /// Initializes a new instance of <see cref="JsonException"/> with a specified error message and inner exception.
  /// </summary>
  /// <param name="message">The error message.</param>
  /// <param name="innerException">The inner exception.</param>
  public JsonException(string? message, Exception? innerException) : base(message, innerException) { }

  /// <summary>
  /// Gets the line number where the error occurred.
  /// </summary>
  public long? LineNumber { get; protected set; }

  /// <summary>
  /// Gets the byte position in the line where the error occurred.
  /// </summary>
  public long? BytePositionInLine { get; protected set; }

  /// <summary>
  /// Gets the path to the JSON property where the error occurred.
  /// </summary>
  public string? Path { get; protected set; }

}

#endif
