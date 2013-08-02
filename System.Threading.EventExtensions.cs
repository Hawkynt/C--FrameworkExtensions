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

namespace System.Threading {
  internal static partial class EventExtensions {
    /// <summary>
    /// Invoke all handlers in parallel.
    /// </summary>
    /// <typeparam name="T">Type of event handlers.</typeparam>
    /// <param name="evtA">The event.</param>
    /// <param name="objSender">The sender.</param>
    /// <param name="objArgs">The args.</param>
    public static void AsyncInvoke<T>(this EventHandler<T> evtA, object objSender, T objArgs) where T : EventArgs {
      EventHandler<T> evtCopy = evtA;
      evtCopy.GetInvocationList().ForEach(ptrDel => {
        Action ptrT = () => ptrDel.DynamicInvoke(objSender, objArgs);
        ptrT.BeginInvoke(ptrT.EndInvoke, null);
      });
    }

    /// <summary>
    /// Invoke all handlers in parallel.
    /// </summary>
    /// <param name="evtA">The event.</param>
    /// <param name="arrArgs">The args.</param>
    public static void AsyncInvoke(this MulticastDelegate evtA, params object[] arrArgs) {
      MulticastDelegate evtCopy = evtA;
      evtCopy.GetInvocationList().ForEach(ptrDel => {
        Action ptrT = () => ptrDel.DynamicInvoke(arrArgs);
        ptrT.BeginInvoke(ptrT.EndInvoke, null);
      });
    }
  }
}
