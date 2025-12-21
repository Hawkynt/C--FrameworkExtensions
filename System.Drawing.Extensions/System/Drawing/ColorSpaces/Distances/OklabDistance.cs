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

namespace System.Drawing.ColorSpaces;

/// <summary>
/// Calculates color distance using the Oklab perceptual color space.
/// </summary>
/// <remarks>
/// <para>Oklab provides excellent perceptual uniformity for color distance calculations.</para>
/// <para>This is recommended for most color comparison tasks where perceptual accuracy matters.</para>
/// </remarks>
public readonly struct OklabDistance : IColorDistanceCalculator {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) => Math.Sqrt(OklabDistanceSquared._Calculate(color1, color2));
}

/// <summary>
/// Calculates squared color distance using the Oklab perceptual color space.
/// Faster than <see cref="OklabDistance"/> when only comparing distances.
/// </summary>
/// <remarks>
/// <para>Oklab provides excellent perceptual uniformity for color distance calculations.</para>
/// <para>Use this when you only need to compare relative distances, as it avoids the expensive square root operation.</para>
/// </remarks>
public readonly struct OklabDistanceSquared : IColorDistanceCalculator {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public double Calculate(Color color1, Color color2) => _Calculate(color1, color2);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static double _Calculate(Color color1, Color color2) {
    var oklab1 = (Oklab)Oklab.FromColor(color1);
    var oklab2 = (Oklab)Oklab.FromColor(color2);

    var dL = oklab1.L - oklab2.L;
    var dA = oklab1.A - oklab2.A;
    var dB = oklab1.B - oklab2.B;

    return dL * dL + dA * dA + dB * dB;
  }
}
