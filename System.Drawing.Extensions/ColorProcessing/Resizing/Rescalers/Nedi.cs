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
using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Rescalers;

/// <summary>
/// NEDI (New Edge-Directed Interpolation) scaler by Xin Li and Michael T. Orchard (2x, 3x, 4x).
/// </summary>
/// <remarks>
/// <para>Uses local autocorrelation to determine optimal interpolation weights via Cramer's rule.</para>
/// <para>
/// Analyzes local edge directions to produce adaptive edge-directed interpolation.
/// Combines diagonal NEDI with cardinal interpolation for full output blocks.
/// </para>
/// <para>Algorithm by Xin Li and Michael T. Orchard, IEEE Trans. Image Processing, 2001.</para>
/// </remarks>
[ScalerInfo("NEDI", Author = "Xin Li/Michael T. Orchard", Year = 2001,
  Description = "New Edge-Directed Interpolation", Category = ScalerCategory.PixelArt)]
public readonly struct Nedi : IPixelScaler {

  private readonly int _scale;

  // Algorithm parameters
  private const float NEDI_WEIGHT = 4.0f;
  private const float NEDI_N = 24.0f;

  /// <summary>
  /// Creates a new NEDI instance.
  /// </summary>
  /// <param name="scale">Scale factor (2, 3, or 4).</param>
  public Nedi(int scale = 2) {
    ArgumentOutOfRangeException.ThrowIfLessThan(scale, 2);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(scale, 4);
    this._scale = scale;
  }

  /// <inheritdoc />
  public ScaleFactor Scale => this._scale == 0 ? new(2, 2) : new(this._scale, this._scale);

  /// <inheritdoc />
  public TResult InvokeKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode, TResult>(
    IKernelCallback<TWork, TKey, TPixel, TEncode, TResult> callback,
    TEquality equality = default,
    TLerp lerp = default)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDistance : struct, IColorMetric<TKey>, INormalizedMetric
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
    where TEncode : struct, IEncode<TWork, TPixel>
    => this._scale switch {
      0 or 2 => callback.Invoke(new Nedi2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      3 => callback.Invoke(new Nedi3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      4 => callback.Invoke(new Nedi4xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      _ => throw new InvalidOperationException($"Invalid scale factor: {this._scale}")
    };

  /// <summary>
  /// Gets the list of scale factors supported.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2), new(3, 3), new(4, 4)];

  /// <summary>
  /// Determines whether the specified scale factor is supported.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 } or { X: 3, Y: 3 } or { X: 4, Y: 4 };

  /// <summary>
  /// Enumerates all possible target dimensions.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
    yield return (sourceWidth * 3, sourceHeight * 3);
    yield return (sourceWidth * 4, sourceHeight * 4);
  }

  /// <summary>
  /// Gets a 2x scale instance.
  /// </summary>
  public static Nedi Scale2x => new(2);

  /// <summary>
  /// Gets a 3x scale instance.
  /// </summary>
  public static Nedi Scale3x => new(3);

  /// <summary>
  /// Gets a 4x scale instance.
  /// </summary>
  public static Nedi Scale4x => new(4);

  /// <summary>
  /// Gets the default configuration (2x).
  /// </summary>
  public static Nedi Default => Scale2x;

  /// <summary>
  /// Computes luma using ColorConverter.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float Luma<TWork>(in TWork color) where TWork : unmanaged, IColorSpace
    => ColorConverter.GetLuminance(color);

  /// <summary>
  /// Solves 2x2 system using Cramer's rule.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static (float a0, float a1) SolveCramer(float r00, float r01, float r11, float b0, float b1) {
    var det = r00 * r11 - r01 * r01;
    if (MathF.Abs(det) < 1e-10f)
      return (0.5f, 0.5f);

    var a0 = (b0 * r11 - b1 * r01) / det;
    var a1 = (r00 * b1 - r01 * b0) / det;

    return (a0, a1);
  }

  /// <summary>
  /// Gets pixel at a position within the 5x5 window (clamped).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static TWork GetPixelAt<TWork, TKey>(
    in NeighborWindow<TWork, TKey> window,
    int row, int col)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace {

    // Clamp to 5x5 window bounds (-2 to +2)
    row = row < -2 ? -2 : row > 2 ? 2 : row;
    col = col < -2 ? -2 : col > 2 ? 2 : col;

    return (row, col) switch {
      (-2, -2) => window.M2M2.Work, (-2, -1) => window.M2M1.Work, (-2, 0) => window.M2P0.Work, (-2, 1) => window.M2P1.Work, (-2, 2) => window.M2P2.Work,
      (-1, -2) => window.M1M2.Work, (-1, -1) => window.M1M1.Work, (-1, 0) => window.M1P0.Work, (-1, 1) => window.M1P1.Work, (-1, 2) => window.M1P2.Work,
      ( 0, -2) => window.P0M2.Work, ( 0, -1) => window.P0M1.Work, ( 0, 0) => window.P0P0.Work, ( 0, 1) => window.P0P1.Work, ( 0, 2) => window.P0P2.Work,
      ( 1, -2) => window.P1M2.Work, ( 1, -1) => window.P1M1.Work, ( 1, 0) => window.P1P0.Work, ( 1, 1) => window.P1P1.Work, ( 1, 2) => window.P1P2.Work,
      ( 2, -2) => window.P2M2.Work, ( 2, -1) => window.P2M1.Work, ( 2, 0) => window.P2P0.Work, ( 2, 1) => window.P2P1.Work, ( 2, 2) => window.P2P2.Work,
      _ => window.P0P0.Work
    };
  }

  /// <summary>
  /// Gets luma at a position within the 5x5 window.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float GetLumaAt<TWork, TKey>(
    in NeighborWindow<TWork, TKey> window,
    int row, int col)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    => Luma(GetPixelAt(window, row, col));

  /// <summary>
  /// Calculates diagonal interpolation using autocorrelation.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static TWork DiagonalInterpolate<TWork, TKey, TLerp>(
    in NeighborWindow<TWork, TKey> window,
    TLerp lerp)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TLerp : struct, ILerp<TWork> {

    // Direction vectors for diagonal sampling (row, col)
    // dir[0] = (-1,-1) NW, dir[1] = (1,1) SE, dir[2] = (-1,1) NE, dir[3] = (1,-1) SW

    // Sample configurations at different positions within 5x5 window
    // Configuration 0: center diagonals (-1,-1), (1,1), (-1,1), (1,-1) - weight 4
    // Configuration 1: extended diagonals clamped to window bounds - weight 1
    // Configuration 2: alternate extended positions - weight 1

    var r00 = 0f; var r01 = 0f; var r11 = 0f;
    var r0 = 0f; var r1 = 0f;

    // Configuration 0: core diagonals (weight 4)
    {
      var y0 = GetLumaAt(window, -1, -1);
      var y1 = GetLumaAt(window,  1,  1);
      var y2 = GetLumaAt(window, -1,  1);
      var y3 = GetLumaAt(window,  1, -1);

      // Diagonal pair sums at each sample point
      var c0a = GetLumaAt(window, -2, -2) + GetLumaAt(window, 0, 0);
      var c0b = GetLumaAt(window, -2,  0) + GetLumaAt(window, 0, -2);
      var c1a = GetLumaAt(window, 0, 0) + GetLumaAt(window, 2, 2);
      var c1b = GetLumaAt(window, 0, 2) + GetLumaAt(window, 2, 0);
      var c2a = GetLumaAt(window, -2, 0) + GetLumaAt(window, 0, 2);
      var c2b = GetLumaAt(window, -2, 2) + GetLumaAt(window, 0, 0);
      var c3a = GetLumaAt(window, 0, -2) + GetLumaAt(window, 2, 0);
      var c3b = GetLumaAt(window, 0, 0) + GetLumaAt(window, 2, -2);

      r00 += NEDI_WEIGHT * (c0a*c0a + c1a*c1a + c2a*c2a + c3a*c3a);
      r01 += NEDI_WEIGHT * (c0a*c0b + c1a*c1b + c2a*c2b + c3a*c3b);
      r11 += NEDI_WEIGHT * (c0b*c0b + c1b*c1b + c2b*c2b + c3b*c3b);
      r0 += NEDI_WEIGHT * (y0*c0a + y1*c1a + y2*c2a + y3*c3a);
      r1 += NEDI_WEIGHT * (y0*c0b + y1*c1b + y2*c2b + y3*c3b);
    }

    // Configuration 1: extended positions (weight 1)
    {
      var y0 = GetLumaAt(window, -2, -1);
      var y1 = GetLumaAt(window,  2,  1);
      var y2 = GetLumaAt(window, -1,  2);
      var y3 = GetLumaAt(window,  1, -2);

      var c0a = GetLumaAt(window, -2, -2) + GetLumaAt(window, -2, 0);
      var c0b = GetLumaAt(window, -2, 0) + GetLumaAt(window, -2, -2);
      var c1a = GetLumaAt(window, 2, 0) + GetLumaAt(window, 2, 2);
      var c1b = GetLumaAt(window, 2, 2) + GetLumaAt(window, 2, 0);
      var c2a = GetLumaAt(window, -2, 2) + GetLumaAt(window, 0, 2);
      var c2b = GetLumaAt(window, 0, 2) + GetLumaAt(window, -2, 2);
      var c3a = GetLumaAt(window, 0, -2) + GetLumaAt(window, 2, -2);
      var c3b = GetLumaAt(window, 2, -2) + GetLumaAt(window, 0, -2);

      r00 += c0a*c0a + c1a*c1a + c2a*c2a + c3a*c3a;
      r01 += c0a*c0b + c1a*c1b + c2a*c2b + c3a*c3b;
      r11 += c0b*c0b + c1b*c1b + c2b*c2b + c3b*c3b;
      r0 += y0*c0a + y1*c1a + y2*c2a + y3*c3a;
      r1 += y0*c0b + y1*c1b + y2*c2b + y3*c3b;
    }

    // Configuration 2: alternate extended (weight 1)
    {
      var y0 = GetLumaAt(window, -2,  1);
      var y1 = GetLumaAt(window,  2, -1);
      var y2 = GetLumaAt(window,  1,  2);
      var y3 = GetLumaAt(window, -1, -2);

      var c0a = GetLumaAt(window, -2, 0) + GetLumaAt(window, -2, 2);
      var c0b = GetLumaAt(window, -2, 2) + GetLumaAt(window, -2, 0);
      var c1a = GetLumaAt(window, 2, -2) + GetLumaAt(window, 2, 0);
      var c1b = GetLumaAt(window, 2, 0) + GetLumaAt(window, 2, -2);
      var c2a = GetLumaAt(window, 0, 2) + GetLumaAt(window, 2, 2);
      var c2b = GetLumaAt(window, 2, 2) + GetLumaAt(window, 0, 2);
      var c3a = GetLumaAt(window, -2, -2) + GetLumaAt(window, 0, -2);
      var c3b = GetLumaAt(window, 0, -2) + GetLumaAt(window, -2, -2);

      r00 += c0a*c0a + c1a*c1a + c2a*c2a + c3a*c3a;
      r01 += c0a*c0b + c1a*c1b + c2a*c2b + c3a*c3b;
      r11 += c0b*c0b + c1b*c1b + c2b*c2b + c3b*c3b;
      r0 += y0*c0a + y1*c1a + y2*c2a + y3*c3a;
      r1 += y0*c0b + y1*c1b + y2*c2b + y3*c3b;
    }

    r00 /= NEDI_N; r01 /= NEDI_N; r11 /= NEDI_N;
    r0 /= NEDI_N; r1 /= NEDI_N;

    var (a0, a1) = SolveCramer(r00, r01, r11, r0, r1);

    var diff = MathF.Max(-1f, MathF.Min(1f, a0 - a1));
    a0 = 0.25f + 0.5f * diff;
    a1 = 0.25f - 0.5f * diff;

    // Get diagonal pixels using named properties
    var v0 = window.M1M1.Work; // (-1,-1) NW
    var v1 = window.P1P1.Work; // (1,1) SE
    var v2 = window.M1P1.Work; // (-1,1) NE
    var v3 = window.P1M1.Work; // (1,-1) SW

    var sum01 = lerp.Lerp(v0, v1);
    var sum23 = lerp.Lerp(v2, v3);

    var total = a0 + a1 + 0.0001f;
    var w2 = (int)(a1 / total * 256f);
    return lerp.Lerp(sum01, sum23, 256 - w2, w2);
  }

  /// <summary>
  /// Calculates horizontal cardinal interpolation.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static TWork CardinalInterpolateHorizontal<TWork, TKey, TLerp>(
    in NeighborWindow<TWork, TKey> window,
    TLerp lerp)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TLerp : struct, ILerp<TWork> {

    var left = window.P0P0.Work;
    var right = window.P0P1.Work;
    var leftLeft = window.P0M1.Work;
    var rightRight = window.P0P2.Work;

    var (w0, w1) = CalculateCardinalWeights(leftLeft, left, right, rightRight);
    var iw2 = (int)(w1 * 256f);
    return lerp.Lerp(left, right, 256 - iw2, iw2);
  }

  /// <summary>
  /// Calculates vertical cardinal interpolation.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static TWork CardinalInterpolateVertical<TWork, TKey, TLerp>(
    in NeighborWindow<TWork, TKey> window,
    TLerp lerp)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TLerp : struct, ILerp<TWork> {

    var top = window.P0P0.Work;
    var bottom = window.P1P0.Work;
    var topTop = window.M1P0.Work;
    var bottomBottom = window.P2P0.Work;

    var (w0, w1) = CalculateCardinalWeights(topTop, top, bottom, bottomBottom);
    var iw2 = (int)(w1 * 256f);
    return lerp.Lerp(top, bottom, 256 - iw2, iw2);
  }

  /// <summary>
  /// Calculates cardinal weights based on local gradient.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static (float w0, float w1) CalculateCardinalWeights<TWork>(
    in TWork c0, in TWork c1, in TWork c2, in TWork c3)
    where TWork : unmanaged, IColorSpace {

    var l0 = Luma(c0);
    var l1 = Luma(c1);
    var l2 = Luma(c2);
    var l3 = Luma(c3);

    var gradLeft = MathF.Abs(l0 - l1);
    var gradRight = MathF.Abs(l2 - l3);
    var gradCenter = MathF.Abs(l1 - l2);

    if (gradCenter < 0.05f)
      return (0.5f, 0.5f);

    var edgeBias = (gradLeft - gradRight) / (gradLeft + gradRight + 0.001f);
    var w0 = 0.5f - 0.25f * edgeBias;
    var w1 = 0.5f + 0.25f * edgeBias;

    return (w0, w1);
  }
}

file readonly struct Nedi2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 2;
  public int ScaleY => 2;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    var center = window.P0P0.Work;
    var diag = Nedi.DiagonalInterpolate(window, lerp);
    var horiz = Nedi.CardinalInterpolateHorizontal(window, lerp);
    var vert = Nedi.CardinalInterpolateVertical(window, lerp);

    // 2x2 bilinear blend
    // (0,0) = center, (1,0) = horiz, (0,1) = vert, (1,1) = diag
    var e00 = center;
    var e01 = lerp.Lerp(center, horiz);
    var e10 = lerp.Lerp(center, vert);
    var e11 = lerp.Lerp(lerp.Lerp(center, horiz), lerp.Lerp(vert, diag));

    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;

    row0[0] = encoder.Encode(e00);
    row0[1] = encoder.Encode(e01);
    row1[0] = encoder.Encode(e10);
    row1[1] = encoder.Encode(e11);
  }
}

file readonly struct Nedi3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 3;
  public int ScaleY => 3;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    var center = window.P0P0.Work;
    var diag = Nedi.DiagonalInterpolate(window, lerp);
    var horiz = Nedi.CardinalInterpolateHorizontal(window, lerp);
    var vert = Nedi.CardinalInterpolateVertical(window, lerp);

    // 3x3 bilinear blend from NEDI corners
    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;
    var row2 = row1 + destStride;

    for (var oy = 0; oy < 3; ++oy)
    for (var ox = 0; ox < 3; ++ox) {
      var fx = (ox + 0.5f) / 3f;
      var fy = (oy + 0.5f) / 3f;

      var wFx2 = (int)(fx * 256f);
      var wFy2 = (int)(fy * 256f);
      var top = lerp.Lerp(center, horiz, 256 - wFx2, wFx2);
      var bottom = lerp.Lerp(vert, diag, 256 - wFx2, wFx2);
      var result = lerp.Lerp(top, bottom, 256 - wFy2, wFy2);

      var rowPtr = destTopLeft + oy * destStride;
      rowPtr[ox] = encoder.Encode(result);
    }
  }
}

file readonly struct Nedi4xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 4;
  public int ScaleY => 4;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    var center = window.P0P0.Work;
    var diag = Nedi.DiagonalInterpolate(window, lerp);
    var horiz = Nedi.CardinalInterpolateHorizontal(window, lerp);
    var vert = Nedi.CardinalInterpolateVertical(window, lerp);

    // 4x4 bilinear blend from NEDI corners
    for (var oy = 0; oy < 4; ++oy)
    for (var ox = 0; ox < 4; ++ox) {
      var fx = (ox + 0.5f) / 4f;
      var fy = (oy + 0.5f) / 4f;

      var wFx2 = (int)(fx * 256f);
      var wFy2 = (int)(fy * 256f);
      var top = lerp.Lerp(center, horiz, 256 - wFx2, wFx2);
      var bottom = lerp.Lerp(vert, diag, 256 - wFx2, wFx2);
      var result = lerp.Lerp(top, bottom, 256 - wFy2, wFy2);

      var rowPtr = destTopLeft + oy * destStride;
      rowPtr[ox] = encoder.Encode(result);
    }
  }
}
