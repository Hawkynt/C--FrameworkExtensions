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

// Provides LastIndexOf with StringComparison for ReadOnlySpan<char>
// This method is only natively available in .NET Core 3.0+ and .NET 5.0+
// System.Memory package provides IndexOf with StringComparison but NOT LastIndexOf
// Note: IndexOf is already provided by System.Memory, so we only add LastIndexOf here

#if !NETCOREAPP3_0_OR_GREATER

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class MemoryPolyfills {
  /// <param name="span">The source span.</param>
  extension(ReadOnlySpan<char> span)
  {
    /// <summary>
    /// Reports the zero-based index of the last occurrence of the specified <paramref name="value"/> in the current <paramref name="span"/>.
    /// </summary>
    /// <param name="value">The value to seek within the source span.</param>
    /// <param name="comparisonType">One of the enumeration values that determines how the <paramref name="span"/> and <paramref name="value"/> are compared.</param>
    /// <returns>The index of the last occurrence of the value in the span. If not found, returns -1.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int LastIndexOf(ReadOnlySpan<char> value, StringComparison comparisonType)
      => span.ToString().LastIndexOf(value.ToString(), comparisonType);
  }

}

#endif
