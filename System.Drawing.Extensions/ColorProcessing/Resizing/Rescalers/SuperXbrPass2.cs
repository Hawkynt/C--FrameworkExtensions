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
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Rescalers;

/// <summary>
/// Hyllian Super-xBR pass-2 post-processing — runs the full pass-2 filter over the
/// already-rescaled (pass-0+1) 2× bitmap.
/// </summary>
/// <remarks>
/// <para>The reference shader's pass 2 (https://github.com/libretro/glsl-shaders/tree/master/xbr/shaders/super-xbr,
/// <c>super-xbr-pass2.glsl</c>) runs at every output position with a 4×4 stencil read from
/// the already-filled pass-0+1 output grid. Mode-0 coefficients:
/// <c>wp = (1, 0, 2, 3, −2, 1)</c>, <c>weight1 ≈ 0.1296</c>, <c>weight2 ≈ 0.0875</c>.</para>
/// <para>This static helper applies that pass uniformly to a 2×-rescaled bitmap so SuperXbr
/// produces canonical 3-pass output without the caller knowing about the multi-pass
/// orchestration. Boundary pixels use clamp-to-edge sampling.</para>
/// </remarks>
internal static class SuperXbrPass2 {

  // Pass-2 mode-0 coefficients per super-xbr-pass2.glsl.
  private const float Wp1 = 1f, Wp2 = 0f, Wp3 = 2f, Wp4 = 3f, Wp5 = -2f, Wp6 = 1f;
  private const float Weight1 = 1.29633f / 10f;
  private const float Weight2 = 1.75068f / 10f / 2f;
  private const float XbrEdgeStr = 0.6f;

  // BT.601 luminance coefficients (matches the rest of the Super-xBR pipeline,
  // and the canonical RGBtoYUV in the reference shader: dot(c, vec3(.2126,.7152,.0722))
  // is BT.709, but for byte-domain post-processing BT.601 is the standard.).
  private const float LumR = 0.299f, LumG = 0.587f, LumB = 0.114f;

  /// <summary>
  /// Applies pass-2 in place on a 2× rescaled BGRA bitmap. The bitmap must be
  /// <see cref="PixelFormat.Format32bppArgb"/>. Boundary pixels (within the 2-pixel
  /// border) use clamp-to-edge stencil sampling.
  /// </summary>
  public static unsafe void Apply(Bitmap bitmap) {
    if (bitmap is null)
      throw new ArgumentNullException(nameof(bitmap));
    if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
      throw new ArgumentException("Bitmap must be Format32bppArgb.", nameof(bitmap));

    var w = bitmap.Width;
    var h = bitmap.Height;
    if (w < 4 || h < 4)
      return; // too small for a meaningful 4×4 stencil

    var rect = new Rectangle(0, 0, w, h);
    var data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
    try {
      var stride = data.Stride;
      var src = (byte*)data.Scan0;

      // Snapshot the input so the pass-2 stencil reads pre-pass state, not partial output.
      var size = stride * h;
      var snapshot = new byte[size];
      Marshal.Copy(data.Scan0, snapshot, 0, size);

      fixed (byte* snap = snapshot) {
        for (var y = 0; y < h; ++y)
          for (var x = 0; x < w; ++x)
            _ApplyAt(snap, src, x, y, w, h, stride);
      }
    } finally {
      bitmap.UnlockBits(data);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe void _ApplyAt(byte* snap, byte* dest, int x, int y, int w, int h, int stride) {
    // 4×4 stencil centred on (x, y). Output pixel sits at E. Layout matches super-xbr-passN.glsl:
    //   |P0|B |C |P1|     row y-2 (or y-1 for the inner row)
    //   |D |E |F |F4|     row y-1 (E at x-1, F at x)
    //   |G |H |I |I4|     row y   (H at x-1, I at x)
    //   |P2|H5|I5|P3|     row y+1
    // To keep the output at integer (x, y) we centre on E=(x-1, y-1), so:
    //   stencil column = x-2, x-1, x, x+1 with E in column 1 (i.e., x-1).
    // For our purposes — sharpening the centre pixel — we equivalently put E at (x, y)
    // and use the same stencil shifted by one row/col. Pick the latter so the output
    // is computed AT (x, y) with E at (x, y).
    var p0r = 0f; var p0g = 0f; var p0b = 0f;
    var br  = 0f; var bg  = 0f; var bb  = 0f;
    var cr  = 0f; var cg  = 0f; var cb  = 0f;
    var p1r = 0f; var p1g = 0f; var p1b = 0f;

    var dr  = 0f; var dg  = 0f; var db  = 0f;
    var er  = 0f; var eg  = 0f; var eb  = 0f; var ea = 0f;
    var fr  = 0f; var fg  = 0f; var fb  = 0f;
    var f4r = 0f; var f4g = 0f; var f4b = 0f;

    var gr  = 0f; var gg  = 0f; var gb  = 0f;
    var hr  = 0f; var hg  = 0f; var hb  = 0f;
    var ir  = 0f; var ig  = 0f; var ib  = 0f;
    var i4r = 0f; var i4g = 0f; var i4b = 0f;

    var p2r = 0f; var p2g = 0f; var p2b = 0f;
    var h5r = 0f; var h5g = 0f; var h5b = 0f;
    var i5r = 0f; var i5g = 0f; var i5b = 0f;
    var p3r = 0f; var p3g = 0f; var p3b = 0f;

    _ReadBgra(snap, x - 1, y - 1, w, h, stride, out p0r, out p0g, out p0b, out _);
    _ReadBgra(snap, x,     y - 1, w, h, stride, out br,  out bg,  out bb,  out _);
    _ReadBgra(snap, x + 1, y - 1, w, h, stride, out cr,  out cg,  out cb,  out _);
    _ReadBgra(snap, x + 2, y - 1, w, h, stride, out p1r, out p1g, out p1b, out _);

    _ReadBgra(snap, x - 1, y,     w, h, stride, out dr,  out dg,  out db,  out _);
    _ReadBgra(snap, x,     y,     w, h, stride, out er,  out eg,  out eb,  out ea);
    _ReadBgra(snap, x + 1, y,     w, h, stride, out fr,  out fg,  out fb,  out _);
    _ReadBgra(snap, x + 2, y,     w, h, stride, out f4r, out f4g, out f4b, out _);

    _ReadBgra(snap, x - 1, y + 1, w, h, stride, out gr,  out gg,  out gb,  out _);
    _ReadBgra(snap, x,     y + 1, w, h, stride, out hr,  out hg,  out hb,  out _);
    _ReadBgra(snap, x + 1, y + 1, w, h, stride, out ir,  out ig,  out ib,  out _);
    _ReadBgra(snap, x + 2, y + 1, w, h, stride, out i4r, out i4g, out i4b, out _);

    _ReadBgra(snap, x - 1, y + 2, w, h, stride, out p2r, out p2g, out p2b, out _);
    _ReadBgra(snap, x,     y + 2, w, h, stride, out h5r, out h5g, out h5b, out _);
    _ReadBgra(snap, x + 1, y + 2, w, h, stride, out i5r, out i5g, out i5b, out _);
    _ReadBgra(snap, x + 2, y + 2, w, h, stride, out p3r, out p3g, out p3b, out _);

    // Luminance per stencil position.
    var p0 = LumR * p0r + LumG * p0g + LumB * p0b;
    var b  = LumR * br  + LumG * bg  + LumB * bb;
    var c  = LumR * cr  + LumG * cg  + LumB * cb;
    var p1 = LumR * p1r + LumG * p1g + LumB * p1b;
    var d  = LumR * dr  + LumG * dg  + LumB * db;
    var e  = LumR * er  + LumG * eg  + LumB * eb;
    var f  = LumR * fr  + LumG * fg  + LumB * fb;
    var f4 = LumR * f4r + LumG * f4g + LumB * f4b;
    var g  = LumR * gr  + LumG * gg  + LumB * gb;
    var hh = LumR * hr  + LumG * hg  + LumB * hb;
    var ii = LumR * ir  + LumG * ig  + LumB * ib;
    var i4 = LumR * i4r + LumG * i4g + LumB * i4b;
    var p2 = LumR * p2r + LumG * p2g + LumB * p2b;
    var h5 = LumR * h5r + LumG * h5g + LumB * h5b;
    var i5 = LumR * i5r + LumG * i5g + LumB * i5b;
    var p3 = LumR * p3r + LumG * p3g + LumB * p3b;

    // d_edge: signed; positive favours one diagonal, negative the other.
    var dEdge = _Dwd(d, b, g, e, c, p2, hh, f, p1, h5, ii, f4, i5, i4)
              - _Dwd(c, f4, b, f, i4, p0, e, ii, p3, d, hh, i5, g, h5);
    var hvEdge = _Hvwd(f, ii, e, hh, c, i5, b, h5)
               - _Hvwd(e, f, hh, ii, d, f4, g, i4);

    var edgeStrength = MathF.Min(1f, MathF.Abs(dEdge) / (XbrEdgeStr + 1e-6f));
    edgeStrength = edgeStrength * edgeStrength * (3f - 2f * edgeStrength);

    // Diagonal candidates c1 (P2-H-F-P1) and c2 (P0-E-I-P3); h/v candidates c3 / c4.
    Filter1Tap(p2r, hr, fr, p1r, out var c1R);
    Filter1Tap(p2g, hg, fg, p1g, out var c1G);
    Filter1Tap(p2b, hb, fb, p1b, out var c1B);
    Filter1Tap(p0r, er, ir, p3r, out var c2R);
    Filter1Tap(p0g, eg, ig, p3g, out var c2G);
    Filter1Tap(p0b, eb, ib, p3b, out var c2B);
    Filter2Pair(dr, gr, er, hr, fr, ir, f4r, i4r, out var c3R);
    Filter2Pair(dg, gg, eg, hg, fg, ig, f4g, i4g, out var c3G);
    Filter2Pair(db, gb, eb, hb, fb, ib, f4b, i4b, out var c3B);
    Filter2Pair(cr, br, fr, er, ir, hr, i5r, h5r, out var c4R);
    Filter2Pair(cg, bg, fg, eg, ig, hg, i5g, h5g, out var c4G);
    Filter2Pair(cb, bb, fb, eb, ib, hb, i5b, h5b, out var c4B);

    // Pick diag side by sign(d_edge), hv side by sign(hv_edge), blend by edge_strength.
    var diagR = dEdge >= 0 ? c2R : c1R;
    var diagG = dEdge >= 0 ? c2G : c1G;
    var diagB = dEdge >= 0 ? c2B : c1B;
    var hvR = hvEdge >= 0 ? c4R : c3R;
    var hvG = hvEdge >= 0 ? c4G : c3G;
    var hvB = hvEdge >= 0 ? c4B : c3B;

    var t = edgeStrength;
    var oR = hvR + (diagR - hvR) * t;
    var oG = hvG + (diagG - hvG) * t;
    var oB = hvB + (diagB - hvB) * t;

    // Anti-ringing: clamp result to local {E, F, H, I} bounding box per pass-2 shader.
    var minR = MathF.Min(MathF.Min(er, fr), MathF.Min(hr, ir));
    var maxR = MathF.Max(MathF.Max(er, fr), MathF.Max(hr, ir));
    var minG = MathF.Min(MathF.Min(eg, fg), MathF.Min(hg, ig));
    var maxG = MathF.Max(MathF.Max(eg, fg), MathF.Max(hg, ig));
    var minB = MathF.Min(MathF.Min(eb, fb), MathF.Min(hb, ib));
    var maxB = MathF.Max(MathF.Max(eb, fb), MathF.Max(hb, ib));
    if (oR < minR) oR = minR; else if (oR > maxR) oR = maxR;
    if (oG < minG) oG = minG; else if (oG > maxG) oG = maxG;
    if (oB < minB) oB = minB; else if (oB > maxB) oB = maxB;

    // Write the new value at output (x, y) — alpha unchanged from E (the centre).
    var off = y * stride + x * 4;
    dest[off + 0] = (byte)(oB * 255f + 0.5f);
    dest[off + 1] = (byte)(oG * 255f + 0.5f);
    dest[off + 2] = (byte)(oR * 255f + 0.5f);
    dest[off + 3] = (byte)(ea * 255f + 0.5f);
  }

  /// <summary>
  /// 4-tap directional filter with weights w1 = (-w, w+0.5, w+0.5, -w), sums to 1.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void Filter1Tap(float a, float b, float c, float d, out float result) {
    var w = Weight1;
    result = -w * a + (w + 0.5f) * b + (w + 0.5f) * c + -w * d;
    if (result < 0f) result = 0f;
    else if (result > 1f) result = 1f;
  }

  /// <summary>
  /// 4-tap on PAIRS with weights w2 = (-w, w+0.25, w+0.25, -w), divided by 3 (matches
  /// the reference shader's <c>mul(w2, mat4x3(..., ..., ..., ...))/3</c> form which sums
  /// pairs of pixels at each tap position).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void Filter2Pair(
    float a, float b, float c, float d, float e, float f, float g, float h, out float result) {
    var w = Weight2;
    result = (-w * (a + b) + (w + 0.25f) * (c + d) + (w + 0.25f) * (e + f) + -w * (g + h)) * (1f / 3f);
    if (result < 0f) result = 0f;
    else if (result > 1f) result = 1f;
  }

  /// <summary>
  /// Hyllian d_wd weighted-distance over a 16-tap stencil with pass-2 coefficients
  /// <c>wp = (1, 0, 2, 3, −2, 1)</c>.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Dwd(
    float b0, float b1,
    float c0, float c1, float c2,
    float d0, float d1, float d2, float d3,
    float e1, float e2, float e3,
    float f2, float f3) {
    static float Df(float a, float b) => MathF.Abs(a - b);
    return Wp1 * (Df(c1, c2) + Df(c1, c0) + Df(e2, e1) + Df(e2, e3))
         + Wp2 * (Df(d2, d3) + Df(d0, d1))
         + Wp3 * (Df(d1, d3) + Df(d0, d2))
         + Wp4 * Df(d1, d2)
         + Wp5 * (Df(c0, c2) + Df(e1, e3))
         + Wp6 * (Df(b0, b1) + Df(f2, f3));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Hvwd(float i1, float i2, float i3, float i4, float e1, float e2, float e3, float e4) {
    static float Df(float a, float b) => MathF.Abs(a - b);
    return Wp4 * (Df(i1, i2) + Df(i3, i4))
         + Wp1 * (Df(i1, e1) + Df(i2, e2) + Df(i3, e3) + Df(i4, e4))
         + Wp3 * (Df(i1, e2) + Df(i3, e4) + Df(e1, i2) + Df(e3, i4));
  }

  /// <summary>
  /// Reads a BGRA pixel from <paramref name="snap"/> at (x, y) with clamp-to-edge.
  /// Returns RGB and A as normalised floats.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe void _ReadBgra(
    byte* snap, int x, int y, int w, int h, int stride,
    out float r, out float g, out float b, out float a) {
    if (x < 0) x = 0; else if (x >= w) x = w - 1;
    if (y < 0) y = 0; else if (y >= h) y = h - 1;
    var off = y * stride + x * 4;
    b = snap[off + 0] * (1f / 255f);
    g = snap[off + 1] * (1f / 255f);
    r = snap[off + 2] * (1f / 255f);
    a = snap[off + 3] * (1f / 255f);
  }
}
