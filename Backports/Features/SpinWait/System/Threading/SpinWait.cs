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

#if !SUPPORTS_SLIM_SEMAPHORES

using System.Diagnostics;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Threading;

/// <summary>
/// Provides support for spin-based waiting.
/// </summary>
/// <remarks>
/// <see cref="SpinWait"/> encapsulates common spinning logic. On single-processor machines, yields are
/// always used instead of busy waits, and on computers with Intel processors employing Hyper-Threading
/// technology, it helps to prevent hardware thread starvation.
/// </remarks>
public struct SpinWait {

  /// <summary>
  /// The number of times that SpinOnce() has been called on this instance.
  /// </summary>
  private int _count;

  /// <summary>
  /// The number of times to spin before yielding.
  /// </summary>
  private const int _YIELD_THRESHOLD = 10;

  /// <summary>
  /// The number of times to spin before sleeping for 0ms.
  /// </summary>
  private const int _SLEEP0_EVERY_HOW_MANY_TIMES = 5;

  /// <summary>
  /// Gets the number of times <see cref="SpinOnce()"/> has been called on this instance.
  /// </summary>
  public int Count {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._count;
  }

  /// <summary>
  /// Gets whether the next call to <see cref="SpinOnce()"/> will yield the processor, triggering a
  /// forced context switch.
  /// </summary>
  /// <value><c>true</c> if the next call to <see cref="SpinOnce()"/> will yield the processor; otherwise, <c>false</c>.</value>
  public bool NextSpinWillYield {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._count >= _YIELD_THRESHOLD || Environment.ProcessorCount == 1;
  }

  /// <summary>
  /// Performs a single spin.
  /// </summary>
  /// <remarks>
  /// This is typically called in a loop, and may change in behavior based on the number of times a
  /// <see cref="SpinOnce()"/> has been called thus far on this instance.
  /// </remarks>
  public void SpinOnce() {
    if (this.NextSpinWillYield) {
      var yieldsSoFar = this._count >= _YIELD_THRESHOLD ? this._count - _YIELD_THRESHOLD : this._count;

      if (yieldsSoFar % _SLEEP0_EVERY_HOW_MANY_TIMES == _SLEEP0_EVERY_HOW_MANY_TIMES - 1)
        Thread.Sleep(0);
      else
        Thread.Sleep(1);
    } else {
      var iterations = 4 << this._count;
      Thread.SpinWait(iterations);
    }

    ++this._count;

    // Prevent overflow
    if (this._count < 0)
      this._count = _YIELD_THRESHOLD;
  }

  /// <summary>
  /// Performs a single spin with the specified sleep threshold.
  /// </summary>
  /// <param name="sleep1Threshold">
  /// The minimum number of spins before switching to a more aggressive yielding strategy that involves sleeping.
  /// A value of -1 disables the sleep1 strategy.
  /// </param>
  public void SpinOnce(int sleep1Threshold) {
    if (sleep1Threshold < -1)
      throw new ArgumentOutOfRangeException(nameof(sleep1Threshold), sleep1Threshold, "Threshold must be -1 or a non-negative value.");

    if (sleep1Threshold >= 0 && this._count >= sleep1Threshold) {
      Thread.Sleep(1);
      ++this._count;
      if (this._count < 0)
        this._count = sleep1Threshold;
      return;
    }

    this.SpinOnce();
  }

  /// <summary>
  /// Resets the spin counter.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Reset() => this._count = 0;

  /// <summary>
  /// Spins until the specified condition is satisfied.
  /// </summary>
  /// <param name="condition">A delegate to be executed over and over until it returns true.</param>
  /// <exception cref="ArgumentNullException"><paramref name="condition"/> is null.</exception>
  public static void SpinUntil(Func<bool> condition) {
    if (condition == null)
      throw new ArgumentNullException(nameof(condition));

    SpinUntil(condition, Timeout.Infinite);
  }

  /// <summary>
  /// Spins until the specified condition is satisfied or until the specified timeout is expired.
  /// </summary>
  /// <param name="condition">A delegate to be executed over and over until it returns true.</param>
  /// <param name="timeout">
  /// A <see cref="TimeSpan"/> that represents the number of milliseconds to wait,
  /// or a TimeSpan that represents -1 milliseconds to wait indefinitely.
  /// </param>
  /// <returns><c>true</c> if the condition is satisfied within the timeout; otherwise, <c>false</c>.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="condition"/> is null.</exception>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is a negative number other than -1 milliseconds.</exception>
  public static bool SpinUntil(Func<bool> condition, TimeSpan timeout) {
    var totalMilliseconds = (long)timeout.TotalMilliseconds;
    if (totalMilliseconds < -1 || totalMilliseconds > int.MaxValue)
      throw new ArgumentOutOfRangeException(nameof(timeout));

    return SpinUntil(condition, (int)totalMilliseconds);
  }

  /// <summary>
  /// Spins until the specified condition is satisfied or until the specified timeout is expired.
  /// </summary>
  /// <param name="condition">A delegate to be executed over and over until it returns true.</param>
  /// <param name="millisecondsTimeout">
  /// The number of milliseconds to wait, or <see cref="Timeout.Infinite"/> (-1) to wait indefinitely.
  /// </param>
  /// <returns><c>true</c> if the condition is satisfied within the timeout; otherwise, <c>false</c>.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="condition"/> is null.</exception>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="millisecondsTimeout"/> is a negative number other than -1.</exception>
  public static bool SpinUntil(Func<bool> condition, int millisecondsTimeout) {
    if (condition == null)
      throw new ArgumentNullException(nameof(condition));
    if (millisecondsTimeout < Timeout.Infinite)
      throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));

    if (millisecondsTimeout == 0)
      return condition();

    var startTicks = millisecondsTimeout == Timeout.Infinite ? 0 : Environment.TickCount;

    SpinWait spinner = default;
    while (!condition()) {
      if (millisecondsTimeout != Timeout.Infinite) {
        var elapsed = Environment.TickCount - startTicks;
        if (elapsed >= millisecondsTimeout)
          return false;
      }

      spinner.SpinOnce();
    }

    return true;
  }

}

#endif
