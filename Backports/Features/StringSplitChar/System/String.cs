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
//

#if !SUPPORTS_STRING_SPLIT_CHAR

using System.Runtime.CompilerServices;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class StringPolyfills {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string[] Split(this string @this, char separator, int count, StringSplitOptions options = StringSplitOptions.None)
    => @this.Split([separator], count, options)
  ;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string[] Split(this string @this, char separator, StringSplitOptions options = StringSplitOptions.None)
    => @this.Split([separator], options)
  ;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string[] Split(this string @this, string separator, int count, StringSplitOptions options = StringSplitOptions.None) => 
    separator is { Length: not 0 }
    ? @this.Split([separator], count, options)
    : count switch {
      < 0 => AlwaysThrow.ArgumentOutOfRangeException<string[]>(nameof(count)),
      0 => [],
      > 0 when string.IsNullOrEmpty(@this) && (options & StringSplitOptions.RemoveEmptyEntries) == StringSplitOptions.RemoveEmptyEntries => [],
      _ => [@this]
    };

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string[] Split(this string @this, string separator, StringSplitOptions options = StringSplitOptions.None)
    => separator is { Length: not 0 }
      ? @this.Split([separator], options)
      : (options & StringSplitOptions.RemoveEmptyEntries) == StringSplitOptions.RemoveEmptyEntries 
        ? @this.Split((char[])null, options) 
        : [@this]
  ;
}

#endif