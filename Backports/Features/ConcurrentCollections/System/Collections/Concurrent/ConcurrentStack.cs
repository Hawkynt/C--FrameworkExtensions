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
/// Represents a thread-safe last in-first out (LIFO) collection.
/// </summary>
/// <typeparam name="T">The type of elements contained in the stack.</typeparam>
/// <remarks>
/// <para>
/// This implementation uses the lock-free Treiber stack algorithm for optimal performance
/// under contention. All public and protected members of <see cref="ConcurrentStack{T}"/>
/// are thread-safe and may be used concurrently from multiple threads.
/// </para>
/// </remarks>
[DebuggerDisplay("Count = {Count}")]
public class ConcurrentStack<T> : IProducerConsumerCollection<T>, IReadOnlyCollection<T> {

  /// <summary>
  /// A node in the lock-free stack.
  /// </summary>
  private sealed class Node {
    public readonly T Value;
    public Node? Next;

    public Node(T value) => this.Value = value;
  }

  /// <summary>
  /// The head of the stack. Null means empty stack.
  /// </summary>
  private volatile Node? _head;

  /// <summary>
  /// Initializes a new instance of the <see cref="ConcurrentStack{T}"/> class.
  /// </summary>
  public ConcurrentStack() { }

  /// <summary>
  /// Initializes a new instance of the <see cref="ConcurrentStack{T}"/> class
  /// that contains elements copied from the specified collection.
  /// </summary>
  /// <param name="collection">The collection whose elements are copied to the new <see cref="ConcurrentStack{T}"/>.</param>
  /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
  public ConcurrentStack(IEnumerable<T> collection) {
    if (collection == null)
      throw new ArgumentNullException(nameof(collection));

    foreach (var item in collection)
      this.Push(item);
  }

  /// <summary>
  /// Gets a value that indicates whether the <see cref="ConcurrentStack{T}"/> is empty.
  /// </summary>
  /// <value>
  /// <c>true</c> if the <see cref="ConcurrentStack{T}"/> is empty; otherwise, <c>false</c>.
  /// </value>
  public bool IsEmpty {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._head == null;
  }

  /// <summary>
  /// Gets the number of elements contained in the <see cref="ConcurrentStack{T}"/>.
  /// </summary>
  /// <value>The number of elements contained in the <see cref="ConcurrentStack{T}"/>.</value>
  /// <remarks>
  /// For determining whether the collection contains any items, use the <see cref="IsEmpty"/>
  /// property instead of retrieving the number of items from the <see cref="Count"/> property
  /// and comparing it to 0.
  /// </remarks>
  public int Count {
    get {
      var count = 0;
      for (var node = this._head; node != null; node = node.Next)
        ++count;
      return count;
    }
  }

  /// <summary>
  /// Inserts an object at the top of the <see cref="ConcurrentStack{T}"/>.
  /// </summary>
  /// <param name="item">The object to push onto the <see cref="ConcurrentStack{T}"/>.</param>
  public void Push(T item) {
    var newNode = new Node(item);
    var spinner = new SpinWait();

    do {
      var head = this._head;
      newNode.Next = head;
      if (Interlocked.CompareExchange(ref this._head, newNode, head) == head)
        return;
      spinner.SpinOnce();
    } while (true);
  }

  /// <summary>
  /// Inserts multiple objects at the top of the <see cref="ConcurrentStack{T}"/> atomically.
  /// </summary>
  /// <param name="items">The objects to push onto the <see cref="ConcurrentStack{T}"/>.</param>
  /// <exception cref="ArgumentNullException"><paramref name="items"/> is null.</exception>
  public void PushRange(T[] items) {
    if (items == null)
      throw new ArgumentNullException(nameof(items));

    this.PushRange(items, 0, items.Length);
  }

  /// <summary>
  /// Inserts multiple objects at the top of the <see cref="ConcurrentStack{T}"/> atomically.
  /// </summary>
  /// <param name="items">The objects to push onto the <see cref="ConcurrentStack{T}"/>.</param>
  /// <param name="startIndex">The zero-based offset in <paramref name="items"/> at which to begin inserting elements.</param>
  /// <param name="count">The number of elements to be inserted.</param>
  /// <exception cref="ArgumentNullException"><paramref name="items"/> is null.</exception>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="startIndex"/> or <paramref name="count"/> is negative, or
  /// <paramref name="startIndex"/> is greater than or equal to the length of <paramref name="items"/>.
  /// </exception>
  /// <exception cref="ArgumentException">
  /// <paramref name="startIndex"/> + <paramref name="count"/> is greater than the length of <paramref name="items"/>.
  /// </exception>
  public void PushRange(T[] items, int startIndex, int count) {
    if (items == null)
      throw new ArgumentNullException(nameof(items));
    if (startIndex < 0)
      throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, "Value must be non-negative.");
    if (count < 0)
      throw new ArgumentOutOfRangeException(nameof(count), count, "Value must be non-negative.");
    if (startIndex >= items.Length && count > 0)
      throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, "Start index must be less than the length of items.");
    if (startIndex + count > items.Length)
      throw new ArgumentException("The sum of startIndex and count is larger than the items array.");

    if (count == 0)
      return;

    // Build a linked list of the items in reverse order (so they appear in correct order when popped)
    Node? firstNode = null;
    Node? lastNode = null;

    for (var i = startIndex + count - 1; i >= startIndex; --i) {
      var newNode = new Node(items[i]);
      if (firstNode == null) {
        firstNode = newNode;
        lastNode = newNode;
      } else {
        newNode.Next = firstNode;
        firstNode = newNode;
      }
    }

    // Atomically push the entire chain
    var spinner = new SpinWait();
    do {
      var head = this._head;
      lastNode!.Next = head;
      if (Interlocked.CompareExchange(ref this._head, firstNode, head) == head)
        return;
      spinner.SpinOnce();
    } while (true);
  }

  /// <summary>
  /// Attempts to pop and return the object at the top of the <see cref="ConcurrentStack{T}"/>.
  /// </summary>
  /// <param name="result">
  /// When this method returns, if the operation was successful, <paramref name="result"/> contains the
  /// object removed. If no object was available to be removed, the value is unspecified.
  /// </param>
  /// <returns>
  /// <c>true</c> if an element was removed and returned from the top of the <see cref="ConcurrentStack{T}"/>
  /// successfully; otherwise, <c>false</c>.
  /// </returns>
  public bool TryPop([MaybeNullWhen(false)] out T result) {
    var spinner = new SpinWait();

    do {
      var head = this._head;
      if (head == null) {
        result = default;
        return false;
      }

      if (Interlocked.CompareExchange(ref this._head, head.Next, head) == head) {
        result = head.Value;
        return true;
      }

      spinner.SpinOnce();
    } while (true);
  }

  /// <summary>
  /// Attempts to pop and return multiple objects from the top of the <see cref="ConcurrentStack{T}"/> atomically.
  /// </summary>
  /// <param name="items">
  /// The <see cref="Array"/> to which objects popped from the top of the <see cref="ConcurrentStack{T}"/>
  /// will be added.
  /// </param>
  /// <returns>The number of objects successfully popped from the top of the <see cref="ConcurrentStack{T}"/>.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="items"/> is null.</exception>
  public int TryPopRange(T[] items) {
    if (items == null)
      throw new ArgumentNullException(nameof(items));

    return this.TryPopRange(items, 0, items.Length);
  }

  /// <summary>
  /// Attempts to pop and return multiple objects from the top of the <see cref="ConcurrentStack{T}"/> atomically.
  /// </summary>
  /// <param name="items">
  /// The <see cref="Array"/> to which objects popped from the top of the <see cref="ConcurrentStack{T}"/>
  /// will be added.
  /// </param>
  /// <param name="startIndex">
  /// The zero-based offset in <paramref name="items"/> at which to begin inserting elements.
  /// </param>
  /// <param name="count">The number of elements to be popped and inserted into <paramref name="items"/>.</param>
  /// <returns>The number of objects successfully popped from the top of the <see cref="ConcurrentStack{T}"/>.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="items"/> is null.</exception>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="startIndex"/> or <paramref name="count"/> is negative, or
  /// <paramref name="startIndex"/> is greater than or equal to the length of <paramref name="items"/>.
  /// </exception>
  /// <exception cref="ArgumentException">
  /// <paramref name="startIndex"/> + <paramref name="count"/> is greater than the length of <paramref name="items"/>.
  /// </exception>
  public int TryPopRange(T[] items, int startIndex, int count) {
    if (items == null)
      throw new ArgumentNullException(nameof(items));
    if (startIndex < 0)
      throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, "Value must be non-negative.");
    if (count < 0)
      throw new ArgumentOutOfRangeException(nameof(count), count, "Value must be non-negative.");
    if (startIndex >= items.Length && count > 0)
      throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, "Start index must be less than the length of items.");
    if (startIndex + count > items.Length)
      throw new ArgumentException("The sum of startIndex and count is larger than the items array.");

    if (count == 0)
      return 0;

    var spinner = new SpinWait();
    var poppedCount = 0;

    do {
      var head = this._head;
      if (head == null)
        return poppedCount;

      // Walk the list to find how many nodes we can pop (up to count)
      var nodesToPop = 0;
      var current = head;
      Node? newHead = null;

      while (current != null && nodesToPop < count) {
        ++nodesToPop;
        newHead = current.Next;
        if (nodesToPop < count)
          current = current.Next;
        else
          break;
      }

      // Try to atomically update head
      if (Interlocked.CompareExchange(ref this._head, newHead, head) == head) {
        // Success - copy values to the array
        current = head;
        for (var i = 0; i < nodesToPop && current != null; ++i) {
          items[startIndex + i] = current.Value;
          current = current.Next;
        }
        return nodesToPop;
      }

      spinner.SpinOnce();
    } while (true);
  }

  /// <summary>
  /// Attempts to return an object from the top of the <see cref="ConcurrentStack{T}"/>
  /// without removing it.
  /// </summary>
  /// <param name="result">
  /// When this method returns, <paramref name="result"/> contains an object from the top of the
  /// <see cref="ConcurrentStack{T}"/> or an unspecified value if the operation failed.
  /// </param>
  /// <returns>
  /// <c>true</c> if an object was returned successfully; otherwise, <c>false</c>.
  /// </returns>
  public bool TryPeek([MaybeNullWhen(false)] out T result) {
    var head = this._head;
    if (head == null) {
      result = default;
      return false;
    }

    result = head.Value;
    return true;
  }

  /// <summary>
  /// Removes all objects from the <see cref="ConcurrentStack{T}"/>.
  /// </summary>
  public void Clear() {
    // Atomically set head to null
    var spinner = new SpinWait();
    do {
      var head = this._head;
      if (Interlocked.CompareExchange(ref this._head, null, head) == head)
        return;
      spinner.SpinOnce();
    } while (true);
  }

  /// <summary>
  /// Copies the items stored in the <see cref="ConcurrentStack{T}"/> to a new array.
  /// </summary>
  /// <returns>A new array containing a snapshot of elements copied from the <see cref="ConcurrentStack{T}"/>.</returns>
  public T[] ToArray() {
    // Get a snapshot of the head
    var head = this._head;
    if (head == null)
      return [];

    // Count and collect items
    var items = new List<T>();
    for (var node = head; node != null; node = node.Next)
      items.Add(node.Value);

    return items.ToArray();
  }

  /// <summary>
  /// Copies the <see cref="ConcurrentStack{T}"/> elements to an existing one-dimensional
  /// <see cref="Array"/>, starting at the specified array index.
  /// </summary>
  /// <param name="array">
  /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from
  /// the <see cref="ConcurrentStack{T}"/>. The <see cref="Array"/> must have zero-based indexing.
  /// </param>
  /// <param name="index">The zero-based index in <paramref name="array"/> at which copying begins.</param>
  /// <exception cref="ArgumentNullException"><paramref name="array"/> is null.</exception>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero.</exception>
  /// <exception cref="ArgumentException">
  /// <paramref name="index"/> is equal to or greater than the length of the <paramref name="array"/>,
  /// or the number of elements in the source <see cref="ConcurrentStack{T}"/> is greater than the
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
  /// Returns an enumerator that iterates through the <see cref="ConcurrentStack{T}"/>.
  /// </summary>
  /// <returns>An enumerator for the <see cref="ConcurrentStack{T}"/>.</returns>
  /// <remarks>
  /// The enumeration represents a moment-in-time snapshot of the contents of the stack.
  /// It does not reflect any updates to the collection after <see cref="GetEnumerator"/>
  /// was called. The enumerator is safe to use concurrently with reads from and writes
  /// to the stack.
  /// </remarks>
  public IEnumerator<T> GetEnumerator() {
    for (var node = this._head; node != null; node = node.Next)
      yield return node.Value;
  }

  /// <summary>
  /// Returns an enumerator that iterates through a collection.
  /// </summary>
  /// <returns>An <see cref="IEnumerator"/> that can be used to iterate through the collection.</returns>
  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  #region IProducerConsumerCollection<T> Implementation

  /// <summary>
  /// Attempts to add an object to the <see cref="ConcurrentStack{T}"/>.
  /// </summary>
  /// <param name="item">The object to add to the <see cref="ConcurrentStack{T}"/>.</param>
  /// <returns><c>true</c> if the object was added successfully; otherwise, <c>false</c>.</returns>
  bool IProducerConsumerCollection<T>.TryAdd(T item) {
    this.Push(item);
    return true;
  }

  /// <summary>
  /// Attempts to remove and return an object from the <see cref="ConcurrentStack{T}"/>.
  /// </summary>
  /// <param name="item">
  /// When this method returns, if the object was removed and returned successfully, <paramref name="item"/>
  /// contains the removed object. If no object was available to be removed, the value is unspecified.
  /// </param>
  /// <returns><c>true</c> if an object was removed and returned successfully; otherwise, <c>false</c>.</returns>
  bool IProducerConsumerCollection<T>.TryTake([MaybeNullWhen(false)] out T item) => this.TryPop(out item);

  /// <summary>
  /// Copies the elements of the <see cref="ICollection"/> to an <see cref="Array"/>,
  /// starting at a particular <see cref="Array"/> index.
  /// </summary>
  /// <param name="array">
  /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from
  /// <see cref="ConcurrentStack{T}"/>. The <see cref="Array"/> must have zero-based indexing.
  /// </param>
  /// <param name="index">The zero-based index in <paramref name="array"/> at which copying begins.</param>
  /// <exception cref="ArgumentNullException"><paramref name="array"/> is null.</exception>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero.</exception>
  /// <exception cref="ArgumentException">
  /// <paramref name="array"/> is multidimensional, or <paramref name="index"/> is equal to or greater
  /// than the length of the <paramref name="array"/>, or the number of elements in the source
  /// <see cref="ConcurrentStack{T}"/> is greater than the available space from <paramref name="index"/>
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
  object ICollection.SyncRoot => throw new NotSupportedException("ConcurrentStack<T> does not support synchronized access via SyncRoot.");

  #endregion

}

#endif
