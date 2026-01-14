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
/// Enhanced Edge-Directed Interpolation 2 (EEDI2) resampler.
/// </summary>
/// <remarks>
/// <para>Originally designed for video deinterlacing, adapted for image upscaling.</para>
/// <para>Uses directional interpolation based on local edge orientation.</para>
/// <para>Evaluates multiple directions and selects best match for sharp edges.</para>
/// <para>Based on tritical's EEDI2 algorithm.</para>
/// </remarks>
[ScalerInfo("EEDI2", Author = "tritical", Year = 2007,
  Description = "Enhanced edge-directed interpolation", Category = ScalerCategory.Resampler)]
public readonly struct Eedi2 : IResampler {

  private readonly int _maxDirections;
  private readonly float _threshold;

  /// <summary>
  /// Creates an EEDI2 resampler with default parameters.
  /// </summary>
  public Eedi2() : this(8, 0.15f) { }

  /// <summary>
  /// Creates an EEDI2 resampler with custom parameters.
  /// </summary>
  /// <param name="maxDirections">Maximum number of directions to evaluate (4, 8, or 16).</param>
  /// <param name="threshold">Edge detection threshold. Lower detects more edges.</param>
  public Eedi2(int maxDirections, float threshold) {
    this._maxDirections = maxDirections switch {
      <= 4 => 4,
      <= 8 => 8,
      _ => 16
    };
    this._threshold = Math.Clamp(threshold, 0.01f, 1f);
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
    int targetHeight)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new Eedi2Kernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this._maxDirections, this._threshold));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Eedi2 Default => new();

  /// <summary>
  /// Gets a high-quality configuration with more directions.
  /// </summary>
  public static Eedi2 HighQuality => new(16, 0.1f);

  /// <summary>
  /// Gets a fast configuration with fewer directions.
  /// </summary>
  public static Eedi2 Fast => new(4, 0.2f);
}

file readonly struct Eedi2Kernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, int maxDirections, float threshold)
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

  // Direction offsets for edge detection (dx, dy pairs)
  // 8 directions: horizontal, vertical, and diagonals
  // Note: Using static arrays instead of ReadOnlySpan to avoid Roslyn SDK 10 compiler bug on older TFMs
  private static readonly float[] _directionOffsetsX = [-1f, 0f, 1f, 1f, 1f, 0f, -1f, -1f, -0.5f, 0.5f, 1f, 1f, 0.5f, -0.5f, -1f, -1f];
  private static readonly float[] _directionOffsetsY = [0f, -1f, 0f, 1f, -1f, 1f, -1f, 1f, -1f, -1f, -0.5f, 0.5f, 1f, 1f, 0.5f, -0.5f];

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    // Map destination pixel center to source coordinates
    var srcXf = (destX + 0.5f) * this._scaleX - 0.5f;
    var srcYf = (destY + 0.5f) * this._scaleY - 0.5f;

    // Integer base coordinates
    var x0 = (int)MathF.Floor(srcXf);
    var y0 = (int)MathF.Floor(srcYf);

    // Fractional parts
    var fx = srcXf - x0;
    var fy = srcYf - y0;

    // Get center and adjacent pixels for edge detection
    var c11 = frame[x0, y0].Work;
    var c21 = frame[x0 + 1, y0].Work;
    var c12 = frame[x0, y0 + 1].Work;
    var c22 = frame[x0 + 1, y0 + 1].Work;

    // Calculate luminances for direction finding
    var l11 = ColorConverter.GetLuminance(in c11);
    var l21 = ColorConverter.GetLuminance(in c21);
    var l12 = ColorConverter.GetLuminance(in c12);
    var l22 = ColorConverter.GetLuminance(in c22);

    // Check if we're at an edge that needs directional interpolation
    var contrast = MathF.Max(
      MathF.Abs(l11 - l22),
      MathF.Abs(l21 - l12)
    );

    TWork result;
    if (contrast < threshold) {
      // Low contrast: use simple bilinear
      result = BilinearInterpolate(c11, c21, c12, c22, fx, fy);
    } else {
      // High contrast: find best direction
      var bestDir = FindBestDirection(frame, x0, y0, fx, fy, l11, l21, l12, l22);
      result = DirectionalInterpolate(frame, x0, y0, fx, fy, bestDir);
    }

    dest[destY * destStride + destX] = encoder.Encode(result);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private int FindBestDirection(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int x0, int y0,
    float fx, float fy,
    float l11, float l21, float l12, float l22) {
    var bestDir = 0;
    var bestCost = float.MaxValue;

    var dirX = _directionOffsetsX;
    var dirY = _directionOffsetsY;

    // Evaluate each direction
    for (var d = 0; d < maxDirections; ++d) {
      var dx = dirX[d];
      var dy = dirY[d];

      // Sample along this direction
      var pMinus = SampleBilinear(frame, x0 + fx - dx, y0 + fy - dy);
      var pPlus = SampleBilinear(frame, x0 + fx + dx, y0 + fy + dy);

      var lMinus = ColorConverter.GetLuminance(in pMinus);
      var lPlus = ColorConverter.GetLuminance(in pPlus);

      // Cost is difference between samples along this direction
      // Lower cost means more consistent edge along this direction
      var cost = MathF.Abs(lMinus - lPlus);

      // Add directional coherence term
      var interpPoint = (lMinus + lPlus) * 0.5f;
      var bilinearPoint = BilinearLuma(l11, l21, l12, l22, fx, fy);
      cost += MathF.Abs(interpPoint - bilinearPoint) * 0.5f;

      if (cost < bestCost) {
        bestCost = cost;
        bestDir = d;
      }
    }

    return bestDir;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private TWork DirectionalInterpolate(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int x0, int y0,
    float fx, float fy,
    int direction) {
    var dirX = _directionOffsetsX;
    var dirY = _directionOffsetsY;

    var dx = dirX[direction];
    var dy = dirY[direction];

    // Sample 4 points along the direction for cubic interpolation
    var p0 = SampleBilinear(frame, x0 + fx - dx * 1.5f, y0 + fy - dy * 1.5f);
    var p1 = SampleBilinear(frame, x0 + fx - dx * 0.5f, y0 + fy - dy * 0.5f);
    var p2 = SampleBilinear(frame, x0 + fx + dx * 0.5f, y0 + fy + dy * 0.5f);
    var p3 = SampleBilinear(frame, x0 + fx + dx * 1.5f, y0 + fy + dy * 1.5f);

    // Use Catmull-Rom spline for smooth interpolation
    // t = 0.5 (interpolating between p1 and p2)
    const float t = 0.5f;
    const float t2 = t * t;
    const float t3 = t2 * t;

    // Catmull-Rom weights at t=0.5
    var w0 = -0.5f * t3 + t2 - 0.5f * t;
    var w1 = 1.5f * t3 - 2.5f * t2 + 1f;
    var w2 = -1.5f * t3 + 2f * t2 + 0.5f * t;
    var w3 = 0.5f * t3 - 0.5f * t2;

    Accum4F<TWork> acc = default;
    acc.AddMul(p0, w0);
    acc.AddMul(p1, w1);
    acc.AddMul(p2, w2);
    acc.AddMul(p3, w3);

    return acc.Result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork BilinearInterpolate(TWork c00, TWork c10, TWork c01, TWork c11, float fx, float fy) {
    var w00 = (1f - fx) * (1f - fy);
    var w10 = fx * (1f - fy);
    var w01 = (1f - fx) * fy;
    var w11 = fx * fy;

    Accum4F<TWork> acc = default;
    acc.AddMul(c00, w00);
    acc.AddMul(c10, w10);
    acc.AddMul(c01, w01);
    acc.AddMul(c11, w11);
    return acc.Result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float BilinearLuma(float l00, float l10, float l01, float l11, float fx, float fy)
    => (1f - fx) * (1f - fy) * l00 + fx * (1f - fy) * l10 +
       (1f - fx) * fy * l01 + fx * fy * l11;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork SampleBilinear(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    float x, float y) {
    var ix = (int)MathF.Floor(x);
    var iy = (int)MathF.Floor(y);
    var fx = x - ix;
    var fy = y - iy;

    var c00 = frame[ix, iy].Work;
    var c10 = frame[ix + 1, iy].Work;
    var c01 = frame[ix, iy + 1].Work;
    var c11 = frame[ix + 1, iy + 1].Work;

    return BilinearInterpolate(c00, c10, c01, c11, fx, fy);
  }

}
