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
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Resizing;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Filtering.Filters;

/// <summary>
/// Surface blur — Photoshop-style edge-preserving smoothing via colour-distance threshold.
/// </summary>
/// <remarks>
/// <para>For each pixel, averages only neighbours whose colour distance from the centre
/// is below a hard threshold; neighbours beyond the threshold are excluded. A
/// hard-threshold simplification of the bilateral filter (which uses a soft
/// Gaussian range weight): faster and easier to tune, but with ringing artefacts
/// near sharp edges where the threshold cuts in/out.</para>
/// <para>Reference: Adobe Photoshop "Filter → Blur → Surface Blur" tool documentation.
/// For the soft-weighted alternative see <see cref="BilateralFilter"/>; for
/// patch-similarity-based smoothing see <see cref="NonLocalMeans"/>.</para>
/// </remarks>
[FilterInfo("SurfaceBlur",
  Description = "Edge-preserving surface blur using color distance threshold", Category = FilterCategory.Enhancement)]
public readonly struct SurfaceBlur(int radius, float threshold) : IPixelFilter, IFrameFilter {
  private readonly int _radius = Math.Max(1, radius);
  private readonly float _threshold = Math.Max(0f, threshold);

  public SurfaceBlur() : this(3, 0.1f) { }

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
    => throw new NotSupportedException("SurfaceBlur requires IFrameFilter dispatch (UsesFrameAccess=true); IPixelFilter direct invocation is not supported. Use Bitmap.ApplyFilter(...) which routes IFrameFilter filters through the resampler pipeline.");

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
    => callback.Invoke(new SurfaceBlurFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._radius, this._threshold, sourceWidth, sourceHeight));

  public static SurfaceBlur Default => new();
}

file readonly struct SurfaceBlurFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int radius, float threshold, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  private readonly float _threshold2 = threshold * threshold;

  public int Radius => radius;
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
    var center = frame[destX, destY].Work;
    var (cr, cg, cb, ca) = ColorConverter.GetNormalizedRgba(in center);

    float ar = 0, ag = 0, ab = 0;
    var count = 0;

    for (var dy = -radius; dy <= radius; ++dy)
    for (var dx = -radius; dx <= radius; ++dx) {
      var px = frame[destX + dx, destY + dy].Work;
      var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in px);

      var dr = r - cr;
      var dg = g - cg;
      var db = b - cb;
      var dist2 = dr * dr + dg * dg + db * db;

      if (dist2 >= _threshold2)
        continue;

      ar += r;
      ag += g;
      ab += b;
      ++count;
    }

    if (count > 0) {
      var inv = 1f / count;
      ar *= inv;
      ag *= inv;
      ab *= inv;
    } else {
      ar = cr;
      ag = cg;
      ab = cb;
    }

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(ar, ag, ab, ca));
  }
}
