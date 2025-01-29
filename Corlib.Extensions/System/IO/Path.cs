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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Guard;

namespace System.IO;

public static partial class PathExtensions {
  #region nested types

  private sealed class TemporaryTokenCleaner {
    #region entries

    [DebuggerDisplay("{" + nameof(_DebuggerDisplay) + "}")]
    private sealed class Entry {
      private long _timeToKill;
      private Entry(FileSystemInfo target) => this.Target = target.FullName;
      public Entry(FileInfo target) : this((FileSystemInfo)target) => this.IsFile = true;
      public Entry(DirectoryInfo target) : this((FileSystemInfo)target) => this.IsFile = false;

      public string Target { get; }
      public bool IsFile { get; }

      public TimeSpan MinimumLifeTimeLeft {
        get => TimeSpan.FromSeconds(this.TicksLeft / (double)Stopwatch.Frequency);
        set => this._timeToKill = (long)(Stopwatch.GetTimestamp() + value.TotalSeconds * Stopwatch.Frequency);
      }

      public long TicksLeft => this._timeToKill == 0 ? 0L : Math.Max(0, this._timeToKill - Stopwatch.GetTimestamp());
      public bool IsAlive { get; set; }

      private string _DebuggerDisplay => $"{(this.IsFile ? "File" : "Directory")} {this.Target} ({(this.IsAlive ? "alive" : "dead")}, {this.MinimumLifeTimeLeft:g} time left)";
    }

    private readonly ConcurrentDictionary<string, Entry> _entries = new(StringComparer.OrdinalIgnoreCase);

    #endregion

    #region Singleton

    private static readonly Lazy<TemporaryTokenCleaner> _instance;
    public static TemporaryTokenCleaner Instance => _instance.Value;
    static TemporaryTokenCleaner() => _instance = new(_Factory);
    private static TemporaryTokenCleaner _Factory() => new();

    #endregion

    public TimeSpan GetTimeLeft(FileSystemInfo target) => this._entries.TryGetValue(target.FullName, out var result)
      ? result.MinimumLifeTimeLeft
      : TimeSpan.Zero;

    public void SetTimeLeft(FileSystemInfo target, TimeSpan value) {
      if (this._entries.TryGetValue(target.FullName, out var entry))
        entry.MinimumLifeTimeLeft = value;
    }

    public void Add(FileInfo target) => this._entries.GetOrAdd(target.FullName, _ => new(target)).IsAlive = true;
    public void Add(DirectoryInfo target) => this._entries.GetOrAdd(target.FullName, _ => new(target)).IsAlive = true;

    public void Delete(FileSystemInfo target) {
      if (!this._entries.TryGetValue(target.FullName, out var entry))
        return;

      entry.IsAlive = false;
      this._ProcessEntry(entry);
    }

    private void _ProcessEntry(Entry entry) {
      if (entry.TicksLeft > 0)
        return;

      var target = entry.Target;
      try {
        if (entry.IsFile) {
          if (File.Exists(target)) {
            File.SetAttributes(target, File.GetAttributes(target) & ~(FileAttributes.ReadOnly | FileAttributes.System | FileAttributes.Hidden));
            File.Delete(target);
            Trace.WriteLine($"[{nameof(TemporaryTokenCleaner)}]File {target} sucessfully deleted");
          } else
            Trace.WriteLine($"[{nameof(TemporaryTokenCleaner)}]File {target} is already gone");
        } else {
          if (Directory.Exists(target)) {
            Directory.Delete(target, true);
            Trace.WriteLine($"[{nameof(TemporaryTokenCleaner)}]Directory {target} sucessfully deleted");
          } else
            Trace.WriteLine($"[{nameof(TemporaryTokenCleaner)}]Directory {target} is already gone");
        }

        this._entries.TryRemove(target, out _);
      } catch (Exception e) {
        Trace.WriteLine($"[{nameof(TemporaryTokenCleaner)}]Unable to delete {target}, {e.Message} - registering handler for later deletion");
        this._RegisterDeleteHandler();
      }
    }

    #region make sure stuff gets deleted - even if it doesn't want to

    private const int _STATE_YES = -1;
    private const int _STATE_NO = 0;
    private int _isRegistered = _STATE_NO;
    private static readonly TimeSpan _CLEANER_TIMEOUT = TimeSpan.FromSeconds(30);

    // ReSharper disable once NotAccessedField.Local
    private Timer _cleaner;

    private void _RegisterDeleteHandler() {
      if (Interlocked.CompareExchange(ref this._isRegistered, _STATE_YES, _STATE_NO) != _STATE_NO) {
        Trace.WriteLine($"[{nameof(TemporaryTokenCleaner)}]Handler already registered");
        return;
      }

      // TODO: we need a way to ensure stuff is deleted even when a debugger kills our process - eg external batch or whatever
      AppDomain.CurrentDomain.ProcessExit += this.CurrentDomain_ProcessExit;
      var timer = this._cleaner = new(this.Timer_Elapsed);
      timer.Change(_CLEANER_TIMEOUT, _CLEANER_TIMEOUT);
    }

    private void Timer_Elapsed(object state) {
      Trace.WriteLine($"[{nameof(TemporaryTokenCleaner)}]Timer elapsed - deleting what's needed to");
      this._DeleteLoop(false);
    }

    private void CurrentDomain_ProcessExit(object _, EventArgs __) {
      Trace.WriteLine($"[{nameof(TemporaryTokenCleaner)}]Shutdown detected - deleting what's still left");
      this._DeleteLoop(true);
    }

    private void _DeleteLoop(bool ignoreAliveFlag) {
      if (ignoreAliveFlag)
        foreach (var item in this._entries.Values)
          this._ProcessEntry(item);
      else
        foreach (var item in this._entries.Values)
          if (!item.IsAlive)
            this._ProcessEntry(item);
    }

    #endregion
  }

  public interface ITemporaryFileToken : IDisposable {
    FileInfo File { get; }
    TimeSpan MinimumLifetimeLeft { get; set; }
  }

  public interface ITemporaryDirectoryToken : IDisposable {
    DirectoryInfo Directory { get; }
    TimeSpan MinimumLifetimeLeft { get; set; }
  }

  private sealed class TemporaryFileToken : ITemporaryFileToken {
    private bool _isDisposed;
    public FileInfo File { get; }

    public TimeSpan MinimumLifetimeLeft {
      get => TemporaryTokenCleaner.Instance.GetTimeLeft(this.File);
      set => TemporaryTokenCleaner.Instance.SetTimeLeft(this.File, value);
    }

    public TemporaryFileToken(FileInfo file) {
      TemporaryTokenCleaner.Instance.Add(file);
      this.File = file;
    }

    public void Dispose() {
      if (this._isDisposed)
        return;

      this._isDisposed = true;
      TemporaryTokenCleaner.Instance.Delete(this.File);
      GC.SuppressFinalize(this);
    }

    ~TemporaryFileToken() => this.Dispose();
  }

  private sealed class TemporaryDirectoryToken : ITemporaryDirectoryToken {
    private bool _isDisposed;
    public DirectoryInfo Directory { get; }

    public TimeSpan MinimumLifetimeLeft {
      get => TemporaryTokenCleaner.Instance.GetTimeLeft(this.Directory);
      set => TemporaryTokenCleaner.Instance.SetTimeLeft(this.Directory, value);
    }

    public TemporaryDirectoryToken(DirectoryInfo directory) {
      TemporaryTokenCleaner.Instance.Add(directory);
      this.Directory = directory;
    }

    public void Dispose() {
      if (this._isDisposed)
        return;

      this._isDisposed = true;
      TemporaryTokenCleaner.Instance.Delete(this.Directory);
      GC.SuppressFinalize(this);
    }

    ~TemporaryDirectoryToken() => this.Dispose();
  }

  #endregion

  /// <summary>
  ///   Creates a temporary file and returns an IDisposable token which deletes the file upon dispose.
  /// </summary>
  /// <param name="name">The name of the file we're trying to create.</param>
  /// <param name="baseDirectory">The base directory if different from systems' temp.</param>
  /// <returns>The token to use.</returns>
  public static ITemporaryFileToken GetTempFileToken(string name = null, string baseDirectory = null) => new TemporaryFileToken(GetTempFile(name, baseDirectory));

  /// <summary>
  ///   Creates a temporary directory and returns an IDisposable token which deletes the directory upon dispose.
  /// </summary>
  /// <param name="name">The name of the directory we're trying to create.</param>
  /// <param name="baseDirectory">The base directory if different from systems' temp.</param>
  /// <returns>The token to use.</returns>
  public static ITemporaryDirectoryToken GetTempDirectoryToken(string name = null, string baseDirectory = null) => new TemporaryDirectoryToken(GetTempDirectory(name, baseDirectory));

  /// <summary>
  ///   Generates a temporary filename which is most like the given one in the temporary folder.
  /// </summary>
  /// <param name="name">The name.</param>
  /// <param name="baseDirectory">The base directory to use, we'll be using the temp directory if this is <c>null</c>.</param>
  /// <returns>A <see cref="FileInfo">FileInfo</see> instance pointing to the file.</returns>
  public static FileInfo GetTempFile(string name = null, string baseDirectory = null) => new(GetTempFileName(name, baseDirectory));

  /// <summary>
  ///   Generates a temporary filename which is most like the given one in the temporary folder.
  /// </summary>
  /// <param name="name">The name.</param>
  /// <param name="baseDirectory">The base directory to use, we'll be using the temp directory if this is <c>null</c>.</param>
  /// <returns>The full path of the created temporary directory.</returns>
  public static string GetTempFileName(string name = null, string baseDirectory = null) {
    name ??= "tmp";
    name = Path.GetFileName(name);

    var path = baseDirectory ?? GetUsableSystemTempDirectoryName();
    var fullName = Path.Combine(path, name);

    // if we could use the given name
    if (TryCreateFile(fullName)) {
      _TryMarkAsTemporaryFile(fullName);
      return fullName;
    }

    // otherwise, count
    var i = 1;
    var fileName = Path.GetFileNameWithoutExtension(name);
    var ext = Path.GetExtension(name);
    while (!TryCreateFile(fullName = Path.Combine(path, $"{fileName}.{++i}{ext}"))) { }

    _TryMarkAsTemporaryFile(fullName);
    return fullName;
  }

  /// <summary>
  ///   Tries to create a new file.
  /// </summary>
  /// <param name="fileName">The file to create.</param>
  /// <param name="attributes">The attributes.</param>
  /// <returns>
  ///   <c>true</c> if the file didn't exist and was successfully created; otherwise, <c>false</c>.
  /// </returns>
  public static bool TryCreateFile(string fileName, FileAttributes attributes = FileAttributes.Normal) {
    Against.ArgumentIsNull(fileName);

    if (File.Exists(fileName))
      return false;

    try {
      var fileHandle = File.Open(fileName, FileMode.CreateNew, FileAccess.Write);
      fileHandle.Close();
      try {
        File.SetAttributes(fileName, attributes);
      } catch {
        ; // swallow exception when attributes couldn't be set
      }

      return true;
    } catch (UnauthorizedAccessException) {
      // in case multiple threads try to create the same file, this gets fired
      return false;
    } catch (IOException) {
      // file already exists
      return false;
    }
  }

  /// <summary>
  ///   Generates a temporary directory which is most like the given one in the temporary folder.
  /// </summary>
  /// <param name="name">The name.</param>
  /// <param name="baseDirectory">The base directory to use, we'll be using the temp directory if this is <c>null</c>.</param>
  /// <returns>A <see cref="DirectoryInfo">DirectoryInfo</see> instance pointint to the directory.</returns>
  public static DirectoryInfo GetTempDirectory(string name = null, string baseDirectory = null) => new(GetTempDirectoryName(name, baseDirectory));

  /// <summary>
  ///   Generates a temporary directory which is most like the given one in the temporary folder.
  /// </summary>
  /// <param name="name">The name.</param>
  /// <param name="baseDirectory">The base directory to use, we'll be using the temp directory if this is <c>null</c>.</param>
  /// <returns>The full path of the created temporary directory.</returns>
  public static string GetTempDirectoryName(string name = null, string baseDirectory = null) {
    var path = baseDirectory ?? GetUsableSystemTempDirectoryName();

    // use a temp name if none given
    if (name == null) {
      const string PREFIX = "tmp";
      const int LENGTH = 4;
      const string SUFFIX = ".tmp";
      string result;
      Random random = new();

      // loop until the temporarely generated name does not exist
      do {
        // generate a temporary name
        StringBuilder tempName = new(PREFIX, LENGTH + PREFIX.Length);
        for (var j = LENGTH; j > 0; --j)
          tempName.Append(random.Next(0, 16).ToString("X"));

        tempName.Append(SUFFIX);
        result = Path.Combine(path, tempName.ToString());
      } while (!TryCreateDirectory(result));

      _TryMarkAsTemporaryDirectory(result);
      return result;
    }

    // a name is given, so try to accommodate this
    name = Path.GetFileName(name);
    var fullName = Path.Combine(path, name);

    // if we could use the given name, return it
    if (TryCreateDirectory(fullName)) {
      _TryMarkAsTemporaryDirectory(fullName);
      return fullName;
    }

    // otherwise count up
    var i = 1;
    while (!TryCreateDirectory(fullName = Path.Combine(path, $"{name}{++i}"))) { }
    
    _TryMarkAsTemporaryDirectory(fullName);
    return fullName;
  }


  public static DirectoryInfo GetUsableSystemTempDirectory() => new(GetUsableSystemTempDirectoryName());

  public static string GetUsableSystemTempDirectoryName() {
    foreach (var (path, shouldCreate) in GetPossibleTempPaths()) {

      // Skip empty or null paths.
      if (path.IsNullOrWhiteSpace())
        continue;

      var trimmed=path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

      try {
        if (!Directory.Exists(trimmed))
          if (shouldCreate) {
            Directory.CreateDirectory(trimmed);
            _TryMarkAsTemporaryDirectory(trimmed);
          } else
            continue;

        if (IsDirectoryWritable(trimmed))
          return trimmed;

      } catch {
        // Ignore errors and continue to the next candidate.
      }
    }
    
    // we should never land here
    throw new DirectoryNotFoundException("No valid temporary directory could be found.");

    static IEnumerable<(string, bool)> GetPossibleTempPaths() {
      yield return (Path.GetTempPath(), true);

      if (Environment.OSVersion.Platform == PlatformID.Win32NT && IsWindows11OrHigher())
        yield return (GetWindowsTempPath(), true);

      string candidate;
      if (Environment.OSVersion.Platform == PlatformID.Unix && (candidate = Environment.GetEnvironmentVariable("TMPDIR")).IsNotNullOrWhiteSpace())
          yield return (candidate, true);

      if ((candidate = Environment.GetEnvironmentVariable("TEMP")).IsNotNullOrWhiteSpace())
        yield return (candidate, true);
      if ((candidate = Environment.GetEnvironmentVariable("TMP")).IsNotNullOrWhiteSpace())
        yield return (candidate, true);

      switch (Environment.OSVersion.Platform) {
        case PlatformID.Win32S:
        case PlatformID.Win32Windows:
        case PlatformID.Win32NT:
        case PlatformID.WinCE:
        case PlatformID.Xbox: {

          // User-specific temp directories
          if ((candidate = Environment.GetEnvironmentVariable("LOCALAPPDATA")).IsNotNullOrWhiteSpace())
            yield return (Path.Combine(candidate, "Temp"), false);
          if ((candidate = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)).IsNotNullOrWhiteSpace())
            yield return (Path.Combine(candidate, "Temp"), false);
          if ((candidate = Environment.GetEnvironmentVariable("APPDATA")).IsNotNullOrWhiteSpace())
            yield return (Path.Combine(candidate, "Temp"), false);
          if ((candidate = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).IsNotNullOrWhiteSpace())
            yield return (Path.Combine(candidate, "Temp"), false);
          if ((candidate = Environment.GetEnvironmentVariable("USERPROFILE")).IsNotNullOrWhiteSpace()) {
            yield return (Path.Combine(candidate, "Temp"), false);
            yield return (Path.Combine(candidate, "AppData/Local/Temp"), false);
            yield return (Path.Combine(candidate, "AppData/Roaming/Temp"), false);
            yield return (Path.Combine(candidate, "AppData/Temp"), false);
          }

          // Windows legacy paths
          if ((candidate = Environment.GetFolderPath(Environment.SpecialFolder.System)).IsNotNullOrWhiteSpace())
            yield return (Path.Combine(candidate, "Temp"), false);
          if ((candidate = Environment.GetEnvironmentVariable("SYSTEMROOT")).IsNotNullOrWhiteSpace())
            yield return (Path.Combine(candidate, "Temp"), false);
          if ((candidate = Environment.GetEnvironmentVariable("WINDIR")).IsNotNullOrWhiteSpace()) {
            yield return (Path.Combine(candidate, "Temp"), false);
            yield return (Path.Combine(Path.GetPathRoot(candidate), "Temp"), false);
          }
          if ((candidate = Environment.GetEnvironmentVariable("SYSTEMDRIVE")).IsNotNullOrWhiteSpace())
            yield return (Path.Combine(candidate, "Temp"), false);

          yield return (@"C:\TEMP", false);
          break;
        }
        case PlatformID.Unix: {
          
          // Systemd-managed temporary directories (Linux)
          foreach (var systemdTempPath in GetSystemDPaths())
            yield return (systemdTempPath, true);

          // Standard Linux temp directories
          yield return ("/tmp", false);
          yield return ("/var/tmp", false);
          yield return ("/temp", false);
          yield return ("/var/temp", false);
          break;
        }
        case PlatformID.MacOSX: {
          yield return (GetMacOSTempDirectory(), false);
          break;
        }
        default: {

          // OpenVMS
          if ((candidate=Environment.GetEnvironmentVariable("SYS$SCRATCH")).IsNotNullOrWhiteSpace())
            yield return (candidate, true);

          // AmigaDOS
          yield return ("T:", false);
          break;
        }
      }

      // if all up to here has failed, we'll gonna create one ourselves
      const string DEFAULT_TEMP_NAME = "$$temp$$.$$$";
      
      // try working directory
      yield return (Path.Combine(Directory.GetCurrentDirectory(), DEFAULT_TEMP_NAME), true);
      yield return (Path.Combine(".",DEFAULT_TEMP_NAME), true);

      // try execution source directory
      if((candidate=AppDomain.CurrentDomain.BaseDirectory).IsNotNullOrWhiteSpace())
        yield return (Path.Combine(candidate, DEFAULT_TEMP_NAME), true);

      if (Assembly.GetEntryAssembly()?.Location is { } entryLocation && (candidate= Path.GetDirectoryName(entryLocation)).IsNotNullOrWhiteSpace())
        yield return (Path.Combine(candidate, DEFAULT_TEMP_NAME), true);

      if (Assembly.GetExecutingAssembly()?.Location is { } executingLocation && (candidate= Path.GetDirectoryName(executingLocation)).IsNotNullOrWhiteSpace())
        yield return (Path.Combine(candidate, DEFAULT_TEMP_NAME), true);

      yield break;

      static bool IsWindows11OrHigher() {
        var version = Environment.OSVersion.Version;
        return version.Major > 10 || (version.Major == 10 && version.Build >= 22000);
      }

      static string GetWindowsTempPath() {
        var result = new StringBuilder(260);
        try {
          return GetTempPath2(result.Capacity, result) > 0 ? result.ToString() : null;
        } catch {
          return null;
        }

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern uint GetTempPath2(int bufferLength, StringBuilder buffer);
      }

      static string GetMacOSTempDirectory() {
        var buffer = new StringBuilder(1024);
        try {
          if (NSTemporaryDirectory(buffer, buffer.Capacity))
            return buffer.ToString();
        } catch {
          // ignore error and continue
        }

        return null;

        [DllImport("Foundation.framework/Foundation", EntryPoint = "NSTemporaryDirectory")]
        static extern bool NSTemporaryDirectory(StringBuilder buffer, int bufferSize);
      }

      static IEnumerable<string> GetSystemDPaths() {
        var results = new List<string>(2);

        try {
          using var process = Process.Start(new ProcessStartInfo {
            FileName = "systemd-path",
            Arguments = "temporary temporary-large",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
          });

          while (!process?.StandardOutput.EndOfStream ?? false) {
            var path = process.StandardOutput.ReadLine()?.Trim();
            results.Add(path);
          }
        } catch {
          // Ignore errors
        }

        return results;
      }
      
    }

    static bool IsDirectoryWritable(string path) {
      string testFile = null;
      try {
        do
          testFile = Path.Combine(path, Path.GetRandomFileName());
        while (File.Exists(testFile));

        using (File.Create(testFile, 1, FileOptions.None)) { }

        return true;
      } catch {
        return false;
      } finally {
        File.Delete(testFile);
      }
    }

  }

  private static bool _TryMarkAsTemporaryFile(string path) {
    try {
      var file = new FileInfo(path);
      file.Attributes |= FileAttributes.NotContentIndexed;
      file.Attributes |= FileAttributes.Temporary;

      return true;
    } catch {
      // ignore errors
      return false;
    }
  }

  private static bool _TryMarkAsTemporaryDirectory(string path) {
    try {
      var dir = new DirectoryInfo(path);

      if (Environment.OSVersion.Platform is PlatformID.Unix or PlatformID.MacOSX)
        Process.Start("chattr", $@"+t ""{path.Replace("\"","\\\"")}""")?.WaitForExit();
      else {
        dir.Attributes |= FileAttributes.NotContentIndexed;
        dir.Attributes |= FileAttributes.Temporary;
      }

      return true;
    } catch {
      // ignore errors
      return false;
    }
  }
  
  /// <summary>
  ///   Tries to create a new folder.
  /// </summary>
  /// <param name="pathName">The directory name.</param>
  /// <param name="attributes">The attributes.</param>
  /// <returns>
  ///   <c>true</c> when the folder didn't exist and was successfully created; otherwise, <c>false</c>.
  /// </returns>
  public static bool TryCreateDirectory(string pathName, FileAttributes attributes = FileAttributes.Normal) {
    Against.ArgumentIsNullOrEmpty(pathName);

    if (Directory.Exists(pathName))
      return false;

    try {
      Directory.CreateDirectory(pathName);
      DirectoryInfo directory = new(pathName);
      if (attributes == FileAttributes.Normal)
        return true;

      try {
        directory.Attributes = attributes;
      } catch {
        // ignore attribute assigment errors
      }

      return true;
    } catch (IOException) {
      return false;
    }
  }

  /// <summary>
  ///   This could contain a full network path eg. user:password@\\server\share\folder\filename.extension
  /// </summary>
  public struct NetworkPath {
    private const char _pathSeparator = '\\';
    private const char _userSeparator = '@';
    private const char _passSeparator = ':';

    private string _username;
    private string _password;
    private string _server;
    private string _share;
    private string _fullPath;
    private string _directory;
    private string _uncPath;

    public string Username {
      readonly get => this._username;
      set {
        this._username = value;
        this._InvalidateUnc();
      }
    }

    public string Password {
      readonly get => this._password;
      set {
        this._password = value;
        this._InvalidateUnc();
      }
    }

    public string Server {
      readonly get => this._server;
      set {
        this._server = value;
        this._InvalidateFullPath();
      }
    }

    public string Share {
      readonly get => this._share;
      set {
        this._share = value;
        this._InvalidateFullPath();
      }
    }

    public string DirectoryAndOrFileName {
      readonly get => this._directory;
      set {
        this._directory = value;
        this._InvalidateFullPath();
      }
    }

    public string FullPath {
      readonly get => this._fullPath;
      set {
        this._fullPath = value;
        this._SplitPath();
        this._InvalidateUnc();
      }
    }

    public string UncPath {
      readonly get => this._uncPath;
      set {
        this._uncPath = value;
        this._SplitUnc();
      }
    }

    public NetworkPath(string uncPath) {
      this._username = this._password = this._server = this._share = this._fullPath = this._directory = this._uncPath = null;
      this.UncPath = uncPath;
    }

    private void _SplitPath() {
      var value = this._fullPath;
      // extract server
      if (value != null && value.StartsWith(_pathSeparator + string.Empty + _pathSeparator)) {
        var idx = value.IndexOf(_pathSeparator, 2);
        if (idx < 0) {
          this._server = value[2..];
          value = null;
        } else {
          this._server = value[2..(idx - 1)];
          value = value[idx..];
        }
      } else
        this._server = null;

      // extract share
      if (!string.IsNullOrEmpty(value) && value[0] == _pathSeparator) {
        var idx = value.IndexOf(_pathSeparator, 1);
        if (idx < 0) {
          this._share = value[1..];
          value = null;
        } else {
          this._share = value[1.. (idx - 1)];
          value = value[(idx + 1)..];
        }
      } else
        this._share = null;

      this._directory = string.IsNullOrEmpty(value) ? null : value;
    }

    private void _SplitUnc() {
      var value = this._uncPath;
      string password;
      var user = password = null;

      var idx = value.IndexOf(_userSeparator);
      if (idx >= 0) {
        var userAndOrPassword = value[..idx];
        value = value[(idx + 1)..];
        idx = userAndOrPassword.IndexOf(_passSeparator);
        if (idx >= 0) {
          user = userAndOrPassword[..idx];
          password = userAndOrPassword[(idx + 1)..];
        } else
          user = userAndOrPassword;
      }

      this._username = string.IsNullOrEmpty(user) ? null : user;
      this._password = string.IsNullOrEmpty(password) ? null : password;
      this._fullPath = value;
      this._SplitPath();
    }

    private void _InvalidateFullPath() {
      var result = this._server == null ? string.Empty : _pathSeparator + _pathSeparator + this._server;
      if (this._share != null)
        result += _pathSeparator + this._share;

      this._fullPath = result + (this._directory == null ? string.Empty : _pathSeparator + this._directory);
      this._InvalidateUnc();
    }

    private void _InvalidateUnc() {
      var result = string.Empty;
      if (this._username != null) {
        result += this._username;
        if (this._password != null)
          result += _passSeparator + this._password;
        result += _userSeparator;
      }

      this.UncPath = result + this._fullPath;
    }
  }
}
