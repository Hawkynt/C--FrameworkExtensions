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

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class StringExtensions {

  private const int _INDEX_NOT_FOUND = -1;

#if SUPPORTS_SPAN
  private const int _MAX_STACKALLOC_STRING_LENGTH = 256;
#endif

  /// <summary>
  ///   Computes the hash of the current string using the specified hash algorithm.
  /// </summary>
  /// <typeparam name="TAlgorithm">
  ///   The type of the hash algorithm to use. Must be a subclass of <see cref="HashAlgorithm" />
  ///   and have a parameterless constructor.
  /// </typeparam>
  /// <param name="this">The current string instance to hash.</param>
  /// <returns>A hexadecimal string representation of the computed hash.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this" /> is <see langword="null" />.</exception>
  /// <example>
  ///   <code>
  /// string input = "Hello, world!";
  /// string sha256Hash = input.ComputeHash&lt;SHA256CryptoServiceProvider&gt;();
  /// Console.WriteLine($"SHA-256 Hash: {sha256Hash}");
  /// 
  /// string md5Hash = input.ComputeHash&lt;MD5CryptoServiceProvider&gt;();
  /// Console.WriteLine($"MD5 Hash: {md5Hash}");
  /// </code>
  ///   This example demonstrates how to compute the SHA-256 and MD5 hashes of a string using the <c>ComputeHash</c> method.
  /// </example>
  /// <remarks>
  ///   This method uses the UTF-8 encoding to convert the string to a byte array before computing the hash.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ComputeHash<TAlgorithm>(this string @this) where TAlgorithm : HashAlgorithm, new() {
    using var hashAlgorithm = new TAlgorithm();
    return ComputeHash(@this, hashAlgorithm);
  }

  /// <summary>
  ///   Computes the hash of the current string using the specified hash algorithm.
  /// </summary>
  /// <param name="this">The current string instance to hash.</param>
  /// <param name="hashAlgorithm">The hash algorithm to use for computing the hash.</param>
  /// <returns>A hexadecimal string representation of the computed hash.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this" /> is <see langword="null" />.</exception>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="hashAlgorithm" /> is <see langword="null" />.</exception>
  /// <example>
  ///   <code>
  /// string input = "Hello, world!";
  /// using (var sha256 = SHA256.Create())
  /// {
  ///     string sha256Hash = input.ComputeHash(sha256);
  ///     Console.WriteLine($"SHA-256 Hash: {sha256Hash}");
  /// }
  /// 
  /// using (var md5 = MD5.Create())
  /// {
  ///     string md5Hash = input.ComputeHash(md5);
  ///     Console.WriteLine($"MD5 Hash: {md5Hash}");
  /// }
  /// </code>
  ///   This example demonstrates how to compute the SHA-256 and MD5 hashes of a string using the <c>ComputeHash</c> method
  ///   with specified hash algorithms.
  /// </example>
  /// <remarks>
  ///   This method uses the UTF-8 encoding to convert the string to a byte array before computing the hash.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ComputeHash(this string @this, HashAlgorithm hashAlgorithm) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(hashAlgorithm);

    var bytes = Encoding.UTF8.GetBytes(@this);
    var hash = hashAlgorithm.ComputeHash(bytes);
    return hash.ToHex();
  }

  #region ExchangeAt

  /// <summary>
  ///   Exchanges a certain part of the string with the given newString.
  /// </summary>
  /// <param name="this">This String.</param>
  /// <param name="index">The index to start the modification at.</param>
  /// <param name="replacement">The new string to insert.</param>
  /// <returns>
  ///   The modified string
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ExchangeAt(this string @this, int index, string replacement) {
    Against.ThisIsNull(@this);
    Against.IndexBelowZero(index);
    Against.ValueIsZero(index);

    return index < @this.Length
        ? @this[..index] + replacement
        : @this + replacement
      ;
  }

#if SUPPORTS_SPAN && !SUPPORTS_STRING_COPYTO_SPAN
  public static void CopyTo(this string @this, Span<char> target) => @this.AsSpan().CopyTo(target);
#endif

  /// <summary>
  ///   Exchanges a certain character of the string with the given character.
  /// </summary>
  /// <param name="this">This String.</param>
  /// <param name="index">The index to start the modification at.</param>
  /// <param name="replacement">The character.</param>
  /// <returns>
  ///   The modified string
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ExchangeAt(this string @this, int index, char replacement) {
    Against.ThisIsNull(@this);
    Against.IndexBelowZero(index);

    var length = @this.Length;

#if SUPPORTS_SPAN
    if (index == 0 && length <= 1)
      return replacement.ToString();

    var result = length < _MAX_STACKALLOC_STRING_LENGTH ? stackalloc char[length + 1] : new char[length + 1]; // allocate one character more
    @this.CopyTo(result);

    if (index >= length) {
      result[length] = replacement;
      return new(result);
    }

    result[index] = replacement;
    return new(result[..^1]); // strip last unneeded character

#else

    string part1;
    var part2 = replacement.ToString();

    if (index < length) {
      if (index < length - 1)
        return @this[..index] + part2 + @this[(index + 1)..];

      part1 = @this[..^1];
    } else if (index == 0) {
      if (length <= 1)
        return part2;

      part1 = part2;
      part2 = @this[1..];
    } else
      part1 = @this;

    return part1 + part2;

#endif
  }

  /// <summary>
  ///   Exchanges a certain part of the string with the given newString.
  /// </summary>
  /// <param name="this">This String.</param>
  /// <param name="index">The index to start the modification at.</param>
  /// <param name="count">The number of characters to replace.</param>
  /// <param name="replacement">The new string to insert.</param>
  /// <returns>The modified string</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ExchangeAt(this string @this, int index, int count, string replacement) {
    Against.ThisIsNull(@this);
    Against.IndexBelowZero(index);
    Against.CountBelowOrEqualZero(count);

    return (index, count, @this.Length) switch {
      (0, var c, var l) when l <= c => replacement,
      (0, var c, _) => replacement + @this[c..],
      var (i, c, l) when i + c >= l => @this[..i] + replacement,
      var (i, c, _) => @this[..i] + replacement + @this[(i + c)..]
    };
  }

  #endregion

  /// <summary>
  ///   Repeats the specified string a certain number of times.
  /// </summary>
  /// <param name="this">This String.</param>
  /// <param name="count">The count.</param>
  /// <returns></returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string Repeat(this string @this, int count) {
    Against.ThisIsNull(@this);
    Against.CountBelowZero(count);

    return count switch {
      0 => string.Empty,
      _ when @this.Length == 0 => string.Empty,
      _ when @this.Length == 1 => @this[0].Repeat(count),
      1 => @this,
      2 => @this + @this,
      3 => string.Concat(@this, @this, @this),
      4 => string.Concat(@this, @this, @this, @this),
      _ => Invoke(@this, count)
    };

    static string Invoke(string t, int c) {
      StringBuilder result = new(t.Length * c);
      for (var i = c; i > 0; --i)
        result.Append(t);

      return result.ToString();
    }
  }

  /// <summary>
  /// Removes the specified number of characters from the start of the given string.
  /// </summary>
  /// <param name="this">The string to process. Must not be <see langword="null"/>.</param>
  /// <param name="count">The number of characters to remove. Must be between 0 and the length of <paramref name="this"/>.</param>
  /// <returns>
  /// A new string with the first <paramref name="count"/> characters removed.
  /// If <paramref name="count"/> is 0, the original string is returned.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown when <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="ArgumentOutOfRangeException">
  /// Thrown when <paramref name="count"/> is negative or greater than the length of <paramref name="this"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// string input = "HelloWorld";
  /// string result = input.RemoveFirst(5);
  /// Console.WriteLine(result); // Output: "World"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string RemoveFirst(this string @this, int count) {
    Against.ThisIsNull(@this);
    Against.CountBelowZero(count);

    return count < @this.Length ? @this[count..] : string.Empty;
  }

  /// <summary>
  /// Removes the specified number of characters from the start of the given <see cref="ReadOnlySpan{char}"/>.
  /// </summary>
  /// <param name="this">The span to process.</param>
  /// <param name="count">The number of characters to remove. Must be greater than 0 and less than or equal to the length of <paramref name="this"/>.</param>
  /// <returns>
  /// A new <see cref="ReadOnlySpan{char}"/> with the first <paramref name="count"/> characters removed.
  /// </returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// Thrown when <paramref name="count"/> is less than or equal to 0, or greater than the length of <paramref name="this"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// ReadOnlySpan&lt;char&gt; span = "HelloWorld".AsSpan();
  /// ReadOnlySpan&lt;char&gt; result = span.RemoveFirst(5);
  /// Console.WriteLine(result.ToString()); // Output: "World"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ReadOnlySpan<char> RemoveFirst(this ReadOnlySpan<char> @this, int count) {
    Against.CountOutOfRange(count, @this.Length);

    return count < @this.Length ? @this[count..] : [];
  }

  /// <summary>
  /// Removes the specified number of characters from the end of the given string.
  /// </summary>
  /// <param name="this">The string to process. Must not be <see langword="null"/>.</param>
  /// <param name="count">The number of characters to remove. Must be between 0 and the length of <paramref name="this"/>.</param>
  /// <returns>
  /// A new string with the last <paramref name="count"/> characters removed.
  /// If <paramref name="count"/> is 0, the original string is returned.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown when <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="ArgumentOutOfRangeException">
  /// Thrown when <paramref name="count"/> is negative or greater than the length of <paramref name="this"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// string input = "HelloWorld";
  /// string result = input.RemoveLast(5);
  /// Console.WriteLine(result); // Output: "Hello"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string RemoveLast(this string @this, int count) {
    Against.ThisIsNull(@this);
    Against.CountBelowZero(count);

    return count < @this.Length ? @this[..^count] : string.Empty;
  }

  /// <summary>
  /// Removes the specified number of characters from the end of the given <see cref="ReadOnlySpan{char}"/>.
  /// </summary>
  /// <param name="this">The span to process.</param>
  /// <param name="count">The number of characters to remove. Must be greater than 0 and less than or equal to the length of <paramref name="this"/>.</param>
  /// <returns>
  /// A new <see cref="ReadOnlySpan{char}"/> with the last <paramref name="count"/> characters removed.
  /// </returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// Thrown when <paramref name="count"/> is less than or equal to 0, or greater than the length of <paramref name="this"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// ReadOnlySpan&lt;char&gt; span = "HelloWorld".AsSpan();
  /// ReadOnlySpan&lt;char&gt; result = span.RemoveLast(5);
  /// Console.WriteLine(result.ToString()); // Output: "Hello"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ReadOnlySpan<char> RemoveLast(this ReadOnlySpan<char> @this, int count) {
    Against.CountOutOfRange(count, @this.Length);

    return count < @this.Length ? @this[..^count] : [];
  }

  /// <summary>
  /// Removes the specified substring from the start of the given string, if present.
  /// </summary>
  /// <param name="this">The string to process. Must not be <see langword="null"/>.</param>
  /// <param name="what">The substring to remove from the beginning of <paramref name="this"/>. Must not be <see langword="null"/>.</param>
  /// <param name="comparison"><i>(Optional)</i> The <see cref="StringComparison"/> mode used to compare substrings. Defaults to <see cref="StringComparison.CurrentCulture"/>.</param>
  /// <returns>
  /// A new string with <paramref name="what"/> removed from the beginning, or the original string if it does not start with <paramref name="what"/>.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown when <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="ArgumentNullException">
  /// Thrown when <paramref name="what"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// string input = "HelloWorld";
  /// string result = input.RemoveAtStart("Hello");
  /// Console.WriteLine(result); // Output: "World"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string RemoveAtStart(this string @this, string what, StringComparison comparison = StringComparison.CurrentCulture) => ReplaceAtStart(@this, what, null, comparison);

  /// <summary>
  /// Removes the specified substring from the start of the given <see cref="ReadOnlySpan{char}"/>, if present.
  /// </summary>
  /// <param name="this">The span to process.</param>
  /// <param name="what">The substring to remove from the beginning of <paramref name="this"/>.</param>
  /// <param name="comparison"><i>(Optional)</i> The <see cref="StringComparison"/> mode used to compare substrings. Defaults to <see cref="StringComparison.CurrentCulture"/>.</param>
  /// <returns>
  /// A new <see cref="ReadOnlySpan{char}"/> with <paramref name="what"/> removed from the start, or the original span if it does not start with <paramref name="what"/>.
  /// </returns>
  /// <example>
  /// <code>
  /// ReadOnlySpan&lt;char&gt; span = "HelloWorld".AsSpan();
  /// ReadOnlySpan&lt;char&gt; result = span.RemoveAtStart("Hello".AsSpan());
  /// Console.WriteLine(result.ToString()); // Output: "World"
  /// </code>
  /// </example>
  public static ReadOnlySpan<char> RemoveAtStart(this ReadOnlySpan<char> @this, ReadOnlySpan<char> what, StringComparison comparison = StringComparison.CurrentCulture) {
    if (what.IsEmpty || @this.IsEmpty || @this.Length < what.Length)
      return @this;

    var prefix = @this[..what.Length];
    if (comparison == StringComparison.Ordinal)
      return prefix.SequenceEqual(what) ? @this[what.Length..] : @this;

    if (prefix.ToString().Equals(what.ToString(), comparison))
      return @this[what.Length..];

    return @this;
  }

  /// <summary>
  /// Removes the specified substring from the end of the given string, if present.
  /// </summary>
  /// <param name="this">The string to process. Must not be <see langword="null"/>.</param>
  /// <param name="what">The substring to remove from the end of <paramref name="this"/>. Must not be <see langword="null"/>.</param>
  /// <param name="comparison"><i>(Optional)</i> The <see cref="StringComparison"/> mode used to compare substrings. Defaults to <see cref="StringComparison.CurrentCulture"/>.</param>
  /// <returns>
  /// A new string with <paramref name="what"/> removed from the end, or the original string if it does not end with <paramref name="what"/>.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown when <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="ArgumentNullException">
  /// Thrown when <paramref name="what"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// string input = "HelloWorld";
  /// string result = input.RemoveAtEnd("World");
  /// Console.WriteLine(result); // Output: "Hello"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string RemoveAtEnd(this string @this, string what, StringComparison comparison = StringComparison.CurrentCulture) => ReplaceAtEnd(@this, what, null, comparison);

  /// <summary>
  /// Removes the specified substring from the end of the given <see cref="ReadOnlySpan{char}"/>, if present.
  /// </summary>
  /// <param name="this">The span to process.</param>
  /// <param name="what">The substring to remove from the end of <paramref name="this"/>.</param>
  /// <param name="comparison"><i>(Optional)</i> The <see cref="StringComparison"/> mode used to compare substrings. Defaults to <see cref="StringComparison.CurrentCulture"/>.</param>
  /// <returns>
  /// A new <see cref="ReadOnlySpan{char}"/> with <paramref name="what"/> removed from the end, or the original span if it does not end with <paramref name="what"/>.
  /// </returns>
  /// <example>
  /// <code>
  /// ReadOnlySpan&lt;char&gt; span = "HelloWorld".AsSpan();
  /// ReadOnlySpan&lt;char&gt; result = span.RemoveAtEnd("World".AsSpan());
  /// Console.WriteLine(result.ToString()); // Output: "Hello"
  /// </code>
  /// </example>
  public static ReadOnlySpan<char> RemoveAtEnd(this ReadOnlySpan<char> @this, ReadOnlySpan<char> what, StringComparison comparison = StringComparison.CurrentCulture) {
    if (what.IsEmpty || @this.IsEmpty || @this.Length < what.Length)
      return @this;

    var suffix = @this[^what.Length..];
    if (comparison == StringComparison.Ordinal)
      return suffix.SequenceEqual(what) ? @this[..^what.Length] : @this;

    if (suffix.ToString().Equals(what.ToString(), comparison))
      return @this[..^what.Length];

    return @this;
  }

  /// <summary>
  /// Extracts a substring from the given <see cref="ReadOnlySpan{char}"/> starting at the specified position and with the specified length.
  /// </summary>
  /// <param name="this">The span to extract the substring from.</param>
  /// <param name="position">The zero-based starting position of the substring. Must be within valid range.</param>
  /// <param name="count">The number of characters to extract. Must be greater than 0 and within the available length from <paramref name="position"/>.</param>
  /// <returns>
  /// A <see cref="ReadOnlySpan{char}"/> that represents the extracted substring.
  /// </returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// Thrown when <paramref name="position"/> is outside the valid range (0 to <c><paramref name="this"/>.Length - 1</c>) 
  /// or when <paramref name="count"/> is less than or equal to 0, or exceeds the available length from <paramref name="position"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// ReadOnlySpan&lt;char&gt; span = "HelloWorld".AsSpan();
  /// ReadOnlySpan&lt;char&gt; result = span.Substring(5, 5);
  /// Console.WriteLine(result.ToString()); // Output: "World"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ReadOnlySpan<char> Substring(this ReadOnlySpan<char> @this, int position, int count) {
    Against.IndexOutOfRange(position, @this.Length - 1);
    Against.CountOutOfRange(count, @this.Length - position);

    return @this[position..(position + count)];
  }

  /// <summary>
  /// Extracts a substring from the given <see cref="ReadOnlySpan{char}"/> starting at the specified position and continuing to the end.
  /// </summary>
  /// <param name="this">The span to extract the substring from.</param>
  /// <param name="position">The zero-based starting position of the substring. Must be within valid range.</param>
  /// <returns>
  /// A <see cref="ReadOnlySpan{char}"/> that represents the extracted substring starting from <paramref name="position"/> to the end.
  /// </returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// Thrown when <paramref name="position"/> is outside the valid range (0 to <c><paramref name="this"/>.Length - 1</c>).
  /// </exception>
  /// <example>
  /// <code>
  /// ReadOnlySpan&lt;char&gt; span = "HelloWorld".AsSpan();
  /// ReadOnlySpan&lt;char&gt; result = span.Substring(5);
  /// Console.WriteLine(result.ToString()); // Output: "World"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ReadOnlySpan<char> Substring(this ReadOnlySpan<char> @this, int position) {
    Against.IndexOutOfRange(position, @this.Length - 1);

    return @this[position..];
  }

  /// <summary>
  /// Extracts a substring from the given string using a start and optional end index, with support for negative indices.
  /// </summary>
  /// <param name="this">The string to extract the substring from. Must not be <see langword="null"/>.</param>
  /// <param name="start">The zero-based starting index. If negative, it is treated as an offset from the end of the string.</param>
  /// <param name="end"><i>(Optional)</i> The zero-based ending index (exclusive). If negative or zero, it is treated as an offset from the end of the string. Defaults to 0.</param>
  /// <returns>
  /// A substring of <paramref name="this"/> determined by <paramref name="start"/> and <paramref name="end"/>. If <paramref name="start"/> and <paramref name="end"/> are equal, returns a single-character string.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown when <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <remarks>
  /// Supports negative indices: <br/>
  /// &#8226; If <paramref name="start"/> is negative, it is interpreted as an offset from the end of the string. <br/>
  /// &#8226; If <paramref name="end"/> is negative or zero, it is interpreted as an offset from the end of the string. <br/>
  /// &#8226; If <paramref name="start"/> is out of range, it is clamped to the valid range. <br/>
  /// &#8226; If <paramref name="end"/> is out of range, it is adjusted accordingly. <br/>
  /// &#8226; If <paramref name="start"/> and <paramref name="end"/> are the same, a single-character string is returned. <br/>
  /// </remarks>
  /// <example>
  /// <code>
  /// string input = "HelloWorld";
  /// string result1 = input.SubString(2, 7);
  /// Console.WriteLine(result1); // Output: "lloWo"
  ///
  /// string result2 = input.SubString(-5, -1);
  /// Console.WriteLine(result2); // Output: "Worl"
  ///
  /// string result3 = input.SubString(-3);
  /// Console.WriteLine(result3); // Output: "rld"
  /// </code>
  /// </example>
  public static string SubString(this string @this, int start, int end = 0) {
    Against.ThisIsNull(@this);
    
    var result = string.Empty;
    var length = @this.Length;

    // ReSharper disable once InvertIf (perf-opt)
    if (length > 0) {
      // if (start < 0) start += length
      // if (end <= 0) end += length
      var startMask = start;
      var endMask = end - 1;
      startMask >>= 31;
      endMask >>= 31;
      startMask &= length;
      endMask &= length;
      start += startMask;
      end += endMask;

      // if (start < 0) start = 0
      startMask = ~start;
      startMask >>= 31;
      start &= startMask;

      if (start != end) {
        var len = end - start;

        // if (len > length) len = length - start
        len -= (len - (length - start)) & ((length - len) >> 31);

        if (len > 0)
          result = @this.Substring(start, len);
      } else
        result = @this[start].ToString();
    }

    return result;
  }

  /// <summary>
  /// Extracts a substring from the given <see cref="ReadOnlySpan{char}"/> using a start and optional end index, with support for negative indices.
  /// </summary>
  /// <param name="this">The span to extract the substring from.</param>
  /// <param name="start">The zero-based starting index. If negative, it is treated as an offset from the end of the span.</param>
  /// <param name="end"><i>(Optional)</i> The zero-based ending index (exclusive). If negative or zero, it is treated as an offset from the end of the span. Defaults to 0.</param>
  /// <returns>
  /// A <see cref="ReadOnlySpan{char}"/> representing the extracted substring. If <paramref name="start"/> and <paramref name="end"/> are equal, returns a single-character span.
  /// </returns>
  /// <remarks>
  /// Supports negative indices: <br/>
  /// &#8226; If <paramref name="start"/> is negative, it is interpreted as an offset from the end of the span. <br/>
  /// &#8226; If <paramref name="end"/> is negative or zero, it is interpreted as an offset from the end of the span. <br/>
  /// &#8226; If <paramref name="start"/> is out of range, it is clamped to the valid range. <br/>
  /// &#8226; If <paramref name="end"/> is out of range, it is adjusted accordingly. <br/>
  /// &#8226; If <paramref name="start"/> and <paramref name="end"/> are the same, a single-character span is returned. <br/>
  /// </remarks>
  /// <example>
  /// <code>
  /// ReadOnlySpan&lt;char&gt; span = "HelloWorld".AsSpan();
  /// ReadOnlySpan&lt;char&gt; result1 = span.SubString(2, 7);
  /// Console.WriteLine(result1.ToString()); // Output: "lloWo"
  ///
  /// ReadOnlySpan&lt;char&gt; result2 = span.SubString(-5, -1);
  /// Console.WriteLine(result2.ToString()); // Output: "Worl"
  ///
  /// ReadOnlySpan&lt;char&gt; result3 = span.SubString(-3);
  /// Console.WriteLine(result3.ToString()); // Output: "rld"
  /// </code>
  /// </example>
  public static ReadOnlySpan<char> SubString(this ReadOnlySpan<char> @this, int start, int end = 0) {
    ReadOnlySpan<char> result = [];
    var length = @this.Length;
    // ReSharper disable once InvertIf (perf-opt)
    if (length > 0) {
      // if (start < 0) start += length
      // if (end <= 0) end += length
      var startMask = start;
      var endMask = end - 1;
      startMask >>= 31;
      endMask >>= 31;
      startMask &= length;
      endMask &= length;
      start += startMask;
      end += endMask;
      // if (start < 0) start = 0
      startMask = ~start;
      startMask >>= 31;
      start &= startMask;
      if (start != end) {
        var len = end - start;
        // if (len > length) len = length - start
        len -= (len - (length - start)) & ((length - len) >> 31);
        if (len > 0)
          result = @this[start..(start + len)];
      } else if (start < length)
        result = @this.Slice(start, 1);
    }
    return result;
  }

  /// <summary>
  /// Retrieves the leftmost <paramref name="count"/> characters from the given string.
  /// </summary>
  /// <param name="this">The string to process. Must not be <see langword="null"/>.</param>
  /// <param name="count">The number of characters to extract. Must be greater than or equal to 0.</param>
  /// <returns>
  /// A substring containing the first <paramref name="count"/> characters of <paramref name="this"/>.
  /// If <paramref name="count"/> is 0, an empty string is returned.
  /// If <paramref name="count"/> exceeds the string's length, the original string is returned.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown when <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="ArgumentOutOfRangeException">
  /// Thrown when <paramref name="count"/> is negative.
  /// </exception>
  /// <example>
  /// <code>
  /// string input = "HelloWorld";
  /// string result1 = input.Left(5);
  /// Console.WriteLine(result1); // Output: "Hello"
  ///
  /// string result2 = input.Left(20);
  /// Console.WriteLine(result2); // Output: "HelloWorld"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string Left(this string @this, int count) {
    Against.ThisIsNull(@this);
    Against.CountBelowZero(count);

    // if (count > @this.Length) count = @this.Length
    var mask = @this.Length - count;
    count += mask & (mask >> 31);
    return @this[..count];
  }

  /// <summary>
  /// Retrieves the leftmost <paramref name="count"/> characters from the given <see cref="ReadOnlySpan{char}"/>.
  /// </summary>
  /// <param name="this">The span to process.</param>
  /// <param name="count">The number of characters to extract. Must be greater than or equal to 0.</param>
  /// <returns>
  /// A <see cref="ReadOnlySpan{char}"/> containing the first <paramref name="count"/> characters of <paramref name="this"/>.
  /// If <paramref name="count"/> is 0, an empty span is returned.
  /// If <paramref name="count"/> exceeds the span's length, the original span is returned.
  /// </returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// Thrown when <paramref name="count"/> is negative.
  /// </exception>
  /// <example>
  /// <code>
  /// ReadOnlySpan&lt;char&gt; span = "HelloWorld".AsSpan();
  /// ReadOnlySpan&lt;char&gt; result1 = span.Left(5);
  /// Console.WriteLine(result1.ToString()); // Output: "Hello"
  ///
  /// ReadOnlySpan&lt;char&gt; result2 = span.Left(20);
  /// Console.WriteLine(result2.ToString()); // Output: "HelloWorld"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ReadOnlySpan<char> Left(this ReadOnlySpan<char> @this, int count) {
    Against.CountBelowZero(count);
    // if (count > @this.Length) count = @this.Length
    var mask = @this.Length - count;
    count += mask & (mask >> 31);
    return @this[..count];
  }

  /// <summary>
  /// Retrieves the rightmost <paramref name="count"/> characters from the given string.
  /// </summary>
  /// <param name="this">The string to process. Must not be <see langword="null"/>.</param>
  /// <param name="count">The number of characters to extract. Must be greater than or equal to 0.</param>
  /// <returns>
  /// A substring containing the last <paramref name="count"/> characters of <paramref name="this"/>.
  /// If <paramref name="count"/> is 0, an empty string is returned.
  /// If <paramref name="count"/> exceeds the string's length, the original string is returned.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown when <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="ArgumentOutOfRangeException">
  /// Thrown when <paramref name="count"/> is negative.
  /// </exception>
  /// <example>
  /// <code>
  /// string input = "HelloWorld";
  /// string result1 = input.Right(5);
  /// Console.WriteLine(result1); // Output: "World"
  ///
  /// string result2 = input.Right(20);
  /// Console.WriteLine(result2); // Output: "HelloWorld"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string Right(this string @this, int count) {
    Against.ThisIsNull(@this);
    Against.CountBelowZero(count);

    // if (count > @this.Length) count = @this.Length
    var mask = @this.Length - count;
    count += mask & (mask >> 31);
    return @this[^count..];
  }

  /// <summary>
  /// Retrieves the rightmost <paramref name="count"/> characters from the given <see cref="ReadOnlySpan{char}"/>.
  /// </summary>
  /// <param name="this">The span to process.</param>
  /// <param name="count">The number of characters to extract. Must be greater than or equal to 0.</param>
  /// <returns>
  /// A <see cref="ReadOnlySpan{char}"/> containing the last <paramref name="count"/> characters of <paramref name="this"/>.
  /// If <paramref name="count"/> is 0, an empty span is returned.
  /// If <paramref name="count"/> exceeds the span's length, the original span is returned.
  /// </returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// Thrown when <paramref name="count"/> is negative.
  /// </exception>
  /// <example>
  /// <code>
  /// ReadOnlySpan&lt;char&gt; span = "HelloWorld".AsSpan();
  /// ReadOnlySpan&lt;char&gt; result1 = span.Right(5);
  /// Console.WriteLine(result1.ToString()); // Output: "World"
  ///
  /// ReadOnlySpan&lt;char&gt; result2 = span.Right(20);
  /// Console.WriteLine(result2.ToString()); // Output: "HelloWorld"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ReadOnlySpan<char> Right(this ReadOnlySpan<char> @this, int count) {
    Against.CountBelowZero(count);
    // if (count > @this.Length) count = @this.Length
    var mask = @this.Length - count;
    count += mask & (mask >> 31);
    return @this[^count..];
  }

  /// <summary>
  /// Replaces the first occurrence of a specified substring within the given string with another substring.
  /// </summary>
  /// <param name="this">The string to process. Must not be <see langword="null"/>.</param>
  /// <param name="what">The substring to be replaced. Must not be <see langword="null"/>.</param>
  /// <param name="replacement">The substring to replace the first occurrence of <paramref name="what"/>.</param>
  /// <param name="comparison"><i>(Optional)</i> The <see cref="StringComparison"/> mode used to compare substrings. Defaults to <see cref="StringComparison.CurrentCulture"/>.</param>
  /// <returns>
  /// A new string where the first occurrence of <paramref name="what"/> in <paramref name="this"/> is replaced with <paramref name="replacement"/>.
  /// If <paramref name="what"/> is not found, the original string is returned.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown when <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="ArgumentNullException">
  /// Thrown when <paramref name="what"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// string input = "hello world, hello universe";
  /// string result = input.ReplaceFirst("hello", "hi");
  /// Console.WriteLine(result); // Output: "hi world, hello universe"
  /// </code>
  /// </example>
  public static string ReplaceFirst(this string @this, string what, string replacement, StringComparison comparison = StringComparison.CurrentCulture) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(what);

    return what.Length switch {
      <= 0 when IsNullOrEmpty(replacement) => @this,
      <= 0 => replacement + @this,
      var needleLength when needleLength > @this.Length => @this,
      var needleLength => @this.IndexOf(what, comparison) switch {
        < 0 => @this,
        0 when IsNullOrEmpty(replacement) => @this[needleLength..],
        0 => replacement + @this[needleLength..],
        var index when IsNullOrEmpty(replacement) => @this[..index] + @this[(index + needleLength)..],
        var index => @this[..index] + replacement + @this[(index + needleLength)..]
      }
    };
  }

  /// <summary>
  /// Replaces the last occurrence of a specified substring within the given string with another substring.
  /// </summary>
  /// <param name="this">The string to process. Must not be <see langword="null"/>.</param>
  /// <param name="what">The substring to be replaced. Must not be <see langword="null"/>.</param>
  /// <param name="replacement">The substring to replace the last occurrence of <paramref name="what"/>.</param>
  /// <param name="comparison"><i>(Optional)</i> The <see cref="StringComparison"/> mode used to compare substrings. Defaults to <see cref="StringComparison.CurrentCulture"/>.</param>
  /// <returns>
  /// A new string where the last occurrence of <paramref name="what"/> in <paramref name="this"/> is replaced with <paramref name="replacement"/>.
  /// If <paramref name="what"/> is not found, the original string is returned.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown when <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="ArgumentNullException">
  /// Thrown when <paramref name="what"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// string input = "hello world, hello universe";
  /// string result = input.ReplaceLast("hello", "hi");
  /// Console.WriteLine(result); // Output: "hello world, hi universe"
  /// </code>
  /// </example>
  public static string ReplaceLast(this string @this, string what, string replacement, StringComparison comparison = StringComparison.CurrentCulture) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(what);

    return what.Length switch {
      <= 0 when IsNullOrEmpty(replacement) => @this,
      <= 0 => @this + replacement,
      var needleLength when needleLength > @this.Length => @this,
      var needleLength => @this.LastIndexOf(what, comparison) switch {
        < 0 => @this,
        0 when IsNullOrEmpty(replacement) => @this[needleLength..],
        0 => replacement + @this[needleLength..],
        var index when IsNullOrEmpty(replacement) => @this[..index] + @this[(index + needleLength)..],
        var index => @this[..index] + replacement + @this[(index + needleLength)..]
      }
    };
  }

  #region First/Last

  /// <summary>
  ///   Gets the first <see cref="char" /> of the <see cref="string" />.
  /// </summary>
  /// <param name="this">This <see cref="string" /></param>
  /// <returns>The first <see cref="char" /></returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static char First(this string @this) => @this[0];

  /// <summary>
  ///   Gets the first <see cref="char" /> of the <see cref="string" /> or a default value.
  /// </summary>
  /// <param name="this">This <see cref="string" /></param>
  /// <param name="default">The default <see cref="char" /> to return</param>
  /// <returns>The first <see cref="char" /></returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static char FirstOrDefault(this string @this, char @default = default) => IsNullOrEmpty(@this) ? @default : @this[0];

  /// <summary>
  ///   Gets the last <see cref="char" /> of the <see cref="string" />.
  /// </summary>
  /// <param name="this">This <see cref="string" /></param>
  /// <returns>The last <see cref="char" /></returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static char Last(this string @this) => @this[^1];

  /// <summary>
  ///   Gets the last <see cref="char" /> of the <see cref="string" /> or a default value.
  /// </summary>
  /// <param name="this">This <see cref="string" /></param>
  /// <param name="default">The default <see cref="char" /> to return</param>
  /// <returns>The last <see cref="char" /></returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static char LastOrDefault(this string @this, char @default = default) => IsNullOrEmpty(@this) ? @default : @this[^1];

  #endregion

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static bool _IsInvalidCharacter(char c) => (__isInvalidCharacter ??= new()).Invoke(c);

  private static __IsInvalidCharacter __isInvalidCharacter;

  private sealed class __IsInvalidCharacter {
    private readonly HashSet<char> _invalidFileNameChars =
      Path
        .GetInvalidFileNameChars()
        .Union(
          """
          <>|:?*/\"
          """
        )
        .ToHashSet(c => c);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Invoke(char c) => c < 32 || c >= 127 || this._invalidFileNameChars.Contains(c);
  }
      
  // Reserved names (Windows)
  private static readonly HashSet<string> _ReservedFileNameBases = new(StringComparer.OrdinalIgnoreCase) {
    "CON","PRN","AUX","NUL",
    "COM1","COM2","COM3","COM4","COM5","COM6","COM7","COM8","COM9",
    "LPT1","LPT2","LPT3","LPT4","LPT5","LPT6","LPT7","LPT8","LPT9",
  };

  /// <summary>
  ///   Sanitizes the text to use as a filename.
  /// </summary>
  /// <param name="this">This String.</param>
  /// <param name="sanitation">The character to use for sanitation; defaults to underscore (_)</param>
  /// <returns>The sanitized string.</returns>
  public static unsafe string SanitizeForFileName(this string @this, char sanitation = '_') {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrEmpty(@this);
        
    var result = @this;
    var length = (uint)@this.Length;
    fixed (char* srcPointer = @this) {
      var currentPointer = srcPointer;
      for (; length > 0; ++currentPointer, --length) {
        if (!_IsInvalidCharacter(*currentPointer))
          continue;

        // copy-on-write the source string
#if SUPPORTS_SPAN
        result = new(@this);
#else
        result = string.Copy(@this);
#endif
        fixed (char* dstPointer = result) {
          // re-base the pointer we're not gonna need the srcPointer from here on
          currentPointer += dstPointer - srcPointer;

          // we already know the current char is invalid
          --length;
          *currentPointer++ = sanitation;

          // process the rest
          for (; length > 0; ++currentPointer, --length)
            if (_IsInvalidCharacter(*currentPointer))
              *currentPointer = sanitation;

          result = new(dstPointer);
        }

        break;
      }
    }

    // Reserved names: Check basename without extension (ignore trailing '.' and ' ')
    var baseName = Path.GetFileNameWithoutExtension(result);
    if (!string.IsNullOrEmpty(baseName)) {
      baseName = baseName.TrimEnd(' ', '.');
      if (_ReservedFileNameBases.Contains(baseName))
        result = sanitation + result;
    }

    return result;
  }

  #region Matching Regexes

  /// <summary>
  ///   Determines whether the specified string matches the given regex.
  /// </summary>
  /// <param name="this">This String.</param>
  /// <param name="regex">The regex.</param>
  /// <returns>
  ///   <c>true</c> if it matches; otherwise, <c>false</c>.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsMatch(this string @this, Regex regex) => @this != null && regex.IsMatch(@this);

  /// <summary>
  ///   Determines whether the specified string matches the given regex.
  /// </summary>
  /// <param name="this">This String.</param>
  /// <param name="regex">The regex.</param>
  /// <returns>
  ///   <c>false</c> if it matches; otherwise, <c>true</c>.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNotMatch(this string @this, Regex regex) => !IsMatch(@this, regex);

  /// <summary>
  ///   Determines whether the specified string matches the given regex.
  /// </summary>
  /// <param name="this">This String.</param>
  /// <param name="regex">The regex.</param>
  /// <param name="regexOptions">The regex options.</param>
  /// <returns><c>true</c> if it matches; otherwise, <c>false</c>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsMatch(this string @this, string regex, RegexOptions regexOptions = RegexOptions.None) => @this != null && @this.IsMatch(new(regex, regexOptions));

  /// <summary>
  ///   Determines whether the specified string matches the given regex.
  /// </summary>
  /// <param name="this">This String.</param>
  /// <param name="regex">The regex.</param>
  /// <param name="regexOptions">The regex options.</param>
  /// <returns><c>false</c> if it matches; otherwise, <c>true</c>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNotMatch(this string @this, string regex, RegexOptions regexOptions = RegexOptions.None) => !IsMatch(@this, regex, regexOptions);

  /// <summary>
  ///   Matches the specified regex.
  /// </summary>
  /// <param name="regex">The regex.</param>
  /// <param name="this">The data.</param>
  /// <param name="regexOptions">The regex options.</param>
  /// <returns>A <see cref="MatchCollection" /> containing the matches.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static MatchCollection Matches(this string @this, string regex, RegexOptions regexOptions = RegexOptions.None) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrEmpty(regex);
    return new Regex(regex, regexOptions).Matches(@this);
  }

  /// <summary>
  ///   Matches the specified regex and returns the groups.
  /// </summary>
  /// <param name="this">This string.</param>
  /// <param name="regex">The regex.</param>
  /// <param name="regexOptions">The regex options.</param>
  /// <returns>
  ///   A <see cref="GroupCollection" /> containing the found groups.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static GroupCollection MatchGroups(this string @this, string regex, RegexOptions regexOptions = RegexOptions.None) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrEmpty(regex);
    return new Regex(regex, regexOptions).Match(@this).Groups;
  }

  #endregion

  #region Formatting

  /// <summary>
  /// Uses the string as a format string and formats it using the specified parameters.
  /// </summary>
  /// <param name="this">The format string.</param>
  /// <param name="parameters">The parameters to use for formatting.</param>
  /// <returns>A formatted string.</returns>
  /// <example>
  /// <code>
  /// string format = "Hello, {0}! Today is {1:dddd}.";
  /// string result = format.FormatWith("Alice", DateTime.Now);
  /// Console.WriteLine(result); // Output: Hello, Alice! Today is [Day of the week].
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string FormatWith(this string @this, params object[] parameters) {
    Against.ThisIsNull(@this);

    return string.Format(@this, parameters);
  }

  /// <summary>
  /// Uses the string as a format string, allowing an extended syntax to get the fields, e.g., {FieldName:FieldFormat}.
  /// </summary>
  /// <param name="this">The format string.</param>
  /// <param name="fields">The fields to use for formatting, represented as a collection of key-value pairs.</param>
  /// <param name="comparer">(Optional: defaults to <see langword="null"/>) The comparer to use for field name comparisons. If <see langword="null"/>, the default equality comparer is used.</param>
  /// <returns>A formatted string.</returns>
  /// <example>
  /// <code>
  /// string format = "Hello, {Name}! Today is {Day:dddd}.";
  /// var fields = new List&lt;KeyValuePair&lt;string, object&gt;&gt;
  /// {
  ///     new KeyValuePair&lt;string, object&gt;("Name", "Alice"),
  ///     new KeyValuePair&lt;string, object&gt;("Day", DateTime.Now)
  /// };
  /// string result = format.FormatWithEx(fields);
  /// Console.WriteLine(result); // Output: Hello, Alice! Today is [Day of the week].
  /// </code>
  /// </example>
  public static string FormatWithEx(this string @this, IEnumerable<KeyValuePair<string, object>> fields, IEqualityComparer<string> comparer = null) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(fields);

    var fieldCache = fields.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, comparer);
    return FormatWithEx(@this, f => fieldCache.TryGetValue(f, out var result) ? result : null);
  }

  /// <summary>
  /// Uses the string as a format string, allowing an extended syntax to get the fields, e.g., {FieldName:FieldFormat}.
  /// </summary>
  /// <param name="this">The format string.</param>
  /// <param name="comparer">The comparer to use for field name comparisons.</param>
  /// <param name="fields">The fields to use for formatting, represented as an array of key-value pairs.</param>
  /// <returns>A formatted string.</returns>
  /// <example>
  /// <code>
  /// string format = "Hello, {Name}! Today is {Day:dddd}.";
  /// var fields = new KeyValuePair&lt;string, object&gt;[]
  /// {
  ///     new KeyValuePair&lt;string, object&gt;("Name", "Alice"),
  ///     new KeyValuePair&lt;string, object&gt;("Day", DateTime.Now)
  /// };
  /// string result = format.FormatWithEx(StringComparer.OrdinalIgnoreCase, fields);
  /// Console.WriteLine(result); // Output: Hello, Alice! Today is [Day of the week].
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string FormatWithEx(this string @this, IEqualityComparer<string> comparer, params KeyValuePair<string, object>[] fields) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(fields);

    return FormatWithEx(@this, fields, comparer);
  }

  /// <summary>
  /// Uses the string as a format string, allowing an extended syntax to get the fields, e.g., {FieldName:FieldFormat}.
  /// </summary>
  /// <param name="this">The format string.</param>
  /// <param name="fields">The fields to use for formatting, represented as an array of key-value pairs.</param>
  /// <returns>A formatted string.</returns>
  /// <example>
  /// <code>
  /// string format = "Hello, {Name}! Today is {Day:dddd}.";
  /// var fields = new KeyValuePair&lt;string, object&gt;[]
  /// {
  ///     new KeyValuePair&lt;string, object&gt;("Name", "Alice"),
  ///     new KeyValuePair&lt;string, object&gt;("Day", DateTime.Now)
  /// };
  /// string result = format.FormatWithEx(fields);
  /// Console.WriteLine(result); // Output: Hello, Alice! Today is [Day of the week].
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string FormatWithEx(this string @this, params KeyValuePair<string, object>[] fields) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(fields);

    return FormatWithEx(@this, fields, null);
  }

  /// <summary>
  /// Uses the string as a format string, allowing an extended syntax to get the fields, e.g., {FieldName:FieldFormat}.
  /// </summary>
  /// <param name="this">The format string.</param>
  /// <param name="fields">The fields to use for formatting, represented as a dictionary of key-value pairs where the key is the field name and the value is the field value.</param>
  /// <returns>A formatted string.</returns>
  /// <example>
  /// <code>
  /// string format = "Hello, {Name}! Today is {Day:dddd}.";
  /// var fields = new Dictionary&lt;string, string&gt;
  /// {
  ///     { "Name", "Alice" },
  ///     { "Day", DateTime.Now.ToString("dddd") }
  /// };
  /// string result = format.FormatWithEx(fields);
  /// Console.WriteLine(result); // Output: Hello, Alice! Today is [Day of the week].
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string FormatWithEx(this string @this, IDictionary<string, string> fields) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(fields);

    return FormatWithEx(@this, f => fields[f]);
  }

  /// <summary>
  /// Uses the string as a format string, allowing an extended syntax to get the fields, e.g., {FieldName:FieldFormat}.
  /// </summary>
  /// <param name="this">The format string.</param>
  /// <param name="fields">The fields to use for formatting, represented as a <see cref="System.Collections.Hashtable"/> where the key is the field name and the value is the field value.</param>
  /// <returns>A formatted string.</returns>
  /// <example>
  /// <code>
  /// string format = "Hello, {Name}! Today is {Day:dddd}.";
  /// var fields = new Hashtable
  /// {
  ///     { "Name", "Alice" },
  ///     { "Day", DateTime.Now.ToString("dddd") }
  /// };
  /// string result = format.FormatWithEx(fields);
  /// Console.WriteLine(result); // Output: Hello, Alice! Today is [Day of the week].
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string FormatWithEx(this string @this, Hashtable fields) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(fields);

    return FormatWithEx(@this, f => fields[f]);
  }

  /// <summary>
  /// Uses the string as a format string, allowing an extended syntax to get the fields, e.g., {FieldName:FieldFormat}, using the properties of the specified object.
  /// </summary>
  /// <param name="this">The format string.</param>
  /// <param name="object">The source object to get the data from using properties of the same name as the format fields.</param>
  /// <typeparam name="T">The type of the source object.</typeparam>
  /// <returns>A formatted string with values replaced by the corresponding properties of the source object.</returns>
  /// <example>
  /// <code>
  /// var person = new
  /// {
  ///     Name = "Alice",
  ///     Day = DateTime.Now.ToString("dddd")
  /// };
  /// string format = "Hello, {Name}! Today is {Day}.";
  /// string result = format.FormatWithObject(person);
  /// Console.WriteLine(result); // Output: Hello, Alice! Today is [Day of the week].
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string FormatWithObject<T>(this string @this, T @object) {
    if (@object is null)
      return @this.FormatWithEx(_ => null);

    var cache = new Dictionary<string, Func<object>>();
    var type = @object.GetType();

    return @this.FormatWithEx(Getter);

    object Getter(string field) {
      if (cache.TryGetValue(field, out var call))
        return call();

      var method = type.GetProperty(field)?.GetGetMethod();
      if (method == null || method.GetParameters().Length != 0) {
        cache.Add(field, () => null);
        return null;
      }

      var returnType = method.ReturnType;

      // short-cut: non-generic return values
      if (!returnType.IsGenericType) {
        call = () => method.Invoke(@object, null);
        cache.Add(field, call);
        return call();
      }

      var genericType = returnType.GetGenericTypeDefinition();
      if (genericType == typeof(Func<,>)) {
        // support for Func<string,>
        var genericArguments = returnType.GetGenericArguments();
        if (genericArguments.Length == 2 && genericArguments[0] == typeof(string))
          call = () => {
            var func = (Delegate)method.Invoke(@object, null);
            return func.DynamicInvoke(field);
          };
        else /* something else */
          call = () => method.Invoke(@object, null);
      } else if (genericType == typeof(Func<>))

        // support for Func<>
        call = () => {
          var func = (Delegate)method.Invoke(@object, null);
          return func.DynamicInvoke();
        };
      else /* non Func<,> / Func<> */
        call = () => method.Invoke(@object, null);

      cache.Add(field, call);
      return call();
    }
  }

  /// <summary>
  /// Uses the string as a format string, allowing an extended syntax to get the fields, e.g., {FieldName:FieldFormat}, using a custom field getter function.
  /// </summary>
  /// <param name="this">The format string.</param>
  /// <param name="fieldGetter">The function used to get the field values. The function takes a field name as input and returns the corresponding field value.</param>
  /// <param name="passFieldFormatToGetter">(Optional: defaults to <c>false</c>) If set to <c>true</c>, passes the field format to the getter function.</param>
  /// <returns>A formatted string.</returns>
  /// <example>
  /// <code>
  /// string format = "Hello, {Name}! Today is {Day:dddd}.";
  /// Func&lt;string, object&gt; fieldGetter = fieldName =>
  /// {
  ///     return fieldName switch
  ///     {
  ///         "Name" => "Alice",
  ///         "Day" => DateTime.Now,
  ///         _ => null
  ///     };
  /// };
  /// string result = format.FormatWithEx(fieldGetter);
  /// Console.WriteLine(result); // Output: Hello, Alice! Today is [Day of the week].
  /// </code>
  /// </example>
  public static string FormatWithEx(this string @this, Func<string, object> fieldGetter, bool passFieldFormatToGetter = false) {
    if (@this == null)
      throw new NullReferenceException();
    if (fieldGetter == null)
      throw new ArgumentNullException(nameof(fieldGetter));

    var length = @this.Length;

    // we will store parts of the newly generated string here
    StringBuilder result = new(length);

    var i = 0;
    var lastStartPos = 0;
    var isInField = false;

    // looping through all characters breaking it up into parts that need to be get using the field getter
    // and parts that simply need to be copied
    while (i < length) {
      var current = @this[i++];
      var next = i < length ? @this[i].ToString() : null;

      var fieldContentLength = i - lastStartPos - 1;
      if (isInField) {
        // we're currently reading a field
        if (current != '}')
          continue;

        // end of field found, pass field description to field getter
        isInField = false;
        var fieldContent = @this.Substring(lastStartPos, fieldContentLength);
        lastStartPos = i;

        int formatStartIndex;
        if (passFieldFormatToGetter || (formatStartIndex = fieldContent.IndexOf(':')) < 0)
          result.Append(fieldGetter(fieldContent));
        else {
          var fieldName = fieldContent[..formatStartIndex];
          var fieldFormat = fieldContent[(formatStartIndex + 1)..];
          result.Append(string.Format($"{{0:{fieldFormat}}}", fieldGetter(fieldName)));
        }
      } else
        // we're currently copying
        switch (current) {
          case '{': {
            // copy what we've already got
            var textContent = @this.Substring(lastStartPos, fieldContentLength);
            lastStartPos = i;
            result.Append(textContent);

            // filter out double brackets
            if (next is "{")
              // skip the following bracket
              ++i;
            else
              // field start found, switch mode
              isInField = true;

            break;
          }
          case '}' when next is "}": {
            // copy what we've already got
            var textContent = @this.Substring(lastStartPos, fieldContentLength);
            lastStartPos = i;
            result.Append(textContent);

            // skip double brackets
            ++i;
            break;
          }
        }
    }

    var remainingContent = @this[lastStartPos..];
    result.Append(remainingContent);
    return result.ToString();
  }

  #endregion

  /// <summary>
  /// Uses the string as a regular expression.
  /// </summary>
  /// <param name="this">The input string to be treated as a regular expression pattern.</param>
  /// <returns>A new instance of <see cref="Regex"/> if the input string is not <see langword="null"/>; otherwise, <see langword="null"/>.</returns>
  /// <example>
  /// <code>
  /// string pattern = @"\d+";
  /// Regex regex = pattern.AsRegularExpression();
  /// bool isMatch = regex.IsMatch("12345");
  /// Console.WriteLine(isMatch); // Output: True
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Regex AsRegularExpression(this string @this) => @this == null ? null : new Regex(@this);

  /// <summary>
  /// Uses the string as a regular expression with the specified options.
  /// </summary>
  /// <param name="this">The input string to be treated as a regular expression pattern.</param>
  /// <param name="options">The options to use when creating the <see cref="Regex"/> instance.</param>
  /// <returns>A new instance of <see cref="Regex"/> if the input string is not <see langword="null"/>; otherwise, <see langword="null"/>.</returns>
  /// <example>
  /// <code>
  /// string pattern = @"\d+";
  /// Regex regex = pattern.AsRegularExpression(RegexOptions.IgnoreCase);
  /// bool isMatch = regex.IsMatch("12345");
  /// Console.WriteLine(isMatch); // Output: True
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Regex AsRegularExpression(this string @this, RegexOptions options) => @this == null ? null : new Regex(@this, options);

  /// <summary>
  /// Replaces multiple substrings in the current string instance with the specified replacement values.
  /// </summary>
  /// <param name="this">The original string on which the replacements will be made.</param>
  /// <param name="replacements">An array of <see cref="KeyValuePair{TKey, TValue}"/> where the key is the substring to be replaced and the value is the replacement object.</param>
  /// <returns>A new string with all specified replacements made.</returns>
  /// <example>
  /// <code>
  /// string original = "Hello, {name}! Today is {day}.";
  /// var replacements = new KeyValuePair&lt;string, object&gt;[]
  /// {
  ///     new KeyValuePair&lt;string, object&gt;("{name}", "Alice"),
  ///     new KeyValuePair&lt;string, object&gt;("{day}", "Monday")
  /// };
  /// string result = original.MultipleReplace(replacements);
  /// Console.WriteLine(result); // Output: Hello, Alice! Today is Monday.
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string MultipleReplace(this string @this, params KeyValuePair<string, object>[] replacements) => MultipleReplace(@this, (IEnumerable<KeyValuePair<string, object>>)replacements);

  /// <summary>
  /// Replaces multiple substrings in the current string instance with a single specified replacement value.
  /// </summary>
  /// <param name="this">The original string on which the replacements will be made.</param>
  /// <param name="replacement">The string to replace all specified substrings with.</param>
  /// <param name="toReplace">An array of strings that will be replaced by the <paramref name="replacement"/> value.</param>
  /// <returns>A new string with all specified substrings replaced by the replacement value.</returns>
  /// <example>
  /// <code>
  /// string original = "Hello, world! Goodbye, world!";
  /// string[] toReplace = { "world", "Goodbye" };
  /// string result = original.MultipleReplace("everyone", toReplace);
  /// Console.WriteLine(result); // Output: Hello, everyone! everyone, everyone!
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string MultipleReplace(this string @this, string replacement, string[] toReplace) {
    Against.ArgumentIsNull(toReplace);

    return MultipleReplace(@this, toReplace.Select(s => new KeyValuePair<string, string>(s, replacement)));
  }

  /// <summary>
  /// Replaces multiple substrings in the current string instance with a single specified replacement value, including two initial specified substrings.
  /// </summary>
  /// <param name="this">The original string on which the replacements will be made.</param>
  /// <param name="replacement">The string to replace all specified substrings with.</param>
  /// <param name="needle1">The first substring to be replaced by the <paramref name="replacement"/> value.</param>
  /// <param name="needle2">The second substring to be replaced by the <paramref name="replacement"/> value.</param>
  /// <param name="toReplace">An array of additional strings that will be replaced by the <paramref name="replacement"/> value.</param>
  /// <returns>A new string with all specified substrings replaced by the replacement value.</returns>
  /// <example>
  /// <code>
  /// string original = "Hello, world! Goodbye, world!";
  /// string result = original.MultipleReplace("everyone", "world", "Goodbye");
  /// Console.WriteLine(result); // Output: Hello, everyone! everyone, everyone!
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string MultipleReplace(this string @this, string replacement, string needle1, string needle2, params string[] toReplace) => MultipleReplace(@this, toReplace.Select(s => new KeyValuePair<string, string>(s, replacement)).Prepend(new KeyValuePair<string, string>(needle1, replacement)).Prepend(new KeyValuePair<string, string>(needle2, replacement)));

  /// <summary>
  /// Replaces multiple substrings in the current string instance with their corresponding replacement values.
  /// </summary>
  /// <param name="this">The original string on which the replacements will be made.</param>
  /// <param name="replacements">An array of <see cref="System.Collections.Generic.KeyValuePair{string, string}"/> where the key is the substring to be replaced and the value is the replacement string.</param>
  /// <returns>A new string with all specified replacements made.</returns>
  /// <example>
  /// <code>
  /// string original = "Hello, {name}! Today is {day}.";
  /// var replacements = new KeyValuePair&lt;string, string&gt;[]
  /// {
  ///     new KeyValuePair&lt;string, string&gt;("{name}", "Alice"),
  ///     new KeyValuePair&lt;string, string&gt;("{day}", "Monday")
  /// };
  /// string result = original.MultipleReplace(replacements);
  /// Console.WriteLine(result); // Output: Hello, Alice! Today is Monday.
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string MultipleReplace(this string @this, params KeyValuePair<string, string>[] replacements) => MultipleReplace(@this, (IEnumerable<KeyValuePair<string, string>>)replacements);

  /// <summary>
  /// Replaces multiple substrings in the current string instance with their corresponding replacement values.
  /// </summary>
  /// <param name="this">The original string on which the replacements will be made.</param>
  /// <param name="replacements">An enumerable collection of <see cref="KeyValuePair{string, string}"/> where the key is the substring to be replaced and the value is the replacement string.</param>
  /// <returns>A new string with all specified replacements made.</returns>
  /// <example>
  /// <code>
  /// string original = "Hello, {name}! Today is {day}.";
  /// var replacements = new List&lt;KeyValuePair&lt;string, string&gt;&gt;
  /// {
  ///     new KeyValuePair&lt;string, string&gt;("{name}", "Alice"),
  ///     new KeyValuePair&lt;string, string&gt;("{day}", "Monday")
  /// };
  /// string result = original.MultipleReplace(replacements);
  /// Console.WriteLine(result); // Output: Hello, Alice! Today is Monday.
  /// </code>
  /// </example>
  public static string MultipleReplace(this string @this, IEnumerable<KeyValuePair<string, string>> replacements) => MultipleReplace(@this, replacements?.Select(kvp => new KeyValuePair<string, object>(kvp.Key, kvp.Value)));

  /// <summary>
  /// Replaces multiple substrings in the current string instance with their corresponding replacement values.
  /// </summary>
  /// <param name="this">The original string on which the replacements will be made.</param>
  /// <param name="replacements">An enumerable collection of <see cref="System.Collections.Generic.KeyValuePair{string, object}"/> where the key is the substring to be replaced and the value is the replacement object.</param>
  /// <returns>A new string with all specified replacements made.</returns>
  /// <example>
  /// <code>
  /// string original = "Hello, {name}! Today is {day}.";
  /// var replacements = new List&lt;KeyValuePair&lt;string, object&gt;&gt;
  /// {
  ///     new KeyValuePair&lt;string, object&gt;("{name}", "Alice"),
  ///     new KeyValuePair&lt;string, object&gt;("{day}", "Monday")
  /// };
  /// string result = original.MultipleReplace(replacements);
  /// Console.WriteLine(result); // Output: Hello, Alice! Today is Monday.
  /// </code>
  /// </example>
  public static string MultipleReplace(this string @this, IEnumerable<KeyValuePair<string, object>> replacements) {
    if (@this.IsNullOrEmpty() || replacements == null)
      return @this;
    
    var list = replacements.OrderByDescending(kvp => kvp.Key.Length).ToArray();
    var length = @this.Length;
    StringBuilder result = new(length);
    for (var i = 0; i < length; ++i) {
      var found = false;
      foreach (var kvp in list) {
        var keyLength = kvp.Key.Length;
        if (i + keyLength > length)
          continue;

        var part = @this.Substring(i, keyLength);
        if (kvp.Key != part)
          continue;

        result.Append(kvp.Value);
        found = true;

        //support for string replacements greater than 1 char
        i += keyLength - 1;
        break;
      }

      if (!found)
        result.Append(@this[i]);
    }

    return result.ToString();
  }

  /// <summary>
  /// Replaces all occurrences of the specified regular expression pattern in the current string instance with the specified replacement string.
  /// </summary>
  /// <param name="this">The original string on which the replacements will be made.</param>
  /// <param name="regex">The regular expression pattern to match in the input string.</param>
  /// <param name="newValue">(Optional: defaults to <see langword="null"/>) The replacement string. If <see langword="null"/>, the matched substrings are replaced with an empty string.</param>
  /// <param name="regexOptions">(Optional: defaults to <see cref="System.Text.RegularExpressions.RegexOptions.None"/>) A bitwise combination of the enumeration values that provide options for matching.</param>
  /// <returns>A new string with all occurrences of the specified regular expression pattern replaced by the <paramref name="newValue"/>.</returns>
  /// <example>
  /// <code>
  /// string original = "Hello, world! Welcome to the world of programming.";
  /// string result = original.ReplaceRegex(@"\bworld\b", "universe");
  /// Console.WriteLine(result); // Output: Hello, universe! Welcome to the universe of programming.
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ReplaceRegex(this string @this, string regex, string newValue = null, RegexOptions regexOptions = RegexOptions.None) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrEmpty(regex);

    return new Regex(regex, regexOptions).Replace(@this, newValue ?? string.Empty);
  }

  /// <summary>
  /// Replaces all occurrences of the specified regular expression pattern in the current string instance with the specified replacement string.
  /// </summary>
  /// <param name="this">The original string on which the replacements will be made.</param>
  /// <param name="regex">The regular expression to match in the input string.</param>
  /// <param name="newValue">The replacement string.</param>
  /// <returns>A new string with all occurrences of the specified regular expression pattern replaced by the <paramref name="newValue"/>.</returns>
  /// <example>
  /// <code>
  /// string original = "Hello, world! Welcome to the world of programming.";
  /// Regex regex = new Regex(@"\bworld\b");
  /// string result = original.Replace(regex, "universe");
  /// Console.WriteLine(result); // Output: Hello, universe! Welcome to the universe of programming.
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string Replace(this string @this, Regex regex, string newValue) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(regex);

    return regex.Replace(@this, newValue);
  }

  /// <summary>
  /// Replaces a specified number of occurrences of a specified string in the current string instance with another specified string, using the specified comparison rules.
  /// </summary>
  /// <param name="this">The original string on which the replacements will be made.</param>
  /// <param name="oldValue">The string to be replaced.</param>
  /// <param name="newValue">The string to replace all occurrences of <paramref name="oldValue"/>.</param>
  /// <param name="count">The maximum number of occurrences to replace.</param>
  /// <param name="comparison">(Optional: defaults to <see cref="StringComparison.CurrentCulture"/>) The comparison rules to use when matching the <paramref name="oldValue"/>.</param>
  /// <returns>A new string with the specified number of occurrences of the <paramref name="oldValue"/> replaced by the <paramref name="newValue"/>.</returns>
  /// <example>
  /// <code>
  /// string original = "Hello, world! Welcome to the world of programming.";
  /// string result = original.Replace("world", "universe", 1, StringComparison.OrdinalIgnoreCase);
  /// Console.WriteLine(result); // Output: Hello, universe! Welcome to the world of programming.
  /// </code>
  /// </example>
  public static string Replace(this string @this, string oldValue, string newValue, int count, StringComparison comparison = StringComparison.CurrentCulture) {
    Against.UnknownEnumValues(comparison);

    if (@this == null || oldValue == null || count <= 0)
      return @this;

    newValue ??= string.Empty;
    var result = @this;

    var removedLength = oldValue.Length;
    var newLength = newValue.Length;

    var pos = 0;
    for (var i = count; i > 0;) {
      --i;

      var n = result.IndexOf(oldValue, pos, comparison);
      if (n < 0)
        break;

      if (n == 0)
        result = newValue + result[removedLength..];
      else
        result = result[..n] + newValue + result[(n + removedLength)..];

      pos = n + newLength;
    }

    return result;
  }

  #region Upper/Lower

  /// <summary>
  /// Converts the first character of the current string instance to uppercase, using the specified culture.
  /// </summary>
  /// <param name="this">The original string whose first character will be converted to uppercase.</param>
  /// <param name="culture">(Optional: defaults to <see langword="null"/>) The culture-specific formatting information to use. If <see langword="null"/>, the current culture is used.</param>
  /// <returns>A new string with the first character converted to uppercase. If the string is empty, the original string is returned.</returns>
  /// <example>
  /// <code>
  /// string original = "hello, world!";
  /// string result = original.UpperFirst();
  /// Console.WriteLine(result); // Output: Hello, world!
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string UpperFirst(this string @this, CultureInfo culture = null) {
    Against.ThisIsNull(@this);

    var length = @this.Length;
    string result;
    if (length != 0) {
      var firstChar = @this[0];
      var firstCharUpper = culture == null ? char.ToUpper(firstChar) : char.ToUpper(firstChar, culture);
      if (firstCharUpper != firstChar) {
#if SUPPORTS_SPAN
        result = new(@this);
#else
        result = string.Copy(@this);
#endif
        unsafe {
          fixed (char* ptrResult = result)
            *ptrResult = firstCharUpper;
        }
      } else
        result = @this;
    } else
      result = string.Empty;

    return result;
  }

  /// <summary>
  /// Converts the first character of the current string instance to uppercase, using the invariant culture.
  /// </summary>
  /// <param name="this">The original string whose first character will be converted to uppercase.</param>
  /// <returns>A new string with the first character converted to uppercase. If the string is empty, the original string is returned.</returns>
  /// <example>
  /// <code>
  /// string original = "hello, world!";
  /// string result = original.UpperFirstInvariant();
  /// Console.WriteLine(result); // Output: Hello, world!
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string UpperFirstInvariant(this string @this) {
    Against.ThisIsNull(@this);

    var length = @this.Length;
    string result;
    if (length != 0) {
      var firstChar = @this[0];
      var firstCharUpper = char.ToUpperInvariant(firstChar);
      if (firstCharUpper != firstChar) {
#if SUPPORTS_SPAN
        result = new(@this);
#else
        result = string.Copy(@this);
#endif
        unsafe {
          fixed (char* ptrResult = result)
            *ptrResult = firstCharUpper;
        }
      } else
        result = @this;
    } else
      result = string.Empty;

    return result;
  }

  /// <summary>
  /// Converts the first character of the current string instance to lowercase, using the specified culture.
  /// </summary>
  /// <param name="this">The original string whose first character will be converted to lowercase.</param>
  /// <param name="culture">(Optional: defaults to <see langword="null"/>) The culture-specific formatting information to use. If <see langword="null"/>, the current culture is used.</param>
  /// <returns>A new string with the first character converted to lowercase. If the string is empty, the original string is returned.</returns>
  /// <example>
  /// <code>
  /// string original = "Hello, World!";
  /// string result = original.LowerFirst();
  /// Console.WriteLine(result); // Output: hello, World!
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string LowerFirst(this string @this, CultureInfo culture = null) {
    Against.ThisIsNull(@this);

    var length = @this.Length;
    string result;
    if (length != 0) {
      var firstChar = @this[0];
      var firstCharLower = culture == null ? char.ToLower(firstChar) : char.ToLower(firstChar, culture);
      if (firstCharLower != firstChar) {
#if SUPPORTS_SPAN
        result = new(@this);
#else
        result = string.Copy(@this);
#endif
        unsafe {
          fixed (char* ptrResult = result)
            *ptrResult = firstCharLower;
        }
      } else
        result = @this;
    } else
      result = string.Empty;

    return result;
  }

  /// <summary>
  /// Converts the first character of the current string instance to lowercase, using the invariant culture.
  /// </summary>
  /// <param name="this">The original string whose first character will be converted to lowercase.</param>
  /// <returns>A new string with the first character converted to lowercase. If the string is empty, the original string is returned.</returns>
  /// <example>
  /// <code>
  /// string original = "Hello, World!";
  /// string result = original.LowerFirstInvariant();
  /// Console.WriteLine(result); // Output: hello, World!
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string LowerFirstInvariant(this string @this) {
    Against.ThisIsNull(@this);

    var length = @this.Length;
    string result;
    if (length != 0) {
      var firstChar = @this[0];
      var firstCharLower = char.ToLowerInvariant(firstChar);
      if (firstCharLower != firstChar) {
#if SUPPORTS_SPAN
        result = new(@this);
#else
        result = string.Copy(@this);
#endif
        unsafe {
          fixed (char* ptrResult = result)
            *ptrResult = firstCharLower;
        }
      } else
        result = @this;
    } else
      result = string.Empty;

    return result;
  }

  #endregion

  #region Splitting

  /// <summary>
  ///   Splits a string into equal length parts.
  /// </summary>
  /// <param name="this">This string.</param>
  /// <param name="length">The number of chars each part should have</param>
  /// <returns>An enumeration of string parts</returns>
  public static IEnumerable<string> Split(this string @this, int length) {
    Against.ThisIsNull(@this);
    Against.NegativeValuesAndZero(length);

    for (var i = 0; i < @this.Length; i += length)
      yield return @this.Substring(i, length);
  }
  
  /// <summary>
  ///   Splits the specified string using a regular expression.
  /// </summary>
  /// <param name="this">This String.</param>
  /// <param name="regex">The regex to use.</param>
  /// <returns>The parts.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string[] Split(this string @this, Regex regex) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(regex);

    return regex.Split(@this);
  }
  
  #endregion

  /// <summary>
  ///   Transforms the given connection string into a linq2sql compatible one by removing the driver.
  /// </summary>
  /// <param name="this">This ConnectionString.</param>
  /// <returns>The transformed result.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ToLinq2SqlConnectionString(this string @this) {
    Against.ThisIsNull(@this);

    Regex regex = new(@"Driver\s*=.*?(;|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
    return regex.Replace(@this, string.Empty);
  }

  /// <summary>
  ///   Escapes the string to be used as sql data.
  /// </summary>
  /// <param name="this">This String.</param>
  /// <returns></returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string MsSqlDataEscape(this object @this) => @this == null ? "NULL" : "'" + string.Format(CultureInfo.InvariantCulture, "{0}", @this).Replace("'", "''") + "'";

  /// <summary>
  ///   Escapes the string to be used as sql identifiers eg. table or column names.
  /// </summary>
  /// <param name="this">This String.</param>
  /// <returns></returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string MsSqlIdentifierEscape(this string @this) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrWhiteSpace(@this);

    return $"[{@this.Replace("]", "]]")}]";
  }

  #region StartsWith/StartsNotWith

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsWith(this string @this, char what, StringComparer comparer) {
    Against.ThisIsNull(@this);

    return comparer?.Equals(@this.Length > 0 ? @this[0] + string.Empty : string.Empty, what + string.Empty) ?? @this.StartsWith(what);
  }

  /// <summary>
  ///   Checks whether the given string starts with the specified character.
  /// </summary>
  /// <param name="this">This String.</param>
  /// <param name="value">The value.</param>
  /// <param name="stringComparison">The string comparison.</param>
  /// <returns>
  ///   <c>true</c> if the string starts with the given character; otherwise, <c>false</c>.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsWith(this string @this, char value, StringComparison stringComparison = StringComparison.CurrentCulture) {
    Against.ThisIsNull(@this);

    return @this.Length > 0 && string.Equals(@this[0].ToString(), value.ToString(), stringComparison);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsWith(this string @this, string what, StringComparer comparer) {
    Against.ThisIsNull(@this);
    if (what == null)
      return false;

    return comparer?.Equals(@this[..what.Length], what) ?? @this.StartsWith(what);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsWith(this string @this, string what, int index) {
    Against.ThisIsNull(@this);
    Against.IndexBelowZero(index);

    return @this.AsSpan(index).StartsWith(what.AsSpan());
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsWith(this string @this, string what, int index, StringComparer comparer) {
    Against.ThisIsNull(@this);
    Against.IndexBelowZero(index);
    Against.ArgumentIsNull(comparer);
    if (index + what.Length > @this.Length)
      return false;

    var substring = @this.Substring(index, what.Length);
    return comparer.Compare(substring, what) == 0;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsWith(this string @this, string what, int index, StringComparison comparison) {
    Against.ThisIsNull(@this);
    Against.IndexBelowZero(index);
    Against.UnknownEnumValues(comparison);
    if (index + what.Length > @this.Length)
      return false;

    return @this.AsSpan(index).StartsWith(what.AsSpan(), comparison);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsNotWith(this string @this, char what, StringComparer comparer) => !StartsWith(@this, what, comparer);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsNotWith(this string @this, string what, StringComparer comparer) => !StartsWith(@this, what, comparer);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsNotWith(this string @this, char value, StringComparison stringComparison = StringComparison.CurrentCulture) => !@this.StartsWith(value, stringComparison);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsNotWith(this string @this, string what, int index) => !StartsWith(@this, what, index);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsNotWith(this string @this, string what, int index, StringComparer comparer) => !StartsWith(@this, what, index, comparer);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsNotWith(this string @this, string what, int index, StringComparison comparison) => !StartsWith(@this, what, index, comparison);

  /// <summary>
  ///   Checks whether the given string starts not with the specified text.
  /// </summary>
  /// <param name="this">This String.</param>
  /// <param name="value">The value.</param>
  /// <param name="stringComparison">The string comparison.</param>
  /// <returns></returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsNotWith(this string @this, string value, StringComparison stringComparison = StringComparison.CurrentCulture) => !@this.StartsWith(value, stringComparison);

  #endregion

  #region EndsWith/EndsNotWith

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsWith(this string @this, char value, StringComparer comparer) {
    Against.ThisIsNull(@this);
    return comparer?.Equals(@this.Length > 0 ? @this[^1].ToString() : string.Empty, value.ToString()) ?? @this.EndsWith(value);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsWith(this string @this, string what, StringComparer comparer) {
    Against.ThisIsNull(@this);
    if (what == null)
      return false;

    return comparer?.Equals(@this[Math.Max(0, @this.Length - what.Length)..], what) ?? @this.EndsWith(what);
  }

  /// <summary>
  ///   Checks whether the given string ends with the specified character.
  /// </summary>
  /// <param name="this">This String.</param>
  /// <param name="value">The value.</param>
  /// <param name="stringComparison">The string comparison.</param>
  /// <returns>
  ///   <c>true</c> if the string ends with the given character; otherwise, <c>false</c>.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsWith(this string @this, char value, StringComparison stringComparison = StringComparison.CurrentCulture) {
    Against.ThisIsNull(@this);

    return @this.Length > 0 && string.Equals(@this[^1] + string.Empty, value + string.Empty, stringComparison);
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsNotWith(this string @this, char what, StringComparer comparer) => !EndsWith(@this, what, comparer);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsNotWith(this string @this, string what, StringComparer comparer) => !EndsWith(@this, what, comparer);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsNotWith(this string @this, char value, StringComparison stringComparison = StringComparison.CurrentCulture) => !@this.EndsWith(value, stringComparison);

  /// <summary>
  ///   Checks whether the given string ends not with the specified text.
  /// </summary>
  /// <param name="this">This String.</param>
  /// <param name="value">The value.</param>
  /// <param name="stringComparison">The string comparison.</param>
  /// <returns></returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsNotWith(this string @this, string value, StringComparison stringComparison = StringComparison.CurrentCulture) => !@this.EndsWith(value, stringComparison);

  #endregion

  #region StartsWithAny/StartsNotWithAny

  /// <summary>
  ///   Checks if the <see cref="string" /> starts with any from the given list.
  /// </summary>
  /// <param name="this">This <see cref="string" />.</param>
  /// <param name="values">The values to compare to.</param>
  /// <returns>
  ///   <see langword="true" /> if there is at least one <see cref="string" /> in the list that matches the start;
  ///   otherwise, <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsWithAny(this string @this, params string[] values) => StartsWithAny(@this, StringComparison.CurrentCulture, values);

  /// <summary>
  ///   Checks if the <see cref="string" /> starts with any from the given list.
  /// </summary>
  /// <param name="this">This <see cref="string" />.</param>
  /// <param name="stringComparison">The mode to use for comparisons</param>
  /// <param name="values">The values to compare to.</param>
  /// <returns>
  ///   <see langword="true" /> if there is at least one <see cref="string" /> in the list that matches the start;
  ///   otherwise, <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsWithAny(this string @this, StringComparison stringComparison, params string[] values) => StartsWithAny(@this, values, stringComparison);

  /// <summary>
  ///   Checks if the <see cref="string" /> starts with any from the given list.
  /// </summary>
  /// <param name="this">This <see cref="string" />.</param>
  /// <param name="comparer">The <see cref="StringComparer" /> to use for comparisons</param>
  /// <param name="values">The values to compare to.</param>
  /// <returns>
  ///   <see langword="true" /> if there is at least one <see cref="string" /> in the list that matches the start;
  ///   otherwise, <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsWithAny(this string @this, StringComparer comparer, params string[] values) => StartsWithAny(@this, values, comparer);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsWithAny(this string @this, IEnumerable<string> values) => StartsWithAny(@this, values, StringComparison.CurrentCulture);

  /// <summary>
  ///   Checks if the <see cref="string" /> starts with any from the given list.
  /// </summary>
  /// <param name="this">This <see cref="string" />.</param>
  /// <param name="values">The values to compare to.</param>
  /// <param name="stringComparison">The mode to use for comparisons</param>
  /// <returns>
  ///   <see langword="true" /> if there is at least one <see cref="string" /> in the list that matches the start;
  ///   otherwise, <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsWithAny(this string @this, IEnumerable<string> values, StringComparison stringComparison) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(values);

    return values.Any(s => @this.StartsWith(s ?? string.Empty, stringComparison));
  }

  /// <summary>
  ///   Checks if the <see cref="string" /> starts with any from the given list.
  /// </summary>
  /// <param name="this">This <see cref="string" />.</param>
  /// <param name="comparer">The <see cref="StringComparer" /> to use for comparisons</param>
  /// <param name="values">The values to compare to.</param>
  /// <returns>
  ///   <see langword="true" /> if there is at least one <see cref="string" /> in the list that matches the start;
  ///   otherwise, <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsWithAny(this string @this, IEnumerable<string> values, StringComparer comparer) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(values);

    return values.Any(s => @this.StartsWith(s ?? string.Empty, comparer));
  }

  /// <summary>
  ///   Checks if the <see cref="string" /> starts with any character from the given list.
  /// </summary>
  /// <param name="this">This <see cref="string" />.</param>
  /// <param name="values">The values to compare to.</param>
  /// <returns>
  ///   <see langword="true" /> if there is at least one <see cref="char" /> in the list that matches the start;
  ///   otherwise, <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsWithAny(this string @this, params char[] values) => StartsWithAny(@this, StringComparison.CurrentCulture, values);

  /// <summary>
  ///   Checks if the <see cref="string" /> starts with any character from the given list.
  /// </summary>
  /// <param name="this">This <see cref="string" />.</param>
  /// <param name="stringComparison">The mode to use for comparisons</param>
  /// <param name="values">The values to compare to.</param>
  /// <returns>
  ///   <see langword="true" /> if there is at least one <see cref="char" /> in the list that matches the start;
  ///   otherwise, <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsWithAny(this string @this, StringComparison stringComparison, params char[] values) => StartsWithAny(@this, values, stringComparison);

  /// <summary>
  ///   Checks if the <see cref="string" /> starts with any character from the given list.
  /// </summary>
  /// <param name="this">This <see cref="string" />.</param>
  /// <param name="comparer">The <see cref="StringComparer" /> to use for comparisons</param>
  /// <param name="values">The values to compare to.</param>
  /// <returns>
  ///   <see langword="true" /> if there is at least one <see cref="char" /> in the list that matches the start;
  ///   otherwise, <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsWithAny(this string @this, StringComparer comparer, params char[] values) => StartsWithAny(@this, values, comparer);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsWithAny(this string @this, IEnumerable<char> values) => StartsWithAny(@this, values, StringComparison.CurrentCulture);

  /// <summary>
  ///   Checks if the <see cref="string" /> starts with any character from the given list.
  /// </summary>
  /// <param name="this">This <see cref="string" />.</param>
  /// <param name="values">The values to compare to.</param>
  /// <param name="stringComparison">The mode to use for comparisons</param>
  /// <returns>
  ///   <see langword="true" /> if there is at least one <see cref="char" /> in the list that matches the start;
  ///   otherwise, <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsWithAny(this string @this, IEnumerable<char> values, StringComparison stringComparison) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(values);

    return values.Any(s => @this.StartsWith(s, stringComparison));
  }

  /// <summary>
  ///   Checks if the <see cref="string" /> starts with any character from the given list.
  /// </summary>
  /// <param name="this">This <see cref="string" />.</param>
  /// <param name="values">The values to compare to.</param>
  /// <param name="comparer">The <see cref="StringComparer" /> to use for comparisons</param>
  /// <returns>
  ///   <see langword="true" /> if there is at least one <see cref="char" /> in the list that matches the start;
  ///   otherwise, <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsWithAny(this string @this, IEnumerable<char> values, StringComparer comparer) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(values);

    return values.Any(s => @this.StartsWith(s, comparer));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsNotWithAny(this string @this, params string[] values) => !StartsWithAny(@this, values);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsNotWithAny(this string @this, StringComparison comparison, params string[] values) => !StartsWithAny(@this, comparison, values);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsNotWithAny(this string @this, StringComparer comparer, params string[] values) => !StartsWithAny(@this, comparer, values);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsNotWithAny(this string @this, params char[] values) => !StartsWithAny(@this, values);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsNotWithAny(this string @this, StringComparison comparison, params char[] values) => !StartsWithAny(@this, comparison, values);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsNotWithAny(this string @this, StringComparer comparer, params char[] values) => !StartsWithAny(@this, comparer, values);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsNotWithAny(this string @this, IEnumerable<string> values) => !StartsWithAny(@this, values);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsNotWithAny(this string @this, IEnumerable<string> values, StringComparison comparison) => !StartsWithAny(@this, values, comparison);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsNotWithAny(this string @this, IEnumerable<string> values, StringComparer comparer) => !StartsWithAny(@this, values, comparer);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsNotWithAny(this string @this, IEnumerable<char> values) => !StartsWithAny(@this, values);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsNotWithAny(this string @this, IEnumerable<char> values, StringComparison comparison) => !StartsWithAny(@this, values, comparison);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool StartsNotWithAny(this string @this, IEnumerable<char> values, StringComparer comparer) => !StartsWithAny(@this, values, comparer);

  #endregion

  #region EndsWithAny/EndsNotWithAny

  /// <summary>
  ///   Checks if the <see cref="string" /> ends with any from the given list.
  /// </summary>
  /// <param name="this">This <see cref="string" />.</param>
  /// <param name="values">The values to compare to.</param>
  /// <returns>
  ///   <see langword="true" /> if there is at least one <see cref="string" /> in the list that matches the end;
  ///   otherwise, <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsWithAny(this string @this, params string[] values) => EndsWithAny(@this, StringComparison.CurrentCulture, values);

  /// <summary>
  ///   Checks if the <see cref="string" /> ends with any from the given list.
  /// </summary>
  /// <param name="this">This <see cref="string" />.</param>
  /// <param name="stringComparison">The mode to use for comparisons</param>
  /// <param name="values">The values to compare to.</param>
  /// <returns>
  ///   <see langword="true" /> if there is at least one <see cref="string" /> in the list that matches the end;
  ///   otherwise, <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsWithAny(this string @this, StringComparison stringComparison, params string[] values) => EndsWithAny(@this, values, stringComparison);

  /// <summary>
  ///   Checks if the <see cref="string" /> ends with any from the given list.
  /// </summary>
  /// <param name="this">This <see cref="string" />.</param>
  /// <param name="comparer">The <see cref="StringComparer" /> to use for comparisons</param>
  /// <param name="values">The values to compare to.</param>
  /// <returns>
  ///   <see langword="true" /> if there is at least one <see cref="string" /> in the list that matches the end;
  ///   otherwise, <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsWithAny(this string @this, StringComparer comparer, params string[] values) => EndsWithAny(@this, values, comparer);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsWithAny(this string @this, IEnumerable<string> values) => EndsWithAny(@this, values, StringComparison.CurrentCulture);

  /// <summary>
  ///   Checks if the <see cref="string" /> ends with any from the given list.
  /// </summary>
  /// <param name="this">This <see cref="string" />.</param>
  /// <param name="values">The values to compare to.</param>
  /// <param name="stringComparison">The mode to use for comparisons</param>
  /// <returns>
  ///   <see langword="true" /> if there is at least one <see cref="string" /> in the list that matches the end;
  ///   otherwise, <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsWithAny(this string @this, IEnumerable<string> values, StringComparison stringComparison) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(values);

    return values.Any(s => @this.EndsWith(s ?? string.Empty, stringComparison));
  }

  /// <summary>
  ///   Checks if the <see cref="string" /> ends with any from the given list.
  /// </summary>
  /// <param name="this">This <see cref="string" />.</param>
  /// <param name="comparer">The <see cref="StringComparer" /> to use for comparisons</param>
  /// <param name="values">The values to compare to.</param>
  /// <returns>
  ///   <see langword="true" /> if there is at least one <see cref="string" /> in the list that matches the end;
  ///   otherwise, <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsWithAny(this string @this, IEnumerable<string> values, StringComparer comparer) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(values);

    return values.Any(s => @this.EndsWith(s ?? string.Empty, comparer));
  }

  /// <summary>
  ///   Checks if the <see cref="string" /> ends with any character from the given list.
  /// </summary>
  /// <param name="this">This <see cref="string" />.</param>
  /// <param name="values">The values to compare to.</param>
  /// <returns>
  ///   <see langword="true" /> if there is at least one <see cref="char" /> in the list that matches the end;
  ///   otherwise, <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsWithAny(this string @this, params char[] values) => EndsWithAny(@this, StringComparison.CurrentCulture, values);

  /// <summary>
  ///   Checks if the <see cref="string" /> ends with any character from the given list.
  /// </summary>
  /// <param name="this">This <see cref="string" />.</param>
  /// <param name="stringComparison">The mode to use for comparisons</param>
  /// <param name="values">The values to compare to.</param>
  /// <returns>
  ///   <see langword="true" /> if there is at least one <see cref="char" /> in the list that matches the end;
  ///   otherwise, <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsWithAny(this string @this, StringComparison stringComparison, params char[] values) => EndsWithAny(@this, values, stringComparison);

  /// <summary>
  ///   Checks if the <see cref="string" /> ends with any character from the given list.
  /// </summary>
  /// <param name="this">This <see cref="string" />.</param>
  /// <param name="comparer">The <see cref="StringComparer" /> to use for comparisons</param>
  /// <param name="values">The values to compare to.</param>
  /// <returns>
  ///   <see langword="true" /> if there is at least one <see cref="char" /> in the list that matches the end;
  ///   otherwise, <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsWithAny(this string @this, StringComparer comparer, params char[] values) => EndsWithAny(@this, values, comparer);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsWithAny(this string @this, IEnumerable<char> values) => EndsWithAny(@this, values, StringComparison.CurrentCulture);

  /// <summary>
  ///   Checks if the <see cref="string" /> ends with any character from the given list.
  /// </summary>
  /// <param name="this">This <see cref="string" />.</param>
  /// <param name="values">The values to compare to.</param>
  /// <param name="stringComparison">The mode to use for comparisons</param>
  /// <returns>
  ///   <see langword="true" /> if there is at least one <see cref="char" /> in the list that matches the end;
  ///   otherwise, <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsWithAny(this string @this, IEnumerable<char> values, StringComparison stringComparison) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(values);

    return values.Any(s => @this.EndsWith(s, stringComparison));
  }

  /// <summary>
  ///   Checks if the <see cref="string" /> ends with any character from the given list.
  /// </summary>
  /// <param name="this">This <see cref="string" />.</param>
  /// <param name="values">The values to compare to.</param>
  /// <param name="comparer">The <see cref="StringComparer" /> to use for comparisons</param>
  /// <returns>
  ///   <see langword="true" /> if there is at least one <see cref="char" /> in the list that matches the end;
  ///   otherwise, <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsWithAny(this string @this, IEnumerable<char> values, StringComparer comparer) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(values);

    return values.Any(s => @this.EndsWith(s, comparer));
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsNotWithAny(this string @this, params string[] values) => !EndsWithAny(@this, values);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsNotWithAny(this string @this, StringComparison comparison, params string[] values) => !EndsWithAny(@this, comparison, values);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsNotWithAny(this string @this, StringComparer comparer, params string[] values) => !EndsWithAny(@this, comparer, values);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsNotWithAny(this string @this, params char[] values) => !EndsWithAny(@this, values);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsNotWithAny(this string @this, StringComparison comparison, params char[] values) => !EndsWithAny(@this, comparison, values);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsNotWithAny(this string @this, StringComparer comparer, params char[] values) => !EndsWithAny(@this, comparer, values);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsNotWithAny(this string @this, IEnumerable<string> values) => !EndsWithAny(@this, values);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsNotWithAny(this string @this, IEnumerable<string> values, StringComparison comparison) => !EndsWithAny(@this, values, comparison);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsNotWithAny(this string @this, IEnumerable<string> values, StringComparer comparer) => !EndsWithAny(@this, values, comparer);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsNotWithAny(this string @this, IEnumerable<char> values) => !EndsWithAny(@this, values);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsNotWithAny(this string @this, IEnumerable<char> values, StringComparison comparison) => !EndsWithAny(@this, values, comparison);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsNotWithAny(this string @this, IEnumerable<char> values, StringComparer comparer) => !EndsWithAny(@this, values, comparer);

  #endregion

  /// <summary>
  ///   Determines whether the given string is surrounded by another one.
  /// </summary>
  /// <param name="this">This String.</param>
  /// <param name="text">The text that should be around the given string.</param>
  /// <param name="stringComparison">The string comparison.</param>
  /// <returns>
  ///   <c>true</c> if the given string is surrounded by the given text; otherwise, <c>false</c>.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsSurroundedWith(this string @this, string text, StringComparison stringComparison = StringComparison.CurrentCulture) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(text);

    return @this.IsSurroundedWith(text, text, stringComparison);
  }

  /// <summary>
  ///   Determines whether the given string is surrounded by two others.
  /// </summary>
  /// <param name="this">This String.</param>
  /// <param name="prefix">The prefix.</param>
  /// <param name="postfix">The postfix.</param>
  /// <param name="stringComparison">The string comparison.</param>
  /// <returns>
  ///   <c>true</c> if the given string is surrounded by the given text; otherwise, <c>false</c>.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsSurroundedWith(this string @this, string prefix, string postfix, StringComparison stringComparison = StringComparison.CurrentCulture) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(prefix);
    Against.ArgumentIsNull(postfix);

    return @this.StartsWith(prefix, stringComparison) && @this.EndsWith(postfix, stringComparison);
  }

  /// <summary>
  /// Replaces the specified substring at the start of the given string with another substring, if present.
  /// </summary>
  /// <param name="this">The string to process. Must not be <see langword="null"/>.</param>
  /// <param name="what">The substring to be replaced at the beginning of <paramref name="this"/>. Must not be <see langword="null"/>.</param>
  /// <param name="replacement">The substring to replace <paramref name="what"/> with.</param>
  /// <param name="stringComparison"><i>(Optional)</i> The <see cref="StringComparison"/> mode used to compare substrings. Defaults to <see cref="StringComparison.CurrentCulture"/>.</param>
  /// <returns>
  /// A new string where the first occurrence of <paramref name="what"/> at the beginning is replaced with <paramref name="replacement"/>.
  /// If <paramref name="this"/> does not start with <paramref name="what"/>, the original string is returned.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown when <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="ArgumentNullException">
  /// Thrown when <paramref name="what"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// string input = "HelloWorld";
  /// string result = input.ReplaceAtStart("Hello", "Hi");
  /// Console.WriteLine(result); // Output: "HiWorld"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ReplaceAtStart(this string @this, string what, string replacement, StringComparison stringComparison = StringComparison.CurrentCulture) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(what);

    if (@this.Length < what.Length)
      return @this;

    return @this.StartsWith(what, stringComparison) ? IsNullOrEmpty(replacement) ? @this[what.Length..] : replacement + @this[what.Length..] : @this;
  }

  /// <summary>
  /// Replaces the specified substring at the end of the given string with another substring, if present.
  /// </summary>
  /// <param name="this">The string to process. Must not be <see langword="null"/>.</param>
  /// <param name="what">The substring to be replaced at the end of <paramref name="this"/>. Must not be <see langword="null"/>.</param>
  /// <param name="replacement">The substring to replace <paramref name="what"/> with.</param>
  /// <param name="stringComparison"><i>(Optional)</i> The <see cref="StringComparison"/> mode used to compare substrings. Defaults to <see cref="StringComparison.CurrentCulture"/>.</param>
  /// <returns>
  /// A new string where the last occurrence of <paramref name="what"/> at the end is replaced with <paramref name="replacement"/>.
  /// If <paramref name="this"/> does not end with <paramref name="what"/>, the original string is returned.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown when <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="ArgumentNullException">
  /// Thrown when <paramref name="what"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// string input = "HelloWorld";
  /// string result = input.ReplaceAtEnd("World", "Universe");
  /// Console.WriteLine(result); // Output: "HelloUniverse"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ReplaceAtEnd(this string @this, string what, string replacement, StringComparison stringComparison = StringComparison.CurrentCulture) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(what);

    if (@this.Length < what.Length)
      return @this;

    return @this.EndsWith(what, stringComparison) ? IsNullOrEmpty(replacement) ? @this[..^what.Length] : @this[..^what.Length] + replacement : @this;
  }

  /// <summary>
  /// Repeatedly removes the specified substring from the start of the given string, if present.
  /// </summary>
  /// <param name="this">The string to process. Must not be <see langword="null"/>.</param>
  /// <param name="what">The substring to remove from the start of <paramref name="this"/>. Must not be <see langword="null"/>.</param>
  /// <param name="stringComparison"><i>(Optional)</i> The <see cref="StringComparison"/> mode used to compare substrings. Defaults to <see cref="StringComparison.CurrentCulture"/>.</param>
  /// <returns>
  /// A new string with <paramref name="what"/> removed from the beginning, or the original string if it does not start with <paramref name="what"/>.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown when <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="ArgumentNullException">
  /// Thrown when <paramref name="what"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// string input = "HelloHelloWorld";
  /// string result = input.TrimStart("Hello");
  /// Console.WriteLine(result); // Output: "World"
  /// </code>
  /// </example>
  public static string TrimStart(this string @this, string what, StringComparison stringComparison = StringComparison.CurrentCulture) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(what);

    var thisLength = @this.Length;
    var needleLength = what.Length;
    if (needleLength == 0 || thisLength < needleLength)
      return @this;

    if (stringComparison == StringComparison.Ordinal) {
      var search = @this.AsSpan();
      for (
        var needle = what.AsSpan(); 
        search.Length >= needleLength && needle.SequenceEqual(search[..needleLength]); 
        search = search[needleLength..]
        )
        ;

      return search.Length == thisLength ? @this : search.ToString();
    }

    var index = 0;
    while (index + needleLength <= thisLength && string.Equals(what, @this.Substring(index, needleLength), stringComparison))
      index += needleLength;

    return index == 0 ? @this : @this[index..];
  }

  /// <summary>
  /// Repeatedly removes all occurrences of the specified substring from the beginning of the given <see cref="ReadOnlySpan{char}"/>.
  /// </summary>
  /// <param name="this">The span to process.</param>
  /// <param name="what">The substring to remove from the start of <paramref name="this"/>.</param>
  /// <param name="stringComparison"><i>(Optional)</i> The <see cref="StringComparison"/> mode used to compare substrings. Defaults to <see cref="StringComparison.CurrentCulture"/>.</param>
  /// <returns>
  /// A <see cref="ReadOnlySpan{char}"/> with all leading occurrences of <paramref name="what"/> removed. 
  /// If <paramref name="this"/> does not start with <paramref name="what"/>, the original span is returned.
  /// </returns>
  /// <example>
  /// <code>
  /// ReadOnlySpan&lt;char&gt; span = "HelloHelloWorld".AsSpan();
  /// ReadOnlySpan&lt;char&gt; result = span.TrimStart("Hello".AsSpan());
  /// Console.WriteLine(result.ToString()); // Output: "World"
  /// </code>
  /// </example>
  public static ReadOnlySpan<char> TrimStart(this ReadOnlySpan<char> @this, ReadOnlySpan<char> what, StringComparison stringComparison = StringComparison.CurrentCulture) {
    var thisLength = @this.Length;
    var needleLength = what.Length;
    if (needleLength == 0 || thisLength < needleLength)
      return @this;

    if (stringComparison == StringComparison.Ordinal) {
      for (
        ;
        @this.Length >= needleLength && what.SequenceEqual(@this[..needleLength]); 
        @this = @this[needleLength..]
        )
        ;
      return @this;
    }

    var index = 0;
    var needle = what.ToString();
    while (index + needleLength <= thisLength && @this.Slice(index, needleLength).ToString().Equals(needle, stringComparison))
      index += needleLength;
      
    return index == 0 ? @this : @this[index..];
  }

  /// <summary>
  /// Repeatedly removes the specified substring from the end of the given string, if present.
  /// </summary>
  /// <param name="this">The current string instance.</param>
  /// <param name="what">The string to remove from the end of the current string.</param>
  /// <param name="stringComparison"><i>(Optional)</i> The <see cref="StringComparison"/> mode used to compare substrings. Defaults to <see cref="StringComparison.CurrentCulture"/>.</param>
  /// <returns>The string after removing the specified string from the end.</returns>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="what" /> is null.</exception>
  /// <example>
  ///   <code>
  /// string example = "Hello World!!!!!";
  /// string trimmed = example.TrimEnd("!!");
  /// Console.WriteLine(trimmed); // Outputs: "Hello World!"
  /// </code>
  /// </example>
  public static string TrimEnd(this string @this, string what, StringComparison stringComparison = StringComparison.CurrentCulture) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(what);

    var thisLength = @this.Length;
    var needleLength = what.Length;
    if (needleLength == 0 || thisLength < needleLength)
      return @this;

    if (stringComparison == StringComparison.Ordinal) {
      var search = @this.AsSpan();
      for (
        var needle = what.AsSpan(); 
        search.Length >= needleLength && needle.SequenceEqual(search[^needleLength..]); 
        search = search[..^needleLength]
        )
        ;

      return search.Length == thisLength ? @this : search.ToString();
    }

    var index = thisLength;
    while (index >= needleLength && string.Equals(what, @this.Substring(index-needleLength, needleLength), stringComparison))
      index -= needleLength;

    return index == thisLength ? @this : @this[..index];
  }

  /// <summary>
  /// Repeatedly removes all occurrences of the specified substring from the end of the given <see cref="ReadOnlySpan{char}"/>.
  /// </summary>
  /// <param name="this">The span to process.</param>
  /// <param name="what">The substring to remove from the end of <paramref name="this"/>.</param>
  /// <param name="stringComparison"><i>(Optional)</i> The <see cref="StringComparison"/> mode used to compare substrings. Defaults to <see cref="StringComparison.CurrentCulture"/>.</param>
  /// <returns>
  /// A <see cref="ReadOnlySpan{char}"/> with all trailing occurrences of <paramref name="what"/> removed. 
  /// If <paramref name="this"/> does not end with <paramref name="what"/>, the original span is returned.
  /// </returns>
  /// <example>
  /// <code>
  /// ReadOnlySpan&lt;char&gt; span = "HelloWorldWorld".AsSpan();
  /// ReadOnlySpan&lt;char&gt; result = span.TrimEnd("World".AsSpan());
  /// Console.WriteLine(result.ToString()); // Output: "Hello"
  /// </code>
  /// </example>
  public static ReadOnlySpan<char> TrimEnd(this ReadOnlySpan<char> @this, ReadOnlySpan<char> what, StringComparison stringComparison = StringComparison.CurrentCulture) {
    var thisLength = @this.Length;
    var needleLength = what.Length;
    if (needleLength == 0 || thisLength < needleLength)
      return @this;

    if (stringComparison == StringComparison.Ordinal) {
      for (
        ; 
        @this.Length >= needleLength && what.SequenceEqual(@this[^needleLength..]); 
        @this = @this [ ..^needleLength]
        )
        ;
      return @this;
    }
    
    var needle = what.ToString();
    var index = thisLength;
    while (index >= needleLength && @this.Slice(index - needleLength, needleLength).ToString().Equals(needle, stringComparison))
      index -= needleLength;

    return index == thisLength ? @this : @this[..index];
  }

  #region Null and WhiteSpace checks

  /// <summary>
  /// Determines whether the specified string is <see langword="null"/> or empty.
  /// </summary>
  /// <param name="this">The string to check.</param>
  /// <returns>
  /// <see langword="true"/> if <paramref name="this"/> is <see langword="null"/> or an empty string; otherwise, <see langword="false"/>.
  /// </returns>
  /// <example>
  /// <code>
  /// string input1 = null;
  /// bool result1 = input1.IsNullOrEmpty(); // Output: true
  ///
  /// string input2 = "";
  /// bool result2 = input2.IsNullOrEmpty(); // Output: true
  ///
  /// string input3 = "Hello";
  /// bool result3 = input3.IsNullOrEmpty(); // Output: false
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNullOrEmpty([NotNullWhen(false)] this string @this) => string.IsNullOrEmpty(@this);

  /// <summary>
  /// Determines whether the specified string is not <see langword="null"/> and not empty.
  /// </summary>
  /// <param name="this">The string to check.</param>
  /// <returns>
  /// <see langword="true"/> if <paramref name="this"/> is not <see langword="null"/> and not an empty string; otherwise, <see langword="false"/>.
  /// </returns>
  /// <example>
  /// <code>
  /// string input1 = null;
  /// bool result1 = input1.IsNotNullOrEmpty(); // Output: false
  ///
  /// string input2 = "";
  /// bool result2 = input2.IsNotNullOrEmpty(); // Output: false
  ///
  /// string input3 = "Hello";
  /// bool result3 = input3.IsNotNullOrEmpty(); // Output: true
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNotNullOrEmpty([NotNullWhen(true)] this string @this) => !string.IsNullOrEmpty(@this);

  /// <summary>
  /// Determines whether the specified string is <see langword="null"/>, empty, or consists only of white-space characters.
  /// </summary>
  /// <param name="this">The string to check.</param>
  /// <returns>
  /// <see langword="true"/> if <paramref name="this"/> is <see langword="null"/>, empty, or contains only white-space characters; otherwise, <see langword="false"/>.
  /// </returns>
  /// <example>
  /// <code>
  /// string input1 = null;
  /// bool result1 = input1.IsNullOrWhiteSpace(); // Output: true
  ///
  /// string input2 = "";
  /// bool result2 = input2.IsNullOrWhiteSpace(); // Output: true
  ///
  /// string input3 = "   ";
  /// bool result3 = input3.IsNullOrWhiteSpace(); // Output: true
  ///
  /// string input4 = "Hello";
  /// bool result4 = input4.IsNullOrWhiteSpace(); // Output: false
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if SUPPORTS_STRING_IS_NULL_OR_WHITESPACE
  public static bool IsNullOrWhiteSpace([NotNullWhen(false)]this string @this) => string.IsNullOrWhiteSpace(@this);
#else
  public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string @this) {
    if (@this == null)
      return true;

    // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
    foreach (var chr in @this)
      if (!chr.IsWhiteSpace())
        return false;

    return true;
  }
#endif

  /// <summary>
  /// Determines whether the specified <see cref="ReadOnlySpan{char}"/> is empty or consists only of white-space characters.
  /// </summary>
  /// <param name="this">The span to check.</param>
  /// <returns>
  /// <see langword="true"/> if <paramref name="this"/> is empty or contains only white-space characters; otherwise, <see langword="false"/>.
  /// </returns>
  /// <example>
  /// <code>
  /// ReadOnlySpan&lt;char&gt; span1 = "   ".AsSpan();
  /// bool result1 = span1.IsEmptyOrWhiteSpace(); // Output: true
  ///
  /// ReadOnlySpan&lt;char&gt; span2 = "".AsSpan();
  /// bool result2 = span2.IsEmptyOrWhiteSpace(); // Output: true
  ///
  /// ReadOnlySpan&lt;char&gt; span3 = "Hello".AsSpan();
  /// bool result3 = span3.IsEmptyOrWhiteSpace(); // Output: false
  /// </code>
  /// </example>
  public static bool IsEmptyOrWhiteSpace(this ReadOnlySpan<char> @this) {
    if (@this.IsEmpty)
      return true;

    // ReSharper disable once LoopCanBeConvertedToQuery
    foreach (var chr in @this)
      if (!chr.IsWhiteSpace())
        return false;

    return true;
  }

  /// <summary>
  /// Determines whether the specified string is not <see langword="null"/>, not empty, and contains at least one non-white-space character.
  /// </summary>
  /// <param name="this">The string to check.</param>
  /// <returns>
  /// <see langword="true"/> if <paramref name="this"/> is not <see langword="null"/>, not empty, and contains at least one non-white-space character; otherwise, <see langword="false"/>.
  /// </returns>
  /// <example>
  /// <code>
  /// string input1 = null;
  /// bool result1 = input1.IsNotNullOrWhiteSpace(); // Output: false
  ///
  /// string input2 = "";
  /// bool result2 = input2.IsNotNullOrWhiteSpace(); // Output: false
  ///
  /// string input3 = "   ";
  /// bool result3 = input3.IsNotNullOrWhiteSpace(); // Output: false
  ///
  /// string input4 = "Hello";
  /// bool result4 = input4.IsNotNullOrWhiteSpace(); // Output: true
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNotNullOrWhiteSpace([NotNullWhen(true)] this string @this) => !IsNullOrWhiteSpace(@this);

  /// <summary>
  /// Determines whether the specified <see cref="ReadOnlySpan{char}"/> is not empty and contains at least one non-white-space character.
  /// </summary>
  /// <param name="this">The span to check.</param>
  /// <returns>
  /// <see langword="true"/> if <paramref name="this"/> is not empty and contains at least one non-white-space character; otherwise, <see langword="false"/>.
  /// </returns>
  /// <example>
  /// <code>
  /// ReadOnlySpan&lt;char&gt; span1 = "   ".AsSpan();
  /// bool result1 = span1.IsNotEmptyOrWhiteSpace(); // Output: false
  ///
  /// ReadOnlySpan&lt;char&gt; span2 = "".AsSpan();
  /// bool result2 = span2.IsNotEmptyOrWhiteSpace(); // Output: false
  ///
  /// ReadOnlySpan&lt;char&gt; span3 = "Hello".AsSpan();
  /// bool result3 = span3.IsNotEmptyOrWhiteSpace(); // Output: true
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNotEmptyOrWhiteSpace(this ReadOnlySpan<char> @this) => !IsEmptyOrWhiteSpace(@this);

  #endregion

  #region IndexOf

  /// <summary>
  /// Finds the zero-based index of the first occurrence of the specified character in the given string, starting from a specified index, using the specified comparison option.
  /// </summary>
  /// <param name="this">The string to search. Must not be <see langword="null"/>.</param>
  /// <param name="value">The character to locate in <paramref name="this"/>.</param>
  /// <param name="startIndex">The zero-based index at which to begin the search.</param>
  /// <param name="comparison">The <see cref="StringComparison"/> mode to use for the search.</param>
  /// <returns>
  /// The zero-based index of the first occurrence of <paramref name="value"/> in <paramref name="this"/> after <paramref name="startIndex"/>, or -1 if the character is not found.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown when <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="ArgumentOutOfRangeException">
  /// Thrown when <paramref name="startIndex"/> is less than 0 or greater than the length of <paramref name="this"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// string input = "Hello World";
  /// int index = input.IndexOf('o', 5, StringComparison.CurrentCulture);
  /// Console.WriteLine(index); // Output: 7
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)] 
  public static int IndexOf(this string @this, char value, int startIndex, StringComparison comparison) 
    => @this.IndexOf(value.ToString(), startIndex, comparison)
    ;

  /// <summary>
  /// Finds the zero-based index of the first occurrence of the specified character in the given <see cref="ReadOnlySpan{char}"/>, 
  /// starting from a specified index, using the specified comparison option.
  /// </summary>
  /// <param name="this">The span to search.</param>
  /// <param name="value">The character to locate in <paramref name="this"/>.</param>
  /// <param name="startIndex">The zero-based index at which to begin the search.</param>
  /// <param name="comparison">The <see cref="StringComparison"/> mode to use for the search.</param>
  /// <returns>
  /// The zero-based index of the first occurrence of <paramref name="value"/> in <paramref name="this"/> after <paramref name="startIndex"/>, 
  /// or -1 if the character is not found.
  /// </returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// Thrown when <paramref name="startIndex"/> is less than 0 or greater than the length of <paramref name="this"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// ReadOnlySpan&lt;char&gt; span = "Hello World".AsSpan();
  /// int index = span.IndexOf('o', 5, StringComparison.CurrentCulture);
  /// Console.WriteLine(index); // Output: 7
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int IndexOf(this ReadOnlySpan<char> @this, char value, int startIndex, StringComparison comparison) 
    => comparison == StringComparison.OrdinalIgnoreCase 
      ? @this[startIndex..].IndexOf(value) 
      : @this[startIndex..].IndexOf([value], comparison)
      ;

  #endregion

  #region Contains/ContainsNot

  /// <summary>
  ///   Returns a value indicating whether a specified string occurs within this string, using the specified comparison
  ///   rules.
  /// </summary>
  /// <param name="this">This <see cref="string" /></param>
  /// <param name="value">The string to seek.</param>
  /// <param name="comparer">The <see cref="StringComparer" /> to use.</param>
  /// <returns>
  ///   <see langword="true" /> if the <paramref name="value" /> parameter occurs within this string, or if
  ///   <paramref name="value" /> is the empty string (""); otherwise, <see langword="false" />.
  /// </returns>
  public static bool Contains(this string @this, string value, StringComparer comparer) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(value);
    Against.ArgumentIsNull(comparer);

    if (comparer == StringComparer.Ordinal)
      return @this.Contains(value, StringComparison.Ordinal);
    if (comparer == StringComparer.OrdinalIgnoreCase)
      return @this.Contains(value, StringComparison.OrdinalIgnoreCase);

    if (comparer == StringComparer.InvariantCulture)
      return @this.Contains(value, StringComparison.InvariantCulture);
    if (comparer == StringComparer.InvariantCultureIgnoreCase)
      return @this.Contains(value, StringComparison.InvariantCultureIgnoreCase);

    // Note: sadly we can't refactor that using a jump table because it depends on the current thread's culture
    if (comparer == StringComparer.CurrentCulture)
      return @this.Contains(value, StringComparison.CurrentCulture);
    if (comparer == StringComparer.CurrentCultureIgnoreCase)
      return @this.Contains(value, StringComparison.CurrentCultureIgnoreCase);

    var otherLength = value.Length;
    for (var i = 0; i < @this.Length - otherLength; ++i)
      if (comparer.Equals(@this[i..otherLength], value))
        return true;

    return false;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool ContainsNot(this string @this, string value) => !@this.Contains(value);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool ContainsNot(this string @this, string value, StringComparison comparisonType) => !@this.Contains(value, comparisonType);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool ContainsNot(this string @this, string value, StringComparer comparer) => !@this.Contains(value, comparer);

  #endregion

  #region ContainsAll/ContainsAny/ContainsNotAny

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool ContainsAll(this string @this, params string[] other) => ContainsAll(@this, other, StringComparison.CurrentCulture);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool ContainsAll(this string @this, StringComparison comparison, params string[] other) => ContainsAll(@this, other, comparison);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool ContainsAll(this string @this, StringComparer comparer, params string[] other) => ContainsAll(@this, other, comparer);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool ContainsAll(this string @this, IEnumerable<string> values) => ContainsAll(@this, values, StringComparison.CurrentCulture);

  /// <summary>
  ///   Whether the given <see cref="string" /> contains all the given values or not.
  /// </summary>
  /// <param name="this">This <see cref="string" /></param>
  /// <param name="values">The values that should be included</param>
  /// <param name="comparison">The type of <see cref="StringComparison" /> to use</param>
  /// <returns>
  ///   <see langword="true" /> if the given <see cref="string" /> contains all the given values; otherwise,
  ///   <see langword="false" />
  /// </returns>
  public static bool ContainsAll(this string @this, IEnumerable<string> values, StringComparison comparison) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(values);
    Against.UnknownEnumValues(comparison);

    return values.All(v => @this.Contains(v ?? string.Empty, comparison));
  }

  /// <summary>
  ///   Whether the given <see cref="string" /> contains all the given values or not.
  /// </summary>
  /// <param name="this">This <see cref="string" /></param>
  /// <param name="values">The values that should be included</param>
  /// <param name="comparer">The <see cref="StringComparer" /> to use for comparisons</param>
  /// <returns>
  ///   <see langword="true" /> if the given <see cref="string" /> contains all the given values; otherwise,
  ///   <see langword="false" />
  /// </returns>
  public static bool ContainsAll(this string @this, IEnumerable<string> values, StringComparer comparer) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(values);
    Against.ArgumentIsNull(comparer);

    return values.All(v => @this.Contains(v ?? string.Empty, comparer));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool ContainsAny(this string @this, params string[] other) => ContainsAny(@this, other, StringComparison.CurrentCulture);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool ContainsAny(this string @this, StringComparison comparisonType, params string[] other) => ContainsAny(@this, other, comparisonType);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool ContainsAny(this string @this, StringComparer comparer, params string[] other) => ContainsAny(@this, other, comparer);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool ContainsAny(this string @this, IEnumerable<string> other) => ContainsAny(@this, other, StringComparison.CurrentCulture);

  /// <summary>
  ///   Determines whether a given <see cref="string" /> contains one of others.
  /// </summary>
  /// <param name="this">This <see cref="string" />.</param>
  /// <param name="other">The strings to look for.</param>
  /// <param name="comparisonType">Type of the comparison.</param>
  /// <returns>
  ///   <see langword="true" /> if any of the other strings is part of the given <see cref="string" />; otherwise,
  ///   <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool ContainsAny(this string @this, IEnumerable<string> other, StringComparison comparisonType) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(other);

    return other.Any(item => @this.Contains(item ?? string.Empty, comparisonType));
  }

  /// <summary>
  ///   Determines whether a given <see cref="string" /> contains one of others.
  /// </summary>
  /// <param name="this">This <see cref="string" />.</param>
  /// <param name="other">The strings to look for.</param>
  /// <param name="comparer">The <see cref="StringComparer" /> to use for comparisons.</param>
  /// <returns>
  ///   <see langword="true" /> if any of the other strings is part of the given <see cref="string" />; otherwise,
  ///   <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool ContainsAny(this string @this, IEnumerable<string> other, StringComparer comparer) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(other);
    Against.ArgumentIsNull(comparer);

    return other.Any(item => @this.Contains(item ?? string.Empty, comparer));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool ContainsNotAny(this string @this, params string[] other) => !ContainsAny(@this, other, StringComparison.CurrentCulture);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool ContainsNotAny(this string @this, StringComparison comparisonType, params string[] other) => !ContainsAny(@this, other, comparisonType);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool ContainsNotAny(this string @this, StringComparer comparer, params string[] other) => !ContainsAny(@this, other, comparer);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool ContainsNotAny(this string @this, IEnumerable<string> other) => !ContainsAny(@this, other, StringComparison.CurrentCulture);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool ContainsNotAny(this string @this, IEnumerable<string> other, StringComparison comparison) => !ContainsAny(@this, other, comparison);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool ContainsNotAny(this string @this, IEnumerable<string> other, StringComparer comparer) => !ContainsAny(@this, other, comparer);

  #endregion

  #region IsAnyOf/IsNotAnyOf

  /// <summary>
  ///   Checks whether the given string matches any of the provided
  /// </summary>
  /// <param name="this">This <see cref="string" /></param>
  /// <param name="needles">String to compare to</param>
  /// <returns><c>true</c> if the string matches; otherwise, <c>false</c></returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsAnyOf(this string @this, params string[] needles) => IsAnyOf(@this, (IEnumerable<string>)needles);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsAnyOf(this string @this, StringComparison comparison, params string[] needles) => IsAnyOf(@this, needles, comparison);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsAnyOf(this string @this, StringComparer comparer, params string[] needles) => IsAnyOf(@this, needles, comparer);

  /// <summary>
  ///   Checks whether the given string matches any of the provided
  /// </summary>
  /// <param name="this">This <see cref="string" /></param>
  /// <param name="needles">String to compare to</param>
  /// <returns><c>true</c> if the string matches; otherwise, <c>false</c></returns>
  public static bool IsAnyOf(this string @this, IEnumerable<string> needles) {
    Against.ArgumentIsNull(needles);

    switch (@this) {
      case null: {
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var value in needles)
          if (value == null)
            return true;

        break;
      }
      default: {
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var value in needles)
          if (value == @this)
            return true;

        break;
      }
    }

    return false;
  }

  /// <summary>
  ///   Checks whether the given string matches any of the provided
  /// </summary>
  /// <param name="this">This <see cref="string" /></param>
  /// <param name="needles">String to compare to</param>
  /// <param name="comparison">The comparison mode</param>
  /// <returns><c>true</c> if the string matches; otherwise, <c>false</c></returns>
  public static bool IsAnyOf(this string @this, IEnumerable<string> needles, StringComparison comparison) {
    Against.ArgumentIsNull(needles);

    switch (@this) {
      case null: {
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var value in needles)
          if (value == null)
            return true;

        break;
      }
      default: {
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var value in needles)
          if (string.Equals(value, @this, comparison))
            return true;

        break;
      }
    }

    return false;
  }

  /// <summary>
  ///   Checks whether the given string matches any of the provided
  /// </summary>
  /// <param name="this">This <see cref="string" /></param>
  /// <param name="needles">String to compare to</param>
  /// <param name="comparer">The <see cref="StringComparer" /> to use for comparisons</param>
  /// <returns><c>true</c> if the string matches; otherwise, <c>false</c></returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsAnyOf(this string @this, IEnumerable<string> needles, StringComparer comparer) {
    Against.ArgumentIsNull(needles);
    Against.ArgumentIsNull(comparer);

    return needles.Contains(@this, comparer);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNotAnyOf(this string @this, params string[] needles) => !IsAnyOf(@this, (IEnumerable<string>)needles);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNotAnyOf(this string @this, StringComparison comparison, params string[] needles) => !IsAnyOf(@this, needles, comparison);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNotAnyOf(this string @this, StringComparer comparer, params string[] needles) => !IsAnyOf(@this, needles, comparer);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNotAnyOf(this string @this, IEnumerable<string> needles) => !IsAnyOf(@this, needles);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNotAnyOf(this string @this, IEnumerable<string> needles, StringComparison comparison) => !IsAnyOf(@this, needles, comparison);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNotAnyOf(this string @this, IEnumerable<string> needles, StringComparer comparer) => !IsAnyOf(@this, needles, comparer);

  #endregion

  #region DefaultIf

  /// <summary>
  ///   Returns a default value if the given <see cref="string" /> is <see langword="null" />.
  /// </summary>
  /// <param name="this">This <see cref="string" /></param>
  /// <param name="defaultValue">The default value</param>
  /// <returns>The given <see cref="string" /> or the given default value.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string DefaultIfNull(this string @this, string defaultValue) => @this ?? defaultValue;

  /// <summary>
  ///   Returns a default value if the given <see cref="string" /> is <see langword="null" />.
  /// </summary>
  /// <param name="this">This <see cref="string" /></param>
  /// <param name="factory">The factory to generate the default value</param>
  /// <returns>The given <see cref="string" /> or the given default value.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string DefaultIfNull(this string @this, Func<string> factory) {
    Against.ArgumentIsNull(factory);

    return @this ?? factory();
  }

  /// <summary>
  /// Returns the specified default value if the given string is <see langword="null"/> or empty; otherwise, returns the original string.
  /// </summary>
  /// <param name="this">The string to check.</param>
  /// <param name="defaultValue"><i>(Optional)</i> The default value to return if <paramref name="this"/> is <see langword="null"/> or empty. Defaults to <see langword="null"/>.</param>
  /// <returns>
  /// <paramref name="defaultValue"/> if <paramref name="this"/> is <see langword="null"/> or empty; otherwise, returns <paramref name="this"/>.
  /// </returns>
  /// <example>
  /// <code>
  /// string input1 = null;
  /// string result1 = input1.DefaultIfNullOrEmpty("Default");
  /// Console.WriteLine(result1); // Output: "Default"
  ///
  /// string input2 = "";
  /// string result2 = input2.DefaultIfNullOrEmpty("Default");
  /// Console.WriteLine(result2); // Output: "Default"
  ///
  /// string input3 = "Hello";
  /// string result3 = input3.DefaultIfNullOrEmpty("Default");
  /// Console.WriteLine(result3); // Output: "Hello"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string DefaultIfNullOrEmpty(this string @this, string defaultValue = null) => @this.IsNullOrEmpty() ? defaultValue : @this;

  /// <summary>
  /// Returns the specified default value if the given <see cref="ReadOnlySpan{char}"/> is empty; otherwise, returns the original span.
  /// </summary>
  /// <param name="this">The span to check.</param>
  /// <param name="defaultValue"><i>(Optional)</i> The default value to return if <paramref name="this"/> is empty. Defaults to an empty <see cref="ReadOnlySpan{char}"/>.</param>
  /// <returns>
  /// <paramref name="defaultValue"/> if <paramref name="this"/> is empty; otherwise, returns <paramref name="this"/>.
  /// </returns>
  /// <example>
  /// <code>
  /// ReadOnlySpan&lt;char&gt; span1 = "".AsSpan();
  /// ReadOnlySpan&lt;char&gt; result1 = span1.DefaultIfEmpty("Default".AsSpan());
  /// Console.WriteLine(result1.ToString()); // Output: "Default"
  ///
  /// ReadOnlySpan&lt;char&gt; span2 = "Hello".AsSpan();
  /// ReadOnlySpan&lt;char&gt; result2 = span2.DefaultIfEmpty("Default".AsSpan());
  /// Console.WriteLine(result2.ToString()); // Output: "Hello"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ReadOnlySpan<char> DefaultIfEmpty(this ReadOnlySpan<char> @this, ReadOnlySpan<char> defaultValue = default) => @this.IsEmpty ? defaultValue : @this;

  /// <summary>
  /// Returns the result of the specified factory function if the given string is <see langword="null"/> or empty; otherwise, returns the original string.
  /// </summary>
  /// <param name="this">The string to check.</param>
  /// <param name="factory">A function that produces a default value if <paramref name="this"/> is <see langword="null"/> or empty. Must not be <see langword="null"/>.</param>
  /// <returns>
  /// The result of <paramref name="factory"/> if <paramref name="this"/> is <see langword="null"/> or empty; otherwise, returns <paramref name="this"/>.
  /// </returns>
  /// <exception cref="ArgumentNullException">
  /// Thrown when <paramref name="factory"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// string input1 = null;
  /// string result1 = input1.DefaultIfNullOrEmpty(() => "Generated Default");
  /// Console.WriteLine(result1); // Output: "Generated Default"
  ///
  /// string input2 = "";
  /// string result2 = input2.DefaultIfNullOrEmpty(() => "Generated Default");
  /// Console.WriteLine(result2); // Output: "Generated Default"
  ///
  /// string input3 = "Hello";
  /// string result3 = input3.DefaultIfNullOrEmpty(() => "Generated Default");
  /// Console.WriteLine(result3); // Output: "Hello"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string DefaultIfNullOrEmpty(this string @this, Func<string> factory) {
    Against.ArgumentIsNull(factory);

    return @this.IsNullOrEmpty() ? factory() : @this;
  }

  /// <summary>
  /// Returns the specified default value if the given string is <see langword="null"/>, empty, or consists only of white-space characters; otherwise, returns the original string.
  /// </summary>
  /// <param name="this">The string to check.</param>
  /// <param name="defaultValue"><i>(Optional)</i> The default value to return if <paramref name="this"/> is <see langword="null"/>, empty, or contains only white-space characters. Defaults to <see langword="null"/>.</param>
  /// <returns>
  /// <paramref name="defaultValue"/> if <paramref name="this"/> is <see langword="null"/>, empty, or contains only white-space characters; otherwise, returns <paramref name="this"/>.
  /// </returns>
  /// <example>
  /// <code>
  /// string input1 = null;
  /// string result1 = input1.DefaultIfNullOrWhiteSpace("Default");
  /// Console.WriteLine(result1); // Output: "Default"
  ///
  /// string input2 = "";
  /// string result2 = input2.DefaultIfNullOrWhiteSpace("Default");
  /// Console.WriteLine(result2); // Output: "Default"
  ///
  /// string input3 = "   ";
  /// string result3 = input3.DefaultIfNullOrWhiteSpace("Default");
  /// Console.WriteLine(result3); // Output: "Default"
  ///
  /// string input4 = "Hello";
  /// string result4 = input4.DefaultIfNullOrWhiteSpace("Default");
  /// Console.WriteLine(result4); // Output: "Hello"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string DefaultIfNullOrWhiteSpace(this string @this, string defaultValue = null) => IsNullOrWhiteSpace(@this) ? defaultValue : @this;

  /// <summary>
  /// Returns the specified default value if the given <see cref="ReadOnlySpan{char}"/> is empty or consists only of white-space characters; otherwise, returns the original span.
  /// </summary>
  /// <param name="this">The span to check.</param>
  /// <param name="defaultValue"><i>(Optional)</i> The default value to return if <paramref name="this"/> is empty or contains only white-space characters. Defaults to an empty <see cref="ReadOnlySpan{char}"/>.</param>
  /// <returns>
  /// <paramref name="defaultValue"/> if <paramref name="this"/> is empty or contains only white-space characters; otherwise, returns <paramref name="this"/>.
  /// </returns>
  /// <example>
  /// <code>
  /// ReadOnlySpan&lt;char&gt; span1 = "".AsSpan();
  /// ReadOnlySpan&lt;char&gt; result1 = span1.DefaultIfEmptyOrWhiteSpace("Default".AsSpan());
  /// Console.WriteLine(result1.ToString()); // Output: "Default"
  ///
  /// ReadOnlySpan&lt;char&gt; span2 = "   ".AsSpan();
  /// ReadOnlySpan&lt;char&gt; result2 = span2.DefaultIfEmptyOrWhiteSpace("Default".AsSpan());
  /// Console.WriteLine(result2.ToString()); // Output: "Default"
  ///
  /// ReadOnlySpan&lt;char&gt; span3 = "Hello".AsSpan();
  /// ReadOnlySpan&lt;char&gt; result3 = span3.DefaultIfEmptyOrWhiteSpace("Default".AsSpan());
  /// Console.WriteLine(result3.ToString()); // Output: "Hello"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ReadOnlySpan<char> DefaultIfEmptyOrWhiteSpace(this ReadOnlySpan<char> @this, ReadOnlySpan<char> defaultValue = default) => IsEmptyOrWhiteSpace(@this) ? defaultValue : @this;

  /// <summary>
  /// Returns the result of the specified factory function if the given string is <see langword="null"/>, empty, or consists only of white-space characters; otherwise, returns the original string.
  /// </summary>
  /// <param name="this">The string to check.</param>
  /// <param name="factory">A function that produces a default value if <paramref name="this"/> is <see langword="null"/>, empty, or contains only white-space characters. Must not be <see langword="null"/>.</param>
  /// <returns>
  /// The result of <paramref name="factory"/> if <paramref name="this"/> is <see langword="null"/>, empty, or contains only white-space characters; otherwise, returns <paramref name="this"/>.
  /// </returns>
  /// <exception cref="ArgumentNullException">
  /// Thrown when <paramref name="factory"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// string input1 = null;
  /// string result1 = input1.DefaultIfNullOrWhiteSpace(() => "Generated Default");
  /// Console.WriteLine(result1); // Output: "Generated Default"
  ///
  /// string input2 = "";
  /// string result2 = input2.DefaultIfNullOrWhiteSpace(() => "Generated Default");
  /// Console.WriteLine(result2); // Output: "Generated Default"
  ///
  /// string input3 = "   ";
  /// string result3 = input3.DefaultIfNullOrWhiteSpace(() => "Generated Default");
  /// Console.WriteLine(result3); // Output: "Generated Default"
  ///
  /// string input4 = "Hello";
  /// string result4 = input4.DefaultIfNullOrWhiteSpace(() => "Generated Default");
  /// Console.WriteLine(result4); // Output: "Hello"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string DefaultIfNullOrWhiteSpace(this string @this, Func<string> factory) {
    Against.ArgumentIsNull(factory);

    return @this.IsNullOrWhiteSpace() ? factory() : @this;
  }

  #endregion

  #region Line-Breaking stuff

  /// <summary>
  ///   The type of line-break
  /// </summary>
  public enum LineBreakMode : short {
    All = -3,
    AutoDetect = -2,
    None = -1,

    CarriageReturn = 0x0D,
    LineFeed = 0x0A,
    CrLf = 0x0D0A,
    LfCr = 0x0A0D,
    FormFeed = 0x0C,
    NextLine = 0x85,
    LineSeparator = 0x2028,
    ParagraphSeparator = 0x2029,
    NegativeAcknowledge = 0x15,
    EndOfLine = 0x9B,
    Zx = 0x76,
    Null = 0x00,

    OSX = LineFeed,
    Linux = LineFeed,
    Posix = LineFeed,
    Unix = LineFeed,
    MacOS = LineFeed,
    BSD = LineFeed,
    Amiga = LineFeed,
    ClassicMacOS = CarriageReturn,
    Commodore = CarriageReturn,
    ZXSpectrum = CarriageReturn,
    Dos = CrLf,
    Windows = CrLf,
    SymbianOS = CrLf,
    Cpm = CrLf,
    PalmOS = CrLf,
    AmstradCPC = CrLf,
    AcornBBC = LfCr,
    IBM = NegativeAcknowledge,
    Atari = EndOfLine,
    Zx8 = Zx,
  }

  /// <summary>
  ///   The type of delimiter to use when joining lines
  /// </summary>
  public enum LineJoinMode : ushort {
    CarriageReturn = 0x0D,
    LineFeed = 0x0A,
    CrLf = 0x0D0A,
    LfCr = 0x0A0D,
    FormFeed = 0x0C,
    NextLine = 0x85,
    LineSeparator = 0x2028,
    ParagraphSeparator = 0x2029,
    NegativeAcknowledge = 0x15,
    EndOfLine = 0x9B,
    Zx = 0x76,
    Null = 0x00,

    OSX = LineFeed,
    Linux = LineFeed,
    Posix = LineFeed,
    Unix = LineFeed,
    MacOS = LineFeed,
    BSD = LineFeed,
    Amiga = LineFeed,
    ClassicMacOS = CarriageReturn,
    Commodore = CarriageReturn,
    ZXSpectrum = CarriageReturn,
    Dos = CrLf,
    Windows = CrLf,
    SymbianOS = CrLf,
    Cpm = CrLf,
    PalmOS = CrLf,
    AmstradCPC = CrLf,
    AcornBBC = LfCr,
    IBM = NegativeAcknowledge,
    Atari = EndOfLine,
    Zx8 = Zx,
  }

  /// <summary>
  /// Detects the type of line break used in the given string.
  /// </summary>
  /// <param name="this">The string to analyze. Must not be <see langword="null"/>.</param>
  /// <returns>
  /// A <see cref="LineBreakMode"/> value indicating the detected line break type.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown when <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// string text = "Hello\r\nWorld";
  /// LineBreakMode mode = text.DetectLineBreakMode();
  /// Console.WriteLine(mode); // Output: LineBreakMode.CrLf
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static LineBreakMode DetectLineBreakMode(this string @this) {
    Against.ThisIsNull(@this);

    return DetectLineBreakMode(@this.AsSpan());
  }

  /// <summary>
  /// Detects the type of line break used in the given <see cref="ReadOnlySpan{char}"/>.
  /// </summary>
  /// <param name="this">The span to analyze.</param>
  /// <returns>
  /// A <see cref="LineBreakMode"/> value indicating the detected line break type.
  /// </returns>
  /// <example>
  /// <code>
  /// ReadOnlySpan&lt;char&gt; text = "Hello\r\nWorld".AsSpan();
  /// LineBreakMode mode = text.DetectLineBreakMode();
  /// Console.WriteLine(mode); // Output: LineBreakMode.CrLf
  /// </code>
  /// </example>
  public static LineBreakMode DetectLineBreakMode(this ReadOnlySpan<char> @this) {
    if (@this.Length == 0)
      return LineBreakMode.None;

    const char CR = (char)LineBreakMode.CarriageReturn;
    const char LF = (char)LineBreakMode.LineFeed;
    const char FF = (char)LineBreakMode.FormFeed;
    const char NEL = (char)LineBreakMode.NextLine;
    const char LS = (char)LineBreakMode.LineSeparator;
    const char PS = (char)LineBreakMode.ParagraphSeparator;
    const char NL = (char)LineBreakMode.NegativeAcknowledge;
    const char EOL = (char)LineBreakMode.EndOfLine;
    const char ZX = (char)LineBreakMode.Zx;
    const char NUL = (char)LineBreakMode.Null;

    var previousChar = @this[0];
    switch (previousChar) {
      case FF: return LineBreakMode.FormFeed;
      case NEL: return LineBreakMode.NextLine;
      case LS: return LineBreakMode.LineSeparator;
      case PS: return LineBreakMode.ParagraphSeparator;
      case NL: return LineBreakMode.NegativeAcknowledge;
      case EOL: return LineBreakMode.EndOfLine;
      case ZX: return LineBreakMode.Zx;
      case NUL: return LineBreakMode.Null;
    }

    for (var i = 1; i < @this.Length; ++i) {
      var currentChar = @this[i];
      switch (currentChar) {
        case CR when previousChar == LF: return LineBreakMode.LfCr;
        case CR when previousChar == CR: return LineBreakMode.CarriageReturn;
        case LF when previousChar == LF: return LineBreakMode.LineFeed;
        case LF when previousChar == CR: return LineBreakMode.CrLf;
        case FF: return LineBreakMode.FormFeed;
        case NEL: return LineBreakMode.NextLine;
        case LS: return LineBreakMode.LineSeparator;
        case PS: return LineBreakMode.ParagraphSeparator;
        case NL: return LineBreakMode.NegativeAcknowledge;
        case EOL: return LineBreakMode.EndOfLine;
        case ZX: return LineBreakMode.Zx;
        case NUL: return LineBreakMode.Null;
      }

      previousChar = currentChar;
    }

    return previousChar switch {
      CR => LineBreakMode.CarriageReturn,
      LF => LineBreakMode.LineFeed,
      _ => LineBreakMode.None
    };
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<string> EnumerateLines(this string @this, StringSplitOptions options = StringSplitOptions.None) => _EnumerateLines(@this, options == StringSplitOptions.RemoveEmptyEntries, 0);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<string> EnumerateLines(this string @this, LineBreakMode mode, StringSplitOptions options = StringSplitOptions.None) {
    Against.ThisIsNull(@this);
    Against.UnknownEnumValues(mode);
    Against.UnknownEnumValues(options);

    return _EnumerateLines(@this, mode, 0, options);
  }

  private static IEnumerable<string> _EnumerateLines(string @this, LineBreakMode mode, int count, StringSplitOptions options) {
    for (;;)
      switch (mode) {
        case LineBreakMode.All: return _EnumerateLines(@this, options == StringSplitOptions.RemoveEmptyEntries, count);
        case LineBreakMode.AutoDetect:
          mode = DetectLineBreakMode(@this);
          continue;
        case LineBreakMode.None:

          static IEnumerable<string> EnumerateOneLine(string line) { yield return line; }

          return EnumerateOneLine(@this);
        case LineBreakMode.CrLf: return _EnumerateLines(@this, options == StringSplitOptions.RemoveEmptyEntries, "\r\n", count);
        case LineBreakMode.LfCr: return _EnumerateLines(@this, options == StringSplitOptions.RemoveEmptyEntries, "\n\r", count);
        case LineBreakMode.CarriageReturn:
        case LineBreakMode.LineFeed:
        case LineBreakMode.FormFeed:
        case LineBreakMode.NextLine:
        case LineBreakMode.LineSeparator:
        case LineBreakMode.ParagraphSeparator:
        case LineBreakMode.NegativeAcknowledge:
        case LineBreakMode.EndOfLine:
        case LineBreakMode.Zx:
        case LineBreakMode.Null:
          return _EnumerateLines(@this, options == StringSplitOptions.RemoveEmptyEntries, (char)mode, count);
        default: throw new NotImplementedException();
      }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<string> EnumerateLines(this string @this, LineBreakMode mode, int count, StringSplitOptions options = StringSplitOptions.None) {
    Against.ThisIsNull(@this);
    Against.UnknownEnumValues(mode);
    Against.CountBelowOrEqualZero(count);
    Against.UnknownEnumValues(options);

    return _EnumerateLines(@this, mode, count, options);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<string> EnumerateLines(this string @this, string delimiter, StringSplitOptions options = StringSplitOptions.None) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrEmpty(delimiter);
    Against.UnknownEnumValues(options);

    return _EnumerateLines(@this, options == StringSplitOptions.RemoveEmptyEntries, delimiter, 0);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IEnumerable<string> EnumerateLines(this string @this, string delimiter, int count, StringSplitOptions options = StringSplitOptions.None) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrEmpty(delimiter);
    Against.CountBelowOrEqualZero(count);
    Against.UnknownEnumValues(options);

    return _EnumerateLines(@this, options == StringSplitOptions.RemoveEmptyEntries, delimiter, count);
  }

  private static string[] _GetLines(string text, bool removeEmpty, int count) => (__getLines ??= new()).Invoke(text, removeEmpty, count);
  private static __GetLines __getLines;

  private sealed class __GetLines {
    private readonly string[] _possibleSplitters = [
      "\r\n",
      "\n\r",
      "\n",
      "\r",
      "\x15",
      "\x0C",
      "\x85",
      "\u2028",
      "\u2029"
    ];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string[] Invoke(string text, bool removeEmpty, int count)
      => count == 0
        ? text.Split(this._possibleSplitters, removeEmpty ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None)
        : text.Split(this._possibleSplitters, count, removeEmpty ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
  }

  // TODO: don't use the array call, do something with less memory and better performance
  private static IEnumerable<string> _EnumerateLines(string text, bool removeEmpty, int count)
    => _GetLines(text, removeEmpty, count);

  // TODO: don't use the array call, do something with less memory and better performance
  private static IEnumerable<string> _EnumerateLines(string text, bool removeEmpty, char delimiter, int count)
    => _GetLines(text, removeEmpty, delimiter, count);

  private static string[] _GetLines(string text, bool removeEmpty, char delimiter, int count) => count == 0
    ? text.Split(delimiter, removeEmpty ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None)
    : text.Split(delimiter, count, removeEmpty ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);

  // TODO: don't use the array call, do something with less memory and better performance
  private static IEnumerable<string> _EnumerateLines(string text, bool removeEmpty, string delimiter, int count)
    => _GetLines(text, removeEmpty, delimiter, count);

  private static string[] _GetLines(string text, bool removeEmpty, string delimiter, int count) => count == 0
    ? text.Split(delimiter, removeEmpty ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None)
    : text.Split(delimiter, count, removeEmpty ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string[] Lines(this string @this, StringSplitOptions options = StringSplitOptions.None) => _GetLines(@this, options == StringSplitOptions.RemoveEmptyEntries, 0);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string[] Lines(this string @this, LineBreakMode mode, StringSplitOptions options = StringSplitOptions.None) {
    Against.ThisIsNull(@this);
    Against.UnknownEnumValues(mode);
    Against.UnknownEnumValues(options);

    return _GetLines(@this, mode, 0, options);
  }

  private static string[] _GetLines(string @this, LineBreakMode mode, int count, StringSplitOptions options) {
    for (;;)
      switch (mode) {
        case LineBreakMode.All: return _GetLines(@this, options == StringSplitOptions.RemoveEmptyEntries, count);
        case LineBreakMode.AutoDetect:
          mode = DetectLineBreakMode(@this);
          continue;
        case LineBreakMode.None: return [@this];
        case LineBreakMode.CrLf: return _GetLines(@this, options == StringSplitOptions.RemoveEmptyEntries, "\r\n", count);
        case LineBreakMode.LfCr: return _GetLines(@this, options == StringSplitOptions.RemoveEmptyEntries, "\n\r", count);
        case LineBreakMode.CarriageReturn:
        case LineBreakMode.LineFeed:
        case LineBreakMode.FormFeed:
        case LineBreakMode.NextLine:
        case LineBreakMode.LineSeparator:
        case LineBreakMode.ParagraphSeparator:
        case LineBreakMode.NegativeAcknowledge:
        case LineBreakMode.EndOfLine:
        case LineBreakMode.Zx:
        case LineBreakMode.Null:
          return _GetLines(@this, options == StringSplitOptions.RemoveEmptyEntries, (char)mode, count);
        default: throw new NotImplementedException();
      }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string[] Lines(this string @this, LineBreakMode mode, int count, StringSplitOptions options = StringSplitOptions.None) {
    Against.ThisIsNull(@this);
    Against.UnknownEnumValues(mode);
    Against.CountBelowOrEqualZero(count);
    Against.UnknownEnumValues(options);

    return _GetLines(@this, mode, count, options);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string[] Lines(this string @this, string delimiter, StringSplitOptions options = StringSplitOptions.None) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrEmpty(delimiter);
    Against.UnknownEnumValues(options);

    return _GetLines(@this, options == StringSplitOptions.RemoveEmptyEntries, delimiter, 0);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string[] Lines(this string @this, string delimiter, int count, StringSplitOptions options = StringSplitOptions.None) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrEmpty(delimiter);
    Against.CountBelowOrEqualZero(count);
    Against.UnknownEnumValues(options);

    return _GetLines(@this, options == StringSplitOptions.RemoveEmptyEntries, delimiter, count);
  }

  /// <summary>
  ///   Counts the number of lines in the given <see cref="string" />.
  /// </summary>
  /// <param name="this">This <see cref="string" />.</param>
  /// <param name="mode">
  ///   The <see cref="LineBreakMode" /> to use for detection; optional, defaults to
  ///   <see cref="LineBreakMode.All" />
  /// </param>
  /// <param name="ignoreEmptyLines">Whether to ignore empty lines or not</param>
  /// <returns>The number of lines.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int LineCount(this string @this, LineBreakMode mode = LineBreakMode.All, bool ignoreEmptyLines = false) {
    Against.ThisIsNull(@this);
    Against.UnknownEnumValues(mode);

    return EnumerateLines(@this, mode, ignoreEmptyLines ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None).Count();
  }

  /// <summary>
  ///   Counts the number of lines in the given <see cref="string" />.
  /// </summary>
  /// <param name="this">This <see cref="string" />.</param>
  /// <param name="mode">
  ///   The <see cref="LineBreakMode" /> to use for detection; optional, defaults to
  ///   <see cref="LineBreakMode.All" />
  /// </param>
  /// <param name="ignoreEmptyLines">Whether to ignore empty lines or not</param>
  /// <returns>The number of lines.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static long LongLineCount(this string @this, LineBreakMode mode = LineBreakMode.All, bool ignoreEmptyLines = false) {
    Against.ThisIsNull(@this);
    Against.UnknownEnumValues(mode);

    return EnumerateLines(@this, mode, ignoreEmptyLines ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None).LongCount();
  }

  /// <summary>
  ///   Retrieves the string representation of a line joiner based on the specified line joining mode.
  /// </summary>
  /// <param name="mode">The line joining mode, represented by the <see cref="LineJoinMode" /> enumeration.</param>
  /// <returns>The string used to join lines according to the specified mode.</returns>
  /// <exception cref="NotImplementedException">Thrown if the provided <paramref name="mode" /> is not implemented.</exception>
  /// <example>
  ///   <code>
  /// var joiner = GetLineJoiner(LineJoinMode.CarriageReturn);
  /// Console.WriteLine($"CarriageReturn joiner: '{joiner}'"); // Output: '\r'
  /// 
  /// joiner = GetLineJoiner(LineJoinMode.CrLf);
  /// Console.WriteLine($"CrLf joiner: '{joiner}'"); // Output: '\r\n'
  /// 
  /// joiner = GetLineJoiner(LineJoinMode.LineFeed);
  /// Console.WriteLine($"LineFeed joiner: '{joiner}'"); // Output: '\n'
  /// </code>
  ///   This example demonstrates how to get the string representation for different line joining modes.
  /// </example>
  /// <remarks>
  ///   This method supports various line joining modes, including single-character modes like CarriageReturn (`\r`) and
  ///   multi-character sequences like CrLf (`\r\n`). It leverages the <see cref="LineJoinMode" /> enumeration values
  ///   directly
  ///   for single-character joiners, converting the enumeration value to its corresponding character representation.
  /// </remarks>
  public static string GetLineJoiner(LineJoinMode mode) =>
    mode switch {
      LineJoinMode.CarriageReturn or LineJoinMode.LineFeed or LineJoinMode.FormFeed or LineJoinMode.NextLine or LineJoinMode.LineSeparator or LineJoinMode.ParagraphSeparator or LineJoinMode.NegativeAcknowledge or LineJoinMode.EndOfLine or LineJoinMode.Zx or LineJoinMode.Null
        => ((char)mode).ToString(),
      LineJoinMode.CrLf => "\r\n",
      LineJoinMode.LfCr => "\n\r",
      _ => throw new NotImplementedException()
    };

  /// <summary>
  ///   Does word-wrapping if needed
  /// </summary>
  /// <param name="this">This <see cref="string" /></param>
  /// <param name="count">The maximum number of characters per line</param>
  /// <param name="mode">How to join lines</param>
  /// <returns>The word-wrapped text</returns>
  /// <exception cref="NotImplementedException"></exception>
  public static string WordWrap(this string @this, int count, LineJoinMode mode) {
    Against.ThisIsNull(@this);
    Against.CountBelowOrEqualZero(count);
    Against.UnknownEnumValues(mode);

    if (@this.Length <= count)
      return @this;

    var joiner = GetLineJoiner(mode);

    var length = @this.Length;
    var joinerLength = joiner.Length;
    StringBuilder result = new(length + (length / count + 1) * joinerLength);

    for (var lineStart = 0;;) {
      var nextBreakAt = lineStart + count - joinerLength;
      if (nextBreakAt >= length) {
        result.Append(@this, lineStart, length - lineStart);
        return result.ToString();
      }

      var proposedBreakAt = nextBreakAt;
      if (char.IsWhiteSpace(@this[nextBreakAt])) {
        // backtrack till last non whitespace
        do
          --nextBreakAt;
        while (nextBreakAt > lineStart && char.IsWhiteSpace(@this[nextBreakAt]));

        if (nextBreakAt > lineStart) {
          // found at least one character, add it
          result.Append(@this, lineStart, nextBreakAt - lineStart + 1);
          result.Append(joiner);
        }

        // because the current char is whitespace, we can move on
        ++proposedBreakAt;
      } else {
        // backtrack to last whitespace
        do
          --nextBreakAt;
        while (nextBreakAt > lineStart && !char.IsWhiteSpace(@this[nextBreakAt]));

        if (nextBreakAt > lineStart) {
          // found at least one character, add it
          result.Append(@this, lineStart, nextBreakAt - lineStart);
          result.Append(joiner);
          proposedBreakAt = nextBreakAt + 1;
        } else {
          // the current word is longer than the count, hard-wrap it
          result.Append(@this, lineStart, proposedBreakAt - lineStart);
          result.Append(joiner);
        }
      }

      // find start of next word
      while (char.IsWhiteSpace(@this[proposedBreakAt])) {
        ++proposedBreakAt;

        // if only whitespace left
        if (proposedBreakAt >= length)
          return result.ToString();
      }

      lineStart = proposedBreakAt;
    }
  }

  #endregion

  #region Truncation

  /// <summary>
  ///   How to truncate <see cref="string" />s that are too long
  /// </summary>
  public enum TruncateMode {
    KeepStart = 0,
    KeepEnd,
    KeepStartAndEnd,
    KeepMiddle,
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string Truncate(this string @this, int count) => Truncate(@this, count, TruncateMode.KeepStart, "...");

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string Truncate(this string @this, int count, TruncateMode mode) => Truncate(@this, count, mode, "...");

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string Truncate(this string @this, int count, string ellipse) => Truncate(@this, count, TruncateMode.KeepStart, ellipse);

  /// <summary>
  ///   Truncates a given <see cref="string" /> if it is too long.
  /// </summary>
  /// <param name="this">This <see cref="string" /></param>
  /// <param name="count">The maximum allowed number of <see cref="char" />s; must be &gt; 0</param>
  /// <param name="mode">How to truncate; optional, defaults to <see cref="TruncateMode.KeepStart" /></param>
  /// <param name="ellipse">What to replace the trimmed parts with; optional, defaults to "..."</param>
  /// <returns>A string that is no longer than the given count</returns>
  /// <exception cref="NotImplementedException"></exception>
  public static string Truncate(this string @this, int count, TruncateMode mode, string ellipse) {
    Against.ThisIsNull(@this);
    Against.CountBelowOrEqualZero(count);
    Against.UnknownEnumValues(mode);
    Against.ArgumentIsNull(ellipse);

    var length = @this.Length;
    if (length <= count)
      return @this;

    var ellipseLength = ellipse.Length;
    var availableChars = count - ellipseLength;
    var hasAvailableChars = availableChars > 0;

    return mode switch {
      TruncateMode.KeepStart => hasAvailableChars ? @this[..availableChars] + ellipse : @this[0] + ellipse[^--count..],
      TruncateMode.KeepEnd => hasAvailableChars ? ellipse + @this[^availableChars..] : ellipse[..--count] + @this[^1],
      TruncateMode.KeepStartAndEnd => hasAvailableChars ? @this[..((availableChars >> 1) + (availableChars & 1))] + ellipse + @this[^(availableChars >> 1)..] : @this[0] + ellipse[..(count - 2)] + @this[^1],
      TruncateMode.KeepMiddle => KeepMiddle(@this, ellipse, availableChars, count),
      _ => throw new NotImplementedException()
    };

    static string KeepMiddle(string original, string ellipse, int availableChars, int count) {
      availableChars -= ellipse.Length; // because we need two ellipses, we have to substract another one
      var hasAvailableCharsLeft = availableChars > 0;
      if (hasAvailableCharsLeft)
        return ellipse + original.Substring((original.Length - availableChars) >> 1, availableChars) + ellipse;

      --count; // one character for the original string
      var charsFromStartEllipse = (count >> 1) + (count & 1);
      var charsFromEndEllipse = count >> 1;
      return ellipse[..charsFromStartEllipse] + original[original.Length >> 1] + ellipse[^charsFromEndEllipse..];
    }
  }

  #endregion

  /// <summary>
  ///   Splits the given string respecting single and double quotes and allows for escape sequences to be used in these
  ///   strings.
  /// </summary>
  /// <param name="this">This String.</param>
  /// <param name="delimiter">The delimiter to use.</param>
  /// <param name="escapeSequence">The escape sequence.</param>
  /// <param name="options">The options.</param>
  /// <returns>
  ///   A sequence containing the parts of the string.
  /// </returns>
  public static IEnumerable<string> QuotedSplit(this string @this, string delimiter = ",", string escapeSequence = "\\", StringSplitOptions options = StringSplitOptions.None) {
    if (@this == null)
      yield break;

    if (delimiter.IsNullOrEmpty()) {
      yield return @this;
      yield break;
    }

    if (escapeSequence == "")
      escapeSequence = null;

    var length = @this.Length;
    var pos = 0;
    var currentlyEscaping = false;
    var currentlyInSingleQuote = false;
    var currentlyInDoubleQuote = false;
    StringBuilder currentPart = new();

    while (pos < length) {
      var chr = @this[pos++];

      if (currentlyEscaping) {
        currentPart.Append(chr);
        currentlyEscaping = false;
      } else if (currentlyInSingleQuote) {
        if (escapeSequence != null && escapeSequence.StartsWith(chr) && @this.Substring(pos - 1, escapeSequence.Length) == escapeSequence) {
          currentlyEscaping = true;
          pos += escapeSequence.Length;
        } else if (chr == '\'')
          currentlyInSingleQuote = false;
        else
          currentPart.Append(chr);
      } else if (currentlyInDoubleQuote) {
        if (escapeSequence != null && escapeSequence.StartsWith(chr) && @this.Substring(pos - 1, escapeSequence.Length) == escapeSequence) {
          currentlyEscaping = true;
          pos += escapeSequence.Length;
        } else if (chr == '"')
          currentlyInDoubleQuote = false;
        else
          currentPart.Append(chr);
      } else if (delimiter.StartsWith(chr) && @this.Substring(pos - 1, delimiter.Length) == delimiter) {
        if (options == StringSplitOptions.None || currentPart.Length > 0)
          yield return currentPart.ToString();

        currentPart.Clear();
        pos += delimiter.Length - 1;
      } else if ( /*currentPart.Length == 0 &&*/ chr == '\'')
        currentlyInSingleQuote = true;
      else if ( /*currentPart.Length == 0 &&*/ chr == '"')
        currentlyInDoubleQuote = true;
      else if (chr == ' ') { } else
        currentPart.Append(chr);
    }

    if (options == StringSplitOptions.None || currentPart.Length > 0)
      yield return currentPart.ToString();
  }

  /// <summary>
  ///   Splits the given string respecting single and double quotes and allows for escape seququences to be used in these
  ///   strings.
  /// </summary>
  /// <param name="this">This String.</param>
  /// <param name="delimiters">The delimiters.</param>
  /// <param name="escapeSequence">The escape sequence.</param>
  /// <param name="options">The options.</param>
  /// <returns>
  ///   A sequence containing the parts of the string.
  /// </returns>
  public static IEnumerable<string> QuotedSplit(this string @this, char[] delimiters, string escapeSequence = "\\", StringSplitOptions options = StringSplitOptions.None) {
    if (@this == null)
      yield break;

    if (delimiters is not { Length: > 0 }) {
      yield return @this;
      yield break;
    }

    if (escapeSequence == "")
      escapeSequence = null;

    var length = @this.Length;
    var pos = 0;
    var currentlyEscaping = false;
    var currentlyInSingleQuote = false;
    var currentlyInDoubleQuote = false;
    StringBuilder currentPart = new();

    while (pos < length) {
      var chr = @this[pos++];

      if (currentlyEscaping) {
        currentPart.Append(chr);
        currentlyEscaping = false;
      } else if (currentlyInSingleQuote) {
        if (escapeSequence != null && escapeSequence.StartsWith(chr) && @this.Substring(pos - 1, escapeSequence.Length) == escapeSequence) {
          currentlyEscaping = true;
          pos += escapeSequence.Length - 1;
        } else if (chr == '\'')
          currentlyInSingleQuote = false;
        else
          currentPart.Append(chr);
      } else if (currentlyInDoubleQuote) {
        if (escapeSequence != null && escapeSequence.StartsWith(chr) && @this.Substring(pos - 1, escapeSequence.Length) == escapeSequence) {
          currentlyEscaping = true;
          pos += escapeSequence.Length - 1;
        } else if (chr == '"')
          currentlyInDoubleQuote = false;
        else
          currentPart.Append(chr);
      } else if (delimiters.Any(i => i == chr) && delimiters.Any(i => @this[pos - 1] == i)) {
        if (options == StringSplitOptions.None || currentPart.Length > 0)
          yield return currentPart.ToString();

        currentPart.Clear();
      } else if ( /*currentPart.Length == 0 &&*/ chr == '\'')
        currentlyInSingleQuote = true;
      else if ( /*currentPart.Length == 0 &&*/ chr == '"')
        currentlyInDoubleQuote = true;
      else if (chr == ' ') { } else
        currentPart.Append(chr);
    }

    if (options == StringSplitOptions.None || currentPart.Length > 0)
      yield return currentPart.ToString();
  }

  #region nested HostEndPoint

  /// <summary>
  ///   A host endpoint with port.
  /// </summary>
  public class HostEndPoint(string host, int port) {
    /// <summary>
    ///   Gets the host.
    /// </summary>
    public string Host { get; } = host;

    /// <summary>
    ///   Gets the port.
    /// </summary>
    public int Port { get; } = port;

    public static explicit operator IPEndPoint(HostEndPoint @this) => new(Dns.GetHostEntry(@this.Host).AddressList[0], @this.Port);
  }

  #endregion

  /// <summary>
  ///   Parses the host and port from a given string.
  /// </summary>
  /// <param name="this">This String, e.g. 172.17.4.3:http .</param>
  /// <returns>Port and host, <c>null</c> on error during parsing.</returns>
  public static HostEndPoint ParseHostAndPort(this string @this) {
    if (@this.IsNullOrWhiteSpace())
      return null;

    Dictionary<string, ushort> officialPortNames = StaticMethodLocal<Dictionary<string, ushort>>.GetOrAdd(
      () => new() {
        { "tcpmux", 1 },
        { "echo", 7 },
        { "discard", 9 },
        { "daytime", 13 },
        { "quote", 17 },
        { "chargen", 19 },
        { "ftp", 21 },
        { "ssh", 22 },
        { "telnet", 23 },
        { "smtp", 25 },
        { "time", 37 },
        { "whois", 43 },
        { "dns", 53 },
        { "mtp", 57 },
        { "tftp", 69 },
        { "gopher", 70 },
        { "finger", 79 },
        { "http", 80 },
        { "kerberos", 88 },
        { "pop2", 109 },
        { "pop3", 110 },
        { "ident", 113 },
        { "auth", 113 },
        { "sftp", 115 },
        { "sql", 118 },
        { "nntp", 119 },
        { "ntp", 123 },
        { "imap", 143 },
        { "bftp", 152 },
        { "sgmp", 153 },
        { "snmp", 161 },
        { "snmptrap", 162 },
        { "irc", 194 },
        { "ipx", 213 },
        { "mpp", 218 },
        { "imap3", 220 },
        { "https", 443 },
        { "rip", 520 },
        { "rpc", 530 },
        { "nntps", 563 },
      }
    );
    
    var host = @this;
    ushort port = 0;
    var index = host.IndexOf(':');
    if (index < 0)
      return new(host, port);

    var portText = host[(index + 1)..];
    host = host.Left(index);
    if (!ushort.TryParse(portText, out port) && !officialPortNames.TryGetValue(portText.Trim().ToLower(), out port))
      return null;

    return new(host, port);
  }

  /// <summary>
  ///   Replaces any of the given characters in the string.
  /// </summary>
  /// <param name="this">This String.</param>
  /// <param name="chars">The chars to replace.</param>
  /// <param name="replacement">The replacement.</param>
  /// <returns>The new string with replacements done.</returns>
  public static string ReplaceAnyOf(this string @this, string chars, string replacement) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrEmpty(chars);

    if (@this.Length == 0)
      return @this;

    HashSet<char> charSet = [..chars];
    StringBuilder result = new(@this.Length);

    foreach (var c in @this)
      if (charSet.Contains(c))
        result.Append(replacement);
      else
        result.Append(c);

    return result.ToString();
  }

  /// <summary>
  /// Returns the leftmost portion of the given string until the first occurrence of the specified pattern.
  /// </summary>
  /// <param name="this">The string to process. Can be <see langword="null"/>.</param>
  /// <param name="pattern">The substring to search for in <paramref name="this"/>.</param>
  /// <param name="comparison"><i>(Optional)</i> The <see cref="StringComparison"/> mode used for the search. Defaults to <see cref="StringComparison.CurrentCulture"/>.</param>
  /// <returns>
  /// A substring from the beginning of <paramref name="this"/> up to (but not including) the first occurrence of <paramref name="pattern"/>.
  /// If <paramref name="pattern"/> is not found, the original string is returned.
  /// If <paramref name="this"/> is <see langword="null"/>, <see langword="null"/> is returned.
  /// </returns>
  /// <example>
  /// <code>
  /// string input = "Hello, World!";
  /// string result = input.LeftUntil(", ");
  /// Console.WriteLine(result); // Output: "Hello"
  ///
  /// string input2 = "HelloWorld";
  /// string result2 = input2.LeftUntil("XYZ");
  /// Console.WriteLine(result2); // Output: "HelloWorld" (pattern not found)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string LeftUntil(this string @this, string pattern, StringComparison comparison = StringComparison.CurrentCulture) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(pattern);

    if (@this.Length <= 0 || pattern.Length <= 0)
      return string.Empty;

    var index = @this.IndexOf(pattern, comparison);
    return index < 0 ? @this : @this[..index];
  }

  /// <summary>
  /// Returns the leftmost portion of the given <see cref="ReadOnlySpan{char}"/> until the first occurrence of the specified pattern.
  /// </summary>
  /// <param name="this">The span to process.</param>
  /// <param name="pattern">The substring to search for in <paramref name="this"/>.</param>
  /// <param name="comparison"><i>(Optional)</i> The <see cref="StringComparison"/> mode used for the search. Defaults to <see cref="StringComparison.CurrentCulture"/>.</param>
  /// <returns>
  /// A <see cref="ReadOnlySpan{char}"/> from the beginning of <paramref name="this"/> up to (but not including) the first occurrence of <paramref name="pattern"/>.
  /// If <paramref name="pattern"/> is not found, the original span is returned.
  /// </returns>
  /// <example>
  /// <code>
  /// ReadOnlySpan&lt;char&gt; span = "Hello, World!".AsSpan();
  /// ReadOnlySpan&lt;char&gt; result = span.LeftUntil(", ".AsSpan());
  /// Console.WriteLine(result.ToString()); // Output: "Hello"
  ///
  /// ReadOnlySpan&lt;char&gt; span2 = "HelloWorld".AsSpan();
  /// ReadOnlySpan&lt;char&gt; result2 = span2.LeftUntil("XYZ".AsSpan());
  /// Console.WriteLine(result2.ToString()); // Output: "HelloWorld" (pattern not found)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ReadOnlySpan<char> LeftUntil(this ReadOnlySpan<char> @this, ReadOnlySpan<char> pattern, StringComparison comparison = StringComparison.CurrentCulture) {
    if (@this.IsEmpty || pattern.IsEmpty)
      return [];

    var index = @this.IndexOf(pattern, comparison);
    return index < 0 ? @this : @this[..index];
  }

  /// <summary>
  /// Returns the rightmost portion of the given string, starting after the last occurrence of the specified pattern.
  /// </summary>
  /// <param name="this">The string to process. Must not be <see langword="null"/>.</param>
  /// <param name="pattern">The substring to search for in <paramref name="this"/>. Must not be <see langword="null"/>.</param>
  /// <param name="comparison"><i>(Optional)</i> The <see cref="StringComparison"/> mode used for the search. Defaults to <see cref="StringComparison.CurrentCulture"/>.</param>
  /// <returns>
  /// A substring from the first occurrence of <paramref name="pattern"/> (excluding the pattern itself) to the end of <paramref name="this"/>.
  /// If <paramref name="pattern"/> is not found, the original string is returned.
  /// </returns>
  /// <exception cref="NullReferenceException">
  /// Thrown when <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="ArgumentNullException">
  /// Thrown when <paramref name="pattern"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// string input = "Hello, World!";
  /// string result = input.RightUntil(", ");
  /// Console.WriteLine(result); // Output: "World!"
  ///
  /// string input2 = "HelloWorld";
  /// string result2 = input2.RightUntil("XYZ");
  /// Console.WriteLine(result2); // Output: "HelloWorld" (pattern not found)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string RightUntil(this string @this, string pattern, StringComparison comparison = StringComparison.CurrentCulture) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(pattern);

    if (@this.Length <=0 || pattern.Length <= 0)
      return string.Empty;

    var index = @this.LastIndexOf(pattern, comparison);
    return index < 0 ? @this : @this[(index + pattern.Length)..];
  }

  /// <summary>
  /// Returns the rightmost portion of the given <see cref="ReadOnlySpan{char}"/>, starting after the last occurrence of the specified pattern.
  /// </summary>
  /// <param name="this">The span to process.</param>
  /// <param name="pattern">The substring to search for in <paramref name="this"/>.</param>
  /// <param name="comparison"><i>(Optional)</i> The <see cref="StringComparison"/> mode used for the search. Defaults to <see cref="StringComparison.CurrentCulture"/>.</param>
  /// <returns>
  /// A <see cref="ReadOnlySpan{char}"/> starting after the first occurrence of <paramref name="pattern"/> (excluding the pattern itself) and extending to the end of <paramref name="this"/>.
  /// If <paramref name="pattern"/> is not found, the original span is returned.
  /// </returns>
  /// <example>
  /// <code>
  /// ReadOnlySpan&lt;char&gt; span = "Hello, World!".AsSpan();
  /// ReadOnlySpan&lt;char&gt; result = span.RightUntil(", ".AsSpan());
  /// Console.WriteLine(result.ToString()); // Output: "World!"
  ///
  /// ReadOnlySpan&lt;char&gt; span2 = "HelloWorld".AsSpan();
  /// ReadOnlySpan&lt;char&gt; result2 = span2.RightUntil("XYZ".AsSpan());
  /// Console.WriteLine(result2.ToString()); // Output: "HelloWorld" (pattern not found)
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ReadOnlySpan<char> RightUntil(this ReadOnlySpan<char> @this, ReadOnlySpan<char> pattern, StringComparison comparison = StringComparison.CurrentCulture) {
    if (@this.IsEmpty || pattern.IsEmpty)
      return [];

    var index = @this.LastIndexOf(pattern, comparison);
    return index < 0 ? @this : @this[(index + pattern.Length)..];
  }

  #region Similarities

  /// <summary>
  ///   Defines the types of case comparison strategies for string operations, providing options suitable for different
  ///   cultural and technical contexts.
  /// </summary>
  public enum CaseComparison {
    /// <summary>
    ///   Specifies that string comparisons should use ordinal (binary) sort rules.
    /// </summary>
    /// <remarks>
    ///   Ordinal comparison is based on the binary values of the characters and does not take linguistic patterns into
    ///   account.
    ///   It is the fastest type of comparison and is culture-insensitive, making it suitable for scenarios where performance
    ///   is critical and
    ///   the text being compared is not culturally sensitive.
    /// </remarks>
    Ordinal,

    /// <summary>
    ///   Specifies that string comparisons should be based on culture-specific rules.
    /// </summary>
    /// <remarks>
    ///   Culture-specific comparisons take into account the cultural context of the data, such as linguistic and alphabetic
    ///   rules specific to the user's culture.
    ///   This type of comparison is essential for applications that are sensitive to local language settings, as it respects
    ///   locale-specific rules for comparing strings.
    /// </remarks>
    CultureSpecific,

    /// <summary>
    ///   Specifies that string comparisons should use rules that are consistent across cultures.
    /// </summary>
    /// <remarks>
    ///   Invariant culture comparison is designed to offer consistency for operations that require culturally neutral results,
    ///   such as formatting and sorting data in a way that does not change with the user's locale.
    ///   This comparison type is suitable for scenarios where data needs to be presented or processed in a uniform manner
    ///   regardless of the local culture settings.
    /// </remarks>
    InvariantCulture
  }

  /// <summary>
  ///   Determines whether the current string and another specified string differ only by case, and are not exactly the same.
  /// </summary>
  /// <param name="this">The current string instance.</param>
  /// <param name="other">The string to compare with the current string.</param>
  /// <returns>
  ///   <see langword="true" /> if the two strings are equivalent when case is ignored and they are not identical in
  ///   case; otherwise, <see langword="false" />.
  /// </returns>
  /// <example>
  ///   <code>
  /// string str1 = "hello";
  /// string str2 = "HELLO";
  /// bool result = str1.OnlyCaseDiffersFrom(str2);
  /// Console.WriteLine(result); // Outputs: True
  /// 
  /// string str3 = "hello";
  /// string str4 = "hello";
  /// result = str3.OnlyCaseDiffersFrom(str4);
  /// Console.WriteLine(result); // Outputs: False
  /// 
  /// string str5 = "hello";
  /// string str6 = "world";
  /// result = str5.OnlyCaseDiffersFrom(str6);
  /// Console.WriteLine(result); // Outputs: False
  /// </code>
  ///   This example demonstrates how to check if two strings differ only by case, but are not the same string when
  ///   considering case.
  /// </example>
  /// <remarks>
  ///   This method checks if two strings are the same ignoring case differences but are not the same when considering case.
  ///   This can be particularly useful for situations where you want to validate that inputs are not simply cased versions
  ///   of the same value.
  /// </remarks>
  public static bool OnlyCaseDiffersFrom(this string @this, string other) => OnlyCaseDiffersFrom(@this, other, CaseComparison.Ordinal);

  /// <summary>
  ///   Determines whether the current string and another specified string differ only by case, considering a specified case
  ///   comparison strategy.
  /// </summary>
  /// <param name="this">The current string instance.</param>
  /// <param name="other">The string to compare with the current string.</param>
  /// <param name="comparison">The case comparison strategy to use when comparing the strings.</param>
  /// <returns>
  ///   <see langword="true" /> if the two strings are equivalent when case is ignored according to the specified
  ///   comparison strategy, and they are not identical considering case; otherwise, <see langword="false" />.
  /// </returns>
  /// <example>
  ///   <code>
  /// string str1 = "hello";
  /// string str2 = "HELLO";
  /// bool result = str1.OnlyCaseDiffersFrom(str2, CaseComparison.Ordinal);
  /// Console.WriteLine(result); // Outputs: True
  /// 
  /// string str3 = "hello";
  /// string str4 = "hello";
  /// result = str3.OnlyCaseDiffersFrom(str4, CaseComparison.Ordinal);
  /// Console.WriteLine(result); // Outputs: False
  /// 
  /// string str5 = "hello";
  /// string str6 = "world";
  /// result = str5.OnlyCaseDiffersFrom(str6, CaseComparison.Ordinal);
  /// Console.WriteLine(result); // Outputs: False
  /// 
  /// string str7 = "stra�e";
  /// string str8 = "STRASSE";
  /// result = str7.OnlyCaseDiffersFrom(str8, CaseComparison.CultureSpecific);
  /// Console.WriteLine(result); // Outputs: True, in a culture where '�' is equivalent to 'SS'
  /// </code>
  ///   This example demonstrates how to check if two strings differ only by case, with different case comparison strategies
  ///   affecting the result.
  /// </example>
  /// <remarks>
  ///   This method is useful in contexts where the sensitivity to case and cultural differences needs to be considered, such
  ///   as user inputs where the intent is identical despite differences in casing and locale-specific forms.
  /// </remarks>
  public static bool OnlyCaseDiffersFrom(this string @this, string other, CaseComparison comparison) {
    Against.UnknownEnumValues(comparison);

    if (ReferenceEquals(@this, other))
      return false;

    if (@this == null || other == null || @this.Length != other.Length)
      return false;

    Func<char, char> comparer = comparison switch {
      CaseComparison.Ordinal => char.ToUpperInvariant,
      CaseComparison.CultureSpecific => char.ToUpper,
      CaseComparison.InvariantCulture => c => char.ToUpper(c, CultureInfo.InvariantCulture),
      _ => throw new ArgumentException($"Unknown enum value: {comparison}", nameof(comparison))
    };

    var foundCaseDifference = false;
    for (var i = 0; i < @this.Length; ++i) {
      if (@this[i] == other[i])
        continue;

      if (comparer(@this[i]) != comparer(other[i]))
        return false;

      foundCaseDifference = true;
    }

    return foundCaseDifference;
  }

  /// <summary>
  ///   Converts a given filename pattern into a regular expression.
  /// </summary>
  /// <param name="this">The pattern.</param>
  /// <returns>The regex.</returns>
  public static Regex ConvertFilePatternToRegex(this string @this) {
    Against.ThisIsNull(@this);

    var regexPattern = BuildPatternFrom(@this);
    var isCaseSensitive = IsFileSystemCaseSensitive();

    return new(regexPattern, RegexOptions.Compiled | (isCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase) | RegexOptions.Singleline);

    static string BuildPatternFrom(string text) {
      var endsWithAnyExtension = false;
      var endsWithoutExtension = true;

      switch (text.Length) {
        case 0:
        case 1 when text[^1] == '.':
          return "^$";
        case > 1 when text[^1] == '.':
          text = text[..^1];
          break;
        case > 2 when text[^2..] == ".*":
          endsWithAnyExtension = true;
          text = text[..^2];
          break;
        case > 1 when text.LastIndexOf('.') is var pIndex && text.LastIndexOfAny(['/','\\']) is var sIndex && pIndex > sIndex:
          endsWithoutExtension = false;
          break;
      }
      
      // Normalize directory separators
      var dirSeps = Regex.Escape(Path.DirectorySeparatorChar + string.Empty + Path.AltDirectorySeparatorChar);
      var directorySeparators = $"[{dirSeps}]";
      var notDirectorySeparators = $"[^{dirSeps}]";
      
      var result = new StringBuilder(text.Length + 16);
      
      // filenames start either at the beginning, after a drive specification or after a directory separator
      result.Append("(?:^|[A-Z]\\:|").Append(directorySeparators).Append(')');

      const string MARKER_ANY_DIRS = "::\0\0";
      const string MARKER_ANY_FILE = "::\0\x01";
      const string MARKER_ANY_NAME = "::\0\x02";
      const string MARKER_ANY_SEPA = "::\0\x03";
      const string MARKER_ANY_NONE = "::\0\x04";

      text = text
        .Replace("**/", MARKER_ANY_DIRS)
        .Replace("**\\", MARKER_ANY_DIRS)
        .Replace("*", MARKER_ANY_FILE)
        .Replace("?", MARKER_ANY_NAME)
        .Replace("\\", MARKER_ANY_SEPA)
        .Replace("/", MARKER_ANY_SEPA)
      ;

      if (endsWithoutExtension)
        text = text.ReplaceLast(MARKER_ANY_FILE, MARKER_ANY_NONE, StringComparison.OrdinalIgnoreCase);

      var regexPattern = Regex.Escape(text);
      regexPattern = regexPattern
        .Replace(MARKER_ANY_DIRS, $"(?:.*{directorySeparators})?")
        .Replace(MARKER_ANY_FILE, $"{notDirectorySeparators}*?")
        .Replace(MARKER_ANY_NAME, notDirectorySeparators)
        .Replace(MARKER_ANY_SEPA, directorySeparators)
        .Replace(MARKER_ANY_NONE, $"[^.{dirSeps}]*?")
      ;

      result.Append(regexPattern);

      // filenames end at the end or with an extension
      result.Append(endsWithAnyExtension ? "(?:$|\\.[^.]*?)" : endsWithoutExtension ? "(?!\\.[^.]*?)$" : "$");

      return result.ToString();
    }

    static bool IsFileSystemCaseSensitive() {
      using var tempFile = PathExtensions.GetTempFileToken("test");
      return File.Exists(tempFile.File.FullName.ToUpperInvariant());
    }

  }

  /// <summary>
  ///   Determines if the given string matches a given file pattern or not.
  /// </summary>
  /// <param name="this">This String.</param>
  /// <param name="pattern">The pattern to apply.</param>
  /// <returns><c>true</c> if the string matches the file pattern; otherwise, <c>false</c>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool MatchesFilePattern(this string @this, string pattern) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrEmpty(pattern);

    Regex illegalFilenameCharacters = StaticMethodLocal<Regex>.GetOrAdd(() => new("[" + @"\/:<>|" + "\"]", RegexOptions.Compiled | RegexOptions.ExplicitCapture));
    if (illegalFilenameCharacters.IsMatch(pattern))
      AlwaysThrow.ArgumentException(nameof(pattern), "Patterns contains ilegal characters.");

    return ConvertFilePatternToRegex(pattern).IsMatch(@this);
  }

  /// <summary>
  ///   Equivalent to SQL LIKE Statement.
  /// </summary>
  /// <param name="this">The text to search.</param>
  /// <param name="toFind">The text to find.</param>
  /// <returns>True if the LIKE matched.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool Like(this string @this, string toFind) => new Regex(
      @"\A"
      + ((Regex)StaticMethodLocal<Regex>.GetOrAdd(() => new(@"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\", RegexOptions.Compiled)))
      .Replace(toFind, ch => @"\" + ch)
      .Replace('_', '.')
      .Replace("%", ".*")
      + @"\z",
      RegexOptions.Singleline
    )
    .IsMatch(@this)
  ;
  
  #endregion

  /// <summary>
  ///   Converts a string to a <a href="https://en.wikipedia.org/wiki/Quoted-printable">quoted-printable</a> version of it.
  /// </summary>
  /// <param name="this">The string to convert</param>
  /// <returns>The quoted-printable string</returns>
  public static string ToQuotedPrintable(this string @this) {
    if (IsNullOrEmpty(@this))
      return @this;

    var bytes = Encoding.UTF8.GetBytes(@this);
    StringBuilder result = new(bytes.Length * 2);
    foreach (var ch in bytes)
      if (ch is < 32 or > 126 || ch == '=') {
        ByteToHex2(ch, out var high, out var low);
        result.Append('=');
        result.Append(high);
        result.Append(low);
      } else
        result.Append((char)ch);

    return result.ToString();

    // see https://github.com/dotnet/runtime/blob/v5.0.3/src/libraries/Common/src/System/HexConverter.cs for the inner workings
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void ByteToHex2(byte value, out char highNibble, out char lowNibble) {
      var temp = (uint)(((value & 0xF0) << 4) + (value & 0xF) - 0x8989);
      temp = (uint)(((-(int)temp & 0x7070) >>> 4) + (int)temp + 0xB9B9);
      lowNibble = (char)(temp & 0xff);
      highNibble = (char)(temp >> 8);
    }

  }

  internal static byte ConvertHexToByte(char highNibble, char lowNibble) {
    byte[] lookupTable= StaticMethodLocal<byte[]>.GetOrAdd(CreateTable);
    var high = lookupTable[highNibble];
    var low = lookupTable[lowNibble];
    if ((high | low) <= 0x0f)
      return (byte)((high << 4) | low);

    if (high > 0xf)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(highNibble), $"'{highNibble}' is not a hexadecimal digit");
    else
      AlwaysThrow.ArgumentOutOfRangeException(nameof(lowNibble), $"'{lowNibble}' is not a hexadecimal digit");

    return (byte)((high << 4) | low);

    static unsafe byte[] CreateTable() {
      var table = new byte[256];
      table.AsSpan().Fill(0xff); // 0xff means "invalid character"

      // filling in the valid chars
      fixed (byte* ptr = &table[0]) {
        *(ulong*)&ptr['0'] = 0x0706050403020100U;
        *(ushort*)&ptr['8'] = 0x0908;

        *(uint*)&ptr['A'] = 0x0D0C0B0AU;
        *(ushort*)&ptr['E'] = 0x0F0E;

        *(uint*)&ptr['a'] = 0x0D0C0B0AU;
        *(ushort*)&ptr['e'] = 0x0F0E;
      }

      return table;
    }

  }

  /// <summary>
  ///   Converts a <a href="https://en.wikipedia.org/wiki/Quoted-printable">quoted-printable</a> string to a normal version
  ///   of it.
  /// </summary>
  /// <param name="this">The string to convert</param>
  /// <returns>The non-quoted-printable string</returns>
  public static string FromQuotedPrintable(this string @this) {
    if (IsNullOrEmpty(@this))
      return @this;

    List<byte> bytes = new(@this.Length);
    for (var index = 0; index < @this.Length; ++index)
      if (@this[index] == '=')
        bytes.Add(ConvertHexToByte(@this[++index], @this[++index]));
      else
        bytes.Add((byte)@this[index]);

    return Encoding.UTF8.GetString(bytes.ToArray());
  }

}
