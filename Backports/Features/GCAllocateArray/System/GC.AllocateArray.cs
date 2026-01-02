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

#if !SUPPORTS_GC_ALLOCATEARRAY

using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System;

/// <summary>
/// Extends <see cref="GC"/> with the <see cref="AllocateArray{T}"/> method.
/// </summary>
public static class GCPolyfills {

  // Stores pinned handles to keep arrays pinned for their lifetime (mimics POH behavior)
  private static readonly List<GCHandle> _pinnedHandles = [];
  private static readonly object _lock = new();

  extension(GC) {

    /// <summary>Allocates an array.</summary>
    /// <typeparam name="T">Specifies the type of the array element.</typeparam>
    /// <param name="length">Specifies the length of the array.</param>
    /// <param name="pinned">Specifies whether the allocated array must be pinned.</param>
    /// <returns>An allocated array.</returns>
    /// <remarks>
    /// <para>
    /// When <paramref name="pinned"/> is <see langword="true"/>, the array is pinned using
    /// a <see cref="GCHandle"/> and remains pinned for the lifetime of the process,
    /// mimicking the behavior of the Pinned Object Heap (POH) on .NET 5.0+.
    /// </para>
    /// <para>
    /// On .NET 5.0+, pinned arrays are allocated on the POH. On older frameworks using
    /// this polyfill, a <see cref="GCHandle"/> keeps the array pinned permanently.
    /// </para>
    /// </remarks>
    public static T[] AllocateArray<T>(int length, bool pinned = false) {
      var array = new T[length];
      if (!pinned)
        return array;

      var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
      lock (_lock)
        _pinnedHandles.Add(handle);

      return array;
    }

    /// <summary>Allocates an array without initializing it.</summary>
    /// <typeparam name="T">Specifies the type of the array element.</typeparam>
    /// <param name="length">Specifies the length of the array.</param>
    /// <param name="pinned">Specifies whether the allocated array must be pinned.</param>
    /// <returns>An allocated array.</returns>
    /// <remarks>
    /// On older frameworks, this behaves identically to <see cref="AllocateArray{T}"/>
    /// since skipping initialization is not supported.
    /// </remarks>
    public static T[] AllocateUninitializedArray<T>(int length, bool pinned = false)
      => AllocateArray<T>(length, pinned);

  }

}

#endif
