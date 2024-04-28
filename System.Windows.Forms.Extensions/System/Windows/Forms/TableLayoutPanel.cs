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

using System.Collections.Generic;
using System.Linq;
#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif

namespace System.Windows.Forms;

public static partial class TableLayoutPanelExtensions {
  /// <summary>
  ///   Removes the given row from a TableLayoutPanel.
  /// </summary>
  /// <param name="this">This TableLayoutPanel.</param>
  /// <param name="row">The row.</param>
  public static void RemoveRow(this TableLayoutPanel @this, int row) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
#endif
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
  ///   Copies the last row of a This and all controls in it.
  /// </summary>
  /// <param name="this">This TableLayoutPanel.</param>
  /// <param name="controlCallback">The control callback if any, passing targetRow,targetColumn,sourceControl,targetControl.</param>
  /// <param name="allowSuspendingLayout">
  ///   if set to <c>true</c> allows suspending the This layout during this process to
  ///   prevent flickering.
  /// </param>
  public static void CopyLastRow(this TableLayoutPanel @this, Action<int, int, Control, Control> controlCallback = null,
    bool allowSuspendingLayout = true) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
#endif
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
      if (controlCallback != null)
        controlCallback(lastRow + 1, i, control, newControl);

      @this.Controls.Add(newControl, i, lastRow + 1);
      @this.SetRowSpan(newControl, @this.GetRowSpan(control));
      @this.SetColumnSpan(newControl, @this.GetColumnSpan(control));
    }

    if (allowSuspendingLayout)
      @this.ResumeLayout(true);
  }

  /// <summary>
  ///   Copies the last row of a This and all controls in it.
  /// </summary>
  /// <param name="this">This TableLayoutPanel.</param>
  /// <param name="controlCallback">The control callback, passing the newly created control.</param>
  /// <param name="allowSuspendingLayout">
  ///   if set to <c>true</c> allows suspending the This layout during this process to
  ///   prevent flickering.
  /// </param>
  public static void CopyLastRow(this TableLayoutPanel @this, Action<Control> controlCallback,
    bool allowSuspendingLayout = true) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
#endif
    @this.CopyLastRow((row, col, src, tgt) => controlCallback(tgt));
  }

  /// <summary>
  ///   Copies the last row of a This and all controls in it.
  /// </summary>
  /// <param name="this">This TableLayoutPanel.</param>
  /// <param name="controlCallback">The control callback, passing column and the newly created control.</param>
  /// <param name="allowSuspendingLayout">
  ///   if set to <c>true</c> allows suspending the This layout during this process to
  ///   prevent flickering.
  /// </param>
  public static void CopyLastRow(this TableLayoutPanel @this, Action<int, Control> controlCallback,
    bool allowSuspendingLayout = true) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
#endif
    @this.CopyLastRow((row, col, src, tgt) => controlCallback(col, tgt));
  }

  /// <summary>
  ///   Gets a value from a control in the given column and row.
  /// </summary>
  /// <typeparam name="TControl">The type of the control which should be there.</typeparam>
  /// <typeparam name="TType">The type of the value we want to read from it.</typeparam>
  /// <param name="this">This TableLayoutPanel.</param>
  /// <param name="row">The row in which the control should be.</param>
  /// <param name="columnIndex">The column in which the control should be.</param>
  /// <param name="reader">The reader delegate which reads the actual value from the given control and returns it.</param>
  /// <returns>The value that was found.</returns>
  public static TType GetColumnValue<TControl, TType>(this TableLayoutPanel @this, uint row, int columnIndex,
    Func<TControl, TType> reader) where TControl : Control {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
#endif
    if (row >= @this.RowCount)
      throw new ArgumentOutOfRangeException(nameof(row), row, "Not Allowed");

    var control = @this.GetControlFromPositionFixed<TControl>(columnIndex, (int)row);
    if (control == null)
      throw new NotSupportedException();

    return reader(control);
  }


  /// <summary>
  ///   Sets a controls' value.
  /// </summary>
  /// <typeparam name="TControl">The type of the control which should be there.</typeparam>
  /// <param name="This">This TableLayoutPanel.</param>
  /// <param name="row">The row in which the control should be.</param>
  /// <param name="columnIndex">The column in which the control should be.</param>
  /// <param name="writer">The writer delegate which sets a value of the control.</param>
  public static void SetColumnValue<TControl>(this TableLayoutPanel This, uint row, int columnIndex,
    Action<TControl> writer) where TControl : Control {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
    if (row >= This.RowCount)
      throw new ArgumentOutOfRangeException(nameof(row), row, "Not Allowed");

    var control = This.GetControlFromPositionFixed<TControl>(columnIndex, (int)row);
    if (control == null)
      throw new NotSupportedException();

    writer(control);
  }


  /// <summary>
  ///   Gets a control from a position in a TableLayoutPanel.
  ///   Note: when no control was found, each control is checked on it's own because of bugs in the layout engine from
  ///   Microsoft.
  /// </summary>
  /// <typeparam name="TControl">The type of the control.</typeparam>
  /// <param name="this">This TableLayoutPanel.</param>
  /// <param name="column">The column.</param>
  /// <param name="row">The row.</param>
  /// <returns>
  ///   The control or <c>null</c>.
  /// </returns>
  public static TControl GetControlFromPositionFixed<TControl>(this TableLayoutPanel @this, int column, int row)
    where TControl : Control {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
#endif
    return @this.GetControlFromPositionFixed(column, row) as TControl;
  }

  /// <summary>
  ///   Gets a control from a position in a TableLayoutPanel.
  ///   Note: when no control was found, each control is checked on it's own because of bugs in the layout engine from
  ///   Microsoft.
  /// </summary>
  /// <param name="this">This TableLayoutPanel.</param>
  /// <param name="column">The column.</param>
  /// <param name="row">The row.</param>
  /// <returns>The control or <c>null</c>.</returns>
  public static Control GetControlFromPositionFixed(this TableLayoutPanel @this, int column, int row) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
#endif
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
  ///   <br>A simple way to use the <see cref="TableLayoutPanel" /> as a table and fill it up with controls.</br>
  ///   <br>Info: The columns and their widths need to be already set, the rows will all be auto-sized.</br>
  /// </summary>
  /// <param name="this">This.</param>
  /// <param name="cellControls">The controls to fill up with, first dimension is the rows, 2nd the columns.</param>
  public static void UseAsTable(this TableLayoutPanel @this, params Control[][] cellControls) {
    //init
    @this.SuspendLayout();
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
    @this.ResumeLayout();
  }
}
