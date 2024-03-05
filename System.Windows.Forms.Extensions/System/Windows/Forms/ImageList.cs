#region (c)2010-2042 Hawkynt

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

using System.IO;
using System.Linq;
#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace System.Windows.Forms;

#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  static partial class ImageListExtensions {
  public static void SaveToDirectory(this ImageList @this, string directoryName) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
#endif
    var images = @this.Images;
    foreach (var image in from i in Enumerable.Range(0, images.Count) select Tuple.Create(i, images[i], images.Keys[i]))
      image.Item2.Save(image.Item3 + ".png");
  }

  public static void SaveToDirectory(this ImageList @this, DirectoryInfo directory) =>
    SaveToDirectory(@this, directory.FullName);
}
