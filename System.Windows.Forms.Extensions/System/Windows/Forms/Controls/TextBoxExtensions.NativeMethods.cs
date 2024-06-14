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

using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Controls;

public static partial class TextBoxExtensions {
  private static partial class NativeMethods {
    [DllImport("user32", EntryPoint = "GetCaretPos", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool _GetCaretPos(out Point p);

    public static Point GetCaretPos() => _GetCaretPos(out var result) ? result : throw new Win32Exception();

    [DllImport("user32", EntryPoint = "SetCaretPos", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool _SetCaretPos(int x, int y);

    public static void SetCaretPos(Point p) {
      if (!_SetCaretPos(p.X, p.Y))
        throw new Win32Exception();
    }
  }
}
