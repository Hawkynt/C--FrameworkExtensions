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

#if !SUPPORTS_PARALLEL_FORASYNC

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Threading.Tasks;

/// <summary>
/// Provides async parallel methods added in .NET 6.0.
/// </summary>
/// <remarks>
/// Since <see cref="Parallel"/> is a static class, extension methods cannot be added to it.
/// Use <c>ParallelAsync.ForAsync</c> and <c>ParallelAsync.ForEachAsync</c> instead.
/// </remarks>
public static class ParallelAsyncPolyfills {

  extension(Parallel) {

    /// <summary>
    /// Executes a for loop in which iterations may run in parallel.
    /// </summary>
    /// <param name="fromInclusive">The start index, inclusive.</param>
    /// <param name="toExclusive">The end index, exclusive.</param>
    /// <param name="body">An asynchronous delegate that is invoked once per iteration.</param>
    /// <returns>A task that represents the entire for operation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task ForAsync(int fromInclusive, int toExclusive, Func<int, CancellationToken, ValueTask> body)
      => ForAsync(fromInclusive, toExclusive, new ParallelOptions(), body);

    /// <summary>
    /// Executes a for loop in which iterations may run in parallel.
    /// </summary>
    /// <param name="fromInclusive">The start index, inclusive.</param>
    /// <param name="toExclusive">The end index, exclusive.</param>
    /// <param name="parallelOptions">An object that configures the behavior of this operation.</param>
    /// <param name="body">An asynchronous delegate that is invoked once per iteration.</param>
    /// <returns>A task that represents the entire for operation.</returns>
    public static async Task ForAsync(int fromInclusive, int toExclusive, ParallelOptions parallelOptions, Func<int, CancellationToken, ValueTask> body) {
      ArgumentNullException.ThrowIfNull(body);
      ArgumentNullException.ThrowIfNull(parallelOptions);

      if (fromInclusive >= toExclusive)
        return;

      var cancellationToken = parallelOptions.CancellationToken;
      cancellationToken.ThrowIfCancellationRequested();

      var maxDegree = parallelOptions.MaxDegreeOfParallelism;
      if (maxDegree <= 0)
        maxDegree = Environment.ProcessorCount;

      using var semaphore = new SemaphoreSlim(maxDegree, maxDegree);
      var exceptions = new List<Exception>();
      var lockObj = new object();
      var tasks = new List<Task>();

      for (var i = fromInclusive; i < toExclusive; ++i) {
        cancellationToken.ThrowIfCancellationRequested();

        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        var index = i;
        var task = Task.Run(
          async () => {
            try {
              await body(index, cancellationToken).ConfigureAwait(false);
            } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
              throw;
            } catch (Exception ex) {
              lock (lockObj)
                exceptions.Add(ex);
            } finally {
              semaphore.Release();
            }
          },
          cancellationToken
        );
        tasks.Add(task);
      }

      try {
        await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
      } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
        throw;
      } catch {
        // Individual exceptions are collected
      }

      if (exceptions.Count > 0)
        throw new AggregateException(exceptions);
    }

    /// <summary>
    /// Executes a for loop with 64-bit indexes in which iterations may run in parallel.
    /// </summary>
    /// <param name="fromInclusive">The start index, inclusive.</param>
    /// <param name="toExclusive">The end index, exclusive.</param>
    /// <param name="body">An asynchronous delegate that is invoked once per iteration.</param>
    /// <returns>A task that represents the entire for operation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task ForAsync(long fromInclusive, long toExclusive, Func<long, CancellationToken, ValueTask> body)
      => ForAsync(fromInclusive, toExclusive, new ParallelOptions(), body);

    /// <summary>
    /// Executes a for loop with 64-bit indexes in which iterations may run in parallel.
    /// </summary>
    /// <param name="fromInclusive">The start index, inclusive.</param>
    /// <param name="toExclusive">The end index, exclusive.</param>
    /// <param name="parallelOptions">An object that configures the behavior of this operation.</param>
    /// <param name="body">An asynchronous delegate that is invoked once per iteration.</param>
    /// <returns>A task that represents the entire for operation.</returns>
    public static async Task ForAsync(long fromInclusive, long toExclusive, ParallelOptions parallelOptions, Func<long, CancellationToken, ValueTask> body) {
      ArgumentNullException.ThrowIfNull(body);
      ArgumentNullException.ThrowIfNull(parallelOptions);

      if (fromInclusive >= toExclusive)
        return;

      var cancellationToken = parallelOptions.CancellationToken;
      cancellationToken.ThrowIfCancellationRequested();

      var maxDegree = parallelOptions.MaxDegreeOfParallelism;
      if (maxDegree <= 0)
        maxDegree = Environment.ProcessorCount;

      using var semaphore = new SemaphoreSlim(maxDegree, maxDegree);
      var exceptions = new List<Exception>();
      var lockObj = new object();
      var tasks = new List<Task>();

      for (var i = fromInclusive; i < toExclusive; ++i) {
        cancellationToken.ThrowIfCancellationRequested();

        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        var index = i;
        var task = Task.Run(
          async () => {
            try {
              await body(index, cancellationToken).ConfigureAwait(false);
            } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
              throw;
            } catch (Exception ex) {
              lock (lockObj)
                exceptions.Add(ex);
            } finally {
              semaphore.Release();
            }
          },
          cancellationToken
        );
        tasks.Add(task);
      }

      try {
        await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
      } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
        throw;
      } catch {
        // Individual exceptions are collected
      }

      if (exceptions.Count > 0)
        throw new AggregateException(exceptions);
    }

    /// <summary>
    /// Executes a foreach operation on an <see cref="IEnumerable{T}"/> in which iterations may run in parallel.
    /// </summary>
    /// <typeparam name="TSource">The type of the data in the source.</typeparam>
    /// <param name="source">An enumerable data source.</param>
    /// <param name="body">An asynchronous delegate that is invoked once per iteration.</param>
    /// <returns>A task that represents the entire foreach operation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task ForEachAsync<TSource>(IEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask> body)
      => ForEachAsync(source, new ParallelOptions(), body);

    /// <summary>
    /// Executes a foreach operation on an <see cref="IEnumerable{T}"/> in which iterations may run in parallel.
    /// </summary>
    /// <typeparam name="TSource">The type of the data in the source.</typeparam>
    /// <param name="source">An enumerable data source.</param>
    /// <param name="parallelOptions">An object that configures the behavior of this operation.</param>
    /// <param name="body">An asynchronous delegate that is invoked once per iteration.</param>
    /// <returns>A task that represents the entire foreach operation.</returns>
    public static async Task ForEachAsync<TSource>(IEnumerable<TSource> source, ParallelOptions parallelOptions, Func<TSource, CancellationToken, ValueTask> body) {
      ArgumentNullException.ThrowIfNull(source);
      ArgumentNullException.ThrowIfNull(body);
      ArgumentNullException.ThrowIfNull(parallelOptions);

      var cancellationToken = parallelOptions.CancellationToken;
      cancellationToken.ThrowIfCancellationRequested();

      var maxDegree = parallelOptions.MaxDegreeOfParallelism;
      if (maxDegree <= 0)
        maxDegree = Environment.ProcessorCount;

      using var semaphore = new SemaphoreSlim(maxDegree, maxDegree);
      var exceptions = new List<Exception>();
      var lockObj = new object();
      var tasks = new List<Task>();

      foreach (var item in source) {
        cancellationToken.ThrowIfCancellationRequested();

        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        var localItem = item;
        var task = Task.Run(
          async () => {
            try {
              await body(localItem, cancellationToken).ConfigureAwait(false);
            } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
              throw;
            } catch (Exception ex) {
              lock (lockObj)
                exceptions.Add(ex);
            } finally {
              semaphore.Release();
            }
          },
          cancellationToken
        );
        tasks.Add(task);
      }

      try {
        await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
      } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
        throw;
      } catch {
        // Individual exceptions are collected
      }

      if (exceptions.Count > 0)
        throw new AggregateException(exceptions);
    }

    /// <summary>
    /// Executes a foreach operation on an <see cref="IAsyncEnumerable{T}"/> in which iterations may run in parallel.
    /// </summary>
    /// <typeparam name="TSource">The type of the data in the source.</typeparam>
    /// <param name="source">An async enumerable data source.</param>
    /// <param name="body">An asynchronous delegate that is invoked once per iteration.</param>
    /// <returns>A task that represents the entire foreach operation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task ForEachAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask> body)
      => ForEachAsync(source, new ParallelOptions(), body);

    /// <summary>
    /// Executes a foreach operation on an <see cref="IAsyncEnumerable{T}"/> in which iterations may run in parallel.
    /// </summary>
    /// <typeparam name="TSource">The type of the data in the source.</typeparam>
    /// <param name="source">An async enumerable data source.</param>
    /// <param name="parallelOptions">An object that configures the behavior of this operation.</param>
    /// <param name="body">An asynchronous delegate that is invoked once per iteration.</param>
    /// <returns>A task that represents the entire foreach operation.</returns>
    public static async Task ForEachAsync<TSource>(IAsyncEnumerable<TSource> source, ParallelOptions parallelOptions, Func<TSource, CancellationToken, ValueTask> body) {
      ArgumentNullException.ThrowIfNull(source);
      ArgumentNullException.ThrowIfNull(body);
      ArgumentNullException.ThrowIfNull(parallelOptions);

      var cancellationToken = parallelOptions.CancellationToken;
      cancellationToken.ThrowIfCancellationRequested();

      var maxDegree = parallelOptions.MaxDegreeOfParallelism;
      if (maxDegree <= 0)
        maxDegree = Environment.ProcessorCount;

      using var semaphore = new SemaphoreSlim(maxDegree, maxDegree);
      var exceptions = new List<Exception>();
      var lockObj = new object();
      var tasks = new List<Task>();

      await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false)) {
        cancellationToken.ThrowIfCancellationRequested();

        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        var localItem = item;
        var task = Task.Run(
          async () => {
            try {
              await body(localItem, cancellationToken).ConfigureAwait(false);
            } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
              throw;
            } catch (Exception ex) {
              lock (lockObj)
                exceptions.Add(ex);
            } finally {
              semaphore.Release();
            }
          },
          cancellationToken
        );
        tasks.Add(task);
      }

      try {
        await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
      } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
        throw;
      } catch {
        // Individual exceptions are collected
      }

      if (exceptions.Count > 0)
        throw new AggregateException(exceptions);
    }

  }
}
#endif
