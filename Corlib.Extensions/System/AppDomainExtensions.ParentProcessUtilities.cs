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
using System.Runtime.InteropServices;

namespace System;

public static partial class AppDomainExtensions {
  /// <summary>
  ///   A utility class to determine a process parent.
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  private struct ParentProcessUtilities {
    // These members must match PROCESS_BASIC_INFORMATION
    internal IntPtr Reserved1;
    internal IntPtr PebBaseAddress;
    internal IntPtr Reserved2_0;
    internal IntPtr Reserved2_1;
    internal IntPtr UniqueProcessId;
    internal IntPtr InheritedFromUniqueProcessId;
    
    [DllImport("ntdll.dll")]
    private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref ParentProcessUtilities processInformation, int processInformationLength, out int returnLength);

    /// <summary>
    ///   Gets the parent process of the current process.
    /// </summary>
    /// <returns>An instance of the Process class.</returns>
    public static Process GetParentProcess() => GetParentProcess(Process.GetCurrentProcess().Handle);

    /// <summary>
    ///   Gets the parent process of specified process.
    /// </summary>
    /// <param name="id">The process id.</param>
    /// <returns>An instance of the Process class.</returns>
    public static Process GetParentProcess(int id) => GetParentProcess(Process.GetProcessById(id).Handle);

    /// <summary>
    ///   Gets the parent process of a specified process.
    /// </summary>
    /// <param name="handle">The process handle.</param>
    /// <returns>An instance of the Process class or null if an error occurred.</returns>
    public static Process GetParentProcess(IntPtr handle) {
      ParentProcessUtilities pbi = new();
      var status = NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf(pbi), out _);
      if (status != 0)
        return null;

      try {
        return Process.GetProcessById(pbi.InheritedFromUniqueProcessId.ToInt32());
      } catch (ArgumentException) {
        // not found
        return null;
      }
    }
  }
}
