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

#if !SUPPORTS_STRINGBUILDER_APPEND_CHARPOINTER

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Text;

public static partial class StringBuilderPolyfills {

  extension(StringBuilder @this) {

    /// <summary>
    /// Appends an array of Unicode characters starting at a specified address to this instance.
    /// </summary>
    /// <param name="value">A pointer to an array of characters.</param>
    /// <param name="valueCount">The number of characters in the array.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe StringBuilder Append(char* value, int valueCount) {
      if (value == null || valueCount == 0)
        return @this;

      return @this.Append(new string(value, 0, valueCount));
    }

  }

}

#endif
