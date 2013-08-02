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
using System.Globalization;

namespace System.Windows.Controls {
  internal static partial class TextBoxExtensions {
    /// <summary>
    /// Tries to parse the content into an int.
    /// </summary>
    /// <param name="This">This TextBox.</param>
    /// <param name="value">The value.</param>
    /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
    public static bool TryParseInt(this TextBox This, ref int value) {
      Contract.Requires(This != null);
      var text = This.Text;
      if (string.IsNullOrWhiteSpace(text))
        return (false);
      int temp;
      if (!int.TryParse(text, out temp))
        return (false);
      value = temp;
      return (true);
    }

    /// <summary>
    /// Tries to parse the content into an int.
    /// </summary>
    /// <param name="This">This TextBox.</param>
    /// <param name="style">The style.</param>
    /// <param name="provider">The format provider.</param>
    /// <param name="value">The value.</param>
    /// <returns>
    ///   <c>true</c> on success; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryParseInt(this TextBox This, NumberStyles style, IFormatProvider provider, ref int value) {
      Contract.Requires(This != null);
      var text = This.Text;
      if (string.IsNullOrWhiteSpace(text))
        return (false);
      int temp;
      if (!int.TryParse(text, style, provider, out temp))
        return (false);
      value = temp;
      return (true);
    }
  }
}
