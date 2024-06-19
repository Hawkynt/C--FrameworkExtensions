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

#if !SUPPORTS_SPAN

namespace System;

public static partial class MemoryExtensions {
  
  public static Span<T> AsSpan<T>(this T[] @this) => new(@this);
  public static Span<T> AsSpan<T>(this T[] @this, int start) => new(@this, start, @this.Length - start);
  public static Span<T> AsSpan<T>(this T[] @this, int start, int length) => new(@this, start, length);
  public static Span<T> AsSpan<T>(this T[] @this, Index startIndex) => AsSpan(@this, startIndex.GetOffset(@this.Length));

  public static Span<T> AsSpan<T>(this T[] @this, Range range) {
    var offsetAndLength = range.GetOffsetAndLength(@this.Length);
    return new(@this, offsetAndLength.Offset, offsetAndLength.Length);
  }

  public static ReadOnlySpan<char> AsSpan(this string @this) => new(@this);
  public static ReadOnlySpan<char> AsSpan(this string @this, int start) => new(@this, start, @this.Length - start);
  public static ReadOnlySpan<char> AsSpan(this string @this, int start, int length) => new(@this, start, length);
  public static ReadOnlySpan<char> AsSpan(this string @this, Index startIndex) => AsSpan(@this, startIndex.GetOffset(@this.Length));

  public static ReadOnlySpan<char> AsSpan(this string @this, Range range) {
    var offsetAndLength = range.GetOffsetAndLength(@this.Length);
    return new(@this.ToCharArray(), offsetAndLength.Offset, offsetAndLength.Length);
  }

}
#endif