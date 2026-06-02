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

namespace System.Text;

/// <summary>
/// ATASCII (Atari 8-bit) text encoding.
/// </summary>
/// <remarks>
/// The printable text range is exact: 0x20-0x5F and 0x61-0x7A map to their ASCII characters, so ordinary
/// text (e.g. "HELLO") encodes to the expected bytes and round-trips. The Atari-specific block-graphics and
/// control glyphs (0x00-0x1F, 0x60, 0x7B-0x7F, and the inverse-video upper half 0x80-0xFF) do not have a
/// reference-verified Unicode mapping here, so they are mapped to a private-use placeholder range (U+E0xx)
/// for lossless round-trip rather than to fabricated glyph code points. Supply a verified glyph table to
/// upgrade those to real Unicode "Symbols for Legacy Computing" glyphs.
/// </remarks>
public sealed class AtasciiEncoding : RetroSingleByteEncoding {

  private const int _GRAPHICS_PUA_BASE = 0xE000;

  private AtasciiEncoding() : base(BuildTable()) { }

  /// <summary>Gets the shared ATASCII encoding instance.</summary>
  public static AtasciiEncoding Instance { get; } = new();

  public override string EncodingName => "atascii";

  private static char[] BuildTable() {
    var table = new char[256];
    for (var b = 0; b < 256; ++b)
      table[b] = b is (>= 0x20 and <= 0x5F) or (>= 0x61 and <= 0x7A)
        ? (char)b                                // exact ASCII text range
        : (char)(_GRAPHICS_PUA_BASE + b);        // graphics/control/inverse -> PUA placeholder
    return table;
  }
}
