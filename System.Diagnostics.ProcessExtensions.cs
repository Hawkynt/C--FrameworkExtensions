#region (c)2010-2020 Hawkynt
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

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace System.Diagnostics {
  internal static partial class ProcessExtensions {
    /// <summary>     
    /// A utility class to determine a process parent.     
    /// </summary>     
    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessBasicInformation {
      // These members must match PROCESS_BASIC_INFORMATION         
      internal IntPtr Reserved1;
      internal IntPtr PebBaseAddress;
      internal IntPtr Reserved2_0;
      internal IntPtr Reserved2_1;
      internal IntPtr UniqueProcessId;
      internal IntPtr InheritedFromUniqueProcessId;
    }

    [DllImport("ntdll.dll", EntryPoint = "NtQueryInformationProcess")]
    private static extern int _NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref ProcessBasicInformation processInformation, int processInformationLength, out int returnLength);

    /// <summary>
    /// Gets the parent process of the given process.
    /// </summary>
    /// <param name="This">This Process.</param>
    /// <returns>The parent process or <c>null</c>.</returns>
    public static Process Parent(this Process This) {
      Contract.Requires(This != null);
      try {
        return (GetParentProcess(This.Handle));
      } catch {
        return (null);
      }
    }

    /// <summary>
    /// Recursively gets all parent processes of the given process.
    /// </summary>
    /// <param name="This">This Process.</param>
    /// <returns></returns>
    public static IEnumerable<Process> Parents(this Process This) {
      var currentChild = This;
      var dontSkip = false;
      while (currentChild != null) {
        if (dontSkip)
          yield return currentChild;
        else
          dontSkip = true;
        currentChild = currentChild.Parent();
      }
    }
    /// <summary>        
    /// Gets the parent process of the current process.      
    /// </summary>        
    /// <returns>An instance of the Process class.</returns>    
    public static Process GetParentProcess() { return GetParentProcess(Process.GetCurrentProcess().Handle); }

    /// <summary>       
    /// Gets the parent process of specified process.  
    /// </summary>    
    /// <param name="id">The process id.</param>   
    /// <returns>An instance of the Process class.</returns>   
    public static Process GetParentProcess(int id) { Process process = Process.GetProcessById(id); return GetParentProcess(process.Handle); }

    /// <summary>   
    /// Gets the parent process of a specified process.   
    /// </summary>        
    /// <param name="handle">The process handle.</param>   
    /// <returns>An instance of the Process class.</returns>  
    public static Process GetParentProcess(IntPtr handle) {
      var pbi = new ProcessBasicInformation();
      int returnLength;
      var status = _NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf(pbi), out returnLength);

      if (status != 0)
        throw new Win32Exception(status);

      try {
        return (Process.GetProcessById(pbi.InheritedFromUniqueProcessId.ToInt32()));
      } catch (ArgumentException) {

        // not found               
        return (null);
      }
    }

  }
}