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

namespace System.Windows.Forms;

public static partial class ControlExtensions {
  [CompilerGenerated]
  // ReSharper disable once InconsistentNaming
  private sealed class __ActionWithDummyParameterWrapper {
#pragma warning disable CC0074 // Make field readonly
    public Action method;
#pragma warning restore CC0074 // Make field readonly

#pragma warning disable CC0057 // Unused parameters
    public void Invoke(object _) => this.method();
#pragma warning restore CC0057 // Unused parameters
  }
}
