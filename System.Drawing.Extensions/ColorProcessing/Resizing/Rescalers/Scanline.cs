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

using System.Collections.Generic;
using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Rescalers;

/// <summary>
/// Horizontal scanline effect - doubles width with alternating original/darkened columns.
/// </summary>
/// <remarks>
/// Creates a CRT-like effect with vertical dark lines between pixels.
/// Scale factor: 2x1 (width doubled, height unchanged).
/// </remarks>
[ScalerInfo("Horizontal Scanlines", Author = "Hawkynt", Year = 2008,
  Description = "CRT-like vertical scanline effect", Category = ScalerCategory.PixelArt)]
public readonly struct ScanlineHorizontal : IPixelScaler {

  private readonly float _brightness;

  /// <summary>
  /// Creates a horizontal scanline scaler with the specified brightness factor.
  /// </summary>
  /// <param name="brightness">Brightness factor for scanlines (0.0 = black, 1.0 = original). Default is 0.5.</param>
  public ScanlineHorizontal(float brightness = 0.5f) => this._brightness = brightness;

  /// <inheritdoc />
  public ScaleFactor Scale => new(2, 1);

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
    => callback.Invoke(new ScanlineHorizontalKernel<TWork, TKey, TPixel, TLerp, TEncode>(this._brightness, lerp));

  /// <summary>
  /// Gets the list of scale factors supported.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 1)];

  /// <summary>
  /// Determines whether the specified scale factor is supported.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 1 };

  /// <summary>
  /// Enumerates all possible target dimensions.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight);
  }

  /// <summary>
  /// Gets the default configuration (50% brightness for scanlines).
  /// </summary>
  public static ScanlineHorizontal Default => new();

  /// <summary>
  /// Creates a configuration with the specified brightness.
  /// </summary>
  public ScanlineHorizontal WithBrightness(float brightness) => new(brightness);
}

file readonly struct ScanlineHorizontalKernel<TWork, TKey, TPixel, TLerp, TEncode>(float brightness, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 2;
  public int ScaleY => 1;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    var pixel = window.P0P0.Work;
    var darkened = lerp.Lerp(default, pixel, brightness);

    destTopLeft[0] = encoder.Encode(pixel);
    destTopLeft[1] = encoder.Encode(darkened);
  }
}

/// <summary>
/// Vertical scanline effect - doubles height with alternating original/darkened rows.
/// </summary>
/// <remarks>
/// Creates a CRT-like effect with horizontal dark lines between pixels.
/// Scale factor: 1x2 (width unchanged, height doubled).
/// </remarks>
[ScalerInfo("Vertical Scanlines", Author = "Hawkynt", Year = 2008,
  Description = "CRT-like horizontal scanline effect", Category = ScalerCategory.PixelArt)]
public readonly struct ScanlineVertical : IPixelScaler {

  private readonly float _brightness;

  /// <summary>
  /// Creates a vertical scanline scaler with the specified brightness factor.
  /// </summary>
  /// <param name="brightness">Brightness factor for scanlines (0.0 = black, 1.0 = original). Default is 0.5.</param>
  public ScanlineVertical(float brightness = 0.5f) => this._brightness = brightness;

  /// <inheritdoc />
  public ScaleFactor Scale => new(1, 2);

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
    => callback.Invoke(new ScanlineVerticalKernel<TWork, TKey, TPixel, TLerp, TEncode>(this._brightness, lerp));

  /// <summary>
  /// Gets the list of scale factors supported.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(1, 2)];

  /// <summary>
  /// Determines whether the specified scale factor is supported.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 1, Y: 2 };

  /// <summary>
  /// Enumerates all possible target dimensions.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth, sourceHeight * 2);
  }

  /// <summary>
  /// Gets the default configuration (50% brightness for scanlines).
  /// </summary>
  public static ScanlineVertical Default => new();

  /// <summary>
  /// Creates a configuration with the specified brightness.
  /// </summary>
  public ScanlineVertical WithBrightness(float brightness) => new(brightness);
}

file readonly struct ScanlineVerticalKernel<TWork, TKey, TPixel, TLerp, TEncode>(float brightness, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 1;
  public int ScaleY => 2;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    var pixel = window.P0P0.Work;
    var darkened = lerp.Lerp(default, pixel, brightness);

    destTopLeft[0] = encoder.Encode(pixel);
    destTopLeft[destStride] = encoder.Encode(darkened);
  }
}
