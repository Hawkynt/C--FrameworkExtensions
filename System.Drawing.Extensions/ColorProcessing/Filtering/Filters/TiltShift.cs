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
/// Tilt-shift miniature effect with selective focus band.
/// Applies position-dependent selective blur: a sharp horizontal band in the center
/// with progressively blurred areas above and below.
/// Always uses frame-level random access for position-dependent blur radius.
/// </summary>
[FilterInfo("TiltShift",
  Description = "Tilt-shift miniature effect with selective focus band", Category = FilterCategory.Artistic)]
public readonly struct TiltShift(float focusPosition, float focusWidth = 0.2f, int blurRadius = 3)
  : IPixelFilter, IFrameFilter {
  private readonly float _focusPosition = Math.Max(0f, Math.Min(1f, focusPosition));
  private readonly float _focusWidth = Math.Max(0f, Math.Min(1f, focusWidth));
  private readonly int _blurRadius = Math.Max(1, blurRadius);

  public TiltShift() : this(0.5f, 0.2f, 3) { }

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
    => callback.Invoke(new TiltShiftPassThroughKernel<TWork, TKey, TPixel, TEncode>());

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
    => callback.Invoke(new TiltShiftFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._focusPosition, this._focusWidth, this._blurRadius, sourceWidth, sourceHeight));

  public static TiltShift Default => new();
}

file readonly struct TiltShiftPassThroughKernel<TWork, TKey, TPixel, TEncode>
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

file readonly struct TiltShiftFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  float focusPosition, float focusWidth, int blurRadius, int sourceWidth, int sourceHeight)
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
  private static float _SmoothStep(float edge0, float edge1, float x) {
    if (edge1 <= edge0)
      return x >= edge0 ? 1f : 0f;

    var t = Math.Max(0f, Math.Min(1f, (x - edge0) / (edge1 - edge0)));
    return t * t * (3f - 2f * t);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    var normalizedY = sourceHeight > 1 ? (float)destY / (sourceHeight - 1) : 0.5f;
    var halfWidth = focusWidth * 0.5f;
    var bandTop = focusPosition - halfWidth;
    var bandBottom = focusPosition + halfWidth;
    var distFromBand = normalizedY < bandTop
      ? bandTop - normalizedY
      : normalizedY > bandBottom
        ? normalizedY - bandBottom
        : 0f;

    var blurStrength = _SmoothStep(0f, 1f - halfWidth, distFromBand);

    if (blurStrength < 0.001f) {
      dest[destY * destStride + destX] = encoder.Encode(frame[destX, destY].Work);
      return;
    }

    var effectiveRadius = (int)Math.Round(blurRadius * blurStrength);
    if (effectiveRadius < 1) {
      dest[destY * destStride + destX] = encoder.Encode(frame[destX, destY].Work);
      return;
    }

    var inv = 1f / ((2 * effectiveRadius + 1) * (2 * effectiveRadius + 1));
    float ar = 0, ag = 0, ab = 0;
    for (var dy = -effectiveRadius; dy <= effectiveRadius; ++dy)
    for (var dx = -effectiveRadius; dx <= effectiveRadius; ++dx) {
      var px = frame[destX + dx, destY + dy].Work;
      var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in px);
      ar += r;
      ag += g;
      ab += b;
    }

    ar *= inv;
    ag *= inv;
    ab *= inv;

    var center = frame[destX, destY].Work;
    var (_, _, _, ca) = ColorConverter.GetNormalizedRgba(in center);
    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(ar, ag, ab, ca));
  }
}
