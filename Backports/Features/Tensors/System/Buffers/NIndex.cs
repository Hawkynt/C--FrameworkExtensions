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

#if !OFFICIAL_TENSORS

namespace System.Buffers;

/// <summary>
/// Represents a native integer index that can be used to index into collections.
/// </summary>
public readonly struct NIndex : IEquatable<NIndex> {

  private readonly nint _value;
  private readonly bool _fromEnd;

  /// <summary>
  /// Initializes a new instance of <see cref="NIndex"/> with the specified value.
  /// </summary>
  /// <param name="value">The index value. Must be non-negative.</param>
  /// <param name="fromEnd">Indicates whether the index is from the end.</param>
  public NIndex(nint value, bool fromEnd = false) {
    if (value < 0)
      throw new ArgumentOutOfRangeException(nameof(value), "Value must be non-negative.");
    this._value = value;
    this._fromEnd = fromEnd;
  }

  /// <summary>
  /// Initializes a new instance of <see cref="NIndex"/> from an <see cref="Index"/>.
  /// </summary>
  /// <param name="index">The index to convert from.</param>
  public NIndex(Index index) {
    this._value = index.IsFromEnd ? index.Value : index.Value;
    this._fromEnd = index.IsFromEnd;
  }

  /// <summary>Gets an <see cref="NIndex"/> pointing to the first element.</summary>
  public static NIndex Start => new(0);

  /// <summary>Gets an <see cref="NIndex"/> pointing past the last element.</summary>
  public static NIndex End => new(0, fromEnd: true);

  /// <summary>Gets the index value.</summary>
  public nint Value => this._value;

  /// <summary>Gets a value indicating whether the index is from the end.</summary>
  public bool IsFromEnd => this._fromEnd;

  /// <summary>Creates an <see cref="NIndex"/> from the start of a collection.</summary>
  /// <param name="value">The index value from the start.</param>
  public static NIndex FromStart(nint value) {
    if (value < 0)
      throw new ArgumentOutOfRangeException(nameof(value), "Value must be non-negative.");
    return new(value, fromEnd: false);
  }

  /// <summary>Creates an <see cref="NIndex"/> from the end of a collection.</summary>
  /// <param name="value">The index value from the end.</param>
  public static NIndex FromEnd(nint value) {
    if (value < 0)
      throw new ArgumentOutOfRangeException(nameof(value), "Value must be non-negative.");
    return new(value, fromEnd: true);
  }

  /// <summary>Calculates the offset from the start of a collection with the given length.</summary>
  /// <param name="length">The length of the collection.</param>
  /// <returns>The offset from the start.</returns>
  public nint GetOffset(nint length) {
    var offset = this._fromEnd ? length - this._value : this._value;
    if (offset < 0 || offset > length)
      throw new ArgumentOutOfRangeException(nameof(length), "Index was out of range.");
    return offset;
  }

  /// <summary>Converts this <see cref="NIndex"/> to an <see cref="Index"/>.</summary>
  /// <returns>An equivalent <see cref="Index"/>.</returns>
  /// <exception cref="OverflowException">Thrown if the value is too large for an <see cref="Index"/>.</exception>
  public Index ToIndex() {
    if (this._value > int.MaxValue)
      throw new OverflowException("NIndex value is too large to convert to Index.");
    return this._fromEnd ? Index.FromEnd((int)this._value) : Index.FromStart((int)this._value);
  }

  /// <summary>Converts this <see cref="NIndex"/> to an <see cref="Index"/> without overflow checking.</summary>
  /// <returns>An equivalent <see cref="Index"/>.</returns>
  public Index ToIndexUnchecked()
    => this._fromEnd ? Index.FromEnd((int)this._value) : Index.FromStart((int)this._value);

  /// <inheritdoc/>
  public bool Equals(NIndex other)
    => this._value == other._value && this._fromEnd == other._fromEnd;

  /// <inheritdoc/>
  public override bool Equals(object? obj)
    => obj is NIndex other && this.Equals(other);

  /// <inheritdoc/>
  public override int GetHashCode()
    => HashCode.Combine(this._value, this._fromEnd);

  /// <inheritdoc/>
  public override string ToString()
    => this._fromEnd ? $"^{this._value}" : this._value.ToString();

  /// <summary>Explicitly converts an <see cref="NIndex"/> to an <see cref="Index"/>.</summary>
  public static explicit operator Index(NIndex value)
    => value.ToIndex();

  /// <summary>Implicitly converts an <see cref="Index"/> to an <see cref="NIndex"/>.</summary>
  public static implicit operator NIndex(Index value)
    => new(value);

  /// <summary>Implicitly converts a native integer to an <see cref="NIndex"/>.</summary>
  public static implicit operator NIndex(nint value)
    => new(value);

  /// <summary>Determines whether two <see cref="NIndex"/> instances are equal.</summary>
  public static bool operator ==(NIndex left, NIndex right)
    => left.Equals(right);

  /// <summary>Determines whether two <see cref="NIndex"/> instances are not equal.</summary>
  public static bool operator !=(NIndex left, NIndex right)
    => !left.Equals(right);

}

#endif
