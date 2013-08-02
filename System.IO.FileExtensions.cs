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
using System.Runtime.InteropServices;
namespace System.IO {
  internal static partial class FileExtensions {
    [DllImport("kernel32.dll")]
    private static extern int DeviceIoControl(
      IntPtr hDevice,
      int dwIoControlCode,
      ref short lpInBuffer,
      int nInBufferSize,
      IntPtr lpOutBuffer,
      int nOutBufferSize,
      ref int lpBytesReturned,
      IntPtr lpOverlapped
      );

    private const int FSCTL_SET_COMPRESSION = 0x9C040;

    public static bool EnableCompression(string fileName) {
      if (!File.Exists(fileName))
        throw new FileNotFoundException(fileName);
      short COMPRESSION_FORMAT_DEFAULT = 1;

      var lpBytesReturned = 0;
      int result;
      using (var f = File.Open(fileName, FileMode.Open, FileAccess.ReadWrite, IO.FileShare.None)) {
        result = DeviceIoControl(
          f.Handle,
          FSCTL_SET_COMPRESSION,
          ref COMPRESSION_FORMAT_DEFAULT,
          2 /*sizeof(short)*/,
          IntPtr.Zero,
          0,
          ref lpBytesReturned,
          IntPtr.Zero);
      }
      return (result >= 0 && result <= 0x7FFFFFFF);
    }
  }
}