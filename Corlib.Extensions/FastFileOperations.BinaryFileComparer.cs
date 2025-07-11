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
using System.Linq;
using System.Runtime.InteropServices;
using Guard;

namespace System.IO;
public partial class FastFileOperations {
  private static partial class NativeMethods {
    [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int memcmp(byte[] b1, byte[] b2, long count);
  }

  /// <summary>
  ///   Compares two files, byte by byte if necessary.
  /// </summary>
  public class BinaryFileComparer : IFileComparer {
    #region Implementation of IEqualityComparer<in FileInfo>

    public bool Equals(FileInfo x, FileInfo y) {
      Against.ArgumentIsNull(x);
      Against.ArgumentIsNull(y);

      // same file
      if (x.FullName == y.FullName)
        return true;

      // unequal lengths
      var length = x.Length;
      if (length != y.Length)
        return false;

      const int __COMPARE_BUFFER_LENGTH = 8 * 1024 * 1024;
      var bufferX = new byte[__COMPARE_BUFFER_LENGTH];
      var bufferY = new byte[__COMPARE_BUFFER_LENGTH];
      var chunks = length / __COMPARE_BUFFER_LENGTH;
      List<long> scanFirstChunks = [0];
      if (chunks > 1)
        scanFirstChunks.Insert(0, (chunks - 1) * __COMPARE_BUFFER_LENGTH);
      if (chunks > 2)
        scanFirstChunks.Add((chunks >> 1) * __COMPARE_BUFFER_LENGTH);

      using FileStream xStream = new(x.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, __COMPARE_BUFFER_LENGTH);
      using FileStream yStream = new(y.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, __COMPARE_BUFFER_LENGTH);

      // first direct scan
      if (scanFirstChunks.Any(offset => _ReadAndCompareTrueWhenDifferent(xStream, yStream, offset, bufferX, bufferY, __COMPARE_BUFFER_LENGTH)))
        return false;

      // full scan
      for (long offset = __COMPARE_BUFFER_LENGTH; offset < length; offset += __COMPARE_BUFFER_LENGTH) {
        if (scanFirstChunks.Contains(offset))
          continue;

        if (_ReadAndCompareTrueWhenDifferent(xStream, yStream, offset, bufferX, bufferY, __COMPARE_BUFFER_LENGTH))
          return false;
      }

      // scanned every byte, they must be equal here
      return true;
    }

    /// <summary>
    ///   Reads a block of both streams into the buffers and compares.
    /// </summary>
    /// <param name="xStream">The x stream.</param>
    /// <param name="yStream">The y stream.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="bufferX">The buffer for x.</param>
    /// <param name="bufferY">The buffer for y.</param>
    /// <param name="compareBufferLength">Length of the compare buffers.</param>
    /// <returns><c>true</c> when buffer contents do not match; otherwise, <c>false</c>.</returns>
    private static bool _ReadAndCompareTrueWhenDifferent(FileStream xStream, FileStream yStream, long offset, byte[] bufferX, byte[] bufferY, int compareBufferLength) {
      xStream.Position = offset;
      yStream.Position = offset;
      var len = xStream.Read(bufferX, 0, compareBufferLength);
      var len2 = yStream.Read(bufferY, 0, compareBufferLength);
      return len != len2 || NativeMethods.memcmp(bufferX, bufferY, len) != 0;
    }

    public int GetHashCode(FileInfo obj) => throw new NotImplementedException();

    #endregion
  }

  /// <summary>
  ///   Compares two files by their length.
  /// </summary>
  public class FileLengthComparer : IFileComparer {
    #region Implementation of IEqualityComparer<in FileInfo>

    public bool Equals(FileInfo x, FileInfo y) {
      Against.ArgumentIsNull(x);
      Against.ArgumentIsNull(y);

      return x.Length == y.Length;
    }

    public int GetHashCode(FileInfo obj) => throw new NotImplementedException();

    #endregion
  }

  /// <summary>
  ///   Compares two files by their basic attributes (HARS)
  /// </summary>
  public class FileSimpleAttributesComparer : IFileComparer {
    #region Implementation of IEqualityComparer<in FileInfo>

    public bool Equals(FileInfo x, FileInfo y) {
      Against.ArgumentIsNull(x);
      Against.ArgumentIsNull(y);

      const FileAttributes MASK = FileAttributes.Archive | FileAttributes.Hidden | FileAttributes.ReadOnly | FileAttributes.System;
      return (x.Attributes & MASK) == (y.Attributes & MASK);
    }

    public int GetHashCode(FileInfo obj) => throw new NotImplementedException();

    #endregion
  }

  /// <summary>
  ///   Compares two files by their creation time.
  /// </summary>
  public class FileCreationTimeComparer : IFileComparer {
    #region Implementation of IEqualityComparer<in FileInfo>

    public bool Equals(FileInfo x, FileInfo y) {
      Against.ArgumentIsNull(x);
      Against.ArgumentIsNull(y);

      return x.CreationTimeUtc == y.CreationTimeUtc;
    }

    public int GetHashCode(FileInfo obj) => throw new NotImplementedException();

    #endregion
  }

  /// <summary>
  ///   Comapres two files by their last write time.
  /// </summary>
  public class FileLastWriteTimeComparer : IFileComparer {
    #region Implementation of IEqualityComparer<in FileInfo>

    public bool Equals(FileInfo x, FileInfo y) {
      Against.ArgumentIsNull(x);
      Against.ArgumentIsNull(y);

      return x.LastWriteTimeUtc == y.LastWriteTimeUtc;
    }

    public int GetHashCode(FileInfo obj) => throw new NotImplementedException();

    #endregion
  }
}
