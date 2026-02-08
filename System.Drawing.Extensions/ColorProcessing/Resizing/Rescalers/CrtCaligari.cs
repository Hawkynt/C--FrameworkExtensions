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
/// CRT-Caligari scaler - performance-focused CRT simulation with electron beam spot.
/// </summary>
/// <remarks>
/// <para>Reference: https://github.com/libretro/slang-shaders/tree/master/crt/shaders</para>
/// <para>Algorithm: Lightweight CRT with electron beam spot simulation and neighbor blending.</para>
/// <para>A simplified CRT shader designed for performance while maintaining good visual quality.</para>
/// </remarks>
[ScalerInfo("CRT-Caligari", Author = "Caligari",
  Url = "https://github.com/libretro/slang-shaders/tree/master/crt/shaders",
  Description = "Performance-focused CRT with electron beam spot simulation", Category = ScalerCategory.Resampler)]
public readonly struct CrtCaligari : IPixelScaler {
  private readonly int _scale;

  /// <summary>
  /// Creates a CRT-Caligari scaler with the specified factor.
  /// </summary>
  /// <param name="scale">Scale factor (2 or 3).</param>
  public CrtCaligari(int scale = 2) {
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
      0 or 2 => callback.Invoke(new CrtCaligari2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      3 => callback.Invoke(new CrtCaligari3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
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

  #region Static Presets

  /// <summary>Gets a 2x CRT-Caligari scaler.</summary>
  public static CrtCaligari X2 => new(2);

  /// <summary>Gets a 3x CRT-Caligari scaler.</summary>
  public static CrtCaligari X3 => new(3);

  /// <summary>Gets the default CRT-Caligari scaler (2x).</summary>
  public static CrtCaligari Default => X2;

  #endregion
}

#region CrtCaligari Helpers

file static class CrtCaligariHelpers {
  /// <summary>Weight scale for integer lerp operations.</summary>
  public const int WeightScale = 1000;

  /// <summary>Electron beam spot center intensity (full brightness).</summary>
  public const int SpotCenter = 1000;

  /// <summary>Electron beam spot edge intensity (65% - simulates spot falloff).</summary>
  public const int SpotEdge = 650;

  /// <summary>Neighbor blend weight for phosphor bloom (15%).</summary>
  public const int BloomBlend = 150;

  /// <summary>Scanline darkening (70% brightness).</summary>
  public const int ScanlineWeight = 700;

  /// <summary>Mid row brightness for 3x (80%).</summary>
  public const int MidWeight = 800;

  /// <summary>Dark scanline gap for 3x (40%).</summary>
  public const int DarkScanline = 400;
}

#endregion

#region CrtCaligari 2x Kernel

file readonly struct CrtCaligari2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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
    var right = window.P0P1.Work;
    var below = window.P1P0.Work;

    // Electron beam spot: center is full, edges blend with neighbors
    var spotRight = lerp.Lerp(center, right, CrtCaligariHelpers.WeightScale - CrtCaligariHelpers.BloomBlend, CrtCaligariHelpers.BloomBlend);
    var spotBelow = lerp.Lerp(center, below, CrtCaligariHelpers.WeightScale - CrtCaligariHelpers.BloomBlend, CrtCaligariHelpers.BloomBlend);

    // Apply spot intensity falloff on edges
    var edgeSpot = lerp.Lerp(default, spotRight, CrtCaligariHelpers.WeightScale - CrtCaligariHelpers.SpotEdge, CrtCaligariHelpers.SpotEdge);

    // Bottom row - scanline darkening
    var scanCenter = lerp.Lerp(default, spotBelow, CrtCaligariHelpers.WeightScale - CrtCaligariHelpers.ScanlineWeight, CrtCaligariHelpers.ScanlineWeight);
    var scanEdge = lerp.Lerp(default, edgeSpot, CrtCaligariHelpers.WeightScale - CrtCaligariHelpers.ScanlineWeight, CrtCaligariHelpers.ScanlineWeight);

    // 2x2 pattern:
    // [center-spot]  [edge-spot]
    // [scan-center]  [scan-edge]
    dest[0] = encoder.Encode(center);
    dest[1] = encoder.Encode(edgeSpot);
    dest[destStride] = encoder.Encode(scanCenter);
    dest[destStride + 1] = encoder.Encode(scanEdge);
  }
}

#endregion

#region CrtCaligari 3x Kernel

file readonly struct CrtCaligari3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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
    var right = window.P0P1.Work;
    var below = window.P1P0.Work;

    // Electron beam spot blending with neighbors
    var bloomRight = lerp.Lerp(center, right, CrtCaligariHelpers.WeightScale - CrtCaligariHelpers.BloomBlend, CrtCaligariHelpers.BloomBlend);
    var bloomBelow = lerp.Lerp(center, below, CrtCaligariHelpers.WeightScale - CrtCaligariHelpers.BloomBlend, CrtCaligariHelpers.BloomBlend);

    // Spot edge falloff
    var edgePixel = lerp.Lerp(default, bloomRight, CrtCaligariHelpers.WeightScale - CrtCaligariHelpers.SpotEdge, CrtCaligariHelpers.SpotEdge);

    // Mid row (80%)
    var midCenter = lerp.Lerp(default, bloomBelow, CrtCaligariHelpers.WeightScale - CrtCaligariHelpers.MidWeight, CrtCaligariHelpers.MidWeight);
    var midEdge = lerp.Lerp(default, edgePixel, CrtCaligariHelpers.WeightScale - CrtCaligariHelpers.MidWeight, CrtCaligariHelpers.MidWeight);

    // Scanline row (40%)
    var scanline = lerp.Lerp(default, center, CrtCaligariHelpers.WeightScale - CrtCaligariHelpers.DarkScanline, CrtCaligariHelpers.DarkScanline);
    var scanlinePixel = encoder.Encode(scanline);

    var centerEncoded = encoder.Encode(center);

    // 3x3 pattern:
    // Row 0: Full brightness with spot profile
    dest[0] = centerEncoded;
    dest[1] = centerEncoded;
    dest[2] = encoder.Encode(edgePixel);

    // Row 1: Medium brightness with bloom
    dest[destStride] = encoder.Encode(midCenter);
    dest[destStride + 1] = encoder.Encode(midCenter);
    dest[destStride + 2] = encoder.Encode(midEdge);

    // Row 2: Dark scanline gap
    dest[2 * destStride] = scanlinePixel;
    dest[2 * destStride + 1] = scanlinePixel;
    dest[2 * destStride + 2] = scanlinePixel;
  }
}

#endregion
