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
/// Lagrange-3 polynomial interpolation resampler (4 sample points, cubic).
/// </summary>
/// <remarks>
/// <para>
/// Uses Lagrange polynomial interpolation with 4 sample points (indices -1, 0, 1, 2).
/// Produces a cubic polynomial passing through all sample points.
/// </para>
/// <para>
/// Unlike B-splines, Lagrange interpolation passes exactly through sample points,
/// making it suitable for applications requiring exact value preservation.
/// </para>
/// </remarks>
[ScalerInfo("Lagrange-3", Author = "Joseph-Louis Lagrange", Year = 1795,
  Description = "Cubic polynomial interpolation through 4 points", Category = ScalerCategory.Resampler)]
public readonly struct Lagrange3 : IResampler {

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
    => callback.Invoke(new LagrangeKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, LagrangeType.Lagrange3));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Lagrange3 Default => new();
}

/// <summary>
/// Lagrange-5 polynomial interpolation resampler (6 sample points, quintic).
/// </summary>
/// <remarks>
/// <para>
/// Uses Lagrange polynomial interpolation with 6 sample points (indices -2, -1, 0, 1, 2, 3).
/// Produces a quintic polynomial passing through all sample points.
/// </para>
/// <para>
/// Provides sharper results than Lagrange-3 but may exhibit more ringing
/// near high-contrast edges.
/// </para>
/// </remarks>
[ScalerInfo("Lagrange-5", Author = "Joseph-Louis Lagrange", Year = 1795,
  Description = "Quintic polynomial interpolation through 6 points", Category = ScalerCategory.Resampler)]
public readonly struct Lagrange5 : IResampler {

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
    => callback.Invoke(new LagrangeKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, LagrangeType.Lagrange5));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Lagrange5 Default => new();
}

/// <summary>
/// Lagrange-7 polynomial interpolation resampler (8 sample points, septic).
/// </summary>
/// <remarks>
/// <para>
/// Uses Lagrange polynomial interpolation with 8 sample points.
/// Produces a septic polynomial passing through all sample points.
/// </para>
/// <para>
/// Highest-quality Lagrange variant but with potential for significant
/// ringing artifacts. Best for smooth, low-contrast images.
/// </para>
/// </remarks>
[ScalerInfo("Lagrange-7", Author = "Joseph-Louis Lagrange", Year = 1795,
  Description = "Septic polynomial interpolation through 8 points", Category = ScalerCategory.Resampler)]
public readonly struct Lagrange7 : IResampler {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 4;

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
    => callback.Invoke(new LagrangeKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, LagrangeType.Lagrange7));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Lagrange7 Default => new();
}

file enum LagrangeType {
  Lagrange3,
  Lagrange5,
  Lagrange7
}

file readonly struct LagrangeKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, LagrangeType type)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => type switch {
    LagrangeType.Lagrange3 => 2,
    LagrangeType.Lagrange5 => 3,
    LagrangeType.Lagrange7 => 4,
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

    // Accumulate weighted colors based on Lagrange type
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
  /// Computes the Lagrange interpolation weight for a given distance.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float Weight(float x) => type switch {
    LagrangeType.Lagrange3 => Lagrange3Weight(x),
    LagrangeType.Lagrange5 => Lagrange5Weight(x),
    LagrangeType.Lagrange7 => Lagrange7Weight(x),
    _ => 0f
  };

  /// <summary>
  /// Lagrange 4-point (cubic) basis polynomial weights.
  /// </summary>
  /// <remarks>
  /// Sample points at -1, 0, 1, 2. For fractional position t in [0, 1]:
  /// L_{-1}(t) = -t(t-1)(t-2)/6
  /// L_0(t) = (t+1)(t-1)(t-2)/2
  /// L_1(t) = -(t+1)t(t-2)/2
  /// L_2(t) = (t+1)t(t-1)/6
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float Lagrange3Weight(float x) {
    // x is distance from sample point, so we compute based on that
    var absX = MathF.Abs(x);
    if (absX >= 2f)
      return 0f;

    // Lagrange basis for 4-point interpolation centered at x=0
    // with sample points at -1, 0, 1, 2
    var t = x;
    return t switch {
      >= -1f and < 0f => -(t + 1f) * t * (t - 1f) * (t - 2f) / 6f * -1f, // Adjusted
      >= 0f and < 1f => (t + 1f) * t * (t - 1f) * (t - 2f) / -2f * -1f,
      _ => ComputeLagrange3Basis(x)
    };
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float ComputeLagrange3Basis(float x) {
    // For sample point at offset i, the basis polynomial at position t is:
    // L_i(t) = ∏_{j≠i} (t - j) / (i - j)
    // Our sample points: -1, 0, 1, 2
    // We want the weight for a sample that is 'x' away from interpolation point

    var absX = MathF.Abs(x);
    if (absX >= 2f)
      return 0f;

    // Centered cubic Lagrange using symmetric formulation
    // For |x| <= 2, with sample points at -1, 0, 1, 2
    if (absX <= 1f) {
      var x2 = x * x;
      // Central cubic: passes through (0,1), approaches 0 at ±2
      return 1f - x2 * (2.5f - 1.5f * absX);
    }

    // |x| in (1, 2)
    var t = 2f - absX;
    return t * (t - 1f) * (t + 1f) * (x > 0 ? -1f : 1f) / 6f;
  }

  /// <summary>
  /// Lagrange 6-point (quintic) basis polynomial weights.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float Lagrange5Weight(float x) {
    var absX = MathF.Abs(x);
    if (absX >= 3f)
      return 0f;

    // 6-point Lagrange with samples at -2, -1, 0, 1, 2, 3
    // Using the general Lagrange basis formula
    var x2 = x * x;
    if (absX <= 1f) {
      // Central region: highest weight
      var x4 = x2 * x2;
      return 1f + x2 * (-2.95833333333333f + x2 * (2.29166666666667f - absX * 0.625f));
    }

    if (absX <= 2f) {
      // Intermediate region
      var t = absX - 1.5f;
      var t2 = t * t;
      return 0.125f * (absX - 3f) * (absX + 2f) * t * (t2 - 0.25f) / (x > 0 ? -1f : 1f);
    }

    // Outer region |x| in (2, 3)
    var u = 3f - absX;
    var u2 = u * u;
    return u * u2 * (u - 1f) * (u - 2f) * (x > 0 ? 1f : -1f) / 120f;
  }

  /// <summary>
  /// Lagrange 8-point (septic) basis polynomial weights.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float Lagrange7Weight(float x) {
    var absX = MathF.Abs(x);
    if (absX >= 4f)
      return 0f;

    // 8-point Lagrange with samples at -3, -2, -1, 0, 1, 2, 3, 4
    var x2 = x * x;
    if (absX <= 1f) {
      // Central region
      var x4 = x2 * x2;
      var x6 = x4 * x2;
      return 1f + x2 * (-3.63888888888889f + x2 * (4.33333333333333f + x2 * (-1.97222222222222f + absX * 0.2708333333333f)));
    }

    if (absX <= 2f) {
      // Inner-intermediate region
      var t = absX - 1f;
      return ComputeLagrange7Intermediate1(t, x > 0);
    }

    if (absX <= 3f) {
      // Outer-intermediate region
      var t = absX - 2f;
      return ComputeLagrange7Intermediate2(t, x > 0);
    }

    // Outer region |x| in (3, 4)
    var u = 4f - absX;
    var sign = x > 0 ? -1f : 1f;
    return sign * u * (u - 1f) * (u - 2f) * (u - 3f) * (u + 1f) * (u + 2f) * (u + 3f) / 5040f;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float ComputeLagrange7Intermediate1(float t, bool positive) {
    // t = |x| - 1, so t in [0, 1)
    var t2 = t * t;
    var t3 = t2 * t;
    var sign = positive ? -1f : 1f;
    return sign * t * (t - 1f) * (t2 - 4f) * (t2 - 9f) / 36f;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float ComputeLagrange7Intermediate2(float t, bool positive) {
    // t = |x| - 2, so t in [0, 1)
    var t2 = t * t;
    var sign = positive ? 1f : -1f;
    return sign * t * (t - 1f) * (t + 1f) * (t2 - 4f) * (t - 2f) / 240f;
  }
}
