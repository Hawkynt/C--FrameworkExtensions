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

#if !SUPPORTS_SLIM_SEMAPHORES

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Threading;

public class ManualResetEventSlim(bool initialState) : IDisposable {
  private readonly ManualResetEvent _manualResetEvent = new(initialState);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ManualResetEventSlim() : this(false) { }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Set() => this._manualResetEvent.Set();

  public bool IsSet {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._manualResetEvent.WaitOne(0);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Wait() => this._manualResetEvent.WaitOne();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Wait(TimeSpan timeout) => this._manualResetEvent.WaitOne(timeout);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Wait(int millisecondsTimeout) => this._manualResetEvent.WaitOne(millisecondsTimeout);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Reset() => this._manualResetEvent.Reset();
  
  ~ManualResetEventSlim() => this.Dispose();

  public void Dispose() {
    this._manualResetEvent.Dispose();
    GC.SuppressFinalize(this);
  }
}

#endif
