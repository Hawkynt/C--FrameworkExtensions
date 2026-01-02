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
/// MAME TV - CRT interlace emulation (2x, 3x).
/// </summary>
/// <remarks>
/// <para>Scales images simulating CRT TV scanlines.</para>
/// <para>
/// 2x mode: Top row shows full brightness, bottom row shows darkened pixels.
/// 3x mode: Top row full brightness, middle row 5/8 brightness, bottom row 5/16 brightness.
/// </para>
/// <para>From MAME emulator.</para>
/// </remarks>
[ScalerInfo("MAME TV", Author = "MAME Team", Year = 1997,
  Description = "CRT interlace emulation", Category = ScalerCategory.PixelArt)]
public readonly struct MameTv : IPixelScaler {

  private readonly int _scale;
  private readonly float _gamma1;
  private readonly float _gamma2;

  /// <summary>
  /// Default gamma for the first scanline darkening (5/8 brightness).
  /// </summary>
  public const float DefaultGamma1 = 5f / 8f;

  /// <summary>
  /// Default gamma for the second scanline darkening (5/16 brightness).
  /// </summary>
  public const float DefaultGamma2 = 5f / 16f;

  /// <summary>
  /// Creates a new MameTv instance.
  /// </summary>
  /// <param name="scale">Scale factor (2 or 3).</param>
  /// <param name="gamma1">Gamma for first darkened scanline (0.0 = black, 1.0 = full).</param>
  /// <param name="gamma2">Gamma for second darkened scanline (3x only).</param>
  public MameTv(int scale = 2, float gamma1 = DefaultGamma1, float gamma2 = DefaultGamma2) {
    if (scale is not (2 or 3))
      throw new ArgumentOutOfRangeException(nameof(scale), scale, "MameTv supports 2x, 3x scaling");
    this._scale = scale;
    this._gamma1 = gamma1;
    this._gamma2 = gamma2;
  }

  /// <summary>
  /// Gets the gamma factor for the first darkened scanline.
  /// </summary>
  public float Gamma1 => this._gamma1 == 0f ? DefaultGamma1 : this._gamma1;

  /// <summary>
  /// Gets the gamma factor for the second darkened scanline (3x only).
  /// </summary>
  public float Gamma2 => this._gamma2 == 0f ? DefaultGamma2 : this._gamma2;

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
      0 or 2 => callback.Invoke(new MameTv2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp, this.Gamma1)),
      3 => callback.Invoke(new MameTv3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp, this.Gamma1, this.Gamma2)),
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
  /// Creates a new MameTv with the specified gamma values.
  /// </summary>
  public MameTv WithGamma(float gamma1, float gamma2 = DefaultGamma2) => new(this._scale == 0 ? 2 : this._scale, gamma1, gamma2);

  /// <summary>
  /// Gets a 2x scale instance.
  /// </summary>
  public static MameTv Scale2x => new(2);

  /// <summary>
  /// Gets a 3x scale instance.
  /// </summary>
  public static MameTv Scale3x => new(3);

  /// <summary>
  /// Gets the default configuration (2x).
  /// </summary>
  public static MameTv Default => Scale2x;
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
