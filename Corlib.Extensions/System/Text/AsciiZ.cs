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
/// Specifies how to handle characters that cannot be represented in the target encoding.
/// </summary>
public enum InvalidCharBehavior {
  /// <summary>
  /// Throws an <see cref="ArgumentException"/> when an invalid character is encountered.
  /// </summary>
  Throw,

  /// <summary>
  /// Replaces invalid characters with a replacement character ('?' for byte-based encodings).
  /// </summary>
  Replace,

  /// <summary>
  /// Skips invalid characters entirely.
  /// </summary>
  Skip
}

/// <summary>
/// Represents a zero-terminated 7-bit ASCII string that cuts content at the first NUL byte.
/// </summary>
/// <remarks>
/// This type is useful for interoperability with native code that uses null-terminated ASCII strings.
/// All characters must be in the range 0-127. Any NUL bytes (0x00) in the input are treated as
/// terminators, and content after them is discarded.
/// Characters are stored in 7-bit packed format, saving 12.5% memory (8 characters use 7 bytes instead of 8).
/// </remarks>
public readonly struct AsciiZ : IEquatable<AsciiZ>, IComparable<AsciiZ>, IComparable {

  private readonly byte[] _packedBytes;
  private readonly int _length;

  private static readonly byte[] _emptyBytes = [];

  /// <summary>
  /// Gets an empty <see cref="AsciiZ"/> instance.
  /// </summary>
  public static AsciiZ Empty => new(_emptyBytes, 0);

  private AsciiZ(byte[] packedBytes, int length) {
    this._packedBytes = packedBytes;
    this._length = length;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="AsciiZ"/> struct from a byte array.
  /// </summary>
  /// <param name="value">The byte array. Content after the first NUL byte is discarded.</param>
  /// <exception cref="ArgumentException">
  /// Thrown when a byte value is greater than 127.
  /// </exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public AsciiZ(byte[]? value) : this(value, InvalidCharBehavior.Throw) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="AsciiZ"/> struct from a byte array with specified invalid char handling.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public AsciiZ(byte[]? value, InvalidCharBehavior behavior) {
    if (value == null || value.Length == 0) {
      this._packedBytes = _emptyBytes;
      this._length = 0;
    } else {
      var unpacked = _ProcessBytes(value.AsSpan(), behavior);
      this._length = unpacked.Length;
      this._packedBytes = this._length == 0 ? _emptyBytes : Ascii7BitPacking.Pack(unpacked);
    }
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="AsciiZ"/> struct from a byte span.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public AsciiZ(ReadOnlySpan<byte> value) : this(value, InvalidCharBehavior.Throw) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="AsciiZ"/> struct from a byte span with specified invalid char handling.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public AsciiZ(ReadOnlySpan<byte> value, InvalidCharBehavior behavior) {
    if (value.IsEmpty) {
      this._packedBytes = _emptyBytes;
      this._length = 0;
    } else {
      var unpacked = _ProcessBytes(value, behavior);
      this._length = unpacked.Length;
      this._packedBytes = this._length == 0 ? _emptyBytes : Ascii7BitPacking.Pack(unpacked);
    }
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="AsciiZ"/> struct from a string.
  /// </summary>
  /// <param name="value">The string value. Content after the first NUL character is discarded.</param>
  /// <exception cref="ArgumentException">
  /// Thrown when a character value is greater than 127.
  /// </exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public AsciiZ(string? value) : this(value, InvalidCharBehavior.Throw) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="AsciiZ"/> struct from a string with specified invalid char handling.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public AsciiZ(string? value, InvalidCharBehavior behavior) {
    if (string.IsNullOrEmpty(value)) {
      this._packedBytes = _emptyBytes;
      this._length = 0;
    } else {
      var unpacked = _ProcessString(value.AsSpan(), behavior);
      this._length = unpacked.Length;
      this._packedBytes = this._length == 0 ? _emptyBytes : Ascii7BitPacking.Pack(unpacked);
    }
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="AsciiZ"/> struct from a character span.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public AsciiZ(ReadOnlySpan<char> value) : this(value, InvalidCharBehavior.Throw) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="AsciiZ"/> struct from a character span with specified invalid char handling.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public AsciiZ(ReadOnlySpan<char> value, InvalidCharBehavior behavior) {
    if (value.IsEmpty) {
      this._packedBytes = _emptyBytes;
      this._length = 0;
    } else {
      var unpacked = _ProcessString(value, behavior);
      this._length = unpacked.Length;
      this._packedBytes = this._length == 0 ? _emptyBytes : Ascii7BitPacking.Pack(unpacked);
    }
  }

  private static byte[] _ProcessBytes(ReadOnlySpan<byte> value, InvalidCharBehavior behavior) {
    if (value.IsEmpty)
      return _emptyBytes;

    var nullIndex = value.IndexOf((byte)0);
    var length = nullIndex < 0 ? value.Length : nullIndex;

    if (behavior == InvalidCharBehavior.Throw) {
      // Use vectorized validation for fast-path check
      var slice = value.Slice(0, length);
      if (Ascii7BitPacking.IsValidAscii(slice))
        return slice.ToArray();

      // Slow path: find exact position for error message
      for (var i = 0; i < length; ++i)
        if (value[i] > 127)
          AlwaysThrow.ArgumentException(nameof(value), $"Invalid ASCII character at position {i}: byte value {value[i]} exceeds 127");
      return slice.ToArray();
    }

    if (behavior == InvalidCharBehavior.Replace) {
      var result = new byte[length];
      for (var i = 0; i < length; ++i)
        result[i] = value[i] > 127 ? (byte)'?' : value[i];
      return result;
    }

    // Skip behavior
    var validCount = 0;
    for (var i = 0; i < length; ++i)
      if (value[i] <= 127)
        ++validCount;

    if (validCount == length)
      return value.Slice(0, length).ToArray();

    var skipResult = new byte[validCount];
    var j = 0;
    for (var i = 0; i < length; ++i)
      if (value[i] <= 127)
        skipResult[j++] = value[i];
    return skipResult;
  }

  private static byte[] _ProcessString(ReadOnlySpan<char> value, InvalidCharBehavior behavior) {
    if (value.IsEmpty)
      return _emptyBytes;

    var nullIndex = value.IndexOf('\0');
    var length = nullIndex < 0 ? value.Length : nullIndex;

    if (behavior == InvalidCharBehavior.Throw) {
      var result = new byte[length];
      for (var i = 0; i < length; ++i) {
        var c = value[i];
        if (c > 127)
          AlwaysThrow.ArgumentException(nameof(value), $"Invalid ASCII character at position {i}: '{c}' (U+{(int)c:X4}) exceeds 127");
        result[i] = (byte)c;
      }
      return result;
    }

    if (behavior == InvalidCharBehavior.Replace) {
      var result = new byte[length];
      for (var i = 0; i < length; ++i)
        result[i] = value[i] > 127 ? (byte)'?' : (byte)value[i];
      return result;
    }

    // Skip behavior
    var validCount = 0;
    for (var i = 0; i < length; ++i)
      if (value[i] <= 127)
        ++validCount;

    if (validCount == length) {
      var fullResult = new byte[length];
      for (var i = 0; i < length; ++i)
        fullResult[i] = (byte)value[i];
      return fullResult;
    }

    var skipResult = new byte[validCount];
    var j = 0;
    for (var i = 0; i < length; ++i)
      if (value[i] <= 127)
        skipResult[j++] = (byte)value[i];
    return skipResult;
  }

  internal byte[] PackedBytes {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._packedBytes ?? _emptyBytes;
  }

  /// <summary>
  /// Gets the number of characters in the string.
  /// </summary>
  public int Length {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._length;
  }

  /// <summary>
  /// Gets a value indicating whether this string is empty.
  /// </summary>
  public bool IsEmpty {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._length == 0;
  }

  /// <summary>
  /// Gets the byte at the specified position.
  /// </summary>
  public byte this[int index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if ((uint)index >= (uint)this._length)
        AlwaysThrow.IndexOutOfRangeException();
      return Ascii7BitPacking.GetCharAt(this.PackedBytes, index);
    }
  }

  /// <summary>
  /// Gets the byte at the specified position using an <see cref="Index"/>.
  /// </summary>
  public byte this[Index index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var offset = index.GetOffset(this._length);
      if ((uint)offset >= (uint)this._length)
        AlwaysThrow.IndexOutOfRangeException();
      return Ascii7BitPacking.GetCharAt(this.PackedBytes, offset);
    }
  }

  /// <summary>
  /// Gets a substring specified by a <see cref="Range"/>.
  /// </summary>
  public AsciiZ this[Range range] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var (start, length) = range.GetOffsetAndLength(this._length);
      if (length == 0)
        return Empty;
      var unpacked = new byte[length];
      for (var i = 0; i < length; ++i)
        unpacked[i] = Ascii7BitPacking.GetCharAt(this.PackedBytes, start + i);
      return new(Ascii7BitPacking.Pack(unpacked), length);
    }
  }

  /// <summary>
  /// Retrieves a substring starting at the specified position.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public AsciiZ Substring(int startIndex) {
    if ((uint)startIndex > (uint)this._length)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(startIndex));
    var length = this._length - startIndex;
    if (length == 0)
      return Empty;
    var unpacked = new byte[length];
    for (var i = 0; i < length; ++i)
      unpacked[i] = Ascii7BitPacking.GetCharAt(this.PackedBytes, startIndex + i);
    return new(Ascii7BitPacking.Pack(unpacked), length);
  }

  /// <summary>
  /// Retrieves a substring starting at the specified position with the specified length.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public AsciiZ Substring(int startIndex, int length) {
    if ((uint)startIndex > (uint)this._length)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(startIndex));
    if ((uint)length > (uint)(this._length - startIndex))
      AlwaysThrow.ArgumentOutOfRangeException(nameof(length));
    if (length == 0)
      return Empty;
    var unpacked = new byte[length];
    for (var i = 0; i < length; ++i)
      unpacked[i] = Ascii7BitPacking.GetCharAt(this.PackedBytes, startIndex + i);
    return new(Ascii7BitPacking.Pack(unpacked), length);
  }

  /// <summary>
  /// Returns a <see cref="ReadOnlySpan{T}"/> over the 7-bit packed bytes.
  /// </summary>
  /// <remarks>
  /// This returns the internal packed representation directly without allocation.
  /// Use <see cref="ToArray()"/> if you need unpacked byte-per-character data.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ReadOnlySpan<byte> AsSpan() => this.PackedBytes.AsSpan();

  /// <summary>
  /// Returns a reference to the first byte of the packed data for use with the fixed statement.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ref readonly byte GetPinnableReference() {
    var packed = this.PackedBytes;
    return ref (packed.Length > 0 ? ref packed[0] : ref _NullRef());
  }

  private static ref readonly byte _NullRef() {
    unsafe {
      return ref Unsafe.AsRef<byte>(null);
    }
  }

  /// <summary>
  /// Returns a null-terminated byte array (unpacked) for P/Invoke.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public byte[] ToNullTerminatedArray() {
    var result = new byte[this._length + 1];
    if (this._length > 0) {
      var unpacked = Ascii7BitPacking.Unpack(this.PackedBytes, this._length);
      Array.Copy(unpacked, result, this._length);
    }
    result[this._length] = 0;
    return result;
  }

  /// <summary>
  /// Returns the unpacked byte array (one byte per character).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public byte[] ToArray() => this._length == 0 ? [] : Ascii7BitPacking.Unpack(this.PackedBytes, this._length);

  /// <summary>
  /// Returns the string representation using ASCII encoding.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override string ToString() {
    if (this._length == 0)
      return string.Empty;
    var unpacked = Ascii7BitPacking.Unpack(this.PackedBytes, this._length);
    var chars = new char[this._length];
    for (var i = 0; i < this._length; ++i)
      chars[i] = (char)unpacked[i];
    return new(chars);
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() {
    if (this._length == 0)
      return 0;
    unchecked {
      var hash = 17;
      for (var i = 0; i < this._length; ++i)
        hash = hash * 31 + Ascii7BitPacking.GetCharAt(this.PackedBytes, i);
      return hash;
    }
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override bool Equals(object? obj) => obj is AsciiZ other && this.Equals(other);

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(AsciiZ other) {
    if (this._length != other._length)
      return false;
    for (var i = 0; i < this._length; ++i)
      if (Ascii7BitPacking.GetCharAt(this.PackedBytes, i) != Ascii7BitPacking.GetCharAt(other.PackedBytes, i))
        return false;
    return true;
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(AsciiZ other) {
    var minLength = this._length < other._length ? this._length : other._length;
    for (var i = 0; i < minLength; ++i) {
      var cmp = Ascii7BitPacking.GetCharAt(this.PackedBytes, i).CompareTo(Ascii7BitPacking.GetCharAt(other.PackedBytes, i));
      if (cmp != 0)
        return cmp;
    }
    return this._length.CompareTo(other._length);
  }

  /// <inheritdoc />
  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not AsciiZ other)
      throw new ArgumentException("Object must be of type AsciiZ.", nameof(obj));
    return this.CompareTo(other);
  }

  /// <summary>
  /// Determines whether two <see cref="AsciiZ"/> instances are equal.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator ==(AsciiZ left, AsciiZ right) => left.Equals(right);

  /// <summary>
  /// Determines whether two <see cref="AsciiZ"/> instances are not equal.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator !=(AsciiZ left, AsciiZ right) => !left.Equals(right);

  /// <summary>
  /// Determines whether the left <see cref="AsciiZ"/> is less than the right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator <(AsciiZ left, AsciiZ right) => left.CompareTo(right) < 0;

  /// <summary>
  /// Determines whether the left <see cref="AsciiZ"/> is greater than the right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator >(AsciiZ left, AsciiZ right) => left.CompareTo(right) > 0;

  /// <summary>
  /// Determines whether the left <see cref="AsciiZ"/> is less than or equal to the right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator <=(AsciiZ left, AsciiZ right) => left.CompareTo(right) <= 0;

  /// <summary>
  /// Determines whether the left <see cref="AsciiZ"/> is greater than or equal to the right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator >=(AsciiZ left, AsciiZ right) => left.CompareTo(right) >= 0;

  /// <summary>
  /// Implicitly converts a <see cref="string"/> to an <see cref="AsciiZ"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator AsciiZ(string? value) => new(value);

  /// <summary>
  /// Implicitly converts an <see cref="AsciiZ"/> to a <see cref="string"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator string(AsciiZ value) => value.ToString();

  /// <summary>
  /// Implicitly converts an <see cref="AsciiZ"/> to an <see cref="AsciiString"/>.
  /// Safe because AsciiZ content has no embedded nulls.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator AsciiString(AsciiZ value) => AsciiString.FromPackedBytes(value._packedBytes, value._length);

  /// <summary>
  /// Explicitly converts an <see cref="AsciiString"/> to an <see cref="AsciiZ"/>.
  /// May truncate at the first embedded null character.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static explicit operator AsciiZ(AsciiString value) => FromAsciiString(value);

  /// <summary>
  /// Explicitly converts a <see cref="FixedAscii"/> to an <see cref="AsciiZ"/>.
  /// May truncate at the first embedded null character.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static explicit operator AsciiZ(FixedAscii value) => FromFixedAscii(value);

  /// <summary>
  /// Creates an <see cref="AsciiZ"/> from an <see cref="AsciiString"/>, truncating at the first null.
  /// </summary>
  private static AsciiZ FromAsciiString(AsciiString value) {
    if (value.IsEmpty)
      return Empty;

    // Scan for first null
    var length = value.Length;
    for (var i = 0; i < length; ++i)
      if (Ascii7BitPacking.GetCharAt(value.PackedBytes, i) == 0) {
        length = i;
        break;
      }

    if (length == 0)
      return Empty;

    if (length == value.Length)
      return new(value.PackedBytes, length);

    // Need to repack with truncated length
    var unpacked = new byte[length];
    for (var i = 0; i < length; ++i)
      unpacked[i] = Ascii7BitPacking.GetCharAt(value.PackedBytes, i);
    return new(Ascii7BitPacking.Pack(unpacked), length);
  }

  /// <summary>
  /// Creates an <see cref="AsciiZ"/> from a <see cref="FixedAscii"/>, truncating at the first null.
  /// </summary>
  private static AsciiZ FromFixedAscii(FixedAscii value) {
    if (value.IsEmpty)
      return Empty;

    // Scan for first null
    var length = value.Length;
    for (var i = 0; i < length; ++i)
      if (Ascii7BitPacking.GetCharAt(value.PackedBytes, i) == 0) {
        length = i;
        break;
      }

    if (length == 0)
      return Empty;

    if (length == value.Length)
      return new(value.PackedBytes, length);

    // Need to repack with truncated length
    var unpacked = new byte[length];
    for (var i = 0; i < length; ++i)
      unpacked[i] = Ascii7BitPacking.GetCharAt(value.PackedBytes, i);
    return new(Ascii7BitPacking.Pack(unpacked), length);
  }

  /// <summary>
  /// Concatenates two <see cref="AsciiZ"/> instances.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static AsciiZ operator +(AsciiZ left, AsciiZ right) {
    var totalLength = left._length + right._length;
    if (totalLength == 0)
      return Empty;
    var unpacked = new byte[totalLength];
    for (var i = 0; i < left._length; ++i)
      unpacked[i] = Ascii7BitPacking.GetCharAt(left.PackedBytes, i);
    for (var i = 0; i < right._length; ++i)
      unpacked[left._length + i] = Ascii7BitPacking.GetCharAt(right.PackedBytes, i);
    return new(Ascii7BitPacking.Pack(unpacked), totalLength);
  }

  /// <summary>
  /// Implicitly converts an <see cref="AsciiZ"/> to an <see cref="AnsiZ"/>.
  /// Safe because ASCII is a subset of ANSI (Windows-1252).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator AnsiZ(AsciiZ value) {
    if (value.IsEmpty)
      return AnsiZ.Empty;
    return AnsiZ.FromBytes(Ascii7BitPacking.Unpack(value.PackedBytes, value._length));
  }

  /// <summary>
  /// Implicitly converts an <see cref="AsciiZ"/> to an <see cref="AnsiString"/>.
  /// Safe because ASCII is a subset of ANSI (Windows-1252).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator AnsiString(AsciiZ value) {
    if (value.IsEmpty)
      return AnsiString.Empty;
    return AnsiString.FromBytes(Ascii7BitPacking.Unpack(value.PackedBytes, value._length));
  }

}
