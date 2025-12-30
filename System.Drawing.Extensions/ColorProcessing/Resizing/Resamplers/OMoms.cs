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

#region OMoms3

/// <summary>
/// Cubic o-Moms resampler (Optimal Maximum-order Minimum-support).
/// </summary>
/// <remarks>
/// <para>
/// o-Moms are piecewise polynomial functions with optimal approximation order
/// for a given support. OMoms3 provides better approximation than cubic B-splines
/// while using the same support.
/// </para>
/// <para>
/// <b>Note:</b> For proper interpolation, this filter requires prefiltering
/// the source image with <see cref="PrefilterInfo.OMoms3"/>.
/// </para>
/// <para>
/// Reference: "Least-Squares Polynomial Spline Approximation" by Blu, Thévenaz, and Unser.
/// </para>
/// </remarks>
[ScalerInfo("o-Moms 3", Year = 2001,
  Description = "Cubic o-Moms interpolation (optimal moments)", Category = ScalerCategory.Resampler)]
public readonly struct OMoms3 : IResampler {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 2;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => PrefilterInfo.OMoms3;

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
    => callback.Invoke(new OMomsKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, OMomsType.OMoms3));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static OMoms3 Default => new();
}

#endregion

#region OMoms5

/// <summary>
/// Quintic o-Moms resampler.
/// </summary>
/// <remarks>
/// <para>
/// Fifth-degree optimal moments interpolation with radius 3.
/// Provides higher approximation order than OMoms3.
/// </para>
/// <para>
/// <b>Note:</b> For proper interpolation, this filter requires prefiltering
/// the source image with <see cref="PrefilterInfo.OMoms5"/>.
/// </para>
/// </remarks>
[ScalerInfo("o-Moms 5", Year = 2001,
  Description = "Quintic o-Moms interpolation", Category = ScalerCategory.Resampler)]
public readonly struct OMoms5 : IResampler {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 3;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => PrefilterInfo.OMoms5;

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
    => callback.Invoke(new OMomsKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, OMomsType.OMoms5));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static OMoms5 Default => new();
}

#endregion

#region OMoms7

/// <summary>
/// Septic o-Moms resampler.
/// </summary>
/// <remarks>
/// <para>
/// Seventh-degree optimal moments interpolation with radius 4.
/// Provides the highest approximation order in this family.
/// </para>
/// <para>
/// <b>Note:</b> For proper interpolation, this filter requires prefiltering
/// the source image with <see cref="PrefilterInfo.OMoms7"/>.
/// </para>
/// </remarks>
[ScalerInfo("o-Moms 7", Year = 2001,
  Description = "Septic o-Moms interpolation", Category = ScalerCategory.Resampler)]
public readonly struct OMoms7 : IResampler {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 4;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => PrefilterInfo.OMoms7;

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
    => callback.Invoke(new OMomsKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, OMomsType.OMoms7));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static OMoms7 Default => new();
}

#endregion

#region Shared Kernel Infrastructure

file enum OMomsType {
  OMoms3,
  OMoms5,
  OMoms7
}

file readonly struct OMomsKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, OMomsType type)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => type switch {
    OMomsType.OMoms3 => 2,
    OMomsType.OMoms5 => 3,
    OMomsType.OMoms7 => 4,
    _ => 2
  };

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
    // Map destination pixel center back to source coordinates
    var srcXf = (destX + 0.5f) * this._scaleX - 0.5f;
    var srcYf = (destY + 0.5f) * this._scaleY - 0.5f;

    // Integer coordinates
    var srcXi = (int)MathF.Floor(srcXf);
    var srcYi = (int)MathF.Floor(srcYf);

    // Fractional parts
    var fx = srcXf - srcXi;
    var fy = srcYf - srcYi;

    // Accumulate weighted colors from kernel
    Accum4F<TWork> acc = default;
    var r = this.Radius;
    for (var ky = -r + 1; ky <= r; ++ky)
    for (var kx = -r + 1; kx <= r; ++kx) {
      var weight = this.Weight(fx - kx) * this.Weight(fy - ky);
      if (weight == 0f)
        continue;

      var pixel = frame[srcXi + kx, srcYi + ky].Work;
      acc.AddMul(pixel, weight);
    }

    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
  }

  /// <summary>
  /// Computes the o-Moms weight for a given distance.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float Weight(float x) => type switch {
    OMomsType.OMoms3 => OMoms3Weight(x),
    OMomsType.OMoms5 => OMoms5Weight(x),
    OMomsType.OMoms7 => OMoms7Weight(x),
    _ => 0f
  };

  /// <summary>
  /// Cubic o-Moms weight function.
  /// </summary>
  /// <remarks>
  /// Based on "Interpolation Revisited" by Thévenaz, Blu, and Unser.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float OMoms3Weight(float x) {
    x = MathF.Abs(x);
    if (x < 1f) {
      // ((x/2 - 1) * x + 1/14) * x + 13/21
      return ((x * 0.5f - 1f) * x + 1f / 14f) * x + 13f / 21f;
    }

    if (x < 2f) {
      // ((-x/6 + 1) * x - 85/42) * x + 29/21
      return ((-x / 6f + 1f) * x - 85f / 42f) * x + 29f / 21f;
    }

    return 0f;
  }

  /// <summary>
  /// Quintic o-Moms weight function.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float OMoms5Weight(float x) {
    x = MathF.Abs(x);
    if (x <= 1f) {
      var x2 = x * x;
      // (1/12 * x^4 - 1/6 * x^3 - 1/4 * x^2 + 1/2 * x + 1/12) adjusted for o-Moms
      // From Blu-Thévenaz: specific polynomial coefficients for optimal moments
      return 11f / 16f + x2 * (-13f / 24f + x2 * (11f / 48f - x * (1f / 12f)));
    }

    if (x < 2f) {
      var t = 2f - x;
      var t2 = t * t;
      var t3 = t2 * t;
      var t4 = t2 * t2;
      // OMoms5 for region [1,2)
      return 1f / 48f + t * (3f / 16f + t * (1f / 8f + t * (-1f / 8f + t * (1f / 16f - t * (1f / 48f)))));
    }

    if (x < 3f) {
      var t = 3f - x;
      var t2 = t * t;
      var t4 = t2 * t2;
      // OMoms5 for region [2,3)
      return t4 * t * (1f / 120f);
    }

    return 0f;
  }

  /// <summary>
  /// Septic o-Moms weight function.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float OMoms7Weight(float x) {
    x = MathF.Abs(x);
    if (x <= 1f) {
      var x2 = x * x;
      var x4 = x2 * x2;
      // OMoms7 central region
      return 151f / 315f + x2 * (-1f / 3f + x2 * (1f / 9f + x2 * (-1f / 36f + x * (1f / 144f))));
    }

    if (x < 2f) {
      // OMoms7 for region [1,2)
      var t = 2f - x;
      var t2 = t * t;
      var t3 = t2 * t;
      var t4 = t2 * t2;
      var t5 = t4 * t;
      var t6 = t3 * t3;
      return 1f / 10080f + t * (1f / 720f + t * (1f / 180f + t * (-1f / 180f + t * (1f / 144f + t * (-1f / 240f + t * (1f / 720f - t * (1f / 5040f)))))));
    }

    if (x < 3f) {
      // OMoms7 for region [2,3)
      var t = 3f - x;
      var t2 = t * t;
      var t4 = t2 * t2;
      var t6 = t4 * t2;
      return t6 * t * (1f / 5040f) + t6 * (1f / 720f);
    }

    if (x < 4f) {
      // OMoms7 for region [3,4)
      var t = 4f - x;
      var t2 = t * t;
      var t4 = t2 * t2;
      return t4 * t2 * t * (1f / 5040f);
    }

    return 0f;
  }
}

#endregion
