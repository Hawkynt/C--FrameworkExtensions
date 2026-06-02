#nullable enable

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

using System.Globalization;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

/// <summary>
/// Represents a 7-bit MIDI note number (0-127), where 69 is A4 = 440 Hz and 60 is middle C (C4).
/// </summary>
public readonly struct MidiNote : IEquatable<MidiNote>, IComparable<MidiNote>, IComparable, IFormattable {

  private static readonly string[] _names = ["C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"];

  /// <summary>Gets the note number (0-127).</summary>
  public byte Number { get; }

  private MidiNote(byte number) => this.Number = number;

  /// <summary>
  /// Creates a MIDI note from a number; only the low 7 bits are significant.
  /// </summary>
  public static MidiNote FromRaw(byte raw) => new((byte)(raw & 0x7F));

  /// <summary>
  /// Creates a MIDI note from a number (0-127).
  /// </summary>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="number"/> is out of range.</exception>
  public static MidiNote FromNumber(int number) {
    ArgumentOutOfRangeException.ThrowIfNegative(number);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(number, 127);
    return new((byte)number);
  }

  /// <summary>Gets the raw 7-bit value.</summary>
  public byte RawValue => this.Number;

  /// <summary>Gets the pitch-class name (C, C#, ... B).</summary>
  public string NoteName => _names[this.Number % 12];

  /// <summary>Gets the octave (C4 = middle C convention, so MIDI 60 is octave 4).</summary>
  public int Octave => this.Number / 12 - 1;

  /// <summary>Gets the equal-tempered frequency in Hz (A4 = 440 Hz).</summary>
  public double Frequency => 440.0 * Math.Pow(2.0, (this.Number - 69) / 12.0);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(MidiNote other) => this.Number == other.Number;

  public override bool Equals(object? obj) => obj is MidiNote other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.Number.GetHashCode();

  public int CompareTo(MidiNote other) => this.Number.CompareTo(other.Number);

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not MidiNote other)
      throw new ArgumentException("Object must be of type MidiNote.", nameof(obj));
    return this.CompareTo(other);
  }

  /// <summary>Returns the note name with octave, e.g. "C4", "A4", "F#5".</summary>
  public override string ToString() => this.NoteName + this.Octave.ToString(CultureInfo.InvariantCulture);

  public string ToString(string? format, IFormatProvider? formatProvider) => this.ToString();

  public static bool operator ==(MidiNote left, MidiNote right) => left.Equals(right);
  public static bool operator !=(MidiNote left, MidiNote right) => !left.Equals(right);

  public static explicit operator MidiNote(byte number) => FromNumber(number);
  public static implicit operator byte(MidiNote note) => note.Number;
}
