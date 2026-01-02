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

#if !SUPPORTS_CONDITIONAL_WEAK_TABLE_TRYADD

namespace System.Runtime.CompilerServices;

#if !SUPPORTS_CONDITIONAL_WEAK_TABLE
// Our polyfill is in charge (pre-.NET 4.0) - add TryAdd as partial class member
public sealed partial class ConditionalWeakTable<TKey, TValue> {

  /// <summary>
  /// Adds a key to the table if it doesn't already exist.
  /// </summary>
  /// <param name="key">The key to add.</param>
  /// <param name="value">The key's property value.</param>
  /// <returns><c>true</c> if the key was added; <c>false</c> if the key already exists.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
  public bool TryAdd(TKey key, TValue value) {
    if (key == null)
      throw new ArgumentNullException(nameof(key));

    lock (this._lock) {
      this.CleanupDeadEntries();

      // Check if key already exists
      var hashCode = RuntimeHelpers.GetHashCode(key);
      foreach (var entry in this._entries)
        if (entry.HashCode == hashCode && entry.TryGetKey(out var existingKey) && ReferenceEquals(existingKey, key))
          return false;

      this._entries.Add(new(key, value, hashCode));
      return true;
    }
  }

}

#else
// BCL ConditionalWeakTable is in charge (.NET 4.0-7.x)
// Use C# 14 extension block to add TryAdd

public static class ConditionalWeakTablePolyfills {

  extension<TKey, TValue>(ConditionalWeakTable<TKey, TValue> @this)
    where TKey : class
    where TValue : class {

    /// <summary>
    /// Adds a key to the table if it doesn't already exist.
    /// </summary>
    /// <param name="key">The key to add.</param>
    /// <param name="value">The key's property value.</param>
    /// <returns><c>true</c> if the key was added; <c>false</c> if the key already exists.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
    public bool TryAdd(TKey key, TValue value) {
      if (key == null)
        throw new ArgumentNullException(nameof(key));

      // BCL ConditionalWeakTable is thread-safe, we need to synchronize check-and-add
      lock (@this) {
        if (@this.TryGetValue(key, out _))
          return false;
        @this.Add(key, value);
        return true;
      }
    }

  }

}

#endif

#endif
