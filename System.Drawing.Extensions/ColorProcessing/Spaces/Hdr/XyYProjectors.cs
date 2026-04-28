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
// <https://github.com/Hawkynt+C--FrameworkExtensions/blob/master/LICENSE>.

#endregion

using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Constants;
using Hawkynt.ColorProcessing.Working;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Spaces.Hdr;

/// <summary>
/// Projects XyzF to XyYF (chromaticity + luminance form).
/// </summary>
/// <remarks>
/// <para>x = X/(X+Y+Z), y = Y/(X+Y+Z), Y = Y. For black (sum = 0) the chromaticity
/// degenerates; this implementation falls back to the D65 white point's (x, y) so
/// that the round-trip XyY→Xyz produces (0, 0, 0) again.</para>
/// </remarks>
public readonly struct XyzFToXyYF : IProject<XyzF, XyYF> {

  // D65 fallback chromaticity (CIE 1931 standard observer).
  private const float D65_x = 0.31271f;
  private const float D65_y = 0.32902f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public XyYF Project(in XyzF xyz) {
    var sum = xyz.X + xyz.Y + xyz.Z;
    if (sum < 1e-9f) return new(D65_x, D65_y, 0f);
    return new(xyz.X / sum, xyz.Y / sum, xyz.Y);
  }
}

/// <summary>
/// Projects XyYF back to XyzF.
/// </summary>
public readonly struct XyYFToXyzF : IProject<XyYF, XyzF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public XyzF Project(in XyYF xyY) {
    if (xyY.Y < 1e-9f) return new(0f, 0f, 0f);
    var X = xyY.BigY * xyY.X / xyY.Y;
    var Z = xyY.BigY * (1f - xyY.X - xyY.Y) / xyY.Y;
    return new(X, xyY.BigY, Z);
  }
}

/// <summary>
/// Projects LinearRgbF to XyYF via XYZ.
/// </summary>
public readonly struct LinearRgbFToXyYF : IProject<LinearRgbF, XyYF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public XyYF Project(in LinearRgbF work) {
    var xyz = new LinearRgbFToXyzF().Project(work);
    return new XyzFToXyYF().Project(xyz);
  }
}

/// <summary>
/// Projects LinearRgbaF to XyYF via XYZ.
/// </summary>
public readonly struct LinearRgbaFToXyYF : IProject<LinearRgbaF, XyYF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public XyYF Project(in LinearRgbaF work) {
    var xyz = new LinearRgbaFToXyzF().Project(work);
    return new XyzFToXyYF().Project(xyz);
  }
}

/// <summary>
/// Projects XyYF back to LinearRgbF via XYZ.
/// </summary>
public readonly struct XyYFToLinearRgbF : IProject<XyYF, LinearRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in XyYF xyY) {
    var xyz = new XyYFToXyzF().Project(xyY);
    return new XyzFToLinearRgbF().Project(xyz);
  }
}
