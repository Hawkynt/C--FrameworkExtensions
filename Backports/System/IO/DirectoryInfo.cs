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

namespace System.IO;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
public static partial class DirectoryInfoPolyfills {

#if !SUPPORTS_ENUMERATING_IO

  public static IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(this DirectoryInfo @this) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));

    return Invoke(@this);

    static IEnumerable<FileSystemInfo> Invoke(DirectoryInfo @this) {
      foreach (var entry in @this.GetFileSystemInfos())
        yield return entry;
    }
  }

  public static IEnumerable<FileInfo> EnumerateFiles(this DirectoryInfo @this) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));

    return Invoke(@this);
    
    static IEnumerable<FileInfo> Invoke(DirectoryInfo @this) {
      foreach (var entry in @this.GetFiles())
        yield return entry;
    }
  }

  public static IEnumerable<DirectoryInfo> EnumerateDirectories(this DirectoryInfo @this) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));

    return Invoke(@this);
    
    static IEnumerable<DirectoryInfo> Invoke(DirectoryInfo @this) {
      foreach (var entry in @this.GetDirectories())
        yield return entry;
    }
  }

  public static IEnumerable<FileInfo> EnumerateFiles(this DirectoryInfo @this, string searchPattern, SearchOption searchOption) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));

    return Invoke(@this, searchPattern, searchOption);

    static IEnumerable<FileInfo> Invoke(DirectoryInfo @this, string searchPattern, SearchOption searchOption) {
      foreach (var entry in @this.GetFiles(searchPattern, searchOption))
        yield return entry;
    }
  }

  public static IEnumerable<DirectoryInfo> EnumerateDirectories(this DirectoryInfo @this, string searchPattern, SearchOption searchOption) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this));

    return Invoke(@this, searchPattern, searchOption);
    
    static IEnumerable<DirectoryInfo> Invoke(DirectoryInfo @this, string searchPattern, SearchOption searchOption) {
      foreach (var entry in @this.GetDirectories(searchPattern, searchOption))
        yield return entry;
    }
  }

#endif
}