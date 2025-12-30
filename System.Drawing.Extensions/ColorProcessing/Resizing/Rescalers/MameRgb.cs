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
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Rescalers;

/// <summary>
/// MAME RGB 2x - LCD RGB channel filter.
/// </summary>
/// <remarks>
/// <para>Scales images by 2x simulating LCD RGB subpixel arrangement.</para>
/// <para>
/// Output pattern:
/// [Red only] [Green only]
/// [Blue only] [Full RGB]
/// </para>
/// <para>From MAME emulator.</para>
/// </remarks>
[ScalerInfo("MAME RGB 2x", Author = "MAME Team", Year = 1997,
  Description = "LCD RGB subpixel filter at 2x", Category = ScalerCategory.PixelArt)]
public readonly struct MameRgb2x : IPixelScaler {

  /// <inheritdoc />
  public ScaleFactor Scale => new(2, 2);

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
    where TEncode : struct, IEncode<TWork, TPixel> {
    if (typeof(TPixel) != typeof(Bgra8888))
      throw new NotSupportedException($"{nameof(MameRgb2x)} requires TPixel to be {nameof(Bgra8888)}, but got {typeof(TPixel).Name}");

    return callback.Invoke(new MameRgb2xKernel<TWork, TKey, TPixel, TEncode>());
  }

  /// <summary>
  /// Gets the list of scale factors supported.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2)];

  /// <summary>
  /// Determines whether the specified scale factor is supported.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 };

  /// <summary>
  /// Enumerates all possible target dimensions.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
  }

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static MameRgb2x Default => new();
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

    // Encode TWork → TPixel, then reinterpret as Bgra8888
    var encoded = encoder.Encode(pixel);
    ref readonly var pix = ref Unsafe.As<TPixel, Bgra8888>(ref encoded);

    // Extract byte components
    var r = pix.R;
    var g = pix.G;
    var b = pix.B;
    var a = pix.A;

    // Create isolated-channel colors as Bgra8888
    var redOnly = new Bgra8888(r, 0, 0, a);
    var greenOnly = new Bgra8888(0, g, 0, a);
    var blueOnly = new Bgra8888(0, 0, b, a);

    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;

    // Write to output (Unsafe.As back to TPixel)
    row0[0] = Unsafe.As<Bgra8888, TPixel>(ref redOnly);
    row0[1] = Unsafe.As<Bgra8888, TPixel>(ref greenOnly);
    row1[0] = Unsafe.As<Bgra8888, TPixel>(ref blueOnly);
    row1[1] = encoded;
  }
}

/// <summary>
/// MAME RGB 3x - LCD RGB channel filter at 3x.
/// </summary>
/// <remarks>
/// <para>Scales images by 3x simulating LCD RGB subpixel arrangement.</para>
/// <para>
/// Output pattern creates a mosaic of individual color channels
/// with full-color pixels at key positions.
/// </para>
/// <para>From MAME emulator.</para>
/// </remarks>
[ScalerInfo("MAME RGB 3x", Author = "MAME Team", Year = 1997,
  Description = "LCD RGB subpixel filter at 3x", Category = ScalerCategory.PixelArt)]
public readonly struct MameRgb3x : IPixelScaler {

  /// <inheritdoc />
  public ScaleFactor Scale => new(3, 3);

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
    where TEncode : struct, IEncode<TWork, TPixel> {
    if (typeof(TPixel) != typeof(Bgra8888))
      throw new NotSupportedException($"{nameof(MameRgb3x)} requires TPixel to be {nameof(Bgra8888)}, but got {typeof(TPixel).Name}");

    return callback.Invoke(new MameRgb3xKernel<TWork, TKey, TPixel, TEncode>());
  }

  /// <summary>
  /// Gets the list of scale factors supported.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(3, 3)];

  /// <summary>
  /// Determines whether the specified scale factor is supported.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 3, Y: 3 };

  /// <summary>
  /// Enumerates all possible target dimensions.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 3, sourceHeight * 3);
  }

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static MameRgb3x Default => new();
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

    // Encode TWork → TPixel, then reinterpret as Bgra8888
    var encoded = encoder.Encode(pixel);
    ref readonly var pix = ref Unsafe.As<TPixel, Bgra8888>(ref encoded);

    // Extract byte components
    var r = pix.R;
    var g = pix.G;
    var b = pix.B;
    var a = pix.A;

    // Create isolated-channel colors as Bgra8888
    var redOnly = new Bgra8888(r, 0, 0, a);
    var greenOnly = new Bgra8888(0, g, 0, a);
    var blueOnly = new Bgra8888(0, 0, b, a);

    // Convert to TPixel
    var full = encoded;
    var red = Unsafe.As<Bgra8888, TPixel>(ref redOnly);
    var green = Unsafe.As<Bgra8888, TPixel>(ref greenOnly);
    var blue = Unsafe.As<Bgra8888, TPixel>(ref blueOnly);

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
