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

#if !SUPPORTS_PARALLEL

using System.Collections.Generic;

namespace System.Threading.Tasks;

/// <summary>
/// Provides support for parallel loops and regions.
/// </summary>
public static class Parallel {

  #region For

  /// <summary>
  /// Executes a for loop in which iterations may run in parallel.
  /// </summary>
  /// <param name="fromInclusive">The start index, inclusive.</param>
  /// <param name="toExclusive">The end index, exclusive.</param>
  /// <param name="body">The delegate that is invoked once per iteration.</param>
  /// <returns>A <see cref="ParallelLoopResult"/> that contains information on what portion of the loop completed.</returns>
  public static ParallelLoopResult For(int fromInclusive, int toExclusive, Action<int> body)
    => For(fromInclusive, toExclusive, new ParallelOptions(), body);

  /// <summary>
  /// Executes a for loop in which iterations may run in parallel and loop options can be configured.
  /// </summary>
  /// <param name="fromInclusive">The start index, inclusive.</param>
  /// <param name="toExclusive">The end index, exclusive.</param>
  /// <param name="parallelOptions">An object that configures the behavior of this operation.</param>
  /// <param name="body">The delegate that is invoked once per iteration.</param>
  /// <returns>A <see cref="ParallelLoopResult"/> that contains information on what portion of the loop completed.</returns>
  public static ParallelLoopResult For(int fromInclusive, int toExclusive, ParallelOptions parallelOptions, Action<int> body) {
    ArgumentNullException.ThrowIfNull(body);
    ArgumentNullException.ThrowIfNull(parallelOptions);

    return _ForImpl(fromInclusive, toExclusive, parallelOptions, (i, _) => body(i));
  }

  /// <summary>
  /// Executes a for loop in which iterations may run in parallel and the state of the loop can be monitored and manipulated.
  /// </summary>
  /// <param name="fromInclusive">The start index, inclusive.</param>
  /// <param name="toExclusive">The end index, exclusive.</param>
  /// <param name="body">The delegate that is invoked once per iteration.</param>
  /// <returns>A <see cref="ParallelLoopResult"/> that contains information on what portion of the loop completed.</returns>
  public static ParallelLoopResult For(int fromInclusive, int toExclusive, Action<int, ParallelLoopState> body)
    => For(fromInclusive, toExclusive, new ParallelOptions(), body);

  /// <summary>
  /// Executes a for loop in which iterations may run in parallel, loop options can be configured, and the state of the loop can be monitored and manipulated.
  /// </summary>
  /// <param name="fromInclusive">The start index, inclusive.</param>
  /// <param name="toExclusive">The end index, exclusive.</param>
  /// <param name="parallelOptions">An object that configures the behavior of this operation.</param>
  /// <param name="body">The delegate that is invoked once per iteration.</param>
  /// <returns>A <see cref="ParallelLoopResult"/> that contains information on what portion of the loop completed.</returns>
  public static ParallelLoopResult For(int fromInclusive, int toExclusive, ParallelOptions parallelOptions, Action<int, ParallelLoopState> body) {
    ArgumentNullException.ThrowIfNull(body);
    ArgumentNullException.ThrowIfNull(parallelOptions);

    return _ForImpl(fromInclusive, toExclusive, parallelOptions, body);
  }

  /// <summary>
  /// Executes a for loop with 64-bit indexes in which iterations may run in parallel.
  /// </summary>
  /// <param name="fromInclusive">The start index, inclusive.</param>
  /// <param name="toExclusive">The end index, exclusive.</param>
  /// <param name="body">The delegate that is invoked once per iteration.</param>
  /// <returns>A <see cref="ParallelLoopResult"/> that contains information on what portion of the loop completed.</returns>
  public static ParallelLoopResult For(long fromInclusive, long toExclusive, Action<long> body)
    => For(fromInclusive, toExclusive, new ParallelOptions(), body);

  /// <summary>
  /// Executes a for loop with 64-bit indexes in which iterations may run in parallel and loop options can be configured.
  /// </summary>
  /// <param name="fromInclusive">The start index, inclusive.</param>
  /// <param name="toExclusive">The end index, exclusive.</param>
  /// <param name="parallelOptions">An object that configures the behavior of this operation.</param>
  /// <param name="body">The delegate that is invoked once per iteration.</param>
  /// <returns>A <see cref="ParallelLoopResult"/> that contains information on what portion of the loop completed.</returns>
  public static ParallelLoopResult For(long fromInclusive, long toExclusive, ParallelOptions parallelOptions, Action<long> body) {
    ArgumentNullException.ThrowIfNull(body);
    ArgumentNullException.ThrowIfNull(parallelOptions);

    return _ForImpl(fromInclusive, toExclusive, parallelOptions, (i, _) => body(i));
  }

  /// <summary>
  /// Executes a for loop with 64-bit indexes in which iterations may run in parallel and the state of the loop can be monitored and manipulated.
  /// </summary>
  /// <param name="fromInclusive">The start index, inclusive.</param>
  /// <param name="toExclusive">The end index, exclusive.</param>
  /// <param name="body">The delegate that is invoked once per iteration.</param>
  /// <returns>A <see cref="ParallelLoopResult"/> that contains information on what portion of the loop completed.</returns>
  public static ParallelLoopResult For(long fromInclusive, long toExclusive, Action<long, ParallelLoopState> body)
    => For(fromInclusive, toExclusive, new ParallelOptions(), body);

  /// <summary>
  /// Executes a for loop with 64-bit indexes in which iterations may run in parallel, loop options can be configured, and the state of the loop can be monitored and manipulated.
  /// </summary>
  /// <param name="fromInclusive">The start index, inclusive.</param>
  /// <param name="toExclusive">The end index, exclusive.</param>
  /// <param name="parallelOptions">An object that configures the behavior of this operation.</param>
  /// <param name="body">The delegate that is invoked once per iteration.</param>
  /// <returns>A <see cref="ParallelLoopResult"/> that contains information on what portion of the loop completed.</returns>
  public static ParallelLoopResult For(long fromInclusive, long toExclusive, ParallelOptions parallelOptions, Action<long, ParallelLoopState> body) {
    ArgumentNullException.ThrowIfNull(body);
    ArgumentNullException.ThrowIfNull(parallelOptions);

    return _ForImpl(fromInclusive, toExclusive, parallelOptions, body);
  }

  private static ParallelLoopResult _ForImpl(long fromInclusive, long toExclusive, ParallelOptions options, Action<long, ParallelLoopState> body) {
    if (fromInclusive >= toExclusive)
      return new ParallelLoopResult { IsCompleted = true };

    var flags = new ParallelLoopStateFlags();
    var state = new ParallelLoopState(flags);
    var exceptions = new List<Exception>();
    var lockObj = new object();

    var count = toExclusive - fromInclusive;
    var maxDegree = options.MaxDegreeOfParallelism;
    if (maxDegree <= 0)
      maxDegree = Environment.ProcessorCount;

    var batchSize = Math.Max(1, (int)Math.Ceiling((double)count / maxDegree));
    var tasks = new List<Task>();

    for (var batchStart = fromInclusive; batchStart < toExclusive; batchStart += batchSize) {
      var localStart = batchStart;
      var localEnd = Math.Min(batchStart + batchSize, toExclusive);

      var task = new Task(
        () => {
          for (var i = localStart; i < localEnd; ++i) {
            if (flags.IsStopped || options.CancellationToken.IsCancellationRequested)
              break;

            if (flags.IsBroken && flags.LowestBreakIteration.HasValue && i >= flags.LowestBreakIteration.Value)
              break;

            try {
              body(i, state);
              if (flags.IsBroken)
                flags.SetLowestBreakIteration(i);
            } catch (Exception ex) {
              lock (lockObj)
                exceptions.Add(ex);
              flags.IsExceptional = true;
            }
          }
        },
        options.CancellationToken
      );
      tasks.Add(task);
      task.Start(options.TaskScheduler);
    }

    foreach (var task in tasks)
      try {
        task.Wait();
      } catch {
        // Exceptions are already captured
      }

    if (exceptions.Count > 0)
      throw new AggregateException(exceptions);

    options.CancellationToken.ThrowIfCancellationRequested();

    return new ParallelLoopResult {
      IsCompleted = !flags.IsStopped && !flags.IsBroken,
      LowestBreakIteration = flags.LowestBreakIteration
    };
  }

  private static ParallelLoopResult _ForImpl(int fromInclusive, int toExclusive, ParallelOptions options, Action<int, ParallelLoopState> body)
    => _ForImpl(fromInclusive, (long)toExclusive, options, (i, s) => body((int)i, s));

  #endregion

  #region ForEach

  /// <summary>
  /// Executes a foreach operation on an <see cref="IEnumerable{T}"/> in which iterations may run in parallel.
  /// </summary>
  /// <typeparam name="TSource">The type of the data in the source.</typeparam>
  /// <param name="source">An enumerable data source.</param>
  /// <param name="body">The delegate that is invoked once per iteration.</param>
  /// <returns>A <see cref="ParallelLoopResult"/> that contains information on what portion of the loop completed.</returns>
  public static ParallelLoopResult ForEach<TSource>(IEnumerable<TSource> source, Action<TSource> body)
    => ForEach(source, new ParallelOptions(), body);

  /// <summary>
  /// Executes a foreach operation on an <see cref="IEnumerable{T}"/> in which iterations may run in parallel and loop options can be configured.
  /// </summary>
  /// <typeparam name="TSource">The type of the data in the source.</typeparam>
  /// <param name="source">An enumerable data source.</param>
  /// <param name="parallelOptions">An object that configures the behavior of this operation.</param>
  /// <param name="body">The delegate that is invoked once per iteration.</param>
  /// <returns>A <see cref="ParallelLoopResult"/> that contains information on what portion of the loop completed.</returns>
  public static ParallelLoopResult ForEach<TSource>(IEnumerable<TSource> source, ParallelOptions parallelOptions, Action<TSource> body) {
    ArgumentNullException.ThrowIfNull(source);
    ArgumentNullException.ThrowIfNull(body);
    ArgumentNullException.ThrowIfNull(parallelOptions);

    return _ForEachImpl(source, parallelOptions, (item, _) => body(item));
  }

  /// <summary>
  /// Executes a foreach operation on an <see cref="IEnumerable{T}"/> in which iterations may run in parallel and the state of the loop can be monitored and manipulated.
  /// </summary>
  /// <typeparam name="TSource">The type of the data in the source.</typeparam>
  /// <param name="source">An enumerable data source.</param>
  /// <param name="body">The delegate that is invoked once per iteration.</param>
  /// <returns>A <see cref="ParallelLoopResult"/> that contains information on what portion of the loop completed.</returns>
  public static ParallelLoopResult ForEach<TSource>(IEnumerable<TSource> source, Action<TSource, ParallelLoopState> body)
    => ForEach(source, new ParallelOptions(), body);

  /// <summary>
  /// Executes a foreach operation on an <see cref="IEnumerable{T}"/> in which iterations may run in parallel, loop options can be configured, and the state of the loop can be monitored and manipulated.
  /// </summary>
  /// <typeparam name="TSource">The type of the data in the source.</typeparam>
  /// <param name="source">An enumerable data source.</param>
  /// <param name="parallelOptions">An object that configures the behavior of this operation.</param>
  /// <param name="body">The delegate that is invoked once per iteration.</param>
  /// <returns>A <see cref="ParallelLoopResult"/> that contains information on what portion of the loop completed.</returns>
  public static ParallelLoopResult ForEach<TSource>(IEnumerable<TSource> source, ParallelOptions parallelOptions, Action<TSource, ParallelLoopState> body) {
    ArgumentNullException.ThrowIfNull(source);
    ArgumentNullException.ThrowIfNull(body);
    ArgumentNullException.ThrowIfNull(parallelOptions);

    return _ForEachImpl(source, parallelOptions, body);
  }

  /// <summary>
  /// Executes a foreach operation on an <see cref="IEnumerable{T}"/> in which iterations may run in parallel and the state of the loop can be monitored and manipulated.
  /// </summary>
  /// <typeparam name="TSource">The type of the data in the source.</typeparam>
  /// <param name="source">An enumerable data source.</param>
  /// <param name="body">The delegate that is invoked once per iteration.</param>
  /// <returns>A <see cref="ParallelLoopResult"/> that contains information on what portion of the loop completed.</returns>
  public static ParallelLoopResult ForEach<TSource>(IEnumerable<TSource> source, Action<TSource, ParallelLoopState, long> body)
    => ForEach(source, new ParallelOptions(), body);

  /// <summary>
  /// Executes a foreach operation on an <see cref="IEnumerable{T}"/> in which iterations may run in parallel, loop options can be configured, and the state of the loop can be monitored and manipulated.
  /// </summary>
  /// <typeparam name="TSource">The type of the data in the source.</typeparam>
  /// <param name="source">An enumerable data source.</param>
  /// <param name="parallelOptions">An object that configures the behavior of this operation.</param>
  /// <param name="body">The delegate that is invoked once per iteration.</param>
  /// <returns>A <see cref="ParallelLoopResult"/> that contains information on what portion of the loop completed.</returns>
  public static ParallelLoopResult ForEach<TSource>(IEnumerable<TSource> source, ParallelOptions parallelOptions, Action<TSource, ParallelLoopState, long> body) {
    ArgumentNullException.ThrowIfNull(source);
    ArgumentNullException.ThrowIfNull(body);
    ArgumentNullException.ThrowIfNull(parallelOptions);

    return _ForEachWithIndexImpl(source, parallelOptions, body);
  }

  private static ParallelLoopResult _ForEachImpl<TSource>(IEnumerable<TSource> source, ParallelOptions options, Action<TSource, ParallelLoopState> body) {
    // Materialize to list for parallel access
    var items = source is IList<TSource> list ? list : new List<TSource>(source);

    if (items.Count == 0)
      return new ParallelLoopResult { IsCompleted = true };

    var flags = new ParallelLoopStateFlags();
    var state = new ParallelLoopState(flags);
    var exceptions = new List<Exception>();
    var lockObj = new object();

    var maxDegree = options.MaxDegreeOfParallelism;
    if (maxDegree <= 0)
      maxDegree = Environment.ProcessorCount;

    var batchSize = Math.Max(1, (int)Math.Ceiling((double)items.Count / maxDegree));
    var tasks = new List<Task>();

    for (var batchStart = 0; batchStart < items.Count; batchStart += batchSize) {
      var localStart = batchStart;
      var localEnd = Math.Min(batchStart + batchSize, items.Count);

      var task = new Task(
        () => {
          for (var i = localStart; i < localEnd; ++i) {
            if (flags.IsStopped || options.CancellationToken.IsCancellationRequested)
              break;

            if (flags.IsBroken && flags.LowestBreakIteration.HasValue && i >= flags.LowestBreakIteration.Value)
              break;

            try {
              body(items[i], state);
              if (flags.IsBroken)
                flags.SetLowestBreakIteration(i);
            } catch (Exception ex) {
              lock (lockObj)
                exceptions.Add(ex);
              flags.IsExceptional = true;
            }
          }
        },
        options.CancellationToken
      );
      tasks.Add(task);
      task.Start(options.TaskScheduler);
    }

    foreach (var task in tasks)
      try {
        task.Wait();
      } catch {
        // Exceptions are already captured
      }

    if (exceptions.Count > 0)
      throw new AggregateException(exceptions);

    options.CancellationToken.ThrowIfCancellationRequested();

    return new ParallelLoopResult {
      IsCompleted = !flags.IsStopped && !flags.IsBroken,
      LowestBreakIteration = flags.LowestBreakIteration
    };
  }

  private static ParallelLoopResult _ForEachWithIndexImpl<TSource>(IEnumerable<TSource> source, ParallelOptions options, Action<TSource, ParallelLoopState, long> body) {
    var items = source is IList<TSource> list ? list : new List<TSource>(source);

    if (items.Count == 0)
      return new ParallelLoopResult { IsCompleted = true };

    var flags = new ParallelLoopStateFlags();
    var state = new ParallelLoopState(flags);
    var exceptions = new List<Exception>();
    var lockObj = new object();

    var maxDegree = options.MaxDegreeOfParallelism;
    if (maxDegree <= 0)
      maxDegree = Environment.ProcessorCount;

    var batchSize = Math.Max(1, (int)Math.Ceiling((double)items.Count / maxDegree));
    var tasks = new List<Task>();

    for (var batchStart = 0; batchStart < items.Count; batchStart += batchSize) {
      var localStart = batchStart;
      var localEnd = Math.Min(batchStart + batchSize, items.Count);

      var task = new Task(
        () => {
          for (var i = localStart; i < localEnd; ++i) {
            if (flags.IsStopped || options.CancellationToken.IsCancellationRequested)
              break;

            if (flags.IsBroken && flags.LowestBreakIteration.HasValue && i >= flags.LowestBreakIteration.Value)
              break;

            try {
              body(items[i], state, i);
              if (flags.IsBroken)
                flags.SetLowestBreakIteration(i);
            } catch (Exception ex) {
              lock (lockObj)
                exceptions.Add(ex);
              flags.IsExceptional = true;
            }
          }
        },
        options.CancellationToken
      );
      tasks.Add(task);
      task.Start(options.TaskScheduler);
    }

    foreach (var task in tasks)
      try {
        task.Wait();
      } catch {
        // Exceptions are already captured
      }

    if (exceptions.Count > 0)
      throw new AggregateException(exceptions);

    options.CancellationToken.ThrowIfCancellationRequested();

    return new ParallelLoopResult {
      IsCompleted = !flags.IsStopped && !flags.IsBroken,
      LowestBreakIteration = flags.LowestBreakIteration
    };
  }

  #endregion

  #region Invoke

  /// <summary>
  /// Executes each of the provided actions, possibly in parallel.
  /// </summary>
  /// <param name="actions">An array of <see cref="Action"/> to execute.</param>
  public static void Invoke(params Action[] actions)
    => Invoke(new ParallelOptions(), actions);

  /// <summary>
  /// Executes each of the provided actions, possibly in parallel, and with options.
  /// </summary>
  /// <param name="parallelOptions">An object that configures the behavior of this operation.</param>
  /// <param name="actions">An array of <see cref="Action"/> to execute.</param>
  public static void Invoke(ParallelOptions parallelOptions, params Action[] actions) {
    ArgumentNullException.ThrowIfNull(actions);
    ArgumentNullException.ThrowIfNull(parallelOptions);

    if (actions.Length == 0)
      return;

    var exceptions = new List<Exception>();
    var lockObj = new object();
    var tasks = new Task[actions.Length];

    for (var i = 0; i < actions.Length; ++i) {
      var action = actions[i];
      if (action == null)
        throw new ArgumentException("One of the actions is null.", nameof(actions));

      tasks[i] = new Task(
        () => {
          try {
            if (!parallelOptions.CancellationToken.IsCancellationRequested)
              action();
          } catch (Exception ex) {
            lock (lockObj)
              exceptions.Add(ex);
          }
        },
        parallelOptions.CancellationToken
      );
      tasks[i].Start(parallelOptions.TaskScheduler);
    }

    foreach (var task in tasks)
      try {
        task.Wait();
      } catch {
        // Exceptions are already captured
      }

    if (exceptions.Count > 0)
      throw new AggregateException(exceptions);

    parallelOptions.CancellationToken.ThrowIfCancellationRequested();
  }

  #endregion

}

#endif
