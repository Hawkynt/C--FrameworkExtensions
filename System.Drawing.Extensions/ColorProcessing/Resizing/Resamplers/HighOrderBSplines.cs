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

#region BSpline2

/// <summary>
/// Quadratic B-spline resampler (degree 2).
/// </summary>
/// <remarks>
/// <para>
/// Uses quadratic B-spline basis function with radius 1.5.
/// Provides smooth interpolation with moderate blurring.
/// </para>
/// <para>
/// <b>Note:</b> For proper interpolation, this filter requires prefiltering
/// the source image with <see cref="PrefilterInfo.BSpline2"/>.
/// </para>
/// </remarks>
[ScalerInfo("B-Spline 2", Year = 1978,
  Description = "Quadratic B-spline interpolation (degree 2)", Category = ScalerCategory.Resampler)]
public readonly struct BSpline2 : IResampler {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 2; // ceil(1.5) = 2

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => PrefilterInfo.BSpline2;

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
    => callback.Invoke(new BSplineKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, BSplineType.BSpline2));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static BSpline2 Default => new();
}

#endregion

#region BSpline5

/// <summary>
/// Quintic B-spline resampler (degree 5).
/// </summary>
/// <remarks>
/// <para>
/// Uses quintic B-spline basis function with radius 3.
/// Provides very smooth interpolation.
/// </para>
/// <para>
/// <b>Note:</b> For proper interpolation, this filter requires prefiltering
/// the source image with <see cref="PrefilterInfo.BSpline5"/>.
/// </para>
/// </remarks>
[ScalerInfo("B-Spline 5", Year = 1978,
  Description = "Quintic B-spline interpolation (degree 5)", Category = ScalerCategory.Resampler)]
public readonly struct BSpline5 : IResampler {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 3;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => PrefilterInfo.BSpline5;

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
    => callback.Invoke(new BSplineKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, BSplineType.BSpline5));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static BSpline5 Default => new();
}

#endregion

#region BSpline7

/// <summary>
/// Septic B-spline resampler (degree 7).
/// </summary>
/// <remarks>
/// <para>
/// Uses septic B-spline basis function with radius 4.
/// Provides extremely smooth interpolation.
/// </para>
/// <para>
/// <b>Note:</b> For proper interpolation, this filter requires prefiltering
/// the source image with <see cref="PrefilterInfo.BSpline7"/>.
/// </para>
/// </remarks>
[ScalerInfo("B-Spline 7", Year = 1978,
  Description = "Septic B-spline interpolation (degree 7)", Category = ScalerCategory.Resampler)]
public readonly struct BSpline7 : IResampler {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 4;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => PrefilterInfo.BSpline7;

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
    => callback.Invoke(new BSplineKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, BSplineType.BSpline7));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static BSpline7 Default => new();
}

#endregion

#region BSpline9

/// <summary>
/// Nonic B-spline resampler (degree 9).
/// </summary>
/// <remarks>
/// <para>
/// Uses nonic B-spline basis function with radius 5.
/// Provides very high smoothness for mathematical approximation.
/// </para>
/// <para>
/// <b>Note:</b> For proper interpolation, this filter requires prefiltering
/// the source image with <see cref="PrefilterInfo.BSpline9"/>.
/// </para>
/// </remarks>
[ScalerInfo("B-Spline 9", Year = 1978,
  Description = "Nonic B-spline interpolation (degree 9)", Category = ScalerCategory.Resampler)]
public readonly struct BSpline9 : IResampler {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 5;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => PrefilterInfo.BSpline9;

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
    => callback.Invoke(new BSplineKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, BSplineType.BSpline9));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static BSpline9 Default => new();
}

#endregion

#region BSpline11

/// <summary>
/// 11th-degree B-spline resampler.
/// </summary>
/// <remarks>
/// <para>
/// Uses 11th-degree B-spline basis function with radius 6.
/// Provides the highest smoothness in this family.
/// </para>
/// <para>
/// <b>Note:</b> For proper interpolation, this filter requires prefiltering
/// the source image with <see cref="PrefilterInfo.BSpline11"/>.
/// </para>
/// </remarks>
[ScalerInfo("B-Spline 11", Year = 1978,
  Description = "11th-degree B-spline interpolation", Category = ScalerCategory.Resampler)]
public readonly struct BSpline11 : IResampler {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 6;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => PrefilterInfo.BSpline11;

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
    => callback.Invoke(new BSplineKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, BSplineType.BSpline11));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static BSpline11 Default => new();
}

#endregion

#region Shared Kernel Infrastructure

file enum BSplineType {
  BSpline2,
  BSpline5,
  BSpline7,
  BSpline9,
  BSpline11
}

file readonly struct BSplineKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, BSplineType type)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => type switch {
    BSplineType.BSpline2 => 2,
    BSplineType.BSpline5 => 3,
    BSplineType.BSpline7 => 4,
    BSplineType.BSpline9 => 5,
    BSplineType.BSpline11 => 6,
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
  /// Computes the B-spline weight for a given distance.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float Weight(float x) => type switch {
    BSplineType.BSpline2 => BSpline2Weight(x),
    BSplineType.BSpline5 => BSpline5Weight(x),
    BSplineType.BSpline7 => BSpline7Weight(x),
    BSplineType.BSpline9 => BSpline9Weight(x),
    BSplineType.BSpline11 => BSpline11Weight(x),
    _ => 0f
  };

  /// <summary>
  /// Quadratic B-spline (degree 2) weight.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float BSpline2Weight(float x) {
    x = MathF.Abs(x);
    if (x <= 0.5f)
      return 0.75f - x * x;
    if (x < 1.5f) {
      var t = 1.5f - x;
      return t * t * 0.5f;
    }

    return 0f;
  }

  /// <summary>
  /// Quintic B-spline (degree 5) weight.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float BSpline5Weight(float x) {
    x = MathF.Abs(x);
    if (x <= 1f) {
      var x2 = x * x;
      return 0.55f + x2 * (-0.5f + x2 * (0.25f - x * (1f / 12f)));
    }

    if (x < 2f) {
      var t = 2f - x;
      var t2 = t * t;
      return t2 * t2 * (t * (1f / 24f) + 1f / 24f);
    }

    if (x < 3f) {
      var t = 3f - x;
      var t2 = t * t;
      return t2 * t2 * t * (1f / 120f);
    }

    return 0f;
  }

  /// <summary>
  /// Septic B-spline (degree 7) weight.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float BSpline7Weight(float x) {
    x = MathF.Abs(x);
    if (x <= 1f) {
      var x2 = x * x;
      var x4 = x2 * x2;
      return 151f / 315f + x2 * (-1f / 3f + x2 * (1f / 9f + x2 * (-1f / 36f + x * (1f / 144f))));
    }

    if (x < 2f) {
      var t = 2f - x;
      var t2 = t * t;
      var t4 = t2 * t2;
      return t4 * t2 * (t * (1f / 720f) + 1f / 720f + 1f / 240f);
    }

    if (x < 3f) {
      var t = 3f - x;
      var t2 = t * t;
      var t4 = t2 * t2;
      return t4 * t2 * t * (1f / 5040f) + t4 * t2 * (1f / 720f);
    }

    if (x < 4f) {
      var t = 4f - x;
      var t2 = t * t;
      var t4 = t2 * t2;
      return t4 * t2 * t * (1f / 5040f);
    }

    return 0f;
  }

  /// <summary>
  /// Nonic B-spline (degree 9) weight.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float BSpline9Weight(float x) {
    x = MathF.Abs(x);
    var x2 = x * x;
    if (x <= 1f) {
      var x4 = x2 * x2;
      return 35f / 72f + x2 * (-5f / 18f + x2 * (5f / 72f + x4 * (-5f / 504f + x * (1f / 1008f))));
    }

    if (x < 2f) {
      var t = 2f - x;
      var t2 = t * t;
      var t4 = t2 * t2;
      var t8 = t4 * t4;
      return t8 * (t * (1f / 40320f) + 1f / 4480f);
    }

    if (x < 3f) {
      var t = 3f - x;
      var t2 = t * t;
      var t4 = t2 * t2;
      var t8 = t4 * t4;
      return t8 * t * (1f / 362880f) + t8 * (1f / 40320f);
    }

    if (x < 4f) {
      var t = 4f - x;
      var t2 = t * t;
      var t4 = t2 * t2;
      var t8 = t4 * t4;
      return t8 * t * (1f / 362880f);
    }

    if (x < 5f) {
      var t = 5f - x;
      var t2 = t * t;
      var t4 = t2 * t2;
      var t8 = t4 * t4;
      return t8 * t * (1f / 3628800f);
    }

    return 0f;
  }

  /// <summary>
  /// 11th-degree B-spline weight.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float BSpline11Weight(float x) {
    x = MathF.Abs(x);
    var x2 = x * x;
    if (x <= 1f) {
      var x4 = x2 * x2;
      var x6 = x4 * x2;
      return 655177f / 1663200f + x2 * (-7f / 33f + x2 * (7f / 132f + x6 * (-7f / 3960f + x * (1f / 7920f))));
    }

    if (x < 2f) {
      var t = 2f - x;
      return _Pow11(t) * (1f / 39916800f) + _Pow10(t) * (1f / 3628800f);
    }

    if (x < 3f) {
      var t = 3f - x;
      return _Pow11(t) * (1f / 39916800f);
    }

    if (x < 4f) {
      var t = 4f - x;
      return _Pow11(t) * (1f / 39916800f);
    }

    if (x < 5f) {
      var t = 5f - x;
      return _Pow11(t) * (1f / 479001600f);
    }

    if (x < 6f) {
      var t = 6f - x;
      return _Pow11(t) * (1f / 479001600f);
    }

    return 0f;
  }

  /// <summary>Helper for x^10.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Pow10(float x) {
    var x2 = x * x;
    var x4 = x2 * x2;
    return x4 * x4 * x2;
  }

  /// <summary>Helper for x^11.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Pow11(float x) {
    var x2 = x * x;
    var x4 = x2 * x2;
    return x4 * x4 * x2 * x;
  }
}

#endregion
