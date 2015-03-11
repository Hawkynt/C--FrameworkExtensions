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
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace System.Windows.Controls {
  internal static partial class TextBoxExtensions {

    #region nested types

    /// <summary>
    /// Can be used to temporarely move the caret somewhere and restore the position afterwards.
    /// </summary>
    public interface ICaretPositionToken : IDisposable {
      /// <summary>
      /// Gets the stored position.
      /// </summary>
      /// <value>
      /// The position.
      /// </value>
      Point Position { get; }
    }

    private class CaretPositionToken : ICaretPositionToken {
      private readonly Point _point;

      public CaretPositionToken() {
        NativeMethods._GetCaretPos(out this._point);
      }

      public void Dispose() {
        NativeMethods._SetCaretPos(this._point.X, this._point.Y);
        GC.SuppressFinalize(this);
      }

      ~CaretPositionToken() {
        this.Dispose();
      }

      public Point Position { get { return (this._point); } }
    }

    private static partial class NativeMethods {
      [DllImport("user32", EntryPoint = "GetCaretPos", SetLastError = true)]
      public extern static int _GetCaretPos(out Point p);
      [DllImport("user32", EntryPoint = "SetCaretPos", SetLastError = true)]
      public extern static int _SetCaretPos(int x, int y);
    }
    #endregion

    /// <summary>
    /// Saves the caret position and restores it upon object disposal.
    /// </summary>
    /// <returns></returns>
    public static ICaretPositionToken SaveCaretPosition() {
      return (new CaretPositionToken());
    }

    /// <summary>
    /// Sets the caret position.
    /// </summary>
    /// <param name="p">The point on screen.</param>
    public static void SetCaretPosition(Point p) {
      NativeMethods._SetCaretPos(p.X, p.Y);
    }

    /// <summary>
    /// Gets the caret position.
    /// </summary>
    /// <returns></returns>
    public static Point GetCaretPosition() {
      Point result;
      NativeMethods._GetCaretPos(out result);
      return (result);
    }

    /// <summary>
    /// Moves the cursor to end of the text.
    /// </summary>
    /// <param name="This">This TextBox.</param>
    public static void MoveCursorToEnd(this TextBox This) {
      Contract.Requires(This != null);
      This.SelectionStart = This.TextLength;
      This.SelectionLength = 0;
    }

    /// <summary>
    /// Tries to parse the content into an int.
    /// </summary>
    /// <param name="This">This TextBox.</param>
    /// <param name="value">The value.</param>
    /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
    public static bool TryParseInt(this TextBox This, ref int value) {
      Contract.Requires(This != null);
      var text = This.Text;
      if (string.IsNullOrWhiteSpace(text))
        return (false);
      int temp;
      if (!int.TryParse(text, out temp))
        return (false);
      value = temp;
      return (true);
    }

    /// <summary>
    /// Tries to parse the content into an int.
    /// </summary>
    /// <param name="This">This TextBox.</param>
    /// <param name="style">The style.</param>
    /// <param name="provider">The format provider.</param>
    /// <param name="value">The value.</param>
    /// <returns>
    ///   <c>true</c> on success; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryParseInt(this TextBox This, NumberStyles style, IFormatProvider provider, ref int value) {
      Contract.Requires(This != null);
      var text = This.Text;
      if (string.IsNullOrWhiteSpace(text))
        return (false);
      int temp;
      if (!int.TryParse(text, style, provider, out temp))
        return (false);
      value = temp;
      return (true);
    }
  }
}
