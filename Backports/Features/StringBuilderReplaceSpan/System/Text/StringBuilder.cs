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

// StringBuilder.Replace(ReadOnlySpan) was added in .NET 6.0
#if !SUPPORTS_STRINGBUILDER_REPLACE_SPAN

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Text;

public static partial class StringBuilderPolyfills {

  extension(StringBuilder @this) {

    /// <summary>
    /// Replaces all occurrences of a specified character span with another character span within this instance.
    /// </summary>
    /// <param name="oldValue">The character span to replace.</param>
    /// <param name="newValue">The character span to replace <paramref name="oldValue"/> with.</param>
    /// <returns>A reference to this instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public StringBuilder Replace(ReadOnlySpan<char> oldValue, ReadOnlySpan<char> newValue) {
      if (oldValue.IsEmpty)
        return @this;

      var oldString = oldValue.ToString();
      var newString = newValue.ToString();
      return @this.Replace(oldString, newString);
    }

    /// <summary>
    /// Replaces all occurrences of a specified character span with another character span within a substring of this instance.
    /// </summary>
    /// <param name="oldValue">The character span to replace.</param>
    /// <param name="newValue">The character span to replace <paramref name="oldValue"/> with.</param>
    /// <param name="startIndex">The position in this instance where the substring begins.</param>
    /// <param name="count">The length of the substring.</param>
    /// <returns>A reference to this instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public StringBuilder Replace(ReadOnlySpan<char> oldValue, ReadOnlySpan<char> newValue, int startIndex, int count) {
      if (oldValue.IsEmpty)
        return @this;

      var oldString = oldValue.ToString();
      var newString = newValue.ToString();
      return @this.Replace(oldString, newString, startIndex, count);
    }

  }

}

#endif
