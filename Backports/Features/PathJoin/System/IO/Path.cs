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

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.IO;

public static partial class PathPolyfills {
  extension(Path) {

#if !SUPPORTS_PATH_JOIN

  /// <summary>
  /// Concatenates two paths into a single path.
  /// </summary>
  /// <param name="path1">The first path to join.</param>
  /// <param name="path2">The second path to join.</param>
  /// <returns>The concatenated path.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string Join(string path1, string path2) {
    if (string.IsNullOrEmpty(path1))
      return path2 ?? string.Empty;
    if (string.IsNullOrEmpty(path2))
      return path1;

    var c = path1[^1];
    return c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar || c == Path.VolumeSeparatorChar
      ? path1 + path2
      : path1 + Path.DirectorySeparatorChar + path2;
  }

  /// <summary>
  /// Concatenates three paths into a single path.
  /// </summary>
  /// <param name="path1">The first path to join.</param>
  /// <param name="path2">The second path to join.</param>
  /// <param name="path3">The third path to join.</param>
  /// <returns>The concatenated path.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string Join(string path1, string path2, string path3)
    => Join(Join(path1, path2), path3);

  /// <summary>
  /// Concatenates four paths into a single path.
  /// </summary>
  /// <param name="path1">The first path to join.</param>
  /// <param name="path2">The second path to join.</param>
  /// <param name="path3">The third path to join.</param>
  /// <param name="path4">The fourth path to join.</param>
  /// <returns>The concatenated path.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string Join(string path1, string path2, string path3, string path4)
    => Join(Join(path1, path2), Join(path3, path4));

  /// <summary>
  /// Concatenates an array of paths into a single path.
  /// </summary>
  /// <param name="paths">An array of paths.</param>
  /// <returns>The concatenated path.</returns>
  public static string Join(params string[] paths) {
    ArgumentNullException.ThrowIfNull(paths);
    if (paths.Length == 0)
      return string.Empty;

    var result = paths[0] ?? string.Empty;
    for (var i = 1; i < paths.Length; ++i)
      result = Join(result, paths[i]);

    return result;
  }

  /// <summary>
  /// Concatenates two path components into a single path.
  /// </summary>
  /// <param name="path1">A character span that contains the first path to join.</param>
  /// <param name="path2">A character span that contains the second path to join.</param>
  /// <returns>The concatenated path.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2)
    => Join(path1.ToString(), path2.ToString());

  /// <summary>
  /// Concatenates three path components into a single path.
  /// </summary>
  /// <param name="path1">A character span that contains the first path to join.</param>
  /// <param name="path2">A character span that contains the second path to join.</param>
  /// <param name="path3">A character span that contains the third path to join.</param>
  /// <returns>The concatenated path.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3)
    => Join(path1.ToString(), path2.ToString(), path3.ToString());

  /// <summary>
  /// Concatenates four path components into a single path.
  /// </summary>
  /// <param name="path1">A character span that contains the first path to join.</param>
  /// <param name="path2">A character span that contains the second path to join.</param>
  /// <param name="path3">A character span that contains the third path to join.</param>
  /// <param name="path4">A character span that contains the fourth path to join.</param>
  /// <returns>The concatenated path.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3, ReadOnlySpan<char> path4)
    => Join(path1.ToString(), path2.ToString(), path3.ToString(), path4.ToString());

#endif

#if !SUPPORTS_PATH_EXISTS

  /// <summary>
  /// Determines whether the specified path exists as a file or directory.
  /// </summary>
  /// <param name="path">The path to check.</param>
  /// <returns><see langword="true"/> if the path exists as a file or directory; otherwise, <see langword="false"/>.</returns>
  /// <remarks>
  /// This method returns <see langword="false"/> if <paramref name="path"/> is <see langword="null"/> or empty.
  /// Unlike <see cref="File.Exists"/> and <see cref="Directory.Exists"/>, this method checks for both files and directories.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool Exists(string? path)
    => !string.IsNullOrEmpty(path) && (File.Exists(path) || Directory.Exists(path));

#endif

#if !SUPPORTS_PATH_ENDSINDIRECTORYSEPARATOR

  /// <summary>
  /// Returns a value that indicates whether the specified path ends in a directory separator.
  /// </summary>
  /// <param name="path">The path to check.</param>
  /// <returns><see langword="true"/> if the path ends in a directory separator; otherwise, <see langword="false"/>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsInDirectorySeparator(string? path) {
    if (string.IsNullOrEmpty(path))
      return false;
    var c = path![^1];
    return c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar;
  }

  /// <summary>
  /// Returns a value that indicates whether the specified path ends in a directory separator.
  /// </summary>
  /// <param name="path">The path to check.</param>
  /// <returns><see langword="true"/> if the path ends in a directory separator; otherwise, <see langword="false"/>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool EndsInDirectorySeparator(ReadOnlySpan<char> path) {
    if (path.IsEmpty)
      return false;
    var c = path[^1];
    return c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar;
  }

#endif

#if !SUPPORTS_PATH_TRIMENDINGDIRECTORYSEPARATOR

  /// <summary>
  /// Trims one trailing directory separator character from the specified path.
  /// </summary>
  /// <param name="path">The path to trim.</param>
  /// <returns>The path with any trailing directory separator removed.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string TrimEndingDirectorySeparator(string? path) {
    if (string.IsNullOrEmpty(path))
      return path ?? string.Empty;
    return EndsInDirectorySeparator(path!) && !IsPathRoot(path!)
      ? path![..^1]
      : path!;
  }

  /// <summary>
  /// Trims one trailing directory separator character from the specified path.
  /// </summary>
  /// <param name="path">The path to trim.</param>
  /// <returns>The path with any trailing directory separator removed.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ReadOnlySpan<char> TrimEndingDirectorySeparator(ReadOnlySpan<char> path) {
    if (path.IsEmpty)
      return path;
    return EndsInDirectorySeparator(path) && !IsPathRoot(path)
      ? path[..^1]
      : path;
  }

  private static bool IsPathRoot(string path) {
    // Root paths like "C:\" or "/" should not have their separator trimmed
    if (path.Length <= 1)
      return true;
    // Windows drive root like "C:\" or "C:/"
    if (path.Length == 3 && path[1] == Path.VolumeSeparatorChar)
      return true;
    // UNC root like "\\" or "//"
    if (path.Length == 2 && (path[0] == Path.DirectorySeparatorChar || path[0] == Path.AltDirectorySeparatorChar))
      return path[1] == path[0];
    return false;
  }

  private static bool IsPathRoot(ReadOnlySpan<char> path) => path.Length switch {
    <= 1 => true,
    3 when path[1] == Path.VolumeSeparatorChar => true,
    2 when (path[0] == Path.DirectorySeparatorChar || path[0] == Path.AltDirectorySeparatorChar) => path[1] == path[0],
    _ => false
  };

#endif

#if !SUPPORTS_PATH_ISPATHFULLYQUALIFIED

  /// <summary>
  /// Returns a value that indicates whether a file path is fully qualified.
  /// </summary>
  /// <param name="path">A file path.</param>
  /// <returns><see langword="true"/> if the path is fully qualified; otherwise, <see langword="false"/>.</returns>
  /// <remarks>
  /// This method handles paths that use either forward slash or backslash as separator.
  /// A path is fully qualified if it starts with a drive letter and colon on Windows,
  /// or with a directory separator on any platform.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsPathFullyQualified(string? path) {
    if (string.IsNullOrEmpty(path))
      return false;
    return IsPathFullyQualified(path.AsSpan());
  }

  /// <summary>
  /// Returns a value that indicates whether a file path is fully qualified.
  /// </summary>
  /// <param name="path">A file path.</param>
  /// <returns><see langword="true"/> if the path is fully qualified; otherwise, <see langword="false"/>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsPathFullyQualified(ReadOnlySpan<char> path) {
    if (path.IsEmpty)
      return false;

    // Unix-style rooted path
    if (path[0] == Path.DirectorySeparatorChar || path[0] == Path.AltDirectorySeparatorChar)
      // UNC paths (\\server\share or //server/share) are always fully qualified
      // Single slash is also fully qualified on Unix
      return path.Length == 1 || path[1] != path[0] || true;

    // Windows drive letter path (e.g., C:\ or C:/)
    if (path.Length >= 3 && path[1] == Path.VolumeSeparatorChar && (path[2] == Path.DirectorySeparatorChar || path[2] == Path.AltDirectorySeparatorChar))
      return true;

    return false;
  }

#endif

#if !SUPPORTS_PATH_GETRELATIVEPATH

  /// <summary>
  /// Returns a relative path from one path to another.
  /// </summary>
  /// <param name="relativeTo">The source path the result should be relative to. This path is always considered to be a directory.</param>
  /// <param name="path">The destination path.</param>
  /// <returns>The relative path, or <paramref name="path"/> if the paths don't share the same root.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="relativeTo"/> or <paramref name="path"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentException"><paramref name="relativeTo"/> or <paramref name="path"/> is empty.</exception>
  public static string GetRelativePath(string relativeTo, string path) {
    ArgumentException.ThrowIfNullOrEmpty(relativeTo);
    ArgumentException.ThrowIfNullOrEmpty(path);

    relativeTo = Path.GetFullPath(relativeTo);
    path = Path.GetFullPath(path);

    var separators = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
    var relativeToParts = relativeTo.Split(separators, StringSplitOptions.RemoveEmptyEntries);
    var pathParts = path.Split(separators, StringSplitOptions.RemoveEmptyEntries);

    var commonLength = 0;
    var minLength = Math.Min(relativeToParts.Length, pathParts.Length);

    for (var i = 0; i < minLength; ++i) {
      if (!string.Equals(relativeToParts[i], pathParts[i], StringComparison.OrdinalIgnoreCase))
        break;
      ++commonLength;
    }

    if (commonLength == 0)
      return path;

    var sb = new System.Text.StringBuilder();
    for (var i = commonLength; i < relativeToParts.Length; ++i) {
      if (sb.Length > 0)
        sb.Append(Path.DirectorySeparatorChar);
      sb.Append("..");
    }

    for (var i = commonLength; i < pathParts.Length; ++i) {
      if (sb.Length > 0)
        sb.Append(Path.DirectorySeparatorChar);
      sb.Append(pathParts[i]);
    }

    return sb.Length == 0 ? "." : sb.ToString();
  }

#endif

  }
}
