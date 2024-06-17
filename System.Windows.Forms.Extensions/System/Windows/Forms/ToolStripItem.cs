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

/// <summary>
///   Extension class for <see cref="ToolStripComboBox" /> objects.
/// </summary>
public static partial class ToolStripItemExtensions {
  /// <summary>
  ///   Sets the <see cref="ToolStripItem.Text" /> property to the specified text and makes the <see cref="ToolStripItem" />
  ///   visible if the text is not <see langword="null" /> or contains only whitespace-characters.
  /// </summary>
  /// <param name="this">The <see cref="ToolStripItem" /> on which this extension method is called.</param>
  /// <param name="text">The text to set for the <see cref="ToolStripItem" />.</param>
  /// <exception cref="NullReferenceException">Thrown if the <paramref name="this" /> parameter is <see langword="null" />.</exception>
  /// <remarks>
  ///   If the text is <see langword="null" /> or whitespace, the item will be hidden.
  /// </remarks>
  /// <example>
  ///   Here's how to use the <c>SetTextAndShow</c> method:
  ///   <code>
  /// ToolStripItem myItem = new ToolStripButton();
  /// myItem.SetTextAndShow("Click Me");
  /// </code>
  ///   This code sets the text of <c>myItem</c> to "Click Me" and ensures it is visible.
  /// </example>
  public static void SetTextAndShow(this ToolStripItem @this, string text) {
    Against.ThisIsNull(@this);

    @this.Text = text;
    @this.Visible = text.IsNotNullOrWhiteSpace();
  }

  /// <summary>
  ///   Clears the text of the <see cref="ToolStripItem" /> and hides it.
  /// </summary>
  /// <param name="this">The <see cref="ToolStripItem" /> on which this extension method is called.</param>
  /// <exception cref="NullReferenceException">Thrown if the <paramref name="this" /> parameter is <see langword="null" />.</exception>
  /// <remarks>
  ///   This method sets the <see cref="ToolStripItem.Text" /> property to <see langword="null" />, effectively clearing any
  ///   text displayed on the item, and hides the item by setting <see cref="ToolStripItem.Visible" /> to
  ///   <see langword="false" />.
  /// </remarks>
  /// <example>
  ///   Here's how to use the <c>ClearTextAndHide</c> method:
  ///   <code>
  /// ToolStripItem myItem = new ToolStripButton();
  /// myItem.ClearTextAndHide();
  /// </code>
  ///   This code clears the text of <c>myItem</c> and hides it.
  /// </example>
  public static void ClearTextAndHide(this ToolStripItem @this) {
    Against.ThisIsNull(@this);

    @this.Text = null;
    @this.Visible = false;
  }
}
