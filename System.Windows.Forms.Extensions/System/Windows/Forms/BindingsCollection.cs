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

using System.Collections.Generic;
#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif
using System.Linq;

namespace System.Windows.Forms {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class BindingsCollectionExtensions {

    /// <summary>
    /// Gets a binding by its bound property.
    /// </summary>
    /// <param name="This">This BindingCollection.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>The first binding that matches the given property name.</returns>
    public static Binding GetBindingByPropertyName(this BindingsCollection This, string propertyName) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
      return (This.GetBindingsByPropertyName(propertyName).First());
    }

    /// <summary>
    /// Gets bindings by their bound property.
    /// </summary>
    /// <param name="This">This BindingCollection.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>A list of bindings that match the given property name.</returns>
    public static IEnumerable<Binding> GetBindingsByPropertyName(this BindingsCollection This, string propertyName) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
      return (This.Cast<Binding>().Where(b => b.PropertyName == propertyName));
    }
  }
}