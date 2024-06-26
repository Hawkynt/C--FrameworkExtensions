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

using System.Diagnostics;
using System.Threading;
using Guard;

namespace System;

public static partial class FunctionExtensions {
  /// <summary>
  ///   Tries to invoke the given delegate, retrying on exceptions.
  /// </summary>
  /// <param name="this">This Action.</param>
  /// <param name="repeatCount">The repeat count, until execution is aborted.</param>
  /// <param name="dueTime">The time to wait between executions; if any.</param>
  [DebuggerStepThrough]
  public static TResult RetryOnException<TResult>(this Func<TResult> @this, int repeatCount, TimeSpan? dueTime = null) {
    Against.ThisIsNull(@this);
    Against.CountBelowOrEqualZero(repeatCount);

    while (--repeatCount >= 0)
      try {
        return @this();
      } catch (Exception) {
        if (repeatCount > 0) {
          if (dueTime != null)
            Thread.Sleep(dueTime.Value);
        } else
          throw;
      }

    // INFO: heuristically unreachable
    return default;
  }

  /// <summary>
  ///   Tries to invoke the given delegate.
  /// </summary>
  /// <typeparam name="TResult">The type of the result.</typeparam>
  /// <param name="this">This Action.</param>
  /// <param name="result">The result.</param>
  /// <param name="repeatCount">The repeat count, until execution is aborted.</param>
  /// <returns>
  ///   <c>true</c> on success; otherwise, <c>false</c>.
  /// </returns>
  public static bool TryInvoke<TResult>(this Func<TResult> @this, out TResult result, int repeatCount = 1) {
    Against.ThisIsNull(@this);
    Against.CountBelowOrEqualZero(repeatCount);

    while (repeatCount-- > 0)
      try {
        result = @this();
        return true;
      } catch (Exception) {
        ;
      }

    result = default;
    return false;
  }

  /// <summary>
  ///   Encapsulates the method so it only returns a result every n timeslices.
  /// </summary>
  /// <typeparam name="TResult">The type of the result.</typeparam>
  /// <param name="this">This Function.</param>
  /// <param name="span">The duration for which the result is valid.</param>
  /// <param name="prolongOnAccess">
  ///   if set to <c>true</c> prolongs the values lifetime on every access; defaults to
  ///   <c>false</c>.
  /// </param>
  /// <returns></returns>
  public static Func<TResult> OnlyOnceFor<TResult>(this Func<TResult> @this, TimeSpan span, bool prolongOnAccess = false) {
    var lastResult = default(TResult);
    var resultValidUntil = 0L;

    return prolongOnAccess ? ProlongingAccessor : NormalAccessor;

    TResult NormalAccessor() {
      var now = Stopwatch.GetTimestamp();
      if (now <= resultValidUntil)
        return lastResult;

      // result no longer valid, generate a new one
      lastResult = @this();
      resultValidUntil = (long)(now + span.TotalSeconds * Stopwatch.Frequency);
      return lastResult;
    }

    TResult ProlongingAccessor() {
      var now = Stopwatch.GetTimestamp();

      // result no longer valid, generate a new one
      if (now > resultValidUntil)
        lastResult = @this();

      resultValidUntil = (long)(now + span.TotalSeconds * Stopwatch.Frequency);
      return lastResult;
    }
  }
}
