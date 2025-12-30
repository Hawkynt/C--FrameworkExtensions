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

public static partial class MathPolyfills {
  extension(Math) {

  /// <summary>
  /// Returns the inverse hyperbolic sine of the specified number.
  /// </summary>
  /// <param name="x">The number whose inverse hyperbolic sine is to be found.</param>
  /// <returns>
  /// The inverse hyperbolic sine of <paramref name="x"/>, such that <c>sinh(result) = x</c>.
  /// </returns>
  /// <remarks>
  /// <para>This method implements <c>asinh(x) = ln(x + sqrt(x² + 1))</c>.</para>
  /// <para>Special cases:</para>
  /// <list type="bullet">
  /// <item><description><c>Asinh(NaN)</c> returns <c>NaN</c>.</description></item>
  /// <item><description><c>Asinh(+∞)</c> returns <c>+∞</c>.</description></item>
  /// <item><description><c>Asinh(-∞)</c> returns <c>-∞</c>.</description></item>
  /// <item><description><c>Asinh(0)</c> returns <c>0</c>.</description></item>
  /// </list>
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Asinh(double x)
    => x switch {
      double.NaN => double.NaN,
      double.PositiveInfinity => double.PositiveInfinity,
      double.NegativeInfinity => double.NegativeInfinity,
      0 => x, // Preserve signed zero
      _ => Math.Log(x + Math.Sqrt(x * x + 1))
    };

  /// <summary>
  /// Returns the inverse hyperbolic cosine of the specified number.
  /// </summary>
  /// <param name="x">The number whose inverse hyperbolic cosine is to be found. Must be ≥ 1.</param>
  /// <returns>
  /// The inverse hyperbolic cosine of <paramref name="x"/>, such that <c>cosh(result) = x</c>.
  /// </returns>
  /// <remarks>
  /// <para>This method implements <c>acosh(x) = ln(x + sqrt(x² - 1))</c>.</para>
  /// <para>Special cases:</para>
  /// <list type="bullet">
  /// <item><description><c>Acosh(NaN)</c> returns <c>NaN</c>.</description></item>
  /// <item><description><c>Acosh(x)</c> where x &lt; 1 returns <c>NaN</c>.</description></item>
  /// <item><description><c>Acosh(+∞)</c> returns <c>+∞</c>.</description></item>
  /// <item><description><c>Acosh(1)</c> returns <c>0</c>.</description></item>
  /// </list>
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Acosh(double x)
    => x switch {
      double.NaN or < 1 => double.NaN,
      double.PositiveInfinity => double.PositiveInfinity,
      1 => 0,
      _ => Math.Log(x + Math.Sqrt(x * x - 1))
    };

  /// <summary>
  /// Returns the inverse hyperbolic tangent of the specified number.
  /// </summary>
  /// <param name="x">The number whose inverse hyperbolic tangent is to be found. Must be in the range (-1, 1).</param>
  /// <returns>
  /// The inverse hyperbolic tangent of <paramref name="x"/>, such that <c>tanh(result) = x</c>.
  /// </returns>
  /// <remarks>
  /// <para>This method implements <c>atanh(x) = 0.5 * ln((1 + x) / (1 - x))</c>.</para>
  /// <para>Special cases:</para>
  /// <list type="bullet">
  /// <item><description><c>Atanh(NaN)</c> returns <c>NaN</c>.</description></item>
  /// <item><description><c>Atanh(x)</c> where |x| &gt; 1 returns <c>NaN</c>.</description></item>
  /// <item><description><c>Atanh(1)</c> returns <c>+∞</c>.</description></item>
  /// <item><description><c>Atanh(-1)</c> returns <c>-∞</c>.</description></item>
  /// <item><description><c>Atanh(0)</c> returns <c>0</c>.</description></item>
  /// </list>
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Atanh(double x)
    => x switch {
      double.NaN => double.NaN,
      > 1 or < -1 => double.NaN,
      1 => double.PositiveInfinity,
      -1 => double.NegativeInfinity,
      0 => x, // Preserve signed zero
      _ => 0.5 * Math.Log((1 + x) / (1 - x))
    };

  }
}

#endif
