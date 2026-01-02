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

// LockRecursionException was added in .NET 3.5 (System.Core)
// Only polyfill for net20 where it doesn't exist
#if !SUPPORTS_LINQ

namespace System.Threading;

/// <summary>
/// The exception that is thrown when recursive entry into a lock is not allowed.
/// </summary>
public class LockRecursionException : Exception {

  /// <summary>
  /// Initializes a new instance of the <see cref="LockRecursionException"/> class with a default message.
  /// </summary>
  public LockRecursionException()
    : base("Recursive locking is not allowed.") { }

  /// <summary>
  /// Initializes a new instance of the <see cref="LockRecursionException"/> class with a specified message.
  /// </summary>
  /// <param name="message">The message that describes the error.</param>
  public LockRecursionException(string message)
    : base(message) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="LockRecursionException"/> class with a specified message
  /// and a reference to the inner exception that is the cause of this exception.
  /// </summary>
  /// <param name="message">The message that describes the error.</param>
  /// <param name="innerException">The exception that is the cause of the current exception.</param>
  public LockRecursionException(string message, Exception innerException)
    : base(message, innerException) { }

}

#endif
