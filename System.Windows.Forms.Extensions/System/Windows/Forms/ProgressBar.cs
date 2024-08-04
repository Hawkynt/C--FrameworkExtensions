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

using Guard;

namespace System.Windows.Forms;

public static partial class ProgressBarExtensions {

  /// <summary>
  /// Sets the value of the <see cref="ProgressBar"/> to the specified percentage.
  /// </summary>
  /// <param name="this">This <see cref="ProgressBar"/> instance.</param>
  /// <param name="percentage">The percentage to set the progress bar value to, where 0.0 represents 0% and 100.0 represents 100%.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="System.ArgumentOutOfRangeException">Thrown if <paramref name="percentage"/> is less than 0.0 or greater than 100.0.</exception>
  /// <example>
  /// <code>
  /// ProgressBar progressBar = new ProgressBar();
  /// progressBar.SetPercent(75.0);
  /// // The progress bar is now set to 75%.
  /// </code>
  /// </example>
  public static void SetPercent(this ProgressBar @this, double percentage) {
    Against.ThisIsNull(@this);

    @this.Value = (int)(@this.Minimum + (@this.Maximum - @this.Minimum) * Math.Min(Math.Max(percentage, 0), 100) * 0.01d);
  }

  /// <summary>
  /// Sets the value of the <see cref="ProgressBar"/> to the specified normalized value.
  /// </summary>
  /// <param name="this">This <see cref="ProgressBar"/> instance.</param>
  /// <param name="value">The normalized value to set, where 0.0 represents the minimum value and 1.0 represents the maximum value.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="System.ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is less than 0.0 or greater than 1.0.</exception>
  /// <example>
  /// <code>
  /// ProgressBar progressBar = new ProgressBar();
  /// progressBar.SetNormalizedValue(0.75);
  /// // The progress bar is now set to 75% of its range.
  /// </code>
  /// </example>
  public static void SetNormalizedValue(this ProgressBar @this, double value) {
    Against.ThisIsNull(@this);

    @this.Value = (int)(@this.Minimum + (@this.Maximum - @this.Minimum) * Math.Min(Math.Max(value, 0), 1));
  }

  /// <summary>
  /// Sets the value of the <see cref="ProgressBar"/> based on the specified current and maximum values.
  /// </summary>
  /// <param name="this">This <see cref="ProgressBar"/> instance.</param>
  /// <param name="current">The current value to set.</param>
  /// <param name="max">The maximum value to set.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="System.ArgumentOutOfRangeException">Thrown if <paramref name="current"/> is less than 0.0 or greater than <paramref name="max"/>.</exception>
  /// <example>
  /// <code>
  /// ProgressBar progressBar = new ProgressBar();
  /// progressBar.SetValue(30.0, 100.0);
  /// // The progress bar is now set to 30% of its range.
  /// </code>
  /// </example>
  public static void SetValue(this ProgressBar @this, double current, double max) {
    Against.ThisIsNull(@this);

    @this.SetPercent(max == 0 ? 0 : current / max * 100);
  }

  /// <summary>
  /// Sets the value of the <see cref="ProgressBar"/> based on the specified current, minimum, and maximum values.
  /// </summary>
  /// <param name="this">This <see cref="ProgressBar"/> instance.</param>
  /// <param name="current">The current value to set.</param>
  /// <param name="min">The minimum value of the range.</param>
  /// <param name="max">The maximum value of the range.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="System.ArgumentOutOfRangeException">Thrown if <paramref name="current"/> is less than <paramref name="min"/> or greater than <paramref name="max"/>.</exception>
  /// <example>
  /// <code>
  /// ProgressBar progressBar = new ProgressBar();
  /// progressBar.SetValue(30.0, 0.0, 100.0);
  /// // The progress bar is now set to 30% of its range.
  /// </code>
  /// </example>
  public static void SetValue(this ProgressBar @this, double current, double min, double max) {
    Against.ThisIsNull(@this);

    var newMax = max - min;
    @this.SetPercent(newMax == 0 ? 0 : (current - min) / newMax * 100);
  }
}
