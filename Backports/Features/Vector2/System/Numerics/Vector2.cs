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
/// Represents a vector with two single-precision floating-point values.
/// </summary>
public struct Vector2 : IEquatable<Vector2>
#if SUPPORTS_SPAN_FORMATTABLE
  , IFormattable
#endif
{
  /// <summary>The X component of the vector.</summary>
  public float X;

  /// <summary>The Y component of the vector.</summary>
  public float Y;

  /// <summary>
  /// Creates a new <see cref="Vector2"/> object whose two elements have the same value.
  /// </summary>
  /// <param name="value">The value to assign to both elements.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Vector2(float value) : this(value, value) { }

  /// <summary>
  /// Creates a vector whose elements have the specified values.
  /// </summary>
  /// <param name="x">The value to assign to the <see cref="X"/> field.</param>
  /// <param name="y">The value to assign to the <see cref="Y"/> field.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Vector2(float x, float y) {
    this.X = x;
    this.Y = y;
  }

  #region Static Properties

  /// <summary>Gets the vector (0,0).</summary>
  public static Vector2 Zero {
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    get => new(0f, 0f);
  }

  /// <summary>Gets the vector (1,1).</summary>
  public static Vector2 One {
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    get => new(1f, 1f);
  }

  /// <summary>Gets the vector (1,0).</summary>
  public static Vector2 UnitX {
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    get => new(1f, 0f);
  }

  /// <summary>Gets the vector (0,1).</summary>
  public static Vector2 UnitY {
#if SUPPORTS_INLINING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    get => new(0f, 1f);
  }

  #endregion

  #region Instance Methods

  /// <summary>Returns the length of the vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly float Length() => (float)Math.Sqrt(this.LengthSquared());

  /// <summary>Returns the length of the vector squared.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly float LengthSquared() => this.X * this.X + this.Y * this.Y;

  #endregion

  #region Static Methods

  /// <summary>Returns a vector whose elements are the absolute values of each of the specified vector's elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 Abs(Vector2 value) => new(Math.Abs(value.X), Math.Abs(value.Y));

  /// <summary>Adds two vectors together.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 Add(Vector2 left, Vector2 right) => left + right;

  /// <summary>Restricts a vector between a minimum and a maximum value.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 Clamp(Vector2 value, Vector2 min, Vector2 max) => new(
    Math.Max(min.X, Math.Min(max.X, value.X)),
    Math.Max(min.Y, Math.Min(max.Y, value.Y))
  );

  /// <summary>Computes the Euclidean distance between the two given points.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Distance(Vector2 value1, Vector2 value2) => (value1 - value2).Length();

  /// <summary>Returns the Euclidean distance squared between two specified points.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float DistanceSquared(Vector2 value1, Vector2 value2) => (value1 - value2).LengthSquared();

  /// <summary>Divides the first vector by the second.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 Divide(Vector2 left, Vector2 right) => left / right;

  /// <summary>Divides the specified vector by a specified scalar value.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 Divide(Vector2 left, float divisor) => left / divisor;

  /// <summary>Returns the dot product of two vectors.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Dot(Vector2 value1, Vector2 value2) => value1.X * value2.X + value1.Y * value2.Y;

  /// <summary>Performs a linear interpolation between two vectors based on the given weighting.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 Lerp(Vector2 value1, Vector2 value2, float amount) => new(
    value1.X + (value2.X - value1.X) * amount,
    value1.Y + (value2.Y - value1.Y) * amount
  );

  /// <summary>Returns a vector whose elements are the maximum of each of the pairs of elements in two specified vectors.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 Max(Vector2 value1, Vector2 value2) => new(
    Math.Max(value1.X, value2.X),
    Math.Max(value1.Y, value2.Y)
  );

  /// <summary>Returns a vector whose elements are the minimum of each of the pairs of elements in two specified vectors.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 Min(Vector2 value1, Vector2 value2) => new(
    Math.Min(value1.X, value2.X),
    Math.Min(value1.Y, value2.Y)
  );

  /// <summary>Returns a new vector whose values are the product of each pair of elements in two specified vectors.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 Multiply(Vector2 left, Vector2 right) => left * right;

  /// <summary>Multiplies a vector by a specified scalar.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 Multiply(Vector2 left, float right) => left * right;

  /// <summary>Multiplies a scalar value by a specified vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 Multiply(float left, Vector2 right) => left * right;

  /// <summary>Negates a specified vector.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 Negate(Vector2 value) => -value;

  /// <summary>Returns a vector with the same direction as the specified vector, but with a length of one.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 Normalize(Vector2 value) => value / value.Length();

  /// <summary>Returns the reflection of a vector off a surface that has the specified normal.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 Reflect(Vector2 vector, Vector2 normal) => vector - 2f * Dot(vector, normal) * normal;

  /// <summary>Returns a vector whose elements are the square root of each of a specified vector's elements.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 SquareRoot(Vector2 value) => new((float)Math.Sqrt(value.X), (float)Math.Sqrt(value.Y));

  /// <summary>Subtracts the second vector from the first.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 Subtract(Vector2 left, Vector2 right) => left - right;

  /// <summary>Transforms a vector by a specified 3x2 matrix.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 Transform(Vector2 position, Matrix3x2 matrix) => new(
    position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M31,
    position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M32
  );

  /// <summary>Transforms a vector by a specified 4x4 matrix.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 Transform(Vector2 position, Matrix4x4 matrix) => new(
    position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M41,
    position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M42
  );

  /// <summary>Transforms a vector by the specified Quaternion rotation value.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 Transform(Vector2 value, Quaternion rotation) {
    var x2 = rotation.X + rotation.X;
    var y2 = rotation.Y + rotation.Y;
    var z2 = rotation.Z + rotation.Z;
    var wz2 = rotation.W * z2;
    var xx2 = rotation.X * x2;
    var xy2 = rotation.X * y2;
    var yy2 = rotation.Y * y2;
    var zz2 = rotation.Z * z2;
    return new(
      value.X * (1f - yy2 - zz2) + value.Y * (xy2 - wz2),
      value.X * (xy2 + wz2) + value.Y * (1f - xx2 - zz2)
    );
  }

  /// <summary>Transforms a vector normal by the given 3x2 matrix.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 TransformNormal(Vector2 normal, Matrix3x2 matrix) => new(
    normal.X * matrix.M11 + normal.Y * matrix.M21,
    normal.X * matrix.M12 + normal.Y * matrix.M22
  );

  /// <summary>Transforms a vector normal by the given 4x4 matrix.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 TransformNormal(Vector2 normal, Matrix4x4 matrix) => new(
    normal.X * matrix.M11 + normal.Y * matrix.M21,
    normal.X * matrix.M12 + normal.Y * matrix.M22
  );

  #endregion

  #region Operators

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 operator +(Vector2 left, Vector2 right) => new(left.X + right.X, left.Y + right.Y);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 operator -(Vector2 left, Vector2 right) => new(left.X - right.X, left.Y - right.Y);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 operator *(Vector2 left, Vector2 right) => new(left.X * right.X, left.Y * right.Y);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 operator *(Vector2 left, float right) => new(left.X * right, left.Y * right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 operator *(float left, Vector2 right) => new(left * right.X, left * right.Y);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 operator /(Vector2 left, Vector2 right) => new(left.X / right.X, left.Y / right.Y);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 operator /(Vector2 value, float divisor) => new(value.X / divisor, value.Y / divisor);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector2 operator -(Vector2 value) => new(-value.X, -value.Y);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator ==(Vector2 left, Vector2 right) => left.Equals(right);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator !=(Vector2 left, Vector2 right) => !left.Equals(right);

  #endregion

  #region Equality

  /// <inheritdoc/>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly bool Equals(Vector2 other) => this.X == other.X && this.Y == other.Y;

  /// <inheritdoc/>
  public override readonly bool Equals(object obj) => obj is Vector2 other && this.Equals(other);

  /// <inheritdoc/>
  public override readonly int GetHashCode() => this.X.GetHashCode() ^ (this.Y.GetHashCode() << 2);

  #endregion

  /// <inheritdoc/>
  public override readonly string ToString() => this.ToString("G", CultureInfo.CurrentCulture);

  /// <summary>Returns a string representation of the current instance using the specified format string.</summary>
  public readonly string ToString(string format) => this.ToString(format, CultureInfo.CurrentCulture);

  /// <summary>Returns a string representation of the current instance using the specified format string and format provider.</summary>
  public readonly string ToString(string format, IFormatProvider formatProvider) {
    var separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;
    return $"<{this.X.ToString(format, formatProvider)}{separator} {this.Y.ToString(format, formatProvider)}>";
  }
}

#endif
