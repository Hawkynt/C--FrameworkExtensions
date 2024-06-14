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

#if !SUPPORTS_MOVETO_OVERWRITE

namespace System.IO;

public static partial class FileInfoPolyfills {
  /// <summary>
  ///   Moves the specified <see cref="FileInfo" /> instance to a new location with an option to overwrite an existing file,
  ///   using a default timeout period for retrying the deletion of the source file if it is locked or cannot be deleted
  ///   immediately.
  /// </summary>
  /// <param name="this">The source <see cref="FileInfo" /> object to move.</param>
  /// <param name="destFileName">The path to the destination file. This cannot be a directory.</param>
  /// <param name="overwrite">
  ///   A <see langword="bool" /> indicating whether to overwrite an existing file at the destination.
  ///   If <see langword="true" />, the file will be overwritten; if <see langword="false" />, an <see cref="IOException" />
  ///   will be thrown
  ///   if a file with the same name already exists at the destination.
  /// </param>
  /// <example>
  ///   <code>
  /// FileInfo sourceFile = new FileInfo(@"C:\source\example.txt");
  /// string destinationPath = @"D:\destination\example.txt";
  /// sourceFile.MoveTo(destinationPath, true);
  /// Console.WriteLine("File moved successfully.");
  /// </code>
  ///   This example demonstrates moving a file from one location to another, with the option to overwrite an existing file
  ///   at the destination.
  /// </example>
  public static void MoveTo(this FileInfo @this, string destFileName, bool overwrite) {
    if (overwrite && File.Exists(destFileName))
      File.Delete(destFileName);

    @this.MoveTo(destFileName);
  }
}

#endif
