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
using Hawkynt.ColorProcessing.Constants;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Resizing;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Filtering.Filters;

/// <summary>
/// Laplacian-of-Gaussian (LoG) edge / blob detector with configurable sigma.
/// </summary>
/// <remarks>
/// <para>
/// The LoG kernel <c>∇²G(σ)</c> combines Gaussian pre-smoothing with the
/// Laplacian second-derivative in a single convolution, giving noise-robust
/// edge and blob detection. Unlike the plain <see cref="LaplacianEdge"/>,
/// LoG suppresses high-frequency noise before taking the second derivative,
/// so it is the de-facto choice for feature detection in photographic images
/// and the foundation of the Marr–Hildreth zero-crossing edge detector and
/// the SIFT feature pyramid.
/// </para>
/// <para>
/// The 2D kernel is
/// <c>LoG(x,y;σ) = -(1 / (π σ⁴)) · (1 - (x² + y²) / (2σ²)) · exp(-(x² + y²) / (2σ²))</c>.
/// The kernel radius is automatically sized to <c>⌈3σ⌉</c> (covering ≈99.7 %
/// of the Gaussian mass). All kernels are zero-mean so the output is signed;
/// the absolute value is emitted so both bright-on-dark and dark-on-bright
/// blobs appear as positive responses.
/// </para>
/// <para>
/// Typical use case: scale-aware blob detection, noise-tolerant edge maps,
/// pre-processing for zero-crossing contour extraction.
/// </para>
/// <para>
/// Reference: Marr, D. &amp; Hildreth, E. (1980) <em>Theory of Edge
/// Detection</em>, Proc. Royal Society B 207, 187–217.
/// </para>
/// </remarks>
[FilterInfo("LaplacianOfGaussian",
  Description = "Laplacian-of-Gaussian edge/blob detector with configurable sigma", Category = FilterCategory.Analysis,
  Author = "David Marr, Ellen Hildreth", Year = 1980)]
public readonly struct LaplacianOfGaussian : IPixelFilter, IFrameFilter {
  private readonly float _sigma;
  private readonly int _radius;
  private readonly float[] _kernel;
  private readonly float _gain;

  public LaplacianOfGaussian() : this(1.4f, 1f) { }

  /// <summary>
  /// Initializes a new Laplacian-of-Gaussian filter.
  /// </summary>
  /// <param name="sigma">
  /// Standard deviation of the underlying Gaussian. Larger values detect
  /// coarser features. Values below <c>0.5</c> degenerate to a bare
  /// Laplacian; typical useful range is <c>0.8 .. 3</c>.
  /// </param>
  /// <param name="gain">
  /// Linear multiplier applied to the response before clamping to
  /// <c>[0, 1]</c>. Useful because LoG responses shrink rapidly with sigma.
  /// </param>
  public LaplacianOfGaussian(float sigma, float gain = 1f) {
    this._sigma = Math.Max(0.3f, sigma);
    this._gain = Math.Max(0f, gain);
    this._radius = Math.Max(1, (int)Math.Ceiling(3.0 * this._sigma));
    this._kernel = _BuildKernel(this._sigma, this._radius);
  }

  /// <inheritdoc />
  public bool UsesFrameAccess => this._radius > 2;

  /// <inheritdoc />
  public TResult InvokeKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode, TResult>(
    IKernelCallback<TWork, TKey, TPixel, TEncode, TResult> callback,
    TEquality equality = default,
    TLerp lerp = default)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDistance : struct, IColorMetric<TKey>, INormalizedMetric
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new LogKernel<TWork, TKey, TPixel, TEncode>(this._kernel, this._radius, this._gain));

  /// <inheritdoc />
  public TResult InvokeFrameKernel<TWork, TKey, TPixel, TDecode, TProject, TEncode, TResult>(
    IResampleKernelCallback<TWork, TKey, TPixel, TDecode, TProject, TEncode, TResult> callback,
    int sourceWidth, int sourceHeight)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new LogFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._kernel, this._radius, this._gain, sourceWidth, sourceHeight));

  /// <summary>
  /// Builds a zero-mean 2D LoG kernel laid out row-major at <c>(2R+1)²</c>
  /// entries, where index <c>(dy + R) * (2R + 1) + (dx + R)</c> holds the
  /// weight for offset <c>(dx, dy)</c>.
  /// </summary>
  private static float[] _BuildKernel(float sigma, int radius) {
    var size = 2 * radius + 1;
    var kernel = new float[size * size];
    double s2 = sigma * sigma;
    double inv2s2 = 1.0 / (2.0 * s2);
    double norm = -1.0 / (Math.PI * s2 * s2);

    double sum = 0.0;
    for (var dy = -radius; dy <= radius; ++dy)
    for (var dx = -radius; dx <= radius; ++dx) {
      double r2 = dx * dx + dy * dy;
      double w = norm * (1.0 - r2 * inv2s2) * Math.Exp(-r2 * inv2s2);
      kernel[(dy + radius) * size + (dx + radius)] = (float)w;
      sum += w;
    }

    // Force zero-mean (numerical correction so DC input ⇒ 0 response).
    var mean = (float)(sum / (size * size));
    for (var i = 0; i < kernel.Length; ++i)
      kernel[i] -= mean;

    return kernel;
  }

  /// <summary>Gets the default LoG filter with σ = 1.4.</summary>
  public static LaplacianOfGaussian Default => new();
}

file readonly struct LogKernel<TWork, TKey, TPixel, TEncode>(float[] kernel, int radius, float gain)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 1;
  public int ScaleY => 1;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Lum(in NeighborPixel<TWork, TKey> p) {
    var px = p.Work;
    var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in px);
    return ColorMatrices.BT601_R * r + ColorMatrices.BT601_G * g + ColorMatrices.BT601_B * b;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var size = 2 * radius + 1;
    var sum = 0f;

    // Small-window path: convolve only the 5x5 subset provided by NeighborWindow.
    // radius is guaranteed <= 2 here (IFrameFilter.UsesFrameAccess routes larger
    // kernels through the frame path).
    for (var dy = -radius; dy <= radius; ++dy)
    for (var dx = -radius; dx <= radius; ++dx) {
      var w = kernel[(dy + radius) * size + (dx + radius)];
      if (w == 0f)
        continue;

      var l = _Lum(_Sample(window, dy, dx));
      sum += l * w;
    }

    var v = Math.Min(1f, Math.Max(0f, Math.Abs(sum) * gain));
    var center = window.P0P0.Work;
    var (_, _, _, ca) = ColorConverter.GetNormalizedRgba(in center);
    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(v, v, v, ca));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static NeighborPixel<TWork, TKey> _Sample(in NeighborWindow<TWork, TKey> w, int dy, int dx)
    => (dy, dx) switch {
      (-2, -2) => w.M2M2, (-2, -1) => w.M2M1, (-2, 0) => w.M2P0, (-2, 1) => w.M2P1, (-2, 2) => w.M2P2,
      (-1, -2) => w.M1M2, (-1, -1) => w.M1M1, (-1, 0) => w.M1P0, (-1, 1) => w.M1P1, (-1, 2) => w.M1P2,
      (0, -2) => w.P0M2, (0, -1) => w.P0M1, (0, 0) => w.P0P0, (0, 1) => w.P0P1, (0, 2) => w.P0P2,
      (1, -2) => w.P1M2, (1, -1) => w.P1M1, (1, 0) => w.P1P0, (1, 1) => w.P1P1, (1, 2) => w.P1P2,
      (2, -2) => w.P2M2, (2, -1) => w.P2M1, (2, 0) => w.P2P0, (2, 1) => w.P2P1, (2, 2) => w.P2P2,
      _ => w.P0P0,
    };
}

file readonly struct LogFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  float[] kernel, int radius, float gain, int sourceWidth, int sourceHeight)
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
  public int TargetWidth => sourceWidth;
  public int TargetHeight => sourceHeight;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    var size = 2 * radius + 1;
    var sum = 0f;

    for (var dy = -radius; dy <= radius; ++dy)
    for (var dx = -radius; dx <= radius; ++dx) {
      var w = kernel[(dy + radius) * size + (dx + radius)];
      if (w == 0f)
        continue;

      var px = frame[destX + dx, destY + dy].Work;
      var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in px);
      var lum = ColorMatrices.BT601_R * r + ColorMatrices.BT601_G * g + ColorMatrices.BT601_B * b;
      sum += lum * w;
    }

    var v = Math.Min(1f, Math.Max(0f, Math.Abs(sum) * gain));
    var center = frame[destX, destY].Work;
    var (_, _, _, ca) = ColorConverter.GetNormalizedRgba(in center);
    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(v, v, v, ca));
  }
}
