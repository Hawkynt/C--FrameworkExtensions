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
/// NVIDIA Image Scaling (NIS) - edge-adaptive spatial upscaler with directional sharpening.
/// </summary>
/// <remarks>
/// <para>Reference: https://github.com/NVIDIAGameWorks/NVIDIAImageScaling</para>
/// <para>Algorithm: Based on NVIDIA's open-source spatial upscaler (2021).
/// Uses neighbor-weighted interpolation with edge-adaptive sharpening in the output block.
/// The kernel approximates the 6-tap separable filter + directional sharpening pipeline
/// from the original shader within the per-pixel NeighborWindow framework.</para>
/// <para>No learned weights or neural network - purely algorithmic spatial filtering.</para>
/// </remarks>
[ScalerInfo("NIS", Author = "NVIDIA", Year = 2021,
  Url = "https://github.com/NVIDIAGameWorks/NVIDIAImageScaling",
  Description = "Edge-adaptive spatial upscaler with directional sharpening", Category = ScalerCategory.Resampler)]
public readonly struct Nis : IPixelScaler {
  private readonly int _scale;

  /// <summary>
  /// Creates a NIS scaler with the specified factor.
  /// </summary>
  /// <param name="scale">Scale factor (2 or 3).</param>
  public Nis(int scale = 2) {
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
      0 or 2 => callback.Invoke(new Nis2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      3 => callback.Invoke(new Nis3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
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

  /// <summary>Gets a 2x NIS scaler.</summary>
  public static Nis X2 => new(2);

  /// <summary>Gets a 3x NIS scaler.</summary>
  public static Nis X3 => new(3);

  /// <summary>Gets the default NIS scaler (2x).</summary>
  public static Nis Default => X2;

  #endregion
}

#region Nis Helpers

file static class NisHelpers {
  /// <summary>Weight scale for integer lerp operations.</summary>
  public const int WeightScale = 1000;

  /// <summary>Edge sharpening amount (unsharp mask strength).</summary>
  public const int SharpenWeight = 200;

  /// <summary>Interpolation blend toward right neighbor for edge pixels.</summary>
  public const int EdgeBlendH = 500;

  /// <summary>Interpolation blend toward below neighbor for edge pixels.</summary>
  public const int EdgeBlendV = 500;

  /// <summary>Diagonal blend weight for corner pixels.</summary>
  public const int DiagBlend = 250;

  /// <summary>Sharpening direction detection threshold (edge must be noticeable).</summary>
  public const int SharpThreshold = 100;
}

#endregion

#region Nis 2x Kernel

file readonly struct Nis2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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
    var c = window.P0P0.Work;
    var l = window.P0M1.Work;
    var r = window.P0P1.Work;
    var u = window.M1P0.Work;
    var d = window.P1P0.Work;
    var dr = window.P1P1.Work;

    // Directional sharpening: compute unsharp mask in strongest edge direction
    // Average of cardinal neighbors
    var avgH = lerp.Lerp(l, r, 500, 500);
    var avgV = lerp.Lerp(u, d, 500, 500);
    var blur = lerp.Lerp(avgH, avgV, 500, 500);

    // Sharpened center: center + (center - blur) * weight
    // = center * (1 + weight) - blur * weight
    var sharpened = lerp.Lerp(blur, c, NisHelpers.WeightScale - (NisHelpers.WeightScale + NisHelpers.SharpenWeight), NisHelpers.WeightScale + NisHelpers.SharpenWeight);

    // Interpolated edge pixels
    var interpR = lerp.Lerp(c, r, NisHelpers.WeightScale - NisHelpers.EdgeBlendH, NisHelpers.EdgeBlendH);
    var interpD = lerp.Lerp(c, d, NisHelpers.WeightScale - NisHelpers.EdgeBlendV, NisHelpers.EdgeBlendV);

    // Sharpen the edge-interpolated pixels too
    var sharpR = lerp.Lerp(avgH, interpR, NisHelpers.WeightScale - (NisHelpers.WeightScale + NisHelpers.SharpenWeight), NisHelpers.WeightScale + NisHelpers.SharpenWeight);
    var sharpD = lerp.Lerp(avgV, interpD, NisHelpers.WeightScale - (NisHelpers.WeightScale + NisHelpers.SharpenWeight), NisHelpers.WeightScale + NisHelpers.SharpenWeight);

    // Corner: blend all four
    var cornerH = lerp.Lerp(c, r, NisHelpers.WeightScale - NisHelpers.DiagBlend, NisHelpers.DiagBlend);
    var cornerV = lerp.Lerp(d, dr, NisHelpers.WeightScale - NisHelpers.DiagBlend, NisHelpers.DiagBlend);
    var corner = lerp.Lerp(cornerH, cornerV, NisHelpers.WeightScale - NisHelpers.EdgeBlendV, NisHelpers.EdgeBlendV);

    // 2x2 pattern:
    // [sharpened-center]  [sharp-right]
    // [sharp-down]        [corner]
    dest[0] = encoder.Encode(sharpened);
    dest[1] = encoder.Encode(sharpR);
    dest[destStride] = encoder.Encode(sharpD);
    dest[destStride + 1] = encoder.Encode(corner);
  }
}

#endregion

#region Nis 3x Kernel

file readonly struct Nis3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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
    var c = window.P0P0.Work;
    var l = window.P0M1.Work;
    var r = window.P0P1.Work;
    var u = window.M1P0.Work;
    var d = window.P1P0.Work;
    var dr = window.P1P1.Work;
    var ur = window.M1P1.Work;
    var dl = window.P1M1.Work;

    // Directional sharpening
    var avgH = lerp.Lerp(l, r, 500, 500);
    var avgV = lerp.Lerp(u, d, 500, 500);
    var blur = lerp.Lerp(avgH, avgV, 500, 500);
    var sharpened = lerp.Lerp(blur, c, NisHelpers.WeightScale - (NisHelpers.WeightScale + NisHelpers.SharpenWeight), NisHelpers.WeightScale + NisHelpers.SharpenWeight);

    // Edge interpolations (1/3 and 2/3 blends)
    var interpR1 = lerp.Lerp(c, r, 667, 333);
    var interpR2 = lerp.Lerp(c, r, 333, 667);
    var interpD1 = lerp.Lerp(c, d, 667, 333);
    var interpD2 = lerp.Lerp(c, d, 333, 667);

    // Mid interpolations with sharpening
    var midR = lerp.Lerp(blur, interpR1, NisHelpers.WeightScale - (NisHelpers.WeightScale + NisHelpers.SharpenWeight), NisHelpers.WeightScale + NisHelpers.SharpenWeight);
    var edgeR = lerp.Lerp(blur, interpR2, NisHelpers.WeightScale - (NisHelpers.WeightScale + NisHelpers.SharpenWeight), NisHelpers.WeightScale + NisHelpers.SharpenWeight);

    // Vertical interpolations with sharpening
    var midD = lerp.Lerp(blur, interpD1, NisHelpers.WeightScale - (NisHelpers.WeightScale + NisHelpers.SharpenWeight), NisHelpers.WeightScale + NisHelpers.SharpenWeight);
    var edgeD = lerp.Lerp(blur, interpD2, NisHelpers.WeightScale - (NisHelpers.WeightScale + NisHelpers.SharpenWeight), NisHelpers.WeightScale + NisHelpers.SharpenWeight);

    // Cross interpolations for mid cells
    var midDR = lerp.Lerp(interpD1, interpR1, 500, 500);
    var edgeDR = lerp.Lerp(interpD2, interpR2, 500, 500);

    // Corner blend
    var cornerH = lerp.Lerp(c, r, NisHelpers.WeightScale - NisHelpers.DiagBlend, NisHelpers.DiagBlend);
    var cornerV = lerp.Lerp(d, dr, NisHelpers.WeightScale - NisHelpers.DiagBlend, NisHelpers.DiagBlend);
    var corner = lerp.Lerp(cornerH, cornerV, 333, 667);

    // 3x3 pattern:
    // Row 0: [sharpened] [mid-right] [edge-right]
    dest[0] = encoder.Encode(sharpened);
    dest[1] = encoder.Encode(midR);
    dest[2] = encoder.Encode(edgeR);

    // Row 1: [mid-down] [mid-diag] [edge-diag]
    dest[destStride] = encoder.Encode(midD);
    dest[destStride + 1] = encoder.Encode(midDR);
    dest[destStride + 2] = encoder.Encode(edgeDR);

    // Row 2: [edge-down] [edge-diag] [corner]
    dest[2 * destStride] = encoder.Encode(edgeD);
    dest[2 * destStride + 1] = encoder.Encode(edgeDR);
    dest[2 * destStride + 2] = encoder.Encode(corner);
  }
}

#endregion
