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
/// Calculates the DIN99 color difference.
/// German standard (DIN 6176) optimized for small color differences.
/// </summary>
/// <remarks>
/// <para>
/// DIN99 transforms Lab coordinates into a more perceptually uniform space
/// before calculating Euclidean distance. It was designed to be computationally
/// simpler than CIEDE2000 while providing good perceptual uniformity.
/// </para>
/// </remarks>
public readonly struct DIN99Distance : IColorDistanceCalculator {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) => Math.Sqrt(DIN99DistanceSquared._Calculate(color1, color2));
}

/// <summary>
/// Calculates the squared DIN99 color difference.
/// Faster than <see cref="DIN99Distance"/> when only comparing distances.
/// </summary>
public readonly struct DIN99DistanceSquared : IColorDistanceCalculator {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) => _Calculate(color1, color2);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static double _Calculate(Color color1, Color color2) {
    var din1 = (Din99Normalized)Din99Normalized.FromColor(color1);
    var din2 = (Din99Normalized)Din99Normalized.FromColor(color2);

    var dL = din1.L - din2.L;
    var da = din1.A - din2.A;
    var db = din1.B - din2.B;

    return dL * dL + da * da + db * db;
  }
}
