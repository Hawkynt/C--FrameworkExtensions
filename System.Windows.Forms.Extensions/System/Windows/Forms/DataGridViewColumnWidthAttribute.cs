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
///   Allows setting an exact width in pixels for automatically generated columns.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DataGridViewColumnWidthAttribute : Attribute {
  public DataGridViewColumnWidthAttribute(char characters) {
    this.Characters = new('@', characters);
    this.Width = -1;
    this.Mode = DataGridViewAutoSizeColumnMode.None;
  }

  public DataGridViewColumnWidthAttribute(string characters) {
    this.Characters = characters;
    this.Width = -1;
    this.Mode = DataGridViewAutoSizeColumnMode.None;
  }

  public DataGridViewColumnWidthAttribute(int width) {
    this.Characters = null;
    this.Width = width;
    this.Mode = DataGridViewAutoSizeColumnMode.None;
  }

  public DataGridViewColumnWidthAttribute(DataGridViewAutoSizeColumnMode mode) {
    this.Characters = null;
    this.Mode = mode;
    this.Width = -1;
  }

  public DataGridViewAutoSizeColumnMode Mode { get; }
  public int Width { get; }
  public string Characters { get; }

  public void ApplyTo(DataGridViewColumn column) {
    if (this.Mode != DataGridViewAutoSizeColumnMode.None) {
      column.AutoSizeMode = this.Mode;
      return;
    }

    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

    if (this.Characters != null) {
      var font = column.DataGridView.Font;
      var width = TextRenderer.MeasureText(this.Characters, font);
      column.MinimumWidth = width.Width;
      column.Width = width.Width;
    } else if (this.Width >= 0) {
      column.MinimumWidth = this.Width;
      column.Width = this.Width;
    }
  }
}
