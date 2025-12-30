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
//

#if !SUPPORTS_MATH_ASINH

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class MathFPolyfills {
  extension(MathF) {

  /// <summary>
  /// Returns the inverse hyperbolic sine of the specified number.
  /// </summary>
  /// <param name="x">The number whose inverse hyperbolic sine is to be found.</param>
  /// <returns>
  /// The inverse hyperbolic sine of <paramref name="x"/>, such that <c>sinh(result) = x</c>.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Asinh(float x) => (float)Math.Asinh(x);

  /// <summary>
  /// Returns the inverse hyperbolic cosine of the specified number.
  /// </summary>
  /// <param name="x">The number whose inverse hyperbolic cosine is to be found. Must be >= 1.</param>
  /// <returns>
  /// The inverse hyperbolic cosine of <paramref name="x"/>, such that <c>cosh(result) = x</c>.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Acosh(float x) => (float)Math.Acosh(x);

  /// <summary>
  /// Returns the inverse hyperbolic tangent of the specified number.
  /// </summary>
  /// <param name="x">The number whose inverse hyperbolic tangent is to be found. Must be in the range (-1, 1).</param>
  /// <returns>
  /// The inverse hyperbolic tangent of <paramref name="x"/>, such that <c>tanh(result) = x</c>.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Atanh(float x) => (float)Math.Atanh(x);

  }
}

#endif
