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

#if !SUPPORTS_ARRAY_EMPTY

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class ArrayExtensions {
  extension(Array) {
    /// <summary>
    /// Returns an empty array.
    /// </summary>
    /// <typeparam name="T">The type of the elements of the array.</typeparam>
    /// <returns>An empty array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] Empty<T>() => EmptyArray<T>.Value;
  }

  private static class EmptyArray<T> {
    internal static readonly T[] Value = [];
  }
}

#endif
