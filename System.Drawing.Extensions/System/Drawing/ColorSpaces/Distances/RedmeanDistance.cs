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

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Drawing.ColorSpaces.Distances;

/// <summary>
/// Calculates the Compuserve color distance (also known as "redmean").
/// </summary>
/// <remarks>
/// <para>
/// This formula adjusts RGB weights based on the average red value,
/// providing better perceptual accuracy than simple Euclidean distance.
/// </para>
/// <para>
/// Formula: sqrt((2 + rMean/256)*dR² + 4*dG² + (2 + (255-rMean)/256)*dB²)
/// where rMean = (R1 + R2) / 2
/// </para>
/// </remarks>
public readonly struct RedmeanDistance : IColorDistanceCalculator {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) => Math.Sqrt(RedmeanDistanceSquared._Calculate(color1, color2));
}

/// <summary>
/// Calculates the squared Compuserve color distance (also known as "redmean").
/// Faster than <see cref="RedmeanDistance"/> when only comparing distances.
/// </summary>
public readonly struct RedmeanDistanceSquared : IColorDistanceCalculator {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) => _Calculate(color1, color2);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static double _Calculate(Color color1, Color color2) {
    var c1 = new Rgba32(color1);
    var c2 = new Rgba32(color2);

    var rMean = (c1.R + c2.R) / 2.0;
    var dr = c1.R - c2.R;
    var dg = c1.G - c2.G;
    var db = c1.B - c2.B;

    // Weights vary based on red mean value
    var rWeight = 2.0 + rMean / 256.0;
    var gWeight = 4.0;
    var bWeight = 2.0 + (255.0 - rMean) / 256.0;

    return rWeight * dr * dr + gWeight * dg * dg + bWeight * db * db;
  }
}
