#region (c)2010-2020 Hawkynt
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

using System.Collections.Generic;
#if NETFX_4
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
  internal static partial class BindingListExtensions {

    /// <summary>
    /// Copies the content of the BindingList to an array.
    /// </summary>
    /// <typeparam name="TItem">The type of items.</typeparam>
    /// <param name="This">This BindingList.</param>
    /// <returns>A copy of the list.</returns>
    public static TItem[] ToArray<TItem>(this BindingList<TItem> This) {
#if NETFX_4
      Contract.Requires(This != null);
#endif
      var result = new TItem[This.Count];
      This.CopyTo(result, 0);
      return (result);
    }

    /// <summary>
    /// Adds the given elements.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="This">This BindingList.</param>
    /// <param name="items">The items.</param>
    public static void AddRange<TItem>(this BindingList<TItem> This, IEnumerable<TItem> items) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(items != null);
#endif
      foreach (var item in items)
        This.Add(item);
    }

    /// <summary>
    /// Moves the given items to the front of the list.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="This">This BindingList.</param>
    /// <param name="items">The items.</param>
    public static void MoveToFront<TItem>(this BindingList<TItem> This, IEnumerable<TItem> items) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(items != null);
#endif
      var raiseEvents = This.RaiseListChangedEvents;
      try {
        This.RaiseListChangedEvents = false;
        foreach (var item in items.Reverse()) {
          This.Remove(item);
          This.Insert(0, item);
        }
      } finally {
        This.RaiseListChangedEvents = raiseEvents;
        This.ResetBindings();
      }
    }

    /// <summary>
    /// Moves the given items to the back of the list.
    /// </summary>
    /// <typeparam name="TItem">The type of the items</typeparam>
    /// <param name="This">This BindingList.</param>
    /// <param name="items">The items.</param>
    public static void MoveToBack<TItem>(this BindingList<TItem> This, IEnumerable<TItem> items) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(items != null);
#endif
      var raiseEvents = This.RaiseListChangedEvents;
      try {
        This.RaiseListChangedEvents = false;
        foreach (var item in items) {
          This.Remove(item);
          This.Add(item);
        }
      } finally {
        This.RaiseListChangedEvents = raiseEvents;
        This.ResetBindings();
      }
    }

    /// <summary>
    /// Moves the given items relative.
    /// </summary>
    /// <typeparam name="TItem">The type of the items</typeparam>
    /// <param name="This">This BindingList.</param>
    /// <param name="items">The items.</param>
    /// <param name="delta">The delta.</param>
    public static void MoveRelative<TItem>(this BindingList<TItem> This, IEnumerable<TItem> items, int delta) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(items != null);
#endif
      if (delta == 0)
        return;

      var raiseEvents = This.RaiseListChangedEvents;
      try {
        This.RaiseListChangedEvents = false;

        var count = This.Count - 1;

        if (delta < 0) {
          var start = 0;
          foreach (var item in items) {
            var index = This.IndexOf(item);
            if (index < 0)
              continue;

            This.RemoveAt(index);
            var newIndex = index + delta;
            This.Insert(newIndex < 0 ? start++ : newIndex, item);
          }
        } else {
          var end = count;
          foreach (var item in items.Reverse()) {
            var index = This.IndexOf(item);
            if (index < 0)
              continue;

            This.RemoveAt(index);
            var newIndex = index + delta;
            if (newIndex > end) {
#if NETFX_4
              Contract.Assume(end >= 0);
#endif
              This.Insert(end--, item);
            } else
              This.Insert(newIndex, item);
          }
        }


      } finally {
        This.RaiseListChangedEvents = raiseEvents;
        This.ResetBindings();
      }
    }

    /// <summary>
    /// Replaces all elements.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="This">This BindingList.</param>
    /// <param name="items">The items.</param>
    public static void ReplaceAll<T>(this BindingList<T> This, IEnumerable<T> items) {
#if NETFX_4
      Contract.Requires(This != null);
      Contract.Requires(items != null);
#endif
      var oldState = This.RaiseListChangedEvents;
      try {
        This.RaiseListChangedEvents = false;
        This.Clear();
        foreach (var item in items)
          This.Add(item);
      } finally {
        This.RaiseListChangedEvents = oldState;
        if (oldState)
          This.ResetBindings();
      }
    }
  }
}