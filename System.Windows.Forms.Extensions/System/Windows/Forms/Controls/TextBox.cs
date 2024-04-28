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

using System.Drawing;
using System.Globalization;
using System.Windows.Form.Extensions;
using System.Windows.Forms;

namespace System.Windows.Controls;
// ReSharper disable once PartialTypeWithSinglePart
// ReSharper disable once UnusedMember.Global
public static partial class TextBoxExtensions {
  
  /// <summary>
  ///   Saves the caret position and restores it upon object disposal.
  /// </summary>
  /// <returns></returns>
  public static ICaretPositionToken SaveCaretPosition() => new CaretPositionToken();

  /// <summary>
  ///   Sets the caret position.
  /// </summary>
  /// <param name="p">The point on screen.</param>
  public static void SetCaretPosition(Point p) => NativeMethods._SetCaretPos(p.X, p.Y);

  /// <summary>
  ///   Gets the caret position.
  /// </summary>
  /// <returns></returns>
  public static Point GetCaretPosition() {
    NativeMethods._GetCaretPos(out var result);
    return result;
  }

  /// <summary>
  ///   Moves the cursor to end of the text.
  /// </summary>
  /// <param name="this">This TextBox.</param>
  public static void MoveCursorToEnd(this TextBox @this) {
    @this.SelectionStart = @this.TextLength;
    @this.SelectionLength = 0;
  }

  /// <summary>
  ///   Tries to parse the content into an int.
  /// </summary>
  /// <param name="this">This TextBox.</param>
  /// <param name="value">The value.</param>
  /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
  public static bool TryParseInt(this TextBox @this, ref int value) {
    var text = @this.Text;
    if (text._FOS_IsNullOrWhiteSpace())
      return false;

    if (!int.TryParse(text, out var temp))
      return false;

    value = temp;
    return true;
  }

  /// <summary>
  ///   Tries to parse the content into an int.
  /// </summary>
  /// <param name="this">This TextBox.</param>
  /// <param name="style">The style.</param>
  /// <param name="provider">The format provider.</param>
  /// <param name="value">The value.</param>
  /// <returns>
  ///   <c>true</c> on success; otherwise, <c>false</c>.
  /// </returns>
  public static bool TryParseInt(this TextBox @this, NumberStyles style, IFormatProvider provider, ref int value) {
    var text = @this.Text;
    if (text._FOS_IsNullOrWhiteSpace())
      return false;
    
    if (!int.TryParse(text, style, provider, out var temp))
      return false;
    
    value = temp;
    return true;
  }
}
