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
/// Calculates color distance using the PNGQuant algorithm.
/// </summary>
/// <remarks>
/// <para>This algorithm considers how colors appear when blended on both black and white backgrounds,
/// making it particularly effective for color quantization in images with gradients.</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
/// <para>Maximum raw distance: 1.0 (sqrt of sum of weights), normalized to UNorm32.One.</para>
/// <para>Reference: https://github.com/pornel/pngquant</para>
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
/// Calculates color distance using the PNGQuant algorithm with alpha support.
/// </summary>
/// <remarks>
/// <para>This version considers alpha blending on both black and white backgrounds,
/// which is the original PNGQuant behavior for semi-transparent colors.</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max distance.</para>
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
