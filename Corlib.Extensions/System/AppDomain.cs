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
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
#if SUPPORTS_APPDOMAIN_SETUPINFORMATION_CONFIGURATIONFILE
using System.Text.RegularExpressions;
using System.IO.Pipes;
using System.Security.Principal;
#endif
using System.Threading;
using Guard;
using MethodImplOptions=Utilities.MethodImplOptions;

namespace System;

public static partial class AppDomainExtensions {
  private const int _PROCESS_ALREADY_PRESENT_RESULT_CODE = 0;

  public static DirectoryInfo BasePath { get; private set; } = new(AppDomain.CurrentDomain.BaseDirectory);

#if SUPPORTS_APPDOMAIN_SETUPINFORMATION_CONFIGURATIONFILE

  /// <summary>
  /// Reruns the given <see cref="AppDomain"/> from a temporary directory.
  /// </summary>
  /// <param name="this">The current <see cref="AppDomain"/> instance. Must not be <see langword="null"/>.</param>
  /// <param name="additionalFilemask">(Optional) An array of file masks specifying additional files to copy to the temporary directory.</param>
  /// <param name="blacklistedFilemask">(Optional) An array of file masks specifying files to exclude from being copied to the temporary directory.</param>
  /// <exception cref="NullReferenceException">
  /// Thrown when <paramref name="this"/> is <see langword="null"/>.
  /// </exception>
  /// <remarks>
  /// This method performs the following steps:
  /// - Creates a temporary directory.
  /// - Copies all assemblies (.exe/.dll) and their debugging information files (.pdb), as well as additional files given by <paramref name="additionalFilemask"/> to the temporary location if they not match any <paramref name="blacklistedFilemask"/>.
  /// - Spawns a new process executing from the temporary location.
  /// - Transfers the current environment via a process pipe.
  /// - Ensures that the temporary location is deleted by the child process upon exit via a separate batch file that also deletes itself.
  /// - Terminates the original parent process using <see cref="Environment.Exit"/>, meaning this method may not return for the parent process.
  /// 
  /// **Important:**  
  /// This method should be the first call in the entry point (e.g., inside `Program.Main()`).
  /// </remarks>
  /// <example>
  /// <code>
  /// class Program {
  ///     static void Main() {
  ///         AppDomain.CurrentDomain.RerunInTemporaryDirectory();
  ///     }
  /// }
  /// </code>
  /// </example>
  public static void RerunInTemporaryDirectory(this AppDomain @this, string[] additionalFilemask = null, string[] blacklistedFilemask = null) {
    Against.ThisIsNull(@this);

    var executable = GetExecutable(@this);
    var mutexName = executable.Name;

    var parentProcess = ParentProcessUtilities.GetParentProcess();
    var parentMutexName = $"{mutexName}_{parentProcess.Id}";
    
    // try to connect to parent pipe
    using (NamedPipeClientStream parentMutex = new(".", parentMutexName, PipeDirection.In, PipeOptions.None, TokenImpersonationLevel.Impersonation)) {
      
      var isChildThread = true;
      try {
        parentMutex.Connect(0);
      } catch(TimeoutException) {
        isChildThread = false;
      }

      // if we could connect, we're the newly spawned child process
      if (isChildThread && parentMutex.IsConnected) {
        LoadEnvironmentFrom(parentMutex, @this,out var directoryToDeleteOnExit);
        RegisterSelfDestruct(@this, directoryToDeleteOnExit);
        return;
      }

    }

    // we are the parent process, so acquire a new pipe for ourselves
    var myMutexName = $"{mutexName}_{Process.GetCurrentProcess().Id}";
    using (NamedPipeServerStream myMutex = new(myMutexName, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.WriteThrough)) {
      var directory = new DirectoryInfo(PathExtensions.GetTempDirectoryName(executable.GetFilenameWithoutExtension()));
      var newTarget = CopyExecutableAndAllAssemblies(executable, directory, additionalFilemask,blacklistedFilemask);

      // restart child with original command line if possible to pass all arguments
      var cmd = Environment.CommandLine;
      var index = cmd.StartsWith('\"') 
        ? cmd.IndexOf('\"', 1, StringComparison.Ordinal) 
        : cmd.IndexOf(' ', StringComparison.Ordinal)
        ;

      cmd = index < 0 ? string.Empty : cmd[(index + 1)..];

      ProcessStartInfo startInfo = new(newTarget.FullName, cmd) { UseShellExecute = false };
      Process.Start(startInfo);

      // wait till the child connects
      myMutex.WaitForConnection();
      SaveEnvironmentTo(myMutex, directory, @this);

      Environment.Exit(_PROCESS_ALREADY_PRESENT_RESULT_CODE);
    }

    return;

    static FileInfo CopyExecutableAndAllAssemblies(FileInfo source, DirectoryInfo target, string[] additionalFilemask, string[] blacklistFilemask) {
      var result = CopyAssemblyAndDebugInformation(source, target);
      var sourceDirectory = source.Directory;
      if (sourceDirectory == null)
        return result;

      var includelist = BuildIncludelist(additionalFilemask);
      var blacklist = BuildBlacklist(blacklistFilemask);

      foreach (var file in sourceDirectory.EnumerateFiles("*.*",SearchOption.AllDirectories))
        if (includelist.IsMatch(file.FullName) && !blacklist.IsMatch(file.FullName))
          CopyIfNotExists(file, target.File(file.FullName[(sourceDirectory.FullName.Length + 1)..]));

      return result;

      static Regex BuildIncludelist(string[] strings) {
        var result = "*.exe"
          .ConvertFilePatternToRegex()
          .Or("*.dll".ConvertFilePatternToRegex())
          .Or("*.exe.config".ConvertFilePatternToRegex())
          .Or("*.dll.config".ConvertFilePatternToRegex())
          .Or("*.pdb".ConvertFilePatternToRegex());

        return (
          strings.IsNullOrEmpty()
          ? result
          : strings
            .Aggregate(
              result,
              (current, mask) => current.Or(mask.ConvertFilePatternToRegex())
            )
        ).Compile();
      }

      static Regex BuildBlacklist(string[] strings) 
        => strings.IsNullOrEmpty()
          ? new("^$", RegexOptions.Compiled) 
          : strings.Aggregate(new Regex("^$"), (current, mask) => current.Or(mask.ConvertFilePatternToRegex())).Compile()
        ;

      static void CopyIfNotExists(FileInfo source, FileInfo target) {
        if (target.Exists)
          return;

        target.Directory.TryCreate();
        source.CopyTo(target);
      }

      static FileInfo CopyAssemblyAndDebugInformation(FileInfo source, DirectoryInfo target, bool overwrite = false) {
        if (!target.Exists)
          target.Create();

        var result = target.File(source.Name);
        if (source.Exists)
          source.CopyTo(result.FullName, overwrite);

        var pdbFile = GetDebuggingInformationFile(source);
        if (pdbFile.Exists)
          pdbFile.CopyTo(target.File(pdbFile.Name), true);

        return result;

        static FileInfo GetDebuggingInformationFile(FileInfo assemblyFile) {
          var pdb = Path.ChangeExtension(assemblyFile.Name, "pdb");
          FileInfo result = new(Path.Combine(assemblyFile.Directory?.FullName ?? ".", pdb));
          return result;
        }
      }
    }

    static void RegisterSelfDestruct(AppDomain domain,DirectoryInfo directoryToDeleteOnExit) {
      if (domain.IsDefaultAppDomain())
        domain.ProcessExit += (_, _) => SelfDestruct(directoryToDeleteOnExit);
      else
        domain.DomainUnload += (_, _) => SelfDestruct(directoryToDeleteOnExit);

      domain.UnhandledException += (_, _) => SelfDestruct(directoryToDeleteOnExit);

      // catch unexpected shutdowns for console applications
      if (Environment.UserInteractive)
        Console.CancelKeyPress += (_, e) => {
          switch (e.SpecialKey) {
            case ConsoleSpecialKey.ControlBreak:
            case ConsoleSpecialKey.ControlC:
              SelfDestruct(directoryToDeleteOnExit);
              return;
            default:
              return;
          }
        };

      return;

      static void SelfDestruct(DirectoryInfo myDirectory) {
        var batchFile = WriteBatchToDeleteDirectory(myDirectory);
        if (batchFile == null)
          return;

        Process process = new() { StartInfo = { FileName = batchFile.FullName, WindowStyle = ProcessWindowStyle.Hidden, CreateNoWindow = true, UseShellExecute = true }, };
        process.Start();
        process.PriorityClass = ProcessPriorityClass.BelowNormal;

        return;

        static FileInfo WriteBatchToDeleteDirectory(DirectoryInfo directoryToDelete) {
          var result = directoryToDelete.File($"..\\DeletePid-{Process.GetCurrentProcess().Id}.$$$.bat");
          if (result.Exists)
            return null;

          result.WriteAllText(
            $"""
              @echo off
              :repeat
              echo Trying to delete...
              rd /q /s "{directoryToDelete.FullName}"
              if exist "{directoryToDelete.FullName}" (
                choice /c ° /n /t 3 /d ° >NUL
                goto repeat
              )
              del "%~0"
             """
          );

          return result;
        }
      }
    }

    static void SaveEnvironmentTo(Stream stream, DirectoryInfo temporaryDirectory, AppDomain domain) {
      stream.WriteLengthPrefixedString(domain.BaseDirectory);
      stream.WriteLengthPrefixedString(temporaryDirectory.FullName);
      stream.WriteLengthPrefixedString(domain.SetupInformation.ConfigurationFile);
    }

    static void LoadEnvironmentFrom(Stream stream, AppDomain domain,out DirectoryInfo directoryToDeleteOnExit) {
      BasePath = new(stream.ReadLengthPrefixedString());
      directoryToDeleteOnExit = new(stream.ReadLengthPrefixedString());
      domain.SetupInformation.ConfigurationFile = stream.ReadLengthPrefixedString();
    }

  }
  
#endif

  /// <summary>
  ///   Queries the environment for another process with the same entry assembly and throws an <see cref="Exception" /> when
  ///   present.
  /// </summary>
  /// <param name="this">The AppDomain to store the mutex instance in</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void EnsureSingleInstanceOrThrow(this AppDomain @this)
    => EnsureSingleInstanceOrThrow(@this, _CreateStandardMutexName(@this));

  /// <summary>
  ///   Queries the environment for another process with the same entry assembly and throws an <see cref="Exception" /> when
  ///   present.
  /// </summary>
  /// <param name="this">The AppDomain to store the mutex instance in</param>
  /// <param name="mutexName">The name of the mutex to query</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void EnsureSingleInstanceOrThrow(this AppDomain @this, string mutexName) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrWhiteSpace(mutexName);

    if (!IsSingleInstance(@this, mutexName))
      throw new("AppDomain already loaded");
  }

  /// <summary>
  ///   Queries the environment for another process with the same entry assembly and exits when present.
  /// </summary>
  /// <param name="this">The AppDomain to store the mutex instance in</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void EnsureSingleInstanceOrExit(this AppDomain @this)
    => EnsureSingleInstanceOrExit(@this, _CreateStandardMutexName(@this));

  /// <summary>
  ///   Queries the environment for another process with the same entry assembly and exits when present.
  /// </summary>
  /// <param name="this">The AppDomain to store the mutex instance in</param>
  /// <param name="mutexName">The name of the mutex to query</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void EnsureSingleInstanceOrExit(this AppDomain @this, string mutexName) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrWhiteSpace(mutexName);

    if (!IsSingleInstance(@this, mutexName))
      Environment.Exit(_PROCESS_ALREADY_PRESENT_RESULT_CODE);
  }

  /// <summary>
  ///   Queries the environment for another process with the same entry assembly.
  ///   Note: Creates a mutex which is held until process exit
  /// </summary>
  /// <param name="this">The AppDomain to store the mutex instance in</param>
  /// <returns>
  ///   <c>true</c> if we successfully acquired the mutex, hence we are the only one using it; otherwise,
  ///   <c>false</c>.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsSingleInstance(this AppDomain @this) => IsSingleInstance(@this, _CreateStandardMutexName(@this));

  /// <summary>
  ///   Create a standard mutex name for a given AppDomain. Defaults to entry assemblies' fullname or friendlyname of the
  ///   domain.
  /// </summary>
  /// <param name="appDomain">The <see cref="AppDomain" /> to generaten the name for</param>
  /// <returns>A name to use</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static string _CreateStandardMutexName(AppDomain appDomain) => Assembly.GetEntryAssembly()?.FullName ?? appDomain.FriendlyName;

  /// <summary>
  ///   Queries the environment for a given mutex and acquires it if not present.
  ///   Note: Mutex is held until process exit
  /// </summary>
  /// <param name="this">The AppDomain to store the mutex instance in</param>
  /// <param name="uniqueName">The name of the mutex to query</param>
  /// <returns>
  ///   <c>true</c> if we successfully acquired the mutex, hence we are the only one using it; otherwise,
  ///   <c>false</c>.
  /// </returns>
  public static bool IsSingleInstance(this AppDomain @this, string uniqueName) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrWhiteSpace(uniqueName);

    Mutex mutex = new(true, uniqueName, out var createNew);
    if (createNew)
      @this.DomainUnload += delegate { mutex.ReleaseMutex(); mutex.Dispose(); };
    else
      mutex.Dispose();

    return createNew;
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
    return Invoke(executable, executable.Name);

    static bool Invoke(FileInfo executable, string mutexName) {
      var parentProcess = ParentProcessUtilities.GetParentProcess();

      if (parentProcess != null) {
        var parentMutexName = mutexName + "_" + parentProcess.Id;

        // try to get parent mutex first
        using var mutex = new Mutex(true, parentMutexName, out var createNew);
        if (!createNew) {
          // we couldn't create it, because we're a child process
          mutex.ReleaseMutex();
          return true;
        }
      }

      // we are the parent process, so acquire a new mutex for ourselves
      var myMutexName = mutexName + "_" + Process.GetCurrentProcess().Id;
      using (Mutex myMutex = new(true, myMutexName, out var createNew)) {
        if (!createNew)
          throw new("Mutex already present?");

        // restart child with original command line if possible to pass all arguments
        var cmd = Environment.CommandLine;
        var index = cmd.StartsWith('\"')
            ? cmd.IndexOf('\"', 1, StringComparison.Ordinal)
            : cmd.IndexOf(' ', StringComparison.Ordinal)
          ;

        cmd = index < 0 ? string.Empty : cmd[(index + 1)..];

        // start a child process
        ProcessStartInfo startInfo = new(executable.FullName, cmd);
        var process = Process.Start(startInfo);
        if (process == null)
          throw new("Unable to spawn child-process");

        // wait till the child releases the mutex
        var isChildReady = myMutex.WaitOne(TimeSpan.FromSeconds(30));
        if (!isChildReady)
          throw new("Timed out waiting for the child-process to spawn");

        return false;
      }
    }
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

}
