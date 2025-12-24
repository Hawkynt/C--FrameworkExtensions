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
/// Interface for 3-component float color spaces (e.g., LinearRgbF, YuvF, LabF).
/// </summary>
/// <typeparam name="TSelf">The implementing type for CRTP pattern.</typeparam>
/// <remarks>
/// Components are typically normalized to 0.0-1.0 for working spaces,
/// but may have different ranges for perceptual spaces like Lab.
/// </remarks>
public interface IColorSpace3F<TSelf> : IColorSpace
  where TSelf : unmanaged, IColorSpace3F<TSelf> {

  /// <summary>Gets the first component.</summary>
  float C1 { get; }

  /// <summary>Gets the second component.</summary>
  float C2 { get; }

  /// <summary>Gets the third component.</summary>
  float C3 { get; }

#if SUPPORTS_ABSTRACT_INTERFACE_MEMBERS
  /// <summary>Creates a new instance from component values.</summary>
  static abstract TSelf Create(float c1, float c2, float c3);
#endif
}
