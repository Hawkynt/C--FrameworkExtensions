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
#if (SUPPORTS_VECTOR || OFFICIAL_VECTOR) && !SUPPORTS_QUATERNION_ZERO_INDEXER

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Numerics;

/// <summary>
/// Polyfills for Quaternion Zero property and indexer added in .NET 7.0.
/// </summary>
public static partial class QuaternionPolyfills {

  /// <summary>
  /// Gets a quaternion representing no rotation (0, 0, 0, 0).
  /// </summary>
  public static Quaternion Zero {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => new(0, 0, 0, 0);
  }

  /// <summary>
  /// Gets the element at the specified index.
  /// </summary>
  /// <param name="this">The quaternion.</param>
  /// <param name="index">The index (0=X, 1=Y, 2=Z, 3=W).</param>
  /// <returns>The element at the specified index.</returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="index"/> is less than 0 or greater than 3.
  /// </exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float GetElement(this Quaternion @this, int index)
    => index switch {
      0 => @this.X,
      1 => @this.Y,
      2 => @this.Z,
      3 => @this.W,
      _ => throw new ArgumentOutOfRangeException(nameof(index))
    };

  /// <summary>
  /// Creates a new Quaternion with the element at the specified index replaced.
  /// </summary>
  /// <param name="this">The quaternion.</param>
  /// <param name="index">The index (0=X, 1=Y, 2=Z, 3=W).</param>
  /// <param name="value">The new value.</param>
  /// <returns>A new quaternion with the specified element changed.</returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="index"/> is less than 0 or greater than 3.
  /// </exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion WithElement(this Quaternion @this, int index, float value) {
    var result = @this;
    switch (index) {
      case 0: result.X = value; break;
      case 1: result.Y = value; break;
      case 2: result.Z = value; break;
      case 3: result.W = value; break;
      default: throw new ArgumentOutOfRangeException(nameof(index));
    }
    return result;
  }

}

#endif
