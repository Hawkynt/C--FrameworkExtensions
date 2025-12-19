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

#if !SUPPORTS_DIRECTORY_CREATETEMPSUBDIRECTORY

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.IO;

public static partial class DirectoryPolyfills {

  extension(Directory) {

    /// <summary>
    /// Creates a uniquely named, empty directory in the specified directory or, if no directory is specified, in the current user's temporary directory.
    /// </summary>
    /// <param name="prefix">The prefix of the directory name, or <see langword="null"/> to use no prefix.</param>
    /// <returns>A <see cref="DirectoryInfo"/> object that represents the newly created directory.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DirectoryInfo CreateTempSubdirectory(string? prefix = null) {
      var tempPath = Path.GetTempPath();
      var uniqueName = (prefix ?? string.Empty) + Guid.NewGuid().ToString("N");
      var fullPath = Path.Combine(tempPath, uniqueName);
      return Directory.CreateDirectory(fullPath);
    }

  }

}

#endif
