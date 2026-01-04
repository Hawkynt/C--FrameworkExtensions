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

// DateTimeOffset.Deconstruct(DateOnly, TimeOnly, TimeSpan) was added in .NET 8.0
#if !SUPPORTS_DATETIMEOFFSET_DECONSTRUCT_WAVE2

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class DateTimeOffsetPolyfills {

  extension(DateTimeOffset @this) {

    /// <summary>
    /// Deconstructs this DateTimeOffset instance into its date, time, and offset components.
    /// </summary>
    /// <param name="date">The date component.</param>
    /// <param name="time">The time component.</param>
    /// <param name="offset">The UTC offset.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out DateOnly date, out TimeOnly time, out TimeSpan offset) {
      var dt = @this.DateTime;
      date = DateOnly.FromDateTime(dt);
      time = TimeOnly.FromDateTime(dt);
      offset = @this.Offset;
    }

  }

}

#endif
