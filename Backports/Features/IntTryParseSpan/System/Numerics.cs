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

#if !SUPPORTS_NUMERIC_PARSE_SPAN

using System.Globalization;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class Int32Polyfills {

  extension(int) {

    /// <summary>
    /// Converts the span representation of a number to its 32-bit signed integer equivalent.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <returns>A 32-bit signed integer equivalent to the number contained in <paramref name="s"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Parse(ReadOnlySpan<char> s)
      => int.Parse(s.ToString());

    /// <summary>
    /// Converts the span representation of a number in a specified style to its 32-bit signed integer equivalent.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="style">A bitwise combination of enumeration values that indicates the style elements.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <returns>A 32-bit signed integer equivalent to the number contained in <paramref name="s"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
      => int.Parse(s.ToString(), style, provider);

    /// <summary>
    /// Converts the span representation of a number to its 32-bit signed integer equivalent. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="result">When this method returns, contains the 32-bit signed integer equivalent of the number contained in <paramref name="s"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> s, out int result)
      => int.TryParse(s.ToString(), out result);

    /// <summary>
    /// Converts the span representation of a number in a specified style and culture-specific format to its 32-bit signed integer equivalent. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="style">A bitwise combination of enumeration values that indicates the style elements.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="result">When this method returns, contains the 32-bit signed integer equivalent of the number contained in <paramref name="s"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out int result)
      => int.TryParse(s.ToString(), style, provider, out result);

  }

}

public static partial class Int64Polyfills {

  extension(long) {

    /// <summary>
    /// Converts the span representation of a number to its 64-bit signed integer equivalent.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <returns>A 64-bit signed integer equivalent to the number contained in <paramref name="s"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Parse(ReadOnlySpan<char> s)
      => long.Parse(s.ToString());

    /// <summary>
    /// Converts the span representation of a number in a specified style to its 64-bit signed integer equivalent.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="style">A bitwise combination of enumeration values that indicates the style elements.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <returns>A 64-bit signed integer equivalent to the number contained in <paramref name="s"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
      => long.Parse(s.ToString(), style, provider);

    /// <summary>
    /// Converts the span representation of a number to its 64-bit signed integer equivalent. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="result">When this method returns, contains the 64-bit signed integer equivalent of the number contained in <paramref name="s"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> s, out long result)
      => long.TryParse(s.ToString(), out result);

    /// <summary>
    /// Converts the span representation of a number in a specified style and culture-specific format to its 64-bit signed integer equivalent. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="style">A bitwise combination of enumeration values that indicates the style elements.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="result">When this method returns, contains the 64-bit signed integer equivalent of the number contained in <paramref name="s"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out long result)
      => long.TryParse(s.ToString(), style, provider, out result);

  }

}

public static partial class DoublePolyfills {

  extension(double) {

    /// <summary>
    /// Converts the span representation of a number to its double-precision floating-point equivalent.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <returns>A double-precision floating-point number equivalent to the number contained in <paramref name="s"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Parse(ReadOnlySpan<char> s)
      => double.Parse(s.ToString());

    /// <summary>
    /// Converts the span representation of a number in a specified style to its double-precision floating-point equivalent.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="style">A bitwise combination of enumeration values that indicates the style elements.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <returns>A double-precision floating-point number equivalent to the number contained in <paramref name="s"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
      => double.Parse(s.ToString(), style, provider);

    /// <summary>
    /// Converts the span representation of a number to its double-precision floating-point equivalent. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="result">When this method returns, contains the double-precision floating-point equivalent of the number contained in <paramref name="s"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> s, out double result)
      => double.TryParse(s.ToString(), out result);

    /// <summary>
    /// Converts the span representation of a number in a specified style and culture-specific format to its double-precision floating-point equivalent. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="style">A bitwise combination of enumeration values that indicates the style elements.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="result">When this method returns, contains the double-precision floating-point equivalent of the number contained in <paramref name="s"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out double result)
      => double.TryParse(s.ToString(), style, provider, out result);

  }

}

public static partial class DecimalPolyfills {

  extension(decimal) {

    /// <summary>
    /// Converts the span representation of a number to its <see cref="decimal"/> equivalent.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <returns>A <see cref="decimal"/> equivalent to the number contained in <paramref name="s"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal Parse(ReadOnlySpan<char> s)
      => decimal.Parse(s.ToString());

    /// <summary>
    /// Converts the span representation of a number in a specified style to its <see cref="decimal"/> equivalent.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="style">A bitwise combination of enumeration values that indicates the style elements.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <returns>A <see cref="decimal"/> equivalent to the number contained in <paramref name="s"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
      => decimal.Parse(s.ToString(), style, provider);

    /// <summary>
    /// Converts the span representation of a number to its <see cref="decimal"/> equivalent. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="result">When this method returns, contains the <see cref="decimal"/> equivalent of the number contained in <paramref name="s"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> s, out decimal result)
      => decimal.TryParse(s.ToString(), out result);

    /// <summary>
    /// Converts the span representation of a number in a specified style and culture-specific format to its <see cref="decimal"/> equivalent. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="style">A bitwise combination of enumeration values that indicates the style elements.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="result">When this method returns, contains the <see cref="decimal"/> equivalent of the number contained in <paramref name="s"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out decimal result)
      => decimal.TryParse(s.ToString(), style, provider, out result);

  }

}

public static partial class SinglePolyfills {

  extension(float) {

    /// <summary>
    /// Converts the span representation of a number to its single-precision floating-point equivalent.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <returns>A single-precision floating-point number equivalent to the number contained in <paramref name="s"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Parse(ReadOnlySpan<char> s)
      => float.Parse(s.ToString());

    /// <summary>
    /// Converts the span representation of a number in a specified style to its single-precision floating-point equivalent.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="style">A bitwise combination of enumeration values that indicates the style elements.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <returns>A single-precision floating-point number equivalent to the number contained in <paramref name="s"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
      => float.Parse(s.ToString(), style, provider);

    /// <summary>
    /// Converts the span representation of a number to its single-precision floating-point equivalent. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="result">When this method returns, contains the single-precision floating-point equivalent of the number contained in <paramref name="s"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> s, out float result)
      => float.TryParse(s.ToString(), out result);

    /// <summary>
    /// Converts the span representation of a number in a specified style and culture-specific format to its single-precision floating-point equivalent. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="style">A bitwise combination of enumeration values that indicates the style elements.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="result">When this method returns, contains the single-precision floating-point equivalent of the number contained in <paramref name="s"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out float result)
      => float.TryParse(s.ToString(), style, provider, out result);

  }

}

public static partial class Int16Polyfills {

  extension(short) {

    /// <summary>
    /// Converts the span representation of a number to its 16-bit signed integer equivalent.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <returns>A 16-bit signed integer equivalent to the number contained in <paramref name="s"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short Parse(ReadOnlySpan<char> s)
      => short.Parse(s.ToString());

    /// <summary>
    /// Converts the span representation of a number in a specified style to its 16-bit signed integer equivalent.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="style">A bitwise combination of enumeration values that indicates the style elements.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <returns>A 16-bit signed integer equivalent to the number contained in <paramref name="s"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
      => short.Parse(s.ToString(), style, provider);

    /// <summary>
    /// Converts the span representation of a number to its 16-bit signed integer equivalent. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="result">When this method returns, contains the 16-bit signed integer equivalent of the number contained in <paramref name="s"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> s, out short result)
      => short.TryParse(s.ToString(), out result);

    /// <summary>
    /// Converts the span representation of a number in a specified style and culture-specific format to its 16-bit signed integer equivalent. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="style">A bitwise combination of enumeration values that indicates the style elements.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="result">When this method returns, contains the 16-bit signed integer equivalent of the number contained in <paramref name="s"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out short result)
      => short.TryParse(s.ToString(), style, provider, out result);

  }

}

public static partial class UInt16Polyfills {

  extension(ushort) {

    /// <summary>
    /// Converts the span representation of a number to its 16-bit unsigned integer equivalent.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <returns>A 16-bit unsigned integer equivalent to the number contained in <paramref name="s"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort Parse(ReadOnlySpan<char> s)
      => ushort.Parse(s.ToString());

    /// <summary>
    /// Converts the span representation of a number in a specified style to its 16-bit unsigned integer equivalent.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="style">A bitwise combination of enumeration values that indicates the style elements.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <returns>A 16-bit unsigned integer equivalent to the number contained in <paramref name="s"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
      => ushort.Parse(s.ToString(), style, provider);

    /// <summary>
    /// Converts the span representation of a number to its 16-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="result">When this method returns, contains the 16-bit unsigned integer equivalent of the number contained in <paramref name="s"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> s, out ushort result)
      => ushort.TryParse(s.ToString(), out result);

    /// <summary>
    /// Converts the span representation of a number in a specified style and culture-specific format to its 16-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="style">A bitwise combination of enumeration values that indicates the style elements.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="result">When this method returns, contains the 16-bit unsigned integer equivalent of the number contained in <paramref name="s"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out ushort result)
      => ushort.TryParse(s.ToString(), style, provider, out result);

  }

}

public static partial class UInt32Polyfills {

  extension(uint) {

    /// <summary>
    /// Converts the span representation of a number to its 32-bit unsigned integer equivalent.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <returns>A 32-bit unsigned integer equivalent to the number contained in <paramref name="s"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Parse(ReadOnlySpan<char> s)
      => uint.Parse(s.ToString());

    /// <summary>
    /// Converts the span representation of a number in a specified style to its 32-bit unsigned integer equivalent.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="style">A bitwise combination of enumeration values that indicates the style elements.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <returns>A 32-bit unsigned integer equivalent to the number contained in <paramref name="s"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
      => uint.Parse(s.ToString(), style, provider);

    /// <summary>
    /// Converts the span representation of a number to its 32-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="result">When this method returns, contains the 32-bit unsigned integer equivalent of the number contained in <paramref name="s"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> s, out uint result)
      => uint.TryParse(s.ToString(), out result);

    /// <summary>
    /// Converts the span representation of a number in a specified style and culture-specific format to its 32-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="style">A bitwise combination of enumeration values that indicates the style elements.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="result">When this method returns, contains the 32-bit unsigned integer equivalent of the number contained in <paramref name="s"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out uint result)
      => uint.TryParse(s.ToString(), style, provider, out result);

  }

}

public static partial class UInt64Polyfills {

  extension(ulong) {

    /// <summary>
    /// Converts the span representation of a number to its 64-bit unsigned integer equivalent.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <returns>A 64-bit unsigned integer equivalent to the number contained in <paramref name="s"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Parse(ReadOnlySpan<char> s)
      => ulong.Parse(s.ToString());

    /// <summary>
    /// Converts the span representation of a number in a specified style to its 64-bit unsigned integer equivalent.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="style">A bitwise combination of enumeration values that indicates the style elements.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <returns>A 64-bit unsigned integer equivalent to the number contained in <paramref name="s"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
      => ulong.Parse(s.ToString(), style, provider);

    /// <summary>
    /// Converts the span representation of a number to its 64-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="result">When this method returns, contains the 64-bit unsigned integer equivalent of the number contained in <paramref name="s"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> s, out ulong result)
      => ulong.TryParse(s.ToString(), out result);

    /// <summary>
    /// Converts the span representation of a number in a specified style and culture-specific format to its 64-bit unsigned integer equivalent. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="style">A bitwise combination of enumeration values that indicates the style elements.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="result">When this method returns, contains the 64-bit unsigned integer equivalent of the number contained in <paramref name="s"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out ulong result)
      => ulong.TryParse(s.ToString(), style, provider, out result);

  }

}

public static partial class BytePolyfills {

  extension(byte) {

    /// <summary>
    /// Converts the span representation of a number to its <see cref="byte"/> equivalent.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <returns>A <see cref="byte"/> equivalent to the number contained in <paramref name="s"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte Parse(ReadOnlySpan<char> s)
      => byte.Parse(s.ToString());

    /// <summary>
    /// Converts the span representation of a number in a specified style to its <see cref="byte"/> equivalent.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="style">A bitwise combination of enumeration values that indicates the style elements.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <returns>A <see cref="byte"/> equivalent to the number contained in <paramref name="s"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
      => byte.Parse(s.ToString(), style, provider);

    /// <summary>
    /// Converts the span representation of a number to its <see cref="byte"/> equivalent. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="result">When this method returns, contains the <see cref="byte"/> equivalent of the number contained in <paramref name="s"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> s, out byte result)
      => byte.TryParse(s.ToString(), out result);

    /// <summary>
    /// Converts the span representation of a number in a specified style and culture-specific format to its <see cref="byte"/> equivalent. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="style">A bitwise combination of enumeration values that indicates the style elements.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="result">When this method returns, contains the <see cref="byte"/> equivalent of the number contained in <paramref name="s"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out byte result)
      => byte.TryParse(s.ToString(), style, provider, out result);

  }

}

public static partial class SBytePolyfills {

  extension(sbyte) {

    /// <summary>
    /// Converts the span representation of a number to its <see cref="sbyte"/> equivalent.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <returns>A <see cref="sbyte"/> equivalent to the number contained in <paramref name="s"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte Parse(ReadOnlySpan<char> s)
      => sbyte.Parse(s.ToString());

    /// <summary>
    /// Converts the span representation of a number in a specified style to its <see cref="sbyte"/> equivalent.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="style">A bitwise combination of enumeration values that indicates the style elements.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <returns>A <see cref="sbyte"/> equivalent to the number contained in <paramref name="s"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
      => sbyte.Parse(s.ToString(), style, provider);

    /// <summary>
    /// Converts the span representation of a number to its <see cref="sbyte"/> equivalent. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="result">When this method returns, contains the <see cref="sbyte"/> equivalent of the number contained in <paramref name="s"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> s, out sbyte result)
      => sbyte.TryParse(s.ToString(), out result);

    /// <summary>
    /// Converts the span representation of a number in a specified style and culture-specific format to its <see cref="sbyte"/> equivalent. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="s">A span containing the characters representing the number to convert.</param>
    /// <param name="style">A bitwise combination of enumeration values that indicates the style elements.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="result">When this method returns, contains the <see cref="sbyte"/> equivalent of the number contained in <paramref name="s"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out sbyte result)
      => sbyte.TryParse(s.ToString(), style, provider, out result);

  }

}

#endif
