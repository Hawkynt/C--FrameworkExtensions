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
/// Represents an immutable fixed-capacity Windows-1252 (ANSI) encoded string stored as bytes.
/// </summary>
/// <remarks>
/// The capacity is specified at construction time. Content is truncated if it exceeds capacity.
/// Uses Windows-1252 (code page 1252) encoding, which is a superset of ISO-8859-1 (Latin-1).
/// Characters that cannot be represented in Windows-1252 are replaced with '?' (0x3F).
/// </remarks>
public readonly struct FixedAnsi : IEquatable<FixedAnsi>, IComparable<FixedAnsi>, IComparable {

  private readonly byte[] _bytes;
  private readonly int _length;

  /// <summary>
  /// Initializes a new instance of the <see cref="FixedAnsi"/> struct with the specified capacity.
  /// </summary>
  /// <param name="capacity">The maximum number of bytes this string can hold.</param>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="capacity"/> is less than zero.
  /// </exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FixedAnsi(int capacity) : this(capacity, string.Empty) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="FixedAnsi"/> struct with the specified capacity and initial value.
  /// </summary>
  /// <param name="capacity">The maximum number of bytes this string can hold.</param>
  /// <param name="value">The initial string value. Truncated if it exceeds capacity.</param>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="capacity"/> is less than zero.
  /// </exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FixedAnsi(int capacity, string? value) {
    if (capacity < 0)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(capacity), "Capacity must be non-negative");

    this._bytes = new byte[capacity];
    if (string.IsNullOrEmpty(value)) {
      this._length = 0;
    } else {
      var encoded = Windows1252Encoding.GetBytes(value!.AsSpan());
      this._length = encoded.Length > capacity ? capacity : encoded.Length;
      Array.Copy(encoded, this._bytes, this._length);
    }
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="FixedAnsi"/> struct with the specified capacity and initial value.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FixedAnsi(int capacity, ReadOnlySpan<char> value) {
    if (capacity < 0)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(capacity), "Capacity must be non-negative");

    this._bytes = new byte[capacity];
    if (value.IsEmpty) {
      this._length = 0;
    } else {
      var encoded = Windows1252Encoding.GetBytes(value);
      this._length = encoded.Length > capacity ? capacity : encoded.Length;
      Array.Copy(encoded, this._bytes, this._length);
    }
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="FixedAnsi"/> struct with the specified capacity and byte data.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FixedAnsi(int capacity, byte[]? value) {
    if (capacity < 0)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(capacity), "Capacity must be non-negative");

    this._bytes = new byte[capacity];
    if (value == null || value.Length == 0) {
      this._length = 0;
    } else {
      this._length = value.Length > capacity ? capacity : value.Length;
      Array.Copy(value, this._bytes, this._length);
    }
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="FixedAnsi"/> struct with the specified capacity and byte span.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FixedAnsi(int capacity, ReadOnlySpan<byte> value) {
    if (capacity < 0)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(capacity), "Capacity must be non-negative");

    this._bytes = new byte[capacity];
    if (value.IsEmpty) {
      this._length = 0;
    } else {
      this._length = value.Length > capacity ? capacity : value.Length;
      value.Slice(0, this._length).CopyTo(this._bytes.AsSpan());
    }
  }

  private FixedAnsi(byte[] bytes, int length) {
    this._bytes = bytes;
    this._length = length;
  }

  private static readonly byte[] _emptyBytes = [];

  internal byte[] Bytes {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._bytes ?? _emptyBytes;
  }

  /// <summary>
  /// Gets the maximum number of bytes this string can hold.
  /// </summary>
  public int Capacity {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._bytes?.Length ?? 0;
  }

  /// <summary>
  /// Gets the actual number of bytes in the string.
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
      return this._bytes[index];
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
      return this._bytes[offset];
    }
  }

  /// <summary>
  /// Gets a substring specified by a <see cref="Range"/>.
  /// </summary>
  public FixedAnsi this[Range range] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var (start, length) = range.GetOffsetAndLength(this._length);
      var result = new byte[this._bytes.Length];
      Array.Copy(this._bytes, start, result, 0, length);
      return new(result, length);
    }
  }

  /// <summary>
  /// Retrieves a substring starting at the specified position.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FixedAnsi Substring(int startIndex) {
    if ((uint)startIndex > (uint)this._length)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(startIndex));
    var length = this._length - startIndex;
    var result = new byte[this._bytes.Length];
    Array.Copy(this._bytes, startIndex, result, 0, length);
    return new(result, length);
  }

  /// <summary>
  /// Retrieves a substring starting at the specified position with the specified length.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FixedAnsi Substring(int startIndex, int length) {
    if ((uint)startIndex > (uint)this._length)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(startIndex));
    if ((uint)length > (uint)(this._length - startIndex))
      AlwaysThrow.ArgumentOutOfRangeException(nameof(length));
    var result = new byte[this._bytes.Length];
    Array.Copy(this._bytes, startIndex, result, 0, length);
    return new(result, length);
  }

  /// <summary>
  /// Returns a new <see cref="FixedAnsi"/> padded on the right to the full capacity.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FixedAnsi PadRight(byte paddingByte = 0) {
    if (this._bytes == null || this._length == this._bytes.Length)
      return this;
    var result = new byte[this._bytes.Length];
    Array.Copy(this._bytes, result, this._length);
    for (var i = this._length; i < result.Length; ++i)
      result[i] = paddingByte;
    return new(result, result.Length);
  }

  /// <summary>
  /// Returns a new <see cref="FixedAnsi"/> padded on the left to the full capacity.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FixedAnsi PadLeft(byte paddingByte = (byte)' ') {
    if (this._bytes == null || this._length == this._bytes.Length)
      return this;
    var result = new byte[this._bytes.Length];
    var padding = this._bytes.Length - this._length;
    for (var i = 0; i < padding; ++i)
      result[i] = paddingByte;
    Array.Copy(this._bytes, 0, result, padding, this._length);
    return new(result, result.Length);
  }

  /// <summary>
  /// Returns a new <see cref="FixedAnsi"/> with trailing NUL bytes and whitespace removed.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FixedAnsi TrimEnd() {
    if (this._bytes == null || this._length == 0)
      return this;
    var newLength = this._length;
    while (newLength > 0 && (this._bytes[newLength - 1] == 0 || this._bytes[newLength - 1] == (byte)' '))
      --newLength;
    if (newLength == this._length)
      return this;
    var result = new byte[this._bytes.Length];
    Array.Copy(this._bytes, result, newLength);
    return new(result, newLength);
  }

  /// <summary>
  /// Returns a <see cref="ReadOnlySpan{T}"/> over the actual content of this string.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ReadOnlySpan<byte> AsSpan() => this._bytes == null ? ReadOnlySpan<byte>.Empty : this._bytes.AsSpan(0, this._length);

  /// <summary>
  /// Returns a <see cref="ReadOnlySpan{T}"/> over the full capacity buffer.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ReadOnlySpan<byte> AsFullSpan() => this._bytes == null ? ReadOnlySpan<byte>.Empty : this._bytes.AsSpan();

  /// <summary>
  /// Returns a reference to the first byte for use with the fixed statement.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ref readonly byte GetPinnableReference()
    => ref (this._bytes != null && this._bytes.Length > 0 ? ref this._bytes[0] : ref _NullRef());

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
    var result = new byte[this._length + 1];
    if (this._bytes != null && this._length > 0)
      Array.Copy(this._bytes, result, this._length);
    result[this._length] = 0;
    return result;
  }

  /// <summary>
  /// Returns the underlying byte array (copy).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public byte[] ToArray() {
    var result = new byte[this._length];
    if (this._bytes != null && this._length > 0)
      Array.Copy(this._bytes, result, this._length);
    return result;
  }

  /// <summary>
  /// Returns the string representation using Windows-1252 encoding (actual content, not padded to capacity).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override string ToString() {
    if (this._bytes == null || this._length == 0)
      return string.Empty;
    return Windows1252Encoding.GetString(this._bytes.AsSpan(0, this._length));
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() {
    if (this._bytes == null || this._length == 0)
      return 0;
    unchecked {
      var hash = 17;
      for (var i = 0; i < this._length; ++i)
        hash = hash * 31 + this._bytes[i];
      return hash;
    }
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override bool Equals(object? obj) => obj is FixedAnsi other && this.Equals(other);

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(FixedAnsi other) {
    if (this._length != other._length)
      return false;
    for (var i = 0; i < this._length; ++i)
      if (this._bytes[i] != other._bytes[i])
        return false;
    return true;
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(FixedAnsi other) {
    var minLength = this._length < other._length ? this._length : other._length;
    for (var i = 0; i < minLength; ++i) {
      var cmp = this._bytes[i].CompareTo(other._bytes[i]);
      if (cmp != 0)
        return cmp;
    }
    return this._length.CompareTo(other._length);
  }

  /// <inheritdoc />
  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not FixedAnsi other)
      throw new ArgumentException("Object must be of type FixedAnsi.", nameof(obj));
    return this.CompareTo(other);
  }

  /// <summary>
  /// Determines whether two <see cref="FixedAnsi"/> instances are equal (by content, not capacity).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator ==(FixedAnsi left, FixedAnsi right) => left.Equals(right);

  /// <summary>
  /// Determines whether two <see cref="FixedAnsi"/> instances are not equal.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator !=(FixedAnsi left, FixedAnsi right) => !left.Equals(right);

  /// <summary>
  /// Determines whether the left <see cref="FixedAnsi"/> is less than the right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator <(FixedAnsi left, FixedAnsi right) => left.CompareTo(right) < 0;

  /// <summary>
  /// Determines whether the left <see cref="FixedAnsi"/> is greater than the right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator >(FixedAnsi left, FixedAnsi right) => left.CompareTo(right) > 0;

  /// <summary>
  /// Determines whether the left <see cref="FixedAnsi"/> is less than or equal to the right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator <=(FixedAnsi left, FixedAnsi right) => left.CompareTo(right) <= 0;

  /// <summary>
  /// Determines whether the left <see cref="FixedAnsi"/> is greater than or equal to the right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator >=(FixedAnsi left, FixedAnsi right) => left.CompareTo(right) >= 0;

  /// <summary>
  /// Implicitly converts a <see cref="FixedAnsi"/> to a <see cref="string"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator string(FixedAnsi value) => value.ToString();

  /// <summary>
  /// Implicitly converts a <see cref="FixedAnsi"/> to an <see cref="AnsiString"/>.
  /// Safe because no data is lost.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator AnsiString(FixedAnsi value) {
    if (value.IsEmpty)
      return AnsiString.Empty;
    var bytes = new byte[value.Length];
    Array.Copy(value.Bytes, bytes, value.Length);
    return AnsiString.FromBytes(bytes);
  }

  /// <summary>
  /// Explicitly converts a <see cref="FixedAnsi"/> to an <see cref="AnsiZ"/>.
  /// May truncate at the first embedded null character.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static explicit operator AnsiZ(FixedAnsi value) => (AnsiZ)(AnsiString)value;

  /// <summary>
  /// Explicitly converts a <see cref="FixedAnsi"/> to an <see cref="AsciiString"/>.
  /// May throw if the value contains non-ASCII characters (bytes &gt; 127).
  /// </summary>
  /// <exception cref="ArgumentException">The value contains non-ASCII characters.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static explicit operator AsciiString(FixedAnsi value) => (AsciiString)(AnsiString)value;

}
