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

#if !SUPPORTS_STRING_CREATE

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class StringPolyfills {
  extension(string) {
  /// <summary>
  /// Creates a new string with a specific length and initializes it after creation by using the specified callback.
  /// </summary>
  /// <typeparam name="TState">The type of the element to pass to <paramref name="action"/>.</typeparam>
  /// <param name="length">The length of the string to create.</param>
  /// <param name="state">The element to pass to <paramref name="action"/>.</param>
  /// <param name="action">A callback to initialize the string.</param>
  /// <returns>The created string.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
  public static string Create<TState>(int length, TState state, SpanAction<char, TState> action) {
    if (action == null)
      AlwaysThrow.ArgumentNullException(nameof(action));
    if (length < 0)
      AlwaysThrow.ArgumentOutOfRangeException(nameof(length), "Non-negative number required.");
    if (length == 0)
      return string.Empty;

    var result = new string('\0', length);
    var handle = GCHandle.Alloc(result, GCHandleType.Pinned);
    try {
      unsafe {
        var ptr = (char*)handle.AddrOfPinnedObject();
        action(new Span<char>(ptr, length), state);
      }
    } finally {
      handle.Free();
    }

    return result;
  }

  /// <summary>
  /// Creates a new string from a <see cref="ReadOnlySpan{T}"/> of characters.
  /// </summary>
  /// <param name="value">A read-only span of characters.</param>
  /// <returns>A string that contains the characters from the span.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string Create(ReadOnlySpan<char> value) {
    if (value.IsEmpty)
      return string.Empty;

    unsafe {
      fixed (char* ptr = value)
        return new string(ptr, 0, value.Length);
    }
  }
  }
}

/// <summary>
/// Encapsulates a method that receives a span of objects of type <typeparamref name="T"/> and a state object of type <typeparamref name="TArg"/>.
/// </summary>
/// <typeparam name="T">The type of the elements in the span.</typeparam>
/// <typeparam name="TArg">The type of the object that represents the state.</typeparam>
/// <param name="span">A <see cref="Span{T}"/> object.</param>
/// <param name="arg">A state object of type <typeparamref name="TArg"/>.</param>
public delegate void SpanAction<T, in TArg>(Span<T> span, TArg arg);

/// <summary>
/// Encapsulates a method that receives a read-only span of objects of type <typeparamref name="T"/> and a state object of type <typeparamref name="TArg"/>.
/// </summary>
/// <typeparam name="T">The type of the elements in the span.</typeparam>
/// <typeparam name="TArg">The type of the object that represents the state.</typeparam>
/// <param name="span">A <see cref="ReadOnlySpan{T}"/> object.</param>
/// <param name="arg">A state object of type <typeparamref name="TArg"/>.</param>
public delegate void ReadOnlySpanAction<T, in TArg>(ReadOnlySpan<T> span, TArg arg);

#endif
