﻿#region (c)2010-2042 Hawkynt

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

#if !SUPPORTS_WAITHANDLE_DISPOSE

using Guard;
using System.Reflection;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Threading;

public static partial class WaitHandlePolyfills {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Dispose(this WaitHandle @this) {
    if (@this == null)
      AlwaysThrow.NullReferenceException(nameof(@this));

    var method = @this.GetType().GetMethod("Dispose", BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
    method?.Invoke(@this, []);
  }
}

#endif
