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
/// ScummVM Simple scaler - basic point duplication for pixel-art scaling.
/// </summary>
/// <remarks>
/// <para>Each source pixel is duplicated to fill NxN target pixels.</para>
/// <para>This is identical to DOSBox's Normal scaler - pure nearest-neighbor with integer factors.</para>
/// <para>Fastest pixel-art scaler with no edge detection or interpolation.</para>
/// <para>Reference: ScummVM project (https://wiki.scummvm.org/index.php/Scalers)</para>
/// </remarks>
[ScalerInfo("Simple", Author = "ScummVM Team", Year = 2001,
  Url = "https://wiki.scummvm.org/index.php/Scalers",
  Description = "Basic point duplication scaling from ScummVM", Category = ScalerCategory.PixelArt)]
public readonly struct Simple : IPixelScaler {
  private readonly int _scale;

  /// <summary>
  /// Creates a Simple scaler with the specified factor.
  /// </summary>
  /// <param name="scale">Scale factor (2, 3, or 4).</param>
  public Simple(int scale = 2) {
    ArgumentOutOfRangeException.ThrowIfLessThan(scale, 2);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(scale, 4);
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
      0 or 2 => callback.Invoke(new Simple2xKernel<TWork, TKey, TPixel, TEncode>()),
      3 => callback.Invoke(new Simple3xKernel<TWork, TKey, TPixel, TEncode>()),
      4 => callback.Invoke(new Simple4xKernel<TWork, TKey, TPixel, TEncode>()),
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

  /// <summary>Gets a 2x Simple scaler.</summary>
  public static Simple X2 => new(2);

  /// <summary>Gets a 3x Simple scaler.</summary>
  public static Simple X3 => new(3);

  /// <summary>Gets a 4x Simple scaler.</summary>
  public static Simple X4 => new(4);

  /// <summary>Gets the default Simple scaler (2x).</summary>
  public static Simple Default => X2;

  #endregion
}

#region Simple 2x Kernel

file readonly struct Simple2xKernel<TWork, TKey, TPixel, TEncode>
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
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    // Simple point duplication - center pixel fills entire 2x2 block
    var pixel = encoder.Encode(window.P0P0.Work);

    dest[0] = pixel;
    dest[1] = pixel;
    dest[destStride] = pixel;
    dest[destStride + 1] = pixel;
  }
}

#endregion

#region Simple 3x Kernel

file readonly struct Simple3xKernel<TWork, TKey, TPixel, TEncode>
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
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    // Simple point duplication - center pixel fills entire 3x3 block
    var pixel = encoder.Encode(window.P0P0.Work);

    // Row 0
    dest[0] = pixel;
    dest[1] = pixel;
    dest[2] = pixel;

    // Row 1
    var row1 = dest + destStride;
    row1[0] = pixel;
    row1[1] = pixel;
    row1[2] = pixel;

    // Row 2
    var row2 = row1 + destStride;
    row2[0] = pixel;
    row2[1] = pixel;
    row2[2] = pixel;
  }
}

#endregion

#region Simple 4x Kernel

file readonly struct Simple4xKernel<TWork, TKey, TPixel, TEncode>
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 4;
  public int ScaleY => 4;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    // Simple point duplication - center pixel fills entire 4x4 block
    var pixel = encoder.Encode(window.P0P0.Work);

    for (var dy = 0; dy < 4; ++dy) {
      var row = dest + dy * destStride;
      row[0] = pixel;
      row[1] = pixel;
      row[2] = pixel;
      row[3] = pixel;
    }
  }
}

#endregion
