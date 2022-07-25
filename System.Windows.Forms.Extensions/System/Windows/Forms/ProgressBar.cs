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

#if NET40_OR_GREATER || NET5_0_OR_GREATER || NETCOREAPP || NETSTANDARD
#define SUPPORTS_CONTRACTS 
#endif

#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif

namespace System.Windows.Forms {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class ProgressBarExtensions {

    /// <summary>
    /// Sets the progress bar percentage.
    /// </summary>
    /// <param name="This">This ProgressBar.</param>
    /// <param name="percentage">The percentage to set.</param>
    public static void SetPercent(this ProgressBar This, double percentage) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
      This.Value = (int)(This.Minimum + (This.Maximum - This.Minimum) * Math.Min(Math.Max(percentage, 0), 100) * 0.01d);
    }

    /// <summary>
    /// Sets the progress bar value.
    /// </summary>
    /// <param name="This">This ProgressBar.</param>
    /// <param name="value">The normalized value to set (0&lt;x&lt;1).</param>
    public static void SetNormalizedValue(this ProgressBar This, double value) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
      This.Value = (int)(This.Minimum + (This.Maximum - This.Minimum) * Math.Min(Math.Max(value, 0), 1));
    }

    /// <summary>
    /// Sets the progress bar according to current and max, without changing it's minimum and maximum values.
    /// </summary>
    /// <param name="This">This ProgressBar.</param>
    /// <param name="current">The current value.</param>
    /// <param name="max">The maximum value to assume.</param>
    public static void SetValue(this ProgressBar This, double current, double max) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
      This.SetPercent(max == 0 ? 0 : (current / max * 100));
    }

    /// <summary>
    /// Sets the progress bar according to current, min and max, without changing it's minimum and maximum values.
    /// </summary>
    /// <param name="This">This ProgressBar.</param>
    /// <param name="current">The current value.</param>
    /// <param name="min">The minimum value to assume.</param>
    /// <param name="max">The maximum value to assume.</param>
    public static void SetValue(this ProgressBar This, double current, double min, double max) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
      var newMax = max - min;
      This.SetPercent(newMax == 0 ? 0 : ((current - min) / newMax) * 100);
    }
  }
}