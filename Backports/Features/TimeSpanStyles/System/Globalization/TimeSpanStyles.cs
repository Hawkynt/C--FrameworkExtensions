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

#if !SUPPORTS_TIMESPANSTYLES

namespace System.Globalization;

/// <summary>
/// Defines the formatting options that customize string parsing for some methods that parse a time interval.
/// </summary>
[Flags]
public enum TimeSpanStyles {
  /// <summary>
  /// Indicates that input is interpreted as a negative time interval only if a negative sign is present.
  /// </summary>
  None = 0,
  /// <summary>
  /// Indicates that input is always interpreted as a negative time interval.
  /// </summary>
  AssumeNegative = 1
}

#endif
