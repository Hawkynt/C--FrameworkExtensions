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
/// Game Boy Color shader scaler with LCD persistence effect.
/// </summary>
/// <remarks>
/// <para>Reference: https://github.com/libretro/slang-shaders/tree/master/handheld</para>
/// <para>Algorithm: Simulates GBC LCD characteristics including pixel grid and color tinting.</para>
/// <para>The Game Boy Color had a TFT LCD with improved colors but still visible pixel structure.</para>
/// </remarks>
[ScalerInfo("GameBoy Shader",
  Url = "https://github.com/libretro/slang-shaders/tree/master/handheld",
  Description = "Game Boy Color LCD simulation with pixel grid", Category = ScalerCategory.Resampler)]
public readonly struct GameBoyShader : IPixelScaler {
  private readonly int _scale;

  /// <summary>
  /// Creates a GameBoy Shader scaler with the specified factor.
  /// </summary>
  /// <param name="scale">Scale factor (2, 3, or 4).</param>
  public GameBoyShader(int scale = 2) {
    if (scale is < 2 or > 4)
      throw new ArgumentOutOfRangeException(nameof(scale), scale, "GameBoy Shader supports 2x, 3x, or 4x scaling");
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
      0 or 2 => callback.Invoke(new GameBoyShader2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      3 => callback.Invoke(new GameBoyShader3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      4 => callback.Invoke(new GameBoyShader4xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
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

  /// <summary>Gets a 2x GameBoy Shader scaler.</summary>
  public static GameBoyShader X2 => new(2);

  /// <summary>Gets a 3x GameBoy Shader scaler.</summary>
  public static GameBoyShader X3 => new(3);

  /// <summary>Gets a 4x GameBoy Shader scaler.</summary>
  public static GameBoyShader X4 => new(4);

  /// <summary>Gets the default GameBoy Shader scaler (2x).</summary>
  public static GameBoyShader Default => X2;

  #endregion
}

#region GameBoyShader Helpers

file static class GameBoyShaderHelpers {
  /// <summary>Weight scale for integer lerp operations.</summary>
  public const int WeightScale = 1000;

  /// <summary>Grid line intensity (12% darker).</summary>
  public const int GridIntensity = 880;

  /// <summary>Alternate row scanline factor (97% brightness).</summary>
  public const int ScanlineFactor = 970;
}

#endregion

#region GameBoyShader 2x Kernel

file readonly struct GameBoyShader2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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
    var pixelEncoded = encoder.Encode(pixel);

    // Grid edge (88% brightness for grid lines)
    var gridPixel = lerp.Lerp(default, pixel, GameBoyShaderHelpers.WeightScale - GameBoyShaderHelpers.GridIntensity, GameBoyShaderHelpers.GridIntensity);
    var gridEncoded = encoder.Encode(gridPixel);

    // 2x2 pattern with grid edges:
    // [full] [grid]
    // [grid] [grid]
    dest[0] = pixelEncoded;
    dest[1] = gridEncoded;
    dest[destStride] = gridEncoded;
    dest[destStride + 1] = gridEncoded;
  }
}

#endregion

#region GameBoyShader 3x Kernel

file readonly struct GameBoyShader3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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

    // Scanline row (97% brightness)
    var scanlinePixel = lerp.Lerp(default, pixel, GameBoyShaderHelpers.WeightScale - GameBoyShaderHelpers.ScanlineFactor, GameBoyShaderHelpers.ScanlineFactor);
    var scanlineEncoded = encoder.Encode(scanlinePixel);

    // Grid edge (88% brightness)
    var gridPixel = lerp.Lerp(default, pixel, GameBoyShaderHelpers.WeightScale - GameBoyShaderHelpers.GridIntensity, GameBoyShaderHelpers.GridIntensity);
    var gridEncoded = encoder.Encode(gridPixel);

    // 3x3 pattern:
    // [full] [full] [grid]
    // [scan] [scan] [grid]
    // [grid] [grid] [grid]
    dest[0] = pixelEncoded;
    dest[1] = pixelEncoded;
    dest[2] = gridEncoded;

    dest[destStride] = scanlineEncoded;
    dest[destStride + 1] = scanlineEncoded;
    dest[destStride + 2] = gridEncoded;

    dest[2 * destStride] = gridEncoded;
    dest[2 * destStride + 1] = gridEncoded;
    dest[2 * destStride + 2] = gridEncoded;
  }
}

#endregion

#region GameBoyShader 4x Kernel

file readonly struct GameBoyShader4xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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

    // Scanline row (97% brightness)
    var scanlinePixel = lerp.Lerp(default, pixel, GameBoyShaderHelpers.WeightScale - GameBoyShaderHelpers.ScanlineFactor, GameBoyShaderHelpers.ScanlineFactor);
    var scanlineEncoded = encoder.Encode(scanlinePixel);

    // Grid edge (88% brightness)
    var gridPixel = lerp.Lerp(default, pixel, GameBoyShaderHelpers.WeightScale - GameBoyShaderHelpers.GridIntensity, GameBoyShaderHelpers.GridIntensity);
    var gridEncoded = encoder.Encode(gridPixel);

    // 4x4 pattern:
    // [full] [full] [full] [grid]
    // [full] [scan] [scan] [grid]
    // [scan] [scan] [scan] [grid]
    // [grid] [grid] [grid] [grid]
    dest[0] = pixelEncoded;
    dest[1] = pixelEncoded;
    dest[2] = pixelEncoded;
    dest[3] = gridEncoded;

    dest[destStride] = pixelEncoded;
    dest[destStride + 1] = scanlineEncoded;
    dest[destStride + 2] = scanlineEncoded;
    dest[destStride + 3] = gridEncoded;

    dest[2 * destStride] = scanlineEncoded;
    dest[2 * destStride + 1] = scanlineEncoded;
    dest[2 * destStride + 2] = scanlineEncoded;
    dest[2 * destStride + 3] = gridEncoded;

    dest[3 * destStride] = gridEncoded;
    dest[3 * destStride + 1] = gridEncoded;
    dest[3 * destStride + 2] = gridEncoded;
    dest[3 * destStride + 3] = gridEncoded;
  }
}

#endregion
