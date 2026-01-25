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
using Hawkynt.ColorProcessing.Metrics;

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

      // Convert back to TWork histogram
      var newHistogram = reducedHistogram.Select(kvp => {
        var (r1, r2, r3, ra) = kvp.Key;
        // Center the reduced value in its range for better representation
        var halfStep = (byte)(1 << (bitsToRemove - 1));
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
