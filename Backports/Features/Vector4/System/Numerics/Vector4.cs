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
/// Represents a vector with four single-precision floating-point values.
/// </summary>
public struct Vector4 : IEquatable<Vector4>
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

  /// <summary>The W component of the vector.</summary>
  public float W;

  /// <summary>
  /// Creates a new <see cref="Vector4"/> object whose four elements have the same value.
  /// </summary>
  /// <param name="value">The value to assign to all four elements.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Vector4(float value) : this(value, value, value, value) { }

  /// <summary>
  /// Creates a new <see cref="Vector4"/> object from a <see cref="Vector2"/> and two float values.
  /// </summary>
  /// <param name="value">The vector with two elements.</param>
  /// <param name="z">The Z component.</param>
  /// <param name="w">The W component.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Vector4(Vector2 value, float z, float w) : this(value.X, value.Y, z, w) { }

  /// <summary>
  /// Creates a new <see cref="Vector4"/> object from a <see cref="Vector3"/> and a float value.
  /// </summary>
  /// <param name="value">The vector with three elements.</param>
  /// <param name="w">The W component.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Vector4(Vector3 value, float w) : this(value.X, value.Y, value.Z, w) { }

  /// <summary>
  /// Creates a vector whose elements have the specified values.
  /// </summary>
  /// <param name="x">The value to assign to the <see cref="X"/> field.</param>
  /// <param name="y">The value to assign to the <see cref="Y"/> field.</param>
  /// <param name="z">The value to assign to the <see cref="Z"/> field.</param>
  /// <param name="w">The value to assign to the <see cref="W"/> field.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Vector4(float x, float y, float z, float w) {
    this.X = x;
    this.Y = y;
    this.Z = z;
    this.W = w;
  }

  #region Static Properties

  /// <summary>Gets the vector (0,0,0,0).</summary>
  public static Vector4 Zero {
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    get => new(0f, 0f, 0f, 0f);
  }

  /// <summary>Gets the vector (1,1,1,1).</summary>
  public static Vector4 One {
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    get => new(1f, 1f, 1f, 1f);
  }

  /// <summary>Gets the vector (1,0,0,0).</summary>
  public static Vector4 UnitX {
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    get => new(1f, 0f, 0f, 0f);
  }

  /// <summary>Gets the vector (0,1,0,0).</summary>
  public static Vector4 UnitY {
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    get => new(0f, 1f, 0f, 0f);
  }

  /// <summary>Gets the vector (0,0,1,0).</summary>
  public static Vector4 UnitZ {
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    get => new(0f, 0f, 1f, 0f);
  }

  /// <summary>Gets the vector (0,0,0,1).</summary>
  public static Vector4 UnitW {
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    get => new(0f, 0f, 0f, 1f);
  }

  #endregion

  #region Instance Methods

  /// <summary>Returns the length of this vector object.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly float Length() => (float)Math.Sqrt(this.LengthSquared());

  /// <summary>Returns the length of the vector squared.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly float LengthSquared() => this.X * this.X + this.Y * this.Y + this.Z * this.Z + this.W * this.W;

  #endregion

  #region Static Methods

  /// <summary>Returns a vector whose elements are the absolute values of each of the specified vector's elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 Abs(Vector4 value) => new(Math.Abs(value.X), Math.Abs(value.Y), Math.Abs(value.Z), Math.Abs(value.W));

  /// <summary>Adds two vectors together.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 Add(Vector4 left, Vector4 right) => left + right;

  /// <summary>Restricts a vector between a minimum and a maximum value.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 Clamp(Vector4 value, Vector4 min, Vector4 max) => new(
    Math.Max(min.X, Math.Min(max.X, value.X)),
    Math.Max(min.Y, Math.Min(max.Y, value.Y)),
    Math.Max(min.Z, Math.Min(max.Z, value.Z)),
    Math.Max(min.W, Math.Min(max.W, value.W))
  );

  /// <summary>Computes the Euclidean distance between the two given points.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Distance(Vector4 value1, Vector4 value2) => (value1 - value2).Length();

  /// <summary>Returns the Euclidean distance squared between two specified points.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float DistanceSquared(Vector4 value1, Vector4 value2) => (value1 - value2).LengthSquared();

  /// <summary>Divides the first vector by the second.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 Divide(Vector4 left, Vector4 right) => left / right;

  /// <summary>Divides the specified vector by a specified scalar value.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 Divide(Vector4 left, float divisor) => left / divisor;

  /// <summary>Returns the dot product of two vectors.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Dot(Vector4 vector1, Vector4 vector2) => vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z + vector1.W * vector2.W;

  /// <summary>Performs a linear interpolation between two vectors based on the given weighting.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 Lerp(Vector4 value1, Vector4 value2, float amount) => new(
    value1.X + (value2.X - value1.X) * amount,
    value1.Y + (value2.Y - value1.Y) * amount,
    value1.Z + (value2.Z - value1.Z) * amount,
    value1.W + (value2.W - value1.W) * amount
  );

  /// <summary>Returns a vector whose elements are the maximum of each of the pairs of elements in two specified vectors.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 Max(Vector4 value1, Vector4 value2) => new(
    Math.Max(value1.X, value2.X),
    Math.Max(value1.Y, value2.Y),
    Math.Max(value1.Z, value2.Z),
    Math.Max(value1.W, value2.W)
  );

  /// <summary>Returns a vector whose elements are the minimum of each of the pairs of elements in two specified vectors.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 Min(Vector4 value1, Vector4 value2) => new(
    Math.Min(value1.X, value2.X),
    Math.Min(value1.Y, value2.Y),
    Math.Min(value1.Z, value2.Z),
    Math.Min(value1.W, value2.W)
  );

  /// <summary>Returns a new vector whose values are the product of each pair of elements in two specified vectors.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 Multiply(Vector4 left, Vector4 right) => left * right;

  /// <summary>Multiplies a vector by a specified scalar.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 Multiply(Vector4 left, float right) => left * right;

  /// <summary>Multiplies a scalar value by a specified vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 Multiply(float left, Vector4 right) => left * right;

  /// <summary>Negates a specified vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 Negate(Vector4 value) => -value;

  /// <summary>Returns a vector with the same direction as the specified vector, but with a length of one.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 Normalize(Vector4 vector) => vector / vector.Length();

  /// <summary>Returns a vector whose elements are the square root of each of a specified vector's elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 SquareRoot(Vector4 value) => new((float)Math.Sqrt(value.X), (float)Math.Sqrt(value.Y), (float)Math.Sqrt(value.Z), (float)Math.Sqrt(value.W));

  /// <summary>Subtracts the second vector from the first.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 Subtract(Vector4 left, Vector4 right) => left - right;

  /// <summary>Transforms a two-dimensional vector by a specified 4x4 matrix.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 Transform(Vector2 position, Matrix4x4 matrix) => new(
    position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M41,
    position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M42,
    position.X * matrix.M13 + position.Y * matrix.M23 + matrix.M43,
    position.X * matrix.M14 + position.Y * matrix.M24 + matrix.M44
  );

  /// <summary>Transforms a three-dimensional vector by a specified 4x4 matrix.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 Transform(Vector3 position, Matrix4x4 matrix) => new(
    position.X * matrix.M11 + position.Y * matrix.M21 + position.Z * matrix.M31 + matrix.M41,
    position.X * matrix.M12 + position.Y * matrix.M22 + position.Z * matrix.M32 + matrix.M42,
    position.X * matrix.M13 + position.Y * matrix.M23 + position.Z * matrix.M33 + matrix.M43,
    position.X * matrix.M14 + position.Y * matrix.M24 + position.Z * matrix.M34 + matrix.M44
  );

  /// <summary>Transforms a four-dimensional vector by a specified 4x4 matrix.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 Transform(Vector4 vector, Matrix4x4 matrix) => new(
    vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31 + vector.W * matrix.M41,
    vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32 + vector.W * matrix.M42,
    vector.X * matrix.M13 + vector.Y * matrix.M23 + vector.Z * matrix.M33 + vector.W * matrix.M43,
    vector.X * matrix.M14 + vector.Y * matrix.M24 + vector.Z * matrix.M34 + vector.W * matrix.M44
  );

  /// <summary>Transforms a two-dimensional vector by the specified Quaternion rotation value.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 Transform(Vector2 value, Quaternion rotation) {
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
      value.X * (1f - yy2 - zz2) + value.Y * (xy2 - wz2),
      value.X * (xy2 + wz2) + value.Y * (1f - xx2 - zz2),
      value.X * (xz2 - wy2) + value.Y * (yz2 + wx2),
      1f
    );
  }

  /// <summary>Transforms a three-dimensional vector by the specified Quaternion rotation value.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 Transform(Vector3 value, Quaternion rotation) {
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
      value.X * (xz2 - wy2) + value.Y * (yz2 + wx2) + value.Z * (1f - xx2 - yy2),
      1f
    );
  }

  /// <summary>Transforms a four-dimensional vector by the specified Quaternion rotation value.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 Transform(Vector4 value, Quaternion rotation) {
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
      value.X * (xz2 - wy2) + value.Y * (yz2 + wx2) + value.Z * (1f - xx2 - yy2),
      value.W
    );
  }

  #endregion

  #region Operators

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 operator +(Vector4 left, Vector4 right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 operator -(Vector4 left, Vector4 right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 operator *(Vector4 left, Vector4 right) => new(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 operator *(Vector4 left, float right) => new(left.X * right, left.Y * right, left.Z * right, left.W * right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 operator *(float left, Vector4 right) => new(left * right.X, left * right.Y, left * right.Z, left * right.W);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 operator /(Vector4 left, Vector4 right) => new(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 operator /(Vector4 value, float divisor) => new(value.X / divisor, value.Y / divisor, value.Z / divisor, value.W / divisor);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector4 operator -(Vector4 value) => new(-value.X, -value.Y, -value.Z, -value.W);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator ==(Vector4 left, Vector4 right) => left.Equals(right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator !=(Vector4 left, Vector4 right) => !left.Equals(right);

  #endregion

  #region Equality

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly bool Equals(Vector4 other) => this.X == other.X && this.Y == other.Y && this.Z == other.Z && this.W == other.W;

  /// <inheritdoc/>
  public override readonly bool Equals(object obj) => obj is Vector4 other && this.Equals(other);

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
