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
/// Projects LinearRgbaF to CmykaF — preserves alpha through CMYK conversion.
/// </summary>
/// <remarks>
/// Uses the standard naive RGB→CMYK formula (max-channel black extraction).
/// Alpha is straight (not premultiplied) and is passed through unchanged; CMYK
/// black-extraction does not interact with alpha.
/// </remarks>
public readonly struct LinearRgbaFToCmykaF : IProject<LinearRgbaF, CmykaF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public CmykaF Project(in LinearRgbaF work) {
    var r = work.R;
    var g = work.G;
    var b = work.B;
    var a = work.A;

    // Subtractive primaries.
    var c = 1f - r;
    var m = 1f - g;
    var y = 1f - b;

    // Black extraction = min(C, M, Y).
    var k = c < m ? (c < y ? c : y) : (m < y ? m : y);

    // Pure-black special case.
    if (k >= 1f)
      return new(0f, 0f, 0f, 1f, a);

    var invK = 1f / (1f - k);
    return new((c - k) * invK, (m - k) * invK, (y - k) * invK, k, a);
  }
}

/// <summary>
/// Projects CmykaF back to LinearRgbaF — restores alpha unchanged.
/// </summary>
public readonly struct CmykaFToLinearRgbaF : IProject<CmykaF, LinearRgbaF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbaF Project(in CmykaF cmyka) {
    var invK = 1f - cmyka.K;
    return new(
      (1f - cmyka.C) * invK,
      (1f - cmyka.M) * invK,
      (1f - cmyka.Y) * invK,
      cmyka.A
    );
  }
}

/// <summary>
/// Projects CmykF (alpha-less) to CmykaF by introducing opaque alpha.
/// </summary>
public readonly struct CmykFToCmykaF : IProject<CmykF, CmykaF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public CmykaF Project(in CmykF cmyk) => new(cmyk.C, cmyk.M, cmyk.Y, cmyk.K, 1f);
}

/// <summary>
/// Projects CmykaF to CmykF by dropping alpha.
/// </summary>
public readonly struct CmykaFToCmykF : IProject<CmykaF, CmykF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public CmykF Project(in CmykaF cmyka) => new(cmyka.C, cmyka.M, cmyka.Y, cmyka.K);
}
