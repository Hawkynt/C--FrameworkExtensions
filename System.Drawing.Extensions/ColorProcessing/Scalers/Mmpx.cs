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
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Pipeline;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Scalers;

#region MMPX 2x

/// <summary>
/// MMPX 2x (Morgan McGuire Pixel eXpansion) - style-preserving pixel art magnification.
/// </summary>
/// <remarks>
/// <para>Modern pixel art upscaling that reconstructs curves, diagonal lines, and sharp corners.</para>
/// <para>Preserves the palette, transparency, and single-pixel features.</para>
/// <para>Uses edge-preserving interpolation with pattern matching.</para>
/// <para>Reference: https://casual-effects.com/research/McGuire2021PixelArt/index.html</para>
/// </remarks>
[ScalerInfo("MMPX 2x", Author = "Morgan McGuire", Year = 2021,
  Description = "MMPX 2x style-preserving pixel art scaling", Category = ScalerCategory.PixelArt,
  Url = "https://casual-effects.com/research/McGuire2021PixelArt/index.html")]
public readonly struct Mmpx2x : IPixelScaler {

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
    => callback.Invoke(new Mmpx2xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp));

  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2)];
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 };
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
  }
  public static Mmpx2x Default => new();
}

#endregion

#region MMPX 2x Kernel

file readonly struct Mmpx2xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
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
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    // 3x3 neighborhood
    var a = window.M1M1;
    var b = window.P0M1;
    var c = window.P1M1;
    var d = window.M1P0;
    var e = window.P0P0;
    var f = window.P1P0;
    var g = window.M1P1;
    var h = window.P0P1;
    var i = window.P1P1;

    var wE = e.Work;
    var kE = e.Key;

    // Default to nearest neighbor
    var e0 = wE;
    var e1 = wE;
    var e2 = wE;
    var e3 = wE;

    // Check if there are any similar neighbors
    var hasNeighbor = equality.Equals(kE, a.Key) || equality.Equals(kE, b.Key) || equality.Equals(kE, c.Key) ||
                      equality.Equals(kE, d.Key) || equality.Equals(kE, f.Key) ||
                      equality.Equals(kE, g.Key) || equality.Equals(kE, h.Key) || equality.Equals(kE, i.Key);

    if (hasNeighbor) {
      var kD = d.Key;
      var kB = b.Key;
      var kF = f.Key;
      var kH = h.Key;

      // Check if it's a solid block
      if (!(equality.Equals(kD, kB) && equality.Equals(kB, kF) && equality.Equals(kF, kH) && equality.Equals(kE, kD))) {
        // Not a solid block - apply pattern matching rules

        // Rule: Corner detection and reconstruction
        // Top-left corner (e0)
        if (equality.Equals(kD, kB) && equality.Equals(kD, kE) && !equality.Equals(kF, kE) && !equality.Equals(kH, kE))
          e0 = d.Work;
        else if (equality.Equals(kD, kB) && !equality.Equals(kD, kE))
          e0 = lerp.Lerp(d.Work, wE, 0.5f);

        // Top-right corner (e1)
        if (equality.Equals(kB, kF) && equality.Equals(kB, kE) && !equality.Equals(kD, kE) && !equality.Equals(kH, kE))
          e1 = b.Work;
        else if (equality.Equals(kB, kF) && !equality.Equals(kB, kE))
          e1 = lerp.Lerp(b.Work, wE, 0.5f);

        // Bottom-left corner (e2)
        if (equality.Equals(kD, kH) && equality.Equals(kD, kE) && !equality.Equals(kB, kE) && !equality.Equals(kF, kE))
          e2 = d.Work;
        else if (equality.Equals(kD, kH) && !equality.Equals(kD, kE))
          e2 = lerp.Lerp(d.Work, wE, 0.5f);

        // Bottom-right corner (e3)
        if (equality.Equals(kF, kH) && equality.Equals(kF, kE) && !equality.Equals(kB, kE) && !equality.Equals(kD, kE))
          e3 = f.Work;
        else if (equality.Equals(kF, kH) && !equality.Equals(kF, kE))
          e3 = lerp.Lerp(f.Work, wE, 0.5f);

        // Rule: Diagonal line reconstruction
        // Top-left to bottom-right diagonal
        if (equality.Equals(a.Key, kE) && equality.Equals(i.Key, kE) && !equality.Equals(kB, kE) && !equality.Equals(kH, kE)) {
          e0 = lerp.Lerp(a.Work, wE, 0.5f);
          e3 = lerp.Lerp(i.Work, wE, 0.5f);
        }

        // Top-right to bottom-left diagonal
        if (equality.Equals(c.Key, kE) && equality.Equals(g.Key, kE) && !equality.Equals(kB, kE) && !equality.Equals(kH, kE)) {
          e1 = lerp.Lerp(c.Work, wE, 0.5f);
          e2 = lerp.Lerp(g.Work, wE, 0.5f);
        }

        // Rule: 2:1 slope edges
        if (equality.Equals(kB, kE) && equality.Equals(kF, i.Key) && !equality.Equals(kF, kE)) {
          e1 = lerp.Lerp(b.Work, f.Work, 0.5f);
          e3 = lerp.Lerp(wE, f.Work, 0.5f);
        }

        if (equality.Equals(kD, kE) && equality.Equals(kH, g.Key) && !equality.Equals(kH, kE)) {
          e2 = lerp.Lerp(d.Work, h.Work, 0.5f);
          e3 = lerp.Lerp(wE, h.Work, 0.5f);
        }

        // Rule: Preserve sharp corners
        if (equality.Equals(a.Key, kD) && equality.Equals(a.Key, kB) && !equality.Equals(a.Key, kE))
          e0 = wE;

        if (equality.Equals(c.Key, kB) && equality.Equals(c.Key, kF) && !equality.Equals(c.Key, kE))
          e1 = wE;

        if (equality.Equals(g.Key, kD) && equality.Equals(g.Key, kH) && !equality.Equals(g.Key, kE))
          e2 = wE;

        if (equality.Equals(i.Key, kF) && equality.Equals(i.Key, kH) && !equality.Equals(i.Key, kE))
          e3 = wE;
      }
    }

    dest[0] = encoder.Encode(e0);
    dest[1] = encoder.Encode(e1);
    dest[destStride] = encoder.Encode(e2);
    dest[destStride + 1] = encoder.Encode(e3);
  }
}

#endregion
