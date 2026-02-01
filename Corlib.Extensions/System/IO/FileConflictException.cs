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

namespace System.IO;

/// <summary>
///   The exception that is thrown when a file conflict is detected during a work-in-progress operation.
/// </summary>
/// <remarks>
///   This exception is thrown when a file has been modified externally while a work-in-progress operation
///   was active, and the <see cref="ConflictResolutionMode" /> is set to throw on conflict detection.
/// </remarks>
public class FileConflictException : IOException {
  /// <summary>
  ///   Initializes a new instance of the <see cref="FileConflictException" /> class with a specified error message.
  /// </summary>
  /// <param name="message">The message that describes the error.</param>
  public FileConflictException(string message) : base(message) { }

  /// <summary>
  ///   Initializes a new instance of the <see cref="FileConflictException" /> class with a specified error message
  ///   and a reference to the inner exception that is the cause of this exception.
  /// </summary>
  /// <param name="message">The error message that explains the reason for the exception.</param>
  /// <param name="innerException">
  ///   The exception that is the cause of the current exception.
  ///   If the <paramref name="innerException" /> parameter is not <see langword="null" />,
  ///   the current exception is raised in a <see langword="catch" /> block that handles the inner exception.
  /// </param>
  public FileConflictException(string message, Exception innerException) : base(message, innerException) { }
}
