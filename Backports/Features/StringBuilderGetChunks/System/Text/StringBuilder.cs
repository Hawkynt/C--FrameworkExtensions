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

#if !SUPPORTS_STRINGBUILDER_GETCHUNKS

using System.Runtime.CompilerServices;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Text;

public static partial class StringBuilderPolyfills {
  /// <param name="this">This <see cref="StringBuilder" /></param>
  extension(StringBuilder @this) {

    /// <summary>
    /// Returns an object that can be used to iterate through the chunks of characters represented in a <see cref="ReadOnlyMemory{T}"/> created from this <see cref="StringBuilder"/> object.
    /// </summary>
    /// <returns>An enumerator for the chunks in the <see cref="StringBuilder"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ChunkEnumerator GetChunks() {
      Against.ThisIsNull(@this);
      return new(@this);
    }

  }
}

/// <summary>
/// Supports iteration over the chunks of a <see cref="StringBuilder"/>.
/// </summary>
public struct ChunkEnumerator {
  private readonly StringBuilder _builder;
  private readonly int _length;
  private int _position;

  internal ChunkEnumerator(StringBuilder builder) {
    this._builder = builder;
    this._length = builder.Length;
    this._position = -1;
  }

  /// <summary>
  /// Gets the current chunk.
  /// </summary>
  public ReadOnlyMemory<char> Current {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get {
      if (this._position < 0 || this._position >= this._length)
        return ReadOnlyMemory<char>.Empty;

      // Since we can't access internal chunks, we return the entire content as a single chunk
      // This is a limitation of the polyfill - the real implementation accesses internal chunk list
      return this._builder.ToString().AsMemory();
    }
  }

  /// <summary>
  /// Advances the enumerator to the next chunk.
  /// </summary>
  /// <returns><see langword="true"/> if there is another chunk; otherwise, <see langword="false"/>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool MoveNext() {
    if (this._position >= 0)
      return false;

    if (this._length == 0)
      return false;

    this._position = 0;
    return true;
  }

  /// <summary>
  /// Gets the enumerator.
  /// </summary>
  /// <returns>This enumerator.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ChunkEnumerator GetEnumerator() => this;
}

#endif
