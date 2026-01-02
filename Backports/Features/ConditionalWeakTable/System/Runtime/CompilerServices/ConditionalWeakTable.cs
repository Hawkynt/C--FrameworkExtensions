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

#if !SUPPORTS_CONDITIONAL_WEAK_TABLE

using System.Collections.Generic;
using System.Threading;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.CompilerServices;

/// <summary>
/// Enables compilers to dynamically attach object fields to managed objects.
/// </summary>
/// <typeparam name="TKey">The reference type to which the field is attached.</typeparam>
/// <typeparam name="TValue">The field's type. This must be a reference type.</typeparam>
/// <remarks>
/// <para>
/// A <see cref="ConditionalWeakTable{TKey,TValue}"/> object is a dictionary that binds a managed object,
/// which is represented by a key, to its attached field, which is represented by a value.
/// </para>
/// <para>
/// The table is weak in that it does not keep the key alive; that is, a key/value entry is removed from
/// the table when the only reference to the key is from the table itself.
/// </para>
/// </remarks>
public sealed partial class ConditionalWeakTable<TKey, TValue>
  where TKey : class
  where TValue : class? {

  /// <summary>
  /// Entry that holds a weak reference to the key and a strong reference to the value.
  /// </summary>
  private sealed class Entry(TKey key, TValue value, int hashCode) {
    public readonly WeakReference<TKey> KeyRef = new(key);
    public TValue Value = value;
    public readonly int HashCode = hashCode;

    public bool TryGetKey(out TKey? key) => this.KeyRef.TryGetTarget(out key);
  }

  /// <summary>
  /// The underlying storage. We use a list of entries and scan it.
  /// For a production implementation, a proper hash table with weak references would be better,
  /// but this is simpler and works correctly.
  /// </summary>
  private readonly List<Entry> _entries = [];

  /// <summary>
  /// Lock for thread safety.
  /// </summary>
  private readonly object _lock = new();

  /// <summary>
  /// Initializes a new instance of the <see cref="ConditionalWeakTable{TKey, TValue}"/> class.
  /// </summary>
  public ConditionalWeakTable() { }

  /// <summary>
  /// Adds a key to the table.
  /// </summary>
  /// <param name="key">The key to add.</param>
  /// <param name="value">The key's property value.</param>
  /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
  /// <exception cref="ArgumentException">An entry with the same key already exists.</exception>
  public void Add(TKey key, TValue value) {
    if (key == null)
      throw new ArgumentNullException(nameof(key));

    lock (this._lock) {
      this.CleanupDeadEntries();

      // Check if key already exists
      var hashCode = RuntimeHelpers.GetHashCode(key);
      foreach (var entry in this._entries)
        if (entry.HashCode == hashCode && entry.TryGetKey(out var existingKey) && ReferenceEquals(existingKey, key))
          throw new ArgumentException("An entry with the same key already exists.", nameof(key));

      this._entries.Add(new(key, value, hashCode));
    }
  }

  /// <summary>
  /// Gets the value of the specified key.
  /// </summary>
  /// <param name="key">The key that represents an object with an attached property.</param>
  /// <param name="value">
  /// When this method returns, contains the attached property value. If <paramref name="key"/> is not found,
  /// <paramref name="value"/> contains the default value.
  /// </param>
  /// <returns><c>true</c> if the key is found; otherwise, <c>false</c>.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
  public bool TryGetValue(TKey key, out TValue? value) {
    if (key == null)
      throw new ArgumentNullException(nameof(key));

    lock (this._lock) {
      var hashCode = RuntimeHelpers.GetHashCode(key);
      foreach (var entry in this._entries)
        if (entry.HashCode == hashCode && entry.TryGetKey(out var existingKey) && ReferenceEquals(existingKey, key)) {
          value = entry.Value;
          return true;
        }
    }

    value = default;
    return false;
  }

  /// <summary>
  /// Removes a key and its value from the table.
  /// </summary>
  /// <param name="key">The key to remove.</param>
  /// <returns><c>true</c> if the key was found and removed; otherwise, <c>false</c>.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
  public bool Remove(TKey key) {
    if (key == null)
      throw new ArgumentNullException(nameof(key));

    lock (this._lock) {
      var hashCode = RuntimeHelpers.GetHashCode(key);
      for (var i = 0; i < this._entries.Count; ++i) {
        var entry = this._entries[i];
        if (entry.HashCode == hashCode && entry.TryGetKey(out var existingKey) && ReferenceEquals(existingKey, key)) {
          this._entries.RemoveAt(i);
          return true;
        }
      }
    }

    return false;
  }

  /// <summary>
  /// Atomically searches for a specified key in the table and returns the corresponding value.
  /// If the key does not exist in the table, the method invokes the default constructor of the class
  /// that represents the table's value and adds a key/value pair to the table.
  /// </summary>
  /// <param name="key">The key to search for.</param>
  /// <returns>
  /// The value that corresponds to <paramref name="key"/>, if <paramref name="key"/> already exists in the table;
  /// otherwise, a new value created by the default constructor of <typeparamref name="TValue"/>.
  /// </returns>
  /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
  /// <exception cref="MissingMethodException">
  /// The class that represents the table's value does not define a parameterless constructor.
  /// </exception>
  public TValue GetOrCreateValue(TKey key) => this.GetValue(key, _ => Activator.CreateInstance<TValue>());

  /// <summary>
  /// Atomically searches for a specified key in the table and returns the corresponding value.
  /// If the key does not exist in the table, the method invokes a callback method to create a value
  /// that is bound to the specified key.
  /// </summary>
  /// <param name="key">The key to search for.</param>
  /// <param name="createValueCallback">
  /// A delegate to a method that can create a value for the given <paramref name="key"/>. It has a single
  /// parameter of type <typeparamref name="TKey"/>, and returns a value of type <typeparamref name="TValue"/>.
  /// </param>
  /// <returns>
  /// The value attached to <paramref name="key"/>, if <paramref name="key"/> already exists in the table;
  /// otherwise, the new value returned by the <paramref name="createValueCallback"/> delegate.
  /// </returns>
  /// <exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="createValueCallback"/> is null.</exception>
  public TValue GetValue(TKey key, CreateValueCallback createValueCallback) {
    if (key == null)
      throw new ArgumentNullException(nameof(key));
    if (createValueCallback == null)
      throw new ArgumentNullException(nameof(createValueCallback));

    lock (this._lock) {
      this.CleanupDeadEntries();

      var hashCode = RuntimeHelpers.GetHashCode(key);

      // Try to find existing
      foreach (var entry in this._entries)
        if (entry.HashCode == hashCode && entry.TryGetKey(out var existingKey) && ReferenceEquals(existingKey, key))
          return entry.Value;

      // Create new value
      var value = createValueCallback(key);
      this._entries.Add(new(key, value, hashCode));
      return value;
    }
  }

  /// <summary>
  /// Removes entries with dead (garbage collected) keys.
  /// </summary>
  private void CleanupDeadEntries() {
    for (var i = this._entries.Count - 1; i >= 0; --i)
      if (!this._entries[i].TryGetKey(out _))
        this._entries.RemoveAt(i);
  }

  /// <summary>
  /// Represents a method that creates a non-default value to add as part of a key/value pair
  /// to a <see cref="ConditionalWeakTable{TKey, TValue}"/> object.
  /// </summary>
  /// <param name="key">The key that belongs to the value to create.</param>
  /// <returns>An instance of a reference type that represents the value to attach to the specified key.</returns>
  public delegate TValue CreateValueCallback(TKey key);

}

/// <summary>
/// Weak reference that can work with generics.
/// </summary>
/// <typeparam name="T">The type of the target object.</typeparam>
internal sealed class WeakReference<T>(T target)
  where T : class {

  private readonly WeakReference _weakReference = new(target);

  public bool TryGetTarget(out T? target) {
    var obj = this._weakReference.Target;
    if (obj is T typedTarget) {
      target = typedTarget;
      return true;
    }

    target = default;
    return false;
  }

}

#endif
