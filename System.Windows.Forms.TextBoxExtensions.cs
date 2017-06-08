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
using System.Linq;
using word = System.UInt32;
namespace System.Windows.Forms {
  internal static partial class TextBoxExtensions {
    /// <summary>
    /// Appends the text and scrolls.
    /// </summary>
    /// <param name="This">This TextBox.</param>
    /// <param name="text">The text.</param>
    public static void AppendTextAndScroll(this TextBox This, string text) {
      Contract.Requires(This != null);
      This.AppendText(text ?? string.Empty);
    }

    /// <summary>
    /// Keeps the last n lines in the textbox removing whatever is before.
    /// </summary>
    /// <param name="This">This TextBox.</param>
    /// <param name="count">The number of lines to keep.</param>
    public static void KeepLastLines(this TextBox This, word count) {
      Contract.Requires(This != null);
      var lines = This.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
      var linesToRemove = Math.Max(0, lines.Count - (int)count);
      This.Text = string.Empty;
      This.AppendText(string.Join(Environment.NewLine, lines.Skip(linesToRemove)));
    }

    /// <summary>
    /// Keeps the first n lines removing whatever is after them.
    /// </summary>
    /// <param name="This">This TextBox.</param>
    /// <param name="count">The number of lines to keep.</param>
    public static void KeepFirstLines(this TextBox This, word count) {
      Contract.Requires(This != null);
      var lines = This.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
      This.Text = string.Empty;
      This.AppendText(string.Join(Environment.NewLine, lines.Take((int)count)));
    }
  }
}