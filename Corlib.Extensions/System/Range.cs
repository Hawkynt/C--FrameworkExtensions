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
//

using System.Collections.Generic;
using System.Linq;

namespace System;
public static partial class RangeExtensions {

  public static bool IsInRange(this byte @this, IEnumerable<Range> ranges) => ranges.Any(range => @this >= range.Start.Value && @this <= range.End.Value);
  public static bool IsInRange(this short @this, IEnumerable<Range> ranges) => ranges.Any(range => @this >= range.Start.Value && @this <= range.End.Value);
  public static bool IsInRange(this ushort @this, IEnumerable<Range> ranges) => ranges.Any(range => @this >= range.Start.Value && @this <= range.End.Value);
  public static bool IsInRange(this int @this, IEnumerable<Range> ranges) => ranges.Any(range => @this >= range.Start.Value && @this <= range.End.Value);
  
}
