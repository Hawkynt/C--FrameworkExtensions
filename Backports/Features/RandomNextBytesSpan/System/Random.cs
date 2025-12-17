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

#if !SUPPORTS_RANDOM_NEXTBYTES_SPAN

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class RandomPolyfills {

  extension(Random @this) {

    /// <summary>
    /// Fills the elements of a specified span of bytes with random numbers.
    /// </summary>
    /// <param name="buffer">The span to be filled with random numbers.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void NextBytes(Span<byte> buffer) {
      Against.ThisIsNull(@this);

      // Write ints directly to avoid allocating a temporary array
      var intSpan = MemoryMarshal.Cast<byte, int>(buffer);
      for (var i = 0; i < intSpan.Length; ++i)
        intSpan[i] = @this.Next();

      // Handle remaining bytes (0-3 bytes)
      var remaining = buffer.Length & 3;
      if (remaining <= 0)
        return;

      var lastInt = @this.Next();

      // TODO: unroll that for performance?
      var offset = buffer.Length - remaining;
      for (var i = 0; i < remaining; ++i)
        buffer[offset + i] = (byte)(lastInt >> (i << 3));

    }

  }

}

#endif
