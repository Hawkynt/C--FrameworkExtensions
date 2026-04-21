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

namespace Hawkynt.Drawing.ColorDomain;

/// <summary>
/// Selectable runtime color-distance metric for tools that pick by name (CLI, config files).
/// Each value can be turned into a <see cref="Func{Color, Color, Int32}"/> via
/// <see cref="ColorMetricExtensions.AsFunc"/>, or invoked directly with
/// <see cref="ColorMetricExtensions.Calculate"/>.
/// </summary>
/// <remarks>
/// All metrics return non-negative integer distances (squared distance for Euclidean
/// variants). The exact numeric scale differs between metrics, so don't compare absolute
/// values across metrics — use them only to compare distances *within* one metric.
/// </remarks>
public enum ColorMetric {
  /// <summary>Squared Euclidean distance over R,G,B,A channels.</summary>
  Euclidean = 0,
  /// <summary>Squared Euclidean distance over R,G,B only (alpha ignored).</summary>
  EuclideanRgbOnly,
  /// <summary>Squared Euclidean distance with BT.709 luma weights (R,G,B = 2126,7152,722).</summary>
  EuclideanBT709,
  /// <summary>Squared Euclidean distance with Nommyde-tuned weights (R,G,B = 4984,8625,2979).</summary>
  EuclideanNommyde,
  /// <summary>Weighted squared Euclidean favoring green/blue over red (R,G,B = 2,4,3).</summary>
  WeightedEuclideanLowRed,
  /// <summary>Weighted squared Euclidean favoring red/green over blue (R,G,B = 3,4,2).</summary>
  WeightedEuclideanHighRed,
  /// <summary>Manhattan (L1) distance over R,G,B,A channels.</summary>
  Manhattan,
  /// <summary>Manhattan distance over R,G,B only (alpha ignored).</summary>
  ManhattanRgbOnly,
  /// <summary>Manhattan distance with BT.709 luma weights.</summary>
  ManhattanBT709,
  /// <summary>Manhattan distance with Nommyde-tuned weights.</summary>
  ManhattanNommyde,
  /// <summary>Manhattan distance favoring green/blue over red.</summary>
  WeightedManhattanLowRed,
  /// <summary>Manhattan distance favoring red/green over blue.</summary>
  WeightedManhattanHighRed,
  /// <summary>CompuPhase low-cost color metric (https://www.compuphase.com/cmetric.htm).</summary>
  CompuPhase,
  /// <summary>PNGQuant alpha-aware difference (white-point weighted).</summary>
  PngQuant,
  /// <summary>Weighted distance in YUV space (luminance preferred).</summary>
  WeightedYuv,
  /// <summary>Weighted distance in YCbCr space.</summary>
  WeightedYCbCr,
  /// <summary>CIE94 with textile-industry coefficients (kL=2, k1=0.048, k2=0.014).</summary>
  Cie94Textiles,
  /// <summary>CIE94 with graphic-arts coefficients (kL=1, k1=0.045, k2=0.015).</summary>
  Cie94GraphicArts,
  /// <summary>CIEDE2000 perceptual color distance.</summary>
  CieDe2000,
}

/// <summary>
/// Bridge between <see cref="ColorMetric"/> and <see cref="Func{Color, Color, Int32}"/>.
/// Each metric is a self-contained integer-arithmetic implementation that operates
/// directly on <see cref="Color"/> components — no upstream typed-metric round-trip
/// required (avoids the conversion overhead).
/// </summary>
public static class ColorMetricExtensions {

  // BT.709 luma coefficients × 10000.
  private const int BT709_R = 2126, BT709_G = 7152, BT709_B = 722, BT709_A = 10000, BT709_DIV = 10000;
  // Nommyde-tuned weights × 10000.
  private const int NOMMYDE_R = 4984, NOMMYDE_G = 8625, NOMMYDE_B = 2979, NOMMYDE_A = 10000, NOMMYDE_DIV = 10000;
  // Low-red weights (favor green/blue).
  private const int LOWRED_R = 2, LOWRED_G = 4, LOWRED_B = 3, LOWRED_A = 1;
  // High-red weights (favor red/green).
  private const int HIGHRED_R = 3, HIGHRED_G = 4, HIGHRED_B = 2, HIGHRED_A = 1;

  /// <summary>Returns a delegate that computes the chosen metric.</summary>
  public static Func<Color, Color, int> AsFunc(this ColorMetric metric) => metric switch {
    ColorMetric.Euclidean => _Euclidean,
    ColorMetric.EuclideanRgbOnly => _EuclideanRgbOnly,
    ColorMetric.EuclideanBT709 => (a, b) => _WeightedEuclidean(a, b, BT709_R, BT709_G, BT709_B, BT709_A, BT709_DIV),
    ColorMetric.EuclideanNommyde => (a, b) => _WeightedEuclidean(a, b, NOMMYDE_R, NOMMYDE_G, NOMMYDE_B, NOMMYDE_A, NOMMYDE_DIV),
    ColorMetric.WeightedEuclideanLowRed => (a, b) => _WeightedEuclidean(a, b, LOWRED_R, LOWRED_G, LOWRED_B, LOWRED_A, 1),
    ColorMetric.WeightedEuclideanHighRed => (a, b) => _WeightedEuclidean(a, b, HIGHRED_R, HIGHRED_G, HIGHRED_B, HIGHRED_A, 1),
    ColorMetric.Manhattan => _Manhattan,
    ColorMetric.ManhattanRgbOnly => _ManhattanRgbOnly,
    ColorMetric.ManhattanBT709 => (a, b) => _WeightedManhattan(a, b, BT709_R, BT709_G, BT709_B, BT709_A, BT709_DIV),
    ColorMetric.ManhattanNommyde => (a, b) => _WeightedManhattan(a, b, NOMMYDE_R, NOMMYDE_G, NOMMYDE_B, NOMMYDE_A, NOMMYDE_DIV),
    ColorMetric.WeightedManhattanLowRed => (a, b) => _WeightedManhattan(a, b, LOWRED_R, LOWRED_G, LOWRED_B, LOWRED_A, 1),
    ColorMetric.WeightedManhattanHighRed => (a, b) => _WeightedManhattan(a, b, HIGHRED_R, HIGHRED_G, HIGHRED_B, HIGHRED_A, 1),
    ColorMetric.CompuPhase => _CompuPhase,
    ColorMetric.PngQuant => _PngQuant,
    ColorMetric.WeightedYuv => _WeightedYuv,
    ColorMetric.WeightedYCbCr => _WeightedYCbCr,
    ColorMetric.Cie94Textiles => (a, b) => _Cie94(a, b, kL: 2.0, k1: 0.048, k2: 0.014),
    ColorMetric.Cie94GraphicArts => (a, b) => _Cie94(a, b, kL: 1.0, k1: 0.045, k2: 0.015),
    ColorMetric.CieDe2000 => _CieDe2000,
    _ => throw new ArgumentOutOfRangeException(nameof(metric), metric, "Unknown ColorMetric value")
  };

  /// <summary>Convenience wrapper around <see cref="AsFunc"/> for a single calculation.</summary>
  public static int Calculate(this ColorMetric metric, Color a, Color b) => metric.AsFunc()(a, b);

  #region channel-arithmetic implementations

  private static int _Euclidean(Color self, Color other) {
    var dr = self.R - other.R;
    var dg = self.G - other.G;
    var db = self.B - other.B;
    var da = self.A - other.A;
    return dr * dr + dg * dg + db * db + da * da;
  }

  private static int _EuclideanRgbOnly(Color self, Color other) {
    var dr = self.R - other.R;
    var dg = self.G - other.G;
    var db = self.B - other.B;
    return dr * dr + dg * dg + db * db;
  }

  private static int _WeightedEuclidean(Color self, Color other, int wr, int wg, int wb, int wa, int divisor) {
    var dr = self.R - other.R;
    var dg = self.G - other.G;
    var db = self.B - other.B;
    var da = self.A - other.A;
    return (wr * dr * dr + wg * dg * dg + wb * db * db + wa * da * da) / divisor;
  }

  private static int _Manhattan(Color self, Color other) =>
    Math.Abs(self.R - other.R) + Math.Abs(self.G - other.G) + Math.Abs(self.B - other.B) + Math.Abs(self.A - other.A);

  private static int _ManhattanRgbOnly(Color self, Color other) =>
    Math.Abs(self.R - other.R) + Math.Abs(self.G - other.G) + Math.Abs(self.B - other.B);

  private static int _WeightedManhattan(Color self, Color other, int wr, int wg, int wb, int wa, int divisor) {
    var dr = Math.Abs(self.R - other.R);
    var dg = Math.Abs(self.G - other.G);
    var db = Math.Abs(self.B - other.B);
    var da = Math.Abs(self.A - other.A);
    return (wr * dr + wg * dg + wb * db + wa * da) / divisor;
  }

  /// <summary>https://www.compuphase.com/cmetric.htm — fixed-point low-cost RGB distance.</summary>
  private static int _CompuPhase(Color self, Color other) {
    var rMean = (self.R + other.R) >> 1;
    var r = self.R - other.R;
    var g = self.G - other.G;
    var b = self.B - other.B;
    var a = self.A - other.A;
    r *= r;
    g *= g;
    b *= b;
    a *= a;
    var rb = ((512 + rMean) * r) >> 8;
    var bb = ((767 - rMean) * b) >> 8;
    g <<= 2;
    return rb + g + bb + a;
  }

  /// <summary>PNGQuant alpha-blend-aware difference using a fixed white-point weighting.</summary>
  private static int _PngQuant(Color self, Color other) {
    // White point = Color.White; weights = (255 << 16) / 255 = 65536 each.
    const int wp = 65536;
    var alphas = (other.A - self.A) * wp;
    return _PngQuantCh(self.R * wp >> 16, other.R * wp >> 16, alphas)
         + _PngQuantCh(self.G * wp >> 16, other.G * wp >> 16, alphas)
         + _PngQuantCh(self.B * wp >> 16, other.B * wp >> 16, alphas);
  }

  private static int _PngQuantCh(int x, int y, int alphaDiff) {
    var black = x - y;
    var white = black + (alphaDiff >> 16);
    return black * black + white * white;
  }

  private static int _WeightedYuv(Color self, Color other) {
    const float wy = 6f, wu = 2f, wv = 2f, wa = 10f;
    const float divisor = wy + wu + wv + wa;
    var (y1, u1, v1, a1) = _Yuv(self);
    var (y2, u2, v2, a2) = _Yuv(other);
    var dy = y1 - y2;
    var du = u1 - u2;
    var dv = v1 - v2;
    var da = a1 - a2;
    return (int)Math.Round(1000 * (wy * dy * dy + wu * du * du + wv * dv * dv + wa * da * da) / divisor);
  }

  private static int _WeightedYCbCr(Color self, Color other) {
    const int wy = 2, wcb = 1, wcr = 1, wa = 1, divisor = 5;
    var (y1, cb1, cr1, a1) = _YCbCr(self);
    var (y2, cb2, cr2, a2) = _YCbCr(other);
    var dy = y1 - y2;
    var dcb = cb1 - cb2;
    var dcr = cr1 - cr2;
    var da = a1 - a2;
    return (wy * dy * dy + wcb * dcb * dcb + wcr * dcr * dcr + wa * da * da) / divisor;
  }

  private static int _Cie94(Color self, Color other, double kL, double k1, double k2) {
    var (L1, a1, b1, _) = _Lab(self);
    var (L2, a2, b2, _) = _Lab(other);
    var deltaL = L1 - L2;
    var deltaA = a1 - a2;
    var deltaB = b1 - b2;
    var C1 = Math.Sqrt(a1 * a1 + b1 * b1);
    var C2 = Math.Sqrt(a2 * a2 + b2 * b2);
    var deltaC = C1 - C2;
    var deltaH2 = deltaA * deltaA + deltaB * deltaB - deltaC * deltaC;
    var deltaH = deltaH2 > 0 ? Math.Sqrt(deltaH2) : 0;
    var sC = 1 + k1 * C1;
    var sH = 1 + k2 * C1;
    var dL = deltaL / kL;
    var dC = deltaC / sC;
    var dH = deltaH / sH;
    var deltaE = Math.Sqrt(dL * dL + dC * dC + dH * dH);
    return (int)(deltaE * deltaE * 100);
  }

  private static int _CieDe2000(Color self, Color other) {
    var (L1, a1, b1, _) = _Lab(self);
    var (L2, a2, b2, _) = _Lab(other);
    var deltaE = _DeltaE2000(L1, a1, b1, L2, a2, b2);
    return (int)(deltaE * deltaE * 100);
  }

  #endregion

  #region color-space conversions

  private static (float y, float u, float v, float a) _Yuv(Color c) {
    var r = c.R / 255f;
    var g = c.G / 255f;
    var b = c.B / 255f;
    var a = c.A / 255f;
    var y = 0.299f * r + 0.587f * g + 0.114f * b;
    var u = -0.14713f * r - 0.28886f * g + 0.436f * b;
    var v = 0.615f * r - 0.51499f * g - 0.10001f * b;
    return (y, u, v, a);
  }

  private static (byte y, byte cb, byte cr, byte a) _YCbCr(Color c) {
    var y = (byte)(0.299 * c.R + 0.587 * c.G + 0.114 * c.B);
    var cb = (byte)(128 - 0.168736 * c.R - 0.331264 * c.G + 0.5 * c.B);
    var cr = (byte)(128 + 0.5 * c.R - 0.418688 * c.G - 0.081312 * c.B);
    return (y, cb, cr, c.A);
  }

  private static (double L, double a, double b, double alpha) _Lab(Color c) {
    var r = c.R / 255f;
    var g = c.G / 255f;
    var b = c.B / 255f;
    var alpha = c.A / 255f;
    r = r <= 0.04045f ? r / 12.92f : (float)Math.Pow((r + 0.055f) / 1.055f, 2.4f);
    g = g <= 0.04045f ? g / 12.92f : (float)Math.Pow((g + 0.055f) / 1.055f, 2.4f);
    b = b <= 0.04045f ? b / 12.92f : (float)Math.Pow((b + 0.055f) / 1.055f, 2.4f);
    var x = (r * 0.4124f + g * 0.3576f + b * 0.1805f) / 0.95047f;
    var y = (r * 0.2126f + g * 0.7152f + b * 0.0722f) / 1.00000f;
    var z = (r * 0.0193f + g * 0.1192f + b * 0.9505f) / 1.08883f;
    var fx = _LabF(x);
    var fy = _LabF(y);
    var fz = _LabF(z);
    var L = 116 * fy - 16;
    var A = 500 * (fx - fy);
    var B = 200 * (fy - fz);
    return (L, A, B, alpha);
  }

  private static double _LabF(double t) => t > 0.008856 ? Math.Pow(t, 1.0 / 3.0) : 7.787 * t + 16.0 / 116.0;

  private static readonly double _Pow25To7 = Math.Pow(25, 7);
  private const double _DegToRad = Math.PI / 180.0;
  private const double _RadToDeg = 180.0 / Math.PI;
  private const double _PiDiv360 = Math.PI / 360.0;

  private static double _DeltaE2000(double L1, double a1, double b1, double L2, double a2, double b2) {
    var C1 = Math.Sqrt(a1 * a1 + b1 * b1);
    var C2 = Math.Sqrt(a2 * a2 + b2 * b2);
    var Cab = (C1 + C2) * 0.5;
    var CabPow7 = Math.Pow(Cab, 7);
    var G = 0.5 * (1 - Math.Sqrt(CabPow7 / (CabPow7 + _Pow25To7)));
    var a1p = a1 * (1 + G);
    var a2p = a2 * (1 + G);
    var C1p = Math.Sqrt(a1p * a1p + b1 * b1);
    var C2p = Math.Sqrt(a2p * a2p + b2 * b2);
    var h1p = Math.Atan2(b1, a1p) * _RadToDeg;
    if (h1p < 0) h1p += 360;
    var h2p = Math.Atan2(b2, a2p) * _RadToDeg;
    if (h2p < 0) h2p += 360;
    var dLp = L2 - L1;
    var dCp = C2p - C1p;
    var dhp = 0.0;
    var prod = C1p * C2p;
    if (prod != 0) {
      var diff = h2p - h1p;
      var ad = Math.Abs(diff);
      dhp = ad <= 180 ? diff : (diff > 180 ? diff - 360 : diff + 360);
    }
    var dHp = 2 * Math.Sqrt(prod) * Math.Sin(dhp * _PiDiv360);
    var Lbp = (L1 + L2) * 0.5;
    var Cbp = (C1p + C2p) * 0.5;
    var hbp = 0.0;
    if (prod != 0) {
      var sum = h1p + h2p;
      var ad = Math.Abs(h1p - h2p);
      hbp = ad <= 180 ? sum * 0.5 : (sum < 360 ? (sum + 360) * 0.5 : (sum - 360) * 0.5);
    }
    var hbpRad = hbp * _DegToRad;
    var T = 1 - 0.17 * Math.Cos((hbp - 30) * _DegToRad)
              + 0.24 * Math.Cos(2 * hbpRad)
              + 0.32 * Math.Cos((3 * hbp + 6) * _DegToRad)
              - 0.20 * Math.Cos((4 * hbp - 63) * _DegToRad);
    var dTheta = 30 * Math.Exp(-Math.Pow((hbp - 275) / 25.0, 2));
    var CbpPow7 = Math.Pow(Cbp, 7);
    var RC = 2 * Math.Sqrt(CbpPow7 / (CbpPow7 + _Pow25To7));
    var Lm50 = Lbp - 50;
    var SL = 1 + 0.015 * Lm50 * Lm50 / Math.Sqrt(20 + Lm50 * Lm50);
    var SC = 1 + 0.045 * Cbp;
    var SH = 1 + 0.015 * Cbp * T;
    var RT = -Math.Sin(2 * dTheta * _DegToRad) * RC;
    var dL = dLp / SL;
    var dC = dCp / SC;
    var dH = dHp / SH;
    return Math.Sqrt(dL * dL + dC * dC + dH * dH + RT * dC * dH);
  }

  #endregion
}
