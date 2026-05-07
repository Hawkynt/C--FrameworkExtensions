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
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Hawkynt.ColorProcessing.FrequencyDomain;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Filtering.Filters;

/// <summary>
/// Canonical Xu, Lu, Xu &amp; Jia 2011 L0-gradient smoothing — half-quadratic-splitting
/// solver with whole-image FFT-domain S-update step. Color-L0 norm couples R, G, B
/// channels through the auxiliary hard-shrink decision (paper eq. 3 / §3.2).
/// </summary>
/// <remarks>
/// <para>References:</para>
/// <para>Xu, L., Lu, C., Xu, Y., &amp; Jia, J. (2011). "Image Smoothing via L0 Gradient
/// Minimization", ACM Transactions on Graphics 30(6), Article 174 (SIGGRAPH Asia).</para>
/// <para>The HQS algorithm alternates between (a) per-pixel analytic hard-shrink on
/// auxiliary fields h, v and (b) a global least-squares S-update solved diagonally in
/// the frequency domain via 2D FFT. β doubles each outer iteration until β ≥ β_max.</para>
/// </remarks>
internal static class L0SmoothingFftHqs {

  /// <summary>
  /// Applies the canonical Xu 2011 FFT-HQS L0 smoothing to the given bitmap. The
  /// returned bitmap has the same dimensions; alpha is preserved unchanged. Source
  /// must be <see cref="PixelFormat.Format32bppArgb"/>.
  /// </summary>
  public static unsafe Bitmap Apply(Bitmap source, float lambda) {
    if (source is null)
      throw new ArgumentNullException(nameof(source));

    var w = source.Width;
    var h = source.Height;
    if (w < 4 || h < 4)
      return (Bitmap)source.Clone();

    // Pad to next power of two on each axis (FFT requirement); periodic boundary.
    var pw = _NextPow2(w);
    var ph = _NextPow2(h);

    // Read source: R, G, B as float[ph, pw] (clamp-to-edge for padded region);
    // alpha kept as byte at original w×h dimensions for unchanged passthrough.
    var sR = new Complex[ph, pw];
    var sG = new Complex[ph, pw];
    var sB = new Complex[ph, pw];
    var alpha = new byte[h * w];

    var srcRect = new Rectangle(0, 0, w, h);
    var srcData = source.LockBits(srcRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
    try {
      var p = (byte*)srcData.Scan0;
      var stride = srcData.Stride;
      for (var y = 0; y < ph; ++y) {
        var sy = y < h ? y : h - 1;
        for (var x = 0; x < pw; ++x) {
          var sx = x < w ? x : w - 1;
          var off = sy * stride + sx * 4;
          sB[y, x] = new(p[off + 0] * (1f / 255f), 0f);
          sG[y, x] = new(p[off + 1] * (1f / 255f), 0f);
          sR[y, x] = new(p[off + 2] * (1f / 255f), 0f);
          if (y < h && x < w)
            alpha[y * w + x] = p[off + 3];
        }
      }
    } finally {
      source.UnlockBits(srcData);
    }

    // F(I) — input image transformed once, kept for the S-update numerator.
    var fIR = (Complex[,])sR.Clone();
    var fIG = (Complex[,])sG.Clone();
    var fIB = (Complex[,])sB.Clone();
    Fft2D.Forward(fIR);
    Fft2D.Forward(fIG);
    Fft2D.Forward(fIB);

    // |F(Dx)(u,v)|² + |F(Dy)(u,v)|² = 4·sin²(πu/pw) + 4·sin²(πv/ph). Real-valued; precompute.
    var denomBase = new float[ph * pw];
    for (var v = 0; v < ph; ++v) {
      var sV = MathF.Sin(MathF.PI * v / ph);
      var sV2 = sV * sV;
      for (var u = 0; u < pw; ++u) {
        var sU = MathF.Sin(MathF.PI * u / pw);
        denomBase[v * pw + u] = 4f * (sU * sU + sV2);
      }
    }

    // β-doubling schedule per Xu 2011 §3.2.
    const float BetaMax = 1.0e5f;
    const float Kappa = 2f;
    var beta = 2f * lambda;
    if (beta <= 0f) beta = 1e-3f;

    var hR = new Complex[ph, pw]; var hG = new Complex[ph, pw]; var hB = new Complex[ph, pw];
    var vR = new Complex[ph, pw]; var vG = new Complex[ph, pw]; var vB = new Complex[ph, pw];

    while (beta < BetaMax) {
      // (a) Auxiliary update — color-L0 hard shrink on (h, v) per pixel.
      _AuxShrink(sR, sG, sB, hR, hG, hB, vR, vG, vB, ph, pw, lambda / beta);

      // FFT auxiliary fields.
      Fft2D.Forward(hR); Fft2D.Forward(hG); Fft2D.Forward(hB);
      Fft2D.Forward(vR); Fft2D.Forward(vG); Fft2D.Forward(vB);

      // (b) S-update in frequency domain:
      //   F(S) = (F(I) + β·(conj(F(Dx))·F(h) + conj(F(Dy))·F(v))) / (1 + β·denomBase)
      // Forward-difference operator FFT (standard DFT sign convention):
      //   F(Dx)(u,v) = exp(−2πi·u/pw) − 1, depends only on u;
      //   F(Dy)(u,v) = exp(−2πi·v/ph) − 1, depends only on v.
      _SUpdate(sR, sG, sB, fIR, fIG, fIB, hR, hG, hB, vR, vG, vB, denomBase, ph, pw, beta);

      // Bring S back to spatial domain for the next auxiliary step.
      Fft2D.Inverse(sR); Fft2D.Inverse(sG); Fft2D.Inverse(sB);

      beta *= Kappa;
    }

    // Write output. Crop to original w×h; restore unchanged alpha.
    var output = new Bitmap(w, h, PixelFormat.Format32bppArgb);
    var outRect = new Rectangle(0, 0, w, h);
    var outData = output.LockBits(outRect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
    try {
      var p = (byte*)outData.Scan0;
      var stride = outData.Stride;
      for (var y = 0; y < h; ++y) {
        for (var x = 0; x < w; ++x) {
          var off = y * stride + x * 4;
          p[off + 0] = _ToByte(sB[y, x].Real);
          p[off + 1] = _ToByte(sG[y, x].Real);
          p[off + 2] = _ToByte(sR[y, x].Real);
          p[off + 3] = alpha[y * w + x];
        }
      }
    } finally {
      output.UnlockBits(outData);
    }

    return output;
  }

  /// <summary>
  /// Per-pixel color-L0 hard-shrink: keep gradients (h=∂xS, v=∂yS) when the SUM of
  /// squared gradients across all 3 channels and both axes exceeds λ/β; else zero them.
  /// Uses cyclic differences (FFT periodic boundary).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _AuxShrink(
    Complex[,] sR, Complex[,] sG, Complex[,] sB,
    Complex[,] hR, Complex[,] hG, Complex[,] hB,
    Complex[,] vR, Complex[,] vG, Complex[,] vB,
    int ph, int pw, float thresh) {
    for (var y = 0; y < ph; ++y) {
      var ny = y + 1; if (ny == ph) ny = 0;
      for (var x = 0; x < pw; ++x) {
        var nx = x + 1; if (nx == pw) nx = 0;
        var dxR = sR[y, nx].Real - sR[y, x].Real;
        var dxG = sG[y, nx].Real - sG[y, x].Real;
        var dxB = sB[y, nx].Real - sB[y, x].Real;
        var dyR = sR[ny, x].Real - sR[y, x].Real;
        var dyG = sG[ny, x].Real - sG[y, x].Real;
        var dyB = sB[ny, x].Real - sB[y, x].Real;

        var grad2 = dxR * dxR + dxG * dxG + dxB * dxB
                  + dyR * dyR + dyG * dyG + dyB * dyB;

        if (grad2 > thresh) {
          hR[y, x] = new(dxR, 0f); hG[y, x] = new(dxG, 0f); hB[y, x] = new(dxB, 0f);
          vR[y, x] = new(dyR, 0f); vG[y, x] = new(dyG, 0f); vB[y, x] = new(dyB, 0f);
        } else {
          hR[y, x] = Complex.Zero; hG[y, x] = Complex.Zero; hB[y, x] = Complex.Zero;
          vR[y, x] = Complex.Zero; vG[y, x] = Complex.Zero; vB[y, x] = Complex.Zero;
        }
      }
    }
  }

  /// <summary>
  /// S-update in frequency domain. Writes the new F(S) into sR/sG/sB (overwrites their
  /// frequency-domain content; the caller will inverse-FFT afterwards).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _SUpdate(
    Complex[,] sR, Complex[,] sG, Complex[,] sB,
    Complex[,] fIR, Complex[,] fIG, Complex[,] fIB,
    Complex[,] hR, Complex[,] hG, Complex[,] hB,
    Complex[,] vR, Complex[,] vG, Complex[,] vB,
    float[] denomBase, int ph, int pw, float beta) {
    // Precompute conj(F(Dx))(u) and conj(F(Dy))(v) once per axis (frequency only depends
    // on the axis index; constant per row / column).
    // Under the standard DFT sign convention (Fft1D forward: exp(−2πi·u/N)),
    // F(Dx)(u) = exp(−2πi·u/pw) − 1, so conj(F(Dx))(u) = exp(+2πi·u/pw) − 1.
    var dxConjRe = new float[pw];
    var dxConjIm = new float[pw];
    for (var u = 0; u < pw; ++u) {
      var phase = 2f * MathF.PI * u / pw;
      dxConjRe[u] = MathF.Cos(phase) - 1f;
      dxConjIm[u] = MathF.Sin(phase);
    }
    var dyConjRe = new float[ph];
    var dyConjIm = new float[ph];
    for (var v = 0; v < ph; ++v) {
      var phase = 2f * MathF.PI * v / ph;
      dyConjRe[v] = MathF.Cos(phase) - 1f;
      dyConjIm[v] = MathF.Sin(phase);
    }

    for (var v = 0; v < ph; ++v) {
      var dyCR = dyConjRe[v];
      var dyCI = dyConjIm[v];
      for (var u = 0; u < pw; ++u) {
        var dxCR = dxConjRe[u];
        var dxCI = dxConjIm[u];
        var invDenom = 1f / (1f + beta * denomBase[v * pw + u]);

        // Per channel: F(S) = (F(I) + β·(conj(Dx)·F(h) + conj(Dy)·F(v))) · invDenom
        sR[v, u] = _UpdateOne(fIR[v, u], hR[v, u], vR[v, u], dxCR, dxCI, dyCR, dyCI, beta, invDenom);
        sG[v, u] = _UpdateOne(fIG[v, u], hG[v, u], vG[v, u], dxCR, dxCI, dyCR, dyCI, beta, invDenom);
        sB[v, u] = _UpdateOne(fIB[v, u], hB[v, u], vB[v, u], dxCR, dxCI, dyCR, dyCI, beta, invDenom);
      }
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Complex _UpdateOne(
    Complex fI, Complex fH, Complex fV,
    float dxCR, float dxCI, float dyCR, float dyCI,
    float beta, float invDenom) {
    // Complex multiply: (a + bi)(c + di) = (ac − bd) + (ad + bc)i.
    var hxRe = dxCR * fH.Real - dxCI * fH.Imaginary;
    var hxIm = dxCR * fH.Imaginary + dxCI * fH.Real;
    var hyRe = dyCR * fV.Real - dyCI * fV.Imaginary;
    var hyIm = dyCR * fV.Imaginary + dyCI * fV.Real;
    var sumRe = hxRe + hyRe;
    var sumIm = hxIm + hyIm;
    var numRe = (fI.Real + beta * sumRe) * invDenom;
    var numIm = (fI.Imaginary + beta * sumIm) * invDenom;
    return new(numRe, numIm);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte _ToByte(float x) {
    var v = x * 255f + 0.5f;
    if (v < 0f) return 0;
    if (v > 255f) return 255;
    return (byte)v;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int _NextPow2(int n) {
    if (n <= 1) return 1;
    var p = 1;
    while (p < n) p <<= 1;
    return p;
  }
}
