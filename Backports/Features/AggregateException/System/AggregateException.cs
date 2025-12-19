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

#if !SUPPORTS_AGGREGATEEXCEPTION

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace System;

/// <summary>
/// Represents one or more errors that occur during application execution.
/// </summary>
public class AggregateException : Exception {
  /// <summary>
  /// Gets a read-only collection of the <see cref="Exception"/> instances that caused the current exception.
  /// </summary>
  public ReadOnlyCollection<Exception> InnerExceptions { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="AggregateException"/> class.
  /// </summary>
  public AggregateException()
    : this("One or more errors occurred.") { }

  /// <summary>
  /// Initializes a new instance of the <see cref="AggregateException"/> class with a specified error message.
  /// </summary>
  /// <param name="message">The message that describes the exception.</param>
  public AggregateException(string message)
    : base(message)
    => this.InnerExceptions = new([]);

  /// <summary>
  /// Initializes a new instance of the <see cref="AggregateException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
  /// </summary>
  /// <param name="message">The message that describes the exception.</param>
  /// <param name="innerException">The exception that is the cause of the current exception.</param>
  public AggregateException(string message, Exception innerException)
    : base(message, innerException)
    => this.InnerExceptions = new([innerException]);

  /// <summary>
  /// Initializes a new instance of the <see cref="AggregateException"/> class with references to the inner exceptions that are the cause of this exception.
  /// </summary>
  /// <param name="innerExceptions">The exceptions that are the cause of the current exception.</param>
  public AggregateException(IEnumerable<Exception> innerExceptions)
    : this("One or more errors occurred.", innerExceptions) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="AggregateException"/> class with references to the inner exceptions that are the cause of this exception.
  /// </summary>
  /// <param name="innerExceptions">The exceptions that are the cause of the current exception.</param>
  public AggregateException(params Exception[] innerExceptions)
    : this("One or more errors occurred.", (IEnumerable<Exception>)innerExceptions) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="AggregateException"/> class with a specified error message and references to the inner exceptions that are the cause of this exception.
  /// </summary>
  /// <param name="message">The error message that explains the reason for the exception.</param>
  /// <param name="innerExceptions">The exceptions that are the cause of the current exception.</param>
  public AggregateException(string message, IEnumerable<Exception> innerExceptions)
    : base(message, _GetFirstException(innerExceptions)) {
    var list = new List<Exception>();
    foreach (var ex in innerExceptions) {
      if (ex == null)
        throw new ArgumentException("An element of innerExceptions is null.", nameof(innerExceptions));

      list.Add(ex);
    }
    this.InnerExceptions = new(list);
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="AggregateException"/> class with a specified error message and references to the inner exceptions that are the cause of this exception.
  /// </summary>
  /// <param name="message">The error message that explains the reason for the exception.</param>
  /// <param name="innerExceptions">The exceptions that are the cause of the current exception.</param>
  public AggregateException(string message, params Exception[] innerExceptions)
    : this(message, (IEnumerable<Exception>)innerExceptions) { }

  private static Exception? _GetFirstException(IEnumerable<Exception> exceptions) {
    ArgumentNullException.ThrowIfNull(exceptions);

    foreach (var ex in exceptions)
      return ex;

    return null;
  }

  /// <summary>
  /// Returns a string that represents the current exception.
  /// </summary>
  public override string ToString() {
    var sb = new StringBuilder();
    sb.Append(base.ToString());

    for (var i = 0; i < this.InnerExceptions.Count; ++i) {
      sb.AppendLine();
      sb.Append("---> (Inner Exception #");
      sb.Append(i);
      sb.Append(") ");
      sb.Append(this.InnerExceptions[i]);
      sb.Append("<---");
    }

    return sb.ToString();
  }

  /// <summary>
  /// Flattens an <see cref="AggregateException"/> instances into a single, new instance.
  /// </summary>
  /// <returns>A new, flattened <see cref="AggregateException"/>.</returns>
  public AggregateException Flatten() {
    var flattenedExceptions = new List<Exception>();
    var exceptionsToFlatten = new Queue<AggregateException>();
    exceptionsToFlatten.Enqueue(this);

    while (exceptionsToFlatten.Count > 0) {
      var current = exceptionsToFlatten.Dequeue();
      foreach (var inner in current.InnerExceptions)
        if (inner is AggregateException aggregate)
          exceptionsToFlatten.Enqueue(aggregate);
        else
          flattenedExceptions.Add(inner);
    }

    return new AggregateException(this.Message, flattenedExceptions);
  }

  /// <summary>
  /// Invokes a handler on each <see cref="Exception"/> contained by this <see cref="AggregateException"/>.
  /// </summary>
  /// <param name="predicate">The predicate to execute for each exception.</param>
  public void Handle(Func<Exception, bool> predicate) {
    ArgumentNullException.ThrowIfNull(predicate);

    var unhandled = new List<Exception>();
    foreach (var inner in this.InnerExceptions)
      if (!predicate(inner))
        unhandled.Add(inner);

    if (unhandled.Count > 0)
      throw new AggregateException(this.Message, unhandled);
  }

}

#endif
