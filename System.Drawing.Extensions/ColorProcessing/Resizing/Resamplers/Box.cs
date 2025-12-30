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
/// Box filter resampler (simple averaging).
/// </summary>
/// <remarks>
/// <para>Simple averaging filter with uniform weights within the radius.</para>
/// <para>Particularly useful for downscaling where it acts as an anti-aliasing filter.</para>
/// </remarks>
[ScalerInfo("Box", Description = "Box/averaging filter for simple interpolation", Category = ScalerCategory.Resampler)]
public readonly struct Box : IResampler {

  private readonly int _radius;

  /// <summary>
  /// Creates a Box resampler with radius 1 (default, 2x2 kernel).
  /// </summary>
  public Box() : this(1) { }

  /// <summary>
  /// Creates a Box resampler with custom radius.
  /// </summary>
  /// <param name="radius">The filter radius.</param>
  public Box(int radius) {
    ArgumentOutOfRangeException.ThrowIfLessThan(radius, 1);
    this._radius = radius;
  }

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => this._radius == 0 ? 1 : this._radius;

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
    => callback.Invoke(new BoxKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.Radius));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Box Default => new();
}

file readonly struct BoxKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, int radius)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => radius;
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

    // Accumulate weighted colors
    Accum4F<TWork> acc = default;
    for (var ky = -radius + 1; ky <= radius; ++ky)
    for (var kx = -radius + 1; kx <= radius; ++kx) {
      var weight = BoxWeight(fx - kx) * BoxWeight(fy - ky);
      if (weight == 0f)
        continue;

      var pixel = frame[srcXi + kx, srcYi + ky].Work;
      acc.AddMul(pixel, weight);
    }

    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
  }

  /// <summary>
  /// Box weight function: 1 within [-0.5, 0.5], 0 otherwise.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float BoxWeight(float x) {
    x = MathF.Abs(x);
    return x <= 0.5f ? 1f : 0f;
  }
}
