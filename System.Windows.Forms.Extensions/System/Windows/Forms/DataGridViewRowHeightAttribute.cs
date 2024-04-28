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

namespace System.Windows.Forms;

/// <summary>
///   Allows setting an exact height in pixels for automatically generated columns.
/// </summary>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public sealed class DataGridViewRowHeightAttribute : Attribute {
  public string RowHeightProperty { get; }
  public string CustomRowHeightEnabledProperty { get; }
  public string CustomRowHeightProperty { get; }
  public int HeightInPixel { get; }

  private readonly Action<DataGridViewRow, object> _applyRowHeightAction;

  public DataGridViewRowHeightAttribute(int heightInPixel) {
    this.HeightInPixel = heightInPixel;

    this._applyRowHeightAction = this._ApplyPixelRowHeightUnconditional;
  }

  public DataGridViewRowHeightAttribute(int heightInPixel, string customRowHeightEnabledProperty) {
    this.HeightInPixel = heightInPixel;
    this.CustomRowHeightEnabledProperty = customRowHeightEnabledProperty;

    this._applyRowHeightAction = this._ApplyPixelRowHeightConditional;
  }

  public DataGridViewRowHeightAttribute(string customRowHeightProperty) {
    this.CustomRowHeightProperty = customRowHeightProperty;

    this._applyRowHeightAction = this._ApplyPropertyConrolledRowHeightUnconditional;
  }

  public DataGridViewRowHeightAttribute(string customRowHeightProperty, string customRowHeightEnabledProperty) {
    this.CustomRowHeightProperty = customRowHeightProperty;
    this.CustomRowHeightEnabledProperty = customRowHeightEnabledProperty;

    this._applyRowHeightAction = this._ApplyPropertyConrolledRowHeightConditional;
  }

  private void _ApplyPixelRowHeightUnconditional(DataGridViewRow row, object rowData) {
    row.MinimumHeight = this.HeightInPixel;
    row.Height = this.HeightInPixel;
  }

  private void _ApplyPixelRowHeightConditional(DataGridViewRow row, object rowData) {
    if (!DataGridViewExtensions.GetPropertyValueOrDefault(rowData, this.CustomRowHeightEnabledProperty, false, false,
          false, false))
      return;

    row.MinimumHeight = this.HeightInPixel;
    row.Height = this.HeightInPixel;
  }

  private void _ApplyPropertyConrolledRowHeightUnconditional(DataGridViewRow row, object rowData) {
    var originalHeight = row.Height;
    var rowHeight = DataGridViewExtensions.GetPropertyValueOrDefault(rowData, this.CustomRowHeightProperty,
      originalHeight, originalHeight, originalHeight, originalHeight);

    row.MinimumHeight = rowHeight;
    row.Height = rowHeight;
  }

  private void _ApplyPropertyConrolledRowHeightConditional(DataGridViewRow row, object rowData) {
    if (!DataGridViewExtensions.GetPropertyValueOrDefault(rowData, this.CustomRowHeightEnabledProperty, false, false,
          false, false))
      return;

    var originalHeight = row.Height;
    var rowHeight = DataGridViewExtensions.GetPropertyValueOrDefault(rowData, this.CustomRowHeightProperty,
      originalHeight, originalHeight, originalHeight, originalHeight);

    row.MinimumHeight = rowHeight;
    row.Height = rowHeight;
  }

  public void ApplyTo(object rowData, DataGridViewRow row) => this._applyRowHeightAction?.Invoke(row, rowData);
}
