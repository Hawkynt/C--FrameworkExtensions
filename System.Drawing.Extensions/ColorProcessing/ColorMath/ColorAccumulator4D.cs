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
/// Double-based color accumulator for weighted averaging with floating-point weights.
/// </summary>
/// <remarks>
/// Uses double precision for both sums and weights.
/// Ideal for quantizers that need fractional or very large weights.
/// </remarks>
internal struct ColorAccumulator4D {
  private double _c1Sum, _c2Sum, _c3Sum, _aSum;

  /// <summary>
  /// Adds a single color sample (weight = 1).
  /// </summary>
  public void Add(UNorm32 c1, UNorm32 c2, UNorm32 c3, UNorm32 a) {
    this._c1Sum += c1.ToFloat();
    this._c2Sum += c2.ToFloat();
    this._c3Sum += c3.ToFloat();
    this._aSum += a.ToFloat();
    this.Weight += 1.0;
  }

  /// <summary>
  /// Adds a color sample with the specified weight.
  /// </summary>
  public void Add(UNorm32 c1, UNorm32 c2, UNorm32 c3, UNorm32 a, double weight) {
    this._c1Sum += c1.ToFloat() * weight;
    this._c2Sum += c2.ToFloat() * weight;
    this._c3Sum += c3.ToFloat() * weight;
    this._aSum += a.ToFloat() * weight;
    this.Weight += weight;
  }

  /// <summary>
  /// Adds a color sample with uint count as weight.
  /// </summary>
  public void Add(UNorm32 c1, UNorm32 c2, UNorm32 c3, UNorm32 a, uint count) {
    this._c1Sum += c1.ToFloat() * count;
    this._c2Sum += c2.ToFloat() * count;
    this._c3Sum += c3.ToFloat() * count;
    this._aSum += a.ToFloat() * count;
    this.Weight += count;
  }

  /// <summary>
  /// Computes the weighted average color.
  /// </summary>
  public readonly TWork ToAverage<TWork>() where TWork : unmanaged, IColorSpace4<TWork> {
    if (this.Weight <= 0)
      return default;

    return ColorFactory.FromNormalized_4<TWork>(
      UNorm32.FromFloatClamped((float)(this._c1Sum / this.Weight)),
      UNorm32.FromFloatClamped((float)(this._c2Sum / this.Weight)),
      UNorm32.FromFloatClamped((float)(this._c3Sum / this.Weight)),
      UNorm32.FromFloatClamped((float)(this._aSum / this.Weight))
    );
  }

  /// <summary>
  /// Gets the total accumulated weight.
  /// </summary>
  public double Weight { get; private set; }

  /// <summary>
  /// Resets the accumulator to zero.
  /// </summary>
  public void Clear() {
    this._c1Sum = 0;
    this._c2Sum = 0;
    this._c3Sum = 0;
    this._aSum = 0;
    this.Weight = 0;
  }
}
