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
/// Represents a Windows FILETIME value: the number of 100-nanosecond intervals
/// since 1601-01-01 00:00:00 UTC.
/// </summary>
public readonly struct FileTime : IComparable, IComparable<FileTime>, IEquatable<FileTime> {

  /// <summary>
  /// Gets the raw 100-nanosecond tick count since 1601-01-01 UTC.
  /// </summary>
  public ulong RawValue { get; }

  private FileTime(ulong raw) => this.RawValue = raw;

  /// <summary>
  /// Creates a <see cref="FileTime"/> from the raw 100-ns tick count.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static FileTime FromRaw(ulong raw) => new(raw);

  /// <summary>
  /// Converts this value to a UTC <see cref="DateTime"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public DateTime ToDateTime() => DateTime.FromFileTimeUtc((long)this.RawValue);

  /// <summary>
  /// Creates a <see cref="FileTime"/> from a <see cref="DateTime"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static FileTime FromDateTime(DateTime dt) => new((ulong)dt.ToUniversalTime().ToFileTimeUtc());

  // Comparison
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(FileTime other) => this.RawValue.CompareTo(other.RawValue);

  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not FileTime other)
      throw new ArgumentException("Object must be of type FileTime.", nameof(obj));
    return this.CompareTo(other);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(FileTime other) => this.RawValue == other.RawValue;

  public override bool Equals(object? obj) => obj is FileTime other && this.Equals(other);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this.RawValue.GetHashCode();

  public override string ToString() => this.ToDateTime().ToString("o", CultureInfo.InvariantCulture);

  // Operators
  public static bool operator ==(FileTime left, FileTime right) => left.Equals(right);
  public static bool operator !=(FileTime left, FileTime right) => !left.Equals(right);
  public static bool operator <(FileTime left, FileTime right) => left.CompareTo(right) < 0;
  public static bool operator >(FileTime left, FileTime right) => left.CompareTo(right) > 0;
  public static bool operator <=(FileTime left, FileTime right) => left.CompareTo(right) <= 0;
  public static bool operator >=(FileTime left, FileTime right) => left.CompareTo(right) >= 0;

  // Conversions
  public static implicit operator DateTime(FileTime value) => value.ToDateTime();

}
