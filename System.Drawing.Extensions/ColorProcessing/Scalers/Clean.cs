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

#region Clean

/// <summary>
/// Clean - edge-aware pixel art scaling optimized for rotation (2x, 4x).
/// </summary>
/// <remarks>
/// <para>Uses edge detection and line width calculations to produce clean edges.</para>
/// <para>Examines quadrants and edge directions to fill the output block.</para>
/// <para>Produces results that look better when the image is rotated.</para>
/// <para>Reference: https://torcado.com/cleanEdge/</para>
/// </remarks>
[ScalerInfo("Clean", Author = "torcado", Year = 2022,
  Description = "Clean edge-aware pixel art scaling", Category = ScalerCategory.PixelArt,
  Url = "https://torcado.com/cleanEdge/")]
public readonly struct Clean : IPixelScaler {
  private readonly int _scale;

  public Clean(int scale = 2) {
    if (scale is not (2 or 4))
      throw new ArgumentOutOfRangeException(nameof(scale), scale, "Clean supports 2x, 4x scaling");
    this._scale = scale;
  }

  public ScaleFactor Scale => new(this._scale, this._scale);

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
    => this._scale switch {
      2 => callback.Invoke(new Clean2xKernel<TWork, TKey, TPixel, TEquality, TEncode>(equality)),
      4 => callback.Invoke(new Clean4xKernel<TWork, TKey, TPixel, TEquality, TEncode>(equality)),
      _ => throw new InvalidOperationException($"Invalid scale factor: {this._scale}")
    };

  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2), new(4, 4)];
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 } or { X: 4, Y: 4 };

  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
    yield return (sourceWidth * 4, sourceHeight * 4);
  }

  public static Clean Scale2x => new(2);
  public static Clean Scale4x => new(4);
  public static Clean Default => Scale2x;
}

#endregion

#region Clean Edge Helpers

/// <summary>
/// Helper methods and constants for Clean edge algorithm.
/// </summary>
file static class CleanHelpers {
  /// <summary>
  /// Line width constant: sqrt(2)/2, aligns slopes along diagonals.
  /// </summary>
  public const float LineWidth = 0.707f;

  /// <summary>
  /// Processes a quadrant to determine edge-aware color.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int ProcessQuadrant<TKey, TEquality>(
    in TEquality equality,
    in TKey center, in TKey edge1, in TKey edge2, in TKey corner,
    float distX, float distY)
    where TKey : unmanaged, IColorSpace
    where TEquality : struct, IColorEquality<TKey> {

    // Check similarity conditions
    var similarEdge1 = equality.Equals(center, edge1);
    var similarEdge2 = equality.Equals(center, edge2);
    var similarCorner = equality.Equals(center, corner);

    // Calculate distance from center for line detection
    var dist = MathF.Sqrt(distX * distX + distY * distY);

    // 45-degree diagonal: if both edges differ from center but are similar to each other and corner
    if (!similarEdge1 && !similarEdge2 && equality.Equals(edge1, edge2) && equality.Equals(edge1, corner))
      if (dist > LineWidth * 0.5f)
        return 1; // Return edge1

    // Horizontal edge dominates
    if (!similarEdge1 && similarEdge2)
      if (distY > LineWidth)
        return 1; // Return edge1

    // Vertical edge dominates
    if (similarEdge1 && !similarEdge2)
      if (distX > LineWidth)
        return 2; // Return edge2

    // Corner case: both edges are different
    if (!similarEdge1 && !similarEdge2) {
      // Calculate which edge is closer based on position
      if (distX > distY && distY > LineWidth)
        return 1; // Return edge1
      if (distY > distX && distX > LineWidth)
        return 2; // Return edge2
      // Diagonal corner
      if (similarCorner && dist > LineWidth * 0.707f)
        return 3; // Return corner
    }

    return 0; // Return center
  }
}

#endregion

#region Clean 2x Kernel

file readonly struct Clean2xKernel<TWork, TKey, TPixel, TEquality, TEncode>(TEquality equality = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 2;
  public int ScaleY => 2;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    // Get 3x3 neighborhood
    var c = window.P0P0;   // center
    var u = window.P0M1;   // up
    var d = window.P0P1;   // down
    var l = window.M1P0;   // left
    var r = window.P1P0;   // right
    var ul = window.M1M1;  // up-left
    var ur = window.P1M1;  // up-right
    var dl = window.M1P1;  // down-left
    var dr = window.P1P1;  // down-right

    var wC = c.Work;
    var kC = c.Key;

    // For 2x scale, we have 4 sub-pixels at positions:
    // (0,0) -> px=-0.25, py=-0.25 -> top-left quadrant
    // (1,0) -> px=+0.25, py=-0.25 -> top-right quadrant
    // (0,1) -> px=-0.25, py=+0.25 -> bottom-left quadrant
    // (1,1) -> px=+0.25, py=+0.25 -> bottom-right quadrant

    // Top-left (quadrant TL): distX=0.25, distY=0.25
    var choice00 = CleanHelpers.ProcessQuadrant(equality, kC, u.Key, l.Key, ul.Key, 0.25f, 0.25f);
    var w00 = choice00 switch {
      1 => u.Work,
      2 => l.Work,
      3 => ul.Work,
      _ => wC
    };

    // Top-right (quadrant TR): distX=0.25, distY=0.25
    var choice01 = CleanHelpers.ProcessQuadrant(equality, kC, u.Key, r.Key, ur.Key, 0.25f, 0.25f);
    var w01 = choice01 switch {
      1 => u.Work,
      2 => r.Work,
      3 => ur.Work,
      _ => wC
    };

    // Bottom-left (quadrant BL): distX=0.25, distY=0.25
    var choice10 = CleanHelpers.ProcessQuadrant(equality, kC, d.Key, l.Key, dl.Key, 0.25f, 0.25f);
    var w10 = choice10 switch {
      1 => d.Work,
      2 => l.Work,
      3 => dl.Work,
      _ => wC
    };

    // Bottom-right (quadrant BR): distX=0.25, distY=0.25
    var choice11 = CleanHelpers.ProcessQuadrant(equality, kC, d.Key, r.Key, dr.Key, 0.25f, 0.25f);
    var w11 = choice11 switch {
      1 => d.Work,
      2 => r.Work,
      3 => dr.Work,
      _ => wC
    };

    dest[0] = encoder.Encode(w00);
    dest[1] = encoder.Encode(w01);
    dest[destStride] = encoder.Encode(w10);
    dest[destStride + 1] = encoder.Encode(w11);
  }
}

#endregion

#region Clean 4x Kernel

file readonly struct Clean4xKernel<TWork, TKey, TPixel, TEquality, TEncode>(TEquality equality = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 4;
  public int ScaleY => 4;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    // Get 3x3 neighborhood
    var c = window.P0P0;   // center
    var u = window.P0M1;   // up
    var d = window.P0P1;   // down
    var l = window.M1P0;   // left
    var r = window.P1P0;   // right
    var ul = window.M1M1;  // up-left
    var ur = window.P1M1;  // up-right
    var dl = window.M1P1;  // down-left
    var dr = window.P1P1;  // down-right

    var wC = c.Work;
    var kC = c.Key;
    var pC = encoder.Encode(wC);

    // Pre-encode neighbor colors for reuse
    var pU = encoder.Encode(u.Work);
    var pD = encoder.Encode(d.Work);
    var pL = encoder.Encode(l.Work);
    var pR = encoder.Encode(r.Work);
    var pUL = encoder.Encode(ul.Work);
    var pUR = encoder.Encode(ur.Work);
    var pDL = encoder.Encode(dl.Work);
    var pDR = encoder.Encode(dr.Work);

    // For 4x scale, iterate through 16 sub-pixels
    // Position formula: px = (sx + 0.5) / 4 - 0.5
    // sx=0: px=-0.375, sx=1: px=-0.125, sx=2: px=0.125, sx=3: px=0.375

    for (var sy = 0; sy < 4; ++sy) {
      var py = (sy + 0.5f) / 4f - 0.5f;
      var destRow = dest + sy * destStride;

      for (var sx = 0; sx < 4; ++sx) {
        var px = (sx + 0.5f) / 4f - 0.5f;

        TPixel result;

        if (px < 0 && py < 0) {
          // Top-left quadrant
          var choice = CleanHelpers.ProcessQuadrant(equality, kC, u.Key, l.Key, ul.Key, -px, -py);
          result = choice switch {
            1 => pU,
            2 => pL,
            3 => pUL,
            _ => pC
          };
        } else if (px >= 0 && py < 0) {
          // Top-right quadrant
          var choice = CleanHelpers.ProcessQuadrant(equality, kC, u.Key, r.Key, ur.Key, px, -py);
          result = choice switch {
            1 => pU,
            2 => pR,
            3 => pUR,
            _ => pC
          };
        } else if (px < 0 && py >= 0) {
          // Bottom-left quadrant
          var choice = CleanHelpers.ProcessQuadrant(equality, kC, d.Key, l.Key, dl.Key, -px, py);
          result = choice switch {
            1 => pD,
            2 => pL,
            3 => pDL,
            _ => pC
          };
        } else {
          // Bottom-right quadrant
          var choice = CleanHelpers.ProcessQuadrant(equality, kC, d.Key, r.Key, dr.Key, px, py);
          result = choice switch {
            1 => pD,
            2 => pR,
            3 => pDR,
            _ => pC
          };
        }

        destRow[sx] = result;
      }
    }
  }
}

#endregion
