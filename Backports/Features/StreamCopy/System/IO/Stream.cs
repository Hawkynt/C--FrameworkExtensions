﻿#region (c)2010-2042 Hawkynt

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

#if !SUPPORTS_STREAM_COPY

#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif

using Guard;

namespace System.IO;

public static partial class StreamPolyfills {
  /// <summary>
  ///   Copies all contents from this <see cref="Stream" /> to another <see cref="Stream" />.
  /// </summary>
  /// <param name="this">This <see cref="Stream" />.</param>
  /// <param name="target">Target <see cref="Stream" />.</param>
  public static void CopyTo(this Stream @this, Stream target) {
    if (@this == null)
      AlwaysThrow.ArgumentNullException(nameof(@this));
    if (target == null)
      AlwaysThrow.ArgumentNullException(nameof(target));
    if (!@this.CanRead)
      AlwaysThrow.ArgumentException("Can not read", nameof(@this));
    if (!target.CanWrite)
      AlwaysThrow.ArgumentException("Can not write", nameof(target));

    var buffer = new byte[65536];
    int count;
    while ((count = @this.Read(buffer, 0, buffer.Length)) != 0)
      target.Write(buffer, 0, count);
  }

  /// <summary>
  ///   Flushes the <see cref="Stream" />.
  /// </summary>
  /// <param name="this">This <see cref="Stream" />.</param>
  /// <param name="_">Dummy, ignored</param>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Flush(this Stream @this, bool _) => @this.Flush();
}

#endif
