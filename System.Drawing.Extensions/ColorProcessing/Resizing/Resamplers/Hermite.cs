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
/// Hermite resampler - cubic interpolation with smooth falloff.
/// </summary>
/// <remarks>
/// <para>Uses cubic Hermite polynomial for smooth interpolation.</para>
/// <para>Produces smoother results than bilinear without the ringing of higher-order filters.</para>
/// </remarks>
[ScalerInfo("Hermite", Author = "Charles Hermite", Year = 1878,
  Description = "Cubic interpolation with smooth falloff", Category = ScalerCategory.Resampler)]
public readonly struct Hermite : IResampler {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 1;

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
    => callback.Invoke(new HermiteKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Hermite Default => new();
}

file readonly struct HermiteKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, bool useCenteredGrid)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => 1;
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

    // Integer coordinates of top-left source pixel
    var x0 = (int)MathF.Floor(srcXf);
    var y0 = (int)MathF.Floor(srcYf);

    // Fractional parts for interpolation weights
    var fx = srcXf - x0;
    var fy = srcYf - y0;

    // Hermite weights (smooth step function)
    var hx0 = HermiteWeight(-fx);
    var hx1 = HermiteWeight(1f - fx);
    var hy0 = HermiteWeight(-fy);
    var hy1 = HermiteWeight(1f - fy);

    // Get 4 neighboring pixels
    var p00 = frame[x0, y0].Work;
    var p10 = frame[x0 + 1, y0].Work;
    var p01 = frame[x0, y0 + 1].Work;
    var p11 = frame[x0 + 1, y0 + 1].Work;

    // Compute combined weights
    var w00 = hx0 * hy0;
    var w10 = hx1 * hy0;
    var w01 = hx0 * hy1;
    var w11 = hx1 * hy1;

    // Accumulate weighted colors
    Accum4F<TWork> acc = default;
    acc.AddMul(p00, w00);
    acc.AddMul(p10, w10);
    acc.AddMul(p01, w01);
    acc.AddMul(p11, w11);

    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
  }

  /// <summary>
  /// Computes the Hermite weight for a given distance.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float HermiteWeight(float x) {
    x = MathF.Abs(x);
    if (x < 1f)
      return (2f * x - 3f) * x * x + 1f;
    return 0f;
  }
}
