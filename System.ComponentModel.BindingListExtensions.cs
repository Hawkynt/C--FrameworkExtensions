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
using System.Diagnostics.Contracts;

namespace System.ComponentModel {
  internal static partial class BindingListExtensions {
    /// <summary>
    /// Copies the content of the BindingList to an array.
    /// </summary>
    /// <typeparam name="T">The type of items.</typeparam>
    /// <param name="This">This BindingList.</param>
    /// <returns>A copy of the list.</returns>
    public static T[] ToArray<T>(this BindingList<T> This) {
      Contract.Requires(This != null);
      var result = new T[This.Count];
      This.CopyTo(result, 0);
      return (result);
    }

    /// <summary>
    /// Adds the given elements.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="This">This BindingList.</param>
    /// <param name="items">The items.</param>
    public static void AddRange<T>(this BindingList<T> This, IEnumerable<T> items) {
      Contract.Requires(This != null);
      Contract.Requires(items != null);
      var oldState = This.RaiseListChangedEvents;
      try {
        This.RaiseListChangedEvents = false;
        foreach (var item in items)
          This.Add(item);
      } finally {
        This.RaiseListChangedEvents = oldState;
        if (oldState)
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
      Contract.Requires(This != null);
      Contract.Requires(items != null);
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