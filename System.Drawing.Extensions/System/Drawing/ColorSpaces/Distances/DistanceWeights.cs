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

namespace System.Drawing.ColorSpaces.Distances;

/// <summary>
/// Provides predefined weight presets for weighted color distance calculations.
/// </summary>
/// <remarks>
/// These weights are used by various weighted distance calculators to adjust
/// the importance of different color channels based on human perception or specific use cases.
/// </remarks>
public static class DistanceWeights {

  /// <summary>
  /// Low red sensitivity weights - emphasizes green over red.
  /// </summary>
  public static class LowRed {
    public const int Red = 2;
    public const int Green = 4;
    public const int Blue = 3;
    public const int Alpha = 1;
  }

  /// <summary>
  /// High red sensitivity weights - emphasizes red over blue.
  /// </summary>
  public static class HighRed {
    public const int Red = 3;
    public const int Green = 4;
    public const int Blue = 2;
    public const int Alpha = 1;
  }

  /// <summary>
  /// BT.709 (Rec. 709) weights based on HDTV standard.
  /// Uses the relative luminance formula: Y = 0.2126R + 0.7152G + 0.0722B
  /// </summary>
  /// <remarks>
  /// Reference: ITU-R Recommendation BT.709
  /// </remarks>
  public static class BT709 {
    public const int Red = 2126;
    public const int Green = 7152;
    public const int Blue = 722;
    public const int Alpha = 10000;
    public const int Divisor = 10000;
  }

  /// <summary>
  /// Nommyde weights optimized for perceptual color difference.
  /// </summary>
  /// <remarks>
  /// Reference: https://github.com/igor-bezkrovny/image-quantization/issues/4#issuecomment-235155320
  /// </remarks>
  public static class Nommyde {
    public const int Red = 4984;
    public const int Green = 8625;
    public const int Blue = 2979;
    public const int Alpha = 10000;
    public const int Divisor = 10000;
  }

}
