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

namespace System.IO {
  /// <summary>
  /// Extensions for the DirectoryInfo type.
  /// </summary>
  internal static partial class DirectoryInfoExtensions {

    /// <summary>
    /// Gets a directory under the current directory.
    /// </summary>
    /// <param name="This">This DirectoryInfo.</param>
    /// <param name="subdirectories">The relative path to the sub-directory.</param>
    /// <returns>A DirectoryInfo instance pointing to the given path.</returns>
    public static DirectoryInfo Directory(this DirectoryInfo This, params string[] subdirectories) {
      return (new DirectoryInfo(Path.Combine(new[] { This.FullName }.Concat(subdirectories).ToArray())));
    }

    /// <summary>
    /// Gets a file under the current directory.
    /// </summary>
    /// <param name="This">This DirectoryInfo.</param>
    /// <param name="filePath">The relative path to the file.</param>
    /// <returns>A FileInfo instance pointing to the given path.</returns>
    public static FileInfo File(this DirectoryInfo This, params string[] filePath) {
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
    public static void Copy(this DirectoryInfo This, DirectoryInfo target) {
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
