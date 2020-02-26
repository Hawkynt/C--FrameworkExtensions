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
#if NET40
using System.Diagnostics.Contracts;
#endif
using System.Linq;
#if NET45
using System.Runtime.CompilerServices;
#endif
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace System.IO {
  /// <summary>
  /// Extensions for the DirectoryInfo type.
  /// </summary>
  internal static partial class DirectoryInfoExtensions {

    #region nested types

    private class SubdirectoryInfo {
      public SubdirectoryInfo(DirectoryInfo directory, string pathRelativeToRoot) {
        this.Directory = directory;
        this.PathRelativeToRoot = pathRelativeToRoot;
      }

      public DirectoryInfo Directory { get; }
      public string PathRelativeToRoot { get; }
    }

    private static class NativeMethods {
      [DllImport("mpr.dll", CharSet = CharSet.Unicode, SetLastError = true)]
      public static extern int WNetGetConnection(
          [MarshalAs(UnmanagedType.LPTStr)] string localName,
          [MarshalAs(UnmanagedType.LPTStr)] StringBuilder remoteName,
          ref int length);
    }

    /// <summary>
    /// Determines the order in which sub-items will be returned.
    /// </summary>
    public enum RecursionMode {
      /// <summary>
      /// The toplevel items only (eg. /a, /b)
      /// </summary>
      ToplevelOnly,
      /// <summary>
      /// The shortest path first (eg. /a, /b, /a/c, /b/d)
      /// </summary>
      ShortestPathFirst,
      /// <summary>
      /// The deepest path first (eg. /a , /a/c, /b, /b/d)
      /// </summary>
      DeepestPathFirst,
    }
    #endregion

    /// <summary>
    /// Renames this directory.
    /// </summary>
    /// <param name="this">This DirectoryInfo.</param>
    /// <param name="newName">The new name.</param>
    public static void RenameTo(this DirectoryInfo @this, string newName) {
      if (newName.Contains(Path.DirectorySeparatorChar) || newName.Contains(Path.AltDirectorySeparatorChar) || newName.Contains(Path.VolumeSeparatorChar))
        throw new ArgumentException("No support for new directory structures", nameof(newName));

      // nothing to do on same name
      if (@this.Name == newName)
        return;

      // more than just casing
      if (!string.Equals(@this.Name, newName, StringComparison.OrdinalIgnoreCase)) {
        @this.MoveTo(Path.Combine(@this.Parent.FullName, newName));
        return;
      }

      // only case has changed, so rename using a temporary intermediate
      var temporaryName = @this.FullName + "$";
      while (IO.Directory.Exists(temporaryName) || IO.File.Exists(temporaryName))
        temporaryName += "$";

      @this.MoveTo(temporaryName);
      @this.MoveTo(Path.Combine(@this.Parent.FullName, newName));
    }

    /// <summary>
    /// Deletes all files and directories in this DirectoryInfo.
    /// </summary>
    /// <param name="this">This DirectoryInfo.</param>
    /// <exception cref="System.NotSupportedException">Unknown FileSystem item</exception>
    public static void Clear(this DirectoryInfo @this) {
#if NET40
      Contract.Requires(@this != null);
      foreach (var item in @this.EnumerateFileSystemInfos()) {
#else
      foreach (var item in @this.GetFileSystemInfos()) {
#endif
        var file = item as FileInfo;
        if (file != null) {
          file.Delete();
          continue;
        }

        var directory = item as DirectoryInfo;
        if (directory != null) {
          directory.Delete(true);
          continue;
        }

        throw new NotSupportedException("Unknown FileSystem item");
      }
    }

    /// <summary>
    /// Gets the size.
    /// </summary>
    /// <param name="this">This DirectoryInfo.</param>
    /// <returns>The number of bytes in this directory</returns>
    public static long GetSize(this DirectoryInfo @this) {
#if NET40
      Contract.Requires(@this != null);
#endif
      // if less than 4 cores, use sequential approach
      if (Environment.ProcessorCount < 4)
#if NET40
        return @this.EnumerateFiles("*", SearchOption.AllDirectories).Select(f => f.Length).Sum();
#else
        return @this.GetFiles("*", SearchOption.AllDirectories).Select(f => f.Length).Sum();
#endif

      // otherwise, use MT approach
      var result = 0L;
      long[] itemsLeft = { 1L };
      using (var evente = new AutoResetEvent(false)) {

        Action<DirectoryInfo> factory = null;
        factory = d => {
          try {
#if NET40
            foreach (var item in d.EnumerateFileSystemInfos()) {
#else
          foreach (var item in d.GetFileSystemInfos()) {
#endif
              var file = item as FileInfo;
              if (file != null) {
                Interlocked.Add(ref result, file.Length);
                continue;
              }

              var folder = item as DirectoryInfo;
              if (folder != null) {
                Interlocked.Increment(ref itemsLeft[0]);
                if (!ThreadPool.QueueUserWorkItem(_ => factory(folder)))
                  factory.BeginInvoke(folder, factory.EndInvoke, null);

                continue;
              }

              throw new NotSupportedException("Unknown FileSystemInfo item");
            }
          } finally {
            Interlocked.Decrement(ref itemsLeft[0]);
            evente.Set();
          }
        };
        factory.BeginInvoke(@this, factory.EndInvoke, null);

        while (Interlocked.Read(ref itemsLeft[0]) > 0)
          evente.WaitOne();
      }

      return result;
    }

    /// <summary>
    /// Given a path, returns the UNC path or the original. (No exceptions
    /// are raised by this function directly). For example, "P:\2008-02-29"
    /// might return: "\\networkserver\Shares\Photos\2008-02-09"
    /// </summary>
    /// <param name="This">The path to convert to a UNC Path</param>
    /// <returns>A UNC path. If a network drive letter is specified, the
    /// drive letter is converted to a UNC or network path. If the
    /// originalPath cannot be converted, it is returned unchanged.</returns>
    public static DirectoryInfo GetRealPath(this DirectoryInfo This) {
      var originalPath = This.FullName;

      // look for the {LETTER}: combination ...
      if (originalPath.Length < 2 || originalPath[1] != ':')
        return (This);

      // don't use char.IsLetter here - as that can be misleading
      // the only valid drive letters are a-z && A-Z.
      var c = originalPath[0];
      if ((c < 'a' || c > 'z') && (c < 'A' || c > 'Z'))
        return (This);

      var sb = new StringBuilder(512);
      var size = sb.Capacity;
      var error = NativeMethods.WNetGetConnection(originalPath.Substring(0, 2), sb, ref size);
      if (error != 0)
        return (This);

      var path = originalPath.Substring(This.Root.FullName.Length);
      return (new DirectoryInfo(Path.Combine(sb.ToString().TrimEnd(), path)));
    }

    /// <summary>
    /// Enumerates the file system infos.
    /// </summary>
    /// <param name="This">This DirectoryInfo.</param>
    /// <param name="mode">The recursion mode.</param>
    /// <param name="recursionFilter">The filter to use for recursing into sub-directories (Walks on <c>true</c>; otherwise, skips recursion).</param>
    /// <returns>
    /// The FileSystemInfos
    /// </returns>
    /// <exception cref="System.NotSupportedException">RecursionMode</exception>
    public static IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(this DirectoryInfo This, RecursionMode mode, Func<DirectoryInfo, bool> recursionFilter = null) {
      switch (mode) {
        case RecursionMode.ToplevelOnly: {
#if NET40
            foreach (var result in This.EnumerateFileSystemInfos())
#else
            foreach (var result in This.GetFileSystemInfos())
#endif
              yield return (result);

            break;
          }
        case RecursionMode.ShortestPathFirst: {
            var results = new Queue<DirectoryInfo>();
            results.Enqueue(This);
            while (results.Any()) {
              var result = results.Dequeue();
#if NET40
              foreach (var fsi in result.EnumerateFileSystemInfos()) {
#else
              foreach (var fsi in result.GetFileSystemInfos()) {
#endif
                yield return (fsi);
                var di = fsi as DirectoryInfo;
                if (di == null)
                  continue;

                if (recursionFilter == null || recursionFilter(di))
                  results.Enqueue(di);
              }
            }
            break;
          }
        case RecursionMode.DeepestPathFirst: {
            var results = new Stack<DirectoryInfo>();
            results.Push(This);
            while (results.Any()) {
              var result = results.Pop();
#if NET40
              foreach (var fsi in result.EnumerateFileSystemInfos()) {
#else
              foreach (var fsi in result.GetFileSystemInfos()) {
#endif
                yield return (fsi);
                var di = fsi as DirectoryInfo;
                if (di == null)
                  continue;

                if (recursionFilter == null || recursionFilter(di))
                  results.Push(di);
              }
            }
            break;
          }
        default: {
            throw new NotSupportedException(nameof(RecursionMode));
          }
      }
    }

    /// <summary>
    /// Tries to set the last write time.
    /// </summary>
    /// <param name="This">This DirectoryInfo.</param>
    /// <param name="lastWriteTimeUtc">The date&amp;time.</param>
    /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
    public static bool TrySetLastWriteTimeUtc(this DirectoryInfo This, DateTime lastWriteTimeUtc) {
#if NET40
      Contract.Requires(This != null);
#endif
      This.Refresh();

      if (!This.Exists)
        return false;

      if (This.LastWriteTimeUtc == lastWriteTimeUtc)
        return true;

      try {
        This.LastWriteTimeUtc = lastWriteTimeUtc;
        return true;
      } catch (Exception) {
        return This.LastWriteTimeUtc == lastWriteTimeUtc;
      }
    }

    /// <summary>
    /// Tries to set the creation time.
    /// </summary>
    /// <param name="This">This DirectoryInfo.</param>
    /// <param name="creationTimeUtc">The date&amp;time.</param>
    /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
    public static bool TrySetCreationTimeUtc(this DirectoryInfo This, DateTime creationTimeUtc) {
#if NET40
      Contract.Requires(This != null);
#endif
      This.Refresh();

      if (!This.Exists)
        return (false);

      if (This.CreationTimeUtc == creationTimeUtc)
        return (true);

      try {
        This.CreationTimeUtc = creationTimeUtc;
        return (true);
      } catch (Exception) {
        return (This.CreationTimeUtc == creationTimeUtc);
      }
    }

    /// <summary>
    /// Tries to set the attributes.
    /// </summary>
    /// <param name="This">This DirectoryInfo.</param>
    /// <param name="attributes">The attributes.</param>
    /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
    public static bool TrySetAttributes(this DirectoryInfo This, FileAttributes attributes) {
#if NET40
      Contract.Requires(This != null);
#endif
      This.Refresh();

      if (!This.Exists)
        return (false);

      if (This.Attributes == attributes)
        return (true);

      try {
        This.Attributes = attributes;
        return (true);
      } catch (Exception) {
        return (This.Attributes == attributes);
      }
    }

    public static bool TryCreate(this DirectoryInfo @this) => TryCreate(@this, false);

    /// <summary>
    /// Tries to create the given directory.
    /// </summary>
    /// <param name="this">This DirectoryInfo.</param>
    /// <param name="recursive">if set to <c>true</c> recursively tries to create all subdirectories.</param>
    /// <returns>
    ///   <c>true</c> on success; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryCreate(this DirectoryInfo @this, bool recursive) {
#if NET40
      Contract.Requires(@this != null);
#endif
      if (@this.Exists)
        return true;

      try {
        if (recursive) {
          IO.Directory.CreateDirectory(@this.FullName);
          @this.Refresh();
        } else
          @this.Create();
        return true;
      } catch (Exception) {
        @this.Refresh();
        return @this.Exists;
      }
    }

    /// <summary>
    /// Tries to delete the given directory.
    /// </summary>
    /// <param name="this">This DirectoyInfo.</param>
    /// <param name="recursive">if set to <c>true</c> deletes recursive.</param>
    /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
    public static bool TryDelete(this DirectoryInfo @this, bool recursive = false) {
#if NET40
      Contract.Requires(@this != null);
#endif
      if (!@this.Exists)
        return (true);

      try {
        @this.Delete(recursive);
        @this.Refresh();
        return (true);
      } catch (Exception) {
        @this.Refresh();
        return (!@this.Exists);
      }
    }

    /// <summary>
    /// Checks whether the given directory does not exist.
    /// </summary>
    /// <param name="This">This DirectoryInfo.</param>
    /// <returns><c>true</c> if it does not exist; otherwise, <c>false</c>.</returns>
#if NET45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool NotExists(this DirectoryInfo This) => !This.Exists;

    /// <summary>
    /// Gets a directory under the current directory.
    /// </summary>
    /// <param name="This">This DirectoryInfo.</param>
    /// <param name="subdirectories">The relative path to the sub-directory.</param>
    /// <returns>A DirectoryInfo instance pointing to the given path.</returns>
#if NET45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if NET40
    public static DirectoryInfo Directory(this DirectoryInfo This, params string[] subdirectories) => new DirectoryInfo(Path.Combine(new[] { This.FullName }.Concat(subdirectories).ToArray()));
#else
    public static DirectoryInfo Directory(this DirectoryInfo This, params string[] subdirectories) => new DirectoryInfo(string.Join(Path.DirectorySeparatorChar + string.Empty, new[] { This.FullName }.Concat(subdirectories).ToArray()));
#endif

    /// <summary>
    /// Gets a file under the current directory.
    /// </summary>
    /// <param name="This">This DirectoryInfo.</param>
    /// <param name="filePath">The relative path to the file.</param>
    /// <returns>A FileInfo instance pointing to the given path.</returns>
#if NET45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if NET40
    public static FileInfo File(this DirectoryInfo This, params string[] filePath) => new FileInfo(Path.Combine(new[] { This.FullName }.Concat(filePath).ToArray()));
#else
    public static FileInfo File(this DirectoryInfo This, params string[] filePath) => new FileInfo(string.Join(Path.DirectorySeparatorChar + string.Empty, new[] { This.FullName }.Concat(filePath).ToArray()));
#endif

    /// <summary>
    /// Determines whether the specified subdirectory exists.
    /// </summary>
    /// <param name="This">This DirectoryInfo.</param>
    /// <param name="searchPattern">The search pattern.</param>
    /// <param name="searchOption">The search option.</param>
    /// <returns><c>true</c> if at least one match was found; otherwise, <c>false</c>.</returns>
#if NET45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasDirectory(this DirectoryInfo This, string searchPattern, SearchOption searchOption = SearchOption.TopDirectoryOnly) => This.EnumerateDirectories(searchPattern, searchOption).Any();
#else
    public static bool HasDirectory(this DirectoryInfo This, string searchPattern, SearchOption searchOption = SearchOption.TopDirectoryOnly) => This.GetDirectories(searchPattern, searchOption).Any();
#endif

    /// <summary>
    /// Determines whether the specified file exists.
    /// </summary>
    /// <param name="This">This DirectoryInfo.</param>
    /// <param name="searchPattern">The search pattern.</param>
    /// <param name="searchOption">The search option.</param>
    /// <returns><c>true</c> if at least one match was found; otherwise, <c>false</c>.</returns>
#if NET45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasFile(this DirectoryInfo This, string searchPattern, SearchOption searchOption = SearchOption.TopDirectoryOnly) => This.EnumerateFiles(searchPattern, searchOption).Any();
#elif NET40
    public static bool HasFile(this DirectoryInfo This, string searchPattern, SearchOption searchOption = SearchOption.TopDirectoryOnly){
      var stack = new Stack<DirectoryInfo>();
      stack.Push(This);
      while (stack.Count > 0) {
        var currentDirectory = stack.Pop();
        try {
          if (currentDirectory.EnumerateFiles(searchPattern).Any())
            return true;

          if (searchOption != SearchOption.TopDirectoryOnly)
            foreach (var item in currentDirectory.EnumerateDirectories())
              stack.Push(item);
        } catch (UnauthorizedAccessException) {
          ;
        }
      }
      return false;
    }
#else
    public static bool HasFile(this DirectoryInfo This, string searchPattern, SearchOption searchOption = SearchOption.TopDirectoryOnly) {
      var stack = new Stack<DirectoryInfo>();
      stack.Push(This);
      while (stack.Count > 0) {
        var currentDirectory = stack.Pop();
        try {
          if (currentDirectory.GetFiles(searchPattern).Any())
            return true;

          if (searchOption != SearchOption.TopDirectoryOnly)
            foreach (var item in currentDirectory.GetDirectories())
              stack.Push(item);
        } catch (UnauthorizedAccessException) {
          ;
        }
      }
      return false;
    }
#endif

    /// <summary>
    /// Creates the directory recursively.
    /// </summary>
    /// <param name="This">This DirectoryInfo.</param>
    public static void CreateDirectory(this DirectoryInfo This) {
#if NET40
      Contract.Requires(This != null);
#endif
      if (This.Parent != null && !This.Exists)
        CreateDirectory(This.Parent);

      if (This.Exists)
        return;

      This.Create();
      This.Refresh();
    }

    /// <summary>
    /// Copies the specified directory.
    /// </summary>
    /// <param name="This">This DirectoryInfo.</param>
    /// <param name="target">The target directory to place files.</param>
    public static void CopyTo(this DirectoryInfo This, DirectoryInfo target) {
#if NET40
      Contract.Requires(This != null);
      Contract.Requires(target != null);
#endif
      var stack = new Stack<SubdirectoryInfo>();
      stack.Push(new SubdirectoryInfo(This, "."));
      while (stack.Count > 0) {
        var current = stack.Pop();
        var relativePath = current.PathRelativeToRoot;
        var targetPath = Path.Combine(target.FullName, relativePath);

        // create directory if it does not exist
        if (!IO.Directory.Exists(targetPath))
          IO.Directory.CreateDirectory(targetPath);

        foreach (var fileSystemInfo in current.Directory.GetFileSystemInfos()) {
          var fileInfo = fileSystemInfo as FileInfo;
          if (fileInfo != null) {
            fileInfo.CopyTo(Path.Combine(targetPath, fileInfo.Name));
            continue;
          }

          var directoryInfo = fileSystemInfo as DirectoryInfo;
#if NET40
          Contract.Assert(directoryInfo != null, "Not a file or directory info, what is it ?");
#endif

          stack.Push(new SubdirectoryInfo(directoryInfo, Path.Combine(relativePath, directoryInfo.Name)));
        }
      }
    }

    /// <summary>
    /// Gets the or adds a subdirectory.
    /// </summary>
    /// <param name="This">This DirectoryInfo.</param>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    public static DirectoryInfo GetOrAddDirectory(this DirectoryInfo This, string name) {
#if NET40
      Contract.Requires(This != null);
#endif
      var fullPath = Path.Combine(This.FullName, name);
      return IO.Directory.Exists(fullPath) ? new DirectoryInfo(fullPath) : This.CreateSubdirectory(name);
    }

    /// <summary>
    /// Determines whether the specified directory contains file.
    /// </summary>
    /// <param name="This">This DirectoryInfo.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="option">The option.</param>
    /// <returns><c>true</c> if there is a matching file; otherwise, <c>false</c>.</returns>
#if NET45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsFile(this DirectoryInfo This, string fileName, SearchOption option = SearchOption.TopDirectoryOnly) => This.EnumerateFiles(fileName, option).Any();
#else
    public static bool ContainsFile(this DirectoryInfo This, string fileName, SearchOption option = SearchOption.TopDirectoryOnly) => This.GetFiles(fileName, option).Any();
#endif

    /// <summary>
    /// Determines whether the specified directory contains directory.
    /// </summary>
    /// <param name="This">This DirectoryInfo.</param>
    /// <param name="directoryName">Name of the directory.</param>
    /// <param name="option">The option.</param>
    /// <returns>
    ///   <c>true</c> if there is a matching directory; otherwise, <c>false</c>.
    /// </returns>
#if NET45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsDirectory(this DirectoryInfo This, string directoryName, SearchOption option = SearchOption.TopDirectoryOnly) => This.EnumerateDirectories(directoryName, option).Any();
#else
    public static bool ContainsDirectory(this DirectoryInfo This, string directoryName, SearchOption option = SearchOption.TopDirectoryOnly) => This.GetDirectories(directoryName, option).Any();
#endif

    /// <summary>
    /// Checks whether the given directory exists and contains at least one file.
    /// </summary>
    /// <param name="This">This DirectoryInfo.</param>
    /// <param name="fileMask">The file mask; defaults to '*.*'.</param>
    /// <returns><c>true</c> if it exists and has matching files; otherwise, <c>false</c>.</returns>
    public static bool ExistsAndHasFiles(this DirectoryInfo This, string fileMask = "*.*") {
      if (!This.Exists)
        return (false);

      try {
        if (This.HasFile(fileMask, SearchOption.AllDirectories))
          return (true);

      } catch (IOException) {
        return (false);
      }
      return (false);
    }

    /// <summary>
    /// Tries to create a temporary file.
    /// </summary>
    /// <param name="This">This DirectoryInfo.</param>
    /// <param name="extension">The extension; defaults to '.tmp'.</param>
    /// <returns>A temporary file</returns>
    public static FileInfo GetTempFile(this DirectoryInfo This, string extension = null) {
#if NET40
      Contract.Requires(This != null);
#endif
      extension = extension == null ? ".tmp" : '.' + extension.TrimStart('.');

      const int LENGTH = 4;
      const string PREFIX = "tmp";
      var random = new Random();
      Func<string> generator = () => {
        var result = new StringBuilder(16);
        result.Append(PREFIX);
        for (var i = 0; i < LENGTH; ++i)
          result.Append(random.Next(0, 16).ToString("X"));
        result.Append(extension);
        return result.ToString();
      };

      while (true) {
        var result = This.TryCreateFile(generator(), FileAttributes.NotContentIndexed | FileAttributes.Temporary);
        if (result != null)
          return result;
      }

    }

    /// <summary>
    /// Tries to create file.
    /// </summary>
    /// <param name="This">This DirectoryInfo.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="attributes">The attributes; defaults to FileAttributes.Normal.</param>
    /// <returns>A FileInfo instance or <c>null</c> on error.</returns>
    public static FileInfo TryCreateFile(this DirectoryInfo This, string fileName, FileAttributes attributes = FileAttributes.Normal) {
#if NET40
      Contract.Requires(This != null);
#endif

      var fullFileName = Path.Combine(This.FullName, fileName);
      if (IO.File.Exists(fullFileName))
        return (null);

      try {
        var fileHandle = IO.File.Open(fullFileName, FileMode.CreateNew, FileAccess.Write);
        fileHandle.Close();
        IO.File.SetAttributes(fullFileName, attributes);
        return (new FileInfo(fullFileName));
      } catch (UnauthorizedAccessException) {

        // in case multiple threads try to create the same file, this gets fired
        return (null);
      } catch (IOException) {

        // file already exists
        return (null);
      }
    }
  }
}

