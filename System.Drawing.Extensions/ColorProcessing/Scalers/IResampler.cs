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
/// Interface for continuous-scale resamplers.
/// </summary>
/// <remarks>
/// <para>
/// Resamplers like Lanczos, Bilinear, and Bicubic support arbitrary
/// floating-point scale factors within their supported range.
/// </para>
/// <para>
/// Unlike <see cref="IPixelScaler"/>, resamplers can scale to any
/// target resolution within their supported scale range.
/// Each concrete resampler type should provide static members:
/// <list type="bullet">
/// <item><c>MinScale</c> - Minimum supported scale factor</item>
/// <item><c>MaxScale</c> - Maximum supported scale factor</item>
/// </list>
/// </para>
/// </remarks>
public interface IResampler : IScalerInfo {

  /// <summary>
  /// Gets the horizontal scaling factor.
  /// </summary>
  float ScaleX { get; }

  /// <summary>
  /// Gets the vertical scaling factor.
  /// </summary>
  float ScaleY { get; }
}
