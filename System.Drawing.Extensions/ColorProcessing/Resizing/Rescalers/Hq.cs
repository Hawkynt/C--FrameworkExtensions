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
using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Storage;
using Hawkynt.ColorProcessing.Working;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Rescalers;

/// <summary>
/// Specifies the quality level for HQ/LQ scalers.
/// </summary>
public enum HqQuality {
  /// <summary>
  /// High quality mode with full interpolation patterns.
  /// </summary>
  High,

  /// <summary>
  /// Low quality mode with simplified interpolation.
  /// Faster but produces less smooth results.
  /// </summary>
  Low
}

/// <summary>
/// Specifies the filtering mode for HQ/LQ scalers.
/// </summary>
public enum HqMode {
  /// <summary>
  /// Standard YUV-based color comparison mode.
  /// Uses original HQ algorithm thresholds.
  /// </summary>
  Normal,

  /// <summary>
  /// Brightness-weighted edge detection mode (SNES9x style).
  /// Adds contrast-based filtering for stronger edges on high-contrast boundaries.
  /// </summary>
  Bold,

  /// <summary>
  /// Automatic mode that switches between Normal and Bold based on corner analysis.
  /// Uses Bold mode when no corners match the center pixel.
  /// </summary>
  Smart
}

#region HqLq (Unified)

/// <summary>
/// HQ/LQ pixel-art scaling algorithm by Maxim Stepin - unified interface.
/// </summary>
/// <remarks>
/// <para>Scales images using pattern-based edge detection with 256 lookup cases.</para>
/// <para>HQ (High Quality) uses full interpolation patterns for smooth results.</para>
/// <para>LQ (Low Quality) uses simplified patterns for faster processing.</para>
/// <para>Supports symmetric (2x, 3x, 4x) and unsymmetric (2x3, 2x4) scaling.</para>
/// <para>Reference: https://code.google.com/archive/p/hqx</para>
/// </remarks>
[ScalerInfo("HQ/LQ", Author = "Maxim Stepin", Year = 2003,
  Description = "High/Low quality upscaler with 256 pattern cases", Category = ScalerCategory.PixelArt,
  Url = "https://code.google.com/archive/p/hqx")]
public readonly struct HqLq : IPixelScaler {
  private readonly HqQuality _quality;
  private readonly int _scaleX;
  private readonly int _scaleY;
  private readonly HqMode _mode;

  /// <summary>
  /// Creates an HQ/LQ scaler with the specified quality, scale factors, and mode.
  /// </summary>
  /// <param name="quality">The quality level (High or Low).</param>
  /// <param name="scaleX">Horizontal scale factor (2, 3, or 4 for symmetric; 2 for unsymmetric).</param>
  /// <param name="scaleY">Vertical scale factor (must match scaleX for symmetric, or 3/4 for unsymmetric 2xN).</param>
  /// <param name="mode">The filtering mode to use.</param>
  public HqLq(HqQuality quality = HqQuality.High, int scaleX = 2, int scaleY = 2, HqMode mode = HqMode.Normal) {
    if (!_IsValidScale(scaleX, scaleY))
      throw new ArgumentOutOfRangeException(nameof(scaleX), $"HQ/LQ supports 2x2, 3x3, 4x4, 2x3, 2x4 scaling");
    this._quality = quality;
    this._scaleX = scaleX;
    this._scaleY = scaleY;
    this._mode = mode;
  }

  private static bool _IsValidScale(int x, int y)
    => (x, y) is (2, 2) or (3, 3) or (4, 4) or (2, 3) or (2, 4);

  /// <summary>Gets the quality level.</summary>
  public HqQuality Quality => this._quality;

  /// <summary>Gets the filtering mode.</summary>
  public HqMode Mode => this._mode;

  /// <inheritdoc />
  public ScaleFactor Scale => this._scaleX == 0 ? new(2, 2) : new(this._scaleX, this._scaleY);

  /// <summary>
  /// Gets the list of scale factors supported by HQ/LQ.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2), new(3, 3), new(4, 4), new(2, 3), new(2, 4)];

  /// <summary>
  /// Determines whether HQ/LQ supports the specified scale factor.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => _IsValidScale(scale.X, scale.Y);

  /// <summary>
  /// Enumerates all possible target dimensions for HQ/LQ.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
    yield return (sourceWidth * 3, sourceHeight * 3);
    yield return (sourceWidth * 4, sourceHeight * 4);
    yield return (sourceWidth * 2, sourceHeight * 3);
    yield return (sourceWidth * 2, sourceHeight * 4);
  }

  /// <summary>Creates a new HqLq with the specified quality.</summary>
  public HqLq WithQuality(HqQuality quality) => new(quality, this._scaleX, this._scaleY, this._mode);

  /// <summary>Creates a new HqLq with the specified mode.</summary>
  public HqLq WithMode(HqMode mode) => new(this._quality, this._scaleX, this._scaleY, mode);

  /// <summary>Gets an HQ 2x scaler.</summary>
  public static HqLq Hq2x(HqMode mode = HqMode.Normal) => new(HqQuality.High, 2, 2, mode);

  /// <summary>Gets an HQ 3x scaler.</summary>
  public static HqLq Hq3x(HqMode mode = HqMode.Normal) => new(HqQuality.High, 3, 3, mode);

  /// <summary>Gets an HQ 4x scaler.</summary>
  public static HqLq Hq4x(HqMode mode = HqMode.Normal) => new(HqQuality.High, 4, 4, mode);

  /// <summary>Gets an LQ 2x scaler.</summary>
  public static HqLq Lq2x(HqMode mode = HqMode.Normal) => new(HqQuality.Low, 2, 2, mode);

  /// <summary>Gets an LQ 3x scaler.</summary>
  public static HqLq Lq3x(HqMode mode = HqMode.Normal) => new(HqQuality.Low, 3, 3, mode);

  /// <summary>Gets an LQ 4x scaler.</summary>
  public static HqLq Lq4x(HqMode mode = HqMode.Normal) => new(HqQuality.Low, 4, 4, mode);

  /// <summary>Gets the default HQ/LQ configuration (HQ 2x Normal).</summary>
  public static HqLq Default => new(HqQuality.High, 2, 2, HqMode.Normal);

  /// <inheritdoc />
  public TResult InvokeKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode, TResult>(
    IKernelCallback<TWork, TKey, TPixel, TEncode, TResult> callback,
    TEquality equality = default,
    TLerp lerp = default)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDistance : struct, IColorMetric<TKey>
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
    where TEncode : struct, IEncode<TWork, TPixel>
    => (this._quality, this._scaleX, this._scaleY, this._mode) switch {
      // HQ variants
      (HqQuality.High, 2, 2, HqMode.Normal) => callback.Invoke(new Hq2xNormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (HqQuality.High, 2, 2, HqMode.Bold) => callback.Invoke(new Hq2xBoldKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (HqQuality.High, 2, 2, HqMode.Smart) => callback.Invoke(new Hq2xSmartKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (HqQuality.High, 3, 3, HqMode.Normal) => callback.Invoke(new Hq3xNormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (HqQuality.High, 3, 3, HqMode.Bold) => callback.Invoke(new Hq3xBoldKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (HqQuality.High, 3, 3, HqMode.Smart) => callback.Invoke(new Hq3xSmartKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (HqQuality.High, 4, 4, HqMode.Normal) => callback.Invoke(new Hq4xNormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (HqQuality.High, 4, 4, HqMode.Bold) => callback.Invoke(new Hq4xBoldKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (HqQuality.High, 4, 4, HqMode.Smart) => callback.Invoke(new Hq4xSmartKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (HqQuality.High, 2, 3, HqMode.Normal) => callback.Invoke(new Hq2x3NormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (HqQuality.High, 2, 3, HqMode.Bold) => callback.Invoke(new Hq2x3BoldKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (HqQuality.High, 2, 3, HqMode.Smart) => callback.Invoke(new Hq2x3SmartKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (HqQuality.High, 2, 4, HqMode.Normal) => callback.Invoke(new Hq2x4NormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (HqQuality.High, 2, 4, HqMode.Bold) => callback.Invoke(new Hq2x4BoldKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (HqQuality.High, 2, 4, HqMode.Smart) => callback.Invoke(new Hq2x4SmartKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      // LQ variants
      (HqQuality.Low, 2, 2, HqMode.Normal) => callback.Invoke(new Lq2xNormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (HqQuality.Low, 2, 2, HqMode.Bold) => callback.Invoke(new Lq2xBoldKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (HqQuality.Low, 2, 2, HqMode.Smart) => callback.Invoke(new Lq2xSmartKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (HqQuality.Low, 3, 3, HqMode.Normal) => callback.Invoke(new Lq3xNormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (HqQuality.Low, 3, 3, HqMode.Bold) => callback.Invoke(new Lq3xBoldKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (HqQuality.Low, 3, 3, HqMode.Smart) => callback.Invoke(new Lq3xSmartKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (HqQuality.Low, 4, 4, HqMode.Normal) => callback.Invoke(new Lq4xNormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (HqQuality.Low, 4, 4, HqMode.Bold) => callback.Invoke(new Lq4xBoldKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (HqQuality.Low, 4, 4, HqMode.Smart) => callback.Invoke(new Lq4xSmartKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (HqQuality.Low, 2, 3, HqMode.Normal) => callback.Invoke(new Lq2x3NormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (HqQuality.Low, 2, 3, HqMode.Bold) => callback.Invoke(new Lq2x3BoldKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (HqQuality.Low, 2, 3, HqMode.Smart) => callback.Invoke(new Lq2x3SmartKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (HqQuality.Low, 2, 4, HqMode.Normal) => callback.Invoke(new Lq2x4NormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (HqQuality.Low, 2, 4, HqMode.Bold) => callback.Invoke(new Lq2x4BoldKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (HqQuality.Low, 2, 4, HqMode.Smart) => callback.Invoke(new Lq2x4SmartKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (_, 0, 0, _) => callback.Invoke(new Hq2xNormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      _ => throw new InvalidOperationException($"Unsupported HQ/LQ configuration: quality={this._quality}, scale={this._scaleX}x{this._scaleY}, mode={this._mode}")
    };
}

#endregion

#region Hq

/// <summary>
/// HQ (High Quality) pixel-art scaling algorithm by Maxim Stepin.
/// </summary>
/// <remarks>
/// <para>Scales images using pattern-based edge detection with 256 lookup cases.</para>
/// <para>Original algorithm by Maxim Stepin (2003), AdvanceMAME implementation by Andrea Mazzoleni.</para>
/// <para>Uses YUV color space comparisons with thresholds Y=48, U=7, V=6.</para>
/// <para>Supports symmetric (2x, 3x, 4x) and unsymmetric (2x3, 2x4) scaling.</para>
/// <para>Reference: https://code.google.com/archive/p/hqx</para>
/// </remarks>
[ScalerInfo("HQ", Author = "Maxim Stepin", Year = 2003,
  Description = "High quality upscaler with 256 pattern cases", Category = ScalerCategory.PixelArt,
  Url = "https://code.google.com/archive/p/hqx")]
public readonly struct Hq : IPixelScaler {
  private readonly int _scaleX;
  private readonly int _scaleY;
  private readonly HqMode _mode;

  /// <summary>
  /// Creates an HQ scaler with the specified scale factors and mode.
  /// </summary>
  /// <param name="scaleX">Horizontal scale factor (2, 3, or 4 for symmetric; 2 for unsymmetric).</param>
  /// <param name="scaleY">Vertical scale factor (must match scaleX for symmetric, or 3/4 for unsymmetric 2xN).</param>
  /// <param name="mode">The filtering mode to use.</param>
  public Hq(int scaleX = 2, int scaleY = 2, HqMode mode = HqMode.Normal) {
    if (!_IsValidScale(scaleX, scaleY))
      throw new ArgumentOutOfRangeException(nameof(scaleX), $"HQ supports 2x2, 3x3, 4x4, 2x3, 2x4 scaling");
    this._scaleX = scaleX;
    this._scaleY = scaleY;
    this._mode = mode;
  }

  private static bool _IsValidScale(int x, int y)
    => (x, y) is (2, 2) or (3, 3) or (4, 4) or (2, 3) or (2, 4);

  public ScaleFactor Scale => this._scaleX == 0 ? new(2, 2) : new(this._scaleX, this._scaleY);

  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2), new(3, 3), new(4, 4), new(2, 3), new(2, 4)];
  public static bool SupportsScale(ScaleFactor scale) => _IsValidScale(scale.X, scale.Y);

  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
    yield return (sourceWidth * 3, sourceHeight * 3);
    yield return (sourceWidth * 4, sourceHeight * 4);
    yield return (sourceWidth * 2, sourceHeight * 3);
    yield return (sourceWidth * 2, sourceHeight * 4);
  }

  public static Hq Scale2x => new(2, 2);
  public static Hq Scale3x => new(3, 3);
  public static Hq Scale4x => new(4, 4);
  public static Hq Scale2x3 => new(2, 3);
  public static Hq Scale2x4 => new(2, 4);
  public static Hq Default => new(2, 2);

  /// <inheritdoc />
  public TResult InvokeKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode, TResult>(
    IKernelCallback<TWork, TKey, TPixel, TEncode, TResult> callback,
    TEquality equality = default,
    TLerp lerp = default)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDistance : struct, IColorMetric<TKey>
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
    where TEncode : struct, IEncode<TWork, TPixel>
    => (this._scaleX, this._scaleY, this._mode) switch {
      (2, 2, HqMode.Normal) => callback.Invoke(new Hq2xNormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (2, 2, HqMode.Bold) => callback.Invoke(new Hq2xBoldKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (2, 2, HqMode.Smart) => callback.Invoke(new Hq2xSmartKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (3, 3, HqMode.Normal) => callback.Invoke(new Hq3xNormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (3, 3, HqMode.Bold) => callback.Invoke(new Hq3xBoldKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (3, 3, HqMode.Smart) => callback.Invoke(new Hq3xSmartKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (4, 4, HqMode.Normal) => callback.Invoke(new Hq4xNormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (4, 4, HqMode.Bold) => callback.Invoke(new Hq4xBoldKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (4, 4, HqMode.Smart) => callback.Invoke(new Hq4xSmartKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (2, 3, HqMode.Normal) => callback.Invoke(new Hq2x3NormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (2, 3, HqMode.Bold) => callback.Invoke(new Hq2x3BoldKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (2, 3, HqMode.Smart) => callback.Invoke(new Hq2x3SmartKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (2, 4, HqMode.Normal) => callback.Invoke(new Hq2x4NormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (2, 4, HqMode.Bold) => callback.Invoke(new Hq2x4BoldKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (2, 4, HqMode.Smart) => callback.Invoke(new Hq2x4SmartKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (0, 0, _) => callback.Invoke(new Hq2xNormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      _ => throw new InvalidOperationException($"Unsupported HQ scale {this._scaleX}x{this._scaleY} or mode {this._mode}.")
    };
}

#endregion

#region Lq

/// <summary>
/// LQ (Low Quality) pixel-art scaling algorithm - simplified HQ variant.
/// </summary>
/// <remarks>
/// <para>A faster variant of HQ that uses simpler interpolation patterns.</para>
/// <para>Produces less smooth results but is computationally cheaper.</para>
/// <para>Supports symmetric (2x, 3x, 4x) and unsymmetric (2x3, 2x4) scaling.</para>
/// </remarks>
[ScalerInfo("LQ", Author = "Maxim Stepin", Year = 2003,
  Description = "Low quality upscaler - simplified HQ", Category = ScalerCategory.PixelArt)]
public readonly struct Lq : IPixelScaler {
  private readonly int _scaleX;
  private readonly int _scaleY;
  private readonly HqMode _mode;

  /// <summary>
  /// Creates an LQ scaler with the specified scale factors and mode.
  /// </summary>
  public Lq(int scaleX = 2, int scaleY = 2, HqMode mode = HqMode.Normal) {
    if (!_IsValidScale(scaleX, scaleY))
      throw new ArgumentOutOfRangeException(nameof(scaleX), $"LQ supports 2x2, 3x3, 4x4, 2x3, 2x4 scaling");
    this._scaleX = scaleX;
    this._scaleY = scaleY;
    this._mode = mode;
  }

  private static bool _IsValidScale(int x, int y)
    => (x, y) is (2, 2) or (3, 3) or (4, 4) or (2, 3) or (2, 4);

  public ScaleFactor Scale => this._scaleX == 0 ? new(2, 2) : new(this._scaleX, this._scaleY);

  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2), new(3, 3), new(4, 4), new(2, 3), new(2, 4)];
  public static bool SupportsScale(ScaleFactor scale) => _IsValidScale(scale.X, scale.Y);

  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
    yield return (sourceWidth * 3, sourceHeight * 3);
    yield return (sourceWidth * 4, sourceHeight * 4);
    yield return (sourceWidth * 2, sourceHeight * 3);
    yield return (sourceWidth * 2, sourceHeight * 4);
  }

  public static Lq Scale2x => new(2, 2);
  public static Lq Scale3x => new(3, 3);
  public static Lq Scale4x => new(4, 4);
  public static Lq Scale2x3 => new(2, 3);
  public static Lq Scale2x4 => new(2, 4);
  public static Lq Default => new(2, 2);

  /// <inheritdoc />
  public TResult InvokeKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode, TResult>(
    IKernelCallback<TWork, TKey, TPixel, TEncode, TResult> callback,
    TEquality equality = default,
    TLerp lerp = default)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDistance : struct, IColorMetric<TKey>
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
    where TEncode : struct, IEncode<TWork, TPixel>
    => (this._scaleX, this._scaleY, this._mode) switch {
      (2, 2, HqMode.Normal) => callback.Invoke(new Lq2xNormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (2, 2, HqMode.Bold) => callback.Invoke(new Lq2xBoldKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (2, 2, HqMode.Smart) => callback.Invoke(new Lq2xSmartKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (3, 3, HqMode.Normal) => callback.Invoke(new Lq3xNormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (3, 3, HqMode.Bold) => callback.Invoke(new Lq3xBoldKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (3, 3, HqMode.Smart) => callback.Invoke(new Lq3xSmartKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (4, 4, HqMode.Normal) => callback.Invoke(new Lq4xNormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (4, 4, HqMode.Bold) => callback.Invoke(new Lq4xBoldKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (4, 4, HqMode.Smart) => callback.Invoke(new Lq4xSmartKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (2, 3, HqMode.Normal) => callback.Invoke(new Lq2x3NormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (2, 3, HqMode.Bold) => callback.Invoke(new Lq2x3BoldKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (2, 3, HqMode.Smart) => callback.Invoke(new Lq2x3SmartKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (2, 4, HqMode.Normal) => callback.Invoke(new Lq2x4NormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (2, 4, HqMode.Bold) => callback.Invoke(new Lq2x4BoldKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (2, 4, HqMode.Smart) => callback.Invoke(new Lq2x4SmartKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      (0, 0, _) => callback.Invoke(new Lq2xNormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      _ => throw new InvalidOperationException($"Unsupported LQ scale {this._scaleX}x{this._scaleY} or mode {this._mode}.")
    };
}

#endregion

#region File-Local Helper Types

/// <summary>
/// Helper methods for HQ/LQ pattern detection and interpolation.
/// </summary>
file static class HqHelpers {

  /// <summary>
  /// Computes pattern byte by comparing center pixel with 8 neighbors (Normal mode).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte ComputePatternNormal<TKey, TEquality>(
    in TKey c0, in TKey c1, in TKey c2, in TKey c3, in TKey c4, in TKey c5, in TKey c6, in TKey c7, in TKey c8,
    TEquality equality)
    where TKey : unmanaged
    where TEquality : struct, IColorEquality<TKey> {
    byte pattern = 0;
    if (!equality.Equals(c4, c0)) pattern |= 1;
    if (!equality.Equals(c4, c1)) pattern |= 2;
    if (!equality.Equals(c4, c2)) pattern |= 4;
    if (!equality.Equals(c4, c3)) pattern |= 8;
    if (!equality.Equals(c4, c5)) pattern |= 16;
    if (!equality.Equals(c4, c6)) pattern |= 32;
    if (!equality.Equals(c4, c7)) pattern |= 64;
    if (!equality.Equals(c4, c8)) pattern |= 128;
    return pattern;
  }

  /// <summary>
  /// Computes pattern byte with brightness-weighted comparison (Bold mode).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte ComputePatternBold<TWork, TKey, TEquality>(
    in TWork w0, in TWork w1, in TWork w2, in TWork w3, in TWork w4, in TWork w5, in TWork w6, in TWork w7, in TWork w8,
    in TKey c0, in TKey c1, in TKey c2, in TKey c3, in TKey c4, in TKey c5, in TKey c6, in TKey c7, in TKey c8,
    TEquality equality)
    where TWork : unmanaged
    where TKey : unmanaged
    where TEquality : struct, IColorEquality<TKey> {

    // Compute brightness values
    var b0 = GetBrightness(w0);
    var b1 = GetBrightness(w1);
    var b2 = GetBrightness(w2);
    var b3 = GetBrightness(w3);
    var b4 = GetBrightness(w4);
    var b5 = GetBrightness(w5);
    var b6 = GetBrightness(w6);
    var b7 = GetBrightness(w7);
    var b8 = GetBrightness(w8);

    var avgBrightness = (b0 + b1 + b2 + b3 + b4 + b5 + b6 + b7 + b8) / 9f;
    var dc4 = b4 > avgBrightness;

    byte pattern = 0;
    if (!equality.Equals(c4, c0) && (b0 > avgBrightness) != dc4) pattern |= 1;
    if (!equality.Equals(c4, c1) && (b1 > avgBrightness) != dc4) pattern |= 2;
    if (!equality.Equals(c4, c2) && (b2 > avgBrightness) != dc4) pattern |= 4;
    if (!equality.Equals(c4, c3) && (b3 > avgBrightness) != dc4) pattern |= 8;
    if (!equality.Equals(c4, c5) && (b5 > avgBrightness) != dc4) pattern |= 16;
    if (!equality.Equals(c4, c6) && (b6 > avgBrightness) != dc4) pattern |= 32;
    if (!equality.Equals(c4, c7) && (b7 > avgBrightness) != dc4) pattern |= 64;
    if (!equality.Equals(c4, c8) && (b8 > avgBrightness) != dc4) pattern |= 128;
    return pattern;
  }

  /// <summary>
  /// Determines if Smart mode should use Bold filtering.
  /// Returns true if no corners match the center pixel.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool ShouldUseBold<TKey, TEquality>(in TKey c0, in TKey c2, in TKey c4, in TKey c6, in TKey c8, TEquality equality)
    where TKey : unmanaged
    where TEquality : struct, IColorEquality<TKey>
    => !equality.Equals(c0, c4) && !equality.Equals(c2, c4) && !equality.Equals(c6, c4) && !equality.Equals(c8, c4);

  /// <summary>
  /// Computes perceptual lightness from a color value using Oklab L component.
  /// </summary>
  /// <remarks>
  /// Oklab L provides perceptually uniform lightness, ideal for contrast-based edge detection.
  /// Reference: https://bottosson.github.io/posts/oklab/
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float GetBrightness<TWork>(in TWork color) where TWork : unmanaged {
    // For LinearRgbaF - use Oklab L component for perceptual lightness
    if (typeof(TWork) == typeof(LinearRgbaF)) {
      var c = Unsafe.As<TWork, LinearRgbaF>(ref Unsafe.AsRef(in color));
      return _ComputeOklabL(c.R, c.G, c.B);
    }
    // For LinearRgbF - same Oklab L calculation
    if (typeof(TWork) == typeof(LinearRgbF)) {
      var c = Unsafe.As<TWork, LinearRgbF>(ref Unsafe.AsRef(in color));
      return _ComputeOklabL(c.R, c.G, c.B);
    }
    // Fallback - neutral gray
    return 0.5f;
  }

  /// <summary>
  /// Computes the Oklab L (lightness) component from linear RGB values.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _ComputeOklabL(float r, float g, float b) {
    // Linear sRGB to LMS
    var l = 0.4122214708f * r + 0.5363325363f * g + 0.0514459929f * b;
    var m = 0.2119034982f * r + 0.6806995451f * g + 0.1073969566f * b;
    var s = 0.0883024619f * r + 0.2817188376f * g + 0.6299787005f * b;

    // Cube root for perceptual uniformity
    var lCbrt = MathF.Cbrt(l);
    var mCbrt = MathF.Cbrt(m);
    var sCbrt = MathF.Cbrt(s);

    // LMS' to Oklab L component
    return 0.2104542553f * lCbrt + 0.7936177850f * mCbrt - 0.0040720468f * sCbrt;
  }
}

#endregion

#region HQ2x Kernels

file readonly struct Hq2xNormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => 2;
  }

  public int ScaleY {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => 2;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    // Extract 3x3 neighborhood (HQ uses only 3x3)
    // 3x3 source neighborhood
    // NeighborWindow uses [Row][Column] naming: M1P0 = row -1, col 0 = TOP
    var n0 = window.M1M1; // top-left (row -1, col -1)
    var n1 = window.M1P0; // top (row -1, col 0)
    var n2 = window.M1P1; // top-right (row -1, col +1)
    var n3 = window.P0M1; // left (row 0, col -1)
    var n4 = window.P0P0; // center (row 0, col 0)
    var n5 = window.P0P1; // right (row 0, col +1)
    var n6 = window.P1M1; // bottom-left (row +1, col -1)
    var n7 = window.P1P0; // bottom (row +1, col 0)
    var n8 = window.P1P1; // bottom-right (row +1, col +1)

    var c0 = n0.Key;
    var c1 = n1.Key;
    var c2 = n2.Key;
    var c3 = n3.Key;
    var c4 = n4.Key;
    var c5 = n5.Key;
    var c6 = n6.Key;
    var c7 = n7.Key;
    var c8 = n8.Key;

    var w0 = n0.Work;
    var w1 = n1.Work;
    var w2 = n2.Work;
    var w3 = n3.Work;
    var w4 = n4.Work;
    var w5 = n5.Work;
    var w6 = n6.Work;
    var w7 = n7.Work;
    var w8 = n8.Work;

    var pattern = HqHelpers.ComputePatternNormal(c0, c1, c2, c3, c4, c5, c6, c7, c8, equality);

    // Output pixels (2x2 block)
    TWork e00, e01, e10, e11;
    e00 = e01 = e10 = e11 = w4;

    Hq2xPatterns.Apply(pattern, w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8, ref e00, ref e01, ref e10, ref e11, lerp, equality);

    dest[0] = encoder.Encode(e00);
    dest[1] = encoder.Encode(e01);
    dest[destStride] = encoder.Encode(e10);
    dest[destStride + 1] = encoder.Encode(e11);
  }
}

file readonly struct Hq2xBoldKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => 2;
  }

  public int ScaleY {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => 2;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    // 3x3 source neighborhood
    // NeighborWindow uses [Row][Column] naming: M1P0 = row -1, col 0 = TOP
    var n0 = window.M1M1; // top-left (row -1, col -1)
    var n1 = window.M1P0; // top (row -1, col 0)
    var n2 = window.M1P1; // top-right (row -1, col +1)
    var n3 = window.P0M1; // left (row 0, col -1)
    var n4 = window.P0P0; // center (row 0, col 0)
    var n5 = window.P0P1; // right (row 0, col +1)
    var n6 = window.P1M1; // bottom-left (row +1, col -1)
    var n7 = window.P1P0; // bottom (row +1, col 0)
    var n8 = window.P1P1; // bottom-right (row +1, col +1)

    var c0 = n0.Key; var c1 = n1.Key; var c2 = n2.Key; var c3 = n3.Key; var c4 = n4.Key;
    var c5 = n5.Key; var c6 = n6.Key; var c7 = n7.Key; var c8 = n8.Key;
    var w0 = n0.Work; var w1 = n1.Work; var w2 = n2.Work; var w3 = n3.Work; var w4 = n4.Work;
    var w5 = n5.Work; var w6 = n6.Work; var w7 = n7.Work; var w8 = n8.Work;

    var pattern = HqHelpers.ComputePatternBold(w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8, equality);

    TWork e00, e01, e10, e11;
    e00 = e01 = e10 = e11 = w4;

    Hq2xPatterns.Apply(pattern, w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8, ref e00, ref e01, ref e10, ref e11, lerp, equality);

    dest[0] = encoder.Encode(e00);
    dest[1] = encoder.Encode(e01);
    dest[destStride] = encoder.Encode(e10);
    dest[destStride + 1] = encoder.Encode(e11);
  }
}

file readonly struct Hq2xSmartKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => 2;
  }

  public int ScaleY {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => 2;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    // 3x3 source neighborhood
    // NeighborWindow uses [Row][Column] naming: M1P0 = row -1, col 0 = TOP
    var n0 = window.M1M1; // top-left (row -1, col -1)
    var n1 = window.M1P0; // top (row -1, col 0)
    var n2 = window.M1P1; // top-right (row -1, col +1)
    var n3 = window.P0M1; // left (row 0, col -1)
    var n4 = window.P0P0; // center (row 0, col 0)
    var n5 = window.P0P1; // right (row 0, col +1)
    var n6 = window.P1M1; // bottom-left (row +1, col -1)
    var n7 = window.P1P0; // bottom (row +1, col 0)
    var n8 = window.P1P1; // bottom-right (row +1, col +1)

    var c0 = n0.Key; var c1 = n1.Key; var c2 = n2.Key; var c3 = n3.Key; var c4 = n4.Key;
    var c5 = n5.Key; var c6 = n6.Key; var c7 = n7.Key; var c8 = n8.Key;
    var w0 = n0.Work; var w1 = n1.Work; var w2 = n2.Work; var w3 = n3.Work; var w4 = n4.Work;
    var w5 = n5.Work; var w6 = n6.Work; var w7 = n7.Work; var w8 = n8.Work;

    // Smart mode: use Bold if no corners match center
    var pattern = HqHelpers.ShouldUseBold(c0, c2, c4, c6, c8, equality)
      ? HqHelpers.ComputePatternBold(w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8, equality)
      : HqHelpers.ComputePatternNormal(c0, c1, c2, c3, c4, c5, c6, c7, c8, equality);

    TWork e00, e01, e10, e11;
    e00 = e01 = e10 = e11 = w4;

    Hq2xPatterns.Apply(pattern, w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8, ref e00, ref e01, ref e10, ref e11, lerp, equality);

    dest[0] = encoder.Encode(e00);
    dest[1] = encoder.Encode(e01);
    dest[destStride] = encoder.Encode(e10);
    dest[destStride + 1] = encoder.Encode(e11);
  }
}

#endregion

#region LQ2x Kernels

file readonly struct Lq2xNormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => 2;
  }

  public int ScaleY {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => 2;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    // 3x3 source neighborhood
    // NeighborWindow uses [Row][Column] naming: M1P0 = row -1, col 0 = TOP
    var n0 = window.M1M1; // top-left (row -1, col -1)
    var n1 = window.M1P0; // top (row -1, col 0)
    var n2 = window.M1P1; // top-right (row -1, col +1)
    var n3 = window.P0M1; // left (row 0, col -1)
    var n4 = window.P0P0; // center (row 0, col 0)
    var n5 = window.P0P1; // right (row 0, col +1)
    var n6 = window.P1M1; // bottom-left (row +1, col -1)
    var n7 = window.P1P0; // bottom (row +1, col 0)
    var n8 = window.P1P1; // bottom-right (row +1, col +1)

    var c0 = n0.Key; var c1 = n1.Key; var c2 = n2.Key; var c3 = n3.Key; var c4 = n4.Key;
    var c5 = n5.Key; var c6 = n6.Key; var c7 = n7.Key; var c8 = n8.Key;
    var w0 = n0.Work; var w1 = n1.Work; var w2 = n2.Work; var w3 = n3.Work; var w4 = n4.Work;
    var w5 = n5.Work; var w6 = n6.Work; var w7 = n7.Work; var w8 = n8.Work;

    var pattern = HqHelpers.ComputePatternNormal(c0, c1, c2, c3, c4, c5, c6, c7, c8, equality);

    TWork e00, e01, e10, e11;
    e00 = e01 = e10 = e11 = w4;

    Lq2xPatterns.Apply(pattern, w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8, ref e00, ref e01, ref e10, ref e11, lerp, equality);

    dest[0] = encoder.Encode(e00);
    dest[1] = encoder.Encode(e01);
    dest[destStride] = encoder.Encode(e10);
    dest[destStride + 1] = encoder.Encode(e11);
  }
}

file readonly struct Lq2xBoldKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => 2;
  }

  public int ScaleY {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => 2;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    // 3x3 source neighborhood
    // NeighborWindow uses [Row][Column] naming: M1P0 = row -1, col 0 = TOP
    var n0 = window.M1M1; // top-left (row -1, col -1)
    var n1 = window.M1P0; // top (row -1, col 0)
    var n2 = window.M1P1; // top-right (row -1, col +1)
    var n3 = window.P0M1; // left (row 0, col -1)
    var n4 = window.P0P0; // center (row 0, col 0)
    var n5 = window.P0P1; // right (row 0, col +1)
    var n6 = window.P1M1; // bottom-left (row +1, col -1)
    var n7 = window.P1P0; // bottom (row +1, col 0)
    var n8 = window.P1P1; // bottom-right (row +1, col +1)

    var c0 = n0.Key; var c1 = n1.Key; var c2 = n2.Key; var c3 = n3.Key; var c4 = n4.Key;
    var c5 = n5.Key; var c6 = n6.Key; var c7 = n7.Key; var c8 = n8.Key;
    var w0 = n0.Work; var w1 = n1.Work; var w2 = n2.Work; var w3 = n3.Work; var w4 = n4.Work;
    var w5 = n5.Work; var w6 = n6.Work; var w7 = n7.Work; var w8 = n8.Work;

    var pattern = HqHelpers.ComputePatternBold(w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8, equality);

    TWork e00, e01, e10, e11;
    e00 = e01 = e10 = e11 = w4;

    Lq2xPatterns.Apply(pattern, w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8, ref e00, ref e01, ref e10, ref e11, lerp, equality);

    dest[0] = encoder.Encode(e00);
    dest[1] = encoder.Encode(e01);
    dest[destStride] = encoder.Encode(e10);
    dest[destStride + 1] = encoder.Encode(e11);
  }
}

file readonly struct Lq2xSmartKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => 2;
  }

  public int ScaleY {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => 2;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    // 3x3 source neighborhood
    // NeighborWindow uses [Row][Column] naming: M1P0 = row -1, col 0 = TOP
    var n0 = window.M1M1; // top-left (row -1, col -1)
    var n1 = window.M1P0; // top (row -1, col 0)
    var n2 = window.M1P1; // top-right (row -1, col +1)
    var n3 = window.P0M1; // left (row 0, col -1)
    var n4 = window.P0P0; // center (row 0, col 0)
    var n5 = window.P0P1; // right (row 0, col +1)
    var n6 = window.P1M1; // bottom-left (row +1, col -1)
    var n7 = window.P1P0; // bottom (row +1, col 0)
    var n8 = window.P1P1; // bottom-right (row +1, col +1)

    var c0 = n0.Key; var c1 = n1.Key; var c2 = n2.Key; var c3 = n3.Key; var c4 = n4.Key;
    var c5 = n5.Key; var c6 = n6.Key; var c7 = n7.Key; var c8 = n8.Key;
    var w0 = n0.Work; var w1 = n1.Work; var w2 = n2.Work; var w3 = n3.Work; var w4 = n4.Work;
    var w5 = n5.Work; var w6 = n6.Work; var w7 = n7.Work; var w8 = n8.Work;

    var pattern = HqHelpers.ShouldUseBold(c0, c2, c4, c6, c8, equality)
      ? HqHelpers.ComputePatternBold(w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8, equality)
      : HqHelpers.ComputePatternNormal(c0, c1, c2, c3, c4, c5, c6, c7, c8, equality);

    TWork e00, e01, e10, e11;
    e00 = e01 = e10 = e11 = w4;

    Lq2xPatterns.Apply(pattern, w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8, ref e00, ref e01, ref e10, ref e11, lerp, equality);

    dest[0] = encoder.Encode(e00);
    dest[1] = encoder.Encode(e01);
    dest[destStride] = encoder.Encode(e10);
    dest[destStride + 1] = encoder.Encode(e11);
  }
}

#endregion

#region Placeholder Kernels for 3x, 4x, and Unsymmetric Variants

// HQ3x kernels
file readonly struct Hq3xNormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace where TKey : unmanaged, IColorSpace where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey> where TLerp : struct, ILerp<TWork> where TEncode : struct, IEncode<TWork, TPixel> {
  public int ScaleX => 3;
  public int ScaleY => 3;
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var n0 = window.M1M1; var n1 = window.P0M1; var n2 = window.P1M1;
    var n3 = window.M1P0; var n4 = window.P0P0; var n5 = window.P1P0;
    var n6 = window.M1P1; var n7 = window.P0P1; var n8 = window.P1P1;
    var c0 = n0.Key; var c1 = n1.Key; var c2 = n2.Key; var c3 = n3.Key; var c4 = n4.Key;
    var c5 = n5.Key; var c6 = n6.Key; var c7 = n7.Key; var c8 = n8.Key;
    var w0 = n0.Work; var w1 = n1.Work; var w2 = n2.Work; var w3 = n3.Work; var w4 = n4.Work;
    var w5 = n5.Work; var w6 = n6.Work; var w7 = n7.Work; var w8 = n8.Work;
    var pattern = HqHelpers.ComputePatternNormal(c0, c1, c2, c3, c4, c5, c6, c7, c8, equality);
    TWork e00, e01, e02, e10, e11, e12, e20, e21, e22;
    e00 = e01 = e02 = e10 = e11 = e12 = e20 = e21 = e22 = w4;
    Hq3xPatterns.Apply(pattern, w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8,
      ref e00, ref e01, ref e02, ref e10, ref e11, ref e12, ref e20, ref e21, ref e22, lerp, equality);
    dest[0] = encoder.Encode(e00); dest[1] = encoder.Encode(e01); dest[2] = encoder.Encode(e02);
    dest[destStride] = encoder.Encode(e10); dest[destStride + 1] = encoder.Encode(e11); dest[destStride + 2] = encoder.Encode(e12);
    dest[destStride * 2] = encoder.Encode(e20); dest[destStride * 2 + 1] = encoder.Encode(e21); dest[destStride * 2 + 2] = encoder.Encode(e22);
  }
}

file readonly struct Hq3xBoldKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace where TKey : unmanaged, IColorSpace where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey> where TLerp : struct, ILerp<TWork> where TEncode : struct, IEncode<TWork, TPixel> {
  public int ScaleX => 3;
  public int ScaleY => 3;
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var n0 = window.M1M1; var n1 = window.P0M1; var n2 = window.P1M1;
    var n3 = window.M1P0; var n4 = window.P0P0; var n5 = window.P1P0;
    var n6 = window.M1P1; var n7 = window.P0P1; var n8 = window.P1P1;
    var c0 = n0.Key; var c1 = n1.Key; var c2 = n2.Key; var c3 = n3.Key; var c4 = n4.Key;
    var c5 = n5.Key; var c6 = n6.Key; var c7 = n7.Key; var c8 = n8.Key;
    var w0 = n0.Work; var w1 = n1.Work; var w2 = n2.Work; var w3 = n3.Work; var w4 = n4.Work;
    var w5 = n5.Work; var w6 = n6.Work; var w7 = n7.Work; var w8 = n8.Work;
    var pattern = HqHelpers.ComputePatternBold(w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8, equality);
    TWork e00, e01, e02, e10, e11, e12, e20, e21, e22;
    e00 = e01 = e02 = e10 = e11 = e12 = e20 = e21 = e22 = w4;
    Hq3xPatterns.Apply(pattern, w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8,
      ref e00, ref e01, ref e02, ref e10, ref e11, ref e12, ref e20, ref e21, ref e22, lerp, equality);
    dest[0] = encoder.Encode(e00); dest[1] = encoder.Encode(e01); dest[2] = encoder.Encode(e02);
    dest[destStride] = encoder.Encode(e10); dest[destStride + 1] = encoder.Encode(e11); dest[destStride + 2] = encoder.Encode(e12);
    dest[destStride * 2] = encoder.Encode(e20); dest[destStride * 2 + 1] = encoder.Encode(e21); dest[destStride * 2 + 2] = encoder.Encode(e22);
  }
}

file readonly struct Hq3xSmartKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace where TKey : unmanaged, IColorSpace where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey> where TLerp : struct, ILerp<TWork> where TEncode : struct, IEncode<TWork, TPixel> {
  public int ScaleX => 3;
  public int ScaleY => 3;
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var n0 = window.M1M1; var n1 = window.P0M1; var n2 = window.P1M1;
    var n3 = window.M1P0; var n4 = window.P0P0; var n5 = window.P1P0;
    var n6 = window.M1P1; var n7 = window.P0P1; var n8 = window.P1P1;
    var c0 = n0.Key; var c1 = n1.Key; var c2 = n2.Key; var c3 = n3.Key; var c4 = n4.Key;
    var c5 = n5.Key; var c6 = n6.Key; var c7 = n7.Key; var c8 = n8.Key;
    var w0 = n0.Work; var w1 = n1.Work; var w2 = n2.Work; var w3 = n3.Work; var w4 = n4.Work;
    var w5 = n5.Work; var w6 = n6.Work; var w7 = n7.Work; var w8 = n8.Work;
    var pattern = HqHelpers.ShouldUseBold(c0, c2, c4, c6, c8, equality)
      ? HqHelpers.ComputePatternBold(w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8, equality)
      : HqHelpers.ComputePatternNormal(c0, c1, c2, c3, c4, c5, c6, c7, c8, equality);
    TWork e00, e01, e02, e10, e11, e12, e20, e21, e22;
    e00 = e01 = e02 = e10 = e11 = e12 = e20 = e21 = e22 = w4;
    Hq3xPatterns.Apply(pattern, w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8,
      ref e00, ref e01, ref e02, ref e10, ref e11, ref e12, ref e20, ref e21, ref e22, lerp, equality);
    dest[0] = encoder.Encode(e00); dest[1] = encoder.Encode(e01); dest[2] = encoder.Encode(e02);
    dest[destStride] = encoder.Encode(e10); dest[destStride + 1] = encoder.Encode(e11); dest[destStride + 2] = encoder.Encode(e12);
    dest[destStride * 2] = encoder.Encode(e20); dest[destStride * 2 + 1] = encoder.Encode(e21); dest[destStride * 2 + 2] = encoder.Encode(e22);
  }
}

// HQ4x kernels
file readonly struct Hq4xNormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace where TKey : unmanaged, IColorSpace where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey> where TLerp : struct, ILerp<TWork> where TEncode : struct, IEncode<TWork, TPixel> {
  public int ScaleX => 4;
  public int ScaleY => 4;
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var n0 = window.M1M1; var n1 = window.P0M1; var n2 = window.P1M1;
    var n3 = window.M1P0; var n4 = window.P0P0; var n5 = window.P1P0;
    var n6 = window.M1P1; var n7 = window.P0P1; var n8 = window.P1P1;
    var c0 = n0.Key; var c1 = n1.Key; var c2 = n2.Key; var c3 = n3.Key; var c4 = n4.Key;
    var c5 = n5.Key; var c6 = n6.Key; var c7 = n7.Key; var c8 = n8.Key;
    var w0 = n0.Work; var w1 = n1.Work; var w2 = n2.Work; var w3 = n3.Work; var w4 = n4.Work;
    var w5 = n5.Work; var w6 = n6.Work; var w7 = n7.Work; var w8 = n8.Work;
    var pattern = HqHelpers.ComputePatternNormal(c0, c1, c2, c3, c4, c5, c6, c7, c8, equality);
    TWork e00, e01, e02, e03, e10, e11, e12, e13, e20, e21, e22, e23, e30, e31, e32, e33;
    e00 = e01 = e02 = e03 = e10 = e11 = e12 = e13 = e20 = e21 = e22 = e23 = e30 = e31 = e32 = e33 = w4;
    Hq4xPatterns.Apply(pattern, w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8,
      ref e00, ref e01, ref e02, ref e03, ref e10, ref e11, ref e12, ref e13,
      ref e20, ref e21, ref e22, ref e23, ref e30, ref e31, ref e32, ref e33, lerp, equality);
    dest[0] = encoder.Encode(e00); dest[1] = encoder.Encode(e01); dest[2] = encoder.Encode(e02); dest[3] = encoder.Encode(e03);
    dest[destStride] = encoder.Encode(e10); dest[destStride + 1] = encoder.Encode(e11); dest[destStride + 2] = encoder.Encode(e12); dest[destStride + 3] = encoder.Encode(e13);
    dest[destStride * 2] = encoder.Encode(e20); dest[destStride * 2 + 1] = encoder.Encode(e21); dest[destStride * 2 + 2] = encoder.Encode(e22); dest[destStride * 2 + 3] = encoder.Encode(e23);
    dest[destStride * 3] = encoder.Encode(e30); dest[destStride * 3 + 1] = encoder.Encode(e31); dest[destStride * 3 + 2] = encoder.Encode(e32); dest[destStride * 3 + 3] = encoder.Encode(e33);
  }
}

file readonly struct Hq4xBoldKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace where TKey : unmanaged, IColorSpace where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey> where TLerp : struct, ILerp<TWork> where TEncode : struct, IEncode<TWork, TPixel> {
  public int ScaleX => 4;
  public int ScaleY => 4;
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var n0 = window.M1M1; var n1 = window.P0M1; var n2 = window.P1M1;
    var n3 = window.M1P0; var n4 = window.P0P0; var n5 = window.P1P0;
    var n6 = window.M1P1; var n7 = window.P0P1; var n8 = window.P1P1;
    var c0 = n0.Key; var c1 = n1.Key; var c2 = n2.Key; var c3 = n3.Key; var c4 = n4.Key;
    var c5 = n5.Key; var c6 = n6.Key; var c7 = n7.Key; var c8 = n8.Key;
    var w0 = n0.Work; var w1 = n1.Work; var w2 = n2.Work; var w3 = n3.Work; var w4 = n4.Work;
    var w5 = n5.Work; var w6 = n6.Work; var w7 = n7.Work; var w8 = n8.Work;
    var pattern = HqHelpers.ComputePatternBold(w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8, equality);
    TWork e00, e01, e02, e03, e10, e11, e12, e13, e20, e21, e22, e23, e30, e31, e32, e33;
    e00 = e01 = e02 = e03 = e10 = e11 = e12 = e13 = e20 = e21 = e22 = e23 = e30 = e31 = e32 = e33 = w4;
    Hq4xPatterns.Apply(pattern, w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8,
      ref e00, ref e01, ref e02, ref e03, ref e10, ref e11, ref e12, ref e13,
      ref e20, ref e21, ref e22, ref e23, ref e30, ref e31, ref e32, ref e33, lerp, equality);
    dest[0] = encoder.Encode(e00); dest[1] = encoder.Encode(e01); dest[2] = encoder.Encode(e02); dest[3] = encoder.Encode(e03);
    dest[destStride] = encoder.Encode(e10); dest[destStride + 1] = encoder.Encode(e11); dest[destStride + 2] = encoder.Encode(e12); dest[destStride + 3] = encoder.Encode(e13);
    dest[destStride * 2] = encoder.Encode(e20); dest[destStride * 2 + 1] = encoder.Encode(e21); dest[destStride * 2 + 2] = encoder.Encode(e22); dest[destStride * 2 + 3] = encoder.Encode(e23);
    dest[destStride * 3] = encoder.Encode(e30); dest[destStride * 3 + 1] = encoder.Encode(e31); dest[destStride * 3 + 2] = encoder.Encode(e32); dest[destStride * 3 + 3] = encoder.Encode(e33);
  }
}

file readonly struct Hq4xSmartKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace where TKey : unmanaged, IColorSpace where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey> where TLerp : struct, ILerp<TWork> where TEncode : struct, IEncode<TWork, TPixel> {
  public int ScaleX => 4;
  public int ScaleY => 4;
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var n0 = window.M1M1; var n1 = window.P0M1; var n2 = window.P1M1;
    var n3 = window.M1P0; var n4 = window.P0P0; var n5 = window.P1P0;
    var n6 = window.M1P1; var n7 = window.P0P1; var n8 = window.P1P1;
    var c0 = n0.Key; var c1 = n1.Key; var c2 = n2.Key; var c3 = n3.Key; var c4 = n4.Key;
    var c5 = n5.Key; var c6 = n6.Key; var c7 = n7.Key; var c8 = n8.Key;
    var w0 = n0.Work; var w1 = n1.Work; var w2 = n2.Work; var w3 = n3.Work; var w4 = n4.Work;
    var w5 = n5.Work; var w6 = n6.Work; var w7 = n7.Work; var w8 = n8.Work;
    var pattern = HqHelpers.ShouldUseBold(c0, c2, c4, c6, c8, equality)
      ? HqHelpers.ComputePatternBold(w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8, equality)
      : HqHelpers.ComputePatternNormal(c0, c1, c2, c3, c4, c5, c6, c7, c8, equality);
    TWork e00, e01, e02, e03, e10, e11, e12, e13, e20, e21, e22, e23, e30, e31, e32, e33;
    e00 = e01 = e02 = e03 = e10 = e11 = e12 = e13 = e20 = e21 = e22 = e23 = e30 = e31 = e32 = e33 = w4;
    Hq4xPatterns.Apply(pattern, w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8,
      ref e00, ref e01, ref e02, ref e03, ref e10, ref e11, ref e12, ref e13,
      ref e20, ref e21, ref e22, ref e23, ref e30, ref e31, ref e32, ref e33, lerp, equality);
    dest[0] = encoder.Encode(e00); dest[1] = encoder.Encode(e01); dest[2] = encoder.Encode(e02); dest[3] = encoder.Encode(e03);
    dest[destStride] = encoder.Encode(e10); dest[destStride + 1] = encoder.Encode(e11); dest[destStride + 2] = encoder.Encode(e12); dest[destStride + 3] = encoder.Encode(e13);
    dest[destStride * 2] = encoder.Encode(e20); dest[destStride * 2 + 1] = encoder.Encode(e21); dest[destStride * 2 + 2] = encoder.Encode(e22); dest[destStride * 2 + 3] = encoder.Encode(e23);
    dest[destStride * 3] = encoder.Encode(e30); dest[destStride * 3 + 1] = encoder.Encode(e31); dest[destStride * 3 + 2] = encoder.Encode(e32); dest[destStride * 3 + 3] = encoder.Encode(e33);
  }
}

// LQ3x kernels
file readonly struct Lq3xNormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace where TKey : unmanaged, IColorSpace where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey> where TLerp : struct, ILerp<TWork> where TEncode : struct, IEncode<TWork, TPixel> {
  public int ScaleX => 3;
  public int ScaleY => 3;
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var n0 = window.M1M1; var n1 = window.P0M1; var n2 = window.P1M1;
    var n3 = window.M1P0; var n4 = window.P0P0; var n5 = window.P1P0;
    var n6 = window.M1P1; var n7 = window.P0P1; var n8 = window.P1P1;
    var c0 = n0.Key; var c1 = n1.Key; var c2 = n2.Key; var c3 = n3.Key; var c4 = n4.Key;
    var c5 = n5.Key; var c6 = n6.Key; var c7 = n7.Key; var c8 = n8.Key;
    var w0 = n0.Work; var w1 = n1.Work; var w2 = n2.Work; var w3 = n3.Work; var w4 = n4.Work;
    var w5 = n5.Work; var w6 = n6.Work; var w7 = n7.Work; var w8 = n8.Work;
    var pattern = HqHelpers.ComputePatternNormal(c0, c1, c2, c3, c4, c5, c6, c7, c8, equality);
    TWork e00, e01, e02, e10, e11, e12, e20, e21, e22;
    e00 = e01 = e02 = e10 = e11 = e12 = e20 = e21 = e22 = w4;
    Lq3xPatterns.Apply(pattern, w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8,
      ref e00, ref e01, ref e02, ref e10, ref e11, ref e12, ref e20, ref e21, ref e22, lerp, equality);
    dest[0] = encoder.Encode(e00); dest[1] = encoder.Encode(e01); dest[2] = encoder.Encode(e02);
    dest[destStride] = encoder.Encode(e10); dest[destStride + 1] = encoder.Encode(e11); dest[destStride + 2] = encoder.Encode(e12);
    dest[destStride * 2] = encoder.Encode(e20); dest[destStride * 2 + 1] = encoder.Encode(e21); dest[destStride * 2 + 2] = encoder.Encode(e22);
  }
}

file readonly struct Lq3xBoldKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace where TKey : unmanaged, IColorSpace where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey> where TLerp : struct, ILerp<TWork> where TEncode : struct, IEncode<TWork, TPixel> {
  public int ScaleX => 3;
  public int ScaleY => 3;
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var n0 = window.M1M1; var n1 = window.P0M1; var n2 = window.P1M1;
    var n3 = window.M1P0; var n4 = window.P0P0; var n5 = window.P1P0;
    var n6 = window.M1P1; var n7 = window.P0P1; var n8 = window.P1P1;
    var c0 = n0.Key; var c1 = n1.Key; var c2 = n2.Key; var c3 = n3.Key; var c4 = n4.Key;
    var c5 = n5.Key; var c6 = n6.Key; var c7 = n7.Key; var c8 = n8.Key;
    var w0 = n0.Work; var w1 = n1.Work; var w2 = n2.Work; var w3 = n3.Work; var w4 = n4.Work;
    var w5 = n5.Work; var w6 = n6.Work; var w7 = n7.Work; var w8 = n8.Work;
    var pattern = HqHelpers.ComputePatternBold(w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8, equality);
    TWork e00, e01, e02, e10, e11, e12, e20, e21, e22;
    e00 = e01 = e02 = e10 = e11 = e12 = e20 = e21 = e22 = w4;
    Lq3xPatterns.Apply(pattern, w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8,
      ref e00, ref e01, ref e02, ref e10, ref e11, ref e12, ref e20, ref e21, ref e22, lerp, equality);
    dest[0] = encoder.Encode(e00); dest[1] = encoder.Encode(e01); dest[2] = encoder.Encode(e02);
    dest[destStride] = encoder.Encode(e10); dest[destStride + 1] = encoder.Encode(e11); dest[destStride + 2] = encoder.Encode(e12);
    dest[destStride * 2] = encoder.Encode(e20); dest[destStride * 2 + 1] = encoder.Encode(e21); dest[destStride * 2 + 2] = encoder.Encode(e22);
  }
}

file readonly struct Lq3xSmartKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace where TKey : unmanaged, IColorSpace where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey> where TLerp : struct, ILerp<TWork> where TEncode : struct, IEncode<TWork, TPixel> {
  public int ScaleX => 3;
  public int ScaleY => 3;
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var n0 = window.M1M1; var n1 = window.P0M1; var n2 = window.P1M1;
    var n3 = window.M1P0; var n4 = window.P0P0; var n5 = window.P1P0;
    var n6 = window.M1P1; var n7 = window.P0P1; var n8 = window.P1P1;
    var c0 = n0.Key; var c1 = n1.Key; var c2 = n2.Key; var c3 = n3.Key; var c4 = n4.Key;
    var c5 = n5.Key; var c6 = n6.Key; var c7 = n7.Key; var c8 = n8.Key;
    var w0 = n0.Work; var w1 = n1.Work; var w2 = n2.Work; var w3 = n3.Work; var w4 = n4.Work;
    var w5 = n5.Work; var w6 = n6.Work; var w7 = n7.Work; var w8 = n8.Work;
    var pattern = HqHelpers.ShouldUseBold(c0, c2, c4, c6, c8, equality)
      ? HqHelpers.ComputePatternBold(w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8, equality)
      : HqHelpers.ComputePatternNormal(c0, c1, c2, c3, c4, c5, c6, c7, c8, equality);
    TWork e00, e01, e02, e10, e11, e12, e20, e21, e22;
    e00 = e01 = e02 = e10 = e11 = e12 = e20 = e21 = e22 = w4;
    Lq3xPatterns.Apply(pattern, w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8,
      ref e00, ref e01, ref e02, ref e10, ref e11, ref e12, ref e20, ref e21, ref e22, lerp, equality);
    dest[0] = encoder.Encode(e00); dest[1] = encoder.Encode(e01); dest[2] = encoder.Encode(e02);
    dest[destStride] = encoder.Encode(e10); dest[destStride + 1] = encoder.Encode(e11); dest[destStride + 2] = encoder.Encode(e12);
    dest[destStride * 2] = encoder.Encode(e20); dest[destStride * 2 + 1] = encoder.Encode(e21); dest[destStride * 2 + 2] = encoder.Encode(e22);
  }
}

// LQ4x kernels
file readonly struct Lq4xNormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace where TKey : unmanaged, IColorSpace where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey> where TLerp : struct, ILerp<TWork> where TEncode : struct, IEncode<TWork, TPixel> {
  public int ScaleX => 4;
  public int ScaleY => 4;
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var n0 = window.M1M1; var n1 = window.P0M1; var n2 = window.P1M1;
    var n3 = window.M1P0; var n4 = window.P0P0; var n5 = window.P1P0;
    var n6 = window.M1P1; var n7 = window.P0P1; var n8 = window.P1P1;
    var c0 = n0.Key; var c1 = n1.Key; var c2 = n2.Key; var c3 = n3.Key; var c4 = n4.Key;
    var c5 = n5.Key; var c6 = n6.Key; var c7 = n7.Key; var c8 = n8.Key;
    var w0 = n0.Work; var w1 = n1.Work; var w2 = n2.Work; var w3 = n3.Work; var w4 = n4.Work;
    var w5 = n5.Work; var w6 = n6.Work; var w7 = n7.Work; var w8 = n8.Work;
    var pattern = HqHelpers.ComputePatternNormal(c0, c1, c2, c3, c4, c5, c6, c7, c8, equality);
    TWork e00, e01, e02, e03, e10, e11, e12, e13, e20, e21, e22, e23, e30, e31, e32, e33;
    e00 = e01 = e02 = e03 = e10 = e11 = e12 = e13 = e20 = e21 = e22 = e23 = e30 = e31 = e32 = e33 = w4;
    Lq4xPatterns.Apply(pattern, w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8,
      ref e00, ref e01, ref e02, ref e03, ref e10, ref e11, ref e12, ref e13,
      ref e20, ref e21, ref e22, ref e23, ref e30, ref e31, ref e32, ref e33, lerp, equality);
    dest[0] = encoder.Encode(e00); dest[1] = encoder.Encode(e01); dest[2] = encoder.Encode(e02); dest[3] = encoder.Encode(e03);
    dest[destStride] = encoder.Encode(e10); dest[destStride + 1] = encoder.Encode(e11); dest[destStride + 2] = encoder.Encode(e12); dest[destStride + 3] = encoder.Encode(e13);
    dest[destStride * 2] = encoder.Encode(e20); dest[destStride * 2 + 1] = encoder.Encode(e21); dest[destStride * 2 + 2] = encoder.Encode(e22); dest[destStride * 2 + 3] = encoder.Encode(e23);
    dest[destStride * 3] = encoder.Encode(e30); dest[destStride * 3 + 1] = encoder.Encode(e31); dest[destStride * 3 + 2] = encoder.Encode(e32); dest[destStride * 3 + 3] = encoder.Encode(e33);
  }
}

file readonly struct Lq4xBoldKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace where TKey : unmanaged, IColorSpace where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey> where TLerp : struct, ILerp<TWork> where TEncode : struct, IEncode<TWork, TPixel> {
  public int ScaleX => 4;
  public int ScaleY => 4;
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var n0 = window.M1M1; var n1 = window.P0M1; var n2 = window.P1M1;
    var n3 = window.M1P0; var n4 = window.P0P0; var n5 = window.P1P0;
    var n6 = window.M1P1; var n7 = window.P0P1; var n8 = window.P1P1;
    var c0 = n0.Key; var c1 = n1.Key; var c2 = n2.Key; var c3 = n3.Key; var c4 = n4.Key;
    var c5 = n5.Key; var c6 = n6.Key; var c7 = n7.Key; var c8 = n8.Key;
    var w0 = n0.Work; var w1 = n1.Work; var w2 = n2.Work; var w3 = n3.Work; var w4 = n4.Work;
    var w5 = n5.Work; var w6 = n6.Work; var w7 = n7.Work; var w8 = n8.Work;
    var pattern = HqHelpers.ComputePatternBold(w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8, equality);
    TWork e00, e01, e02, e03, e10, e11, e12, e13, e20, e21, e22, e23, e30, e31, e32, e33;
    e00 = e01 = e02 = e03 = e10 = e11 = e12 = e13 = e20 = e21 = e22 = e23 = e30 = e31 = e32 = e33 = w4;
    Lq4xPatterns.Apply(pattern, w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8,
      ref e00, ref e01, ref e02, ref e03, ref e10, ref e11, ref e12, ref e13,
      ref e20, ref e21, ref e22, ref e23, ref e30, ref e31, ref e32, ref e33, lerp, equality);
    dest[0] = encoder.Encode(e00); dest[1] = encoder.Encode(e01); dest[2] = encoder.Encode(e02); dest[3] = encoder.Encode(e03);
    dest[destStride] = encoder.Encode(e10); dest[destStride + 1] = encoder.Encode(e11); dest[destStride + 2] = encoder.Encode(e12); dest[destStride + 3] = encoder.Encode(e13);
    dest[destStride * 2] = encoder.Encode(e20); dest[destStride * 2 + 1] = encoder.Encode(e21); dest[destStride * 2 + 2] = encoder.Encode(e22); dest[destStride * 2 + 3] = encoder.Encode(e23);
    dest[destStride * 3] = encoder.Encode(e30); dest[destStride * 3 + 1] = encoder.Encode(e31); dest[destStride * 3 + 2] = encoder.Encode(e32); dest[destStride * 3 + 3] = encoder.Encode(e33);
  }
}

file readonly struct Lq4xSmartKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace where TKey : unmanaged, IColorSpace where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey> where TLerp : struct, ILerp<TWork> where TEncode : struct, IEncode<TWork, TPixel> {
  public int ScaleX => 4;
  public int ScaleY => 4;
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var n0 = window.M1M1; var n1 = window.P0M1; var n2 = window.P1M1;
    var n3 = window.M1P0; var n4 = window.P0P0; var n5 = window.P1P0;
    var n6 = window.M1P1; var n7 = window.P0P1; var n8 = window.P1P1;
    var c0 = n0.Key; var c1 = n1.Key; var c2 = n2.Key; var c3 = n3.Key; var c4 = n4.Key;
    var c5 = n5.Key; var c6 = n6.Key; var c7 = n7.Key; var c8 = n8.Key;
    var w0 = n0.Work; var w1 = n1.Work; var w2 = n2.Work; var w3 = n3.Work; var w4 = n4.Work;
    var w5 = n5.Work; var w6 = n6.Work; var w7 = n7.Work; var w8 = n8.Work;
    var pattern = HqHelpers.ShouldUseBold(c0, c2, c4, c6, c8, equality)
      ? HqHelpers.ComputePatternBold(w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8, equality)
      : HqHelpers.ComputePatternNormal(c0, c1, c2, c3, c4, c5, c6, c7, c8, equality);
    TWork e00, e01, e02, e03, e10, e11, e12, e13, e20, e21, e22, e23, e30, e31, e32, e33;
    e00 = e01 = e02 = e03 = e10 = e11 = e12 = e13 = e20 = e21 = e22 = e23 = e30 = e31 = e32 = e33 = w4;
    Lq4xPatterns.Apply(pattern, w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8,
      ref e00, ref e01, ref e02, ref e03, ref e10, ref e11, ref e12, ref e13,
      ref e20, ref e21, ref e22, ref e23, ref e30, ref e31, ref e32, ref e33, lerp, equality);
    dest[0] = encoder.Encode(e00); dest[1] = encoder.Encode(e01); dest[2] = encoder.Encode(e02); dest[3] = encoder.Encode(e03);
    dest[destStride] = encoder.Encode(e10); dest[destStride + 1] = encoder.Encode(e11); dest[destStride + 2] = encoder.Encode(e12); dest[destStride + 3] = encoder.Encode(e13);
    dest[destStride * 2] = encoder.Encode(e20); dest[destStride * 2 + 1] = encoder.Encode(e21); dest[destStride * 2 + 2] = encoder.Encode(e22); dest[destStride * 2 + 3] = encoder.Encode(e23);
    dest[destStride * 3] = encoder.Encode(e30); dest[destStride * 3 + 1] = encoder.Encode(e31); dest[destStride * 3 + 2] = encoder.Encode(e32); dest[destStride * 3 + 3] = encoder.Encode(e33);
  }
}

// Unsymmetric HQ2x3 kernels
file readonly struct Hq2x3NormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace where TKey : unmanaged, IColorSpace where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey> where TLerp : struct, ILerp<TWork> where TEncode : struct, IEncode<TWork, TPixel> {
  public int ScaleX => 2;
  public int ScaleY => 3;
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var n0 = window.M1M1; var n1 = window.P0M1; var n2 = window.P1M1;
    var n3 = window.M1P0; var n4 = window.P0P0; var n5 = window.P1P0;
    var n6 = window.M1P1; var n7 = window.P0P1; var n8 = window.P1P1;
    var c0 = n0.Key; var c1 = n1.Key; var c2 = n2.Key; var c3 = n3.Key; var c4 = n4.Key;
    var c5 = n5.Key; var c6 = n6.Key; var c7 = n7.Key; var c8 = n8.Key;
    var w0 = n0.Work; var w1 = n1.Work; var w2 = n2.Work; var w3 = n3.Work; var w4 = n4.Work;
    var w5 = n5.Work; var w6 = n6.Work; var w7 = n7.Work; var w8 = n8.Work;
    var pattern = HqHelpers.ComputePatternNormal(c0, c1, c2, c3, c4, c5, c6, c7, c8, equality);
    TWork e00, e01, e10, e11, e20, e21;
    e00 = e01 = e10 = e11 = e20 = e21 = w4;
    Hq2x3Patterns.Apply(pattern, w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8,
      ref e00, ref e01, ref e10, ref e11, ref e20, ref e21, lerp, equality);
    dest[0] = encoder.Encode(e00); dest[1] = encoder.Encode(e01);
    dest[destStride] = encoder.Encode(e10); dest[destStride + 1] = encoder.Encode(e11);
    dest[destStride * 2] = encoder.Encode(e20); dest[destStride * 2 + 1] = encoder.Encode(e21);
  }
}

file readonly struct Hq2x3BoldKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace where TKey : unmanaged, IColorSpace where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey> where TLerp : struct, ILerp<TWork> where TEncode : struct, IEncode<TWork, TPixel> {
  public int ScaleX => 2;
  public int ScaleY => 3;
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var n0 = window.M1M1; var n1 = window.P0M1; var n2 = window.P1M1;
    var n3 = window.M1P0; var n4 = window.P0P0; var n5 = window.P1P0;
    var n6 = window.M1P1; var n7 = window.P0P1; var n8 = window.P1P1;
    var c0 = n0.Key; var c1 = n1.Key; var c2 = n2.Key; var c3 = n3.Key; var c4 = n4.Key;
    var c5 = n5.Key; var c6 = n6.Key; var c7 = n7.Key; var c8 = n8.Key;
    var w0 = n0.Work; var w1 = n1.Work; var w2 = n2.Work; var w3 = n3.Work; var w4 = n4.Work;
    var w5 = n5.Work; var w6 = n6.Work; var w7 = n7.Work; var w8 = n8.Work;
    var pattern = HqHelpers.ComputePatternBold(w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8, equality);
    TWork e00, e01, e10, e11, e20, e21;
    e00 = e01 = e10 = e11 = e20 = e21 = w4;
    Hq2x3Patterns.Apply(pattern, w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8,
      ref e00, ref e01, ref e10, ref e11, ref e20, ref e21, lerp, equality);
    dest[0] = encoder.Encode(e00); dest[1] = encoder.Encode(e01);
    dest[destStride] = encoder.Encode(e10); dest[destStride + 1] = encoder.Encode(e11);
    dest[destStride * 2] = encoder.Encode(e20); dest[destStride * 2 + 1] = encoder.Encode(e21);
  }
}

file readonly struct Hq2x3SmartKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace where TKey : unmanaged, IColorSpace where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey> where TLerp : struct, ILerp<TWork> where TEncode : struct, IEncode<TWork, TPixel> {
  public int ScaleX => 2;
  public int ScaleY => 3;
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var n0 = window.M1M1; var n1 = window.P0M1; var n2 = window.P1M1;
    var n3 = window.M1P0; var n4 = window.P0P0; var n5 = window.P1P0;
    var n6 = window.M1P1; var n7 = window.P0P1; var n8 = window.P1P1;
    var c0 = n0.Key; var c1 = n1.Key; var c2 = n2.Key; var c3 = n3.Key; var c4 = n4.Key;
    var c5 = n5.Key; var c6 = n6.Key; var c7 = n7.Key; var c8 = n8.Key;
    var w0 = n0.Work; var w1 = n1.Work; var w2 = n2.Work; var w3 = n3.Work; var w4 = n4.Work;
    var w5 = n5.Work; var w6 = n6.Work; var w7 = n7.Work; var w8 = n8.Work;
    var pattern = HqHelpers.ShouldUseBold(c0, c2, c4, c6, c8, equality)
      ? HqHelpers.ComputePatternBold(w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8, equality)
      : HqHelpers.ComputePatternNormal(c0, c1, c2, c3, c4, c5, c6, c7, c8, equality);
    TWork e00, e01, e10, e11, e20, e21;
    e00 = e01 = e10 = e11 = e20 = e21 = w4;
    Hq2x3Patterns.Apply(pattern, w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8,
      ref e00, ref e01, ref e10, ref e11, ref e20, ref e21, lerp, equality);
    dest[0] = encoder.Encode(e00); dest[1] = encoder.Encode(e01);
    dest[destStride] = encoder.Encode(e10); dest[destStride + 1] = encoder.Encode(e11);
    dest[destStride * 2] = encoder.Encode(e20); dest[destStride * 2 + 1] = encoder.Encode(e21);
  }
}

// Unsymmetric HQ2x4 kernels
file readonly struct Hq2x4NormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace where TKey : unmanaged, IColorSpace where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey> where TLerp : struct, ILerp<TWork> where TEncode : struct, IEncode<TWork, TPixel> {
  public int ScaleX => 2;
  public int ScaleY => 4;
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var n0 = window.M1M1; var n1 = window.P0M1; var n2 = window.P1M1;
    var n3 = window.M1P0; var n4 = window.P0P0; var n5 = window.P1P0;
    var n6 = window.M1P1; var n7 = window.P0P1; var n8 = window.P1P1;
    var c0 = n0.Key; var c1 = n1.Key; var c2 = n2.Key; var c3 = n3.Key; var c4 = n4.Key;
    var c5 = n5.Key; var c6 = n6.Key; var c7 = n7.Key; var c8 = n8.Key;
    var w0 = n0.Work; var w1 = n1.Work; var w2 = n2.Work; var w3 = n3.Work; var w4 = n4.Work;
    var w5 = n5.Work; var w6 = n6.Work; var w7 = n7.Work; var w8 = n8.Work;
    var pattern = HqHelpers.ComputePatternNormal(c0, c1, c2, c3, c4, c5, c6, c7, c8, equality);
    TWork e00, e01, e10, e11, e20, e21, e30, e31;
    e00 = e01 = e10 = e11 = e20 = e21 = e30 = e31 = w4;
    Hq2x4Patterns.Apply(pattern, w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8,
      ref e00, ref e01, ref e10, ref e11, ref e20, ref e21, ref e30, ref e31, lerp, equality);
    dest[0] = encoder.Encode(e00); dest[1] = encoder.Encode(e01);
    dest[destStride] = encoder.Encode(e10); dest[destStride + 1] = encoder.Encode(e11);
    dest[destStride * 2] = encoder.Encode(e20); dest[destStride * 2 + 1] = encoder.Encode(e21);
    dest[destStride * 3] = encoder.Encode(e30); dest[destStride * 3 + 1] = encoder.Encode(e31);
  }
}

file readonly struct Hq2x4BoldKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace where TKey : unmanaged, IColorSpace where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey> where TLerp : struct, ILerp<TWork> where TEncode : struct, IEncode<TWork, TPixel> {
  public int ScaleX => 2;
  public int ScaleY => 4;
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var n0 = window.M1M1; var n1 = window.P0M1; var n2 = window.P1M1;
    var n3 = window.M1P0; var n4 = window.P0P0; var n5 = window.P1P0;
    var n6 = window.M1P1; var n7 = window.P0P1; var n8 = window.P1P1;
    var c0 = n0.Key; var c1 = n1.Key; var c2 = n2.Key; var c3 = n3.Key; var c4 = n4.Key;
    var c5 = n5.Key; var c6 = n6.Key; var c7 = n7.Key; var c8 = n8.Key;
    var w0 = n0.Work; var w1 = n1.Work; var w2 = n2.Work; var w3 = n3.Work; var w4 = n4.Work;
    var w5 = n5.Work; var w6 = n6.Work; var w7 = n7.Work; var w8 = n8.Work;
    var pattern = HqHelpers.ComputePatternBold(w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8, equality);
    TWork e00, e01, e10, e11, e20, e21, e30, e31;
    e00 = e01 = e10 = e11 = e20 = e21 = e30 = e31 = w4;
    Hq2x4Patterns.Apply(pattern, w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8,
      ref e00, ref e01, ref e10, ref e11, ref e20, ref e21, ref e30, ref e31, lerp, equality);
    dest[0] = encoder.Encode(e00); dest[1] = encoder.Encode(e01);
    dest[destStride] = encoder.Encode(e10); dest[destStride + 1] = encoder.Encode(e11);
    dest[destStride * 2] = encoder.Encode(e20); dest[destStride * 2 + 1] = encoder.Encode(e21);
    dest[destStride * 3] = encoder.Encode(e30); dest[destStride * 3 + 1] = encoder.Encode(e31);
  }
}

file readonly struct Hq2x4SmartKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace where TKey : unmanaged, IColorSpace where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey> where TLerp : struct, ILerp<TWork> where TEncode : struct, IEncode<TWork, TPixel> {
  public int ScaleX => 2;
  public int ScaleY => 4;
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var n0 = window.M1M1; var n1 = window.P0M1; var n2 = window.P1M1;
    var n3 = window.M1P0; var n4 = window.P0P0; var n5 = window.P1P0;
    var n6 = window.M1P1; var n7 = window.P0P1; var n8 = window.P1P1;
    var c0 = n0.Key; var c1 = n1.Key; var c2 = n2.Key; var c3 = n3.Key; var c4 = n4.Key;
    var c5 = n5.Key; var c6 = n6.Key; var c7 = n7.Key; var c8 = n8.Key;
    var w0 = n0.Work; var w1 = n1.Work; var w2 = n2.Work; var w3 = n3.Work; var w4 = n4.Work;
    var w5 = n5.Work; var w6 = n6.Work; var w7 = n7.Work; var w8 = n8.Work;
    var pattern = HqHelpers.ShouldUseBold(c0, c2, c4, c6, c8, equality)
      ? HqHelpers.ComputePatternBold(w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8, equality)
      : HqHelpers.ComputePatternNormal(c0, c1, c2, c3, c4, c5, c6, c7, c8, equality);
    TWork e00, e01, e10, e11, e20, e21, e30, e31;
    e00 = e01 = e10 = e11 = e20 = e21 = e30 = e31 = w4;
    Hq2x4Patterns.Apply(pattern, w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8,
      ref e00, ref e01, ref e10, ref e11, ref e20, ref e21, ref e30, ref e31, lerp, equality);
    dest[0] = encoder.Encode(e00); dest[1] = encoder.Encode(e01);
    dest[destStride] = encoder.Encode(e10); dest[destStride + 1] = encoder.Encode(e11);
    dest[destStride * 2] = encoder.Encode(e20); dest[destStride * 2 + 1] = encoder.Encode(e21);
    dest[destStride * 3] = encoder.Encode(e30); dest[destStride * 3 + 1] = encoder.Encode(e31);
  }
}

// Unsymmetric LQ2x3 kernels
file readonly struct Lq2x3NormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace where TKey : unmanaged, IColorSpace where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey> where TLerp : struct, ILerp<TWork> where TEncode : struct, IEncode<TWork, TPixel> {
  public int ScaleX => 2;
  public int ScaleY => 3;
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var n0 = window.M1M1; var n1 = window.P0M1; var n2 = window.P1M1;
    var n3 = window.M1P0; var n4 = window.P0P0; var n5 = window.P1P0;
    var n6 = window.M1P1; var n7 = window.P0P1; var n8 = window.P1P1;
    var c0 = n0.Key; var c1 = n1.Key; var c2 = n2.Key; var c3 = n3.Key; var c4 = n4.Key;
    var c5 = n5.Key; var c6 = n6.Key; var c7 = n7.Key; var c8 = n8.Key;
    var w0 = n0.Work; var w1 = n1.Work; var w2 = n2.Work; var w3 = n3.Work; var w4 = n4.Work;
    var w5 = n5.Work; var w6 = n6.Work; var w7 = n7.Work; var w8 = n8.Work;
    var pattern = HqHelpers.ComputePatternNormal(c0, c1, c2, c3, c4, c5, c6, c7, c8, equality);
    TWork e00, e01, e10, e11, e20, e21;
    e00 = e01 = e10 = e11 = e20 = e21 = w4;
    Lq2x3Patterns.Apply(pattern, w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8,
      ref e00, ref e01, ref e10, ref e11, ref e20, ref e21, lerp, equality);
    dest[0] = encoder.Encode(e00); dest[1] = encoder.Encode(e01);
    dest[destStride] = encoder.Encode(e10); dest[destStride + 1] = encoder.Encode(e11);
    dest[destStride * 2] = encoder.Encode(e20); dest[destStride * 2 + 1] = encoder.Encode(e21);
  }
}

file readonly struct Lq2x3BoldKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace where TKey : unmanaged, IColorSpace where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey> where TLerp : struct, ILerp<TWork> where TEncode : struct, IEncode<TWork, TPixel> {
  public int ScaleX => 2;
  public int ScaleY => 3;
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var n0 = window.M1M1; var n1 = window.P0M1; var n2 = window.P1M1;
    var n3 = window.M1P0; var n4 = window.P0P0; var n5 = window.P1P0;
    var n6 = window.M1P1; var n7 = window.P0P1; var n8 = window.P1P1;
    var c0 = n0.Key; var c1 = n1.Key; var c2 = n2.Key; var c3 = n3.Key; var c4 = n4.Key;
    var c5 = n5.Key; var c6 = n6.Key; var c7 = n7.Key; var c8 = n8.Key;
    var w0 = n0.Work; var w1 = n1.Work; var w2 = n2.Work; var w3 = n3.Work; var w4 = n4.Work;
    var w5 = n5.Work; var w6 = n6.Work; var w7 = n7.Work; var w8 = n8.Work;
    var pattern = HqHelpers.ComputePatternBold(w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8, equality);
    TWork e00, e01, e10, e11, e20, e21;
    e00 = e01 = e10 = e11 = e20 = e21 = w4;
    Lq2x3Patterns.Apply(pattern, w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8,
      ref e00, ref e01, ref e10, ref e11, ref e20, ref e21, lerp, equality);
    dest[0] = encoder.Encode(e00); dest[1] = encoder.Encode(e01);
    dest[destStride] = encoder.Encode(e10); dest[destStride + 1] = encoder.Encode(e11);
    dest[destStride * 2] = encoder.Encode(e20); dest[destStride * 2 + 1] = encoder.Encode(e21);
  }
}

file readonly struct Lq2x3SmartKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace where TKey : unmanaged, IColorSpace where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey> where TLerp : struct, ILerp<TWork> where TEncode : struct, IEncode<TWork, TPixel> {
  public int ScaleX => 2;
  public int ScaleY => 3;
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var n0 = window.M1M1; var n1 = window.P0M1; var n2 = window.P1M1;
    var n3 = window.M1P0; var n4 = window.P0P0; var n5 = window.P1P0;
    var n6 = window.M1P1; var n7 = window.P0P1; var n8 = window.P1P1;
    var c0 = n0.Key; var c1 = n1.Key; var c2 = n2.Key; var c3 = n3.Key; var c4 = n4.Key;
    var c5 = n5.Key; var c6 = n6.Key; var c7 = n7.Key; var c8 = n8.Key;
    var w0 = n0.Work; var w1 = n1.Work; var w2 = n2.Work; var w3 = n3.Work; var w4 = n4.Work;
    var w5 = n5.Work; var w6 = n6.Work; var w7 = n7.Work; var w8 = n8.Work;
    var pattern = HqHelpers.ShouldUseBold(c0, c2, c4, c6, c8, equality)
      ? HqHelpers.ComputePatternBold(w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8, equality)
      : HqHelpers.ComputePatternNormal(c0, c1, c2, c3, c4, c5, c6, c7, c8, equality);
    TWork e00, e01, e10, e11, e20, e21;
    e00 = e01 = e10 = e11 = e20 = e21 = w4;
    Lq2x3Patterns.Apply(pattern, w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8,
      ref e00, ref e01, ref e10, ref e11, ref e20, ref e21, lerp, equality);
    dest[0] = encoder.Encode(e00); dest[1] = encoder.Encode(e01);
    dest[destStride] = encoder.Encode(e10); dest[destStride + 1] = encoder.Encode(e11);
    dest[destStride * 2] = encoder.Encode(e20); dest[destStride * 2 + 1] = encoder.Encode(e21);
  }
}

// Unsymmetric LQ2x4 kernels
file readonly struct Lq2x4NormalKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace where TKey : unmanaged, IColorSpace where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey> where TLerp : struct, ILerp<TWork> where TEncode : struct, IEncode<TWork, TPixel> {
  public int ScaleX => 2;
  public int ScaleY => 4;
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var n0 = window.M1M1; var n1 = window.P0M1; var n2 = window.P1M1;
    var n3 = window.M1P0; var n4 = window.P0P0; var n5 = window.P1P0;
    var n6 = window.M1P1; var n7 = window.P0P1; var n8 = window.P1P1;
    var c0 = n0.Key; var c1 = n1.Key; var c2 = n2.Key; var c3 = n3.Key; var c4 = n4.Key;
    var c5 = n5.Key; var c6 = n6.Key; var c7 = n7.Key; var c8 = n8.Key;
    var w0 = n0.Work; var w1 = n1.Work; var w2 = n2.Work; var w3 = n3.Work; var w4 = n4.Work;
    var w5 = n5.Work; var w6 = n6.Work; var w7 = n7.Work; var w8 = n8.Work;
    var pattern = HqHelpers.ComputePatternNormal(c0, c1, c2, c3, c4, c5, c6, c7, c8, equality);
    TWork e00, e01, e10, e11, e20, e21, e30, e31;
    e00 = e01 = e10 = e11 = e20 = e21 = e30 = e31 = w4;
    Lq2x4Patterns.Apply(pattern, w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8,
      ref e00, ref e01, ref e10, ref e11, ref e20, ref e21, ref e30, ref e31, lerp, equality);
    dest[0] = encoder.Encode(e00); dest[1] = encoder.Encode(e01);
    dest[destStride] = encoder.Encode(e10); dest[destStride + 1] = encoder.Encode(e11);
    dest[destStride * 2] = encoder.Encode(e20); dest[destStride * 2 + 1] = encoder.Encode(e21);
    dest[destStride * 3] = encoder.Encode(e30); dest[destStride * 3 + 1] = encoder.Encode(e31);
  }
}

file readonly struct Lq2x4BoldKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace where TKey : unmanaged, IColorSpace where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey> where TLerp : struct, ILerp<TWork> where TEncode : struct, IEncode<TWork, TPixel> {
  public int ScaleX => 2;
  public int ScaleY => 4;
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var n0 = window.M1M1; var n1 = window.P0M1; var n2 = window.P1M1;
    var n3 = window.M1P0; var n4 = window.P0P0; var n5 = window.P1P0;
    var n6 = window.M1P1; var n7 = window.P0P1; var n8 = window.P1P1;
    var c0 = n0.Key; var c1 = n1.Key; var c2 = n2.Key; var c3 = n3.Key; var c4 = n4.Key;
    var c5 = n5.Key; var c6 = n6.Key; var c7 = n7.Key; var c8 = n8.Key;
    var w0 = n0.Work; var w1 = n1.Work; var w2 = n2.Work; var w3 = n3.Work; var w4 = n4.Work;
    var w5 = n5.Work; var w6 = n6.Work; var w7 = n7.Work; var w8 = n8.Work;
    var pattern = HqHelpers.ComputePatternBold(w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8, equality);
    TWork e00, e01, e10, e11, e20, e21, e30, e31;
    e00 = e01 = e10 = e11 = e20 = e21 = e30 = e31 = w4;
    Lq2x4Patterns.Apply(pattern, w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8,
      ref e00, ref e01, ref e10, ref e11, ref e20, ref e21, ref e30, ref e31, lerp, equality);
    dest[0] = encoder.Encode(e00); dest[1] = encoder.Encode(e01);
    dest[destStride] = encoder.Encode(e10); dest[destStride + 1] = encoder.Encode(e11);
    dest[destStride * 2] = encoder.Encode(e20); dest[destStride * 2 + 1] = encoder.Encode(e21);
    dest[destStride * 3] = encoder.Encode(e30); dest[destStride * 3 + 1] = encoder.Encode(e31);
  }
}

file readonly struct Lq2x4SmartKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace where TKey : unmanaged, IColorSpace where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey> where TLerp : struct, ILerp<TWork> where TEncode : struct, IEncode<TWork, TPixel> {
  public int ScaleX => 2;
  public int ScaleY => 4;
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var n0 = window.M1M1; var n1 = window.P0M1; var n2 = window.P1M1;
    var n3 = window.M1P0; var n4 = window.P0P0; var n5 = window.P1P0;
    var n6 = window.M1P1; var n7 = window.P0P1; var n8 = window.P1P1;
    var c0 = n0.Key; var c1 = n1.Key; var c2 = n2.Key; var c3 = n3.Key; var c4 = n4.Key;
    var c5 = n5.Key; var c6 = n6.Key; var c7 = n7.Key; var c8 = n8.Key;
    var w0 = n0.Work; var w1 = n1.Work; var w2 = n2.Work; var w3 = n3.Work; var w4 = n4.Work;
    var w5 = n5.Work; var w6 = n6.Work; var w7 = n7.Work; var w8 = n8.Work;
    var pattern = HqHelpers.ShouldUseBold(c0, c2, c4, c6, c8, equality)
      ? HqHelpers.ComputePatternBold(w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8, equality)
      : HqHelpers.ComputePatternNormal(c0, c1, c2, c3, c4, c5, c6, c7, c8, equality);
    TWork e00, e01, e10, e11, e20, e21, e30, e31;
    e00 = e01 = e10 = e11 = e20 = e21 = e30 = e31 = w4;
    Lq2x4Patterns.Apply(pattern, w0, w1, w2, w3, w4, w5, w6, w7, w8, c0, c1, c2, c3, c4, c5, c6, c7, c8,
      ref e00, ref e01, ref e10, ref e11, ref e20, ref e21, ref e30, ref e31, lerp, equality);
    dest[0] = encoder.Encode(e00); dest[1] = encoder.Encode(e01);
    dest[destStride] = encoder.Encode(e10); dest[destStride + 1] = encoder.Encode(e11);
    dest[destStride * 2] = encoder.Encode(e20); dest[destStride * 2 + 1] = encoder.Encode(e21);
    dest[destStride * 3] = encoder.Encode(e30); dest[destStride * 3 + 1] = encoder.Encode(e31);
  }
}

#endregion

// HQ2x and LQ2x patterns have been moved to their respective partial class files (Hq2xPatterns.cs, Lq2xPatterns.cs)
// HQ3x, HQ4x, LQ3x, and LQ4x patterns have been moved to their respective partial class files
// (Hq3xPatterns.cs, Hq4xPatterns.cs, Lq3xPatterns.cs, Lq4xPatterns.cs)
