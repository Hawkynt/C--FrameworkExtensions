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

#if !SUPPORTS_UNREACHABLE_EXCEPTION

namespace System.Diagnostics;

/// <summary>
/// Exception thrown when the program executes an instruction that was thought to be unreachable.
/// </summary>
#if SUPPORTS_SERIALIZATION
[Serializable]
#endif
public sealed class UnreachableException : Exception {

  private const string DEFAULT_MESSAGE = "The program executed an instruction that was thought to be unreachable.";

  /// <summary>
  /// Initializes a new instance of the <see cref="UnreachableException"/> class with a default message.
  /// </summary>
  public UnreachableException()
    : base(DEFAULT_MESSAGE) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="UnreachableException"/> class with a specified error message.
  /// </summary>
  /// <param name="message">The error message that explains the reason for the exception.</param>
  public UnreachableException(string message)
    : base(message ?? DEFAULT_MESSAGE) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="UnreachableException"/> class with a specified error message
  /// and a reference to the inner exception that is the cause of this exception.
  /// </summary>
  /// <param name="message">The error message that explains the reason for the exception.</param>
  /// <param name="innerException">The exception that is the cause of the current exception.</param>
  public UnreachableException(string message, Exception innerException)
    : base(message ?? DEFAULT_MESSAGE, innerException) { }

#if SUPPORTS_SERIALIZATION
  /// <summary>
  /// Initializes a new instance of the <see cref="UnreachableException"/> class with serialized data.
  /// </summary>
  /// <param name="info">The object that holds the serialized object data.</param>
  /// <param name="context">The contextual information about the source or destination.</param>
  private UnreachableException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
    : base(info, context) { }
#endif

}

#endif
