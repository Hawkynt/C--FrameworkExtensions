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
#if !SUPPORTS_MATRIX4X4_CREATE

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Numerics;

/// <summary>
/// Polyfills for Matrix4x4 factory methods and row accessors added in .NET 10.
/// </summary>
public static partial class Matrix4x4Polyfills {

  #region Create Factory Methods

  /// <summary>
  /// Creates a Matrix4x4 from a Matrix3x2.
  /// </summary>
  /// <param name="value">The source matrix.</param>
  /// <returns>A new Matrix4x4.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 Create(Matrix3x2 value) => new(value);

  /// <summary>
  /// Creates a Matrix4x4 with all elements initialized to the specified value.
  /// </summary>
  /// <param name="value">The value to assign to all elements.</param>
  /// <returns>A new Matrix4x4 with all elements set to <paramref name="value"/>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 Create(float value) => new(
    value, value, value, value,
    value, value, value, value,
    value, value, value, value,
    value, value, value, value
  );

  /// <summary>
  /// Creates a Matrix4x4 from a Vector4 (broadcast to all rows).
  /// </summary>
  /// <param name="value">The vector value.</param>
  /// <returns>A new Matrix4x4.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 Create(Vector4 value) => new(
    value.X, value.Y, value.Z, value.W,
    value.X, value.Y, value.Z, value.W,
    value.X, value.Y, value.Z, value.W,
    value.X, value.Y, value.Z, value.W
  );

  /// <summary>
  /// Creates a Matrix4x4 from four row vectors.
  /// </summary>
  /// <param name="x">The first row.</param>
  /// <param name="y">The second row.</param>
  /// <param name="z">The third row.</param>
  /// <param name="w">The fourth row.</param>
  /// <returns>A new Matrix4x4.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 Create(Vector4 x, Vector4 y, Vector4 z, Vector4 w) => new(
    x.X, x.Y, x.Z, x.W,
    y.X, y.Y, y.Z, y.W,
    z.X, z.Y, z.Z, z.W,
    w.X, w.Y, w.Z, w.W
  );

  /// <summary>
  /// Creates a Matrix4x4 from the specified components.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 Create(
    float m11, float m12, float m13, float m14,
    float m21, float m22, float m23, float m24,
    float m31, float m32, float m33, float m34,
    float m41, float m42, float m43, float m44
  ) => new(
    m11, m12, m13, m14,
    m21, m22, m23, m24,
    m31, m32, m33, m34,
    m41, m42, m43, m44
  );

  /// <summary>
  /// Creates a left-handed billboard matrix.
  /// </summary>
  /// <param name="objectPosition">The position of the object.</param>
  /// <param name="cameraPosition">The position of the camera.</param>
  /// <param name="cameraUpVector">The up vector of the camera.</param>
  /// <param name="cameraForwardVector">The forward vector of the camera.</param>
  /// <returns>The billboard matrix.</returns>
  public static Matrix4x4 CreateBillboardLeftHanded(Vector3 objectPosition, Vector3 cameraPosition, Vector3 cameraUpVector, Vector3 cameraForwardVector) {
    var zAxis = objectPosition - cameraPosition;
    var norm = zAxis.LengthSquared();

    zAxis = norm < 0.0001f ? -cameraForwardVector : zAxis * (1.0f / MathF.Sqrt(norm));

    var xAxis = Vector3.Normalize(Vector3.Cross(cameraUpVector, zAxis));
    var yAxis = Vector3.Cross(zAxis, xAxis);

    return new(
      xAxis.X, xAxis.Y, xAxis.Z, 0,
      yAxis.X, yAxis.Y, yAxis.Z, 0,
      zAxis.X, zAxis.Y, zAxis.Z, 0,
      objectPosition.X, objectPosition.Y, objectPosition.Z, 1
    );
  }

  /// <summary>
  /// Creates a left-handed constrained billboard matrix.
  /// </summary>
  /// <param name="objectPosition">The position of the object.</param>
  /// <param name="cameraPosition">The position of the camera.</param>
  /// <param name="rotateAxis">The axis to rotate around.</param>
  /// <param name="cameraForwardVector">The forward vector of the camera.</param>
  /// <param name="objectForwardVector">The forward vector of the object.</param>
  /// <returns>The constrained billboard matrix.</returns>
  public static Matrix4x4 CreateConstrainedBillboardLeftHanded(Vector3 objectPosition, Vector3 cameraPosition, Vector3 rotateAxis, Vector3 cameraForwardVector, Vector3 objectForwardVector) {
    var faceDir = objectPosition - cameraPosition;
    var norm = faceDir.LengthSquared();

    faceDir = norm < 0.0001f ? -cameraForwardVector : faceDir * (1.0f / MathF.Sqrt(norm));

    var yAxis = rotateAxis;
    Vector3 xAxis;
    Vector3 zAxis;

    var dot = Vector3.Dot(rotateAxis, faceDir);
    if (MathF.Abs(dot) > 0.9982547f) {
      zAxis = objectForwardVector;
      dot = Vector3.Dot(rotateAxis, zAxis);
      if (MathF.Abs(dot) > 0.9982547f)
        zAxis = MathF.Abs(rotateAxis.Z) > 0.9982547f ? new Vector3(1, 0, 0) : new Vector3(0, 0, -1);

      xAxis = Vector3.Normalize(Vector3.Cross(rotateAxis, zAxis));
      zAxis = Vector3.Normalize(Vector3.Cross(xAxis, rotateAxis));
    } else {
      xAxis = Vector3.Normalize(Vector3.Cross(rotateAxis, faceDir));
      zAxis = Vector3.Normalize(Vector3.Cross(xAxis, rotateAxis));
    }

    return new(
      xAxis.X, xAxis.Y, xAxis.Z, 0,
      yAxis.X, yAxis.Y, yAxis.Z, 0,
      zAxis.X, zAxis.Y, zAxis.Z, 0,
      objectPosition.X, objectPosition.Y, objectPosition.Z, 1
    );
  }

  #endregion

  #region Row Accessors

  /// <summary>
  /// Gets the specified row of the matrix as a Vector4.
  /// </summary>
  /// <param name="this">The matrix.</param>
  /// <param name="index">The row index (0-3).</param>
  /// <returns>The row as a Vector4.</returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="index"/> is less than 0 or greater than 3.
  /// </exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 GetRow(this Matrix4x4 @this, int index)
    => index switch {
      0 => new(@this.M11, @this.M12, @this.M13, @this.M14),
      1 => new(@this.M21, @this.M22, @this.M23, @this.M24),
      2 => new(@this.M31, @this.M32, @this.M33, @this.M34),
      3 => new(@this.M41, @this.M42, @this.M43, @this.M44),
      _ => throw new ArgumentOutOfRangeException(nameof(index))
    };

  /// <summary>
  /// Creates a new Matrix4x4 with the specified row replaced.
  /// </summary>
  /// <param name="this">The matrix.</param>
  /// <param name="index">The row index (0-3).</param>
  /// <param name="value">The new row value.</param>
  /// <returns>A new matrix with the specified row changed.</returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="index"/> is less than 0 or greater than 3.
  /// </exception>
  public static Matrix4x4 WithRow(this Matrix4x4 @this, int index, Vector4 value) {
    var result = @this;
    switch (index) {
      case 0:
        result.M11 = value.X;
        result.M12 = value.Y;
        result.M13 = value.Z;
        result.M14 = value.W;
        break;
      case 1:
        result.M21 = value.X;
        result.M22 = value.Y;
        result.M23 = value.Z;
        result.M24 = value.W;
        break;
      case 2:
        result.M31 = value.X;
        result.M32 = value.Y;
        result.M33 = value.Z;
        result.M34 = value.W;
        break;
      case 3:
        result.M41 = value.X;
        result.M42 = value.Y;
        result.M43 = value.Z;
        result.M44 = value.W;
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(index));
    }
    return result;
  }

  #endregion

  #region Row Properties

  extension(Matrix4x4 @this) {

    /// <summary>
    /// Gets or sets the first row of the matrix.
    /// </summary>
    public Vector4 X {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => new(@this.M11, @this.M12, @this.M13, @this.M14);
    }

    /// <summary>
    /// Gets or sets the second row of the matrix.
    /// </summary>
    public Vector4 Y {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => new(@this.M21, @this.M22, @this.M23, @this.M24);
    }

    /// <summary>
    /// Gets or sets the third row of the matrix.
    /// </summary>
    public Vector4 Z {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => new(@this.M31, @this.M32, @this.M33, @this.M34);
    }

    /// <summary>
    /// Gets or sets the fourth row of the matrix.
    /// </summary>
    public Vector4 W {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => new(@this.M41, @this.M42, @this.M43, @this.M44);
    }

  }

  /// <summary>
  /// Creates a new Matrix4x4 with the first row replaced.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 WithX(this Matrix4x4 @this, Vector4 value) => new(
    value.X, value.Y, value.Z, value.W,
    @this.M21, @this.M22, @this.M23, @this.M24,
    @this.M31, @this.M32, @this.M33, @this.M34,
    @this.M41, @this.M42, @this.M43, @this.M44
  );

  /// <summary>
  /// Creates a new Matrix4x4 with the second row replaced.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 WithY(this Matrix4x4 @this, Vector4 value) => new(
    @this.M11, @this.M12, @this.M13, @this.M14,
    value.X, value.Y, value.Z, value.W,
    @this.M31, @this.M32, @this.M33, @this.M34,
    @this.M41, @this.M42, @this.M43, @this.M44
  );

  /// <summary>
  /// Creates a new Matrix4x4 with the third row replaced.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 WithZ(this Matrix4x4 @this, Vector4 value) => new(
    @this.M11, @this.M12, @this.M13, @this.M14,
    @this.M21, @this.M22, @this.M23, @this.M24,
    value.X, value.Y, value.Z, value.W,
    @this.M41, @this.M42, @this.M43, @this.M44
  );

  /// <summary>
  /// Creates a new Matrix4x4 with the fourth row replaced.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 WithW(this Matrix4x4 @this, Vector4 value) => new(
    @this.M11, @this.M12, @this.M13, @this.M14,
    @this.M21, @this.M22, @this.M23, @this.M24,
    @this.M31, @this.M32, @this.M33, @this.M34,
    value.X, value.Y, value.Z, value.W
  );

  #endregion

}

#endif
