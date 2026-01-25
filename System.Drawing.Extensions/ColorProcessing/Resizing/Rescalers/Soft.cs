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
/// Guest's 4xSoft scaling filter - smooth scaling with contrast-based interpolation.
/// </summary>
/// <remarks>
/// <para>Uses 12-point neighborhood analysis with three weighted interpolation paths:</para>
/// <list type="bullet">
///   <item><description>Horizontal/vertical contrast-weighted blend</description></item>
///   <item><description>Diagonal contrast-weighted blend</description></item>
///   <item><description>Inner diagonal blend</description></item>
/// </list>
/// <para>Produces smooth results by blending based on local contrast.</para>
/// <para>Reference: guest(r) 2007 - https://github.com/libretro/common-shaders/tree/master/xsoft</para>
/// </remarks>
[ScalerInfo("4xSoft", Author = "guest(r)", Year = 2007,
  Url = "https://github.com/libretro/common-shaders/tree/master/xsoft",
  Description = "Contrast-weighted soft interpolation", Category = ScalerCategory.Resampler)]
public readonly struct Soft : IPixelScaler {
  /// <inheritdoc />
  public ScaleFactor Scale => new(4, 4);

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
    => callback.Invoke(new Soft4xKernel<TWork, TKey, TPixel, TDistance, TLerp, TEncode>(lerp));

  /// <summary>
  /// Gets the list of scale factors supported.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(4, 4)];

  /// <summary>
  /// Determines whether the specified scale factor is supported.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 4, Y: 4 };

  /// <summary>
  /// Enumerates all possible target dimensions.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 4, sourceHeight * 4);
  }

  #region Static Presets

  /// <summary>Gets a 4x Soft scaler.</summary>
  public static Soft X4 => new();

  /// <summary>Gets the default Soft scaler (4x).</summary>
  public static Soft Default => X4;

  #endregion
}

#region Soft 4x Kernel

file readonly struct Soft4xKernel<TWork, TKey, TPixel, TDistance, TLerp, TEncode>(TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TDistance : struct, IColorMetric<TKey>, INormalizedMetric
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  private const float CONTRAST = 3.0f;

  public int ScaleX => 4;
  public int ScaleY => 4;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    // Get center and all neighbors
    var c11 = window.P0P0;  // Center
    var c00 = window.M1M1;  // Top-left
    var c10 = window.P0M1;  // Top
    var c20 = window.P1M1;  // Top-right
    var c01 = window.M1P0;  // Left
    var c21 = window.P1P0;  // Right
    var c02 = window.M1P1;  // Bottom-left
    var c12 = window.P0P1;  // Bottom
    var c22 = window.P1P1;  // Bottom-right

    // Inner diagonal samples (blend center with corners)
    var s00 = lerp.Lerp(c11.Work, c00.Work);
    var s20 = lerp.Lerp(c11.Work, c20.Work);
    var s22 = lerp.Lerp(c11.Work, c22.Work);
    var s02 = lerp.Lerp(c11.Work, c02.Work);

    // Calculate color distances using the metric
    TDistance metric = default;
    var d1 = _GetDistance(metric, c00.Key, c22.Key) + 0.0001f;  // Main diagonal
    var d2 = _GetDistance(metric, c20.Key, c02.Key) + 0.0001f;  // Anti-diagonal
    var hl = _GetDistance(metric, c01.Key, c21.Key) + 0.0001f;  // Horizontal
    var vl = _GetDistance(metric, c10.Key, c12.Key) + 0.0001f;  // Vertical

    // Calculate T1: Horizontal/Vertical weighted blend
    var hlvl = hl + vl;
    var t1 = _ComputeT1(lerp, c10.Work, c12.Work, c01.Work, c21.Work, c11.Work, hl, vl, hlvl);

    // Calculate T2: Diagonal weighted blend
    var d1d2 = d1 + d2;
    var t2 = _ComputeT2(lerp, c20.Work, c02.Work, c00.Work, c22.Work, c11.Work, d1, d2, d1d2);

    // Calculate T3: Inner diagonal blend
    var t3 = _ComputeT3(lerp, s00, s22, s02, s20, d1, d2);

    // Final result: blend of three interpolation paths
    var result = _BlendThree(lerp, t1, t2, t3, c11.Work);

    // Write 4x4 output block with subpixel variations
    for (var oy = 0; oy < 4; ++oy) {
      var row = dest + oy * destStride;
      for (var ox = 0; ox < 4; ++ox) {
        // Calculate subpixel blend weights
        var fx = (ox + 0.5f) / 4.0f;
        var fy = (oy + 0.5f) / 4.0f;

        // Blend between corners and result based on position
        var subpixelResult = _SubpixelBlend(lerp, c11.Work, c00.Work, c20.Work, c02.Work, c22.Work, result, fx, fy);
        row[ox] = encoder.Encode(subpixelResult);
      }
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _GetDistance(in TDistance metric, in TKey a, in TKey b)
    => metric.Distance(a, b).ToFloat();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork _ComputeT1(in TLerp lerp, in TWork c10, in TWork c12, in TWork c01, in TWork c21, in TWork c11,
    float hl, float vl, float hlvl) {
    // Weight calculation for horizontal/vertical blend
    var hlWeight = (int)(hl / hlvl * 256);
    var vlWeight = 256 - hlWeight;

    // Blend vertical neighbors with horizontal weight
    var vertBlend = lerp.Lerp(c10, c12);
    // Blend horizontal neighbors with vertical weight
    var horzBlend = lerp.Lerp(c01, c21);

    // Combine with center bias
    var hvBlend = lerp.Lerp(vertBlend, horzBlend, hlWeight, vlWeight);
    return lerp.Lerp(hvBlend, c11, 2, 1);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork _ComputeT2(in TLerp lerp, in TWork c20, in TWork c02, in TWork c00, in TWork c22, in TWork c11,
    float d1, float d2, float d1d2) {
    // Weight calculation for diagonal blend
    var d1Weight = (int)(d1 / d1d2 * 256);
    var d2Weight = 256 - d1Weight;

    // Blend anti-diagonal with main diagonal weight
    var antiDiagBlend = lerp.Lerp(c20, c02);
    // Blend main diagonal with anti-diagonal weight
    var mainDiagBlend = lerp.Lerp(c00, c22);

    // Combine with center bias
    var diagBlend = lerp.Lerp(antiDiagBlend, mainDiagBlend, d1Weight, d2Weight);
    return lerp.Lerp(diagBlend, c11, 2, 1);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork _ComputeT3(in TLerp lerp, in TWork s00, in TWork s22, in TWork s02, in TWork s20, float m1, float m2) {
    var total = m1 + m2;
    var m2Weight = (int)(m2 / total * 256);
    var m1Weight = 256 - m2Weight;

    var mainBlend = lerp.Lerp(s00, s22);
    var antiBlend = lerp.Lerp(s02, s20);

    return lerp.Lerp(mainBlend, antiBlend, m2Weight, m1Weight);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork _BlendThree(in TLerp lerp, in TWork t1, in TWork t2, in TWork t3, in TWork center) {
    // Blend the three interpolation results
    var t12 = lerp.Lerp(t1, t2);
    var t123 = lerp.Lerp(t12, t3, 2, 1);
    // Add some center bias
    return lerp.Lerp(t123, center, 3, 1);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork _SubpixelBlend(in TLerp lerp, in TWork center, in TWork c00, in TWork c20, in TWork c02, in TWork c22,
    in TWork result, float fx, float fy) {
    // Distance from center (0.5, 0.5)
    var dx = Math.Abs(fx - 0.5f) * 2f;
    var dy = Math.Abs(fy - 0.5f) * 2f;
    var cornerWeight = (int)(dx * dy * 0.3f * 256f);
    var resultWeight = 256 - cornerWeight;

    // Select appropriate corner based on quadrant
    TWork corner;
    if (fx < 0.5f && fy < 0.5f)
      corner = c00;
    else if (fx >= 0.5f && fy < 0.5f)
      corner = c20;
    else if (fx < 0.5f)
      corner = c02;
    else
      corner = c22;

    return lerp.Lerp(result, corner, resultWeight, cornerWeight);
  }
}

#endregion
