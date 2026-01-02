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

using Hawkynt.ColorProcessing.Metrics;

namespace Hawkynt.ColorProcessing;

/// <summary>
/// Interface for 3-component byte color spaces (e.g., Rgb24).
/// </summary>
/// <typeparam name="TSelf">The implementing type for CRTP pattern.</typeparam>
public interface IColorSpace3B<TSelf> : IColorSpace
  where TSelf : unmanaged, IColorSpace3B<TSelf> {

  /// <summary>Gets the first component (0-255).</summary>
  byte C1 { get; }

  /// <summary>Gets the second component (0-255).</summary>
  byte C2 { get; }

  /// <summary>Gets the third component (0-255).</summary>
  byte C3 { get; }

  /// <summary>Returns components normalized to [0.0, 1.0].</summary>
  (UNorm32 C1, UNorm32 C2, UNorm32 C3) ToNormalized();

#if SUPPORTS_ABSTRACT_INTERFACE_MEMBERS
  /// <summary>Creates a new instance from component values.</summary>
  static abstract TSelf Create(byte c1, byte c2, byte c3);

  /// <summary>Creates from normalized values.</summary>
  static abstract TSelf FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3);
#endif
}
