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

namespace Hawkynt.ColorProcessing;

/// <summary>
/// Marker interface for all color space types.
/// </summary>
/// <remarks>
/// This is the base interface for the "Three Spaces" architecture:
/// <list type="bullet">
///   <item><description><b>Storage</b>: Byte-oriented, packed formats (Rgb24, Rgba32, etc.)</description></item>
///   <item><description><b>Working</b>: Float-based linear space for math (LinearRgbF, LinearRgbaF)</description></item>
///   <item><description><b>Key</b>: Decision space for equality/distance (YuvF, LabF)</description></item>
/// </list>
/// </remarks>
public interface IColorSpace;
