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

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Collections.Generic;

// Wave 1: Core PriorityQueue (.NET 6.0)
#if !SUPPORTS_PRIORITYQUEUE

/// <summary>
/// Represents a min priority queue - a collection of items that have a value and a priority.
/// On dequeue, the item with the lowest priority value is removed.
/// </summary>
/// <typeparam name="TElement">Specifies the type of elements in the queue.</typeparam>
/// <typeparam name="TPriority">Specifies the type of priority associated with enqueued elements.</typeparam>
public class PriorityQueue<TElement, TPriority> {
  private const int _DEFAULT_CAPACITY = 4;

  private (TElement Element, TPriority Priority)[] _nodes;
  private int _size;
  private int _version;
  private readonly IComparer<TPriority> _comparer;

  #region Constructors

  /// <summary>
  /// Initializes a new instance of the <see cref="PriorityQueue{TElement, TPriority}"/> class.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public PriorityQueue() : this(0, null) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="PriorityQueue{TElement, TPriority}"/> class
  /// with the specified custom priority comparer.
  /// </summary>
  /// <param name="comparer">
  /// Custom comparer dictating the ordering of elements.
  /// Uses <see cref="Comparer{T}.Default"/> if the argument is <see langword="null"/>.
  /// </param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public PriorityQueue(IComparer<TPriority>? comparer) : this(0, comparer) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="PriorityQueue{TElement, TPriority}"/> class
  /// with the specified initial capacity.
  /// </summary>
  /// <param name="initialCapacity">Initial capacity to allocate in the underlying heap array.</param>
  /// <exception cref="ArgumentOutOfRangeException">
  /// The specified <paramref name="initialCapacity"/> was negative.
  /// </exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public PriorityQueue(int initialCapacity) : this(initialCapacity, null) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="PriorityQueue{TElement, TPriority}"/> class
  /// with the specified initial capacity and custom priority comparer.
  /// </summary>
  /// <param name="initialCapacity">Initial capacity to allocate in the underlying heap array.</param>
  /// <param name="comparer">
  /// Custom comparer dictating the ordering of elements.
  /// Uses <see cref="Comparer{T}.Default"/> if the argument is <see langword="null"/>.
  /// </param>
  /// <exception cref="ArgumentOutOfRangeException">
  /// The specified <paramref name="initialCapacity"/> was negative.
  /// </exception>
  public PriorityQueue(int initialCapacity, IComparer<TPriority>? comparer) {
    ArgumentOutOfRangeException.ThrowIfNegative(initialCapacity);

    this._nodes = initialCapacity == 0 ? [] : new (TElement, TPriority)[initialCapacity];
    this._comparer = comparer ?? Comparer<TPriority>.Default;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="PriorityQueue{TElement, TPriority}"/> class
  /// that is populated with the specified elements and priorities.
  /// </summary>
  /// <param name="items">The pairs of elements and priorities with which to populate the queue.</param>
  /// <exception cref="ArgumentNullException">
  /// The specified <paramref name="items"/> argument was <see langword="null"/>.
  /// </exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public PriorityQueue(IEnumerable<(TElement Element, TPriority Priority)> items)
    : this(items, null) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="PriorityQueue{TElement, TPriority}"/> class
  /// that is populated with the specified elements and priorities,
  /// and with the specified custom priority comparer.
  /// </summary>
  /// <param name="items">The pairs of elements and priorities with which to populate the queue.</param>
  /// <param name="comparer">
  /// Custom comparer dictating the ordering of elements.
  /// Uses <see cref="Comparer{T}.Default"/> if the argument is <see langword="null"/>.
  /// </param>
  /// <exception cref="ArgumentNullException">
  /// The specified <paramref name="items"/> argument was <see langword="null"/>.
  /// </exception>
  public PriorityQueue(IEnumerable<(TElement Element, TPriority Priority)> items, IComparer<TPriority>? comparer) {
    ArgumentNullException.ThrowIfNull(items);

    this._comparer = comparer ?? Comparer<TPriority>.Default;
    this._nodes = items is ICollection<(TElement, TPriority)> collection
      ? new (TElement, TPriority)[collection.Count]
      : [];

    foreach (var (element, priority) in items)
      this.Enqueue(element, priority);
  }

  #endregion

  #region Properties

  /// <summary>
  /// Gets the number of elements contained in the <see cref="PriorityQueue{TElement, TPriority}"/>.
  /// </summary>
  public int Count {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._size;
  }

  /// <summary>
  /// Gets the priority comparer used by the <see cref="PriorityQueue{TElement, TPriority}"/>.
  /// </summary>
  public IComparer<TPriority> Comparer {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._comparer;
  }

  /// <summary>
  /// Gets a collection that enumerates the elements of the queue in an unordered manner.
  /// </summary>
  public UnorderedItemsCollection UnorderedItems => field ??= new(this);

  #endregion

  #region Public Methods

  /// <summary>
  /// Adds the specified element with associated priority to the <see cref="PriorityQueue{TElement, TPriority}"/>.
  /// </summary>
  /// <param name="element">The element to add to the <see cref="PriorityQueue{TElement, TPriority}"/>.</param>
  /// <param name="priority">The priority with which to associate the new element.</param>
  public void Enqueue(TElement element, TPriority priority) {
    this._EnsureCapacity(this._size + 1);

    this._size++;
    this._version++;

    this._MoveUp((element, priority), this._size - 1);
  }

  /// <summary>
  /// Returns the minimal element from the <see cref="PriorityQueue{TElement, TPriority}"/> without removing it.
  /// </summary>
  /// <returns>The minimal element of the <see cref="PriorityQueue{TElement, TPriority}"/>.</returns>
  /// <exception cref="InvalidOperationException">The <see cref="PriorityQueue{TElement, TPriority}"/> is empty.</exception>
  public TElement Peek() {
    if (this._size == 0)
      throw new InvalidOperationException("The queue is empty.");

    return this._nodes[0].Element;
  }

  /// <summary>
  /// Removes and returns the minimal element from the <see cref="PriorityQueue{TElement, TPriority}"/>.
  /// </summary>
  /// <returns>The minimal element of the <see cref="PriorityQueue{TElement, TPriority}"/>.</returns>
  /// <exception cref="InvalidOperationException">The queue is empty.</exception>
  public TElement Dequeue() {
    if (this._size == 0)
      throw new InvalidOperationException("The queue is empty.");

    var element = this._nodes[0].Element;
    this._RemoveAt(0);
    return element;
  }

  /// <summary>
  /// Returns a value that indicates whether there is a minimal element in the <see cref="PriorityQueue{TElement, TPriority}"/>,
  /// and if one is present, copies it to the <paramref name="element"/> parameter,
  /// and copies its associated priority to the <paramref name="priority"/> parameter.
  /// The element is not removed from the <see cref="PriorityQueue{TElement, TPriority}"/>.
  /// </summary>
  /// <param name="element">The minimal element in the queue.</param>
  /// <param name="priority">The priority associated with the minimal element.</param>
  /// <returns>
  /// <see langword="true"/> if there is a minimal element;
  /// <see langword="false"/> if the <see cref="PriorityQueue{TElement, TPriority}"/> is empty.
  /// </returns>
  public bool TryPeek(out TElement element, out TPriority priority) {
    if (this._size == 0) {
      element = default!;
      priority = default!;
      return false;
    }

    (element, priority) = this._nodes[0];
    return true;
  }

  /// <summary>
  /// Removes the minimal element from the <see cref="PriorityQueue{TElement, TPriority}"/>,
  /// and copies it to the <paramref name="element"/> parameter,
  /// and its associated priority to the <paramref name="priority"/> parameter.
  /// </summary>
  /// <param name="element">The removed element.</param>
  /// <param name="priority">The priority associated with the removed element.</param>
  /// <returns>
  /// <see langword="true"/> if the element is successfully removed;
  /// <see langword="false"/> if the <see cref="PriorityQueue{TElement, TPriority}"/> is empty.
  /// </returns>
  public bool TryDequeue(out TElement element, out TPriority priority) {
    if (this._size == 0) {
      element = default!;
      priority = default!;
      return false;
    }

    (element, priority) = this._nodes[0];
    this._RemoveAt(0);
    return true;
  }

  /// <summary>
  /// Adds the specified element with associated priority to the <see cref="PriorityQueue{TElement, TPriority}"/>,
  /// and immediately removes the minimal element, returning the result.
  /// </summary>
  /// <param name="element">The element to add to the <see cref="PriorityQueue{TElement, TPriority}"/>.</param>
  /// <param name="priority">The priority with which to associate the new element.</param>
  /// <returns>The minimal element removed after the enqueue operation.</returns>
  public TElement EnqueueDequeue(TElement element, TPriority priority) {
    if (this._size == 0)
      return element;

    ref var root = ref this._nodes[0];
    if (this._comparer.Compare(priority, root.Priority) <= 0)
      return element;

    var result = root.Element;
    root = (element, priority);
    this._version++;
    this._MoveDown((element, priority), 0);
    return result;
  }

  /// <summary>
  /// Enqueues a sequence of element/priority pairs to the <see cref="PriorityQueue{TElement, TPriority}"/>.
  /// </summary>
  /// <param name="items">The pairs of elements and priorities to add to the queue.</param>
  /// <exception cref="ArgumentNullException">
  /// The specified <paramref name="items"/> argument was <see langword="null"/>.
  /// </exception>
  public void EnqueueRange(IEnumerable<(TElement Element, TPriority Priority)> items) {
    ArgumentNullException.ThrowIfNull(items);

    foreach (var (element, priority) in items)
      this.Enqueue(element, priority);
  }

  /// <summary>
  /// Enqueues a sequence of elements with the specified priority to the <see cref="PriorityQueue{TElement, TPriority}"/>.
  /// </summary>
  /// <param name="elements">The elements to add to the queue.</param>
  /// <param name="priority">The priority to associate with the new elements.</param>
  /// <exception cref="ArgumentNullException">
  /// The specified <paramref name="elements"/> argument was <see langword="null"/>.
  /// </exception>
  public void EnqueueRange(IEnumerable<TElement> elements, TPriority priority) {
    ArgumentNullException.ThrowIfNull(elements);

    foreach (var element in elements)
      this.Enqueue(element, priority);
  }

  /// <summary>
  /// Removes all items from the <see cref="PriorityQueue{TElement, TPriority}"/>.
  /// </summary>
  public void Clear() {
    // Always clear the array to allow GC of references
    // This is slightly less efficient for value types but ensures correctness
    Array.Clear(this._nodes, 0, this._size);

    this._size = 0;
    this._version++;
  }

  /// <summary>
  /// Ensures that the <see cref="PriorityQueue{TElement, TPriority}"/> can hold up to
  /// <paramref name="capacity"/> items without further expansion of its backing storage.
  /// </summary>
  /// <param name="capacity">The minimum capacity to be used.</param>
  /// <exception cref="ArgumentOutOfRangeException">
  /// The specified <paramref name="capacity"/> is negative.
  /// </exception>
  /// <returns>The current capacity of the <see cref="PriorityQueue{TElement, TPriority}"/>.</returns>
  public int EnsureCapacity(int capacity) {
    ArgumentOutOfRangeException.ThrowIfNegative(capacity);

    this._EnsureCapacity(capacity);
    this._version++;
    return this._nodes.Length;
  }

  /// <summary>
  /// Sets the capacity to the actual number of items in the <see cref="PriorityQueue{TElement, TPriority}"/>,
  /// if that is less than 90 percent of current capacity.
  /// </summary>
  public void TrimExcess() {
    var threshold = (int)(this._nodes.Length * 0.9);
    if (this._size < threshold)
      Array.Resize(ref this._nodes, this._size);

    this._version++;
  }

  #endregion

  #region Private Heap Operations

  private void _EnsureCapacity(int capacity) {
    if (this._nodes.Length >= capacity)
      return;

    var newCapacity = this._nodes.Length == 0 ? _DEFAULT_CAPACITY : this._nodes.Length * 2;
    if (newCapacity < capacity)
      newCapacity = capacity;

    Array.Resize(ref this._nodes, newCapacity);
  }

  private void _MoveUp((TElement Element, TPriority Priority) node, int nodeIndex) {
    while (nodeIndex > 0) {
      var parentIndex = (nodeIndex - 1) >> 1;
      var parent = this._nodes[parentIndex];

      if (this._comparer.Compare(node.Priority, parent.Priority) >= 0)
        break;

      this._nodes[nodeIndex] = parent;
      nodeIndex = parentIndex;
    }

    this._nodes[nodeIndex] = node;
  }

  private void _MoveDown((TElement Element, TPriority Priority) node, int nodeIndex) {
    int childIndex;
    while ((childIndex = (nodeIndex << 1) + 1) < this._size) {
      var rightChildIndex = childIndex + 1;
      if (rightChildIndex < this._size && this._comparer.Compare(this._nodes[rightChildIndex].Priority, this._nodes[childIndex].Priority) < 0)
        childIndex = rightChildIndex;

      var child = this._nodes[childIndex];
      if (this._comparer.Compare(node.Priority, child.Priority) <= 0)
        break;

      this._nodes[nodeIndex] = child;
      nodeIndex = childIndex;
    }

    this._nodes[nodeIndex] = node;
  }

  private void _RemoveAt(int index) {
    this._size--;
    this._version++;

    if (index < this._size) {
      var lastNode = this._nodes[this._size];
      this._MoveDown(lastNode, index);
    }

    // Always clear to allow GC of references
    this._nodes[this._size] = default;
  }

  #endregion

  #region UnorderedItemsCollection

  /// <summary>
  /// Enumerates the contents of a <see cref="PriorityQueue{TElement, TPriority}"/>, without any ordering guarantees.
  /// </summary>
  public sealed class UnorderedItemsCollection : IReadOnlyCollection<(TElement Element, TPriority Priority)>, ICollection {
    private readonly PriorityQueue<TElement, TPriority> _queue;

    internal UnorderedItemsCollection(PriorityQueue<TElement, TPriority> queue) => this._queue = queue;

    /// <inheritdoc/>
    public int Count => this._queue._size;

    /// <inheritdoc/>
    bool ICollection.IsSynchronized => false;

    /// <inheritdoc/>
    object ICollection.SyncRoot => this;

    /// <inheritdoc/>
    void ICollection.CopyTo(Array array, int index) {
      ArgumentNullException.ThrowIfNull(array);
      ArgumentOutOfRangeException.ThrowIfNegative(index);

      if (array.Rank != 1)
        throw new ArgumentException("Array is multidimensional.", nameof(array));

      if (array.Length - index < this._queue._size)
        throw new ArgumentException("Destination array is not long enough.", nameof(array));

      try {
        Array.Copy(this._queue._nodes, 0, array, index, this._queue._size);
      } catch (ArrayTypeMismatchException) {
        throw new ArgumentException("Invalid array type.", nameof(array));
      }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="UnorderedItemsCollection"/>.
    /// </summary>
    public Enumerator GetEnumerator() => new(this._queue);

    /// <inheritdoc/>
    IEnumerator<(TElement Element, TPriority Priority)> IEnumerable<(TElement Element, TPriority Priority)>.GetEnumerator()
      => this.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    /// <summary>
    /// Enumerates the element and priority pairs of a <see cref="PriorityQueue{TElement, TPriority}"/>,
    /// without any ordering guarantees.
    /// </summary>
    public struct Enumerator : IEnumerator<(TElement Element, TPriority Priority)> {
      private readonly PriorityQueue<TElement, TPriority> _queue;
      private readonly int _version;
      private int _index;
      private (TElement Element, TPriority Priority) _current;

      internal Enumerator(PriorityQueue<TElement, TPriority> queue) {
        this._queue = queue;
        this._version = queue._version;
        this._index = -1;
        this._current = default;
      }

      /// <inheritdoc/>
      public readonly (TElement Element, TPriority Priority) Current => this._current;

      /// <inheritdoc/>
      readonly object IEnumerator.Current => this.Current;

      /// <inheritdoc/>
      public bool MoveNext() {
        if (this._version != this._queue._version)
          throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");

        if (++this._index < this._queue._size) {
          this._current = this._queue._nodes[this._index];
          return true;
        }

        this._current = default;
        return false;
      }

      /// <inheritdoc/>
      public void Reset() {
        if (this._version != this._queue._version)
          throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");

        this._index = -1;
        this._current = default;
      }

      /// <inheritdoc/>
      public readonly void Dispose() { }
    }
  }

  #endregion
}

#endif

// Wave 2: DequeueEnqueue (.NET 8.0)
#if !SUPPORTS_PRIORITYQUEUE_DEQUEUEENQUEUE

public static partial class PriorityQueuePolyfills {

  extension<TElement, TPriority>(PriorityQueue<TElement, TPriority> @this) {
  
    /// <summary>
    /// Removes the minimal element and then immediately adds the specified element with associated priority to the
    /// <see cref="PriorityQueue{TElement, TPriority}"/>.
    /// </summary>
    /// <typeparam name="TElement">Specifies the type of elements in the queue.</typeparam>
    /// <typeparam name="TPriority">Specifies the type of priority associated with enqueued elements.</typeparam>
    /// <param name="element">The element to add to the <see cref="PriorityQueue{TElement, TPriority}"/>.</param>
    /// <param name="priority">The priority with which to associate the new element.</param>
    /// <returns>The minimal element removed before the enqueue operation.</returns>
    /// <exception cref="InvalidOperationException">The queue is empty.</exception>
    public TElement DequeueEnqueue(TElement element, TPriority priority) {
      var result = @this.Dequeue();
      @this.Enqueue(element, priority);
      return result;
    }
  }
}

#endif

// Wave 3: Remove (.NET 9.0)
#if !SUPPORTS_PRIORITYQUEUE_REMOVE

public static partial class PriorityQueuePolyfills {

  extension<TElement, TPriority>(PriorityQueue<TElement, TPriority> @this) {

    /// <summary>
    /// Removes the first occurrence of the specified element from the <see cref="PriorityQueue{TElement, TPriority}"/>.
    /// </summary>
    /// <typeparam name="TElement">Specifies the type of elements in the queue.</typeparam>
    /// <typeparam name="TPriority">Specifies the type of priority associated with enqueued elements.</typeparam>
    /// <param name="element">The element to remove.</param>
    /// <param name="removedElement">The removed element.</param>
    /// <param name="priority">The priority associated with the removed element.</param>
    /// <param name="equalityComparer">The equality comparer to use to locate the <paramref name="element"/>.</param>
    /// <returns><see langword="true"/> if matching entry was found and removed; otherwise, <see langword="false"/>.</returns>
    public bool Remove(
      TElement element,
      out TElement removedElement,
      out TPriority priority,
      IEqualityComparer<TElement>? equalityComparer = null
    ) {
      equalityComparer ??= EqualityComparer<TElement>.Default;

      // Search through unordered items to find the element
      var found = false;
      (TElement Element, TPriority Priority) foundItem = default;

      foreach (var item in @this.UnorderedItems) {
        if (!equalityComparer.Equals(item.Element, element))
          continue;

        foundItem = item;
        found = true;
        break;
      }

      if (!found) {
        removedElement = default!;
        priority = default!;
        return false;
      }

      // We need to rebuild the queue without this element
      // This is O(n) but matches the expected behavior
      var items = new List<(TElement Element, TPriority Priority)>(@this.Count - 1);
      var skipOnce = true;
      foreach (var item in @this.UnorderedItems) {
        if (skipOnce && equalityComparer.Equals(item.Element, element)) {
          skipOnce = false;
          continue;
        }

        items.Add(item);
      }

      @this.Clear();
      @this.EnqueueRange(items);

      removedElement = foundItem.Element;
      priority = foundItem.Priority;
      return true;
    }
  }
}

#endif

// Wave 4: Capacity property (.NET 10.0)
#if !SUPPORTS_PRIORITYQUEUE_CAPACITY

public static partial class PriorityQueuePolyfills {

  extension<TElement, TPriority>(PriorityQueue<TElement, TPriority> @this) {

    /// <summary>
    /// Gets the number of elements that the <see cref="PriorityQueue{TElement, TPriority}"/> can hold
    /// without having to increase its capacity.
    /// </summary>
    /// <typeparam name="TElement">Specifies the type of elements in the queue.</typeparam>
    /// <typeparam name="TPriority">Specifies the type of priority associated with enqueued elements.</typeparam>
    /// <returns>The current capacity of the priority queue.</returns>
    public int Capacity =>
      // We can get an approximation by calling EnsureCapacity(0) which returns current capacity
      // without modifying the queue
      @this.EnsureCapacity(0);
  }
}

#endif
