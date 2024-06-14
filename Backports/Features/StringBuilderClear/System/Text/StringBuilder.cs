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

#if !SUPPORTS_STRING_BUILDER_CLEAR

namespace System.Text;

public static partial class StringBuilderPolyfills {
  public static void Clear(this StringBuilder @this) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));

    @this.Length = 0;
  }
}

#endif
