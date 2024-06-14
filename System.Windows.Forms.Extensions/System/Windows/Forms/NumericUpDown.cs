#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY

#endregion

using Guard;

namespace System.Windows.Forms;

public static partial class NumericUpDownExtensions {
  /// <summary>
  ///   Sets the min,max & step values.
  /// </summary>
  /// <param name="This">This NumericUpDown.</param>
  /// <param name="min">The min.</param>
  /// <param name="max">The max.</param>
  /// <param name="step">The step.</param>
  public static void SetMinMaxStep(this NumericUpDown This, decimal min, decimal max, decimal step) {
    Against.ThisIsNull(This);

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
    Against.ThisIsNull(This);

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
    Against.ThisIsNull(This);

    This.SetMinMaxStep((decimal)min, max, step);
  }

  /// <summary>
  ///   Sets the value so that it is between bounds.
  /// </summary>
  /// <param name="This">This NumericUpDown.</param>
  /// <param name="value">The value to set.</param>
  public static void SetSaveValue(this NumericUpDown This, decimal value) {
    Against.ThisIsNull(This);

    This.Value = Math.Min(Math.Max(This.Minimum, value), This.Maximum);
  }

  /// <summary>
  ///   Sets the value so that it is between bounds.
  /// </summary>
  /// <param name="This">This NumericUpDown.</param>
  /// <param name="value">The value to set.</param>
  public static void SetSaveValue(this NumericUpDown This, double value) {
    Against.ThisIsNull(This);

    This.SetSaveValue((decimal)value);
  }

  /// <summary>
  ///   Sets the value so that it is between bounds.
  /// </summary>
  /// <param name="This">This NumericUpDown.</param>
  /// <param name="value">The value to set.</param>
  public static void SetSaveValue(this NumericUpDown This, int value) {
    Against.ThisIsNull(This);

    This.SetSaveValue((decimal)value);
  }

  /// <summary>
  ///   Sets the value so that it is between bounds.
  /// </summary>
  /// <param name="This">This NumericUpDown.</param>
  /// <param name="value">The value to set.</param>
  public static void SetSaveValue(this NumericUpDown This, uint value) {
    Against.ThisIsNull(This);

    This.SetSaveValue((decimal)value);
  }

  /// <summary>
  ///   Sets the value so that it is between bounds.
  /// </summary>
  /// <param name="This">This NumericUpDown.</param>
  /// <param name="value">The value to set.</param>
  public static void SetSaveValue(this NumericUpDown This, long value) {
    Against.ThisIsNull(This);

    This.SetSaveValue((decimal)value);
  }

  /// <summary>
  ///   Sets the value so that it is between bounds.
  /// </summary>
  /// <param name="This">This NumericUpDown.</param>
  /// <param name="value">The value to set.</param>
  public static void SetSaveValue(this NumericUpDown This, ulong value) {
    Against.ThisIsNull(This);

    This.SetSaveValue((decimal)value);
  }
}
