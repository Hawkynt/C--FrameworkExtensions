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
using Hawkynt.ColorProcessing.FrequencyDomain;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Resamplers;

/// <summary>
/// Wiener-deconvolution resampler — frequency-domain inverse-filtering against an assumed
/// Gaussian PSF.
/// </summary>
/// <remarks>
/// <para>The Wiener filter recovers detail lost to a known point-spread function (here a
/// Gaussian) while regularising against noise:</para>
/// <code>Ŵ(u, v) = H*(u, v) / (|H(u, v)|² + 1/SNR)</code>
/// <para>This implementation precomputes the Wiener inverse as a small spatial-domain kernel
/// at construction time — one 16×16 FFT/IFFT pair via the library's <see cref="Fft2D"/> —
/// then applies it via spatial convolution per output pixel. That sidesteps having to FFT
/// the whole image at run time while preserving the frequency-domain shape of the filter.</para>
/// <para>References: Wiener 1949, "Extrapolation, Interpolation, and Smoothing of Stationary
/// Time Series" (MIT Press). Standard frequency-domain SR primitive in optics / astronomy /
/// microscopy.</para>
/// </remarks>
[ScalerInfo("WienerDeconvolution", Author = "Wiener", Year = 1949,
  Description = "Wiener inverse-filter deconvolution against a Gaussian PSF",
  Category = ScalerCategory.Resampler)]
public readonly struct WienerDeconvolution : IResampler {

  /// <summary>Default Gaussian PSF σ (1.0 pixel).</summary>
  public const float DefaultPsfSigma = 1.0f;

  /// <summary>Default signal-to-noise ratio (100).</summary>
  public const float DefaultSnr = 100f;

  /// <summary>Spatial convolution radius — half of the precomputed FFT grid.</summary>
  public const int KernelRadius = 4;

  private const int FftSize = 16;

  private readonly float _psfSigma;
  private readonly float _snr;
  private readonly float[] _kernel;

  /// <summary>Creates a Wiener resampler with default σ and SNR.</summary>
  public WienerDeconvolution() : this(DefaultPsfSigma, DefaultSnr) { }

  /// <summary>Creates a Wiener resampler with custom σ and SNR.</summary>
  /// <param name="psfSigma">Assumed Gaussian-PSF σ in pixels (must be &gt; 0, ≤ 5).</param>
  /// <param name="snr">Signal-to-noise ratio (must be &gt; 0, typically 10..1000). Lower SNR
  /// = stronger noise suppression at the cost of detail recovery.</param>
  public WienerDeconvolution(float psfSigma, float snr) {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(psfSigma);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(psfSigma, 5f);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(snr);
    this._psfSigma = psfSigma;
    this._snr = snr;
    this._kernel = ComputeWienerKernel(psfSigma, snr);
  }

  /// <summary>Gets the Gaussian PSF σ.</summary>
  public float PsfSigma => this._psfSigma == 0f ? DefaultPsfSigma : this._psfSigma;

  /// <summary>Gets the signal-to-noise ratio.</summary>
  public float Snr => this._snr == 0f ? DefaultSnr : this._snr;

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => KernelRadius;

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
    => callback.Invoke(new WienerKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this._kernel ?? ComputeWienerKernel(DefaultPsfSigma, DefaultSnr), useCenteredGrid));

  /// <summary>Gets the default configuration (σ=1.0, SNR=100).</summary>
  public static WienerDeconvolution Default => new();

  /// <summary>Stronger sharpening (σ=1.5, SNR=200).</summary>
  public static WienerDeconvolution Strong => new(1.5f, 200f);

  /// <summary>Softer sharpening (σ=0.7, SNR=50).</summary>
  public static WienerDeconvolution Soft => new(0.7f, 50f);

  /// <summary>
  /// Builds the spatial-domain Wiener kernel by FFT-inverting the regularised PSF spectrum.
  /// Returns a flat float[FftSize × FftSize] in row-major order, fft-shifted so that the
  /// centre tap sits at index [r=KernelRadius, c=KernelRadius].
  /// </summary>
  private static float[] ComputeWienerKernel(float psfSigma, float snr) {
    // Build a centred Gaussian PSF on a FftSize × FftSize grid. Centre at (FftSize/2,
    // FftSize/2) so the FFT result has linear phase that we'll undo via fft-shift on the
    // way back.
    var psf = new Complex[FftSize, FftSize];
    var c = FftSize / 2;
    var twoSigSq = 2f * psfSigma * psfSigma;
    var sum = 0f;
    for (var y = 0; y < FftSize; ++y)
    for (var x = 0; x < FftSize; ++x) {
      var dx = x - c;
      var dy = y - c;
      var v = MathF.Exp(-(dx * dx + dy * dy) / twoSigSq);
      psf[y, x] = new Complex(v, 0f);
      sum += v;
    }
    // Normalise PSF energy so it integrates to 1 (preserves DC after deconvolution).
    var invSum = 1f / sum;
    for (var y = 0; y < FftSize; ++y)
    for (var x = 0; x < FftSize; ++x)
      psf[y, x] = new Complex(psf[y, x].Real * invSum, 0f);

    Fft2D.Forward(psf);

    // Wiener spectrum: H* / (|H|² + 1/SNR).
    var noiseFloor = 1f / snr;
    for (var y = 0; y < FftSize; ++y)
    for (var x = 0; x < FftSize; ++x) {
      var h = psf[y, x];
      var magSq = h.Real * h.Real + h.Imaginary * h.Imaginary;
      var denom = magSq + noiseFloor;
      var inv = 1f / denom;
      // H* = (H.Real, -H.Imaginary). Multiply by 1/denom (scalar).
      psf[y, x] = new Complex(h.Real * inv, -h.Imaginary * inv);
    }

    Fft2D.Inverse(psf);

    // The spatial Wiener filter is symmetric and largely real for a centred symmetric PSF.
    // Extract the real part and fft-shift so the centre tap is at index [c, c].
    var kernel = new float[FftSize * FftSize];
    for (var y = 0; y < FftSize; ++y)
    for (var x = 0; x < FftSize; ++x) {
      var sy = (y + c) % FftSize;
      var sx = (x + c) % FftSize;
      kernel[y * FftSize + x] = psf[sy, sx].Real;
    }

    // Renormalise the truncated 9×9 window to sum=1: this preserves the DC-component
    // (solid input → solid output) and uniformly scales kernel coefficients without
    // changing relative magnitudes — the centre-tap-vs-side-lobe RATIO is preserved,
    // which is what gives Wiener its high-frequency boost. The full FFT kernel sums to
    // ~1 only over its INFINITE support; on the finite 9×9 window we explicitly enforce it.
    const int radius = 4;
    var centerSum = 0f;
    for (var y = c - radius; y <= c + radius; ++y)
    for (var x = c - radius; x <= c + radius; ++x)
      centerSum += kernel[y * FftSize + x];
    if (centerSum != 0f) {
      var norm = 1f / centerSum;
      for (var i = 0; i < kernel.Length; ++i)
        kernel[i] *= norm;
    }
    return kernel;
  }
}

file readonly struct WienerKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight,
  float[] kernel, bool useCenteredGrid)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  private const int FftSize = 16;
  private const int KernelRadius = 4;

  public int Radius => KernelRadius;
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

    // Convolve a (2·R+1) × (2·R+1) source neighbourhood centred at (x0, y0) with the
    // precomputed Wiener kernel. The kernel is FftSize × FftSize (16×16); we use only the
    // central (2·R+1) × (2·R+1) = 9×9 taps — the rest decays to ~0 for typical σ. Kernel
    // sums to 1 (renormalised in CreateKernel), so Accum4F.Result is effectively a
    // pure convolution (its divide-by-weight-sum is a no-op).
    Accum4F<TWork> acc = default;
    for (var ky = -KernelRadius; ky <= KernelRadius; ++ky)
    for (var kx = -KernelRadius; kx <= KernelRadius; ++kx) {
      var w = kernel[(ky + KernelRadius) * FftSize + (kx + KernelRadius)];
      if (w == 0f) continue;
      acc.AddMul(frame[x0 + kx, y0 + ky].Work, w);
    }

    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
  }
}
