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

// TimeSpan.Microseconds/Nanoseconds/TotalMicroseconds/TotalNanoseconds were added in .NET 7.0
#if !SUPPORTS_TIMESPAN_MICROSECONDS

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class TimeSpanPolyfills {

  /// <summary>
  /// The number of ticks per microsecond.
  /// </summary>
  private const long _TICKS_PER_MICROSECOND = 10L;

  /// <summary>
  /// The number of ticks per nanosecond (fractional, but we use integer division).
  /// </summary>
  private const double _TICKS_PER_NANOSECOND = 0.01;

  /// <summary>
  /// The number of nanoseconds per tick.
  /// </summary>
  private const int _NANOSECONDS_PER_TICK = 100;

  extension(TimeSpan @this) {

    /// <summary>
    /// Gets the microseconds component of the time interval represented by the current <see cref="TimeSpan"/> structure.
    /// </summary>
    /// <value>The microsecond component of the current <see cref="TimeSpan"/> structure. The return value ranges from -999 through 999.</value>
    public int Microseconds {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => (int)(@this.Ticks / _TICKS_PER_MICROSECOND % 1000);
    }

    /// <summary>
    /// Gets the nanoseconds component of the time interval represented by the current <see cref="TimeSpan"/> structure.
    /// </summary>
    /// <value>The nanosecond component of the current <see cref="TimeSpan"/> structure. The return value ranges from -900 through 900.</value>
    public int Nanoseconds {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => (int)(@this.Ticks % _TICKS_PER_MICROSECOND * _NANOSECONDS_PER_TICK);
    }

    /// <summary>
    /// Gets the value of the current <see cref="TimeSpan"/> structure expressed in whole and fractional microseconds.
    /// </summary>
    /// <value>The total number of microseconds represented by this instance.</value>
    public double TotalMicroseconds {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => @this.Ticks / (double)_TICKS_PER_MICROSECOND;
    }

    /// <summary>
    /// Gets the value of the current <see cref="TimeSpan"/> structure expressed in whole and fractional nanoseconds.
    /// </summary>
    /// <value>The total number of nanoseconds represented by this instance.</value>
    public double TotalNanoseconds {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => @this.Ticks * _NANOSECONDS_PER_TICK;
    }

  }

  // Note: FromMicroseconds is provided by TimeSpanMultiplyDivide feature
  // FromNanoseconds is provided here since it's a .NET 7.0 addition
  extension(TimeSpan) {

    /// <summary>
    /// Returns a <see cref="TimeSpan"/> that represents a specified number of nanoseconds.
    /// </summary>
    /// <param name="value">A number of nanoseconds.</param>
    /// <returns>An object that represents <paramref name="value"/>.</returns>
    /// <exception cref="OverflowException">
    /// <paramref name="value"/> is less than <see cref="TimeSpan.MinValue"/> or greater than <see cref="TimeSpan.MaxValue"/>.
    /// -or-
    /// <paramref name="value"/> is <see cref="double.PositiveInfinity"/>
    /// -or-
    /// <paramref name="value"/> is <see cref="double.NegativeInfinity"/>
    /// </exception>
    /// <exception cref="ArgumentException"><paramref name="value"/> is <see cref="double.NaN"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeSpan FromNanoseconds(double value) {
      if (double.IsNaN(value))
        throw new ArgumentException("TimeSpan does not accept floating point Not-a-Number values.", nameof(value));

      var ticks = value * _TICKS_PER_NANOSECOND;
      if (ticks > TimeSpan.MaxValue.Ticks || ticks < TimeSpan.MinValue.Ticks)
        throw new OverflowException("TimeSpan overflowed because the duration is too long.");

      return new((long)ticks);
    }

  }

}

#endif
