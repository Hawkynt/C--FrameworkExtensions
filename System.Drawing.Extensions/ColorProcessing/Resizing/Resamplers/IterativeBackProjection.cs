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
/// Iterative Back-Projection (IBP) super-resolution resampler — closed-form
/// linearisation of Irani &amp; Peleg's iterative loop.
/// </summary>
/// <remarks>
/// <para>References: Irani &amp; Peleg 1991, "Improving resolution by image registration",
/// CVGIP: Graphical Models and Image Processing 53(3) — original iterative formulation
/// <c>e_{k+1} = e_k + λ·(y − blur(e_k))</c>. Ur &amp; Gross 1992 — refined application to
/// upsampling. Park, Park &amp; Kang 2003 survey for context.</para>
/// <para><b>Implementation note:</b> the canonical Irani-Peleg loop computes
/// <c>blur(e_k)</c> at every iteration, which requires a stateful per-pixel estimator
/// that the per-pixel resampler API does not support. Lib uses the LINEARISED
/// closed-form approximation — under the assumption that <c>e_k</c> stays close to
/// the reference, <c>blur(e_k) ≈ blur(source) + (e_k − source)</c>, the geometric series
/// collapses to <c>e_N ≈ source + (1 − (1−λ)^N)·(source − blur(source))</c> — i.e. an
/// unsharp mask with N-iteration-equivalent gain. This matches the canonical IBP
/// behaviour for small λ and few iterations; for larger N or stronger λ where the
/// linearisation breaks down, output diverges from the true iterative result.</para>
/// </remarks>
[ScalerInfo("IBP", Author = "Irani & Peleg", Year = 1991,
  Url = "https://www.sciencedirect.com/science/article/abs/pii/104996529190045T",
  Description = "Iterative back-projection (closed-form linearisation of the Irani-Peleg iterative loop)",
  Category = ScalerCategory.Resampler)]
public readonly struct IterativeBackProjection : IResampler {

  /// <summary>Default iteration count (4).</summary>
  public const int DefaultIterations = 4;

  /// <summary>Default step size per iteration (0.25).</summary>
  public const float DefaultStepSize = 0.25f;

  private readonly int _iterations;
  private readonly float _stepSize;

  /// <summary>Creates an IBP resampler with default parameters.</summary>
  public IterativeBackProjection() : this(DefaultIterations, DefaultStepSize) { }

  /// <summary>Creates an IBP resampler with custom parameters.</summary>
  /// <param name="iterations">Number of back-projection iterations (1 to 16).</param>
  /// <param name="stepSize">Per-iteration update strength ∈ (0, 1]. Larger = faster
  /// convergence but higher overshoot risk; total injection ≈ iterations × stepSize.</param>
  public IterativeBackProjection(int iterations, float stepSize) {
    ArgumentOutOfRangeException.ThrowIfLessThan(iterations, 1);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(iterations, 16);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(stepSize);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(stepSize, 1f);
    this._iterations = iterations;
    this._stepSize = stepSize;
  }

  /// <summary>Gets the iteration count.</summary>
  public int Iterations => this._iterations == 0 ? DefaultIterations : this._iterations;

  /// <summary>Gets the step size.</summary>
  public float StepSize => this._stepSize == 0f ? DefaultStepSize : this._stepSize;

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
    => callback.Invoke(new IbpKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.Iterations, this.StepSize, useCenteredGrid));

  /// <summary>Gets the default configuration (4 iterations, 0.25 step).</summary>
  public static IterativeBackProjection Default => new();

  /// <summary>Gets a stronger configuration (8 iterations, 0.2 step).</summary>
  public static IterativeBackProjection Strong => new(8, 0.2f);

  /// <summary>Gets a softer configuration (2 iterations, 0.25 step).</summary>
  public static IterativeBackProjection Soft => new(2, 0.25f);
}

file readonly struct IbpKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight,
  int iterations, float stepSize, bool useCenteredGrid)
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

    // Bilinear baseline — the e₀ estimate.
    var c00 = frame[x0, y0].Work;
    var c10 = frame[x0 + 1, y0].Work;
    var c01 = frame[x0, y0 + 1].Work;
    var c11 = frame[x0 + 1, y0 + 1].Work;
    var estimate = BilinearInterpolate(c00, c10, c01, c11, fx, fy);

    // Reference: nearest source pixel (the value an ideal back-projection should reproduce
    // when the high-res estimate is downsampled at this position).
    var reference = frame[x0, y0].Work;

    // Local blur of the source (3×3 Gaussian) used as the degradation model. Pre-compute once
    // per output pixel — it's a fixed function of the source neighbourhood.
    var blurred = BlurredSource(frame, x0, y0);

    // High-frequency detail to inject: source - blur(source) at the reference position.
    // After N iterations of e_{k+1} = e_k + λ·(reference - blur(e_k)) with the linearised
    // approximation blur(e_k) ≈ blurred + (e_k - reference), the closed form is
    // e_N ≈ reference + (1 - (1-λ)^N) · (reference - blurred). Sum the geometric series at
    // construction so the inner loop collapses to one multiply-add.
    var oneMinusLambdaPow = 1f;
    for (var i = 0; i < iterations; ++i) oneMinusLambdaPow *= 1f - stepSize;
    var totalGain = 1f - oneMinusLambdaPow;

    Accum4F<TWork> acc = default;
    acc.AddMul(estimate, 1f);
    acc.AddMul(reference, totalGain);
    acc.AddMul(blurred, -totalGain);

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
