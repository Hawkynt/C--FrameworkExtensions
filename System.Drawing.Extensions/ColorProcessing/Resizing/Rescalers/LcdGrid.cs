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
/// LCD Grid scaler - simulates LCD subpixel structure.
/// </summary>
/// <remarks>
/// <para>Reference: https://github.com/libretro/slang-shaders/tree/master/handheld</para>
/// <para>Algorithm: Creates RGB subpixel pattern with inter-pixel gap simulation.</para>
/// <para>Simulates how colors appear on LCD displays with visible pixel structure.</para>
/// <para>Requires a 4-component float color space (e.g., LinearRgbaF) as working space.</para>
/// </remarks>
[ScalerInfo("LcdGrid",
  Url = "https://github.com/libretro/slang-shaders/tree/master/handheld",
  Description = "LCD subpixel grid simulation", Category = ScalerCategory.Resampler)]
public readonly struct LcdGrid : IPixelScaler {
  private readonly int _scale;

  /// <summary>
  /// Creates an LcdGrid scaler with the specified factor.
  /// </summary>
  /// <param name="scale">Scale factor (2, 3, or 4).</param>
  public LcdGrid(int scale = 3) {
    if (scale is < 2 or > 4)
      throw new ArgumentOutOfRangeException(nameof(scale), scale, "LcdGrid supports 2x, 3x, or 4x scaling");
    this._scale = scale;
  }

  /// <inheritdoc />
  public ScaleFactor Scale => this._scale == 0 ? new(3, 3) : new(this._scale, this._scale);

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
      2 => callback.Invoke(new LcdGrid2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      0 or 3 => callback.Invoke(new LcdGrid3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      4 => callback.Invoke(new LcdGrid4xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      _ => throw new InvalidOperationException($"Invalid scale factor: {this._scale}")
    };

  /// <summary>
  /// Gets the list of scale factors supported.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2), new(3, 3), new(4, 4)];

  /// <summary>
  /// Determines whether the specified scale factor is supported.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 } or { X: 3, Y: 3 } or { X: 4, Y: 4 };

  /// <summary>
  /// Enumerates all possible target dimensions.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
    yield return (sourceWidth * 3, sourceHeight * 3);
    yield return (sourceWidth * 4, sourceHeight * 4);
  }

  #region Static Presets

  /// <summary>Gets a 2x LcdGrid scaler.</summary>
  public static LcdGrid X2 => new(2);

  /// <summary>Gets a 3x LcdGrid scaler.</summary>
  public static LcdGrid X3 => new(3);

  /// <summary>Gets a 4x LcdGrid scaler.</summary>
  public static LcdGrid X4 => new(4);

  /// <summary>Gets the default LcdGrid scaler (3x).</summary>
  public static LcdGrid Default => X3;

  #endregion
}

#region LcdGrid Helpers

file static class LcdGridHelpers {
  /// <summary>Weight scale for integer lerp operations.</summary>
  public const int WeightScale = 1000;

  /// <summary>Grid line darkness factor (15% darker).</summary>
  public const int GridDarkWeight = 850;

  /// <summary>Subpixel blend factor (85%).</summary>
  public const int SubpixelBlend = 850;

  /// <summary>Off-channel dim factor (30% of SubpixelBlend).</summary>
  public const int OffChannelWeight = 255;

  /// <summary>
  /// Applies grid line darkening to a color.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TWork ApplyGridDarkening<TWork, TLerp>(in TLerp lerp, in TWork color)
    where TWork : unmanaged, IColorSpace
    where TLerp : struct, ILerp<TWork>
    => lerp.Lerp(default, color, WeightScale - GridDarkWeight, GridDarkWeight);
}

#endregion

#region LcdGrid 2x Kernel

file readonly struct LcdGrid2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 2;
  public int ScaleY => 2;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var pixel = window.P0P0.Work;
    var darkPixel = LcdGridHelpers.ApplyGridDarkening(lerp, pixel);
    var dimPixel = lerp.Lerp(default, pixel, LcdGridHelpers.WeightScale - LcdGridHelpers.OffChannelWeight, LcdGridHelpers.OffChannelWeight);

    // 2x2 grid with simple RGB pattern:
    // Row 0: [RGB bright] [dark edge]
    // Row 1: [dark edge]  [dark edge]
    dest[0] = encoder.Encode(pixel);
    dest[1] = encoder.Encode(darkPixel);
    dest[destStride] = encoder.Encode(darkPixel);
    dest[destStride + 1] = encoder.Encode(dimPixel);
  }
}

#endregion

#region LcdGrid 3x Kernel

file readonly struct LcdGrid3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 3;
  public int ScaleY => 3;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var pixel = window.P0P0.Work;
    var pixelEncoded = encoder.Encode(pixel);
    var darkPixel = encoder.Encode(LcdGridHelpers.ApplyGridDarkening(lerp, pixel));
    var dimPixel = encoder.Encode(lerp.Lerp(default, pixel, LcdGridHelpers.WeightScale - LcdGridHelpers.SubpixelBlend, LcdGridHelpers.SubpixelBlend));

    // 3x3 grid pattern simulating RGB subpixels:
    // [R-ish] [G-ish] [B-ish]
    // [R-ish] [G-ish] [dark ]
    // [dark ] [dark ] [dark ]
    // Since we can't manipulate individual channels with generic color space,
    // we use brightness variation to suggest subpixel structure
    dest[0] = pixelEncoded;
    dest[1] = pixelEncoded;
    dest[2] = dimPixel;
    dest[destStride] = pixelEncoded;
    dest[destStride + 1] = dimPixel;
    dest[destStride + 2] = darkPixel;
    dest[2 * destStride] = dimPixel;
    dest[2 * destStride + 1] = darkPixel;
    dest[2 * destStride + 2] = darkPixel;
  }
}

#endregion

#region LcdGrid 4x Kernel

file readonly struct LcdGrid4xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 4;
  public int ScaleY => 4;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var pixel = window.P0P0.Work;
    var pixelEncoded = encoder.Encode(pixel);
    var darkPixel = encoder.Encode(LcdGridHelpers.ApplyGridDarkening(lerp, pixel));
    var dimPixel = encoder.Encode(lerp.Lerp(default, pixel, LcdGridHelpers.WeightScale - LcdGridHelpers.SubpixelBlend, LcdGridHelpers.SubpixelBlend));

    // 4x4 grid pattern with subpixel structure:
    // [full] [full] [full] [dark]
    // [full] [dim ] [dim ] [dark]
    // [dim ] [dim ] [dim ] [dark]
    // [dark] [dark] [dark] [dark]

    // Row 0: mostly full brightness with dark edge
    dest[0] = pixelEncoded;
    dest[1] = pixelEncoded;
    dest[2] = pixelEncoded;
    dest[3] = darkPixel;

    // Row 1: gradient
    dest[destStride] = pixelEncoded;
    dest[destStride + 1] = dimPixel;
    dest[destStride + 2] = dimPixel;
    dest[destStride + 3] = darkPixel;

    // Row 2: dimmer
    dest[2 * destStride] = dimPixel;
    dest[2 * destStride + 1] = dimPixel;
    dest[2 * destStride + 2] = dimPixel;
    dest[2 * destStride + 3] = darkPixel;

    // Row 3: dark edge
    dest[3 * destStride] = darkPixel;
    dest[3 * destStride + 1] = darkPixel;
    dest[3 * destStride + 2] = darkPixel;
    dest[3 * destStride + 3] = darkPixel;
  }
}

#endregion
