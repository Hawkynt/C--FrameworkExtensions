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
using word = System.UInt32;
namespace System.Windows.Forms {
  internal static partial class RichTextBoxExtensions {
    /// <summary>
    /// Appends the text and scrolls.
    /// </summary>
    /// <param name="This">This TextBox.</param>
    /// <param name="text">The text.</param>
    public static void AppendTextAndScroll(this RichTextBox This, string text) {
      Contract.Requires(This != null);
      This.AppendText(text ?? string.Empty);
      This.ScrollToEnd();
    }

    /// <summary>
    /// Scrolls to the end of the RTB.
    /// </summary>
    /// <param name="This">This RichTextBox.</param>
    public static void ScrollToEnd(this RichTextBox This) {
      Contract.Requires(This != null);
      var start = This.SelectionStart;
      var length = This.SelectionLength;
      This.SelectionStart = This.Text.Length;
      This.ScrollToCaret();
      This.SelectionStart = start;
      This.SelectionLength = length;
    }
  }
}