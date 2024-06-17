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

using System.Runtime.InteropServices;

namespace System.Windows.Forms;

public static partial class TreeViewExtensions {
  private sealed partial class DragDropInstance {
    private static class NativeMethods {
      [DllImport("comctl32.dll")]
      private static extern bool InitCommonControls();

      [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
      public static extern bool ImageList_BeginDrag(IntPtr himlTrack, int iTrack, int dxHotspot, int dyHotspot);

      [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
      public static extern bool ImageList_DragMove(int x, int y);

      [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
      public static extern void ImageList_EndDrag();

      [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
      public static extern bool ImageList_DragEnter(IntPtr hwndLock, int x, int y);

      [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
      public static extern bool ImageList_DragLeave(IntPtr hwndLock);

      /// <summary>
      ///   Shows or hides the drag image.
      /// </summary>
      /// <param name="fShow">if set to <c>true</c> the image will be shown; otherwise, it will be hidden.</param>
      /// <returns></returns>
      [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
      public static extern bool ImageList_DragShowNolock(bool fShow);

      static NativeMethods() => InitCommonControls();
    }
  }
}
