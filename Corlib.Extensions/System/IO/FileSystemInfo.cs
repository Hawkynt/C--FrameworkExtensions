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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Guard;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.IO;

public static partial class FileSystemInfoExtensions {
  /// <summary>
  ///   Checks whether the given FileSystemInfo does not exist.
  /// </summary>
  /// <param name="this">This FileSystemInfo.</param>
  /// <returns><c>true</c> if it does not exist; otherwise, <c>false</c>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool NotExists(this FileSystemInfo @this) => !@this.Exists;

  /// <summary>
  ///   Checks whether the given FileSystemInfo is <c>null</c> or if it does not exists.
  /// </summary>
  /// <param name="this">This FileSystemInfo</param>
  /// <returns><c>true</c> if it is either <c>null</c> or can not be found; otherwise, <c>false</c>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNullOrDoesNotExist([NotNullWhen(false)] this FileSystemInfo @this) => @this is not { Exists: true };

  /// <summary>
  ///   Checks whether the given FileSystemInfo is not <c>null</c> and if it exists.
  /// </summary>
  /// <param name="this">This FileSystemInfo</param>
  /// <returns><c>true</c> if it is not <c>null</c> and can not be found; otherwise, <c>false</c>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsNotNullAndExists([NotNullWhen(true)] this FileSystemInfo @this) => @this is { Exists: true };

  /// <summary>
  ///   Returns a given path relative to another.
  /// </summary>
  /// <param name="tgtPath">The target path.</param>
  /// <param name="srcPath">The base path.</param>
  /// <returns>A relative path, if possible; otherwise, the absolute target path is returned.</returns>
  private static string _RelativeTo(string tgtPath, string srcPath) {
    Against.ArgumentIsNull(tgtPath);
    Against.ArgumentIsNull(srcPath);

    // convert backslashes and slashes to whatever the os prefers and split into parts
    var tgtArray = tgtPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).TrimEnd(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);
    var srcArray = srcPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).TrimEnd(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);

    var caseSensitive = Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX;
    List<string> result = [];
    var i = 0;

    // find out how many parts match
    while (i < srcArray.Length && i < tgtArray.Length && string.Equals(srcArray[i], tgtArray[i], caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase))
      ++i;

    // if no match was found at all, both paths do not have the same base so we return the target path
    if (i == 0)
      return tgtPath;

    // walk up till we are at the match
    for (var j = 0; j < srcArray.Length - i; ++j)
      result.Add("..");

    // walk down to the target
    for (var j = 0; j < tgtArray.Length - i; ++j)
      result.Add(tgtArray[j + i]);

    return string.Join(
      Path.DirectorySeparatorChar + string.Empty,
#if SUPPORTS_JOIN_ENUMERABLES
      result
#else
      result.ToArray()
#endif
    );
  }

  /// <summary>
  ///   Returns a given path relative to another.
  /// </summary>
  /// <param name="this">This FileSystemInfo.</param>
  /// <param name="source">The base path.</param>
  /// <returns>A relative path, if possible; otherwise, the absolute target path is returned.</returns>
  public static string RelativeTo(this FileSystemInfo @this, FileSystemInfo source) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(source);

    return _RelativeTo(@this.FullName, source.FullName);
  }

  /// <summary>
  ///   Determines whether two objects are on the same physical drive.
  /// </summary>
  /// <param name="this">This FileSystemInfo.</param>
  /// <param name="other">The other FileSystemInfo.</param>
  /// <returns>
  ///   <c>true</c> if both are on the same physical drive; otherwise, <c>false</c>.
  /// </returns>
  public static bool IsOnSamePhysicalDrive(this FileSystemInfo @this, FileSystemInfo other) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(other);

    return IsOnSamePhysicalDrive(@this.FullName, other.FullName);
  }


  /// <summary>
  ///   Determines whether two paths are on the same physical drive.
  /// </summary>
  /// <param name="path">The path.</param>
  /// <param name="other">The other path.</param>
  /// <returns>
  ///   <c>true</c> if both are on the same physical drive; otherwise, <c>false</c>.
  /// </returns>
  public static bool IsOnSamePhysicalDrive(string path, string other) {
    Against.ArgumentIsNull(path);
    Against.ArgumentIsNull(other);

    path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
    other = other.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

    var prefix = Path.DirectorySeparatorChar + string.Empty + Path.DirectorySeparatorChar;

    string drive, otherdrive;

    if (path.IndexOf(Path.VolumeSeparatorChar) >= 0)
      drive = Path.GetPathRoot(path);
    else
      drive = path.StartsWith(prefix)
          ? path[..path.IndexOf(Path.DirectorySeparatorChar, prefix.Length + 1)]
          : path
        ;

    if (other.IndexOf(Path.VolumeSeparatorChar) >= 0)
      otherdrive = Path.GetPathRoot(other);
    else
      otherdrive = other.StartsWith(prefix)
          ? other[..other.IndexOf(Path.DirectorySeparatorChar, prefix.Length + 1)]
          : other
        ;

    return drive == otherdrive;
  }

  public static TimeSpan Age(this FileSystemInfo @this) => DateTime.UtcNow - @this.LastWriteTimeUtc;

  /// <summary>
  ///   Checks whether a FileInfo or DirectoryInfo object is a directory, or intended to be a directory.
  /// </summary>
  /// <param name="this">This object</param>
  /// <returns>A bool indicating wether the given FileSystemInfo is a directory or not</returns>
  public static bool IsDirectory(this FileSystemInfo @this) {
    if (@this == null)
      return false;

    if ((int)@this.Attributes != -1)
      return @this.Attributes.HasFlag(FileAttributes.Directory);

    return @this is DirectoryInfo;
  }
}
