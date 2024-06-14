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

#if !SUPPORTS_HAS_FLAG

namespace System;

public static partial class EnumPolyfills {
  public static bool HasFlag<T>(this T @this, T flag) where T : Enum
    => ((ulong)(object)@this & (ulong)(object)flag) == (ulong)(object)flag;
}

#endif
