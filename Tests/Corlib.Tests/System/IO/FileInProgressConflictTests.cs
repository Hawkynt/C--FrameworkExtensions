using System.Text;
using System.Threading;
using NUnit.Framework;

namespace System.IO;

[TestFixture]
public class FileInProgressConflictTests {
  private string _testDirectory;
  private string _TestDirectory => this._testDirectory!;

  private const string OriginalContent = "Original content";
  private const string ModifiedContent = "Modified content";
  private const string ExternalContent = "External modification";

  [SetUp]
  public void SetUp() {
    this._testDirectory = Path.Combine(Path.GetTempPath(), "FileInProgressConflictTests_" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(this._TestDirectory);
  }

  [TearDown]
  public void TearDown() {
    try {
      if (Directory.Exists(this._TestDirectory))
        Directory.Delete(this._TestDirectory, true);
    } catch {
      // Ignore cleanup errors
    }
  }

  #region None Mode Tests

  [Test]
  public void NoneMode_NoConflict_AppliesChanges() {
    var file = this.CreateTestFile("test.txt", OriginalContent);

    using (var wip = file.StartWorkInProgress(true, ConflictResolutionMode.None)) {
      Assert.AreEqual(ConflictResolutionMode.None, wip.ConflictMode);
      wip.WriteAllText(ModifiedContent);
    }

    Assert.AreEqual(ModifiedContent, File.ReadAllText(file.FullName));
  }

  [Test]
  public void NoneMode_ExternalModification_LastWriteWins() {
    var file = this.CreateTestFile("test.txt", OriginalContent);

    using (var wip = file.StartWorkInProgress(true, ConflictResolutionMode.None)) {
      wip.WriteAllText(ModifiedContent);
      File.WriteAllText(file.FullName, ExternalContent);
    }

    Assert.AreEqual(ModifiedContent, File.ReadAllText(file.FullName));
  }

  #endregion

  #region LockWithReadShare Mode Tests

  [Test]
  public void LockWithReadShare_NoConflict_AppliesChanges() {
    var file = this.CreateTestFile("test.txt", OriginalContent);

    using (var wip = file.StartWorkInProgress(true, ConflictResolutionMode.LockWithReadShare)) {
      Assert.AreEqual(ConflictResolutionMode.LockWithReadShare, wip.ConflictMode);
      wip.WriteAllText(ModifiedContent);
    }

    Assert.AreEqual(ModifiedContent, File.ReadAllText(file.FullName));
  }

  [Test]
  public void LockWithReadShare_AllowsReaders() {
    var file = this.CreateTestFile("test.txt", OriginalContent);

    using (var wip = file.StartWorkInProgress(true, ConflictResolutionMode.LockWithReadShare)) {
      // Readers must use FileShare.ReadWrite to coexist with a writer handle
      using (var readStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
      using (var reader = new StreamReader(readStream)) {
        var content = reader.ReadToEnd();
        Assert.AreEqual(OriginalContent, content);
      }

      wip.WriteAllText(ModifiedContent);
    }

    Assert.AreEqual(ModifiedContent, File.ReadAllText(file.FullName));
  }

  [Test]
  public void LockWithReadShare_BlocksWriters() {
    if (Environment.OSVersion.Platform == PlatformID.Unix)
      Assert.Ignore("File locking behavior differs on Unix");

    var file = this.CreateTestFile("test.txt", OriginalContent);

    using (var wip = file.StartWorkInProgress(true, ConflictResolutionMode.LockWithReadShare)) {
      Assert.Throws<IOException>(() => File.WriteAllText(file.FullName, ExternalContent));
      wip.WriteAllText(ModifiedContent);
    }

    Assert.AreEqual(ModifiedContent, File.ReadAllText(file.FullName));
  }

  #endregion

  #region LockExclusive Mode Tests

  [Test]
  public void LockExclusive_NoConflict_AppliesChanges() {
    var file = this.CreateTestFile("test.txt", OriginalContent);

    using (var wip = file.StartWorkInProgress(true, ConflictResolutionMode.LockExclusive)) {
      Assert.AreEqual(ConflictResolutionMode.LockExclusive, wip.ConflictMode);
      wip.WriteAllText(ModifiedContent);
    }

    Assert.AreEqual(ModifiedContent, File.ReadAllText(file.FullName));
  }

  [Test]
  public void LockExclusive_BlocksReaders() {
    if (Environment.OSVersion.Platform == PlatformID.Unix)
      Assert.Ignore("File locking behavior differs on Unix");

    var file = this.CreateTestFile("test.txt", OriginalContent);

    using (var wip = file.StartWorkInProgress(true, ConflictResolutionMode.LockExclusive)) {
      Assert.Throws<IOException>(() => File.ReadAllText(file.FullName));
      wip.WriteAllText(ModifiedContent);
    }

    Assert.AreEqual(ModifiedContent, File.ReadAllText(file.FullName));
  }

  [Test]
  public void LockExclusive_BlocksWriters() {
    if (Environment.OSVersion.Platform == PlatformID.Unix)
      Assert.Ignore("File locking behavior differs on Unix");

    var file = this.CreateTestFile("test.txt", OriginalContent);

    using (var wip = file.StartWorkInProgress(true, ConflictResolutionMode.LockExclusive)) {
      Assert.Throws<IOException>(() => File.WriteAllText(file.FullName, ExternalContent));
      wip.WriteAllText(ModifiedContent);
    }

    Assert.AreEqual(ModifiedContent, File.ReadAllText(file.FullName));
  }

  #endregion

  #region CheckLastWriteTimeAndThrow Mode Tests

  [Test]
  public void CheckLastWriteTimeAndThrow_NoConflict_AppliesChanges() {
    var file = this.CreateTestFile("test.txt", OriginalContent);

    using (var wip = file.StartWorkInProgress(true, ConflictResolutionMode.CheckLastWriteTimeAndThrow)) {
      Assert.AreEqual(ConflictResolutionMode.CheckLastWriteTimeAndThrow, wip.ConflictMode);
      wip.WriteAllText(ModifiedContent);
    }

    Assert.AreEqual(ModifiedContent, File.ReadAllText(file.FullName));
  }

  [Test]
  public void CheckLastWriteTimeAndThrow_ExternalModification_ThrowsException() {
    var file = this.CreateTestFile("test.txt", OriginalContent);
    FileInfoExtensions.IFileInProgress wip = null;

    try {
      wip = file.StartWorkInProgress(true, ConflictResolutionMode.CheckLastWriteTimeAndThrow);
      wip.WriteAllText(ModifiedContent);

      Thread.Sleep(100);
      File.WriteAllText(file.FullName, ExternalContent);

      Assert.Throws<FileConflictException>(() => wip.Dispose());
    } finally {
      if (wip != null) {
        try {
          wip.Dispose();
        } catch (FileConflictException) {
          // Expected
        }
      }
    }
  }

  #endregion

  #region CheckLastWriteTimeAndIgnoreUpdate Mode Tests

  [Test]
  public void CheckLastWriteTimeAndIgnoreUpdate_NoConflict_AppliesChanges() {
    var file = this.CreateTestFile("test.txt", OriginalContent);

    using (var wip = file.StartWorkInProgress(true, ConflictResolutionMode.CheckLastWriteTimeAndIgnoreUpdate)) {
      Assert.AreEqual(ConflictResolutionMode.CheckLastWriteTimeAndIgnoreUpdate, wip.ConflictMode);
      wip.WriteAllText(ModifiedContent);
    }

    Assert.AreEqual(ModifiedContent, File.ReadAllText(file.FullName));
  }

  [Test]
  public void CheckLastWriteTimeAndIgnoreUpdate_ExternalModification_DiscardsChanges() {
    var file = this.CreateTestFile("test.txt", OriginalContent);

    using (var wip = file.StartWorkInProgress(true, ConflictResolutionMode.CheckLastWriteTimeAndIgnoreUpdate)) {
      wip.WriteAllText(ModifiedContent);

      Thread.Sleep(100);
      File.WriteAllText(file.FullName, ExternalContent);
    }

    Assert.AreEqual(ExternalContent, File.ReadAllText(file.FullName));
  }

  #endregion

  #region CheckChecksumAndThrow Mode Tests

  [Test]
  public void CheckChecksumAndThrow_NoConflict_AppliesChanges() {
    var file = this.CreateTestFile("test.txt", OriginalContent);

    using (var wip = file.StartWorkInProgress(true, ConflictResolutionMode.CheckChecksumAndThrow)) {
      Assert.AreEqual(ConflictResolutionMode.CheckChecksumAndThrow, wip.ConflictMode);
      wip.WriteAllText(ModifiedContent);
    }

    Assert.AreEqual(ModifiedContent, File.ReadAllText(file.FullName));
  }

  [Test]
  public void CheckChecksumAndThrow_ExternalModification_ThrowsException() {
    var file = this.CreateTestFile("test.txt", OriginalContent);
    FileInfoExtensions.IFileInProgress wip = null;

    try {
      wip = file.StartWorkInProgress(true, ConflictResolutionMode.CheckChecksumAndThrow);
      wip.WriteAllText(ModifiedContent);

      File.WriteAllText(file.FullName, ExternalContent);

      Assert.Throws<FileConflictException>(() => wip.Dispose());
    } finally {
      if (wip != null) {
        try {
          wip.Dispose();
        } catch (FileConflictException) {
          // Expected
        }
      }
    }
  }

  [Test]
  public void CheckChecksumAndThrow_SameContentRewritten_DoesNotThrow() {
    var file = this.CreateTestFile("test.txt", OriginalContent);

    using (var wip = file.StartWorkInProgress(true, ConflictResolutionMode.CheckChecksumAndThrow)) {
      wip.WriteAllText(ModifiedContent);

      File.WriteAllText(file.FullName, OriginalContent);
    }

    Assert.AreEqual(ModifiedContent, File.ReadAllText(file.FullName));
  }

  #endregion

  #region CheckChecksumAndIgnoreUpdate Mode Tests

  [Test]
  public void CheckChecksumAndIgnoreUpdate_NoConflict_AppliesChanges() {
    var file = this.CreateTestFile("test.txt", OriginalContent);

    using (var wip = file.StartWorkInProgress(true, ConflictResolutionMode.CheckChecksumAndIgnoreUpdate)) {
      Assert.AreEqual(ConflictResolutionMode.CheckChecksumAndIgnoreUpdate, wip.ConflictMode);
      wip.WriteAllText(ModifiedContent);
    }

    Assert.AreEqual(ModifiedContent, File.ReadAllText(file.FullName));
  }

  [Test]
  public void CheckChecksumAndIgnoreUpdate_ExternalModification_DiscardsChanges() {
    var file = this.CreateTestFile("test.txt", OriginalContent);

    using (var wip = file.StartWorkInProgress(true, ConflictResolutionMode.CheckChecksumAndIgnoreUpdate)) {
      wip.WriteAllText(ModifiedContent);
      File.WriteAllText(file.FullName, ExternalContent);
    }

    Assert.AreEqual(ExternalContent, File.ReadAllText(file.FullName));
  }

  [Test]
  public void CheckChecksumAndIgnoreUpdate_SameContentRewritten_AppliesChanges() {
    var file = this.CreateTestFile("test.txt", OriginalContent);

    using (var wip = file.StartWorkInProgress(true, ConflictResolutionMode.CheckChecksumAndIgnoreUpdate)) {
      wip.WriteAllText(ModifiedContent);

      File.WriteAllText(file.FullName, OriginalContent);
    }

    Assert.AreEqual(ModifiedContent, File.ReadAllText(file.FullName));
  }

  #endregion

  #region CancelChanges Tests

  [Test]
  public void AllModes_CancelChanges_DoesNotApplyChanges([Values] ConflictResolutionMode mode) {
    var file = this.CreateTestFile("test.txt", OriginalContent);

    using (var wip = file.StartWorkInProgress(true, mode)) {
      wip.WriteAllText(ModifiedContent);
      wip.CancelChanges = true;
    }

    Assert.AreEqual(OriginalContent, File.ReadAllText(file.FullName));
  }

  #endregion

  #region Default Overload Tests

  [Test]
  public void DefaultOverload_UsesNoneMode() {
    var file = this.CreateTestFile("test.txt", OriginalContent);

    using (var wip = file.StartWorkInProgress(true))
      Assert.AreEqual(ConflictResolutionMode.None, wip.ConflictMode);
  }

  #endregion

  #region Original File Deleted Tests

  [Test]
  public void NoneMode_OriginalDeleted_MovesTempToOriginal() {
    var file = this.CreateTestFile("test.txt", OriginalContent);

    using (var wip = file.StartWorkInProgress(false, ConflictResolutionMode.None)) {
      wip.WriteAllText(ModifiedContent);
      File.Delete(file.FullName);
      Assert.IsFalse(File.Exists(file.FullName));
    }

    Assert.IsTrue(File.Exists(file.FullName));
    Assert.AreEqual(ModifiedContent, File.ReadAllText(file.FullName));
  }

  [Test]
  public void CheckLastWriteTimeAndThrow_OriginalDeleted_MovesTempToOriginal() {
    var file = this.CreateTestFile("test.txt", OriginalContent);

    using (var wip = file.StartWorkInProgress(false, ConflictResolutionMode.CheckLastWriteTimeAndThrow)) {
      wip.WriteAllText(ModifiedContent);
      File.Delete(file.FullName);
    }

    Assert.IsTrue(File.Exists(file.FullName));
    Assert.AreEqual(ModifiedContent, File.ReadAllText(file.FullName));
  }

  [Test]
  public void CheckChecksumAndIgnoreUpdate_OriginalDeleted_MovesTempToOriginal() {
    var file = this.CreateTestFile("test.txt", OriginalContent);

    using (var wip = file.StartWorkInProgress(false, ConflictResolutionMode.CheckChecksumAndIgnoreUpdate)) {
      wip.WriteAllText(ModifiedContent);
      File.Delete(file.FullName);
    }

    Assert.IsTrue(File.Exists(file.FullName));
    Assert.AreEqual(ModifiedContent, File.ReadAllText(file.FullName));
  }

  #endregion

  #region Atomic Read Tests (Lock Modes)

  [Test]
  public void LockExclusive_CopyContents_ReadsFromLockedStream() {
    var file = this.CreateTestFile("test.txt", OriginalContent);

    using (var wip = file.StartWorkInProgress(true, ConflictResolutionMode.LockExclusive)) {
      var content = wip.ReadAllText();
      Assert.AreEqual(OriginalContent, content);
    }
  }

  [Test]
  public void LockWithReadShare_CopyContents_ReadsFromLockedStream() {
    var file = this.CreateTestFile("test.txt", OriginalContent);

    using (var wip = file.StartWorkInProgress(true, ConflictResolutionMode.LockWithReadShare)) {
      var content = wip.ReadAllText();
      Assert.AreEqual(OriginalContent, content);
    }
  }

  [Test]
  public void LockExclusive_NoCopyContents_StartsEmpty() {
    var file = this.CreateTestFile("test.txt", OriginalContent);

    using (var wip = file.StartWorkInProgress(false, ConflictResolutionMode.LockExclusive)) {
      var content = wip.ReadAllText();
      Assert.AreEqual(string.Empty, content);
      wip.WriteAllText(ModifiedContent);
    }

    Assert.AreEqual(ModifiedContent, File.ReadAllText(file.FullName));
  }

  #endregion

  #region File Size Change Tests (Write-Through)

  [Test]
  public void LockExclusive_FileGrows_WriteThroughSucceeds() {
    var smallContent = "Small";
    var largeContent = new string('X', 10000);
    var file = this.CreateTestFile("test.txt", smallContent);

    using (var wip = file.StartWorkInProgress(true, ConflictResolutionMode.LockExclusive)) {
      Assert.AreEqual(smallContent, wip.ReadAllText());
      wip.WriteAllText(largeContent);
    }

    Assert.AreEqual(largeContent, File.ReadAllText(file.FullName));
  }

  [Test]
  public void LockExclusive_FileShrinks_WriteThroughTruncates() {
    var largeContent = new string('X', 10000);
    var smallContent = "Small";
    var file = this.CreateTestFile("test.txt", largeContent);

    using (var wip = file.StartWorkInProgress(true, ConflictResolutionMode.LockExclusive)) {
      Assert.AreEqual(largeContent, wip.ReadAllText());
      wip.WriteAllText(smallContent);
    }

    var result = File.ReadAllText(file.FullName);
    Assert.AreEqual(smallContent, result);
    Assert.AreEqual(smallContent.Length, result.Length);
  }

  [Test]
  public void LockWithReadShare_FileGrows_WriteThroughSucceeds() {
    var smallContent = "Small";
    var largeContent = new string('Y', 10000);
    var file = this.CreateTestFile("test.txt", smallContent);

    using (var wip = file.StartWorkInProgress(true, ConflictResolutionMode.LockWithReadShare))
      wip.WriteAllText(largeContent);

    Assert.AreEqual(largeContent, File.ReadAllText(file.FullName));
  }

  [Test]
  public void LockWithReadShare_FileShrinks_WriteThroughTruncates() {
    var largeContent = new string('Y', 10000);
    var smallContent = "Small";
    var file = this.CreateTestFile("test.txt", largeContent);

    using (var wip = file.StartWorkInProgress(true, ConflictResolutionMode.LockWithReadShare))
      wip.WriteAllText(smallContent);

    var result = File.ReadAllText(file.FullName);
    Assert.AreEqual(smallContent, result);
  }

  #endregion

  #region Concurrent Access Tests

  [Test]
  public void LockExclusive_SecondLockAttempt_Fails() {
    if (Environment.OSVersion.Platform == PlatformID.Unix)
      Assert.Ignore("File locking behavior differs on Unix");

    var file = this.CreateTestFile("test.txt", OriginalContent);

    using (var wip1 = file.StartWorkInProgress(true, ConflictResolutionMode.LockExclusive))
      Assert.Throws<IOException>(() => file.StartWorkInProgress(true, ConflictResolutionMode.LockExclusive));
  }

  [Test]
  public void LockWithReadShare_SecondLockAttempt_Fails() {
    if (Environment.OSVersion.Platform == PlatformID.Unix)
      Assert.Ignore("File locking behavior differs on Unix");

    var file = this.CreateTestFile("test.txt", OriginalContent);

    using (var wip1 = file.StartWorkInProgress(true, ConflictResolutionMode.LockWithReadShare))
      Assert.Throws<IOException>(() => file.StartWorkInProgress(true, ConflictResolutionMode.LockExclusive));
  }

  [Test]
  public void LockExclusive_SequentialOperations_BothSucceed() {
    var file = this.CreateTestFile("test.txt", OriginalContent);

    using (var wip1 = file.StartWorkInProgress(true, ConflictResolutionMode.LockExclusive))
      wip1.WriteAllText("First modification");

    Assert.AreEqual("First modification", File.ReadAllText(file.FullName));

    using (var wip2 = file.StartWorkInProgress(true, ConflictResolutionMode.LockExclusive))
      wip2.WriteAllText("Second modification");

    Assert.AreEqual("Second modification", File.ReadAllText(file.FullName));
  }

  #endregion

  #region Binary Data Tests

  [Test]
  public void LockExclusive_BinaryData_WriteThroughPreservesBytes() {
    var binaryData = new byte[] { 0x00, 0x01, 0x02, 0xFF, 0xFE, 0x00, 0x7F, 0x80 };
    var file = this.CreateTestFileWithBytes("test.bin", binaryData);

    var newData = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };

    using (var wip = file.StartWorkInProgress(true, ConflictResolutionMode.LockExclusive)) {
      var readData = wip.ReadAllBytes();
      CollectionAssert.AreEqual(binaryData, readData);
      wip.WriteAllBytes(newData);
    }

    CollectionAssert.AreEqual(newData, File.ReadAllBytes(file.FullName));
  }

  [Test]
  public void NoneMode_BinaryData_ReplacePreservesBytes() {
    var binaryData = new byte[] { 0x00, 0x01, 0x02, 0xFF, 0xFE, 0x00, 0x7F, 0x80 };
    var file = this.CreateTestFileWithBytes("test.bin", binaryData);

    var newData = new byte[] { 0xCA, 0xFE, 0xBA, 0xBE };

    using (var wip = file.StartWorkInProgress(true, ConflictResolutionMode.None)) {
      var readData = wip.ReadAllBytes();
      CollectionAssert.AreEqual(binaryData, readData);
      wip.WriteAllBytes(newData);
    }

    CollectionAssert.AreEqual(newData, File.ReadAllBytes(file.FullName));
  }

  #endregion

  #region Empty File Tests

  [Test]
  public void LockExclusive_EmptyFile_HandlesCorrectly() {
    var file = this.CreateTestFile("empty.txt", string.Empty);

    using (var wip = file.StartWorkInProgress(true, ConflictResolutionMode.LockExclusive)) {
      Assert.AreEqual(string.Empty, wip.ReadAllText());
      wip.WriteAllText(ModifiedContent);
    }

    Assert.AreEqual(ModifiedContent, File.ReadAllText(file.FullName));
  }

  [Test]
  public void LockExclusive_WriteEmptyFile_Truncates() {
    var file = this.CreateTestFile("test.txt", OriginalContent);

    using (var wip = file.StartWorkInProgress(true, ConflictResolutionMode.LockExclusive))
      wip.WriteAllText(string.Empty);

    Assert.AreEqual(string.Empty, File.ReadAllText(file.FullName));
    Assert.AreEqual(0, new FileInfo(file.FullName).Length);
  }

  #endregion

  #region Helper Methods

  private FileInfo CreateTestFileWithBytes(string fileName, byte[] data) {
    var filePath = Path.Combine(this._TestDirectory, fileName);
    File.WriteAllBytes(filePath, data);

    GC.Collect();
    GC.WaitForPendingFinalizers();

    return new(filePath);
  }

  private FileInfo CreateTestFile(string fileName, string content) {
    var filePath = Path.Combine(this._TestDirectory, fileName);

    using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
    using (var writer = new StreamWriter(stream, new UTF8Encoding(false))) {
      writer.Write(content);
      writer.Flush();
      stream.Flush();
    }

    GC.Collect();
    GC.WaitForPendingFinalizers();

    return new(filePath);
  }

  #endregion
}
