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

using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Guard;

// ReSharper disable UnusedMember.Global

namespace System.Reflection;

public static class AssemblyExtensions {
  /// <summary>
  ///   Gets the embedded resource file.
  /// </summary>
  /// <param name="this">This Assembly.</param>
  /// <param name="fileName">Name of the file.</param>
  /// <returns>The resource stream.</returns>
  public static Stream GetResourceFileStream(this Assembly @this, string fileName) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(fileName);

    var resourceName = $"{@this
      .EntryPoint
      .DeclaringType
      .Namespace}.{fileName
      .Replace(Path.DirectorySeparatorChar, '.')
      .Replace(Path.AltDirectorySeparatorChar, '.')}";

    return @this.GetManifestResourceStream(resourceName);
  }

  /// <summary>
  ///   Gets the embedded resource stream reader.
  /// </summary>
  /// <param name="this">This Assembly.</param>
  /// <param name="fileName">Name of the file.</param>
  /// <returns>A usable stream reader.</returns>
  public static StreamReader GetResourceStreamReader(this Assembly @this, string fileName) {
    Against.ThisIsNull(@this);

    return new(@this.GetResourceFileStream(fileName));
  }

  /// <summary>
  ///   Gets the embedded resource binary reader.
  /// </summary>
  /// <param name="this">This Assembly.</param>
  /// <param name="fileName">Name of the file.</param>
  /// <returns>A usable binary reader.</returns>
  public static BinaryReader GetResourceBinaryReader(this Assembly @this, string fileName) {
    Against.ThisIsNull(@this);

    return new(@this.GetResourceFileStream(fileName));
  }

  /// <summary>
  ///   Gets the text of an embedded resource.
  /// </summary>
  /// <param name="this">This Assembly.</param>
  /// <param name="fileName">Name of the file.</param>
  /// <returns>All text from the resource.</returns>
  public static string ReadResourceAllText(this Assembly @this, string fileName) {
    Against.ThisIsNull(@this);

    using var reader = @this.GetResourceStreamReader(fileName);
    return reader.ReadToEnd();
  }

  /// <summary>
  ///   Gets the lines of an embedded resource.
  /// </summary>
  /// <param name="this">This Assembly.</param>
  /// <param name="fileName">Name of the file.</param>
  /// <returns>All lines from the resource.</returns>
  public static IEnumerable<string> ReadResourceAllLines(this Assembly @this, string fileName) {
    Against.ThisIsNull(@this);

    return @this
        .ReadResourceAllText(fileName)
        .Lines()
      ;
  }

  /// <summary>
  ///   Gets the bytes of an embedded resource.
  /// </summary>
  /// <param name="this">This Assembly.</param>
  /// <param name="fileName">Name of the file.</param>
  /// <returns>All bytes from the resource.</returns>
  public static byte[] ReadResourceAllBytes(this Assembly @this, string fileName) {
    Against.ThisIsNull(@this);

    using var reader = @this.GetResourceBinaryReader(fileName);
    return reader.ReadAllBytes();
  }

  /// <summary>
  ///   Get the guid from, the assembly attributes or returns the fallabck
  /// </summary>
  /// <param name="this">Assembly to use</param>
  /// <param name="fallbackGuid">Fallback to return if needed</param>
  /// <returns>a valid Guid</returns>
  public static string GetGuidOrFallback(this Assembly @this, string fallbackGuid = null) {
    Against.ThisIsNull(@this);

    var attributes = @this.GetCustomAttributes(typeof(GuidAttribute), true);
    return attributes.Length > 0 ? ((GuidAttribute)attributes[0]).Value : fallbackGuid;
  }
}
