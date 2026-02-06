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
/// Iterative Curvature-Based Interpolation (ICBI) resampler.
/// </summary>
/// <remarks>
/// <para>Algorithm: ICBI by Li and Orchard (2001)</para>
/// <para>Reference: https://ieeexplore.ieee.org/document/977589</para>
/// <para>Paper: "Image Interpolation by Curvature Continuation" - IEEE Trans. Image Processing</para>
/// <para>Performs bilinear interpolation with structure tensor analysis and curvature-minimizing
/// correction. In edge regions, interpolates along the detected edge direction.
/// In smooth regions, applies Laplacian smoothing correction.</para>
/// <para>Adapted as single-pass resampler with configurable correction strength.</para>
/// </remarks>
[ScalerInfo("ICBI", Author = "Li & Orchard", Year = 2001,
  Url = "https://ieeexplore.ieee.org/document/977589",
  Description = "Curvature-based interpolation with structure tensor analysis", Category = ScalerCategory.Resampler)]
public readonly struct Icbi : IResampler {

  private readonly float _coherenceThreshold;
  private readonly float _correctionFactor;

  /// <summary>
  /// Creates an ICBI resampler with default parameters.
  /// </summary>
  public Icbi() : this(0.3f, 0.2f) { }

  /// <summary>
  /// Creates an ICBI resampler with custom parameters.
  /// </summary>
  /// <param name="coherenceThreshold">Edge coherence threshold (0-1). Lower = more edge detection.</param>
  /// <param name="correctionFactor">Curvature correction strength. Higher = stronger correction.</param>
  public Icbi(float coherenceThreshold, float correctionFactor) {
    this._coherenceThreshold = coherenceThreshold;
    this._correctionFactor = correctionFactor;
  }

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
    => callback.Invoke(new IcbiKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this._coherenceThreshold, this._correctionFactor, useCenteredGrid));

  /// <summary>Gets the default configuration (standard correction).</summary>
  public static Icbi Default => new();
}

/// <summary>
/// ICBI Fast variant - lightest curvature correction for speed.
/// </summary>
[ScalerInfo("ICBI Fast", Author = "Li & Orchard", Year = 2001,
  Url = "https://ieeexplore.ieee.org/document/977589",
  Description = "Fast ICBI with light curvature correction", Category = ScalerCategory.Resampler)]
public readonly struct IcbiFast : IResampler {

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
    => callback.Invoke(new IcbiKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, 0.3f, 0.1f, useCenteredGrid));

  /// <summary>Gets the default Fast ICBI configuration.</summary>
  public static IcbiFast Default => new();
}

/// <summary>
/// ICBI HQ variant - strongest curvature correction for highest quality.
/// </summary>
[ScalerInfo("ICBI HQ", Author = "Li & Orchard", Year = 2001,
  Url = "https://ieeexplore.ieee.org/document/977589",
  Description = "High-quality ICBI with strong curvature correction", Category = ScalerCategory.Resampler)]
public readonly struct IcbiHq : IResampler {

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
    => callback.Invoke(new IcbiKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, 0.2f, 0.3f, useCenteredGrid));

  /// <summary>Gets the default HQ ICBI configuration.</summary>
  public static IcbiHq Default => new();
}

file readonly struct IcbiKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight,
  float coherenceThreshold, float correctionFactor, bool useCenteredGrid)
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

    // Step 1: Initial bilinear interpolation
    var c00 = frame[x0, y0].Work;
    var c10 = frame[x0 + 1, y0].Work;
    var c01 = frame[x0, y0 + 1].Work;
    var c11 = frame[x0 + 1, y0 + 1].Work;

    var bilinear = BilinearInterpolate(c00, c10, c01, c11, fx, fy);

    // Step 2: Compute structure tensor from source neighborhood
    var left = frame[x0 - 1, y0].Work;
    var right = frame[x0 + 1, y0].Work;
    var up = frame[x0, y0 - 1].Work;
    var down = frame[x0, y0 + 1].Work;

    // Gradient computation across color channels
    var gxC1 = (right.C1 - left.C1) * 0.5f;
    var gyC1 = (down.C1 - up.C1) * 0.5f;
    var gxC2 = (right.C2 - left.C2) * 0.5f;
    var gyC2 = (down.C2 - up.C2) * 0.5f;
    var gxC3 = (right.C3 - left.C3) * 0.5f;
    var gyC3 = (down.C3 - up.C3) * 0.5f;

    // Structure tensor components
    var ixx = gxC1 * gxC1 + gxC2 * gxC2 + gxC3 * gxC3;
    var iyy = gyC1 * gyC1 + gyC2 * gyC2 + gyC3 * gyC3;
    var ixy = gxC1 * gyC1 + gxC2 * gyC2 + gxC3 * gyC3;

    // Eigenvalue analysis
    var trace = ixx + iyy;
    var det = ixx * iyy - ixy * ixy;
    var disc = trace * trace - 4 * det;
    if (disc < 0)
      disc = 0;
    var sqrtDisc = MathF.Sqrt(disc);

    var lambda1 = (trace + sqrtDisc) * 0.5f;
    var lambda2 = (trace - sqrtDisc) * 0.5f;

    // Coherence: how "edge-like" the region is
    var coherence = (lambda1 - lambda2) / (lambda1 + lambda2 + 0.0001f);

    TWork result;
    if (coherence > coherenceThreshold) {
      // Step 3a: Strong edge - interpolate along edge direction
      var angle = 0.5f * MathF.Atan2(2 * ixy, ixx - iyy);
      var cosA = MathF.Cos(angle);
      var sinA = MathF.Sin(angle);

      // Sample along edge direction (perpendicular to gradient)
      var along1 = SampleBilinear(frame, srcXf + cosA, srcYf + sinA);
      var along2 = SampleBilinear(frame, srcXf - cosA, srcYf - sinA);

      // Blend bilinear with edge-directed average, weighted by coherence
      var edgeWeight = coherence * 0.3f;

      Accum4F<TWork> acc = default;
      acc.AddMul(bilinear, 1f - edgeWeight);
      acc.AddMul(along1, edgeWeight * 0.5f);
      acc.AddMul(along2, edgeWeight * 0.5f);
      result = acc.Result;
    } else {
      // Step 3b: Smooth region - apply Laplacian curvature minimization
      // Laplacian = average of 4 neighbors minus center
      // Apply correction: result = bilinear + correctionFactor * Laplacian
      // = bilinear * (1 - 4*cf) + (left + right + up + down) * cf
      // But to avoid negative weights, decompose:
      // = bilinear + cf * (left + right + up + down - 4 * center)
      // where center ≈ bilinear for small fractional offsets
      var center = frame[x0, y0].Work;
      var cf = correctionFactor;

      Accum4F<TWork> acc = default;
      acc.AddMul(bilinear, 1f);
      acc.AddMul(left, cf);
      acc.AddMul(right, cf);
      acc.AddMul(up, cf);
      acc.AddMul(down, cf);
      acc.AddMul(center, -4f * cf);
      result = acc.Result;
    }

    dest[destY * destStride + destX] = encoder.Encode(result);
  }

  /// <summary>
  /// Standard bilinear interpolation of 4 pixels.
  /// </summary>
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

  /// <summary>
  /// Samples at fractional coordinates using bilinear interpolation.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork SampleBilinear(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    float x, float y) {
    var ix = (int)MathF.Floor(x);
    var iy = (int)MathF.Floor(y);
    var fx = x - ix;
    var fy = y - iy;

    var c00 = frame[ix, iy].Work;
    var c10 = frame[ix + 1, iy].Work;
    var c01 = frame[ix, iy + 1].Work;
    var c11 = frame[ix + 1, iy + 1].Work;

    return BilinearInterpolate(c00, c10, c01, c11, fx, fy);
  }
}
