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

#if SUPPORTS_CONDITIONAL_WEAK_TABLE
using System.Runtime.CompilerServices;
#endif
#if !SUPPORTS_CONDITIONAL_WEAK_TABLE
using System.Collections.Generic;
#endif
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using Guard;
using DrawingSize = System.Drawing.Size;

namespace System.Windows.Forms;

public static partial class RichTextBoxExtensions {

  #region Native Methods

  private static partial class NativeMethods {
    // ReSharper disable MemberCanBePrivate.Local

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect {
      public int Left;
      public int Top;
      public int Right;
      public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Charrange {
      public int cpMin;
      public int cpMax;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Formatrange {
      public IntPtr hdc;
      public IntPtr hdcTarget;
      public Rect rc;
      public Rect rcPage;
      public Charrange chrg;
    }
    // ReSharper restore MemberCanBePrivate.Local

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wp, IntPtr lp);

    private const int _WM_SETREDRAW = 0x0b;
    private const int _WM_USER = 0x400;
    private const int _EM_FORMATRANGE = _WM_USER + 57;

    public static void BeginUpdate(IntPtr handle)
      => SendMessage(handle, _WM_SETREDRAW, (IntPtr)0, IntPtr.Zero);

    public static void EndUpdate(IntPtr handle)
      =>
        SendMessage(handle, _WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);

    public static void FormatRange(IntPtr handle, IntPtr formatRange)
      => SendMessage(handle, _EM_FORMATRANGE, 1, formatRange);
  }

  #endregion

  /// <summary>
  ///   Stops this control from being repainted.
  /// </summary>
  /// <param name="this">This RichTextBox.</param>
  // ReSharper disable once UnusedParameter.Global
  public static void BeginUpdate(this RichTextBox @this) {
    Against.ThisIsNull(@this);

    NativeMethods.BeginUpdate(@this.Handle);
  }

  /// <summary>
  ///   Resumes repainting this control.
  /// </summary>
  /// <param name="this">This RichTextBox.</param>
  public static void EndUpdate(this RichTextBox @this) {
    Against.ThisIsNull(@this);

    NativeMethods.EndUpdate(@this.Handle);
    @this.Invalidate();
  }

  /// <summary>
  /// Appends text to the <see cref="RichTextBox"/> and scrolls to the end. Optionally sets the text color.
  /// </summary>
  /// <param name="this">This <see cref="RichTextBox"/> instance.</param>
  /// <param name="text">The text to append.</param>
  /// <param name="color">(Optional: defaults to <see langword="null"/>) The color to set for the appended text. If <see langword="null"/>, the default text color is used.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// RichTextBox richTextBox = new RichTextBox();
  /// richTextBox.AppendTextAndScroll("Hello, world!", Color.Blue);
  /// // The RichTextBox now contains the text "Hello, world!" in blue color and is scrolled to the end.
  /// </code>
  /// </example>
  public static void AppendTextAndScroll(this RichTextBox @this, string text, Color? color = null) {
    Against.ThisIsNull(@this);

    if (string.IsNullOrEmpty(text))
      return;

    AppendText(@this, text, color);

    //TODO: race condition, crashes sometimes
    @this.ScrollToEnd();
  }

  /// <summary>
  /// Appends text to the <see cref="RichTextBox"/>. Optionally sets the text color.
  /// </summary>
  /// <param name="this">This <see cref="RichTextBox"/> instance.</param>
  /// <param name="text">The text to append.</param>
  /// <param name="color">(Optional: defaults to <see langword="null"/>) The color to set for the appended text. If <see langword="null"/>, the default text color is used.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// RichTextBox richTextBox = new RichTextBox();
  /// richTextBox.AppendText("Hello, world!", Color.Blue);
  /// // The RichTextBox now contains the text "Hello, world!" in blue color.
  /// </code>
  /// </example>
  public static void AppendText(this RichTextBox @this, string text, Color? color = null) {
    Against.ThisIsNull(@this);

    @this.SelectionStart = @this.TextLength;
    @this.SelectionLength = 0;

    @this.SelectionColor = color ?? @this.ForeColor;

    @this.AppendText(text);
    @this.SelectionColor = @this.ForeColor;
  }

  /// <summary>
  /// Scrolls the <see cref="RichTextBox"/> to the end.
  /// </summary>
  /// <param name="this">This <see cref="RichTextBox"/> instance.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// RichTextBox richTextBox = new RichTextBox();
  /// richTextBox.AppendText("Hello, world!");
  /// richTextBox.ScrollToEnd();
  /// // The RichTextBox is now scrolled to the end.
  /// </code>
  /// </example>
  public static void ScrollToEnd(this RichTextBox @this) {
    Against.ThisIsNull(@this);

    var start = @this.SelectionStart;
    var length = @this.SelectionLength;
    @this.SelectionStart = @this.Text.Length;
    @this.ScrollToCaret();
    @this.Select(start, length);
  }

  /// <summary>
  /// Changes the style of a specified section of text in the <see cref="RichTextBox"/>.
  /// </summary>
  /// <param name="this">This <see cref="RichTextBox"/> instance.</param>
  /// <param name="index">The starting position of the section.</param>
  /// <param name="count">The length of the section.</param>
  /// <param name="foreground">The foreground color to apply to the section.</param>
  /// <param name="background">The background color to apply to the section.</param>
  /// <param name="font">(Optional: defaults to <see langword="null"/>) The font to apply to the section. If <see langword="null"/>, the current font is used.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="System.ArgumentOutOfRangeException">Thrown if <paramref name="index"/> or <paramref name="count"/> is out of range.</exception>
  /// <example>
  /// <code>
  /// RichTextBox richTextBox = new RichTextBox();
  /// richTextBox.AppendText("This is a sample text.");
  /// richTextBox.ChangeSectionStyle(0, 4, Color.Red, Color.Yellow, new Font("Arial", 12, FontStyle.Bold));
  /// // The first 4 characters of the text are now red with a yellow background and bold Arial font.
  /// </code>
  /// </example>
  public static void ChangeSectionStyle(this RichTextBox @this, int index, int count, Color foreground, Color background, Font font = null) {
    Against.ThisIsNull(@this);
    Against.IndexOutOfRange(index, @this.TextLength - 1);
    Against.CountOutOfRange(count, @this.TextLength);

    @this.Select(index, count);
    @this.SelectionColor = foreground;
    @this.SelectionBackColor = background;

    if (font != null)
      @this.SelectionFont = font;

    @this.SelectionLength = 0;
  }

  /// <summary>
  /// Resets the style of a specified section of text in the <see cref="RichTextBox"/> to the default style.
  /// </summary>
  /// <param name="this">This <see cref="RichTextBox"/> instance.</param>
  /// <param name="index">The starting position of the section.</param>
  /// <param name="count">The length of the section.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="System.ArgumentOutOfRangeException">Thrown if <paramref name="index"/> or <paramref name="count"/> is out of range.</exception>
  /// <example>
  /// <code>
  /// RichTextBox richTextBox = new RichTextBox();
  /// richTextBox.AppendText("This is a sample text.");
  /// richTextBox.ChangeSectionStyle(0, 4, Color.Red, Color.Yellow, new Font("Arial", 12, FontStyle.Bold));
  /// richTextBox.ResetSectionStyle(0, 4);
  /// // The first 4 characters of the text are now reset to the default style.
  /// </code>
  /// </example>
  public static void ResetSectionStyle(this RichTextBox @this, int index, int count) {
    Against.ThisIsNull(@this);
    Against.IndexOutOfRange(index, @this.TextLength - 1);
    Against.CountOutOfRange(count, @this.TextLength);

    @this.ChangeSectionStyle(index, count, @this.ForeColor, @this.BackColor, @this.Font);
  }

  /// <summary>
  /// Resets the style of the entire text in the <see cref="RichTextBox"/> to the default style.
  /// </summary>
  /// <param name="this">This <see cref="RichTextBox"/> instance.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// RichTextBox richTextBox = new RichTextBox();
  /// richTextBox.AppendText("This is a sample text.");
  /// richTextBox.ChangeSectionStyle(0, 4, Color.Red, Color.Yellow, new Font("Arial", 12, FontStyle.Bold));
  /// richTextBox.ResetStyle();
  /// // The entire text is now reset to the default style.
  /// </code>
  /// </example>
  public static void ResetStyle(this RichTextBox @this) {
    Against.ThisIsNull(@this);

    @this.ChangeSectionStyle(0, @this.TextLength, @this.ForeColor, @this.BackColor, @this.Font);
  }

  /// <summary>
  /// Changes the font of a specified section of text in the <see cref="RichTextBox"/>.
  /// </summary>
  /// <param name="this">This <see cref="RichTextBox"/> instance.</param>
  /// <param name="index">The starting index of the section.</param>
  /// <param name="count">The number of characters to apply the font to.</param>
  /// <param name="font">The font to apply to the section.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="System.ArgumentOutOfRangeException">Thrown if <paramref name="index"/> or <paramref name="count"/> is out of range.</exception>
  /// <example>
  /// <code>
  /// RichTextBox richTextBox = new RichTextBox();
  /// richTextBox.AppendText("This is a sample text.");
  /// richTextBox.ChangeSectionFont(0, 4, new Font("Arial", 12, FontStyle.Bold));
  /// // The first 4 characters of the text are now in bold Arial font.
  /// </code>
  /// </example>
  public static void ChangeSectionFont(this RichTextBox @this, int index, int count, Font font) {
    Against.ThisIsNull(@this);
    Against.IndexOutOfRange(index, @this.TextLength - 1);
    Against.CountOutOfRange(count, @this.TextLength);

    @this.Select(index, count);
    @this.SelectionFont = font;
  }

  /// <summary>
  /// Changes the foreground color of a specified section of text in the <see cref="RichTextBox"/>.
  /// </summary>
  /// <param name="this">This <see cref="RichTextBox"/> instance.</param>
  /// <param name="index">The starting index of the section.</param>
  /// <param name="count">The number of characters to apply the color to.</param>
  /// <param name="color">The foreground color to apply to the section.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="System.ArgumentOutOfRangeException">Thrown if <paramref name="index"/> or <paramref name="count"/> is out of range.</exception>
  /// <example>
  /// <code>
  /// RichTextBox richTextBox = new RichTextBox();
  /// richTextBox.AppendText("This is a sample text.");
  /// richTextBox.ChangeSectionForeground(0, 4, Color.Red);
  /// // The first 4 characters of the text are now red.
  /// </code>
  /// </example>
  public static void ChangeSectionForeground(this RichTextBox @this, int index, int count, Color color) {
    Against.ThisIsNull(@this);
    Against.IndexOutOfRange(index, @this.TextLength - 1);
    Against.CountOutOfRange(count, @this.TextLength);

    @this.Select(index, count);
    @this.SelectionColor = color;
  }

  /// <summary>
  /// Changes the background color of a specified section of text in the <see cref="RichTextBox"/>.
  /// </summary>
  /// <param name="this">This <see cref="RichTextBox"/> instance.</param>
  /// <param name="index">The starting index of the section.</param>
  /// <param name="count">The number of characters to apply the color to.</param>
  /// <param name="color">The background color to apply to the section.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="System.ArgumentOutOfRangeException">Thrown if <paramref name="index"/> or <paramref name="count"/> is out of range.</exception>
  /// <example>
  /// <code>
  /// RichTextBox richTextBox = new RichTextBox();
  /// richTextBox.AppendText("This is a sample text.");
  /// richTextBox.ChangeSectionBackground(0, 4, Color.Yellow);
  /// // The first 4 characters of the text now have a yellow background.
  /// </code>
  /// </example>
  public static void ChangeSectionBackground(this RichTextBox @this, int index, int count, Color color) {
    Against.ThisIsNull(@this);
    Against.IndexOutOfRange(index, @this.TextLength - 1);
    Against.CountOutOfRange(count, @this.TextLength);

    @this.Select(index, count);
    @this.SelectionBackColor = color;
  }

  #region Struct_Const_4_DrawToBitmap

  private const double _INCH = 14.4;

  #endregion

  /// <summary>
  /// Draws the content of the <see cref="RichTextBox"/> to a <see cref="Bitmap"/> with the specified dimensions.
  /// </summary>
  /// <param name="this">This <see cref="RichTextBox"/> instance.</param>
  /// <param name="width">The width of the <see cref="Bitmap"/>.</param>
  /// <param name="height">The height of the <see cref="Bitmap"/>.</param>
  /// <returns>A <see cref="Bitmap"/> containing the content of the <see cref="RichTextBox"/>.</returns>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// RichTextBox richTextBox = new RichTextBox();
  /// richTextBox.AppendText("This is a sample text.");
  /// Bitmap bitmap = richTextBox.DrawToBitmap(200, 100);
  /// // The content of the RichTextBox is now drawn onto a 200x100 bitmap.
  /// </code>
  /// </example>
  public static Bitmap DrawToBitmap(this RichTextBox @this, int width, int height) {
    Against.ThisIsNull(@this);

    var result = new Bitmap(width, height);
    DrawToBitmap(@this, result);
    return result;
  }

  /// <summary>
  /// Draws the content of the <see cref="RichTextBox"/> to a <see cref="Bitmap"/> with the specified size.
  /// </summary>
  /// <param name="this">This <see cref="RichTextBox"/> instance.</param>
  /// <param name="size">The size of the <see cref="Bitmap"/>.</param>
  /// <returns>A <see cref="Bitmap"/> containing the content of the <see cref="RichTextBox"/>.</returns>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// RichTextBox richTextBox = new RichTextBox();
  /// richTextBox.AppendText("This is a sample text.");
  /// Size size = new Size(200, 100);
  /// Bitmap bitmap = richTextBox.DrawToBitmap(size);
  /// // The content of the RichTextBox is now drawn onto a 200x100 bitmap.
  /// </code>
  /// </example>
  public static Bitmap DrawToBitmap(this RichTextBox @this, DrawingSize size) {
    Against.ThisIsNull(@this);

    return DrawToBitmap(@this, size.Width, size.Height);
  }

  /// <summary>
  /// Draws the content of the <see cref="RichTextBox"/> onto the specified <see cref="Bitmap"/>.
  /// </summary>
  /// <param name="this">This <see cref="RichTextBox"/> instance.</param>
  /// <param name="target">The target <see cref="Bitmap"/> to draw onto.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> or <paramref name="target"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// RichTextBox richTextBox = new RichTextBox();
  /// richTextBox.AppendText("This is a sample text.");
  /// Bitmap bitmap = new Bitmap(200, 100);
  /// richTextBox.DrawToBitmap(bitmap);
  /// // The content of the RichTextBox is now drawn onto the specified bitmap.
  /// </code>
  /// </example>
  public static void DrawToBitmap(this RichTextBox @this, Bitmap target) {
    Against.ThisIsNull(@this);

    _DrawToBitmap(@this, target);
  }

  /// <summary>
  ///   Draws to bitmap.
  /// </summary>
  /// <param name="this">This RichTextBox.</param>
  /// <param name="target">The bitmap.</param>
  // ReSharper disable once SuggestBaseTypeForParameter
  private static void _DrawToBitmap(RichTextBox @this, Bitmap target) {
    using var graphics = Graphics.FromImage(target);

    var hdc = IntPtr.Zero;
    try {
      hdc = graphics.GetHdc();

      var intPtr = hdc;
      var hDc = intPtr;

      var rect = new NativeMethods.Rect {
        Top = 0, 
        Left = 0, 
        Bottom = (int)(target.Height + target.Height * (target.HorizontalResolution / 100) * _INCH), 
        Right = (int)(target.Width + target.Width * (target.VerticalResolution / 100) * _INCH)
      };

      var fmtRange = new NativeMethods.Formatrange {
        chrg = {
          cpMin = 0, 
          cpMax = -1
        }, 
        hdc = hDc, 
        hdcTarget = hDc,
        rc = rect, 
        rcPage = rect
      };

      var allocCoTaskMem = IntPtr.Zero;
      try {
        allocCoTaskMem = Marshal.AllocCoTaskMem(Marshal.SizeOf(fmtRange));

        Marshal.StructureToPtr(fmtRange, allocCoTaskMem, false);
        NativeMethods.FormatRange(@this.Handle, allocCoTaskMem);
        NativeMethods.FormatRange(@this.Handle, IntPtr.Zero);
      } finally {
        if (allocCoTaskMem != IntPtr.Zero)
          Marshal.FreeCoTaskMem(allocCoTaskMem);
      }
    } finally {
      if (hdc != IntPtr.Zero)
        graphics.ReleaseHdc();
    }
  }

  /// <summary>
  /// Keeps only the last specified number of lines in the <see cref="RichTextBox"/>.
  /// </summary>
  /// <param name="this">This <see cref="RichTextBox"/> instance.</param>
  /// <param name="count">The maximum number of lines to keep. If fewer lines are present, all lines are kept.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// RichTextBox richTextBox = new RichTextBox();
  /// richTextBox.AppendText("Line 1\nLine 2\nLine 3\nLine 4\nLine 5");
  /// richTextBox.KeepLastLines(3);
  /// // The RichTextBox now contains only the last 3 lines: "Line 3\nLine 4\nLine 5"
  /// </code>
  /// </example>
  public static void KeepLastLines(this RichTextBox @this, int count) {
    Against.ThisIsNull(@this);

    var lines = @this.Lines;
    var linesLength = lines.Length - 1;
    var firstLineStillStanding = linesLength - count;
    if (firstLineStillStanding <= 0)
      return;

    var position = lines.Take(firstLineStillStanding).Sum(l => l.Length + 2);
    @this.Select(0, position - 1);
    @this.SelectedText = string.Empty;
    @this.SelectionStart = @this.TextLength;
    @this.SelectionLength = 0;
  }

  #region richtextbox syntax highlighting

#if !SUPPORTS_CONDITIONAL_WEAK_TABLE
  private static readonly Dictionary<RichTextBox, SyntaxHighlighter> _syntaxHighlighterCache = [];
#else
  private static readonly ConditionalWeakTable<RichTextBox, SyntaxHighlighter> _syntaxHighlighterCache = new();
#endif

  /// <summary>
  /// Applies syntax highlighting to the <see cref="RichTextBox"/> based on the specified configuration.
  /// </summary>
  /// <param name="this">This <see cref="RichTextBox"/> instance.</param>
  /// <param name="configuration">The <see cref="SyntaxHighlightingConfiguration"/> containing the rules for syntax highlighting.</param>
  /// <param name="reapply">(Optional: defaults to <see langword="false"/>) If set to <see langword="true"/>, reapplies the syntax highlighting.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="configuration"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// RichTextBox richTextBox = new RichTextBox();
  /// var config = new SyntaxHighlightingConfiguration(
  ///     SyntaxHighlightPattern.FromKeyword("public", new SyntaxStyle(Color.Blue)),
  ///     SyntaxHighlightPattern.FromKeyword("void", new SyntaxStyle(Color.Green))
  /// );
  /// richTextBox.Text = "public void Method() { }";
  /// richTextBox.ApplySyntaxHighlighting(config);
  /// // The keywords "public" and "void" are now highlighted in blue and green, respectively.
  /// </code>
  /// </example>
  public static void ApplySyntaxHighlighting(this RichTextBox @this, SyntaxHighlightingConfiguration configuration, bool reapply = false) {
    Against.ThisIsNull(@this);

    if (configuration.Patterns is not { Length: > 0 })
      return;

    var highlighter = new SyntaxHighlighter(configuration);

    if (reapply) {
      if (_syntaxHighlighterCache.TryGetValue(@this, out _))
        _syntaxHighlighterCache.Remove(@this);
      else {
        @this.TextChanged += Rtb_OnTextChanged;
        @this.Disposed += Rtb_OnDispose;
      }

      _syntaxHighlighterCache.Add(@this, highlighter);
    }

    highlighter.ApplySyntaxHighlighting(@this);
  }

  /// <summary>
  /// Removes all syntax highlighting from the <see cref="RichTextBox"/>, resetting the text to the default style.
  /// </summary>
  /// <param name="this">This <see cref="RichTextBox"/> instance.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// RichTextBox richTextBox = new RichTextBox();
  /// richTextBox.AppendText("This is a sample text.");
  /// var config = new SyntaxHighlightingConfiguration(
  ///     SyntaxHighlightPatternFactory.FromKeyword("sample", new SyntaxStyle(Color.Blue))
  /// );
  /// richTextBox.ApplySyntaxHighlighting(config);
  /// richTextBox.RemoveSyntaxHighlighting();
  /// // The text "This is a sample text." is now reset to the default style.
  /// </code>
  /// </example>
  public static void RemoveSyntaxHighlighting(this RichTextBox @this) {
    Against.ThisIsNull(@this);

    @this.TextChanged -= Rtb_OnTextChanged;
    @this.Disposed -= Rtb_OnDispose;

    var style = new _SyntaxStyle(@this.ForeColor, @this.BackColor, @this.SelectionFont.Style);
    style.ApplyTo(@this, 0, @this.TextLength);

    if (_syntaxHighlighterCache.TryGetValue(@this, out _))
      _syntaxHighlighterCache.Remove(@this);
  }

  private static void Rtb_OnDispose(object sender, EventArgs _) {
    if (sender is RichTextBox rtb)
      RemoveSyntaxHighlighting(rtb);
  }

  private static void Rtb_OnTextChanged(object sender, EventArgs e) {
    if (sender is not RichTextBox rtb)
      return;

    if (_syntaxHighlighterCache.TryGetValue(rtb, out var highlighter))
      highlighter.ApplySyntaxHighlighting(rtb);
  }

  #endregion

}
