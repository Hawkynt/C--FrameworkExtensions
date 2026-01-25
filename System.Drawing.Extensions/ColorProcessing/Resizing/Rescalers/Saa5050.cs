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
/// Mullard SAA5050 teletext character smoothing algorithm.
/// </summary>
/// <remarks>
/// <para>Reference: SAA5050 datasheet (Mullard 1980)</para>
/// <para>Algorithm: Non-square 2x3 scaling with edge-aware smoothing for teletext displays.</para>
/// <para>The SAA5050 was a teletext character generator IC that used smoothing to improve readability.</para>
/// <para>This replicates the diagonal smoothing behavior of the original hardware.</para>
/// </remarks>
[ScalerInfo("SAA5050", Author = "Mullard", Year = 1980,
  Url = "https://en.wikipedia.org/wiki/Mullard_SAA5050",
  Description = "Teletext character smoothing with 2x3 scaling", Category = ScalerCategory.PixelArt)]
public readonly struct Saa5050 : IPixelScaler {

  /// <inheritdoc />
  public ScaleFactor Scale => new(2, 3);

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
    => callback.Invoke(new Saa5050Kernel<TWork, TKey, TPixel, TEquality, TEncode>(equality));

  /// <summary>
  /// Gets the list of scale factors supported.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 3)];

  /// <summary>
  /// Determines whether the specified scale factor is supported.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 3 };

  /// <summary>
  /// Enumerates all possible target dimensions.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 3);
  }

  /// <summary>Gets the default SAA5050 scaler.</summary>
  public static Saa5050 Default => new();
}

#region SAA5050 Kernel

file readonly struct Saa5050Kernel<TWork, TKey, TPixel, TEquality, TEncode>(TEquality equality = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 2;
  public int ScaleY => 3;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var center = window.P0P0;
    var top = window.M1P0;
    var bottom = window.P1P0;
    var left = window.P0M1;
    var right = window.P0P1;

    var centerPixel = encoder.Encode(center.Work);

    // Default all 6 pixels to center color
    var p00 = centerPixel; // Top-left
    var p10 = centerPixel; // Top-right
    var p01 = centerPixel; // Middle-left
    var p11 = centerPixel; // Middle-right
    var p02 = centerPixel; // Bottom-left
    var p12 = centerPixel; // Bottom-right

    // SAA5050 diagonal smoothing rules:
    // Top row: blend with top neighbor if matching side neighbor
    var topKey = top.Key;
    var leftKey = left.Key;
    var rightKey = right.Key;
    var bottomKey = bottom.Key;
    var centerKey = center.Key;

    // Top-left corner: if top matches left and both differ from center, use top
    if (equality.Equals(topKey, leftKey) && !equality.Equals(topKey, centerKey))
      p00 = encoder.Encode(top.Work);

    // Top-right corner: if top matches right and both differ from center, use top
    if (equality.Equals(topKey, rightKey) && !equality.Equals(topKey, centerKey))
      p10 = encoder.Encode(top.Work);

    // Bottom-left corner: if bottom matches left and both differ from center, use bottom
    if (equality.Equals(bottomKey, leftKey) && !equality.Equals(bottomKey, centerKey))
      p02 = encoder.Encode(bottom.Work);

    // Bottom-right corner: if bottom matches right and both differ from center, use bottom
    if (equality.Equals(bottomKey, rightKey) && !equality.Equals(bottomKey, centerKey))
      p12 = encoder.Encode(bottom.Work);

    // Write the 2x3 block
    dest[0] = p00;
    dest[1] = p10;

    dest[destStride] = p01;
    dest[destStride + 1] = p11;

    dest[2 * destStride] = p02;
    dest[2 * destStride + 1] = p12;
  }
}

#endregion
