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

namespace Hawkynt.ColorProcessing.ColorMath;

/// <summary>
/// Pure integer accumulator for 3-component byte color spaces.
/// </summary>
/// <typeparam name="TColor">The color type implementing <see cref="IColorSpace3B{TColor}"/>.</typeparam>
/// <remarks>
/// <para>Uses 32-bit integer accumulation - safe for up to ~16 million weighted samples.</para>
/// <para>No float arithmetic is used. Integer division with rounding in <see cref="Result"/>.</para>
/// </remarks>
public struct Accum3I<TColor> : IAccumInt<Accum3I<TColor>, TColor>, IAccum<Accum3I<TColor>, TColor>
  where TColor : unmanaged, IColorSpace3B<TColor> {

  private int _c1, _c2, _c3, _weightSum;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AddMul(in TColor color, int weight) {
    this._c1 += color.C1 * weight;
    this._c2 += color.C2 * weight;
    this._c3 += color.C3 * weight;
    this._weightSum += weight;
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Add(in TColor color) {
    this._c1 += color.C1;
    this._c2 += color.C2;
    this._c3 += color.C3;
    ++this._weightSum;
  }

  /// <inheritdoc />
  /// <remarks>Converts float weight to integer (256 scale) and delegates to integer AddMul.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AddMul(in TColor color, float weight) {
    var intWeight = (int)(weight * 256f + 0.5f);
    this.AddMul(color, intWeight);
  }

  /// <inheritdoc />
  public TColor Result {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var half = this._weightSum >> 1;
      return ColorFactory.Create3B<TColor>(
        (byte)((this._c1 + half) / this._weightSum),
        (byte)((this._c2 + half) / this._weightSum),
        (byte)((this._c3 + half) / this._weightSum)
      );
    }
  }
}

/// <summary>
/// Pure integer accumulator for 4-component byte color spaces with alpha.
/// </summary>
/// <typeparam name="TColor">The color type implementing <see cref="IColorSpace4B{TColor}"/>.</typeparam>
/// <remarks>
/// <para>Uses 32-bit integer accumulation - safe for up to ~16 million weighted samples.</para>
/// <para>No float arithmetic is used. Integer division with rounding in <see cref="Result"/>.</para>
/// </remarks>
public struct Accum4I<TColor> : IAccumInt<Accum4I<TColor>, TColor>, IAccum<Accum4I<TColor>, TColor>
  where TColor : unmanaged, IColorSpace4B<TColor> {

  private int _c1, _c2, _c3, _a, _weightSum;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AddMul(in TColor color, int weight) {
    this._c1 += color.C1 * weight;
    this._c2 += color.C2 * weight;
    this._c3 += color.C3 * weight;
    this._a += color.A * weight;
    this._weightSum += weight;
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Add(in TColor color) {
    this._c1 += color.C1;
    this._c2 += color.C2;
    this._c3 += color.C3;
    this._a += color.A;
    ++this._weightSum;
  }

  /// <inheritdoc />
  /// <remarks>Converts float weight to integer (256 scale) and delegates to integer AddMul.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AddMul(in TColor color, float weight) {
    var intWeight = (int)(weight * 256f + 0.5f);
    this.AddMul(color, intWeight);
  }

  /// <inheritdoc />
  public TColor Result {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var half = this._weightSum >> 1;
      return ColorFactory.Create4B<TColor>(
        (byte)((this._c1 + half) / this._weightSum),
        (byte)((this._c2 + half) / this._weightSum),
        (byte)((this._c3 + half) / this._weightSum),
        (byte)((this._a + half) / this._weightSum)
      );
    }
  }
}

/// <summary>
/// Pure integer accumulator for 5-component byte color spaces with alpha.
/// </summary>
/// <typeparam name="TColor">The color type implementing <see cref="IColorSpace5B{TColor}"/>.</typeparam>
/// <remarks>
/// <para>Uses 32-bit integer accumulation - safe for up to ~16 million weighted samples.</para>
/// <para>No float arithmetic is used. Integer division with rounding in <see cref="Result"/>.</para>
/// </remarks>
public struct Accum5I<TColor> : IAccumInt<Accum5I<TColor>, TColor>, IAccum<Accum5I<TColor>, TColor>
  where TColor : unmanaged, IColorSpace5B<TColor> {

  private int _c1, _c2, _c3, _c4, _a, _weightSum;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AddMul(in TColor color, int weight) {
    this._c1 += color.C1 * weight;
    this._c2 += color.C2 * weight;
    this._c3 += color.C3 * weight;
    this._c4 += color.C4 * weight;
    this._a += color.A * weight;
    this._weightSum += weight;
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Add(in TColor color) {
    this._c1 += color.C1;
    this._c2 += color.C2;
    this._c3 += color.C3;
    this._c4 += color.C4;
    this._a += color.A;
    ++this._weightSum;
  }

  /// <inheritdoc />
  /// <remarks>Converts float weight to integer (256 scale) and delegates to integer AddMul.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AddMul(in TColor color, float weight) {
    var intWeight = (int)(weight * 256f + 0.5f);
    this.AddMul(color, intWeight);
  }

  /// <inheritdoc />
  public TColor Result {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var half = this._weightSum >> 1;
      return ColorFactory.Create5B<TColor>(
        (byte)((this._c1 + half) / this._weightSum),
        (byte)((this._c2 + half) / this._weightSum),
        (byte)((this._c3 + half) / this._weightSum),
        (byte)((this._c4 + half) / this._weightSum),
        (byte)((this._a + half) / this._weightSum)
      );
    }
  }
}
