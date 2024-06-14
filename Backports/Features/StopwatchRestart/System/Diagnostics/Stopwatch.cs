#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY

#endregion

#if !SUPPORTS_STOPWATCH_RESTART

namespace System.Diagnostics;

public static partial class StopwatchPolyfills {
  /// <summary>
  ///   Stops time interval measurement, resets the elapsed time to zero, and starts measuring elapsed time.
  /// </summary>
  /// <param name="this">This <see cref="Stopwatch" /></param>
  public static void Restart(this Stopwatch @this) {
    if (@this == null)
      throw new NullReferenceException();

    @this.Reset();
    @this.Start();
  }
}

#endif
