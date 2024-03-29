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

using System.Collections.Generic;
#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace System.Reflection {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static class AssemblyExtensions {
    /// <summary>
    /// Gets the embedded resource file.
    /// </summary>
    /// <param name="This">This Assembly.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>The resource stream.</returns>
    public static Stream GetResourceFileStream(this Assembly This, string fileName) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
      Contract.Requires(fileName != null);
#endif
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
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
      return (new(This.GetResourceFileStream(fileName)));
    }

    /// <summary>
    /// Gets the embedded resource binary reader.
    /// </summary>
    /// <param name="This">This Assembly.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>A usable binary reader.</returns>
    public static BinaryReader GetResourceBinaryReader(this Assembly This, string fileName) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
      return (new(This.GetResourceFileStream(fileName)));
    }

    /// <summary>
    /// Gets the text of an embedded resource.
    /// </summary>
    /// <param name="This">This Assembly.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>All text from the resource.</returns>
    public static string ReadResourceAllText(this Assembly This, string fileName) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
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
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
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
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
      using (var reader = This.GetResourceBinaryReader(fileName))
        return (reader.ReadAllBytes());
    }

    /// <summary>
    /// Get the guid from, the assembly attributes or returns the fallabck
    /// </summary>
    /// <param name="this">Assembly to use</param>
    /// <param name="fallbackGuid">Fallback to return if needed</param>
    /// <returns>a valid Guid</returns>
    public static string GetGuidOrFallback(this Assembly @this, string fallbackGuid = null) {
      var attributes = @this.GetCustomAttributes(typeof(GuidAttribute), true);
      return attributes.Length > 0 ? ((GuidAttribute)attributes[0]).Value : fallbackGuid;
    }
  }
}