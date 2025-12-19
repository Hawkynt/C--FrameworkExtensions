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
/// Interface for color spaces with four primary components (e.g., CMYK) using byte values.
/// Enables generic distance calculations with zero-cost abstraction.
/// </summary>
/// <remarks>
/// Implementations should be structs with inlined property getters for JIT optimization.
/// </remarks>
public interface IFourComponentColor : IColorSpace {
  /// <summary>Deconstructs into four components plus alpha (e.g., (C,M,Y,K,A)).</summary>
  void Deconstruct(out byte C, out byte M, out byte Y, out byte K, out byte A);

#if SUPPORTS_ABSTRACT_INTERFACE_MEMBERS

  /// <summary>Creates a new instance from component values.</summary>
  static abstract IFourComponentColor Create(byte c1, byte c2, byte c3, byte c4, byte a);

#endif

}

/// <summary>
/// Interface for color spaces with four primary components (e.g., CMYK) using normalized float values.
/// Enables generic distance calculations with zero-cost abstraction.
/// </summary>
/// <remarks>
/// Implementations should be structs with inlined property getters for JIT optimization.
/// </remarks>
public interface IFourComponentFloatColor : IColorSpace {
  /// <summary>Deconstructs into four components plus alpha (e.g., (C,M,Y,K,A)).</summary>
  void Deconstruct(out float C, out float M, out float Y, out float K, out float A);

#if SUPPORTS_ABSTRACT_INTERFACE_MEMBERS

  /// <summary>Creates a new instance from component values.</summary>
  static abstract IFourComponentFloatColor Create(float c1, float c2, float c3, float c4, float a);

#endif

}
