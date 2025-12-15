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

#if !SUPPORTS_MATRIX3X2_INDEXER

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Numerics;

/// <summary>
/// Polyfills for Matrix3x2 indexer access (Item[row, column]) added in .NET 7.0.
/// </summary>
public static partial class Matrix3x2Polyfills {
  /// <param name="this">The matrix.</param>
  extension(Matrix3x2 @this) {
    /// <summary>
    /// Gets the element at the specified row and column.
    /// </summary>
    /// <param name="row">The row index (0-2).</param>
    /// <param name="column">The column index (0-1).</param>
    /// <returns>The element at the specified position.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="row"/> is less than 0 or greater than 2, or
    /// <paramref name="column"/> is less than 0 or greater than 1.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float get_Item(int row, int column) {
      ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, 3, nameof(row));
      ArgumentOutOfRangeException.ThrowIfNegative(row, nameof(row));
      ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(column, 2, nameof(column));
      ArgumentOutOfRangeException.ThrowIfNegative(column, nameof(column));

      return row switch {
        0 => column == 0 ? @this.M11 : @this.M12,
        1 => column == 0 ? @this.M21 : @this.M22,
        _ => column == 0 ? @this.M31 : @this.M32
      };
    }

  }
}

#endif
