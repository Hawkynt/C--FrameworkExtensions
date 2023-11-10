using System;
using System.IO;
using NUnit.Framework;

namespace Corlib.Tests.System.IO;

internal class FileInfoTest {
  private string testDirectory;
  private FileInfo sourceFile;
  private FileInfo destinationFile;
  private FileInfo backupFile;

  [SetUp]
  public void Setup() {
    // Create a temporary directory for testing
    testDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    Directory.CreateDirectory(testDirectory);

    // Setup initial file states for each test
    sourceFile = new FileInfo(Path.Combine(testDirectory, "source.txt"));
    File.WriteAllText(sourceFile.FullName, "Source file content");

    destinationFile = new FileInfo(Path.Combine(testDirectory, "destination.txt"));
    backupFile = new FileInfo(Path.Combine(testDirectory, "backup.txt"));
  }

  [TearDown]
  public void TearDown() {
    // Cleanup the test directory after each test
    if (Directory.Exists(testDirectory)) {
      Directory.Delete(testDirectory, true);
    }
  }

  [Test]
  public void ReplaceWith_WhenDestinationDoesNotExist_ShouldMoveFile() {
    destinationFile.ReplaceWith(this.sourceFile, null, false);
    this.sourceFile.Refresh();
    this.destinationFile.Refresh();
    this.backupFile.Refresh();

    Assert.IsFalse(sourceFile.Exists);
    Assert.IsTrue(destinationFile.Exists);
    Assert.AreEqual("Source file content", File.ReadAllText(destinationFile.FullName));
  }

  [Test]
  public void ReplaceWith_WhenDestinationExists_ShouldReplaceFile() {
    File.WriteAllText(destinationFile.FullName, "Original destination content");
    destinationFile.ReplaceWith(this.sourceFile, null, false);
    this.sourceFile.Refresh();
    this.destinationFile.Refresh();
    this.backupFile.Refresh();

    Assert.IsFalse(sourceFile.Exists);
    Assert.IsTrue(destinationFile.Exists);
    Assert.AreEqual("Source file content", File.ReadAllText(destinationFile.FullName));
  }

  [Test]
  public void ReplaceWith_WhenBackupIsProvided_ShouldBackupDestination() {
    File.WriteAllText(destinationFile.FullName, "Original destination content");
    destinationFile.ReplaceWith(this.sourceFile, backupFile, false);
    this.sourceFile.Refresh();
    this.destinationFile.Refresh();
    this.backupFile.Refresh();

    Assert.IsTrue(this.destinationFile.Exists);
    Assert.IsFalse(this.sourceFile.Exists);
    Assert.IsTrue(backupFile.Exists);
    Assert.AreEqual("Original destination content", File.ReadAllText(backupFile.FullName));
    Assert.AreEqual("Source file content", File.ReadAllText(destinationFile.FullName));
  }

}