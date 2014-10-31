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
using word = System.UInt32;
namespace System.Windows.Forms {
  internal static partial class RichTextBoxExtensions {

    #region Native Methods
    private static partial class NativeMethods {
      [DllImport("user32.dll")]
      private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
      private const int WM_SETREDRAW = 0x0b;

      public static void BeginUpdate(IntPtr handle) {
        SendMessage(handle, WM_SETREDRAW, (IntPtr)0, IntPtr.Zero);
      }
      public static void EndUpdate(IntPtr handle) {
        SendMessage(handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
      }
    }
    #endregion

    /// <summary>
    /// Stops this control from being repainted.
    /// </summary>
    /// <param name="This">This RichTextBox.</param>
    public static void BeginUpdate(this RichTextBox This) {
      NativeMethods.BeginUpdate(This.Handle);
    }

    /// <summary>
    /// Resumes repainting this control.
    /// </summary>
    /// <param name="This">This RichTextBox.</param>
    public static void EndUpdate(this RichTextBox This) {
      NativeMethods.EndUpdate(This.Handle);
      This.Invalidate();
    }

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

    /// <summary>
    /// Changes the graphicals props of a certain RTB section.
    /// </summary>
    /// <param name="This">This RichTextBox.</param>
    /// <param name="start">The start.</param>
    /// <param name="length">The length.</param>
    /// <param name="foreground">The foreground.</param>
    /// <param name="background">The background.</param>
    /// <param name="font">The font.</param>
    public static void ChangeSectionStyle(this RichTextBox This, int start, int length, Color foreground, Color background, Font font) {
      This.SelectionStart = start;
      This.SelectionLength = length;
      This.SelectionColor = foreground;
      This.SelectionBackColor = background;
      This.SelectionFont = font;
    }

    /// <summary>
    /// Resets the graphicals props of a certain RTB section.
    /// </summary>
    /// <param name="This">This RichTextBox.</param>
    /// <param name="start">The start.</param>
    /// <param name="length">The length.</param>
    public static void ResetSectionStyle(this RichTextBox This, int start, int length) {
      This.ChangeSectionStyle(start, length, This.ForeColor, This.BackColor, This.Font);
    }

    /// <summary>
    /// Resets the graphicals props of the whole RTB.
    /// </summary>
    /// <param name="This">This RichTextBox.</param>
    public static void ResetStyle(this RichTextBox This) {
      This.ChangeSectionStyle(0, This.TextLength, This.ForeColor, This.BackColor, This.Font);
    }

    /// <summary>
    /// Changes the graphicals props of a certain RTB section.
    /// </summary>
    /// <param name="This">This RichTextBox.</param>
    /// <param name="start">The start.</param>
    /// <param name="length">The length.</param>
    /// <param name="font">The font.</param>
    public static void ChangeSectionFont(this RichTextBox This, int start, int length, Font font) {
      This.SelectionStart = start;
      This.SelectionLength = length;
      This.SelectionFont = font;
    }

    /// <summary>
    /// Changes the graphicals props of a certain RTB section.
    /// </summary>
    /// <param name="This">This RichTextBox.</param>
    /// <param name="start">The start.</param>
    /// <param name="length">The length.</param>
    /// <param name="color">The foreground.</param>
    public static void ChangeSectionForeground(this RichTextBox This, int start, int length, Color color) {
      This.SelectionStart = start;
      This.SelectionLength = length;
      This.SelectionColor = color;
    }

    /// <summary>
    /// Changes the graphicals props of a certain RTB section.
    /// </summary>
    /// <param name="This">This RichTextBox.</param>
    /// <param name="start">The start.</param>
    /// <param name="length">The length.</param>
    /// <param name="color">The background.</param>
    public static void ChangeSectionBackground(this RichTextBox This, int start, int length, Color color) {
      This.SelectionStart = start;
      This.SelectionLength = length;
      This.SelectionBackColor = color;
    }


  }
}