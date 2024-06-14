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

using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Controls;

public static partial class TextBoxExtensions {
  private static partial class NativeMethods {
    [DllImport("user32", EntryPoint = "GetCaretPos", SetLastError = true)]
    public static extern int _GetCaretPos(out Point p);

    [DllImport("user32", EntryPoint = "SetCaretPos", SetLastError = true)]
    public static extern int _SetCaretPos(int x, int y);
  }
}
