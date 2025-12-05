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
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.IO;

public static partial class PathExtensions {
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

    var c = path1[path1.Length - 1];
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
    if (paths == null || paths.Length == 0)
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

#if !SUPPORTS_PATH_GETRELATIVEPATH

  /// <summary>
  /// Returns a relative path from one path to another.
  /// </summary>
  /// <param name="relativeTo">The source path the result should be relative to. This path is always considered to be a directory.</param>
  /// <param name="path">The destination path.</param>
  /// <returns>The relative path, or <paramref name="path"/> if the paths don't share the same root.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="relativeTo"/> or <paramref name="path"/> is <see langword="null"/>.</exception>
  public static string GetRelativePath(string relativeTo, string path) {
    if (string.IsNullOrEmpty(relativeTo))
      AlwaysThrow.ArgumentNullException(nameof(relativeTo));
    if (string.IsNullOrEmpty(path))
      AlwaysThrow.ArgumentNullException(nameof(path));

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
