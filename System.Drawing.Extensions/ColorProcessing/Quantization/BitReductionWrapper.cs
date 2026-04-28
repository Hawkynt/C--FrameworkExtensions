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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Storage;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Quantization;

/// <summary>
/// Wrapper that reduces color precision before quantization by masking off least significant bits.
/// </summary>
/// <remarks>
/// <para>
/// Reduces the number of unique colors in the histogram by ANDing each component with a mask,
/// effectively grouping similar colors together. This can speed up quantization and create
/// a more retro/posterized aesthetic.
/// </para>
/// <para>
/// The reduction happens before the inner quantizer sees the histogram, so the inner
/// quantizer works with fewer unique colors. Supports chaining with other wrappers:
/// <code>new BitReductionWrapper&lt;KMeansRefinementWrapper&lt;PcaQuantizerWrapper&lt;OctreeQuantizer&gt;&gt;&gt;(...)</code>
/// </para>
/// <para>
/// Bit reduction effects per channel:
/// <list type="table">
/// <listheader>
/// <term>Bits Removed</term>
/// <term>Levels</term>
/// <term>Max Unique RGB Colors</term>
/// </listheader>
/// <item><term>1</term><term>128</term><term>2,097,152</term></item>
/// <item><term>2</term><term>64</term><term>262,144</term></item>
/// <item><term>3</term><term>32</term><term>32,768</term></item>
/// <item><term>4</term><term>16</term><term>4,096</term></item>
/// </list>
/// </para>
/// </remarks>
/// <typeparam name="TInner">The type of the wrapped quantizer.</typeparam>
[Quantizer(QuantizationType.Preprocessing, DisplayName = "Bit Reduction", QualityRating = 0)]
public readonly struct BitReductionWrapper<TInner> : IQuantizer
  where TInner : struct, IQuantizer {

  private readonly TInner _inner;
  private readonly int _bitsToRemove;

  /// <summary>
  /// Creates a bit reduction wrapper around the specified quantizer.
  /// </summary>
  /// <param name="inner">The quantizer to wrap.</param>
  /// <param name="bitsToRemove">Number of least significant bits to remove per component (1-7). Default is 1.</param>
  public BitReductionWrapper(TInner inner, int bitsToRemove = 1) {
    this._inner = inner;
    this._bitsToRemove = Math.Max(1, Math.Min(7, bitsToRemove));
  }

  /// <inheritdoc />
  IQuantizer<TWork> IQuantizer.CreateKernel<TWork>()
    => new Kernel<TWork>(((IQuantizer)this._inner).CreateKernel<TWork>(), this._bitsToRemove);

  private sealed class Kernel<TWork>(IQuantizer<TWork> innerKernel, int bitsToRemove) : IQuantizer<TWork>
    where TWork : unmanaged, IColorSpace4<TWork> {

    /// <inheritdoc />
    public TWork[] GeneratePalette(IEnumerable<(TWork color, uint count)> histogram, int colorCount) {
      // Fast path: when each reduced channel uses few enough bits that 4 channels packed
      // into a single index fit a flat ulong[] of cache-friendly size, skip the dictionary.
      // bitsToRemove >= 4 => bitsPerChannel <= 4 => total <= 16 bits => array <= 65536 ulong (~512 KB max).
      // Typical reduction (bitsToRemove == 4): 4096 entries (32 KB) — comfortably L1/L2 sized.
      // For wider reductions (bitsToRemove <= 3) the array would balloon (>= 256 MB), so keep the
      // dictionary path in that case.
      if (bitsToRemove >= 4)
        return this._GeneratePaletteFlat(histogram, colorCount);

      return this._GeneratePaletteDict(histogram, colorCount);
    }

    private TWork[] _GeneratePaletteFlat(IEnumerable<(TWork color, uint count)> histogram, int colorCount) {
      var bitsPerChannel = 8 - bitsToRemove;
      var perChannelMask = (1 << bitsPerChannel) - 1; // mask after right-shift
      var bucketCount = 1 << (bitsPerChannel * 4);
      var buckets = new ulong[bucketCount];
      // Populated indices tracker — saves a full scan of the (potentially 64 K-entry) array
      // when only a handful of buckets are occupied. For dense images we'd hit the array length.
      var populated = new List<int>();

      var shift = bitsToRemove;
      var halfStep = (byte)(1 << (bitsToRemove - 1));

      // byte-domain fast path for 32bpp 4-channel storage TWork (today only Bgra8888).
      // Skips per-entry ToNormalized() + 4× ToByte() — bytes are read straight from the packed
      // pixel via the JIT-folded layout descriptor. Bit-exact because (UNorm32.FromByte(b).ToByte()
      // == b) by construction, so the bucket key is identical to the slow path.
      if (typeof(TWork) == typeof(Bgra8888))
        _BucketHistogramFast32bpp4ch<BgraLayout>(histogram, buckets, populated, shift, perChannelMask, bitsPerChannel);
      else
        _BucketHistogramSlow(histogram, buckets, populated, shift, perChannelMask, bitsPerChannel);

      // Materialize the reduced histogram in insertion order.
      var newHistogram = new (TWork color, uint count)[populated.Count];
      for (var i = 0; i < populated.Count; ++i) {
        var idx = populated[i];
        var k1 = idx & perChannelMask;
        var k2 = (idx >> bitsPerChannel) & perChannelMask;
        var k3 = (idx >> (2 * bitsPerChannel)) & perChannelMask;
        var ka = (idx >> (3 * bitsPerChannel)) & perChannelMask;

        // Reconstruct the masked byte (shift back up) then center within the bucket.
        var r1 = (byte)Math.Min(255, (k1 << shift) + halfStep);
        var r2 = (byte)Math.Min(255, (k2 << shift) + halfStep);
        var r3 = (byte)Math.Min(255, (k3 << shift) + halfStep);
        var ra = (byte)Math.Min(255, (ka << shift) + halfStep);

        var color = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromByte(r1),
          UNorm32.FromByte(r2),
          UNorm32.FromByte(r3),
          UNorm32.FromByte(ra)
        );
        newHistogram[i] = (color, (uint)Math.Min(buckets[idx], uint.MaxValue));
      }

      return innerKernel.GeneratePalette(newHistogram, colorCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void _BucketHistogramSlow(IEnumerable<(TWork color, uint count)> histogram, ulong[] buckets, List<int> populated, int shift, int perChannelMask, int bitsPerChannel) {
      foreach (var (color, count) in histogram) {
        var (c1, c2, c3, a) = color.ToNormalized();

        var k1 = (c1.ToByte() >> shift) & perChannelMask;
        var k2 = (c2.ToByte() >> shift) & perChannelMask;
        var k3 = (c3.ToByte() >> shift) & perChannelMask;
        var ka = (a.ToByte() >> shift) & perChannelMask;

        var idx = k1
                  | (k2 << bitsPerChannel)
                  | (k3 << (2 * bitsPerChannel))
                  | (ka << (3 * bitsPerChannel));

        if (buckets[idx] == 0)
          populated.Add(idx);
        buckets[idx] += count;
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void _BucketHistogramFast32bpp4ch<TLayout>(IEnumerable<(TWork color, uint count)> histogram, ulong[] buckets, List<int> populated, int shift, int perChannelMask, int bitsPerChannel)
      where TLayout : struct {
      foreach (var (color, count) in histogram) {
        var local = color;
        var packed = Unsafe.As<TWork, uint>(ref local);
        var (rByte, gByte, bByte, aByte) = StorageLayoutFast.UnpackBytes<TLayout>(packed);

        // (C1, C2, C3, A) = (R, G, B, A) on Bgra8888 / future layouts.
        var k1 = (rByte >> shift) & perChannelMask;
        var k2 = (gByte >> shift) & perChannelMask;
        var k3 = (bByte >> shift) & perChannelMask;
        var ka = (aByte >> shift) & perChannelMask;

        var idx = k1
                  | (k2 << bitsPerChannel)
                  | (k3 << (2 * bitsPerChannel))
                  | (ka << (3 * bitsPerChannel));

        if (buckets[idx] == 0)
          populated.Add(idx);
        buckets[idx] += count;
      }
    }

    private TWork[] _GeneratePaletteDict(IEnumerable<(TWork color, uint count)> histogram, int colorCount) {
      // Apply bit reduction and re-bucket
      var mask = (uint)(0xFF << bitsToRemove) & 0xFF;
      var reducedHistogram = new Dictionary<(byte, byte, byte, byte), ulong>();

      foreach (var (color, count) in histogram) {
        var (c1, c2, c3, a) = color.ToNormalized();

        // Apply mask to each component
        var r1 = (byte)(c1.ToByte() & mask);
        var r2 = (byte)(c2.ToByte() & mask);
        var r3 = (byte)(c3.ToByte() & mask);
        var ra = (byte)(a.ToByte() & mask);

        var key = (r1, r2, r3, ra);
        if (reducedHistogram.TryGetValue(key, out var existing))
          reducedHistogram[key] = existing + count;
        else
          reducedHistogram[key] = count;
      }

      // Center the reduced value in its range for better representation. Hoisted out of
      // the per-entry projection — invariant for the duration of this call.
      var halfStep = (byte)(1 << (bitsToRemove - 1));

      // Convert back to TWork histogram
      var newHistogram = reducedHistogram.Select(kvp => {
        var (r1, r2, r3, ra) = kvp.Key;
        var color = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromByte((byte)Math.Min(255, r1 + halfStep)),
          UNorm32.FromByte((byte)Math.Min(255, r2 + halfStep)),
          UNorm32.FromByte((byte)Math.Min(255, r3 + halfStep)),
          UNorm32.FromByte((byte)Math.Min(255, ra + halfStep))
        );
        return (color, count: (uint)Math.Min(kvp.Value, uint.MaxValue));
      });

      return innerKernel.GeneratePalette(newHistogram, colorCount);
    }
  }
}
