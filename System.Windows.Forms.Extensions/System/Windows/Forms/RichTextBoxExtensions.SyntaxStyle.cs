﻿#region (c)2010-2042 Hawkynt

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

using System.Drawing;

namespace System.Windows.Forms;

public static partial class RichTextBoxExtensions {
  public static class SyntaxStyle {
    public static ISyntaxStyle Foreground(Color color) => new _SyntaxStyle(color);
    public static ISyntaxStyle Background(Color color) => new _SyntaxStyle(background: color);
    public static ISyntaxStyle Color(Color foreground, Color background) => new _SyntaxStyle(foreground, background);
    public static ISyntaxStyle Style(FontStyle style) => new _SyntaxStyle(fontStyle: style);

    public static ISyntaxStyle Bold(Color? foreground = null) => new _SyntaxStyle(foreground, fontStyle: FontStyle.Bold);

    public static ISyntaxStyle Italic(Color? foreground = null) => new _SyntaxStyle(foreground, fontStyle: FontStyle.Italic);

    public static ISyntaxStyle Underline(Color? foreground = null) => new _SyntaxStyle(foreground, fontStyle: FontStyle.Underline);

    public static ISyntaxStyle CreateFrom(Color? foreground = null, Color? background = null, FontStyle? fontStyle = null) => new _SyntaxStyle(foreground, background, fontStyle);
  }
}
