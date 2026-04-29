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
/// Glasner-style self-similarity super-resolution resampler.
/// </summary>
/// <remarks>
/// <para>The image-is-its-own-training-set principle: small image patches recur within a
/// natural image at slightly different positions, so a search of the local source
/// neighbourhood usually turns up patches that have already been observed at higher
/// effective resolution. The per-output-pixel form here picks the source patch most similar
/// to the local 3×3 patch around the bilinear sample point and blends its centre value
/// into the bilinear baseline, weighted by the inverse of the matching cost.</para>
/// <para>This is a single-pass approximation of the multi-scale Glasner pipeline — adequate
/// for the per-pixel kernel scaffold, deterministic, and byte-exact reproducible. Patch
/// radius and search radius are both fixed (3×3 patches in a 5×5 search window) so each
/// output pixel costs ~25 patch comparisons of 9 sums each.</para>
/// <para>Reference: Glasner, Bagon &amp; Irani 2009, "Super-Resolution from a Single Image",
/// ICCV. Public-domain algorithm; the paper's code is BSD.</para>
/// </remarks>
[ScalerInfo("GlasnerSelfSimilarity", Author = "Glasner, Bagon & Irani", Year = 2009,
  Description = "Patch-based self-similarity super-resolution",
  Category = ScalerCategory.Resampler)]
public readonly struct GlasnerSelfSimilarity : IResampler {

  /// <summary>Default similarity strength ∈ [0, 1] (0.5 = balanced bilinear/patch).</summary>
  public const float DefaultStrength = 0.5f;

  private readonly float _strength;

  /// <summary>Creates a Glasner resampler with default strength.</summary>
  public GlasnerSelfSimilarity() : this(DefaultStrength) { }

  /// <summary>Creates a Glasner resampler with custom blend strength.</summary>
  /// <param name="strength">How much of the best-matching patch's centre to mix into the
  /// bilinear baseline ∈ [0, 1]. 0 = pure bilinear; 1 = full replacement at the strongest
  /// match.</param>
  public GlasnerSelfSimilarity(float strength) {
    ArgumentOutOfRangeException.ThrowIfNegative(strength);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(strength, 1f);
    this._strength = strength;
  }

  /// <summary>Gets the blend strength.</summary>
  public float Strength => this._strength == 0f ? DefaultStrength : this._strength;

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 4; // 3×3 patch + 5×5 search window → reach ±4 source pixels.

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
    => callback.Invoke(new GlasnerKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.Strength, useCenteredGrid));

  /// <summary>Gets the default configuration.</summary>
  public static GlasnerSelfSimilarity Default => new();

  /// <summary>Gets a stronger configuration (more patch influence).</summary>
  public static GlasnerSelfSimilarity Strong => new(0.8f);

  /// <summary>Gets a softer configuration (less patch influence).</summary>
  public static GlasnerSelfSimilarity Soft => new(0.25f);
}

file readonly struct GlasnerKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight,
  float strength, bool useCenteredGrid)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  private const int SearchRadius = 2; // 5×5 search window
  private const int PatchRadius = 1;  // 3×3 patch

  public int Radius => SearchRadius + PatchRadius;
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

    // Bilinear baseline.
    var c00 = frame[x0, y0].Work;
    var c10 = frame[x0 + 1, y0].Work;
    var c01 = frame[x0, y0 + 1].Work;
    var c11 = frame[x0 + 1, y0 + 1].Work;
    var bilinear = BilinearInterpolate(c00, c10, c01, c11, fx, fy);

    // Reference 3×3 patch (luminance only) centred on (x0, y0). Used as the query for the
    // self-similarity search. Luminance keeps the comparison cheap and chrominance-tolerant.
    var qNW = ColorConverter.GetLuminance(frame[x0 - 1, y0 - 1].Work);
    var qN  = ColorConverter.GetLuminance(frame[x0,     y0 - 1].Work);
    var qNE = ColorConverter.GetLuminance(frame[x0 + 1, y0 - 1].Work);
    var qW  = ColorConverter.GetLuminance(frame[x0 - 1, y0    ].Work);
    var qC  = ColorConverter.GetLuminance(frame[x0,     y0    ].Work);
    var qE  = ColorConverter.GetLuminance(frame[x0 + 1, y0    ].Work);
    var qSW = ColorConverter.GetLuminance(frame[x0 - 1, y0 + 1].Work);
    var qS  = ColorConverter.GetLuminance(frame[x0,     y0 + 1].Work);
    var qSE = ColorConverter.GetLuminance(frame[x0 + 1, y0 + 1].Work);

    // Search a 5×5 neighbourhood around (x0, y0) for the patch with the smallest sum-of-
    // absolute-differences against the query. Skip the centre patch (offset 0,0) — it's our
    // own and would always win.
    var bestCost = float.MaxValue;
    var bestX = x0;
    var bestY = y0;
    for (var dy = -SearchRadius; dy <= SearchRadius; ++dy)
    for (var dx = -SearchRadius; dx <= SearchRadius; ++dx) {
      if (dx == 0 && dy == 0) continue;
      var cx = x0 + dx;
      var cy = y0 + dy;

      var cost =
        MathF.Abs(qNW - ColorConverter.GetLuminance(frame[cx - 1, cy - 1].Work)) +
        MathF.Abs(qN  - ColorConverter.GetLuminance(frame[cx,     cy - 1].Work)) +
        MathF.Abs(qNE - ColorConverter.GetLuminance(frame[cx + 1, cy - 1].Work)) +
        MathF.Abs(qW  - ColorConverter.GetLuminance(frame[cx - 1, cy    ].Work)) +
        MathF.Abs(qC  - ColorConverter.GetLuminance(frame[cx,     cy    ].Work)) +
        MathF.Abs(qE  - ColorConverter.GetLuminance(frame[cx + 1, cy    ].Work)) +
        MathF.Abs(qSW - ColorConverter.GetLuminance(frame[cx - 1, cy + 1].Work)) +
        MathF.Abs(qS  - ColorConverter.GetLuminance(frame[cx,     cy + 1].Work)) +
        MathF.Abs(qSE - ColorConverter.GetLuminance(frame[cx + 1, cy + 1].Work));

      if (cost >= bestCost) continue;
      bestCost = cost;
      bestX = cx;
      bestY = cy;
    }

    // Sample the matched patch's centre and translate by the matched offset to get the
    // would-be high-resolution candidate at the bilinear sample point.
    var matched00 = frame[bestX, bestY].Work;
    var matched10 = frame[bestX + 1, bestY].Work;
    var matched01 = frame[bestX, bestY + 1].Work;
    var matched11 = frame[bestX + 1, bestY + 1].Work;
    var matchedSample = BilinearInterpolate(matched00, matched10, matched01, matched11, fx, fy);

    // Confidence: similarity-weighted blend strength. bestCost = 0 at perfect match; at the
    // typical noisy match the SAD of 9 luminance terms is ~1, so we scale by 1/(1 + 9·cost)
    // and multiply by the user-set strength.
    var confidence = 1f / (1f + 9f * bestCost);
    var w = confidence * strength;

    Accum4F<TWork> acc = default;
    acc.AddMul(bilinear, 1f - w);
    acc.AddMul(matchedSample, w);
    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
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
}
