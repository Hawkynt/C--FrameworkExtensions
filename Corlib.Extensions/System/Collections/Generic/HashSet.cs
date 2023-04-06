#region (c)2010-2042 Hawkynt
/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software:
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.
    If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System.Diagnostics;
using System.Linq;
#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif
#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace System.Collections.Generic;

#if COMPILE_TO_EXTENSION_DLL
public
#else
internal
#endif
static partial class HashSetExtensions {

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
  /// Changeset between two hashsets.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  private class ChangeSet<TItem> : IChangeSet<TItem> {
    public ChangeSet(ChangeType type, TItem item) {
      this.Item = item;
      this.Type = type;
    }
    public ChangeType Type { get; }
    public TItem Item { get; }
  }

  #endregion

  /// <summary>
  /// Compares two hashes against each other.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This HashSet.</param>
  /// <param name="other">The other HashSet.</param>
  /// <returns></returns>
  /// <exception cref="NullReferenceException"></exception>
  /// <exception cref="ArgumentNullException"></exception>
  [DebuggerStepThrough]
  public static IEnumerable<IChangeSet<TItem>> CompareTo<TItem>(this HashSet<TItem> @this, HashSet<TItem> other) {
    Guard.Against.ThisIsNull(@this);
    Guard.Against.ArgumentIsNull(other);
    
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

  /// <summary>
  /// Determines whether the specified HashSet does not contain the given item.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This HashSet.</param>
  /// <param name="item">The item.</param>
  /// <returns><c>true</c> if the item is not in the set; otherwise, <c>false</c>.</returns>
#if SUPPORTS_CONTRACTS
  [Pure]
#endif
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  [DebuggerStepThrough]
  public static bool ContainsNot<TItem>(this HashSet<TItem> @this, TItem item) {
    Guard.Against.ThisIsNull(@this);
    
    return !@this.Contains(item);
  }

  /// <summary>
  /// Tries to add a value.
  /// </summary>
  /// <typeparam name="TItem">The type of the values.</typeparam>
  /// <param name="this">This HashSet.</param>
  /// <param name="value">The value.</param>
  /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
  /// <remarks></remarks>
  [DebuggerStepThrough]
  public static bool TryAdd<TItem>(this HashSet<TItem> @this, TItem value) {
    Guard.Against.ThisIsNull(@this);

    if (@this.Contains(value))
      return false;

    @this.Add(value);
    return true;
  }

  /// <summary>
  /// Tries to remove the given item.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <param name="this">This HashSet.</param>
  /// <param name="item">The item.</param>
  /// <returns></returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  [DebuggerStepThrough]
  public static bool TryRemove<TItem>(this HashSet<TItem> @this, TItem item) {
    Guard.Against.ThisIsNull(@this);
    
    return @this.Remove(item);
  }
  
}
