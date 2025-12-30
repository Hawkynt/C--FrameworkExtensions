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

#if !SUPPORTS_STOPWATCH_TOSTRING

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Diagnostics;

/// <summary>
/// Polyfills for <see cref="Stopwatch"/> ToString method added in .NET 8.0.
/// </summary>
public static partial class StopwatchPolyfills {

  /// <param name="this">The <see cref="Stopwatch"/> instance.</param>
  extension(Stopwatch @this) {
    /// <summary>
    /// Returns the <see cref="Stopwatch.Elapsed"/> time as a string.
    /// </summary>
    /// <returns>A string representation of the elapsed time.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToString() => @this.Elapsed.ToString();
  }
}

#endif
