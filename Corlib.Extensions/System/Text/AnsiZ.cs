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
/// Represents a zero-terminated Windows-1252 (ANSI) encoded string that cuts content at the first NUL byte.
/// </summary>
/// <remarks>
/// This type is useful for interoperability with native code that uses null-terminated ANSI strings.
/// It uses Windows-1252 (code page 1252) encoding, which is a superset of ISO-8859-1 (Latin-1).
/// Any NUL bytes (0x00) in the input are treated as terminators, and content after them is discarded.
/// </remarks>
public readonly struct AnsiZ : IEquatable<AnsiZ>, IComparable<AnsiZ>, IComparable {
  private static readonly byte[] _emptyBytes = [];

  /// <summary>
  /// Gets an empty <see cref="AnsiZ"/> instance.
  /// </summary>
  public static AnsiZ Empty => new(_emptyBytes);

  /// <summary>
  /// Initializes a new instance of the <see cref="AnsiZ"/> struct from a byte array.
  /// </summary>
  /// <param name="value">The byte array. Content after the first NUL byte is discarded.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public AnsiZ(byte[]? value) => this.Bytes = value == null ? _emptyBytes : _TruncateAtNull(value.AsSpan());

  /// <summary>
  /// Initializes a new instance of the <see cref="AnsiZ"/> struct from a byte span.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public AnsiZ(ReadOnlySpan<byte> value) => this.Bytes = _TruncateAtNull(value);

  /// <summary>
  /// Initializes a new instance of the <see cref="AnsiZ"/> struct from a string.
  /// </summary>
  /// <param name="value">The string value. Content after the first NUL character is discarded.</param>
  /// <remarks>
  /// Characters that cannot be represented in Windows-1252 are replaced with '?' (0x3F).
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public AnsiZ(string? value)
    => this.Bytes = string.IsNullOrEmpty(value) ? _emptyBytes : _ProcessString(value.AsSpan());

  /// <summary>
  /// Initializes a new instance of the <see cref="AnsiZ"/> struct from a character span.
  /// </summary>
  /// <remarks>
  /// Characters that cannot be represented in Windows-1252 are replaced with '?' (0x3F).
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public AnsiZ(ReadOnlySpan<char> value) => this.Bytes = _ProcessString(value);

  private static byte[] _TruncateAtNull(ReadOnlySpan<byte> value) {
    if (value.IsEmpty)
      return _emptyBytes;

    var nullIndex = value.IndexOf((byte)0);
    return nullIndex < 0 ? value.ToArray() : value.Slice(0, nullIndex).ToArray();
  }

  private static byte[] _ProcessString(ReadOnlySpan<char> value) {
    if (value.IsEmpty)
      return _emptyBytes;

    var nullIndex = value.IndexOf('\0');
    var length = nullIndex < 0 ? value.Length : nullIndex;

    var result = new byte[length];
    for (var i = 0; i < length; ++i)
      result[i] = Windows1252Encoding.ToByte(value[i]);

    return result;
  }

  internal byte[] Bytes {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => field ?? _emptyBytes;
  }

  /// <summary>
  /// Creates an <see cref="AnsiZ"/> from a byte array. Internal use only.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static AnsiZ FromBytes(byte[]? bytes)
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
  public AnsiZ this[Range range] {
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
  public AnsiZ Substring(int startIndex) {
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
  public AnsiZ Substring(int startIndex, int length) {
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
      for (var i = 0; i < bytes.Length; ++i)
        hash = hash * 31 + bytes[i];
      return hash;
    }
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override bool Equals(object? obj) => obj is AnsiZ other && this.Equals(other);

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(AnsiZ other) {
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
  public int CompareTo(AnsiZ other) {
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
    if (obj is not AnsiZ other)
      throw new ArgumentException("Object must be of type AnsiZ.", nameof(obj));
    return this.CompareTo(other);
  }

  /// <summary>
  /// Determines whether two <see cref="AnsiZ"/> instances are equal.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator ==(AnsiZ left, AnsiZ right) => left.Equals(right);

  /// <summary>
  /// Determines whether two <see cref="AnsiZ"/> instances are not equal.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator !=(AnsiZ left, AnsiZ right) => !left.Equals(right);

  /// <summary>
  /// Determines whether the left <see cref="AnsiZ"/> is less than the right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator <(AnsiZ left, AnsiZ right) => left.CompareTo(right) < 0;

  /// <summary>
  /// Determines whether the left <see cref="AnsiZ"/> is greater than the right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator >(AnsiZ left, AnsiZ right) => left.CompareTo(right) > 0;

  /// <summary>
  /// Determines whether the left <see cref="AnsiZ"/> is less than or equal to the right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator <=(AnsiZ left, AnsiZ right) => left.CompareTo(right) <= 0;

  /// <summary>
  /// Determines whether the left <see cref="AnsiZ"/> is greater than or equal to the right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator >=(AnsiZ left, AnsiZ right) => left.CompareTo(right) >= 0;

  /// <summary>
  /// Implicitly converts a <see cref="string"/> to an <see cref="AnsiZ"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator AnsiZ(string? value) => new(value);

  /// <summary>
  /// Implicitly converts an <see cref="AnsiZ"/> to a <see cref="string"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator string(AnsiZ value) => value.ToString();

  /// <summary>
  /// Implicitly converts an <see cref="AnsiZ"/> to an <see cref="AnsiString"/>.
  /// Safe because no data is lost.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator AnsiString(AnsiZ value) => AnsiString.FromBytes(value.Bytes);

  /// <summary>
  /// Explicitly converts an <see cref="AnsiString"/> to an <see cref="AnsiZ"/>.
  /// May truncate at the first embedded null character.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static explicit operator AnsiZ(AnsiString value) => FromAnsiString(value);

  /// <summary>
  /// Explicitly converts a <see cref="FixedAnsi"/> to an <see cref="AnsiZ"/>.
  /// May truncate at the first embedded null character.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static explicit operator AnsiZ(FixedAnsi value) => FromFixedAnsi(value);

  /// <summary>
  /// Creates an <see cref="AnsiZ"/> from an <see cref="AnsiString"/>, truncating at the first null.
  /// </summary>
  private static AnsiZ FromAnsiString(AnsiString value) {
    if (value.IsEmpty)
      return Empty;

    var bytes = value.Bytes;
    var nullIndex = Array.IndexOf(bytes, (byte)0);
    if (nullIndex < 0)
      return new(bytes);
    if (nullIndex == 0)
      return Empty;

    var result = new byte[nullIndex];
    Array.Copy(bytes, result, nullIndex);
    return new(result);
  }

  /// <summary>
  /// Creates an <see cref="AnsiZ"/> from a <see cref="FixedAnsi"/>, truncating at the first null.
  /// </summary>
  private static AnsiZ FromFixedAnsi(FixedAnsi value) {
    if (value.IsEmpty)
      return Empty;

    var bytes = value.Bytes;
    var length = value.Length;
    var nullIndex = -1;
    for (var i = 0; i < length; ++i)
      if (bytes[i] == 0) {
        nullIndex = i;
        break;
      }

    if (nullIndex == 0)
      return Empty;
    if (nullIndex < 0)
      nullIndex = length;

    var result = new byte[nullIndex];
    Array.Copy(bytes, result, nullIndex);
    return new(result);
  }

  /// <summary>
  /// Concatenates two <see cref="AnsiZ"/> instances.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static AnsiZ operator +(AnsiZ left, AnsiZ right) {
    var leftBytes = left.Bytes;
    var rightBytes = right.Bytes;
    var result = new byte[leftBytes.Length + rightBytes.Length];
    Array.Copy(leftBytes, result, leftBytes.Length);
    Array.Copy(rightBytes, 0, result, leftBytes.Length, rightBytes.Length);
    return new(result);
  }

  /// <summary>
  /// Explicitly converts an <see cref="AnsiZ"/> to an <see cref="AsciiZ"/>.
  /// May throw if the value contains non-ASCII characters (bytes &gt; 127).
  /// </summary>
  /// <exception cref="ArgumentException">The value contains non-ASCII characters.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static explicit operator AsciiZ(AnsiZ value) => new(value.Bytes);

  /// <summary>
  /// Explicitly converts an <see cref="AnsiZ"/> to an <see cref="AsciiString"/>.
  /// May throw if the value contains non-ASCII characters (bytes &gt; 127).
  /// </summary>
  /// <exception cref="ArgumentException">The value contains non-ASCII characters.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static explicit operator AsciiString(AnsiZ value) => new(value.Bytes);

}
