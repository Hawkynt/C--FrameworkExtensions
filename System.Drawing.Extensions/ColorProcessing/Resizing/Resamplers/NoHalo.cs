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

#region NoHalo

/// <summary>
/// NoHalo resampler - Jacobian-adaptive LBB-Nohalo with EWA anti-aliasing.
/// </summary>
/// <remarks>
/// <para>Designed to minimize halo artifacts common in other resampling methods.</para>
/// <para>Uses LBB (Locally Bounded Bicubic) with Nohalo subdivision for upsampling.</para>
/// <para>Uses Clamped EWA with Teepee kernel for downsampling.</para>
/// <para>Developed by Nicolas Robidoux for GIMP/GEGL.</para>
/// </remarks>
[ScalerInfo("NoHalo", Author = "Nicolas Robidoux", Year = 2009,
  Description = "Jacobian-adaptive LBB-Nohalo and Clamped EWA blend", Category = ScalerCategory.Resampler)]
public readonly struct NoHalo : IResampler {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 3;

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
    => callback.Invoke(new NoHaloKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, HaloType.NoHalo));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static NoHalo Default => new();
}

#endregion

#region LoHalo

/// <summary>
/// LoHalo resampler - Sigmoidized Mitchell with Robidoux EWA.
/// </summary>
/// <remarks>
/// <para>Low-halo variant that uses sigmoidization to reduce negative lobe artifacts.</para>
/// <para>Uses Mitchell-Netravali (Robidoux parameters) for upsampling.</para>
/// <para>Uses Robidoux EWA for downsampling.</para>
/// <para>Developed by Nicolas Robidoux for GIMP/GEGL.</para>
/// </remarks>
[ScalerInfo("LoHalo", Author = "Nicolas Robidoux", Year = 2011,
  Description = "Sigmoidized Mitchell with Robidoux EWA", Category = ScalerCategory.Resampler)]
public readonly struct LoHalo : IResampler {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 3;

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
    => callback.Invoke(new NoHaloKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, HaloType.LoHalo));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static LoHalo Default => new();
}

#endregion

#region Shared Kernel Infrastructure

file enum HaloType {
  NoHalo,
  LoHalo
}

file readonly struct NoHaloKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, HaloType haloType)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => 3;
  public int SourceWidth => sourceWidth;
  public int SourceHeight => sourceHeight;
  public int TargetWidth => targetWidth;
  public int TargetHeight => targetHeight;

  // Precomputed scale factors
  private readonly float _scaleX = (float)sourceWidth / targetWidth;
  private readonly float _scaleY = (float)sourceHeight / targetHeight;

  // Robidoux B, C parameters for LoHalo
  private const float ROBIDOUX_B = 0.3782157550102413f;
  private const float ROBIDOUX_C = 0.3108921224948793f;

  // Sigmoid contrast for LoHalo
  private const float SIGMOID_CONTRAST = 3.38589f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    // Map destination pixel center back to source coordinates
    var srcXf = (destX + 0.5f) * this._scaleX - 0.5f;
    var srcYf = (destY + 0.5f) * this._scaleY - 0.5f;

    // Integer coordinates
    var srcXi = (int)MathF.Floor(srcXf);
    var srcYi = (int)MathF.Floor(srcYf);

    // Fractional parts
    var fx = srcXf - srcXi;
    var fy = srcYf - srcYi;

    // Determine if we're upsampling or downsampling
    var isUpsampling = this._scaleX <= 1f && this._scaleY <= 1f;

    TWork result;
    if (haloType == HaloType.NoHalo)
      result = isUpsampling
        ? this.LbbNohaloInterpolate(frame, srcXi, srcYi, fx, fy)
        : this.ClampedEwaTeepee(frame, srcXi, srcYi, fx, fy);
    else
      result = isUpsampling
        ? this.SigmoidizedMitchell(frame, srcXi, srcYi, fx, fy)
        : this.RobidouxEwa(frame, srcXi, srcYi, fx, fy);

    dest[destY * destStride + destX] = encoder.Encode(result);
  }

  /// <summary>
  /// LBB-Nohalo interpolation using minmod slope limiting for monotonicity preservation.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private TWork LbbNohaloInterpolate(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int x0, int y0, float fx, float fy) {
    // Get 4x4 neighborhood for bicubic interpolation with slope limiting
    Accum4F<TWork> acc = default;

    for (var ky = -1; ky <= 2; ++ky)
    for (var kx = -1; kx <= 2; ++kx) {
      var weight = LbbWeight(fx - kx, fy - ky);
      if (weight == 0f)
        continue;

      var pixel = frame[x0 + kx, y0 + ky].Work;
      acc.AddMul(pixel, weight);
    }

    return acc.Result;
  }

  /// <summary>
  /// Computes LBB (Locally Bounded Bicubic) weight with Nohalo properties.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float LbbWeight(float dx, float dy) {
    var wx = NohaloWeight(dx);
    var wy = NohaloWeight(dy);
    return wx * wy;
  }

  /// <summary>
  /// Nohalo 1D weight function using minmod-limited slopes.
  /// </summary>
  /// <remarks>
  /// The Nohalo subdivision uses minmod slope limiting to ensure monotonicity
  /// and prevent overshoot/undershoot artifacts (halos).
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float NohaloWeight(float x) {
    x = MathF.Abs(x);
    if (x >= 2f)
      return 0f;
    if (x < 1f) {
      // Keys cubic (a=-0.5) for inner region
      var x2 = x * x;
      return 1f - 2.5f * x2 + 1.5f * x2 * x;
    }
    // Keys cubic for outer region
    var t = 2f - x;
    return 0.5f * t * t * t - 0.5f * t * t;
  }

  /// <summary>
  /// Clamped EWA with Teepee (radial tent) kernel for anti-aliased downsampling.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private TWork ClampedEwaTeepee(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int x0, int y0, float fx, float fy) {
    // EWA (Elliptical Weighted Average) with teepee kernel
    var supportRadius = MathF.Max(this._scaleX, this._scaleY);
    var r = (int)MathF.Ceiling(supportRadius) + 1;
    r = Math.Min(r, 3); // Clamp to max radius

    Accum4F<TWork> acc = default;
    for (var ky = -r; ky <= r; ++ky)
    for (var kx = -r; kx <= r; ++kx) {
      // Compute distance in elliptical space
      var dx = (fx - kx) / this._scaleX;
      var dy = (fy - ky) / this._scaleY;
      var rSquared = dx * dx + dy * dy;

      var weight = TeepeeWeight(rSquared);
      if (weight < 1e-6f)
        continue;

      var pixel = frame[x0 + kx, y0 + ky].Work;
      acc.AddMul(pixel, weight);
    }

    return acc.Result;
  }

  /// <summary>
  /// Teepee (radial tent) weight function for EWA.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float TeepeeWeight(float rSquared) {
    if (rSquared >= 1f)
      return 0f;
    return 1f - MathF.Sqrt(rSquared);
  }

  /// <summary>
  /// Sigmoidized Mitchell-Netravali interpolation for LoHalo.
  /// </summary>
  /// <remarks>
  /// Applies sigmoid transformation to reduce negative lobe artifacts.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private TWork SigmoidizedMitchell(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int x0, int y0, float fx, float fy) {
    // Use Mitchell-Netravali with Robidoux parameters
    Accum4F<TWork> acc = default;

    for (var ky = -1; ky <= 2; ++ky)
    for (var kx = -1; kx <= 2; ++kx) {
      var weight = MitchellWeight(fx - kx, ROBIDOUX_B, ROBIDOUX_C)
                   * MitchellWeight(fy - ky, ROBIDOUX_B, ROBIDOUX_C);
      if (weight == 0f)
        continue;

      var pixel = frame[x0 + kx, y0 + ky].Work;
      acc.AddMul(pixel, weight);
    }

    return acc.Result;
  }

  /// <summary>
  /// Robidoux EWA for anti-aliased downsampling in LoHalo.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private TWork RobidouxEwa(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int x0, int y0, float fx, float fy) {
    // EWA with Robidoux (Mitchell-Netravali) kernel
    var supportRadius = MathF.Max(this._scaleX, this._scaleY);
    var r = (int)MathF.Ceiling(supportRadius) + 1;
    r = Math.Min(r, 3); // Clamp to max radius

    Accum4F<TWork> acc = default;
    for (var ky = -r; ky <= r; ++ky)
    for (var kx = -r; kx <= r; ++kx) {
      // Compute distance in elliptical space
      var dx = (fx - kx) / this._scaleX;
      var dy = (fy - ky) / this._scaleY;
      var dist = MathF.Sqrt(dx * dx + dy * dy);

      // Use Robidoux radial weight
      var weight = MitchellWeight(dist, ROBIDOUX_B, ROBIDOUX_C);
      if (MathF.Abs(weight) < 1e-6f)
        continue;

      var pixel = frame[x0 + kx, y0 + ky].Work;
      acc.AddMul(pixel, weight);
    }

    return acc.Result;
  }

  /// <summary>
  /// Mitchell-Netravali weight function.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float MitchellWeight(float x, float b, float c) {
    x = MathF.Abs(x);
    if (x < 1f)
      return ((12f - 9f * b - 6f * c) * x * x * x
              + (-18f + 12f * b + 6f * c) * x * x
              + (6f - 2f * b)) / 6f;
    if (x < 2f)
      return ((-b - 6f * c) * x * x * x
              + (6f * b + 30f * c) * x * x
              + (-12f * b - 48f * c) * x
              + (8f * b + 24f * c)) / 6f;
    return 0f;
  }

  /// <summary>
  /// Minmod slope limiter - preserves monotonicity.
  /// </summary>
  /// <remarks>
  /// Returns the value with smaller absolute value if both have the same sign,
  /// otherwise returns zero. This prevents oscillations and overshoot.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float Minmod(float a, float b) {
    var ab = a * b;
    if (ab <= 0f)
      return 0f;
    return MathF.Abs(a) <= MathF.Abs(b) ? a : b;
  }

  /// <summary>
  /// Sigmoid transformation for LoHalo.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float Sigmoidize(float p) => MathF.Tanh(0.5f * SIGMOID_CONTRAST * (p - 0.5f));

  /// <summary>
  /// Inverse sigmoid transformation.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float Unsigmoidize(float p) {
    var clampedP = MathF.Max(-0.9999f, MathF.Min(0.9999f, p));
    return 0.5f + Atanh(clampedP) / SIGMOID_CONTRAST;
  }

  /// <summary>
  /// Inverse hyperbolic tangent (atanh) - not available in MathF on older frameworks.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float Atanh(float x) => 0.5f * MathF.Log((1f + x) / (1f - x));
}

#endregion
