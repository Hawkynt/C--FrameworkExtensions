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
/// Lens distortion — Brown-Conrady barrel / pincushion radial-distortion model.
/// </summary>
/// <remarks>
/// <para>Forward radial distortion model used by photogrammetry / camera-calibration
/// pipelines (OpenCV, Hugin, Lensfun):</para>
/// <code>
///   r_distorted = r · (1 + k1·r² + k2·r⁴ + ...)
/// </code>
/// <para>k1 &gt; 0 produces pincushion distortion (telephoto-like edges pulled out);
/// k1 &lt; 0 produces barrel distortion (wide-angle edges bowed out). Reference:
/// D. C. Brown, "Decentering distortion of lenses", Photogrammetric Engineering
/// 32(3):444-462, 1966; A. E. Conrady, "Decentred lens-systems", Monthly Notices
/// of the Royal Astronomical Society 79(5):384-390, 1919.</para>
/// </remarks>
[FilterInfo("LensDistortion",
  Description = "Brown-Conrady barrel/pincushion lens distortion model", Category = FilterCategory.Distortion)]
public readonly struct LensDistortion(float k1, float k2 = 0f) : IPixelFilter, IFrameFilter {
  private readonly float _k1 = k1;
  private readonly float _k2 = k2;

  public LensDistortion() : this(0.3f, 0f) { }

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
    => throw new NotSupportedException("LensDistortion requires IFrameFilter dispatch (UsesFrameAccess=true); IPixelFilter direct invocation is not supported. Use Bitmap.ApplyFilter(...) which routes IFrameFilter filters through the resampler pipeline.");

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
    => callback.Invoke(new LensDistortionFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._k1, this._k2, sourceWidth, sourceHeight));

  public static LensDistortion Default => new();
}

file readonly struct LensDistortionFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  float k1, float k2, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => (int)(Math.Abs(k1) * Math.Max(sourceWidth, sourceHeight) / 4) + 1;
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
    var cx = sourceWidth * 0.5f;
    var cy = sourceHeight * 0.5f;
    var nx = (destX - cx) / cx;
    var ny = (destY - cy) / cy;
    var r2 = nx * nx + ny * ny;
    var r4 = r2 * r2;
    var scale = 1f + k1 * r2 + k2 * r4;

    var sx = (int)Math.Floor(cx + nx * scale * cx);
    var sy = (int)Math.Floor(cy + ny * scale * cy);

    var px = frame[sx, sy].Work;
    var (r, g, b, a) = ColorConverter.GetNormalizedRgba(in px);
    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(r, g, b, a));
  }
}
