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
/// Median-based non-linear resampler — edge-preserving via order-statistic selection.
/// </summary>
/// <remarks>
/// <para>For each output pixel, ranks the 25 source pixels in a 5×5 neighbourhood by
/// luminance and selects the pixel whose luminance sits at the median rank. Blends the
/// selected pixel into the bilinear baseline by the configured strength.</para>
/// <para>Different edge-preservation mechanism than the existing bilateral, total-variation,
/// and Gaussian-style resamplers in this namespace: median-rank selection rejects salt-and-
/// pepper outliers and impulse noise that bilateral/Gaussian smooth over, and preserves
/// step edges (which always include both sides of the step among their 25 neighbours, so
/// the median sits cleanly on one side rather than averaging across).</para>
/// <para>Reference: Tukey 1977, "Exploratory Data Analysis" (median-filter foundations);
/// applied to images by Pratt 1978, "Digital Image Processing". Public domain.</para>
/// </remarks>
[ScalerInfo("Median", Author = "Tukey / Pratt", Year = 1978,
  Description = "Non-linear median-rank resampler",
  Category = ScalerCategory.Resampler)]
public readonly struct MedianResampler : IResampler {

  /// <summary>Default median-blend strength (0.6 — moderate non-linear influence).</summary>
  public const float DefaultStrength = 0.6f;

  private readonly float _strength;

  /// <summary>Creates a Median resampler with default strength.</summary>
  public MedianResampler() : this(DefaultStrength) { }

  /// <summary>Creates a Median resampler with custom blend strength.</summary>
  /// <param name="strength">Blend ratio between bilinear baseline and median pick ∈ [0, 1].
  /// 0 = pure bilinear; 1 = pure median pick.</param>
  public MedianResampler(float strength) {
    ArgumentOutOfRangeException.ThrowIfNegative(strength);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(strength, 1f);
    this._strength = strength;
  }

  /// <summary>Gets the blend strength.</summary>
  public float Strength => this._strength == 0f ? DefaultStrength : this._strength;

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
    => callback.Invoke(new MedianKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.Strength, useCenteredGrid));

  /// <summary>Gets the default configuration.</summary>
  public static MedianResampler Default => new();

  /// <summary>Pure-median configuration (no bilinear blend).</summary>
  public static MedianResampler Pure => new(1f);

  /// <summary>Mild configuration (strong bilinear, light median).</summary>
  public static MedianResampler Mild => new(0.3f);
}

file readonly struct MedianKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight,
  float strength, bool useCenteredGrid)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  private const int WindowRadius = 2; // 5×5 = 25 candidates
  private const int SampleCount = 25;
  private const int MedianRank = 12;  // 0-indexed (25/2)

  public int Radius => WindowRadius;
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

    // Sample 25 source pixels (5×5 neighbourhood centred on (x0, y0)). Track luminance and
    // a parallel ordinal for cheap "which pixel was at this rank?" lookup after sorting.
    var lums = stackalloc float[SampleCount];
    var idxs = stackalloc int[SampleCount];
    var n = 0;
    for (var dy = -WindowRadius; dy <= WindowRadius; ++dy)
    for (var dx = -WindowRadius; dx <= WindowRadius; ++dx) {
      lums[n] = ColorConverter.GetLuminance(frame[x0 + dx, y0 + dy].Work);
      idxs[n] = (dy + WindowRadius) * (2 * WindowRadius + 1) + (dx + WindowRadius);
      ++n;
    }

    // Insertion sort by luminance — fine for n=25 and keeps the indices in lock-step. Median
    // sits at index 12 after sorting.
    for (var i = 1; i < SampleCount; ++i) {
      var keyLum = lums[i];
      var keyIdx = idxs[i];
      var j = i - 1;
      while (j >= 0 && lums[j] > keyLum) {
        lums[j + 1] = lums[j];
        idxs[j + 1] = idxs[j];
        --j;
      }
      lums[j + 1] = keyLum;
      idxs[j + 1] = keyIdx;
    }

    // Fetch the pixel that sat at the median luminance rank.
    var medianIdx = idxs[MedianRank];
    var mdy = medianIdx / (2 * WindowRadius + 1) - WindowRadius;
    var mdx = medianIdx % (2 * WindowRadius + 1) - WindowRadius;
    var medianPixel = frame[x0 + mdx, y0 + mdy].Work;

    // Blend with bilinear baseline at the configured strength.
    Accum4F<TWork> acc = default;
    acc.AddMul(bilinear, 1f - strength);
    acc.AddMul(medianPixel, strength);
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
