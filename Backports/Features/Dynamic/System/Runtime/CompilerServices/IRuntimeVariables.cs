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

namespace System.Runtime.CompilerServices;

/// <summary>
/// Provides access to runtime variables at specified indexes.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IRuntimeVariables"/> is used by the DLR to provide access to
/// variables captured in closures or passed to RuntimeVariablesExpression.
/// </para>
/// <para>
/// The indexer provides both read and write access to variables by their index.
/// </para>
/// </remarks>
public interface IRuntimeVariables {

  /// <summary>
  /// Gets the number of variables.
  /// </summary>
  /// <value>The count of variables accessible through this interface.</value>
  int Count { get; }

  /// <summary>
  /// Gets or sets the value of the variable at the specified index.
  /// </summary>
  /// <param name="index">The zero-based index of the variable.</param>
  /// <returns>The current value of the variable at the specified index.</returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="index"/> is less than 0 or greater than or equal to <see cref="Count"/>.
  /// </exception>
  object this[int index] { get; set; }

}

#endif
