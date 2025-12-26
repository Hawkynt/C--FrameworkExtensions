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

#if !SUPPORTS_MATH_CBRT

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class MathPolyfills {
  extension(Math) {

  /// <summary>
  /// Returns the cube root of a specified number.
  /// </summary>
  /// <param name="x">The number whose cube root is to be found.</param>
  /// <returns>The cube root of <paramref name="x"/>.</returns>
  /// <remarks>
  /// For negative values, returns the negative of the cube root of the absolute value.
  /// This matches the IEEE 754-2008 definition.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double Cbrt(double x)
    => x switch {
      0 => x, // Preserve signed zero
      double.PositiveInfinity => double.PositiveInfinity,
      double.NegativeInfinity => double.NegativeInfinity,
      _ when double.IsNaN(x) => x,
      >= 0 => Math.Pow(x, 1.0 / 3.0),
      _ => -Math.Pow(-x, 1.0 / 3.0)
    };

  }
}

// MathF.Cbrt polyfill
#if !SUPPORTS_MATHF

public static partial class MathFPolyfills {
  extension(MathF) {

  /// <summary>
  /// Returns the cube root of a specified number.
  /// </summary>
  /// <param name="x">The number whose cube root is to be found.</param>
  /// <returns>The cube root of <paramref name="x"/>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Cbrt(float x)
    => x switch {
      0 => x, // Preserve signed zero
      float.PositiveInfinity => float.PositiveInfinity,
      float.NegativeInfinity => float.NegativeInfinity,
      _ when float.IsNaN(x) => x,
      >= 0 => (float)Math.Pow(x, 1.0 / 3.0),
      _ => -(float)Math.Pow(-x, 1.0 / 3.0)
    };

  }
}

#else

public static partial class MathFPolyfills {
  extension(MathF) {

  /// <summary>
  /// Returns the cube root of a specified number.
  /// </summary>
  /// <param name="x">The number whose cube root is to be found.</param>
  /// <returns>The cube root of <paramref name="x"/>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Cbrt(float x)
    => x switch {
      0 => x, // Preserve signed zero
      float.PositiveInfinity => float.PositiveInfinity,
      float.NegativeInfinity => float.NegativeInfinity,
      _ when float.IsNaN(x) => x,
      >= 0 => MathF.Pow(x, 1.0f / 3.0f),
      _ => -MathF.Pow(-x, 1.0f / 3.0f)
    };

  }
}

#endif

#endif
