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

#if !SUPPORTS_TIMESPAN_MULTIPLY_DIVIDE

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class TimeSpanPolyfills {

  extension(TimeSpan @this) {

    /// <summary>
    /// Returns a new <see cref="TimeSpan"/> object whose value is the result of multiplying this instance by the specified factor.
    /// </summary>
    /// <param name="factor">The value to multiply by.</param>
    /// <returns>A new object that represents the value of this instance multiplied by the value of <paramref name="factor"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TimeSpan Multiply(double factor)
      => TimeSpan.FromTicks((long)(@this.Ticks * factor));

    /// <summary>
    /// Returns a new <see cref="TimeSpan"/> object whose value is the result of dividing this instance by the specified divisor.
    /// </summary>
    /// <param name="divisor">The value to divide by.</param>
    /// <returns>A new object that represents the value of this instance divided by the value of <paramref name="divisor"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TimeSpan Divide(double divisor)
      => TimeSpan.FromTicks((long)(@this.Ticks / divisor));

    /// <summary>
    /// Returns a new <see cref="double"/> value that is the result of dividing this instance by the specified <see cref="TimeSpan"/>.
    /// </summary>
    /// <param name="ts">The value to divide by.</param>
    /// <returns>A new value that represents the ratio of this instance to <paramref name="ts"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Divide(TimeSpan ts)
      => (double)@this.Ticks / ts.Ticks;

  }

  extension(TimeSpan) {

    /// <summary>
    /// Returns a <see cref="TimeSpan"/> that represents a specified number of microseconds.
    /// </summary>
    /// <param name="value">A number of microseconds.</param>
    /// <returns>An object that represents <paramref name="value"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeSpan FromMicroseconds(double value)
      => TimeSpan.FromTicks((long)(value * TimeSpan.TicksPerMillisecond / 1000));

  }

}

#endif
