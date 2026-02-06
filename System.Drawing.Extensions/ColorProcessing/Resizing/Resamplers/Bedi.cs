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
/// Bilinear Edge-Directed Interpolation (BEDI) resampler.
/// </summary>
/// <remarks>
/// <para>A hybrid algorithm that uses standard bilinear interpolation for smooth regions
/// and edge-directed interpolation along detected edges.</para>
/// <para>Algorithm steps:</para>
/// <list type="number">
/// <item>Compute gradient magnitude using Sobel operator</item>
/// <item>Classify region as smooth, transition, or edge based on threshold</item>
/// <item>Smooth: pure bilinear interpolation</item>
/// <item>Edge: interpolate along edge direction, not across it</item>
/// <item>Transition: smoothstep blend between bilinear and edge-directed</item>
/// </list>
/// </remarks>
[ScalerInfo("BEDI", Year = 2010,
  Description = "Bilinear edge-directed interpolation with Sobel gradient classification", Category = ScalerCategory.Resampler)]
public readonly struct Bedi : IResampler {

  private readonly float _edgeThreshold;

  /// <summary>
  /// Creates a BEDI resampler with default edge threshold.
  /// </summary>
  public Bedi() : this(30f) { }

  /// <summary>
  /// Creates a BEDI resampler with custom edge threshold.
  /// </summary>
  /// <param name="edgeThreshold">Edge detection threshold. Lower = more edges detected.</param>
  public Bedi(float edgeThreshold) {
    this._edgeThreshold = edgeThreshold;
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
    int targetHeight,
    bool useCenteredGrid = true)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new BediKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this._edgeThreshold, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Bedi Default => new();
}

file readonly struct BediKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, float edgeThreshold, bool useCenteredGrid)
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

    var x0 = (int)MathF.Floor(srcXf);
    var y0 = (int)MathF.Floor(srcYf);
    var fx = srcXf - x0;
    var fy = srcYf - y0;

    // Sample 2x2 neighborhood for bilinear
    var c00 = frame[x0, y0].Work;
    var c10 = frame[x0 + 1, y0].Work;
    var c01 = frame[x0, y0 + 1].Work;
    var c11 = frame[x0 + 1, y0 + 1].Work;

    // Compute Sobel gradients from 3x3 neighborhood
    var (gradX, gradY) = ComputeSobelGradients(frame, x0, y0);
    var gradMagnitude = MathF.Sqrt(gradX * gradX + gradY * gradY);

    var lowThreshold = edgeThreshold * 0.5f;
    var highThreshold = edgeThreshold * 1.5f;

    TWork result;
    if (gradMagnitude < lowThreshold) {
      // Smooth region: pure bilinear
      result = BilinearInterpolate(c00, c10, c01, c11, fx, fy);
    } else if (gradMagnitude > highThreshold) {
      // Edge region: pure edge-directed
      result = EdgeDirectedInterpolate(c00, c10, c01, c11, fx, fy, gradX, gradY);
    } else {
      // Transition region: smoothstep blend
      var bilinear = BilinearInterpolate(c00, c10, c01, c11, fx, fy);
      var edgeDirected = EdgeDirectedInterpolate(c00, c10, c01, c11, fx, fy, gradX, gradY);
      var blend = (gradMagnitude - lowThreshold) / edgeThreshold;
      blend = blend * blend * (3f - 2f * blend); // smoothstep

      Accum4F<TWork> acc = default;
      acc.AddMul(bilinear, 1f - blend);
      acc.AddMul(edgeDirected, blend);
      result = acc.Result;
    }

    dest[destY * destStride + destX] = encoder.Encode(result);
  }

  /// <summary>
  /// Computes Sobel gradients using BT.709 luminance.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static (float GradX, float GradY) ComputeSobelGradients(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int x, int y) {
    var p00 = Luminance(frame[x - 1, y - 1].Work);
    var p10 = Luminance(frame[x, y - 1].Work);
    var p20 = Luminance(frame[x + 1, y - 1].Work);
    var p01 = Luminance(frame[x - 1, y].Work);
    var p21 = Luminance(frame[x + 1, y].Work);
    var p02 = Luminance(frame[x - 1, y + 1].Work);
    var p12 = Luminance(frame[x, y + 1].Work);
    var p22 = Luminance(frame[x + 1, y + 1].Work);

    // Sobel X: [-1, 0, 1; -2, 0, 2; -1, 0, 1]
    var gx = -p00 + p20 - 2 * p01 + 2 * p21 - p02 + p22;
    // Sobel Y: [-1, -2, -1; 0, 0, 0; 1, 2, 1]
    var gy = -p00 - 2 * p10 - p20 + p02 + 2 * p12 + p22;

    return (gx, gy);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float Luminance(in TWork c) => ColorConverter.GetLuminance(c);

  /// <summary>
  /// Standard bilinear interpolation.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork BilinearInterpolate(in TWork c00, in TWork c10, in TWork c01, in TWork c11, float fx, float fy) {
    var invFx = 1f - fx;
    var invFy = 1f - fy;

    Accum4F<TWork> acc = default;
    acc.AddMul(c00, invFx * invFy);
    acc.AddMul(c10, fx * invFy);
    acc.AddMul(c01, invFx * fy);
    acc.AddMul(c11, fx * fy);
    return acc.Result;
  }

  /// <summary>
  /// Edge-directed interpolation along edge direction.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork EdgeDirectedInterpolate(
    in TWork c00, in TWork c10, in TWork c01, in TWork c11,
    float fx, float fy, float gradX, float gradY) {
    // Determine edge orientation: how horizontal vs vertical
    var absGradX = MathF.Abs(gradX);
    var absGradY = MathF.Abs(gradY);
    var horizontalness = absGradY / (absGradX + absGradY + 0.001f);

    // For horizontal edge (gradY dominant): interpolate along X, then blend rows
    Accum4F<TWork> hAcc = default;
    {
      Accum4F<TWork> topAcc = default;
      topAcc.AddMul(c00, 1f - fx);
      topAcc.AddMul(c10, fx);
      var topRow = topAcc.Result;

      Accum4F<TWork> botAcc = default;
      botAcc.AddMul(c01, 1f - fx);
      botAcc.AddMul(c11, fx);
      var bottomRow = botAcc.Result;

      hAcc.AddMul(topRow, 1f - fy);
      hAcc.AddMul(bottomRow, fy);
    }
    var horizontalResult = hAcc.Result;

    // For vertical edge (gradX dominant): interpolate along Y, then blend cols
    Accum4F<TWork> vAcc = default;
    {
      Accum4F<TWork> leftAcc = default;
      leftAcc.AddMul(c00, 1f - fy);
      leftAcc.AddMul(c01, fy);
      var leftCol = leftAcc.Result;

      Accum4F<TWork> rightAcc = default;
      rightAcc.AddMul(c10, 1f - fy);
      rightAcc.AddMul(c11, fy);
      var rightCol = rightAcc.Result;

      vAcc.AddMul(leftCol, 1f - fx);
      vAcc.AddMul(rightCol, fx);
    }
    var verticalResult = vAcc.Result;

    // Blend based on edge orientation
    Accum4F<TWork> finalAcc = default;
    finalAcc.AddMul(verticalResult, 1f - horizontalness);
    finalAcc.AddMul(horizontalResult, horizontalness);
    return finalAcc.Result;
  }
}
