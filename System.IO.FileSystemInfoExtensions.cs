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

namespace System.IO {
  internal static partial class FileSystemInfoExtensions {
    /// <summary>
    /// Tests the RelativeTo routine.
    /// </summary>
    internal static void _TestRelativeTo() {
      var tests = new Func<bool>[] {
        ()=>RelativeTo("a/b","a")=="b",
        ()=>RelativeTo("a","a/b")=="..",
        ()=>RelativeTo("a/b","a/b")=="",
        ()=>RelativeTo("c","a")=="c",
        ()=>RelativeTo("a/b","a\\b")=="",
        ()=>RelativeTo("\\/A","//a/b/c")=="..\\..",
      };
      foreach (var test in tests) {
        if (!test())
          throw new NotImplementedException("Implementation did not meet test requirements");
      }
    }

    /// <summary>
    /// Returns a given path relative to another.
    /// </summary>
    /// <param name="tgtPath">The target path.</param>
    /// <param name="srcPath">The base path.</param>
    /// <returns>A relative path, if possible; otherwise, the absolute target path is returned.</returns>
    public static string RelativeTo(string tgtPath, string srcPath) {
      Contract.Requires(tgtPath != null);
      Contract.Requires(srcPath != null);

      // convert backslashes and slashes to whatever the os prefers and split into parts
      var tgtArray = tgtPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);
      var srcArray = srcPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);

      var caseSensitive = Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX;
      var result = new List<string>();
      var i = 0;

      // find out how many parts match
      while (i < srcArray.Length && i < tgtArray.Length && string.Equals(srcArray[i], tgtArray[i], caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase))
        i++;

      // if no match was found at all, both paths do not have the same base so we return the target path
      if (i == 0)
        return (tgtPath);

      // walk up till we are at the match
      for (var j = 0; j < srcArray.Length - i; j++)
        result.Add("..");

      // walk down to the target
      for (var j = 0; j < tgtArray.Length - i; j++)
        result.Add(tgtArray[j + i]);

      return (string.Join(Path.DirectorySeparatorChar + string.Empty, result));
    }

    /// <summary>
    /// Returns a given path relative to another.
    /// </summary>
    /// <param name="This">This FileSystemInfo.</param>
    /// <param name="source">The base path.</param>
    /// <returns>A relative path, if possible; otherwise, the absolute target path is returned.</returns>
    public static string RelativeTo(this FileSystemInfo This, FileSystemInfo source) {
      Contract.Requires(This != null);
      Contract.Requires(source != null);
      return (RelativeTo(This.FullName, source.FullName));
    }

    /// <summary>
    /// Determines whether two objects are on the same physical drive.
    /// </summary>
    /// <param name="This">This FileSystemInfo.</param>
    /// <param name="other">The other FileSystemInfo.</param>
    /// <returns>
    ///   <c>true</c> if both are on the same physical drive; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsOnSamePhysicalDrive(this FileSystemInfo This, FileSystemInfo other) {
      Contract.Requires(This != null);
      Contract.Requires(other != null);
      return (IsOnSamePhysicalDrive(This.FullName, other.FullName));
    }


    /// <summary>
    /// Determines whether two paths are on the same physical drive.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="other">The other path.</param>
    /// <returns>
    ///   <c>true</c> if both are on the same physical drive; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsOnSamePhysicalDrive(string path, string other) {
      Contract.Requires(path != null);
      Contract.Requires(other != null);

      path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
      other = other.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

      var prefix = Path.DirectorySeparatorChar + string.Empty + Path.DirectorySeparatorChar;

      string drive, otherdrive;

      if (path.IndexOf(Path.VolumeSeparatorChar) >= 0)
        drive = Path.GetPathRoot(path);
      else {
        drive = path.StartsWith(prefix) ? path.Substring(0, path.IndexOf(Path.DirectorySeparatorChar, prefix.Length + 1)) : path;
      }

      if (other.IndexOf(Path.VolumeSeparatorChar) >= 0)
        otherdrive = Path.GetPathRoot(other);
      else
        otherdrive = other.StartsWith(prefix) ? other.Substring(0, other.IndexOf(Path.DirectorySeparatorChar, prefix.Length + 1)) : other;


      return (drive == otherdrive);
    }
  }
}
