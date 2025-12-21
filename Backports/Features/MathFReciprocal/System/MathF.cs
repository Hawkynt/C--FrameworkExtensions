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

#if !SUPPORTS_MATH_RECIPROCAL_ESTIMATE

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class MathFPolyfills {
  extension(MathF) {
    /// <summary>
    /// Returns an estimate of the reciprocal of a specified number.
    /// </summary>
    /// <param name="x">The number whose reciprocal is to be estimated.</param>
    /// <returns>An estimate of the reciprocal of <paramref name="x"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ReciprocalEstimate(float x) => 1.0f / x;

    /// <summary>
    /// Returns an estimate of the reciprocal square root of a specified number.
    /// </summary>
    /// <param name="x">The number whose reciprocal square root is to be estimated.</param>
    /// <returns>An estimate of the reciprocal square root of <paramref name="x"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ReciprocalSqrtEstimate(float x) => 1.0f / MathF.Sqrt(x);
  }
}

#endif
