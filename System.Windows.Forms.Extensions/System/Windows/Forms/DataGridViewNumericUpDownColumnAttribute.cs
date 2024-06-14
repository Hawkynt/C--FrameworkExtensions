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
///   Allows specifying a value to be used as column with numeric up down control
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DataGridViewNumericUpDownColumnAttribute(double minimum, double maximum, double increment = 1, int decimalPlaces = 2)
  : Attribute {
  public decimal Minimum { get; } = (decimal)minimum;
  public decimal Maximum { get; } = (decimal)maximum;
  public int DecimalPlaces { get; } = decimalPlaces;
  public decimal Increment { get; } = (decimal)increment;
}
