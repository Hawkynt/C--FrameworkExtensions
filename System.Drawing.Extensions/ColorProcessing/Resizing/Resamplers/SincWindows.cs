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

#region Blackman

/// <summary>
/// Blackman-windowed sinc resampler.
/// </summary>
/// <remarks>
/// <para>Very low sidelobe response, making it excellent for reducing aliasing artifacts.</para>
/// <para>Named after Ralph Blackman who developed this window function in 1958.</para>
/// </remarks>
[ScalerInfo("Blackman", Author = "Ralph Blackman", Year = 1958,
  Description = "Blackman-windowed sinc resampler with very low sidelobes", Category = ScalerCategory.Resampler)]
public readonly struct Blackman : IResampler {

  private readonly int _radius;

  /// <summary>
  /// Creates a Blackman resampler with radius 3 (default).
  /// </summary>
  public Blackman() : this(3) { }

  /// <summary>
  /// Creates a Blackman resampler with custom radius.
  /// </summary>
  /// <param name="radius">The filter radius (typically 2, 3, or 4).</param>
  public Blackman(int radius) {
    ArgumentOutOfRangeException.ThrowIfLessThan(radius, 1);
    this._radius = radius;
  }

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => this._radius == 0 ? 3 : this._radius;

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
    => callback.Invoke(new SincWindowKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.Radius, WindowType.Blackman, 0f, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Blackman Default => new();
}

#endregion

#region Hann

/// <summary>
/// Hann-windowed sinc resampler (also known as Hanning window).
/// </summary>
/// <remarks>
/// <para>Cosine taper window, commonly used in audio and spectral analysis.</para>
/// <para>Named after Julius von Hann who developed this window function in 1903.</para>
/// </remarks>
[ScalerInfo("Hann", Author = "Julius von Hann", Year = 1903,
  Description = "Hann-windowed sinc resampler with cosine taper", Category = ScalerCategory.Resampler)]
public readonly struct Hann : IResampler {

  private readonly int _radius;

  /// <summary>
  /// Creates a Hann resampler with radius 3 (default).
  /// </summary>
  public Hann() : this(3) { }

  /// <summary>
  /// Creates a Hann resampler with custom radius.
  /// </summary>
  /// <param name="radius">The filter radius (typically 2, 3, or 4).</param>
  public Hann(int radius) {
    ArgumentOutOfRangeException.ThrowIfLessThan(radius, 1);
    this._radius = radius;
  }

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => this._radius == 0 ? 3 : this._radius;

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
    => callback.Invoke(new SincWindowKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.Radius, WindowType.Hann, 0f, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Hann Default => new();
}

#endregion

#region Hamming

/// <summary>
/// Hamming-windowed sinc resampler.
/// </summary>
/// <remarks>
/// <para>Similar to Hann but with less sidelobe suppression and a non-zero edge value.</para>
/// <para>Named after Richard Hamming who developed this window function in 1977.</para>
/// </remarks>
[ScalerInfo("Hamming", Author = "Richard Hamming", Year = 1977,
  Description = "Hamming-windowed sinc resampler", Category = ScalerCategory.Resampler)]
public readonly struct Hamming : IResampler {

  private readonly int _radius;

  /// <summary>
  /// Creates a Hamming resampler with radius 3 (default).
  /// </summary>
  public Hamming() : this(3) { }

  /// <summary>
  /// Creates a Hamming resampler with custom radius.
  /// </summary>
  /// <param name="radius">The filter radius (typically 2, 3, or 4).</param>
  public Hamming(int radius) {
    ArgumentOutOfRangeException.ThrowIfLessThan(radius, 1);
    this._radius = radius;
  }

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => this._radius == 0 ? 3 : this._radius;

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
    => callback.Invoke(new SincWindowKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.Radius, WindowType.Hamming, 0f, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Hamming Default => new();
}

#endregion

#region Kaiser

/// <summary>
/// Kaiser-windowed sinc resampler.
/// </summary>
/// <remarks>
/// <para>Adjustable window using modified Bessel function I₀ with parameter β.</para>
/// <para>Higher β values give better sidelobe suppression at the cost of wider main lobe.</para>
/// <para>Named after James Kaiser who developed this window function in 1974.</para>
/// </remarks>
[ScalerInfo("Kaiser", Author = "James Kaiser", Year = 1974,
  Description = "Kaiser-windowed sinc resampler with adjustable β parameter", Category = ScalerCategory.Resampler)]
public readonly struct Kaiser : IResampler {

  /// <summary>
  /// Default β parameter (8.6 gives excellent sidelobe suppression).
  /// </summary>
  public const float DefaultBeta = 8.6f;

  private readonly int _radius;
  private readonly float _beta;

  /// <summary>
  /// Creates a Kaiser resampler with radius 3 and default β.
  /// </summary>
  public Kaiser() : this(3, DefaultBeta) { }

  /// <summary>
  /// Creates a Kaiser resampler with custom radius and default β.
  /// </summary>
  /// <param name="radius">The filter radius (typically 2, 3, or 4).</param>
  public Kaiser(int radius) : this(radius, DefaultBeta) { }

  /// <summary>
  /// Creates a Kaiser resampler with custom radius and β.
  /// </summary>
  /// <param name="radius">The filter radius (typically 2, 3, or 4).</param>
  /// <param name="beta">The β parameter controlling sidelobe suppression (typically 4-12).</param>
  public Kaiser(int radius, float beta) {
    ArgumentOutOfRangeException.ThrowIfLessThan(radius, 1);
    ArgumentOutOfRangeException.ThrowIfNegative(beta);
    this._radius = radius;
    this._beta = beta;
  }

  /// <summary>
  /// Gets the β parameter.
  /// </summary>
  public float Beta => this._beta == 0f ? DefaultBeta : this._beta;

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => this._radius == 0 ? 3 : this._radius;

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
    => callback.Invoke(new SincWindowKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.Radius, WindowType.Kaiser, this.Beta, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Kaiser Default => new();
}

#endregion

#region Welch

/// <summary>
/// Welch-windowed sinc resampler.
/// </summary>
/// <remarks>
/// <para>Simple parabolic window with decent sidelobe suppression.</para>
/// <para>Named after Peter Welch who developed this window function in 1967.</para>
/// </remarks>
[ScalerInfo("Welch", Author = "Peter Welch", Year = 1967,
  Description = "Welch-windowed sinc resampler with parabolic taper", Category = ScalerCategory.Resampler)]
public readonly struct Welch : IResampler {

  private readonly int _radius;

  /// <summary>
  /// Creates a Welch resampler with radius 3 (default).
  /// </summary>
  public Welch() : this(3) { }

  /// <summary>
  /// Creates a Welch resampler with custom radius.
  /// </summary>
  /// <param name="radius">The filter radius (typically 2, 3, or 4).</param>
  public Welch(int radius) {
    ArgumentOutOfRangeException.ThrowIfLessThan(radius, 1);
    this._radius = radius;
  }

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => this._radius == 0 ? 3 : this._radius;

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
    => callback.Invoke(new SincWindowKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.Radius, WindowType.Welch, 0f, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Welch Default => new();
}

#endregion

#region Bartlett

/// <summary>
/// Bartlett-windowed sinc resampler (triangular window).
/// </summary>
/// <remarks>
/// <para>Simple triangular window with linear taper.</para>
/// <para>Named after M.S. Bartlett who developed this window function in 1950.</para>
/// </remarks>
[ScalerInfo("Bartlett", Author = "M.S. Bartlett", Year = 1950,
  Description = "Bartlett-windowed sinc resampler with triangular taper", Category = ScalerCategory.Resampler)]
public readonly struct Bartlett : IResampler {

  private readonly int _radius;

  /// <summary>
  /// Creates a Bartlett resampler with radius 3 (default).
  /// </summary>
  public Bartlett() : this(3) { }

  /// <summary>
  /// Creates a Bartlett resampler with custom radius.
  /// </summary>
  /// <param name="radius">The filter radius (typically 2, 3, or 4).</param>
  public Bartlett(int radius) {
    ArgumentOutOfRangeException.ThrowIfLessThan(radius, 1);
    this._radius = radius;
  }

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => this._radius == 0 ? 3 : this._radius;

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
    => callback.Invoke(new SincWindowKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.Radius, WindowType.Bartlett, 0f, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Bartlett Default => new();
}

#endregion

#region Nuttal

/// <summary>
/// Nuttal-windowed sinc resampler.
/// </summary>
/// <remarks>
/// <para>4-term cosine sum with very low sidelobes.</para>
/// <para>Coefficients: a₀=0.355768, a₁=0.487396, a₂=0.144232, a₃=0.012604.</para>
/// </remarks>
[ScalerInfo("Nuttal", Year = 1981,
  Description = "Nuttal-windowed sinc resampler with 4-term cosine sum", Category = ScalerCategory.Resampler)]
public readonly struct Nuttal : IResampler {

  private readonly int _radius;

  /// <summary>
  /// Creates a Nuttal resampler with radius 3 (default).
  /// </summary>
  public Nuttal() : this(3) { }

  /// <summary>
  /// Creates a Nuttal resampler with custom radius.
  /// </summary>
  /// <param name="radius">The filter radius (typically 2, 3, or 4).</param>
  public Nuttal(int radius) {
    ArgumentOutOfRangeException.ThrowIfLessThan(radius, 1);
    this._radius = radius;
  }

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => this._radius == 0 ? 3 : this._radius;

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
    => callback.Invoke(new SincWindowKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.Radius, WindowType.Nuttal, 0f, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Nuttal Default => new();
}

#endregion

#region BlackmanNuttal

/// <summary>
/// Blackman-Nuttal-windowed sinc resampler.
/// </summary>
/// <remarks>
/// <para>4-term cosine sum combining Blackman and Nuttal characteristics.</para>
/// <para>Coefficients: a₀=0.3635819, a₁=0.4891775, a₂=0.1365995, a₃=0.0106411.</para>
/// </remarks>
[ScalerInfo("BlackmanNuttal", Year = 1981,
  Description = "Blackman-Nuttal-windowed sinc resampler", Category = ScalerCategory.Resampler)]
public readonly struct BlackmanNuttal : IResampler {

  private readonly int _radius;

  /// <summary>
  /// Creates a Blackman-Nuttal resampler with radius 3 (default).
  /// </summary>
  public BlackmanNuttal() : this(3) { }

  /// <summary>
  /// Creates a Blackman-Nuttal resampler with custom radius.
  /// </summary>
  /// <param name="radius">The filter radius (typically 2, 3, or 4).</param>
  public BlackmanNuttal(int radius) {
    ArgumentOutOfRangeException.ThrowIfLessThan(radius, 1);
    this._radius = radius;
  }

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => this._radius == 0 ? 3 : this._radius;

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
    => callback.Invoke(new SincWindowKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.Radius, WindowType.BlackmanNuttal, 0f, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static BlackmanNuttal Default => new();
}

#endregion

#region BlackmanHarris

/// <summary>
/// Blackman-Harris-windowed sinc resampler.
/// </summary>
/// <remarks>
/// <para>4-term cosine sum with minimum sidelobe level.</para>
/// <para>Coefficients: a₀=0.35875, a₁=0.48829, a₂=0.14128, a₃=0.01168.</para>
/// </remarks>
[ScalerInfo("BlackmanHarris", Author = "Fredric Harris", Year = 1978,
  Description = "Blackman-Harris-windowed sinc resampler with minimum sidelobes", Category = ScalerCategory.Resampler)]
public readonly struct BlackmanHarris : IResampler {

  private readonly int _radius;

  /// <summary>
  /// Creates a Blackman-Harris resampler with radius 3 (default).
  /// </summary>
  public BlackmanHarris() : this(3) { }

  /// <summary>
  /// Creates a Blackman-Harris resampler with custom radius.
  /// </summary>
  /// <param name="radius">The filter radius (typically 2, 3, or 4).</param>
  public BlackmanHarris(int radius) {
    ArgumentOutOfRangeException.ThrowIfLessThan(radius, 1);
    this._radius = radius;
  }

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => this._radius == 0 ? 3 : this._radius;

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
    => callback.Invoke(new SincWindowKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.Radius, WindowType.BlackmanHarris, 0f, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static BlackmanHarris Default => new();
}

#endregion

#region FlatTop

/// <summary>
/// FlatTop-windowed sinc resampler.
/// </summary>
/// <remarks>
/// <para>5-term cosine sum with very flat passband response.</para>
/// <para>Excellent for amplitude measurement applications.</para>
/// <para>Coefficients: a₀=1.0, a₁=1.93, a₂=1.29, a₃=0.388, a₄=0.0322.</para>
/// </remarks>
[ScalerInfo("FlatTop", Year = 1990,
  Description = "FlatTop-windowed sinc resampler with flat passband", Category = ScalerCategory.Resampler)]
public readonly struct FlatTop : IResampler {

  private readonly int _radius;

  /// <summary>
  /// Creates a FlatTop resampler with radius 3 (default).
  /// </summary>
  public FlatTop() : this(3) { }

  /// <summary>
  /// Creates a FlatTop resampler with custom radius.
  /// </summary>
  /// <param name="radius">The filter radius (typically 2, 3, or 4).</param>
  public FlatTop(int radius) {
    ArgumentOutOfRangeException.ThrowIfLessThan(radius, 1);
    this._radius = radius;
  }

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => this._radius == 0 ? 3 : this._radius;

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
    => callback.Invoke(new SincWindowKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.Radius, WindowType.FlatTop, 0f, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static FlatTop Default => new();
}

#endregion

#region Cosine

/// <summary>
/// Cosine-windowed sinc resampler.
/// </summary>
/// <remarks>
/// <para>Simple cosine window: cos(πx/2R).</para>
/// <para>Provides smooth taper with moderate sidelobe suppression.</para>
/// </remarks>
[ScalerInfo("Cosine",
  Description = "Cosine-windowed sinc resampler", Category = ScalerCategory.Resampler)]
public readonly struct Cosine : IResampler {

  private readonly int _radius;

  /// <summary>
  /// Creates a Cosine resampler with radius 3 (default).
  /// </summary>
  public Cosine() : this(3) { }

  /// <summary>
  /// Creates a Cosine resampler with custom radius.
  /// </summary>
  /// <param name="radius">The filter radius (typically 2, 3, or 4).</param>
  public Cosine(int radius) {
    ArgumentOutOfRangeException.ThrowIfLessThan(radius, 1);
    this._radius = radius;
  }

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => this._radius == 0 ? 3 : this._radius;

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
    => callback.Invoke(new SincWindowKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.Radius, WindowType.Cosine, 0f, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Cosine Default => new();
}

#endregion

#region PowerOfCosine

/// <summary>
/// Power-of-Cosine-windowed sinc resampler.
/// </summary>
/// <remarks>
/// <para>Generalized cosine window: cos^α(πx/2R).</para>
/// <para>Higher α values provide stronger tapering.</para>
/// </remarks>
[ScalerInfo("PowerOfCosine",
  Description = "Power-of-Cosine-windowed sinc resampler with adjustable α", Category = ScalerCategory.Resampler)]
public readonly struct PowerOfCosine : IResampler {

  /// <summary>
  /// Default α parameter.
  /// </summary>
  public const float DefaultAlpha = 1.5f;

  private readonly int _radius;
  private readonly float _alpha;

  /// <summary>
  /// Creates a PowerOfCosine resampler with radius 3 and default α.
  /// </summary>
  public PowerOfCosine() : this(3, DefaultAlpha) { }

  /// <summary>
  /// Creates a PowerOfCosine resampler with custom radius and default α.
  /// </summary>
  /// <param name="radius">The filter radius (typically 2, 3, or 4).</param>
  public PowerOfCosine(int radius) : this(radius, DefaultAlpha) { }

  /// <summary>
  /// Creates a PowerOfCosine resampler with custom radius and α.
  /// </summary>
  /// <param name="radius">The filter radius (typically 2, 3, or 4).</param>
  /// <param name="alpha">The power exponent (typically 1.0-3.0).</param>
  public PowerOfCosine(int radius, float alpha) {
    ArgumentOutOfRangeException.ThrowIfLessThan(radius, 1);
    ArgumentOutOfRangeException.ThrowIfNegative(alpha);
    this._radius = radius;
    this._alpha = alpha;
  }

  /// <summary>
  /// Gets the α parameter.
  /// </summary>
  public float Alpha => this._alpha == 0f ? DefaultAlpha : this._alpha;

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => this._radius == 0 ? 3 : this._radius;

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
    => callback.Invoke(new SincWindowKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.Radius, WindowType.PowerOfCosine, this.Alpha, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static PowerOfCosine Default => new();
}

#endregion

#region Tukey

/// <summary>
/// Tukey-windowed sinc resampler (tapered cosine).
/// </summary>
/// <remarks>
/// <para>Combines flat-top with cosine tapers at edges.</para>
/// <para>α controls the taper fraction (0=rectangular, 1=Hann).</para>
/// </remarks>
[ScalerInfo("Tukey", Author = "John Tukey", Year = 1967,
  Description = "Tukey-windowed sinc resampler with tapered cosine", Category = ScalerCategory.Resampler)]
public readonly struct Tukey : IResampler {

  /// <summary>
  /// Default α parameter (0.5 = half tapered).
  /// </summary>
  public const float DefaultAlpha = 0.5f;

  private readonly int _radius;
  private readonly float _alpha;

  /// <summary>
  /// Creates a Tukey resampler with radius 3 and default α.
  /// </summary>
  public Tukey() : this(3, DefaultAlpha) { }

  /// <summary>
  /// Creates a Tukey resampler with custom radius and default α.
  /// </summary>
  /// <param name="radius">The filter radius (typically 2, 3, or 4).</param>
  public Tukey(int radius) : this(radius, DefaultAlpha) { }

  /// <summary>
  /// Creates a Tukey resampler with custom radius and α.
  /// </summary>
  /// <param name="radius">The filter radius (typically 2, 3, or 4).</param>
  /// <param name="alpha">The taper fraction (0=rectangular, 1=Hann).</param>
  public Tukey(int radius, float alpha) {
    ArgumentOutOfRangeException.ThrowIfLessThan(radius, 1);
    ArgumentOutOfRangeException.ThrowIfNegative(alpha);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(alpha, 1f);
    this._radius = radius;
    this._alpha = alpha;
  }

  /// <summary>
  /// Gets the α parameter.
  /// </summary>
  public float Alpha => this._alpha == 0f ? DefaultAlpha : this._alpha;

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => this._radius == 0 ? 3 : this._radius;

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
    => callback.Invoke(new SincWindowKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.Radius, WindowType.Tukey, this.Alpha, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Tukey Default => new();
}

#endregion

#region Poisson

/// <summary>
/// Poisson-windowed sinc resampler (exponential window).
/// </summary>
/// <remarks>
/// <para>Exponential decay window: exp(-|x|*d/R).</para>
/// <para>Higher d values provide faster decay.</para>
/// </remarks>
[ScalerInfo("Poisson",
  Description = "Poisson-windowed sinc resampler with exponential decay", Category = ScalerCategory.Resampler)]
public readonly struct Poisson : IResampler {

  /// <summary>
  /// Default decay parameter (d=60 gives ~0 at edges).
  /// </summary>
  public const float DefaultDecay = 60f;

  private readonly int _radius;
  private readonly float _decay;

  /// <summary>
  /// Creates a Poisson resampler with radius 3 and default decay.
  /// </summary>
  public Poisson() : this(3, DefaultDecay) { }

  /// <summary>
  /// Creates a Poisson resampler with custom radius and default decay.
  /// </summary>
  /// <param name="radius">The filter radius (typically 2, 3, or 4).</param>
  public Poisson(int radius) : this(radius, DefaultDecay) { }

  /// <summary>
  /// Creates a Poisson resampler with custom radius and decay.
  /// </summary>
  /// <param name="radius">The filter radius (typically 2, 3, or 4).</param>
  /// <param name="decay">The decay parameter (higher = faster decay).</param>
  public Poisson(int radius, float decay) {
    ArgumentOutOfRangeException.ThrowIfLessThan(radius, 1);
    ArgumentOutOfRangeException.ThrowIfNegative(decay);
    this._radius = radius;
    this._decay = decay;
  }

  /// <summary>
  /// Gets the decay parameter.
  /// </summary>
  public float Decay => this._decay == 0f ? DefaultDecay : this._decay;

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => this._radius == 0 ? 3 : this._radius;

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
    => callback.Invoke(new SincWindowKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.Radius, WindowType.Poisson, this.Decay, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Poisson Default => new();
}

#endregion

#region BartlettHann

/// <summary>
/// Bartlett-Hann-windowed sinc resampler.
/// </summary>
/// <remarks>
/// <para>Hybrid of Bartlett (triangular) and Hann windows.</para>
/// <para>Provides intermediate characteristics between the two.</para>
/// </remarks>
[ScalerInfo("BartlettHann",
  Description = "Bartlett-Hann-windowed sinc resampler (linear + cosine hybrid)", Category = ScalerCategory.Resampler)]
public readonly struct BartlettHann : IResampler {

  private readonly int _radius;

  /// <summary>
  /// Creates a Bartlett-Hann resampler with radius 3 (default).
  /// </summary>
  public BartlettHann() : this(3) { }

  /// <summary>
  /// Creates a Bartlett-Hann resampler with custom radius.
  /// </summary>
  /// <param name="radius">The filter radius (typically 2, 3, or 4).</param>
  public BartlettHann(int radius) {
    ArgumentOutOfRangeException.ThrowIfLessThan(radius, 1);
    this._radius = radius;
  }

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => this._radius == 0 ? 3 : this._radius;

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
    => callback.Invoke(new SincWindowKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.Radius, WindowType.BartlettHann, 0f, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static BartlettHann Default => new();
}

#endregion

#region HanningPoisson

/// <summary>
/// Hanning-Poisson-windowed sinc resampler.
/// </summary>
/// <remarks>
/// <para>Product of Hann and Poisson windows.</para>
/// <para>Combines smooth cosine taper with exponential decay.</para>
/// </remarks>
[ScalerInfo("HanningPoisson",
  Description = "Hanning-Poisson-windowed sinc resampler (Hann × Poisson)", Category = ScalerCategory.Resampler)]
public readonly struct HanningPoisson : IResampler {

  /// <summary>
  /// Default α parameter.
  /// </summary>
  public const float DefaultAlpha = 2f;

  private readonly int _radius;
  private readonly float _alpha;

  /// <summary>
  /// Creates a Hanning-Poisson resampler with radius 3 and default α.
  /// </summary>
  public HanningPoisson() : this(3, DefaultAlpha) { }

  /// <summary>
  /// Creates a Hanning-Poisson resampler with custom radius and default α.
  /// </summary>
  /// <param name="radius">The filter radius (typically 2, 3, or 4).</param>
  public HanningPoisson(int radius) : this(radius, DefaultAlpha) { }

  /// <summary>
  /// Creates a Hanning-Poisson resampler with custom radius and α.
  /// </summary>
  /// <param name="radius">The filter radius (typically 2, 3, or 4).</param>
  /// <param name="alpha">The decay parameter.</param>
  public HanningPoisson(int radius, float alpha) {
    ArgumentOutOfRangeException.ThrowIfLessThan(radius, 1);
    ArgumentOutOfRangeException.ThrowIfNegative(alpha);
    this._radius = radius;
    this._alpha = alpha;
  }

  /// <summary>
  /// Gets the α parameter.
  /// </summary>
  public float Alpha => this._alpha == 0f ? DefaultAlpha : this._alpha;

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => this._radius == 0 ? 3 : this._radius;

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
    => callback.Invoke(new SincWindowKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.Radius, WindowType.HanningPoisson, this.Alpha, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static HanningPoisson Default => new();
}

#endregion

#region Bohman

/// <summary>
/// Bohman-windowed sinc resampler.
/// </summary>
/// <remarks>
/// <para>Convolution of two half-duration cosine windows.</para>
/// <para>Formula: (1-|x/R|)cos(π|x/R|) + sin(π|x/R|)/π.</para>
/// </remarks>
[ScalerInfo("Bohman",
  Description = "Bohman-windowed sinc resampler", Category = ScalerCategory.Resampler)]
public readonly struct Bohman : IResampler {

  private readonly int _radius;

  /// <summary>
  /// Creates a Bohman resampler with radius 3 (default).
  /// </summary>
  public Bohman() : this(3) { }

  /// <summary>
  /// Creates a Bohman resampler with custom radius.
  /// </summary>
  /// <param name="radius">The filter radius (typically 2, 3, or 4).</param>
  public Bohman(int radius) {
    ArgumentOutOfRangeException.ThrowIfLessThan(radius, 1);
    this._radius = radius;
  }

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => this._radius == 0 ? 3 : this._radius;

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
    => callback.Invoke(new SincWindowKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.Radius, WindowType.Bohman, 0f, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Bohman Default => new();
}

#endregion

#region Cauchy

/// <summary>
/// Cauchy-windowed sinc resampler (Lorentzian window).
/// </summary>
/// <remarks>
/// <para>Lorentzian function: 1/(1+(αx/R)²).</para>
/// <para>Higher α values provide narrower main lobe.</para>
/// </remarks>
[ScalerInfo("Cauchy",
  Description = "Cauchy-windowed sinc resampler (Lorentzian)", Category = ScalerCategory.Resampler)]
public readonly struct Cauchy : IResampler {

  /// <summary>
  /// Default α parameter.
  /// </summary>
  public const float DefaultAlpha = 3f;

  private readonly int _radius;
  private readonly float _alpha;

  /// <summary>
  /// Creates a Cauchy resampler with radius 3 and default α.
  /// </summary>
  public Cauchy() : this(3, DefaultAlpha) { }

  /// <summary>
  /// Creates a Cauchy resampler with custom radius and default α.
  /// </summary>
  /// <param name="radius">The filter radius (typically 2, 3, or 4).</param>
  public Cauchy(int radius) : this(radius, DefaultAlpha) { }

  /// <summary>
  /// Creates a Cauchy resampler with custom radius and α.
  /// </summary>
  /// <param name="radius">The filter radius (typically 2, 3, or 4).</param>
  /// <param name="alpha">The shape parameter (higher = narrower).</param>
  public Cauchy(int radius, float alpha) {
    ArgumentOutOfRangeException.ThrowIfLessThan(radius, 1);
    ArgumentOutOfRangeException.ThrowIfNegative(alpha);
    this._radius = radius;
    this._alpha = alpha;
  }

  /// <summary>
  /// Gets the α parameter.
  /// </summary>
  public float Alpha => this._alpha == 0f ? DefaultAlpha : this._alpha;

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => this._radius == 0 ? 3 : this._radius;

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
    => callback.Invoke(new SincWindowKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.Radius, WindowType.Cauchy, this.Alpha, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Cauchy Default => new();
}

#endregion

#region Rectangular

/// <summary>
/// Rectangular-windowed sinc resampler (box window / no windowing).
/// </summary>
/// <remarks>
/// <para>Pure sinc function with no window tapering.</para>
/// <para>Produces the sharpest results but may have significant ringing artifacts.</para>
/// <para>Equivalent to an ideal low-pass filter.</para>
/// </remarks>
[ScalerInfo("Rectangular",
  Description = "Unwindowed sinc resampler (pure sinc / box window)", Category = ScalerCategory.Resampler)]
public readonly struct Rectangular : IResampler {

  private readonly int _radius;

  /// <summary>
  /// Creates a Rectangular resampler with radius 3 (default).
  /// </summary>
  public Rectangular() : this(3) { }

  /// <summary>
  /// Creates a Rectangular resampler with custom radius.
  /// </summary>
  /// <param name="radius">The filter radius (typically 2, 3, or 4).</param>
  public Rectangular(int radius) {
    ArgumentOutOfRangeException.ThrowIfLessThan(radius, 1);
    this._radius = radius;
  }

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => this._radius == 0 ? 3 : this._radius;

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
    => callback.Invoke(new SincWindowKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.Radius, WindowType.Rectangular, 0f, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Rectangular Default => new();
}

#endregion

#region Shared Kernel Infrastructure

file enum WindowType {
  Blackman,
  Hann,
  Hamming,
  Kaiser,
  Welch,
  Bartlett,
  Nuttal,
  BlackmanNuttal,
  BlackmanHarris,
  FlatTop,
  Cosine,
  PowerOfCosine,
  Tukey,
  Poisson,
  BartlettHann,
  HanningPoisson,
  Bohman,
  Cauchy,
  Rectangular
}

file readonly struct SincWindowKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, int radius, WindowType windowType, float param = 0f, bool useCenteredGrid = true)
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

    // Accumulate weighted colors from (2*radius)x(2*radius) kernel
    Accum4F<TWork> acc = default;
    for (var ky = -radius + 1; ky <= radius; ++ky)
    for (var kx = -radius + 1; kx <= radius; ++kx) {
      var weight = this.Weight(fx - kx) * this.Weight(fy - ky);
      if (weight == 0f)
        continue;

      var pixel = frame[srcXi + kx, srcYi + ky].Work;
      acc.AddMul(pixel, weight);
    }

    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
  }

  /// <summary>
  /// Computes the windowed sinc weight for a given distance.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float Weight(float x) {
    if (x == 0f)
      return 1f;
    var absX = MathF.Abs(x);
    if (absX >= radius)
      return 0f;
    var sinc = Sinc(x);
    return sinc * this.Window(x);
  }

  /// <summary>
  /// Computes the normalized sinc function: sin(πx)/(πx).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float Sinc(float x) {
    if (x == 0f)
      return 1f;
    var pix = MathF.PI * x;
    return MathF.Sin(pix) / pix;
  }

  /// <summary>
  /// Computes the window function value for the current window type.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float Window(float x) => windowType switch {
    WindowType.Blackman => this.BlackmanWindow(x),
    WindowType.Hann => this.HannWindow(x),
    WindowType.Hamming => this.HammingWindow(x),
    WindowType.Kaiser => this.KaiserWindow(x, param == 0f ? Kaiser.DefaultBeta : param),
    WindowType.Welch => this.WelchWindow(x),
    WindowType.Bartlett => this.BartlettWindow(x),
    WindowType.Nuttal => this.NuttalWindow(x),
    WindowType.BlackmanNuttal => this.BlackmanNuttalWindow(x),
    WindowType.BlackmanHarris => this.BlackmanHarrisWindow(x),
    WindowType.FlatTop => this.FlatTopWindow(x),
    WindowType.Cosine => this.CosineWindow(x),
    WindowType.PowerOfCosine => this.PowerOfCosineWindow(x, param == 0f ? PowerOfCosine.DefaultAlpha : param),
    WindowType.Tukey => this.TukeyWindow(x, param == 0f ? Tukey.DefaultAlpha : param),
    WindowType.Poisson => this.PoissonWindow(x, param == 0f ? Poisson.DefaultDecay : param),
    WindowType.BartlettHann => this.BartlettHannWindow(x),
    WindowType.HanningPoisson => this.HanningPoissonWindow(x, param == 0f ? HanningPoisson.DefaultAlpha : param),
    WindowType.Bohman => this.BohmanWindow(x),
    WindowType.Cauchy => this.CauchyWindow(x, param == 0f ? Cauchy.DefaultAlpha : param),
    WindowType.Rectangular => 1f, // No windowing - pure sinc
    _ => 1f
  };

  /// <summary>
  /// Blackman window: 0.42 + 0.5*cos(πx/R) + 0.08*cos(2πx/R)
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float BlackmanWindow(float x) {
    var t = MathF.PI * x / radius;
    return 0.42f + 0.5f * MathF.Cos(t) + 0.08f * MathF.Cos(2f * t);
  }

  /// <summary>
  /// Hann window: 0.5*(1 + cos(πx/R))
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float HannWindow(float x) {
    var t = MathF.PI * x / radius;
    return 0.5f * (1f + MathF.Cos(t));
  }

  /// <summary>
  /// Hamming window: 0.54 + 0.46*cos(πx/R)
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float HammingWindow(float x) {
    var t = MathF.PI * x / radius;
    return 0.54f + 0.46f * MathF.Cos(t);
  }

  /// <summary>
  /// Kaiser window: I₀(β√(1-(x/R)²))/I₀(β)
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float KaiserWindow(float x, float beta) {
    var r = x / radius;
    var arg = 1f - r * r;
    if (arg <= 0f)
      return 0f;
    return BesselI0(beta * MathF.Sqrt(arg)) / BesselI0(beta);
  }

  /// <summary>
  /// Welch window (parabolic): 1 - (x/R)²
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float WelchWindow(float x) {
    var r = x / radius;
    return 1f - r * r;
  }

  /// <summary>
  /// Bartlett window (triangular): 1 - |x/R|
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float BartlettWindow(float x) => 1f - MathF.Abs(x) / radius;

  /// <summary>
  /// Nuttal window: 4-term cosine sum.
  /// </summary>
  /// <remarks>
  /// Coefficients: a₀=0.355768, a₁=0.487396, a₂=0.144232, a₃=0.012604.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float NuttalWindow(float x) {
    var n = x + radius;
    var N = 2f * radius;
    var t = 2f * MathF.PI * n / N;
    return 0.355768f - 0.487396f * MathF.Cos(t) + 0.144232f * MathF.Cos(2f * t) - 0.012604f * MathF.Cos(3f * t);
  }

  /// <summary>
  /// Blackman-Nuttal window: 4-term cosine sum.
  /// </summary>
  /// <remarks>
  /// Coefficients: a₀=0.3635819, a₁=0.4891775, a₂=0.1365995, a₃=0.0106411.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float BlackmanNuttalWindow(float x) {
    var n = x + radius;
    var N = 2f * radius;
    var t = 2f * MathF.PI * n / N;
    return 0.3635819f - 0.4891775f * MathF.Cos(t) + 0.1365995f * MathF.Cos(2f * t) - 0.0106411f * MathF.Cos(3f * t);
  }

  /// <summary>
  /// Blackman-Harris window: 4-term cosine sum with minimum sidelobes.
  /// </summary>
  /// <remarks>
  /// Coefficients: a₀=0.35875, a₁=0.48829, a₂=0.14128, a₃=0.01168.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float BlackmanHarrisWindow(float x) {
    var n = x + radius;
    var N = 2f * radius;
    var t = 2f * MathF.PI * n / N;
    return 0.35875f - 0.48829f * MathF.Cos(t) + 0.14128f * MathF.Cos(2f * t) - 0.01168f * MathF.Cos(3f * t);
  }

  /// <summary>
  /// FlatTop window: 5-term cosine sum with flat passband.
  /// </summary>
  /// <remarks>
  /// Coefficients: a₀=1.0, a₁=1.93, a₂=1.29, a₃=0.388, a₄=0.0322.
  /// Normalized to have peak value of 1.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float FlatTopWindow(float x) {
    var n = x + radius;
    var N = 2f * radius;
    var t = 2f * MathF.PI * n / N;
    var raw = 1f - 1.93f * MathF.Cos(t) + 1.29f * MathF.Cos(2f * t)
              - 0.388f * MathF.Cos(3f * t) + 0.0322f * MathF.Cos(4f * t);
    // Normalize (max value is ~4.636 at center)
    return raw / 4.636f;
  }

  /// <summary>
  /// Cosine window: cos(πx/2R).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float CosineWindow(float x) {
    var t = MathF.PI * x / (2f * radius);
    return MathF.Cos(t);
  }

  /// <summary>
  /// Power-of-Cosine window: cos^α(πx/2R).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float PowerOfCosineWindow(float x, float alpha) {
    var t = MathF.PI * x / (2f * radius);
    return MathF.Pow(MathF.Cos(t), alpha);
  }

  /// <summary>
  /// Tukey window (tapered cosine): flat center with cosine tapers.
  /// </summary>
  /// <remarks>
  /// α=0 gives rectangular window, α=1 gives Hann window.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float TukeyWindow(float x, float alpha) {
    var absX = MathF.Abs(x);
    var r = absX / radius;
    if (r <= 1f - alpha)
      return 1f;
    if (r <= 1f) {
      var t = MathF.PI * (r - (1f - alpha)) / alpha;
      return 0.5f * (1f + MathF.Cos(t));
    }
    return 0f;
  }

  /// <summary>
  /// Poisson window (exponential): exp(-|x|*d/(N-1)).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float PoissonWindow(float x, float decay) {
    var n = x + radius;
    var N = 2f * radius;
    var tau = (N - 1f) * 0.5f / decay;
    return MathF.Exp(-MathF.Abs(n - (N - 1f) * 0.5f) / tau);
  }

  /// <summary>
  /// Bartlett-Hann window: hybrid of Bartlett and Hann.
  /// </summary>
  /// <remarks>
  /// Formula: a₀ - a₁*|n/(N-1) - 0.5| - a₂*cos(2πn/(N-1))
  /// where a₀=0.62, a₁=0.48, a₂=0.38.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float BartlettHannWindow(float x) {
    var n = x + radius;
    var N = 2f * radius;
    var t = n / N;
    return 0.62f - 0.48f * MathF.Abs(t - 0.5f) - 0.38f * MathF.Cos(2f * MathF.PI * t);
  }

  /// <summary>
  /// Hanning-Poisson window: Hann × Poisson.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float HanningPoissonWindow(float x, float alpha) {
    var n = x + radius;
    var N = 2f * radius;
    var hann = 0.5f * (1f - MathF.Cos(2f * MathF.PI * n / N));
    var poisson = MathF.Exp(-alpha * MathF.Abs(N - 1f - 2f * n) / (N - 1f));
    return hann * poisson;
  }

  /// <summary>
  /// Bohman window: (1-|x/R|)cos(π|x/R|) + sin(π|x/R|)/π.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float BohmanWindow(float x) {
    var f = MathF.Abs(x / radius);
    if (f >= 1f)
      return 0f;
    var pif = MathF.PI * f;
    return (1f - f) * MathF.Cos(pif) + MathF.Sin(pif) / MathF.PI;
  }

  /// <summary>
  /// Cauchy window (Lorentzian): 1/(1+(αx/R)²).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float CauchyWindow(float x, float alpha) {
    var r = alpha * x / radius;
    return 1f / (1f + r * r);
  }

  /// <summary>
  /// Modified Bessel function of the first kind, order 0.
  /// </summary>
  /// <remarks>
  /// Polynomial approximation from Numerical Recipes in C.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float BesselI0(float x) {
    var ax = MathF.Abs(x);
    if (ax < 3.75f) {
      var y = x / 3.75f;
      y *= y;
      return 1f + y * (3.5156229f + y * (3.0899424f + y * (1.2067492f
        + y * (0.2659732f + y * (0.0360768f + y * 0.0045813f)))));
    }

    var ay = 3.75f / ax;
    return MathF.Exp(ax) / MathF.Sqrt(ax) * (0.39894228f
      + ay * (0.01328592f + ay * (0.00225319f + ay * (-0.00157565f
      + ay * (0.00916281f + ay * (-0.02057706f + ay * (0.02635537f
      + ay * (-0.01647633f + ay * 0.00392377f))))))));
  }
}

#endregion
