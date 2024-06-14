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

namespace System.Windows.Forms;

/// <summary>
///   Allows specifying a string or image property to be used as a button column.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DataGridViewButtonColumnAttribute : Attribute {
  /// <summary>
  ///   Initializes a new instance of the <see cref="DataGridViewButtonColumnAttribute" /> class.
  /// </summary>
  /// <param name="onClickMethodName">The target method name to call upon click.</param>
  /// <param name="isEnabledWhen">The boolean property which enables or disables the buttons.</param>
  public DataGridViewButtonColumnAttribute(string onClickMethodName, string isEnabledWhen = null) {
    this.OnClickMethodName = onClickMethodName;
    this.IsEnabledWhen = isEnabledWhen;
  }

  public string IsEnabledWhen { get; }

  public string OnClickMethodName { get; }

  /// <summary>
  ///   Executes the callback with the given object instance.
  /// </summary>
  /// <param name="row">The value.</param>
  public void OnClick(object row) {
    if (this.IsEnabled(row))
      DataGridViewExtensions.CallLateBoundMethod(row, this.OnClickMethodName);
  }

  public bool IsEnabled(object row)
    => DataGridViewExtensions.GetPropertyValueOrDefault(row, this.IsEnabledWhen, false, true, false, false);
}
