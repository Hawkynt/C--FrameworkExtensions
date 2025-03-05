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

#if !SUPPORTS_SPAN

namespace System;

internal static partial class SpanHelper {
  public class StringHandler(string source, int start) : IMemoryHandler<char> {
    #region Implementation of IMemoryHandler<char>

    /// <inheritdoc />
    public ref char this[int index] => ref new[] { source[start + index] }[0];

    /// <inheritdoc />
    public IMemoryHandler<char> SliceFrom(int offset) => new StringHandler(source, start + offset);

    /// <inheritdoc />
    public void CopyTo(IMemoryHandler<char> other, int length) {
      for (int i = 0, offset = start; i < length; ++offset, ++i)
        other[i] = source[offset];
    }

    /// <inheritdoc />
    public void CopyTo(char[] target, int count) => source.CopyTo(start, target, 0, count);

    #endregion

    #region Overrides of Object

    /// <inheritdoc />
    public override string ToString() => start == 0 ? source : source[start..];

    #endregion

    public string ToString(int length) => start == 0 && length == source.Length ? source : source.Substring(start, length);

  }
}

#endif