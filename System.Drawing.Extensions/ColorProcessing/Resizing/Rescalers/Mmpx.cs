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
    where TDistance : struct, IColorMetric<TKey>, INormalizedMetric
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

  /// <summary>
  /// Computes luma for tie-breaking in MMPX algorithm.
  /// Uses standard luminance calculation.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float Luma(in TWork color) => ColorConverter.GetLuminance(color);

  /// <summary>Returns true if any of the three values equals the reference.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private bool AnyEq3(in TKey reference, in TKey a, in TKey b, in TKey c)
    => equality.Equals(reference, a) || equality.Equals(reference, b) || equality.Equals(reference, c);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    // Reference MMPX uses 5x5 neighborhood with this naming:
    //       P
    //   Q A B C R
    //     D E F
    //     G H I
    //       S
    // NeighborWindow: M2P0=row-2,col0, P0M2=row0,col-2, etc.

    // Extended neighbors (5x5)
    var p = window.M2P0; // 2 above center
    var q = window.P0M2; // 2 left of center
    var r = window.P0P2; // 2 right of center
    var s = window.P2P0; // 2 below center

    // Core 3x3 neighborhood
    var a = window.M1M1; // top-left
    var b = window.M1P0; // top
    var c = window.M1P1; // top-right
    var d = window.P0M1; // left
    var e = window.P0P0; // center
    var f = window.P0P1; // right
    var g = window.P1M1; // bottom-left
    var h = window.P1P0; // bottom
    var i = window.P1P1; // bottom-right

    var wE = e.Work;
    var kP = p.Key;
    var kQ = q.Key;
    var kR = r.Key;
    var kS = s.Key;
    var kA = a.Key;
    var kB = b.Key;
    var kC = c.Key;
    var kD = d.Key;
    var kE = e.Key;
    var kF = f.Key;
    var kG = g.Key;
    var kH = h.Key;
    var kI = i.Key;

    // Luma values for tie-breaking (use Work colors for luminance)
    var lE = Luma(e.Work);
    var lD = Luma(d.Work);
    var lB = Luma(b.Work);
    var lF = Luma(f.Work);
    var lH = Luma(h.Work);

    // Output 2x2 block: J K / L M
    // Default to center pixel
    var j = wE;
    var k = wE;
    var l = wE;
    var m = wE;

    // 1:1 Slope Rules with full reference conditions
    // J = D when: (D==B && D!=H && D!=F) && (lE>=lD || E==A) && any_eq3(E,A,C,G) && (lE<lD || A!=D || E!=P || E!=Q)
    if (equality.Equals(kD, kB) && !equality.Equals(kD, kH) && !equality.Equals(kD, kF))
      if ((lE >= lD || equality.Equals(kE, kA)) && AnyEq3(kE, kA, kC, kG))
        if (lE < lD || !equality.Equals(kA, kD) || !equality.Equals(kE, kP) || !equality.Equals(kE, kQ))
          j = lerp.Lerp(d.Work, b.Work);

    // K = B when: (B==F && B!=D && B!=H) && (lE>=lB || E==C) && any_eq3(E,A,C,I) && (lE<lB || C!=B || E!=P || E!=R)
    if (equality.Equals(kB, kF) && !equality.Equals(kB, kD) && !equality.Equals(kB, kH))
      if ((lE >= lB || equality.Equals(kE, kC)) && AnyEq3(kE, kA, kC, kI))
        if (lE < lB || !equality.Equals(kC, kB) || !equality.Equals(kE, kP) || !equality.Equals(kE, kR))
          k = lerp.Lerp(b.Work, f.Work);

    // L = H when: (H==D && H!=F && H!=B) && (lE>=lH || E==G) && any_eq3(E,A,G,I) && (lE<lH || G!=H || E!=S || E!=Q)
    if (equality.Equals(kH, kD) && !equality.Equals(kH, kF) && !equality.Equals(kH, kB))
      if ((lE >= lH || equality.Equals(kE, kG)) && AnyEq3(kE, kA, kG, kI))
        if (lE < lH || !equality.Equals(kG, kH) || !equality.Equals(kE, kS) || !equality.Equals(kE, kQ))
          l = lerp.Lerp(d.Work, h.Work);

    // M = F when: (F==H && F!=B && F!=D) && (lE>=lF || E==I) && any_eq3(E,C,G,I) && (lE<lF || I!=H || E!=R || E!=S)
    if (equality.Equals(kF, kH) && !equality.Equals(kF, kB) && !equality.Equals(kF, kD))
      if ((lE >= lF || equality.Equals(kE, kI)) && AnyEq3(kE, kC, kG, kI))
        if (lE < lF || !equality.Equals(kI, kH) || !equality.Equals(kE, kR) || !equality.Equals(kE, kS))
          m = lerp.Lerp(f.Work, h.Work);

    // Intersection rules - when E differs from edge and forms a diagonal pattern
    // E != F && all_eq4(E,C,I,D,Q) && all_eq2(F,B,H) → K=M=F
    if (!equality.Equals(kE, kF) && equality.Equals(kE, kC) && equality.Equals(kE, kI) && equality.Equals(kE, kD) && equality.Equals(kE, kQ)
        && equality.Equals(kF, kB) && equality.Equals(kF, kH)) {
      k = lerp.Lerp(f.Work, wE);
      m = lerp.Lerp(f.Work, wE);
    }

    // E != D && all_eq4(E,A,G,F,R) && all_eq2(D,B,H) → J=L=D
    if (!equality.Equals(kE, kD) && equality.Equals(kE, kA) && equality.Equals(kE, kG) && equality.Equals(kE, kF) && equality.Equals(kE, kR)
        && equality.Equals(kD, kB) && equality.Equals(kD, kH)) {
      j = lerp.Lerp(d.Work, wE);
      l = lerp.Lerp(d.Work, wE);
    }

    // E != B && all_eq4(E,A,C,H,S) && all_eq2(B,D,F) → J=K=B
    if (!equality.Equals(kE, kB) && equality.Equals(kE, kA) && equality.Equals(kE, kC) && equality.Equals(kE, kH) && equality.Equals(kE, kS)
        && equality.Equals(kB, kD) && equality.Equals(kB, kF)) {
      j = lerp.Lerp(b.Work, wE);
      k = lerp.Lerp(b.Work, wE);
    }

    // E != H && all_eq4(E,G,I,B,P) && all_eq2(H,D,F) → L=M=H
    if (!equality.Equals(kE, kH) && equality.Equals(kE, kG) && equality.Equals(kE, kI) && equality.Equals(kE, kB) && equality.Equals(kE, kP)
        && equality.Equals(kH, kD) && equality.Equals(kH, kF)) {
      l = lerp.Lerp(h.Work, wE);
      m = lerp.Lerp(h.Work, wE);
    }

    // Lower-luma rules - preserve darker features
    if (lB < lE && equality.Equals(kE, kG) && equality.Equals(kE, kH) && equality.Equals(kE, kI) && equality.Equals(kE, kS) && !equality.Equals(kE, kA) && !equality.Equals(kE, kD) && !equality.Equals(kE, kC) && !equality.Equals(kE, kF)) {
      j = lerp.Lerp(b.Work, wE);
      k = lerp.Lerp(b.Work, wE);
    }

    if (lH < lE && equality.Equals(kE, kA) && equality.Equals(kE, kB) && equality.Equals(kE, kC) && equality.Equals(kE, kP) && !equality.Equals(kE, kG) && !equality.Equals(kE, kD) && !equality.Equals(kE, kI) && !equality.Equals(kE, kF)) {
      l = lerp.Lerp(h.Work, wE);
      m = lerp.Lerp(h.Work, wE);
    }

    if (lD < lE && equality.Equals(kE, kC) && equality.Equals(kE, kF) && equality.Equals(kE, kI) && equality.Equals(kE, kR) && !equality.Equals(kE, kA) && !equality.Equals(kE, kB) && !equality.Equals(kE, kG) && !equality.Equals(kE, kH)) {
      j = lerp.Lerp(d.Work, wE);
      l = lerp.Lerp(d.Work, wE);
    }

    if (lF < lE && equality.Equals(kE, kA) && equality.Equals(kE, kD) && equality.Equals(kE, kG) && equality.Equals(kE, kQ) && !equality.Equals(kE, kC) && !equality.Equals(kE, kB) && !equality.Equals(kE, kI) && !equality.Equals(kE, kH)) {
      k = lerp.Lerp(f.Work, wE);
      m = lerp.Lerp(f.Work, wE);
    }

    // 2:1 slope rules
    if (!equality.Equals(kH, kB)) {
      if (equality.Equals(kE, kB) && equality.Equals(kF, kI) && !equality.Equals(kF, kE) && (!equality.Equals(kE, kA) || !equality.Equals(kE, kD) || equality.Equals(kF, kC))) {
        k = lerp.Lerp(b.Work, f.Work);
        m = lerp.Lerp(wE, f.Work);
      }

      if (equality.Equals(kE, kB) && equality.Equals(kD, kA) && !equality.Equals(kD, kE) && (!equality.Equals(kE, kC) || !equality.Equals(kE, kF) || equality.Equals(kD, kG))) {
        j = lerp.Lerp(b.Work, d.Work);
        l = lerp.Lerp(wE, d.Work);
      }

      if (equality.Equals(kE, kH) && equality.Equals(kF, kC) && !equality.Equals(kF, kE) && (!equality.Equals(kE, kG) || !equality.Equals(kE, kD) || equality.Equals(kF, kI))) {
        k = lerp.Lerp(wE, f.Work);
        m = lerp.Lerp(h.Work, f.Work);
      }

      if (equality.Equals(kE, kH) && equality.Equals(kD, kG) && !equality.Equals(kD, kE) && (!equality.Equals(kE, kI) || !equality.Equals(kE, kF) || equality.Equals(kD, kA))) {
        j = lerp.Lerp(wE, d.Work);
        l = lerp.Lerp(h.Work, d.Work);
      }
    }

    if (!equality.Equals(kD, kF)) {
      if (equality.Equals(kE, kD) && equality.Equals(kH, kG) && !equality.Equals(kH, kE) && (!equality.Equals(kE, kC) || !equality.Equals(kE, kB) || equality.Equals(kH, kI))) {
        l = lerp.Lerp(d.Work, h.Work);
        m = lerp.Lerp(wE, h.Work);
      }

      if (equality.Equals(kE, kD) && equality.Equals(kB, kA) && !equality.Equals(kB, kE) && (!equality.Equals(kE, kI) || !equality.Equals(kE, kH) || equality.Equals(kB, kC))) {
        j = lerp.Lerp(d.Work, b.Work);
        k = lerp.Lerp(wE, b.Work);
      }

      if (equality.Equals(kE, kF) && equality.Equals(kH, kI) && !equality.Equals(kH, kE) && (!equality.Equals(kE, kA) || !equality.Equals(kE, kB) || equality.Equals(kH, kG))) {
        l = lerp.Lerp(wE, h.Work);
        m = lerp.Lerp(f.Work, h.Work);
      }

      if (equality.Equals(kE, kF) && equality.Equals(kB, kC) && !equality.Equals(kB, kE) && (!equality.Equals(kE, kG) || !equality.Equals(kE, kH) || equality.Equals(kB, kA))) {
        j = lerp.Lerp(wE, b.Work);
        k = lerp.Lerp(f.Work, b.Work);
      }
    }

    dest[0] = encoder.Encode(j);
    dest[1] = encoder.Encode(k);
    dest[destStride] = encoder.Encode(l);
    dest[destStride + 1] = encoder.Encode(m);
  }
}

#endregion
