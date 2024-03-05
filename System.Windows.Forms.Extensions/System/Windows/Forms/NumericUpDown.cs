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

#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif

namespace System.Windows.Forms;
// ReSharper disable once PartialTypeWithSinglePart
// ReSharper disable once UnusedMember.Global

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  static partial class NumericUpDownExtensions {
  /// <summary>
  ///   Sets the min,max & step values.
  /// </summary>
  /// <param name="This">This NumericUpDown.</param>
  /// <param name="min">The min.</param>
  /// <param name="max">The max.</param>
  /// <param name="step">The step.</param>
  public static void SetMinMaxStep(this NumericUpDown This, decimal min, decimal max, decimal step) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
    This.Minimum = min;
    This.Maximum = max;
    This.Increment = step;
  }

  /// <summary>
  ///   Sets the min,max & step values.
  /// </summary>
  /// <param name="This">This NumericUpDown.</param>
  /// <param name="min">The min.</param>
  /// <param name="max">The max.</param>
  /// <param name="step">The step.</param>
  public static void SetMinMaxStep(this NumericUpDown This, double min, double max, double step) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
    This.SetMinMaxStep((decimal)min, (decimal)max, (decimal)step);
  }

  /// <summary>
  ///   Sets the min,max & step values.
  /// </summary>
  /// <param name="This">This NumericUpDown.</param>
  /// <param name="min">The min.</param>
  /// <param name="max">The max.</param>
  /// <param name="step">The step.</param>
  public static void SetMinMaxStep(this NumericUpDown This, int min, int max, int step) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
    This.SetMinMaxStep((decimal)min, max, step);
  }

  /// <summary>
  ///   Sets the value so that it is between bounds.
  /// </summary>
  /// <param name="This">This NumericUpDown.</param>
  /// <param name="value">The value to set.</param>
  public static void SetSaveValue(this NumericUpDown This, decimal value) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
    This.Value = Math.Min(Math.Max(This.Minimum, value), This.Maximum);
  }

  /// <summary>
  ///   Sets the value so that it is between bounds.
  /// </summary>
  /// <param name="This">This NumericUpDown.</param>
  /// <param name="value">The value to set.</param>
  public static void SetSaveValue(this NumericUpDown This, double value) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
    This.SetSaveValue((decimal)value);
  }

  /// <summary>
  ///   Sets the value so that it is between bounds.
  /// </summary>
  /// <param name="This">This NumericUpDown.</param>
  /// <param name="value">The value to set.</param>
  public static void SetSaveValue(this NumericUpDown This, int value) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
    This.SetSaveValue((decimal)value);
  }

  /// <summary>
  ///   Sets the value so that it is between bounds.
  /// </summary>
  /// <param name="This">This NumericUpDown.</param>
  /// <param name="value">The value to set.</param>
  public static void SetSaveValue(this NumericUpDown This, uint value) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
    This.SetSaveValue((decimal)value);
  }

  /// <summary>
  ///   Sets the value so that it is between bounds.
  /// </summary>
  /// <param name="This">This NumericUpDown.</param>
  /// <param name="value">The value to set.</param>
  public static void SetSaveValue(this NumericUpDown This, long value) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
    This.SetSaveValue((decimal)value);
  }

  /// <summary>
  ///   Sets the value so that it is between bounds.
  /// </summary>
  /// <param name="This">This NumericUpDown.</param>
  /// <param name="value">The value to set.</param>
  public static void SetSaveValue(this NumericUpDown This, ulong value) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
    This.SetSaveValue((decimal)value);
  }
}
