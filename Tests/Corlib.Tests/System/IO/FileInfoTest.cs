using System;
using System.IO;
using NUnit.Framework;

namespace Corlib.Tests.System.IO;

internal class FileInfoTest {
  private string? _testDirectory;
  private FileInfo? _sourceFile;
  private FileInfo? _destinationFile;
  private FileInfo? _backupFile;

  [SetUp]
  public void Setup() {
    // Create a temporary directory for testing
    this._testDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    Directory.CreateDirectory(this._testDirectory);

    // Setup initial file states for each test
    this._sourceFile = new(Path.Combine(this._testDirectory, "source.txt"));
    File.WriteAllText(this._sourceFile.FullName, "Source file content");

    this._destinationFile = new(Path.Combine(this._testDirectory, "destination.txt"));
    this._backupFile = new(Path.Combine(this._testDirectory, "backup.txt"));
  }

  [TearDown]
  public void TearDown() {
    // Cleanup the test directory after each test
    if (Directory.Exists(this._testDirectory))
      Directory.Delete(this._testDirectory, true);
  }

  [Test]
  public void ReplaceWith_WhenDestinationDoesNotExist_ShouldMoveFile() {
    this._destinationFile.ReplaceWith(this._sourceFile, null, false);
    this._sourceFile!.Refresh();
    this._destinationFile!.Refresh();
    this._backupFile!.Refresh();

    Assert.IsFalse(this._sourceFile.Exists);
    Assert.IsTrue(this._destinationFile.Exists);
    Assert.AreEqual("Source file content", File.ReadAllText(this._destinationFile.FullName));
  }

  [Test]
  public void ReplaceWith_WhenDestinationExists_ShouldReplaceFile() {
    File.WriteAllText(this._destinationFile!.FullName, "Original destination content");
    this._destinationFile.ReplaceWith(this._sourceFile, null, false);
    this._sourceFile!.Refresh();
    this._destinationFile.Refresh();
    this._backupFile!.Refresh();

    Assert.IsFalse(this._sourceFile.Exists);
    Assert.IsTrue(this._destinationFile.Exists);
    Assert.AreEqual("Source file content", File.ReadAllText(this._destinationFile.FullName));
  }

  [Test]
  public void ReplaceWith_WhenBackupIsProvided_ShouldBackupDestination() {
    File.WriteAllText(this._destinationFile!.FullName, "Original destination content");
    this._destinationFile.ReplaceWith(this._sourceFile, this._backupFile, false);
    this._sourceFile!.Refresh();
    this._destinationFile.Refresh();
    this._backupFile!.Refresh();

    Assert.IsTrue(this._destinationFile.Exists);
    Assert.IsFalse(this._sourceFile.Exists);
    Assert.IsTrue(this._backupFile.Exists);
    Assert.AreEqual("Original destination content", File.ReadAllText(this._backupFile.FullName));
    Assert.AreEqual("Source file content", File.ReadAllText(this._destinationFile.FullName));
  }

}