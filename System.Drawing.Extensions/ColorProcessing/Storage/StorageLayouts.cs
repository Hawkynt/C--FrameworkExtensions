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

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Storage;

/// <summary>
/// Compile-time channel-layout descriptors for the four 32bpp 4-channel storage
/// shapes. They share the same memory shape (32 bits, 4 bytes, 8 bits per
/// channel) — only byte ordering differs.
/// </summary>
/// <remarks>
/// <para>
/// Used by the M3/M4 fast paths to parameterise a single byte-domain implementation
/// over the four orderings via a generic <c>where TLayout : struct</c> constraint.
/// The JIT monomorphises each instantiation so the per-channel byte offsets are
/// folded to constants at code-gen time.
/// </para>
/// <para>
/// Today only <see cref="BgraLayout"/> instantiates because <c>Bgra8888</c> is the
/// only 32bpp-4ch storage type in the library. The other three exist so the
/// generic surface is ready when (if) <c>Rgba8888</c> / <c>Argb8888</c> /
/// <c>Abgr8888</c> are added.
/// </para>
/// </remarks>
internal readonly struct BgraLayout {
  public const int B = 0;
  public const int G = 1;
  public const int R = 2;
  public const int A = 3;
}

/// <summary>RGBA byte-ordering: R at 0, G at 1, B at 2, A at 3.</summary>
internal readonly struct RgbaLayout {
  public const int R = 0;
  public const int G = 1;
  public const int B = 2;
  public const int A = 3;
}

/// <summary>ARGB byte-ordering: A at 0, R at 1, G at 2, B at 3.</summary>
internal readonly struct ArgbLayout {
  public const int A = 0;
  public const int R = 1;
  public const int G = 2;
  public const int B = 3;
}

/// <summary>ABGR byte-ordering: A at 0, B at 1, G at 2, R at 3.</summary>
internal readonly struct AbgrLayout {
  public const int A = 0;
  public const int B = 1;
  public const int G = 2;
  public const int R = 3;
}

/// <summary>
/// Helpers for extracting a channel byte from a packed <see cref="uint"/> via a constant
/// byte offset. The offset is intended to be one of the <c>const int</c> members on
/// <see cref="BgraLayout"/> / <see cref="RgbaLayout"/> / <see cref="ArgbLayout"/> /
/// <see cref="AbgrLayout"/>. The JIT folds the constant shift in monomorphic generic
/// instantiations — there is no runtime cost beyond the byte extraction itself.
/// </summary>
internal static class ChannelByte {

  /// <summary>Extracts the byte at <paramref name="byteOffset"/> from <paramref name="packed"/>.</summary>
  /// <param name="packed">Little-endian packed 4-byte value (uint view of a 32bpp/4ch pixel).</param>
  /// <param name="byteOffset">0..3 byte offset within the packed value (a layout descriptor constant).</param>
  /// <returns>The selected channel byte.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte Read(uint packed, int byteOffset) => (byte)(packed >> (byteOffset * 8));
}

/// <summary>
/// Layout-parametric helpers used by the M3 byte-domain histogram fast paths across the
/// splitting-style quantizers. Each method takes a <c>where TLayout : struct</c> generic and the
/// JIT folds the layout constants for each monomorphic instantiation.
/// </summary>
/// <remarks>
/// The helpers are bit-exact substitutes for <c>color.ToNormalized().Cn.ToFloat()</c> on
/// 32bpp 4-channel storage types (today only <c>Bgra8888</c>): the byte is read directly from the
/// packed 4-byte view and converted to float via the same arithmetic the slow path uses
/// (<c>(uint)b * 0x01010101u * (1f / uint.MaxValue)</c>). No floating-point associativity changes
/// happen in the conversion itself; sums are still accumulated in <c>double</c> in caller code.
/// </remarks>
internal static class StorageLayoutFast {

  private const float _UnormToFloat = 1f / uint.MaxValue;

  /// <summary>
  /// Extracts (R, G, B, A) bytes from a packed <see cref="uint"/> view of a 32bpp 4-channel
  /// pixel using the JIT-folded byte offsets on <typeparamref name="TLayout"/>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (byte r, byte g, byte b, byte a) UnpackBytes<TLayout>(uint packed)
    where TLayout : struct {
    if (typeof(TLayout) == typeof(BgraLayout))
      return (
        (byte)(packed >> (BgraLayout.R * 8)),
        (byte)(packed >> (BgraLayout.G * 8)),
        (byte)(packed >> (BgraLayout.B * 8)),
        (byte)(packed >> (BgraLayout.A * 8))
      );
    if (typeof(TLayout) == typeof(RgbaLayout))
      return (
        (byte)(packed >> (RgbaLayout.R * 8)),
        (byte)(packed >> (RgbaLayout.G * 8)),
        (byte)(packed >> (RgbaLayout.B * 8)),
        (byte)(packed >> (RgbaLayout.A * 8))
      );
    if (typeof(TLayout) == typeof(ArgbLayout))
      return (
        (byte)(packed >> (ArgbLayout.R * 8)),
        (byte)(packed >> (ArgbLayout.G * 8)),
        (byte)(packed >> (ArgbLayout.B * 8)),
        (byte)(packed >> (ArgbLayout.A * 8))
      );
    return (
      (byte)(packed >> (AbgrLayout.R * 8)),
      (byte)(packed >> (AbgrLayout.G * 8)),
      (byte)(packed >> (AbgrLayout.B * 8)),
      (byte)(packed >> (AbgrLayout.A * 8))
    );
  }

  /// <summary>
  /// Returns the float values that <c>color.ToNormalized()</c> would have produced, but reads
  /// channel bytes directly from the packed view to skip the per-call tuple deconstruction. The
  /// per-channel arithmetic (<c>b * 0x01010101u</c> then implicit float-cast then multiply by
  /// <c>1/uint.MaxValue</c>) is identical to <see cref="UNorm32.FromByte"/> +
  /// <c>UNorm32.ToFloat()</c>, so the values are bit-exact with the slow path. The Bgra8888
  /// component convention <c>(C1,C2,C3,A) = (R,G,B,A)</c> is preserved across layouts.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (float c1, float c2, float c3, float a) UnpackFloats<TLayout>(uint packed)
    where TLayout : struct {
    var (r, g, b, a) = UnpackBytes<TLayout>(packed);
    return (
      (float)((uint)r * 0x01010101u) * _UnormToFloat,
      (float)((uint)g * 0x01010101u) * _UnormToFloat,
      (float)((uint)b * 0x01010101u) * _UnormToFloat,
      (float)((uint)a * 0x01010101u) * _UnormToFloat
    );
  }
}
