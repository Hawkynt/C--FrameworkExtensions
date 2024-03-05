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

#if SUPPORTS_CONDITIONAL_WEAK_TABLE
using System.Runtime.CompilerServices;
#endif
#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Form.Extensions;
using DrawingSize = System.Drawing.Size;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace System.Windows.Forms;

#if COMPILE_TO_EXTENSION_DLL
public
#else
internal
#endif
static partial class RichTextBoxExtensions {
  
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
  public static void BeginUpdate(this RichTextBox @this) => NativeMethods.BeginUpdate(@this.Handle);

  /// <summary>
  ///   Resumes repainting this control.
  /// </summary>
  /// <param name="this">This RichTextBox.</param>
  public static void EndUpdate(this RichTextBox @this) {
    NativeMethods.EndUpdate(@this.Handle);
    @this.Invalidate();
  }

  /// <summary>
  ///   Appends the text and scrolls.
  /// </summary>
  /// <param name="this">This TextBox.</param>
  /// <param name="text">The text.</param>
  /// <param name="color">The text color</param>
  public static void AppendTextAndScroll(this RichTextBox @this, string text, Color? color = null) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
#endif

    if (string.IsNullOrEmpty(text))
      return;

    AppendText(@this, text, color);

    //TODO: race condition, crashes sometimes
    @this.ScrollToEnd();
  }

  public static void AppendText(this RichTextBox @this, string text, Color? color = null) {
    @this.SelectionStart = @this.TextLength;
    @this.SelectionLength = 0;

    @this.SelectionColor = color ?? @this.ForeColor;

    @this.AppendText(text);
    @this.SelectionColor = @this.ForeColor;
  }

  /// <summary>
  ///   Scrolls to the end of the RTB.
  /// </summary>
  /// <param name="this">This RichTextBox.</param>
  public static void ScrollToEnd(this RichTextBox @this) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
#endif
    var start = @this.SelectionStart;
    var length = @this.SelectionLength;
    @this.SelectionStart = @this.Text.Length;
    @this.ScrollToCaret();
    @this.Select(start, length);
  }

  /// <summary>
  ///   Changes the graphic props of a certain RTB section.
  /// </summary>
  /// <param name="this">This RichTextBox.</param>
  /// <param name="start">The start.</param>
  /// <param name="length">The length.</param>
  /// <param name="foreground">The foreground.</param>
  /// <param name="background">The background.</param>
  /// <param name="font">The font.</param>
  public static void ChangeSectionStyle(this RichTextBox @this, int start, int length, Color foreground,
    Color background, Font font = null) {
    @this.Select(start, length);
    @this.SelectionColor = foreground;
    @this.SelectionBackColor = background;

    if (font != null)
      @this.SelectionFont = font;
  }

  /// <summary>
  ///   Resets the graphic props of a certain RTB section.
  /// </summary>
  /// <param name="this">This RichTextBox.</param>
  /// <param name="start">The start.</param>
  /// <param name="length">The length.</param>
  public static void ResetSectionStyle(this RichTextBox @this, int start, int length) =>
    @this.ChangeSectionStyle(start, length, @this.ForeColor, @this.BackColor, @this.Font);

  /// <summary>
  ///   Resets the graphic props of the whole RTB.
  /// </summary>
  /// <param name="this">This RichTextBox.</param>
  public static void ResetStyle(this RichTextBox @this) =>
    @this.ChangeSectionStyle(0, @this.TextLength, @this.ForeColor, @this.BackColor, @this.Font);

  /// <summary>
  ///   Changes the graphic props of a certain RTB section.
  /// </summary>
  /// <param name="this">This RichTextBox.</param>
  /// <param name="start">The start.</param>
  /// <param name="length">The length.</param>
  /// <param name="font">The font.</param>
  public static void ChangeSectionFont(this RichTextBox @this, int start, int length, Font font) {
    @this.Select(start, length);
    @this.SelectionFont = font;
  }

  /// <summary>
  ///   Changes the graphic props of a certain RTB section.
  /// </summary>
  /// <param name="this">This RichTextBox.</param>
  /// <param name="start">The start.</param>
  /// <param name="length">The length.</param>
  /// <param name="color">The foreground.</param>
  public static void ChangeSectionForeground(this RichTextBox @this, int start, int length, Color color) {
    @this.Select(start, length);
    @this.SelectionColor = color;
  }

  /// <summary>
  ///   Changes the graphic props of a certain RTB section.
  /// </summary>
  /// <param name="this">This RichTextBox.</param>
  /// <param name="start">The start.</param>
  /// <param name="length">The length.</param>
  /// <param name="color">The background.</param>
  public static void ChangeSectionBackground(this RichTextBox @this, int start, int length, Color color) {
    @this.Select(start, length);
    @this.SelectionBackColor = color;
  }

  #region Struct_Const_4_DrawToBitmap

  private const double _INCH = 14.4;

  #endregion

  public static Bitmap DrawToBitmap(this RichTextBox @this, int width, int height) {
    var result = new Bitmap(width, height);
    DrawToBitmap(@this, result);
    return result;
  }

  public static Bitmap DrawToBitmap(this RichTextBox @this, DrawingSize size) =>
    DrawToBitmap(@this, size.Width, size.Height);

  public static void DrawToBitmap(this RichTextBox @this, Bitmap target) => _DrawToBitmap(@this, target);

  /// <summary>
  ///   Draws to bitmap.
  /// </summary>
  /// <param name="this">This RichTextBox.</param>
  /// <param name="target">The bitmap.</param>
  // ReSharper disable once SuggestBaseTypeForParameter
  private static void _DrawToBitmap(RichTextBox @this, Bitmap target) {
    using (var graphics = Graphics.FromImage(target)) {
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
  }

  public static void KeepLastLines(this RichTextBox @this, int count) {
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
  private static readonly Dictionary<RichTextBox, SyntaxHighlighter> _syntaxHighlighterCache = new();
#else
  private static readonly ConditionalWeakTable<RichTextBox, SyntaxHighlighter> _syntaxHighlighterCache = new ConditionalWeakTable<RichTextBox, SyntaxHighlighter>();
#endif

  public static void ApplySyntaxHighlighting(this RichTextBox @this, SyntaxHighlightingConfiguration configuration,
    bool reapply = false) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));

    if (configuration.Patterns == null || configuration.Patterns.Length <= 0)
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

  public static void RemoveSyntaxHighlighting(this RichTextBox @this) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));

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
    if (!(sender is RichTextBox rtb))
      return;

    if (_syntaxHighlighterCache.TryGetValue(rtb, out var highlighter))
      highlighter.ApplySyntaxHighlighting(rtb);
  }

  #region Syntax hightlighting classes

  public interface ISyntaxStyle {
    Color? Foreground { get; }
    FontStyle? FontStyle { get; }
    Color? Background { get; }
  }

  public static class SyntaxStyle {
    public static ISyntaxStyle Foreground(Color color) => new _SyntaxStyle(color);
    public static ISyntaxStyle Background(Color color) => new _SyntaxStyle(background: color);
    public static ISyntaxStyle Color(Color foreground, Color background) => new _SyntaxStyle(foreground, background);
    public static ISyntaxStyle Style(FontStyle style) => new _SyntaxStyle(fontStyle: style);

    public static ISyntaxStyle Bold(Color? foreground = null) =>
      new _SyntaxStyle(foreground, fontStyle: FontStyle.Bold);

    public static ISyntaxStyle Italic(Color? foreground = null) =>
      new _SyntaxStyle(foreground, fontStyle: FontStyle.Italic);

    public static ISyntaxStyle Underline(Color? foreground = null) =>
      new _SyntaxStyle(foreground, fontStyle: FontStyle.Underline);

    public static ISyntaxStyle CreateFrom(Color? foreground = null, Color? background = null,
      FontStyle? fontStyle = null) => new _SyntaxStyle(foreground, background, fontStyle);
  }

  private struct _SyntaxStyle : ISyntaxStyle {
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

  public struct SyntaxHighlightingConfiguration {
    public ISyntaxHighlightPattern[] Patterns { get; }

    public SyntaxHighlightingConfiguration(IEnumerable<ISyntaxHighlightPattern> patterns) {
      if (patterns == null)
        throw new ArgumentNullException(nameof(patterns));

      this.Patterns = patterns.ToArray();
    }

    public SyntaxHighlightingConfiguration(params ISyntaxHighlightPattern[] patterns) => this.Patterns = patterns;
  }

  public interface ISyntaxHighlightPattern {
    Regex RegularExpression { get; }
    ISyntaxStyle Style { get; }
    IReadOnlyDictionary<string, ISyntaxStyle> GroupStyles { get; }
  }

  public static class SyntaxHighlightPattern {
    public static ISyntaxHighlightPattern
      FromKeywords(IEnumerable<string> keywords, ISyntaxStyle style, bool ignoreCase = false) => new Pattern(
      new($@"(?<=^|\W)({keywords.Select(Regex.Escape)._FOS_Join("|")})(?=\W|$)",
        RegexOptions.Compiled | RegexOptions.Singleline | (ignoreCase ? RegexOptions.IgnoreCase : 0)), style);

    public static ISyntaxHighlightPattern FromKeyword(string word, ISyntaxStyle style, bool ignoreCase = false) =>
      new Pattern(
        new($@"(?<=^|\W)({Regex.Escape(word)})(?=\W|$)",
          RegexOptions.Compiled | RegexOptions.Singleline | (ignoreCase ? RegexOptions.IgnoreCase : 0)), style);

    public static ISyntaxHighlightPattern FromRegex(Regex regex, ISyntaxStyle style) => new Pattern(regex, style);

    public static ISyntaxHighlightPattern FromRegex(Regex regex, IDictionary<string, ISyntaxStyle> styles) =>
      new Pattern(regex, null, new ReadOnlyDictionary<string, ISyntaxStyle>(styles));

    public static ISyntaxHighlightPattern FromPattern(string pattern, ISyntaxStyle style) =>
      new Pattern(new(pattern, RegexOptions.Compiled), style);

    public static ISyntaxHighlightPattern FromPattern(string pattern, IDictionary<string, ISyntaxStyle> styles) =>
      new Pattern(new(pattern, RegexOptions.Compiled), null, new ReadOnlyDictionary<string, ISyntaxStyle>(styles));

    public static ISyntaxHighlightPattern FromPattern(string pattern, RegexOptions options, ISyntaxStyle style) =>
      new Pattern(new(pattern, options | RegexOptions.Compiled), style);

    public static ISyntaxHighlightPattern FromPattern(string pattern, RegexOptions options,
      IDictionary<string, ISyntaxStyle> styles) => new Pattern(new(pattern, options | RegexOptions.Compiled), null,
      new ReadOnlyDictionary<string, ISyntaxStyle>(styles));

    private struct Pattern : ISyntaxHighlightPattern {
      public Pattern(Regex regularExpression, ISyntaxStyle style, IReadOnlyDictionary<string, ISyntaxStyle> groupStyles = null) {
        this.RegularExpression = regularExpression;
        this.Style = style;
        this.GroupStyles = groupStyles;
      }

      #region Implementation of ISyntaxHighlightPattern

      public Regex RegularExpression { get; }
      public ISyntaxStyle Style { get; }
      public IReadOnlyDictionary<string, ISyntaxStyle> GroupStyles { get; }

      #endregion
    }
  }

  private class SyntaxHighlighter {
    private struct RtbState {
      private readonly RichTextBox _owner;
      private readonly int _selectionStart;
      private readonly int _selectionLength;

      private RtbState(RichTextBox owner) {
        this._owner = owner;
        this._selectionStart = owner.SelectionStart;
        this._selectionLength = owner.SelectionLength;
      }

      public static RtbState Save(RichTextBox owner) => new(owner);

      public void Load() {
        var rtb = this._owner;
        rtb.SelectionStart = this._selectionStart;
        rtb.SelectionLength = this._selectionLength;
      }
    }

    private readonly SyntaxHighlightingConfiguration _configuration;

    public SyntaxHighlighter(SyntaxHighlightingConfiguration configuration) => this._configuration = configuration;

    public void ApplySyntaxHighlighting(RichTextBox rtb) {
      rtb.SuspendLayout();
      try {
        var rtbState = RtbState.Save(rtb);
        try {
          this._StyleRichTextBox(rtb);
        } finally {
          rtbState.Load();
        }
      } finally {
        rtb.ResumeLayout(true);
      }
    }

    private void _StyleRichTextBox(RichTextBox rtb) {
      var text = rtb.Text ?? rtb.Rtf;
      foreach (var pattern in this._configuration.Patterns)
      foreach (Match match in pattern.RegularExpression.Matches(text)) {
        ((_SyntaxStyle?)pattern.Style)?.ApplyTo(rtb, match.Index, match.Length);

        if (pattern.GroupStyles == null)
          continue;

        foreach (var kvp in pattern.GroupStyles) {
          var group = match.Groups[kvp.Key];
          if (group == null || !group.Success)
            continue;

          ((_SyntaxStyle?)kvp.Value)?.ApplyTo(rtb, group.Index, group.Length);
        }
      }
    }

    #endregion

    #endregion
  }
}
