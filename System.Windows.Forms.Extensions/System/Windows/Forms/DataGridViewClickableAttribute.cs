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

using System.Collections.Concurrent;

namespace System.Windows.Forms;

[AttributeUsage(AttributeTargets.Property)]
public class DataGridViewClickableAttribute(string onClickMethodName = null, string onDoubleClickMethodName = null)
  : Attribute {
  public string OnClickMethodName { get; } = onClickMethodName;
  public string OnDoubleClickMethodName { get; } = onDoubleClickMethodName;

  private static readonly ConcurrentDictionary<object, System.Threading.Timer> _clickTimers = [];

  private void _HandleClick(object row) {
    _clickTimers.TryRemove(row, out _);
    DataGridViewExtensions.CallLateBoundMethod(row, this.OnClickMethodName);
  }

  public void OnClick(object row) {
    if (this.OnDoubleClickMethodName == null)
      DataGridViewExtensions.CallLateBoundMethod(row, this.OnClickMethodName);

    var newTimer = new System.Threading.Timer(this._HandleClick, row, SystemInformation.DoubleClickTime, int.MaxValue);
    do
      if (_clickTimers.TryRemove(row, out var timer))
        timer.Dispose();
    while (!_clickTimers.TryAdd(row, newTimer));
  }

  public void OnDoubleClick(object row) {
    if (_clickTimers.TryRemove(row, out var timer))
      timer.Dispose();

    DataGridViewExtensions.CallLateBoundMethod(row, this.OnDoubleClickMethodName);
  }
}
