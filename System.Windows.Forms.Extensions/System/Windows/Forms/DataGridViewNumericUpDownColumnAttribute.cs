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
///   Allows specifying a value to be used as column with numeric up down control
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DataGridViewNumericUpDownColumnAttribute : Attribute {
  public decimal Minimum { get; }
  public decimal Maximum { get; }
  public int DecimalPlaces { get; }
  public decimal Increment { get; }

  public DataGridViewNumericUpDownColumnAttribute(double minimum, double maximum, double increment = 1, int decimalPlaces = 2) {
    this.Minimum = (decimal)minimum;
    this.Maximum = (decimal)maximum;
    this.Increment = (decimal)increment;
    this.DecimalPlaces = decimalPlaces;
  }
}
