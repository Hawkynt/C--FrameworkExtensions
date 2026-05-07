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
using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Constants;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Resizing;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Filtering.Filters;

/// <summary>
/// S-curve sigmoid contrast adjustment.
/// Applies a logistic sigmoid function per channel to remap intensities,
/// producing smooth contrast enhancement centered around a configurable midpoint.
/// </summary>
[FilterInfo("SigmoidContrast",
  Description = "S-curve sigmoid contrast adjustment", Category = FilterCategory.ColorCorrection)]
public readonly struct SigmoidContrast(float contrast = 5f, float midpoint = 0.5f) : IPixelFilter, IFrameFilter {
  private readonly float _contrast = contrast;
  private readonly float _midpoint = midpoint;

  /// <inheritdoc />
  public bool UsesFrameAccess => true;

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
    => callback.Invoke(new SigmoidContrastKernel<TWork, TKey, TPixel, TEncode>(this._contrast, this._midpoint));

  /// <inheritdoc />
  public TResult InvokeFrameKernel<TWork, TKey, TPixel, TDecode, TProject, TEncode, TResult>(
    IResampleKernelCallback<TWork, TKey, TPixel, TDecode, TProject, TEncode, TResult> callback,
    int sourceWidth, int sourceHeight)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new SigmoidContrastFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._contrast, this._midpoint, sourceWidth, sourceHeight));

  public static SigmoidContrast Default => new(5f, 0.5f);
}

// ImageMagick "sigmoidal-contrast" formula:
//   σ(x) = 1 / (1 + exp(c·(β − x)))
//   out  = (σ(x) − σ(0)) / (σ(1) − σ(0))
// The endpoint normalisation is required so x=0→0 and x=1→1; without it,
// black is lifted (~0.076 with c=5, β=0.5) and white is dimmed (~0.924).
// Reference: https://imagemagick.org/script/command-line-options.php#sigmoidal-contrast
//   and Hagen & Hartl 2011 (ImageMagick implementation).
file readonly struct SigmoidContrastKernel<TWork, TKey, TPixel, TEncode>(float contrast, float midpoint)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 1;
  public int ScaleY => 1;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var px = window.P0P0.Work;
    var (r, g, b, a) = ColorConverter.GetNormalizedRgba(in px);

    var sigma0 = 1f / (1f + (float)Math.Exp(contrast * midpoint));
    var sigma1 = 1f / (1f + (float)Math.Exp(contrast * (midpoint - 1f)));
    var range = sigma1 - sigma0;

    var outR = (1f / (1f + (float)Math.Exp(-contrast * (r - midpoint))) - sigma0) / range;
    var outG = (1f / (1f + (float)Math.Exp(-contrast * (g - midpoint))) - sigma0) / range;
    var outB = (1f / (1f + (float)Math.Exp(-contrast * (b - midpoint))) - sigma0) / range;

    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(outR, outG, outB, a));
  }
}

file readonly struct SigmoidContrastFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  float contrast, float midpoint, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => 1;
  public int SourceWidth => sourceWidth;
  public int SourceHeight => sourceHeight;
  public int TargetWidth => sourceWidth;
  public int TargetHeight => sourceHeight;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    var px = frame[destX, destY].Work;
    var (r, g, b, a) = ColorConverter.GetNormalizedRgba(in px);

    var sigma0 = 1f / (1f + (float)Math.Exp(contrast * midpoint));
    var sigma1 = 1f / (1f + (float)Math.Exp(contrast * (midpoint - 1f)));
    var range = sigma1 - sigma0;

    var outR = (1f / (1f + (float)Math.Exp(-contrast * (r - midpoint))) - sigma0) / range;
    var outG = (1f / (1f + (float)Math.Exp(-contrast * (g - midpoint))) - sigma0) / range;
    var outB = (1f / (1f + (float)Math.Exp(-contrast * (b - midpoint))) - sigma0) / range;

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(outR, outG, outB, a));
  }
}
