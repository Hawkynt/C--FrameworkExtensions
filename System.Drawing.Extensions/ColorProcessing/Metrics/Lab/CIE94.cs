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
using Hawkynt.ColorProcessing.Spaces.Lab;
using UNorm32 = Hawkynt.ColorProcessing.Metrics.UNorm32;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Metrics.Lab;

/// <summary>
/// Calculates the CIE94 delta E between two Lab colors.
/// </summary>
/// <remarks>
/// <para>CIE94 improves on CIE76 by accounting for perceptual non-uniformity
/// in chroma and hue at different lightness levels.
/// This version uses graphic arts parameters (kL=1, K1=0.045, K2=0.015).</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max delta E of 100.</para>
/// </remarks>
public readonly struct CIE94 : IColorMetric<LabF>, INormalizedMetric {

  // Graphic arts parameters
  private const float KL = 1f;
  private const float K1 = 0.045f;
  private const float K2 = 0.015f;

  // Practical max delta E for normalization
  private const float MaxDeltaE = 100f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in LabF a, in LabF b) {
    var dL = a.L - b.L;
    var da = a.A - b.A;
    var db = a.B - b.B;

    var c1 = (float)Math.Sqrt(a.A * a.A + a.B * a.B);
    var c2 = (float)Math.Sqrt(b.A * b.A + b.B * b.B);
    var dC = c1 - c2;

    var dH2 = da * da + db * db - dC * dC;
    var dH = dH2 > 0 ? (float)Math.Sqrt(dH2) : 0f;

    var sL = 1f;
    var sC = 1f + K1 * c1;
    var sH = 1f + K2 * c1;

    var lightness = dL / (KL * sL);
    var chroma = dC / sC;
    var hue = dH / sH;

    var raw = (float)Math.Sqrt(lightness * lightness + chroma * chroma + hue * hue);
    return UNorm32.FromFloatClamped(raw / MaxDeltaE);
  }
}

/// <summary>
/// Calculates the CIE94 delta E with textile parameters.
/// </summary>
/// <remarks>
/// <para>Uses textile parameters (kL=2, K1=0.048, K2=0.014) which give
/// more weight to lightness differences.</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max delta E of 100.</para>
/// </remarks>
public readonly struct CIE94Textile : IColorMetric<LabF>, INormalizedMetric {

  // Textile parameters
  private const float KL = 2f;
  private const float K1 = 0.048f;
  private const float K2 = 0.014f;

  // Practical max delta E for normalization
  private const float MaxDeltaE = 100f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in LabF a, in LabF b) {
    var dL = a.L - b.L;
    var da = a.A - b.A;
    var db = a.B - b.B;

    var c1 = (float)Math.Sqrt(a.A * a.A + a.B * a.B);
    var c2 = (float)Math.Sqrt(b.A * b.A + b.B * b.B);
    var dC = c1 - c2;

    var dH2 = da * da + db * db - dC * dC;
    var dH = dH2 > 0 ? (float)Math.Sqrt(dH2) : 0f;

    var sL = 1f;
    var sC = 1f + K1 * c1;
    var sH = 1f + K2 * c1;

    var lightness = dL / (KL * sL);
    var chroma = dC / sC;
    var hue = dH / sH;

    var raw = (float)Math.Sqrt(lightness * lightness + chroma * chroma + hue * hue);
    return UNorm32.FromFloatClamped(raw / MaxDeltaE);
  }
}
