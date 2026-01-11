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

// Array.Clear(Array) single-parameter overload was added in .NET 6.0
#if !SUPPORTS_ARRAY_CLEAR_SINGLE_PARAM

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class ArrayPolyfills {

  extension(Array) {

    /// <summary>
    /// Clears the contents of an array.
    /// </summary>
    /// <param name="array">The array to clear.</param>
    /// <exception cref="ArgumentNullException"><paramref name="array"/> is <see langword="null"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Clear(Array array) {
      ArgumentNullException.ThrowIfNull(array);
      Array.Clear(array, 0, array.Length);
    }

  }

}

#endif
