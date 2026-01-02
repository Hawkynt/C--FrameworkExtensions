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

#if !SUPPORTS_THREADLOCAL

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Threading;

/// <summary>
/// Provides thread-local storage of data.
/// </summary>
/// <typeparam name="T">Specifies the type of data stored per-thread.</typeparam>
/// <remarks>
/// <para>
/// With the exception of <see cref="Dispose()"/>, all public and protected members of <see cref="ThreadLocal{T}"/>
/// are thread-safe and may be used concurrently from multiple threads.
/// </para>
/// </remarks>
public class ThreadLocal<T> : IDisposable {

  /// <summary>
  /// Per-thread values indexed by ManagedThreadId.
  /// </summary>
  private readonly Dictionary<int, LinkedListNode<T>> _values = [];

  /// <summary>
  /// All values for tracking (only used when trackAllValues is true).
  /// </summary>
  private readonly LinkedList<T>? _allValues;

  /// <summary>
  /// The value factory for lazy initialization.
  /// </summary>
  private readonly Func<T>? _valueFactory;

  /// <summary>
  /// Lock for thread-safe access.
  /// </summary>
  private readonly object _lock = new();

  /// <summary>
  /// Whether this instance has been disposed.
  /// </summary>
  private volatile bool _disposed;

  /// <summary>
  /// Whether to track all values.
  /// </summary>
  private readonly bool _trackAllValues;

  /// <summary>
  /// Initializes a new instance of the <see cref="ThreadLocal{T}"/> class.
  /// </summary>
  public ThreadLocal()
    : this(null, false) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="ThreadLocal{T}"/> class with the specified
  /// <paramref name="valueFactory"/> function.
  /// </summary>
  /// <param name="valueFactory">
  /// The <see cref="Func{T}"/> invoked to produce a lazily-initialized value when an attempt is made
  /// to retrieve <see cref="Value"/> without it having been previously initialized.
  /// </param>
  /// <exception cref="ArgumentNullException"><paramref name="valueFactory"/> is null.</exception>
  public ThreadLocal(Func<T> valueFactory)
    : this(valueFactory, false) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="ThreadLocal{T}"/> class that specifies whether
  /// all values are accessible from any thread.
  /// </summary>
  /// <param name="trackAllValues">
  /// Whether to track all values set on the instance and expose them via the <see cref="Values"/> property.
  /// </param>
  public ThreadLocal(bool trackAllValues)
    : this(null, trackAllValues) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="ThreadLocal{T}"/> class with the specified
  /// <paramref name="valueFactory"/> function and a flag that indicates whether all values are accessible.
  /// </summary>
  /// <param name="valueFactory">
  /// The <see cref="Func{T}"/> invoked to produce a lazily-initialized value when an attempt is made
  /// to retrieve <see cref="Value"/> without it having been previously initialized.
  /// </param>
  /// <param name="trackAllValues">
  /// Whether to track all values set on the instance and expose them via the <see cref="Values"/> property.
  /// </param>
  public ThreadLocal(Func<T>? valueFactory, bool trackAllValues) {
    this._valueFactory = valueFactory;
    this._trackAllValues = trackAllValues;
    if (trackAllValues)
      this._allValues = new();
  }

  /// <summary>
  /// Gets or sets the value of this instance for the current thread.
  /// </summary>
  /// <value>
  /// An instance of the value for the current thread, lazily initialized if necessary.
  /// </value>
  /// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
  /// <exception cref="InvalidOperationException">
  /// The initialization function referenced <see cref="Value"/> recursively.
  /// </exception>
  public T Value {
    get {
      this.ThrowIfDisposed();

      var threadId = Thread.CurrentThread.ManagedThreadId;

      lock (this._lock)
        if (this._values.TryGetValue(threadId, out var node))
          return node.Value;

      // Value not found, create it
      var value = this._valueFactory != null ? this._valueFactory() : default!;
      this.SetValueInternal(threadId, value);
      return value;
    }
    set {
      this.ThrowIfDisposed();

      var threadId = Thread.CurrentThread.ManagedThreadId;
      this.SetValueInternal(threadId, value);
    }
  }

  /// <summary>
  /// Sets the value for a specific thread.
  /// </summary>
  private void SetValueInternal(int threadId, T value) {
    lock (this._lock) {
      if (this._values.TryGetValue(threadId, out var existingNode)) {
        // Update existing value
        existingNode.Value = value;
      } else {
        // Create new entry
        LinkedListNode<T> newNode;
        if (this._trackAllValues && this._allValues != null) {
          newNode = this._allValues.AddLast(value);
        } else
          newNode = new(value);

        this._values[threadId] = newNode;
      }
    }
  }

  /// <summary>
  /// Gets whether <see cref="Value"/> is initialized on the current thread.
  /// </summary>
  /// <value>
  /// <c>true</c> if <see cref="Value"/> is initialized on the current thread; otherwise, <c>false</c>.
  /// </value>
  /// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
  public bool IsValueCreated {
    get {
      this.ThrowIfDisposed();

      var threadId = Thread.CurrentThread.ManagedThreadId;

      lock (this._lock)
        return this._values.ContainsKey(threadId);
    }
  }

  /// <summary>
  /// Gets a list for all of the values currently stored by all of the threads that have accessed this instance.
  /// </summary>
  /// <value>
  /// A list for all of the values currently stored by all of the threads that have accessed this instance.
  /// </value>
  /// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
  /// <exception cref="InvalidOperationException">
  /// The <see cref="ThreadLocal{T}"/> instance was not created with <c>trackAllValues</c> set to <c>true</c>.
  /// </exception>
  public IList<T> Values {
    get {
      this.ThrowIfDisposed();

      if (!this._trackAllValues)
        throw new InvalidOperationException("This ThreadLocal instance was not created with trackAllValues set to true.");

      lock (this._lock) {
        var result = new List<T>();
        if (this._allValues != null)
          foreach (var value in this._allValues)
            result.Add(value);
        return result;
      }
    }
  }

  /// <summary>
  /// Creates and returns a string representation of this instance for the current thread.
  /// </summary>
  /// <returns>The result of calling <see cref="object.ToString"/> on the <see cref="Value"/>.</returns>
  /// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
  /// <exception cref="NullReferenceException">The <see cref="Value"/> for the current thread is null.</exception>
  public override string? ToString() => this.Value?.ToString();

  /// <summary>
  /// Releases all resources used by the current instance of the <see cref="ThreadLocal{T}"/> class.
  /// </summary>
  /// <remarks>
  /// Unlike most of the members of <see cref="ThreadLocal{T}"/>, this method is not thread-safe.
  /// </remarks>
  public void Dispose() {
    this.Dispose(true);
    GC.SuppressFinalize(this);
  }

  /// <summary>
  /// Releases the resources used by this <see cref="ThreadLocal{T}"/> instance.
  /// </summary>
  /// <param name="disposing">
  /// <c>true</c> if this method is being called due to a call to <see cref="Dispose()"/>; otherwise, <c>false</c>.
  /// </param>
  protected virtual void Dispose(bool disposing) {
    if (this._disposed)
      return;

    this._disposed = true;

    if (disposing) {
      lock (this._lock) {
        this._values.Clear();
        this._allValues?.Clear();
      }
    }
  }

  /// <summary>
  /// Throws if this instance has been disposed.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void ThrowIfDisposed() {
    if (this._disposed)
      throw new ObjectDisposedException(nameof(ThreadLocal<T>));
  }

  /// <summary>
  /// Finalizes an instance of the <see cref="ThreadLocal{T}"/> class.
  /// </summary>
  ~ThreadLocal() => this.Dispose(false);

}

#endif
