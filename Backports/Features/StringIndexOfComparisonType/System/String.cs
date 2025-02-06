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

#if !SUPPORTS_STRING_CONTAINS_COMPARISON_TYPE

#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif
namespace System;

public static partial class StringPolyfills {
  
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static int IndexOf(this string @this, char value, StringComparison comparisonType) {
    if (@this == null)
      throw new NullReferenceException(nameof(@this));
    
    return @this.IndexOf(value.ToString(), comparisonType);
  }

}

#endif
