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
/// Represents a native integer range with start and end indices.
/// </summary>
public readonly struct NRange : IEquatable<NRange> {

  /// <summary>Gets the start index of the range.</summary>
  public NIndex Start { get; }

  /// <summary>Gets the end index of the range.</summary>
  public NIndex End { get; }

  /// <summary>
  /// Initializes a new instance of <see cref="NRange"/> with the specified start and end indices.
  /// </summary>
  /// <param name="start">The start index.</param>
  /// <param name="end">The end index.</param>
  public NRange(NIndex start, NIndex end) {
    this.Start = start;
    this.End = end;
  }

  /// <summary>
  /// Initializes a new instance of <see cref="NRange"/> from a <see cref="Range"/>.
  /// </summary>
  /// <param name="range">The range to convert from.</param>
  public NRange(Range range) {
    this.Start = new NIndex(range.Start);
    this.End = new NIndex(range.End);
  }

  /// <summary>Gets a range that represents the entire collection.</summary>
  public static NRange All => new(NIndex.Start, NIndex.End);

  /// <summary>Creates a range from the specified start index to the end.</summary>
  /// <param name="start">The start index.</param>
  public static NRange StartAt(NIndex start)
    => new(start, NIndex.End);

  /// <summary>Creates a range from the start to the specified end index.</summary>
  /// <param name="end">The end index.</param>
  public static NRange EndAt(NIndex end)
    => new(NIndex.Start, end);

  /// <summary>Calculates the offset and length for a collection with the given length.</summary>
  /// <param name="length">The length of the collection.</param>
  /// <returns>A tuple containing the offset and length.</returns>
  public (nint Offset, nint Length) GetOffsetAndLength(nint length) {
    var start = this.Start.GetOffset(length);
    var end = this.End.GetOffset(length);
    if (end < start)
      throw new ArgumentOutOfRangeException(nameof(length), "End index is less than start index.");
    return (start, end - start);
  }

  /// <summary>Converts this <see cref="NRange"/> to a <see cref="Range"/>.</summary>
  /// <returns>An equivalent <see cref="Range"/>.</returns>
  /// <exception cref="OverflowException">Thrown if the values are too large for a <see cref="Range"/>.</exception>
  public Range ToRange()
    => new(this.Start.ToIndex(), this.End.ToIndex());

  /// <summary>Converts this <see cref="NRange"/> to a <see cref="Range"/> without overflow checking.</summary>
  /// <returns>An equivalent <see cref="Range"/>.</returns>
  public Range ToRangeUnchecked()
    => new(this.Start.ToIndexUnchecked(), this.End.ToIndexUnchecked());

  /// <inheritdoc/>
  public bool Equals(NRange other)
    => this.Start == other.Start && this.End == other.End;

  /// <inheritdoc/>
  public override bool Equals(object obj)
    => obj is NRange other && this.Equals(other);

  /// <inheritdoc/>
  public override int GetHashCode()
    => HashCode.Combine(this.Start, this.End);

  /// <inheritdoc/>
  public override string ToString()
    => $"{this.Start}..{this.End}";

  /// <summary>Explicitly converts an <see cref="NRange"/> to a <see cref="Range"/>.</summary>
  public static explicit operator Range(NRange value)
    => value.ToRange();

  /// <summary>Implicitly converts a <see cref="Range"/> to an <see cref="NRange"/>.</summary>
  public static implicit operator NRange(Range value)
    => new(value);

  /// <summary>Determines whether two <see cref="NRange"/> instances are equal.</summary>
  public static bool operator ==(NRange left, NRange right)
    => left.Equals(right);

  /// <summary>Determines whether two <see cref="NRange"/> instances are not equal.</summary>
  public static bool operator !=(NRange left, NRange right)
    => !left.Equals(right);

}

#endif
