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

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

/// <summary>
/// An MXFP4 tensor: a sequence of <see cref="E2M1"/> elements packed two-per-byte, with one shared
/// <see cref="E8M0"/> (power-of-two) scale per block of <see cref="BlockSize"/> elements.
/// Decoded value = element x blockScale. Follows the OCP Microscaling specification.
/// </summary>
/// <remarks>
/// The packed nibble storage is a <see cref="PackedBitBuffer{TBitOrder}"/> of width 4; this type adds the
/// per-block scaling. There is no <c>Span&lt;E2M1&gt;</c> (sub-byte elements are not addressable) — use the
/// indexer for scalars and <see cref="DecodeTo"/>/<see cref="DecodeBlock"/> for bulk dequantization.
/// </remarks>
public sealed class MXFP4 {
  /// <summary>The number of elements that share one scale.</summary>
  public const int BlockSize = 32;

  // The largest base-two exponent representable by an E2M1 magnitude (6.0 = 1.5 x 2^2).
  private const int ElementMaxExponent = 2;

  private readonly PackedBitBuffer<LsbFirst> _codes;
  private readonly E8M0[] _scales;

  private MXFP4(PackedBitBuffer<LsbFirst> codes, E8M0[] scales, int length) {
    this._codes = codes;
    this._scales = scales;
    this.Length = length;
  }

  /// <summary>Gets the number of elements.</summary>
  public int Length { get; }

  /// <summary>Gets the number of blocks.</summary>
  public int BlockCount => this._scales.Length;

  /// <summary>
  /// Quantizes a sequence of floats into MXFP4 (per-block power-of-two scale + E2M1 elements).
  /// </summary>
  public static MXFP4 Encode(ReadOnlySpan<float> values) {
    var length = values.Length;
    var blockCount = (length + BlockSize - 1) / BlockSize;
    var codes = new PackedBitBuffer<LsbFirst>(length, 4);
    var scales = new E8M0[blockCount];

    for (var b = 0; b < blockCount; ++b) {
      var start = b * BlockSize;
      var end = Math.Min(start + BlockSize, length);

      var amax = 0f;
      for (var i = start; i < end; ++i) {
        var m = Math.Abs(values[i]);
        if (m > amax)
          amax = m;
      }

      // Shared scale chosen so the block maximum lands in the top of the E2M1 range.
      var scale = amax > 0 ? E8M0.FromExponent((int)Math.Floor(Math.Log(amax, 2.0)) - ElementMaxExponent) : E8M0.One;
      scales[b] = scale;

      var s = scale.ToSingle();
      for (var i = start; i < end; ++i)
        codes.SetBits(i, E2M1.FromSingle(values[i] / s).RawValue);
    }

    return new(codes, scales, length);
  }

  /// <summary>
  /// Wraps existing packed codes and scales without re-quantizing.
  /// </summary>
  public static MXFP4 FromPacked(byte[] packedCodes, E8M0[] scales, int length) {
    ArgumentNullException.ThrowIfNull(packedCodes);
    ArgumentNullException.ThrowIfNull(scales);
    var expectedBlocks = (length + BlockSize - 1) / BlockSize;
    ArgumentOutOfRangeException.ThrowIfLessThan(scales.Length, expectedBlocks, nameof(scales));
    return new(PackedBitBuffer<LsbFirst>.FromPacked(packedCodes, length, 4), scales, length);
  }

  /// <summary>Gets the raw element (unscaled) at the given index.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public E2M1 GetElement(int index) => E2M1.FromRaw((byte)this._codes.GetBits(index));

  /// <summary>Gets the scale for the given block.</summary>
  public E8M0 GetScale(int blockIndex) => this._scales[blockIndex];

  /// <summary>
  /// Gets or sets the decoded value at the given index.
  /// </summary>
  /// <remarks>
  /// The setter re-quantizes against the block's existing (fixed) scale and is therefore lossy and can
  /// saturate. To re-derive scales, re-<see cref="Encode"/> a decoded array instead.
  /// </remarks>
  public float this[int index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => E2M1.FromRaw((byte)this._codes.GetBits(index)).ToSingle() * this._scales[index / BlockSize].ToSingle();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set => this._codes.SetBits(index, E2M1.FromSingle(value / this._scales[index / BlockSize].ToSingle()).RawValue);
  }

  /// <summary>
  /// Dequantizes all elements into <paramref name="destination"/>.
  /// </summary>
  public void DecodeTo(Span<float> destination) {
    ArgumentOutOfRangeException.ThrowIfLessThan(destination.Length, this.Length, nameof(destination));
    for (var i = 0; i < this.Length; ++i)
      destination[i] = this[i];
  }

  /// <summary>
  /// Dequantizes a single block into <paramref name="destination"/> (returns the element count written).
  /// </summary>
  public int DecodeBlock(int blockIndex, Span<float> destination) {
    var start = blockIndex * BlockSize;
    var end = Math.Min(start + BlockSize, this.Length);
    var count = end - start;
    ArgumentOutOfRangeException.ThrowIfLessThan(destination.Length, count, nameof(destination));
    var s = this._scales[blockIndex].ToSingle();
    for (var i = 0; i < count; ++i)
      destination[i] = E2M1.FromRaw((byte)this._codes.GetBits(start + i)).ToSingle() * s;
    return count;
  }

  /// <summary>Returns all dequantized values as a new array.</summary>
  public float[] ToArray() {
    var result = new float[this.Length];
    for (var i = 0; i < result.Length; ++i)
      result[i] = this[i];
    return result;
  }

  /// <summary>Gets the packed nibble storage (2 elements per byte) for interop.</summary>
  public ReadOnlySpan<byte> PackedData => this._codes.PackedData;

  /// <summary>Gets the per-block scales.</summary>
  public ReadOnlySpan<E8M0> Scales => this._scales;

  /// <summary>Enumerates the dequantized values.</summary>
  public IEnumerator<float> GetEnumerator() {
    for (var i = 0; i < this.Length; ++i)
      yield return this[i];
  }
}
