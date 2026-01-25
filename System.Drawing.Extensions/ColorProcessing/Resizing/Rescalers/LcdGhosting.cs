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
/// LCD ghosting/response time blur scaler.
/// </summary>
/// <remarks>
/// <para>Reference: https://github.com/libretro/slang-shaders/tree/master/handheld</para>
/// <para>Algorithm: Simulates LCD response time blur by blending with spatial neighbors.</para>
/// <para>Emulates the motion blur effect caused by slow LCD pixel response times.</para>
/// </remarks>
[ScalerInfo("LCD Ghosting",
  Url = "https://github.com/libretro/slang-shaders/tree/master/handheld",
  Description = "LCD response time blur simulation", Category = ScalerCategory.Resampler)]
public readonly struct LcdGhosting : IPixelScaler {
  private readonly int _scale;

  /// <summary>
  /// Creates an LcdGhosting scaler with the specified factor.
  /// </summary>
  /// <param name="scale">Scale factor (2, 3, or 4).</param>
  public LcdGhosting(int scale = 2) {
    if (scale is < 2 or > 4)
      throw new ArgumentOutOfRangeException(nameof(scale), scale, "LCD Ghosting supports 2x, 3x, or 4x scaling");
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
      0 or 2 => callback.Invoke(new LcdGhosting2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      3 => callback.Invoke(new LcdGhosting3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      4 => callback.Invoke(new LcdGhosting4xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
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

  /// <summary>Gets a 2x LCD Ghosting scaler.</summary>
  public static LcdGhosting X2 => new(2);

  /// <summary>Gets a 3x LCD Ghosting scaler.</summary>
  public static LcdGhosting X3 => new(3);

  /// <summary>Gets a 4x LCD Ghosting scaler.</summary>
  public static LcdGhosting X4 => new(4);

  /// <summary>Gets the default LCD Ghosting scaler (2x).</summary>
  public static LcdGhosting Default => X2;

  #endregion
}

#region LcdGhosting Helpers

file static class LcdGhostingHelpers {
  /// <summary>Weight scale for integer lerp operations.</summary>
  public const int WeightScale = 1000;

  /// <summary>Ghost weight for neighbor blending (15%).</summary>
  public const int GhostWeight = 150;

  /// <summary>Center weight (100% - 4*15% = 40%).</summary>
  public const int CenterWeight = WeightScale - 4 * GhostWeight;

  /// <summary>Grid darkness factor (8% darker at edges).</summary>
  public const int GridDarkWeight = 920;

  /// <summary>
  /// Blends center pixel with 4 neighbors for ghosting effect.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TWork BlendGhosting<TWork, TLerp>(
    in TLerp lerp,
    in TWork center,
    in TWork left,
    in TWork right,
    in TWork top,
    in TWork bottom)
    where TWork : unmanaged, IColorSpace
    where TLerp : struct, ILerp<TWork> {
    // Blend: center*40% + left*15% + right*15% + top*15% + bottom*15%
    // Do it in steps: first blend left+right, then top+bottom, then combine with center
    var horizontal = lerp.Lerp(left, right, 500, 500);
    var vertical = lerp.Lerp(top, bottom, 500, 500);
    var neighbors = lerp.Lerp(horizontal, vertical, 500, 500);

    // Blend center (40%) with average of neighbors (60% total = 4*15%)
    return lerp.Lerp(center, neighbors, CenterWeight, WeightScale - CenterWeight);
  }

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

#region LcdGhosting 2x Kernel

file readonly struct LcdGhosting2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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
    var center = window.P0P0.Work;
    var left = window.P0M1.Work;
    var right = window.P0P1.Work;
    var top = window.M1P0.Work;
    var bottom = window.P1P0.Work;

    // Blend with neighbors for ghosting effect
    var ghosted = LcdGhostingHelpers.BlendGhosting(lerp, center, left, right, top, bottom);
    var ghostedPixel = encoder.Encode(ghosted);

    // Apply grid darkening to edge pixels
    var gridPixel = encoder.Encode(LcdGhostingHelpers.ApplyGridDarkening(lerp, ghosted));

    // 2x2 pattern with grid edges:
    // [ghosted] [grid]
    // [grid]    [grid]
    dest[0] = ghostedPixel;
    dest[1] = gridPixel;
    dest[destStride] = gridPixel;
    dest[destStride + 1] = gridPixel;
  }
}

#endregion

#region LcdGhosting 3x Kernel

file readonly struct LcdGhosting3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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
    var center = window.P0P0.Work;
    var left = window.P0M1.Work;
    var right = window.P0P1.Work;
    var top = window.M1P0.Work;
    var bottom = window.P1P0.Work;

    // Blend with neighbors for ghosting effect
    var ghosted = LcdGhostingHelpers.BlendGhosting(lerp, center, left, right, top, bottom);
    var ghostedPixel = encoder.Encode(ghosted);
    var gridPixel = encoder.Encode(LcdGhostingHelpers.ApplyGridDarkening(lerp, ghosted));

    // 3x3 pattern:
    // [ghosted] [ghosted] [grid]
    // [ghosted] [ghosted] [grid]
    // [grid]    [grid]    [grid]
    dest[0] = ghostedPixel;
    dest[1] = ghostedPixel;
    dest[2] = gridPixel;

    dest[destStride] = ghostedPixel;
    dest[destStride + 1] = ghostedPixel;
    dest[destStride + 2] = gridPixel;

    dest[2 * destStride] = gridPixel;
    dest[2 * destStride + 1] = gridPixel;
    dest[2 * destStride + 2] = gridPixel;
  }
}

#endregion

#region LcdGhosting 4x Kernel

file readonly struct LcdGhosting4xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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
    var center = window.P0P0.Work;
    var left = window.P0M1.Work;
    var right = window.P0P1.Work;
    var top = window.M1P0.Work;
    var bottom = window.P1P0.Work;

    // Blend with neighbors for ghosting effect
    var ghosted = LcdGhostingHelpers.BlendGhosting(lerp, center, left, right, top, bottom);
    var ghostedPixel = encoder.Encode(ghosted);
    var gridPixel = encoder.Encode(LcdGhostingHelpers.ApplyGridDarkening(lerp, ghosted));

    // 4x4 pattern:
    // [ghosted] [ghosted] [ghosted] [grid]
    // [ghosted] [ghosted] [ghosted] [grid]
    // [ghosted] [ghosted] [ghosted] [grid]
    // [grid]    [grid]    [grid]    [grid]
    for (var dy = 0; dy < 3; ++dy) {
      var row = dest + dy * destStride;
      row[0] = ghostedPixel;
      row[1] = ghostedPixel;
      row[2] = ghostedPixel;
      row[3] = gridPixel;
    }

    var lastRow = dest + 3 * destStride;
    lastRow[0] = gridPixel;
    lastRow[1] = gridPixel;
    lastRow[2] = gridPixel;
    lastRow[3] = gridPixel;
  }
}

#endregion
