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

#if !SUPPORTS_READ_ONLY_COLLECTIONS

namespace System.Collections.Generic;

/// <summary>
/// Represents a strongly-typed, read-only collection of elements.
/// </summary>
/// <typeparam name="T">The type of the elements.</typeparam>
/// <remarks>
/// Note: This polyfill does not use covariance (out T) because .NET 2.0/3.5
/// do not support generic variance. On .NET 4.0+, use the BCL version.
/// </remarks>
public interface IReadOnlyCollection<T> : IEnumerable<T> {
  /// <summary>
  /// Gets the number of elements in the collection.
  /// </summary>
  int Count { get; }
}

#endif
