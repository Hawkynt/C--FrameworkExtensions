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
///   Allows specifying certain properties as read-only depending on the underlying object instance.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DataGridViewConditionalReadOnlyAttribute(string isReadOnlyWhen) : Attribute {
  public string IsReadOnlyWhen { get; } = isReadOnlyWhen;

  public bool IsReadOnly(object row)
    => DataGridViewExtensions.GetPropertyValueOrDefault(row, this.IsReadOnlyWhen, false, false, false, false);
}
