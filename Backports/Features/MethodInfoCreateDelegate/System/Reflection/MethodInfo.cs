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

#if !SUPPORTS_METHODINFO_CREATEDELEGATE

namespace System.Reflection;

public static partial class MethodInfoPolyfills {
  public static Delegate CreateDelegate(this MethodInfo @this, Type result) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));
    if (result == null)
      throw new ArgumentNullException(nameof(result));

    return Delegate.CreateDelegate(result, @this);
  }
}

#endif
