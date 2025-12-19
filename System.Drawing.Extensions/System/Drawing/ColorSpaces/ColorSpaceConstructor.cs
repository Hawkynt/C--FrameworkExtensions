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

using System.Reflection;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Drawing.ColorSpaces;

/// <summary>
/// Provides a factory for constructing color space instances from component values.
/// Uses cached delegates to enable generic color space construction without static abstract members.
/// </summary>
/// <typeparam name="TColorSpace">The color space type to construct.</typeparam>
/// <remarks>
/// <para>
/// This factory pattern is necessary because the project targets .NET 3.5+ which does not support
/// static abstract interface members (requires .NET 7+).
/// </para>
/// <para>
/// The delegate lookup is performed once per type and cached for subsequent calls.
/// After the initial setup, calls to <see cref="Create"/> are as fast as direct method calls.
/// </para>
/// </remarks>
internal static class ColorSpaceConstructor<TColorSpace> where TColorSpace : struct, IThreeComponentColor {

  /// <summary>
  /// Constructs a new color space instance from component values.
  /// </summary>
  /// <param name="c1">First component value.</param>
  /// <param name="c2">Second component value.</param>
  /// <param name="c3">Third component value.</param>
  /// <param name="a">Alpha component value.</param>
  /// <returns>A new color space instance.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if SUPPORTS_ABSTRACT_INTERFACE_MEMBERS
  public static TColorSpace Create(byte c1, byte c2, byte c3, byte a) => (TColorSpace)TColorSpace.Create(c1, c2, c3, a);
#else
  public static TColorSpace Create(byte c1, byte c2, byte c3, byte a) => (TColorSpace)_creator(c1, c2, c3, a);

  /// <summary>
  /// Cached creator delegate for the color space type.
  /// Initialized on first access via static constructor.
  /// </summary>
  private static readonly Func<byte, byte, byte, byte, IThreeComponentColor> _creator = _CreateCreator();

  /// <summary>
  /// Creates the creator delegate by finding the static Create method via reflection.
  /// </summary>
  private static Func<byte, byte, byte, byte, IThreeComponentColor> _CreateCreator() {
    var type = typeof(TColorSpace);

    // Look for public static Create(byte, byte, byte, byte) method
    var method = type.GetMethod(
      nameof(Rgb.Create),
      BindingFlags.Public | BindingFlags.Static,
      null,
      [typeof(byte), typeof(byte), typeof(byte), typeof(byte)],
      null
    );

    if (method != null && typeof(IThreeComponentColor).IsAssignableFrom(method.ReturnType))
      return (Func<byte, byte, byte, byte, IThreeComponentColor>)Delegate.CreateDelegate(
        typeof(Func<byte, byte, byte, byte, IThreeComponentColor>), method);

    // No Create method found - throw with helpful message
    throw new InvalidOperationException(
      $"Color space type '{type.Name}' must have a public static method: " +
      $"public static IThreeComponentColor Create(byte c1, byte c2, byte c3, byte a)");
  }
#endif
}

/// <summary>
/// Provides a factory for constructing 4-component color space instances from component values.
/// </summary>
/// <typeparam name="TColorSpace">The color space type to construct (e.g., CMYK).</typeparam>
internal static class ColorSpaceConstructor4<TColorSpace> where TColorSpace : struct, IFourComponentColor {

  /// <summary>
  /// Constructs a new 4-component color space instance from component values.
  /// </summary>
  /// <param name="c1">First component value.</param>
  /// <param name="c2">Second component value.</param>
  /// <param name="c3">Third component value.</param>
  /// <param name="c4">Fourth component value.</param>
  /// <param name="a">Alpha component value.</param>
  /// <returns>A new color space instance.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if SUPPORTS_ABSTRACT_INTERFACE_MEMBERS
  public static TColorSpace Create(byte c1, byte c2, byte c3, byte c4, byte a) => (TColorSpace)TColorSpace.Create(c1, c2, c3, c4, a);
#else
  public static TColorSpace Create(byte c1, byte c2, byte c3, byte c4, byte a) => (TColorSpace)_creator(c1, c2, c3, c4, a);

  /// <summary>
  /// Cached creator delegate for the color space type.
  /// </summary>
  private static readonly Func<byte, byte, byte, byte, byte, IFourComponentColor> _creator = _CreateCreator();

  /// <summary>
  /// Creates the creator delegate by finding the static Create method via reflection.
  /// </summary>
  private static Func<byte, byte, byte, byte, byte, IFourComponentColor> _CreateCreator() {
    var type = typeof(TColorSpace);

    // Look for public static Create(byte, byte, byte, byte, byte) method
    var method = type.GetMethod(
      nameof(Cmyk.Create),
      BindingFlags.Public | BindingFlags.Static,
      null,
      [typeof(byte), typeof(byte), typeof(byte), typeof(byte), typeof(byte)],
      null
    );

    if (method != null && typeof(IFourComponentColor).IsAssignableFrom(method.ReturnType))
      return (Func<byte, byte, byte, byte, byte, IFourComponentColor>)Delegate.CreateDelegate(
        typeof(Func<byte, byte, byte, byte, byte, IFourComponentColor>), method);

    // No Create method found - throw with helpful message
    throw new InvalidOperationException(
      $"Color space type '{type.Name}' must have a public static method: " +
      $"public static IFourComponentColor Create(byte c1, byte c2, byte c3, byte c4, byte a)");
  }
#endif
}
