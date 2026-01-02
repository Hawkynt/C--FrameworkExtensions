#region (c)2010-2042 Hawkynt

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

#endregion

using System;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Metrics;

/// <summary>
/// A semantic float type that guarantees a value in the range [0.0, 1.0].
/// </summary>
/// <remarks>
/// <para>Used by normalized color metrics to express distance values that are
/// always within a predictable range, enabling consistent threshold comparisons
/// in image scaling algorithms.</para>
/// <para>Values outside [0,1] are clamped on construction.</para>
/// </remarks>
public readonly struct NormalizedFloat : IEquatable<NormalizedFloat>, IComparable<NormalizedFloat> {

  private readonly float _value;

  /// <summary>
  /// Creates a NormalizedFloat, clamping the value to [0.0, 1.0].
  /// </summary>
  /// <param name="value">The raw float value.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public NormalizedFloat(float value) =>
    this._value = value < 0f ? 0f : value > 1f ? 1f : value;

  /// <summary>
  /// Gets the underlying float value (guaranteed 0.0-1.0).
  /// </summary>
  public float Value {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._value;
  }

  /// <summary>
  /// Implicit conversion to float for seamless use in calculations.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator float(NormalizedFloat n) => n._value;

  /// <summary>
  /// Creates a NormalizedFloat from a byte value (0-255 mapped to 0.0-1.0).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static NormalizedFloat FromByte(byte value) => new(value / 255f);

  /// <summary>
  /// Creates a NormalizedFloat from a float, clamping to [0.0, 1.0].
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static NormalizedFloat FromFloat(float value) => new(value);

  /// <summary>
  /// Creates a NormalizedFloat from a raw distance and its known maximum.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static NormalizedFloat FromRatio(float distance, float maxDistance) => new(distance / maxDistance);

  /// <summary>
  /// The zero value (0.0).
  /// </summary>
  public static NormalizedFloat Zero => new(0f);

  /// <summary>
  /// The maximum value (1.0).
  /// </summary>
  public static NormalizedFloat One => new(1f);

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(NormalizedFloat other) => this._value.Equals(other._value);

  /// <inheritdoc />
  public override bool Equals(object obj) => obj is NormalizedFloat other && this.Equals(other);

  /// <inheritdoc />
  public override int GetHashCode() => this._value.GetHashCode();

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(NormalizedFloat other) => this._value.CompareTo(other._value);

  /// <inheritdoc />
  public override string ToString() => this._value.ToString("F4");

  public static bool operator ==(NormalizedFloat left, NormalizedFloat right) => left.Equals(right);
  public static bool operator !=(NormalizedFloat left, NormalizedFloat right) => !left.Equals(right);
  public static bool operator <(NormalizedFloat left, NormalizedFloat right) => left._value < right._value;
  public static bool operator >(NormalizedFloat left, NormalizedFloat right) => left._value > right._value;
  public static bool operator <=(NormalizedFloat left, NormalizedFloat right) => left._value <= right._value;
  public static bool operator >=(NormalizedFloat left, NormalizedFloat right) => left._value >= right._value;
}
