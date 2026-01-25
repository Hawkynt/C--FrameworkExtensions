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

#region Schaum2

/// <summary>
/// Schaum2 resampler - quadratic Schaum interpolation.
/// </summary>
/// <remarks>
/// <para>Quadratic interpolation kernel with radius ~1.5.</para>
/// <para>Has discontinuities at x=0.5 and x=1.5.</para>
/// <para>Provides sharper results than bilinear but with some artifacts.</para>
/// </remarks>
[ScalerInfo("Schaum2",
  Description = "Quadratic Schaum interpolation kernel", Category = ScalerCategory.Resampler)]
public readonly struct Schaum2 : IResampler {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 2; // ceil(1.51) = 2

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
    => callback.Invoke(new SchaumKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, SchaumType.Schaum2, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Schaum2 Default => new();
}

#endregion

#region Schaum3

/// <summary>
/// Schaum3 resampler - cubic Schaum interpolation.
/// </summary>
/// <remarks>
/// <para>Cubic interpolation kernel with radius 2.</para>
/// <para>Provides smooth interpolation with good sharpness.</para>
/// <para>Related to Catmull-Rom spline family.</para>
/// </remarks>
[ScalerInfo("Schaum3",
  Description = "Cubic Schaum interpolation kernel", Category = ScalerCategory.Resampler)]
public readonly struct Schaum3 : IResampler {

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
    => callback.Invoke(new SchaumKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, SchaumType.Schaum3, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Schaum3 Default => new();
}

#endregion

#region Shared Kernel Infrastructure

file enum SchaumType {
  Schaum2,
  Schaum3
}

file readonly struct SchaumKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, SchaumType schaumType, bool useCenteredGrid = true)
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

    // Accumulate weighted colors from kernel
    Accum4F<TWork> acc = default;
    for (var ky = -1; ky <= 2; ++ky)
    for (var kx = -1; kx <= 2; ++kx) {
      var weight = this.Weight(fx - kx) * this.Weight(fy - ky);
      if (weight == 0f)
        continue;

      var pixel = frame[srcXi + kx, srcYi + ky].Work;
      acc.AddMul(pixel, weight);
    }

    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
  }

  /// <summary>
  /// Computes the Schaum weight for a given distance.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float Weight(float x) => schaumType switch {
    SchaumType.Schaum2 => Schaum2Weight(x),
    SchaumType.Schaum3 => Schaum3Weight(x),
    _ => 0f
  };

  /// <summary>
  /// Schaum2 (quadratic) weight function.
  /// </summary>
  /// <remarks>
  /// Has discontinuities at x=0.5 and x=1.5.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float Schaum2Weight(float x) {
    x = MathF.Abs(x);
    if (x < 0.5f)
      return 1f - x * x;
    if (x == 0.5f)
      return 0.5625f; // Discontinuity value
    if (x < 1.5f)
      return (x - 3f) * x / 2f + 1f;
    if (x == 1.5f)
      return -0.0625f; // Discontinuity value
    return 0f;
  }

  /// <summary>
  /// Schaum3 (cubic) weight function.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float Schaum3Weight(float x) {
    x = MathF.Abs(x);
    if (x <= 1f)
      return ((x - 2f) * x - 1f) * x / 2f + 1f;
    if (x < 2f)
      return ((-x + 6f) * x - 11f) * x / 6f + 1f;
    return 0f;
  }
}

#endregion
