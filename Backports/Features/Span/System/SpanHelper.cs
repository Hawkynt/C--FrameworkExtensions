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

#if !SUPPORTS_SPAN

namespace System;

internal static partial class SpanHelper {

  public static bool IsChar<T>() => typeof(T) == typeof(char);

  public static bool IsValueType<T>() => 
    typeof(T) == typeof(byte) ||
    typeof(T) == typeof(sbyte) ||
    typeof(T) == typeof(short) ||
    typeof(T) == typeof(ushort) ||
    typeof(T) == typeof(int) ||
    typeof(T) == typeof(uint) ||
    typeof(T) == typeof(long) ||
    typeof(T) == typeof(ulong) ||
    typeof(T) == typeof(float) ||
    typeof(T) == typeof(double) ||
    typeof(T) == typeof(char) ||
    typeof(T) == typeof(bool) ||
    typeof(T) == typeof(decimal) ||
    typeof(T).IsValueType
  ;

}

#endif
