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

#if !SUPPORTS_METHODINFO_CREATEDELEGATE

using Guard;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Reflection;

public static partial class MethodInfoPolyfills {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Delegate CreateDelegate(this MethodInfo @this, Type result) {
    if (@this == null)
      AlwaysThrow.ArgumentNullException(nameof(@this));
    if (result == null)
      AlwaysThrow.ArgumentNullException(nameof(result));

    return Delegate.CreateDelegate(result, @this);
  }
}

#endif
