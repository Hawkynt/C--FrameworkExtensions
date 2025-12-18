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

#if !SUPPORTS_STRINGBUILDER_APPEND_SPAN

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Text;

public static partial class StringBuilderPolyfills {

  extension(StringBuilder @this) {

    /// <summary>
    /// Appends the string representation of a specified read-only character span to this instance.
    /// </summary>
    /// <param name="value">The read-only character span to append.</param>
    /// <returns>A reference to this instance after the append operation is completed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public StringBuilder Append(ReadOnlySpan<char> value) {
#if SUPPORTS_SPAN_STRING_CTOR
      return @this.Append(new string(value));
#else
      unsafe {
        fixed (char* ptr = value)
          return @this.Append(new string(ptr, 0, value.Length));
      }
#endif
    }

    /// <summary>
    /// Inserts the string representation of a specified read-only character span into this instance at the specified character position.
    /// </summary>
    /// <param name="index">The position in this instance where insertion begins.</param>
    /// <param name="value">The read-only character span to insert.</param>
    /// <returns>A reference to this instance after the insert operation is completed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public StringBuilder Insert(int index, ReadOnlySpan<char> value) {
#if SUPPORTS_SPAN_STRING_CTOR
      return @this.Insert(index, new string(value));
#else
      unsafe {
        fixed (char* ptr = value)
          return @this.Insert(index, new string(ptr, 0, value.Length));
      }
#endif
    }

  }

}

#endif
