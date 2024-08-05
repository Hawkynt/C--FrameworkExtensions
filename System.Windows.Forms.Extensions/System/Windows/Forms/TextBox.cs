#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY without even the implied
// warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the LICENSE file for more details.
// 
// You should have received a copy of the License along with Hawkynt's
// .NET Framework extensions. If not, see
// <https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/LICENSE>.

#endregion

using System.Linq;
using System.Windows.Form.Extensions;
using Guard;

namespace System.Windows.Forms;

public static partial class TextBoxExtensions {

  /// <summary>
  /// Appends text to the <see cref="TextBox"/> and scrolls to the end.
  /// </summary>
  /// <param name="this">This <see cref="TextBox"/> instance.</param>
  /// <param name="text">The text to append.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// TextBox textBox = new TextBox();
  /// textBox.AppendTextAndScroll("Hello, World!");
  /// // The TextBox now contains the text "Hello, World!" and is scrolled to the end.
  /// </code>
  /// </example>
  public static void AppendTextAndScroll(this TextBox @this, string text) {
    Against.ThisIsNull(@this);

    @this.AppendText(text ?? string.Empty);
  }

  /// <summary>
  /// Keeps only the last specified number of lines in the <see cref="TextBox"/>.
  /// </summary>
  /// <param name="this">This <see cref="TextBox"/> instance.</param>
  /// <param name="count">The maximum number of lines to keep. If fewer lines are present, all lines are kept.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// TextBox textBox = new TextBox();
  /// textBox.AppendText("Line 1\r\nLine 2\r\nLine 3\r\nLine 4\r\nLine 5");
  /// textBox.KeepLastLines(3);
  /// // The TextBox now contains only the last 3 lines: "Line 3", "Line 4", "Line 5"
  /// </code>
  /// </example>
  public static void KeepLastLines(this TextBox @this, uint count) {
    Against.ThisIsNull(@this);

    var lines = @this.Text.Split([Environment.NewLine], StringSplitOptions.None).ToList();
    var linesToRemove = Math.Max(0, lines.Count - (int)count);
    @this.Text = string.Empty;
    @this.AppendText(lines.Skip(linesToRemove)._FOS_Join(Environment.NewLine));
  }

  /// <summary>
  /// Keeps only the first specified number of lines in the <see cref="TextBox"/>.
  /// </summary>
  /// <param name="this">This <see cref="TextBox"/> instance.</param>
  /// <param name="count">The maximum number of lines to keep. If fewer lines are present, all lines are kept.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// TextBox textBox = new TextBox();
  /// textBox.AppendText("Line 1\r\nLine 2\r\nLine 3\r\nLine 4\r\nLine 5");
  /// textBox.KeepFirstLines(3);
  /// // The TextBox now contains only the first 3 lines: "Line 1", "Line 2", "Line 3"
  /// </code>
  /// </example>
  public static void KeepFirstLines(this TextBox @this, uint count) {
    Against.ThisIsNull(@this);

    var lines = @this.Text.Split([Environment.NewLine], StringSplitOptions.None);
    @this.Text = string.Empty;
    @this.AppendText(lines.Take((int)count)._FOS_Join(Environment.NewLine));
  }

  /// <summary>
  ///   Duplicates the given TextBox
  /// </summary>
  /// <param name="this">This TextBox</param>
  /// <param name="newName">The new name</param>
  /// <returns>The duplicated TextBox</returns>
  public static TextBox Duplicate(this TextBox @this, string newName = null) {
    Against.ThisIsNull(@this);

    newName ??= new Guid().ToString();

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
