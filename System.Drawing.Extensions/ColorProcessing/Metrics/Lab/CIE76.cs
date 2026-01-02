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
/// Calculates the CIE76 delta E (ΔE*ab) between two Lab colors.
/// </summary>
/// <remarks>
/// <para>CIE76 is the simplest and fastest Lab distance formula.
/// It's simply the Euclidean distance in Lab space.
/// While not perfectly perceptually uniform, it's adequate for many uses.</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max delta E of 100.</para>
/// </remarks>
public readonly struct CIE76 : IColorMetric<LabF>, INormalizedMetric {

  // Practical max delta E for normalization (colors beyond this are "completely different")
  private const float MaxDeltaE = 100f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in LabF a, in LabF b) {
    var raw = MathF.Sqrt(CIE76Squared._Calculate(a, b));
    return UNorm32.FromFloatClamped(raw / MaxDeltaE);
  }
}

/// <summary>
/// Calculates the squared CIE76 delta E between two Lab colors.
/// </summary>
/// <remarks>
/// <para>Faster than CIE76 when only comparing distances (no sqrt).</para>
/// <para>Returns UNorm32 normalized distance where UNorm32.One = max delta E² of 10000.</para>
/// </remarks>
public readonly struct CIE76Squared : IColorMetric<LabF>, INormalizedMetric {

  // Practical max delta E squared for normalization (100²)
  private const float MaxDeltaESquared = 10000f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float _Calculate(in LabF a, in LabF b) {
    var dL = a.L - b.L;
    var da = a.A - b.A;
    var db = a.B - b.B;
    return dL * dL + da * da + db * db;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UNorm32 Distance(in LabF a, in LabF b)
    => UNorm32.FromFloatClamped(_Calculate(a, b) / MaxDeltaESquared);
}
