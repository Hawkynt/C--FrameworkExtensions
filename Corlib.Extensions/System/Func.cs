#region (c)2010-2042 Hawkynt
/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software: 
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that 
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied 
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.  
    If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System.Diagnostics;
using System.Threading;

#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace System;

public static partial class FunctionExtensions {

  /// <summary>
  /// Tries to invoke the given delegate, retrying on exceptions.
  /// </summary>
  /// <param name="this">This Action.</param>
  /// <param name="repeatCount">The repeat count, until execution is aborted.</param>
  /// <param name="dueTime">The time to wait between executions; if any.</param>
  [DebuggerStepThrough]
  public static TResult RetryOnException<TResult>(this Func<TResult> @this, int repeatCount, TimeSpan? dueTime = null) {
    if (@this == null)
      throw new NullReferenceException();
    if (repeatCount <= 0)
      throw new ArgumentException("Must be > 0", nameof(repeatCount));

    while (--repeatCount >= 0) {
      try {
        return @this();
      } catch (Exception) {
        if (repeatCount > 0) {
          if (dueTime != null)
            Thread.Sleep(dueTime.Value);
        } else
          throw;
      }
    }

    // INFO: heuristically unreachable
    return default(TResult);
  }

  /// <summary>
  /// Tries to invoke the given delegate.
  /// </summary>
  /// <typeparam name="TResult">The type of the result.</typeparam>
  /// <param name="this">This Action.</param>
  /// <param name="result">The result.</param>
  /// <param name="repeatCount">The repeat count, until execution is aborted.</param>
  /// <returns>
  ///   <c>true</c> on success; otherwise, <c>false</c>.
  /// </returns>
  public static bool TryInvoke<TResult>(this Func<TResult> @this, out TResult result, int repeatCount = 1) {
#if SUPPORTS_CONTRACTS
    Contract.Requires(@this != null);
    Contract.Requires(repeatCount > 0);
#endif

    while (repeatCount-- > 0) {
      try {
        result = @this();
        return true;
      } catch (Exception) {
        ;
      }
    }
    result = default(TResult);
    return false;
  }

  /// <summary>
  /// Encapsulates the method so it only returns a result every n timeslices.
  /// </summary>
  /// <typeparam name="TResult">The type of the result.</typeparam>
  /// <param name="this">This Function.</param>
  /// <param name="span">The duration for which the result is valid.</param>
  /// <param name="prolongOnAccess">if set to <c>true</c> prolongs the values lifetime on every access; defaults to <c>false</c>.</param>
  /// <returns></returns>
  public static Func<TResult> OnlyOnceFor<TResult>(this Func<TResult> @this, TimeSpan span, bool prolongOnAccess = false) {
    var lastResult = default(TResult);
    var resultValidUntil = 0L;

    Func<TResult> wrapper;
    if (prolongOnAccess) {
      wrapper = () => {
        var now = Stopwatch.GetTimestamp();

        // result no longer valid, generate a new one
        if (now > resultValidUntil)
          lastResult = @this();

        resultValidUntil = (long)(now + span.TotalSeconds * Stopwatch.Frequency);
        return lastResult;
      };
    } else {
      wrapper = () => {
        var now = Stopwatch.GetTimestamp();
        if (now <= resultValidUntil)
          return lastResult;

        // result no longer valid, generate a new one
        lastResult = @this();
        resultValidUntil = (long)(now + span.TotalSeconds * Stopwatch.Frequency);
        return lastResult;
      };
    }
    return wrapper;
  }

}