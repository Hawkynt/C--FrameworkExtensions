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
/// Represents a zero-terminated UTF-16 string that cuts content at the first NUL character.
/// </summary>
/// <remarks>
/// This type is useful for interoperability with native code that uses null-terminated strings.
/// Any NUL characters in the input are treated as terminators, and content after them is discarded.
/// </remarks>
public readonly struct StringZ : IEquatable<StringZ>, IComparable<StringZ>, IComparable {

  private readonly string _value;

  /// <summary>
  /// Gets an empty <see cref="StringZ"/> instance.
  /// </summary>
  public static StringZ Empty => new(string.Empty);

  /// <summary>
  /// Initializes a new instance of the <see cref="StringZ"/> struct from a string.
  /// </summary>
  /// <param name="value">The string value. Content after the first NUL character is discarded.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public StringZ(string? value) => this._value = _TruncateAtNull(value);

  /// <summary>
  /// Initializes a new instance of the <see cref="StringZ"/> struct from a character span.
  /// </summary>
  /// <param name="value">The character span. Content after the first NUL character is discarded.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public StringZ(ReadOnlySpan<char> value) => this._value = _TruncateAtNull(value);

  /// <summary>
  /// Initializes a new instance of the <see cref="StringZ"/> struct from a character array.
  /// </summary>
  /// <param name="value">The character array. Content after the first NUL character is discarded.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public StringZ(char[]? value) => this._value = value == null ? string.Empty : _TruncateAtNull(value.AsSpan());

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static string _TruncateAtNull(string? value) {
    if (string.IsNullOrEmpty(value))
      return string.Empty;

    var nullIndex = value.IndexOf('\0');
    return nullIndex < 0 ? value : value.Substring(0, nullIndex);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static string _TruncateAtNull(ReadOnlySpan<char> value) {
    if (value.IsEmpty)
      return string.Empty;

    var nullIndex = value.IndexOf('\0');
    var span = nullIndex < 0 ? value : value.Slice(0, nullIndex);
    var chars = new char[span.Length];
    span.CopyTo(chars.AsSpan());
    return new(chars);
  }

  /// <summary>
  /// Gets the number of characters in the string.
  /// </summary>
  public int Length {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._value.Length;
  }

  /// <summary>
  /// Gets a value indicating whether this string is empty.
  /// </summary>
  public bool IsEmpty {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._value.Length == 0;
  }

  /// <summary>
  /// Gets the character at the specified position.
  /// </summary>
  /// <param name="index">The zero-based index of the character.</param>
  /// <exception cref="IndexOutOfRangeException">
  /// <paramref name="index"/> is less than zero or greater than or equal to <see cref="Length"/>.
  /// </exception>
  public char this[int index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if ((uint)index >= (uint)this._value.Length)
        AlwaysThrow.IndexOutOfRangeException();
      return this._value[index];
    }
  }

  /// <summary>
  /// Gets the character at the specified position using an <see cref="Index"/>.
  /// </summary>
  /// <param name="index">The index of the character.</param>
  /// <exception cref="IndexOutOfRangeException">
  /// The calculated index is out of range.
  /// </exception>
  public char this[Index index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var offset = index.GetOffset(this._value.Length);
      if ((uint)offset >= (uint)this._value.Length)
        AlwaysThrow.IndexOutOfRangeException();
      return this._value[offset];
    }
  }

  /// <summary>
  /// Gets a substring specified by a <see cref="Range"/>.
  /// </summary>
  /// <param name="range">The range of characters to extract.</param>
  /// <returns>A new <see cref="StringZ"/> containing the specified range.</returns>
  public StringZ this[Range range] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var (start, length) = range.GetOffsetAndLength(this._value.Length);
      return new(this._value.Substring(start, length));
    }
  }

  /// <summary>
  /// Retrieves a substring starting at the specified position.
  /// </summary>
  /// <param name="startIndex">The zero-based starting character position.</param>
  /// <returns>A new <see cref="StringZ"/> containing the substring.</returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="startIndex"/> is less than zero or greater than <see cref="Length"/>.
  /// </exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public StringZ Substring(int startIndex) {
    if ((uint)startIndex > (uint)this._value.Length)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(startIndex));
    return new(this._value.Substring(startIndex));
  }

  /// <summary>
  /// Retrieves a substring starting at the specified position with the specified length.
  /// </summary>
  /// <param name="startIndex">The zero-based starting character position.</param>
  /// <param name="length">The number of characters to include.</param>
  /// <returns>A new <see cref="StringZ"/> containing the substring.</returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="startIndex"/> or <paramref name="length"/> is out of range.
  /// </exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public StringZ Substring(int startIndex, int length) {
    if ((uint)startIndex > (uint)this._value.Length)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(startIndex));
    if ((uint)length > (uint)(this._value.Length - startIndex))
      AlwaysThrow.ArgumentOutOfRangeException(nameof(length));
    return new(this._value.Substring(startIndex, length));
  }

  /// <summary>
  /// Returns a <see cref="ReadOnlySpan{T}"/> over the characters of this string.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ReadOnlySpan<char> AsSpan() => this._value.AsSpan();

  /// <summary>
  /// Returns a <see cref="ReadOnlySpan{T}"/> over a portion of this string.
  /// </summary>
  /// <param name="start">The starting index.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ReadOnlySpan<char> AsSpan(int start) => this._value.AsSpan(start);

  /// <summary>
  /// Returns a <see cref="ReadOnlySpan{T}"/> over a portion of this string.
  /// </summary>
  /// <param name="start">The starting index.</param>
  /// <param name="length">The number of characters.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ReadOnlySpan<char> AsSpan(int start, int length) => this._value.AsSpan(start, length);

  /// <summary>
  /// Returns a reference to the first character for use with the fixed statement.
  /// </summary>
  /// <returns>A reference to the first character, or a null reference if the string is empty.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ref readonly char GetPinnableReference()
    => ref (this._value.Length > 0 ? ref this._value.AsSpan().GetPinnableReference() : ref _NullRef());

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
    var result = new char[this._value.Length + 1];
    this._value.CopyTo(0, result, 0, this._value.Length);
    result[this._value.Length] = '\0';
    return result;
  }

  /// <summary>
  /// Returns the string representation.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override string ToString() => this._value;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this._value.GetHashCode();

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override bool Equals(object? obj) => obj is StringZ other && this.Equals(other);

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(StringZ other) => this._value == other._value;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(StringZ other) => string.Compare(this._value, other._value, StringComparison.Ordinal);

  /// <inheritdoc />
  public int CompareTo(object? obj) {
    if (obj is null)
      return 1;
    if (obj is not StringZ other)
      throw new ArgumentException("Object must be of type StringZ.", nameof(obj));
    return this.CompareTo(other);
  }

  /// <summary>
  /// Determines whether two <see cref="StringZ"/> instances are equal.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator ==(StringZ left, StringZ right) => left.Equals(right);

  /// <summary>
  /// Determines whether two <see cref="StringZ"/> instances are not equal.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator !=(StringZ left, StringZ right) => !left.Equals(right);

  /// <summary>
  /// Determines whether the left <see cref="StringZ"/> is less than the right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator <(StringZ left, StringZ right) => left.CompareTo(right) < 0;

  /// <summary>
  /// Determines whether the left <see cref="StringZ"/> is greater than the right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator >(StringZ left, StringZ right) => left.CompareTo(right) > 0;

  /// <summary>
  /// Determines whether the left <see cref="StringZ"/> is less than or equal to the right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator <=(StringZ left, StringZ right) => left.CompareTo(right) <= 0;

  /// <summary>
  /// Determines whether the left <see cref="StringZ"/> is greater than or equal to the right.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator >=(StringZ left, StringZ right) => left.CompareTo(right) >= 0;

  /// <summary>
  /// Implicitly converts a <see cref="string"/> to a <see cref="StringZ"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator StringZ(string? value) => new(value);

  /// <summary>
  /// Implicitly converts a <see cref="StringZ"/> to a <see cref="string"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator string(StringZ value) => value._value;

  /// <summary>
  /// Concatenates two <see cref="StringZ"/> instances.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static StringZ operator +(StringZ left, StringZ right) => new(left._value + right._value);

}
