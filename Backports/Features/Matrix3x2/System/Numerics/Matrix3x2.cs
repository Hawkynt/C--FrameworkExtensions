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

// Only compile when no vector support exists (net20, net35, net40)
#if !(SUPPORTS_VECTOR || OFFICIAL_VECTOR)

using System.Globalization;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Numerics;

/// <summary>
/// Represents a 3x2 matrix.
/// </summary>
public struct Matrix3x2 : IEquatable<Matrix3x2>
#if SUPPORTS_SPAN_FORMATTABLE
  , IFormattable
#endif
{
  /// <summary>The first element of the first row.</summary>
  public float M11;
  /// <summary>The second element of the first row.</summary>
  public float M12;
  /// <summary>The first element of the second row.</summary>
  public float M21;
  /// <summary>The second element of the second row.</summary>
  public float M22;
  /// <summary>The first element of the third row.</summary>
  public float M31;
  /// <summary>The second element of the third row.</summary>
  public float M32;

  /// <summary>
  /// Creates a 3x2 matrix from the specified components.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Matrix3x2(float m11, float m12, float m21, float m22, float m31, float m32) {
    this.M11 = m11;
    this.M12 = m12;
    this.M21 = m21;
    this.M22 = m22;
    this.M31 = m31;
    this.M32 = m32;
  }

  #region Static Properties

  /// <summary>Gets the multiplicative identity matrix.</summary>
  public static Matrix3x2 Identity {
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    get => new(1f, 0f, 0f, 1f, 0f, 0f);
  }

  #endregion

  #region Instance Properties

  /// <summary>Gets a value that indicates whether the current matrix is the identity matrix.</summary>
  public readonly bool IsIdentity => this.M11 == 1f && this.M22 == 1f && this.M12 == 0f && this.M21 == 0f && this.M31 == 0f && this.M32 == 0f;

  /// <summary>Gets or sets the translation component of this matrix.</summary>
  public Vector2 Translation {
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    readonly get => new(this.M31, this.M32);
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    set {
      this.M31 = value.X;
      this.M32 = value.Y;
    }
  }

  #endregion

  #region Static Methods

  /// <summary>Adds each element in one matrix with its corresponding element in a second matrix.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 Add(Matrix3x2 value1, Matrix3x2 value2) => value1 + value2;

  /// <summary>Creates a rotation matrix using the given rotation in radians.</summary>
  /// <param name="radians">The amount of rotation, in radians.</param>
  /// <returns>The rotation matrix.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 CreateRotation(float radians) {
    var c = (float)Math.Cos(radians);
    var s = (float)Math.Sin(radians);
    return new(c, s, -s, c, 0f, 0f);
  }

  /// <summary>Creates a rotation matrix using the specified rotation in radians and a center point.</summary>
  /// <param name="radians">The amount of rotation, in radians.</param>
  /// <param name="centerPoint">The center point.</param>
  /// <returns>The rotation matrix.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 CreateRotation(float radians, Vector2 centerPoint) {
    var c = (float)Math.Cos(radians);
    var s = (float)Math.Sin(radians);
    var x = centerPoint.X * (1f - c) + centerPoint.Y * s;
    var y = centerPoint.Y * (1f - c) - centerPoint.X * s;
    return new(c, s, -s, c, x, y);
  }

  /// <summary>Creates a scaling matrix from the specified vector scale.</summary>
  /// <param name="scales">The scale to use.</param>
  /// <returns>The scaling matrix.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 CreateScale(Vector2 scales) => new(scales.X, 0f, 0f, scales.Y, 0f, 0f);

  /// <summary>Creates a scaling matrix from the specified X and Y components.</summary>
  /// <param name="xScale">The value to scale by on the X axis.</param>
  /// <param name="yScale">The value to scale by on the Y axis.</param>
  /// <returns>The scaling matrix.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 CreateScale(float xScale, float yScale) => new(xScale, 0f, 0f, yScale, 0f, 0f);

  /// <summary>Creates a scaling matrix that scales uniformly with the specified scale.</summary>
  /// <param name="scale">The uniform scale to use.</param>
  /// <returns>The scaling matrix.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 CreateScale(float scale) => new(scale, 0f, 0f, scale, 0f, 0f);

  /// <summary>Creates a scaling matrix from the specified vector scale with an offset from the specified center point.</summary>
  /// <param name="scales">The scale to use.</param>
  /// <param name="centerPoint">The center offset.</param>
  /// <returns>The scaling matrix.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 CreateScale(Vector2 scales, Vector2 centerPoint) => new(
    scales.X, 0f,
    0f, scales.Y,
    centerPoint.X * (1f - scales.X), centerPoint.Y * (1f - scales.Y)
  );

  /// <summary>Creates a scaling matrix from the specified X and Y components with an offset from the specified center point.</summary>
  /// <param name="xScale">The value to scale by on the X axis.</param>
  /// <param name="yScale">The value to scale by on the Y axis.</param>
  /// <param name="centerPoint">The center offset.</param>
  /// <returns>The scaling matrix.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 CreateScale(float xScale, float yScale, Vector2 centerPoint) => new(
    xScale, 0f,
    0f, yScale,
    centerPoint.X * (1f - xScale), centerPoint.Y * (1f - yScale)
  );

  /// <summary>Creates a scaling matrix that scales uniformly with the given scale with an offset from the specified center.</summary>
  /// <param name="scale">The uniform scale to use.</param>
  /// <param name="centerPoint">The center offset.</param>
  /// <returns>The scaling matrix.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 CreateScale(float scale, Vector2 centerPoint) => new(
    scale, 0f,
    0f, scale,
    centerPoint.X * (1f - scale), centerPoint.Y * (1f - scale)
  );

  /// <summary>Creates a skew matrix from the specified angles in radians.</summary>
  /// <param name="radiansX">The X angle, in radians.</param>
  /// <param name="radiansY">The Y angle, in radians.</param>
  /// <returns>The skew matrix.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 CreateSkew(float radiansX, float radiansY) => new(
    1f, (float)Math.Tan(radiansY),
    (float)Math.Tan(radiansX), 1f,
    0f, 0f
  );

  /// <summary>Creates a skew matrix from the specified angles in radians and a center point.</summary>
  /// <param name="radiansX">The X angle, in radians.</param>
  /// <param name="radiansY">The Y angle, in radians.</param>
  /// <param name="centerPoint">The center point.</param>
  /// <returns>The skew matrix.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 CreateSkew(float radiansX, float radiansY, Vector2 centerPoint) {
    var xTan = (float)Math.Tan(radiansX);
    var yTan = (float)Math.Tan(radiansY);
    return new(
      1f, yTan,
      xTan, 1f,
      -centerPoint.Y * xTan, -centerPoint.X * yTan
    );
  }

  /// <summary>Creates a translation matrix from the specified 2-dimensional vector.</summary>
  /// <param name="position">The translation position.</param>
  /// <returns>The translation matrix.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 CreateTranslation(Vector2 position) => new(1f, 0f, 0f, 1f, position.X, position.Y);

  /// <summary>Creates a translation matrix from the specified X and Y components.</summary>
  /// <param name="xPosition">The X position.</param>
  /// <param name="yPosition">The Y position.</param>
  /// <returns>The translation matrix.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 CreateTranslation(float xPosition, float yPosition) => new(1f, 0f, 0f, 1f, xPosition, yPosition);

  /// <summary>Calculates the determinant of the current 3x2 matrix.</summary>
  /// <returns>The determinant.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly float GetDeterminant() => this.M11 * this.M22 - this.M21 * this.M12;

  /// <summary>Attempts to invert the specified matrix.</summary>
  /// <param name="matrix">The matrix to invert.</param>
  /// <param name="result">When this method returns, contains the inverted matrix if the operation succeeded.</param>
  /// <returns><see langword="true"/> if the matrix was inverted successfully; otherwise, <see langword="false"/>.</returns>
  public static bool Invert(Matrix3x2 matrix, out Matrix3x2 result) {
    var det = matrix.GetDeterminant();

    if (Math.Abs(det) < float.Epsilon) {
      result = new(float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN);
      return false;
    }

    var invDet = 1f / det;
    result = new(
      matrix.M22 * invDet,
      -matrix.M12 * invDet,
      -matrix.M21 * invDet,
      matrix.M11 * invDet,
      (matrix.M21 * matrix.M32 - matrix.M31 * matrix.M22) * invDet,
      (matrix.M31 * matrix.M12 - matrix.M11 * matrix.M32) * invDet
    );
    return true;
  }

  /// <summary>Performs a linear interpolation from one matrix to a second matrix based on a value that specifies the weighting of the second matrix.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 Lerp(Matrix3x2 matrix1, Matrix3x2 matrix2, float amount) => new(
    matrix1.M11 + (matrix2.M11 - matrix1.M11) * amount,
    matrix1.M12 + (matrix2.M12 - matrix1.M12) * amount,
    matrix1.M21 + (matrix2.M21 - matrix1.M21) * amount,
    matrix1.M22 + (matrix2.M22 - matrix1.M22) * amount,
    matrix1.M31 + (matrix2.M31 - matrix1.M31) * amount,
    matrix1.M32 + (matrix2.M32 - matrix1.M32) * amount
  );

  /// <summary>Returns the matrix that results from multiplying two matrices together.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 Multiply(Matrix3x2 value1, Matrix3x2 value2) => value1 * value2;

  /// <summary>Returns the matrix that results from scaling all the elements of a specified matrix by a scalar factor.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 Multiply(Matrix3x2 value1, float value2) => value1 * value2;

  /// <summary>Negates the specified matrix by multiplying all its values by -1.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 Negate(Matrix3x2 value) => -value;

  /// <summary>Subtracts each element in a second matrix from its corresponding element in a first matrix.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 Subtract(Matrix3x2 value1, Matrix3x2 value2) => value1 - value2;

  #endregion

  #region Operators

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 operator +(Matrix3x2 value1, Matrix3x2 value2) => new(
    value1.M11 + value2.M11, value1.M12 + value2.M12,
    value1.M21 + value2.M21, value1.M22 + value2.M22,
    value1.M31 + value2.M31, value1.M32 + value2.M32
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 operator -(Matrix3x2 value1, Matrix3x2 value2) => new(
    value1.M11 - value2.M11, value1.M12 - value2.M12,
    value1.M21 - value2.M21, value1.M22 - value2.M22,
    value1.M31 - value2.M31, value1.M32 - value2.M32
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 operator *(Matrix3x2 value1, Matrix3x2 value2) => new(
    value1.M11 * value2.M11 + value1.M12 * value2.M21,
    value1.M11 * value2.M12 + value1.M12 * value2.M22,
    value1.M21 * value2.M11 + value1.M22 * value2.M21,
    value1.M21 * value2.M12 + value1.M22 * value2.M22,
    value1.M31 * value2.M11 + value1.M32 * value2.M21 + value2.M31,
    value1.M31 * value2.M12 + value1.M32 * value2.M22 + value2.M32
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 operator *(Matrix3x2 value1, float value2) => new(
    value1.M11 * value2, value1.M12 * value2,
    value1.M21 * value2, value1.M22 * value2,
    value1.M31 * value2, value1.M32 * value2
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix3x2 operator -(Matrix3x2 value) => new(
    -value.M11, -value.M12,
    -value.M21, -value.M22,
    -value.M31, -value.M32
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator ==(Matrix3x2 value1, Matrix3x2 value2) => value1.Equals(value2);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator !=(Matrix3x2 value1, Matrix3x2 value2) => !value1.Equals(value2);

  #endregion

  #region Equality

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly bool Equals(Matrix3x2 other) =>
    this.M11 == other.M11 && this.M12 == other.M12 &&
    this.M21 == other.M21 && this.M22 == other.M22 &&
    this.M31 == other.M31 && this.M32 == other.M32;

  /// <inheritdoc/>
  public override readonly bool Equals(object obj) => obj is Matrix3x2 other && this.Equals(other);

  /// <inheritdoc/>
  public override readonly int GetHashCode() =>
    this.M11.GetHashCode() ^ (this.M12.GetHashCode() << 2) ^
    (this.M21.GetHashCode() >> 2) ^ (this.M22.GetHashCode() >> 1) ^
    (this.M31.GetHashCode() << 1) ^ this.M32.GetHashCode();

  #endregion

  /// <inheritdoc/>
  public override readonly string ToString() => this.ToString("G", CultureInfo.CurrentCulture);

  /// <summary>Returns a string representation of the current instance using the specified format string.</summary>
  public readonly string ToString(string format) => this.ToString(format, CultureInfo.CurrentCulture);

  /// <summary>Returns a string representation of the current instance using the specified format string and format provider.</summary>
  public readonly string ToString(string format, IFormatProvider formatProvider) {
    var separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;
    return $"{{ {{M11:{this.M11.ToString(format, formatProvider)}{separator} M12:{this.M12.ToString(format, formatProvider)}}} {{M21:{this.M21.ToString(format, formatProvider)}{separator} M22:{this.M22.ToString(format, formatProvider)}}} {{M31:{this.M31.ToString(format, formatProvider)}{separator} M32:{this.M32.ToString(format, formatProvider)}}} }}";
  }
}

#endif
