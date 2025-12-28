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
//

// Requires System.Numerics.Vectors (SUPPORTS_VECTOR or OFFICIAL_VECTOR) for base types
#if !SUPPORTS_QUATERNION_ZERO_INDEXER

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Numerics;

/// <summary>
/// Polyfills for Quaternion Zero property and indexer added in .NET 7.0.
/// </summary>
public static partial class QuaternionPolyfills {

  extension(Quaternion) {

    /// <summary>
    /// Gets a quaternion representing no rotation (0, 0, 0, 0).
    /// </summary>
    public static Quaternion Zero {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => new(0, 0, 0, 0);
    }

  }

  /// <param name="this">The quaternion.</param>
  extension(Quaternion @this)
  {
    /// <summary>
    /// Gets the element at the specified index (indexer polyfill).
    /// </summary>
    /// <param name="index">The index (0=X, 1=Y, 2=Z, 3=W).</param>
    /// <returns>The element at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0 or greater than 3.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float get_Item(int index) {
      ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
      ArgumentOutOfRangeException.ThrowIfGreaterThan(index, 3, nameof(index));

      return index switch {
        0 => @this.X,
        1 => @this.Y,
        2 => @this.Z,
        _ => @this.W
      };
    }
  }
}

#endif
