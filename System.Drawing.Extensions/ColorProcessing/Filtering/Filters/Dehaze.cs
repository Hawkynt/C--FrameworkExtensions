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
/// He et al. 2009 "Single Image Haze Removal Using Dark Channel Prior" (CVPR), with
/// the simplifying assumption A = (1, 1, 1) for atmospheric light.
/// </summary>
/// <remarks>
/// <para>Reference: K. He, J. Sun &amp; X. Tang, "Single Image Haze Removal Using Dark
/// Channel Prior", CVPR 2009.</para>
/// <para>Implements the canonical formula
/// <c>t(x) = 1 − ω · min_{y∈Ω(x)} min_c (I_c(y) / A_c)</c> and
/// <c>J(x) = (I(x) − A) / max(t(x), t0) + A</c> with ω = strength·0.95 and t0 = 0.1
/// (paper §4.5 defaults).</para>
/// <para><b>Approximation:</b> the canonical algorithm computes A globally as the average of
/// the brightest 0.1% dark-channel pixels — that requires frame-level pre-pass state the
/// per-pixel kernel cannot maintain. This filter assumes A = (1, 1, 1) (clear-sky white).
/// For typical outdoor haze scenes this is accurate; for images with strongly-tinted
/// haze (e.g. sunset, sandstorm) the colour cast is not removed. No soft-matting / guided-
/// filter transmission refinement is applied (paper's optional refinement step).</para>
/// </remarks>
[FilterInfo("Dehaze",
  Author = "He, Sun & Tang", Year = 2009,
  Url = "https://kaiminghe.github.io/publications/cvpr09.pdf",
  Description = "He 2009 dark-channel haze removal (with A = white assumption)",
  Category = FilterCategory.Enhancement)]
public readonly struct Dehaze(float strength, int radius = 7) : IPixelFilter, IFrameFilter {
  private readonly float _strength = ColorConverter.Saturate(strength);
  private readonly int _radius = Math.Max(1, radius);

  public Dehaze() : this(0.5f, 7) { }

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
    => throw new NotSupportedException("Dehaze requires IFrameFilter dispatch (UsesFrameAccess=true); IPixelFilter direct invocation is not supported. Use Bitmap.ApplyFilter(...) which routes IFrameFilter filters through the resampler pipeline.");

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
    => callback.Invoke(new DehazeFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._strength, this._radius, sourceWidth, sourceHeight));

  public static Dehaze Default => new();
}

file readonly struct DehazeFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  float strength, int radius, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => Math.Max(1, radius);
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
    // He, Sun & Tang 2009 "Single Image Haze Removal Using Dark Channel Prior" (CVPR):
    //   t(x) = 1 - ω · min_{y∈Ω(x)} min_c (I_c(y) / A_c)
    //   J(x) = (I(x) - A) / max(t(x), t0) + A
    // The canonical algorithm needs global atmospheric light A from the brightest 0.1%
    // dark-channel pixels; that requires a frame-level precompute step the per-pixel
    // kernel cannot support without external state. As a defensible approximation we
    // assume A = (1, 1, 1) (clear-sky white) — accurate for most outdoor scenes; users
    // wanting per-image A should run a separate atmospheric-light estimator.
    // ω = strength·ω_max controls the haze-removal aggressiveness; t0 = 0.1 floors the
    // transmission to avoid noise amplification (paper §4.5).
    var center = frame[destX, destY].Work;
    var (cr, cg, cb, ca) = ColorConverter.GetNormalizedRgba(in center);

    // Local dark channel min over Ω: min_{y∈Ω(x)} min_c I_c(y).
    // With A = (1, 1, 1) the ratio I_c/A_c reduces to I_c, so this also serves as
    // min_c (I_c / A_c) needed for the transmission.
    var darkMin = 1f;
    for (var dy = -radius; dy <= radius; ++dy)
    for (var dx = -radius; dx <= radius; ++dx) {
      var px = frame[destX + dx, destY + dy].Work;
      var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in px);
      var localMin = Math.Min(r, Math.Min(g, b));
      if (localMin < darkMin)
        darkMin = localMin;
    }

    const float omegaMax = 0.95f; // He 2009 §4.5 default ω
    const float t0 = 0.1f;        // transmission floor
    var omega = strength * omegaMax;
    var t = 1f - omega * darkMin;
    if (t < t0) t = t0;

    // J_c = (I_c - A_c) / t + A_c, with A_c = 1.
    var or = ColorConverter.Saturate((cr - 1f) / t + 1f);
    var og = ColorConverter.Saturate((cg - 1f) / t + 1f);
    var ob = ColorConverter.Saturate((cb - 1f) / t + 1f);

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(or, og, ob, ca));
  }
}
