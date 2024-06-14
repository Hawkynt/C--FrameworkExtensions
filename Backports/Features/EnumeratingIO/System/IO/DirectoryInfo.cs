#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY

#endregion

#if !SUPPORTS_ENUMERATING_IO

using System.Collections.Generic;

namespace System.IO;

public static partial class DirectoryInfoPolyfills {
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
}

#endif
