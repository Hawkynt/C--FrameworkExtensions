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

namespace Hawkynt.ColorProcessing.Scalers;

/// <summary>
/// Interface for integer-ratio downscalers.
/// </summary>
/// <remarks>
/// <para>
/// Downscalers reduce image size by combining multiple source pixels
/// into single output pixels using averaging or other techniques.
/// </para>
/// <para>
/// Unlike <see cref="IPixelScaler"/> which upscales, downscalers work
/// by reading NxN blocks of source pixels and producing single output pixels.
/// Each concrete downscaler type should provide static members:
/// <list type="bullet">
/// <item><c>SupportedRatios</c> - Array of supported downscale ratios</item>
/// <item><c>SupportsRatio(int)</c> - Check if a ratio is supported</item>
/// </list>
/// </para>
/// </remarks>
public interface IDownscaler : IScalerInfo {

  /// <summary>
  /// Gets the horizontal downscale ratio (2 means source width / 2).
  /// </summary>
  int RatioX { get; }

  /// <summary>
  /// Gets the vertical downscale ratio (2 means source height / 2).
  /// </summary>
  int RatioY { get; }
}
