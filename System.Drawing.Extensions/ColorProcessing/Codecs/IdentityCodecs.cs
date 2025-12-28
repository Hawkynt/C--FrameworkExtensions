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

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Codecs;

/// <summary>
/// Identity decoder that passes Bgra8888 through unchanged.
/// </summary>
/// <remarks>
/// Used in fast-path scaling where no color space conversion is needed.
/// </remarks>
public readonly struct IdentityDecode<TPixel> : IDecode<TPixel, TPixel> where TPixel: unmanaged, IStorageSpace {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TPixel Decode(in TPixel pixel) => pixel;
}

/// <summary>
/// Identity encoder that passes Bgra8888 through unchanged.
/// </summary>
/// <remarks>
/// Used in fast-path scaling where no color space conversion is needed.
/// </remarks>
public readonly struct IdentityEncode<TPixel> : IEncode<TPixel, TPixel> where TPixel : unmanaged, IStorageSpace {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TPixel Encode(in TPixel color) => color;
}

/// <summary>
/// Identity projector that passes Bgra8888 through unchanged.
/// </summary>
/// <remarks>
/// Used when the work color and key color are the same type.
/// </remarks>
public readonly struct IdentityProject<TPixel> : IProject<TPixel, TPixel> where TPixel : unmanaged, IStorageSpace {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TPixel Project(in TPixel color) => color;
}

/// <summary>
/// Identity projector for working color spaces.
/// </summary>
/// <remarks>
/// Used when the work color and key color are the same type and are color spaces (not storage spaces).
/// This is used for scalers that don't perform pattern matching (e.g., SaL, Lanczos).
/// </remarks>
public readonly struct IdentityProjectColor<TColor> : IProject<TColor, TColor> where TColor : unmanaged, IColorSpace {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TColor Project(in TColor color) => color;
}
