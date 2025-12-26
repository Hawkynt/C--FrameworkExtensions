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
/// Represents a read-only collection of elements that can be accessed by index.
/// </summary>
/// <typeparam name="T">The type of elements in the read-only list.</typeparam>
/// <remarks>
/// Note: This polyfill does not use covariance (out T) because .NET 2.0/3.5
/// do not support generic variance. On .NET 4.5+, use the BCL version.
/// </remarks>
public interface IReadOnlyList<T> : IReadOnlyCollection<T> {
  /// <summary>
  /// Gets the element at the specified index in the read-only list.
  /// </summary>
  /// <param name="index">The zero-based index of the element to get.</param>
  /// <returns>The element at the specified index in the read-only list.</returns>
  T this[int index] { get; }
}

#endif
