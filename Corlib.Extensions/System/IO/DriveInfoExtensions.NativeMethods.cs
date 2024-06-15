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

using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.IO;

public static partial class DriveInfoExtensions {
  private static class NativeMethods {
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto, EntryPoint = "GetDiskFreeSpace")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool _GetDiskFreeSpace(
      string lpRootPathName,
      out uint lpSectorsPerCluster,
      out uint lpBytesPerSector,
      out uint lpNumberOfFreeClusters,
      out uint lpTotalNumberOfClusters
    );

    public static void GetDiskFreeSpace(string rootPath, out uint sectorsPerCluster, out uint bytesPerSector, out uint numberOfFreeClusters, out uint totalNumberOfClusters) {
      if (!_GetDiskFreeSpace(rootPath, out sectorsPerCluster, out bytesPerSector, out numberOfFreeClusters, out totalNumberOfClusters))
        throw new Win32Exception();
    }
  }
}
