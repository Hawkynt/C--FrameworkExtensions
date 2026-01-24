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
/// Catmull-Rom spline interpolation scaler - smooth curves using 4x4 neighborhood.
/// </summary>
/// <remarks>
/// <para>Algorithm: Catmull-Rom Spline by Edwin Catmull and Raphael Rom (1974)</para>
/// <para>Uses centripetal Catmull-Rom splines for smooth interpolation.</para>
/// <para>Provides C1 continuity with local control - changes affect only adjacent segments.</para>
/// <para>Equivalent to Mitchell-Netravali with B=0, C=0.5 parameters.</para>
/// <para>Reference: https://en.wikipedia.org/wiki/Centripetal_Catmull-Rom_spline</para>
/// </remarks>
[ScalerInfo("Catmull-Rom", Author = "Edwin Catmull, Raphael Rom", Year = 1974,
  Url = "https://en.wikipedia.org/wiki/Centripetal_Catmull-Rom_spline",
  Description = "Catmull-Rom spline interpolation for smooth curves", Category = ScalerCategory.Resampler)]
public readonly struct CatmullRom : IPixelScaler {
  private readonly int _scale;

  /// <summary>
  /// Creates a Catmull-Rom scaler with the specified factor.
  /// </summary>
  /// <param name="scale">Scale factor (2, 3, or 4).</param>
  public CatmullRom(int scale = 2) {
    if (scale is < 2 or > 4)
      throw new ArgumentOutOfRangeException(nameof(scale), scale, "Catmull-Rom supports 2x, 3x, 4x scaling");
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
      0 or 2 => callback.Invoke(new CatmullRom2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      3 => callback.Invoke(new CatmullRom3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      4 => callback.Invoke(new CatmullRom4xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
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

  #region Static Presets

  /// <summary>Gets a 2x Catmull-Rom scaler.</summary>
  public static CatmullRom X2 => new(2);

  /// <summary>Gets a 3x Catmull-Rom scaler.</summary>
  public static CatmullRom X3 => new(3);

  /// <summary>Gets a 4x Catmull-Rom scaler.</summary>
  public static CatmullRom X4 => new(4);

  /// <summary>Gets the default Catmull-Rom scaler (2x).</summary>
  public static CatmullRom Default => X2;

  #endregion
}

#region Catmull-Rom Helpers

/// <summary>
/// Helper methods for Catmull-Rom spline interpolation.
/// </summary>
file static class CatmullRomHelpers {
  /// <summary>
  /// Catmull-Rom spline weight function.
  /// </summary>
  /// <remarks>
  /// Formula: P(t) = 0.5 * ((2*P1) + (-P0+P2)*t + (2*P0-5*P1+4*P2-P3)*t² + (-P0+3*P1-3*P2+P3)*t³)
  /// This is equivalent to Mitchell-Netravali with B=0, C=0.5
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float CatmullRomWeight(float x) {
    x = x < 0 ? -x : x;

    if (x < 1f) {
      // 0.5 * (3*x³ - 5*x² + 2) for |x| < 1
      var x2 = x * x;
      var x3 = x2 * x;
      return 0.5f * (3f * x3 - 5f * x2 + 2f);
    }

    if (x < 2f) {
      // 0.5 * (-x³ + 5*x² - 8*x + 4) for 1 <= |x| < 2
      var x2 = x * x;
      var x3 = x2 * x;
      return 0.5f * (-x3 + 5f * x2 - 8f * x + 4f);
    }

    return 0f;
  }

  /// <summary>
  /// Pre-computed weights for 2x scaling.
  /// </summary>
  public static readonly (float[] wx, float[] wy)[] Weights2x = ComputeWeights(2);

  /// <summary>
  /// Pre-computed weights for 3x scaling.
  /// </summary>
  public static readonly (float[] wx, float[] wy)[] Weights3x = ComputeWeights(3);

  /// <summary>
  /// Pre-computed weights for 4x scaling.
  /// </summary>
  public static readonly (float[] wx, float[] wy)[] Weights4x = ComputeWeights(4);

  private static (float[] wx, float[] wy)[] ComputeWeights(int scale) {
    var result = new (float[] wx, float[] wy)[scale * scale];
    for (var y = 0; y < scale; ++y)
    for (var x = 0; x < scale; ++x) {
      // Map to source coordinates
      var fx = (x + 0.5f) / scale;
      var fy = (y + 0.5f) / scale;

      // Compute weights for 4 horizontal and 4 vertical samples
      var wx = new float[4];
      var wy = new float[4];

      for (var i = -1; i <= 2; ++i) {
        wx[i + 1] = CatmullRomWeight(i - fx);
        wy[i + 1] = CatmullRomWeight(i - fy);
      }

      result[y * scale + x] = (wx, wy);
    }
    return result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void GetNeighborhood<TWork, TKey>(in NeighborWindow<TWork, TKey> window, TWork* p)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace {
    // Row -1
    p[0] = window.M1M1.Work;
    p[1] = window.P0M1.Work;
    p[2] = window.P1M1.Work;
    p[3] = window.P2M1.Work;
    // Row 0
    p[4] = window.M1P0.Work;
    p[5] = window.P0P0.Work;
    p[6] = window.P1P0.Work;
    p[7] = window.P2P0.Work;
    // Row 1
    p[8] = window.M1P1.Work;
    p[9] = window.P0P1.Work;
    p[10] = window.P1P1.Work;
    p[11] = window.P2P1.Work;
    // Row 2
    p[12] = window.M1P2.Work;
    p[13] = window.P0P2.Work;
    p[14] = window.P1P2.Work;
    p[15] = window.P2P2.Work;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe TWork Interpolate<TWork, TLerp>(TWork* p, float[] wx, float[] wy, in TLerp lerp)
    where TWork : unmanaged, IColorSpace
    where TLerp : struct, ILerp<TWork> {
    // First interpolate 4 rows horizontally
    var row0 = InterpolateRow(p, 0, wx, lerp);
    var row1 = InterpolateRow(p, 4, wx, lerp);
    var row2 = InterpolateRow(p, 8, wx, lerp);
    var row3 = InterpolateRow(p, 12, wx, lerp);

    // Then interpolate vertically
    return InterpolateVertical(row0, row1, row2, row3, wy, lerp);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe TWork InterpolateRow<TWork, TLerp>(TWork* p, int offset, float[] wx, in TLerp lerp)
    where TWork : unmanaged, IColorSpace
    where TLerp : struct, ILerp<TWork> {
    // Normalize weights
    var sum = wx[0] + wx[1] + wx[2] + wx[3];
    if (sum < 0.0001f)
      return p[offset + 1];

    var w0 = (int)(wx[0] / sum * 256);
    var w1 = (int)(wx[1] / sum * 256);
    var w2 = (int)(wx[2] / sum * 256);
    var w3 = 256 - w0 - w1 - w2;

    var ab = lerp.Lerp(p[offset], p[offset + 1], w0, w1);
    var cd = lerp.Lerp(p[offset + 2], p[offset + 3], w2, w3);
    return lerp.Lerp(ab, cd);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork InterpolateVertical<TWork, TLerp>(in TWork r0, in TWork r1, in TWork r2, in TWork r3, float[] wy, in TLerp lerp)
    where TWork : unmanaged, IColorSpace
    where TLerp : struct, ILerp<TWork> {
    var sum = wy[0] + wy[1] + wy[2] + wy[3];
    if (sum < 0.0001f)
      return r1;

    var w0 = (int)(wy[0] / sum * 256);
    var w1 = (int)(wy[1] / sum * 256);
    var w2 = (int)(wy[2] / sum * 256);
    var w3 = 256 - w0 - w1 - w2;

    var ab = lerp.Lerp(r0, r1, w0, w1);
    var cd = lerp.Lerp(r2, r3, w2, w3);
    return lerp.Lerp(ab, cd);
  }
}

#endregion

#region Catmull-Rom 2x Kernel

file readonly struct CatmullRom2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    // Get 4x4 neighborhood
    var p = stackalloc TWork[16];
    CatmullRomHelpers.GetNeighborhood(window, p);

    var weights = CatmullRomHelpers.Weights2x;

    for (var y = 0; y < 2; ++y) {
      var row = dest + y * destStride;
      for (var x = 0; x < 2; ++x) {
        var (wx, wy) = weights[y * 2 + x];
        var result = CatmullRomHelpers.Interpolate(p, wx, wy, lerp);
        row[x] = encoder.Encode(result);
      }
    }
  }
}

#endregion

#region Catmull-Rom 3x Kernel

file readonly struct CatmullRom3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var p = stackalloc TWork[16];
    CatmullRomHelpers.GetNeighborhood(window, p);

    var weights = CatmullRomHelpers.Weights3x;

    for (var y = 0; y < 3; ++y) {
      var row = dest + y * destStride;
      for (var x = 0; x < 3; ++x) {
        var (wx, wy) = weights[y * 3 + x];
        var result = CatmullRomHelpers.Interpolate(p, wx, wy, lerp);
        row[x] = encoder.Encode(result);
      }
    }
  }
}

#endregion

#region Catmull-Rom 4x Kernel

file readonly struct CatmullRom4xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var p = stackalloc TWork[16];
    CatmullRomHelpers.GetNeighborhood(window, p);

    var weights = CatmullRomHelpers.Weights4x;

    for (var y = 0; y < 4; ++y) {
      var row = dest + y * destStride;
      for (var x = 0; x < 4; ++x) {
        var (wx, wy) = weights[y * 4 + x];
        var result = CatmullRomHelpers.Interpolate(p, wx, wy, lerp);
        row[x] = encoder.Encode(result);
      }
    }
  }
}

#endregion
