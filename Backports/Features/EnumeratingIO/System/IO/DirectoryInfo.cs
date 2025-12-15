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

#if !SUPPORTS_ENUMERATING_IO

using Guard;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.IO;

public static partial class DirectoryInfoPolyfills {
  
  extension(DirectoryInfo @this)
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<FileSystemInfo> EnumerateFileSystemInfos() {
      Against.ThisIsNull(@this);

      return Invoke(@this);

      static IEnumerable<FileSystemInfo> Invoke(DirectoryInfo @this) {
        foreach (var entry in @this.GetFileSystemInfos())
          yield return entry;
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<FileInfo> EnumerateFiles() {
      Against.ThisIsNull(@this);

      return Invoke(@this);

      static IEnumerable<FileInfo> Invoke(DirectoryInfo @this) {
        foreach (var entry in @this.GetFiles())
          yield return entry;
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<DirectoryInfo> EnumerateDirectories() {
      Against.ThisIsNull(@this);

      return Invoke(@this);

      static IEnumerable<DirectoryInfo> Invoke(DirectoryInfo @this) {
        foreach (var entry in @this.GetDirectories())
          yield return entry;
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<FileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption) {
      Against.ThisIsNull(@this);

      return Invoke(@this, searchPattern, searchOption);

      static IEnumerable<FileInfo> Invoke(DirectoryInfo @this, string searchPattern, SearchOption searchOption) {
        foreach (var entry in @this.GetFiles(searchPattern, searchOption))
          yield return entry;
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<DirectoryInfo> EnumerateDirectories(string searchPattern, SearchOption searchOption) {
      Against.ThisIsNull(@this);

      return Invoke(@this, searchPattern, searchOption);

      static IEnumerable<DirectoryInfo> Invoke(DirectoryInfo @this, string searchPattern, SearchOption searchOption) {
        foreach (var entry in @this.GetDirectories(searchPattern, searchOption))
          yield return entry;
      }
    }
  }
}

#endif
