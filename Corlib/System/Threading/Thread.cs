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

using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace System.Threading {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class ThreadExtensions {
    /// <summary>
    /// Allows pushing the current thread into the low-IO mode introduced with Windows Vista.
    /// </summary>
    private class IoBackgroundModeToken : IDisposable {

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

        public static void EnterBackgroundMode() {
          SetThreadPriority(GetCurrentThread(), (int)ThreadBackgroundMode.Begin);
        }

        public static void LeaveBackgroundMode() {
          SetThreadPriority(GetCurrentThread(), (int)ThreadBackgroundMode.End);
        }
      }

      private bool _isDisposed;

      public IoBackgroundModeToken() {
        Thread.BeginThreadAffinity();
        NativeMethods.EnterBackgroundMode();
      }

      #region Implementation of IDisposable

      ~IoBackgroundModeToken() {
        this.Dispose();
      }

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
    /// Pushes the CURRENT thread into the low-IO mode introduced with Windows Vista.
    /// </summary>
    /// <param name="This">This Thread.</param>
    /// <returns>A disposable object that automatically releases from the low-io mode upon destruction.</returns>
    public static IDisposable IoBackgroundMode(this Thread This) {
      Contract.Requires(Thread.CurrentThread == This);
      return (new IoBackgroundModeToken());
    }
  }
}