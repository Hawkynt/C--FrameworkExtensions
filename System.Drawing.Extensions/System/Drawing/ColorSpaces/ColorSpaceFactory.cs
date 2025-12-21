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

using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Drawing.ColorSpaces;

/// <summary>
/// Provides a factory for creating color space instances from <see cref="Color"/>.
/// Uses cached reflection to enable generic color space conversion without static abstract members.
/// </summary>
/// <typeparam name="TColorSpace">The color space type to create.</typeparam>
/// <remarks>
/// <para>
/// This factory pattern is necessary because the project targets .NET 3.5+ which does not support
/// static abstract interface members (requires .NET 7+).
/// </para>
/// <para>
/// The reflection lookup is performed once per type and cached as a delegate for subsequent calls.
/// After the initial setup, calls to <see cref="FromColor"/> are as fast as direct method calls.
/// </para>
/// </remarks>
internal static class ColorSpaceFactory<TColorSpace> where TColorSpace : struct, IColorSpace {

  /// <summary>
  /// Cached converter delegate for the color space type.
  /// Initialized on first access via static constructor.
  /// </summary>
  private static readonly Func<Color, TColorSpace> _converter = _CreateConverter();

  /// <summary>
  /// Converts a <see cref="Color"/> to the target color space.
  /// </summary>
  /// <param name="color">The color to convert.</param>
  /// <returns>The color in the target color space.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if SUPPORTS_ABSTRACT_INTERFACE_MEMBERS
  public static TColorSpace FromColor(Color color) => (TColorSpace)TColorSpace.FromColor(color);
#else
  public static TColorSpace FromColor(Color color) => _converter(color);
#endif

  /// <summary>
  /// Creates the converter delegate by finding the static FromColor method via reflection.
  /// </summary>
  private static Func<Color, TColorSpace> _CreateConverter() {
    var type = typeof(TColorSpace);

    // Look for public static FromColor(Color) method
    var method = type.GetMethod(
      nameof(Rgb.FromColor),
      BindingFlags.Public | BindingFlags.Static,
      null,
      [typeof(Color)],
      null
    );

    if (method != null) {
      // If the method returns the exact type, create a direct delegate
      if (method.ReturnType == type)
        return (Func<Color, TColorSpace>)Delegate.CreateDelegate(typeof(Func<Color, TColorSpace>), method);

      // If the method returns IColorSpace (the interface contract), build a compiled expression with cast
      if (typeof(IColorSpace).IsAssignableFrom(method.ReturnType)) {
        // Build: (Color color) => (TColorSpace)FromColor(color)
        var colorParam = Expression.Parameter(typeof(Color), "color");
        var callExpr = Expression.Call(method, colorParam);
        var convertExpr = Expression.Convert(callExpr, type);
        var lambda = Expression.Lambda<Func<Color, TColorSpace>>(convertExpr, colorParam);
        return lambda.Compile();
      }
    }

    // No FromColor method found - throw with helpful message
    throw new InvalidOperationException(
      $"Color space type '{type.Name}' must have a public static method: " +
      $"public static {type.Name} FromColor(Color color) or " +
      $"public static IColorSpace FromColor(Color color)");
  }
}
