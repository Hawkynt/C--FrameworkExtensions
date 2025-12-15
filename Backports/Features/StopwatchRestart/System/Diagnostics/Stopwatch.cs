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

#if !SUPPORTS_STOPWATCH_RESTART
using Guard;
#endif

namespace System.Diagnostics;

public static partial class StopwatchPolyfills {

#if !SUPPORTS_STOPWATCH_RESTART

  /// <param name="this">This <see cref="Stopwatch" /></param>
  extension(Stopwatch @this) {
    /// <summary>
    ///   Stops time interval measurement, resets the elapsed time to zero, and starts measuring elapsed time.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Restart() {
      Against.ThisIsNull(@this);
      
      @this.Reset();
      @this.Start();
    }
  }

#endif

#if !SUPPORTS_STOPWATCH_GETELAPSEDTIME

  extension(Stopwatch) {
    /// <summary>
    /// Gets the elapsed time since the <paramref name="startingTimestamp"/> value retrieved using <see cref="Stopwatch.GetTimestamp"/>.
    /// </summary>
    /// <param name="startingTimestamp">The timestamp marking the beginning of the time period.</param>
    /// <returns>A <see cref="TimeSpan"/> representing the elapsed time between the starting timestamp and the time of this call.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeSpan GetElapsedTime(long startingTimestamp)
      => GetElapsedTime(startingTimestamp, Stopwatch.GetTimestamp());

    /// <summary>
    /// Gets the elapsed time between two timestamps retrieved using <see cref="Stopwatch.GetTimestamp"/>.
    /// </summary>
    /// <param name="startingTimestamp">The timestamp marking the beginning of the time period.</param>
    /// <param name="endingTimestamp">The timestamp marking the end of the time period.</param>
    /// <returns>A <see cref="TimeSpan"/> representing the elapsed time between the two timestamps.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp) {
      var ticksElapsed = endingTimestamp - startingTimestamp;
      return Stopwatch.IsHighResolution
        ? TimeSpan.FromTicks((long)(ticksElapsed * ((double)TimeSpan.TicksPerSecond / Stopwatch.Frequency)))
        : TimeSpan.FromTicks(ticksElapsed);
    }
  }

#endif

}
