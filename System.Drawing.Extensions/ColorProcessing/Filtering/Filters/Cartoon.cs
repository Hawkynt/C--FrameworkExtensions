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
/// Cartoon effect combining color quantization with edge darkening.
/// Quantizes each channel to a configurable number of levels and darkens
/// pixels where Sobel edge magnitude exceeds the threshold.
/// Always uses frame-level random access due to configurable blur radius.
/// </summary>
[FilterInfo("Cartoon",
  Description = "Cartoon effect with quantized colors and edge darkening", Category = FilterCategory.Artistic)]
public readonly struct Cartoon(int levels, float edgeThreshold, int blurRadius = 1) : IPixelFilter, IFrameFilter {
  private readonly int _levels = Math.Max(2, levels);
  private readonly float _edgeThreshold = ColorConverter.Saturate(edgeThreshold);
  private readonly int _blurRadius = Math.Max(1, blurRadius);

  public Cartoon() : this(6, 0.1f, 1) { }

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
    => throw new NotSupportedException("Cartoon requires IFrameFilter dispatch (UsesFrameAccess=true); IPixelFilter direct invocation is not supported. Use Bitmap.ApplyFilter(...) which routes IFrameFilter filters through the resampler pipeline.");

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
    => callback.Invoke(new CartoonFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._levels, this._edgeThreshold, this._blurRadius, sourceWidth, sourceHeight));

  public static Cartoon Default => new();
}

file readonly struct CartoonFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int levels, float edgeThreshold, int blurRadius, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => Math.Max(1, blurRadius);
  public int SourceWidth => sourceWidth;
  public int SourceHeight => sourceHeight;
  public int TargetWidth => sourceWidth;
  public int TargetHeight => sourceHeight;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Lum(in NeighborPixel<TWork, TKey> p) {
    var px = p.Work;
    var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in px);
    return ColorConverter.LuminanceFromRgb(r, g, b);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    var center = frame[destX, destY].Work;
    var (cr, cg, cb, ca) = ColorConverter.GetNormalizedRgba(in center);

    // Quantize each channel to the configured number of levels using a
    // round-to-nearest scheme (Photoshop posterize). This matches the
    // behaviour of the Posterize filter and avoids the off-by-one
    // overflow at c=1.0 that the previous floor(c*levels)/(levels-1) had.
    var div = levels - 1f;
    var qr = (float)Math.Floor(cr * div + 0.5f) / div;
    var qg = (float)Math.Floor(cg * div + 0.5f) / div;
    var qb = (float)Math.Floor(cb * div + 0.5f) / div;
    qr = ColorConverter.Saturate(qr);
    qg = ColorConverter.Saturate(qg);
    qb = ColorConverter.Saturate(qb);

    // Sobel edge detection
    var tl = _Lum(frame[destX - 1, destY - 1]);
    var t = _Lum(frame[destX, destY - 1]);
    var tr = _Lum(frame[destX + 1, destY - 1]);
    var l = _Lum(frame[destX - 1, destY]);
    var r = _Lum(frame[destX + 1, destY]);
    var bl = _Lum(frame[destX - 1, destY + 1]);
    var b = _Lum(frame[destX, destY + 1]);
    var br = _Lum(frame[destX + 1, destY + 1]);

    var gx = -tl + tr - 2f * l + 2f * r - bl + br;
    var gy = -tl - 2f * t - tr + bl + 2f * b + br;
    // Sobel kernel sum = 8 (4 per axis); normalise to [0,1] like SobelEdge does.
    var edgeMag = Math.Min(1f, (float)Math.Sqrt(gx * gx + gy * gy) / 8f);

    // Darken at edges exceeding the threshold
    if (edgeMag > edgeThreshold) {
      qr = 0f;
      qg = 0f;
      qb = 0f;
    }

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(qr, qg, qb, ca));
  }
}
