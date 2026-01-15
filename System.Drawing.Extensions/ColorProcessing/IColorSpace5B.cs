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
/// Interface for 5-component byte color spaces with alpha (e.g., Cmyka40).
/// </summary>
/// <typeparam name="TSelf">The implementing type for CRTP pattern.</typeparam>
/// <remarks>
/// Used for CMYK+Alpha storage formats.
/// </remarks>
public interface IColorSpace5B<out TSelf> : IColorSpace5<TSelf>
  where TSelf : unmanaged, IColorSpace5B<TSelf> {

  /// <summary>Gets the first component (0-255).</summary>
  byte C1 { get; }

  /// <summary>Gets the second component (0-255).</summary>
  byte C2 { get; }

  /// <summary>Gets the third component (0-255).</summary>
  byte C3 { get; }

  /// <summary>Gets the fourth component (0-255).</summary>
  byte C4 { get; }

  /// <summary>Gets the alpha component (0-255).</summary>
  byte A { get; }

#if SUPPORTS_ABSTRACT_INTERFACE_MEMBERS
  /// <summary>Creates a new instance from component values.</summary>
  static abstract TSelf Create(byte c1, byte c2, byte c3, byte c4, byte a);
#endif
}
