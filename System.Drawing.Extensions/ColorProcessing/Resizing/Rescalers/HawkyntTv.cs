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
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Rescalers;

/// <summary>
/// Hawkynt TV - RGB channel separation effect (2x, 3x).
/// </summary>
/// <remarks>
/// <para>Scales images simulating LCD/CRT RGB sub-pixel arrangement.</para>
/// <para>
/// 2x mode: Each 2x2 output block shows:
/// [0,0] = Red channel only,
/// [1,0] = Green channel only,
/// [0,1] = Blue channel only,
/// [1,1] = Luminance (grayscale).
/// </para>
/// <para>
/// 3x mode: Creates a 3x3 RGB stripe pattern with each row showing
/// Red, Green, Blue sub-pixels across the columns.
/// </para>
/// <para>Original algorithm by Hawkynt, 1998.</para>
/// </remarks>
[ScalerInfo("Hawkynt TV", Author = "Hawkynt", Year = 1998,
  Description = "RGB channel separation effect", Category = ScalerCategory.PixelArt)]
public readonly struct HawkyntTv : IPixelScaler {

  private readonly int _scale;

  /// <summary>
  /// Creates a new HawkyntTv instance.
  /// </summary>
  /// <param name="scale">Scale factor (2 or 3).</param>
  public HawkyntTv(int scale = 2) {
    if (scale is not (2 or 3))
      throw new ArgumentOutOfRangeException(nameof(scale), scale, "HawkyntTv supports 2x, 3x scaling");
    this._scale = scale;
  }

  /// <inheritdoc />
  public ScaleFactor Scale => this._scale == 0 ? new(2, 2) : new(this._scale, this._scale);

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
    => this._scale switch {
      0 or 2 => callback.Invoke(new HawkyntTv2xKernel<TWork, TKey, TPixel, TEncode>()),
      3 => callback.Invoke(new HawkyntTv3xKernel<TWork, TKey, TPixel, TEncode>()),
      _ => throw new InvalidOperationException($"Invalid scale factor: {this._scale}")
    };

  /// <summary>
  /// Gets the list of scale factors supported.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2), new(3, 3)];

  /// <summary>
  /// Determines whether the specified scale factor is supported.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 } or { X: 3, Y: 3 };

  /// <summary>
  /// Enumerates all possible target dimensions.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
    yield return (sourceWidth * 3, sourceHeight * 3);
  }

  /// <summary>
  /// Gets a 2x scale instance.
  /// </summary>
  public static HawkyntTv Scale2x => new(2);

  /// <summary>
  /// Gets a 3x scale instance.
  /// </summary>
  public static HawkyntTv Scale3x => new(3);

  /// <summary>
  /// Gets the default configuration (2x).
  /// </summary>
  public static HawkyntTv Default => Scale2x;
}

file readonly struct HawkyntTv2xKernel<TWork, TKey, TPixel, TEncode>
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 2;
  public int ScaleY => 2;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    var pixel = window.P0P0.Work;

    // Get RGBA components using ColorConverter
    var (r, g, b) = ColorConverter.GetNormalizedRgb(pixel);
    var a = ColorConverter.GetAlpha(pixel);
    var luminance = ColorConverter.GetLuminance(pixel);

    // Create single-channel pixels using ColorConverter
    var redOnly = ColorConverter.FromNormalizedRgba<TWork>(r, 0, 0, a);
    var greenOnly = ColorConverter.FromNormalizedRgba<TWork>(0, g, 0, a);
    var blueOnly = ColorConverter.FromNormalizedRgba<TWork>(0, 0, b, a);
    var grey = ColorConverter.FromNormalizedRgba<TWork>(luminance, luminance, luminance, a);

    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;

    // [0,0] = Red only
    row0[0] = encoder.Encode(redOnly);
    // [1,0] = Green only
    row0[1] = encoder.Encode(greenOnly);
    // [0,1] = Blue only
    row1[0] = encoder.Encode(blueOnly);
    // [1,1] = Luminance
    row1[1] = encoder.Encode(grey);
  }
}

file readonly struct HawkyntTv3xKernel<TWork, TKey, TPixel, TEncode>
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 3;
  public int ScaleY => 3;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    var pixel = window.P0P0.Work;

    // Get RGBA components using ColorConverter
    var (r, g, b) = ColorConverter.GetNormalizedRgb(pixel);
    var a = ColorConverter.GetAlpha(pixel);

    // Create single-channel pixels using ColorConverter
    var redOnly = ColorConverter.FromNormalizedRgba<TWork>(r, 0, 0, a);
    var greenOnly = ColorConverter.FromNormalizedRgba<TWork>(0, g, 0, a);
    var blueOnly = ColorConverter.FromNormalizedRgba<TWork>(0, 0, b, a);

    var encodedR = encoder.Encode(redOnly);
    var encodedG = encoder.Encode(greenOnly);
    var encodedB = encoder.Encode(blueOnly);

    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;
    var row2 = row1 + destStride;

    // All 3 rows show R, G, B stripe pattern
    // This creates vertical RGB stripes like an LCD display
    row0[0] = encodedR;
    row0[1] = encodedG;
    row0[2] = encodedB;
    row1[0] = encodedR;
    row1[1] = encodedG;
    row1[2] = encodedB;
    row2[0] = encodedR;
    row2[1] = encodedG;
    row2[2] = encodedB;
  }
}
