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

namespace System;

/// <summary>
/// Shared encode/decode routines for Posit (unum type III) numbers, parameterized by nbits and es.
/// All bit manipulation works on a <see cref="uint"/> field holding the low <c>nbits</c> bits.
/// </summary>
internal static class PositCodec {

  /// <summary>
  /// Decodes a raw posit bit pattern of width <paramref name="nbits"/> with exponent-size <paramref name="es"/> to a double.
  /// </summary>
  public static double Decode(uint x, int nbits, int es) {
    var mask = (nbits == 32) ? 0xFFFFFFFFu : ((1u << nbits) - 1u);
    x &= mask;

    var signMask = 1u << (nbits - 1);

    // Special patterns.
    if (x == 0)
      return 0.0;
    if (x == signMask)
      return double.NaN; // NaR

    var negative = (x & signMask) != 0;
    if (negative)
      x = (~x + 1u) & mask; // two's complement within nbits

    // Bits below the sign bit, MSB-first. There are (nbits - 1) such bits.
    var remaining = nbits - 1;

    // The bit just below the sign bit.
    var firstRegimeBit = (x >> (remaining - 1)) & 1u;

    // Count the run of identical bits, scanning from MSB toward LSB.
    var runLength = 1;
    var pos = remaining - 2; // next bit position below the first regime bit
    while (pos >= 0 && ((x >> pos) & 1u) == firstRegimeBit) {
      ++runLength;
      --pos;
    }

    // Regime value k.
    var k = firstRegimeBit == 1u ? runLength - 1 : -runLength;

    // Consume the regime run plus one terminating bit (the opposite bit), if present.
    // Bits consumed so far from the (nbits-1) field: runLength + (terminator present ? 1 : 0).
    var consumed = runLength;
    if (pos >= 0)
      ++consumed; // terminating bit exists

    // Number of bits left for exponent + fraction.
    var bitsLeft = remaining - consumed;

    // Read es exponent bits (MSB-first), padding with zeros on the right when fewer remain.
    var e = 0u;
    var exponentBitsAvailable = bitsLeft < es ? bitsLeft : es;
    // The next bits start at position (bitsLeft - 1) going down.
    var bitCursor = bitsLeft - 1;
    for (var i = 0; i < es; ++i) {
      e <<= 1;
      if (i < exponentBitsAvailable) {
        e |= (x >> bitCursor) & 1u;
        --bitCursor;
      }
      // else: padded with zero (already shifted in a 0)
    }

    // Fraction: remaining bits after the exponent.
    var numFractionBits = bitsLeft - exponentBitsAvailable;
    var fractionBits = 0u;
    for (var i = 0; i < numFractionBits; ++i) {
      fractionBits <<= 1;
      fractionBits |= (x >> bitCursor) & 1u;
      --bitCursor;
    }

    var significand = 1.0 + (numFractionBits > 0 ? fractionBits / Math.Pow(2, numFractionBits) : 0.0);

    // value = significand * 2^(k * 2^es + e)   (useed = 2^(2^es))
    var scale = k * (1 << es) + (int)e;
    var value = significand * Math.Pow(2, scale);

    return negative ? -value : value;
  }

  /// <summary>
  /// Encodes a real value into a raw posit bit pattern of width <paramref name="nbits"/> with exponent-size <paramref name="es"/>,
  /// using round-to-nearest-even. NaN/Infinity map to NaR; zero maps to zero. Out-of-range values clamp to max/min magnitude.
  /// </summary>
  public static uint Encode(double v, int nbits, int es) {
    var mask = (nbits == 32) ? 0xFFFFFFFFu : ((1u << nbits) - 1u);
    var signMask = 1u << (nbits - 1);

    if (double.IsNaN(v) || double.IsInfinity(v))
      return signMask; // NaR
    if (v == 0.0)
      return 0u;

    var negative = v < 0.0;
    var a = Math.Abs(v);

    // total scale m = floor(log2(a)); split into k and e.
    var twoPowEs = 1 << es;
    var m = (int)Math.Floor(Math.Log(a, 2));

    // Guard against log2 rounding error: ensure the significand a/2^m lands in [1,2).
    var significand = a / Math.Pow(2, m);
    if (significand >= 2.0) {
      ++m;
      significand = a / Math.Pow(2, m);
    } else if (significand < 1.0) {
      --m;
      significand = a / Math.Pow(2, m);
    }

    // Decompose m = k * 2^es + e with 0 <= e < 2^es.
    var k = (int)Math.Floor((double)m / twoPowEs);
    var e = m - k * twoPowEs;

    var fraction = significand - 1.0; // in [0,1)

    // Build the magnitude pattern bit-by-bit (MSB-first) into a list of bits, then round.
    // We assemble into a wide accumulator with extra guard bits, then round-to-nearest-even
    // down to nbits.

    // Regime bits:
    //   k >= 0 => (k+1) ones followed by a terminating zero.
    //   k <  0 => (-k) zeros followed by a terminating one.

    // Total useful bit budget excluding the sign bit.
    var fieldBits = nbits - 1;

    // We construct an unbounded conceptual bit string:
    //   [regime bits][es exponent bits][fraction bits...]
    // then round it to fieldBits keeping round-to-nearest-even, then prepend the sign bit (0 here; magnitude only).

    // Use a 64-bit accumulator: place the constructed bits left-aligned with plenty of room,
    // but it's simpler to build the exact field value plus a remainder to decide rounding.

    const int GuardBits = 4;

    // We produce the magnitude bits sequentially (MSB-first), packing the first 'fieldBits' of them
    // into 'field' (bit fieldBits-1 = MSB). The first bit beyond the field is the round bit; any further
    // set bit makes 'sticky' true. This enables round-to-nearest-even without a big-integer buffer.

    // Sequential bit generator state.
    var produced = 0; // number of magnitude bits produced toward the field (capped)
    ulong field = 0; // packed field, MSB at bit (fieldBits-1)
    var roundBit = 0; // first bit beyond the field
    var sticky = false; // any bit beyond the round bit set
    var roundDecided = false;

    void Emit(int bit) {
      if (produced < fieldBits) {
        field |= (ulong)(uint)bit << (fieldBits - 1 - produced);
        ++produced;
      } else if (!roundDecided) {
        roundBit = bit;
        roundDecided = true;
      } else if (bit != 0)
        sticky = true;
    }

    // Regime.
    if (k >= 0) {
      for (var i = 0; i < k + 1; ++i)
        Emit(1);
      Emit(0); // terminator
    } else {
      for (var i = 0; i < -k; ++i)
        Emit(0);
      Emit(1); // terminator
    }

    // Exponent bits (es of them), MSB-first.
    for (var i = es - 1; i >= 0; --i)
      Emit((e >> i) & 1);

    // Fraction bits: keep emitting until we've filled the field and have round+sticky resolved.
    // We stop once we've produced enough to decide rounding plus a little slack.
    var fracIterations = fieldBits + GuardBits + 4;
    var frac = fraction;
    for (var i = 0; i < fracIterations; ++i) {
      frac *= 2.0;
      var bit = 0;
      if (frac >= 1.0) {
        bit = 1;
        frac -= 1.0;
      }
      Emit(bit);
      // Once field is full and round decided, additional nonzero bits only affect sticky.
      if (produced >= fieldBits && roundDecided && (sticky || frac == 0.0))
        break;
    }
    if (frac != 0.0)
      sticky = true;

    // Round-to-nearest-even on 'field' using roundBit/sticky.
    var fieldMask = fieldBits >= 64 ? ulong.MaxValue : ((1UL << fieldBits) - 1UL);
    field &= fieldMask;
    if (roundBit == 1 && (sticky || (field & 1UL) != 0))
      ++field;

    uint magnitude;
    if (field > fieldMask) {
      // Rounding overflowed the field; clamp to max representable magnitude.
      magnitude = (uint)(signMask - 1u); // 0b0111...1
    } else
      magnitude = (uint)(field & fieldMask);

    // A magnitude that rounded down to 0 is correct: values below half of minpos round to zero
    // (round-to-nearest). Values in [minpos/2, minpos) are rounded up to minpos by the round bit above.

    // Clamp magnitude to maxpos (= signMask-1); posits have no infinity, so overflow saturates.
    if (magnitude > signMask - 1u)
      magnitude = signMask - 1u;

    var result = negative ? ((~magnitude + 1u) & mask) : (magnitude & mask);
    return result & mask;
  }

}
