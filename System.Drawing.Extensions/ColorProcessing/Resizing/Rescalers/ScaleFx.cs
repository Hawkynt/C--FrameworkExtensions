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

#region ScaleFX 3x

/// <summary>
/// ScaleFX 3x edge interpolation algorithm by Sp00kyFox.
/// </summary>
/// <remarks>
/// <para>Multi-pass edge detection that interpolates edges with smooth slope transitions.</para>
/// <para>Uses perceptual color distance and corner strength analysis.</para>
/// <para>Output consists only of colors present in the original image (no blending).</para>
/// <para>Reference: https://github.com/libretro/common-shaders/tree/master/scalefx</para>
/// </remarks>
[ScalerInfo("ScaleFX 3x", Author = "Sp00kyFox", Year = 2016,
  Description = "ScaleFX 3x edge interpolation", Category = ScalerCategory.PixelArt,
  Url = "https://github.com/libretro/common-shaders/tree/master/scalefx")]
public readonly struct ScaleFx3x : IPixelScaler {

  /// <inheritdoc />
  public ScaleFactor Scale => new(3, 3);

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
    => callback.Invoke(new ScaleFx3xKernel<TWork, TKey, TPixel, TDistance, TEncode>());

  /// <summary>Gets the list of scale factors supported by ScaleFX 3x.</summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(3, 3)];

  /// <summary>Determines whether ScaleFX 3x supports the specified scale factor.</summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 3, Y: 3 };

  /// <summary>Enumerates all possible target dimensions for ScaleFX 3x.</summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 3, sourceHeight * 3);
  }

  /// <summary>Gets the default ScaleFX 3x configuration.</summary>
  public static ScaleFx3x Default => new();
}

#endregion

#region ScaleFX Helpers

/// <summary>
/// Helper constants and methods for ScaleFX algorithm.
/// </summary>
file static class ScaleFxHelpers {
  public const float DefaultThreshold = 0.50f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float CornerStrength(UNorm32 d, UNorm32 a1, UNorm32 a2, UNorm32 b1, UNorm32 b2, float threshold) {
    var df = (float)d;
    var a1f = (float)a1;
    var a2f = (float)a2;
    var b1f = (float)b1;
    var b2f = (float)b2;
    var diff = a1f - a2f;
    var wght1 = Math.Max(threshold - df, 0f) / threshold;
    var rawWght2 = (1f - df) + (Math.Min(a1f, b1f) + a1f > Math.Min(a2f, b2f) + a2f ? diff : -diff);
    var wght2 = rawWght2 < 0f ? 0f : rawWght2 > 1f ? 1f : rawWght2;
    return 2f * df < a1f + a2f ? wght1 * wght2 * a1f * a2f : 0f;
  }
}

#endregion

#region ScaleFX 3x Kernel

file readonly struct ScaleFx3xKernel<TWork, TKey, TPixel, TMetric, TEncode>
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TMetric : struct, IColorMetric<TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  private readonly TMetric _metric;

  public ScaleFx3xKernel(TMetric metric = default) => this._metric = metric;

  public int ScaleX => 3;
  public int ScaleY => 3;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    // Extract neighborhood (5x5 used for multi-pass edge analysis)
    // NeighborWindow uses [Row][Column] naming: M1P0 = row -1, col 0 = TOP
    var a = window.M1M1; var b = window.M1P0; var c = window.M1P1; // row -1
    var d = window.P0M1; var e = window.P0P0; var f = window.P0P1; // row 0
    var g = window.P1M1; var h = window.P1P0; var i = window.P1P1; // row +1

    // Additional neighbors for extended analysis
    var a0 = window.M1M2; var b0 = window.M2P0; var c0 = window.M1P2; // row -2/-1: far corners and top
    var d0 = window.P0M2;                         var f0 = window.P0P2; // row 0: far left and right
    var g0 = window.P1M2; var h0 = window.P2P0; var i0 = window.P1P2; // row +1/+2: far corners and bottom

    // Compute color distances from center (E)
    var distEA = this._metric.Distance(e.Key, a.Key);
    var distEB = this._metric.Distance(e.Key, b.Key);
    var distEC = this._metric.Distance(e.Key, c.Key);
    var distED = this._metric.Distance(e.Key, d.Key);
    var distEF = this._metric.Distance(e.Key, f.Key);
    var distEG = this._metric.Distance(e.Key, g.Key);
    var distEH = this._metric.Distance(e.Key, h.Key);
    var distEI = this._metric.Distance(e.Key, i.Key);

    // Additional distances for corner strength
    var distAD = this._metric.Distance(a.Key, d.Key);
    var distAB = this._metric.Distance(a.Key, b.Key);
    var distBC = this._metric.Distance(b.Key, c.Key);
    var distBF = this._metric.Distance(b.Key, f.Key);
    var distCF = this._metric.Distance(c.Key, f.Key);
    var distFI = this._metric.Distance(f.Key, i.Key);
    var distHI = this._metric.Distance(h.Key, i.Key);
    var distHG = this._metric.Distance(h.Key, g.Key);
    var distDG = this._metric.Distance(d.Key, g.Key);
    var distDH = this._metric.Distance(d.Key, h.Key);

    const float threshold = 0.50f;

    // Compute corner strengths for all 4 corners
    var strTL = ScaleFxHelpers.CornerStrength(distEA, distED, distEB, distAD, distAB, threshold);
    var strTR = ScaleFxHelpers.CornerStrength(distEC, distEB, distEF, distBC, distCF, threshold);
    var strBR = ScaleFxHelpers.CornerStrength(distEI, distEF, distEH, distFI, distHI, threshold);
    var strBL = ScaleFxHelpers.CornerStrength(distEG, distEH, distED, distHG, distDG, threshold);

    // Select output pixels based on corner strength
    // ScaleFX outputs only original colors (no interpolation)
    var wE = e.Work;
    var wB = b.Work;
    var wD = d.Work;
    var wF = f.Work;
    var wH = h.Work;

    // Top-left corner
    var tl = strTL > 0.001f ? (distED < distEB ? wD : wB) : wE;
    // Top-right corner
    var tr = strTR > 0.001f ? (distEB < distEF ? wB : wF) : wE;
    // Bottom-right corner
    var br = strBR > 0.001f ? (distEF < distEH ? wF : wH) : wE;
    // Bottom-left corner
    var bl = strBL > 0.001f ? (distEH < distED ? wH : wD) : wE;

    // Top edge midpoint
    var tm = strTL > 0.001f && strTR > 0.001f ? wB : (strTL > strTR ? tl : strTR > strTL ? tr : wE);
    // Right edge midpoint
    var rm = strTR > 0.001f && strBR > 0.001f ? wF : (strTR > strBR ? tr : strBR > strTR ? br : wE);
    // Bottom edge midpoint
    var bm = strBR > 0.001f && strBL > 0.001f ? wH : (strBR > strBL ? br : strBL > strBR ? bl : wE);
    // Left edge midpoint
    var lm = strBL > 0.001f && strTL > 0.001f ? wD : (strBL > strTL ? bl : strTL > strBL ? tl : wE);

    // Write 3x3 output
    dest[0] = encoder.Encode(tl);
    dest[1] = encoder.Encode(tm);
    dest[2] = encoder.Encode(tr);
    dest[destStride] = encoder.Encode(lm);
    dest[destStride + 1] = encoder.Encode(wE);
    dest[destStride + 2] = encoder.Encode(rm);
    dest[destStride * 2] = encoder.Encode(bl);
    dest[destStride * 2 + 1] = encoder.Encode(bm);
    dest[destStride * 2 + 2] = encoder.Encode(br);
  }
}

#endregion
