#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY

#endregion

using System.IO;

namespace Microsoft.Office.Interop.Word;

public static class DocumentExtensions {
  /// <summary>
  ///   Returns the file that belong to this <see cref="Document" />.
  /// </summary>
  /// <param name="this">The document to get the <see cref="FileInfo" /> from.</param>
  /// <returns>The created <see cref="FileInfo" />.</returns>
  public static FileInfo File(this Document @this) => new(@this.FullName);
  
}