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
/// Represents a vector with three single-precision floating-point values.
/// </summary>
public struct Vector3 : IEquatable<Vector3>
#if SUPPORTS_SPAN_FORMATTABLE
  , IFormattable
#endif
{
  /// <summary>The X component of the vector.</summary>
  public float X;

  /// <summary>The Y component of the vector.</summary>
  public float Y;

  /// <summary>The Z component of the vector.</summary>
  public float Z;

  /// <summary>
  /// Creates a new <see cref="Vector3"/> object whose three elements have the same value.
  /// </summary>
  /// <param name="value">The value to assign to all three elements.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Vector3(float value) : this(value, value, value) { }

  /// <summary>
  /// Creates a new <see cref="Vector3"/> object from the specified <see cref="Vector2"/> object and a Z component.
  /// </summary>
  /// <param name="value">The vector with two elements.</param>
  /// <param name="z">The Z component.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Vector3(Vector2 value, float z) : this(value.X, value.Y, z) { }

  /// <summary>
  /// Creates a vector whose elements have the specified values.
  /// </summary>
  /// <param name="x">The value to assign to the <see cref="X"/> field.</param>
  /// <param name="y">The value to assign to the <see cref="Y"/> field.</param>
  /// <param name="z">The value to assign to the <see cref="Z"/> field.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Vector3(float x, float y, float z) {
    this.X = x;
    this.Y = y;
    this.Z = z;
  }

  #region Static Properties

  /// <summary>Gets the vector (0,0,0).</summary>
  public static Vector3 Zero {
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    get => new(0f, 0f, 0f);
  }

  /// <summary>Gets the vector (1,1,1).</summary>
  public static Vector3 One {
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    get => new(1f, 1f, 1f);
  }

  /// <summary>Gets the vector (1,0,0).</summary>
  public static Vector3 UnitX {
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    get => new(1f, 0f, 0f);
  }

  /// <summary>Gets the vector (0,1,0).</summary>
  public static Vector3 UnitY {
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    get => new(0f, 1f, 0f);
  }

  /// <summary>Gets the vector (0,0,1).</summary>
  public static Vector3 UnitZ {
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    get => new(0f, 0f, 1f);
  }

  #endregion

  #region Instance Methods

  /// <summary>Returns the length of this vector object.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly float Length() => (float)Math.Sqrt(this.LengthSquared());

  /// <summary>Returns the length of the vector squared.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly float LengthSquared() => this.X * this.X + this.Y * this.Y + this.Z * this.Z;

  #endregion

  #region Static Methods

  /// <summary>Returns a vector whose elements are the absolute values of each of the specified vector's elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 Abs(Vector3 value) => new(Math.Abs(value.X), Math.Abs(value.Y), Math.Abs(value.Z));

  /// <summary>Adds two vectors together.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 Add(Vector3 left, Vector3 right) => left + right;

  /// <summary>Restricts a vector between a minimum and a maximum value.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 Clamp(Vector3 value, Vector3 min, Vector3 max) => new(
    Math.Max(min.X, Math.Min(max.X, value.X)),
    Math.Max(min.Y, Math.Min(max.Y, value.Y)),
    Math.Max(min.Z, Math.Min(max.Z, value.Z))
  );

  /// <summary>Computes the cross product of two vectors.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 Cross(Vector3 vector1, Vector3 vector2) => new(
    vector1.Y * vector2.Z - vector1.Z * vector2.Y,
    vector1.Z * vector2.X - vector1.X * vector2.Z,
    vector1.X * vector2.Y - vector1.Y * vector2.X
  );

  /// <summary>Computes the Euclidean distance between the two given points.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Distance(Vector3 value1, Vector3 value2) => (value1 - value2).Length();

  /// <summary>Returns the Euclidean distance squared between two specified points.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float DistanceSquared(Vector3 value1, Vector3 value2) => (value1 - value2).LengthSquared();

  /// <summary>Divides the first vector by the second.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 Divide(Vector3 left, Vector3 right) => left / right;

  /// <summary>Divides the specified vector by a specified scalar value.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 Divide(Vector3 left, float divisor) => left / divisor;

  /// <summary>Returns the dot product of two vectors.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Dot(Vector3 vector1, Vector3 vector2) => vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z;

  /// <summary>Performs a linear interpolation between two vectors based on the given weighting.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 Lerp(Vector3 value1, Vector3 value2, float amount) => new(
    value1.X + (value2.X - value1.X) * amount,
    value1.Y + (value2.Y - value1.Y) * amount,
    value1.Z + (value2.Z - value1.Z) * amount
  );

  /// <summary>Returns a vector whose elements are the maximum of each of the pairs of elements in two specified vectors.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 Max(Vector3 value1, Vector3 value2) => new(
    Math.Max(value1.X, value2.X),
    Math.Max(value1.Y, value2.Y),
    Math.Max(value1.Z, value2.Z)
  );

  /// <summary>Returns a vector whose elements are the minimum of each of the pairs of elements in two specified vectors.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 Min(Vector3 value1, Vector3 value2) => new(
    Math.Min(value1.X, value2.X),
    Math.Min(value1.Y, value2.Y),
    Math.Min(value1.Z, value2.Z)
  );

  /// <summary>Returns a new vector whose values are the product of each pair of elements in two specified vectors.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 Multiply(Vector3 left, Vector3 right) => left * right;

  /// <summary>Multiplies a vector by a specified scalar.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 Multiply(Vector3 left, float right) => left * right;

  /// <summary>Multiplies a scalar value by a specified vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 Multiply(float left, Vector3 right) => left * right;

  /// <summary>Negates a specified vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 Negate(Vector3 value) => -value;

  /// <summary>Returns a vector with the same direction as the specified vector, but with a length of one.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 Normalize(Vector3 value) => value / value.Length();

  /// <summary>Returns the reflection of a vector off a surface that has the specified normal.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 Reflect(Vector3 vector, Vector3 normal) => vector - 2f * Dot(vector, normal) * normal;

  /// <summary>Returns a vector whose elements are the square root of each of a specified vector's elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 SquareRoot(Vector3 value) => new((float)Math.Sqrt(value.X), (float)Math.Sqrt(value.Y), (float)Math.Sqrt(value.Z));

  /// <summary>Subtracts the second vector from the first.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 Subtract(Vector3 left, Vector3 right) => left - right;

  /// <summary>Transforms a vector by a specified 4x4 matrix.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 Transform(Vector3 position, Matrix4x4 matrix) => new(
    position.X * matrix.M11 + position.Y * matrix.M21 + position.Z * matrix.M31 + matrix.M41,
    position.X * matrix.M12 + position.Y * matrix.M22 + position.Z * matrix.M32 + matrix.M42,
    position.X * matrix.M13 + position.Y * matrix.M23 + position.Z * matrix.M33 + matrix.M43
  );

  /// <summary>Transforms a vector by the specified Quaternion rotation value.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 Transform(Vector3 value, Quaternion rotation) {
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
    return new(
      value.X * (1f - yy2 - zz2) + value.Y * (xy2 - wz2) + value.Z * (xz2 + wy2),
      value.X * (xy2 + wz2) + value.Y * (1f - xx2 - zz2) + value.Z * (yz2 - wx2),
      value.X * (xz2 - wy2) + value.Y * (yz2 + wx2) + value.Z * (1f - xx2 - yy2)
    );
  }

  /// <summary>Transforms a vector normal by the given 4x4 matrix.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 TransformNormal(Vector3 normal, Matrix4x4 matrix) => new(
    normal.X * matrix.M11 + normal.Y * matrix.M21 + normal.Z * matrix.M31,
    normal.X * matrix.M12 + normal.Y * matrix.M22 + normal.Z * matrix.M32,
    normal.X * matrix.M13 + normal.Y * matrix.M23 + normal.Z * matrix.M33
  );

  #endregion

  #region Operators

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 operator +(Vector3 left, Vector3 right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 operator -(Vector3 left, Vector3 right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 operator *(Vector3 left, Vector3 right) => new(left.X * right.X, left.Y * right.Y, left.Z * right.Z);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 operator *(Vector3 left, float right) => new(left.X * right, left.Y * right, left.Z * right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 operator *(float left, Vector3 right) => new(left * right.X, left * right.Y, left * right.Z);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 operator /(Vector3 left, Vector3 right) => new(left.X / right.X, left.Y / right.Y, left.Z / right.Z);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 operator /(Vector3 value, float divisor) => new(value.X / divisor, value.Y / divisor, value.Z / divisor);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 operator -(Vector3 value) => new(-value.X, -value.Y, -value.Z);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator ==(Vector3 left, Vector3 right) => left.Equals(right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator !=(Vector3 left, Vector3 right) => !left.Equals(right);

  #endregion

  #region Equality

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly bool Equals(Vector3 other) => this.X == other.X && this.Y == other.Y && this.Z == other.Z;

  /// <inheritdoc/>
  public override readonly bool Equals(object obj) => obj is Vector3 other && this.Equals(other);

  /// <inheritdoc/>
  public override readonly int GetHashCode() => this.X.GetHashCode() ^ (this.Y.GetHashCode() << 2) ^ (this.Z.GetHashCode() >> 2);

  #endregion

  /// <inheritdoc/>
  public override readonly string ToString() => this.ToString("G", CultureInfo.CurrentCulture);

  /// <summary>Returns a string representation of the current instance using the specified format string.</summary>
  public readonly string ToString(string format) => this.ToString(format, CultureInfo.CurrentCulture);

  /// <summary>Returns a string representation of the current instance using the specified format string and format provider.</summary>
  public readonly string ToString(string format, IFormatProvider formatProvider) {
    var separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;
    return $"<{this.X.ToString(format, formatProvider)}{separator} {this.Y.ToString(format, formatProvider)}{separator} {this.Z.ToString(format, formatProvider)}>";
  }
}

#endif
