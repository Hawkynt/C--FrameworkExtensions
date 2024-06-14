#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY

#endregion

using System.Drawing;

namespace System.Windows.Forms;

public static partial class DataGridViewExtensions {
  /// <summary>
  ///   Saves the state of a DataGridView during Enable/Disable state transitions.
  /// </summary>
  private sealed class DataGridViewState {
    private readonly bool _readonly;
    private readonly Color _defaultCellStyleBackColor;
    private readonly Color _defaultCellStyleForeColor;
    private readonly Color _columnHeadersDefaultCellStyleBackColor;
    private readonly Color _columnHeadersDefaultCellStyleForeColor;
    private readonly bool _enableHeadersVisualStyles;
    private readonly Color _backgroundColor;

    private DataGridViewState(
      bool @readonly,
      Color defaultCellStyleBackColor,
      Color defaultCellStyleForeColor,
      Color columnHeadersDefaultCellStyleBackColor,
      Color columnHeadersDefaultCellStyleForeColor,
      bool enableHeadersVisualStyles,
      Color backgroundColor
    ) {
      this._readonly = @readonly;
      this._defaultCellStyleBackColor = defaultCellStyleBackColor;
      this._defaultCellStyleForeColor = defaultCellStyleForeColor;
      this._columnHeadersDefaultCellStyleBackColor = columnHeadersDefaultCellStyleBackColor;
      this._columnHeadersDefaultCellStyleForeColor = columnHeadersDefaultCellStyleForeColor;
      this._enableHeadersVisualStyles = enableHeadersVisualStyles;
      this._backgroundColor = backgroundColor;
    }

    /// <summary>
    ///   Restores the saved state to the given DataGridView.
    /// </summary>
    /// <param name="dataGridView">The DataGridView to restore state to.</param>
    public void RestoreTo(DataGridView dataGridView) {
      using var _ = dataGridView.PauseLayout();
      dataGridView.ReadOnly = this._readonly;
      dataGridView.DefaultCellStyle.BackColor = this._defaultCellStyleBackColor;
      dataGridView.DefaultCellStyle.ForeColor = this._defaultCellStyleForeColor;
      dataGridView.ColumnHeadersDefaultCellStyle.BackColor = this._columnHeadersDefaultCellStyleBackColor;
      dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = this._columnHeadersDefaultCellStyleForeColor;
      dataGridView.EnableHeadersVisualStyles = this._enableHeadersVisualStyles;
      dataGridView.BackgroundColor = this._backgroundColor;
    }

    /// <summary>
    ///   Saves the state of the given DataGridView.
    /// </summary>
    /// <param name="dataGridView">The DataGridView to save state from.</param>
    /// <returns></returns>
    public static DataGridViewState FromDataGridView(DataGridView dataGridView) =>
      new(
        dataGridView.ReadOnly,
        dataGridView.DefaultCellStyle.BackColor,
        dataGridView.DefaultCellStyle.ForeColor,
        dataGridView.ColumnHeadersDefaultCellStyle.BackColor,
        dataGridView.ColumnHeadersDefaultCellStyle.ForeColor,
        dataGridView.EnableHeadersVisualStyles,
        dataGridView.BackgroundColor
      );

    public static void ChangeToDisabled(DataGridView dataGridView) {
      using var _ = dataGridView.PauseLayout();
      dataGridView.ReadOnly = true;
      dataGridView.EnableHeadersVisualStyles = false;
      dataGridView.DefaultCellStyle.ForeColor = SystemColors.GrayText;
      dataGridView.DefaultCellStyle.BackColor = SystemColors.Control;
      dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = SystemColors.GrayText;
      dataGridView.ColumnHeadersDefaultCellStyle.BackColor = SystemColors.Control;
      dataGridView.BackgroundColor = SystemColors.Control;
    }
  }
}
