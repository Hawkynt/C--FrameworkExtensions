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
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Runtime.InteropServices;
using DrawingSize = System.Drawing.Size;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace System.Windows.Forms {
  internal static partial class RichTextBoxExtensions {

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
        => SendMessage(handle, _WM_SETREDRAW, (IntPtr)0, IntPtr.Zero)
        ;

      public static void EndUpdate(IntPtr handle)
        =>
        SendMessage(handle, _WM_SETREDRAW, (IntPtr)1, IntPtr.Zero)
        ;

      public static void FormatRange(IntPtr handle, IntPtr formatRange)
        => SendMessage(handle, _EM_FORMATRANGE, 1, formatRange)
        ;
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

    #region Struct_Const_4_DrawToBitmap

    private const double _INCH = 14.4;
    #endregion

    public static Bitmap DrawToBitmap(this RichTextBox @this, int width, int height) {
      var result = new Bitmap(width, height);
      DrawToBitmap(@this, result);
      return result;
    }

    public static Bitmap DrawToBitmap(this RichTextBox @this, DrawingSize size) => DrawToBitmap(@this, size.Width, size.Height);
    public static void DrawToBitmap(this RichTextBox @this, Bitmap target) => _DrawToBitmap(@this, target);

    /// <summary>
    /// Draws to bitmap.
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

  }
}