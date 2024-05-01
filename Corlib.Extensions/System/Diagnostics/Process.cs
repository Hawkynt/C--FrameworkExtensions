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

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif
using Guard;
using System.Linq;

namespace System.Diagnostics;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
public static partial class ProcessExtensions {

  private static class NativeMethods {

    /// <summary>     
    /// A utility class to determine a this parent.     
    /// </summary>     
    [StructLayout(LayoutKind.Sequential)]
    public struct ProcessBasicInformation {
      // These members must match PROCESS_BASIC_INFORMATION         
      public IntPtr Reserved1;
      public IntPtr PebBaseAddress;
      public IntPtr Reserved2_0;
      public IntPtr Reserved2_1;
      public IntPtr UniqueProcessId;
      public IntPtr InheritedFromUniqueProcessId;
    }

    [DllImport("ntdll.dll", EntryPoint = "NtQueryInformationProcess", SetLastError = true)]
    private static extern int _NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref ProcessBasicInformation processInformation, int processInformationLength, out int returnLength);

    public static ProcessBasicInformation NtQueryInformationProcess(IntPtr processHandle, int processInformationClass = 0) {
      ProcessBasicInformation result = new();
      var status = _NtQueryInformationProcess(processHandle, processInformationClass, ref result, Marshal.SizeOf(result), out _);
      if (status != 0)
        throw new Win32Exception();

      return result;
    }

  }

  /// <summary>
  /// Gets the parent <see cref="Process"/> of the given <see cref="Process"/>.
  /// </summary>
  /// <param name="this">This <see cref="Process"/>.</param>
  /// <returns>The parent <see cref="Process"/> or <see langword="null"/>.</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static Process Parent(this Process @this) {
    Against.ThisIsNull(@this);
    return GetParentProcessOrNull(@this);
  }

  /// <summary>
  /// Recursively gets all parent processes of the given <see cref="Process"/>.
  /// </summary>
  /// <param name="this">This <see cref="Process"/>.</param>
  /// <returns>An enumeration of <see cref="Process"/>es</returns>
  public static IEnumerable<Process> Parents(this Process @this) {
    Against.ThisIsNull(@this);

    return Invoke(@this);

    static IEnumerable<Process> Invoke(Process @this) {
      var currentChild = @this;
      for (;;) {
        currentChild = GetParentProcessOrNull(currentChild);
        if (currentChild == null)
          yield break;

        yield return currentChild;
      }
    }
  }

  /// <summary>
  /// Get the direct child processes of the given <see cref="Process"/>.
  /// </summary>
  /// <param name="this">This <see cref="Process"/></param>
  /// <returns>An enumeration of <see cref="Process"/>es</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static IEnumerable<Process> Children(this Process @this) {
    Against.ThisIsNull(@this);

    var id = @this.Id;
    return Process.GetProcesses().Where(p => p.GetParentProcessOrNull()?.Id == id);
  }

  /// <summary>
  /// Get all child processes and their children ..and... of the given <see cref="Process"/>.
  /// </summary>
  /// <param name="this">This <see cref="Process"/></param>
  /// <returns>An enumeration of <see cref="Process"/>es</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static IEnumerable<Process> AllChildren(this Process @this) {
    Against.ThisIsNull(@this);
    
    return Invoke(@this);
    
    static IEnumerable<Process> Invoke(Process @this) {
      var allProcessIdsWithChildren = Process
          .GetProcesses()
          .Select(p => new { Process = p, Parent = GetParentProcessOrNull(p) })
          .Where(p => p.Parent.IsNotNull())
          .GroupBy(p => p.Parent.Id)
          .ToDictionary(g => g.Key, g => g.Select(p => p.Process).ToArray())
        ;

      Stack<Process> stack = new();
      List<Process> tempList = new();
      var result = @this;
      for (;;) {
        FillList(result, tempList, allProcessIdsWithChildren);
        PushReverse(tempList, stack);

        if (!stack.Any())
          break;

        result = stack.Pop();
        yield return result;
      }
    }

    static void FillList(Process process, List<Process> list, IDictionary<int, Process[]> allProcessIdsWithChildren) {
      list.Clear();
      if(allProcessIdsWithChildren.TryGetValue(process.Id, out var children))
        list.AddRange(children);
    }

    static void PushReverse(IList<Process> list, Stack<Process> stack) {
      for (var i = list.Count - 1; i >= 0; --i)
        stack.Push(list[i]);
    }
  }

  /// <summary>        
  /// Gets the parent <see cref="Process"/> of the current one.
  /// </summary>        
  /// <returns>An instance of the <see cref="Process"/>-class.</returns>    
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static Process GetParentProcess() => GetParentProcess(Process.GetCurrentProcess());

  /// <summary>       
  /// Gets the parent <see cref="Process"/> of the given process id or throws.
  /// </summary>    
  /// <param name="processId">The process id.</param>   
  /// <returns>An instance of the <see cref="Process"/> class.</returns>   
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static Process GetParentProcess(int processId) => GetParentProcess(Process.GetProcessById(processId));

  /// <summary>       
  /// Gets the parent <see cref="Process"/> of the given process id or throws.
  /// </summary>    
  /// <param name="processId">The process id.</param>   
  /// <returns>An instance of the <see cref="Process"/> class or <see langword="null"/>.</returns>   
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static Process GetParentProcessOrNull(int processId) {
    Process process;
    try {
      process = Process.GetProcessById(processId);
    } catch (ArgumentException /* process not found */ ) {
      return null;
    }

    return GetParentProcessOrNull(process);
  }

  /// <summary>
  /// Gets the parent process of the given process or throws.
  /// </summary>
  /// <param name="this">This <see cref="Process"/></param>
  /// <returns>Another <see cref="Process"/>-Instance</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static Process GetParentProcess(this Process @this) {
    Against.ThisIsNull(@this);
    return GetParentProcess(@this.Handle);
  }    
  
  /// <summary>
  /// Gets the parent process of the given process if any.
  /// </summary>
  /// <param name="this">This <see cref="Process"/></param>
  /// <returns>Another <see cref="Process"/>-Instance or <see langword="null"/></returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static Process GetParentProcessOrNull(this Process @this) {
    Against.ThisIsNull(@this);
    try {
      return GetParentProcessOrNull(@this.Handle);
    } catch (Exception) {
      return null;
    }
  }

  /// <summary>   
  /// Gets the parent Process of a specified process handle.
  /// </summary>        
  /// <param name="handle">The process handle.</param>   
  /// <returns>An instance of the <see cref="Process"/> class.</returns>  
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static Process GetParentProcess(IntPtr handle) {
    var pbi = NativeMethods.NtQueryInformationProcess(handle);
    return Process.GetProcessById(pbi.InheritedFromUniqueProcessId.ToInt32());
  }

  /// <summary>   
  /// Gets the parent Process of a specified process handle or <see langword="null"/>.
  /// </summary>        
  /// <param name="handle">The this handle.</param>   
  /// <returns>An instance of the Process class or <see langword="null"/>.</returns>
  public static Process GetParentProcessOrNull(IntPtr handle) {
    try {
      return GetParentProcess(handle);
    } catch (Win32Exception /* can not find process */) {
      return null;
    } catch (InvalidOperationException /* process exited */) {
      return null;
    } catch (ArgumentException /* process no longer alive */) {
      return null;
    }
  }

}
