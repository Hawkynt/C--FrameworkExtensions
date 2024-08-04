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

using Guard;

namespace System.Windows.Forms;

public static partial class ControlCollectionExtensions {

  /// <summary>
  /// Converts the <see cref="Control.ControlCollection"/> to an array of controls.
  /// </summary>
  /// <param name="this">This <see cref="Control.ControlCollection"/> instance.</param>
  /// <returns>An array of <see cref="Control"/> elements.</returns>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// Form form = new Form();
  /// form.Controls.Add(new Button());
  /// form.Controls.Add(new TextBox());
  /// Control[] controls = form.Controls.ToArray();
  /// Console.WriteLine($"Number of controls: {controls.Length}");
  /// // Output: Number of controls: 2
  /// </code>
  /// </example>
  public static Control[] ToArray(this Control.ControlCollection @this) {
    Against.ThisIsNull(@this);

    var result = new Control[@this.Count];
    @this.CopyTo(result, 0);
    
    return result;
  }

}
