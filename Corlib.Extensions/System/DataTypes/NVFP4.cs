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
/// An NVFP4 tensor: a sequence of <see cref="E2M1"/> elements packed two-per-byte, with one fractional
/// <see cref="E4M3"/> scale per block of <see cref="BlockSize"/> elements, plus a single per-tensor FP32
/// scale. Decoded value = element x blockScale x tensorScale. Follows NVIDIA's NVFP4 format.
/// </summary>
/// <remarks>
/// The smaller block and fractional (E4M3) block scale give finer granularity than MXFP4; the per-tensor
/// FP32 scale restores dynamic range that the limited E4M3 range would otherwise lose. There is no
/// <c>Span&lt;E2M1&gt;</c> — use the indexer for scalars and <see cref="DecodeTo"/>/<see cref="DecodeBlock"/>
/// for bulk dequantization.
/// </remarks>
public sealed class NVFP4 {
  /// <summary>The number of elements that share one block scale.</summary>
  public const int BlockSize = 16;

  private const float ElementMax = 6f;   // max E2M1 magnitude
  private const float ScaleMax = 448f;   // max E4M3 magnitude

  private readonly PackedBitBuffer<LsbFirst> _codes;
  private readonly E4M3[] _scales;

  private NVFP4(PackedBitBuffer<LsbFirst> codes, E4M3[] scales, float tensorScale, int length) {
    this._codes = codes;
    this._scales = scales;
    this.TensorScale = tensorScale;
    this.Length = length;
  }

  /// <summary>Gets the number of elements.</summary>
  public int Length { get; }

  /// <summary>Gets the number of blocks.</summary>
  public int BlockCount => this._scales.Length;

  /// <summary>Gets the per-tensor FP32 scale.</summary>
  public float TensorScale { get; }

  /// <summary>
  /// Quantizes a sequence of floats into NVFP4 (per-tensor FP32 scale + per-block E4M3 scale + E2M1 elements).
  /// </summary>
  public static NVFP4 Encode(ReadOnlySpan<float> values) {
    var length = values.Length;
    var blockCount = (length + BlockSize - 1) / BlockSize;
    var codes = new PackedBitBuffer<LsbFirst>(length, 4);
    var scales = new E4M3[blockCount];

    var amaxTensor = 0f;
    for (var i = 0; i < length; ++i) {
      var m = Math.Abs(values[i]);
      if (m > amaxTensor)
        amaxTensor = m;
    }

    // Per-tensor scale maps the global maximum to the top of the (element x blockScale) range.
    var tensorScale = amaxTensor > 0 ? amaxTensor / (ElementMax * ScaleMax) : 1f;

    for (var b = 0; b < blockCount; ++b) {
      var start = b * BlockSize;
      var end = Math.Min(start + BlockSize, length);

      var amaxBlock = 0f;
      for (var i = start; i < end; ++i) {
        var m = Math.Abs(values[i]);
        if (m > amaxBlock)
          amaxBlock = m;
      }

      var blockScale = E4M3.FromSingle(amaxBlock > 0 ? amaxBlock / (ElementMax * tensorScale) : 0f);
      scales[b] = blockScale;

      var combined = tensorScale * blockScale.ToSingle();
      if (combined > 0)
        for (var i = start; i < end; ++i)
          codes.SetBits(i, E2M1.FromSingle(values[i] / combined).RawValue);
      // else: block is all-zero -> codes already zero
    }

    return new(codes, scales, tensorScale, length);
  }

  /// <summary>
  /// Wraps existing packed codes, scales and tensor scale without re-quantizing.
  /// </summary>
  public static NVFP4 FromPacked(byte[] packedCodes, E4M3[] scales, float tensorScale, int length) {
    ArgumentNullException.ThrowIfNull(packedCodes);
    ArgumentNullException.ThrowIfNull(scales);
    var expectedBlocks = (length + BlockSize - 1) / BlockSize;
    ArgumentOutOfRangeException.ThrowIfLessThan(scales.Length, expectedBlocks, nameof(scales));
    return new(PackedBitBuffer<LsbFirst>.FromPacked(packedCodes, length, 4), scales, tensorScale, length);
  }

  /// <summary>Gets the raw element (unscaled) at the given index.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public E2M1 GetElement(int index) => E2M1.FromRaw((byte)this._codes.GetBits(index));

  /// <summary>Gets the block scale for the given block.</summary>
  public E4M3 GetScale(int blockIndex) => this._scales[blockIndex];

  /// <summary>
  /// Gets or sets the decoded value at the given index.
  /// </summary>
  /// <remarks>
  /// The setter re-quantizes against the block's existing (fixed) scale and the tensor scale, and is
  /// therefore lossy and can saturate. To re-derive scales, re-<see cref="Encode"/> a decoded array instead.
  /// </remarks>
  public float this[int index] {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => E2M1.FromRaw((byte)this._codes.GetBits(index)).ToSingle() * this._scales[index / BlockSize].ToSingle() * this.TensorScale;
    set {
      var combined = this._scales[index / BlockSize].ToSingle() * this.TensorScale;
      this._codes.SetBits(index, combined > 0 ? E2M1.FromSingle(value / combined).RawValue : (ulong)0);
    }
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
    var s = this._scales[blockIndex].ToSingle() * this.TensorScale;
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
  public ReadOnlySpan<E4M3> Scales => this._scales;

  /// <summary>Enumerates the dequantized values.</summary>
  public IEnumerator<float> GetEnumerator() {
    for (var i = 0; i < this.Length; ++i)
      yield return this[i];
  }
}
