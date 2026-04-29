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
/// Lucy-Richardson iterative deconvolution resampler.
/// </summary>
/// <remarks>
/// <para>Multiplicative iterative deconvolution; the canonical companion to the additive
/// IBP method (also in this namespace). Each iteration multiplies the current estimate by
/// the ratio of the observed source to the modelled blur of the estimate, converging toward
/// a deconvolved image under a Gaussian PSF.</para>
/// <para>This is a per-pixel approximation of the full-image LR loop — each output pixel
/// runs N iterations on a small source neighbourhood, so the iterative reach is bounded by
/// kernel size × N. Sufficient for typical sharpening / detail recovery without the cost of
/// whole-image passes.</para>
/// <para>References: Richardson 1972, "Bayesian-based iterative method of image
/// restoration", J. Optical Society of America. Lucy 1974, "An iterative technique for the
/// rectification of observed distributions", Astronomical Journal. Both public domain.</para>
/// </remarks>
[ScalerInfo("LucyRichardson", Author = "Richardson (1972) / Lucy (1974)", Year = 1972,
  Description = "Multiplicative iterative deconvolution super-resolution",
  Category = ScalerCategory.Resampler)]
public readonly struct LucyRichardson : IResampler {

  /// <summary>Default iteration count (4).</summary>
  public const int DefaultIterations = 4;

  /// <summary>Default damping factor (0.5 — half-step relaxation).</summary>
  public const float DefaultDamping = 0.5f;

  private readonly int _iterations;
  private readonly float _damping;

  /// <summary>Creates an LR resampler with default parameters.</summary>
  public LucyRichardson() : this(DefaultIterations, DefaultDamping) { }

  /// <summary>Creates an LR resampler with custom parameters.</summary>
  /// <param name="iterations">Number of LR iterations (1 to 16).</param>
  /// <param name="damping">Per-iteration relaxation factor ∈ (0, 1]; smaller values produce
  /// gentler updates and reduce ringing. The classical LR has damping=1.</param>
  public LucyRichardson(int iterations, float damping) {
    ArgumentOutOfRangeException.ThrowIfLessThan(iterations, 1);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(iterations, 16);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(damping);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(damping, 1f);
    this._iterations = iterations;
    this._damping = damping;
  }

  /// <summary>Gets the iteration count.</summary>
  public int Iterations => this._iterations == 0 ? DefaultIterations : this._iterations;

  /// <summary>Gets the damping factor.</summary>
  public float Damping => this._damping == 0f ? DefaultDamping : this._damping;

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
    => callback.Invoke(new LucyRichardsonKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.Iterations, this.Damping, useCenteredGrid));

  /// <summary>Gets the default configuration (4 iterations, 0.5 damping).</summary>
  public static LucyRichardson Default => new();

  /// <summary>Gets a stronger configuration (8 iterations, 0.4 damping).</summary>
  public static LucyRichardson Strong => new(8, 0.4f);

  /// <summary>Gets a softer configuration (2 iterations, 0.5 damping).</summary>
  public static LucyRichardson Soft => new(2, 0.5f);
}

file readonly struct LucyRichardsonKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight,
  int iterations, float damping, bool useCenteredGrid)
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

    // Bilinear baseline = e₀.
    var c00 = frame[x0, y0].Work;
    var c10 = frame[x0 + 1, y0].Work;
    var c01 = frame[x0, y0 + 1].Work;
    var c11 = frame[x0 + 1, y0 + 1].Work;
    var bilinear = BilinearInterpolate(c00, c10, c01, c11, fx, fy);

    // Reference (the observed value to back-project against) and the modelled blur of the
    // local source — same Gaussian 3×3 kernel as the IBP companion shipped earlier so the
    // two methods are directly comparable.
    var reference = frame[x0, y0].Work;
    var blurred = BlurredSource(frame, x0, y0);

    // Run the LR iteration on a scalar "gain" relative to bilinear, using luminance as the
    // proxy. This keeps the inner loop in single-precision scalars (one MathF.Max + divide
    // per iteration) and avoids per-channel divides that would tear into the gain budget on
    // older TFMs where MathF intrinsics aren't available.
    var bLum = ColorConverter.GetLuminance(bilinear);
    var rLum = ColorConverter.GetLuminance(reference);
    var blurLum = ColorConverter.GetLuminance(blurred);

    var e = bLum;
    for (var i = 0; i < iterations; ++i) {
      // Linearised blur model: blur(e) ≈ blurLum + (e − rLum). When e converges, this gives
      // blur(e) = blurLum, matching the source's actual blur — i.e. e_∞ is consistent with
      // the observed `reference` pixel under the assumed Gaussian PSF.
      var modelBlur = MathF.Max(blurLum + (e - rLum), 1e-6f);
      var ratio = rLum / modelBlur;
      // Damped multiplicative update: e ← e · (1 − d + d · ratio).
      e *= 1f - damping + damping * ratio;
    }
    var gain = e / MathF.Max(bLum, 1e-6f);

    // Apply the scalar gain to the bilinear sample. Clamp the gain to a sane range so a
    // pathological reference pixel can't blow chrominance into infinity.
    if (gain < 0.25f) gain = 0.25f;
    else if (gain > 4f) gain = 4f;

    var result = ColorFactory.Create4F<TWork>(
      bilinear.C1 * gain, bilinear.C2 * gain, bilinear.C3 * gain, bilinear.A);
    dest[destY * destStride + destX] = encoder.Encode(result);
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

  // 3×3 normalised Gaussian (1/16, 2/16, 1/16; 2/16, 4/16, 2/16; 1/16, 2/16, 1/16) at (x0, y0).
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork BlurredSource(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int x0, int y0) {
    Accum4F<TWork> acc = default;
    acc.AddMul(frame[x0 - 1, y0 - 1].Work, 1f / 16f);
    acc.AddMul(frame[x0,     y0 - 1].Work, 2f / 16f);
    acc.AddMul(frame[x0 + 1, y0 - 1].Work, 1f / 16f);
    acc.AddMul(frame[x0 - 1, y0    ].Work, 2f / 16f);
    acc.AddMul(frame[x0,     y0    ].Work, 4f / 16f);
    acc.AddMul(frame[x0 + 1, y0    ].Work, 2f / 16f);
    acc.AddMul(frame[x0 - 1, y0 + 1].Work, 1f / 16f);
    acc.AddMul(frame[x0,     y0 + 1].Work, 2f / 16f);
    acc.AddMul(frame[x0 + 1, y0 + 1].Work, 1f / 16f);
    return acc.Result;
  }
}
