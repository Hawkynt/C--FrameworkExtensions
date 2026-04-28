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
using System.Runtime.CompilerServices;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Codecs;

/// <summary>
/// Scalar fallback adapter that wraps any <see cref="IDecode{TPixel,TWork}"/> implementation
/// and exposes <see cref="IBatchDecode{TPixel,TWork}"/> via a per-pixel loop.
/// </summary>
/// <typeparam name="TDecode">The wrapped per-pixel decoder type.</typeparam>
/// <typeparam name="TPixel">The storage pixel type.</typeparam>
/// <typeparam name="TWork">The working color type.</typeparam>
/// <remarks>
/// <para>
/// Used by call sites that want a uniform <see cref="IBatchDecode{TPixel,TWork}"/> contract
/// regardless of whether the underlying decoder ships a native batch impl. Probe pattern:
/// </para>
/// <code>
/// IBatchDecode&lt;TPixel,TWork&gt; batch = decoder is IBatchDecode&lt;TPixel,TWork&gt; nativeBatch
///   ? nativeBatch
///   : new BatchDecodeAdapter&lt;TDecode,TPixel,TWork&gt;(decoder);
/// </code>
/// <para>
/// Because this is a struct, the JIT can devirtualize when used with constrained generics,
/// keeping the fallback path inlinable. Bit-exact with the underlying decoder by construction.
/// </para>
/// </remarks>
public readonly struct BatchDecodeAdapter<TDecode, TPixel, TWork> : IBatchDecode<TPixel, TWork>
  where TDecode : struct, IDecode<TPixel, TWork>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace {

  private readonly TDecode _decoder;

  /// <summary>Initialises the adapter with a per-pixel decoder.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public BatchDecodeAdapter(in TDecode decoder) => this._decoder = decoder;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void DecodeBatch(ReadOnlySpan<TPixel> source, Span<TWork> destination) {
    var n = source.Length;
    Against.CountBelow(destination.Length, n);

    // Hoist the decoder into a local so the JIT can keep it in a register
    // and devirtualize the per-iteration call against the concrete struct.
    var decoder = this._decoder;
    for (var i = 0; i < n; ++i)
      destination[i] = decoder.Decode(source[i]);
  }
}
