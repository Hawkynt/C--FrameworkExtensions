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
#if !SUPPORTS_MATRIX4X4_INDEXER

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Numerics;

/// <summary>
/// Polyfills for Matrix4x4 indexer access (Item[row, column]) added in .NET 7.0.
/// </summary>
public static partial class Matrix4x4Polyfills {
  /// <param name="this">The matrix.</param>
  extension(Matrix4x4 @this)
  {
    /// <summary>
    /// Gets the element at the specified row and column.
    /// </summary>
    /// <param name="row">The row index (0-3).</param>
    /// <param name="column">The column index (0-3).</param>
    /// <returns>The element at the specified position.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="row"/> or <paramref name="column"/> is less than 0 or greater than 3.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float get_Item(int row, int column) {
      if ((uint)row >= 4)
        throw new ArgumentOutOfRangeException(nameof(row));
      if ((uint)column >= 4)
        throw new ArgumentOutOfRangeException(nameof(column));

      return row switch {
        0 => column switch { 0 => @this.M11, 1 => @this.M12, 2 => @this.M13, _ => @this.M14 },
        1 => column switch { 0 => @this.M21, 1 => @this.M22, 2 => @this.M23, _ => @this.M24 },
        2 => column switch { 0 => @this.M31, 1 => @this.M32, 2 => @this.M33, _ => @this.M34 },
        _ => column switch { 0 => @this.M41, 1 => @this.M42, 2 => @this.M43, _ => @this.M44 }
      };
    }
    
  }
}

#endif
