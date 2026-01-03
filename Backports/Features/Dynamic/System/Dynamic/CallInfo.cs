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

// System.Dynamic was introduced in .NET 4.0
// Only polyfill for net20/net35 where no DLR exists
#if !SUPPORTS_DYNAMIC

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Dynamic;

/// <summary>
/// Describes the arguments passed to a dynamic operation.
/// </summary>
/// <remarks>
/// <see cref="CallInfo"/> is used by binders to describe the arguments passed to dynamic operations
/// like method calls, indexer access, and constructor invocations. It contains both the total number
/// of arguments and the names of any named arguments.
/// </remarks>
public sealed class CallInfo {

  private readonly ReadOnlyCollection<string> _argumentNames;

  /// <summary>
  /// Initializes a new instance of the <see cref="CallInfo"/> class.
  /// </summary>
  /// <param name="argCount">The number of arguments.</param>
  /// <param name="argNames">The names of the named arguments (may be empty but not null).</param>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="argCount"/> is less than zero.
  /// </exception>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="argNames"/> is null.
  /// </exception>
  /// <exception cref="ArgumentException">
  /// The number of named arguments is greater than the total number of arguments.
  /// </exception>
  public CallInfo(int argCount, params string[] argNames)
    : this(argCount, (IEnumerable<string>)argNames) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="CallInfo"/> class.
  /// </summary>
  /// <param name="argCount">The number of arguments.</param>
  /// <param name="argNames">The names of the named arguments.</param>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="argCount"/> is less than zero.
  /// </exception>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="argNames"/> is null.
  /// </exception>
  /// <exception cref="ArgumentException">
  /// The number of named arguments is greater than the total number of arguments.
  /// </exception>
  public CallInfo(int argCount, IEnumerable<string> argNames) {
    if (argCount < 0)
      throw new ArgumentOutOfRangeException(nameof(argCount), "Argument count must be non-negative.");
    if (argNames == null)
      throw new ArgumentNullException(nameof(argNames));

    var namesList = new List<string>(argNames);
    if (namesList.Count > argCount)
      throw new ArgumentException("The number of named arguments cannot exceed the total number of arguments.", nameof(argNames));

    this.ArgumentCount = argCount;
    this._argumentNames = new ReadOnlyCollection<string>(namesList);
  }

  /// <summary>
  /// Gets the number of arguments.
  /// </summary>
  /// <value>The total number of arguments (both positional and named).</value>
  public int ArgumentCount { get; }

  /// <summary>
  /// Gets the names of the named arguments.
  /// </summary>
  /// <value>
  /// A read-only collection containing the names of the named arguments.
  /// Named arguments are positioned at the end of the argument list.
  /// </value>
  public ReadOnlyCollection<string> ArgumentNames => this._argumentNames;

  /// <summary>
  /// Determines whether the specified object is equal to the current <see cref="CallInfo"/>.
  /// </summary>
  /// <param name="obj">The object to compare with the current instance.</param>
  /// <returns>
  /// <see langword="true"/> if the specified object is a <see cref="CallInfo"/> with the same
  /// argument count and argument names; otherwise, <see langword="false"/>.
  /// </returns>
  public override bool Equals(object obj) {
    if (obj is not CallInfo other)
      return false;
    if (this.ArgumentCount != other.ArgumentCount)
      return false;
    if (this._argumentNames.Count != other._argumentNames.Count)
      return false;
    for (var i = 0; i < this._argumentNames.Count; ++i)
      if (this._argumentNames[i] != other._argumentNames[i])
        return false;
    return true;
  }

  /// <summary>
  /// Returns a hash code for the current <see cref="CallInfo"/>.
  /// </summary>
  /// <returns>A hash code for the current instance.</returns>
  public override int GetHashCode() {
    var hash = this.ArgumentCount;
    foreach (var name in this._argumentNames)
      hash = hash * 31 + (name?.GetHashCode() ?? 0);
    return hash;
  }

}

#endif
