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
/// Represents an immutable fixed-capacity 7-bit ASCII string stored as bytes.
/// </summary>
/// <remarks>
/// The capacity is specified at construction time. Content is truncated if it exceeds capacity.
/// All characters must be in the range 0-127.
/// Characters are stored in 7-bit packed format, saving 12.5% memory (8 characters use 7 bytes instead of 8).
/// </remarks>
public readonly struct FixedAscii : IEquatable<FixedAscii>, IComparable<FixedAscii>, IComparable {

  private readonly byte[] _packedBytes;
  private readonly int _capacity;
  private readonly int _length;

  private static readonly byte[] _emptyBytes = [];

  /// <summary>
  /// Initializes a new instance of the <see cref="FixedAscii"/> struct with the specified capacity.
  /// </summary>
  /// <param name="capacity">The maximum number of bytes this string can hold.</param>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="capacity"/> is less than zero.
  /// </exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FixedAscii(int capacity) : this(capacity, string.Empty, InvalidCharBehavior.Throw) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="FixedAscii"/> struct with the specified capacity and initial value.
  /// </summary>
  /// <param name="capacity">The maximum number of bytes this string can hold.</param>
  /// <param name="value">The initial string value. Truncated if it exceeds capacity.</param>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="capacity"/> is less than zero.
  /// </exception>
  /// <exception cref="ArgumentException">
  /// Thrown when a character value is greater than 127 and behavior is <see cref="InvalidCharBehavior.Throw"/>.
  /// </exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FixedAscii(int capacity, string? value) : this(capacity, value, InvalidCharBehavior.Throw) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="FixedAscii"/> struct with the specified capacity, initial value, and invalid char handling.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FixedAscii(int capacity, string? value, InvalidCharBehavior behavior) {
    if (capacity < 0)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(capacity), "Capacity must be non-negative");

    this._capacity = capacity;
    if (capacity == 0) {
      this._packedBytes = _emptyBytes;
      this._length = 0;
    } else {
      this._packedBytes = new byte[Ascii7BitPacking.GetPackedByteCount(capacity)];
      if (string.IsNullOrEmpty(value))
        this._length = 0;
      else
        this._length = _ProcessString(value.AsSpan(), this._packedBytes, capacity, behavior);
    }
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="FixedAscii"/> struct with the specified capacity and initial value.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FixedAscii(int capacity, ReadOnlySpan<char> value) : this(capacity, value, InvalidCharBehavior.Throw) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="FixedAscii"/> struct with the specified capacity, initial value, and invalid char handling.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FixedAscii(int capacity, ReadOnlySpan<char> value, InvalidCharBehavior behavior) {
    if (capacity < 0)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(capacity), "Capacity must be non-negative");

    this._capacity = capacity;
    if (capacity == 0) {
      this._packedBytes = _emptyBytes;
      this._length = 0;
    } else {
      this._packedBytes = new byte[Ascii7BitPacking.GetPackedByteCount(capacity)];
      if (value.IsEmpty)
        this._length = 0;
      else
        this._length = _ProcessString(value, this._packedBytes, capacity, behavior);
    }
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="FixedAscii"/> struct with the specified capacity and byte data.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FixedAscii(int capacity, byte[]? value) : this(capacity, value, InvalidCharBehavior.Throw) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="FixedAscii"/> struct with the specified capacity, byte data, and invalid char handling.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FixedAscii(int capacity, byte[]? value, InvalidCharBehavior behavior) {
    if (capacity < 0)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(capacity), "Capacity must be non-negative");

    this._capacity = capacity;
    if (capacity == 0) {
      this._packedBytes = _emptyBytes;
      this._length = 0;
    } else {
      this._packedBytes = new byte[Ascii7BitPacking.GetPackedByteCount(capacity)];
      if (value == null || value.Length == 0)
        this._length = 0;
      else
        this._length = _ProcessBytes(value.AsSpan(), this._packedBytes, capacity, behavior);
    }
  }

  private FixedAscii(byte[] packedBytes, int capacity, int length) {
    this._packedBytes = packedBytes;
    this._capacity = capacity;
    this._length = length;
  }

  private static int _ProcessString(ReadOnlySpan<char> value, byte[] packedTarget, int capacity, InvalidCharBehavior behavior) {
    var maxLength = value.Length > capacity ? capacity : value.Length;

    if (behavior == InvalidCharBehavior.Throw) {
      for (var i = 0; i < maxLength; ++i) {
        var c = value[i];
        if (c > 127)
          AlwaysThrow.ArgumentException(nameof(value), $"Invalid ASCII character at position {i}: '{c}' (U+{(int)c:X4}) exceeds 127");
        Ascii7BitPacking.SetCharAt(packedTarget, i, (byte)c);
      }
      return maxLength;
    }

    if (behavior == InvalidCharBehavior.Replace) {
      for (var i = 0; i < maxLength; ++i)
        Ascii7BitPacking.SetCharAt(packedTarget, i, value[i] > 127 ? (byte)'?' : (byte)value[i]);
      return maxLength;
    }

    // Skip behavior
    var j = 0;
    for (var i = 0; i < value.Length && j < capacity; ++i)
      if (value[i] <= 127)
        Ascii7BitPacking.SetCharAt(packedTarget, j++, (byte)value[i]);
    return j;
  }

  private static int _ProcessBytes(ReadOnlySpan<byte> value, byte[] packedTarget, int capacity, InvalidCharBehavior behavior) {
    var maxLength = value.Length > capacity ? capacity : value.Length;

    if (behavior == InvalidCharBehavior.Throw) {
      // Use vectorized validation for fast-path check
      var slice = value.Slice(0, maxLength);
      if (Ascii7BitPacking.IsValidAscii(slice)) {
        Ascii7BitPacking.PackInto(slice, packedTarget, maxLength);
        return maxLength;
      }

      // Slow path: find exact position for error message
      for (var i = 0; i < maxLength; ++i)
        if (value[i] > 127)
          AlwaysThrow.ArgumentException(nameof(value), $"Invalid ASCII character at position {i}: byte value {value[i]} exceeds 127");

      Ascii7BitPacking.PackInto(slice, packedTarget, maxLength);
      return maxLength;
    }

    if (behavior == InvalidCharBehavior.Replace) {
      for (var i = 0; i < maxLength; ++i)
        Ascii7BitPacking.SetCharAt(packedTarget, i, value[i] > 127 ? (byte)'?' : value[i]);
      return maxLength;
    }

    // Skip behavior
    var j = 0;
    for (var i = 0; i < value.Length && j < capacity; ++i)
      if (value[i] <= 127)
        Ascii7BitPacking.SetCharAt(packedTarget, j++, value[i]);
    return j;
  }

  internal byte[] PackedBytes {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._packedBytes ?? _emptyBytes;
  }

  /// <summary>
  /// Gets the maximum number of characters this string can hold.
  /// </summary>
  public int Capacity {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._capacity;
  }

  /// <summary>
  /// Gets the actual number of characters in the string.
  /// </summary>
  public int Length {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._length;
  }

  /// <summary>
  /// Gets a value indicating whether this string is empty (has zero length).
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
  public FixedAscii this[Range range] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var (start, length) = range.GetOffsetAndLength(this._length);
      if (length == 0)
        return new(this._capacity);
      var packedSize = Ascii7BitPacking.GetPackedByteCount(this._capacity);
      var result = new byte[packedSize];
      for (var i = 0; i < length; ++i)
        Ascii7BitPacking.SetCharAt(result, i, Ascii7BitPacking.GetCharAt(this.PackedBytes, start + i));
      return new(result, this._capacity, length);
    }
  }

  /// <summary>
  /// Retrieves a substring starting at the specified position.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FixedAscii Substring(int startIndex) {
    if ((uint)startIndex > (uint)this._length)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(startIndex));
    var length = this._length - startIndex;
    if (length == 0)
      return new(this._capacity);
    var packedSize = Ascii7BitPacking.GetPackedByteCount(this._capacity);
    var result = new byte[packedSize];
    for (var i = 0; i < length; ++i)
      Ascii7BitPacking.SetCharAt(result, i, Ascii7BitPacking.GetCharAt(this.PackedBytes, startIndex + i));
    return new(result, this._capacity, length);
  }

  /// <summary>
  /// Retrieves a substring starting at the specified position with the specified length.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FixedAscii Substring(int startIndex, int length) {
    if ((uint)startIndex > (uint)this._length)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(startIndex));
    if ((uint)length > (uint)(this._length - startIndex))
      AlwaysThrow.ArgumentOutOfRangeException(nameof(length));
    if (length == 0)
      return new(this._capacity);
    var packedSize = Ascii7BitPacking.GetPackedByteCount(this._capacity);
    var result = new byte[packedSize];
    for (var i = 0; i < length; ++i)
      Ascii7BitPacking.SetCharAt(result, i, Ascii7BitPacking.GetCharAt(this.PackedBytes, startIndex + i));
    return new(result, this._capacity, length);
  }

  /// <summary>
  /// Returns a new <see cref="FixedAscii"/> padded on the right to the full capacity.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FixedAscii PadRight(byte paddingByte = 0) {
    if (this._capacity == 0 || this._length == this._capacity)
      return this;
    var packedSize = Ascii7BitPacking.GetPackedByteCount(this._capacity);
    var result = new byte[packedSize];
    for (var i = 0; i < this._length; ++i)
      Ascii7BitPacking.SetCharAt(result, i, Ascii7BitPacking.GetCharAt(this.PackedBytes, i));
    for (var i = this._length; i < this._capacity; ++i)
      Ascii7BitPacking.SetCharAt(result, i, paddingByte);
    return new(result, this._capacity, this._capacity);
  }

  /// <summary>
  /// Returns a new <see cref="FixedAscii"/> padded on the left to the full capacity.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FixedAscii PadLeft(byte paddingByte = (byte)' ') {
    if (this._capacity == 0 || this._length == this._capacity)
      return this;
    var packedSize = Ascii7BitPacking.GetPackedByteCount(this._capacity);
    var result = new byte[packedSize];
    var padding = this._capacity - this._length;
    for (var i = 0; i < padding; ++i)
      Ascii7BitPacking.SetCharAt(result, i, paddingByte);
    for (var i = 0; i < this._length; ++i)
      Ascii7BitPacking.SetCharAt(result, padding + i, Ascii7BitPacking.GetCharAt(this.PackedBytes, i));
    return new(result, this._capacity, this._capacity);
  }

  /// <summary>
  /// Returns a new <see cref="FixedAscii"/> with trailing NUL bytes and whitespace removed.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FixedAscii TrimEnd() {
    if (this._capacity == 0 || this._length == 0)
      return this;
    var newLength = this._length;
    while (newLength > 0) {
      var c = Ascii7BitPacking.GetCharAt(this.PackedBytes, newLength - 1);
      if (c != 0 && c != (byte)' ')
        break;
      --newLength;
    }
    if (newLength == this._length)
      return this;
    var packedSize = Ascii7BitPacking.GetPackedByteCount(this._capacity);
    var result = new byte[packedSize];
    for (var i = 0; i < newLength; ++i)
      Ascii7BitPacking.SetCharAt(result, i, Ascii7BitPacking.GetCharAt(this.PackedBytes, i));
    return new(result, this._capacity, newLength);
  }

  /// <summary>
  /// Returns a <see cref="ReadOnlySpan{T}"/> over the 7-bit packed bytes.
  /// </summary>
  /// <remarks>
  /// This returns the internal packed representation directly without allocation.
  /// Use <see cref="ToArray()"/> if you need unpacked byte-per-character data.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ReadOnlySpan<byte> AsSpan() => this.PackedBytes.AsSpan(0, Ascii7BitPacking.GetPackedByteCount(this._length));

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
  /// Returns the string representation (actual content, not padded to capacity).
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
  public override bool Equals(object? obj) => obj is FixedAscii other && this.Equals(other);

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(FixedAscii other) {
    if (this._length != other._length)
      return false;
    for (var i = 0; i < this._length; ++i)
      if (Ascii7BitPacking.GetCharAt(this.PackedBytes, i) != Ascii7BitPacking.GetCharAt(other.PackedBytes, i))
        return false;
    return true;
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(FixedAscii other) {
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
    if (obj is not FixedAscii other)
      throw new ArgumentException("Object must be of type FixedAscii.", nameof(obj));
    return this.CompareTo(other);
  }

  /// <summary>
  /// Determines whether two <see cref="FixedAscii"/> instances are equal (by content, not capacity).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator ==(FixedAscii left, FixedAscii right) => left.Equals(right);

  /// <summary>
  /// Determines whether two <see cref="FixedAscii"/> instances are not equal.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator !=(FixedAscii left, FixedAscii right) => !left.Equals(right);

  /// <summary>
  /// Determines whether the left <see cref="FixedAscii"/> is less than the right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator <(FixedAscii left, FixedAscii right) => left.CompareTo(right) < 0;

  /// <summary>
  /// Determines whether the left <see cref="FixedAscii"/> is greater than the right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator >(FixedAscii left, FixedAscii right) => left.CompareTo(right) > 0;

  /// <summary>
  /// Determines whether the left <see cref="FixedAscii"/> is less than or equal to the right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator <=(FixedAscii left, FixedAscii right) => left.CompareTo(right) <= 0;

  /// <summary>
  /// Determines whether the left <see cref="FixedAscii"/> is greater than or equal to the right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator >=(FixedAscii left, FixedAscii right) => left.CompareTo(right) >= 0;

  /// <summary>
  /// Implicitly converts a <see cref="FixedAscii"/> to a <see cref="string"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator string(FixedAscii value) => value.ToString();

  /// <summary>
  /// Implicitly converts a <see cref="FixedAscii"/> to an <see cref="AsciiString"/>.
  /// Safe because no data is lost.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator AsciiString(FixedAscii value) => AsciiString.FromPackedBytes(value.PackedBytes, value.Length);

  /// <summary>
  /// Explicitly converts a <see cref="FixedAscii"/> to an <see cref="AsciiZ"/>.
  /// May truncate at the first embedded null character.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static explicit operator AsciiZ(FixedAscii value) => (AsciiZ)(AsciiString)value;

  /// <summary>
  /// Implicitly converts a <see cref="FixedAscii"/> to an <see cref="AnsiString"/>.
  /// Safe because ASCII is a subset of ANSI (Windows-1252).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator AnsiString(FixedAscii value) {
    if (value.IsEmpty)
      return AnsiString.Empty;
    return AnsiString.FromBytes(Ascii7BitPacking.Unpack(value.PackedBytes, value.Length));
  }

}
