using System.IO;
using NUnit.Framework;

namespace Corlib.Tests.System.IO;

internal class FileInfoTest {

  [TestFixture]
  public class FileInProgressTests {
    private const string _TEST_CONTENT = "Test content";
    private const string _NEW_CONTENT = "New content";
    private FileInfo? _sourceFile;
    
    [SetUp]
    public void Setup() {
      // Create a source file for testing
      this._sourceFile = new("test.txt");
      this._sourceFile.WriteAllText(_TEST_CONTENT);
    }

    [Test]
    public void ShouldCreateTemporaryFile() {
      using var fileInProgress = this._sourceFile.StartWorkInProgress();
      Assert.IsTrue(File.Exists(this._sourceFile!.FullName+".$$$"));
    }

    [Test]
    public void ShouldApplyChangesOnDisposeIfNotCancelled() {
      string? tempFileName;
      using (var fileInProgress = this._sourceFile.StartWorkInProgress()) {
        fileInProgress.WriteAllText(_NEW_CONTENT);
        tempFileName = this._sourceFile!.FullName + ".$$$";
        fileInProgress.CancelChanges = false;
      }

      // Assuming ReplaceWith correctly replaces the source file with temporary file
      Assert.IsFalse(File.Exists(tempFileName));
      Assert.AreEqual(_NEW_CONTENT,this._sourceFile.ReadAllText());
    }

    [Test]
    public void ShouldNotApplyChangesOnDisposeIfCancelled() {
      using (var fileInProgress = this._sourceFile.StartWorkInProgress()) {
        fileInProgress.WriteAllText(_NEW_CONTENT);
        fileInProgress.CancelChanges = true;
      }

      // Assuming ReplaceWith correctly replaces the source file with temporary file
      Assert.AreEqual(_TEST_CONTENT, this._sourceFile.ReadAllText());
    }

    [Test]
    public void ShouldCopyContentsIfRequested() {
      using var fileInProgress = this._sourceFile.StartWorkInProgress(true);
      Assert.AreEqual(_TEST_CONTENT, fileInProgress.ReadAllText());
    }

    [Test]
    public void ShouldNotCopyContentsIfRequested() {
      using var fileInProgress = this._sourceFile.StartWorkInProgress(false);
      Assert.AreEqual(string.Empty, fileInProgress.ReadAllText());
    }

    [TearDown]
    public void TearDown() => this._sourceFile.TryDelete();
  }


  public class ReplaceWithTest {
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


}