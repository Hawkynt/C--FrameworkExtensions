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

/// <summary>
///   Allows specifying a column to host a combobox contaning the elements specified by a property.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DataGridViewComboboxColumnAttribute(string dataSourcePropertyName, string enabledWhenPropertyName = null, string valueMember = null, string displayMember = null)
  : Attribute {
  public string EnabledWhenPropertyName { get; } = enabledWhenPropertyName;
  public string ValueMember { get; } = valueMember;
  public string DisplayMember { get; } = displayMember;
  public string DataSourcePropertyName { get; } = dataSourcePropertyName;
}
