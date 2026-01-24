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

#nullable enable

using System.Runtime.CompilerServices;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Text;

/// <summary>
/// Represents an immutable Windows-1252 (ANSI) encoded string stored as bytes.
/// </summary>
/// <remarks>
/// Unlike <see cref="AnsiZ"/>, this type does not treat NUL bytes as terminators.
/// It uses Windows-1252 (code page 1252) encoding, which is a superset of ISO-8859-1 (Latin-1).
/// </remarks>
public readonly struct AnsiString : IEquatable<AnsiString>, IComparable<AnsiString>, IComparable {
  private static readonly byte[] _emptyBytes = [];

  /// <summary>
  /// Gets an empty <see cref="AnsiString"/> instance.
  /// </summary>
  public static AnsiString Empty => new(_emptyBytes);

  /// <summary>
  /// Initializes a new instance of the <see cref="AnsiString"/> struct from a byte array.
  /// </summary>
  /// <param name="value">The byte array containing ANSI data.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public AnsiString(byte[]? value) => this.Bytes = value == null ? _emptyBytes : value.AsSpan().ToArray();

  /// <summary>
  /// Initializes a new instance of the <see cref="AnsiString"/> struct from a byte span.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public AnsiString(ReadOnlySpan<byte> value) => this.Bytes = value.IsEmpty ? _emptyBytes : value.ToArray();

  /// <summary>
  /// Initializes a new instance of the <see cref="AnsiString"/> struct from a string.
  /// </summary>
  /// <param name="value">The string value.</param>
  /// <remarks>
  /// Characters that cannot be represented in Windows-1252 are replaced with '?' (0x3F).
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public AnsiString(string? value) => this.Bytes = string.IsNullOrEmpty(value) ? _emptyBytes : Windows1252Encoding.GetBytes(value);

  /// <summary>
  /// Initializes a new instance of the <see cref="AnsiString"/> struct from a character span.
  /// </summary>
  /// <remarks>
  /// Characters that cannot be represented in Windows-1252 are replaced with '?' (0x3F).
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public AnsiString(ReadOnlySpan<char> value) => this.Bytes = value.IsEmpty ? _emptyBytes : Windows1252Encoding.GetBytes(value);

  internal byte[] Bytes {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => field ?? _emptyBytes;
  }

  /// <summary>
  /// Creates an <see cref="AnsiString"/> from a byte array. Internal use only.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static AnsiString FromBytes(byte[]? bytes)
    => bytes == null || bytes.Length == 0 ? Empty : new(bytes);

  /// <summary>
  /// Gets the number of bytes in the string.
  /// </summary>
  public int Length {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Bytes.Length;
  }

  /// <summary>
  /// Gets a value indicating whether this string is empty.
  /// </summary>
  public bool IsEmpty {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Bytes.Length == 0;
  }

  /// <summary>
  /// Gets the byte at the specified position.
  /// </summary>
  public byte this[int index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var bytes = this.Bytes;
      if ((uint)index >= (uint)bytes.Length)
        AlwaysThrow.IndexOutOfRangeException();
      return bytes[index];
    }
  }

  /// <summary>
  /// Gets the byte at the specified position using an <see cref="Index"/>.
  /// </summary>
  public byte this[Index index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var bytes = this.Bytes;
      var offset = index.GetOffset(bytes.Length);
      if ((uint)offset >= (uint)bytes.Length)
        AlwaysThrow.IndexOutOfRangeException();
      return bytes[offset];
    }
  }

  /// <summary>
  /// Gets a substring specified by a <see cref="Range"/>.
  /// </summary>
  public AnsiString this[Range range] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var bytes = this.Bytes;
      var (start, length) = range.GetOffsetAndLength(bytes.Length);
      var result = new byte[length];
      Array.Copy(bytes, start, result, 0, length);
      return new(result);
    }
  }

  /// <summary>
  /// Retrieves a substring starting at the specified position.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public AnsiString Substring(int startIndex) {
    var bytes = this.Bytes;
    ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)startIndex, (uint)bytes.Length);
    var length = bytes.Length - startIndex;
    var result = new byte[length];
    Array.Copy(bytes, startIndex, result, 0, length);
    return new(result);
  }

  /// <summary>
  /// Retrieves a substring starting at the specified position with the specified length.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public AnsiString Substring(int startIndex, int length) {
    var bytes = this.Bytes;
    ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)startIndex, (uint)bytes.Length);
    ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)length, (uint)(bytes.Length - startIndex));
    var result = new byte[length];
    Array.Copy(bytes, startIndex, result, 0, length);
    return new(result);
  }

  /// <summary>
  /// Returns a <see cref="ReadOnlySpan{T}"/> over the bytes of this string.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ReadOnlySpan<byte> AsSpan() => this.Bytes.AsSpan();

  /// <summary>
  /// Returns a <see cref="ReadOnlySpan{T}"/> over a portion of this string.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ReadOnlySpan<byte> AsSpan(int start) => this.Bytes.AsSpan(start);

  /// <summary>
  /// Returns a <see cref="ReadOnlySpan{T}"/> over a portion of this string.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ReadOnlySpan<byte> AsSpan(int start, int length) => this.Bytes.AsSpan(start, length);

  /// <summary>
  /// Returns a reference to the first byte for use with the fixed statement.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ref readonly byte GetPinnableReference() {
    var bytes = this.Bytes;
    return ref (bytes.Length > 0 ? ref bytes[0] : ref _NullRef());
  }

  private static ref readonly byte _NullRef() {
    unsafe {
      return ref Unsafe.AsRef<byte>(null);
    }
  }

  /// <summary>
  /// Returns a null-terminated byte array for P/Invoke.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public byte[] ToNullTerminatedArray() {
    var bytes = this.Bytes;
    var result = new byte[bytes.Length + 1];
    Array.Copy(bytes, result, bytes.Length);
    result[bytes.Length] = 0;
    return result;
  }

  /// <summary>
  /// Returns the underlying byte array.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public byte[] ToArray() {
    var bytes = this.Bytes;
    var result = new byte[bytes.Length];
    Array.Copy(bytes, result, bytes.Length);
    return result;
  }

  /// <summary>
  /// Returns the string representation using Windows-1252 encoding.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override string ToString() {
    var bytes = this.Bytes;
    return bytes.Length == 0 ? string.Empty : Windows1252Encoding.GetString(bytes);
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() {
    var bytes = this.Bytes;
    unchecked {
      var hash = 17;
      foreach (var @byte in bytes)
        hash = hash * 31 + @byte;

      return hash;
    }
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override bool Equals(object? obj) => obj is AnsiString other && this.Equals(other);

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(AnsiString other) {
    var bytes = this.Bytes;
    var otherBytes = other.Bytes;
    if (bytes.Length != otherBytes.Length)
      return false;
    
    for (var i = 0; i < bytes.Length; ++i)
      if (bytes[i] != otherBytes[i])
        return false;

    return true;
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(AnsiString other) {
    var bytes = this.Bytes;
    var otherBytes = other.Bytes;
    var minLength = bytes.Length < otherBytes.Length ? bytes.Length : otherBytes.Length;
    for (var i = 0; i < minLength; ++i) {
      var cmp = bytes[i].CompareTo(otherBytes[i]);
      if (cmp != 0)
        return cmp;
    }
    return bytes.Length.CompareTo(otherBytes.Length);
  }

  /// <inheritdoc />
  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    
    return obj is AnsiString other ? this.CompareTo(other) : throw new ArgumentException("Object must be of type AnsiString.", nameof(obj));
  }

  /// <summary>
  /// Determines whether two <see cref="AnsiString"/> instances are equal.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator ==(AnsiString left, AnsiString right) => left.Equals(right);

  /// <summary>
  /// Determines whether two <see cref="AnsiString"/> instances are not equal.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator !=(AnsiString left, AnsiString right) => !left.Equals(right);

  /// <summary>
  /// Determines whether the left <see cref="AnsiString"/> is less than the right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator <(AnsiString left, AnsiString right) => left.CompareTo(right) < 0;

  /// <summary>
  /// Determines whether the left <see cref="AnsiString"/> is greater than the right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator >(AnsiString left, AnsiString right) => left.CompareTo(right) > 0;

  /// <summary>
  /// Determines whether the left <see cref="AnsiString"/> is less than or equal to the right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator <=(AnsiString left, AnsiString right) => left.CompareTo(right) <= 0;

  /// <summary>
  /// Determines whether the left <see cref="AnsiString"/> is greater than or equal to the right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator >=(AnsiString left, AnsiString right) => left.CompareTo(right) >= 0;

  /// <summary>
  /// Implicitly converts a <see cref="string"/> to an <see cref="AnsiString"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator AnsiString(string? value) => new(value);

  /// <summary>
  /// Implicitly converts an <see cref="AnsiString"/> to a <see cref="string"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator string(AnsiString value) => value.ToString();

  /// <summary>
  /// Concatenates two <see cref="AnsiString"/> instances.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static AnsiString operator +(AnsiString left, AnsiString right) {
    var leftBytes = left.Bytes;
    var rightBytes = right.Bytes;
    var result = new byte[leftBytes.Length + rightBytes.Length];
    Array.Copy(leftBytes, result, leftBytes.Length);
    Array.Copy(rightBytes, 0, result, leftBytes.Length, rightBytes.Length);
    return new(result);
  }

  /// <summary>
  /// Explicitly converts an <see cref="AnsiString"/> to an <see cref="AsciiString"/>.
  /// May throw if the value contains non-ASCII characters (bytes &gt; 127).
  /// </summary>
  /// <exception cref="ArgumentException">The value contains non-ASCII characters.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static explicit operator AsciiString(AnsiString value) => new(value.Bytes);

}
