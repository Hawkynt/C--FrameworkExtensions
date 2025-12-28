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

using System.Drawing;

namespace Hawkynt.ColorProcessing.Scalers;

/// <summary>
/// Internal interface for scaler dispatch. Hidden from public API.
/// </summary>
/// <remarks>
/// <para>
/// Scalers implement this interface using explicit implementation to provide
/// self-dispatch capability while keeping the <see cref="Apply"/> method
/// hidden from the public API surface.
/// </para>
/// <para>
/// The method is only accessible through the interface type, not through
/// the implementing struct directly.
/// </para>
/// </remarks>
internal interface IScalerDispatch {

  /// <summary>
  /// Applies scaling to a bitmap.
  /// </summary>
  /// <param name="source">The source bitmap to scale.</param>
  /// <param name="quality">The quality mode for color operations.</param>
  /// <returns>A new bitmap scaled according to the scaler's factors.</returns>
  Bitmap Apply(Bitmap source, ScalerQuality quality);
}
