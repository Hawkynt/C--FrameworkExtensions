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

// Guid.TryWriteBytes was added in .NET Core 2.1
#if !SUPPORTS_GUID_TRYWRITEBYTES

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class GuidPolyfills {

  extension(Guid @this) {

    /// <summary>
    /// Tries to write the current GUID instance into a span of bytes.
    /// </summary>
    /// <param name="destination">When this method returns, contains the GUID as a span of bytes.</param>
    /// <returns><see langword="true"/> if the GUID was successfully written to the span; <see langword="false"/> if the span was too small.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryWriteBytes(Span<byte> destination) {
      if (destination.Length < 16)
        return false;

      @this.ToByteArray().AsSpan().CopyTo(destination);
      return true;
    }

  }

}

#endif
