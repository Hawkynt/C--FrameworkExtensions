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
/// Lanczos-2 resampler - windowed sinc with a=2.
/// </summary>
/// <remarks>
/// <para>High-quality resampling using windowed sinc function.</para>
/// <para>Good balance between sharpness and ringing artifacts.</para>
/// </remarks>
[ScalerInfo("Lanczos-2", Author = "Cornelius Lanczos", Year = 1950,
  Description = "Windowed sinc resampler with a=2", Category = ScalerCategory.Resampler)]
public readonly struct Lanczos2 : IResampler {

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
    => callback.Invoke(new LanczosKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, 2));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Lanczos2 Default => new();
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
public readonly struct Lanczos3 : IResampler {

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
    => callback.Invoke(new LanczosKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, 3));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Lanczos3 Default => new();
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
public readonly struct Lanczos4 : IResampler {

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
    => callback.Invoke(new LanczosKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, 4));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Lanczos4 Default => new();
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
public readonly struct Lanczos5 : IResampler {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 5;

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
    => callback.Invoke(new LanczosKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, 5));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Lanczos5 Default => new();
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
public readonly struct Lanczos : IResampler {

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
    => callback.Invoke(new LanczosKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this._a == 0 ? 3 : this._a));

  /// <summary>
  /// Gets the default configuration (a=3).
  /// </summary>
  public static Lanczos Default => new();
}

file readonly struct LanczosKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, int a)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
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

  // Precomputed scale factors
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

    // Accumulate weighted colors from (2*a)x(2*a) kernel
    Accum4F<TWork> acc = default;
    for (var ky = -a + 1; ky <= a; ++ky)
    for (var kx = -a + 1; kx <= a; ++kx) {
      var weight = LanczosWeight(fx - kx, a) * LanczosWeight(fy - ky, a);
      if (weight == 0f)
        continue;

      var pixel = frame[srcXi + kx, srcYi + ky].Work;
      acc.AddMul(pixel, weight);
    }

    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
  }

  /// <summary>
  /// Computes the Lanczos weight for a given distance.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float LanczosWeight(float x, int a) {
    if (x == 0f)
      return 1f;
    var absX = MathF.Abs(x);
    if (absX >= a)
      return 0f;
    var pix = MathF.PI * x;
    return a * MathF.Sin(pix) * MathF.Sin(pix / a) / (pix * pix);
  }
}
