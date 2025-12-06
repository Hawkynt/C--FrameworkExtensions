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

#if !SUPPORTS_RANDOM_SHARED

using System.Runtime.CompilerServices;
using System.Threading;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

// Keep as polyfill class until compiler bug is fixed
public static partial class RandomPolyfills {
  [ThreadStatic]
  private static Random _threadLocalRandom;

  extension(Random) {

    /// <summary>
    /// Provides a thread-safe <see cref="Random"/> instance that may be used concurrently from any thread.
    /// </summary>
    public static Random Shared {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _threadLocalRandom ??= new(Environment.TickCount ^ Thread.CurrentThread.ManagedThreadId);
    }

  }
}

#endif
