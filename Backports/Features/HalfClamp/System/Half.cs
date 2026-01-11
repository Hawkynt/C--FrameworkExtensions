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

// Half.Clamp was added in .NET 7.0
// When Half exists (SUPPORTS_HALF) but Clamp doesn't (!SUPPORTS_HALF_CLAMP), we provide the extension
#if !SUPPORTS_HALF_CLAMP

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class HalfPolyfills {

  extension(Half) {

    /// <summary>
    /// Returns value clamped to the inclusive range of min and max.
    /// </summary>
    /// <param name="value">The value to be clamped.</param>
    /// <param name="min">The lower bound of the result.</param>
    /// <param name="max">The upper bound of the result.</param>
    /// <returns>
    /// <paramref name="value"/> if <paramref name="min"/> ≤ <paramref name="value"/> ≤ <paramref name="max"/>.
    /// -or- <paramref name="min"/> if <paramref name="value"/> &lt; <paramref name="min"/>.
    /// -or- <paramref name="max"/> if <paramref name="max"/> &lt; <paramref name="value"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Half Clamp(Half value, Half min, Half max) {
      if (min > max)
        throw new ArgumentException($"'{min}' cannot be greater than {max}.");
      return value < min ? min : value > max ? max : value;
    }

  }

}

#endif
