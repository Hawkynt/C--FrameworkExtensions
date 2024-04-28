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
///   Allows specifying a column to host a combobox contaning the elements specified by a property.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DataGridViewComboboxColumnAttribute : Attribute {
  public string EnabledWhenPropertyName { get; }
  public string ValueMember { get; }
  public string DisplayMember { get; }
  public string DataSourcePropertyName { get; }

  public DataGridViewComboboxColumnAttribute(string dataSourcePropertyName, string enabledWhenPropertyName = null,
    string valueMember = null, string displayMember = null) {
    this.EnabledWhenPropertyName = enabledWhenPropertyName;
    this.ValueMember = valueMember;
    this.DisplayMember = displayMember;
    this.DataSourcePropertyName = dataSourcePropertyName;
  }
}
