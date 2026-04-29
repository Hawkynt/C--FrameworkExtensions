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
using Hawkynt.ColorProcessing.Spaces.Hdr;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.ColorMath;

/// <summary>
/// CAT16 chromatic-adaptation transform — Li, Luo et al. 2017's improvement over CAT02.
/// </summary>
/// <remarks>
/// <para>Maps an XYZ color from one illuminant's white point to another by separating into
/// LMS-like cone responses, scaling each cone in proportion to the destination/source white-
/// point cone ratio (Von Kries hypothesis), then projecting back to XYZ. CAT16 uses a
/// different LMS basis than Bradford / CAT02 — slightly more accurate near saturated colors
/// and constructed in the same paper that defines CAM16 / CAM16-UCS, so the three pieces
/// share a common appearance-model foundation.</para>
/// <para>Reference: Li, Li, Wang, Zu, Luo, Cui, Melgosa, Brill &amp; Pointer 2017,
/// "Comprehensive color solutions: CAM16, CAT16, and CAM16-UCS", Color Research &amp;
/// Application 42(6):703–718.</para>
/// </remarks>
public static class Cat16 {

  // CAT16 forward matrix M_CAT16 (cone-response basis from the 2017 paper, Table 1).
  private const float M_R_X = 0.401288f;
  private const float M_R_Y = 0.650173f;
  private const float M_R_Z = -0.051461f;
  private const float M_G_X = -0.250268f;
  private const float M_G_Y = 1.204414f;
  private const float M_G_Z = 0.045854f;
  private const float M_B_X = -0.002079f;
  private const float M_B_Y = 0.048952f;
  private const float M_B_Z = 0.953127f;

  // CAT16 inverse matrix (numerical inverse of M_CAT16).
  private const float Mi_R_X = 1.86206786f;
  private const float Mi_R_Y = -1.01125463f;
  private const float Mi_R_Z = 0.14918677f;
  private const float Mi_G_X = 0.38752654f;
  private const float Mi_G_Y = 0.62144744f;
  private const float Mi_G_Z = -0.00897398f;
  private const float Mi_B_X = -0.01584150f;
  private const float Mi_B_Y = -0.03412294f;
  private const float Mi_B_Z = 1.04996444f;

  /// <summary>
  /// Adapts <paramref name="xyz"/> from <paramref name="srcWhite"/> to <paramref name="dstWhite"/>.
  /// </summary>
  /// <remarks>Both white points must be in XYZ at the same scale (typically Y = 1 or Y = 100).</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static XyzF Adapt(in XyzF xyz, in XyzF srcWhite, in XyzF dstWhite) {
    var rs = M_R_X * srcWhite.X + M_R_Y * srcWhite.Y + M_R_Z * srcWhite.Z;
    var gs = M_G_X * srcWhite.X + M_G_Y * srcWhite.Y + M_G_Z * srcWhite.Z;
    var bs = M_B_X * srcWhite.X + M_B_Y * srcWhite.Y + M_B_Z * srcWhite.Z;
    var rd = M_R_X * dstWhite.X + M_R_Y * dstWhite.Y + M_R_Z * dstWhite.Z;
    var gd = M_G_X * dstWhite.X + M_G_Y * dstWhite.Y + M_G_Z * dstWhite.Z;
    var bd = M_B_X * dstWhite.X + M_B_Y * dstWhite.Y + M_B_Z * dstWhite.Z;

    var r = M_R_X * xyz.X + M_R_Y * xyz.Y + M_R_Z * xyz.Z;
    var g = M_G_X * xyz.X + M_G_Y * xyz.Y + M_G_Z * xyz.Z;
    var b = M_B_X * xyz.X + M_B_Y * xyz.Y + M_B_Z * xyz.Z;

    r *= rd / rs;
    g *= gd / gs;
    b *= bd / bs;

    return new XyzF(
      Mi_R_X * r + Mi_R_Y * g + Mi_R_Z * b,
      Mi_G_X * r + Mi_G_Y * g + Mi_G_Z * b,
      Mi_B_X * r + Mi_B_Y * g + Mi_B_Z * b
    );
  }

  /// <summary>D65 reference white in XYZ (Y = 1).</summary>
  public static XyzF D65 { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => new(0.95047f, 1.0f, 1.08883f); }

  /// <summary>D50 reference white in XYZ (Y = 1).</summary>
  public static XyzF D50 { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => new(0.96422f, 1.0f, 0.82521f); }
}
