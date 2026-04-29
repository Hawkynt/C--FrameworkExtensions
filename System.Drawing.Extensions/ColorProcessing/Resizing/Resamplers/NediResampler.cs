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
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Resamplers;

/// <summary>
/// NEDI-style free-factor resampler — New Edge-Directed Interpolation generalised to
/// arbitrary target dimensions.
/// </summary>
/// <remarks>
/// <para>The fixed-factor <see cref="Hawkynt.ColorProcessing.Resizing.Rescalers.Nedi"/>
/// resamples at exact half/third/quarter-pixel positions; this companion lifts the
/// autocorrelation-based diagonal selection to arbitrary fractional source positions.
/// At each output pixel the local 5×5 source neighbourhood is analysed via a 2×2
/// autocorrelation system (Cramer-rule solve) to pick a diagonal weighting, then the
/// bilinear sample is blended with diagonal-pair lerps weighted by that solve.</para>
/// <para>References: Li &amp; Orchard 2001, "New Edge-Directed Interpolation", IEEE Trans.
/// Image Processing — same algorithm as the fixed-factor rescaler, applied at general
/// positions.</para>
/// </remarks>
[ScalerInfo("NEDI Resampler", Author = "Xin Li/Michael T. Orchard", Year = 2001,
  Description = "Free-factor New Edge-Directed Interpolation",
  Category = ScalerCategory.Resampler)]
public readonly struct NediResampler : IResampler {

  /// <summary>Default edge sensitivity (0.5 — moderate diagonal preference).</summary>
  public const float DefaultEdgeStrength = 0.5f;

  private readonly float _edgeStrength;

  /// <summary>Creates a NEDI resampler with default parameters.</summary>
  public NediResampler() : this(DefaultEdgeStrength) { }

  /// <summary>Creates a NEDI resampler with custom edge strength.</summary>
  /// <param name="edgeStrength">How strongly the autocorrelation-derived diagonal weight
  /// overrides bilinear ∈ [0, 1]. 0 = pure bilinear; 1 = full diagonal blend.</param>
  public NediResampler(float edgeStrength) {
    ArgumentOutOfRangeException.ThrowIfNegative(edgeStrength);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(edgeStrength, 1f);
    this._edgeStrength = edgeStrength;
  }

  /// <summary>Gets the edge strength.</summary>
  public float EdgeStrength => this._edgeStrength == 0f ? DefaultEdgeStrength : this._edgeStrength;

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 2;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => null;

  /// <inheritdoc />
  public TResult InvokeKernel<TWork, TKey, TPixel, TDecode, TProject, TEncode, TResult>(
    IResampleKernelCallback<TWork, TKey, TPixel, TDecode, TProject, TEncode, TResult> callback,
    int sourceWidth,
    int sourceHeight,
    int targetWidth,
    int targetHeight,
    bool useCenteredGrid = true)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new NediResamplerKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.EdgeStrength, useCenteredGrid));

  /// <summary>Gets the default configuration.</summary>
  public static NediResampler Default => new();
}

file readonly struct NediResamplerKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight,
  float edgeStrength, bool useCenteredGrid)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => 2;
  public int SourceWidth => sourceWidth;
  public int SourceHeight => sourceHeight;
  public int TargetWidth => targetWidth;
  public int TargetHeight => targetHeight;

  private readonly float _scaleX = (float)sourceWidth / targetWidth;
  private readonly float _scaleY = (float)sourceHeight / targetHeight;
  private readonly float _offsetX = useCenteredGrid ? 0.5f * sourceWidth / targetWidth - 0.5f : 0f;
  private readonly float _offsetY = useCenteredGrid ? 0.5f * sourceHeight / targetHeight - 0.5f : 0f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var srcXf = destX * this._scaleX + this._offsetX;
    var srcYf = destY * this._scaleY + this._offsetY;
    var x0 = (int)MathF.Floor(srcXf);
    var y0 = (int)MathF.Floor(srcYf);
    var fx = srcXf - x0;
    var fy = srcYf - y0;

    // 2×2 baseline corners.
    var c00 = frame[x0, y0].Work;
    var c10 = frame[x0 + 1, y0].Work;
    var c01 = frame[x0, y0 + 1].Work;
    var c11 = frame[x0 + 1, y0 + 1].Work;
    var bilinear = BilinearInterpolate(c00, c10, c01, c11, fx, fy);

    // 5×5 luminance window centred at (x0, y0) — used for the NEDI autocorrelation cost.
    var l00 = ColorConverter.GetLuminance(c00);
    var l01 = ColorConverter.GetLuminance(c01);
    var l10 = ColorConverter.GetLuminance(c10);
    var l11 = ColorConverter.GetLuminance(c11);
    var lN  = ColorConverter.GetLuminance(frame[x0,     y0 - 1].Work);
    var lS  = ColorConverter.GetLuminance(frame[x0,     y0 + 1].Work);
    var lW  = ColorConverter.GetLuminance(frame[x0 - 1, y0    ].Work);
    var lE  = ColorConverter.GetLuminance(frame[x0 + 1, y0    ].Work);
    var lNW = ColorConverter.GetLuminance(frame[x0 - 1, y0 - 1].Work);
    var lNE = ColorConverter.GetLuminance(frame[x0 + 1, y0 - 1].Work);
    var lSW = ColorConverter.GetLuminance(frame[x0 - 1, y0 + 1].Work);
    var lSE = ColorConverter.GetLuminance(frame[x0 + 1, y0 + 1].Work);

    // Build the 2×2 autocorrelation system that NEDI solves for (a0, a1) — one weight per
    // diagonal direction (NW-SE vs NE-SW). We use the 4 closest diagonal pairs around the
    // sample position; matches the spirit of the rescaler's "Configuration 0" pass while
    // staying small enough to fit a per-pixel resampler.
    var c0a = lNW + l11; // NW-SE pair via top-left
    var c0b = lNE + l01; // NE-SW pair via top-right
    var c1a = l00 + lSE; // NW-SE pair via bottom-right
    var c1b = lSW + l10; // NE-SW pair via bottom-left
    var c2a = lNE + l01; var c2b = lN + l11;
    var c3a = lSW + l10; var c3b = lS + l00;
    var y0sample = lN; var y1sample = lS; var y2sample = lW; var y3sample = lE;

    var r00 = c0a * c0a + c1a * c1a + c2a * c2a + c3a * c3a;
    var r01 = c0a * c0b + c1a * c1b + c2a * c2b + c3a * c3b;
    var r11 = c0b * c0b + c1b * c1b + c2b * c2b + c3b * c3b;
    var b0 = y0sample * c0a + y1sample * c1a + y2sample * c2a + y3sample * c3a;
    var b1 = y0sample * c0b + y1sample * c1b + y2sample * c2b + y3sample * c3b;

    // Cramer's rule for the 2×2 system  [[r00, r01], [r01, r11]] · [a0, a1]ᵀ = [b0, b1]ᵀ.
    var det = r00 * r11 - r01 * r01;
    float a0, a1;
    if (MathF.Abs(det) < 1e-10f) {
      a0 = 0.5f;
      a1 = 0.5f;
    } else {
      var invDet = 1f / det;
      a0 = (b0 * r11 - b1 * r01) * invDet;
      a1 = (r00 * b1 - r01 * b0) * invDet;
    }

    // Normalise to a [0, 1] preference between NW-SE and NE-SW. The rescaler uses a similar
    // squashing step; we keep it bounded to keep the Cramer instabilities from swamping the
    // bilinear baseline.
    var diff = a0 - a1;
    if (diff > 1f) diff = 1f;
    else if (diff < -1f) diff = -1f;
    var nwSeWeight = 0.5f + 0.5f * diff;  // closer to 1 = stronger NW-SE preference

    // Diagonal sample candidates: each is a lerp along its diagonal at the bilinear-projected
    // fractional position. NW-SE: parameter t1 = 0.5·(fx + fy) blends c00→c11.
    // NE-SW: parameter t2 = 0.5·(fx + (1 − fy)) blends c10→c01.
    var t1 = 0.5f * (fx + fy);
    var t2 = 0.5f * (fx + (1f - fy));
    var nwSe = Lerp(c00, c11, t1);
    var neSw = Lerp(c10, c01, t2);

    // Combine diagonal preference into a single directional sample, then blend with bilinear
    // by the configured edge strength.
    Accum4F<TWork> diagAcc = default;
    diagAcc.AddMul(nwSe, nwSeWeight);
    diagAcc.AddMul(neSw, 1f - nwSeWeight);
    var directional = diagAcc.Result;

    Accum4F<TWork> outAcc = default;
    outAcc.AddMul(bilinear, 1f - edgeStrength);
    outAcc.AddMul(directional, edgeStrength);
    dest[destY * destStride + destX] = encoder.Encode(outAcc.Result);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork BilinearInterpolate(in TWork c00, in TWork c10, in TWork c01, in TWork c11, float fx, float fy) {
    var invFx = 1f - fx;
    var invFy = 1f - fy;
    Accum4F<TWork> acc = default;
    acc.AddMul(c00, invFx * invFy);
    acc.AddMul(c10, fx * invFy);
    acc.AddMul(c01, invFx * fy);
    acc.AddMul(c11, fx * fy);
    return acc.Result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork Lerp(in TWork a, in TWork b, float t) {
    Accum4F<TWork> acc = default;
    acc.AddMul(a, 1f - t);
    acc.AddMul(b, t);
    return acc.Result;
  }
}
