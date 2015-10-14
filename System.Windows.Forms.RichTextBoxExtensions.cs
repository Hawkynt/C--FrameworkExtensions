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
using System.Drawing;
using System.Runtime.InteropServices;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace System.Windows.Forms {
  internal static partial class RichTextBoxExtensions {

    #region Native Methods
    private static partial class NativeMethods {
      [DllImport("user32.dll")]
      private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
      private const int WM_SETREDRAW = 0x0b;

      public static void BeginUpdate(IntPtr handle) => SendMessage(handle, WM_SETREDRAW, (IntPtr)0, IntPtr.Zero);
      public static void EndUpdate(IntPtr handle) => SendMessage(handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
    }
    #endregion

    /// <summary>
    /// Stops this control from being repainted.
    /// </summary>
    /// <param name="this">This RichTextBox.</param>
    // ReSharper disable once UnusedParameter.Global
    public static void BeginUpdate(this RichTextBox @this) => NativeMethods.BeginUpdate(@this.Handle);

    /// <summary>
    /// Resumes repainting this control.
    /// </summary>
    /// <param name="this">This RichTextBox.</param>
    public static void EndUpdate(this RichTextBox @this) {
      NativeMethods.EndUpdate(@this.Handle);
      @this.Invalidate();
    }

    /// <summary>
    /// Appends the text and scrolls.
    /// </summary>
    /// <param name="this">This TextBox.</param>
    /// <param name="text">The text.</param>
    public static void AppendTextAndScroll(this RichTextBox @this, string text) {
      Contract.Requires(@this != null);
      @this.AppendText(text ?? string.Empty);
      @this.ScrollToEnd();
    }

    /// <summary>
    /// Scrolls to the end of the RTB.
    /// </summary>
    /// <param name="this">This RichTextBox.</param>
    public static void ScrollToEnd(this RichTextBox @this) {
      Contract.Requires(@this != null);
      var start = @this.SelectionStart;
      var length = @this.SelectionLength;
      @this.SelectionStart = @this.Text.Length;
      @this.ScrollToCaret();
      @this.Select(start, length);
    }

    /// <summary>
    /// Changes the graphicals props of a certain RTB section.
    /// </summary>
    /// <param name="this">This RichTextBox.</param>
    /// <param name="start">The start.</param>
    /// <param name="length">The length.</param>
    /// <param name="foreground">The foreground.</param>
    /// <param name="background">The background.</param>
    /// <param name="font">The font.</param>
    public static void ChangeSectionStyle(this RichTextBox @this, int start, int length, Color foreground, Color background, Font font = null) {
      @this.Select(start, length);
      @this.SelectionColor = foreground;
      @this.SelectionBackColor = background;

      if (font != null)
        @this.SelectionFont = font;
    }

    /// <summary>
    /// Resets the graphicals props of a certain RTB section.
    /// </summary>
    /// <param name="this">This RichTextBox.</param>
    /// <param name="start">The start.</param>
    /// <param name="length">The length.</param>
    public static void ResetSectionStyle(this RichTextBox @this, int start, int length) => @this.ChangeSectionStyle(start, length, @this.ForeColor, @this.BackColor, @this.Font);

    /// <summary>
    /// Resets the graphicals props of the whole RTB.
    /// </summary>
    /// <param name="this">This RichTextBox.</param>
    public static void ResetStyle(this RichTextBox @this) => @this.ChangeSectionStyle(0, @this.TextLength, @this.ForeColor, @this.BackColor, @this.Font);

    /// <summary>
    /// Changes the graphicals props of a certain RTB section.
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
    /// Changes the graphicals props of a certain RTB section.
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
    /// Changes the graphicals props of a certain RTB section.
    /// </summary>
    /// <param name="this">This RichTextBox.</param>
    /// <param name="start">The start.</param>
    /// <param name="length">The length.</param>
    /// <param name="color">The background.</param>
    public static void ChangeSectionBackground(this RichTextBox @this, int start, int length, Color color) {
      @this.Select(start, length);
      @this.SelectionBackColor = color;
    }

  }
}