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

namespace System.Drawing.ColorSpaces;

/// <summary>
/// Interface for color spaces with three primary components.
/// Enables generic distance calculations with zero-cost abstraction.
/// </summary>
/// <remarks>
/// Implementations should be structs with inlined property getters for JIT optimization.
/// Component values should be normalized or scaled consistently within each color space.
/// </remarks>
public interface IThreeComponentColor : IColorSpace {
  /// <summary>Gets all three components as a tuple (e.g., (R,G,B), (H,S,L), (L,a,b)).</summary>
  void Deconstruct(out byte H, out byte S, out byte L, out byte A);

#if SUPPORTS_ABSTRACT_INTERFACE_MEMBERS

  /// <summary>Creates a new instance from component values.</summary>
  static abstract IThreeComponentColor Create(byte c1, byte c2, byte c3, byte a);

#endif

}
public interface IThreeComponentFloatColor : IColorSpace {
  /// <summary>Gets all three components as a tuple (e.g., (R,G,B), (H,S,L), (L,a,b)).</summary>
  void Deconstruct(out float H, out float S, out float L, out float A);

#if SUPPORTS_ABSTRACT_INTERFACE_MEMBERS
  
  static abstract IThreeComponentFloatColor Create(float c1, float c2, float c3, float a);

#endif
}
