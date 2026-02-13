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
/// Sponge texture with high-contrast color bins.
/// Like oil painting but with fewer luminance bins and additional contrast enhancement.
/// Always uses frame-level random access.
/// </summary>
[FilterInfo("Sponge",
  Description = "Sponge texture with high-contrast color bins", Category = FilterCategory.Artistic)]
public readonly struct Sponge(int brushSize = 2, float definition = 5f, float smoothness = 5f) : IPixelFilter, IFrameFilter {
  private readonly int _brushSize = Math.Max(1, brushSize);
  private readonly float _definition = definition;
  private readonly float _smoothness = smoothness;

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
    => callback.Invoke(new SpongePassThroughKernel<TWork, TKey, TPixel, TEncode>());

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
    => callback.Invoke(new SpongeFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._brushSize, this._definition, this._smoothness, sourceWidth, sourceHeight));

  public static Sponge Default => new(2, 5f, 5f);
}

file readonly struct SpongePassThroughKernel<TWork, TKey, TPixel, TEncode>
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
    in TEncode encoder)
    => dest[0] = encoder.Encode(window.P0P0.Work);
}

file readonly struct SpongeFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int brushSize, float definition, float smoothness, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => Math.Max(1, brushSize);
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
    var bins = Math.Max(2, (int)definition);
    var binCount = new int[bins];
    var binR = new float[bins];
    var binG = new float[bins];
    var binB = new float[bins];

    for (var dy = -brushSize; dy <= brushSize; ++dy)
    for (var dx = -brushSize; dx <= brushSize; ++dx) {
      var px = frame[destX + dx, destY + dy].Work;
      var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in px);
      var lum = ColorMatrices.BT601_R * r + ColorMatrices.BT601_G * g + ColorMatrices.BT601_B * b;
      var bin = (int)(lum * (bins - 1));
      if (bin < 0)
        bin = 0;
      else if (bin >= bins)
        bin = bins - 1;

      ++binCount[bin];
      binR[bin] += r;
      binG[bin] += g;
      binB[bin] += b;
    }

    var maxBin = 0;
    var maxCount = binCount[0];
    for (var i = 1; i < bins; ++i) {
      if (binCount[i] <= maxCount)
        continue;

      maxCount = binCount[i];
      maxBin = i;
    }

    var inv = 1f / maxCount;
    var outR = binR[maxBin] * inv;
    var outG = binG[maxBin] * inv;
    var outB = binB[maxBin] * inv;

    outR = 0.5f + (outR - 0.5f) * (1f + smoothness * 0.2f);
    outG = 0.5f + (outG - 0.5f) * (1f + smoothness * 0.2f);
    outB = 0.5f + (outB - 0.5f) * (1f + smoothness * 0.2f);

    outR = Math.Max(0f, Math.Min(1f, outR));
    outG = Math.Max(0f, Math.Min(1f, outG));
    outB = Math.Max(0f, Math.Min(1f, outB));

    var center = frame[destX, destY].Work;
    var (_, _, _, a) = ColorConverter.GetNormalizedRgba(in center);
    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(outR, outG, outB, a));
  }
}
