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
/// MAME AdvInterp - Advanced interpolation similar to Scale2x/Scale3x (2x, 3x).
/// </summary>
/// <remarks>
/// <para>Scales images using interpolation-based corner and edge detection.</para>
/// <para>
/// Similar to Scale2x/Scale3x but uses weighted interpolation (5:3 ratio) when
/// corners are detected, producing smoother edges.
/// </para>
/// <para>From MAME emulator, modified by Hawkynt for threshold support.</para>
/// </remarks>
[ScalerInfo("MAME AdvInterp", Author = "MAME Team", Year = 1997,
  Description = "Advanced interpolation similar to Scale2x/Scale3x", Category = ScalerCategory.PixelArt)]
public readonly struct MameAdvInterp : IPixelScaler {

  private readonly int _scale;

  /// <summary>
  /// Creates a new MameAdvInterp instance.
  /// </summary>
  /// <param name="scale">Scale factor (2 or 3).</param>
  public MameAdvInterp(int scale = 2) {
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
      0 or 2 => callback.Invoke(new MameAdvInterp2xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      3 => callback.Invoke(new MameAdvInterp3xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
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
  public static MameAdvInterp Scale2x => new(2);

  /// <summary>
  /// Gets a 3x scale instance.
  /// </summary>
  public static MameAdvInterp Scale3x => new(3);

  /// <summary>
  /// Gets the default configuration (2x).
  /// </summary>
  public static MameAdvInterp Default => Scale2x;
}

file readonly struct MameAdvInterp2xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
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
    // Cross neighborhood
    // NeighborWindow uses [Row][Column] naming: M1P0 = row -1, col 0 = TOP
    var c1 = window.M1P0; // top (row -1, col 0)
    var c3 = window.P0M1; // left (row 0, col -1)
    var c4 = window.P0P0; // center (row 0, col 0)
    var c5 = window.P0P1; // right (row 0, col +1)
    var c7 = window.P1P0; // bottom (row +1, col 0)

    var center = c4.Work;

    // Default all outputs to center
    var e00 = center;
    var e01 = center;
    var e10 = center;
    var e11 = center;

    // Pre-compute keys
    var k1 = c1.Key;
    var k3 = c3.Key;
    var k5 = c5.Key;
    var k7 = c7.Key;

    // AdvInterp condition: top != bottom AND left != right
    if (!equality.Equals(k1, k7) && !equality.Equals(k3, k5)) {
      // Corners with weighted interpolation (5:3 ratio)
      // avg * 5/8 + center * 3/8 where avg = (neighbor1 + neighbor2) / 2
      // lerp(avg, center, 3/8) = avg * 5/8 + center * 3/8

      if (equality.Equals(k3, k1)) {
        var avg = lerp.Lerp(c1.Work, c3.Work);
        e00 = lerp.Lerp(avg, center, 5, 3);
      }

      if (equality.Equals(k5, k1)) {
        var avg = lerp.Lerp(c1.Work, c5.Work);
        e01 = lerp.Lerp(avg, center, 5, 3);
      }

      if (equality.Equals(k3, k7)) {
        var avg = lerp.Lerp(c7.Work, c3.Work);
        e10 = lerp.Lerp(avg, center, 5, 3);
      }

      if (equality.Equals(k5, k7)) {
        var avg = lerp.Lerp(c7.Work, c5.Work);
        e11 = lerp.Lerp(avg, center, 5, 3);
      }
    }

    // Write 2x2 output
    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;

    row0[0] = encoder.Encode(e00);
    row0[1] = encoder.Encode(e01);
    row1[0] = encoder.Encode(e10);
    row1[1] = encoder.Encode(e11);
  }
}

file readonly struct MameAdvInterp3xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
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
    // Full 3x3 neighborhood
    // NeighborWindow uses [Row][Column] naming: M1P0 = row -1, col 0 = TOP
    var c0 = window.M1M1; // top-left (row -1, col -1)
    var c1 = window.M1P0; // top (row -1, col 0)
    var c2 = window.M1P1; // top-right (row -1, col +1)
    var c3 = window.P0M1; // left (row 0, col -1)
    var c4 = window.P0P0; // center (row 0, col 0)
    var c5 = window.P0P1; // right (row 0, col +1)
    var c6 = window.P1M1; // bottom-left (row +1, col -1)
    var c7 = window.P1P0; // bottom (row +1, col 0)
    var c8 = window.P1P1; // bottom-right (row +1, col +1)

    var center = c4.Work;

    // Default all outputs to center
    var e00 = center;
    var e01 = center;
    var e02 = center;
    var e10 = center;
    var e11 = center;
    var e12 = center;
    var e20 = center;
    var e21 = center;
    var e22 = center;

    // Pre-compute keys
    var k0 = c0.Key;
    var k1 = c1.Key;
    var k2 = c2.Key;
    var k3 = c3.Key;
    var k4 = c4.Key;
    var k5 = c5.Key;
    var k6 = c6.Key;
    var k7 = c7.Key;
    var k8 = c8.Key;

    // AdvInterp condition: top != bottom AND left != right
    if (!equality.Equals(k1, k7) && !equality.Equals(k3, k5)) {
      // Corners with weighted interpolation (5:3 ratio)
      // avg * 5/8 + center * 3/8 where avg = (neighbor1 + neighbor2) / 2
      // lerp(avg, center, 3/8) = avg * 5/8 + center * 3/8

      if (equality.Equals(k3, k1)) {
        var avg = lerp.Lerp(c3.Work, c1.Work);
        e00 = lerp.Lerp(avg, center, 5, 3);
      }

      if (equality.Equals(k1, k5)) {
        var avg = lerp.Lerp(c5.Work, c1.Work);
        e02 = lerp.Lerp(avg, center, 5, 3);
      }

      if (equality.Equals(k3, k7)) {
        var avg = lerp.Lerp(c3.Work, c7.Work);
        e20 = lerp.Lerp(avg, center, 5, 3);
      }

      if (equality.Equals(k7, k5)) {
        var avg = lerp.Lerp(c7.Work, c5.Work);
        e22 = lerp.Lerp(avg, center, 5, 3);
      }

      // Edge interpolation
      var eq31 = equality.Equals(k3, k1);
      var eq15 = equality.Equals(k1, k5);
      var eq37 = equality.Equals(k3, k7);
      var eq75 = equality.Equals(k7, k5);

      // Top edge
      if (eq31 && !equality.Equals(k4, k2) && eq15 && !equality.Equals(k4, k0)) {
        // Equal blend of 3 colors: (c1 + c3 + c5) / 3
        var avg = lerp.Lerp(c3.Work, c5.Work);
        e01 = lerp.Lerp(avg, c1.Work, 2, 1);
      } else if (eq31 && !equality.Equals(k4, k2)) {
        // 50/50 blend: (c3 + c1) / 2
        e01 = lerp.Lerp(c3.Work, c1.Work);
      } else if (eq15 && !equality.Equals(k4, k0)) {
        // 50/50 blend: (c5 + c1) / 2
        e01 = lerp.Lerp(c5.Work, c1.Work);
      }

      // Left edge
      if (eq31 && !equality.Equals(k4, k6) && eq37 && !equality.Equals(k4, k0)) {
        // Equal blend of 3 colors: (c3 + c1 + c7) / 3
        var avg = lerp.Lerp(c1.Work, c7.Work);
        e10 = lerp.Lerp(avg, c3.Work, 2, 1);
      } else if (eq31 && !equality.Equals(k4, k6)) {
        // 50/50 blend: (c3 + c1) / 2
        e10 = lerp.Lerp(c3.Work, c1.Work);
      } else if (eq37 && !equality.Equals(k4, k0)) {
        // 50/50 blend: (c3 + c7) / 2
        e10 = lerp.Lerp(c3.Work, c7.Work);
      }

      // Right edge
      if (eq15 && !equality.Equals(k4, k8) && eq75 && !equality.Equals(k4, k2)) {
        // Equal blend of 3 colors: (c5 + c1 + c7) / 3
        var avg = lerp.Lerp(c1.Work, c7.Work);
        e12 = lerp.Lerp(avg, c5.Work, 2, 1);
      } else if (eq15 && !equality.Equals(k4, k8)) {
        // 50/50 blend: (c5 + c1) / 2
        e12 = lerp.Lerp(c5.Work, c1.Work);
      } else if (eq75 && !equality.Equals(k4, k2)) {
        // 50/50 blend: (c5 + c7) / 2
        e12 = lerp.Lerp(c5.Work, c7.Work);
      }

      // Bottom edge
      if (eq37 && !equality.Equals(k4, k8) && eq75 && !equality.Equals(k4, k6)) {
        // Equal blend of 3 colors: (c7 + c3 + c5) / 3
        var avg = lerp.Lerp(c3.Work, c5.Work);
        e21 = lerp.Lerp(avg, c7.Work, 2, 1);
      } else if (eq37 && !equality.Equals(k4, k8)) {
        // 50/50 blend: (c3 + c7) / 2
        e21 = lerp.Lerp(c3.Work, c7.Work);
      } else if (eq75 && !equality.Equals(k4, k6)) {
        // 50/50 blend: (c5 + c7) / 2
        e21 = lerp.Lerp(c5.Work, c7.Work);
      }
    }

    // Write 3x3 output
    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;
    var row2 = row1 + destStride;

    row0[0] = encoder.Encode(e00);
    row0[1] = encoder.Encode(e01);
    row0[2] = encoder.Encode(e02);
    row1[0] = encoder.Encode(e10);
    row1[1] = encoder.Encode(e11);
    row1[2] = encoder.Encode(e12);
    row2[0] = encoder.Encode(e20);
    row2[1] = encoder.Encode(e21);
    row2[2] = encoder.Encode(e22);
  }
}
