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

using System.Drawing;
using System.Globalization;
using System.Windows.Form.Extensions;
using System.Windows.Forms;
using Guard;

namespace System.Windows.Controls;

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
  public static void SetCaretPosition(Point p) => NativeMethods.SetCaretPos(p);

  /// <summary>
  ///   Gets the caret position.
  /// </summary>
  /// <returns></returns>
  public static Point GetCaretPosition() => NativeMethods.GetCaretPos();

  /// <summary>
  /// Moves the cursor to the end of the text in the <see cref="TextBox"/>.
  /// </summary>
  /// <param name="this">This <see cref="TextBox"/> instance.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// TextBox textBox = new TextBox();
  /// textBox.Text = "Hello, World!";
  /// textBox.MoveCursorToEnd();
  /// // The cursor is now at the end of the text.
  /// </code>
  /// </example>
  public static void MoveCursorToEnd(this TextBox @this) {
    Against.ThisIsNull(@this);

    @this.SelectionStart = @this.TextLength;
    @this.SelectionLength = 0;
  }

  /// <summary>
  /// Tries to parse the text of the <see cref="TextBox"/> as an integer.
  /// </summary>
  /// <param name="this">This <see cref="TextBox"/> instance.</param>
  /// <param name="value">The parsed integer value if the parsing is successful; otherwise, the original value.</param>
  /// <returns><see langword="true"/> if the parsing is successful; otherwise, <see langword="false"/>.</returns>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// TextBox textBox = new TextBox();
  /// textBox.Text = "123";
  /// int result = 0;
  /// bool success = textBox.TryParseInt(ref result);
  /// Console.WriteLine($"Success: {success}, Result: {result}");
  /// // Output: Success: True, Result: 123
  /// </code>
  /// </example>
  public static bool TryParseInt(this TextBox @this, ref int value) {
    Against.ThisIsNull(@this);

    var text = @this.Text;
    if (text._FOS_IsNullOrWhiteSpace())
      return false;

    if (!int.TryParse(text, out var temp))
      return false;

    value = temp;
    return true;
  }

  /// <summary>
  /// Tries to parse the text of the <see cref="TextBox"/> as an integer using the specified style and format provider.
  /// </summary>
  /// <param name="this">This <see cref="TextBox"/> instance.</param>
  /// <param name="style">A bitwise combination of enumeration values that indicates the permitted format of the input string.</param>
  /// <param name="provider">An object that supplies culture-specific formatting information.</param>
  /// <param name="value">The parsed integer value if the parsing is successful; otherwise, the original value.</param>
  /// <returns><see langword="true"/> if the parsing is successful; otherwise, <see langword="false"/>.</returns>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// TextBox textBox = new TextBox();
  /// textBox.Text = "123";
  /// int result = 0;
  /// bool success = textBox.TryParseInt(NumberStyles.Integer, CultureInfo.InvariantCulture, ref result);
  /// Console.WriteLine($"Success: {success}, Result: {result}");
  /// // Output: Success: True, Result: 123
  /// </code>
  /// </example>
  public static bool TryParseInt(this TextBox @this, NumberStyles style, IFormatProvider provider, ref int value) {
    Against.ThisIsNull(@this);

    var text = @this.Text;
    if (text._FOS_IsNullOrWhiteSpace())
      return false;

    if (!int.TryParse(text, style, provider, out var temp))
      return false;

    value = temp;
    return true;
  }

}
