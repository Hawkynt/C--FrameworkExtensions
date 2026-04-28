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
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Gamut;

/// <summary>
/// Jed Smith's "Gamut Compress" gamut-mapping operator (2021).
/// </summary>
/// <remarks>
/// <para>Adopted as one of the ACES Output Transform 2.0 reference operators (ACES
/// "OT v2" / "Gamut Compress"). Compresses out-of-gamut colours <em>asymptotically</em>
/// toward the gamut boundary using a per-channel power function:
/// values above the threshold T are remapped onto a "limit" L &gt; 1 via
/// <c>d = (d − T) / (L − T)</c>; <c>d' = (d − d^p / (1 + d^p)^(1/p))·(L − T) + T</c>
/// where p is the curve "power" (controls how soft the rolloff is).</para>
/// <para>Hue and lightness are preserved because the operator works in the
/// <em>achromatic-distance</em> space (channels relative to the local achromatic
/// max), so the in-gamut sub-region [0, T]³ is left bit-identical and only the
/// over-bright tails are reshaped.</para>
/// <para>Default thresholds and limits below are the ACES OT 2.0 published values
/// (Jed Smith, ACEScentral 2021).</para>
/// <para>References:
/// <list type="bullet">
///   <item><description>Jed Smith, "Gamut Compress" (2021) — original Nuke gizmo and OCIO config.</description></item>
///   <item><description>ACES OT v2 working group meeting notes (2022-2023).</description></item>
/// </list>
/// </para>
/// </remarks>
public readonly struct GamutCompress : IGamutMap {

  // ACES OT v2 reference defaults — distance limits per channel above which
  // the curve asymptotes (i.e. how far out-of-gamut to absorb).
  private const float DistLimitC = 1.147f;  // cyan complement of red
  private const float DistLimitM = 1.264f;  // magenta complement of green
  private const float DistLimitY = 1.312f;  // yellow complement of blue

  // Threshold below which colours are passed through unchanged (per channel,
  // in distance from achromatic). Values < 1 give some "in-gamut soft-toe".
  private const float ThreshC = 0.815f;
  private const float ThreshM = 0.803f;
  private const float ThreshY = 0.880f;

  // Curve power — higher = sharper rolloff close to the limit.
  private const float Power = 1.2f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Map(in LinearRgbF color) {
    // Achromatic axis = max(R, G, B); preserves hue + value during compression.
    var ach = MathF.Max(color.R, MathF.Max(color.G, color.B));
    if (ach < 1e-9f) return new(0f, 0f, 0f);

    var dR = (ach - color.R) / ach;
    var dG = (ach - color.G) / ach;
    var dB = (ach - color.B) / ach;

    var dRc = Compress(dR, DistLimitC, ThreshC);
    var dGc = Compress(dG, DistLimitM, ThreshM);
    var dBc = Compress(dB, DistLimitY, ThreshY);

    return new(
      ach * (1f - dRc),
      ach * (1f - dGc),
      ach * (1f - dBc)
    );
  }

  /// <summary>Asymptotic Reinhard-like compression of a single distance value.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float Compress(float d, float l, float t) {
    if (d < t) return d;

    var s = (l - t) / MathF.Pow(MathF.Pow((1f - t) / (l - t), -Power) - 1f, 1f / Power);
    var n = (d - t) / s;
    return t + s * (n / MathF.Pow(1f + MathF.Pow(n, Power), 1f / Power));
  }
}
