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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace System.IO {
  /// <summary>
  /// Extensions for the DirectoryInfo type.
  /// </summary>
  internal static partial class DirectoryInfoExtensions {

    #region nested types

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
          foreach (var result in This.EnumerateFileSystemInfos())
            yield return (result);
          break;
        }
        case RecursionMode.ShortestPathFirst: {
          var results = new Queue<DirectoryInfo>();
          results.Enqueue(This);
          while (results.Any()) {
            var result = results.Dequeue();
            foreach (var fsi in result.EnumerateFileSystemInfos()) {
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
            foreach (var fsi in result.EnumerateFileSystemInfos()) {
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
          throw new NotSupportedException("RecursionMode");
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
      Contract.Requires(This != null);
      This.Refresh();

      if (!This.Exists)
        return (false);

      if (This.LastWriteTimeUtc == lastWriteTimeUtc)
        return (true);

      try {
        This.LastWriteTimeUtc = lastWriteTimeUtc;
        return (true);
      } catch {
        return (This.LastWriteTimeUtc == lastWriteTimeUtc);
      }
    }

    /// <summary>
    /// Tries to set the creation time.
    /// </summary>
    /// <param name="This">This DirectoryInfo.</param>
    /// <param name="creationTimeUtc">The date&amp;time.</param>
    /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
    public static bool TrySetCreationTimeUtc(this DirectoryInfo This, DateTime creationTimeUtc) {
      Contract.Requires(This != null);
      This.Refresh();

      if (!This.Exists)
        return (false);

      if (This.CreationTimeUtc == creationTimeUtc)
        return (true);

      try {
        This.CreationTimeUtc = creationTimeUtc;
        return (true);
      } catch {
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
      Contract.Requires(This != null);
      This.Refresh();

      if (!This.Exists)
        return (false);

      if (This.Attributes == attributes)
        return (true);

      try {
        This.Attributes = attributes;
        return (true);
      } catch {
        return (This.Attributes == attributes);
      }
    }

    /// <summary>
    /// Tries to create the given directory.
    /// </summary>
    /// <param name="This">This DirectoryInfo.</param>
    /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
    public static bool TryCreate(this DirectoryInfo This) {
      Contract.Requires(This != null);
      if (This.Exists)
        return (true);

      try {
        This.Create();
        return (true);
      } catch {
        return (This.Exists);
      }

    }

    /// <summary>
    /// Checks whether the given directory does not exist.
    /// </summary>
    /// <param name="This">This DirectoryInfo.</param>
    /// <returns><c>true</c> if it does not exist; otherwise, <c>false</c>.</returns>
    public static bool NotExists(this DirectoryInfo This) {
      Contract.Requires(This != null);
      return (!This.Exists);
    }

    /// <summary>
    /// Gets a directory under the current directory.
    /// </summary>
    /// <param name="This">This DirectoryInfo.</param>
    /// <param name="subdirectories">The relative path to the sub-directory.</param>
    /// <returns>A DirectoryInfo instance pointing to the given path.</returns>
    public static DirectoryInfo Directory(this DirectoryInfo This, params string[] subdirectories) {
      Contract.Requires(This != null);
      return (new DirectoryInfo(Path.Combine(new[] { This.FullName }.Concat(subdirectories).ToArray())));
    }

    /// <summary>
    /// Gets a file under the current directory.
    /// </summary>
    /// <param name="This">This DirectoryInfo.</param>
    /// <param name="filePath">The relative path to the file.</param>
    /// <returns>A FileInfo instance pointing to the given path.</returns>
    public static FileInfo File(this DirectoryInfo This, params string[] filePath) {
      Contract.Requires(This != null);
      return (new FileInfo(Path.Combine(new[] { This.FullName }.Concat(filePath).ToArray())));
    }

    /// <summary>
    /// Determines whether the specified subdirectory exists.
    /// </summary>
    /// <param name="This">This DirectoryInfo.</param>
    /// <param name="searchPattern">The search pattern.</param>
    /// <param name="searchOption">The search option.</param>
    /// <returns><c>true</c> if at least one match was found; otherwise, <c>false</c>.</returns>
    public static bool HasDirectory(this DirectoryInfo This, string searchPattern, SearchOption searchOption = SearchOption.TopDirectoryOnly) {
      Contract.Requires(This != null);
      return (This.EnumerateDirectories(searchPattern, searchOption).Any());
    }

    /// <summary>
    /// Determines whether the specified file exists.
    /// </summary>
    /// <param name="This">This DirectoryInfo.</param>
    /// <param name="searchPattern">The search pattern.</param>
    /// <param name="searchOption">The search option.</param>
    /// <returns><c>true</c> if at least one match was found; otherwise, <c>false</c>.</returns>
    public static bool HasFile(this DirectoryInfo This, string searchPattern, SearchOption searchOption = SearchOption.TopDirectoryOnly) {
      Contract.Requires(This != null);
      return (This.EnumerateFiles(searchPattern, searchOption).Any());
    }

    /// <summary>
    /// Creates the directory recursively.
    /// </summary>
    /// <param name="This">This DirectoryInfo.</param>
    public static void CreateDirectory(this DirectoryInfo This) {
      Contract.Requires(This != null);
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
      Contract.Requires(This != null);
      Contract.Requires(target != null);
      var stack = new Stack<Tuple<DirectoryInfo, string>>();
      stack.Push(Tuple.Create(This, "."));
      while (stack.Count > 0) {
        var current = stack.Pop();
        var relativePath = current.Item2;
        var targetPath = Path.Combine(target.FullName, relativePath);

        // create directory if it does not exist
        if (!IO.Directory.Exists(targetPath))
          IO.Directory.CreateDirectory(targetPath);

        foreach (var fileSystemInfo in current.Item1.GetFileSystemInfos()) {
          var fileInfo = fileSystemInfo as FileInfo;
          if (fileInfo != null) {
            fileInfo.CopyTo(Path.Combine(targetPath, fileInfo.Name));
            continue;
          }

          var directoryInfo = fileSystemInfo as DirectoryInfo;
          Contract.Assert(directoryInfo != null, "Not a file or directory info, what is it ?");

          stack.Push(Tuple.Create(directoryInfo, Path.Combine(relativePath, directoryInfo.Name)));
        }
      }
    }
  }
}
