#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY

#endregion

using System.Linq;
using System.Windows.Form.Extensions;
using Guard;

namespace System.Windows.Forms;

public static partial class TextBoxExtensions {
  /// <summary>
  ///   Appends the text and scrolls.
  /// </summary>
  /// <param name="this">This TextBox.</param>
  /// <param name="text">The text.</param>
  public static void AppendTextAndScroll(this TextBox @this, string text) {
    Against.ThisIsNull(@this);

    @this.AppendText(text ?? string.Empty);
  }

  /// <summary>
  ///   Keeps the last n lines in the textbox removing whatever is before.
  /// </summary>
  /// <param name="this">This TextBox.</param>
  /// <param name="count">The number of lines to keep.</param>
  public static void KeepLastLines(this TextBox @this, uint count) {
    Against.ThisIsNull(@this);

    var lines = @this.Text.Split([Environment.NewLine], StringSplitOptions.None).ToList();
    var linesToRemove = Math.Max(0, lines.Count - (int)count);
    @this.Text = string.Empty;
    @this.AppendText(lines.Skip(linesToRemove)._FOS_Join(Environment.NewLine));
  }

  /// <summary>
  ///   Keeps the first n lines removing whatever is after them.
  /// </summary>
  /// <param name="this">This TextBox.</param>
  /// <param name="count">The number of lines to keep.</param>
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
