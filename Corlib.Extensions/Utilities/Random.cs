﻿// This file is part of Hawkynt's .NET Framework extensions.
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

namespace Utilities;
internal static class Random {

#if SUPPORTS_RANDOM_SHARED
  public static readonly System.Random Shared = System.Random.Shared;
#else
  public static readonly System.Random Shared = new ((int) System.Diagnostics.Stopwatch.GetTimestamp());
#endif
  
}
