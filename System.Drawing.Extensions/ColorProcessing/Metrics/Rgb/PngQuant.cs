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
using SysMath = System.Math;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Metrics.Rgb;

/// <summary>
/// Calculates color distance using the PNGQuant algorithm.
/// </summary>
/// <remarks>
/// This algorithm considers how colors appear when blended on both black and white backgrounds,
/// making it particularly effective for color quantization in images with gradients.
/// Reference: https://github.com/pornel/pngquant
/// </remarks>
public readonly struct PngQuant : IColorMetric<LinearRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in LinearRgbF a, in LinearRgbF b)
#if SUPPORTS_MATHF
    => MathF.Sqrt(PngQuantSquared._Calculate(a, b));
#else
    => (float)SysMath.Sqrt(PngQuantSquared._Calculate(a, b));
#endif
}

/// <summary>
/// Calculates squared color distance using the PNGQuant algorithm.
/// </summary>
/// <remarks>
/// Faster than PngQuant when only comparing distances (no sqrt).
/// </remarks>
public readonly struct PngQuantSquared : IColorMetric<LinearRgbF> {

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
  public float Distance(in LinearRgbF a, in LinearRgbF b) => _Calculate(a, b);
}

/// <summary>
/// Calculates color distance using the PNGQuant algorithm with alpha support.
/// </summary>
/// <remarks>
/// This version considers alpha blending on both black and white backgrounds,
/// which is the original PNGQuant behavior for semi-transparent colors.
/// </remarks>
public readonly struct PngQuantRgba : IColorMetric<LinearRgbaF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public float Distance(in LinearRgbaF a, in LinearRgbaF b) {
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

    return (float)SysMath.Sqrt(rDist + gDist + bDist);
  }
}
