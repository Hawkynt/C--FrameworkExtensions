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

using System.Diagnostics;

namespace System.ComponentModel;

public static partial class SynchronizeInvokeExtensions {
  public static bool SafeInvoke<T>(this T @this, Action<T> call, bool async = false) where T : class, ISynchronizeInvoke {
    Debug.Assert(@this != null);
    Debug.Assert(call != null);

    if (@this.InvokeRequired) {
      if (async)
        @this.BeginInvoke(new Action<T>(call), [@this]);
      else
        @this.Invoke(new Action<T>(call), [@this]);
      return false;
    }

    call(@this);
    return true;
  }
}
