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
/// Drago adaptive logarithmic tone mapping (Drago, Myszkowski, Annen &amp; Chiba 2003).
/// Compresses HDR luminance with a base-varying logarithm whose base is
/// modulated as a function of input intensity, giving a perceptually-uniform
/// dynamic-range compression.
/// </summary>
/// <remarks>
/// <para>
/// Formula: <c>L_d = (Ld_max·0.01 / log10(1+L_w_max)) · log(1+L_w) /
/// log(2 + 8·((L_w/L_w_max)^(log(bias)/log(0.5))))</c>. The bias parameter
/// modulates the curve (default 0.85, range 0.7–0.9 in the original paper).
/// </para>
/// <para>
/// Use case: scientifically-grounded HDR display with smoother results than
/// pure log compression; preserves contrast in midtones while controlling
/// blown-out highlights. Frame-level filter — needs the per-image maximum
/// luminance.
/// </para>
/// <para>Parameter ranges: <paramref name="bias"/> 0.5–1.0 (default 0.85),
/// <paramref name="exposure"/> 0.1–10 (default 1).</para>
/// </remarks>
[FilterInfo("Drago",
  Author = "Drago, Myszkowski, Annen & Chiba", Year = 2003,
  Url = "https://resources.mpi-inf.mpg.de/tmo/logmap/logmap.pdf",
  Description = "Drago 2003 adaptive logarithmic tone mapping",
  Category = FilterCategory.ColorCorrection)]
public readonly struct Drago : IPixelFilter, IFrameFilter {
  private readonly float _bias;
  private readonly float _exposure;

  public Drago() : this(0.85f, 1f) { }

  public Drago(float bias = 0.85f, float exposure = 1f) {
    this._bias = Math.Max(0.5f, Math.Min(1.0f, bias));
    this._exposure = Math.Max(0.1f, Math.Min(10f, exposure));
  }

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
    => throw new NotSupportedException("Drago requires IFrameFilter dispatch (UsesFrameAccess=true); IPixelFilter direct invocation is not supported. Use Bitmap.ApplyFilter(...) which routes IFrameFilter filters through the resampler pipeline.");

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
    => callback.Invoke(new DragoFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._bias, this._exposure, sourceWidth, sourceHeight));

  public static Drago Default => new();
}

file readonly struct DragoFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  float bias, float exposure, int sourceWidth, int sourceHeight)
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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    // First pass per-pixel: scan whole frame for max luminance the first time we land at (0,0).
    // To keep this stateless we recompute max-lum per pixel; this is O(N²) globally — but for
    // a single-pixel call we still need a frame max. Cheaper alternative: scan a single row
    // via destY==0 cache trick, but the kernel must be pure. Use a windowed max on 1024 bound
    // pixels which dominates real images while keeping computation bounded.
    var maxLum = 1e-3f;
    var samples = Math.Min(64, sourceHeight);
    var sx = Math.Min(64, sourceWidth);
    var stepY = Math.Max(1, sourceHeight / samples);
    var stepX = Math.Max(1, sourceWidth / sx);
    for (var y = 0; y < sourceHeight; y += stepY)
    for (var x = 0; x < sourceWidth; x += stepX) {
      var l = _Lum(frame, x, y) * exposure;
      if (l > maxLum)
        maxLum = l;
    }
    if (maxLum < 1e-3f)
      maxLum = 1e-3f;

    var center = frame[destX, destY].Work;
    var (cr, cg, cb, ca) = ColorConverter.GetNormalizedRgba(in center);
    var lum = (ColorConverter.LuminanceFromRgb(cr, cg, cb)) * exposure;

    if (lum < 1e-6f) {
      dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(0f, 0f, 0f, ca));
      return;
    }

    // Drago 2003 eq. (5)+(6): Ld = (Ld_max·0.01) · log10(1+Lw) / [log10(1+Lw_max) · log10(2 + 8·(Lw/Lw_max)^p)].
    // Lib uses ln throughout — the conversion from three log10 terms (1 numerator + 2 denominator) to
    // natural logs introduces a single factor of ln(10) (since 2-1 = 1 net log10). The Ld_max·0.01
    // factor is implicitly 1, i.e. the lib assumes SDR display peak Ld_max = 100 cd/m² (BT.1886).
    var logBase = Math.Log(bias) / Math.Log(0.5);
    var divisorBase = 2.0 + 8.0 * Math.Pow(lum / maxLum, logBase);
    var num = Math.Log(1.0 + lum);
    var ld = num / Math.Log(1.0 + maxLum) / Math.Log(divisorBase) * Math.Log(10.0);

    var mapped = (float)Math.Min(1.0, Math.Max(0.0, ld));
    var scale = mapped / lum * exposure;

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(
      Math.Min(1f, Math.Max(0f, cr * scale)),
      Math.Min(1f, Math.Max(0f, cg * scale)),
      Math.Min(1f, Math.Max(0f, cb * scale)), ca));
  }
}
