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
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Working;
using UNorm32 = Hawkynt.ColorProcessing.Metrics.UNorm32;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Metrics.Rgb;

/// <summary>
/// Calculates color distance using the PNGQuant luma-weighted RGB algorithm.
/// </summary>
/// <remarks>
/// <para>Kornel Lesiński's PNGQuant uses a Rec.601 luma-weighted squared RGB difference
/// (rW=0.299, gW=0.587, bW=0.114) instead of unweighted Euclidean. The weights match
/// the human-eye photopic-sensitivity ratio, giving green-channel mismatches more
/// influence on the distance — the same principle as BT.601 luma. Cheap to compute
/// and a good practical proxy for perceptual difference in palette quantisation.</para>
/// <code>
///   ΔE² = 0.299·ΔR² + 0.587·ΔG² + 0.114·ΔB²
/// </code>
/// <para>Reference: K. Lesiński, "pngquant — lossy PNG compressor",
/// <see href="https://github.com/kornelski/pngquant"/>;
/// the colour-difference function is in <c>libimagequant/pam.c</c>. Weight constants
/// from ITU-R BT.601-7 §2.5.1.</para>
/// </remarks>
public readonly struct PngQuant : IColorMetric<LinearRgbF>, INormalizedMetric {

  // Max distance is sqrt(0.299 + 0.587 + 0.114) = sqrt(1) = 1
  private const float MaxDistance = 1f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in LinearRgbF a, in LinearRgbF b) {
    var raw = MathF.Sqrt(PngQuantSquared._Calculate(a, b));
    return UNorm32.FromFloatClamped(raw / MaxDistance);
  }
}

/// <summary>
/// Calculates squared color distance using the PNGQuant algorithm.
/// </summary>
/// <remarks>
/// <para>Faster than PngQuant when only comparing distances (no sqrt).</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
/// <para>Maximum raw distance: 1.0 (sum of weights), normalized to UNorm32.One.</para>
/// </remarks>
public readonly struct PngQuantSquared : IColorMetric<LinearRgbF>, INormalizedMetric {

  // Max squared distance is 0.299 + 0.587 + 0.114 = 1.0
  private const float MaxDistance = 1f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float _Calculate(in LinearRgbF a, in LinearRgbF b) {
    var dr = a.R - b.R;
    var dg = a.G - b.G;
    var db = a.B - b.B;

    const float rWeight = 0.299f;
    const float gWeight = 0.587f;
    const float bWeight = 0.114f;

    return rWeight * dr * dr + gWeight * dg * dg + bWeight * db * db;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in LinearRgbF a, in LinearRgbF b)
    => UNorm32.FromFloatClamped(_Calculate(a, b) / MaxDistance);
}

/// <summary>
/// Calculates color distance using PNGQuant's alpha-aware blend-distance algorithm.
/// </summary>
/// <remarks>
/// <para>PNGQuant's RGBA distance: rather than treating alpha as a 4th independent
/// channel, this metric measures how the colour would appear when alpha-composited
/// on a black background and on a white background, then sums the squared differences.
/// Two semi-transparent colours that look identical on both backgrounds are considered
/// equal regardless of their straight-RGBA values — which is the right notion for
/// quantising images with antialiasing or gradients.</para>
/// <para>Reference: K. Lesiński, "pngquant — lossy PNG compressor",
/// <see href="https://github.com/kornelski/pngquant"/>; original blend-distance
/// algorithm in libimagequant.</para>
/// </remarks>
public readonly struct PngQuantRgba : IColorMetric<LinearRgbaF>, INormalizedMetric {

  // Max distance for RGBA is approximately sqrt(6) ≈ 2.45 (worst case blending)
  private const float MaxDistance = 2.45f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in LinearRgbaF a, in LinearRgbaF b) {
    var dr = a.R - b.R;
    var dg = a.G - b.G;
    var db = a.B - b.B;
    var da = a.A - b.A;

    // Consider blending on black (using premultiplied colors)
    var drBlack = a.R * a.A - b.R * b.A;
    var dgBlack = a.G * a.A - b.G * b.A;
    var dbBlack = a.B * a.A - b.B * b.A;

    // Consider blending on white (1 - a + a*c)
    var drWhite = (1f - a.A + a.R * a.A) - (1f - b.A + b.R * b.A);
    var dgWhite = (1f - a.A + a.G * a.A) - (1f - b.A + b.G * b.A);
    var dbWhite = (1f - a.A + a.B * a.A) - (1f - b.A + b.B * b.A);

    // Use max of black and white distances
    var rDist = drBlack * drBlack + drWhite * drWhite;
    var gDist = dgBlack * dgBlack + dgWhite * dgWhite;
    var bDist = dbBlack * dbBlack + dbWhite * dbWhite;

    var raw = MathF.Sqrt(rDist + gDist + bDist);
    return UNorm32.FromFloatClamped(raw / MaxDistance);
  }
}
