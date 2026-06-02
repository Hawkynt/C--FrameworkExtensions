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
/// Represents an immutable fixed-capacity UTF-16 string.
/// </summary>
/// <remarks>
/// The capacity is specified at construction time. Content is truncated if it exceeds capacity.
/// Useful for interop scenarios requiring fixed-length character buffers.
/// </remarks>
public readonly struct FixedString : IEquatable<FixedString>, IComparable<FixedString>, IComparable {

  private readonly char[] _chars;
  private readonly int _length;

  /// <summary>
  /// Initializes a new instance of the <see cref="FixedString"/> struct with the specified capacity.
  /// </summary>
  /// <param name="capacity">The maximum number of characters this string can hold.</param>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="capacity"/> is less than zero.
  /// </exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FixedString(int capacity) : this(capacity, string.Empty) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="FixedString"/> struct with the specified capacity and initial value.
  /// </summary>
  /// <param name="capacity">The maximum number of characters this string can hold.</param>
  /// <param name="value">The initial string value. Truncated if it exceeds capacity.</param>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="capacity"/> is less than zero.
  /// </exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FixedString(int capacity, string? value) {
    if (capacity < 0)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(capacity), "Capacity must be non-negative");

    this._chars = new char[capacity];
    if (string.IsNullOrEmpty(value)) {
      this._length = 0;
    } else {
      this._length = value!.Length > capacity ? capacity : value.Length;
      value.CopyTo(0, this._chars, 0, this._length);
    }
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="FixedString"/> struct with the specified capacity and initial value.
  /// </summary>
  /// <param name="capacity">The maximum number of characters this string can hold.</param>
  /// <param name="value">The initial character span. Truncated if it exceeds capacity.</param>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="capacity"/> is less than zero.
  /// </exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FixedString(int capacity, ReadOnlySpan<char> value) {
    if (capacity < 0)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(capacity), "Capacity must be non-negative");

    this._chars = new char[capacity];
    if (value.IsEmpty) {
      this._length = 0;
    } else {
      this._length = value.Length > capacity ? capacity : value.Length;
      value.Slice(0, this._length).CopyTo(this._chars.AsSpan());
    }
  }

  private FixedString(char[] chars, int length) {
    this._chars = chars;
    this._length = length;
  }

  /// <summary>
  /// Gets the maximum number of characters this string can hold.
  /// </summary>
  public int Capacity {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._chars?.Length ?? 0;
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
  /// Gets the character at the specified position.
  /// </summary>
  public char this[int index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if ((uint)index >= (uint)this._length)
        AlwaysThrow.IndexOutOfRangeException();
      return this._chars[index];
    }
  }

  /// <summary>
  /// Gets the character at the specified position using an <see cref="Index"/>.
  /// </summary>
  public char this[Index index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var offset = index.GetOffset(this._length);
      if ((uint)offset >= (uint)this._length)
        AlwaysThrow.IndexOutOfRangeException();
      return this._chars[offset];
    }
  }

  /// <summary>
  /// Gets a substring specified by a <see cref="Range"/>.
  /// </summary>
  /// <remarks>
  /// The returned substring has the same capacity as the original, but contains only the specified range.
  /// </remarks>
  public FixedString this[Range range] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var (start, length) = range.GetOffsetAndLength(this._length);
      var result = new char[this._chars.Length];
      Array.Copy(this._chars, start, result, 0, length);
      return new(result, length);
    }
  }

  /// <summary>
  /// Retrieves a substring starting at the specified position.
  /// </summary>
  /// <remarks>
  /// The returned substring has the same capacity as the original.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FixedString Substring(int startIndex) {
    if ((uint)startIndex > (uint)this._length)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(startIndex));
    var length = this._length - startIndex;
    var result = new char[this._chars.Length];
    Array.Copy(this._chars, startIndex, result, 0, length);
    return new(result, length);
  }

  /// <summary>
  /// Retrieves a substring starting at the specified position with the specified length.
  /// </summary>
  /// <remarks>
  /// The returned substring has the same capacity as the original.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FixedString Substring(int startIndex, int length) {
    if ((uint)startIndex > (uint)this._length)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(startIndex));
    if ((uint)length > (uint)(this._length - startIndex))
      AlwaysThrow.ArgumentOutOfRangeException(nameof(length));
    var result = new char[this._chars.Length];
    Array.Copy(this._chars, startIndex, result, 0, length);
    return new(result, length);
  }

  /// <summary>
  /// Returns a new <see cref="FixedString"/> padded on the right to the full capacity.
  /// </summary>
  /// <param name="paddingChar">The character to use for padding. Defaults to NUL.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FixedString PadRight(char paddingChar = '\0') {
    if (this._chars == null || this._length == this._chars.Length)
      return this;
    var result = new char[this._chars.Length];
    Array.Copy(this._chars, result, this._length);
    for (var i = this._length; i < result.Length; ++i)
      result[i] = paddingChar;
    return new(result, result.Length);
  }

  /// <summary>
  /// Returns a new <see cref="FixedString"/> padded on the left to the full capacity.
  /// </summary>
  /// <param name="paddingChar">The character to use for padding. Defaults to space.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FixedString PadLeft(char paddingChar = ' ') {
    if (this._chars == null || this._length == this._chars.Length)
      return this;
    var result = new char[this._chars.Length];
    var padding = this._chars.Length - this._length;
    for (var i = 0; i < padding; ++i)
      result[i] = paddingChar;
    Array.Copy(this._chars, 0, result, padding, this._length);
    return new(result, result.Length);
  }

  /// <summary>
  /// Returns a new <see cref="FixedString"/> with trailing NUL characters and whitespace removed.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FixedString TrimEnd() {
    if (this._chars == null || this._length == 0)
      return this;
    var newLength = this._length;
    while (newLength > 0 && (this._chars[newLength - 1] == '\0' || char.IsWhiteSpace(this._chars[newLength - 1])))
      --newLength;
    if (newLength == this._length)
      return this;
    var result = new char[this._chars.Length];
    Array.Copy(this._chars, result, newLength);
    return new(result, newLength);
  }

  /// <summary>
  /// Returns a <see cref="ReadOnlySpan{T}"/> over the actual content of this string.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ReadOnlySpan<char> AsSpan() => this._chars == null ? ReadOnlySpan<char>.Empty : this._chars.AsSpan(0, this._length);

  /// <summary>
  /// Returns a <see cref="ReadOnlySpan{T}"/> over the full capacity buffer.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ReadOnlySpan<char> AsFullSpan() => this._chars == null ? ReadOnlySpan<char>.Empty : this._chars.AsSpan();

  /// <summary>
  /// Returns a reference to the first character for use with the fixed statement.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ref readonly char GetPinnableReference()
    => ref (this._chars != null && this._chars.Length > 0 ? ref this._chars[0] : ref _NullRef());

  private static ref readonly char _NullRef() {
    unsafe {
      return ref Unsafe.AsRef<char>(null);
    }
  }

  /// <summary>
  /// Returns a null-terminated character array for P/Invoke.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public char[] ToNullTerminatedArray() {
    var result = new char[this._length + 1];
    if (this._chars != null && this._length > 0)
      Array.Copy(this._chars, result, this._length);
    result[this._length] = '\0';
    return result;
  }

  /// <summary>
  /// Returns the string representation (actual content, not padded to capacity).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override string ToString()
    => this._chars == null || this._length == 0 ? string.Empty : new string(this._chars, 0, this._length);

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() {
    if (this._chars == null || this._length == 0)
      return 0;
    unchecked {
      var hash = 17;
      for (var i = 0; i < this._length; ++i)
        hash = hash * 31 + this._chars[i];
      return hash;
    }
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override bool Equals(object? obj) => obj is FixedString other && this.Equals(other);

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(FixedString other) {
    if (this._length != other._length)
      return false;
    for (var i = 0; i < this._length; ++i)
      if (this._chars[i] != other._chars[i])
        return false;
    return true;
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(FixedString other) {
    var minLength = this._length < other._length ? this._length : other._length;
    for (var i = 0; i < minLength; ++i) {
      var cmp = this._chars[i].CompareTo(other._chars[i]);
      if (cmp != 0)
        return cmp;
    }
    return this._length.CompareTo(other._length);
  }

  /// <inheritdoc />
  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not FixedString other)
      throw new ArgumentException("Object must be of type FixedString.", nameof(obj));
    return this.CompareTo(other);
  }

  /// <summary>
  /// Determines whether two <see cref="FixedString"/> instances are equal (by content, not capacity).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator ==(FixedString left, FixedString right) => left.Equals(right);

  /// <summary>
  /// Determines whether two <see cref="FixedString"/> instances are not equal.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator !=(FixedString left, FixedString right) => !left.Equals(right);

  /// <summary>
  /// Determines whether the left <see cref="FixedString"/> is less than the right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator <(FixedString left, FixedString right) => left.CompareTo(right) < 0;

  /// <summary>
  /// Determines whether the left <see cref="FixedString"/> is greater than the right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator >(FixedString left, FixedString right) => left.CompareTo(right) > 0;

  /// <summary>
  /// Determines whether the left <see cref="FixedString"/> is less than or equal to the right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator <=(FixedString left, FixedString right) => left.CompareTo(right) <= 0;

  /// <summary>
  /// Determines whether the left <see cref="FixedString"/> is greater than or equal to the right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator >=(FixedString left, FixedString right) => left.CompareTo(right) >= 0;

  /// <summary>
  /// Implicitly converts a <see cref="FixedString"/> to a <see cref="string"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator string(FixedString value) => value.ToString();

  /// <summary>
  /// Explicitly converts a <see cref="FixedString"/> to a <see cref="StringZ"/>.
  /// May truncate at the first embedded null character.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static explicit operator StringZ(FixedString value) => new(value.ToString());

}
