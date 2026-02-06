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
/// SMAA - Subpixel Morphological Anti-Aliasing (Jimenez et al. 2012).
/// </summary>
/// <remarks>
/// <para>Reference: https://www.iryoku.com/smaa/</para>
/// <para>Paper: "SMAA: Enhanced Subpixel Morphological Antialiasing" - EUROGRAPHICS 2012</para>
/// <para>Enhanced MLAA with subpixel precision and diagonal pattern detection:</para>
/// <list type="bullet">
/// <item>Phase 1: Edge detection with local contrast adaptation</item>
/// <item>Phase 2: Pattern recognition including diagonal edge detection</item>
/// <item>Phase 3: Corner detection for artifact prevention</item>
/// <item>Phase 4: Subpixel blending for smoother anti-aliasing</item>
/// </list>
/// </remarks>
[ScalerInfo("SMAA", Author = "Jorge Jimenez et al.", Year = 2012, Url = "https://www.iryoku.com/smaa/",
  Description = "Subpixel Morphological Anti-Aliasing", Category = ScalerCategory.PixelArt)]
public readonly struct Smaa : IPixelScaler {

  private readonly int _scale;
  private readonly SmaaQuality _quality;

  /// <summary>
  /// Creates a new SMAA instance.
  /// </summary>
  /// <param name="scale">Scale factor (2, 3, or 4).</param>
  /// <param name="quality">Quality preset (Low, Standard, High, Ultra).</param>
  public Smaa(int scale = 2, SmaaQuality quality = SmaaQuality.Standard) {
    ArgumentOutOfRangeException.ThrowIfLessThan(scale, 2);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(scale, 4);
    this._scale = scale;
    this._quality = quality;
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
    where TEncode : struct, IEncode<TWork, TPixel> {
    var (edgeThreshold, contrastFactor) = SmaaHelpers.GetQualityParams(this._quality);
    return this._scale switch {
      0 or 2 => callback.Invoke(new Smaa2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp, edgeThreshold, contrastFactor)),
      3 => callback.Invoke(new Smaa3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp, edgeThreshold, contrastFactor)),
      4 => callback.Invoke(new Smaa4xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp, edgeThreshold, contrastFactor)),
      _ => throw new InvalidOperationException($"Invalid scale factor: {this._scale}")
    };
  }

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

  /// <summary>Gets a 2x SMAA scaler with standard quality.</summary>
  public static Smaa X2 => new(2);

  /// <summary>Gets a 3x SMAA scaler with standard quality.</summary>
  public static Smaa X3 => new(3);

  /// <summary>Gets a 4x SMAA scaler with standard quality.</summary>
  public static Smaa X4 => new(4);

  /// <summary>Gets the default SMAA scaler (2x standard).</summary>
  public static Smaa Default => X2;

  /// <summary>Gets a 2x SMAA scaler with low quality (faster).</summary>
  public static Smaa X2Low => new(2, SmaaQuality.Low);

  /// <summary>Gets a 2x SMAA scaler with high quality.</summary>
  public static Smaa X2High => new(2, SmaaQuality.High);

  /// <summary>Gets a 2x SMAA scaler with ultra quality (best).</summary>
  public static Smaa X2Ultra => new(2, SmaaQuality.Ultra);

  #endregion

  /// <summary>
  /// Creates a configuration with the specified quality.
  /// </summary>
  public Smaa WithQuality(SmaaQuality quality) => new(this._scale == 0 ? 2 : this._scale, quality);
}

/// <summary>
/// SMAA quality presets.
/// </summary>
public enum SmaaQuality {
  /// <summary>Low quality - conservative edge detection, less smoothing.</summary>
  Low,
  /// <summary>Standard quality - balanced edge detection and smoothing.</summary>
  Standard,
  /// <summary>High quality - sensitive edge detection, more smoothing.</summary>
  High,
  /// <summary>Ultra quality - maximum smoothing for best quality.</summary>
  Ultra
}

#region SMAA Helpers

file static class SmaaHelpers {

  // Quality preset parameters
  private const float LowEdgeThreshold = 0.15f;
  private const float LowContrastFactor = 0.3f;
  private const float StandardEdgeThreshold = 0.1f;
  private const float StandardContrastFactor = 0.5f;
  private const float HighEdgeThreshold = 0.05f;
  private const float HighContrastFactor = 0.6f;
  private const float UltraEdgeThreshold = 0.03f;
  private const float UltraContrastFactor = 0.7f;

  /// <summary>
  /// Gets edge threshold and contrast factor for the specified quality.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (float edgeThreshold, float contrastFactor) GetQualityParams(SmaaQuality quality) => quality switch {
    SmaaQuality.Low => (LowEdgeThreshold, LowContrastFactor),
    SmaaQuality.Standard => (StandardEdgeThreshold, StandardContrastFactor),
    SmaaQuality.High => (HighEdgeThreshold, HighContrastFactor),
    SmaaQuality.Ultra => (UltraEdgeThreshold, UltraContrastFactor),
    _ => (StandardEdgeThreshold, StandardContrastFactor)
  };

  /// <summary>
  /// Computes local contrast for adaptive thresholding.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float CalculateLocalContrast(float lCenter, float lN, float lS, float lE, float lW) {
    var maxLum = MathF.Max(lCenter, MathF.Max(MathF.Max(lN, lS), MathF.Max(lE, lW)));
    var minLum = MathF.Min(lCenter, MathF.Min(MathF.Min(lN, lS), MathF.Min(lE, lW)));
    return maxLum - minLum;
  }

  /// <summary>
  /// Smooth step function for gradual transitions.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float SmoothStep(float x) {
    x = MathF.Max(0, MathF.Min(1, x));
    return x * x * (3 - 2 * x);
  }

  /// <summary>
  /// Calculates blend weight for horizontal edges based on subpixel position.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float CalculateHorizontalBlendWeight(float fy, bool edgePositive)
    => SmoothStep(edgePositive ? 1 - fy : fy);

  /// <summary>
  /// Calculates blend weight for vertical edges based on subpixel position.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float CalculateVerticalBlendWeight(float fx, bool edgePositive)
    => SmoothStep(edgePositive ? 1 - fx : fx);

  /// <summary>
  /// Detects if the current pixel is at a corner (to prevent over-blending).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsCorner(bool hEdge, bool vEdge, bool diagOpposite1, bool diagOpposite2)
    => hEdge && vEdge && !diagOpposite1 && !diagOpposite2;

  /// <summary>
  /// Applies SMAA edge detection and subpixel blending for a single output pixel.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TWork ApplySmaaSubpixel<TWork, TLerp>(
    in TWork center, in TWork n, in TWork s, in TWork e, in TWork w,
    in TWork nw, in TWork ne, in TWork sw, in TWork se,
    float fx, float fy,
    float edgeThreshold, float contrastFactor,
    TLerp lerp)
    where TWork : unmanaged, IColorSpace
    where TLerp : struct, ILerp<TWork> {

    // Calculate luminance values
    var lCenter = ColorConverter.GetLuminance(center);
    var lN = ColorConverter.GetLuminance(n);
    var lS = ColorConverter.GetLuminance(s);
    var lE = ColorConverter.GetLuminance(e);
    var lW = ColorConverter.GetLuminance(w);
    var lNW = ColorConverter.GetLuminance(nw);
    var lNE = ColorConverter.GetLuminance(ne);
    var lSW = ColorConverter.GetLuminance(sw);
    var lSE = ColorConverter.GetLuminance(se);

    // Calculate local contrast for adaptive thresholding
    var localContrast = CalculateLocalContrast(lCenter, lN, lS, lE, lW);
    var adaptiveThreshold = edgeThreshold + localContrast * contrastFactor;

    // Detect edges
    var edgeHorizontal = lCenter - lN;
    var edgeVertical = lCenter - lW;
    var hasHEdge = MathF.Abs(edgeHorizontal) > adaptiveThreshold;
    var hasVEdge = MathF.Abs(edgeVertical) > adaptiveThreshold;

    // Detect diagonal edges
    var diagNW = lCenter - lNW;
    var diagNE = lCenter - lNE;
    var diagSW = lCenter - lSW;
    var diagSE = lCenter - lSE;

    var hasDiagNW = MathF.Abs(diagNW) > adaptiveThreshold;
    var hasDiagNE = MathF.Abs(diagNE) > adaptiveThreshold;
    var hasDiagSW = MathF.Abs(diagSW) > adaptiveThreshold;
    var hasDiagSE = MathF.Abs(diagSE) > adaptiveThreshold;

    var result = center;

    // Apply horizontal edge blending
    if (hasHEdge) {
      var blendWeight = CalculateHorizontalBlendWeight(fy, edgeHorizontal > 0);
      var blendW2 = (int)(blendWeight * 0.5f * 256f);
      if (blendW2 > 0)
        result = lerp.Lerp(result, n, 256 - blendW2, blendW2);
    }

    // Apply vertical edge blending
    if (hasVEdge) {
      var blendWeight = CalculateVerticalBlendWeight(fx, edgeVertical > 0);
      var blendW2 = (int)(blendWeight * 0.5f * 256f);
      if (blendW2 > 0)
        result = lerp.Lerp(result, w, 256 - blendW2, blendW2);
    }

    // Apply diagonal blending with corner detection
    if (hasDiagNW && fx < 0.5f && fy < 0.5f && !IsCorner(hasHEdge, hasVEdge, hasDiagNE, hasDiagSW)) {
      var diagWeight = (0.5f - fx) * (0.5f - fy) * 2;
      var blendW2 = (int)(diagWeight * 0.25f * 256f);
      if (blendW2 > 0)
        result = lerp.Lerp(result, nw, 256 - blendW2, blendW2);
    }

    if (hasDiagNE && fx > 0.5f && fy < 0.5f && !IsCorner(hasHEdge, hasVEdge, hasDiagNW, hasDiagSE)) {
      var diagWeight = (fx - 0.5f) * (0.5f - fy) * 2;
      var blendW2 = (int)(diagWeight * 0.25f * 256f);
      if (blendW2 > 0)
        result = lerp.Lerp(result, ne, 256 - blendW2, blendW2);
    }

    if (hasDiagSW && fx < 0.5f && fy > 0.5f && !IsCorner(hasHEdge, hasVEdge, hasDiagNW, hasDiagSE)) {
      var diagWeight = (0.5f - fx) * (fy - 0.5f) * 2;
      var blendW2 = (int)(diagWeight * 0.25f * 256f);
      if (blendW2 > 0)
        result = lerp.Lerp(result, sw, 256 - blendW2, blendW2);
    }

    if (hasDiagSE && fx > 0.5f && fy > 0.5f && !IsCorner(hasHEdge, hasVEdge, hasDiagNE, hasDiagSW)) {
      var diagWeight = (fx - 0.5f) * (fy - 0.5f) * 2;
      var blendW2 = (int)(diagWeight * 0.25f * 256f);
      if (blendW2 > 0)
        result = lerp.Lerp(result, se, 256 - blendW2, blendW2);
    }

    return result;
  }
}

#endregion

#region SMAA 2x Kernel

file readonly struct Smaa2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp, float edgeThreshold, float contrastFactor)
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
    // Get 3x3 neighborhood
    var center = window.P0P0.Work;
    var n = window.M1P0.Work;
    var s = window.P1P0.Work;
    var e = window.P0P1.Work;
    var w = window.P0M1.Work;
    var nw = window.M1M1.Work;
    var ne = window.M1P1.Work;
    var sw = window.P1M1.Work;
    var se = window.P1P1.Work;

    // Apply SMAA with subpixel blending for each output pixel
    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;

    row0[0] = encoder.Encode(SmaaHelpers.ApplySmaaSubpixel(center, n, s, e, w, nw, ne, sw, se, 0.25f, 0.25f, edgeThreshold, contrastFactor, lerp));
    row0[1] = encoder.Encode(SmaaHelpers.ApplySmaaSubpixel(center, n, s, e, w, nw, ne, sw, se, 0.75f, 0.25f, edgeThreshold, contrastFactor, lerp));
    row1[0] = encoder.Encode(SmaaHelpers.ApplySmaaSubpixel(center, n, s, e, w, nw, ne, sw, se, 0.25f, 0.75f, edgeThreshold, contrastFactor, lerp));
    row1[1] = encoder.Encode(SmaaHelpers.ApplySmaaSubpixel(center, n, s, e, w, nw, ne, sw, se, 0.75f, 0.75f, edgeThreshold, contrastFactor, lerp));
  }
}

#endregion

#region SMAA 3x Kernel

file readonly struct Smaa3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp, float edgeThreshold, float contrastFactor)
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
    var n = window.M1P0.Work;
    var s = window.P1P0.Work;
    var e = window.P0P1.Work;
    var w = window.P0M1.Work;
    var nw = window.M1M1.Work;
    var ne = window.M1P1.Work;
    var sw = window.P1M1.Work;
    var se = window.P1P1.Work;

    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;
    var row2 = row1 + destStride;

    // Subpixel positions for 3x3 grid: 1/6, 3/6, 5/6
    const float p0 = 1f / 6f;
    const float p1 = 0.5f;
    const float p2 = 5f / 6f;

    row0[0] = encoder.Encode(SmaaHelpers.ApplySmaaSubpixel(center, n, s, e, w, nw, ne, sw, se, p0, p0, edgeThreshold, contrastFactor, lerp));
    row0[1] = encoder.Encode(SmaaHelpers.ApplySmaaSubpixel(center, n, s, e, w, nw, ne, sw, se, p1, p0, edgeThreshold, contrastFactor, lerp));
    row0[2] = encoder.Encode(SmaaHelpers.ApplySmaaSubpixel(center, n, s, e, w, nw, ne, sw, se, p2, p0, edgeThreshold, contrastFactor, lerp));
    row1[0] = encoder.Encode(SmaaHelpers.ApplySmaaSubpixel(center, n, s, e, w, nw, ne, sw, se, p0, p1, edgeThreshold, contrastFactor, lerp));
    row1[1] = encoder.Encode(SmaaHelpers.ApplySmaaSubpixel(center, n, s, e, w, nw, ne, sw, se, p1, p1, edgeThreshold, contrastFactor, lerp));
    row1[2] = encoder.Encode(SmaaHelpers.ApplySmaaSubpixel(center, n, s, e, w, nw, ne, sw, se, p2, p1, edgeThreshold, contrastFactor, lerp));
    row2[0] = encoder.Encode(SmaaHelpers.ApplySmaaSubpixel(center, n, s, e, w, nw, ne, sw, se, p0, p2, edgeThreshold, contrastFactor, lerp));
    row2[1] = encoder.Encode(SmaaHelpers.ApplySmaaSubpixel(center, n, s, e, w, nw, ne, sw, se, p1, p2, edgeThreshold, contrastFactor, lerp));
    row2[2] = encoder.Encode(SmaaHelpers.ApplySmaaSubpixel(center, n, s, e, w, nw, ne, sw, se, p2, p2, edgeThreshold, contrastFactor, lerp));
  }
}

#endregion

#region SMAA 4x Kernel

file readonly struct Smaa4xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp, float edgeThreshold, float contrastFactor)
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
    var n = window.M1P0.Work;
    var s = window.P1P0.Work;
    var e = window.P0P1.Work;
    var w = window.P0M1.Work;
    var nw = window.M1M1.Work;
    var ne = window.M1P1.Work;
    var sw = window.P1M1.Work;
    var se = window.P1P1.Work;

    // Subpixel positions for 4x4 grid: 1/8, 3/8, 5/8, 7/8
    const float p0 = 0.125f;
    const float p1 = 0.375f;
    const float p2 = 0.625f;
    const float p3 = 0.875f;

    for (var dy = 0; dy < 4; ++dy) {
      var row = destTopLeft + dy * destStride;
      var fy = dy switch { 0 => p0, 1 => p1, 2 => p2, _ => p3 };

      row[0] = encoder.Encode(SmaaHelpers.ApplySmaaSubpixel(center, n, s, e, w, nw, ne, sw, se, p0, fy, edgeThreshold, contrastFactor, lerp));
      row[1] = encoder.Encode(SmaaHelpers.ApplySmaaSubpixel(center, n, s, e, w, nw, ne, sw, se, p1, fy, edgeThreshold, contrastFactor, lerp));
      row[2] = encoder.Encode(SmaaHelpers.ApplySmaaSubpixel(center, n, s, e, w, nw, ne, sw, se, p2, fy, edgeThreshold, contrastFactor, lerp));
      row[3] = encoder.Encode(SmaaHelpers.ApplySmaaSubpixel(center, n, s, e, w, nw, ne, sw, se, p3, fy, edgeThreshold, contrastFactor, lerp));
    }
  }
}

#endregion
