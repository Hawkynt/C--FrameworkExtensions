#if !SUPPORTS_SLIM_SEMAPHORES

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

namespace System.Threading;
#if COMPILE_TO_EXTENSION_DLL
public
#else
internal
#endif
class ManualResetEventSlim : IDisposable {
  private readonly ManualResetEvent _manualResetEvent;

  public ManualResetEventSlim() : this(false) { }
  public ManualResetEventSlim(bool initialState) => this._manualResetEvent = new(initialState);
  public void Set() => this._manualResetEvent.Set();
  public bool IsSet => this._manualResetEvent.WaitOne(0);
  public void Wait() => this._manualResetEvent.WaitOne();
  public bool Wait(TimeSpan timeout) => this._manualResetEvent.WaitOne(timeout);
  public void Reset() => this._manualResetEvent.Reset();
  ~ManualResetEventSlim() => this.Dispose();
  public void Dispose() => GC.SuppressFinalize(this);
}

#endif
