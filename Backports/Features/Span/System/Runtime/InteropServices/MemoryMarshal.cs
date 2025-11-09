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

#if !SUPPORTS_SPAN && !OFFICIAL_SPAN

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.InteropServices;

public static class MemoryMarshal {

  /// <summary>Returns a reference to the element of the span at index 0.</summary>
  /// <param name="span">The span from which the reference is retrieved.</param>
  /// <typeparam name="T">The type of items in the span.</typeparam>
  /// <returns>A reference to the element at index 0.</returns>
  public static ref T GetReference<T>(Span<T> span) => ref span.memoryHandler.GetRef(0);

  /// <summary>Returns a reference to the element of the read-only span at index 0.</summary>
  /// <param name="span">The read-only span from which the reference is retrieved.</param>
  /// <typeparam name="T">The type of items in the span.</typeparam>
  /// <returns>A reference to the element at index 0.</returns>
  public static ref T GetReference<T>(ReadOnlySpan<T> span) => ref span.memoryHandler.GetRef(0);

  /// <summary>Casts a <see cref="T:System.Span`1" /> of one primitive type, <paramref name="T" />, to a <see langword="Span&lt;Byte&gt;" />.</summary>
  /// <param name="span">The source slice to convert.</param>
  /// <typeparam name="T">The type of items in the span.</typeparam>
  /// <exception cref="T:System.ArgumentException">
  /// <paramref name="T" /> contains managed object references.</exception>
  /// <exception cref="T:System.OverflowException">The <see cref="P:System.ReadOnlySpan`1.Length" /> property of the new <see cref="T:System.ReadOnlySpan`1" /> would exceed <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see></exception>
  /// <returns>A span of type <see cref="T:System.Byte" />.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Span<byte> AsBytes<T>(Span<T> span) where T : struct 
    => new(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), checked(span.Length * Unsafe.SizeOf<T>()))
  ;

  /// <summary>Casts a <see cref="T:System.ReadOnlySpan`1" /> of one primitive type, <paramref name="T" />, to a <see langword="ReadOnlySpan&lt;Byte&gt;" />.</summary>
  /// <param name="span">The source slice to convert.</param>
  /// <typeparam name="T">The type of items in the read-only span.</typeparam>
  /// <exception cref="T:System.ArgumentException">
  /// <paramref name="T" /> contains managed object references.</exception>
  /// <exception cref="T:System.OverflowException">The <see cref="P:System.ReadOnlySpan`1.Length" /> property of the new <see cref="T:System.ReadOnlySpan`1" /> would exceed <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see></exception>
  /// <returns>A read-only span of type <see cref="T:System.Byte" />.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ReadOnlySpan<byte> AsBytes<T>(ReadOnlySpan<T> span) where T : struct
    => new(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), checked(span.Length * Unsafe.SizeOf<T>()))
  ;

  /// <summary>Casts a span of one primitive type to a span of another primitive type.</summary>
  /// <param name="span">The source slice to convert.</param>
  /// <typeparam name="TFrom">The type of the source span.</typeparam>
  /// <typeparam name="TTo">The type of the target span.</typeparam>
  /// <exception cref="T:System.ArgumentException">
  /// <paramref name="TFrom" /> or <paramref name="TTo" /> contains managed object references.</exception>
  /// <exception cref="T:System.OverflowException">The <see cref="P:System.ReadOnlySpan`1.Length" /> property of the new <see cref="T:System.ReadOnlySpan`1" /> would exceed <see cref="F:System.Int32.MaxValue" />.</exception>
  /// <returns>The converted span.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Span<TTo> Cast<TFrom, TTo>(Span<TFrom> span)
    where TFrom : struct
    where TTo : struct {
    var num1 = (uint)Unsafe.SizeOf<TFrom>();
    var num2 = (uint)Unsafe.SizeOf<TTo>();
    var length1 = (uint)span.Length;
    var length2 = (int)num1 != (int)num2 ? (num1 != 1U ? checked((int)unchecked(length1 * (ulong)num1 / num2)) : (int)(length1 / num2)) : (int)length1;
    return new(ref Unsafe.As<TFrom, TTo>(ref MemoryMarshal.GetReference(span)), length2);
  }

  /// <summary>Casts a read-only span of one primitive type to a read-only span of another primitive type.</summary>
  /// <param name="span">The source slice to convert.</param>
  /// <typeparam name="TFrom">The type of the source span.</typeparam>
  /// <typeparam name="TTo">The type of the target span.</typeparam>
  /// <exception cref="T:System.ArgumentException">
  /// <paramref name="TFrom" /> or <paramref name="TTo" /> contains managed object references.</exception>
  /// <exception cref="T:System.OverflowException">The <see cref="P:System.ReadOnlySpan`1.Length" /> property of the new <see cref="T:System.ReadOnlySpan`1" /> would exceed <see cref="F:System.Int32.MaxValue" />.</exception>
  /// <returns>The converted read-only span.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ReadOnlySpan<TTo> Cast<TFrom, TTo>(ReadOnlySpan<TFrom> span)
    where TFrom : struct
    where TTo : struct {
    var num1 = (uint)Unsafe.SizeOf<TFrom>();
    var num2 = (uint)Unsafe.SizeOf<TTo>();
    var length1 = (uint)span.Length;
    var length2 = (int)num1 != (int)num2 ? (num1 != 1U ? checked((int)unchecked(length1 * (ulong)num1 / num2)) : (int)(length1 / num2)) : (int)length1;
    return new(ref Unsafe.As<TFrom, TTo>(ref MemoryMarshal.GetReference<TFrom>(span)), length2);
  }
}

#endif