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

using System.Runtime.InteropServices;
using Guard;

namespace System.Threading;

public static partial class ThreadExtensions {
  /// <summary>
  ///   Allows pushing the current thread into the low-IO mode introduced with Windows Vista.
  /// </summary>
  private sealed class IoBackgroundModeToken : IDisposable {
    private static class NativeMethods {
      private enum ThreadBackgroundMode {
        Begin = 0x00010000,
        End = 0x00020000,
      }

      [DllImport("Kernel32.dll", EntryPoint = "GetCurrentThread")]
      private static extern IntPtr GetCurrentThread();

      [DllImport("Kernel32.dll", EntryPoint = "SetThreadPriority")]
      [return: MarshalAs(UnmanagedType.Bool)]
      private static extern bool SetThreadPriority(IntPtr hThread, int nPriority);

      public static void EnterBackgroundMode() => SetThreadPriority(GetCurrentThread(), (int)ThreadBackgroundMode.Begin);
      public static void LeaveBackgroundMode() => SetThreadPriority(GetCurrentThread(), (int)ThreadBackgroundMode.End);
    }

    private bool _isDisposed;

    public IoBackgroundModeToken() {
      Thread.BeginThreadAffinity();
      NativeMethods.EnterBackgroundMode();
    }

    #region Implementation of IDisposable

    ~IoBackgroundModeToken() => this.Dispose();

    public void Dispose() {
      if (this._isDisposed)
        return;

      this._isDisposed = true;
      GC.SuppressFinalize(this);
      NativeMethods.LeaveBackgroundMode();
      Thread.EndThreadAffinity();
    }

    #endregion
  }

  /// <summary>
  ///   Pushes the CURRENT thread into the low-IO mode introduced with Windows Vista.
  /// </summary>
  /// <param name="this">This Thread.</param>
  /// <returns>A disposable object that automatically releases from the low-io mode upon destruction.</returns>
  public static IDisposable IoBackgroundMode(this Thread @this) {
    Against.DifferentInstances(@this, Thread.CurrentThread);

    return new IoBackgroundModeToken();
  }
}
