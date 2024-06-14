#region (c)2010-2042 Hawkynt
/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software: 
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that 
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied 
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.  
    If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

#if SUPPORTS_SLIM_SEMAPHORES

#if DEBUG
using System.Diagnostics;
#endif

namespace System.Threading;

public static partial class SemaphoreSlimExtensions {

#region nested types

  private sealed class SemaphoreAcquired : IDisposable {
    private readonly SemaphoreSlim _semaphore;
    private readonly bool _entered;
    private int _isDisposed;

    public SemaphoreAcquired(SemaphoreSlim semaphore) {
      this._semaphore = semaphore;
      this._entered = this._semaphore.Wait(-1, new());
#if DEBUG
      Trace.WriteLine($"SemaphoreSlim({this._semaphore.GetHashCode():X8}): {this._semaphore.CurrentCount} free");
#endif
    }

    ~SemaphoreAcquired() {
      this.Dispose();
    }

#region IDisposable

    public void Dispose() {
      if (Interlocked.CompareExchange(ref this._isDisposed, 1, 0) != 0)
        return;

      if (this._entered)
        this._semaphore.Release();

#if DEBUG
      Trace.WriteLine($"SemaphoreSlim({this._semaphore.GetHashCode():X8}): {this._semaphore.CurrentCount} free");
#endif
    }

#endregion
  }

#endregion

  /// <summary>
  /// Tries to wait for the semaphore to enter.
  /// </summary>
  /// <param name="this">This <see cref="SemaphoreSlim">SemaphoreSlim</see></param>
  /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
  public static bool TryWait(this SemaphoreSlim @this) => @this.Wait(-1, new());

  /// <summary>
  /// Wait to enter the semaphore.
  /// </summary>
  /// <param name="this">This <see cref="SemaphoreSlim">SemaphoreSlim</see></param>
  /// <returns>An <see cref="IDisposable">IDisposable</see> to be used in using-blocks</returns>
  public static IDisposable Enter(this SemaphoreSlim @this) => new SemaphoreAcquired(@this);

}

#endif