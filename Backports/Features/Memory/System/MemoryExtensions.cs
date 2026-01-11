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

#if !SUPPORTS_MEMORY && !OFFICIAL_MEMORY

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

// This class name is required by the compiler for certain wellknown members.
public static partial class MemoryExtensions {

  extension<T>(T[] array) {

    /// <summary>
    /// Creates a new <see cref="Memory{T}"/> over the entirety of the target array.
    /// </summary>
    /// <returns>The memory representation of the array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Memory<T> AsMemory() => array == null ? default : new(array);

    /// <summary>
    /// Creates a new <see cref="Memory{T}"/> over the portion of the target array beginning at 'start' index.
    /// </summary>
    /// <param name="start">The index at which to begin the memory.</param>
    /// <returns>The memory representation of the array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Memory<T> AsMemory(int start) => array == null ? default : new(array, start, array.Length - start);

    /// <summary>
    /// Creates a new <see cref="Memory{T}"/> over the portion of the target array beginning at 'start' index and of given length.
    /// </summary>
    /// <param name="start">The index at which to begin the memory.</param>
    /// <param name="length">The number of items in the memory.</param>
    /// <returns>The memory representation of the array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Memory<T> AsMemory(int start, int length) => array == null ? default : new(array, start, length);

    /// <summary>
    /// Creates a new <see cref="Memory{T}"/> over the portion of the target array beginning at the specified index.
    /// </summary>
    /// <param name="startIndex">The index at which to begin the memory.</param>
    /// <returns>The memory representation of the array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Memory<T> AsMemory(Index startIndex) => array == null ? default : array.AsMemory(startIndex.GetOffset(array.Length));

    /// <summary>
    /// Creates a new <see cref="Memory{T}"/> over the portion of the target array corresponding to the specified range.
    /// </summary>
    /// <param name="range">The range of the array to convert.</param>
    /// <returns>The memory representation of the array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Memory<T> AsMemory(Range range) {
      if (array == null)
        return default;
      var offsetAndLength = range.GetOffsetAndLength(array.Length);
      return new(array, offsetAndLength.Offset, offsetAndLength.Length);
    }
  }

  extension(string text) {

    /// <summary>
    /// Creates a new <see cref="ReadOnlyMemory{T}"/> over the entirety of the target string.
    /// </summary>
    /// <returns>The read-only memory representation of the string.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyMemory<char> AsMemory() => text == null ? default : new(text, 0, text.Length);

    /// <summary>
    /// Creates a new <see cref="ReadOnlyMemory{T}"/> over the portion of the target string beginning at 'start' index.
    /// </summary>
    /// <param name="start">The index at which to begin the memory.</param>
    /// <returns>The read-only memory representation of the string.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyMemory<char> AsMemory(int start) => text == null ? default : new(text, start, text.Length - start);

    /// <summary>
    /// Creates a new <see cref="ReadOnlyMemory{T}"/> over the portion of the target string beginning at 'start' index and of given length.
    /// </summary>
    /// <param name="start">The index at which to begin the memory.</param>
    /// <param name="length">The number of items in the memory.</param>
    /// <returns>The read-only memory representation of the string.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyMemory<char> AsMemory(int start, int length) => text == null ? default : new(text, start, length);

    /// <summary>
    /// Creates a new <see cref="ReadOnlyMemory{T}"/> over the portion of the target string beginning at the specified index.
    /// </summary>
    /// <param name="startIndex">The index at which to begin the memory.</param>
    /// <returns>The read-only memory representation of the string.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyMemory<char> AsMemory(Index startIndex) => text == null ? default : text.AsMemory(startIndex.GetOffset(text.Length));

    /// <summary>
    /// Creates a new <see cref="ReadOnlyMemory{T}"/> over the portion of the target string corresponding to the specified range.
    /// </summary>
    /// <param name="range">The range of the string to convert.</param>
    /// <returns>The read-only memory representation of the string.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyMemory<char> AsMemory(Range range) {
      if (text == null)
        return default;
      var offsetAndLength = range.GetOffsetAndLength(text.Length);
      return new(text, offsetAndLength.Offset, offsetAndLength.Length);
    }
  }
}

#endif
