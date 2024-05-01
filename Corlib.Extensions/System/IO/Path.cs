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

#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Guard;

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace System.IO;

public static partial class PathExtensions {

  #region nested types

  private class TemporaryTokenCleaner {

    #region entries

    [DebuggerDisplay("{" + nameof(_DebuggerDisplay) + "}")]
    private class Entry {
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
      : TimeSpan.Zero
    ;

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

  private class TemporaryFileToken : ITemporaryFileToken {
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

  private class TemporaryDirectoryToken : ITemporaryDirectoryToken {
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
  /// Creates a temporary file and returns an IDisposable token which deletes the file upon dispose.
  /// </summary>
  /// <param name="name">The name of the file we're trying to create.</param>
  /// <param name="baseDirectory">The base directory if different from systems' temp.</param>
  /// <returns>The token to use.</returns>
  public static ITemporaryFileToken GetTempFileToken(string name = null, string baseDirectory = null) => new TemporaryFileToken(GetTempFile(name, baseDirectory));

  /// <summary>
  /// Creates a temporary directory and returns an IDisposable token which deletes the directory upon dispose.
  /// </summary>
  /// <param name="name">The name of the directory we're trying to create.</param>
  /// <param name="baseDirectory">The base directory if different from systems' temp.</param>
  /// <returns>The token to use.</returns>
  public static ITemporaryDirectoryToken GetTempDirectoryToken(string name = null, string baseDirectory = null) => new TemporaryDirectoryToken(GetTempDirectory(name, baseDirectory));

  /// <summary>
  /// Generates a temporary filename which is most like the given one in the temporary folder.
  /// </summary>
  /// <param name="name">The name.</param>
  /// <param name="baseDirectory">The base directory to use, we'll be using the temp directory if this is <c>null</c>.</param>
  /// <returns>A <see cref="FileInfo">FileInfo</see> instance pointing to the file.</returns>
  public static FileInfo GetTempFile(string name = null, string baseDirectory = null) => new(GetTempFileName(name, baseDirectory));

  /// <summary>
  /// Generates a temporary filename which is most like the given one in the temporary folder.
  /// </summary>
  /// <param name="name">The name.</param>
  /// <param name="baseDirectory">The base directory to use, we'll be using the temp directory if this is <c>null</c>.</param>
  /// <returns>The full path of the created temporary directory.</returns>
  public static string GetTempFileName(string name = null, string baseDirectory = null) {

    // use fully random name if none is given
    if (name == null)
      return Path.GetTempFileName();

    var path = baseDirectory ?? Path.GetTempPath();
    name = Path.GetFileName(name);
#if SUPPORTS_CONTRACTS
    Contract.Assert(name != null, "Filename went <null>");
#endif
    var fullName = Path.Combine(path, name);

    // if we could use the given name
    if (TryCreateFile(fullName, FileAttributes.NotContentIndexed | FileAttributes.Temporary))
      return fullName;

    // otherwise, count
    var i = 1;
    var fileName = Path.GetFileNameWithoutExtension(name);
    var ext = Path.GetExtension(name);
    while (!TryCreateFile(fullName = Path.Combine(path, $"{fileName}.{++i}{ext}"), FileAttributes.NotContentIndexed | FileAttributes.Temporary)) { }
    return fullName;
  }

  /// <summary>
  /// Tries to create a new file.
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
  /// Generates a temporary directory which is most like the given one in the temporary folder.
  /// </summary>
  /// <param name="name">The name.</param>
  /// <param name="baseDirectory">The base directory to use, we'll be using the temp directory if this is <c>null</c>.</param>
  /// <returns>A <see cref="DirectoryInfo">DirectoryInfo</see> instance pointint to the directory.</returns>
  public static DirectoryInfo GetTempDirectory(string name = null, string baseDirectory = null) => new(GetTempDirectoryName(name, baseDirectory));

  /// <summary>
  /// Generates a temporary directory which is most like the given one in the temporary folder.
  /// </summary>
  /// <param name="name">The name.</param>
  /// <param name="baseDirectory">The base directory to use, we'll be using the temp directory if this is <c>null</c>.</param>
  /// <returns>The full path of the created temporary directory.</returns>
  public static string GetTempDirectoryName(string name = null, string baseDirectory = null) {
    var path = baseDirectory ?? Path.GetTempPath();

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
#if SUPPORTS_CONTRACTS
        Contract.Assume(!string.IsNullOrEmpty(result));
#endif
      } while (!TryCreateDirectory(result));

      return result;
    }

    // a name is given, so try to accommodate this
    name = Path.GetFileName(name);
#if SUPPORTS_CONTRACTS
    Contract.Assert(name != null, "DirectoryName went <null>");
#endif
    var fullName = Path.Combine(path, name);

    // if we could use the given name, return it
#if SUPPORTS_CONTRACTS
    Contract.Assume(!string.IsNullOrEmpty(fullName));
#endif
    if (TryCreateDirectory(fullName, FileAttributes.NotContentIndexed))
      return fullName;

    // otherwise count up
    var i = 1;
    while (!TryCreateDirectory(fullName = Path.Combine(path, $"{name}{++i}"), FileAttributes.NotContentIndexed)) { }
    return fullName;
  }

  /// <summary>
  /// Tries to create a new folder.
  /// </summary>
  /// <param name="pathName">The directory name.</param>
  /// <param name="attributes">The attributes.</param>
  /// <returns>
  ///   <c>true</c> when the folder didn't exist and was successfully created; otherwise, <c>false</c>.
  /// </returns>
  public static bool TryCreateDirectory(string pathName, FileAttributes attributes = FileAttributes.Normal) {
#if SUPPORTS_CONTRACTS
    Contract.Requires(!string.IsNullOrEmpty(pathName));
#endif
    if (Directory.Exists(pathName))
      return false;

    try {
      Directory.CreateDirectory(pathName);
      DirectoryInfo directory = new(pathName);
      directory.Attributes = attributes;
      return true;
    } catch (IOException) {
      return false;
    }
  }

  /// <summary>
  /// This could contain a full network path eg. user:password@\\server\share\folder\filename.extension
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
      get => this._username;
      set { this._username = value; this._InvalidateUnc(); }
    }
    public string Password {
      get => this._password;
      set { this._password = value; this._InvalidateUnc(); }
    }

    public string Server {
      get => this._server;
      set { this._server = value; this._InvalidateFullPath(); }
    }

    public string Share {
      get => this._share;
      set { this._share = value; this._InvalidateFullPath(); }
    }

    public string DirectoryAndOrFileName {
      get => this._directory;
      set { this._directory = value; this._InvalidateFullPath(); }
    }
    public string FullPath {
      get => this._fullPath;
      set {
        this._fullPath = value;
        this._SplitPath();
        this._InvalidateUnc();
      }
    }
    public string UncPath {
      get => this._uncPath;
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
#if SUPPORTS_CONTRACTS
        Contract.Assume(value.Length > 2);
#endif
        var idx = value.IndexOf(_pathSeparator, 2);
        if (idx < 0) {
          this._server = value.Substring(2);
          value = null;
        } else {
          this._server = value.Substring(2, idx - 2);
          value = value.Substring(idx);
        }
      } else {
        this._server = null;
      }

      // extract share
      if (!string.IsNullOrEmpty(value) && value[0] == _pathSeparator) {
#if SUPPORTS_CONTRACTS
        Contract.Assume(value.Length > 1);
#endif
        var idx = value.IndexOf(_pathSeparator, 1);
        if (idx < 0) {
          this._share = value.Substring(1);
          value = null;
        } else {
          this._share = value.Substring(1, idx - 1);
          value = value.Substring(idx + 1);
        }
      } else {
        this._share = null;
      }

      this._directory = string.IsNullOrEmpty(value) ? null : value;
    }

    private void _SplitUnc() {
      var value = this._uncPath;
      string password;
      var user = password = null;

      var idx = value.IndexOf(_userSeparator);
      if (idx >= 0) {
        var userAndOrPassword = value.Substring(0, idx);
        value = value.Substring(idx + 1);
        idx = userAndOrPassword.IndexOf(_passSeparator);
        if (idx >= 0) {
          user = userAndOrPassword.Substring(0, idx);
          password = userAndOrPassword.Substring(idx + 1);
        } else {
          user = userAndOrPassword;
        }
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