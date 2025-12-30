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

using System.Text;
using Guard;

namespace System.IO.Compression {

/// <summary>
/// Provides static methods for creating, extracting, and opening zip archives.
/// </summary>
public static class ZipFile {
  #region CreateFromDirectory

  /// <summary>
  /// Creates a zip archive that contains the files and directories from the specified directory.
  /// </summary>
  /// <param name="sourceDirectoryName">The path to the directory to be archived, specified as a relative or absolute path.</param>
  /// <param name="destinationArchiveFileName">The path of the archive to be created, specified as a relative or absolute path.</param>
  public static void CreateFromDirectory(string sourceDirectoryName, string destinationArchiveFileName)
    => CreateFromDirectory(sourceDirectoryName, destinationArchiveFileName, CompressionLevel.Optimal, includeBaseDirectory: false);

  /// <summary>
  /// Creates a zip archive that contains the files and directories from the specified directory, uses the specified compression level, and optionally includes the base directory.
  /// </summary>
  /// <param name="sourceDirectoryName">The path to the directory to be archived, specified as a relative or absolute path.</param>
  /// <param name="destinationArchiveFileName">The path of the archive to be created, specified as a relative or absolute path.</param>
  /// <param name="compressionLevel">One of the enumeration values that indicates whether to emphasize speed or compression effectiveness when creating the entry.</param>
  /// <param name="includeBaseDirectory">true to include the directory name from sourceDirectoryName at the root of the archive; false to include only the contents of the directory.</param>
  public static void CreateFromDirectory(string sourceDirectoryName, string destinationArchiveFileName, CompressionLevel compressionLevel, bool includeBaseDirectory)
    => CreateFromDirectory(sourceDirectoryName, destinationArchiveFileName, compressionLevel, includeBaseDirectory, entryNameEncoding: null);

  /// <summary>
  /// Creates a zip archive that contains the files and directories from the specified directory, uses the specified compression level and character encoding for entry names, and optionally includes the base directory.
  /// </summary>
  /// <param name="sourceDirectoryName">The path to the directory to be archived, specified as a relative or absolute path.</param>
  /// <param name="destinationArchiveFileName">The path of the archive to be created, specified as a relative or absolute path.</param>
  /// <param name="compressionLevel">One of the enumeration values that indicates whether to emphasize speed or compression effectiveness when creating the entry.</param>
  /// <param name="includeBaseDirectory">true to include the directory name from sourceDirectoryName at the root of the archive; false to include only the contents of the directory.</param>
  /// <param name="entryNameEncoding">The encoding to use when reading or writing entry names in this archive.</param>
  public static void CreateFromDirectory(string sourceDirectoryName, string destinationArchiveFileName, CompressionLevel compressionLevel, bool includeBaseDirectory, Encoding? entryNameEncoding) {
    Against.ArgumentIsNull(sourceDirectoryName);
    Against.ArgumentIsNull(destinationArchiveFileName);

    sourceDirectoryName = Path.GetFullPath(sourceDirectoryName);
    destinationArchiveFileName = Path.GetFullPath(destinationArchiveFileName);

    if (!Directory.Exists(sourceDirectoryName))
      throw new DirectoryNotFoundException($"Could not find a part of the path '{sourceDirectoryName}'.");

    // Ensure destination directory exists
    var destinationDir = Path.GetDirectoryName(destinationArchiveFileName);
    if (!string.IsNullOrEmpty(destinationDir) && !Directory.Exists(destinationDir))
      Directory.CreateDirectory(destinationDir);

    using var fileStream = new FileStream(destinationArchiveFileName, FileMode.CreateNew, FileAccess.Write, FileShare.None);
    using var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, leaveOpen: false, entryNameEncoding);

    var basePath = includeBaseDirectory ? Path.GetFileName(sourceDirectoryName) + "/" : string.Empty;

    // Add all files
    foreach (var filePath in Directory.GetFiles(sourceDirectoryName, "*", SearchOption.AllDirectories)) {
      var relativePath = _GetRelativePath(sourceDirectoryName, filePath);
      var entryName = basePath + relativePath.Replace(Path.DirectorySeparatorChar, '/');
      archive.CreateEntryFromFile(filePath, entryName, compressionLevel);
    }

    // Add empty directories
    foreach (var dirPath in Directory.GetDirectories(sourceDirectoryName, "*", SearchOption.AllDirectories))
      if (Directory.GetFiles(dirPath).Length == 0 && Directory.GetDirectories(dirPath).Length == 0) {
        var relativePath = _GetRelativePath(sourceDirectoryName, dirPath);
        var entryName = basePath + relativePath.Replace(Path.DirectorySeparatorChar, '/') + "/";
        archive.CreateEntry(entryName);
      }
  }

  #endregion

  #region ExtractToDirectory

  /// <summary>
  /// Extracts all the files in the specified zip archive to a directory on the file system.
  /// </summary>
  /// <param name="sourceArchiveFileName">The path to the archive that is to be extracted.</param>
  /// <param name="destinationDirectoryName">The path to the directory in which to place the extracted files, specified as a relative or absolute path.</param>
  public static void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName)
    => ExtractToDirectory(sourceArchiveFileName, destinationDirectoryName, entryNameEncoding: null);

  /// <summary>
  /// Extracts all the files in the specified zip archive to a directory on the file system and uses the specified character encoding for entry names.
  /// </summary>
  /// <param name="sourceArchiveFileName">The path to the archive that is to be extracted.</param>
  /// <param name="destinationDirectoryName">The path to the directory in which to place the extracted files, specified as a relative or absolute path.</param>
  /// <param name="entryNameEncoding">The encoding to use when reading entry names in this archive.</param>
  public static void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName, Encoding? entryNameEncoding) {
    Against.ArgumentIsNull(sourceArchiveFileName);
    Against.ArgumentIsNull(destinationDirectoryName);

    sourceArchiveFileName = Path.GetFullPath(sourceArchiveFileName);
    destinationDirectoryName = Path.GetFullPath(destinationDirectoryName);

    if (!File.Exists(sourceArchiveFileName))
      throw new FileNotFoundException($"Could not find file '{sourceArchiveFileName}'.", sourceArchiveFileName);

    using var archive = Open(sourceArchiveFileName, ZipArchiveMode.Read, entryNameEncoding);
    archive.ExtractToDirectory(destinationDirectoryName);
  }

  #endregion

  #region Open

  /// <summary>
  /// Opens a zip archive at the specified path and in the specified mode.
  /// </summary>
  /// <param name="archiveFileName">The path to the archive to open, specified as a relative or absolute path.</param>
  /// <param name="mode">One of the enumeration values that specifies the actions that are allowed on the entries in the opened archive.</param>
  /// <returns>The opened zip archive.</returns>
  public static ZipArchive Open(string archiveFileName, ZipArchiveMode mode)
    => Open(archiveFileName, mode, entryNameEncoding: null);

  /// <summary>
  /// Opens a zip archive at the specified path, in the specified mode, and by using the specified character encoding for entry names.
  /// </summary>
  /// <param name="archiveFileName">The path to the archive to open, specified as a relative or absolute path.</param>
  /// <param name="mode">One of the enumeration values that specifies the actions that are allowed on the entries in the opened archive.</param>
  /// <param name="entryNameEncoding">The encoding to use when reading or writing entry names in this archive.</param>
  /// <returns>The opened zip archive.</returns>
  public static ZipArchive Open(string archiveFileName, ZipArchiveMode mode, Encoding? entryNameEncoding) {
    Against.ArgumentIsNull(archiveFileName);

    archiveFileName = Path.GetFullPath(archiveFileName);

    FileMode fileMode;
    FileAccess fileAccess;
    FileShare fileShare;

    switch (mode) {
      case ZipArchiveMode.Read:
        fileMode = FileMode.Open;
        fileAccess = FileAccess.Read;
        fileShare = FileShare.Read;
        break;
      case ZipArchiveMode.Create:
        fileMode = FileMode.CreateNew;
        fileAccess = FileAccess.Write;
        fileShare = FileShare.None;
        break;
      case ZipArchiveMode.Update:
        fileMode = FileMode.OpenOrCreate;
        fileAccess = FileAccess.ReadWrite;
        fileShare = FileShare.None;
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(mode));
    }

    FileStream? fileStream = null;
    try {
      fileStream = new(archiveFileName, fileMode, fileAccess, fileShare);
      return new(fileStream, mode, leaveOpen: false, entryNameEncoding);
    } catch {
      fileStream?.Dispose();
      throw;
    }
  }

  /// <summary>
  /// Opens a zip archive for reading at the specified path.
  /// </summary>
  /// <param name="archiveFileName">The path to the archive to open, specified as a relative or absolute path.</param>
  /// <returns>The opened zip archive.</returns>
  public static ZipArchive OpenRead(string archiveFileName) => Open(archiveFileName, ZipArchiveMode.Read, entryNameEncoding: null);

  #endregion

  #region Helpers

  private static string _GetRelativePath(string basePath, string fullPath) {
    if (!basePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
      basePath += Path.DirectorySeparatorChar;

    return fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase) ? fullPath[basePath.Length..] : fullPath;
  }

  #endregion
}

}

#endif

#if !SUPPORTS_ZIPFILE_EXTRACT_OVERWRITE

namespace System.IO.Compression {

public static partial class ZipFilePolyfills {

  extension(ZipFile) {

    /// <summary>
    /// Extracts all the files in the specified archive to a directory on the file system.
    /// </summary>
    /// <param name="sourceArchiveFileName">The path on the file system to the archive that is to be extracted.</param>
    /// <param name="destinationDirectoryName">The path to the destination directory on the file system.</param>
    /// <param name="overwriteFiles">true to overwrite existing files; false otherwise.</param>
    public static void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName, bool overwriteFiles)
      => ExtractToDirectory(sourceArchiveFileName, destinationDirectoryName, entryNameEncoding: null, overwriteFiles);

    /// <summary>
    /// Extracts all the files in the specified archive to a directory on the file system.
    /// </summary>
    /// <param name="sourceArchiveFileName">The path on the file system to the archive that is to be extracted.</param>
    /// <param name="destinationDirectoryName">The path to the destination directory on the file system.</param>
    /// <param name="entryNameEncoding">The encoding to use when reading entry names in this archive.</param>
    /// <param name="overwriteFiles">true to overwrite existing files; false otherwise.</param>
    public static void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName, global::System.Text.Encoding? entryNameEncoding, bool overwriteFiles) {
      Guard.Against.ArgumentIsNull(sourceArchiveFileName);
      Guard.Against.ArgumentIsNull(destinationDirectoryName);

      sourceArchiveFileName = IO.Path.GetFullPath(sourceArchiveFileName);
      destinationDirectoryName = IO.Path.GetFullPath(destinationDirectoryName);

      if (!IO.File.Exists(sourceArchiveFileName))
        throw new IO.FileNotFoundException($"Could not find file '{sourceArchiveFileName}'.", sourceArchiveFileName);

      using var archive = ZipFile.Open(sourceArchiveFileName, ZipArchiveMode.Read, entryNameEncoding);
      archive.ExtractToDirectory(destinationDirectoryName, overwriteFiles);
    }
  }

  /// <param name="archive">The zip archive to extract.</param>
  extension(ZipArchive archive) {

    /// <summary>
    /// Extracts all the files in the zip archive to a directory on the file system and optionally allows overwriting.
    /// </summary>
    /// <param name="destinationDirectoryName">The path to the directory to place the extracted files in.</param>
    /// <param name="overwriteFiles">true to overwrite existing files; false otherwise.</param>
    public void ExtractToDirectory(string destinationDirectoryName, bool overwriteFiles) {
      Guard.Against.ArgumentIsNull(archive);
      Guard.Against.ArgumentIsNull(destinationDirectoryName);

      destinationDirectoryName = IO.Path.GetFullPath(destinationDirectoryName);

      if (!IO.Directory.Exists(destinationDirectoryName))
        IO.Directory.CreateDirectory(destinationDirectoryName);

      foreach (var entry in archive.Entries) {
        if (string.IsNullOrEmpty(entry.Name))
          continue;

        var destinationPath = IO.Path.GetFullPath(IO.Path.Combine(destinationDirectoryName, entry.FullName));

        // Security: Prevent Zip Slip vulnerability
        if (!destinationPath.StartsWith(destinationDirectoryName + IO.Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) &&
            !destinationPath.Equals(destinationDirectoryName, StringComparison.OrdinalIgnoreCase))
          throw new IO.IOException($"Extracting Zip entry would have resulted in a file outside the specified destination directory: {entry.FullName}");

        entry.ExtractToFile(destinationPath, overwriteFiles);
      }
    }
  }

}

}

#endif
