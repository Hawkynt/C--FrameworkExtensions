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
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Pipeline;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Scalers;

/// <summary>
/// MAME TV 2x - CRT interlace emulation.
/// </summary>
/// <remarks>
/// <para>Scales images by 2x simulating CRT TV scanlines.</para>
/// <para>
/// Top row of each output block shows full brightness pixels.
/// Bottom row shows darkened pixels (5/8 brightness) to simulate scanlines.
/// </para>
/// <para>From MAME emulator.</para>
/// </remarks>
[ScalerInfo("MAME TV 2x", Author = "MAME Team", Year = 1997,
  Description = "CRT interlace emulation at 2x", Category = ScalerCategory.PixelArt)]
public readonly struct MameTv2x(float gamma = 5f / 8f) : IPixelScaler {

  /// <summary>
  /// Gets the gamma factor for scanline darkening (0.0 = black, 1.0 = full brightness).
  /// </summary>
  public float Gamma { get; } = gamma;

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
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new MameTv2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp, this.Gamma));

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
  public static MameTv2x Default => new();
}

file readonly struct MameTv2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp, float gamma)
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
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    var pixel = window.P0P0.Work;
    var subPixel = lerp.Lerp(default, pixel, gamma);

    var encodedPixel = encoder.Encode(pixel);
    var encodedSubPixel = encoder.Encode(subPixel);

    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;

    row0[0] = encodedPixel;
    row0[1] = encodedPixel;
    row1[0] = encodedSubPixel;
    row1[1] = encodedSubPixel;
  }
}

/// <summary>
/// MAME TV 3x - CRT interlace emulation at 3x.
/// </summary>
/// <remarks>
/// <para>Scales images by 3x simulating CRT TV scanlines with gradient.</para>
/// <para>
/// Top row shows full brightness, middle row at 5/8 brightness,
/// bottom row at 5/16 brightness for a gradient scanline effect.
/// </para>
/// <para>From MAME emulator.</para>
/// </remarks>
[ScalerInfo("MAME TV 3x", Author = "MAME Team", Year = 1997,
  Description = "CRT interlace emulation at 3x with gradient", Category = ScalerCategory.PixelArt)]
public readonly struct MameTv3x(float gamma1 = 5f / 8f, float gamma2 = 5f / 16f) : IPixelScaler {

  /// <summary>
  /// Gets the gamma factor for the middle scanline (0.0 = black, 1.0 = full brightness).
  /// </summary>
  public float Gamma1 { get; } = gamma1;

  /// <summary>
  /// Gets the gamma factor for the bottom scanline (0.0 = black, 1.0 = full brightness).
  /// </summary>
  public float Gamma2 { get; } = gamma2;

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
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new MameTv3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp, this.Gamma1, this.Gamma2));

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
  public static MameTv3x Default => new();
}

file readonly struct MameTv3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp, float gamma1, float gamma2)
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
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    var pixel = window.P0P0.Work;
    var subPixel1 = lerp.Lerp(default, pixel, gamma1);
    var subPixel2 = lerp.Lerp(default, pixel, gamma2);

    var encodedPixel = encoder.Encode(pixel);
    var encodedSubPixel1 = encoder.Encode(subPixel1);
    var encodedSubPixel2 = encoder.Encode(subPixel2);

    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;
    var row2 = row1 + destStride;

    row0[0] = encodedPixel;
    row0[1] = encodedPixel;
    row0[2] = encodedPixel;
    row1[0] = encodedSubPixel1;
    row1[1] = encodedSubPixel1;
    row1[2] = encodedSubPixel1;
    row2[0] = encodedSubPixel2;
    row2[1] = encodedSubPixel2;
    row2[2] = encodedSubPixel2;
  }
}
