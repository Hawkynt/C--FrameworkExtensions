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

using Hawkynt.ColorProcessing.Working;

namespace Hawkynt.ColorProcessing.Gamut;

/// <summary>
/// Maps a (potentially out-of-gamut) <see cref="LinearRgbF"/> triplet into the
/// destination gamut, defined here as the unit cube [0, 1]³.
/// </summary>
/// <remarks>
/// <para>Wide → narrow gamut conversions (e.g. Rec.2020 → Rec.709, Display-P3 → sRGB,
/// ProPhoto-RGB → sRGB) routinely produce <see cref="LinearRgbF"/> values outside
/// [0, 1]. Naively clamping each channel introduces hue shifts and loss of detail
/// in saturated regions; gamut-mapping operators trade some accuracy for visually
/// pleasing reconstructions of out-of-gamut colours.</para>
/// <para>Implementations are stateless structs for zero-cost generic dispatch in
/// hot pixel loops.</para>
/// </remarks>
public interface IGamutMap {

  /// <summary>
  /// Returns an in-gamut representation (each channel in [0, 1]) of <paramref name="color"/>.
  /// </summary>
  /// <param name="color">A linear-RGB colour, possibly outside [0, 1]³.</param>
  /// <returns>The mapped colour, with all channels guaranteed to lie in [0, 1].</returns>
  LinearRgbF Map(in LinearRgbF color);
}
