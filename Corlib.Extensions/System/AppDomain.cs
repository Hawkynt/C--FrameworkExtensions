﻿#region (c)2010-2042 Hawkynt

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

#if !NETSTANDARD && !NETCOREAPP
#define _SUPPORTS_APP_CONFIGURATION_PATH
#endif

using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Principal;
using System.Threading;
using Guard;

namespace System;


#if SUPPORTS_INLINING

#endif

public static partial class AppDomainExtensions {
  private const int _PROCESS_ALREADY_PRESENT_RESULT_CODE = 0;

  #region nested types

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

  #endregion

#if _SUPPORTS_APP_CONFIGURATION_PATH

  /// <summary>
  ///   Saves the parent process' environment to disk.
  /// </summary>
  /// <param name="stream">The stream.</param>
  /// <param name="targetDirectory">The target directory.</param>
  /// <param name="domain">The domain.</param>
  private static void _SaveEnvironmentTo(Stream stream, DirectoryInfo targetDirectory, AppDomain domain) {
    Dictionary<string, string> environment = new() {
      { "baseDirectory", domain.BaseDirectory },
      { "configurationFile", domain.SetupInformation.ConfigurationFile },
      { "deleteOnExit", targetDirectory.FullName }
    };

    BinaryFormatter formatter = new();
    formatter.Serialize(stream, environment);
  }

  /// <summary>
  ///   Loads a saved environment from the parent process.
  /// </summary>
  /// <param name="stream">The stream.</param>
  /// <param name="domain">The domain.</param>
  private static void _LoadEnvironmentFrom(Stream stream, AppDomain domain) {
    BinaryFormatter formatter = new();
    var environment = formatter.Deserialize(stream) as Dictionary<string, string> ?? new Dictionary<string, string>();

    // var baseDirectory = environment["baseDirectory"];
    var configurationFile = environment["configurationFile"];
    DirectoryInfo directoryToDeleteOnExit = new(environment["deleteOnExit"]);

    domain.SetupInformation.ConfigurationFile = configurationFile;

    // make sure we're removed from disk after exit
    if (domain.IsDefaultAppDomain())
      domain.ProcessExit += (_, _) => _SelfDestruct(directoryToDeleteOnExit);
    else
      domain.DomainUnload += (_, _) => _SelfDestruct(directoryToDeleteOnExit);

    domain.UnhandledException += (_, _) => _SelfDestruct(directoryToDeleteOnExit);

    // catch unexpected shutdowns for console applications
    if (Environment.UserInteractive)
      Console.CancelKeyPress += (_, e) => {
        switch (e.SpecialKey) {
          case ConsoleSpecialKey.ControlBreak:
            _SelfDestruct(directoryToDeleteOnExit);
            return;
          case ConsoleSpecialKey.ControlC:
            _SelfDestruct(directoryToDeleteOnExit);
            return;
          default: return;
        }
      };
  }

#endif

  /// <summary>
  ///   Removes the given directory by spawning a child-process, thus allowing to remove ourselves.
  /// </summary>
  /// <param name="myDirectory">My directory.</param>
  private static void _SelfDestruct(DirectoryInfo myDirectory) {
    var batchFile = _WriteBatchToDeleteDirectory(myDirectory);
    if (batchFile == null)
      return;

    Process process = new(){
      StartInfo = {
        FileName = batchFile.FullName,
        WindowStyle = ProcessWindowStyle.Hidden,
        CreateNoWindow = true,
        UseShellExecute = true
      },
    };
    process.Start();
    process.PriorityClass = ProcessPriorityClass.BelowNormal;
  }

  /// <summary>
  ///   Writes a batch file deleting a given directory and itself afterwards.
  ///   Note: Batch file is written to the directories parent directory.
  /// </summary>
  /// <param name="directoryToDelete">The directory to delete.</param>
  /// <returns>The generated batch file</returns>
  private static FileInfo _WriteBatchToDeleteDirectory(DirectoryInfo directoryToDelete) {
    FileInfo result = new(Path.Combine(directoryToDelete.Parent?.FullName ?? ".", $"DeletePid-{Process.GetCurrentProcess().Id}.$$$.bat"));
    if (result.Exists)
      return null;

    File.WriteAllText(
      result.FullName,
      $"""
       @echo off
       :repeat
       echo Trying to delete...
       rd /q /s "{directoryToDelete.FullName}"
       if exist "{directoryToDelete.FullName}" (
         ping 127.0.0.1 -n 3 >NUL
         goto repeat
       )
       del "%~0"
       """
    );
    return result;
  }

#if _SUPPORTS_APP_CONFIGURATION_PATH

  /// <summary>
  ///   Reruns the given app domain from a temporary directory.
  ///   Note: This should always be the first method to call upon entry point (ie. in Program.Main() method)
  ///   * this first creates a temporary directory
  ///   * copies all assemblies (.exe/.dll) and their debugging information files (.pdb) to the temp directory
  ///   * saves the current environment to a file
  ///   * spawns the new process at the temporary location
  ///   * restores the saved environment
  ///   * makes sure that the temporary location is deleted by the child-process upon exit
  /// </summary>
  /// <param name="this">This AppDomain.</param>
  /// <exception cref="System.Exception">Semaphore already present?</exception>
  public static void RerunInTemporaryDirectory(this AppDomain @this) {
    Against.ThisIsNull(@this);

    var executable = GetExecutable(@this);
    var semaphoreName = executable.Name;

    var parentProcess = ParentProcessUtilities.GetParentProcess();
    var parentSemaphoreName = semaphoreName + "_" + parentProcess.Id;

    // try to connect to parent pipe
    using (NamedPipeClientStream parentSemaphore = new(".", parentSemaphoreName, PipeDirection.In, PipeOptions.None, TokenImpersonationLevel.Impersonation)) {
      try {
        parentSemaphore.Connect(0);
      } catch {
        // ignored
      }

      // if we could connect, we're the newly spawned child process
      if (parentSemaphore.IsConnected) {
        _LoadEnvironmentFrom(parentSemaphore, @this);
        return;
      }
    }

    // we are the parent process, so acquire a new pipe for ourselves
    var mySemaphoreName = semaphoreName + "_" + Process.GetCurrentProcess().Id;
    using (NamedPipeServerStream mySemaphore = new(mySemaphoreName, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.WriteThrough)) {
      var directory = _CreateTempDirectory();
      var newTarget = _CopyExecutableAndAllAssemblies(executable, directory);

      // restart child with original command line if possible to pass all arguments
      var cmd = Environment.CommandLine;
      if (cmd.StartsWith("\"")) {
        var index = cmd.IndexOf("\"", 1, StringComparison.Ordinal);
        cmd = index < 0 ? string.Empty : cmd[(index + 1)..];
      } else {
        var index = cmd.IndexOf(" ", StringComparison.Ordinal);
        cmd = index < 0 ? string.Empty : cmd[(index + 1)..];
      }

      // start a child process
      ProcessStartInfo startInfo = new(newTarget.FullName, cmd) { UseShellExecute = false };
      Process.Start(startInfo);

      // wait till the child release the semaphore
      mySemaphore.WaitForConnection();
      _SaveEnvironmentTo(mySemaphore, directory, @this);

      Environment.Exit(_PROCESS_ALREADY_PRESENT_RESULT_CODE);
    }
  }

#endif

  /// <summary>
  ///   Queries the environment for another process with the same entry assembly and throws an <see cref="Exception" /> when
  ///   present.
  /// </summary>
  /// <param name="this">The AppDomain to store the semaphore instance in</param>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void EnsureSingleInstanceOrThrow(this AppDomain @this)
    => EnsureSingleInstanceOrThrow(@this, _CreateStandardSemaphoreName(@this));

  /// <summary>
  ///   Queries the environment for another process with the same entry assembly and throws an <see cref="Exception" /> when
  ///   present.
  /// </summary>
  /// <param name="this">The AppDomain to store the semaphore instance in</param>
  /// <param name="semaphoreName">The name of the semaphore to query</param>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void EnsureSingleInstanceOrThrow(this AppDomain @this, string semaphoreName) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrWhiteSpace(semaphoreName);

    if (!IsSingleInstance(@this, semaphoreName))
      throw new("AppDomain already loaded");
  }

  /// <summary>
  ///   Queries the environment for another process with the same entry assembly and exits when present.
  /// </summary>
  /// <param name="this">The AppDomain to store the semaphore instance in</param>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void EnsureSingleInstanceOrExit(this AppDomain @this)
    => EnsureSingleInstanceOrExit(@this, _CreateStandardSemaphoreName(@this));

  /// <summary>
  ///   Queries the environment for another process with the same entry assembly and exits when present.
  /// </summary>
  /// <param name="this">The AppDomain to store the semaphore instance in</param>
  /// <param name="semaphoreName">The name of the semaphore to query</param>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void EnsureSingleInstanceOrExit(this AppDomain @this, string semaphoreName) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrWhiteSpace(semaphoreName);

    if (!IsSingleInstance(@this, semaphoreName))
      Environment.Exit(_PROCESS_ALREADY_PRESENT_RESULT_CODE);
  }

  /// <summary>
  ///   Queries the environment for another process with the same entry assembly.
  ///   Note: Creates a semaphore which is held until process exit
  /// </summary>
  /// <param name="this">The AppDomain to store the semaphore instance in</param>
  /// <returns>
  ///   <c>true</c> if we successfully acquired the semaphore, hence we are the only one using it; otherwise,
  ///   <c>false</c>.
  /// </returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool IsSingleInstance(this AppDomain @this) => IsSingleInstance(@this, _CreateStandardSemaphoreName(@this));

  /// <summary>
  ///   Create a standard semaphore name for a given AppDomain. Defaults to entry assemblies' fullname or friendlyname of the
  ///   domain.
  /// </summary>
  /// <param name="appDomain">The <see cref="AppDomain" /> to generaten the name for</param>
  /// <returns>A name to use</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private static string _CreateStandardSemaphoreName(AppDomain appDomain) => Assembly.GetEntryAssembly()?.FullName ?? appDomain.FriendlyName;

  /// <summary>
  ///   Queries the environment for a given semaphore and acquires it if not present.
  ///   Note: Semaphore is held until process exit
  /// </summary>
  /// <param name="this">The AppDomain to store the semaphore instance in</param>
  /// <param name="semaphoreName">The name of the semaphore to query</param>
  /// <returns>
  ///   <c>true</c> if we successfully acquired the semaphore, hence we are the only one using it; otherwise,
  ///   <c>false</c>.
  /// </returns>
  public static bool IsSingleInstance(this AppDomain @this, string semaphoreName) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrWhiteSpace(semaphoreName);

    Semaphore semaphore = new(0, 1, semaphoreName, out var createNew);
    if (createNew)
      @this.DomainUnload += delegate { semaphore.Dispose(); };
    else
      semaphore.Dispose();

    return createNew;
  }

  /// <summary>
  ///   Creates a new process from the given executable using the same command line.
  /// </summary>
  /// <param name="executable">The executable.</param>
  /// <param name="semaphoreName">Name of the semaphore to share.</param>
  /// <returns>
  ///   <c>true</c> if the current process is the created child-process; otherwise, <c>false</c> for the parent process.
  /// </returns>
  /// <exception cref="Exception">Semaphore already present?</exception>
  private static bool _Fork(FileInfo executable, string semaphoreName) {
    var parentProcess = ParentProcessUtilities.GetParentProcess();
    var parentSemaphoreName = semaphoreName + "_" + parentProcess.Id;
    bool createNew;

    // try to get parent semaphore first
    using (Semaphore parentSemaphore = new(0, 1, parentSemaphoreName, out createNew))
      if (!createNew) {
        // we couldn't create it, because we're a child process
        parentSemaphore.Release();
        return true;
      }

    // we are the parent process, so acquire a new semaphore for ourselves
    var mySemaphoreName = semaphoreName + "_" + Process.GetCurrentProcess().Id;
    using (Semaphore mySemaphore = new(0, 1, mySemaphoreName, out createNew)) {
      if (!createNew)
        throw new("Semaphore already present?");

      // restart child with original command line if possible to pass all arguments
      var cmd = Environment.CommandLine;
      if (cmd.StartsWith("\"")) {
        var index = cmd.IndexOf("\"", 1, StringComparison.Ordinal);
        cmd = index < 0 ? string.Empty : cmd[(index + 1)..];
      } else {
        var index = cmd.IndexOf(" ", StringComparison.Ordinal);
        cmd = index < 0 ? string.Empty : cmd[(index + 1)..];
      }

      // start a child process
      ProcessStartInfo startInfo = new(executable.FullName, cmd);
      Process.Start(startInfo);

      // wait till the child release the semaphore
      mySemaphore.WaitOne();
      return false;
    }
  }

  /// <summary>
  ///   Creates a new process from the given appdomain using the same executable and command line.
  ///   Note: This should always be the first method to call upon entry point (ie. in Program.Main() method)
  /// </summary>
  /// <param name="this">This AppDomain.</param>
  /// <returns>
  ///   <c>true</c> if the current process is the created child-process; otherwise, <c>false</c> for the parent
  ///   process.
  /// </returns>
  public static bool Fork(this AppDomain @this) {
    Against.ThisIsNull(@this);

    var executable = GetExecutable(@this);
    return _Fork(executable, executable.Name);
  }

  /// <summary>
  ///   Creates a temporary directory.
  /// </summary>
  /// <returns>The temporary directory.</returns>
  [DebuggerStepThrough]
  private static DirectoryInfo _CreateTempDirectory() {
    FileInfo file = new(Path.GetTempFileName());
    var directory = file.Directory;
    DirectoryInfo result = new(Path.Combine(directory?.FullName ?? ".", file.Name));
    file.Delete();
    result.Create();
    return result;
  }

  /// <summary>
  ///   Copies an assembly file (.exe/.dll) to a target directory and keeps PDB debugging files.
  /// </summary>
  /// <param name="source">The source.</param>
  /// <param name="target">The target.</param>
  /// <param name="overwrite">if set to <c>true</c> overwrites files already at the target location.</param>
  /// <returns>The new assembly file</returns>
  [DebuggerStepThrough]
  private static FileInfo _CopyAssemblyAndDebugInformation(FileInfo source, DirectoryInfo target, bool overwrite = false) {
    if (!target.Exists)
      target.Create();

    FileInfo result = new(Path.Combine(target.FullName, source.Name));
    if (source.Exists)
      source.CopyTo(result.FullName, overwrite);

    var pdbFile = _GetDebuggingInformationFile(source);
    if (pdbFile.Exists)
      pdbFile.CopyTo(Path.Combine(target.FullName, pdbFile.Name), true);

    return result;
  }

  /// <summary>
  ///   Copies the given executable file and all other executables in the same directory to the given location, retaining
  ///   original directory structure.
  /// </summary>
  /// <param name="source">The source executable.</param>
  /// <param name="target">The target directory.</param>
  /// <returns>The new executable file.</returns>
  [DebuggerStepThrough]
  private static FileInfo _CopyExecutableAndAllAssemblies(FileInfo source, DirectoryInfo target) {
    var result = _CopyAssemblyAndDebugInformation(source, target);
    var sourceDirectory = source.Directory;
    foreach (var dll in sourceDirectory
      .EnumerateFiles("*.dll", SearchOption.AllDirectories)
      .Concat(sourceDirectory.EnumerateFiles("*.exe", SearchOption.AllDirectories))
    )
      _CopyAssemblyAndDebugInformation(dll, new FileInfo(Path.Combine(target.FullName, dll.FullName[(sourceDirectory.FullName.Length + 1)..])).Directory, true);

    return result;
  }

  /// <summary>
  ///   Gets the executable for the given app domain.
  /// </summary>
  /// <param name="this">This AppDomain.</param>
  /// <returns></returns>
  [DebuggerStepThrough]
  public static FileInfo GetExecutable(this AppDomain @this) {
    Against.ThisIsNull(@this);

    var fileName = @this.FriendlyName;
    const string vsHostPostfix = ".vshost.exe";
    if (fileName.EndsWith(vsHostPostfix))
      fileName = fileName[..^vsHostPostfix.Length] + ".exe";

    return new(Path.Combine(@this.BaseDirectory, fileName));
  }

  /// <summary>
  ///   Gets the debugging information file for the given assembly file.
  /// </summary>
  /// <param name="assemblyFile">The assembly file.</param>
  /// <returns></returns>
  [DebuggerStepThrough]
  private static FileInfo _GetDebuggingInformationFile(FileInfo assemblyFile) {
    var pdb = Path.ChangeExtension(assemblyFile.Name, "pdb");
    FileInfo result = new(Path.Combine(assemblyFile.Directory?.FullName ?? ".", pdb));
    return result;
  }
}
