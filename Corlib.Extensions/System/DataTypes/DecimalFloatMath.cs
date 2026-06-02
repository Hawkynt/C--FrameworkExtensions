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
using System.Numerics;
using System.Text;

namespace System;

/// <summary>
/// Shared engine for IEEE-754 decimal floating-point in the BID (Binary Integer Decimal) encoding.
/// Operates on a decomposed value (sign, integer coefficient, base-10 exponent, kind) using
/// <see cref="BigInteger"/> so arithmetic is exact in base 10 (e.g. 0.1 + 0.2 == 0.3 exactly).
/// Parameterized by <see cref="DecimalFormat"/> so it serves both decimal32 and decimal64.
/// </summary>
internal enum DecimalKind { Finite, Infinity, NaN }

internal readonly struct DecimalFormat {
  public readonly int TotalBits;
  public readonly int Precision;   // significant decimal digits (p)
  public readonly int Bias;        // exponent bias for the integer-coefficient form
  public readonly int ExpBits;     // 2 + exponent-continuation bits
  public readonly int MinQ;        // emin - (p-1)
  public readonly int MaxQ;        // emax - (p-1)

  public DecimalFormat(int totalBits, int precision, int bias, int expBits, int minQ, int maxQ) {
    this.TotalBits = totalBits;
    this.Precision = precision;
    this.Bias = bias;
    this.ExpBits = expBits;
    this.MinQ = minQ;
    this.MaxQ = maxQ;
  }

  public static readonly DecimalFormat D32 = new(32, 7, 101, 8, -101, 90);
  public static readonly DecimalFormat D64 = new(64, 16, 398, 10, -398, 369);
}

internal static class DecimalFloatMath {

  // ---- decomposed value ----
  internal readonly struct Value {
    public readonly int Sign;            // 0 = positive, 1 = negative
    public readonly BigInteger Coeff;    // >= 0
    public readonly int Q;               // base-10 exponent: value = (-1)^Sign * Coeff * 10^Q
    public readonly DecimalKind Kind;

    public Value(int sign, BigInteger coeff, int q, DecimalKind kind) {
      this.Sign = sign;
      this.Coeff = coeff;
      this.Q = q;
      this.Kind = kind;
    }

    public bool IsZero => this.Kind == DecimalKind.Finite && this.Coeff.IsZero;
  }

  private static int _Digits(BigInteger c) => c.IsZero ? 1 : c.ToString(CultureInfo.InvariantCulture).Length;

  // ---- BID decode / encode ----

  public static Value Decode(ulong raw, in DecimalFormat fmt) {
    var signShift = fmt.TotalBits - 1;
    var sign = (int)((raw >> signShift) & 1);
    var g2 = (raw >> (fmt.TotalBits - 3)) & 3; // two bits below the sign

    var expMask = (1UL << fmt.ExpBits) - 1;
    var threshold = BigInteger.One << (fmt.TotalBits - 1 - fmt.ExpBits);

    if (g2 != 3) {
      var e = (int)((raw >> (fmt.TotalBits - 1 - fmt.ExpBits)) & expMask);
      var c = (BigInteger)(raw & (ulong)(threshold - 1));
      return new(sign, c, e - fmt.Bias, DecimalKind.Finite);
    }

    var top5 = (raw >> (fmt.TotalBits - 6)) & 0x1F;
    if (top5 == 0x1E)
      return new(sign, BigInteger.Zero, 0, DecimalKind.Infinity);
    if (top5 == 0x1F)
      return new(sign, BigInteger.Zero, 0, DecimalKind.NaN);

    // form 2: implied "100" coefficient prefix
    var e2 = (int)((raw >> (fmt.TotalBits - 3 - fmt.ExpBits)) & expMask);
    var fieldMask = (1UL << (fmt.TotalBits - 3 - fmt.ExpBits)) - 1;
    var c2 = threshold + (raw & fieldMask);
    return new(sign, c2, e2 - fmt.Bias, DecimalKind.Finite);
  }

  public static ulong Encode(Value v, in DecimalFormat fmt) {
    var signBit = (ulong)v.Sign << (fmt.TotalBits - 1);
    switch (v.Kind) {
      case DecimalKind.NaN: return signBit | (0x1FUL << (fmt.TotalBits - 6));
      case DecimalKind.Infinity: return signBit | (0x1EUL << (fmt.TotalBits - 6));
    }

    var c = v.Coeff;
    var q = v.Q;
    _RoundAndClamp(ref c, ref q, fmt, out var overflow);
    if (overflow)
      return signBit | (0x1EUL << (fmt.TotalBits - 6)); // overflow -> infinity

    var e = (ulong)(q + fmt.Bias);
    var threshold = BigInteger.One << (fmt.TotalBits - 1 - fmt.ExpBits);
    if (c < threshold)
      return signBit | (e << (fmt.TotalBits - 1 - fmt.ExpBits)) | (ulong)c;

    return signBit | (0x3UL << (fmt.TotalBits - 3)) | (e << (fmt.TotalBits - 3 - fmt.ExpBits)) | (ulong)(c - threshold);
  }

  // round the coefficient to <= precision digits (half-even) and bring q into range
  private static void _RoundAndClamp(ref BigInteger c, ref int q, in DecimalFormat fmt, out bool overflow) {
    overflow = false;
    if (c.IsZero) {
      q = q < fmt.MinQ ? fmt.MinQ : q > fmt.MaxQ ? fmt.MaxQ : q;
      return;
    }

    _RoundToDigits(ref c, ref q, fmt.Precision);

    // exponent too large: try to absorb by appending zeros (keeping <= precision digits)
    while (q > fmt.MaxQ && _Digits(c) < fmt.Precision) {
      c *= 10;
      --q;
    }

    if (q > fmt.MaxQ) {
      overflow = true;
      return;
    }

    // exponent too small (subnormal / underflow): drop precision down to MinQ
    if (q < fmt.MinQ) {
      var drop = fmt.MinQ - q;
      _DropDigits(ref c, drop);
      q = fmt.MinQ;
    }
  }

  private static void _RoundToDigits(ref BigInteger c, ref int q, int precision) {
    var digits = _Digits(c);
    if (digits <= precision)
      return;

    _DropDigits(ref c, digits - precision);
    q += digits - precision;
    if (_Digits(c) > precision) { // rounding carried (e.g. 9_999_999 + 1)
      c /= 10;
      ++q;
    }
  }

  private static void _DropDigits(ref BigInteger c, int drop) {
    if (drop <= 0)
      return;
    var pow = BigInteger.Pow(10, drop);
    var rounded = BigInteger.DivRem(c, pow, out var rem);
    var twice = rem * 2;
    if (twice > pow || (twice == pow && !rounded.IsEven))
      rounded += BigInteger.One;
    c = rounded;
  }

  // ---- arithmetic ----

  public static Value Add(Value a, Value b, in DecimalFormat fmt) {
    if (a.Kind == DecimalKind.NaN || b.Kind == DecimalKind.NaN)
      return _NaN;
    if (a.Kind == DecimalKind.Infinity || b.Kind == DecimalKind.Infinity) {
      if (a.Kind == DecimalKind.Infinity && b.Kind == DecimalKind.Infinity)
        return a.Sign == b.Sign ? a : _NaN; // inf + (-inf) = NaN
      return a.Kind == DecimalKind.Infinity ? a : b;
    }

    var q = Math.Min(a.Q, b.Q);
    var ca = a.Coeff * BigInteger.Pow(10, a.Q - q) * (a.Sign == 0 ? 1 : -1);
    var cb = b.Coeff * BigInteger.Pow(10, b.Q - q) * (b.Sign == 0 ? 1 : -1);
    var sum = ca + cb;
    var sign = sum.Sign < 0 ? 1 : 0;
    return new(sign, BigInteger.Abs(sum), q, DecimalKind.Finite);
  }

  public static Value Negate(Value a) => a.Kind == DecimalKind.NaN ? a : new(a.Sign ^ 1, a.Coeff, a.Q, a.Kind);

  public static Value Subtract(Value a, Value b, in DecimalFormat fmt) => Add(a, Negate(b), fmt);

  public static Value Multiply(Value a, Value b, in DecimalFormat fmt) {
    if (a.Kind == DecimalKind.NaN || b.Kind == DecimalKind.NaN)
      return _NaN;
    var sign = a.Sign ^ b.Sign;
    if (a.Kind == DecimalKind.Infinity || b.Kind == DecimalKind.Infinity)
      return a.IsZero || b.IsZero ? _NaN : new(sign, BigInteger.Zero, 0, DecimalKind.Infinity);
    return new(sign, a.Coeff * b.Coeff, a.Q + b.Q, DecimalKind.Finite);
  }

  public static Value Divide(Value a, Value b, in DecimalFormat fmt) {
    if (a.Kind == DecimalKind.NaN || b.Kind == DecimalKind.NaN)
      return _NaN;
    var sign = a.Sign ^ b.Sign;
    if (a.Kind == DecimalKind.Infinity && b.Kind == DecimalKind.Infinity)
      return _NaN;
    if (a.Kind == DecimalKind.Infinity)
      return new(sign, BigInteger.Zero, 0, DecimalKind.Infinity);
    if (b.Kind == DecimalKind.Infinity)
      return new(sign, BigInteger.Zero, 0, DecimalKind.Finite);
    if (b.IsZero)
      return a.IsZero ? _NaN : new(sign, BigInteger.Zero, 0, DecimalKind.Infinity); // x/0 -> inf, 0/0 -> NaN
    if (a.IsZero)
      return new(sign, BigInteger.Zero, a.Q - b.Q, DecimalKind.Finite);

    // produce precision + 2 guard digits, then round
    var extra = fmt.Precision + 2;
    var scaled = a.Coeff * BigInteger.Pow(10, extra);
    var quotient = BigInteger.DivRem(scaled, b.Coeff, out var rem);
    var q = a.Q - b.Q - extra;
    if (!rem.IsZero) {
      quotient = quotient * 10 + 1; // sticky digit so rounding sees the result is inexact
      --q;
    }

    _RoundToDigits(ref quotient, ref q, fmt.Precision);

    // remove trailing zeros down to the ideal exponent (a.Q - b.Q): 5 / 2 -> 2.5, not 2.500000
    var ideal = a.Q - b.Q;
    while (q < ideal && !quotient.IsZero && (quotient % 10).IsZero) {
      quotient /= 10;
      ++q;
    }

    return new(sign, quotient, q, DecimalKind.Finite);
  }

  public static int Compare(Value a, Value b, in DecimalFormat fmt) {
    var diff = Subtract(a, b, fmt);
    if (diff.IsZero)
      return 0;
    return diff.Sign == 0 ? 1 : -1;
  }

  private static readonly Value _NaN = new(0, BigInteger.Zero, 0, DecimalKind.NaN);

  // ---- text ----

  public static string ToString(Value v) {
    switch (v.Kind) {
      case DecimalKind.NaN: return "NaN";
      case DecimalKind.Infinity: return v.Sign == 0 ? "Infinity" : "-Infinity";
    }

    var digits = v.Coeff.ToString(CultureInfo.InvariantCulture);
    var sb = new StringBuilder();
    if (v.Sign != 0)
      sb.Append('-');

    if (v.Q >= 0) {
      sb.Append(digits);
      sb.Append('0', v.Q);
    } else {
      var pointFromRight = -v.Q;
      if (digits.Length <= pointFromRight) {
        sb.Append("0.");
        sb.Append('0', pointFromRight - digits.Length);
        sb.Append(digits);
      } else {
        var split = digits.Length - pointFromRight;
        sb.Append(digits, 0, split);
        sb.Append('.');
        sb.Append(digits, split, digits.Length - split);
      }
    }

    return sb.ToString();
  }

  public static Value Parse(string s, in DecimalFormat fmt) {
    s = s.Trim();
    if (s.Length == 0)
      throw new FormatException("Empty decimal string.");

    var sign = 0;
    var i = 0;
    if (s[0] is '+' or '-') {
      sign = s[0] == '-' ? 1 : 0;
      i = 1;
    }

    var rest = s.Substring(i);
    if (string.Equals(rest, "Infinity", StringComparison.OrdinalIgnoreCase) || string.Equals(rest, "Inf", StringComparison.OrdinalIgnoreCase))
      return new(sign, BigInteger.Zero, 0, DecimalKind.Infinity);
    if (string.Equals(rest, "NaN", StringComparison.OrdinalIgnoreCase))
      return new(sign, BigInteger.Zero, 0, DecimalKind.NaN);

    var exp = 0;
    var ePos = rest.IndexOfAny(['e', 'E']);
    if (ePos >= 0) {
      exp = int.Parse(rest.Substring(ePos + 1), CultureInfo.InvariantCulture);
      rest = rest.Substring(0, ePos);
    }

    var dot = rest.IndexOf('.');
    var fractionDigits = 0;
    if (dot >= 0) {
      fractionDigits = rest.Length - dot - 1;
      rest = rest.Remove(dot, 1);
    }

    if (rest.Length == 0)
      throw new FormatException($"Invalid decimal string: '{s}'.");

    var coeff = BigInteger.Parse(rest, CultureInfo.InvariantCulture);
    return new(sign, coeff, exp - fractionDigits, DecimalKind.Finite);
  }

  public static double ToDouble(Value v) => v.Kind switch {
    DecimalKind.NaN => double.NaN,
    DecimalKind.Infinity => v.Sign == 0 ? double.PositiveInfinity : double.NegativeInfinity,
    _ => (v.Sign == 0 ? 1.0 : -1.0) * (double)v.Coeff * Math.Pow(10, v.Q),
  };
}
