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

namespace Utilities;
internal static class Random {

#if SUPPORTS_RANDOM_SHARED
  public static readonly System.Random Shared = System.Random.Shared;
#else
  public static readonly System.Random Shared = new (_CreateSeed());

  private static int _CreateSeed() {
    var ticks = (ulong)System.Diagnostics.Stopwatch.GetTimestamp();
    const ulong _GOLDEN_GAMMA = 0x9E3779B97F4A7C15;
    ticks += _GOLDEN_GAMMA;
    ticks = (ticks ^ (ticks >> 30)) * 0xBF58476D1CE4E5B9;
    ticks = (ticks ^ (ticks >> 27)) * 0x94D049BB133111EB;
    ticks ^= ticks >> 32;
    return (int)ticks;
  }

#endif
  
}
