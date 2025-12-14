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
/// Represents a plane in three-dimensional space.
/// </summary>
public struct Plane : IEquatable<Plane>
#if SUPPORTS_SPAN_FORMATTABLE
  , IFormattable
#endif
{
  /// <summary>The normal vector of the plane.</summary>
  public Vector3 Normal;

  /// <summary>The distance of the plane along its normal from the origin.</summary>
  public float D;

  /// <summary>
  /// Creates a <see cref="Plane"/> object from a specified four-dimensional vector.
  /// </summary>
  /// <param name="value">A vector whose first three elements describe the normal vector, and whose W component defines the distance along that normal from the origin.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Plane(Vector4 value) : this(new Vector3(value.X, value.Y, value.Z), value.W) { }

  /// <summary>
  /// Creates a <see cref="Plane"/> object from a specified normal and the distance along the normal from the origin.
  /// </summary>
  /// <param name="normal">The plane's normal vector.</param>
  /// <param name="d">The plane's distance from the origin along its normal vector.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Plane(Vector3 normal, float d) {
    this.Normal = normal;
    this.D = d;
  }

  /// <summary>
  /// Creates a <see cref="Plane"/> object from the X, Y, and Z components of its normal, and its distance from the origin on that normal.
  /// </summary>
  /// <param name="x">The X component of the normal.</param>
  /// <param name="y">The Y component of the normal.</param>
  /// <param name="z">The Z component of the normal.</param>
  /// <param name="d">The distance of the plane along its normal from the origin.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Plane(float x, float y, float z, float d) : this(new Vector3(x, y, z), d) { }

  #region Static Methods

  /// <summary>Creates a <see cref="Plane"/> object that contains three specified points.</summary>
  /// <param name="point1">The first point defining the plane.</param>
  /// <param name="point2">The second point defining the plane.</param>
  /// <param name="point3">The third point defining the plane.</param>
  /// <returns>The plane containing the three points.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Plane CreateFromVertices(Vector3 point1, Vector3 point2, Vector3 point3) {
    var a = point2 - point1;
    var b = point3 - point1;
    var n = Vector3.Normalize(Vector3.Cross(a, b));
    var d = -Vector3.Dot(n, point1);
    return new(n, d);
  }

  /// <summary>Calculates the dot product of a plane and a 4-dimensional vector.</summary>
  /// <param name="plane">The plane.</param>
  /// <param name="value">The four-dimensional vector.</param>
  /// <returns>The dot product.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Dot(Plane plane, Vector4 value) =>
    plane.Normal.X * value.X + plane.Normal.Y * value.Y + plane.Normal.Z * value.Z + plane.D * value.W;

  /// <summary>Returns the dot product of a specified three-dimensional vector and the normal vector of this plane plus the distance value of the plane.</summary>
  /// <param name="plane">The plane.</param>
  /// <param name="value">The 3-dimensional vector.</param>
  /// <returns>The dot product.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float DotCoordinate(Plane plane, Vector3 value) =>
    plane.Normal.X * value.X + plane.Normal.Y * value.Y + plane.Normal.Z * value.Z + plane.D;

  /// <summary>Returns the dot product of a specified three-dimensional vector and the Normal vector of this plane.</summary>
  /// <param name="plane">The plane.</param>
  /// <param name="value">The three-dimensional vector.</param>
  /// <returns>The dot product.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float DotNormal(Plane plane, Vector3 value) =>
    plane.Normal.X * value.X + plane.Normal.Y * value.Y + plane.Normal.Z * value.Z;

  /// <summary>Creates a new <see cref="Plane"/> object whose normal vector is the source plane's normal vector normalized.</summary>
  /// <param name="value">The source plane.</param>
  /// <returns>The normalized plane.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Plane Normalize(Plane value) {
    var lengthSquared = value.Normal.LengthSquared();
    if (Math.Abs(lengthSquared - 1f) < 1.192093E-07f)
      return value;

    var invLength = 1f / (float)Math.Sqrt(lengthSquared);
    return new(value.Normal * invLength, value.D * invLength);
  }

  /// <summary>Transforms a normalized plane by a 4x4 matrix.</summary>
  /// <param name="plane">The normalized plane to transform.</param>
  /// <param name="matrix">The transformation matrix to apply to the plane.</param>
  /// <returns>The transformed plane.</returns>
  public static Plane Transform(Plane plane, Matrix4x4 matrix) {
    Matrix4x4.Invert(matrix, out var inverted);

    var x = plane.Normal.X;
    var y = plane.Normal.Y;
    var z = plane.Normal.Z;
    var w = plane.D;

    return new(
      x * inverted.M11 + y * inverted.M12 + z * inverted.M13 + w * inverted.M14,
      x * inverted.M21 + y * inverted.M22 + z * inverted.M23 + w * inverted.M24,
      x * inverted.M31 + y * inverted.M32 + z * inverted.M33 + w * inverted.M34,
      x * inverted.M41 + y * inverted.M42 + z * inverted.M43 + w * inverted.M44
    );
  }

  /// <summary>Transforms a normalized plane by a Quaternion rotation.</summary>
  /// <param name="plane">The normalized plane to transform.</param>
  /// <param name="rotation">The Quaternion rotation to apply to the plane.</param>
  /// <returns>A new plane that results from applying the Quaternion rotation.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Plane Transform(Plane plane, Quaternion rotation) {
    var x2 = rotation.X + rotation.X;
    var y2 = rotation.Y + rotation.Y;
    var z2 = rotation.Z + rotation.Z;
    var wx2 = rotation.W * x2;
    var wy2 = rotation.W * y2;
    var wz2 = rotation.W * z2;
    var xx2 = rotation.X * x2;
    var xy2 = rotation.X * y2;
    var xz2 = rotation.X * z2;
    var yy2 = rotation.Y * y2;
    var yz2 = rotation.Y * z2;
    var zz2 = rotation.Z * z2;

    var nx = plane.Normal.X;
    var ny = plane.Normal.Y;
    var nz = plane.Normal.Z;

    return new(
      nx * (1f - yy2 - zz2) + ny * (xy2 - wz2) + nz * (xz2 + wy2),
      nx * (xy2 + wz2) + ny * (1f - xx2 - zz2) + nz * (yz2 - wx2),
      nx * (xz2 - wy2) + ny * (yz2 + wx2) + nz * (1f - xx2 - yy2),
      plane.D
    );
  }

  #endregion

  #region Operators

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator ==(Plane value1, Plane value2) => value1.Equals(value2);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator !=(Plane value1, Plane value2) => !value1.Equals(value2);

  #endregion

  #region Equality

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly bool Equals(Plane other) => this.Normal.Equals(other.Normal) && this.D == other.D;

  /// <inheritdoc/>
  public override readonly bool Equals(object obj) => obj is Plane other && this.Equals(other);

  /// <inheritdoc/>
  public override readonly int GetHashCode() => this.Normal.GetHashCode() ^ this.D.GetHashCode();

  #endregion

  /// <inheritdoc/>
  public override readonly string ToString() => this.ToString("G", CultureInfo.CurrentCulture);

  /// <summary>Returns a string representation of the current instance using the specified format string.</summary>
  public readonly string ToString(string format) => this.ToString(format, CultureInfo.CurrentCulture);

  /// <summary>Returns a string representation of the current instance using the specified format string and format provider.</summary>
  public readonly string ToString(string format, IFormatProvider formatProvider) {
    var separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;
    return $"{{Normal:{this.Normal.ToString(format, formatProvider)}{separator} D:{this.D.ToString(format, formatProvider)}}}";
  }
}

#endif
