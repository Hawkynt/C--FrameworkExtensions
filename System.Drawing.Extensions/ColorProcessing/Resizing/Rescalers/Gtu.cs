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
/// GTU (Gaussian-kernel TV Upscaler) scaler - TV signal bandwidth emulation.
/// </summary>
/// <remarks>
/// <para>Reference: https://github.com/aliaspider/interpolation-shaders</para>
/// <para>Algorithm: Gaussian blur simulation of TV bandwidth limitations with scanlines
/// and horizontal color bleeding (lower chroma bandwidth).</para>
/// <para>A different approach from CRT phosphor-based effects, focusing on analog TV signal characteristics.</para>
/// </remarks>
[ScalerInfo("GTU",
  Url = "https://github.com/aliaspider/interpolation-shaders",
  Description = "Gaussian TV upscaler with bandwidth simulation and scanlines", Category = ScalerCategory.Resampler)]
public readonly struct Gtu : IPixelScaler {
  private readonly int _scale;

  /// <summary>
  /// Creates a GTU scaler with the specified factor.
  /// </summary>
  /// <param name="scale">Scale factor (2 or 3).</param>
  public Gtu(int scale = 2) {
    ArgumentOutOfRangeException.ThrowIfLessThan(scale, 2);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(scale, 3);
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
      0 or 2 => callback.Invoke(new Gtu2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      3 => callback.Invoke(new Gtu3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      _ => throw new InvalidOperationException($"Invalid scale factor: {this._scale}")
    };

  /// <summary>
  /// Gets the list of scale factors supported.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2), new(3, 3)];

  /// <summary>
  /// Determines whether the specified scale factor is supported.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 } or { X: 3, Y: 3 };

  /// <summary>
  /// Enumerates all possible target dimensions.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
    yield return (sourceWidth * 3, sourceHeight * 3);
  }

  #region Static Presets

  /// <summary>Gets a 2x GTU scaler.</summary>
  public static Gtu X2 => new(2);

  /// <summary>Gets a 3x GTU scaler.</summary>
  public static Gtu X3 => new(3);

  /// <summary>Gets the default GTU scaler (2x).</summary>
  public static Gtu Default => X2;

  #endregion
}

#region Gtu Helpers

file static class GtuHelpers {
  /// <summary>Weight scale for integer lerp operations.</summary>
  public const int WeightScale = 1000;

  /// <summary>Horizontal blur blend (Gaussian bandwidth simulation, 30%).</summary>
  public const int BlurBlend = 300;

  /// <summary>Color bleed weight (simulates lower chroma bandwidth, 15%).</summary>
  public const int ChromaBleed = 150;

  /// <summary>Scanline darkening for 2x (70% brightness).</summary>
  public const int ScanlineWeight2x = 700;

  /// <summary>Scanline mid row for 3x (75%).</summary>
  public const int ScanlineMid3x = 750;

  /// <summary>Scanline dark gap for 3x (50%).</summary>
  public const int ScanlineDark3x = 500;
}

#endregion

#region Gtu 2x Kernel

file readonly struct Gtu2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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
    var center = window.P0P0.Work;
    var left = window.P0M1.Work;
    var right = window.P0P1.Work;

    // Gaussian horizontal blur: blend with neighbors (bandwidth simulation)
    var blurredLeft = lerp.Lerp(center, left, GtuHelpers.WeightScale - GtuHelpers.BlurBlend, GtuHelpers.BlurBlend);
    var blurredRight = lerp.Lerp(center, right, GtuHelpers.WeightScale - GtuHelpers.BlurBlend, GtuHelpers.BlurBlend);

    // Color bleeding: additional chroma blending from neighbors
    var bledLeft = lerp.Lerp(blurredLeft, left, GtuHelpers.WeightScale - GtuHelpers.ChromaBleed, GtuHelpers.ChromaBleed);
    var bledRight = lerp.Lerp(blurredRight, right, GtuHelpers.WeightScale - GtuHelpers.ChromaBleed, GtuHelpers.ChromaBleed);

    // Scanline row (70% brightness)
    var scanLeft = lerp.Lerp(default, bledLeft, GtuHelpers.WeightScale - GtuHelpers.ScanlineWeight2x, GtuHelpers.ScanlineWeight2x);
    var scanRight = lerp.Lerp(default, bledRight, GtuHelpers.WeightScale - GtuHelpers.ScanlineWeight2x, GtuHelpers.ScanlineWeight2x);

    // 2x2 pattern:
    // [blurred-left]   [blurred-right]
    // [scanline-left]  [scanline-right]
    dest[0] = encoder.Encode(bledLeft);
    dest[1] = encoder.Encode(bledRight);
    dest[destStride] = encoder.Encode(scanLeft);
    dest[destStride + 1] = encoder.Encode(scanRight);
  }
}

#endregion

#region Gtu 3x Kernel

file readonly struct Gtu3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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
    var center = window.P0P0.Work;
    var left = window.P0M1.Work;
    var right = window.P0P1.Work;

    // Gaussian horizontal blur with neighbors
    var blurLeft = lerp.Lerp(center, left, GtuHelpers.WeightScale - GtuHelpers.BlurBlend, GtuHelpers.BlurBlend);
    var blurRight = lerp.Lerp(center, right, GtuHelpers.WeightScale - GtuHelpers.BlurBlend, GtuHelpers.BlurBlend);

    // Color bleeding
    var bledLeft = lerp.Lerp(blurLeft, left, GtuHelpers.WeightScale - GtuHelpers.ChromaBleed, GtuHelpers.ChromaBleed);
    var bledCenter = lerp.Lerp(center, lerp.Lerp(left, right, 500, 500), GtuHelpers.WeightScale - GtuHelpers.ChromaBleed, GtuHelpers.ChromaBleed);
    var bledRight = lerp.Lerp(blurRight, right, GtuHelpers.WeightScale - GtuHelpers.ChromaBleed, GtuHelpers.ChromaBleed);

    // Mid row (75%)
    var midLeft = lerp.Lerp(default, bledLeft, GtuHelpers.WeightScale - GtuHelpers.ScanlineMid3x, GtuHelpers.ScanlineMid3x);
    var midCenter = lerp.Lerp(default, bledCenter, GtuHelpers.WeightScale - GtuHelpers.ScanlineMid3x, GtuHelpers.ScanlineMid3x);
    var midRight = lerp.Lerp(default, bledRight, GtuHelpers.WeightScale - GtuHelpers.ScanlineMid3x, GtuHelpers.ScanlineMid3x);

    // Scanline row (50%)
    var scanline = lerp.Lerp(default, bledCenter, GtuHelpers.WeightScale - GtuHelpers.ScanlineDark3x, GtuHelpers.ScanlineDark3x);
    var scanlinePixel = encoder.Encode(scanline);

    // 3x3 pattern:
    // Row 0: Gaussian-blurred with bandwidth simulation
    dest[0] = encoder.Encode(bledLeft);
    dest[1] = encoder.Encode(bledCenter);
    dest[2] = encoder.Encode(bledRight);

    // Row 1: Medium brightness
    dest[destStride] = encoder.Encode(midLeft);
    dest[destStride + 1] = encoder.Encode(midCenter);
    dest[destStride + 2] = encoder.Encode(midRight);

    // Row 2: Scanline gap
    dest[2 * destStride] = scanlinePixel;
    dest[2 * destStride + 1] = scanlinePixel;
    dest[2 * destStride + 2] = scanlinePixel;
  }
}

#endregion
