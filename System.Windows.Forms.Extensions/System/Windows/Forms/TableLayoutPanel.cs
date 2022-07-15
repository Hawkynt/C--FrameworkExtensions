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
#if !NET35_OR_GREATER
using System.Diagnostics.Contracts;
#else 
using Debug = System.Diagnostics.Debug;
#endif
using System.Linq;

namespace System.Windows.Forms {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class TableLayoutPanelExtensions {
    /// <summary>
    /// Removes the given row from a TableLayoutPanel.
    /// </summary>
    /// <param name="This">This TableLayoutPanel.</param>
    /// <param name="row">The row.</param>
    public static void RemoveRow(this TableLayoutPanel This, int row) {
#if NET35_OR_GREATER
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var rowCount = This.RowCount;
      if (row < 0 || row >= rowCount)
        return;
      var columnCount = This.ColumnCount;

      // delete all controls of the given row
      for (var i = columnCount; i > 0;) {
        --i;
        var control = This.GetControlFromPosition(i, row);
        if (control == null)
          continue;
        This.Controls.Remove(control);
      }

      // order all controls one up
      for (var i = row + 1; i < rowCount; ++i) {
        for (var j = columnCount; j > 0;) {
          --j;
          var control = This.GetControlFromPosition(j, i);
          if (control == null)
            continue;
          This.SetRow(control, i - 1);
        }
      }

      // remove style
      This.RowStyles.RemoveAt(row);

      // decrement total rows
      This.RowCount--;
    }

    /// <summary>
    /// Copies the last row of a This and all controls in it.
    /// </summary>
    /// <param name="This">This TableLayoutPanel.</param>
    /// <param name="controlCallback">The control callback if any, passing targetRow,targetColumn,sourceControl,targetControl.</param>
    /// <param name="allowSuspendingLayout">if set to <c>true</c> allows suspending the This layout during this process to prevent flickering.</param>
    public static void CopyLastRow(this TableLayoutPanel This, Action<int, int, Control, Control> controlCallback = null, bool allowSuspendingLayout = true) {
#if NET35_OR_GREATER
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      if (allowSuspendingLayout)
        This.SuspendLayout();

      var lastRow = This.RowCount - 1;

      // add line
      This.RowCount++;

      // add style
      var rowStyle = This.RowStyles[lastRow];
      This.RowStyles.Add(new RowStyle(rowStyle.SizeType, rowStyle.Height));

      // copy controls
      var alreadyVisitedControls = new Dictionary<Control, bool>();
      for (var i = This.ColumnCount; i > 0;) {
        --i;

        var control = This.GetControlFromPositionFixed(i, lastRow);
        if (control == null || !alreadyVisitedControls.TryAdd(control, true))
          continue;

        var newControl = control.Duplicate();
        if (controlCallback != null)
          controlCallback(lastRow + 1, i, control, newControl);

        This.Controls.Add(newControl, i, lastRow + 1);
        This.SetRowSpan(newControl, This.GetRowSpan(control));
        This.SetColumnSpan(newControl, This.GetColumnSpan(control));
      }

      if (allowSuspendingLayout)
        This.ResumeLayout(true);
    }
    /// <summary>
    /// Copies the last row of a This and all controls in it.
    /// </summary>
    /// <param name="This">This TableLayoutPanel.</param>
    /// <param name="controlCallback">The control callback, passing the newly created control.</param>
    /// <param name="allowSuspendingLayout">if set to <c>true</c> allows suspending the This layout during this process to prevent flickering.</param>
    public static void CopyLastRow(this TableLayoutPanel This, Action<Control> controlCallback, bool allowSuspendingLayout = true) {
#if NET35_OR_GREATER
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      This.CopyLastRow((row, col, src, tgt) => controlCallback(tgt));
    }
    /// <summary>
    /// Copies the last row of a This and all controls in it.
    /// </summary>
    /// <param name="This">This TableLayoutPanel.</param>
    /// <param name="controlCallback">The control callback, passing column and the newly created control.</param>
    /// <param name="allowSuspendingLayout">if set to <c>true</c> allows suspending the This layout during this process to prevent flickering.</param>
    public static void CopyLastRow(this TableLayoutPanel This, Action<int, Control> controlCallback, bool allowSuspendingLayout = true) {
#if NET35_OR_GREATER
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      This.CopyLastRow((row, col, src, tgt) => controlCallback(col, tgt));
    }

    /// <summary>
    /// Gets a value from a control in the given column and row.
    /// </summary>
    /// <typeparam name="TControl">The type of the control which should be there.</typeparam>
    /// <typeparam name="TType">The type of the value we want to read from it.</typeparam>
    /// <param name="This">This TableLayoutPanel.</param>
    /// <param name="row">The row in which the control should be.</param>
    /// <param name="columnIndex">The column in which the control should be.</param>
    /// <param name="reader">The reader delegate which reads the actual value from the given control and returns it.</param>
    /// <returns>The value that was found.</returns>
    public static TType GetColumnValue<TControl, TType>(this TableLayoutPanel This, uint row, int columnIndex, Func<TControl, TType> reader) where TControl : Control {
#if NET35_OR_GREATER
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      if (row >= This.RowCount)
        throw new ArgumentOutOfRangeException("row", row, "Not Allowed");
      var control = This.GetControlFromPositionFixed<TControl>(columnIndex, (int)row);
      if (control == null)
        throw new NotSupportedException();
      return (reader(control));
    }


    /// <summary>
    /// Sets a controls' value.
    /// </summary>
    /// <typeparam name="TControl">The type of the control which should be there.</typeparam>
    /// <param name="This">This TableLayoutPanel.</param>
    /// <param name="row">The row in which the control should be.</param>
    /// <param name="columnIndex">The column in which the control should be.</param>
    /// <param name="writer">The writer delegate which sets a value of the control.</param>
    public static void SetColumnValue<TControl>(this TableLayoutPanel This, uint row, int columnIndex, Action<TControl> writer) where TControl : Control {
#if NET35_OR_GREATER
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      if (row >= This.RowCount)
        throw new ArgumentOutOfRangeException("row", row, "Not Allowed");

      var control = This.GetControlFromPositionFixed<TControl>(columnIndex, (int)row);
      if (control == null)
        throw new NotSupportedException();
      writer(control);
    }


    /// <summary>
    /// Gets a control from a position in a TableLayoutPanel.
    /// Note: when no control was found, each control is checked on it's own because of bugs in the layout engine from Microsoft.
    /// </summary>
    /// <typeparam name="TControl">The type of the control.</typeparam>
    /// <param name="This">This TableLayoutPanel.</param>
    /// <param name="column">The column.</param>
    /// <param name="row">The row.</param>
    /// <returns>
    /// The control or <c>null</c>.
    /// </returns>
    public static TControl GetControlFromPositionFixed<TControl>(this TableLayoutPanel This, int column, int row) where TControl : Control {
#if NET35_OR_GREATER
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      return (This.GetControlFromPositionFixed(column, row) as TControl);
    }

    /// <summary>
    /// Gets a control from a position in a TableLayoutPanel.
    /// Note: when no control was found, each control is checked on it's own because of bugs in the layout engine from Microsoft.
    /// </summary>
    /// <param name="This">This TableLayoutPanel.</param>
    /// <param name="column">The column.</param>
    /// <param name="row">The row.</param>
    /// <returns>The control or <c>null</c>.</returns>
    public static Control GetControlFromPositionFixed(this TableLayoutPanel This, int column, int row) {
#if NET35_OR_GREATER
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var result = This.GetControlFromPosition(column, row);
      if (result != null)
        return (result);

#if DEBUG
      // This one's for debugging only
      var allControls = This.Controls.Cast<Control>().Where(c => c != null).ToDictionary(control => control, control => Tuple.Create(This.GetColumn(control), This.GetRow(control)));
#endif

      return (
        from control in This.Controls.Cast<Control>().Where(c => c != null)
        where This.GetColumn(control) == column && This.GetRow(control) == row
        select (control)
      ).FirstOrDefault();
    }

  }
}