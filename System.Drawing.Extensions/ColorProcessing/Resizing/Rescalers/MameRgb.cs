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
/// MAME RGB - LCD RGB channel filter (2x, 3x).
/// </summary>
/// <remarks>
/// <para>Scales images simulating LCD RGB subpixel arrangement.</para>
/// <para>
/// 2x mode: Output pattern: [Red only] [Green only] / [Blue only] [Full RGB]
/// 3x mode: Creates a mosaic of individual color channels with full-color pixels at key positions.
/// </para>
/// <para>From MAME emulator.</para>
/// </remarks>
[ScalerInfo("MAME RGB", Author = "MAME Team", Year = 1997,
  Description = "LCD RGB subpixel filter", Category = ScalerCategory.PixelArt)]
public readonly struct MameRgb : IPixelScaler {

  private readonly int _scale;

  /// <summary>
  /// Creates a new MameRgb instance.
  /// </summary>
  /// <param name="scale">Scale factor (2 or 3).</param>
  public MameRgb(int scale = 2) {
    ArgumentOutOfRangeException.ThrowIfLessThan(scale, 2);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(scale, 3);
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
    where TDistance : struct, IColorMetric<TKey>, INormalizedMetric
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
    where TEncode : struct, IEncode<TWork, TPixel>
    => this._scale switch {
      0 or 2 => callback.Invoke(new MameRgb2xKernel<TWork, TKey, TPixel, TEncode>()),
      3 => callback.Invoke(new MameRgb3xKernel<TWork, TKey, TPixel, TEncode>()),
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
  public static MameRgb Scale2x => new(2);

  /// <summary>
  /// Gets a 3x scale instance.
  /// </summary>
  public static MameRgb Scale3x => new(3);

  /// <summary>
  /// Gets the default configuration (2x).
  /// </summary>
  public static MameRgb Default => Scale2x;
}

file readonly struct MameRgb2xKernel<TWork, TKey, TPixel, TEncode>
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

    // Create single-channel pixels using ColorConverter
    var redOnly = ColorConverter.FromNormalizedRgba<TWork>(r, 0, 0, a);
    var greenOnly = ColorConverter.FromNormalizedRgba<TWork>(0, g, 0, a);
    var blueOnly = ColorConverter.FromNormalizedRgba<TWork>(0, 0, b, a);

    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;

    row0[0] = encoder.Encode(redOnly);
    row0[1] = encoder.Encode(greenOnly);
    row1[0] = encoder.Encode(blueOnly);
    row1[1] = encoder.Encode(pixel);
  }
}

file readonly struct MameRgb3xKernel<TWork, TKey, TPixel, TEncode>
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

    // Encode to TPixel
    var full = encoder.Encode(pixel);
    var red = encoder.Encode(redOnly);
    var green = encoder.Encode(greenOnly);
    var blue = encoder.Encode(blueOnly);

    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;
    var row2 = row1 + destStride;

    // Pattern from original MAME:
    // [full]  [green] [blue]
    // [blue]  [full]  [red]
    // [red]   [green] [full]
    row0[0] = full;
    row0[1] = green;
    row0[2] = blue;
    row1[0] = blue;
    row1[1] = full;
    row1[2] = red;
    row2[0] = red;
    row2[1] = green;
    row2[2] = full;
  }
}
