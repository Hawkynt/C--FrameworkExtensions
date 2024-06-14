#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY

#endregion

#if !SUPPORTS_STREAM_COPY

#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif

namespace System.IO;

public static partial class StreamPolyfills {
  /// <summary>
  ///   Copies all contents from this <see cref="Stream" /> to another <see cref="Stream" />.
  /// </summary>
  /// <param name="this">This <see cref="Stream" />.</param>
  /// <param name="target">Target <see cref="Stream" />.</param>
  public static void CopyTo(this Stream @this, Stream target) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));
    if (target == null)
      throw new ArgumentNullException(nameof(target));
    if (!@this.CanRead)
      throw new ArgumentException("Can not read", nameof(@this));
    if (!target.CanWrite)
      throw new ArgumentException("Can not write", nameof(target));

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
