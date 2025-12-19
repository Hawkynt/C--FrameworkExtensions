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

#if !SUPPORTS_DICTIONARY_ENSURECAPACITY

using System.Reflection;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Collections.Generic;

public static partial class DictionaryPolyfills {

  extension<TKey, TValue>(Dictionary<TKey, TValue> @this) {

    /// <summary>
    /// Ensures that the capacity of this dictionary is at least the specified <paramref name="capacity"/>.
    /// </summary>
    /// <param name="capacity">The minimum capacity to ensure.</param>
    /// <returns>The new capacity of this dictionary.</returns>
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
      return currentCapacity >= capacity ? currentCapacity : _TryResize(@this, capacity);
    }

  }

  // Field names vary by .NET version:
  // .NET Framework 2.0-4.x: entries, buckets
  // .NET Core 2.1+: _entries, _buckets
  private static readonly FieldInfo? _entriesField = typeof(Dictionary<,>).GetField("entries", BindingFlags.NonPublic | BindingFlags.Instance)
                                                     ?? typeof(Dictionary<,>).GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance);

  private static int _GetCapacity<TKey, TValue>(Dictionary<TKey, TValue> dictionary) {
    try {
      var entriesFieldTyped = _entriesField?.DeclaringType!.MakeGenericType(typeof(TKey), typeof(TValue)).GetField(_entriesField.Name, BindingFlags.NonPublic | BindingFlags.Instance);
      var entries = entriesFieldTyped?.GetValue(dictionary) as Array;
      return entries?.Length ?? dictionary.Count;
    } catch {
      return dictionary.Count;
    }
  }

  private static int _TryResize<TKey, TValue>(Dictionary<TKey, TValue> dictionary, int capacity) {
    try {
      var dictType = typeof(Dictionary<TKey, TValue>);

      // In .NET Framework, there's an Initialize(int) method
      var initMethod = dictType.GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Instance, null, [typeof(int)], null);

      // In .NET Core 2.1+, there's a Resize(int, bool) method
      var resizeMethod = dictType.GetMethod("Resize", BindingFlags.NonPublic | BindingFlags.Instance, null, [typeof(int), typeof(bool)], null);

      if (dictionary.Count == 0 && initMethod != null) {
        initMethod.Invoke(dictionary, [capacity]);
        return _GetCapacity(dictionary);
      }

      if (resizeMethod != null) {
        // Call Resize with the new capacity
        resizeMethod.Invoke(dictionary, [capacity, false]);
        return _GetCapacity(dictionary);
      }

      // Fallback: copy all items, clear, reinitialize, and re-add
      if (dictionary.Count <= 0)
        return _GetCapacity(dictionary);

      var items = new KeyValuePair<TKey, TValue>[dictionary.Count];
      ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).CopyTo(items, 0);

      dictionary.Clear();
      initMethod?.Invoke(dictionary, [capacity]);

      foreach (var item in items)
        dictionary.Add(item.Key, item.Value);

      return _GetCapacity(dictionary);

    } catch {
      return dictionary.Count;
    }
  }

}

#endif
