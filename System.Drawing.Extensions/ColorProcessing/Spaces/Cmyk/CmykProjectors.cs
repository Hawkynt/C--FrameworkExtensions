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
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Working;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Spaces.Cmyk;

/// <summary>
/// Projects LinearRgbF to CmykF.
/// </summary>
public readonly struct LinearRgbFToCmykF : IProject<LinearRgbF, CmykF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public CmykF Project(in LinearRgbF work) {
    var r = work.R;
    var g = work.G;
    var b = work.B;

    // Calculate CMY (subtractive primaries)
    var c = 1f - r;
    var m = 1f - g;
    var y = 1f - b;

    // Find K (key/black) as the minimum
    var k = c < m ? (c < y ? c : y) : (m < y ? m : y);

    // Handle pure black case
    if (k >= 1f)
      return new(0f, 0f, 0f, 1f);

    // Adjust CMY values relative to K
    var invK = 1f / (1f - k);
    return new((c - k) * invK, (m - k) * invK, (y - k) * invK, k);
  }
}

/// <summary>
/// Projects LinearRgbaF to CmykF (drops alpha).
/// </summary>
public readonly struct LinearRgbaFToCmykF : IProject<LinearRgbaF, CmykF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public CmykF Project(in LinearRgbaF work) {
    var r = work.R;
    var g = work.G;
    var b = work.B;

    // Calculate CMY (subtractive primaries)
    var c = 1f - r;
    var m = 1f - g;
    var y = 1f - b;

    // Find K (key/black) as the minimum
    var k = c < m ? (c < y ? c : y) : (m < y ? m : y);

    // Handle pure black case
    if (k >= 1f)
      return new(0f, 0f, 0f, 1f);

    // Adjust CMY values relative to K
    var invK = 1f / (1f - k);
    return new((c - k) * invK, (m - k) * invK, (y - k) * invK, k);
  }
}

/// <summary>
/// Projects CmykF back to LinearRgbF.
/// </summary>
public readonly struct CmykFToLinearRgbF : IProject<CmykF, LinearRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in CmykF cmyk) {
    var c = cmyk.C;
    var m = cmyk.M;
    var y = cmyk.Y;
    var k = cmyk.K;

    // Convert CMYK to RGB
    var invK = 1f - k;
    return new(
      (1f - c) * invK,
      (1f - m) * invK,
      (1f - y) * invK
    );
  }
}
