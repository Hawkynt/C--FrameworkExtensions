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
using System.Drawing;
using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Spaces.Perceptual;
using Hawkynt.ColorProcessing.Storage;
using Hawkynt.ColorProcessing.Working;
using Hawkynt.Drawing;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Resamplers;

/// <summary>
/// Bicubic resampler using Keys cubic interpolation.
/// </summary>
/// <remarks>
/// <para>Uses a 4x4 kernel with cubic polynomial weights.</para>
/// <para>Good balance between sharpness and smoothness.</para>
/// <para>Default parameter a=-0.5 (Keys cubic).</para>
/// </remarks>
[ScalerInfo("Bicubic", Author = "Robert Keys", Year = 1981,
  Description = "4x4 cubic polynomial interpolation", Category = ScalerCategory.Resampler)]
public readonly struct Bicubic : IKernelResampler, IResamplerWithSafePath {

  private readonly float _a;

  /// <summary>
  /// Creates a Bicubic resampler with default parameter (a=-0.5).
  /// </summary>
  public Bicubic() : this(-0.5f) { }

  /// <summary>
  /// Creates a Bicubic resampler with custom parameter.
  /// </summary>
  /// <param name="a">The cubic coefficient. Common values: -0.5 (Keys), -0.75 (sharper), -1.0 (very sharp).</param>
  public Bicubic(float a) => this._a = a;

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 2;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => null;

  /// <inheritdoc />
  public float EvaluateWeight(float distance) {
    var x = MathF.Abs(distance);
    var a = this._a;
    if (x < 1f)
      return ((a + 2f) * x - (a + 3f)) * x * x + 1f;
    if (x < 2f)
      return a * (((x - 5f) * x + 8f) * x - 4f);
    return 0f;
  }

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
    => callback.Invoke(new BicubicKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this._a, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Bicubic Default => new();

  /// <inheritdoc />
  /// <remarks>
  /// Instantiates the concrete <c>BicubicKernel</c> specialisation — which implements
  /// <see cref="IResampleKernelWithSafePath{TPixel,TWork,TKey,TDecode,TProject,TEncode}"/> —
  /// and forwards to <see cref="BitmapScalerExtensions.InvokeSafePathResampler"/>. The JIT
  /// specialises the generic call so the safe-interior path dispatches statically; no
  /// reflection, no boxing inside the hot loop.
  /// </remarks>
  public Bitmap ResampleWithSafePath(
    Bitmap source,
    int targetWidth,
    int targetHeight,
    OutOfBoundsMode horizontalMode,
    OutOfBoundsMode verticalMode,
    Color canvasColor,
    bool useCenteredGrid) {
    ArgumentNullException.ThrowIfNull(source);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetWidth);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetHeight);

    var kernel = new BicubicKernel<Bgra8888, LinearRgbaF, OklabF, Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>(
      source.Width, source.Height, targetWidth, targetHeight, this._a, useCenteredGrid);
    return BitmapScalerExtensions.InvokeSafePathResampler<
      BicubicKernel<Bgra8888, LinearRgbaF, OklabF, Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>
    >(source, targetWidth, targetHeight, kernel, horizontalMode, verticalMode, new Bgra8888(canvasColor));
  }
}

file readonly struct BicubicKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, float a, bool useCenteredGrid)
  : IResampleKernelWithSafePath<TPixel, TWork, TKey, TDecode, TProject, TEncode>
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

    // Integer coordinates of top-left source pixel
    var x0 = (int)MathF.Floor(srcXf);
    var y0 = (int)MathF.Floor(srcYf);

    // Fractional parts
    var fx = srcXf - x0;
    var fy = srcYf - y0;

    // Precompute cubic weights for x and y
    var wx = stackalloc float[4];
    var wy = stackalloc float[4];
    for (var i = 0; i < 4; ++i) {
      wx[i] = CubicWeight(fx - (i - 1), a);
      wy[i] = CubicWeight(fy - (i - 1), a);
    }

    // Edge path: bounds-checked indexer for every sample. The ScalerPipeline only routes
    // destination pixels here that sit on the edge band; the safe interior goes through
    // ResampleUnchecked (below) — no per-pixel OOB branch at all.
    Accum4F<TWork> acc = default;
    for (var ky = 0; ky < 4; ++ky)
    for (var kx = 0; kx < 4; ++kx) {
      var weight = wx[kx] * wy[ky];
      if (weight == 0f) continue;
      acc.AddMul(frame[x0 + kx - 1, y0 + ky - 1].Work, weight);
    }

    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void ResampleUnchecked(
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

    var wx = stackalloc float[4];
    var wy = stackalloc float[4];
    for (var i = 0; i < 4; ++i) {
      wx[i] = CubicWeight(fx - (i - 1), a);
      wy[i] = CubicWeight(fy - (i - 1), a);
    }

    // Safe-interior path: caller guarantees the entire [x0-1, x0+3) × [y0-1, y0+3) window
    // is in-bounds. Each sample is a single MOV; inner loop is tight enough that a follow-up
    // can SIMD-fuse the 4×4 accumulate via Vector256/Vector512 when TPixel == TWork == Bgra8888.
    Accum4F<TWork> acc = default;
    for (var ky = 0; ky < 4; ++ky)
    for (var kx = 0; kx < 4; ++kx) {
      var weight = wx[kx] * wy[ky];
      if (weight == 0f) continue;
      acc.AddMul(frame.GetUnchecked(x0 + kx - 1, y0 + ky - 1).Work, weight);
    }

    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
  }

  /// <inheritdoc />
  public System.Drawing.Rectangle GetSafeDestinationRegion()
    => ResampleKernelHelpers.ComputeSafeDestinationRegion(
      kxMin: -1, kxMaxExcl: 3, this._scaleX, this._offsetX, sourceWidth, targetWidth,
      kyMin: -1, kyMaxExcl: 3, this._scaleY, this._offsetY, sourceHeight, targetHeight);

  /// <summary>
  /// Computes the cubic weight for a given distance.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float CubicWeight(float x, float a) {
    x = MathF.Abs(x);
    if (x < 1f)
      return ((a + 2f) * x - (a + 3f)) * x * x + 1f;
    if (x < 2f)
      return a * (((x - 5f) * x + 8f) * x - 4f);
    return 0f;
  }
}
