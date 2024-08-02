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

namespace System.Windows.Forms;

public static partial class ErrorProviderExtensions {

  /// <summary>
  /// Clears the error message for the specified control.
  /// </summary>
  /// <param name="this">The <see cref="System.Windows.Forms.ErrorProvider"/> instance.</param>
  /// <param name="control">The <see cref="System.Windows.Forms.Control"/> for which the error message will be cleared.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// ErrorProvider errorProvider = new ErrorProvider();
  /// TextBox textBox = new TextBox();
  /// errorProvider.SetError(textBox, "This is an error message.");
  ///
  /// textBox.KeyDown += (sender, e) =>
  /// {
  ///     errorProvider.Clear(textBox);
  /// };
  /// 
  /// // The error message for the textBox is cleared when a key is pressed.
  /// </code>
  /// </example>
  public static void Clear(this ErrorProvider @this, Control control) => @this.SetError(control, null);

}
