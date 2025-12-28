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

#region Float-based Accumulators

/// <summary>
/// Generic accumulator for 3-component float color spaces.
/// </summary>
/// <typeparam name="TColor">The color type implementing <see cref="IColorSpace3F{TColor}"/>.</typeparam>
public struct Accum3F<TColor> : IAccum<Accum3F<TColor>, TColor>
  where TColor : unmanaged, IColorSpace3F<TColor> {

  private float _c1, _c2, _c3, _weightSum;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AddMul(in TColor color, float weight) {
    this._c1 += color.C1 * weight;
    this._c2 += color.C2 * weight;
    this._c3 += color.C3 * weight;
    this._weightSum += weight;
  }

  /// <inheritdoc />
  public TColor Result {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var inv = 1f / this._weightSum;
      return ColorFactory.Create3F<TColor>(this._c1 * inv, this._c2 * inv, this._c3 * inv);
    }
  }
}

/// <summary>
/// Generic accumulator for 4-component float color spaces with alpha.
/// </summary>
/// <typeparam name="TColor">The color type implementing <see cref="IColorSpace4F{TColor}"/>.</typeparam>
public struct Accum4F<TColor> : IAccum<Accum4F<TColor>, TColor>
  where TColor : unmanaged, IColorSpace4F<TColor> {

  private float _c1, _c2, _c3, _a, _weightSum;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AddMul(in TColor color, float weight) {
    this._c1 += color.C1 * weight;
    this._c2 += color.C2 * weight;
    this._c3 += color.C3 * weight;
    this._a += color.A * weight;
    this._weightSum += weight;
  }

  /// <inheritdoc />
  public TColor Result {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var inv = 1f / this._weightSum;
      return ColorFactory.Create4F<TColor>(this._c1 * inv, this._c2 * inv, this._c3 * inv, this._a * inv);
    }
  }
}

/// <summary>
/// Generic accumulator for 5-component float color spaces with alpha (e.g., CMYKA).
/// </summary>
/// <typeparam name="TColor">The color type implementing <see cref="IColorSpace5F{TColor}"/>.</typeparam>
public struct Accum5F<TColor> : IAccum<Accum5F<TColor>, TColor>
  where TColor : unmanaged, IColorSpace5F<TColor> {

  private float _c1, _c2, _c3, _c4, _a, _weightSum;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AddMul(in TColor color, float weight) {
    this._c1 += color.C1 * weight;
    this._c2 += color.C2 * weight;
    this._c3 += color.C3 * weight;
    this._c4 += color.C4 * weight;
    this._a += color.A * weight;
    this._weightSum += weight;
  }

  /// <inheritdoc />
  public TColor Result {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var inv = 1f / this._weightSum;
      return ColorFactory.Create5F<TColor>(this._c1 * inv, this._c2 * inv, this._c3 * inv, this._c4 * inv, this._a * inv);
    }
  }
}

#endregion

#region Byte-based Accumulators

/// <summary>
/// Generic accumulator for 3-component byte color spaces.
/// </summary>
/// <typeparam name="TColor">The color type implementing <see cref="IColorSpace3B{TColor}"/>.</typeparam>
/// <remarks>
/// Accumulates in float precision and clamps to byte range in <see cref="Result"/>.
/// </remarks>
public struct Accum3B<TColor> : IAccum<Accum3B<TColor>, TColor>
  where TColor : unmanaged, IColorSpace3B<TColor> {

  private float _c1, _c2, _c3, _weightSum;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AddMul(in TColor color, float weight) {
    this._c1 += color.C1 * weight;
    this._c2 += color.C2 * weight;
    this._c3 += color.C3 * weight;
    this._weightSum += weight;
  }

  /// <inheritdoc />
  public TColor Result {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var inv = 1f / this._weightSum;
      return ColorFactory.Create3B<TColor>(
        ClampToByte(this._c1 * inv + 0.5f),
        ClampToByte(this._c2 * inv + 0.5f),
        ClampToByte(this._c3 * inv + 0.5f)
      );
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte ClampToByte(float value) => value < 0 ? (byte)0 : value > 255 ? (byte)255 : (byte)value;
}

/// <summary>
/// Generic accumulator for 4-component byte color spaces with alpha.
/// </summary>
/// <typeparam name="TColor">The color type implementing <see cref="IColorSpace4B{TColor}"/>.</typeparam>
/// <remarks>
/// Accumulates in float precision and clamps to byte range in <see cref="Result"/>.
/// </remarks>
public struct Accum4B<TColor> : IAccum<Accum4B<TColor>, TColor>
  where TColor : unmanaged, IColorSpace4B<TColor> {

  private float _c1, _c2, _c3, _a, _weightSum;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AddMul(in TColor color, float weight) {
    this._c1 += color.C1 * weight;
    this._c2 += color.C2 * weight;
    this._c3 += color.C3 * weight;
    this._a += color.A * weight;
    this._weightSum += weight;
  }

  /// <inheritdoc />
  public TColor Result {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var inv = 1f / this._weightSum;
      return ColorFactory.Create4B<TColor>(
        ClampToByte(this._c1 * inv + 0.5f),
        ClampToByte(this._c2 * inv + 0.5f),
        ClampToByte(this._c3 * inv + 0.5f),
        ClampToByte(this._a * inv + 0.5f)
      );
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte ClampToByte(float value) => value < 0 ? (byte)0 : value > 255 ? (byte)255 : (byte)value;
}

/// <summary>
/// Generic accumulator for 5-component byte color spaces with alpha.
/// </summary>
/// <typeparam name="TColor">The color type implementing <see cref="IColorSpace5B{TColor}"/>.</typeparam>
/// <remarks>
/// Accumulates in float precision and clamps to byte range in <see cref="Result"/>.
/// </remarks>
public struct Accum5B<TColor> : IAccum<Accum5B<TColor>, TColor>
  where TColor : unmanaged, IColorSpace5B<TColor> {

  private float _c1, _c2, _c3, _c4, _a, _weightSum;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AddMul(in TColor color, float weight) {
    this._c1 += color.C1 * weight;
    this._c2 += color.C2 * weight;
    this._c3 += color.C3 * weight;
    this._c4 += color.C4 * weight;
    this._a += color.A * weight;
    this._weightSum += weight;
  }

  /// <inheritdoc />
  public TColor Result {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      var inv = 1f / this._weightSum;
      return ColorFactory.Create5B<TColor>(
        ClampToByte(this._c1 * inv + 0.5f),
        ClampToByte(this._c2 * inv + 0.5f),
        ClampToByte(this._c3 * inv + 0.5f),
        ClampToByte(this._c4 * inv + 0.5f),
        ClampToByte(this._a * inv + 0.5f)
      );
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte ClampToByte(float value) => value < 0 ? (byte)0 : value > 255 ? (byte)255 : (byte)value;
}

#endregion
