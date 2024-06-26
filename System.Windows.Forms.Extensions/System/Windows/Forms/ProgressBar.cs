﻿#region (c)2010-2042 Hawkynt

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

using Guard;

namespace System.Windows.Forms;

public static partial class ProgressBarExtensions {
  /// <summary>
  ///   Sets the progress bar percentage.
  /// </summary>
  /// <param name="This">This ProgressBar.</param>
  /// <param name="percentage">The percentage to set.</param>
  public static void SetPercent(this ProgressBar This, double percentage) {
    Against.ThisIsNull(This);

    This.Value = (int)(This.Minimum + (This.Maximum - This.Minimum) * Math.Min(Math.Max(percentage, 0), 100) * 0.01d);
  }

  /// <summary>
  ///   Sets the progress bar value.
  /// </summary>
  /// <param name="This">This ProgressBar.</param>
  /// <param name="value">The normalized value to set (0&lt;x&lt;1).</param>
  public static void SetNormalizedValue(this ProgressBar This, double value) {
    Against.ThisIsNull(This);

    This.Value = (int)(This.Minimum + (This.Maximum - This.Minimum) * Math.Min(Math.Max(value, 0), 1));
  }

  /// <summary>
  ///   Sets the progress bar according to current and max, without changing it's minimum and maximum values.
  /// </summary>
  /// <param name="This">This ProgressBar.</param>
  /// <param name="current">The current value.</param>
  /// <param name="max">The maximum value to assume.</param>
  public static void SetValue(this ProgressBar This, double current, double max) {
    Against.ThisIsNull(This);

    This.SetPercent(max == 0 ? 0 : current / max * 100);
  }

  /// <summary>
  ///   Sets the progress bar according to current, min and max, without changing it's minimum and maximum values.
  /// </summary>
  /// <param name="This">This ProgressBar.</param>
  /// <param name="current">The current value.</param>
  /// <param name="min">The minimum value to assume.</param>
  /// <param name="max">The maximum value to assume.</param>
  public static void SetValue(this ProgressBar This, double current, double min, double max) {
    Against.ThisIsNull(This);

    var newMax = max - min;
    This.SetPercent(newMax == 0 ? 0 : (current - min) / newMax * 100);
  }
}
