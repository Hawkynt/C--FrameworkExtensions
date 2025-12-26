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

#if !SUPPORTS_PATH_COMBINE_ARRAYS

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.IO;

public static partial class PathPolyfills {
  extension(Path) {

    /// <summary>
    /// Combines three strings into a path.
    /// </summary>
    /// <param name="path1">The first path to combine.</param>
    /// <param name="path2">The second path to combine.</param>
    /// <param name="path3">The third path to combine.</param>
    /// <returns>The combined paths.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="path1"/>, <paramref name="path2"/>, or <paramref name="path3"/> contains
    /// one or more of the invalid characters defined in <see cref="Path.GetInvalidPathChars"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="path1"/>, <paramref name="path2"/>, or <paramref name="path3"/> is <see langword="null"/>.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Combine(string path1, string path2, string path3)
      => Path.Combine(Path.Combine(path1, path2), path3);

    /// <summary>
    /// Combines four strings into a path.
    /// </summary>
    /// <param name="path1">The first path to combine.</param>
    /// <param name="path2">The second path to combine.</param>
    /// <param name="path3">The third path to combine.</param>
    /// <param name="path4">The fourth path to combine.</param>
    /// <returns>The combined paths.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="path1"/>, <paramref name="path2"/>, <paramref name="path3"/>, or <paramref name="path4"/> contains
    /// one or more of the invalid characters defined in <see cref="Path.GetInvalidPathChars"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="path1"/>, <paramref name="path2"/>, <paramref name="path3"/>, or <paramref name="path4"/> is <see langword="null"/>.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Combine(string path1, string path2, string path3, string path4)
      => Path.Combine(Path.Combine(path1, path2), Path.Combine(path3, path4));

    /// <summary>
    /// Combines an array of strings into a path.
    /// </summary>
    /// <param name="paths">An array of parts of the path.</param>
    /// <returns>The combined paths.</returns>
    /// <exception cref="ArgumentException">
    /// One of the strings in the array contains one or more of the invalid characters defined in <see cref="Path.GetInvalidPathChars"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// One of the strings in the array is <see langword="null"/>.
    /// </exception>
    public static string Combine(params string[] paths) {
      ArgumentNullException.ThrowIfNull(paths);

      if (paths.Length == 0)
        return string.Empty;

      var result = paths[0];
      for (var i = 1; i < paths.Length; ++i)
        result = Path.Combine(result, paths[i]);

      return result;
    }

  }
}

#endif
