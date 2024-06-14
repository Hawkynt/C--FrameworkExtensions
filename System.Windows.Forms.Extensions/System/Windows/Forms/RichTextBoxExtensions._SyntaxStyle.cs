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

using System.Drawing;

namespace System.Windows.Forms;

public static partial class RichTextBoxExtensions {
  private readonly struct _SyntaxStyle : ISyntaxStyle {
    //TODO: maybe font cache (somehow) || maybe change FontStyle to Font Object

    public Color? Foreground { get; }
    public FontStyle? FontStyle { get; }
    public Color? Background { get; }

    public _SyntaxStyle(Color? foreground = null, Color? background = null, FontStyle? fontStyle = null) {
      this.Foreground = foreground;
      this.Background = background;
      this.FontStyle = fontStyle;
    }

    public void ApplyTo(RichTextBox rtb, int startIndex, int length) {
      rtb.SelectionStart = startIndex;
      rtb.SelectionLength = length;

      if (this.Foreground != null)
        rtb.SelectionColor = this.Foreground.Value;

      if (this.Background != null)
        rtb.SelectionBackColor = this.Background.Value;

      if (this.FontStyle != null)
        using (var font = new Font(rtb.Font, this.FontStyle.Value))
          rtb.SelectionFont = font;
    }
  }
}
