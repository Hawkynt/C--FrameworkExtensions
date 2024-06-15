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

using System.Diagnostics;
using System.Linq;
using Guard;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Collections.Generic;

public static partial class HashSetExtensions {
  #region nested types

  public enum ChangeType {
    Equal = 0,
    Added = 2,
    Removed = 3,
  }

  public interface IChangeSet<out TItem> {
    ChangeType Type { get; }
    TItem Item { get; }
  }

  /// <summary>
  ///   Changeset between two hashsets.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  private sealed class ChangeSet<TItem>(ChangeType type, TItem item) : IChangeSet<TItem> {
    public ChangeType Type { get; } = type;
    public TItem Item { get; } = item;
  }

  #endregion

  /// <summary>
  ///   Compares two hashes against each other.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This HashSet.</param>
  /// <param name="other">The other HashSet.</param>
  /// <returns></returns>
  /// <exception cref="NullReferenceException"></exception>
  /// <exception cref="ArgumentNullException"></exception>
  [DebuggerStepThrough]
  public static IEnumerable<IChangeSet<TItem>> CompareTo<TItem>(this HashSet<TItem> @this, HashSet<TItem> other) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(other);

    return Invoke(@this, other);

    static IEnumerable<IChangeSet<TItem>> Invoke(HashSet<TItem> @this, HashSet<TItem> other) {
      var keys = @this.Concat(other).Distinct(@this.Comparer);
      foreach (var key in keys) {
        if (!other.Contains(key)) {
          yield return new ChangeSet<TItem>(ChangeType.Added, key);
          continue;
        }

        if (!@this.Contains(key)) {
          yield return new ChangeSet<TItem>(ChangeType.Removed, key);
          continue;
        }

        yield return new ChangeSet<TItem>(ChangeType.Equal, key);
      }
    }
  }

  /// <summary>
  ///   Determines whether the specified HashSet does not contain the given item.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This HashSet.</param>
  /// <param name="item">The item.</param>
  /// <returns><c>true</c> if the item is not in the set; otherwise, <c>false</c>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  [DebuggerStepThrough]
  public static bool ContainsNot<TItem>(this HashSet<TItem> @this, TItem item) {
    Against.ThisIsNull(@this);

    return !@this.Contains(item);
  }

  /// <summary>
  ///   Tries to add a value.
  /// </summary>
  /// <typeparam name="TItem">The type of the values.</typeparam>
  /// <param name="this">This HashSet.</param>
  /// <param name="value">The value.</param>
  /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
  /// <remarks></remarks>
  [DebuggerStepThrough]
  public static bool TryAdd<TItem>(this HashSet<TItem> @this, TItem value) {
    Against.ThisIsNull(@this);

    return @this.Add(value);
  }

  /// <summary>
  ///   Tries to remove the given item.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This HashSet.</param>
  /// <param name="item">The item.</param>
  /// <returns></returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  [DebuggerStepThrough]
  public static bool TryRemove<TItem>(this HashSet<TItem> @this, TItem item) {
    Against.ThisIsNull(@this);

    return @this.Remove(item);
  }
}
