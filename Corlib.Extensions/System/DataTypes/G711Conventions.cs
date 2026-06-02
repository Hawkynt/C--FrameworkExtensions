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

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

/// <summary>
/// A G.711 companding convention: how a 16-bit linear PCM sample is quantized to a µ-law / A-law code.
/// </summary>
/// <remarks>
/// G.711 decoding is identical across conventions, but encoding (quantization) differs near full scale.
/// Implemented by zero-size value types (<see cref="SunG711"/>, <see cref="ItuG711"/>) so the JIT can inline
/// the chosen convention when used as the <c>TConvention</c> type argument of
/// <see cref="MuLaw.FromPcm16{TConvention}"/> / <see cref="ALaw.FromPcm16{TConvention}"/> — no runtime branch.
/// </remarks>
public interface IG711Convention {
  /// <summary>Quantizes a 16-bit linear PCM sample to a µ-law code.</summary>
  byte EncodeMuLaw(short pcm);

  /// <summary>Quantizes a 16-bit linear PCM sample to an A-law code.</summary>
  byte EncodeALaw(short pcm);
}

/// <summary>
/// The Sun / Reese-Campbell public-domain G.711 convention (full-scale, widely used in software).
/// </summary>
public readonly struct SunG711 : IG711Convention {
  private const int MuBias = 0x84;
  private const int MuClip = 32635;

  private static readonly byte[] _muExponent = [
    0, 0, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3,
    4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
    5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
    5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
    6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
    6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
    6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
    6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
    7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
    7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
    7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
    7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
    7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
    7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
    7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
    7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7
  ];

  private static readonly int[] _alawSegEnd = [0x1F, 0x3F, 0x7F, 0xFF, 0x1FF, 0x3FF, 0x7FF, 0xFFF];

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public byte EncodeMuLaw(short pcm) {
    var sign = (pcm >> 8) & 0x80;
    var sample = sign != 0 ? -pcm : pcm;
    if (sample > MuClip)
      sample = MuClip;
    sample += MuBias;

    var exponent = _muExponent[(sample >> 7) & 0xFF];
    var mantissa = (sample >> (exponent + 3)) & 0x0F;
    return (byte)~(sign | (exponent << 4) | mantissa);
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public byte EncodeALaw(short pcm) {
    var sample = pcm >> 3;
    int mask;
    if (sample >= 0)
      mask = 0xD5;
    else {
      mask = 0x55;
      sample = -sample - 1;
    }

    var seg = 0;
    while (seg < _alawSegEnd.Length && sample > _alawSegEnd[seg])
      ++seg;
    if (seg >= 8)
      return (byte)(0x7F ^ mask);

    var code = seg << 4;
    code |= seg < 2 ? (sample >> 1) & 0x0F : (sample >> seg) & 0x0F;
    return (byte)(code ^ mask);
  }
}

/// <summary>
/// The ITU-T G.191 STL reference G.711 convention (the telecom-standard quantizer).
/// </summary>
public readonly struct ItuG711 : IG711Convention {
  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public byte EncodeMuLaw(short pcm) {
    var absno = pcm < 0 ? ((~pcm) >> 2) + 33 : (pcm >> 2) + 33;
    if (absno > 0x1FFF)
      absno = 0x1FFF;

    var i = absno >> 6;
    var segno = 1;
    while (i != 0) {
      ++segno;
      i >>= 1;
    }

    var high = 8 - segno;
    var low = 0x0F - ((absno >> segno) & 0x0F);
    var code = (high << 4) | low;
    if (pcm >= 0)
      code |= 0x80;
    return (byte)code;
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public byte EncodeALaw(short pcm) {
    var ix = pcm < 0 ? (~pcm) >> 4 : pcm >> 4;
    if (ix > 15) {
      var iexp = 1;
      while (ix > 16 + 15) {
        ix >>= 1;
        ++iexp;
      }

      ix -= 16;
      ix += iexp << 4;
    }

    if (pcm >= 0)
      ix |= 0x80;
    return (byte)(ix ^ 0x55);
  }
}
