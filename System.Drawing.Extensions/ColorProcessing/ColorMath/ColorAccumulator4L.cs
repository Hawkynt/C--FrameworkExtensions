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

using Hawkynt.ColorProcessing;

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

using Hawkynt.ColorProcessing.Metrics;

namespace System.Drawing.Extensions.ColorProcessing.ColorMath;

/// <summary>
/// Long-based color accumulator for integer-only weighted averaging.
/// </summary>
/// <remarks>
/// Uses raw UNorm32 values (uint range) accumulated in longs to avoid floating-point operations.
/// Ideal for quantizers that use uint counts as weights.
/// </remarks>
internal struct ColorAccumulator4L {
  private long _c1Sum, _c2Sum, _c3Sum, _aSum;

  /// <summary>
  /// Adds a single color sample (weight = 1).
  /// </summary>
  public void Add(UNorm32 c1, UNorm32 c2, UNorm32 c3, UNorm32 a) {
    this._c1Sum += c1.RawValue;
    this._c2Sum += c2.RawValue;
    this._c3Sum += c3.RawValue;
    this._aSum += a.RawValue;
    ++this.Count;
  }

  /// <summary>
  /// Adds a color sample with the specified count as weight.
  /// </summary>
  public void Add(UNorm32 c1, UNorm32 c2, UNorm32 c3, UNorm32 a, uint count) {
    this._c1Sum += (long)c1.RawValue * count;
    this._c2Sum += (long)c2.RawValue * count;
    this._c3Sum += (long)c3.RawValue * count;
    this._aSum += (long)a.RawValue * count;
    this.Count += count;
  }

  /// <summary>
  /// Adds a color sample with the specified count as weight.
  /// </summary>
  public void Add(UNorm32 c1, UNorm32 c2, UNorm32 c3, UNorm32 a, ulong count) {
    this._c1Sum += (long)(c1.RawValue * count);
    this._c2Sum += (long)(c2.RawValue * count);
    this._c3Sum += (long)(c3.RawValue * count);
    this._aSum += (long)(a.RawValue * count);
    this.Count += count;
  }

  /// <summary>
  /// Computes the weighted average color.
  /// </summary>
  public readonly TWork ToAverage<TWork>() where TWork : unmanaged, IColorSpace4<TWork> {
    if (this.Count == 0)
      return default;

    var divisor = (long)this.Count;
    return ColorFactory.FromNormalized_4<TWork>(
      UNorm32.FromRaw((uint)(this._c1Sum / divisor)),
      UNorm32.FromRaw((uint)(this._c2Sum / divisor)),
      UNorm32.FromRaw((uint)(this._c3Sum / divisor)),
      UNorm32.FromRaw((uint)(this._aSum / divisor))
    );
  }

  /// <summary>
  /// Gets the total accumulated count/weight.
  /// </summary>
  public ulong Count { get; private set; }

  /// <summary>
  /// Resets the accumulator to zero.
  /// </summary>
  public void Clear() {
    this._c1Sum = 0;
    this._c2Sum = 0;
    this._c3Sum = 0;
    this._aSum = 0;
    this.Count = 0;
  }
}
