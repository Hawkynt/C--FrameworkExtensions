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

using System.Collections.Concurrent;

namespace System.Windows.Forms;

[AttributeUsage(AttributeTargets.Property)]
public class DataGridViewClickableAttribute : Attribute {
  public DataGridViewClickableAttribute(string onClickMethodName = null, string onDoubleClickMethodName = null) {
    this.OnClickMethodName = onClickMethodName;
    this.OnDoubleClickMethodName = onDoubleClickMethodName;
  }

  public string OnClickMethodName { get; }
  public string OnDoubleClickMethodName { get; }

  private static readonly ConcurrentDictionary<object, System.Threading.Timer> _clickTimers = new();

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
