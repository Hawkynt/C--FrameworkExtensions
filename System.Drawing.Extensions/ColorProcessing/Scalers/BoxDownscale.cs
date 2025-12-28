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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Pipeline;
using Hawkynt.ColorProcessing.Storage;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Scalers;

/// <summary>
/// Box (area) downscaling algorithm with configurable block size.
/// </summary>
/// <remarks>
/// <para>Reduces image size by averaging NxN blocks of source pixels into single output pixels.</para>
/// <para>
/// Box filtering provides simple but effective downscaling that preserves overall luminance
/// and produces smooth results without aliasing artifacts.
/// </para>
/// <para>Block sizes of 2 (1/2 scale), 3 (1/3 scale), 4 (1/4 scale), and 5 (1/5 scale) are supported.</para>
/// <para>Uses IAccum&lt;TAccum, TWork&gt; for zero-overhead weighted accumulation with proper precision.</para>
/// </remarks>
[ScalerInfo("BoxDownscale", Description = "Box filter downscaling with configurable ratio", Category = ScalerCategory.Resampler)]
public readonly struct BoxDownscale : IDownscaler {

  private readonly int _ratioX;
  private readonly int _ratioY;

  /// <summary>
  /// Creates a box downscaler with the specified ratio.
  /// </summary>
  /// <param name="ratioX">Horizontal downscale ratio (2 = 1/2, 3 = 1/3, etc.). Maximum is 5.</param>
  /// <param name="ratioY">Vertical downscale ratio (2 = 1/2, 3 = 1/3, etc.). Maximum is 5.</param>
  /// <exception cref="ArgumentOutOfRangeException">Thrown when ratio is less than 2 or greater than 5.</exception>
  public BoxDownscale(int ratioX, int ratioY) {
    ArgumentOutOfRangeException.ThrowIfLessThan(ratioX, 2);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(ratioX, 5);
    ArgumentOutOfRangeException.ThrowIfLessThan(ratioY, 2);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(ratioY, 5);

    this._ratioX = ratioX;
    this._ratioY = ratioY;
  }

  /// <summary>
  /// Creates a box downscaler with uniform ratio in both dimensions.
  /// </summary>
  /// <param name="ratio">Downscale ratio for both dimensions (2 = 1/2, 3 = 1/3, etc.). Maximum is 5.</param>
  public BoxDownscale(int ratio) : this(ratio, ratio) { }

  /// <inheritdoc />
  public int RatioX => this._ratioX;

  /// <inheritdoc />
  public int RatioY => this._ratioY;

  /// <inheritdoc />
  /// <remarks>
  /// For downscaling, Scale represents the output-to-input ratio.
  /// A ratio of 2 means each output pixel represents 2 input pixels,
  /// so the effective scale is (1, 1) since each output block covers 2x2 source.
  /// We return (1, 1) to indicate this is a shrinking operation.
  /// </remarks>
  public ScaleFactor Scale => new(1, 1);

  /// <summary>
  /// Gets the list of commonly used downscale ratios.
  /// </summary>
  public static int[] SupportedRatios { get; } = [2, 3, 4, 5];

  /// <summary>
  /// Determines whether BoxDownscale supports the specified ratio.
  /// </summary>
  /// <param name="ratio">The downscale ratio to check.</param>
  /// <returns><c>true</c> if ratio is between 2 and 5 (inclusive); otherwise, <c>false</c>.</returns>
  public static bool SupportsRatio(int ratio) => ratio is >= 2 and <= 5;

  /// <summary>
  /// Enumerates common target dimensions for BoxDownscale.
  /// </summary>
  /// <param name="sourceWidth">The source image width.</param>
  /// <param name="sourceHeight">The source image height.</param>
  /// <returns>Common target dimensions (1/2, 1/3, 1/4, 1/5 in both dimensions).</returns>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    if (sourceWidth >= 4 && sourceHeight >= 4)
      yield return (sourceWidth / 2, sourceHeight / 2);
    if (sourceWidth >= 6 && sourceHeight >= 6)
      yield return (sourceWidth / 3, sourceHeight / 3);
    if (sourceWidth >= 8 && sourceHeight >= 8)
      yield return (sourceWidth / 4, sourceHeight / 4);
    if (sourceWidth >= 10 && sourceHeight >= 10)
      yield return (sourceWidth / 5, sourceHeight / 5);
  }

  #region Static Presets

  /// <summary>Gets a box downscaler at 1/2 scale (2x2 blocks).</summary>
  public static BoxDownscale Ratio2 => new(2);

  /// <summary>Gets a box downscaler at 1/3 scale (3x3 blocks).</summary>
  public static BoxDownscale Ratio3 => new(3);

  /// <summary>Gets a box downscaler at 1/4 scale (4x4 blocks).</summary>
  public static BoxDownscale Ratio4 => new(4);

  /// <summary>Gets a box downscaler at 1/5 scale (5x5 blocks).</summary>
  public static BoxDownscale Ratio5 => new(5);

  /// <summary>Gets the default BoxDownscale configuration (1/2 scale).</summary>
  public static BoxDownscale Default => Ratio2;

  #endregion

  /// <inheritdoc />
  /// <remarks>
  /// BoxDownscale uses Accum4F internally for 4-component color spaces.
  /// TWork must implement <see cref="IColorSpace4F{TWork}"/>.
  /// </remarks>
  public TResult InvokeKernel<TWork, TKey, TPixel, TEncode, TResult>(
    IDownscaleKernelCallback<TWork, TKey, TPixel, TEncode, TResult> callback)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TEncode : struct, IEncode<TWork, TPixel>
    => (this._ratioX, this._ratioY) switch {
      (2, 2) => callback.Invoke(new Box2x2Kernel<TWork, TKey, TPixel, TEncode>()),
      (3, 3) => callback.Invoke(new Box3x3Kernel<TWork, TKey, TPixel, TEncode>()),
      (4, 4) => callback.Invoke(new Box4x4Kernel<TWork, TKey, TPixel, TEncode>()),
      (5, 5) => callback.Invoke(new Box5x5Kernel<TWork, TKey, TPixel, TEncode>()),
      _ => throw new InvalidOperationException($"No kernel available for ratio {this._ratioX}x{this._ratioY}")
    };
}

#region Box Kernels

/// <summary>
/// Box 2x2 kernel for 2:1 downscaling (4 pixels → 1 pixel).
/// </summary>
/// <remarks>
/// Reads P0P0, P0P1, P1P0, P1P1 from the window (positions 0,0 to 1,1).
/// Uses internal accumulation with <see cref="Accum4F{TColor}"/>.
/// </remarks>
file readonly struct Box2x2Kernel<TWork, TKey, TPixel, TEncode> : IDownscaleKernel<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  /// <inheritdoc />
  public int RatioX => 2;

  /// <inheritdoc />
  public int RatioY => 2;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TPixel Average(in NeighborWindow<TWork, TKey> w, in TEncode encoder) {
    Accum4F<TWork> acc = default;
    acc.AddMul(w.P0P0.Work, 1f);
    acc.AddMul(w.P0P1.Work, 1f);
    acc.AddMul(w.P1P0.Work, 1f);
    acc.AddMul(w.P1P1.Work, 1f);
    return encoder.Encode(acc.Result);
  }
}

/// <summary>
/// Box 3x3 kernel for 3:1 downscaling (9 pixels → 1 pixel).
/// </summary>
/// <remarks>
/// Reads M1M1 through P1P1 from the window (positions -1,-1 to 1,1).
/// Uses internal accumulation with <see cref="Accum4F{TColor}"/>.
/// </remarks>
file readonly struct Box3x3Kernel<TWork, TKey, TPixel, TEncode> : IDownscaleKernel<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  /// <inheritdoc />
  public int RatioX => 3;

  /// <inheritdoc />
  public int RatioY => 3;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TPixel Average(in NeighborWindow<TWork, TKey> w, in TEncode encoder) {
    Accum4F<TWork> acc = default;
    // Row M1 (y = -1)
    acc.AddMul(w.M1M1.Work, 1f);
    acc.AddMul(w.M1P0.Work, 1f);
    acc.AddMul(w.M1P1.Work, 1f);
    // Row P0 (y = 0)
    acc.AddMul(w.P0M1.Work, 1f);
    acc.AddMul(w.P0P0.Work, 1f);
    acc.AddMul(w.P0P1.Work, 1f);
    // Row P1 (y = 1)
    acc.AddMul(w.P1M1.Work, 1f);
    acc.AddMul(w.P1P0.Work, 1f);
    acc.AddMul(w.P1P1.Work, 1f);
    return encoder.Encode(acc.Result);
  }
}

/// <summary>
/// Box 4x4 kernel for 4:1 downscaling (16 pixels → 1 pixel).
/// </summary>
/// <remarks>
/// Reads M1M1 through P2P2 from the window (positions -1,-1 to 2,2).
/// Uses internal accumulation with <see cref="Accum4F{TColor}"/>.
/// </remarks>
file readonly struct Box4x4Kernel<TWork, TKey, TPixel, TEncode> : IDownscaleKernel<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  /// <inheritdoc />
  public int RatioX => 4;

  /// <inheritdoc />
  public int RatioY => 4;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TPixel Average(in NeighborWindow<TWork, TKey> w, in TEncode encoder) {
    Accum4F<TWork> acc = default;
    // Row M1 (y = -1)
    acc.AddMul(w.M1M1.Work, 1f);
    acc.AddMul(w.M1P0.Work, 1f);
    acc.AddMul(w.M1P1.Work, 1f);
    acc.AddMul(w.M1P2.Work, 1f);
    // Row P0 (y = 0)
    acc.AddMul(w.P0M1.Work, 1f);
    acc.AddMul(w.P0P0.Work, 1f);
    acc.AddMul(w.P0P1.Work, 1f);
    acc.AddMul(w.P0P2.Work, 1f);
    // Row P1 (y = 1)
    acc.AddMul(w.P1M1.Work, 1f);
    acc.AddMul(w.P1P0.Work, 1f);
    acc.AddMul(w.P1P1.Work, 1f);
    acc.AddMul(w.P1P2.Work, 1f);
    // Row P2 (y = 2)
    acc.AddMul(w.P2M1.Work, 1f);
    acc.AddMul(w.P2P0.Work, 1f);
    acc.AddMul(w.P2P1.Work, 1f);
    acc.AddMul(w.P2P2.Work, 1f);
    return encoder.Encode(acc.Result);
  }
}

/// <summary>
/// Box 5x5 kernel for 5:1 downscaling (25 pixels → 1 pixel).
/// </summary>
/// <remarks>
/// Reads M2M2 through P2P2 from the window (full 5x5, positions -2,-2 to 2,2).
/// Uses internal accumulation with <see cref="Accum4F{TColor}"/>.
/// </remarks>
file readonly struct Box5x5Kernel<TWork, TKey, TPixel, TEncode> : IDownscaleKernel<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  /// <inheritdoc />
  public int RatioX => 5;

  /// <inheritdoc />
  public int RatioY => 5;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TPixel Average(in NeighborWindow<TWork, TKey> w, in TEncode encoder) {
    Accum4F<TWork> acc = default;
    // Row M2 (y = -2)
    acc.AddMul(w.M2M2.Work, 1f);
    acc.AddMul(w.M2M1.Work, 1f);
    acc.AddMul(w.M2P0.Work, 1f);
    acc.AddMul(w.M2P1.Work, 1f);
    acc.AddMul(w.M2P2.Work, 1f);
    // Row M1 (y = -1)
    acc.AddMul(w.M1M2.Work, 1f);
    acc.AddMul(w.M1M1.Work, 1f);
    acc.AddMul(w.M1P0.Work, 1f);
    acc.AddMul(w.M1P1.Work, 1f);
    acc.AddMul(w.M1P2.Work, 1f);
    // Row P0 (y = 0)
    acc.AddMul(w.P0M2.Work, 1f);
    acc.AddMul(w.P0M1.Work, 1f);
    acc.AddMul(w.P0P0.Work, 1f);
    acc.AddMul(w.P0P1.Work, 1f);
    acc.AddMul(w.P0P2.Work, 1f);
    // Row P1 (y = 1)
    acc.AddMul(w.P1M2.Work, 1f);
    acc.AddMul(w.P1M1.Work, 1f);
    acc.AddMul(w.P1P0.Work, 1f);
    acc.AddMul(w.P1P1.Work, 1f);
    acc.AddMul(w.P1P2.Work, 1f);
    // Row P2 (y = 2)
    acc.AddMul(w.P2M2.Work, 1f);
    acc.AddMul(w.P2M1.Work, 1f);
    acc.AddMul(w.P2P0.Work, 1f);
    acc.AddMul(w.P2P1.Work, 1f);
    acc.AddMul(w.P2P2.Work, 1f);
    return encoder.Encode(acc.Result);
  }
}

#endregion
