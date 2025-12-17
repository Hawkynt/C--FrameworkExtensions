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

#if !SUPPORTS_TIMESPAN_TRYPARSE

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class TimeSpanPolyfills {

  extension(TimeSpan) {

    /// <summary>
    /// Converts the string representation of a time interval to its <see cref="TimeSpan"/> equivalent. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="s">A string that specifies the time interval to convert.</param>
    /// <param name="result">When this method returns, contains the <see cref="TimeSpan"/> equivalent of the time interval contained in <paramref name="s"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string s, out TimeSpan result) {
      result = TimeSpan.Zero;

      if (string.IsNullOrEmpty(s))
        return false;

      try {
        result = TimeSpan.Parse(s);
        return true;
      } catch {
        return false;
      }
    }

    /// <summary>
    /// Converts the string representation of a time interval to its <see cref="TimeSpan"/> equivalent using the specified culture-specific format information. A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="input">A string that specifies the time interval to convert.</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    /// <param name="result">When this method returns, contains the <see cref="TimeSpan"/> equivalent of the time interval contained in <paramref name="input"/>, if the conversion succeeded.</param>
    /// <returns><see langword="true"/> if <paramref name="input"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(string input, IFormatProvider formatProvider, out TimeSpan result) 
      => TryParse(input, out result)
      // In .NET 2.0/3.5, TimeSpan.Parse doesn't have an IFormatProvider overload
      // We just use the basic TryParse
      ;
  }

}

#endif
