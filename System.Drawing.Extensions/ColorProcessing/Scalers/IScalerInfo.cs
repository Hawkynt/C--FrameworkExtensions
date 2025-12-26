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
/// Base interface for all image scaling algorithms.
/// </summary>
/// <remarks>
/// Provides the fundamental <see cref="Scale"/> property that defines
/// the scaling factor. Extended by <see cref="IPixelScaler"/> for
/// discrete pixel-art scalers and <see cref="IResampler"/> for
/// continuous-scale resamplers.
/// </remarks>
public interface IScalerInfo {

  /// <summary>
  /// Gets the scaling factor for this scaler instance.
  /// </summary>
  ScaleFactor Scale { get; }
}
