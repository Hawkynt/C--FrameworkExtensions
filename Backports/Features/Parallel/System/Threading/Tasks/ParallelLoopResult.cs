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

#if !SUPPORTS_PARALLEL

namespace System.Threading.Tasks;

/// <summary>
/// Provides completion status on the execution of a <see cref="Parallel"/> loop.
/// </summary>
public struct ParallelLoopResult {

  /// <summary>
  /// Gets whether the loop ran to completion, such that all iterations of the loop were executed.
  /// </summary>
  public bool IsCompleted { get; internal set; }

  /// <summary>
  /// Gets the index of the lowest iteration from which <see cref="ParallelLoopState.Break"/> was called.
  /// </summary>
  public long? LowestBreakIteration { get; internal set; }

}

#endif
