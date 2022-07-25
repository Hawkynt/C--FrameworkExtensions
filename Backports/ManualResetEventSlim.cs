#if !NET40_OR_GREATER && !NET5_0_OR_GREATER && !NETCOREAPP && !NETSTANDARD

namespace System.Threading {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  class ManualResetEventSlim {

    private readonly ManualResetEvent _manualResetEvent;

    public ManualResetEventSlim():this(false){}
    public ManualResetEventSlim(bool initialState) => this._manualResetEvent = new ManualResetEvent(initialState);
    public void Set() => this._manualResetEvent.Set();
    public bool IsSet => this._manualResetEvent.WaitOne(0);
    public void Wait() => this._manualResetEvent.WaitOne();
    public bool Wait(TimeSpan timeout) => this._manualResetEvent.WaitOne(timeout);
    public void Reset() => this._manualResetEvent.Reset();

  }
}

#endif