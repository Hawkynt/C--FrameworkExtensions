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

#if !SUPPORTS_HASHSET_ENSURECAPACITY && SUPPORTS_HASHSET

using System.Reflection;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Collections.Generic;

public static partial class HashSetPolyfills {

  extension<T>(HashSet<T> @this) {

    /// <summary>
    /// Ensures that the capacity of this hash set is at least the specified <paramref name="capacity"/>.
    /// </summary>
    /// <param name="capacity">The minimum capacity to ensure.</param>
    /// <returns>The new capacity of this hash set.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than 0.</exception>
    /// <remarks>
    /// This polyfill uses reflection to access internal fields. If the internal structure cannot be accessed,
    /// it falls back to returning the count as a lower bound estimate.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int EnsureCapacity(int capacity) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentOutOfRangeException.ThrowIfNegative(capacity);

      var currentCapacity = _GetCapacity(@this);
      if (currentCapacity >= capacity)
        return currentCapacity;

      // To grow the HashSet, we need to trigger its internal resize
      // Since we can't directly resize, we work around by temporarily adding items
      // then removing them, or use reflection to call internal methods
      return _TryResize(@this, capacity);
    }

  }

  // Field names vary by .NET version:
  // .NET Framework 3.5-4.x: m_slots, m_buckets, m_count, m_lastIndex
  // .NET Core 2.1+: _entries, _buckets, _count
  private static readonly FieldInfo? _slotsField = typeof(HashSet<>).GetField("m_slots", BindingFlags.NonPublic | BindingFlags.Instance)
                                                   ?? typeof(HashSet<>).GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance);

  private static readonly FieldInfo? _bucketsField = typeof(HashSet<>).GetField("m_buckets", BindingFlags.NonPublic | BindingFlags.Instance)
                                                     ?? typeof(HashSet<>).GetField("_buckets", BindingFlags.NonPublic | BindingFlags.Instance);

  private static readonly FieldInfo? _lastIndexField = typeof(HashSet<>).GetField("m_lastIndex", BindingFlags.NonPublic | BindingFlags.Instance);

  private static int _GetCapacity<T>(HashSet<T> hashSet) {
    try {
      var slotsFieldTyped = _slotsField?.DeclaringType!.MakeGenericType(typeof(T)).GetField(_slotsField.Name, BindingFlags.NonPublic | BindingFlags.Instance);
      var slots = slotsFieldTyped?.GetValue(hashSet) as Array;
      return slots?.Length ?? hashSet.Count;
    } catch {
      return hashSet.Count;
    }
  }

  private static int _TryResize<T>(HashSet<T> hashSet, int capacity) {
    try {
      // Try to find and call the internal Initialize or Resize method
      var hashSetType = typeof(HashSet<T>);

      // In .NET Framework, there's an Initialize(int) method
      var initMethod = hashSetType.GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Instance, null, [typeof(int)], null);
      if (initMethod != null && hashSet.Count == 0) {
        initMethod.Invoke(hashSet, [capacity]);
        return _GetCapacity(hashSet);
      }

      // If we have items, we need to preserve them
      // Copy all items, clear, initialize with new capacity, and re-add
      if (hashSet.Count > 0) {
        var items = new T[hashSet.Count];
        hashSet.CopyTo(items);

        // Try to clear and reinitialize
        hashSet.Clear();

        if (initMethod != null)
          initMethod.Invoke(hashSet, [capacity]);

        foreach (var item in items)
          hashSet.Add(item);

        return _GetCapacity(hashSet);
      }

      // If no Initialize method and empty, try other approaches
      // Fallback: just return current capacity
      return _GetCapacity(hashSet);
    } catch {
      // If reflection fails, return count as lower bound
      return hashSet.Count;
    }
  }

}

#endif
