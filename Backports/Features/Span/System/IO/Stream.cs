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

#if !SUPPORTS_SPAN

namespace System.IO;

public static partial class StreamPolyfills {
  public static int Read(this Stream @this, Span<byte> buffer) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));

    var size = buffer.Length;
    var doubleBuffer = new byte[size];
    var result = @this.Read(doubleBuffer, 0, size);
    doubleBuffer.AsSpan()[..result].CopyTo(buffer[..result]);
    return result;
  }
}

#endif
