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
    int targetHeight,
    bool useCenteredGrid = true)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new LagrangeKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, LagrangeType.Lagrange3, useCenteredGrid));

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
    int targetHeight,
    bool useCenteredGrid = true)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new LagrangeKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, LagrangeType.Lagrange5, useCenteredGrid));

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
    int targetHeight,
    bool useCenteredGrid = true)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new LagrangeKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, LagrangeType.Lagrange7, useCenteredGrid));

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
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, LagrangeType type, bool useCenteredGrid)
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

  // Precomputed scale factors and offsets for zero-cost grid centering
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
    // Map destination pixel back to source coordinates
    var srcXf = destX * this._scaleX + this._offsetX;
    var srcYf = destY * this._scaleY + this._offsetY;

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
  /// Sample points at -1, 0, 1, 2 relative to floor(srcX).
  /// For fractional position t in [0, 1]:
  /// L_{-1}(t) = -t(t-1)(t-2)/6
  /// L_0(t) = (t+1)(t-1)(t-2)/2
  /// L_1(t) = -(t+1)t(t-2)/2
  /// L_2(t) = (t+1)t(t-1)/6
  ///
  /// The weight function receives x = fx - kx (offset from interpolation point to sample).
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float Lagrange3Weight(float x) {
    if (x >= 2f || x < -2f)
      return 0f;

    if (x >= 1f) {
      // x ∈ [1, 2): sample at index -1, L_{-1}(fx) where fx = x - 1
      var t = x - 1f;
      return -t * (t - 1f) * (t - 2f) / 6f;
    }

    if (x >= 0f) {
      // x ∈ [0, 1): sample at index 0, L_0(fx) where fx = x
      return (x + 1f) * (x - 1f) * (x - 2f) / 2f;
    }

    if (x >= -1f) {
      // x ∈ [-1, 0): sample at index 1, L_1(fx) where fx = x + 1
      var t = x + 1f;
      return -(t + 1f) * t * (t - 2f) / 2f;
    }

    // x ∈ [-2, -1): sample at index 2, L_2(fx) where fx = x + 2
    var u = x + 2f;
    return (u + 1f) * u * (u - 1f) / 6f;
  }

  /// <summary>
  /// Lagrange 6-point (quintic) basis polynomial weights.
  /// </summary>
  /// <remarks>
  /// Sample points at -2, -1, 0, 1, 2, 3 relative to floor(srcX).
  /// The weight function receives x = fx - kx (offset from interpolation point to sample).
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float Lagrange5Weight(float x) {
    if (x >= 3f || x < -3f)
      return 0f;

    if (x >= 2f) {
      // x ∈ [2, 3): sample at index -2, L_{-2}(fx) where fx = x - 2
      var t = x - 2f;
      return -(t + 1f) * t * (t - 1f) * (t - 2f) * (t - 3f) / 120f;
    }

    if (x >= 1f) {
      // x ∈ [1, 2): sample at index -1, L_{-1}(fx) where fx = x - 1
      var t = x - 1f;
      return -(t + 2f) * t * (t - 1f) * (t - 2f) * (t - 3f) / 24f;
    }

    if (x >= 0f) {
      // x ∈ [0, 1): sample at index 0, L_0(fx) where fx = x
      return (x + 2f) * (x + 1f) * (x - 1f) * (x - 2f) * (x - 3f) / 12f;
    }

    if (x >= -1f) {
      // x ∈ [-1, 0): sample at index 1, L_1(fx) where fx = x + 1
      var t = x + 1f;
      return -(t + 2f) * (t + 1f) * t * (t - 2f) * (t - 3f) / 12f;
    }

    if (x >= -2f) {
      // x ∈ [-2, -1): sample at index 2, L_2(fx) where fx = x + 2
      var t = x + 2f;
      return -(t + 2f) * (t + 1f) * t * (t - 1f) * (t - 3f) / 24f;
    }

    // x ∈ [-3, -2): sample at index 3, L_3(fx) where fx = x + 3
    var u = x + 3f;
    return (u + 2f) * (u + 1f) * u * (u - 1f) * (u - 2f) / 120f;
  }

  /// <summary>
  /// Lagrange 8-point (septic) basis polynomial weights.
  /// </summary>
  /// <remarks>
  /// Sample points at -3, -2, -1, 0, 1, 2, 3, 4 relative to floor(srcX).
  /// The weight function receives x = fx - kx (offset from interpolation point to sample).
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float Lagrange7Weight(float x) {
    if (x >= 4f || x < -4f)
      return 0f;

    if (x >= 3f) {
      // x ∈ [3, 4): sample at index -3, L_{-3}(fx) where fx = x - 3
      var t = x - 3f;
      return -(t + 2f) * (t + 1f) * t * (t - 1f) * (t - 2f) * (t - 3f) * (t - 4f) / 5040f;
    }

    if (x >= 2f) {
      // x ∈ [2, 3): sample at index -2, L_{-2}(fx) where fx = x - 2
      var t = x - 2f;
      return (t + 3f) * (t + 1f) * t * (t - 1f) * (t - 2f) * (t - 3f) * (t - 4f) / 720f;
    }

    if (x >= 1f) {
      // x ∈ [1, 2): sample at index -1, L_{-1}(fx) where fx = x - 1
      var t = x - 1f;
      return -(t + 3f) * (t + 2f) * t * (t - 1f) * (t - 2f) * (t - 3f) * (t - 4f) / 240f;
    }

    if (x >= 0f) {
      // x ∈ [0, 1): sample at index 0, L_0(fx) where fx = x
      return (x + 3f) * (x + 2f) * (x + 1f) * (x - 1f) * (x - 2f) * (x - 3f) * (x - 4f) / 144f;
    }

    if (x >= -1f) {
      // x ∈ [-1, 0): sample at index 1, L_1(fx) where fx = x + 1
      var t = x + 1f;
      return -(t + 3f) * (t + 2f) * (t + 1f) * t * (t - 2f) * (t - 3f) * (t - 4f) / 144f;
    }

    if (x >= -2f) {
      // x ∈ [-2, -1): sample at index 2, L_2(fx) where fx = x + 2
      var t = x + 2f;
      return -(t + 3f) * (t + 2f) * (t + 1f) * t * (t - 1f) * (t - 3f) * (t - 4f) / 240f;
    }

    if (x >= -3f) {
      // x ∈ [-3, -2): sample at index 3, L_3(fx) where fx = x + 3
      var t = x + 3f;
      return -(t + 3f) * (t + 2f) * (t + 1f) * t * (t - 1f) * (t - 2f) * (t - 4f) / 720f;
    }

    // x ∈ [-4, -3): sample at index 4, L_4(fx) where fx = x + 4
    var u = x + 4f;
    return (u + 3f) * (u + 2f) * (u + 1f) * u * (u - 1f) * (u - 2f) * (u - 3f) / 5040f;
  }
}
