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

using Guard;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable PartialTypeWithSinglePart

namespace System.Windows.Forms;

public static partial class ToolStripProgressBarExtensions {
  /// <summary>
  ///   Sets the progress bar percentage.
  /// </summary>
  /// <param name="this">This ToolStripProgressBar.</param>
  /// <param name="percentage">The percentage to set.</param>
  public static void SetPercent(this ToolStripProgressBar @this, double percentage) {
    Against.ThisIsNull(@this);

    @this.SetNormalizedValue(percentage * 0.01d);
  }

  /// <summary>
  ///   Sets the progressbar's normalized value.
  /// </summary>
  /// <param name="this">This ToolStripProgressBar.</param>
  /// <param name="value">The value between 0 and 1 both included, representing the current progress.</param>
  public static void SetNormalizedValue(this ToolStripProgressBar @this, float value) {
    Against.ThisIsNull(@this);

    @this.Value = (int)(@this.Minimum + (@this.Maximum - @this.Minimum) * Math.Min(Math.Max(value, 0), 1));
  }

  /// <summary>
  ///   Sets the progressbar's normalized value.
  /// </summary>
  /// <param name="this">This ToolStripProgressBar.</param>
  /// <param name="value">The value between 0 and 1 both included, representing the current progress.</param>
  public static void SetNormalizedValue(this ToolStripProgressBar @this, double value) {
    Against.ThisIsNull(@this);

    @this.Value = (int)(@this.Minimum + (@this.Maximum - @this.Minimum) * Math.Min(Math.Max(value, 0), 1));
  }

  /// <summary>
  ///   Sets the progress bar according to current and max, without changing it's minimum and maximum values.
  /// </summary>
  /// <param name="this">This ToolStripProgressBar.</param>
  /// <param name="current">The current value.</param>
  /// <param name="max">The maximum value to assume.</param>
  public static void SetValue(this ToolStripProgressBar @this, double current, double max) {
    Against.ThisIsNull(@this);

    @this.SetNormalizedValue(max == 0 ? 0 : current / max);
  }

  /// <summary>
  ///   Sets the progress bar according to current, min and max, without changing it's minimum and maximum values.
  /// </summary>
  /// <param name="this">This ToolStripProgressBar.</param>
  /// <param name="current">The current value.</param>
  /// <param name="min">The minimum value to assume.</param>
  /// <param name="max">The maximum value to assume.</param>
  public static void SetValue(this ToolStripProgressBar @this, double current, double min, double max) {
    Against.ThisIsNull(@this);

    var newMax = max - min;
    @this.SetNormalizedValue(newMax == 0 ? 0 : (current - min) / newMax);
  }
  
}
