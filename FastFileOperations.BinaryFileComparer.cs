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
using System.Linq;
using System.Runtime.InteropServices;

namespace System.IO {
  partial class FastFileOperations {

    private static partial class NativeMethods {
      [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
      public static extern int memcmp(byte[] b1, byte[] b2, long count);
    }

    /// <summary>
    /// Compares two files, byte by byte if necessary.
    /// </summary>
    public class BinaryFileComparer : IFileComparer {

      #region Implementation of IEqualityComparer<in FileInfo>

      public bool Equals(FileInfo x, FileInfo y) {
        Contract.Requires(x != null && y != null);

        // same file
        if (x.FullName == y.FullName)
          return (true);

        // unequal lengths
        var length = x.Length;
        if (length != y.Length)
          return (false);

        const int __COMPARE_BUFFER_LENGTH = 8 * 1024 * 1024;
        var bufferX = new byte[__COMPARE_BUFFER_LENGTH];
        var bufferY = new byte[__COMPARE_BUFFER_LENGTH];
        var chunks = length / __COMPARE_BUFFER_LENGTH;
        var scanFirstChunks = new List<long> { 0 };
        if (chunks > 1)
          scanFirstChunks.Insert(0, (chunks - 1) * __COMPARE_BUFFER_LENGTH);
        if (chunks > 2)
          scanFirstChunks.Add((chunks >> 1) * __COMPARE_BUFFER_LENGTH);

        using (var xStream = x.Open(FileMode.Open, FileAccess.Read, FileShare.Read, __COMPARE_BUFFER_LENGTH))
        using (var yStream = y.Open(FileMode.Open, FileAccess.Read, FileShare.Read, __COMPARE_BUFFER_LENGTH)) {

          // first direct scan
          if (scanFirstChunks.Any(offset => _ReadAndCompareTrueWhenDifferent(xStream, yStream, offset, bufferX, bufferY, __COMPARE_BUFFER_LENGTH)))
            return (false);

          // full scan
          for (long offset = __COMPARE_BUFFER_LENGTH; offset < length; offset += __COMPARE_BUFFER_LENGTH) {
            if (scanFirstChunks.Contains(offset))
              continue;

            if (_ReadAndCompareTrueWhenDifferent(xStream, yStream, offset, bufferX, bufferY, __COMPARE_BUFFER_LENGTH))
              return (false);
          }

          // scanned every byte, they must be equal here
          return (true);
        }

      }

      /// <summary>
      /// Reads a block of both streams into the buffers and compares.
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
        yStream.Read(bufferY, 0, compareBufferLength);
        return (NativeMethods.memcmp(bufferX, bufferY, len) != 0);
      }

      public int GetHashCode(FileInfo obj) {
        throw new NotImplementedException();
      }

      #endregion
    }

    /// <summary>
    /// Compares two files by their length.
    /// </summary>
    public class FileLengthComparer : IFileComparer {
      #region Implementation of IEqualityComparer<in FileInfo>

      public bool Equals(FileInfo x, FileInfo y) {
        Contract.Requires(x != null && y != null);
        return (x.Length == y.Length);
      }

      public int GetHashCode(FileInfo obj) {
        throw new NotImplementedException();
      }

      #endregion
    }

    /// <summary>
    /// Compares two files by their basic attributes (HARS)
    /// </summary>
    public class FileSimpleAttributesComparer : IFileComparer {
      #region Implementation of IEqualityComparer<in FileInfo>

      public bool Equals(FileInfo x, FileInfo y) {
        Contract.Requires(x != null && y != null);
        const FileAttributes MASK = FileAttributes.Archive | FileAttributes.Hidden | FileAttributes.ReadOnly | FileAttributes.System;
        return ((x.Attributes & MASK) == (y.Attributes & MASK));
      }

      public int GetHashCode(FileInfo obj) {
        throw new NotImplementedException();
      }

      #endregion
    }

    /// <summary>
    /// Compares two files by their creation time.
    /// </summary>
    public class FileCreationTimeComparer : IFileComparer {
      #region Implementation of IEqualityComparer<in FileInfo>

      public bool Equals(FileInfo x, FileInfo y) {
        Contract.Requires(x != null && y != null);
        return (x.CreationTimeUtc == y.CreationTimeUtc);
      }

      public int GetHashCode(FileInfo obj) {
        throw new NotImplementedException();
      }

      #endregion
    }

    /// <summary>
    /// Comapres two files by their last write time.
    /// </summary>
    public class FileLastWriteTimeComparer : IFileComparer {
      #region Implementation of IEqualityComparer<in FileInfo>

      public bool Equals(FileInfo x, FileInfo y) {
        Contract.Requires(x != null && y != null);
        return (x.LastWriteTimeUtc == y.LastWriteTimeUtc);
      }

      public int GetHashCode(FileInfo obj) {
        throw new NotImplementedException();
      }

      #endregion
    }

  }
}
