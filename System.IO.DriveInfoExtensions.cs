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

using System.Diagnostics.Contracts;

namespace System.IO {
  /// <summary>
  /// Extensions for the DirectoryInfo type.
  /// </summary>
  internal static partial class DriveInfoExtensions {
    /// <summary>
    /// Tests whether the specified drive exists.
    /// </summary>
    /// <param name="This">This DriveInfo.</param>
    /// <returns><c>true</c> when the drive exists; otherwise, <c>false</c>.</returns>
    public static bool Exists(this DriveInfo This) {
      Contract.Requires(This != null);
      try {
        return (This.IsReady || This.DriveType != DriveType.NoRootDirectory);
      } catch {
        return (false);
      }
    }
  }
}
