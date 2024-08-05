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

using System.Collections.Generic;
using System.Linq;
using Guard;

namespace System.Windows.Forms;

public static partial class TableLayoutPanelExtensions {

  /// <summary>
  /// Removes the specified row from the <see cref="TableLayoutPanel"/>.
  /// </summary>
  /// <param name="this">This <see cref="TableLayoutPanel"/> instance.</param>
  /// <param name="row">The index of the row to remove.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="System.ArgumentOutOfRangeException">Thrown if <paramref name="row"/> is outside the valid range of rows.</exception>
  /// <example>
  /// <code>
  /// TableLayoutPanel tableLayoutPanel = new TableLayoutPanel();
  /// tableLayoutPanel.RowCount = 3;
  /// tableLayoutPanel.Controls.Add(new Label { Text = "Row 0" }, 0, 0);
  /// tableLayoutPanel.Controls.Add(new Label { Text = "Row 1" }, 0, 1);
  /// tableLayoutPanel.Controls.Add(new Label { Text = "Row 2" }, 0, 2);
  /// tableLayoutPanel.RemoveRow(1);
  /// // The TableLayoutPanel now has two rows, with "Row 1" removed.
  /// </code>
  /// </example>
  public static void RemoveRow(this TableLayoutPanel @this, int row) {
    Against.ThisIsNull(@this);

    var rowCount = @this.RowCount;
    if (row < 0 || row >= rowCount)
      return;
    var columnCount = @this.ColumnCount;

    // delete all controls of the given row
    for (var i = columnCount; i > 0;) {
      --i;
      var control = @this.GetControlFromPosition(i, row);
      if (control == null)
        continue;
      @this.Controls.Remove(control);
    }

    // order all controls one up
    for (var i = row + 1; i < rowCount; ++i)
    for (var j = columnCount; j > 0;) {
      --j;
      var control = @this.GetControlFromPosition(j, i);
      if (control == null)
        continue;
      @this.SetRow(control, i - 1);
    }

    // remove style
    @this.RowStyles.RemoveAt(row);

    // decrement total rows
    @this.RowCount--;
  }

  /// <summary>
  /// Copies the last row of the <see cref="TableLayoutPanel"/> and adds it as a new row at the end, duplicating all existing controls in it.
  /// </summary>
  /// <param name="this">This <see cref="TableLayoutPanel"/> instance.</param>
  /// <param name="controlCallback">(Optional: defaults to <see langword="null"/>) A callback that is invoked for each control in the copied row, with parameters for the column index, row index, the original control, and the new control.</param>
  /// <param name="allowSuspendingLayout">(Optional: defaults to <see langword="true"/>) If set to <see langword="true"/>, suspends layout during the operation to avoid flickering.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// TableLayoutPanel tableLayoutPanel = new TableLayoutPanel();
  /// tableLayoutPanel.RowCount = 2;
  /// tableLayoutPanel.Controls.Add(new Label { Text = "Row 0, Col 0" }, 0, 0);
  /// tableLayoutPanel.Controls.Add(new Label { Text = "Row 0, Col 1" }, 1, 0);
  /// tableLayoutPanel.Controls.Add(new Label { Text = "Row 1, Col 0" }, 0, 1);
  /// tableLayoutPanel.Controls.Add(new Label { Text = "Row 1, Col 1" }, 1, 1);
  /// tableLayoutPanel.CopyLastRow((col, row, original, copy) => copy.Text = $"Copied from ({row}, {col})");
  /// // The TableLayoutPanel now has three rows, with the last row copied.
  /// </code>
  /// </example>
  public static void CopyLastRow(this TableLayoutPanel @this, Action<int, int, Control, Control> controlCallback = null, bool allowSuspendingLayout = true) {
    Against.ThisIsNull(@this);

    if (allowSuspendingLayout)
      @this.SuspendLayout();

    var lastRow = @this.RowCount - 1;

    // add line
    @this.RowCount++;

    // add style
    var rowStyle = @this.RowStyles[lastRow];
    @this.RowStyles.Add(new(rowStyle.SizeType, rowStyle.Height));

    // copy controls
    var alreadyVisitedControls = new Dictionary<Control, bool>();
    for (var i = @this.ColumnCount; i > 0;) {
      --i;

      var control = @this.GetControlFromPositionFixed(i, lastRow);
      if (control == null || alreadyVisitedControls.ContainsKey(control))
        continue;

      alreadyVisitedControls.Add(control, true);
      var newControl = control.Duplicate();
      controlCallback?.Invoke(lastRow + 1, i, control, newControl);

      @this.Controls.Add(newControl, i, lastRow + 1);
      @this.SetRowSpan(newControl, @this.GetRowSpan(control));
      @this.SetColumnSpan(newControl, @this.GetColumnSpan(control));
    }

    if (allowSuspendingLayout)
      @this.ResumeLayout(true);
  }

  /// <summary>
  /// Copies the last row of the <see cref="TableLayoutPanel"/> and adds it as a new row at the end, duplicating all existing controls in it.
  /// </summary>
  /// <param name="this">This <see cref="TableLayoutPanel"/> instance.</param>
  /// <param name="controlCallback">(Optional: defaults to <see langword="null"/>) A callback that is invoked for each control in the copied row, with the new control as the parameter.</param>
  /// <param name="allowSuspendingLayout">(Optional: defaults to <see langword="true"/>) If set to <see langword="true"/>, suspends layout during the operation to avoid flickering.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// TableLayoutPanel tableLayoutPanel = new TableLayoutPanel();
  /// tableLayoutPanel.RowCount = 2;
  /// tableLayoutPanel.Controls.Add(new Label { Text = "Row 0, Col 0" }, 0, 0);
  /// tableLayoutPanel.Controls.Add(new Label { Text = "Row 0, Col 1" }, 1, 0);
  /// tableLayoutPanel.Controls.Add(new Label { Text = "Row 1, Col 0" }, 0, 1);
  /// tableLayoutPanel.Controls.Add(new Label { Text = "Row 1, Col 1" }, 1, 1);
  /// tableLayoutPanel.CopyLastRow(copy => copy.Text = "Copied");
  /// // The TableLayoutPanel now has three rows, with the last row copied and text set to "Copied".
  /// </code>
  /// </example>
  public static void CopyLastRow(this TableLayoutPanel @this, Action<Control> controlCallback, bool allowSuspendingLayout = true) {
    Against.ThisIsNull(@this);

    @this.CopyLastRow((_, _, _, tgt) => controlCallback(tgt));
  }

  /// <summary>
  /// Copies the last row of the <see cref="TableLayoutPanel"/> and adds it as a new row at the end, duplicating all existing controls in it.
  /// </summary>
  /// <param name="this">This <see cref="TableLayoutPanel"/> instance.</param>
  /// <param name="controlCallback">(Optional: defaults to <see langword="null"/>) A callback that is invoked for each control in the copied row, with the column index and the new control as parameters.</param>
  /// <param name="allowSuspendingLayout">(Optional: defaults to <see langword="true"/>) If set to <see langword="true"/>, suspends layout during the operation to avoid flickering.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// TableLayoutPanel tableLayoutPanel = new TableLayoutPanel();
  /// tableLayoutPanel.RowCount = 2;
  /// tableLayoutPanel.Controls.Add(new Label { Text = "Row 0, Col 0" }, 0, 0);
  /// tableLayoutPanel.Controls.Add(new Label { Text = "Row 0, Col 1" }, 1, 0);
  /// tableLayoutPanel.Controls.Add(new Label { Text = "Row 1, Col 0" }, 0, 1);
  /// tableLayoutPanel.Controls.Add(new Label { Text = "Row 1, Col 1" }, 1, 1);
  /// tableLayoutPanel.CopyLastRow((col, copy) => copy.Text = $"Copied Col {col}");
  /// // The TableLayoutPanel now has three rows, with the last row copied and text set to "Copied Col X".
  /// </code>
  /// </example>
  public static void CopyLastRow(this TableLayoutPanel @this, Action<int, Control> controlCallback, bool allowSuspendingLayout = true) {
    Against.ThisIsNull(@this);

    @this.CopyLastRow((_, col, _, tgt) => controlCallback(col, tgt));
  }

  /// <summary>
  /// Gets the value from a specified column in a specified row of the <see cref="TableLayoutPanel"/> using the provided reader function.
  /// </summary>
  /// <typeparam name="TControl">The type of the control.</typeparam>
  /// <typeparam name="TType">The type of the value to retrieve.</typeparam>
  /// <param name="this">This <see cref="TableLayoutPanel"/> instance.</param>
  /// <param name="row">The index of the row.</param>
  /// <param name="columnIndex">The index of the column.</param>
  /// <param name="reader">The function to read the value from the control.</param>
  /// <returns>The value of type <typeparamref name="TType"/> retrieved using the reader function.</returns>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="System.ArgumentOutOfRangeException">Thrown if <paramref name="row"/> or <paramref name="columnIndex"/> is outside the valid range.</exception>
  /// <exception cref="System.InvalidCastException">Thrown if the control at the specified position is not of type <typeparamref name="TControl"/>.</exception>
  /// <example>
  /// <code>
  /// TableLayoutPanel tableLayoutPanel = new TableLayoutPanel();
  /// var textBox = new TextBox { Text = "Sample Text" };
  /// tableLayoutPanel.Controls.Add(textBox, 0, 0);
  /// string value = tableLayoutPanel.GetColumnValue&lt;TextBox, string&gt;(0, 0, ctrl => ctrl.Text);
  /// Console.WriteLine(value); // Output: Sample Text
  /// </code>
  /// </example>
  public static TType GetColumnValue<TControl, TType>(this TableLayoutPanel @this, uint row, int columnIndex, Func<TControl, TType> reader) where TControl : Control {
    Against.ThisIsNull(@this);
    Against.ValuesAboveOrEqual(row, @this.RowCount);
    Against.ValuesAboveOrEqual(columnIndex, @this.ColumnCount);

    if (row >= @this.RowCount)
      throw new ArgumentOutOfRangeException(nameof(row), row, "Not Allowed");

    var control = @this.GetControlFromPositionFixed<TControl>(columnIndex, (int)row) ?? throw new NotSupportedException();
    return reader(control);
  }

  /// <summary>
  /// Sets the value of a specified column in a specified row of the <see cref="TableLayoutPanel"/> using the provided writer action.
  /// </summary>
  /// <typeparam name="TControl">The type of the <see cref="Control"/>.</typeparam>
  /// <param name="this">This <see cref="TableLayoutPanel"/> instance.</param>
  /// <param name="row">The index of the row.</param>
  /// <param name="columnIndex">The index of the column.</param>
  /// <param name="writer">The action to write the value to the <see cref="Control"/>.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="System.ArgumentOutOfRangeException">Thrown if <paramref name="row"/> or <paramref name="columnIndex"/> is outside the valid range.</exception>
  /// <exception cref="System.InvalidCastException">Thrown if the <see cref="Control"/> at the specified position is not of type <typeparamref name="TControl"/>.</exception>
  /// <example>
  /// <code>
  /// TableLayoutPanel tableLayoutPanel = new TableLayoutPanel();
  /// var textBox = new TextBox();
  /// tableLayoutPanel.Controls.Add(textBox, 0, 0);
  /// tableLayoutPanel.SetColumnValue&lt;TextBox&gt;(0, 0, ctrl => ctrl.Text = "New Text");
  /// Console.WriteLine(textBox.Text); // Output: New Text
  /// </code>
  /// </example>
  public static void SetColumnValue<TControl>(this TableLayoutPanel @this, uint row, int columnIndex, Action<TControl> writer) where TControl : Control {
    Against.ThisIsNull(@this);
    Against.ValuesAboveOrEqual(row, @this.RowCount);
    Against.ValuesAboveOrEqual(columnIndex, @this.ColumnCount);

    var control = @this.GetControlFromPositionFixed<TControl>(columnIndex, (int)row) ?? throw new NotSupportedException();
    writer(control);
  }


  /// <summary>
  /// Gets the control from the specified position in the <see cref="TableLayoutPanel"/> and casts it to the specified control type.
  /// </summary>
  /// <typeparam name="TControl">The type of the control.</typeparam>
  /// <param name="this">This <see cref="TableLayoutPanel"/> instance.</param>
  /// <param name="column">The index of the column.</param>
  /// <param name="row">The index of the row.</param>
  /// <returns>The control at the specified position cast to <typeparamref name="TControl"/>.</returns>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="System.ArgumentOutOfRangeException">Thrown if <paramref name="row"/> or <paramref name="column"/> is outside the valid range.</exception>
  /// <exception cref="System.InvalidCastException">Thrown if the control at the specified position is not of type <typeparamref name="TControl"/>.</exception>
  /// <example>
  /// <code>
  /// TableLayoutPanel tableLayoutPanel = new TableLayoutPanel();
  /// var textBox = new TextBox();
  /// tableLayoutPanel.Controls.Add(textBox, 0, 0);
  /// TextBox retrievedTextBox = tableLayoutPanel.GetControlFromPositionFixed&lt;TextBox&gt;(0, 0);
  /// Console.WriteLine(retrievedTextBox == textBox); // Output: True
  /// </code>
  /// </example>
  public static TControl GetControlFromPositionFixed<TControl>(this TableLayoutPanel @this, int column, int row) where TControl : Control {
    Against.ThisIsNull(@this);
    Against.ValuesAboveOrEqual(row, @this.RowCount);
    Against.ValuesAboveOrEqual(column, @this.ColumnCount);

    return GetControlFromPositionFixed(@this, column, row) as TControl;
  }

  /// <summary>
  /// Gets the control from the specified position in the <see cref="TableLayoutPanel"/>.
  /// </summary>
  /// <param name="this">This <see cref="TableLayoutPanel"/> instance.</param>
  /// <param name="column">The index of the column.</param>
  /// <param name="row">The index of the row.</param>
  /// <returns>The <see cref="Control"/> at the specified position.</returns>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="System.ArgumentOutOfRangeException">Thrown if <paramref name="row"/> or <paramref name="column"/> is outside the valid range.</exception>
  /// <example>
  /// <code>
  /// TableLayoutPanel tableLayoutPanel = new TableLayoutPanel();
  /// var textBox = new TextBox();
  /// tableLayoutPanel.Controls.Add(textBox, 0, 0);
  /// Control retrievedControl = tableLayoutPanel.GetControlFromPositionFixed(0, 0);
  /// Console.WriteLine(retrievedControl == textBox); // Output: True
  /// </code>
  /// </example>
  public static Control GetControlFromPositionFixed(this TableLayoutPanel @this, int column, int row) {
    Against.ThisIsNull(@this);
    Against.ValuesAboveOrEqual(row, @this.RowCount);
    Against.ValuesAboveOrEqual(column, @this.ColumnCount);

    var result = @this.GetControlFromPosition(column, row);
    if (result != null)
      return result;

#if DEBUG
    // This one's for debugging only
    var allControls = @this.Controls.Cast<Control>().Where(c => c != null).ToDictionary(control => control,
      control => Tuple.Create(@this.GetColumn(control), @this.GetRow(control)));
#endif

    return (
      from control in @this.Controls.Cast<Control>().Where(c => c != null)
      where @this.GetColumn(control) == column && @this.GetRow(control) == row
      select control
    ).FirstOrDefault();
  }

  /// <summary>
  /// Populates the <see cref="TableLayoutPanel"/> with the specified controls, arranging them as a table.
  /// </summary>
  /// <param name="this">This <see cref="TableLayoutPanel"/> instance.</param>
  /// <param name="cellControls">A two-dimensional array of controls where the first dimension represents rows and the second dimension represents columns.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <remarks>The columns and their widths need to be already set, the rows will all be auto-sized.</remarks>
  /// <example>
  /// <code>
  /// TableLayoutPanel tableLayoutPanel = new TableLayoutPanel();
  /// tableLayoutPanel.ColumnCount = 2;
  /// tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
  /// tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
  /// tableLayoutPanel.UseAsTable(
  ///     new Control[] { new Label { Text = "R1C1" }, new Label { Text = "R1C2" } },
  ///     new Control[] { new Label { Text = "R2C1" }, new Label { Text = "R2C2" } }
  /// );
  /// // The TableLayoutPanel now contains 2 rows and 2 columns with the specified controls.
  /// </code>
  /// </example>
  public static void UseAsTable(this TableLayoutPanel @this, params Control[][] cellControls) {
    Against.ThisIsNull(@this);
    
    //init
    using var _ = @this.PauseLayout();
    @this.RowStyles.Clear();
    @this.Controls.Clear();

    //add all controls
    for (var y = 0; y < cellControls.Length; ++y) {
      var rowControls = cellControls[y];
      @this.RowStyles.Add(new(SizeType.AutoSize));

      for (var x = 0; x < rowControls.Length; ++x)
        @this.Controls.Add(rowControls[x], x, y);
    }

    //add last row for sizing
    @this.RowStyles.Add(new(SizeType.AutoSize));
    @this.Controls.Add(new Label { Width = 0, Height = 0 }, 0, cellControls.Length); //hack for correct display
  }

}
