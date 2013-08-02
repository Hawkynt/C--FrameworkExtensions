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

namespace System.ComponentModel {
  internal static partial class PropertyChangedExtensions {

    /// <summary>
    /// The synchornization context to use for all safe invocations if any.
    /// </summary>
    public static ISynchronizeInvoke SynchronizationContext;

    /// <summary>
    /// Safely invokes the event.
    /// </summary>
    /// <param name="This">This event.</param>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
    public static void SafeInvoke(this PropertyChangedEventHandler This, object sender, PropertyChangedEventArgs e) {
      if (This == null)
        return;

      var context = SynchronizationContext;
      if (context != null) {
        context.Invoke(new Action(() => This(sender, e)), null);
        return;
      }

      foreach (var subscriber in This.GetInvocationList()) {
        var sync = subscriber.Target as ISynchronizeInvoke;
        if (sync != null) {
          if (sync.InvokeRequired) {
            sync.Invoke(subscriber, new[] { sender, e });
            continue;
          }
        }
        subscriber.DynamicInvoke(sender, e);

      }
    }
  }
}