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

// DateTimeOffset.UnixEpoch was added in .NET Core 2.1
#if !SUPPORTS_DATETIMEOFFSET_UNIXEPOCH

namespace System;

public static partial class DateTimeOffsetPolyfills {

  extension(DateTimeOffset) {

    /// <summary>
    /// Represents the Unix epoch: January 1, 1970, at 00:00:00 UTC.
    /// </summary>
    public static DateTimeOffset UnixEpoch => new(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

  }
}

#endif
