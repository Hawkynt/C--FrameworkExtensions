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
/// Directional Cubic Convolution Interpolation (DCCI) resampler.
/// </summary>
/// <remarks>
/// <para>Uses edge direction detection via structure tensor (eigenvalue analysis).</para>
/// <para>Interpolates along detected edge directions for sharp edge preservation.</para>
/// <para>Falls back to bicubic interpolation in low-coherence areas.</para>
/// <para>Based on "New Edge-Directed Interpolation" by Xin Li and Michael T. Orchard (2001).</para>
/// </remarks>
[ScalerInfo("DCCI", Author = "Xin Li, Michael T. Orchard", Year = 2001,
  Description = "Edge-directed cubic convolution interpolation", Category = ScalerCategory.Resampler)]
public readonly struct Dcci : IResampler {

  private readonly float _cubicA;
  private readonly float _coherenceThreshold;

  /// <summary>
  /// Creates a DCCI resampler with default parameters.
  /// </summary>
  public Dcci() : this(-0.5f, 0.3f) { }

  /// <summary>
  /// Creates a DCCI resampler with custom parameters.
  /// </summary>
  /// <param name="cubicA">The cubic coefficient. -0.5 = Keys, -0.75 = sharper.</param>
  /// <param name="coherenceThreshold">Edge coherence threshold (0-1). Lower = more edge detection.</param>
  public Dcci(float cubicA, float coherenceThreshold) {
    this._cubicA = cubicA;
    this._coherenceThreshold = coherenceThreshold;
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
    int targetHeight)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new DcciKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this._cubicA, this._coherenceThreshold));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Dcci Default => new();

  /// <summary>
  /// Gets a sharper configuration with more aggressive edge detection.
  /// </summary>
  public static Dcci Sharp => new(-0.75f, 0.2f);

  /// <summary>
  /// Gets a smoother configuration with reduced ringing.
  /// </summary>
  public static Dcci Smooth => new(-0.25f, 0.4f);
}

file readonly struct DcciKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, float cubicA, float coherenceThreshold)
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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    // Map destination pixel center to source coordinates
    var srcXf = (destX + 0.5f) * this._scaleX - 0.5f;
    var srcYf = (destY + 0.5f) * this._scaleY - 0.5f;

    // Integer base coordinates
    var x0 = (int)MathF.Floor(srcXf);
    var y0 = (int)MathF.Floor(srcYf);

    // Fractional parts
    var fx = srcXf - x0;
    var fy = srcYf - y0;

    // Compute edge direction using structure tensor
    var (coherence, angle) = ComputeEdgeDirection(frame, x0, y0);

    TWork result;
    if (coherence > coherenceThreshold)
      result = DirectionalCubicInterpolate(frame, x0, y0, fx, fy, angle, cubicA);
    else
      result = BicubicInterpolate(frame, x0, y0, fx, fy, cubicA);

    dest[destY * destStride + destX] = encoder.Encode(result);
  }

  /// <summary>
  /// Computes edge direction using structure tensor eigenvalue analysis.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static (float Coherence, float Angle) ComputeEdgeDirection(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int x, int y) {
    // Sample 3x3 neighborhood
    var c00 = frame[x - 1, y - 1].Work;
    var c10 = frame[x, y - 1].Work;
    var c20 = frame[x + 1, y - 1].Work;
    var c01 = frame[x - 1, y].Work;
    var c21 = frame[x + 1, y].Work;
    var c02 = frame[x - 1, y + 1].Work;
    var c12 = frame[x, y + 1].Work;
    var c22 = frame[x + 1, y + 1].Work;

    // Sobel gradients using first 3 color components (C1, C2, C3)
    var gxC1 = -c00.C1 - 2 * c01.C1 - c02.C1 + c20.C1 + 2 * c21.C1 + c22.C1;
    var gyC1 = -c00.C1 - 2 * c10.C1 - c20.C1 + c02.C1 + 2 * c12.C1 + c22.C1;
    var gxC2 = -c00.C2 - 2 * c01.C2 - c02.C2 + c20.C2 + 2 * c21.C2 + c22.C2;
    var gyC2 = -c00.C2 - 2 * c10.C2 - c20.C2 + c02.C2 + 2 * c12.C2 + c22.C2;
    var gxC3 = -c00.C3 - 2 * c01.C3 - c02.C3 + c20.C3 + 2 * c21.C3 + c22.C3;
    var gyC3 = -c00.C3 - 2 * c10.C3 - c20.C3 + c02.C3 + 2 * c12.C3 + c22.C3;

    // Structure tensor components (sum of squared gradients across channels)
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

    // Coherence: how "edge-like" the local region is (0 = flat, 1 = perfect edge)
    var coherence = (lambda1 - lambda2) / (lambda1 + lambda2 + 0.0001f);

    // Edge angle (perpendicular to gradient direction)
    var angle = 0.5f * MathF.Atan2(2 * ixy, ixx - iyy);

    return (coherence, angle);
  }

  /// <summary>
  /// Interpolates along the detected edge direction using cubic kernel.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork DirectionalCubicInterpolate(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int x0, int y0,
    float fx, float fy,
    float angle,
    float a) {
    // Unit vector along edge direction
    var cosA = MathF.Cos(angle);
    var sinA = MathF.Sin(angle);

    // Project fractional offset onto edge direction
    var t = fx * cosA + fy * sinA;

    // Sample 4 points along the edge
    var baseX = x0 + fx;
    var baseY = y0 + fy;

    var p0 = SampleBilinear(frame, baseX - cosA, baseY - sinA);
    var p1 = SampleBilinear(frame, baseX, baseY);
    var p2 = SampleBilinear(frame, baseX + cosA, baseY + sinA);
    var p3 = SampleBilinear(frame, baseX + 2 * cosA, baseY + 2 * sinA);

    // Cubic interpolation weights
    var t2 = t * t;
    var t3 = t2 * t;

    var w0 = a * t3 - 2 * a * t2 + a * t;
    var w1 = (a + 2) * t3 - (a + 3) * t2 + 1;
    var w2 = -(a + 2) * t3 + (2 * a + 3) * t2 - a * t;
    var w3 = -a * t3 + a * t2;

    // Accumulate weighted result
    Accum4F<TWork> acc = default;
    acc.AddMul(p0, w0);
    acc.AddMul(p1, w1);
    acc.AddMul(p2, w2);
    acc.AddMul(p3, w3);

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

    var w00 = (1 - fx) * (1 - fy);
    var w10 = fx * (1 - fy);
    var w01 = (1 - fx) * fy;
    var w11 = fx * fy;

    Accum4F<TWork> acc = default;
    acc.AddMul(c00, w00);
    acc.AddMul(c10, w10);
    acc.AddMul(c01, w01);
    acc.AddMul(c11, w11);

    return acc.Result;
  }

  /// <summary>
  /// Standard bicubic interpolation for low-coherence areas.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe TWork BicubicInterpolate(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int x0, int y0,
    float fx, float fy,
    float a) {
    // Precompute weights for x and y
    var wx = stackalloc float[4];
    var wy = stackalloc float[4];
    for (var i = 0; i < 4; ++i) {
      wx[i] = CubicWeight(fx - (i - 1), a);
      wy[i] = CubicWeight(fy - (i - 1), a);
    }

    // Accumulate weighted colors from 4x4 kernel
    Accum4F<TWork> acc = default;
    for (var ky = 0; ky < 4; ++ky)
    for (var kx = 0; kx < 4; ++kx) {
      var weight = wx[kx] * wy[ky];
      if (weight == 0f)
        continue;

      var pixel = frame[x0 + kx - 1, y0 + ky - 1].Work;
      acc.AddMul(pixel, weight);
    }

    return acc.Result;
  }

  /// <summary>
  /// Computes cubic kernel weight.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float CubicWeight(float x, float a) {
    x = MathF.Abs(x);
    if (x < 1f)
      return ((a + 2f) * x - (a + 3f)) * x * x + 1f;
    if (x < 2f)
      return a * (((x - 5f) * x + 8f) * x - 4f);
    return 0f;
  }
}
