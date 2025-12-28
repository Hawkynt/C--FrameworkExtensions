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

#if !SUPPORTS_MATRIX3X2_CREATE

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Numerics;

/// <summary>
/// Polyfills for Matrix3x2 factory methods and row accessors added in .NET 10.
/// </summary>
public static partial class Matrix3x2Polyfills {

  #region Create Factory Methods

  extension(Matrix3x2) {

    /// <summary>
    /// Creates a Matrix3x2 with all elements initialized to the specified value.
    /// </summary>
    /// <param name="value">The value to assign to all elements.</param>
    /// <returns>A new Matrix3x2 with all elements set to <paramref name="value"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix3x2 Create(float value) => new(value, value, value, value, value, value);

    /// <summary>
    /// Creates a Matrix3x2 with all elements initialized from a Vector2.
    /// The X component is used for even columns, Y for odd columns.
    /// </summary>
    /// <param name="value">The vector value.</param>
    /// <returns>A new Matrix3x2.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix3x2 Create(Vector2 value) => new(value.X, value.Y, value.X, value.Y, value.X, value.Y);

    /// <summary>
    /// Creates a Matrix3x2 from three row vectors.
    /// </summary>
    /// <param name="x">The first row.</param>
    /// <param name="y">The second row.</param>
    /// <param name="z">The third row.</param>
    /// <returns>A new Matrix3x2.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix3x2 Create(Vector2 x, Vector2 y, Vector2 z) => new(x.X, x.Y, y.X, y.Y, z.X, z.Y);

    /// <summary>
    /// Creates a Matrix3x2 from the specified components.
    /// </summary>
    /// <param name="m11">The value for row 0, column 0.</param>
    /// <param name="m12">The value for row 0, column 1.</param>
    /// <param name="m21">The value for row 1, column 0.</param>
    /// <param name="m22">The value for row 1, column 1.</param>
    /// <param name="m31">The value for row 2, column 0.</param>
    /// <param name="m32">The value for row 2, column 1.</param>
    /// <returns>A new Matrix3x2.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix3x2 Create(float m11, float m12, float m21, float m22, float m31, float m32)
      => new(m11, m12, m21, m22, m31, m32);

  }

  #endregion

  #region Row Accessors

  /// <param name="this">The matrix.</param>
  extension(Matrix3x2 @this)
  {
    /// <summary>
    /// Gets the specified row of the matrix as a Vector2.
    /// </summary>
    /// <param name="index">The row index (0-2).</param>
    /// <returns>The row as a Vector2.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0 or greater than 2.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 GetRow(int index) {
      ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
      ArgumentOutOfRangeException.ThrowIfGreaterThan(index, 2, nameof(index));

      return index switch {
        0 => new(@this.M11, @this.M12),
        1 => new(@this.M21, @this.M22),
        _ => new(@this.M31, @this.M32)
      };
    }

    /// <summary>
    /// Creates a new Matrix3x2 with the specified row replaced.
    /// </summary>
    /// <param name="index">The row index (0-2).</param>
    /// <param name="value">The new row value.</param>
    /// <returns>A new matrix with the specified row changed.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0 or greater than 2.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix3x2 WithRow(int index, Vector2 value) {
      ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
      ArgumentOutOfRangeException.ThrowIfGreaterThan(index, 2, nameof(index));

      var result = @this;
      switch (index) {
        case 0:
          result.M11 = value.X;
          result.M12 = value.Y;
          break;
        case 1:
          result.M21 = value.X;
          result.M22 = value.Y;
          break;
        default:
          result.M31 = value.X;
          result.M32 = value.Y;
          break;
      }
      return result;
    }
  }

  #endregion

  #region Row Properties

  extension(Matrix3x2 @this) {

    /// <summary>
    /// Gets or sets the first row of the matrix.
    /// </summary>
    public Vector2 X {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => new(@this.M11, @this.M12);
    }

    /// <summary>
    /// Gets or sets the second row of the matrix.
    /// </summary>
    public Vector2 Y {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => new(@this.M21, @this.M22);
    }

    /// <summary>
    /// Gets or sets the third row of the matrix.
    /// </summary>
    public Vector2 Z {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => new(@this.M31, @this.M32);
    }

  }

  /// <param name="this">The matrix.</param>
  extension(Matrix3x2 @this)
  {
    /// <summary>
    /// Creates a new Matrix3x2 with the first row replaced.
    /// </summary>
    /// <param name="value">The new first row.</param>
    /// <returns>A new matrix with the first row changed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix3x2 WithX(Vector2 value)
      => new(value.X, value.Y, @this.M21, @this.M22, @this.M31, @this.M32);

    /// <summary>
    /// Creates a new Matrix3x2 with the second row replaced.
    /// </summary>
    /// <param name="value">The new second row.</param>
    /// <returns>A new matrix with the second row changed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix3x2 WithY(Vector2 value)
      => new(@this.M11, @this.M12, value.X, value.Y, @this.M31, @this.M32);

    /// <summary>
    /// Creates a new Matrix3x2 with the third row replaced.
    /// </summary>
    /// <param name="value">The new third row.</param>
    /// <returns>A new matrix with the third row changed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix3x2 WithZ(Vector2 value)
      => new(@this.M11, @this.M12, @this.M21, @this.M22, value.X, value.Y);
  }

  #endregion

}

#endif
