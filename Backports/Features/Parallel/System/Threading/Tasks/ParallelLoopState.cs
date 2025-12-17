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
/// Enables iterations of parallel loops to interact with other iterations.
/// </summary>
public class ParallelLoopState {

  private readonly ParallelLoopStateFlags _flags;

  internal ParallelLoopState(ParallelLoopStateFlags flags) => this._flags = flags;

  /// <summary>
  /// Gets whether the current iteration of the loop should exit based on requests made by this or other iterations.
  /// </summary>
  public bool ShouldExitCurrentIteration => this._flags.IsStopped || this._flags.IsBroken || this._flags.IsExceptional;

  /// <summary>
  /// Gets whether any iteration of the loop has called <see cref="Stop"/>.
  /// </summary>
  public bool IsStopped => this._flags.IsStopped;

  /// <summary>
  /// Gets whether any iteration of the loop has thrown an exception that went unhandled by that iteration.
  /// </summary>
  public bool IsExceptional => this._flags.IsExceptional;

  /// <summary>
  /// Gets the lowest iteration of the loop from which <see cref="Break"/> was called.
  /// </summary>
  public long? LowestBreakIteration => this._flags.LowestBreakIteration;

  /// <summary>
  /// Communicates that the <see cref="Parallel"/> loop should cease execution at the system's earliest convenience.
  /// </summary>
  public void Stop() {
    if (this._flags.IsBroken)
      throw new InvalidOperationException("Break was already called.");

    this._flags.IsStopped = true;
  }

  /// <summary>
  /// Communicates that the <see cref="Parallel"/> loop should cease execution at the system's earliest convenience of iterations beyond the current iteration.
  /// </summary>
  public void Break() {
    if (this._flags.IsStopped)
      throw new InvalidOperationException("Stop was already called.");

    this._flags.IsBroken = true;
  }

}

internal class ParallelLoopStateFlags {
  private volatile bool _isStopped;
  private volatile bool _isBroken;
  private volatile bool _isExceptional;
  private long? _lowestBreakIteration;
  private readonly object _lock = new();

  public bool IsStopped {
    get => this._isStopped;
    set => this._isStopped = value;
  }

  public bool IsBroken {
    get => this._isBroken;
    set => this._isBroken = value;
  }

  public bool IsExceptional {
    get => this._isExceptional;
    set => this._isExceptional = value;
  }

  public long? LowestBreakIteration {
    get {
      lock (this._lock)
        return this._lowestBreakIteration;
    }
  }

  public void SetLowestBreakIteration(long iteration) {
    lock (this._lock)
      if (!this._lowestBreakIteration.HasValue || iteration < this._lowestBreakIteration.Value)
        this._lowestBreakIteration = iteration;
  }
}

#endif
