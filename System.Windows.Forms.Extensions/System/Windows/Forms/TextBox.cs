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

#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif
using System.Linq;
using System.Windows.Form.Extensions;
using word = System.UInt32;

namespace System.Windows.Forms;

public static partial class TextBoxExtensions {
  /// <summary>
  ///   Appends the text and scrolls.
  /// </summary>
  /// <param name="This">This TextBox.</param>
  /// <param name="text">The text.</param>
  public static void AppendTextAndScroll(this TextBox This, string text) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
    This.AppendText(text ?? string.Empty);
  }

  /// <summary>
  ///   Keeps the last n lines in the textbox removing whatever is before.
  /// </summary>
  /// <param name="This">This TextBox.</param>
  /// <param name="count">The number of lines to keep.</param>
  public static void KeepLastLines(this TextBox This, word count) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
    var lines = This.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
    var linesToRemove = Math.Max(0, lines.Count - (int)count);
    This.Text = string.Empty;
    This.AppendText(lines.Skip(linesToRemove)._FOS_Join(Environment.NewLine));
  }

  /// <summary>
  ///   Keeps the first n lines removing whatever is after them.
  /// </summary>
  /// <param name="This">This TextBox.</param>
  /// <param name="count">The number of lines to keep.</param>
  public static void KeepFirstLines(this TextBox This, word count) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
    var lines = This.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
    This.Text = string.Empty;
    This.AppendText(lines.Take((int)count)._FOS_Join(Environment.NewLine));
  }

  /// <summary>
  ///   Duplicates the given TextBox
  /// </summary>
  /// <param name="this">This TextBox</param>
  /// <param name="newName">The new name</param>
  /// <returns>The duplicated TextBox</returns>
  public static TextBox Duplicate(this TextBox @this, string newName = null) {
    newName = newName ?? new Guid().ToString();

    var newTextBox = new TextBox {
      AllowDrop = @this.AllowDrop,
      Anchor = @this.Anchor,
      AutoSize = @this.AutoSize,
      BackColor = @this.BackColor,
      BackgroundImage = @this.BackgroundImage,
      AutoScrollOffset = @this.AutoScrollOffset,
      BackgroundImageLayout = @this.BackgroundImageLayout,
      Bounds = @this.Bounds,
      Capture = @this.Capture,
      Text = @this.Text,
      Tag = @this.Tag,
      CausesValidation = @this.CausesValidation,
      ClientSize = @this.ClientSize,
#if !NET5_0_OR_GREATER && !NETSTANDARD && !NETCOREAPP
      ContextMenu = @this.ContextMenu,
#endif
      Cursor = @this.Cursor,
      Enabled = @this.Enabled,
      Visible = @this.Visible,
      Dock = @this.Dock,
      Font = @this.Font,
      ForeColor = @this.ForeColor,
      ContextMenuStrip = @this.ContextMenuStrip,
      Location = @this.Location,
      Size = @this.Size,
      TextAlign = @this.TextAlign,
      Padding = @this.Padding,
      Margin = @this.Margin,
      UseWaitCursor = @this.UseWaitCursor,
      Name = newName,
      RightToLeft = @this.RightToLeft,
      MinimumSize = @this.MinimumSize,
      MaximumSize = @this.MaximumSize,
      AcceptsReturn = @this.AcceptsReturn,
      AcceptsTab = @this.AcceptsTab,
      AccessibleDescription = @this.AccessibleDescription,
      AccessibleName = @this.AccessibleName,
      AccessibleRole = @this.AccessibleRole,
      BorderStyle = @this.BorderStyle,
      CharacterCasing = @this.CharacterCasing,
      AccessibleDefaultActionDescription = @this.AccessibleDefaultActionDescription,
    };

    return newTextBox;
  }
}
