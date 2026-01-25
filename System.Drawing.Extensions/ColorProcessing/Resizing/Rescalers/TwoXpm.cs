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
using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Rescalers;

/// <summary>
/// 2xPM pixel-art scaling algorithm by Pablo Medina.
/// </summary>
/// <remarks>
/// <para>Reference: Pablo Medina (Kega Fusion plugin)</para>
/// <para>Algorithm: 3x3 neighborhood pattern matching with smooth edge interpolation.</para>
/// <para>High-quality pixel-perfect 2x scaling with edge morphing for clean diagonal edges.</para>
/// </remarks>
[ScalerInfo("2xPM", Author = "Pablo Medina",
  Description = "High-quality 2x pixel-art scaler with edge morphing", Category = ScalerCategory.PixelArt)]
public readonly struct TwoXpm : IPixelScaler {

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
    where TDistance : struct, IColorMetric<TKey>, INormalizedMetric
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new TwoXpmKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp));

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

  /// <summary>Gets the default 2xPM scaler.</summary>
  public static TwoXpm Default => new();
}

#region TwoXpm Kernel

/// <summary>
/// Kernel for 2xPM algorithm using 3x3 neighborhood pattern matching.
/// </summary>
/// <remarks>
/// 2xPM pattern (uses 3x3 neighborhood):
///
/// A B C      (top-left, top, top-right)
/// D E F      (left, center, right)
/// G H I      (bottom-left, bottom, bottom-right)
///
/// Output 2x2 block:
/// E0 E1
/// E2 E3
///
/// The algorithm detects horizontal, vertical, and diagonal edges
/// and applies smooth blending at edge transitions.
/// </remarks>
file readonly struct TwoXpmKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
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
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    // Get the 3x3 source neighborhood (row, column naming)
    var a = window.M1M1; // top-left
    var b = window.M1P0; // top
    var c = window.M1P1; // top-right
    var d = window.P0M1; // left
    var e = window.P0P0; // center
    var f = window.P0P1; // right
    var g = window.P1M1; // bottom-left
    var h = window.P1P0; // bottom
    var i = window.P1P1; // bottom-right

    var eWork = e.Work;

    // Default all outputs to center pixel
    var e0 = eWork;
    var e1 = eWork;
    var e2 = eWork;
    var e3 = eWork;

    // Pre-compute keys for equality tests
    var aKey = a.Key;
    var bKey = b.Key;
    var cKey = c.Key;
    var dKey = d.Key;
    var eKey = e.Key;
    var fKey = f.Key;
    var gKey = g.Key;
    var hKey = h.Key;
    var iKey = i.Key;

    var dontReblit = false;

    // Horizontal edge detection
    if (!equality.Equals(dKey, fKey)) {
      // Check for diagonal edge from left
      if (!equality.Equals(eKey, dKey) && equality.Equals(dKey, hKey) && equality.Equals(dKey, iKey) &&
          !equality.Equals(eKey, gKey) &&
          (!equality.Equals(dKey, gKey) || !equality.Equals(eKey, fKey) || !equality.Equals(aKey, dKey)) &&
          !(equality.Equals(dKey, aKey) && equality.Equals(dKey, gKey) && equality.Equals(eKey, bKey) && equality.Equals(eKey, fKey))) {
        e2 = h.Work;
        e3 = lerp.Lerp(eWork, h.Work, 500, 500);
        dontReblit = true;
      }
      // Check for diagonal edge from right
      else if (!equality.Equals(eKey, fKey) && equality.Equals(fKey, hKey) && equality.Equals(fKey, gKey) &&
               !equality.Equals(eKey, iKey) &&
               (!equality.Equals(fKey, iKey) || !equality.Equals(eKey, dKey) || !equality.Equals(cKey, fKey)) &&
               !(equality.Equals(fKey, cKey) && equality.Equals(fKey, iKey) && equality.Equals(eKey, bKey) && equality.Equals(eKey, dKey))) {
        e2 = lerp.Lerp(eWork, h.Work, 500, 500);
        e3 = h.Work;
        dontReblit = true;
      }
    }

    // Vertical edge detection
    if (!equality.Equals(bKey, hKey)) {
      if (!equality.Equals(eKey, bKey)) {
        if (!equality.Equals(aKey, bKey) || !equality.Equals(bKey, cKey) || !equality.Equals(eKey, hKey)) {
          // Top-left diagonal
          if (equality.Equals(bKey, dKey) && equality.Equals(bKey, gKey) && !equality.Equals(eKey, aKey) &&
              !(equality.Equals(dKey, aKey) && equality.Equals(dKey, cKey) && equality.Equals(eKey, hKey) && equality.Equals(eKey, fKey))) {
            e0 = lerp.Lerp(eWork, b.Work, 250, 750);
            e2 = lerp.Lerp(eWork, b.Work, 750, 250);
            dontReblit = true;
          }
          // Top-right diagonal
          else if (equality.Equals(bKey, fKey) && equality.Equals(bKey, iKey) && !equality.Equals(eKey, cKey) &&
                   !(equality.Equals(fKey, cKey) && equality.Equals(fKey, aKey) && equality.Equals(eKey, hKey) && equality.Equals(eKey, dKey))) {
            e1 = lerp.Lerp(eWork, b.Work, 250, 750);
            e3 = lerp.Lerp(eWork, b.Work, 750, 250);
            dontReblit = true;
          }
        }
      }

      if (!equality.Equals(eKey, hKey)) {
        if (!equality.Equals(gKey, hKey) || !equality.Equals(eKey, bKey) || !equality.Equals(hKey, iKey)) {
          // Bottom-left diagonal
          if (equality.Equals(hKey, dKey) && equality.Equals(hKey, aKey) && !equality.Equals(eKey, gKey) &&
              !(equality.Equals(dKey, gKey) && equality.Equals(dKey, iKey) && equality.Equals(eKey, bKey) && equality.Equals(eKey, fKey))) {
            e2 = lerp.Lerp(eWork, h.Work, 250, 750);
            e0 = lerp.Lerp(eWork, h.Work, 750, 250);
            dontReblit = true;
          }
          // Bottom-right diagonal
          else if (equality.Equals(hKey, fKey) && equality.Equals(hKey, cKey) && !equality.Equals(eKey, iKey) &&
                   !(equality.Equals(fKey, iKey) && equality.Equals(fKey, gKey) && equality.Equals(eKey, bKey) && equality.Equals(eKey, dKey))) {
            e3 = lerp.Lerp(eWork, h.Work, 250, 750);
            e1 = lerp.Lerp(eWork, h.Work, 750, 250);
            dontReblit = true;
          }
        }
      }
    }

    // Diagonal pattern detection (fallback if no edge detected)
    if (!dontReblit) {
      if (!equality.Equals(bKey, hKey) && !equality.Equals(dKey, fKey)) {
        // Top-left corner blend
        if (equality.Equals(bKey, dKey) && !equality.Equals(eKey, dKey) &&
            !(equality.Equals(eKey, aKey) && equality.Equals(bKey, cKey) && equality.Equals(eKey, fKey)) &&
            !(equality.Equals(bKey, aKey) && equality.Equals(bKey, gKey)) &&
            !(equality.Equals(dKey, aKey) && equality.Equals(dKey, cKey) && equality.Equals(eKey, fKey) && !equality.Equals(gKey, dKey) && !equality.Equals(gKey, eKey)))
          e0 = lerp.Lerp(eWork, b.Work, 500, 500);

        // Top-right corner blend
        if (equality.Equals(bKey, fKey) && !equality.Equals(eKey, fKey) &&
            !(equality.Equals(eKey, cKey) && equality.Equals(bKey, aKey) && equality.Equals(eKey, dKey)) &&
            !(equality.Equals(bKey, cKey) && equality.Equals(bKey, iKey)) &&
            !(equality.Equals(fKey, aKey) && equality.Equals(fKey, cKey) && equality.Equals(eKey, dKey) && !equality.Equals(iKey, fKey) && !equality.Equals(iKey, eKey)))
          e1 = lerp.Lerp(eWork, b.Work, 500, 500);

        // Bottom-left corner blend
        if (equality.Equals(hKey, dKey) && (!equality.Equals(eKey, gKey) || !equality.Equals(eKey, dKey)) &&
            !(equality.Equals(eKey, gKey) && equality.Equals(hKey, iKey) && equality.Equals(eKey, fKey)) &&
            !(equality.Equals(hKey, gKey) && equality.Equals(hKey, aKey)) &&
            !(equality.Equals(dKey, gKey) && equality.Equals(dKey, iKey) && equality.Equals(eKey, fKey) && !equality.Equals(aKey, dKey) && !equality.Equals(aKey, eKey)))
          e2 = lerp.Lerp(eWork, h.Work, 500, 500);

        // Bottom-right corner blend
        if (equality.Equals(hKey, fKey) && (!equality.Equals(eKey, iKey) || !equality.Equals(eKey, fKey)) &&
            !(equality.Equals(eKey, iKey) && equality.Equals(hKey, gKey) && equality.Equals(eKey, dKey)) &&
            !(equality.Equals(hKey, iKey) && equality.Equals(hKey, cKey)) &&
            !(equality.Equals(fKey, gKey) && equality.Equals(fKey, iKey) && equality.Equals(eKey, dKey) && !equality.Equals(cKey, fKey) && !equality.Equals(iKey, eKey)))
          e3 = lerp.Lerp(eWork, h.Work, 500, 500);
      }
      // Special pattern: D matches B, F, H but not E
      else if (equality.Equals(dKey, bKey) && equality.Equals(dKey, fKey) && equality.Equals(dKey, hKey) && !equality.Equals(dKey, eKey)) {
        if (equality.Equals(dKey, gKey) || equality.Equals(dKey, cKey)) {
          e1 = lerp.Lerp(eWork, d.Work, 500, 500);
          e2 = e1;
        }

        if (equality.Equals(dKey, aKey) || equality.Equals(dKey, iKey)) {
          e0 = lerp.Lerp(eWork, d.Work, 500, 500);
          e3 = e0;
        }
      }
    }

    // Write output
    var row0 = dest;
    var row1 = dest + destStride;
    row0[0] = encoder.Encode(e0);
    row0[1] = encoder.Encode(e1);
    row1[0] = encoder.Encode(e2);
    row1[1] = encoder.Encode(e3);
  }
}

#endregion
