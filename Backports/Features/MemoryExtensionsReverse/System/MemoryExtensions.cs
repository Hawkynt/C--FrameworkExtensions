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

#if !SUPPORTS_MEMORYEXTENSIONS_REVERSE

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class MemoryExtensionsPolyfills {

  extension(MemoryExtensions) {

    /// <summary>
    /// Reverses the sequence of the elements in the entire span.
    /// </summary>
    /// <typeparam name="T">The type of elements in the span.</typeparam>
    /// <param name="span">The span to reverse.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Reverse<T>(Span<T> span) {
      if (span.Length <= 1)
        return;

      var left = 0;
      var right = span.Length - 1;
      while (left < right) {
        (span[left], span[right]) = (span[right], span[left]);
        ++left;
        --right;
      }
    }

  }

}

#endif
