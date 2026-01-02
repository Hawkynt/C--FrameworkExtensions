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

#if !SUPPORTS_CONCURRENT_COLLECTIONS

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Collections.Concurrent;

/// <summary>
/// Represents a thread-safe first in-first out (FIFO) collection.
/// </summary>
/// <typeparam name="T">The type of elements contained in the queue.</typeparam>
/// <remarks>
/// <para>
/// This implementation uses the lock-free Michael-Scott queue algorithm for optimal performance
/// under contention. All public and protected members of <see cref="ConcurrentQueue{T}"/>
/// are thread-safe and may be used concurrently from multiple threads.
/// </para>
/// </remarks>
[DebuggerDisplay("Count = {Count}")]
public class ConcurrentQueue<T> : IProducerConsumerCollection<T>, IReadOnlyCollection<T> {

  /// <summary>
  /// A node in the lock-free queue.
  /// </summary>
  private sealed class Node {
    public T Value;
    public volatile Node? Next;

    public Node(T value) => this.Value = value;

    // Constructor for sentinel node
    public Node() => this.Value = default!;
  }

  /// <summary>
  /// The head of the queue (sentinel node - actual head is head.Next).
  /// </summary>
  private volatile Node _head;

  /// <summary>
  /// The tail of the queue.
  /// </summary>
  private volatile Node _tail;

  /// <summary>
  /// Initializes a new instance of the <see cref="ConcurrentQueue{T}"/> class.
  /// </summary>
  public ConcurrentQueue() {
    // Initialize with a sentinel node
    var sentinel = new Node();
    this._head = sentinel;
    this._tail = sentinel;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="ConcurrentQueue{T}"/> class
  /// that contains elements copied from the specified collection.
  /// </summary>
  /// <param name="collection">The collection whose elements are copied to the new <see cref="ConcurrentQueue{T}"/>.</param>
  /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
  public ConcurrentQueue(IEnumerable<T> collection) : this() {
    if (collection == null)
      throw new ArgumentNullException(nameof(collection));

    foreach (var item in collection)
      this.Enqueue(item);
  }

  /// <summary>
  /// Gets a value that indicates whether the <see cref="ConcurrentQueue{T}"/> is empty.
  /// </summary>
  /// <value>
  /// <c>true</c> if the <see cref="ConcurrentQueue{T}"/> is empty; otherwise, <c>false</c>.
  /// </value>
  public bool IsEmpty {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._head.Next == null;
  }

  /// <summary>
  /// Gets the number of elements contained in the <see cref="ConcurrentQueue{T}"/>.
  /// </summary>
  /// <value>The number of elements contained in the <see cref="ConcurrentQueue{T}"/>.</value>
  /// <remarks>
  /// For determining whether the collection contains any items, use the <see cref="IsEmpty"/>
  /// property instead of retrieving the number of items from the <see cref="Count"/> property
  /// and comparing it to 0.
  /// </remarks>
  public int Count {
    get {
      var count = 0;
      // Skip the sentinel node, start counting from head.Next
      for (var node = this._head.Next; node != null; node = node.Next)
        ++count;
      return count;
    }
  }

  /// <summary>
  /// Adds an object to the end of the <see cref="ConcurrentQueue{T}"/>.
  /// </summary>
  /// <param name="item">The object to add to the end of the <see cref="ConcurrentQueue{T}"/>.</param>
  public void Enqueue(T item) {
    var newNode = new Node(item);
    var spinner = new SpinWait();

    do {
      var tail = this._tail;
      var next = tail.Next;

      // Is tail still the actual tail?
      if (tail == this._tail) {
        if (next == null) {
          // Try to link the new node at the end
          if (Interlocked.CompareExchange(ref tail.Next, newNode, null) == null) {
            // Enqueue succeeded, try to advance tail (OK if this fails, another thread will do it)
            Interlocked.CompareExchange(ref this._tail, newNode, tail);
            return;
          }
        } else {
          // Tail is lagging behind, try to advance it
          Interlocked.CompareExchange(ref this._tail, next, tail);
        }
      }

      spinner.SpinOnce();
    } while (true);
  }

  /// <summary>
  /// Attempts to remove and return the object at the beginning of the <see cref="ConcurrentQueue{T}"/>.
  /// </summary>
  /// <param name="result">
  /// When this method returns, if the operation was successful, <paramref name="result"/> contains the
  /// object removed. If no object was available to be removed, the value is unspecified.
  /// </param>
  /// <returns>
  /// <c>true</c> if an element was removed and returned from the beginning of the <see cref="ConcurrentQueue{T}"/>
  /// successfully; otherwise, <c>false</c>.
  /// </returns>
  public bool TryDequeue([MaybeNullWhen(false)] out T result) {
    var spinner = new SpinWait();

    do {
      var head = this._head;
      var tail = this._tail;
      var next = head.Next;

      // Is head still valid?
      if (head == this._head) {
        // Is queue empty or tail lagging?
        if (head == tail) {
          if (next == null) {
            // Queue is empty
            result = default;
            return false;
          }
          // Tail is lagging, try to advance it
          Interlocked.CompareExchange(ref this._tail, next, tail);
        } else {
          // Read value before CAS, otherwise another dequeue might free the node
          if (next != null) {
            result = next.Value;
            // Try to advance head
            if (Interlocked.CompareExchange(ref this._head, next, head) == head) {
              // Clear the value in the new sentinel to help GC
              next.Value = default!;
              return true;
            }
          }
        }
      }

      spinner.SpinOnce();
    } while (true);
  }

  /// <summary>
  /// Attempts to return an object from the beginning of the <see cref="ConcurrentQueue{T}"/>
  /// without removing it.
  /// </summary>
  /// <param name="result">
  /// When this method returns, <paramref name="result"/> contains an object from the beginning of the
  /// <see cref="ConcurrentQueue{T}"/> or an unspecified value if the operation failed.
  /// </param>
  /// <returns>
  /// <c>true</c> if an object was returned successfully; otherwise, <c>false</c>.
  /// </returns>
  public bool TryPeek([MaybeNullWhen(false)] out T result) {
    var spinner = new SpinWait();

    do {
      var head = this._head;
      var tail = this._tail;
      var next = head.Next;

      // Is head still valid?
      if (head == this._head) {
        if (head == tail) {
          if (next == null) {
            // Queue is empty
            result = default;
            return false;
          }
          // Tail is lagging, try to advance it
          Interlocked.CompareExchange(ref this._tail, next, tail);
        } else {
          if (next != null) {
            result = next.Value;
            return true;
          }
        }
      }

      spinner.SpinOnce();
    } while (true);
  }

  /// <summary>
  /// Removes all objects from the <see cref="ConcurrentQueue{T}"/>.
  /// </summary>
  public void Clear() {
    // Atomically replace with a new empty queue structure
    var spinner = new SpinWait();

    do {
      var head = this._head;
      var tail = this._tail;

      // If already empty, we're done
      if (head.Next == null)
        return;

      // Create a new sentinel
      var newSentinel = new Node();

      // Try to set the new head
      if (Interlocked.CompareExchange(ref this._head, newSentinel, head) == head) {
        // Update tail to point to the new sentinel
        // Loop until successful since other threads may be trying to enqueue
        var spinnerInner = new SpinWait();
        do {
          var currentTail = this._tail;
          if (Interlocked.CompareExchange(ref this._tail, newSentinel, currentTail) == currentTail)
            return;
          // Check if another thread already cleared and reset
          if (this._head == this._tail)
            return;
          spinnerInner.SpinOnce();
        } while (true);
      }

      spinner.SpinOnce();
    } while (true);
  }

  /// <summary>
  /// Copies the items stored in the <see cref="ConcurrentQueue{T}"/> to a new array.
  /// </summary>
  /// <returns>A new array containing a snapshot of elements copied from the <see cref="ConcurrentQueue{T}"/>.</returns>
  public T[] ToArray() {
    // Get a snapshot starting from head.Next (skip sentinel)
    var next = this._head.Next;
    if (next == null)
      return [];

    var items = new List<T>();
    for (var node = next; node != null; node = node.Next)
      items.Add(node.Value);

    return items.ToArray();
  }

  /// <summary>
  /// Copies the <see cref="ConcurrentQueue{T}"/> elements to an existing one-dimensional
  /// <see cref="Array"/>, starting at the specified array index.
  /// </summary>
  /// <param name="array">
  /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from
  /// the <see cref="ConcurrentQueue{T}"/>. The <see cref="Array"/> must have zero-based indexing.
  /// </param>
  /// <param name="index">The zero-based index in <paramref name="array"/> at which copying begins.</param>
  /// <exception cref="ArgumentNullException"><paramref name="array"/> is null.</exception>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero.</exception>
  /// <exception cref="ArgumentException">
  /// <paramref name="index"/> is equal to or greater than the length of the <paramref name="array"/>,
  /// or the number of elements in the source <see cref="ConcurrentQueue{T}"/> is greater than the
  /// available space from <paramref name="index"/> to the end of the destination <paramref name="array"/>.
  /// </exception>
  public void CopyTo(T[] array, int index) {
    if (array == null)
      throw new ArgumentNullException(nameof(array));
    if (index < 0)
      throw new ArgumentOutOfRangeException(nameof(index), index, "Value must be non-negative.");

    var items = this.ToArray();
    if (index > array.Length - items.Length)
      throw new ArgumentException("The destination array is not large enough.");

    Array.Copy(items, 0, array, index, items.Length);
  }

  /// <summary>
  /// Returns an enumerator that iterates through the <see cref="ConcurrentQueue{T}"/>.
  /// </summary>
  /// <returns>An enumerator for the <see cref="ConcurrentQueue{T}"/>.</returns>
  /// <remarks>
  /// The enumeration represents a moment-in-time snapshot of the contents of the queue.
  /// It does not reflect any updates to the collection after <see cref="GetEnumerator"/>
  /// was called. The enumerator is safe to use concurrently with reads from and writes
  /// to the queue.
  /// </remarks>
  public IEnumerator<T> GetEnumerator() {
    // Skip the sentinel node
    for (var node = this._head.Next; node != null; node = node.Next)
      yield return node.Value;
  }

  /// <summary>
  /// Returns an enumerator that iterates through a collection.
  /// </summary>
  /// <returns>An <see cref="IEnumerator"/> that can be used to iterate through the collection.</returns>
  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  #region IProducerConsumerCollection<T> Implementation

  /// <summary>
  /// Attempts to add an object to the <see cref="ConcurrentQueue{T}"/>.
  /// </summary>
  /// <param name="item">The object to add to the <see cref="ConcurrentQueue{T}"/>.</param>
  /// <returns><c>true</c> if the object was added successfully; otherwise, <c>false</c>.</returns>
  bool IProducerConsumerCollection<T>.TryAdd(T item) {
    this.Enqueue(item);
    return true;
  }

  /// <summary>
  /// Attempts to remove and return an object from the <see cref="ConcurrentQueue{T}"/>.
  /// </summary>
  /// <param name="item">
  /// When this method returns, if the object was removed and returned successfully, <paramref name="item"/>
  /// contains the removed object. If no object was available to be removed, the value is unspecified.
  /// </param>
  /// <returns><c>true</c> if an object was removed and returned successfully; otherwise, <c>false</c>.</returns>
  bool IProducerConsumerCollection<T>.TryTake([MaybeNullWhen(false)] out T item) => this.TryDequeue(out item);

  /// <summary>
  /// Copies the elements of the <see cref="ICollection"/> to an <see cref="Array"/>,
  /// starting at a particular <see cref="Array"/> index.
  /// </summary>
  /// <param name="array">
  /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from
  /// <see cref="ConcurrentQueue{T}"/>. The <see cref="Array"/> must have zero-based indexing.
  /// </param>
  /// <param name="index">The zero-based index in <paramref name="array"/> at which copying begins.</param>
  /// <exception cref="ArgumentNullException"><paramref name="array"/> is null.</exception>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero.</exception>
  /// <exception cref="ArgumentException">
  /// <paramref name="array"/> is multidimensional, or <paramref name="index"/> is equal to or greater
  /// than the length of the <paramref name="array"/>, or the number of elements in the source
  /// <see cref="ConcurrentQueue{T}"/> is greater than the available space from <paramref name="index"/>
  /// to the end of the destination <paramref name="array"/>.
  /// </exception>
  void ICollection.CopyTo(Array array, int index) {
    if (array == null)
      throw new ArgumentNullException(nameof(array));
    if (index < 0)
      throw new ArgumentOutOfRangeException(nameof(index), index, "Value must be non-negative.");

    var items = this.ToArray();
    if (array.Rank != 1)
      throw new ArgumentException("Array must be single-dimensional.");
    if (index > array.Length - items.Length)
      throw new ArgumentException("The destination array is not large enough.");

    try {
      Array.Copy(items, 0, array, index, items.Length);
    } catch (ArrayTypeMismatchException) {
      throw new ArgumentException("The array type is incompatible with the type of items in the collection.");
    }
  }

  /// <summary>
  /// Gets a value indicating whether access to the <see cref="ICollection"/> is synchronized (thread safe).
  /// </summary>
  /// <value>Always returns <c>false</c>.</value>
  bool ICollection.IsSynchronized => false;

  /// <summary>
  /// Gets an object that can be used to synchronize access to the <see cref="ICollection"/>.
  /// </summary>
  /// <value>Always throws <see cref="NotSupportedException"/>.</value>
  /// <exception cref="NotSupportedException">Always thrown.</exception>
  object ICollection.SyncRoot => throw new NotSupportedException("ConcurrentQueue<T> does not support synchronized access via SyncRoot.");

  #endregion

}

#endif
