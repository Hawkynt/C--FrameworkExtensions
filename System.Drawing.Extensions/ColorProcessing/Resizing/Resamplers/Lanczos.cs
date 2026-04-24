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
/// Lanczos-2 resampler - windowed sinc with a=2.
/// </summary>
/// <remarks>
/// <para>High-quality resampling using windowed sinc function.</para>
/// <para>Good balance between sharpness and ringing artifacts.</para>
/// </remarks>
[ScalerInfo("Lanczos-2", Author = "Cornelius Lanczos", Year = 1950,
  Description = "Windowed sinc resampler with a=2", Category = ScalerCategory.Resampler)]
public readonly struct Lanczos2 : IKernelResampler, IResamplerWithSafePath {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 2;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => null;

  /// <inheritdoc />
  public float EvaluateWeight(float distance) => LanczosMath.Weight(distance, 2);

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
    => callback.Invoke(new LanczosKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, 2, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Lanczos2 Default => new();

  /// <inheritdoc />
  public Bitmap ResampleWithSafePath(Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode, Color canvasColor, bool useCenteredGrid)
    => _LanczosSafePath.Dispatch(2, source, targetWidth, targetHeight, horizontalMode, verticalMode, canvasColor, useCenteredGrid);
}

/// <summary>
/// Lanczos-3 resampler - windowed sinc with a=3.
/// </summary>
/// <remarks>
/// <para>High-quality resampling using windowed sinc function.</para>
/// <para>Sharper than Lanczos-2 but may produce more ringing on high-contrast edges.</para>
/// </remarks>
[ScalerInfo("Lanczos-3", Author = "Cornelius Lanczos", Year = 1950,
  Description = "Windowed sinc resampler with a=3", Category = ScalerCategory.Resampler)]
public readonly struct Lanczos3 : IKernelResampler, IResamplerWithSafePath {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 3;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => null;

  /// <inheritdoc />
  public float EvaluateWeight(float distance) => LanczosMath.Weight(distance, 3);

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
    => callback.Invoke(new LanczosKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, 3, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Lanczos3 Default => new();

  /// <inheritdoc />
  public Bitmap ResampleWithSafePath(Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode, Color canvasColor, bool useCenteredGrid)
    => _LanczosSafePath.Dispatch(3, source, targetWidth, targetHeight, horizontalMode, verticalMode, canvasColor, useCenteredGrid);
}

/// <summary>
/// Lanczos-4 resampler - windowed sinc with a=4.
/// </summary>
/// <remarks>
/// <para>High-quality resampling using windowed sinc function.</para>
/// <para>Very sharp results but may produce more ringing on high-contrast edges.</para>
/// </remarks>
[ScalerInfo("Lanczos-4", Author = "Cornelius Lanczos", Year = 1950,
  Description = "Windowed sinc resampler with a=4", Category = ScalerCategory.Resampler)]
public readonly struct Lanczos4 : IKernelResampler, IResamplerWithSafePath {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 4;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => null;

  /// <inheritdoc />
  public float EvaluateWeight(float distance) => LanczosMath.Weight(distance, 4);

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
    => callback.Invoke(new LanczosKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, 4, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Lanczos4 Default => new();

  /// <inheritdoc />
  public Bitmap ResampleWithSafePath(Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode, Color canvasColor, bool useCenteredGrid)
    => _LanczosSafePath.Dispatch(4, source, targetWidth, targetHeight, horizontalMode, verticalMode, canvasColor, useCenteredGrid);
}

/// <summary>
/// Lanczos-5 resampler - windowed sinc with a=5.
/// </summary>
/// <remarks>
/// <para>High-quality resampling using windowed sinc function.</para>
/// <para>Extremely sharp results with significant ringing on high-contrast edges.</para>
/// </remarks>
[ScalerInfo("Lanczos-5", Author = "Cornelius Lanczos", Year = 1950,
  Description = "Windowed sinc resampler with a=5", Category = ScalerCategory.Resampler)]
public readonly struct Lanczos5 : IKernelResampler, IResamplerWithSafePath {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 5;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => null;

  /// <inheritdoc />
  public float EvaluateWeight(float distance) => LanczosMath.Weight(distance, 5);

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
    => callback.Invoke(new LanczosKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, 5, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Lanczos5 Default => new();

  /// <inheritdoc />
  public Bitmap ResampleWithSafePath(Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode, Color canvasColor, bool useCenteredGrid)
    => _LanczosSafePath.Dispatch(5, source, targetWidth, targetHeight, horizontalMode, verticalMode, canvasColor, useCenteredGrid);
}

/// <summary>
/// Configurable Lanczos resampler with custom window size.
/// </summary>
/// <remarks>
/// <para>High-quality resampling using windowed sinc function.</para>
/// <para>Larger values of 'a' produce sharper results but more ringing.</para>
/// </remarks>
[ScalerInfo("Lanczos", Author = "Cornelius Lanczos", Year = 1950,
  Description = "Configurable windowed sinc resampler", Category = ScalerCategory.Resampler)]
public readonly struct Lanczos : IKernelResampler, IResamplerWithSafePath {

  private readonly int _a;

  /// <summary>
  /// Creates a Lanczos resampler with a=3 (default).
  /// </summary>
  public Lanczos() : this(3) { }

  /// <summary>
  /// Creates a Lanczos resampler with custom window size.
  /// </summary>
  /// <param name="a">The window size (typically 2, 3, or 4).</param>
  public Lanczos(int a) {
    ArgumentOutOfRangeException.ThrowIfLessThan(a, 1);
    this._a = a;
  }

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => this._a == 0 ? 3 : this._a;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => null;

  /// <inheritdoc />
  public float EvaluateWeight(float distance) => LanczosMath.Weight(distance, this.Radius);

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
    => callback.Invoke(new LanczosKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this._a == 0 ? 3 : this._a, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration (a=3).
  /// </summary>
  public static Lanczos Default => new();

  /// <inheritdoc />
  public Bitmap ResampleWithSafePath(Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode, Color canvasColor, bool useCenteredGrid)
    => _LanczosSafePath.Dispatch(this._a == 0 ? 3 : this._a, source, targetWidth, targetHeight, horizontalMode, verticalMode, canvasColor, useCenteredGrid);
}

file readonly struct LanczosKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, int a, bool useCenteredGrid)
  : IResampleKernelWithSafePath<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => a;
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

    // Edge path: bounds-checked indexer. Pipeline routes only edge-band destination pixels here.
    Accum4F<TWork> acc = default;
    for (var ky = -a + 1; ky <= a; ++ky)
    for (var kx = -a + 1; kx <= a; ++kx) {
      var weight = LanczosWeight(fx - kx, a) * LanczosWeight(fy - ky, a);
      if (weight == 0f) continue;
      acc.AddMul(frame[srcXi + kx, srcYi + ky].Work, weight);
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
    var srcXi = (int)MathF.Floor(srcXf);
    var srcYi = (int)MathF.Floor(srcYf);
    var fx = srcXf - srcXi;
    var fy = srcYf - srcYi;

    // Safe interior — no OOB check, each load one MOV. Ready for SIMD over the (2a)×(2a) taps.
    Accum4F<TWork> acc = default;
    for (var ky = -a + 1; ky <= a; ++ky)
    for (var kx = -a + 1; kx <= a; ++kx) {
      var weight = LanczosWeight(fx - kx, a) * LanczosWeight(fy - ky, a);
      if (weight == 0f) continue;
      acc.AddMul(frame.GetUnchecked(srcXi + kx, srcYi + ky).Work, weight);
    }

    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
  }

  /// <inheritdoc />
  public Rectangle GetSafeDestinationRegion()
    => ResampleKernelHelpers.ComputeSafeDestinationRegion(
      kxMin: -a + 1, kxMaxExcl: a + 1, this._scaleX, this._offsetX, sourceWidth, targetWidth,
      kyMin: -a + 1, kyMaxExcl: a + 1, this._scaleY, this._offsetY, sourceHeight, targetHeight);

  /// <summary>
  /// Computes the Lanczos weight for a given distance.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float LanczosWeight(float x, int a) => LanczosMath.Weight(x, a);
}

/// <summary>
/// Runtime dispatcher for Lanczos's <see cref="IResamplerWithSafePath"/> path: the kernel's
/// <c>a</c> is an <c>int</c> field not a generic type parameter, so we switch on the handful of
/// common values to give the JIT concrete instantiations of <see cref="BitmapScalerExtensions.InvokeSafePathResampler"/>.
/// For arbitrary <c>a</c> (Lanczos(7), Lanczos(12)…) there's a fallback that still works — it
/// just doesn't get a pre-JITted specialisation.
/// </summary>
internal static class _LanczosSafePath {
  public static Bitmap Dispatch(int a, Bitmap source, int targetWidth, int targetHeight,
    OutOfBoundsMode horizontalMode, OutOfBoundsMode verticalMode, Color canvasColor, bool useCenteredGrid) {
    ArgumentNullException.ThrowIfNull(source);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetWidth);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetHeight);

    var kernel = new LanczosKernel<Bgra8888, LinearRgbaF, OklabF, Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>(
      source.Width, source.Height, targetWidth, targetHeight, a, useCenteredGrid);
    return BitmapScalerExtensions.InvokeSafePathResampler<
      LanczosKernel<Bgra8888, LinearRgbaF, OklabF, Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32>
    >(source, targetWidth, targetHeight, kernel, horizontalMode, verticalMode, new Bgra8888(canvasColor));
  }
}

internal static class LanczosMath {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Weight(float x, int a) {
    if (x == 0f)
      return 1f;
    var absX = MathF.Abs(x);
    if (absX >= a)
      return 0f;
    var pix = MathF.PI * x;
    return a * MathF.Sin(pix) * MathF.Sin(pix / a) / (pix * pix);
  }
}
