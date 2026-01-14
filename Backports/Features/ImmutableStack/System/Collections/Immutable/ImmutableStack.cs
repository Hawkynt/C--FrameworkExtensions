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
using System.Linq;

namespace System.Collections.Immutable;

#region interfaces

/// <summary>
/// Represents an immutable last-in, first-out (LIFO) collection.
/// </summary>
/// <typeparam name="T">The type of elements in the stack.</typeparam>
public interface IImmutableStack<T> : IEnumerable<T> {
  /// <summary>
  /// Gets a value indicating whether this stack is empty.
  /// </summary>
  bool IsEmpty { get; }

  /// <summary>
  /// Removes all elements from the stack.
  /// </summary>
  IImmutableStack<T> Clear();

  /// <summary>
  /// Returns the element at the top of the stack without removing it.
  /// </summary>
  T Peek();

  /// <summary>
  /// Removes the element at the top of the stack and returns the new stack.
  /// </summary>
  IImmutableStack<T> Pop();

  /// <summary>
  /// Pushes an element onto the stack.
  /// </summary>
  IImmutableStack<T> Push(T value);
}

#endregion

/// <summary>
/// Represents an immutable stack.
/// </summary>
/// <typeparam name="T">The type of elements in the stack.</typeparam>
public sealed class ImmutableStack<T> : IImmutableStack<T> {
  private readonly T _head = default!;
  private readonly ImmutableStack<T>? _tail;

  /// <summary>
  /// Gets an empty immutable stack.
  /// </summary>
  public static readonly ImmutableStack<T> Empty = new();

  private ImmutableStack() => this.IsEmpty = true;

  private ImmutableStack(T head, ImmutableStack<T> tail) {
    this._head = head;
    this._tail = tail;
    this.IsEmpty = false;
  }

  /// <inheritdoc />
  public bool IsEmpty { get; }

  /// <inheritdoc />
  public T Peek() => this.IsEmpty ? throw new InvalidOperationException("Stack is empty.") : this._head;

  /// <summary>
  /// Gets the element at the top of the stack, or the default value if the stack is empty.
  /// </summary>
  public T? PeekOrDefault() => this.IsEmpty ? default : this._head;

  /// <summary>
  /// Pushes an element onto the stack.
  /// </summary>
  /// <param name="value">The element to push.</param>
  /// <returns>A new stack with the element pushed.</returns>
  public ImmutableStack<T> Push(T value) => new(value, this);

  /// <summary>
  /// Removes the element at the top of the stack and returns the new stack.
  /// </summary>
  /// <returns>A new stack with the top element removed.</returns>
  public ImmutableStack<T> Pop() => this.IsEmpty ? throw new InvalidOperationException("Stack is empty.") : this._tail!;

  /// <summary>
  /// Removes the element at the top of the stack and returns the value.
  /// </summary>
  public ImmutableStack<T> Pop(out T value) {
    if (this.IsEmpty)
      throw new InvalidOperationException("Stack is empty.");

    value = this._head;
    return this._tail!;
  }

  /// <summary>
  /// Removes all elements from the stack.
  /// </summary>
  /// <returns>An empty stack.</returns>
  public ImmutableStack<T> Clear() => Empty;

  #region IImmutableStack explicit implementation

  IImmutableStack<T> IImmutableStack<T>.Push(T value) => this.Push(value);

  IImmutableStack<T> IImmutableStack<T>.Pop() => this.Pop();

  IImmutableStack<T> IImmutableStack<T>.Clear() => this.Clear();

  #endregion

  /// <summary>
  /// Returns an enumerator that iterates through the stack.
  /// </summary>
  public Enumerator GetEnumerator() => new(this);

  IEnumerator<T> IEnumerable<T>.GetEnumerator() => new EnumeratorClass(this);

  IEnumerator IEnumerable.GetEnumerator() => new EnumeratorClass(this);

  /// <summary>
  /// Enumerates the contents of the immutable stack.
  /// </summary>
  public struct Enumerator {
    private ImmutableStack<T>? _current;
    private readonly ImmutableStack<T> _original;

    internal Enumerator(ImmutableStack<T> stack) {
      this._original = stack;
      this._current = null;
    }

    /// <summary>
    /// Gets the current element.
    /// </summary>
    public T Current => this._current == null ? default! : this._current._head;

    /// <summary>
    /// Advances the enumerator to the next element.
    /// </summary>
    public bool MoveNext() {
      this._current = this._current == null ? this._original : this._current._tail;
      return this._current is { IsEmpty: false };
    }
  }

  private sealed class EnumeratorClass : IEnumerator<T> {
    private ImmutableStack<T>? _current;
    private readonly ImmutableStack<T> _original;

    internal EnumeratorClass(ImmutableStack<T> stack) {
      this._original = stack;
      this._current = null;
    }

    public T Current => this._current == null ? default! : this._current._head;

    object? IEnumerator.Current => this.Current;

    public bool MoveNext() {
      this._current = this._current == null ? this._original : this._current._tail;
      return this._current is { IsEmpty: false };
    }

    public void Reset() => this._current = null;

    public void Dispose() { }
  }
}

/// <summary>
/// Provides a set of static methods for creating immutable stacks.
/// </summary>
public static class ImmutableStack {
  /// <summary>
  /// Creates an empty immutable stack.
  /// </summary>
  public static ImmutableStack<T> Create<T>() =>
    ImmutableStack<T>.Empty;

  /// <summary>
  /// Creates an immutable stack with the specified item.
  /// </summary>
  public static ImmutableStack<T> Create<T>(T item) =>
    ImmutableStack<T>.Empty.Push(item);

  /// <summary>
  /// Creates an immutable stack with the specified items.
  /// </summary>
  public static ImmutableStack<T> Create<T>(params T[] items) {
    var stack = ImmutableStack<T>.Empty;
    foreach (var item in items)
      stack = stack.Push(item);
    return stack;
  }

  /// <summary>
  /// Creates an immutable stack from the specified items.
  /// </summary>
  public static ImmutableStack<T> CreateRange<T>(IEnumerable<T> items) {
    var stack = ImmutableStack<T>.Empty;
    foreach (var item in items)
      stack = stack.Push(item);
    return stack;
  }

  /// <summary>
  /// Enumerates a sequence and produces an immutable stack of its contents.
  /// </summary>
  public static ImmutableStack<T> ToImmutableStack<T>(this IEnumerable<T> source) =>
    CreateRange(source);

  /// <summary>
  /// Removes the element at the top of the stack and returns the value along with the new stack.
  /// </summary>
  public static IImmutableStack<T> Pop<T>(this IImmutableStack<T> stack, out T value) {
    value = stack.Peek();
    return stack.Pop();
  }
}

#endif
