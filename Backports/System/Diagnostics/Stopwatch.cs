namespace System.Diagnostics;

// ReSharper disable once UnusedMember.Global
public static class StopwatchPolyfills {

#if !SUPPORTS_STOPWATCH_RESTART

  /// <summary>
  /// Stops time interval measurement, resets the elapsed time to zero, and starts measuring elapsed time.
  /// </summary>
  /// <param name="this">This <see cref="Stopwatch"/></param>
  public static void Restart(this Stopwatch @this) {
    if (@this == null)
      throw new NullReferenceException();
    
    @this.Reset();
    @this.Start();
  }

#endif

}

