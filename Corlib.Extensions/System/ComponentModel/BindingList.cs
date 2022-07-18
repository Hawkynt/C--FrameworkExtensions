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

#if NET40_OR_GREATER || NET5_0_OR_GREATER || NETCOREAPP || NETSTANDARD
#define SUPPORTS_CONTRACTS 
#endif

using System.Collections.Generic;
#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif
using System.Linq;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace System.ComponentModel {
  /// <summary>
  /// 
  /// </summary>

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class BindingListExtensions {

    /// <summary>
    /// Copies the content of the BindingList to an array.
    /// </summary>
    /// <typeparam name="TItem">The type of items.</typeparam>
    /// <param name="this">This BindingList.</param>
    /// <returns>A copy of the list.</returns>
    public static TItem[] ToArray<TItem>(this BindingList<TItem> @this) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
#endif
      var result = new TItem[@this.Count];
      @this.CopyTo(result, 0);
      return (result);
    }

    /// <summary>
    /// Adds the given elements.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="this">This BindingList.</param>
    /// <param name="items">The items.</param>
    public static void AddRange<TItem>(this BindingList<TItem> @this, IEnumerable<TItem> items) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
      Contract.Requires(items != null);
#endif
      foreach (var item in items)
        @this.Add(item);
    }

    /// <summary>
    /// Moves the given items to the front of the list.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="this">This BindingList.</param>
    /// <param name="items">The items.</param>
    public static void MoveToFront<TItem>(this BindingList<TItem> @this, IEnumerable<TItem> items) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
      Contract.Requires(items != null);
#endif
      var raiseEvents = @this.RaiseListChangedEvents;
      try {
        @this.RaiseListChangedEvents = false;
        foreach (var item in items.Reverse()) {
          @this.Remove(item);
          @this.Insert(0, item);
        }
      } finally {
        @this.RaiseListChangedEvents = raiseEvents;
        @this.ResetBindings();
      }
    }

    /// <summary>
    /// Moves the given items to the back of the list.
    /// </summary>
    /// <typeparam name="TItem">The type of the items</typeparam>
    /// <param name="this">This BindingList.</param>
    /// <param name="items">The items.</param>
    public static void MoveToBack<TItem>(this BindingList<TItem> @this, IEnumerable<TItem> items) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
      Contract.Requires(items != null);
#endif
      var raiseEvents = @this.RaiseListChangedEvents;
      try {
        @this.RaiseListChangedEvents = false;
        foreach (var item in items) {
          @this.Remove(item);
          @this.Add(item);
        }
      } finally {
        @this.RaiseListChangedEvents = raiseEvents;
        @this.ResetBindings();
      }
    }

    /// <summary>
    /// Moves the given items relative.
    /// </summary>
    /// <typeparam name="TItem">The type of the items</typeparam>
    /// <param name="this">This BindingList.</param>
    /// <param name="items">The items.</param>
    /// <param name="delta">The delta.</param>
    public static void MoveRelative<TItem>(this BindingList<TItem> @this, IEnumerable<TItem> items, int delta) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
      Contract.Requires(items != null);
#endif
      if (delta == 0)
        return;

      var raiseEvents = @this.RaiseListChangedEvents;
      try {
        @this.RaiseListChangedEvents = false;

        var count = @this.Count - 1;

        if (delta < 0) {
          var start = 0;
          foreach (var item in items) {
            var index = @this.IndexOf(item);
            if (index < 0)
              continue;

            @this.RemoveAt(index);
            var newIndex = index + delta;
            @this.Insert(newIndex < 0 ? start++ : newIndex, item);
          }
        } else {
          var end = count;
          foreach (var item in items.Reverse()) {
            var index = @this.IndexOf(item);
            if (index < 0)
              continue;

            @this.RemoveAt(index);
            var newIndex = index + delta;
            if (newIndex > end) {
#if SUPPORTS_CONTRACTS
              Contract.Assume(end >= 0);
#endif
              @this.Insert(end--, item);
            } else
              @this.Insert(newIndex, item);
          }
        }


      } finally {
        @this.RaiseListChangedEvents = raiseEvents;
        @this.ResetBindings();
      }
    }

    /// <summary>
    /// Replaces all elements.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this">This BindingList.</param>
    /// <param name="items">The items.</param>
    public static void ReplaceAll<T>(this BindingList<T> @this, IEnumerable<T> items) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
      Contract.Requires(items != null);
#endif
      var oldState = @this.RaiseListChangedEvents;
      try {
        @this.RaiseListChangedEvents = false;
        @this.Clear();
        foreach (var item in items)
          @this.Add(item);
      } finally {
        @this.RaiseListChangedEvents = oldState;
        if (oldState)
          @this.ResetBindings();
      }
    }

    /// <summary>
    /// Refreshes all items int list.
    /// </summary>
    /// <typeparam name="T">The type of the items</typeparam>
    /// <param name="this">This BindingList.</param>
    /// <param name="items">The updated item list.</param>
    /// <param name="keyGetter">The key getter to compare what items are added/removed/updated.</param>
    /// <param name="itemUpdateMethod">The item update method; takes the old and new item reference and returns the updated item.</param>
    public static void RefreshAll<T>(this BindingList<T> @this, IEnumerable<T> items, Func<T, string> keyGetter, Func<T, T, T> itemUpdateMethod) {
      var oldState = @this.RaiseListChangedEvents;
      try {
        @this.RaiseListChangedEvents = false;
        var oldKeys = @this.ToDictionary(keyGetter);
        var newKeys = items.ToDictionary(keyGetter);

        // remove not longer needed items
        foreach (var key in oldKeys.Keys.Where(k => !newKeys.ContainsKey(k)))
          @this.Remove(oldKeys[key]);

        foreach (var key in newKeys.Keys) {

          // add new items
          if (!oldKeys.ContainsKey(key)) {
            @this.Add(newKeys[key]);
            continue;
          }

          // update items
          var oldItem = oldKeys[key];
          var newItem = itemUpdateMethod(oldItem, newKeys[key]);
          if (!ReferenceEquals(oldItem, newItem))
            @this[@this.IndexOf(oldItem)] = newItem;
        }

      } finally {
        @this.RaiseListChangedEvents = oldState;
        if (oldState)
          @this.ResetBindings();
      }
    }

    public static int RemoveWhere<TItem>(this BindingList<TItem> @this, Predicate<TItem> selector) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
      Contract.Requires(selector != null);
#endif
      var result = 0;
      var items = @this.Where(i => selector(i)).ToArray();
      foreach (var item in items) {
        @this.Remove(item);
        ++result;
      }

      return result;
    }
  }
}