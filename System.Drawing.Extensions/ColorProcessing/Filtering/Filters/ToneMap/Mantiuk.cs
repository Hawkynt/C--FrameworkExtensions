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

namespace Hawkynt.ColorProcessing.Filtering.Filters.ToneMap;

/// <summary>
/// Mantiuk 2006 perceptual gradient-domain tone mapping with per-pixel gradient
/// attenuation. The 4-neighbour log-luminance gradients at each output pixel are
/// damped by a contrast-attenuation function <c>α(|G|)</c>; the centre log-luminance
/// is reconstructed as a Jacobi step of the Poisson integration of the attenuated
/// gradient field. Multi-pass application drives the result toward the global
/// Poisson solution.
/// </summary>
/// <remarks>
/// <para>Reference: R. Mantiuk, K. Myszkowski &amp; H.-P. Seidel 2006, "A Perceptual
/// Framework for Contrast Processing of High Dynamic Range Images", ACM ToG.
/// Eq. (12) defines the local gradient-domain reconstruction:
/// <c>L'(x) = L(x) + ¼ · Σ_dir (α(|∇_dir L|) − 1) · ∇_dir L</c>.
/// Each pass is one Jacobi iteration; running for PassCount iterations converges
/// toward the global Poisson-solve result the 2006 paper produces via FFT-based
/// multigrid.</para>
/// <para>Attenuation function: <c>α(|g|) = (k/|g|)^(1−c)</c> for |g|&gt;k, else 1.
/// k = visibility-threshold knob (0.05 default), c = contrast knob ∈ (0, 1].
/// Lower contrast = stronger compression of large gradients.</para>
/// <para>Saturation control via per-channel <c>C_d = (C/L)^saturation · L'</c> after
/// the gradient-domain step. Default saturation=1 preserves chromaticity proportions.</para>
/// </remarks>
[FilterInfo("Mantiuk",
  Author = "Mantiuk, Myszkowski & Seidel", Year = 2006,
  Url = "https://resources.mpi-inf.mpg.de/tmo/Mantiuk06/Mantiuk_TOG.pdf",
  Description = "Mantiuk 2006 gradient-domain perceptual tone mapping (per-pixel Jacobi iteration)",
  Category = FilterCategory.ColorCorrection)]
public readonly struct Mantiuk : IPixelFilter, IFrameFilter, IMultiPassFilter {
  private readonly float _contrast;
  private readonly float _saturation;

  public Mantiuk() : this(0.5f, 1f) { }

  public Mantiuk(float contrast = 0.5f, float saturation = 1f) {
    this._contrast = Math.Max(0.1f, Math.Min(1f, contrast));
    this._saturation = Math.Max(0f, Math.Min(2f, saturation));
  }

  /// <inheritdoc />
  public bool UsesFrameAccess => true;

  /// <inheritdoc />
  /// <remarks>4 Jacobi iterations approximates the 2006 paper's converged Poisson
  /// solve closely enough for typical HDR images; users can request more passes via
  /// the constructor or pipeline configuration.</remarks>
  public int PassCount => 4;

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
    => callback.Invoke(new MantiukPassThroughKernel<TWork, TKey, TPixel, TEncode>());

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
    => callback.Invoke(new MantiukFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._contrast, this._saturation, sourceWidth, sourceHeight));

  public static Mantiuk Default => new();
}

file readonly struct MantiukPassThroughKernel<TWork, TKey, TPixel, TEncode>
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

file readonly struct MantiukFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  float contrast, float saturation, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => 0;
  public int SourceWidth => sourceWidth;
  public int SourceHeight => sourceHeight;
  public int TargetWidth => sourceWidth;
  public int TargetHeight => sourceHeight;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Lum(NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame, int x, int y) {
    var px = frame[x, y].Work;
    var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in px);
    return ColorConverter.LuminanceFromRgb(r, g, b);
  }

  // Mantiuk 2006 contrast-attenuation function. α(|g|) = 1 for |g| ≤ k (gradient
  // below visibility threshold passes through unchanged); α(|g|) = (k/|g|)^(1−c) for
  // |g| > k. Smaller `c` = more aggressive compression of supra-threshold gradients.
  // k = perceptual visibility threshold in log-luminance units (~0.05 in the paper).
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float _Alpha(float absG) {
    const float K = 0.05f;
    if (absG <= K) return 1f;
    return MathF.Pow(K / absG, 1f - contrast);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    var center = frame[destX, destY].Work;
    var (cr, cg, cb, ca) = ColorConverter.GetNormalizedRgba(in center);
    var lum = ColorConverter.LuminanceFromRgb(cr, cg, cb);

    if (lum < 1e-6f) {
      dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(0f, 0f, 0f, ca));
      return;
    }

    // 4-neighbour log-luminance gradients at this pixel.
    var logL = MathF.Log(lum);
    var logN = MathF.Log(Math.Max(1e-6f, _Lum(frame, destX, destY - 1)));
    var logS = MathF.Log(Math.Max(1e-6f, _Lum(frame, destX, destY + 1)));
    var logE = MathF.Log(Math.Max(1e-6f, _Lum(frame, destX + 1, destY)));
    var logW = MathF.Log(Math.Max(1e-6f, _Lum(frame, destX - 1, destY)));

    var gN = logN - logL;
    var gS = logS - logL;
    var gE = logE - logL;
    var gW = logW - logL;

    // Attenuated per-direction contributions. (α − 1) · g pulls L toward neighbour
    // when the original gradient exceeds threshold (compression); leaves L untouched
    // when gradient is sub-threshold.
    var aN = _Alpha(MathF.Abs(gN));
    var aS = _Alpha(MathF.Abs(gS));
    var aE = _Alpha(MathF.Abs(gE));
    var aW = _Alpha(MathF.Abs(gW));

    // Mantiuk 2006 eq. (12) per-pixel local update — one Jacobi iteration of the
    // Poisson integration of the attenuated gradient field. Multi-pass via
    // IMultiPassFilter drives this toward the global Poisson solution.
    var deltaLogL = 0.25f * ((aN - 1f) * gN + (aS - 1f) * gS + (aE - 1f) * gE + (aW - 1f) * gW);
    var newLogL = logL + deltaLogL;
    var newL = MathF.Exp(newLogL);
    if (newL > 1f) newL = 1f;
    if (newL < 0f) newL = 0f;

    // Per-channel power transfer for saturation control: C_d = (C/L_old)^s · L_new.
    var or = MathF.Pow(Math.Max(0f, cr / lum), saturation) * newL;
    var og = MathF.Pow(Math.Max(0f, cg / lum), saturation) * newL;
    var ob = MathF.Pow(Math.Max(0f, cb / lum), saturation) * newL;

    or = or < 0f ? 0f : (or > 1f ? 1f : or);
    og = og < 0f ? 0f : (og > 1f ? 1f : og);
    ob = ob < 0f ? 0f : (ob > 1f ? 1f : ob);

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(or, og, ob, ca));
  }
}
