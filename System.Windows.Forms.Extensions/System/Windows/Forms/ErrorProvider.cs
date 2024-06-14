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

public static partial class ErrorProviderExtensions {
  public static void Clear(this ErrorProvider @this, Control control) => @this.SetError(control, null);
}
