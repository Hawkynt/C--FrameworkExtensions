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

using System.Diagnostics.Contracts;

namespace System.Windows.Forms {
  internal static partial class BindingExtensions {
    /// <summary>
    /// Adds a value converter for a certain type of values.
    /// </summary>
    /// <typeparam name="TType">The type of the value to convert.</typeparam>
    /// <param name="This">This Binding.</param>
    /// <param name="converter">The converter.</param>
    public static void AddTypeConverter<TType>(this Binding This, Func<object, TType> converter) {
      Contract.Requires(This != null);
      Contract.Requires(converter != null);
      This.Format += (s, e) => {
        if (e.DesiredType != typeof(TType))
          return;
        e.Value = converter(e.Value);
      };
    }
  }
}