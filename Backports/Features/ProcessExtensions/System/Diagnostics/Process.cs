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

// Process.WaitForExitAsync and Process.Kill(bool) were added in .NET 5.0
#if !SUPPORTS_PROCESS_WAITFOREXITASYNC

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;
using TaskCreationOptions = Utilities.TaskCreationOptions;

namespace System.Diagnostics;

public static partial class ProcessPolyfills {

  extension(Process @this) {

    /// <summary>
    /// Instructs the Process component to wait asynchronously and indefinitely for the associated process to exit.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the wait operation.</param>
    /// <returns>A task that represents the asynchronous wait operation.</returns>
    /// <exception cref="InvalidOperationException">
    /// No process <see cref="Process.Id"/> has been set, and a <see cref="Process.Handle"/> from which the
    /// <see cref="Process.Id"/> property can be determined does not exist.
    /// -or- There is no process associated with this <see cref="Process"/> object.
    /// -or- You are attempting to call <see cref="WaitForExitAsync(CancellationToken)"/> for a process that
    /// is running on a remote computer. This method is available only for processes that are running on the local computer.
    /// </exception>
    public Task WaitForExitAsync(CancellationToken cancellationToken = default) {
      Against.ThisIsNull(@this);

      // If already exited, return immediately
      if (@this.HasExited)
        return Task.CompletedTask;

      // Check for cancellation
      if (cancellationToken.IsCancellationRequested)
        return Task.FromCanceled(cancellationToken);

      var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

      // Register cancellation callback
      CancellationTokenRegistration registration = default;
      if (cancellationToken.CanBeCanceled)
        registration = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));

      void OnExited(object sender, EventArgs e) {
        registration.Dispose();
        tcs.TrySetResult(true);
      }

      // Enable events and subscribe
      @this.EnableRaisingEvents = true;
      @this.Exited += OnExited;

      // Check again in case it exited between our check and subscribing
      if (@this.HasExited) {
        @this.Exited -= OnExited;
        registration.Dispose();
        tcs.TrySetResult(true);
      }

      return tcs.Task;
    }

    /// <summary>
    /// Immediately stops the associated process, and optionally its child/descendant processes.
    /// </summary>
    /// <param name="entireProcessTree">
    /// <see langword="true"/> to kill the associated process and its descendants;
    /// <see langword="false"/> to kill only the associated process.
    /// </param>
    /// <exception cref="NotSupportedException">You are attempting to call Kill for a process that is running on a remote computer.</exception>
    /// <remarks>
    /// Unlike <see cref="Process.Kill()"/>, this method does not throw if the process has already exited.
    /// This matches the behavior of the native .NET 5+ <c>Kill(bool)</c> method.
    /// </remarks>
    public void Kill(bool entireProcessTree) {
      Against.ThisIsNull(@this);

      if (!entireProcessTree) {
        // Just kill the process itself (matches .NET 5+ behavior - doesn't throw if already exited)
        if (!@this.HasExited)
          @this.Kill();
        return;
      }

      // Kill the entire process tree
      _KillProcessTree(@this.Id);
    }

  }

  private static void _KillProcessTree(int processId) {
    // Get all child processes first (depth-first to kill children before parents)
    var childProcessIds = _GetChildProcessIds(processId);

    // Kill all children first
    foreach (var childId in childProcessIds)
      _KillProcessTree(childId);

    // Now kill the process itself
    try {
      using var process = Process.GetProcessById(processId);
      if (!process.HasExited)
        process.Kill();
    } catch (ArgumentException) {
      // Process already exited
    } catch (InvalidOperationException) {
      // Process already exited
    }
  }

  private static List<int> _GetChildProcessIds(int parentId) {
    var children = new List<int>();

    // RuntimeInformation is available via polyfill, BCL, or official NuGet package
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
      _GetWindowsChildProcesses(parentId, children);
    else
      _GetUnixChildProcesses(parentId, children);

    return children;
  }

  private static void _GetWindowsChildProcesses(int parentId, List<int> children) {
    // Use CreateToolhelp32Snapshot to enumerate processes on Windows
    try {
      const int TH32CS_SNAPPROCESS = 0x00000002;

      var snapshot = _CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
      if (snapshot == IntPtr.Zero || snapshot == new IntPtr(-1))
        return;

      try {
        var entry = new _PROCESSENTRY32 { dwSize = (uint)Marshal.SizeOf(typeof(_PROCESSENTRY32)) };

        if (_Process32First(snapshot, ref entry)) {
          do {
            if (entry.th32ParentProcessID == parentId)
              children.Add((int)entry.th32ProcessID);
          } while (_Process32Next(snapshot, ref entry));
        }
      } finally {
        _CloseHandle(snapshot);
      }
    } catch {
      // If native calls fail, just return empty list
    }
  }

  private static void _GetUnixChildProcesses(int parentId, List<int> children) {
    // On Unix, iterate through /proc/*/stat to find child processes
    try {
      var procDir = new System.IO.DirectoryInfo("/proc");
      if (!procDir.Exists)
        return;

      foreach (var dir in procDir.GetDirectories()) {
        if (!int.TryParse(dir.Name, out var pid))
          continue;

        try {
          var statPath = System.IO.Path.Combine(dir.FullName, "stat");
          if (!System.IO.File.Exists(statPath))
            continue;

          var stat = System.IO.File.ReadAllText(statPath);

          // Format: pid (comm) state ppid ...
          // Find the closing ) to skip the command name which might contain spaces
          var endOfComm = stat.LastIndexOf(')');
          if (endOfComm < 0)
            continue;

          var fields = stat.Substring(endOfComm + 2).Split(' ');
          if (fields.Length >= 2 && int.TryParse(fields[1], out var ppid) && ppid == parentId)
            children.Add(pid);
        } catch {
          // Skip processes we can't read
        }
      }
    } catch {
      // If /proc enumeration fails, return empty list
    }
  }

  #region Windows Native Methods

  [DllImport("kernel32.dll", SetLastError = true)]
  private static extern IntPtr _CreateToolhelp32Snapshot(int dwFlags, uint th32ProcessID);

  [DllImport("kernel32.dll", SetLastError = true)]
  private static extern bool _Process32First(IntPtr hSnapshot, ref _PROCESSENTRY32 lppe);

  [DllImport("kernel32.dll", SetLastError = true)]
  private static extern bool _Process32Next(IntPtr hSnapshot, ref _PROCESSENTRY32 lppe);

  [DllImport("kernel32.dll", SetLastError = true)]
  private static extern bool _CloseHandle(IntPtr hObject);

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
  private struct _PROCESSENTRY32 {
    public uint dwSize;
    public uint cntUsage;
    public uint th32ProcessID;
    public IntPtr th32DefaultHeapID;
    public uint th32ModuleID;
    public uint cntThreads;
    public uint th32ParentProcessID;
    public int pcPriClassBase;
    public uint dwFlags;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
    public string szExeFile;
  }

  #endregion

}

#endif