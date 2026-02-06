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
/// Advanced Anti-Aliasing resampler by guest(r).
/// </summary>
/// <remarks>
/// <para>Algorithm: Advanced AA by guest(r) (2006)</para>
/// <para>Reference: https://github.com/libretro/glsl-shaders/tree/master/anti-aliasing/shaders/advanced-aa</para>
/// <para>Uses Manhattan RGB distance on a 3x3 neighborhood to compute edge weights.
/// Computes axis (horizontal/vertical) and diagonal interpolation candidates,
/// weighted by their deviation from center, then blends the two results.</para>
/// </remarks>
[ScalerInfo("Advanced AA", Author = "guest(r)", Year = 2006,
  Url = "https://github.com/libretro/glsl-shaders/tree/master/anti-aliasing/shaders/advanced-aa",
  Description = "Edge-weighted anti-aliasing interpolation", Category = ScalerCategory.Resampler)]
public readonly struct AdvancedAa : IResampler {

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
    => callback.Invoke(new AdvancedAaKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static AdvancedAa Default => new();
}

file readonly struct AdvancedAaKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, bool useCenteredGrid)
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

    var ix = (int)MathF.Floor(srcXf);
    var iy = (int)MathF.Floor(srcYf);

    // Get 3x3 neighborhood
    var c00 = frame[ix - 1, iy - 1].Work;
    var c10 = frame[ix, iy - 1].Work;
    var c20 = frame[ix + 1, iy - 1].Work;
    var c01 = frame[ix - 1, iy].Work;
    var c11 = frame[ix, iy].Work;
    var c21 = frame[ix + 1, iy].Work;
    var c02 = frame[ix - 1, iy + 1].Work;
    var c12 = frame[ix, iy + 1].Work;
    var c22 = frame[ix + 1, iy + 1].Work;

    // Calculate edge weights using Manhattan channel distance
    var d1 = ManhattanDistance(c00, c22) + 0.0001f; // diagonal 1
    var d2 = ManhattanDistance(c20, c02) + 0.0001f; // diagonal 2
    var hl = ManhattanDistance(c01, c21) + 0.0001f; // horizontal
    var vl = ManhattanDistance(c10, c12) + 0.0001f; // vertical

    var k1 = 0.5f * (hl + vl);
    var k2 = 0.5f * (d1 + d2);

    // Axis-aligned interpolation (horizontal + vertical weighted)
    Accum4F<TWork> t1Acc = default;
    t1Acc.AddMul(c10, hl);
    t1Acc.AddMul(c12, hl);
    t1Acc.AddMul(c01, vl);
    t1Acc.AddMul(c21, vl);
    t1Acc.AddMul(c11, k1);
    var t1 = t1Acc.Result;

    // Diagonal interpolation
    Accum4F<TWork> t2Acc = default;
    t2Acc.AddMul(c20, d1);
    t2Acc.AddMul(c02, d1);
    t2Acc.AddMul(c00, d2);
    t2Acc.AddMul(c22, d2);
    t2Acc.AddMul(c11, k2);
    var t2 = t2Acc.Result;

    // Calculate deviation weights for final blend
    var dev1 = ChannelDeviation(t1, c11);
    var dev2 = ChannelDeviation(t2, c11);

    // Final weighted blend: more weight to result closer to center
    Accum4F<TWork> finalAcc = default;
    finalAcc.AddMul(t2, dev1);
    finalAcc.AddMul(t1, dev2);
    var result = finalAcc.Result;

    dest[destY * destStride + destX] = encoder.Encode(result);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float ManhattanDistance(in TWork a, in TWork b)
    => MathF.Abs(ColorConverter.GetLuminance(a) - ColorConverter.GetLuminance(b));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float ChannelDeviation(in TWork a, in TWork center)
    => MathF.Abs(ColorConverter.GetLuminance(a) - ColorConverter.GetLuminance(center)) + 0.0001f;
}
