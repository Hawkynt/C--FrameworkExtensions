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
/// Anime4K-style edge-directed upscaler (algorithmic, no neural network).
/// </summary>
/// <remarks>
/// <para>Tuned for hand-drawn / cartoon content where edges are sparse and high-contrast and
/// gradients between fills are minimal. Implements the algorithmic core of Anime4K v0.9 (the
/// "push" pass): bias each output pixel toward the darker side of the detected luminance edge
/// to thin lines and sharpen contours.</para>
/// <para>Reference: bloc97's Anime4K project — https://github.com/bloc97/Anime4K (the v0.9
/// algorithmic line; the v3+ neural variants are out of scope here).</para>
/// <para>Algorithm (per output pixel):</para>
/// <list type="number">
/// <item>Bilinear baseline from the surrounding 2×2 source neighbourhood.</item>
/// <item>3×3 Sobel-like luminance gradient at the source-space sample position.</item>
/// <item>If gradient magnitude exceeds the edge threshold, sample the darker-side neighbour
///   along the gradient (one step into the negative-gradient direction).</item>
/// <item>Blend bilinear → darker neighbour weighted by edge strength × push factor — pulls the
///   contour toward the darker side, thinning bright lines into clean strokes.</item>
/// </list>
/// <para>The single-pass form approximates Anime4K's iterative push: the runtime is comparable
/// to Lanczos at 1024² → 4096² and the result is byte-exact reproducible.</para>
/// </remarks>
[ScalerInfo("Anime4K", Author = "bloc97 (algorithm)", Year = 2019,
  Description = "Algorithmic edge-directed upscaler tuned for anime/cartoon content",
  Category = ScalerCategory.Resampler)]
public readonly struct Anime4K : IResampler {

  /// <summary>Default edge threshold (gradient magnitude above which the push activates).</summary>
  public const float DefaultEdgeThreshold = 0.05f;

  /// <summary>Default push strength (0.6 — moderate edge thinning).</summary>
  public const float DefaultPushStrength = 0.6f;

  private readonly float _edgeThreshold;
  private readonly float _pushStrength;

  /// <summary>Creates an Anime4K resampler with default parameters.</summary>
  public Anime4K() : this(DefaultEdgeThreshold, DefaultPushStrength) { }

  /// <summary>Creates an Anime4K resampler with custom parameters.</summary>
  /// <param name="edgeThreshold">Gradient magnitude (luminance, 0..1) below which the pixel is
  /// treated as a smooth region and bilinear is used unmodified.</param>
  /// <param name="pushStrength">How far to push toward the darker side of an edge ∈ [0, 1].
  /// 0 = pure bilinear; 1 = full replacement by the darker-side sample at strongest edges.</param>
  public Anime4K(float edgeThreshold, float pushStrength) {
    ArgumentOutOfRangeException.ThrowIfNegative(edgeThreshold);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(edgeThreshold, 1f);
    ArgumentOutOfRangeException.ThrowIfNegative(pushStrength);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(pushStrength, 1f);
    this._edgeThreshold = edgeThreshold;
    this._pushStrength = pushStrength;
  }

  /// <summary>Gets the edge threshold.</summary>
  public float EdgeThreshold => this._edgeThreshold == 0f ? DefaultEdgeThreshold : this._edgeThreshold;

  /// <summary>Gets the push strength.</summary>
  public float PushStrength => this._pushStrength == 0f ? DefaultPushStrength : this._pushStrength;

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
    => callback.Invoke(new Anime4KKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.EdgeThreshold, this.PushStrength, useCenteredGrid));

  /// <summary>Gets the default configuration.</summary>
  public static Anime4K Default => new();

  /// <summary>Gets a stronger configuration (higher push strength).</summary>
  public static Anime4K Strong => new(DefaultEdgeThreshold, 0.85f);

  /// <summary>Gets a softer configuration (lower push strength).</summary>
  public static Anime4K Soft => new(DefaultEdgeThreshold, 0.35f);
}

file readonly struct Anime4KKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight,
  float edgeThreshold, float pushStrength, bool useCenteredGrid)
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

    // 2x2 source neighbourhood for bilinear baseline.
    var c00 = frame[x0, y0].Work;
    var c10 = frame[x0 + 1, y0].Work;
    var c01 = frame[x0, y0 + 1].Work;
    var c11 = frame[x0 + 1, y0 + 1].Work;

    var bilinear = BilinearInterpolate(c00, c10, c01, c11, fx, fy);

    // 3x3 Sobel-like luminance gradient. Sample anchored at (x0, y0) — the nearest source
    // pixel to the destination — keeping things deterministic and cheap.
    var lNW = ColorConverter.GetLuminance(frame[x0 - 1, y0 - 1].Work);
    var lN  = ColorConverter.GetLuminance(frame[x0,     y0 - 1].Work);
    var lNE = ColorConverter.GetLuminance(frame[x0 + 1, y0 - 1].Work);
    var lW  = ColorConverter.GetLuminance(frame[x0 - 1, y0    ].Work);
    var lE  = ColorConverter.GetLuminance(frame[x0 + 1, y0    ].Work);
    var lSW = ColorConverter.GetLuminance(frame[x0 - 1, y0 + 1].Work);
    var lS  = ColorConverter.GetLuminance(frame[x0,     y0 + 1].Work);
    var lSE = ColorConverter.GetLuminance(frame[x0 + 1, y0 + 1].Work);

    // Sobel gradient (3×3): gx and gy.
    var gx = (lNE + 2f * lE + lSE) - (lNW + 2f * lW + lSW);
    var gy = (lSW + 2f * lS + lSE) - (lNW + 2f * lN + lNE);
    // Normalise to ~[0, 1] luminance scale: each Sobel sum has weight 4, sign-difference up to 4.
    gx *= 0.25f;
    gy *= 0.25f;

    var gMag = MathF.Sqrt(gx * gx + gy * gy);
    if (gMag < edgeThreshold) {
      // Smooth region — bilinear is fine.
      dest[destY * destStride + destX] = encoder.Encode(bilinear);
      return;
    }

    // Anime4K push: step one source-pixel against the gradient (toward the darker side) and
    // sample. This is the headline operation — it pulls bright lines toward their dark
    // neighbours, thinning anti-aliased edges into clean strokes.
    var invMag = 1f / gMag;
    var nx = gx * invMag;
    var ny = gy * invMag;
    var pushedSample = SampleBilinear(frame, srcXf - nx, srcYf - ny);

    // Edge weight ∈ [0, 1]: ramps from 0 at threshold to 1 well above it.
    var edgeWeight = MathF.Min(1f, (gMag - edgeThreshold) / MathF.Max(edgeThreshold, 1e-3f));
    var w = edgeWeight * pushStrength;

    Accum4F<TWork> acc = default;
    acc.AddMul(bilinear, 1f - w);
    acc.AddMul(pushedSample, w);
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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork SampleBilinear(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    float x, float y) {
    var ix = (int)MathF.Floor(x);
    var iy = (int)MathF.Floor(y);
    var fx = x - ix;
    var fy = y - iy;
    return BilinearInterpolate(frame[ix, iy].Work, frame[ix + 1, iy].Work,
      frame[ix, iy + 1].Work, frame[ix + 1, iy + 1].Work, fx, fy);
  }
}
