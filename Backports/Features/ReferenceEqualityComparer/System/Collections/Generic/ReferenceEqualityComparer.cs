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

// ReferenceEqualityComparer was added in .NET 5.0
#if !SUPPORTS_REFERENCE_EQUALITY_COMPARER

#pragma warning disable CS0436 // Type conflicts with imported type - intentional use of RuntimeHelpers polyfill

using System.Runtime.CompilerServices;

namespace System.Collections.Generic;

/// <summary>
/// An <see cref="IEqualityComparer{T}"/> that uses reference equality (<see cref="object.ReferenceEquals(object, object)"/>)
/// instead of value equality (<see cref="object.Equals(object)"/>) when comparing two object instances.
/// </summary>
public sealed class ReferenceEqualityComparer : IEqualityComparer<object?>, IEqualityComparer {

  /// <summary>
  /// Gets the singleton instance of the <see cref="ReferenceEqualityComparer"/> class.
  /// </summary>
  public static ReferenceEqualityComparer Instance { get; } = new();

  private ReferenceEqualityComparer() { }

  /// <summary>
  /// Determines whether two object references refer to the same object instance.
  /// </summary>
  /// <param name="x">The first object to compare.</param>
  /// <param name="y">The second object to compare.</param>
  /// <returns><see langword="true"/> if both <paramref name="x"/> and <paramref name="y"/> refer to the same object instance
  /// or if both are <see langword="null"/>; otherwise, <see langword="false"/>.</returns>
  public new bool Equals(object? x, object? y)
    => ReferenceEquals(x, y);

  /// <summary>
  /// Returns a hash code for the specified object. The hash code is based on the object's reference identity.
  /// </summary>
  /// <param name="obj">The object for which to get a hash code.</param>
  /// <returns>A hash code for the specified object.</returns>
  public int GetHashCode(object? obj)
    => RuntimeHelpers.GetHashCode(obj!);

}

#pragma warning restore CS0436

#endif
