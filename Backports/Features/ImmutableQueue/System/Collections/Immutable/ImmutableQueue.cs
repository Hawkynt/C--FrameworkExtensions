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
// See LICENSE file for more details.

#endregion

#if !OFFICIAL_IMMUTABLE_COLLECTIONS

using System.Collections.Generic;

namespace System.Collections.Immutable;

#region interfaces

/// <summary>
/// Represents an immutable first-in, first-out (FIFO) collection.
/// </summary>
/// <typeparam name="T">The type of elements in the queue.</typeparam>
public interface IImmutableQueue<T> : IEnumerable<T> {
  /// <summary>
  /// Gets a value indicating whether this queue is empty.
  /// </summary>
  bool IsEmpty { get; }

  /// <summary>
  /// Removes all elements from the queue.
  /// </summary>
  IImmutableQueue<T> Clear();

  /// <summary>
  /// Removes the element at the beginning of the queue and returns the new queue.
  /// </summary>
  IImmutableQueue<T> Dequeue();

  /// <summary>
  /// Adds an element to the end of the queue.
  /// </summary>
  IImmutableQueue<T> Enqueue(T value);

  /// <summary>
  /// Returns the element at the beginning of the queue without removing it.
  /// </summary>
  T Peek();
}

#endregion

/// <summary>
/// Represents an immutable queue.
/// </summary>
/// <typeparam name="T">The type of elements in the queue.</typeparam>
public sealed class ImmutableQueue<T> : IImmutableQueue<T> {
  private readonly ImmutableStack<T> _backwards;
  private readonly ImmutableStack<T> _forwards;
  private ImmutableStack<T>? _backwardsReversed;

  /// <summary>
  /// Gets an empty immutable queue.
  /// </summary>
  public static readonly ImmutableQueue<T> Empty = new(ImmutableStack<T>.Empty, ImmutableStack<T>.Empty);

  private ImmutableQueue(ImmutableStack<T> forwards, ImmutableStack<T> backwards) {
    this._forwards = forwards;
    this._backwards = backwards;
  }

  /// <inheritdoc />
  public bool IsEmpty => this._forwards.IsEmpty && this._backwards.IsEmpty;

  private ImmutableStack<T> BackwardsReversed {
    get {
      if (this._backwardsReversed != null)
        return this._backwardsReversed;

      var reversed = ImmutableStack<T>.Empty;
      var current = this._backwards;
      while (!current.IsEmpty) {
        reversed = reversed.Push(current.Peek());
        current = current.Pop();
      }
      this._backwardsReversed = reversed;
      return this._backwardsReversed;
    }
  }

  /// <inheritdoc />
  public T Peek() {
    if (this.IsEmpty)
      throw new InvalidOperationException("Queue is empty.");

    return this._forwards.IsEmpty ? this.BackwardsReversed.Peek() : this._forwards.Peek();
  }

  /// <summary>
  /// Gets the element at the beginning of the queue, or the default value if the queue is empty.
  /// </summary>
  public T? PeekOrDefault() {
    if (this.IsEmpty)
      return default;

    return this._forwards.IsEmpty ? this.BackwardsReversed.Peek() : this._forwards.Peek();
  }

  /// <summary>
  /// Adds an element to the end of the queue.
  /// </summary>
  /// <param name="value">The element to add.</param>
  /// <returns>A new queue with the element added.</returns>
  public ImmutableQueue<T> Enqueue(T value) => new(this._forwards, this._backwards.Push(value));

  /// <summary>
  /// Removes the element at the beginning of the queue and returns the new queue.
  /// </summary>
  /// <returns>A new queue with the first element removed.</returns>
  public ImmutableQueue<T> Dequeue() {
    if (this.IsEmpty)
      throw new InvalidOperationException("Queue is empty.");

    var newForwards = this._forwards;
    var newBackwards = this._backwards;

    if (newForwards.IsEmpty) {
      newForwards = this.BackwardsReversed;
      newBackwards = ImmutableStack<T>.Empty;
    }

    newForwards = newForwards.Pop();
    return new(newForwards, newBackwards);
  }

  /// <summary>
  /// Removes the element at the beginning of the queue and returns the value.
  /// </summary>
  public ImmutableQueue<T> Dequeue(out T value) {
    value = this.Peek();
    return this.Dequeue();
  }

  /// <summary>
  /// Removes all elements from the queue.
  /// </summary>
  /// <returns>An empty queue.</returns>
  public ImmutableQueue<T> Clear() => Empty;

  #region IImmutableQueue explicit implementation

  IImmutableQueue<T> IImmutableQueue<T>.Enqueue(T value) => this.Enqueue(value);

  IImmutableQueue<T> IImmutableQueue<T>.Dequeue() => this.Dequeue();

  IImmutableQueue<T> IImmutableQueue<T>.Clear() => this.Clear();

  #endregion

  /// <summary>
  /// Returns an enumerator that iterates through the queue.
  /// </summary>
  public Enumerator GetEnumerator() => new(this);

  IEnumerator<T> IEnumerable<T>.GetEnumerator() => new EnumeratorClass(this);

  IEnumerator IEnumerable.GetEnumerator() => new EnumeratorClass(this);

  /// <summary>
  /// Enumerates the contents of the immutable queue.
  /// </summary>
  public struct Enumerator {
    private readonly ImmutableQueue<T> _queue;
    private ImmutableStack<T>? _forwards;
    private ImmutableStack<T>? _backwards;
    private bool _started;

    internal Enumerator(ImmutableQueue<T> queue) {
      this._queue = queue;
      this._forwards = null;
      this._backwards = null;
      this._started = false;
    }

    /// <summary>
    /// Gets the current element.
    /// </summary>
    public T Current {
      get {
        if (!this._started || (this._forwards?.IsEmpty != false && this._backwards?.IsEmpty != false))
          return default!;

        return this._forwards is { IsEmpty: false } fwd ? fwd.Peek() : this._backwards!.Peek();
      }
    }

    /// <summary>
    /// Advances the enumerator to the next element.
    /// </summary>
    public bool MoveNext() {
      if (!this._started) {
        this._started = true;
        this._forwards = this._queue._forwards;
        this._backwards = this._queue.BackwardsReversed;
        return !this._forwards!.IsEmpty || !this._backwards!.IsEmpty;
      }

      if (!this._forwards!.IsEmpty) {
        this._forwards = this._forwards.Pop();
        return !this._forwards.IsEmpty || !this._backwards!.IsEmpty;
      }

      if (!this._backwards!.IsEmpty) {
        this._backwards = this._backwards.Pop();
        return !this._backwards.IsEmpty;
      }

      return false;
    }
  }

  private sealed class EnumeratorClass : IEnumerator<T> {
    private readonly ImmutableQueue<T> _queue;
    private ImmutableStack<T>? _forwards;
    private ImmutableStack<T>? _backwards;
    private bool _started;

    internal EnumeratorClass(ImmutableQueue<T> queue) {
      this._queue = queue;
      this._forwards = null;
      this._backwards = null;
      this._started = false;
    }

    public T Current {
      get {
        if (!this._started)
          return default!;
        if (this._forwards is { IsEmpty: false })
          return this._forwards.Peek();
        if (this._backwards is { IsEmpty: false })
          return this._backwards.Peek();
        return default!;
      }
    }

    object? IEnumerator.Current => this.Current;

    public bool MoveNext() {
      if (!this._started) {
        this._started = true;
        this._forwards = this._queue._forwards;
        this._backwards = this._queue.BackwardsReversed;
        return !this._forwards!.IsEmpty || !this._backwards!.IsEmpty;
      }

      if (!this._forwards!.IsEmpty) {
        this._forwards = this._forwards.Pop();
        return !this._forwards.IsEmpty || !this._backwards!.IsEmpty;
      }

      if (!this._backwards!.IsEmpty) {
        this._backwards = this._backwards.Pop();
        return !this._backwards.IsEmpty;
      }

      return false;
    }

    public void Reset() {
      this._started = false;
      this._forwards = null;
      this._backwards = null;
    }

    public void Dispose() { }
  }
}

/// <summary>
/// Provides a set of static methods for creating immutable queues.
/// </summary>
public static class ImmutableQueue {
  /// <summary>
  /// Creates an empty immutable queue.
  /// </summary>
  public static ImmutableQueue<T> Create<T>() =>
    ImmutableQueue<T>.Empty;

  /// <summary>
  /// Creates an immutable queue with the specified item.
  /// </summary>
  public static ImmutableQueue<T> Create<T>(T item) =>
    ImmutableQueue<T>.Empty.Enqueue(item);

  /// <summary>
  /// Creates an immutable queue with the specified items.
  /// </summary>
  public static ImmutableQueue<T> Create<T>(params T[] items) {
    var queue = ImmutableQueue<T>.Empty;
    foreach (var item in items)
      queue = queue.Enqueue(item);
    return queue;
  }

  /// <summary>
  /// Creates an immutable queue from the specified items.
  /// </summary>
  public static ImmutableQueue<T> CreateRange<T>(IEnumerable<T> items) {
    var queue = ImmutableQueue<T>.Empty;
    foreach (var item in items)
      queue = queue.Enqueue(item);
    return queue;
  }

  /// <summary>
  /// Enumerates a sequence and produces an immutable queue of its contents.
  /// </summary>
  public static ImmutableQueue<T> ToImmutableQueue<T>(this IEnumerable<T> source) =>
    CreateRange(source);

  /// <summary>
  /// Removes the element at the beginning of the queue and returns the value along with the new queue.
  /// </summary>
  public static IImmutableQueue<T> Dequeue<T>(this IImmutableQueue<T> queue, out T value) {
    value = queue.Peek();
    return queue.Dequeue();
  }
}

#endif
