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

using System.Diagnostics;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

#if !SUPPORTS_ENVIRONMENT_TICKCOUNT64

public static partial class EnvironmentPolyfills {
  extension(Environment) {
    /// <summary>
    /// Gets the number of milliseconds elapsed since the system started.
    /// </summary>
    /// <value>A 64-bit signed integer containing the amount of time in milliseconds that has passed since the last time the computer was started.</value>
    /// <remarks>
    /// Unlike <see cref="Environment.TickCount"/>, this property does not wrap around after approximately 24.9 days.
    /// </remarks>
    public static long TickCount64 {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => Stopwatch.GetTimestamp() * 1000L / Stopwatch.Frequency;
    }
  }
}

#endif

#if !SUPPORTS_ENVIRONMENT_PROCESSID

public static partial class EnvironmentPolyfills {
  extension(Environment) {
    /// <summary>
    /// Gets the unique identifier for the current process.
    /// </summary>
    /// <value>A number that represents the unique identifier for the current process.</value>
    public static int ProcessId {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        using var process = Process.GetCurrentProcess();
        return process.Id;
      }
    }
  }
}

#endif

#if !SUPPORTS_ENVIRONMENT_PROCESSPATH

public static partial class EnvironmentPolyfills {
  extension(Environment) {
    /// <summary>
    /// Gets the path of the executable that started the currently executing process.
    /// </summary>
    /// <value>The path of the executable that started the currently executing process, or <see langword="null"/> if the path is not available.</value>
    public static string? ProcessPath {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get {
        using var process = Process.GetCurrentProcess();
        return process.MainModule?.FileName;
      }
    }
  }
}

#endif
