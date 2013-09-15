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
using System.IO;
using System.Linq;

namespace System.Reflection {
  internal static class AssemblyExtensions {
    /// <summary>
    /// Gets the embedded resource file.
    /// </summary>
    /// <param name="This">This Assembly.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>The resource stream.</returns>
    public static Stream GetResourceFileStream(this Assembly This, string fileName) {
      Contract.Requires(This != null);
      Contract.Requires(fileName != null);
      var resourceName = string.Format(
        "{0}.{1}",
        This
          .EntryPoint
          .DeclaringType
          .Namespace,
        fileName
          .Replace(Path.DirectorySeparatorChar, '.')
          .Replace(Path.AltDirectorySeparatorChar, '.')
      );

      return (This.GetManifestResourceStream(resourceName));
    }

    /// <summary>
    /// Gets the embedded resource stream reader.
    /// </summary>
    /// <param name="This">This Assembly.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>A usable stream reader.</returns>
    public static StreamReader GetResourceStreamReader(this Assembly This, string fileName) {
      Contract.Requires(This != null);
      return (new StreamReader(This.GetResourceFileStream(fileName)));
    }

    /// <summary>
    /// Gets the embedded resource binary reader.
    /// </summary>
    /// <param name="This">This Assembly.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>A usable binary reader.</returns>
    public static BinaryReader GetResourceBinaryReader(this Assembly This, string fileName) {
      Contract.Requires(This != null);
      return (new BinaryReader(This.GetResourceFileStream(fileName)));
    }

    /// <summary>
    /// Gets the text of an embedded resource.
    /// </summary>
    /// <param name="This">This Assembly.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>All text from the resource.</returns>
    public static string ReadResourceAllText(this Assembly This, string fileName) {
      Contract.Requires(This != null);
      using (var reader = This.GetResourceStreamReader(fileName))
        return (reader.ReadToEnd());
    }

    /// <summary>
    /// Gets the lines of an embedded resource.
    /// </summary>
    /// <param name="This">This Assembly.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>All lines from the resource.</returns>
    public static IEnumerable<string> ReadResourceAllLines(this Assembly This, string fileName) {
      Contract.Requires(This != null);
      return (
        This
        .ReadResourceAllText(fileName)
        .Split(new[] { "\r\n" }, StringSplitOptions.None)
        .SelectMany(l => l.Split(new[] { "\n\r" }, StringSplitOptions.None))
        .SelectMany(l => l.Split(new[] { "\r" }, StringSplitOptions.None))
        .SelectMany(l => l.Split(new[] { "\n" }, StringSplitOptions.None))
      );
    }

    /// <summary>
    /// Gets the bytes of an embedded resource.
    /// </summary>
    /// <param name="This">This Assembly.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>All bytes from the resource.</returns>
    public static byte[] ReadResourceAllBytes(this Assembly This, string fileName) {
      Contract.Requires(This != null);
      using (var reader = This.GetResourceBinaryReader(fileName))
        return (reader.ReadAllBytes());
    }
  }
}