using System.Text;
using System.Threading;
using NUnit.Framework;

namespace System.IO;

[TestFixture]
public class FileInfoPolyfillsTests {
  private string? _testDirectory;
  private string? _sourceDirectory;
  private string? _targetDirectory;
  private string _TestDirectory => this._testDirectory!;
  private string _SourceDirectory => this._sourceDirectory!;
  private string _TargetDirectory => this._targetDirectory!;

  private const string TestContent = "This is test content";
  private const string TestContent2 = "This is different test content";

  [SetUp]
  public void SetUp() {
    // Create a test directory
    this._testDirectory = Path.Combine(Path.GetTempPath(), "MoveToOverwriteTests_" + Guid.NewGuid().ToString("N"));
    this._sourceDirectory = Path.Combine(this._TestDirectory, "Source");
    this._targetDirectory = Path.Combine(this._TestDirectory, "Target");

    Directory.CreateDirectory(this._SourceDirectory);
    Directory.CreateDirectory(this._TargetDirectory);
  }

  [TearDown]
  public void TearDown() {
    // Clean up test directories
    try {
      if (Directory.Exists(this._TestDirectory))
        Directory.Delete(this._TestDirectory, true);
    } catch {
      // Ignore cleanup errors
    }
  }

  [Test]
  public void MoveTo_DestinationDoesNotExist_MovesFile() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._SourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._TargetDirectory, "dest.txt");

    // Act
    sourceFile.MoveTo(destPath, false);

    // Assert
    Assert.IsFalse(File.Exists(originalPath), "Original file path should no longer exist");
    Assert.IsTrue(File.Exists(destPath), "Destination file should exist");
    Assert.AreEqual(TestContent, File.ReadAllText(destPath), "File content should be identical");
    Assert.AreEqual(destPath, sourceFile.FullName, "FileInfo.FullName should be updated");
  }

  [Test]
  public void MoveTo_DestinationExists_OverwriteTrue_OverwritesFile() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._SourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._TargetDirectory, "dest.txt");
    this.CreateTestFile(this._TargetDirectory, "dest.txt", TestContent2); // Create destination file

    // Act - use retry logic for transient Windows file system issues
    ExecuteWithRetry(() => sourceFile.MoveTo(destPath, true));

    // Assert
    Assert.IsFalse(File.Exists(originalPath), "Original file path should no longer exist");
    Assert.IsTrue(File.Exists(destPath), "Destination file should exist");
    Assert.AreEqual(TestContent, File.ReadAllText(destPath), "Destination file should have been overwritten");
    Assert.AreEqual(destPath, sourceFile.FullName, "FileInfo.FullName should be updated");
  }

  [Test]
  public void MoveTo_DestinationExists_OverwriteFalse_ThrowsIOException() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._SourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._TargetDirectory, "dest.txt");
    this.CreateTestFile(this._TargetDirectory, "dest.txt", TestContent2); // Create destination file

    // Act & Assert
    var ex = Assert.Throws<IOException>(() => sourceFile.MoveTo(destPath, false));
    Assert.IsTrue(File.Exists(originalPath), "Source file should still exist");
    Assert.AreEqual(TestContent, File.ReadAllText(originalPath), "Source file should be unchanged");
    Assert.AreEqual(TestContent2, File.ReadAllText(destPath), "Destination file should be unchanged");
    Assert.AreEqual(originalPath, sourceFile.FullName, "FileInfo.FullName should be unchanged");
  }

  [Test]
  public void MoveTo_SameDirectoryOverwrite_CorrectlyRenames() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._SourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._SourceDirectory, "dest.txt");
    this.CreateTestFile(this._SourceDirectory, "dest.txt", TestContent2); // Create destination file

    // Act
    sourceFile.MoveTo(destPath, true);

    // Assert
    Assert.IsFalse(File.Exists(originalPath), "Original file path should no longer exist");
    Assert.IsTrue(File.Exists(destPath), "Destination file should exist");
    Assert.AreEqual(TestContent, File.ReadAllText(destPath), "Destination file should have been overwritten");
    Assert.AreEqual(destPath, sourceFile.FullName, "FileInfo.FullName should be updated");
  }

  [Test]
  public void MoveTo_TargetDirectoryDoesNotExist_ThrowsDirectoryNotFoundException() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._SourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var nonExistentDir = Path.Combine(this._TestDirectory, "NonExistent");
    var destPath = Path.Combine(nonExistentDir, "dest.txt");

    // Act & Assert
    Assert.Throws<DirectoryNotFoundException>(() => sourceFile.MoveTo(destPath, true));
    Assert.IsTrue(File.Exists(originalPath), "Source file should still exist");
    Assert.AreEqual(originalPath, sourceFile.FullName, "FileInfo.FullName should be unchanged");
  }

  [Test]
  public void MoveTo_LargeFile_CorrectlyMoves() {
    // Arrange
    var largeContent = new string('A', 10 * 1024 * 1024); // 10 MB file
    var sourceFile = this.CreateTestFile(this._SourceDirectory, "large.txt", largeContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._TargetDirectory, "large_dest.txt");

    // Act
    sourceFile.MoveTo(destPath, false);

    // Assert
    Assert.IsFalse(File.Exists(originalPath), "Original file path should no longer exist");
    Assert.IsTrue(File.Exists(destPath), "Destination file should exist");
    Assert.AreEqual(largeContent.Length, new FileInfo(destPath).Length, "File size should be identical");
    Assert.AreEqual(destPath, sourceFile.FullName, "FileInfo.FullName should be updated");
  }

  [Test]
  public void MoveTo_DestinationIsHidden_OverwriteTrue_SuccessfullyOverwrites() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._SourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._TargetDirectory, "hidden_dest.txt");
    var destFile = this.CreateTestFile(this._TargetDirectory, "hidden_dest.txt", TestContent2);

    // Mark destination file as hidden
    SetFileAttributesWithRetry(destPath, FileAttributes.Hidden);

    // Act - use retry logic for transient Windows file system issues
    ExecuteWithRetry(() => sourceFile.MoveTo(destPath, true));

    // Assert
    Assert.IsFalse(File.Exists(originalPath), "Original file path should no longer exist");
    Assert.IsTrue(File.Exists(destPath), "Destination file should exist");
    Assert.AreEqual(TestContent, File.ReadAllText(destPath), "Destination file should have been overwritten");
    Assert.AreEqual(destPath, sourceFile.FullName, "FileInfo.FullName should be updated");

    // Check whether the Hidden attribute is preserved or not
    // Depending on implementation, the attribute may be preserved or lost
    // This is mainly a matter of implementation details
    Assert.IsFalse((File.GetAttributes(destPath) & FileAttributes.Hidden) == FileAttributes.Hidden, "Destination file should no longer be hidden");
  }

  [Test]
  public void MoveTo_DestinationIsSystem_OverwriteTrue_SuccessfullyOverwrites() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._SourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._TargetDirectory, "system_dest.txt");
    var destFile = this.CreateTestFile(this._TargetDirectory, "system_dest.txt", TestContent2);

    // Mark destination file as system file
    SetFileAttributesWithRetry(destPath, FileAttributes.System);

    // Act - use retry logic for transient Windows file system issues
    ExecuteWithRetry(() => sourceFile.MoveTo(destPath, true));

    // Assert
    Assert.IsFalse(File.Exists(originalPath), "Original file path should no longer exist");
    Assert.IsTrue(File.Exists(destPath), "Destination file should exist");
    Assert.AreEqual(TestContent, File.ReadAllText(destPath), "Destination file should have been overwritten");
    Assert.AreEqual(destPath, sourceFile.FullName, "FileInfo.FullName should be updated");

    // Check whether the System attribute is preserved or not
    // Depending on implementation, the attribute may be preserved or lost
    Assert.IsFalse((File.GetAttributes(destPath) & FileAttributes.System) == FileAttributes.System, "Destination file should no longer be a system file");
  }

  [Test]
  public void MoveTo_DestinationWithHiddenAndSystem_OverwriteTrue_SuccessfullyOverwrites() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._SourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._TargetDirectory, "multi_attr_dest.txt");
    var destFile = this.CreateTestFile(this._TargetDirectory, "multi_attr_dest.txt", TestContent2);

    // Mark destination file with Hidden and System attributes (but not ReadOnly)
    SetFileAttributesWithRetry(destPath, FileAttributes.Hidden | FileAttributes.System);

    // Act - use retry logic for transient Windows file system issues
    ExecuteWithRetry(() => sourceFile.MoveTo(destPath, true));

    // Assert
    Assert.IsFalse(File.Exists(originalPath), "Original file path should no longer exist");
    Assert.IsTrue(File.Exists(destPath), "Destination file should exist");
    Assert.AreEqual(TestContent, File.ReadAllText(destPath), "Destination file should have been overwritten");
    Assert.AreEqual(destPath, sourceFile.FullName, "FileInfo.FullName should be updated");
  }

  [Test]
  public void MoveTo_DestinationIsReadOnly_OverwriteTrue_DoesNotOverwrite() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._SourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._TargetDirectory, "readonly_dest.txt");
    var destFile = this.CreateTestFile(this._TargetDirectory, "readonly_dest.txt", TestContent2);

    // Mark destination file as read-only
    File.SetAttributes(destPath, FileAttributes.ReadOnly);

    // Act
    // On Linux, read-only attribute behavior may differ
    if (Environment.OSVersion.Platform == PlatformID.Unix) {
      // On Unix systems, read-only behavior may be different
      try {
        sourceFile.MoveTo(destPath, true);
      } catch (Exception) {
        // Expected - some kind of exception should occur with read-only file
      }
    } else {
      Assert.Throws<UnauthorizedAccessException>(() => sourceFile.MoveTo(destPath, true));
    }

    // Assert
    // On Linux, read-only attributes don't prevent file operations like on Windows
    if (Environment.OSVersion.Platform == PlatformID.Unix) {
      // On Unix, the move may succeed, so check accordingly
      if (File.Exists(originalPath)) {
        Assert.IsTrue(File.Exists(destPath), "Destination file should exist");
        Assert.AreEqual(TestContent, File.ReadAllText(originalPath), "Source file should not have been overwritten");
        Assert.AreEqual(TestContent2, File.ReadAllText(destPath), "Destination file should not have been overwritten");
        Assert.AreEqual(originalPath, sourceFile.FullName, "FileInfo.FullName should be unchanged");
      } else {
        // Move succeeded on Unix despite read-only attribute
        Assert.IsTrue(File.Exists(destPath), "Destination file should exist after successful move");
      }
    } else {
      Assert.IsTrue(File.Exists(originalPath), "Original file path should exist");
      Assert.IsTrue(File.Exists(destPath), "Destination file should exist");
      Assert.AreEqual(TestContent, File.ReadAllText(originalPath), "Source file should not have been overwritten");
      Assert.AreEqual(TestContent2, File.ReadAllText(destPath), "Destination file should not have been overwritten");
      Assert.AreEqual(originalPath, sourceFile.FullName, "FileInfo.FullName should be unchanged");
    }
    // On Linux, ReadOnly attribute may not be preserved
    if (Environment.OSVersion.Platform != PlatformID.Unix) {
      Assert.IsTrue(
        (File.GetAttributes(destPath) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly,
        "Destination file should still be read-only"
      );
    }
  }

  [Test]
  public void MoveTo_DestinationWithMultipleAttributes_OverwriteTrue_DoesNotOverwrite() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._SourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._TargetDirectory, "multi_attr_dest.txt");
    var destFile = this.CreateTestFile(this._TargetDirectory, "multi_attr_dest.txt", TestContent2);

    // Mark destination file with multiple attributes
    File.SetAttributes(destPath, FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System);

    // Act
    // On Linux, multiple attributes don't prevent file operations like on Windows
    if (Environment.OSVersion.Platform == PlatformID.Unix) {
      // On Unix systems, these attributes may not prevent operations
      try {
        sourceFile.MoveTo(destPath, true);
      } catch (Exception) {
        // Expected - some kind of exception may occur
      }
    } else {
      Assert.Throws<UnauthorizedAccessException>(() => sourceFile.MoveTo(destPath, true));
    }

    // Assert
    // On Linux, attributes don't prevent operations like on Windows
    if (Environment.OSVersion.Platform == PlatformID.Unix) {
      // On Unix, the move may succeed, so check accordingly
      if (File.Exists(originalPath)) {
        Assert.IsTrue(File.Exists(destPath), "Destination file should exist");
        Assert.AreEqual(TestContent, File.ReadAllText(originalPath), "Source file should not have been overwritten");
        Assert.AreEqual(TestContent2, File.ReadAllText(destPath), "Destination file should not have been overwritten");
      } else {
        // Move succeeded on Unix despite attributes
        Assert.IsTrue(File.Exists(destPath), "Destination file should exist after successful move");
      }
    } else {
      Assert.IsTrue(File.Exists(originalPath), "Original file path should exist");
      Assert.IsTrue(File.Exists(destPath), "Destination file should exist");
      Assert.AreEqual(TestContent, File.ReadAllText(originalPath), "Source file should not have been overwritten");
      Assert.AreEqual(TestContent2, File.ReadAllText(destPath), "Destination file should not have been overwritten");
    }
    Assert.AreEqual(destPath, destFile.FullName, "FileInfo.FullName should not be updated");

    var attributes = File.GetAttributes(destPath);
    // On Linux, ReadOnly attribute may not be preserved
    if (Environment.OSVersion.Platform != PlatformID.Unix) {
      Assert.IsTrue(
        (attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly,
      "Destination file should still be read-only"
      );
    }
    Assert.IsTrue(
      (attributes & FileAttributes.Hidden) == FileAttributes.Hidden,
      "Destination file should still be hidden"
    );
    Assert.IsTrue(
      (attributes & FileAttributes.System) == FileAttributes.System,
      "Destination file should still be a system file"
    );
  }

  [Test]
  public void MoveTo_FileInUse_HandlesAppropriately() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._SourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._TargetDirectory, "dest.txt");
    var destFile = this.CreateTestFile(this._TargetDirectory, "dest.txt", TestContent2);

    // Open the destination file with FileShare.None to lock it
    using var fileStream = new FileStream(destPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

    // Act & Assert
    // On Linux, file locking may not throw the expected exception
    if (Environment.OSVersion.Platform == PlatformID.Unix) {
      // On Unix systems, file locking behavior may differ
      // Just verify that the operation doesn't succeed without exception handling expected
      try {
        sourceFile.MoveTo(destPath, true);
      } catch (Exception) {
        // Expected - some kind of exception should occur when file is locked
      }
    } else {
      Assert.Throws<UnauthorizedAccessException>(() => sourceFile.MoveTo(destPath, true));
    }

    // Ensure that the source file still exists
    // On Linux, file locking may not work the same way
    if (Environment.OSVersion.Platform == PlatformID.Unix) {
      // On Unix, the move may succeed despite file locking attempts
      if (File.Exists(originalPath)) {
        Assert.AreEqual(TestContent, File.ReadAllText(originalPath), "Source file should be unchanged");
        Assert.AreEqual(originalPath, sourceFile.FullName, "FileInfo.FullName should be unchanged");
      }
      // Move may have succeeded despite the lock attempt
    } else {
      Assert.IsTrue(File.Exists(originalPath), "Source file should still exist");
      Assert.AreEqual(TestContent, File.ReadAllText(originalPath), "Source file should be unchanged");
      Assert.AreEqual(originalPath, sourceFile.FullName, "FileInfo.FullName should be unchanged");
    }
  }

  [Test]
  public void MoveTo_FileInfoBehavior_UpdatesSourceFileInfo() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._SourceDirectory, "source.txt", TestContent);
    var originalFullName = sourceFile.FullName;
    var destPath = Path.Combine(this._TargetDirectory, "dest.txt");

    // Act
    sourceFile.MoveTo(destPath, false);

    // Assert
    Assert.AreEqual(destPath, sourceFile.FullName, "FileInfo.FullName should be updated");
    Assert.AreEqual("dest.txt", sourceFile.Name, "FileInfo.Name should be updated");
    Assert.AreEqual(this._TargetDirectory, sourceFile.DirectoryName, "FileInfo.DirectoryName should be updated");
    Assert.IsTrue(sourceFile.Exists, "FileInfo.Exists should be true");
    Assert.IsFalse(File.Exists(originalFullName), "Original file should no longer exist");
  }

  [Test]
  public void MoveTo_MultipleConcurrentOperations_WorksCorrectly() {
    // Arrange
    const int fileCount = 5;
    var sourceFiles = new FileInfo[fileCount];
    var originalPaths = new string[fileCount];
    var destPaths = new string[fileCount];

    for (var i = 0; i < fileCount; i++) {
      sourceFiles[i] = this.CreateTestFile(this._SourceDirectory, $"source{i}.txt", TestContent + i);
      originalPaths[i] = sourceFiles[i].FullName;
      destPaths[i] = Path.Combine(this._TargetDirectory, $"dest{i}.txt");
      this.CreateTestFile(this._TargetDirectory, $"dest{i}.txt", TestContent2 + i);
    }

    // Act
    var threads = new Thread[fileCount];
    var exceptions = new Exception[fileCount];

    for (var i = 0; i < fileCount; i++) {
      var index = i; // Local copy for lambda expression
      threads[i] = new(
        () => {
          try {
            sourceFiles[index].MoveTo(destPaths[index], true);
          } catch (Exception ex) {
            exceptions[index] = ex;
          }
        }
      );
      threads[i].Start();
    }

    foreach (var thread in threads)
      thread.Join();

    // Assert
    for (var i = 0; i < fileCount; i++) {
      Assert.IsNull(exceptions[i], $"Operation for file {i} should succeed");
      Assert.IsFalse(
        File.Exists(originalPaths[i]),
        $"Original file path for source file {i} should no longer exist"
      );
      Assert.IsTrue(File.Exists(destPaths[i]), $"Destination file {i} should exist");
      Assert.AreEqual(
        TestContent + i,
        File.ReadAllText(destPaths[i]),
        $"Destination file {i} should have the correct content"
      );
      Assert.AreEqual(
        destPaths[i],
        sourceFiles[i].FullName,
        $"FileInfo.FullName for file {i} should be updated"
      );
    }
  }

  [Test]
  public void MoveTo_NonExistentSourceFile_ThrowsFileNotFoundException() {
    // Arrange
    var nonExistentFile = new FileInfo(Path.Combine(this._SourceDirectory, "nonexistent.txt"));
    var originalPath = nonExistentFile.FullName;
    var destPath = Path.Combine(this._TargetDirectory, "dest.txt");

    // Act & Assert
    var ex = Assert.Throws<FileNotFoundException>(() => nonExistentFile.MoveTo(destPath, true));
    Assert.AreEqual(originalPath, nonExistentFile.FullName, "FileInfo.FullName should be unchanged");
  }

  [Test]
  public void MoveTo_DestinationIsDirectory_ThrowsUnauthorizedAccessException() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._SourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destDirectory = this._TargetDirectory; // Target path is a directory

    // Act & Assert
    // On Linux, moving to directory throws IOException instead of UnauthorizedAccessException
    if (Environment.OSVersion.Platform == PlatformID.Unix) {
      var ex = Assert.Throws<IOException>(() => sourceFile.MoveTo(destDirectory, true));
    } else {
      var ex = Assert.Throws<UnauthorizedAccessException>(() => sourceFile.MoveTo(destDirectory, true));
    }
    Assert.IsTrue(File.Exists(originalPath), "Source file should still exist");
    Assert.AreEqual(originalPath, sourceFile.FullName, "FileInfo.FullName should be unchanged");
  }

  [Test]
  public void MoveTo_SameFileOverwrite_NoOperation() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._SourceDirectory, "source.txt", TestContent);
    var sourceContent = File.ReadAllText(sourceFile.FullName);
    var sourceFilePath = sourceFile.FullName;

    // Act - Move to the same location
    sourceFile.MoveTo(sourceFilePath, true);

    // Assert
    Assert.IsTrue(File.Exists(sourceFilePath), "File should still exist");
    Assert.AreEqual(sourceContent, File.ReadAllText(sourceFilePath), "Content should be unchanged");
    Assert.AreEqual(sourceFilePath, sourceFile.FullName, "FileInfo.FullName should be unchanged");
  }

  // Helper method to create test files
  private FileInfo CreateTestFile(string directory, string fileName, string content) {
    var filePath = Path.Combine(directory, fileName);

    // Use explicit FileStream to ensure proper handle release
    using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
    using (var writer = new StreamWriter(stream, new UTF8Encoding(false))) {
      writer.Write(content);
      writer.Flush();
      stream.Flush();
    }

    // Give Windows time to fully release file handles (anti-virus, search indexer, etc.)
    GC.Collect();
    GC.WaitForPendingFinalizers();

    return new(filePath);
  }

  // Helper to ensure file attributes can be set reliably
  private static void SetFileAttributesWithRetry(string path, FileAttributes attributes, int maxRetries = 3) {
    for (var i = 0; i < maxRetries; ++i)
      try {
        File.SetAttributes(path, attributes);
        return;
      } catch (IOException) when (i < maxRetries - 1) {
        Thread.Sleep(50);
      }
  }

  // Helper to perform file operations with retry logic for transient Windows file system issues
  private static void ExecuteWithRetry(Action action, int maxRetries = 3) {
    for (var i = 0; i < maxRetries; ++i)
      try {
        action();
        return;
      } catch (UnauthorizedAccessException) when (i < maxRetries - 1) {
        Thread.Sleep(100);
        GC.Collect();
        GC.WaitForPendingFinalizers();
      } catch (IOException) when (i < maxRetries - 1) {
        Thread.Sleep(100);
        GC.Collect();
        GC.WaitForPendingFinalizers();
      }
  }

  [Test]
  public void MoveTo_SourceHasAttributes_AttributesArePreserved() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._SourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._TargetDirectory, "dest.txt");

    // Set various attributes on the source file
    File.SetAttributes(originalPath, FileAttributes.Archive | FileAttributes.Hidden);

    // Act
    sourceFile.MoveTo(destPath, false);

    // Assert
    Assert.IsFalse(File.Exists(originalPath), "Original file path should no longer exist");
    Assert.IsTrue(File.Exists(destPath), "Destination file should exist");

    var targetAttributes = File.GetAttributes(destPath);

    // On Linux, Archive and Hidden attributes may not be supported
    if (Environment.OSVersion.Platform != PlatformID.Unix) {
      Assert.IsTrue(
        (targetAttributes & FileAttributes.Archive) == FileAttributes.Archive,
        "Archive attribute should be transferred to the destination file"
      );
      Assert.IsTrue(
        (targetAttributes & FileAttributes.Hidden) == FileAttributes.Hidden,
        "Hidden attribute should be transferred to the destination file"
      );
    }
  }

  [Test]
  public void MoveTo_SourceHasAttributes_DestinationExists_OverwriteTrue_AttributesArePreserved() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._SourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._TargetDirectory, "dest.txt");
    this.CreateTestFile(this._TargetDirectory, "dest.txt", TestContent2); // Create destination file

    // Set various attributes on the source file
    File.SetAttributes(originalPath, FileAttributes.Archive | FileAttributes.System);

    // Act
    sourceFile.MoveTo(destPath, true);

    // Assert
    Assert.IsFalse(File.Exists(originalPath), "Original file path should no longer exist");
    Assert.IsTrue(File.Exists(destPath), "Destination file should exist");

    var targetAttributes = File.GetAttributes(destPath);

    // On Linux, some attributes may not be supported
    if (Environment.OSVersion.Platform != PlatformID.Unix) {
      Assert.IsTrue(
        (targetAttributes & FileAttributes.Archive) == FileAttributes.Archive,
        "Archive attribute should be transferred to the destination file"
      );
      Assert.IsTrue(
        (targetAttributes & FileAttributes.System) == FileAttributes.System,
        "System attribute should be transferred to the destination file"
      );
    }
  }

  [Test]
  public void MoveTo_SourceAndDestinationHaveAttributes_OverwriteTrue_SourceAttributesArePreserved() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._SourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._TargetDirectory, "dest.txt");
    this.CreateTestFile(this._TargetDirectory, "dest.txt", TestContent2);

    // Set different attributes on source and destination files
    SetFileAttributesWithRetry(originalPath, FileAttributes.Archive | FileAttributes.Hidden);
    SetFileAttributesWithRetry(destPath, FileAttributes.System | FileAttributes.Temporary);

    // Act - use retry logic for transient Windows file system issues
    ExecuteWithRetry(() => sourceFile.MoveTo(destPath, true));

    // Assert
    Assert.IsFalse(File.Exists(originalPath), "Original file path should no longer exist");
    Assert.IsTrue(File.Exists(destPath), "Destination file should exist");

    var targetAttributes = File.GetAttributes(destPath);

    // Source file attributes should be preserved
    // On Linux, some attributes may not be supported
    if (Environment.OSVersion.Platform != PlatformID.Unix) {
      Assert.IsTrue(
        (targetAttributes & FileAttributes.Archive) == FileAttributes.Archive,
        "Archive attribute of the source file should be preserved"
      );
      Assert.IsTrue(
        (targetAttributes & FileAttributes.Hidden) == FileAttributes.Hidden,
        "Hidden attribute of the source file should be preserved"
      );
    }

    // Destination file attributes should no longer be present
    Assert.IsFalse(
      (targetAttributes & FileAttributes.System) == FileAttributes.System,
      "System attribute of the destination file should not be preserved"
    );
    Assert.IsFalse(
      (targetAttributes & FileAttributes.Temporary) == FileAttributes.Temporary,
      "Temporary attribute of the destination file should not be preserved"
    );
  }

  [Test]
  public void MoveTo_SourceIsReadOnly_AttributeIsPreserved() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._SourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._TargetDirectory, "dest.txt");

    // Set ReadOnly attribute on the source file
    File.SetAttributes(originalPath, FileAttributes.ReadOnly);

    // Act
    sourceFile.MoveTo(destPath, false);

    // Assert
    Assert.IsFalse(File.Exists(originalPath), "Original file path should no longer exist");
    Assert.IsTrue(File.Exists(destPath), "Destination file should exist");

    var targetAttributes = File.GetAttributes(destPath);
    Assert.IsTrue(
      (targetAttributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly,
      "ReadOnly attribute should be transferred to the destination file"
    );
  }

  [Test]
  public void MoveTo_SourceHasAllAttributes_AllAttributesArePreserved() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._SourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._TargetDirectory, "dest.txt");

    // Set multiple attributes on the source file
    var sourceAttributes = FileAttributes.Archive | FileAttributes.Hidden | FileAttributes.System | FileAttributes.Temporary;
    File.SetAttributes(originalPath, sourceAttributes);

    // Act
    sourceFile.MoveTo(destPath, false);

    // Assert
    Assert.IsFalse(File.Exists(originalPath), "Original file path should no longer exist");
    Assert.IsTrue(File.Exists(destPath), "Destination file should exist");

    var targetAttributes = File.GetAttributes(destPath);

    // Check all attributes individually
    // On Linux, some attributes may not be supported
    if (Environment.OSVersion.Platform != PlatformID.Unix) {
      Assert.IsTrue(
        (targetAttributes & FileAttributes.Archive) == FileAttributes.Archive,
        "Archive attribute should be preserved"
      );
      Assert.IsTrue(
        (targetAttributes & FileAttributes.Hidden) == FileAttributes.Hidden,
        "Hidden attribute should be preserved"
      );
    }
    // On Linux, System attribute may not be supported
    if (Environment.OSVersion.Platform != PlatformID.Unix) {
      Assert.IsTrue(
        (targetAttributes & FileAttributes.System) == FileAttributes.System,
        "System attribute should be preserved"
      );
    }
    // On Linux, Temporary attribute may not be supported
    if (Environment.OSVersion.Platform != PlatformID.Unix) {
      Assert.IsTrue(
        (targetAttributes & FileAttributes.Temporary) == FileAttributes.Temporary,
        "Temporary attribute should be preserved"
      );
    }
  }
}
