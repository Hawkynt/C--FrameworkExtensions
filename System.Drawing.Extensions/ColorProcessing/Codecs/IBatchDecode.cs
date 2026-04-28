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

using System;

namespace Hawkynt.ColorProcessing.Codecs;

/// <summary>
/// Optional batch-decode capability for an <see cref="IDecode{TPixel,TWork}"/>.
/// Implementing types provide a span-to-span decode for amortised gamma-LUT / FP-conversion
/// costs, mirroring the <c>IBatchDistance</c> opt-in pattern used by <see cref="Metrics"/>.
/// </summary>
/// <typeparam name="TPixel">The storage pixel type (e.g., Bgra8888, Bgr888).</typeparam>
/// <typeparam name="TWork">The working color type (e.g., LinearRgbaF, OklabaF).</typeparam>
/// <remarks>
/// <para>
/// Detected at kernel-construction time via
/// <c>decoder is IBatchDecode&lt;TPixel,TWork&gt;</c>. Decoders that do not implement this
/// interface are wrapped by <see cref="BatchDecodeAdapter{TDecode,TPixel,TWork}"/>, which
/// provides a scalar fallback loop. Implementing this interface natively (typically with
/// loop-unrolling and/or SIMD) avoids the per-pixel virtual / generic-struct dispatch overhead
/// and enables coalesced gamma-LUT lookups.
/// </para>
/// <para>
/// <b>Bit-exactness contract.</b> An implementation MUST produce element-wise identical output
/// to a per-pixel <see cref="IDecode{TPixel,TWork}.Decode"/> loop:
/// <c>DecodeBatch([px0, px1, ..., pxN])[i] == Decode(pxi)</c> for all <c>i</c>. Goldens are
/// the binding test.
/// </para>
/// </remarks>
public interface IBatchDecode<TPixel, TWork>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace {

  /// <summary>
  /// Decodes a contiguous run of source pixels into a contiguous run of working colours.
  /// </summary>
  /// <param name="source">The source pixels to decode.</param>
  /// <param name="destination">The destination span; MUST be at least as long as <paramref name="source"/>.</param>
  /// <remarks>
  /// The implementation MUST produce element-wise identical output to a per-pixel
  /// <see cref="IDecode{TPixel,TWork}.Decode"/> loop. Implementations should mark
  /// the inner method <see cref="System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining"/>
  /// so the call cost amortises across the span.
  /// </remarks>
  void DecodeBatch(ReadOnlySpan<TPixel> source, Span<TWork> destination);
}
