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
/// Represents a vector that is used to encode three-dimensional physical rotations.
/// </summary>
public struct Quaternion : IEquatable<Quaternion>
#if SUPPORTS_SPAN_FORMATTABLE
  , IFormattable
#endif
{
  /// <summary>The X value of the vector component of the quaternion.</summary>
  public float X;

  /// <summary>The Y value of the vector component of the quaternion.</summary>
  public float Y;

  /// <summary>The Z value of the vector component of the quaternion.</summary>
  public float Z;

  /// <summary>The rotation component of the quaternion.</summary>
  public float W;

  /// <summary>
  /// Creates a quaternion from a vector and a rotation component.
  /// </summary>
  /// <param name="vectorPart">The vector part of the quaternion.</param>
  /// <param name="scalarPart">The rotation part of the quaternion.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Quaternion(Vector3 vectorPart, float scalarPart) : this(vectorPart.X, vectorPart.Y, vectorPart.Z, scalarPart) { }

  /// <summary>
  /// Creates a quaternion from the specified components.
  /// </summary>
  /// <param name="x">The value to assign to the X component.</param>
  /// <param name="y">The value to assign to the Y component.</param>
  /// <param name="z">The value to assign to the Z component.</param>
  /// <param name="w">The value to assign to the W component.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Quaternion(float x, float y, float z, float w) {
    this.X = x;
    this.Y = y;
    this.Z = z;
    this.W = w;
  }

  #region Static Properties

  /// <summary>Gets a quaternion that represents no rotation.</summary>
  public static Quaternion Identity {
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    get => new(0f, 0f, 0f, 1f);
  }

  #endregion

  #region Instance Properties

  /// <summary>Gets a value that indicates whether the current instance is the identity quaternion.</summary>
  public readonly bool IsIdentity => this.X == 0f && this.Y == 0f && this.Z == 0f && this.W == 1f;

  #endregion

  #region Instance Methods

  /// <summary>Returns the length of the quaternion.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly float Length() => (float)Math.Sqrt(this.LengthSquared());

  /// <summary>Returns the squared length of the quaternion.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly float LengthSquared() => this.X * this.X + this.Y * this.Y + this.Z * this.Z + this.W * this.W;

  #endregion

  #region Static Methods

  /// <summary>Adds two quaternions.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion Add(Quaternion value1, Quaternion value2) => value1 + value2;

  /// <summary>Concatenates two quaternions.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion Concatenate(Quaternion value1, Quaternion value2) => new(
    value2.X * value1.W + value1.X * value2.W + value2.Y * value1.Z - value2.Z * value1.Y,
    value2.Y * value1.W + value1.Y * value2.W + value2.Z * value1.X - value2.X * value1.Z,
    value2.Z * value1.W + value1.Z * value2.W + value2.X * value1.Y - value2.Y * value1.X,
    value2.W * value1.W - value2.X * value1.X - value2.Y * value1.Y - value2.Z * value1.Z
  );

  /// <summary>Returns the conjugate of a specified quaternion.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion Conjugate(Quaternion value) => new(-value.X, -value.Y, -value.Z, value.W);

  /// <summary>Creates a quaternion from a unit vector and an angle to rotate around the vector.</summary>
  /// <param name="axis">The unit vector to rotate around.</param>
  /// <param name="angle">The angle, in radians, to rotate around the vector.</param>
  /// <returns>The newly created quaternion.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion CreateFromAxisAngle(Vector3 axis, float angle) {
    var halfAngle = angle * 0.5f;
    var s = (float)Math.Sin(halfAngle);
    var c = (float)Math.Cos(halfAngle);
    return new(axis.X * s, axis.Y * s, axis.Z * s, c);
  }

  /// <summary>Creates a quaternion from the specified rotation matrix.</summary>
  public static Quaternion CreateFromRotationMatrix(Matrix4x4 matrix) {
    var trace = matrix.M11 + matrix.M22 + matrix.M33;

    if (trace > 0f) {
      var s = (float)Math.Sqrt(trace + 1f);
      var invS = 0.5f / s;
      return new(
        (matrix.M23 - matrix.M32) * invS,
        (matrix.M31 - matrix.M13) * invS,
        (matrix.M12 - matrix.M21) * invS,
        s * 0.5f
      );
    }

    if (matrix.M11 >= matrix.M22 && matrix.M11 >= matrix.M33) {
      var s = (float)Math.Sqrt(1f + matrix.M11 - matrix.M22 - matrix.M33);
      var invS = 0.5f / s;
      return new(
        0.5f * s,
        (matrix.M12 + matrix.M21) * invS,
        (matrix.M13 + matrix.M31) * invS,
        (matrix.M23 - matrix.M32) * invS
      );
    }

    if (matrix.M22 > matrix.M33) {
      var s = (float)Math.Sqrt(1f + matrix.M22 - matrix.M11 - matrix.M33);
      var invS = 0.5f / s;
      return new(
        (matrix.M21 + matrix.M12) * invS,
        0.5f * s,
        (matrix.M32 + matrix.M23) * invS,
        (matrix.M31 - matrix.M13) * invS
      );
    }

    {
      var s = (float)Math.Sqrt(1f + matrix.M33 - matrix.M11 - matrix.M22);
      var invS = 0.5f / s;
      return new(
        (matrix.M31 + matrix.M13) * invS,
        (matrix.M32 + matrix.M23) * invS,
        0.5f * s,
        (matrix.M12 - matrix.M21) * invS
      );
    }
  }

  /// <summary>Creates a quaternion from the specified yaw, pitch, and roll.</summary>
  /// <param name="yaw">The yaw angle, in radians, around the Y axis.</param>
  /// <param name="pitch">The pitch angle, in radians, around the X axis.</param>
  /// <param name="roll">The roll angle, in radians, around the Z axis.</param>
  /// <returns>The newly created quaternion.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion CreateFromYawPitchRoll(float yaw, float pitch, float roll) {
    var halfRoll = roll * 0.5f;
    var sr = (float)Math.Sin(halfRoll);
    var cr = (float)Math.Cos(halfRoll);

    var halfPitch = pitch * 0.5f;
    var sp = (float)Math.Sin(halfPitch);
    var cp = (float)Math.Cos(halfPitch);

    var halfYaw = yaw * 0.5f;
    var sy = (float)Math.Sin(halfYaw);
    var cy = (float)Math.Cos(halfYaw);

    return new(
      cy * sp * cr + sy * cp * sr,
      sy * cp * cr - cy * sp * sr,
      cy * cp * sr - sy * sp * cr,
      cy * cp * cr + sy * sp * sr
    );
  }

  /// <summary>Divides a quaternion by another quaternion.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion Divide(Quaternion value1, Quaternion value2) => value1 * Inverse(value2);

  /// <summary>Calculates the dot product of two quaternions.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Dot(Quaternion quaternion1, Quaternion quaternion2) => quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y + quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W;

  /// <summary>Returns the inverse of a quaternion.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion Inverse(Quaternion value) {
    var invNorm = 1f / value.LengthSquared();
    return new(-value.X * invNorm, -value.Y * invNorm, -value.Z * invNorm, value.W * invNorm);
  }

  /// <summary>Performs a linear interpolation between two quaternions based on a value that specifies the weighting of the second quaternion.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion Lerp(Quaternion quaternion1, Quaternion quaternion2, float amount) {
    var t = amount;
    var t1 = 1f - t;

    var dot = Dot(quaternion1, quaternion2);
    Quaternion r;

    if (dot >= 0f) {
      r = new(
        t1 * quaternion1.X + t * quaternion2.X,
        t1 * quaternion1.Y + t * quaternion2.Y,
        t1 * quaternion1.Z + t * quaternion2.Z,
        t1 * quaternion1.W + t * quaternion2.W
      );
    } else {
      r = new(
        t1 * quaternion1.X - t * quaternion2.X,
        t1 * quaternion1.Y - t * quaternion2.Y,
        t1 * quaternion1.Z - t * quaternion2.Z,
        t1 * quaternion1.W - t * quaternion2.W
      );
    }

    return Normalize(r);
  }

  /// <summary>Returns a new quaternion whose values are the product of each pair of elements in specified quaternion and scalar.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion Multiply(Quaternion value1, float value2) => value1 * value2;

  /// <summary>Returns the quaternion that results from multiplying two quaternions together.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion Multiply(Quaternion value1, Quaternion value2) => value1 * value2;

  /// <summary>Reverses the sign of each component of the quaternion.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion Negate(Quaternion value) => -value;

  /// <summary>Divides each component of a specified Quaternion by its length.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion Normalize(Quaternion value) {
    var invNorm = 1f / value.Length();
    return new(value.X * invNorm, value.Y * invNorm, value.Z * invNorm, value.W * invNorm);
  }

  /// <summary>Interpolates between two quaternions, using spherical linear interpolation.</summary>
  public static Quaternion Slerp(Quaternion quaternion1, Quaternion quaternion2, float amount) {
    var t = amount;
    var cosOmega = Dot(quaternion1, quaternion2);
    var flip = false;

    if (cosOmega < 0f) {
      flip = true;
      cosOmega = -cosOmega;
    }

    float s1, s2;

    if (cosOmega > 0.999999f) {
      s1 = 1f - t;
      s2 = flip ? -t : t;
    } else {
      var omega = (float)Math.Acos(cosOmega);
      var invSinOmega = 1f / (float)Math.Sin(omega);
      s1 = (float)Math.Sin((1f - t) * omega) * invSinOmega;
      s2 = flip ? -(float)Math.Sin(t * omega) * invSinOmega : (float)Math.Sin(t * omega) * invSinOmega;
    }

    return new(
      s1 * quaternion1.X + s2 * quaternion2.X,
      s1 * quaternion1.Y + s2 * quaternion2.Y,
      s1 * quaternion1.Z + s2 * quaternion2.Z,
      s1 * quaternion1.W + s2 * quaternion2.W
    );
  }

  /// <summary>Subtracts each element in a second quaternion from its corresponding element in a first quaternion.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion Subtract(Quaternion value1, Quaternion value2) => value1 - value2;

  #endregion

  #region Operators

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion operator +(Quaternion value1, Quaternion value2) => new(value1.X + value2.X, value1.Y + value2.Y, value1.Z + value2.Z, value1.W + value2.W);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion operator -(Quaternion value1, Quaternion value2) => new(value1.X - value2.X, value1.Y - value2.Y, value1.Z - value2.Z, value1.W - value2.W);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion operator *(Quaternion value1, Quaternion value2) => new(
    value1.X * value2.W + value2.X * value1.W + value1.Y * value2.Z - value1.Z * value2.Y,
    value1.Y * value2.W + value2.Y * value1.W + value1.Z * value2.X - value1.X * value2.Z,
    value1.Z * value2.W + value2.Z * value1.W + value1.X * value2.Y - value1.Y * value2.X,
    value1.W * value2.W - value1.X * value2.X - value1.Y * value2.Y - value1.Z * value2.Z
  );

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion operator *(Quaternion value1, float value2) => new(value1.X * value2, value1.Y * value2, value1.Z * value2, value1.W * value2);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion operator /(Quaternion value1, Quaternion value2) => value1 * Inverse(value2);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion operator -(Quaternion value) => new(-value.X, -value.Y, -value.Z, -value.W);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator ==(Quaternion value1, Quaternion value2) => value1.Equals(value2);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator !=(Quaternion value1, Quaternion value2) => !value1.Equals(value2);

  #endregion

  #region Equality

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly bool Equals(Quaternion other) => this.X == other.X && this.Y == other.Y && this.Z == other.Z && this.W == other.W;

  /// <inheritdoc/>
  public override readonly bool Equals(object obj) => obj is Quaternion other && this.Equals(other);

  /// <inheritdoc/>
  public override readonly int GetHashCode() => this.X.GetHashCode() ^ (this.Y.GetHashCode() << 2) ^ (this.Z.GetHashCode() >> 2) ^ (this.W.GetHashCode() >> 1);

  #endregion

  /// <inheritdoc/>
  public override readonly string ToString() => this.ToString("G", CultureInfo.CurrentCulture);

  /// <summary>Returns a string representation of the current instance using the specified format string.</summary>
  public readonly string ToString(string format) => this.ToString(format, CultureInfo.CurrentCulture);

  /// <summary>Returns a string representation of the current instance using the specified format string and format provider.</summary>
  public readonly string ToString(string format, IFormatProvider formatProvider) {
    var separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;
    return $"<{this.X.ToString(format, formatProvider)}{separator} {this.Y.ToString(format, formatProvider)}{separator} {this.Z.ToString(format, formatProvider)}{separator} {this.W.ToString(format, formatProvider)}>";
  }
}

#endif
