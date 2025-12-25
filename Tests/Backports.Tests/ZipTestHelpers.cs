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

using System;
using System.IO;

namespace Backports.Tests;

/// <summary>
/// Provides helper methods for ZipArchive tests including temporary directory management.
/// </summary>
public sealed class ZipTestHelpers : IDisposable {
  private readonly string _testDirectory;
  private bool _isDisposed;

  /// <summary>
  /// Gets the root test directory for this test session.
  /// </summary>
  public string TestDirectory => this._testDirectory;

  /// <summary>
  /// Creates a new test helper with an isolated temporary directory.
  /// </summary>
  public ZipTestHelpers() {
    this._testDirectory = Path.Combine(Path.GetTempPath(), "ZipTests_" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(this._testDirectory);
  }

  /// <summary>
  /// Creates a subdirectory within the test directory.
  /// </summary>
  public string CreateSubDirectory(string name) {
    var path = Path.Combine(this._testDirectory, name);
    Directory.CreateDirectory(path);
    return path;
  }

  /// <summary>
  /// Creates a test file with specified content.
  /// </summary>
  public string CreateTestFile(string relativePath, string content) {
    var fullPath = Path.Combine(this._testDirectory, relativePath);
    var directory = Path.GetDirectoryName(fullPath);
    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
      Directory.CreateDirectory(directory);

    File.WriteAllText(fullPath, content);
    return fullPath;
  }

  /// <summary>
  /// Creates a test file with specified binary content.
  /// </summary>
  public string CreateTestFile(string relativePath, byte[] content) {
    var fullPath = Path.Combine(this._testDirectory, relativePath);
    var directory = Path.GetDirectoryName(fullPath);
    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
      Directory.CreateDirectory(directory);

    File.WriteAllBytes(fullPath, content);
    return fullPath;
  }

  /// <summary>
  /// Creates a directory structure with sample files for testing.
  /// </summary>
  public string CreateSampleDirectoryStructure(string baseName = "source") {
    var baseDir = this.CreateSubDirectory(baseName);

    // Create some files at root level
    File.WriteAllText(Path.Combine(baseDir, "file1.txt"), "Content of file 1");
    File.WriteAllText(Path.Combine(baseDir, "file2.txt"), "Content of file 2");

    // Create a subdirectory with files
    var subDir = Path.Combine(baseDir, "subdir");
    Directory.CreateDirectory(subDir);
    File.WriteAllText(Path.Combine(subDir, "nested.txt"), "Nested file content");

    // Create a deeper nested structure
    var deepDir = Path.Combine(subDir, "deep");
    Directory.CreateDirectory(deepDir);
    File.WriteAllText(Path.Combine(deepDir, "deep.txt"), "Deep nested content");

    return baseDir;
  }

  /// <summary>
  /// Gets a unique file path for a zip archive in the test directory.
  /// </summary>
  public string GetZipPath(string name = null)
    => Path.Combine(this._testDirectory, (name ?? Guid.NewGuid().ToString("N")) + ".zip");

  /// <summary>
  /// Reads all text from a file.
  /// </summary>
  public static string ReadAllText(string path) => File.ReadAllText(path);

  /// <summary>
  /// Reads all bytes from a file.
  /// </summary>
  public static byte[] ReadAllBytes(string path) => File.ReadAllBytes(path);

  /// <summary>
  /// Cleans up the test directory and all its contents.
  /// </summary>
  public void Dispose() {
    if (this._isDisposed)
      return;

    this._isDisposed = true;

    try {
      if (Directory.Exists(this._testDirectory))
        Directory.Delete(this._testDirectory, recursive: true);
    } catch {
      // Best effort cleanup - don't fail tests due to cleanup issues
    }
  }
}
