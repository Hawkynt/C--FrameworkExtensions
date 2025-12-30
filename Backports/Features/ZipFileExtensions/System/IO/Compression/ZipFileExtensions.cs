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

#if !SUPPORTS_ZIPARCHIVE

using Guard;

namespace System.IO.Compression;

/// <summary>
/// Provides extension methods for the <see cref="ZipArchive"/> and <see cref="ZipArchiveEntry"/> classes.
/// </summary>
public static class ZipFileExtensionsPolyfills {
  /// <param name="destination">The zip archive to add the file to.</param>
  extension(ZipArchive destination)
  {
    /// <summary>
    /// Archives a file by compressing it and adding it to the zip archive.
    /// </summary>
    /// <param name="sourceFileName">The path to the file to be archived.</param>
    /// <param name="entryName">The name of the entry to create in the zip archive.</param>
    /// <returns>A wrapper for the new entry in the zip archive.</returns>
    public ZipArchiveEntry CreateEntryFromFile(string sourceFileName, string entryName)
      => CreateEntryFromFile(destination, sourceFileName, entryName, CompressionLevel.Optimal);

    /// <summary>
    /// Archives a file by compressing it using the specified compression level and adding it to the zip archive.
    /// </summary>
    /// <param name="sourceFileName">The path to the file to be archived.</param>
    /// <param name="entryName">The name of the entry to create in the zip archive.</param>
    /// <param name="compressionLevel">One of the enumeration values that indicates whether to emphasize speed or compression effectiveness when creating the entry.</param>
    /// <returns>A wrapper for the new entry in the zip archive.</returns>
    public ZipArchiveEntry CreateEntryFromFile(string sourceFileName, string entryName, CompressionLevel compressionLevel) {
      Against.ArgumentIsNull(destination);
      Against.ArgumentIsNull(sourceFileName);
      Against.ArgumentIsNull(entryName);

      sourceFileName = Path.GetFullPath(sourceFileName);

      if (!File.Exists(sourceFileName))
        throw new FileNotFoundException($"Could not find file '{sourceFileName}'.", sourceFileName);

      var entry = destination.CreateEntry(entryName, compressionLevel);

      // Copy file attributes
      var fileInfo = new FileInfo(sourceFileName);
      entry.LastWriteTime = fileInfo.LastWriteTime;

      // Copy file content
      using var source = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
      using var entryStream = entry.Open();
      source.CopyTo(entryStream);

      return entry;
    }

    /// <summary>
    /// Extracts all the files in the zip archive to a directory on the file system.
    /// </summary>
    /// <param name="destinationDirectoryName">The path to the directory to place the extracted files in.</param>
    public void ExtractToDirectory(string destinationDirectoryName)
      => destination.ExtractToDirectory(destinationDirectoryName, overwriteFiles: false);
  }

  /// <param name="source">The zip archive entry to extract.</param>
  extension(ZipArchiveEntry source)
  {
    /// <summary>
    /// Extracts an entry in the zip archive to a file.
    /// </summary>
    /// <param name="destinationFileName">The path of the file to create from the contents of the entry.</param>
    public void ExtractToFile(string destinationFileName)
      => source.ExtractToFile(destinationFileName, overwrite: false);

    /// <summary>
    /// Extracts an entry in the zip archive to a file, and optionally overwrites an existing file that has the same name.
    /// </summary>
    /// <param name="destinationFileName">The path of the file to create from the contents of the entry.</param>
    /// <param name="overwrite">true to overwrite an existing file that has the same name as the destination file; otherwise, false.</param>
    public void ExtractToFile(string destinationFileName, bool overwrite) {
      Against.ArgumentIsNull(source);
      Against.ArgumentIsNull(destinationFileName);

      destinationFileName = Path.GetFullPath(destinationFileName);

      var fileMode = overwrite ? FileMode.Create : FileMode.CreateNew;

      var directory = Path.GetDirectoryName(destinationFileName);
      if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        Directory.CreateDirectory(directory);

      using (var entryStream = source.Open())
      using (var fileStream = new FileStream(destinationFileName, fileMode, FileAccess.Write, FileShare.None))
        entryStream.CopyTo(fileStream);

      File.SetLastWriteTime(destinationFileName, source.LastWriteTime.DateTime);
    }
  }
}

#endif
