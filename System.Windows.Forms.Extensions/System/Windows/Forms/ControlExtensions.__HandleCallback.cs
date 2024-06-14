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

using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Windows.Forms;

public static partial class ControlExtensions {
  [CompilerGenerated]
  // ReSharper disable once InconsistentNaming
  private sealed class __HandleCallback<TControl> where TControl : Control {
#pragma warning disable CC0074 // Make field readonly
    public Action<TControl> method;
    public ManualResetEventSlim resetEvent;
#pragma warning restore CC0074 // Make field readonly

    public void Invoke(object sender, EventArgs _) {
      var control = (TControl)sender;
      control.HandleCreated -= this.Invoke;
      try {
        this.method(control);
      } finally {
        this.resetEvent?.Set();
      }
    }
  }
}
