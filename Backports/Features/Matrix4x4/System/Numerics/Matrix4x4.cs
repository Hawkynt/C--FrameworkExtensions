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
/// Represents a 4x4 matrix.
/// </summary>
public struct Matrix4x4 : IEquatable<Matrix4x4>
#if SUPPORTS_SPAN_FORMATTABLE
  , IFormattable
#endif
{
  /// <summary>The first element of the first row.</summary>
  public float M11;
  /// <summary>The second element of the first row.</summary>
  public float M12;
  /// <summary>The third element of the first row.</summary>
  public float M13;
  /// <summary>The fourth element of the first row.</summary>
  public float M14;
  /// <summary>The first element of the second row.</summary>
  public float M21;
  /// <summary>The second element of the second row.</summary>
  public float M22;
  /// <summary>The third element of the second row.</summary>
  public float M23;
  /// <summary>The fourth element of the second row.</summary>
  public float M24;
  /// <summary>The first element of the third row.</summary>
  public float M31;
  /// <summary>The second element of the third row.</summary>
  public float M32;
  /// <summary>The third element of the third row.</summary>
  public float M33;
  /// <summary>The fourth element of the third row.</summary>
  public float M34;
  /// <summary>The first element of the fourth row.</summary>
  public float M41;
  /// <summary>The second element of the fourth row.</summary>
  public float M42;
  /// <summary>The third element of the fourth row.</summary>
  public float M43;
  /// <summary>The fourth element of the fourth row.</summary>
  public float M44;

  /// <summary>
  /// Creates a <see cref="Matrix4x4"/> from a <see cref="Matrix3x2"/>.
  /// </summary>
  /// <param name="value">The source matrix.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Matrix4x4(Matrix3x2 value) {
    this.M11 = value.M11;
    this.M12 = value.M12;
    this.M13 = 0f;
    this.M14 = 0f;
    this.M21 = value.M21;
    this.M22 = value.M22;
    this.M23 = 0f;
    this.M24 = 0f;
    this.M31 = 0f;
    this.M32 = 0f;
    this.M33 = 1f;
    this.M34 = 0f;
    this.M41 = value.M31;
    this.M42 = value.M32;
    this.M43 = 0f;
    this.M44 = 1f;
  }

  /// <summary>
  /// Creates a 4x4 matrix from the specified components.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Matrix4x4(
    float m11, float m12, float m13, float m14,
    float m21, float m22, float m23, float m24,
    float m31, float m32, float m33, float m34,
    float m41, float m42, float m43, float m44
  ) {
    this.M11 = m11; this.M12 = m12; this.M13 = m13; this.M14 = m14;
    this.M21 = m21; this.M22 = m22; this.M23 = m23; this.M24 = m24;
    this.M31 = m31; this.M32 = m32; this.M33 = m33; this.M34 = m34;
    this.M41 = m41; this.M42 = m42; this.M43 = m43; this.M44 = m44;
  }

  #region Static Properties

  /// <summary>Gets the multiplicative identity matrix.</summary>
  public static Matrix4x4 Identity {
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    get => new(
      1f, 0f, 0f, 0f,
      0f, 1f, 0f, 0f,
      0f, 0f, 1f, 0f,
      0f, 0f, 0f, 1f
    );
  }

  #endregion

  #region Instance Properties

  /// <summary>Gets a value that indicates whether the current matrix is the identity matrix.</summary>
  public readonly bool IsIdentity =>
    this.M11 == 1f && this.M22 == 1f && this.M33 == 1f && this.M44 == 1f &&
    this.M12 == 0f && this.M13 == 0f && this.M14 == 0f &&
    this.M21 == 0f && this.M23 == 0f && this.M24 == 0f &&
    this.M31 == 0f && this.M32 == 0f && this.M34 == 0f &&
    this.M41 == 0f && this.M42 == 0f && this.M43 == 0f;

  /// <summary>Gets or sets the translation component of this matrix.</summary>
  public Vector3 Translation {
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    readonly get => new(this.M41, this.M42, this.M43);
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    set {
      this.M41 = value.X;
      this.M42 = value.Y;
      this.M43 = value.Z;
    }
  }

  #endregion

  #region Static Methods

  /// <summary>Adds each element in one matrix with its corresponding element in a second matrix.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 Add(Matrix4x4 value1, Matrix4x4 value2) => value1 + value2;

  /// <summary>Creates a spherical billboard that rotates around a specified object position.</summary>
  public static Matrix4x4 CreateBillboard(Vector3 objectPosition, Vector3 cameraPosition, Vector3 cameraUpVector, Vector3 cameraForwardVector) {
    var zAxis = cameraPosition - objectPosition;
    var norm = zAxis.LengthSquared();

    zAxis = norm < 0.0001f ? -cameraForwardVector : zAxis * (1f / (float)Math.Sqrt(norm));

    var xAxis = Vector3.Normalize(Vector3.Cross(cameraUpVector, zAxis));
    var yAxis = Vector3.Cross(zAxis, xAxis);

    return new(
      xAxis.X, xAxis.Y, xAxis.Z, 0f,
      yAxis.X, yAxis.Y, yAxis.Z, 0f,
      zAxis.X, zAxis.Y, zAxis.Z, 0f,
      objectPosition.X, objectPosition.Y, objectPosition.Z, 1f
    );
  }

  /// <summary>Creates a cylindrical billboard that rotates around a specified axis.</summary>
  public static Matrix4x4 CreateConstrainedBillboard(Vector3 objectPosition, Vector3 cameraPosition, Vector3 rotateAxis, Vector3 cameraForwardVector, Vector3 objectForwardVector) {
    var faceDir = cameraPosition - objectPosition;
    var norm = faceDir.LengthSquared();

    faceDir = norm < 0.0001f ? -cameraForwardVector : faceDir * (1f / (float)Math.Sqrt(norm));

    var yAxis = rotateAxis;
    Vector3 xAxis;
    Vector3 zAxis;

    var dot = Vector3.Dot(rotateAxis, faceDir);
    if (Math.Abs(dot) > 0.9982547f) {
      zAxis = objectForwardVector;
      dot = Vector3.Dot(rotateAxis, zAxis);
      if (Math.Abs(dot) > 0.9982547f)
        zAxis = Math.Abs(rotateAxis.Z) > 0.9982547f ? new Vector3(1f, 0f, 0f) : new Vector3(0f, 0f, -1f);

      xAxis = Vector3.Normalize(Vector3.Cross(rotateAxis, zAxis));
      zAxis = Vector3.Normalize(Vector3.Cross(xAxis, rotateAxis));
    } else {
      xAxis = Vector3.Normalize(Vector3.Cross(rotateAxis, faceDir));
      zAxis = Vector3.Normalize(Vector3.Cross(xAxis, rotateAxis));
    }

    return new(
      xAxis.X, xAxis.Y, xAxis.Z, 0f,
      yAxis.X, yAxis.Y, yAxis.Z, 0f,
      zAxis.X, zAxis.Y, zAxis.Z, 0f,
      objectPosition.X, objectPosition.Y, objectPosition.Z, 1f
    );
  }

  /// <summary>Creates a matrix that rotates around an arbitrary vector.</summary>
  public static Matrix4x4 CreateFromAxisAngle(Vector3 axis, float angle) {
    var x = axis.X;
    var y = axis.Y;
    var z = axis.Z;
    var sa = (float)Math.Sin(angle);
    var ca = (float)Math.Cos(angle);
    var xx = x * x;
    var yy = y * y;
    var zz = z * z;
    var xy = x * y;
    var xz = x * z;
    var yz = y * z;

    return new(
      xx + ca * (1f - xx), xy - ca * xy + sa * z, xz - ca * xz - sa * y, 0f,
      xy - ca * xy - sa * z, yy + ca * (1f - yy), yz - ca * yz + sa * x, 0f,
      xz - ca * xz + sa * y, yz - ca * yz - sa * x, zz + ca * (1f - zz), 0f,
      0f, 0f, 0f, 1f
    );
  }

  /// <summary>Creates a rotation matrix from the specified Quaternion rotation value.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 CreateFromQuaternion(Quaternion quaternion) {
    var xx = quaternion.X * quaternion.X;
    var yy = quaternion.Y * quaternion.Y;
    var zz = quaternion.Z * quaternion.Z;
    var xy = quaternion.X * quaternion.Y;
    var wz = quaternion.Z * quaternion.W;
    var xz = quaternion.Z * quaternion.X;
    var wy = quaternion.Y * quaternion.W;
    var yz = quaternion.Y * quaternion.Z;
    var wx = quaternion.X * quaternion.W;

    return new(
      1f - 2f * (yy + zz), 2f * (xy + wz), 2f * (xz - wy), 0f,
      2f * (xy - wz), 1f - 2f * (zz + xx), 2f * (yz + wx), 0f,
      2f * (xz + wy), 2f * (yz - wx), 1f - 2f * (yy + xx), 0f,
      0f, 0f, 0f, 1f
    );
  }

  /// <summary>Creates a rotation matrix from the specified yaw, pitch, and roll.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 CreateFromYawPitchRoll(float yaw, float pitch, float roll)
    => CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll));

  /// <summary>Creates a right-handed view matrix.</summary>
  public static Matrix4x4 CreateLookAt(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector) {
    var zAxis = Vector3.Normalize(cameraPosition - cameraTarget);
    var xAxis = Vector3.Normalize(Vector3.Cross(cameraUpVector, zAxis));
    var yAxis = Vector3.Cross(zAxis, xAxis);

    return new(
      xAxis.X, yAxis.X, zAxis.X, 0f,
      xAxis.Y, yAxis.Y, zAxis.Y, 0f,
      xAxis.Z, yAxis.Z, zAxis.Z, 0f,
      -Vector3.Dot(xAxis, cameraPosition),
      -Vector3.Dot(yAxis, cameraPosition),
      -Vector3.Dot(zAxis, cameraPosition),
      1f
    );
  }

  /// <summary>Creates a right-handed orthographic perspective matrix.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 CreateOrthographic(float width, float height, float zNearPlane, float zFarPlane) {
    var range = 1f / (zNearPlane - zFarPlane);
    return new(
      2f / width, 0f, 0f, 0f,
      0f, 2f / height, 0f, 0f,
      0f, 0f, range, 0f,
      0f, 0f, range * zNearPlane, 1f
    );
  }

  /// <summary>Creates a right-handed customized orthographic projection matrix.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 CreateOrthographicOffCenter(float left, float right, float bottom, float top, float zNearPlane, float zFarPlane) {
    var range = 1f / (zNearPlane - zFarPlane);
    return new(
      2f / (right - left), 0f, 0f, 0f,
      0f, 2f / (top - bottom), 0f, 0f,
      0f, 0f, range, 0f,
      (left + right) / (left - right), (top + bottom) / (bottom - top), range * zNearPlane, 1f
    );
  }

  /// <summary>Creates a right-handed perspective projection matrix based on a field of view.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 CreatePerspectiveFieldOfView(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance) {
    if (fieldOfView <= 0f || fieldOfView >= (float)Math.PI)
      throw new ArgumentOutOfRangeException(nameof(fieldOfView));
    if (nearPlaneDistance <= 0f)
      throw new ArgumentOutOfRangeException(nameof(nearPlaneDistance));
    if (farPlaneDistance <= 0f)
      throw new ArgumentOutOfRangeException(nameof(farPlaneDistance));
    if (nearPlaneDistance >= farPlaneDistance)
      throw new ArgumentOutOfRangeException(nameof(nearPlaneDistance));

    var yScale = 1f / (float)Math.Tan(fieldOfView * 0.5f);
    var xScale = yScale / aspectRatio;
    var range = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);

    return new(
      xScale, 0f, 0f, 0f,
      0f, yScale, 0f, 0f,
      0f, 0f, range, -1f,
      0f, 0f, range * nearPlaneDistance, 0f
    );
  }

  /// <summary>Creates a right-handed perspective projection matrix.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 CreatePerspective(float width, float height, float nearPlaneDistance, float farPlaneDistance) {
    if (nearPlaneDistance <= 0f)
      throw new ArgumentOutOfRangeException(nameof(nearPlaneDistance));
    if (farPlaneDistance <= 0f)
      throw new ArgumentOutOfRangeException(nameof(farPlaneDistance));
    if (nearPlaneDistance >= farPlaneDistance)
      throw new ArgumentOutOfRangeException(nameof(nearPlaneDistance));

    var range = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);

    return new(
      2f * nearPlaneDistance / width, 0f, 0f, 0f,
      0f, 2f * nearPlaneDistance / height, 0f, 0f,
      0f, 0f, range, -1f,
      0f, 0f, range * nearPlaneDistance, 0f
    );
  }

  /// <summary>Creates a right-handed customized perspective projection matrix.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 CreatePerspectiveOffCenter(float left, float right, float bottom, float top, float nearPlaneDistance, float farPlaneDistance) {
    if (nearPlaneDistance <= 0f)
      throw new ArgumentOutOfRangeException(nameof(nearPlaneDistance));
    if (farPlaneDistance <= 0f)
      throw new ArgumentOutOfRangeException(nameof(farPlaneDistance));
    if (nearPlaneDistance >= farPlaneDistance)
      throw new ArgumentOutOfRangeException(nameof(nearPlaneDistance));

    var range = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
    var twoNear = 2f * nearPlaneDistance;

    return new(
      twoNear / (right - left), 0f, 0f, 0f,
      0f, twoNear / (top - bottom), 0f, 0f,
      (left + right) / (right - left), (top + bottom) / (top - bottom), range, -1f,
      0f, 0f, range * nearPlaneDistance, 0f
    );
  }

  /// <summary>Creates a matrix that reflects the coordinate system about a specified Plane.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 CreateReflection(Plane value) {
    var p = Plane.Normalize(value);
    var a = p.Normal.X;
    var b = p.Normal.Y;
    var c = p.Normal.Z;
    var fa = -2f * a;
    var fb = -2f * b;
    var fc = -2f * c;

    return new(
      fa * a + 1f, fb * a, fc * a, 0f,
      fa * b, fb * b + 1f, fc * b, 0f,
      fa * c, fb * c, fc * c + 1f, 0f,
      fa * p.D, fb * p.D, fc * p.D, 1f
    );
  }

  /// <summary>Creates a matrix for rotating points around the X axis.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 CreateRotationX(float radians) {
    var c = (float)Math.Cos(radians);
    var s = (float)Math.Sin(radians);
    return new(
      1f, 0f, 0f, 0f,
      0f, c, s, 0f,
      0f, -s, c, 0f,
      0f, 0f, 0f, 1f
    );
  }

  /// <summary>Creates a matrix for rotating points around the X axis from a center point.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 CreateRotationX(float radians, Vector3 centerPoint) {
    var c = (float)Math.Cos(radians);
    var s = (float)Math.Sin(radians);
    var y = centerPoint.Y * (1f - c) + centerPoint.Z * s;
    var z = centerPoint.Z * (1f - c) - centerPoint.Y * s;
    return new(
      1f, 0f, 0f, 0f,
      0f, c, s, 0f,
      0f, -s, c, 0f,
      0f, y, z, 1f
    );
  }

  /// <summary>Creates a matrix for rotating points around the Y axis.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 CreateRotationY(float radians) {
    var c = (float)Math.Cos(radians);
    var s = (float)Math.Sin(radians);
    return new(
      c, 0f, -s, 0f,
      0f, 1f, 0f, 0f,
      s, 0f, c, 0f,
      0f, 0f, 0f, 1f
    );
  }

  /// <summary>Creates a matrix for rotating points around the Y axis from a center point.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 CreateRotationY(float radians, Vector3 centerPoint) {
    var c = (float)Math.Cos(radians);
    var s = (float)Math.Sin(radians);
    var x = centerPoint.X * (1f - c) - centerPoint.Z * s;
    var z = centerPoint.Z * (1f - c) + centerPoint.X * s;
    return new(
      c, 0f, -s, 0f,
      0f, 1f, 0f, 0f,
      s, 0f, c, 0f,
      x, 0f, z, 1f
    );
  }

  /// <summary>Creates a matrix for rotating points around the Z axis.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 CreateRotationZ(float radians) {
    var c = (float)Math.Cos(radians);
    var s = (float)Math.Sin(radians);
    return new(
      c, s, 0f, 0f,
      -s, c, 0f, 0f,
      0f, 0f, 1f, 0f,
      0f, 0f, 0f, 1f
    );
  }

  /// <summary>Creates a matrix for rotating points around the Z axis from a center point.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 CreateRotationZ(float radians, Vector3 centerPoint) {
    var c = (float)Math.Cos(radians);
    var s = (float)Math.Sin(radians);
    var x = centerPoint.X * (1f - c) + centerPoint.Y * s;
    var y = centerPoint.Y * (1f - c) - centerPoint.X * s;
    return new(
      c, s, 0f, 0f,
      -s, c, 0f, 0f,
      0f, 0f, 1f, 0f,
      x, y, 0f, 1f
    );
  }

  /// <summary>Creates a scaling matrix from the specified X, Y, and Z components.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 CreateScale(float xScale, float yScale, float zScale) => new(
    xScale, 0f, 0f, 0f,
    0f, yScale, 0f, 0f,
    0f, 0f, zScale, 0f,
    0f, 0f, 0f, 1f
  );

  /// <summary>Creates a scaling matrix from the specified X, Y, and Z components from a center point.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 CreateScale(float xScale, float yScale, float zScale, Vector3 centerPoint) => new(
    xScale, 0f, 0f, 0f,
    0f, yScale, 0f, 0f,
    0f, 0f, zScale, 0f,
    centerPoint.X * (1f - xScale), centerPoint.Y * (1f - yScale), centerPoint.Z * (1f - zScale), 1f
  );

  /// <summary>Creates a scaling matrix from the specified vector scale.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 CreateScale(Vector3 scales) => CreateScale(scales.X, scales.Y, scales.Z);

  /// <summary>Creates a scaling matrix from the specified vector scale with an offset from the specified center point.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 CreateScale(Vector3 scales, Vector3 centerPoint) => CreateScale(scales.X, scales.Y, scales.Z, centerPoint);

  /// <summary>Creates a uniform scaling matrix that scales equally on each axis.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 CreateScale(float scale) => CreateScale(scale, scale, scale);

  /// <summary>Creates a uniform scaling matrix that scales equally on each axis with a center point.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 CreateScale(float scale, Vector3 centerPoint) => CreateScale(scale, scale, scale, centerPoint);

  /// <summary>Creates a matrix that flattens geometry into a specified Plane as if casting a shadow from a specified light source.</summary>
  public static Matrix4x4 CreateShadow(Vector3 lightDirection, Plane plane) {
    var p = Plane.Normalize(plane);
    var dot = p.Normal.X * lightDirection.X + p.Normal.Y * lightDirection.Y + p.Normal.Z * lightDirection.Z;

    var a = -p.Normal.X;
    var b = -p.Normal.Y;
    var c = -p.Normal.Z;
    var d = -p.D;

    return new(
      a * lightDirection.X + dot, a * lightDirection.Y, a * lightDirection.Z, 0f,
      b * lightDirection.X, b * lightDirection.Y + dot, b * lightDirection.Z, 0f,
      c * lightDirection.X, c * lightDirection.Y, c * lightDirection.Z + dot, 0f,
      d * lightDirection.X, d * lightDirection.Y, d * lightDirection.Z, dot
    );
  }

  /// <summary>Creates a translation matrix from the specified 3-dimensional vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 CreateTranslation(Vector3 position) => new(
    1f, 0f, 0f, 0f,
    0f, 1f, 0f, 0f,
    0f, 0f, 1f, 0f,
    position.X, position.Y, position.Z, 1f
  );

  /// <summary>Creates a translation matrix from the specified X, Y, and Z components.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 CreateTranslation(float xPosition, float yPosition, float zPosition) => new(
    1f, 0f, 0f, 0f,
    0f, 1f, 0f, 0f,
    0f, 0f, 1f, 0f,
    xPosition, yPosition, zPosition, 1f
  );

  /// <summary>Creates a world matrix with the specified parameters.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 CreateWorld(Vector3 position, Vector3 forward, Vector3 up) {
    var zAxis = Vector3.Normalize(-forward);
    var xAxis = Vector3.Normalize(Vector3.Cross(up, zAxis));
    var yAxis = Vector3.Cross(zAxis, xAxis);

    return new(
      xAxis.X, xAxis.Y, xAxis.Z, 0f,
      yAxis.X, yAxis.Y, yAxis.Z, 0f,
      zAxis.X, zAxis.Y, zAxis.Z, 0f,
      position.X, position.Y, position.Z, 1f
    );
  }

  /// <summary>Calculates the determinant of the current 4x4 matrix.</summary>
  public readonly float GetDeterminant() {
    var a = this.M11;
    var b = this.M12;
    var c = this.M13;
    var d = this.M14;
    var e = this.M21;
    var f = this.M22;
    var g = this.M23;
    var h = this.M24;
    var i = this.M31;
    var j = this.M32;
    var k = this.M33;
    var l = this.M34;
    var m = this.M41;
    var n = this.M42;
    var o = this.M43;
    var p = this.M44;

    var kp_lo = k * p - l * o;
    var jp_ln = j * p - l * n;
    var jo_kn = j * o - k * n;
    var ip_lm = i * p - l * m;
    var io_km = i * o - k * m;
    var in_jm = i * n - j * m;

    return a * (f * kp_lo - g * jp_ln + h * jo_kn)
           - b * (e * kp_lo - g * ip_lm + h * io_km)
           + c * (e * jp_ln - f * ip_lm + h * in_jm)
           - d * (e * jo_kn - f * io_km + g * in_jm);
  }

  /// <summary>Attempts to invert the specified matrix.</summary>
  public static bool Invert(Matrix4x4 matrix, out Matrix4x4 result) {
    var a = matrix.M11;
    var b = matrix.M12;
    var c = matrix.M13;
    var d = matrix.M14;
    var e = matrix.M21;
    var f = matrix.M22;
    var g = matrix.M23;
    var h = matrix.M24;
    var i = matrix.M31;
    var j = matrix.M32;
    var k = matrix.M33;
    var l = matrix.M34;
    var m = matrix.M41;
    var n = matrix.M42;
    var o = matrix.M43;
    var p = matrix.M44;

    var kp_lo = k * p - l * o;
    var jp_ln = j * p - l * n;
    var jo_kn = j * o - k * n;
    var ip_lm = i * p - l * m;
    var io_km = i * o - k * m;
    var in_jm = i * n - j * m;

    var a11 = f * kp_lo - g * jp_ln + h * jo_kn;
    var a12 = -(e * kp_lo - g * ip_lm + h * io_km);
    var a13 = e * jp_ln - f * ip_lm + h * in_jm;
    var a14 = -(e * jo_kn - f * io_km + g * in_jm);

    var det = a * a11 + b * a12 + c * a13 + d * a14;

    if (Math.Abs(det) < float.Epsilon) {
      result = new(float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN);
      return false;
    }

    var invDet = 1f / det;

    var gp_ho = g * p - h * o;
    var fp_hn = f * p - h * n;
    var fo_gn = f * o - g * n;
    var ep_hm = e * p - h * m;
    var eo_gm = e * o - g * m;
    var en_fm = e * n - f * m;
    var gl_hk = g * l - h * k;
    var fl_hj = f * l - h * j;
    var fk_gj = f * k - g * j;
    var el_hi = e * l - h * i;
    var ek_gi = e * k - g * i;
    var ej_fi = e * j - f * i;

    result = new(
      a11 * invDet,
      -(b * kp_lo - c * jp_ln + d * jo_kn) * invDet,
      (b * gp_ho - c * fp_hn + d * fo_gn) * invDet,
      -(b * gl_hk - c * fl_hj + d * fk_gj) * invDet,
      a12 * invDet,
      (a * kp_lo - c * ip_lm + d * io_km) * invDet,
      -(a * gp_ho - c * ep_hm + d * eo_gm) * invDet,
      (a * gl_hk - c * el_hi + d * ek_gi) * invDet,
      a13 * invDet,
      -(a * jp_ln - b * ip_lm + d * in_jm) * invDet,
      (a * fp_hn - b * ep_hm + d * en_fm) * invDet,
      -(a * fl_hj - b * el_hi + d * ej_fi) * invDet,
      a14 * invDet,
      (a * jo_kn - b * io_km + c * in_jm) * invDet,
      -(a * fo_gn - b * eo_gm + c * en_fm) * invDet,
      (a * fk_gj - b * ek_gi + c * ej_fi) * invDet
    );
    return true;
  }

  /// <summary>Performs a linear interpolation from one matrix to a second matrix based on a value that specifies the weighting.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 Lerp(Matrix4x4 matrix1, Matrix4x4 matrix2, float amount) => new(
    matrix1.M11 + (matrix2.M11 - matrix1.M11) * amount,
    matrix1.M12 + (matrix2.M12 - matrix1.M12) * amount,
    matrix1.M13 + (matrix2.M13 - matrix1.M13) * amount,
    matrix1.M14 + (matrix2.M14 - matrix1.M14) * amount,
    matrix1.M21 + (matrix2.M21 - matrix1.M21) * amount,
    matrix1.M22 + (matrix2.M22 - matrix1.M22) * amount,
    matrix1.M23 + (matrix2.M23 - matrix1.M23) * amount,
    matrix1.M24 + (matrix2.M24 - matrix1.M24) * amount,
    matrix1.M31 + (matrix2.M31 - matrix1.M31) * amount,
    matrix1.M32 + (matrix2.M32 - matrix1.M32) * amount,
    matrix1.M33 + (matrix2.M33 - matrix1.M33) * amount,
    matrix1.M34 + (matrix2.M34 - matrix1.M34) * amount,
    matrix1.M41 + (matrix2.M41 - matrix1.M41) * amount,
    matrix1.M42 + (matrix2.M42 - matrix1.M42) * amount,
    matrix1.M43 + (matrix2.M43 - matrix1.M43) * amount,
    matrix1.M44 + (matrix2.M44 - matrix1.M44) * amount
  );

  /// <summary>Returns the matrix that results from multiplying two matrices together.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 Multiply(Matrix4x4 value1, Matrix4x4 value2) => value1 * value2;

  /// <summary>Returns the matrix that results from scaling all the elements of a specified matrix by a scalar factor.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 Multiply(Matrix4x4 value1, float value2) => value1 * value2;

  /// <summary>Negates the specified matrix by multiplying all its values by -1.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 Negate(Matrix4x4 value) => -value;

  /// <summary>Subtracts each element in a second matrix from its corresponding element in a first matrix.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 Subtract(Matrix4x4 value1, Matrix4x4 value2) => value1 - value2;

  /// <summary>Transposes the rows and columns of a matrix.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 Transpose(Matrix4x4 matrix) => new(
    matrix.M11, matrix.M21, matrix.M31, matrix.M41,
    matrix.M12, matrix.M22, matrix.M32, matrix.M42,
    matrix.M13, matrix.M23, matrix.M33, matrix.M43,
    matrix.M14, matrix.M24, matrix.M34, matrix.M44
  );

  #endregion

  #region Operators

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 operator +(Matrix4x4 value1, Matrix4x4 value2) => new(
    value1.M11 + value2.M11, value1.M12 + value2.M12, value1.M13 + value2.M13, value1.M14 + value2.M14,
    value1.M21 + value2.M21, value1.M22 + value2.M22, value1.M23 + value2.M23, value1.M24 + value2.M24,
    value1.M31 + value2.M31, value1.M32 + value2.M32, value1.M33 + value2.M33, value1.M34 + value2.M34,
    value1.M41 + value2.M41, value1.M42 + value2.M42, value1.M43 + value2.M43, value1.M44 + value2.M44
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 operator -(Matrix4x4 value1, Matrix4x4 value2) => new(
    value1.M11 - value2.M11, value1.M12 - value2.M12, value1.M13 - value2.M13, value1.M14 - value2.M14,
    value1.M21 - value2.M21, value1.M22 - value2.M22, value1.M23 - value2.M23, value1.M24 - value2.M24,
    value1.M31 - value2.M31, value1.M32 - value2.M32, value1.M33 - value2.M33, value1.M34 - value2.M34,
    value1.M41 - value2.M41, value1.M42 - value2.M42, value1.M43 - value2.M43, value1.M44 - value2.M44
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 operator *(Matrix4x4 value1, Matrix4x4 value2) => new(
    value1.M11 * value2.M11 + value1.M12 * value2.M21 + value1.M13 * value2.M31 + value1.M14 * value2.M41,
    value1.M11 * value2.M12 + value1.M12 * value2.M22 + value1.M13 * value2.M32 + value1.M14 * value2.M42,
    value1.M11 * value2.M13 + value1.M12 * value2.M23 + value1.M13 * value2.M33 + value1.M14 * value2.M43,
    value1.M11 * value2.M14 + value1.M12 * value2.M24 + value1.M13 * value2.M34 + value1.M14 * value2.M44,
    value1.M21 * value2.M11 + value1.M22 * value2.M21 + value1.M23 * value2.M31 + value1.M24 * value2.M41,
    value1.M21 * value2.M12 + value1.M22 * value2.M22 + value1.M23 * value2.M32 + value1.M24 * value2.M42,
    value1.M21 * value2.M13 + value1.M22 * value2.M23 + value1.M23 * value2.M33 + value1.M24 * value2.M43,
    value1.M21 * value2.M14 + value1.M22 * value2.M24 + value1.M23 * value2.M34 + value1.M24 * value2.M44,
    value1.M31 * value2.M11 + value1.M32 * value2.M21 + value1.M33 * value2.M31 + value1.M34 * value2.M41,
    value1.M31 * value2.M12 + value1.M32 * value2.M22 + value1.M33 * value2.M32 + value1.M34 * value2.M42,
    value1.M31 * value2.M13 + value1.M32 * value2.M23 + value1.M33 * value2.M33 + value1.M34 * value2.M43,
    value1.M31 * value2.M14 + value1.M32 * value2.M24 + value1.M33 * value2.M34 + value1.M34 * value2.M44,
    value1.M41 * value2.M11 + value1.M42 * value2.M21 + value1.M43 * value2.M31 + value1.M44 * value2.M41,
    value1.M41 * value2.M12 + value1.M42 * value2.M22 + value1.M43 * value2.M32 + value1.M44 * value2.M42,
    value1.M41 * value2.M13 + value1.M42 * value2.M23 + value1.M43 * value2.M33 + value1.M44 * value2.M43,
    value1.M41 * value2.M14 + value1.M42 * value2.M24 + value1.M43 * value2.M34 + value1.M44 * value2.M44
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 operator *(Matrix4x4 value1, float value2) => new(
    value1.M11 * value2, value1.M12 * value2, value1.M13 * value2, value1.M14 * value2,
    value1.M21 * value2, value1.M22 * value2, value1.M23 * value2, value1.M24 * value2,
    value1.M31 * value2, value1.M32 * value2, value1.M33 * value2, value1.M34 * value2,
    value1.M41 * value2, value1.M42 * value2, value1.M43 * value2, value1.M44 * value2
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Matrix4x4 operator -(Matrix4x4 value) => new(
    -value.M11, -value.M12, -value.M13, -value.M14,
    -value.M21, -value.M22, -value.M23, -value.M24,
    -value.M31, -value.M32, -value.M33, -value.M34,
    -value.M41, -value.M42, -value.M43, -value.M44
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator ==(Matrix4x4 value1, Matrix4x4 value2) => value1.Equals(value2);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator !=(Matrix4x4 value1, Matrix4x4 value2) => !value1.Equals(value2);

  #endregion

  #region Equality

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly bool Equals(Matrix4x4 other) =>
    this.M11 == other.M11 && this.M12 == other.M12 && this.M13 == other.M13 && this.M14 == other.M14 &&
    this.M21 == other.M21 && this.M22 == other.M22 && this.M23 == other.M23 && this.M24 == other.M24 &&
    this.M31 == other.M31 && this.M32 == other.M32 && this.M33 == other.M33 && this.M34 == other.M34 &&
    this.M41 == other.M41 && this.M42 == other.M42 && this.M43 == other.M43 && this.M44 == other.M44;

  /// <inheritdoc/>
  public override readonly bool Equals(object obj) => obj is Matrix4x4 other && this.Equals(other);

  /// <inheritdoc/>
  public override readonly int GetHashCode() {
    var hash = this.M11.GetHashCode();
    hash = (hash << 5) + hash ^ this.M12.GetHashCode();
    hash = (hash << 5) + hash ^ this.M13.GetHashCode();
    hash = (hash << 5) + hash ^ this.M14.GetHashCode();
    hash = (hash << 5) + hash ^ this.M21.GetHashCode();
    hash = (hash << 5) + hash ^ this.M22.GetHashCode();
    hash = (hash << 5) + hash ^ this.M23.GetHashCode();
    hash = (hash << 5) + hash ^ this.M24.GetHashCode();
    hash = (hash << 5) + hash ^ this.M31.GetHashCode();
    hash = (hash << 5) + hash ^ this.M32.GetHashCode();
    hash = (hash << 5) + hash ^ this.M33.GetHashCode();
    hash = (hash << 5) + hash ^ this.M34.GetHashCode();
    hash = (hash << 5) + hash ^ this.M41.GetHashCode();
    hash = (hash << 5) + hash ^ this.M42.GetHashCode();
    hash = (hash << 5) + hash ^ this.M43.GetHashCode();
    hash = (hash << 5) + hash ^ this.M44.GetHashCode();
    return hash;
  }

  #endregion

  /// <inheritdoc/>
  public override readonly string ToString() => this.ToString("G", CultureInfo.CurrentCulture);

  /// <summary>Returns a string representation of the current instance using the specified format string.</summary>
  public readonly string ToString(string format) => this.ToString(format, CultureInfo.CurrentCulture);

  /// <summary>Returns a string representation of the current instance using the specified format string and format provider.</summary>
  public readonly string ToString(string format, IFormatProvider formatProvider) {
    var separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;
    return $"{{ {{M11:{this.M11.ToString(format, formatProvider)}{separator} M12:{this.M12.ToString(format, formatProvider)}{separator} M13:{this.M13.ToString(format, formatProvider)}{separator} M14:{this.M14.ToString(format, formatProvider)}}} {{M21:{this.M21.ToString(format, formatProvider)}{separator} M22:{this.M22.ToString(format, formatProvider)}{separator} M23:{this.M23.ToString(format, formatProvider)}{separator} M24:{this.M24.ToString(format, formatProvider)}}} {{M31:{this.M31.ToString(format, formatProvider)}{separator} M32:{this.M32.ToString(format, formatProvider)}{separator} M33:{this.M33.ToString(format, formatProvider)}{separator} M34:{this.M34.ToString(format, formatProvider)}}} {{M41:{this.M41.ToString(format, formatProvider)}{separator} M42:{this.M42.ToString(format, formatProvider)}{separator} M43:{this.M43.ToString(format, formatProvider)}{separator} M44:{this.M44.ToString(format, formatProvider)}}} }}";
  }
}

#endif
